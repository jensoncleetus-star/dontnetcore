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

        [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult BackUp()
        {
           
            ViewBag.FPath = db.companys.Where(a => a.CompanyID == 1).Select(a => a.DbBackUpPath).FirstOrDefault();
            return View();
        }
       // [HttpPost]
       // [QkAuthorize(Roles = "Dev,Database Backup")]
        public ActionResult BackUpdone()
        {
            string result = string.Empty;
            string filepath = "c:/shiyas/";
            if (filepath != null) {
                string orgpath = "c:/shiyas/";//LegacyWeb.MapPath("/backup/");
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
                cmd.Parameters.AddWithValue("@filepath", filepath);
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
                        System.IO.File.Delete(LegacyWeb.MapPath("/uploads/") + "backup.zip");

                    }
                    System.IO.File.Move(orgpath+"backup.zip", LegacyWeb.MapPath("/uploads/") + "backup.zip");

                    System.IO.File.Delete(filepath);
                    ViewBag.link = "<a href='/uploads/backup.zip'>Download</a>";
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
        public ActionResult Restore()
        {
           
            return View();
        }
        //[HttpPost]
        //      //var file = LegacyWeb.MapPath(filename);
        //      //if (Request.Form.Files.Count > 0)
        //      //    try

        //      //  Get all files from Request object  


        //      //// Checking for Internet Explorer  
        //      //if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
        //      //else

        //      //// Get the complete folder path and store the file inside it.  
        //      //fname = Path.Combine(LegacyWeb.MapPath("~/Uploads/"), fname);

        //      //// Returns message that successfully uploaded  





        //      //    catch (Exception ex)
        //      //else

        [HttpPost]
        public ActionResult Restore(string filepath)
        {
            string result = string.Empty;
            //var absolutePath = LegacyWeb.MapPath(filepath);
            if (filepath != null)
            {
                SqlConnection cnn = new SqlConnection();
                cnn.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
                cnn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_BackUpAndRestore";
                cmd.Connection = cnn;
                cmd.Parameters.AddWithValue("@filepath", filepath);
                cmd.Parameters.AddWithValue("@mode", 1);

                result = cmd.ExecuteNonQuery().ToString();
                if (result == "-1")
                {
                    Success("Successfully Restore The Database", true);
                    return RedirectToAction("Restore", "BackUpRestore");
                }
                else
                {
                    Danger("Restore Database Failed.", false);
                    return RedirectToAction("Restore", "BackUpRestore");
                }
            }
            else
            {
                Danger("File Not found.", false);
                return RedirectToAction("Restore", "BackUpRestore");
            }

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
