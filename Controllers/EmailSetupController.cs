using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickSoft.Models;
using System;
using System.Linq;

namespace QuickSoft.Controllers
{
    // Dedicated Email (SMTP) setup page, moved out of Company Settings. Stores into the SAME company
    // SMTP columns (old-DB-safe, no new table). Provider presets make Gmail / Office 365 / etc. connect cleanly.
    public class EmailSetupController : BaseController
    {
        ApplicationDbContext db;
        public EmailSetupController() { db = new ApplicationDbContext(); }

        [QkAuthorize(Roles = "Dev,Company Edit,Email Setup")]
        public ActionResult Index()
        {
            var comp = db.companys.FirstOrDefault();
            return View(comp);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Company Edit,Email Setup")]
        public ActionResult Save(string SMTPEmail, string SMTPHost, long? SMTPPort, string SMTPUsername, string SMTPPassword, bool EnableSsl)
        {
            try
            {
                var comp = db.companys.FirstOrDefault();
                if (comp == null) return Json(new { success = false, message = "Company record not found." });
                comp.SMTPEmail = SMTPEmail;
                comp.SMTPHost = SMTPHost;
                comp.SMTPPort = SMTPPort;
                comp.SMTPUsername = SMTPUsername;
                if (!string.IsNullOrEmpty(SMTPPassword)) comp.SMTPPassword = SMTPPassword;   // keep saved password if left blank
                comp.EnableSsl = EnableSsl;
                db.Entry(comp).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, message = "Email settings saved." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Save failed: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // Sends a test email using the VALUES ON THE FORM (so it works before saving). Blank password falls
        // back to the saved one. Surfaces the exact SMTP error so the user can fix host/port/credentials.
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Company Edit,Email Setup")]
        public ActionResult TestEmail(string toEmail, string SMTPHost, long? SMTPPort, string SMTPUsername, string SMTPPassword, bool EnableSsl, string SMTPEmail)
        {
            try
            {
                var comp = db.companys.FirstOrDefault();
                var pass = string.IsNullOrEmpty(SMTPPassword) ? comp?.SMTPPassword : SMTPPassword;
                var from = string.IsNullOrWhiteSpace(SMTPEmail) ? SMTPUsername : SMTPEmail;
                if (string.IsNullOrWhiteSpace(SMTPHost) || SMTPPort == null || string.IsNullOrWhiteSpace(SMTPUsername))
                    return Json(new { success = false, message = "Host, Port and Username are required." });
                var to = string.IsNullOrWhiteSpace(toEmail) ? from : toEmail.Trim();
                using (var msg = new System.Net.Mail.MailMessage())
                {
                    msg.From = new System.Net.Mail.MailAddress(from);
                    msg.To.Add(to);
                    msg.Subject = "Email setup test - " + (comp != null ? (comp.CPName ?? "Company") : "Company");
                    msg.Body = "<p>This is a test email. If you received it, your email settings are working correctly.</p>";
                    msg.IsBodyHtml = true;
                    using (var smtp = new System.Net.Mail.SmtpClient(SMTPHost, Convert.ToInt32(SMTPPort)))
                    {
                        smtp.EnableSsl = EnableSsl;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new System.Net.NetworkCredential(SMTPUsername, pass);
                        smtp.Send(msg);
                    }
                }
                return Json(new { success = true, message = "Test email sent to " + to + ". Check the inbox (and spam)." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }
    }
}
