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

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult BackUpdone()
        {
            var connectionString = LegacyWeb.ConnectionString;
            var dbName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
            var configuredPath = db.companys.Where(a => a.CompanyID == 1).Select(a => a.DbBackUpPath).FirstOrDefault();
            var orgpath = !string.IsNullOrWhiteSpace(configuredPath) ? configuredPath : DefaultBackupDir;

            var fname = $"{dbName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            var bakFile = Path.Combine(orgpath, fname + ".bak");
            var zipFile = Path.Combine(orgpath, fname + ".zip");

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
                        var result = cmd.ExecuteNonQuery().ToString();
                        if (result != "-1")
                        {
                            Danger("BackUp Database Failed.", false);
                            return RedirectToAction("BackUp", "BackUpRestore");
                        }
                    }
                }

                using (var zip = ZipFile.Open(zipFile, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(bakFile, fname + ".bak");
                }
                System.IO.File.Delete(bakFile);

                byte[] filedata = System.IO.File.ReadAllBytes(zipFile);
                return File(filedata, "application/zip", fname + ".zip");
            }
            catch (Exception ex)
            {
                Danger("BackUp Database Failed: " + ex.Message, false);
                return RedirectToAction("BackUp", "BackUpRestore");
            }
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

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Database Backup")]
        [RequestFormLimits(MultipartBodyLengthLimit = 5_368_709_120)]
        [DisableRequestSizeLimit]
        public ActionResult Restore(string filepath, IFormFile bakfile)
        {
            string resolvedPath = filepath;
            bool isUploadedTemp = false;

            if (bakfile != null && bakfile.Length > 0)
            {
                var ext = Path.GetExtension(bakfile.FileName);
                if (!string.Equals(ext, ".bak", StringComparison.OrdinalIgnoreCase))
                {
                    Danger("Only .bak files are allowed.", false);
                    return RedirectToAction("Restore", "BackUpRestore");
                }
                Directory.CreateDirectory(RestoreUploadDir);
                resolvedPath = Path.Combine(RestoreUploadDir, Guid.NewGuid().ToString("N") + ".bak");
                bakfile.SaveAs(resolvedPath);
                isUploadedTemp = true;
            }

            if (string.IsNullOrWhiteSpace(resolvedPath))
            {
                Danger("File Not found.", false);
                return RedirectToAction("Restore", "BackUpRestore");
            }

            try
            {
                using (var cnn = new SqlConnection(LegacyWeb.ConnectionString))
                {
                    cnn.Open();
                    using (var cmd = new SqlCommand("SP_BackUpAndRestore", cnn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 1800 })
                    {
                        cmd.Parameters.AddWithValue("@filepath", resolvedPath);
                        cmd.Parameters.AddWithValue("@mode", 1);
                        var result = cmd.ExecuteNonQuery().ToString();
                        if (result == "-1")
                            Success("Successfully Restore The Database", true);
                        else
                            Danger("Restore Database Failed.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                Danger("Restore Database Failed: " + ex.Message, false);
            }
            finally
            {
                if (isUploadedTemp && System.IO.File.Exists(resolvedPath))
                {
                    try { System.IO.File.Delete(resolvedPath); } catch { }
                }
            }

            return RedirectToAction("Restore", "BackUpRestore");
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
