using Microsoft.AspNetCore.Identity;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace QuickSoft.Models
{
    // Profile data on the Identity user (same custom fields as the legacy ApplicationUser).
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public long BranchID { get; set; }
        public BranchAccess BranchAccess { get; set; }

        public choice sync_status { get; set; }
        public int Status { get; set; }

        public decimal? Discount { get; set; }

        public ApplicationUser()
        {
            sync_status = 0;
        }
    }

    // Email/SMS senders, simplified for the Core port (OWIN IIdentityMessageService removed).
    // Wire real SMTP settings via IConfiguration / IEmailSender in a later wave.
    public class EmailService
    {
        public Task SendAsync(string destination, string subject, string body)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("info@quicknet.me"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(destination);
                var smtp = new SmtpClient("smtp.quicknet.me", 25) { EnableSsl = true };
                return smtp.SendMailAsync(mail);
            }
            catch
            {
                return Task.CompletedTask;
            }
        }
    }

    public class SmsService
    {
        public Task SendAsync(string destination, string message) => Task.CompletedTask;
    }
}
