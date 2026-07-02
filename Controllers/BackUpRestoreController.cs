using System.Linq.Dynamic.Core;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class BackUpRestoreController : BaseController
    {
        ApplicationDbContext db;
        public BackUpRestoreController()
        {
            db = new ApplicationDbContext();
        }

        // ---- Database Optimize (re-index): rebuilds/reorganizes fragmented indexes + refreshes stats.
        //      Touches performance only, never data. Admin-only. No user input -> no injection risk. ----
        private const string ReindexSql = @"
SET NOCOUNT ON;
DECLARE @cmds TABLE (id INT IDENTITY(1,1), cmd NVARCHAR(MAX));
INSERT INTO @cmds(cmd)
SELECT 'ALTER INDEX ' + QUOTENAME(i.name) + ' ON ' + QUOTENAME(sc.name) + '.' + QUOTENAME(t.name)
       + CASE WHEN ps.avg_fragmentation_in_percent > 30 THEN ' REBUILD;' ELSE ' REORGANIZE;' END
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps
JOIN sys.indexes i ON i.object_id = ps.object_id AND i.index_id = ps.index_id
JOIN sys.tables t ON t.object_id = i.object_id
JOIN sys.schemas sc ON sc.schema_id = t.schema_id
WHERE i.name IS NOT NULL AND i.index_id > 0 AND ps.page_count > 128 AND ps.avg_fragmentation_in_percent > 10;
DECLARE @cmd NVARCHAR(MAX), @i INT = 1, @mx INT = (SELECT ISNULL(MAX(id),0) FROM @cmds), @done INT = 0;
WHILE @i <= @mx
BEGIN
  SET @cmd = (SELECT cmd FROM @cmds WHERE id = @i);
  IF @cmd IS NOT NULL BEGIN BEGIN TRY EXEC sp_executesql @cmd; SET @done = @done + 1; END TRY BEGIN CATCH END CATCH END
  SET @i = @i + 1;
END
BEGIN TRY EXEC sp_updatestats; END TRY BEGIN CATCH END CATCH
SELECT @mx AS Found, @done AS Processed;";

        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult Reindex()
        {
            try
            {
                using (var con = new SqlConnection(db.Database.GetDbConnection().ConnectionString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(@"SELECT COUNT(*) AS Frag, ISNULL(MAX(ps.avg_fragmentation_in_percent),0) AS Worst
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps
JOIN sys.indexes i ON i.object_id = ps.object_id AND i.index_id = ps.index_id
WHERE i.name IS NOT NULL AND ps.page_count > 128 AND ps.avg_fragmentation_in_percent > 10;", con) { CommandTimeout = 180 })
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            ViewBag.FragCount = Convert.ToInt32(rdr["Frag"]);
                            ViewBag.Worst = Math.Round(Convert.ToDouble(rdr["Worst"]), 1);
                        }
                    }
                }
            }
            catch (Exception ex) { ViewBag.FragErr = ex.Message; }
            ViewBag.Active = "Settings";
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult RunReindex()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int found = 0, processed = 0;
            try
            {
                using (var con = new SqlConnection(db.Database.GetDbConnection().ConnectionString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(ReindexSql, con) { CommandTimeout = 1800 })
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            found = Convert.ToInt32(rdr["Found"]);
                            processed = Convert.ToInt32(rdr["Processed"]);
                        }
                    }
                }
                sw.Stop();
                return Json(new { ok = true, found = found, processed = processed, seconds = Math.Round(sw.Elapsed.TotalSeconds, 1) });
            }
            catch (Exception ex)
            {
                sw.Stop();
                return Json(new { ok = false, error = ex.Message, seconds = Math.Round(sw.Elapsed.TotalSeconds, 1) });
            }
        }

        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult BackUp()
        {
           
            ViewBag.FPath = db.companys.Where(a => a.CompanyID == 1).Select(a => a.DbBackUpPath).FirstOrDefault();
            return View();
        }
        // Backups are written here (content root, outside wwwroot) unless the company has a configured
        // DbBackUpPath, so a finished backup is never reachable through the static-file server.
        private static string DefaultBackupDir => Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "DbBackups");

        public class BackupJobStatus
        {
            public string State = "Running"; // Running | Completed | Failed
            public int Percent;
            public string Message = "Starting backup...";
            public string ZipPath;
            public string FileName;
        }

        // In-memory job board for the async backup flow below. A single-process app (this one runs as a
        // Windows Service / one instance per customer) doesn't need anything heavier than this.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, BackupJobStatus> BackupJobs
            = new System.Collections.Concurrent.ConcurrentDictionary<string, BackupJobStatus>();

        // Kicks off the backup on a background thread and returns immediately with a job id, so the
        // browser can poll BackUpStatus for real progress instead of hanging on one long request
        // (which also meant a returned File() result never navigated the page, leaving the "please
        // wait" overlay stuck forever with no way to know the backup had actually finished).
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult BackUpStart()
        {
            var connectionString = LegacyWeb.ConnectionString;
            var dbName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
            var configuredPath = db.companys.Where(a => a.CompanyID == 1).Select(a => a.DbBackUpPath).FirstOrDefault();
            var orgpath = !string.IsNullOrWhiteSpace(configuredPath) ? configuredPath : DefaultBackupDir;

            var fname = $"{dbName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            var bakFile = Path.Combine(orgpath, fname + ".bak");
            var zipFile = Path.Combine(orgpath, fname + ".zip");

            var jobId = Guid.NewGuid().ToString("N");
            var status = new BackupJobStatus();
            BackupJobs[jobId] = status;

            System.Threading.Tasks.Task.Run(() => RunBackupJob(connectionString, orgpath, bakFile, zipFile, fname, status));

            return Json(new { jobId });
        }

        private static void RunBackupJob(string connectionString, string orgpath, string bakFile, string zipFile, string fname, BackupJobStatus status)
        {
            try
            {
                Directory.CreateDirectory(orgpath);

                using (var cnn = new SqlConnection(connectionString))
                {
                    cnn.Open();
                    using (var cmd = new SqlCommand("SP_BackUpAndRestore", cnn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 1800 })
                    {
                        cmd.Parameters.AddWithValue("@filepath", bakFile);
                        cmd.Parameters.AddWithValue("@mode", 0);

                        string result;
                        using (var progressCts = new System.Threading.CancellationTokenSource())
                        {
                            var progressTask = System.Threading.Tasks.Task.Run(() => PollBackupProgress(connectionString, status, progressCts.Token));
                            try
                            {
                                result = cmd.ExecuteNonQuery().ToString();
                            }
                            finally
                            {
                                progressCts.Cancel();
                                progressTask.Wait(2000);
                            }
                        }

                        if (result != "-1")
                        {
                            status.State = "Failed";
                            status.Message = "Backup Database Failed.";
                            return;
                        }
                    }
                }

                status.Percent = 95;
                status.Message = "Compressing backup...";
                using (var zip = ZipFile.Open(zipFile, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(bakFile, fname + ".bak");
                }
                System.IO.File.Delete(bakFile);

                status.Percent = 100;
                status.ZipPath = zipFile;
                status.FileName = fname + ".zip";
                status.State = "Completed";
                status.Message = "Backup completed successfully.";
            }
            catch (Exception ex)
            {
                status.State = "Failed";
                status.Message = "Backup Database Failed: " + ex.Message;
            }
        }

        // Best-effort progress via SQL Server's own DMV. Requires VIEW SERVER STATE; if the login
        // doesn't have it, this just silently never advances the bar past 0 and the backup still runs.
        private static void PollBackupProgress(string connectionString, BackupJobStatus status, System.Threading.CancellationToken token)
        {
            try
            {
                using (var cnn = new SqlConnection(connectionString))
                {
                    cnn.Open();
                    while (!token.IsCancellationRequested)
                    {
                        System.Threading.Thread.Sleep(1500);
                        if (token.IsCancellationRequested) break;
                        try
                        {
                            using (var cmd = new SqlCommand(
                                "SELECT TOP 1 percent_complete FROM sys.dm_exec_requests WHERE command = 'BACKUP DATABASE' ORDER BY percent_complete DESC", cnn))
                            {
                                var pct = cmd.ExecuteScalar();
                                if (pct != null && pct != DBNull.Value)
                                {
                                    status.Percent = Math.Min(94, (int)Math.Round(Convert.ToDouble(pct)));
                                    status.Message = $"Backing up database... {status.Percent}%";
                                }
                            }
                        }
                        catch { /* transient polling error: keep last known percent */ }
                    }
                }
            }
            catch { /* no VIEW SERVER STATE or similar: progress just stays indeterminate */ }
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult BackUpStatus(string jobId)
        {
            if (jobId == null || !BackupJobs.TryGetValue(jobId, out var status))
                return Json(new { state = "Failed", percent = 0, message = "Unknown backup job." });

            return Json(new
            {
                state = status.State,
                percent = status.Percent,
                message = status.Message,
                downloadUrl = status.State == "Completed" ? Url.Action("BackUpDownload", "BackUpRestore", new { jobId }) : null
            });
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult BackUpDownload(string jobId)
        {
            if (jobId == null || !BackupJobs.TryRemove(jobId, out var status) || status.State != "Completed" || !System.IO.File.Exists(status.ZipPath))
            {
                Danger("Backup file not found or already downloaded.", false);
                return RedirectToAction("BackUp", "BackUpRestore");
            }

            byte[] filedata = System.IO.File.ReadAllBytes(status.ZipPath);
            return File(filedata, "application/zip", status.FileName);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult BackUpdonenew()
        {
            string result = string.Empty;
            string filepath = "./uploads";
            if (filepath != null)
            {
                string orgpath = "/uploads/";//LegacyWeb.MapPath("/backup/");
                string fname = "BackUp_" + DateTime.Now.ToString("dd-MM-yyyy--HH-mm-sstt") + ".Bak";
                filepath = orgpath + fname;
                SqlConnection cnn = new SqlConnection();
                cnn.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
                cnn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_BackUpAndRestore";
                cmd.Connection = cnn;
                cmd.CommandTimeout = 1000;
                cmd.Parameters.AddWithValue("@filepath", LegacyWeb.MapPath(filepath));
                cmd.Parameters.AddWithValue("@mode", 0);

                result = cmd.ExecuteNonQuery().ToString();

                if (result == "-1")
                {

                    if (System.IO.File.Exists(orgpath + "backup.zip"))
                    {
                        System.IO.File.Delete(orgpath + "backup.zip");

                    }
                    using (ZipArchive zip = ZipFile.Open(orgpath + "backup.zip", ZipArchiveMode.Create))
                    {
                        zip.CreateEntryFromFile(filepath, "backup");
                    }
                    if (System.IO.File.Exists(LegacyWeb.MapPath("/uploads/") + "backup.zip"))
                    {
                      //  System.IO.File.Delete(LegacyWeb.MapPath("/uploads/") + "backup.zip");

                    }
                    System.IO.File.Copy(orgpath + "backup.zip", LegacyWeb.MapPath("/uploads/") + "backup.zip");

                    ViewBag.link = "<a href='./uploads/backup.zip'>Download</a>";
                    byte[] filedata = System.IO.File.ReadAllBytes(LegacyWeb.MapPath("/uploads/") + "backup.zip");
                    return File(filedata, "application/zip", "BackUp_" + DateTime.Now.ToString("dd-MM-yyyy--HH-mm-sstt") + ".zip");

                    //    client.DownloadFile(LegacyWeb.MapPath("/uploads/") + "backup.zip","backup.zip");
                }
                else
                {
                    Danger("BackUp Database Failed.", false);
                    return RedirectToAction("BackUp", "BackUpRestore");
                }
            }
            else
            {
                Danger("Folder Not found.", false);
                return RedirectToAction("BackUp", "BackUpRestore");
            }

        }

        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult downloadallbackup()
        {
            string qry = "exec sp_BackupDatabases @backupType='F',@backupLocation='C:\\QUCKSOFT\\SOFTWARE\\QUICKNET-1200\\uploads\\'";

            SqlConnection con = new SqlConnection(db.Database.GetDbConnection().ConnectionString);
                con.Open();
                SqlCommand cmd = new SqlCommand(qry, con);
            cmd.CommandTimeout = 60*60;
            SqlDataReader sdr = cmd.ExecuteReader();
            string orgpath = LegacyWeb.MapPath("/uploads/");
            if (System.IO.File.Exists(orgpath + "backupall.zip"))
            {
                System.IO.File.Delete(orgpath + "backupall.zip");

            }
            using (ZipArchive zip = ZipFile.Open(orgpath + "backupall.zip", ZipArchiveMode.Create))
            {
                for (int i = 1; i < 50; i++)
                {
                    string filename = i + ".ZIP";
                    if (System.IO.File.Exists(orgpath + filename))
                    {
                        zip.CreateEntryFromFile(orgpath + filename, filename);
                    }
                }
            }
           


            return View();
            

        }
        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult Restore()
        {

            return View();
        }
        // Uploaded .bak files land here (content root, outside wwwroot) so a restore file is never
        // reachable through the static-file server the way /uploads content is.
        private static string RestoreUploadDir => Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "DbRestore");

        public class RestoreJobStatus
        {
            public string State = "Running"; // Running | Completed | Failed
            public int Percent;
            public string Message = "Starting restore...";
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, RestoreJobStatus> RestoreJobs
            = new System.Collections.Concurrent.ConcurrentDictionary<string, RestoreJobStatus>();

        // Same async job pattern as BackUpStart: kick off on a background thread, return a job id
        // immediately, let the browser poll RestoreStatus for live percent + a clear final state
        // instead of blocking on one request with no feedback until the page reloads.
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Database Backup")]
        [RequestFormLimits(MultipartBodyLengthLimit = 5_368_709_120)]
        [DisableRequestSizeLimit]
        public ActionResult RestoreStart(string filepath, IFormFile bakfile)
        {
            string resolvedPath = filepath;
            bool isUploadedTemp = false;

            if (bakfile != null && bakfile.Length > 0)
            {
                var ext = Path.GetExtension(bakfile.FileName);
                if (!string.Equals(ext, ".bak", StringComparison.OrdinalIgnoreCase))
                    return Json(new { error = "Only .bak files are allowed." });

                Directory.CreateDirectory(RestoreUploadDir);
                resolvedPath = Path.Combine(RestoreUploadDir, Guid.NewGuid().ToString("N") + ".bak");
                bakfile.SaveAs(resolvedPath);
                isUploadedTemp = true;
            }

            if (string.IsNullOrWhiteSpace(resolvedPath))
                return Json(new { error = "Please upload a .bak file or enter a server file path." });

            var jobId = Guid.NewGuid().ToString("N");
            var status = new RestoreJobStatus();
            RestoreJobs[jobId] = status;

            var connectionString = LegacyWeb.ConnectionString;
            System.Threading.Tasks.Task.Run(() => RunRestoreJob(connectionString, resolvedPath, isUploadedTemp, status));

            return Json(new { jobId });
        }

        private static void RunRestoreJob(string connectionString, string resolvedPath, bool isUploadedTemp, RestoreJobStatus status)
        {
            try
            {
                using (var cnn = new SqlConnection(connectionString))
                {
                    cnn.Open();
                    using (var cmd = new SqlCommand("SP_BackUpAndRestore", cnn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 1800 })
                    {
                        cmd.Parameters.AddWithValue("@filepath", resolvedPath);
                        cmd.Parameters.AddWithValue("@mode", 1);

                        string result;
                        using (var progressCts = new System.Threading.CancellationTokenSource())
                        {
                            var progressTask = System.Threading.Tasks.Task.Run(() => PollRestoreProgress(connectionString, status, progressCts.Token));
                            try
                            {
                                result = cmd.ExecuteNonQuery().ToString();
                            }
                            finally
                            {
                                progressCts.Cancel();
                                progressTask.Wait(2000);
                            }
                        }

                        if (result == "-1")
                        {
                            status.Percent = 100;
                            status.State = "Completed";
                            status.Message = "Database restored successfully.";
                        }
                        else
                        {
                            status.State = "Failed";
                            status.Message = "Restore Database Failed.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status.State = "Failed";
                status.Message = "Restore Database Failed: " + ex.Message;
            }
            finally
            {
                if (isUploadedTemp && System.IO.File.Exists(resolvedPath))
                {
                    try { System.IO.File.Delete(resolvedPath); } catch { }
                }
            }
        }

        // Best-effort progress via SQL Server's own DMV, same approach as PollBackupProgress.
        private static void PollRestoreProgress(string connectionString, RestoreJobStatus status, System.Threading.CancellationToken token)
        {
            try
            {
                using (var cnn = new SqlConnection(connectionString))
                {
                    cnn.Open();
                    while (!token.IsCancellationRequested)
                    {
                        System.Threading.Thread.Sleep(1500);
                        if (token.IsCancellationRequested) break;
                        try
                        {
                            using (var cmd = new SqlCommand(
                                "SELECT TOP 1 percent_complete FROM sys.dm_exec_requests WHERE command = 'RESTORE DATABASE' ORDER BY percent_complete DESC", cnn))
                            {
                                var pct = cmd.ExecuteScalar();
                                if (pct != null && pct != DBNull.Value)
                                {
                                    status.Percent = Math.Min(99, (int)Math.Round(Convert.ToDouble(pct)));
                                    status.Message = $"Restoring database... {status.Percent}%";
                                }
                            }
                        }
                        catch { /* transient polling error: keep last known percent */ }
                    }
                }
            }
            catch { /* no VIEW SERVER STATE or similar: progress just stays indeterminate */ }
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult RestoreStatus(string jobId)
        {
            if (jobId == null || !RestoreJobs.TryGetValue(jobId, out var status))
                return Json(new { state = "Failed", percent = 0, message = "Unknown restore job." });

            if (status.State != "Running")
                RestoreJobs.TryRemove(jobId, out _);

            return Json(new { state = status.State, percent = status.Percent, message = status.Message });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
