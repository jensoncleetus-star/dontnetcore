using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickSoft.Models;

namespace QuickSoft.Helpers
{
    // Daily background job that emails tenants whose tenancy contract is expiring within
    // the configured window. Self-contained: own ApplicationDbContext + own SMTP send so it
    // never depends on a request scope. Auto-send is gated by EnableSettings('ReminderAutoSend')
    // and defaults OFF, so it is silent until the owner enables it. PropertyReminders.Run() is
    // shared with the Insights "Send Now" button.
    public sealed class PropertyReminderService : BackgroundService
    {
        private readonly ILogger<PropertyReminderService> _logger;
        public PropertyReminderService(ILogger<PropertyReminderService> logger) { _logger = logger; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // let the app finish starting before the first sweep
            try { await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); } catch { return; }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var r = PropertyReminders.Run(auto: true);
                    if (r.AutoEnabled)
                        _logger.LogInformation("Property reminders: {Sent} sent, {NoEmail} no-email, {Failed} failed of {Considered} due", r.Sent, r.NoEmail, r.Failed, r.Considered);
                }
                catch (Exception ex) { _logger.LogDebug(ex, "property reminder sweep failed"); }
                try { await Task.Delay(TimeSpan.FromHours(12), stoppingToken); } catch { break; }
            }
        }
    }

    public class ReminderRunResult
    {
        public bool AutoEnabled { get; set; }
        public int DaysAhead { get; set; }
        public int Considered { get; set; }
        public int Sent { get; set; }
        public int NoEmail { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }   // already-sent (deduped)
        public List<string> Lines { get; set; } = new List<string>();
    }

    public static class PropertyReminders
    {
        // Builds the list of contracts expiring within the window (read-only preview helper,
        // also used by Run). Returns rows already joined with tenant/property/email.
        public class DueRow
        {
            public long Id { get; set; }
            public string Code { get; set; }
            public string Tenant { get; set; }
            public string Property { get; set; }
            public string Email { get; set; }
            public DateTime Expiry { get; set; }
            public int Days { get; set; }
            public decimal Rent { get; set; }
            public bool AlreadySent { get; set; }
        }

        public static List<DueRow> Upcoming(ApplicationDbContext db, int daysAhead)
        {
            var today = DateTime.Today;
            var limit = today.AddDays(daysAhead);

            var contracts = (from c in db.TenancyContracts
                             where c.Status == Status.active && c.EndDate >= today && c.EndDate <= limit
                             join tn in db.Tenants on c.Tenant equals tn.TenantID into tt from tn in tt.DefaultIfEmpty()
                             join p in db.PropertyMains on c.Property equals p.Id into pp from p in pp.DefaultIfEmpty()
                             select new { c.Id, c.Code, c.EndDate, c.Rent, tenantName = tn.TenantName, contact = (long?)tn.Contact, propName = p.Name }).ToList();

            var sentIds = db.PropertyReminderLogs.Where(l => l.Kind == "ContractExpiry" && l.Result == "Sent")
                            .Select(l => l.RefID).ToList();
            var sent = new HashSet<long>(sentIds);

            var contactIds = contracts.Where(x => x.contact != null).Select(x => x.contact.Value).Distinct().ToList();
            var emails = db.Contacts.Where(c => contactIds.Contains(c.ContactID))
                           .Select(c => new { c.ContactID, c.EmailId }).ToList()
                           .ToDictionary(x => x.ContactID, x => x.EmailId);

            return contracts.Select(x => new DueRow
            {
                Id = x.Id,
                Code = x.Code ?? ("TC-" + x.Id),
                Tenant = x.tenantName ?? "-",
                Property = x.propName ?? "-",
                Email = (x.contact != null && emails.ContainsKey(x.contact.Value)) ? (emails[x.contact.Value] ?? "") : "",
                Expiry = x.EndDate,
                Days = (int)Math.Round((x.EndDate.Date - today).TotalDays),
                Rent = x.Rent ?? 0,
                AlreadySent = sent.Contains(x.Id)
            }).OrderBy(x => x.Days).ToList();
        }

        public static ReminderRunResult Run(bool auto)
        {
            var res = new ReminderRunResult();
            using var db = new ApplicationDbContext();

            var autoRow = db.EnableSettings.FirstOrDefault(e => e.EnableType == "ReminderAutoSend");
            res.AutoEnabled = autoRow != null && autoRow.Status == Status.active;
            var daysRow = db.EnableSettings.FirstOrDefault(e => e.EnableType == "ReminderDaysAhead");
            int days = 30;
            if (daysRow != null) int.TryParse(daysRow.TypeValue, out days);
            if (days <= 0) days = 30;
            res.DaysAhead = days;

            // auto run respects the toggle; manual "Send Now" always proceeds
            if (auto && !res.AutoEnabled) return res;

            var due = Upcoming(db, days);
            string company = db.companys.Select(c => c.CPName).FirstOrDefault() ?? "Property Management";

            foreach (var d in due)
            {
                res.Considered++;
                if (d.AlreadySent) { res.Skipped++; continue; }

                string subject = "Tenancy Contract Expiry Reminder — " + d.Code;
                string body = BuildBody(company, d);
                string result;
                if (string.IsNullOrWhiteSpace(d.Email) || !d.Email.Contains("@"))
                {
                    result = "No email"; res.NoEmail++;
                }
                else
                {
                    var (ok, err) = TrySend(db, d.Email.Trim(), subject, body);
                    if (ok) { result = "Sent"; res.Sent++; }
                    else { result = "Failed: " + Trunc(err, 380); res.Failed++; }
                }

                db.PropertyReminderLogs.Add(new PropertyReminderLog
                {
                    Kind = "ContractExpiry",
                    RefID = d.Id,
                    Title = d.Tenant + " — " + d.Property,
                    ToEmail = string.IsNullOrWhiteSpace(d.Email) ? "(none)" : d.Email,
                    Subject = subject,
                    ExpiryDate = d.Expiry,
                    SentDate = DateTime.Now,
                    Result = result
                });
                res.Lines.Add(d.Tenant + " (" + d.Property + ") → " + result);
            }
            if (res.Considered > 0) db.SaveChanges();
            return res;
        }

        private static string BuildBody(string company, DueRow d)
        {
            return
                "<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:auto;border:1px solid #eee;border-radius:10px;overflow:hidden'>" +
                "<div style='background:linear-gradient(120deg,#0ea5e9,#6366f1);color:#fff;padding:18px 22px'>" +
                "<h2 style='margin:0;font-size:18px'>Tenancy Contract Expiry Reminder</h2></div>" +
                "<div style='padding:22px'>" +
                "<p>Dear " + Enc(d.Tenant) + ",</p>" +
                "<p>This is a friendly reminder that your tenancy contract <b>" + Enc(d.Code) + "</b> for " +
                "<b>" + Enc(d.Property) + "</b> is due to expire on <b>" + d.Expiry.ToString("dd MMM yyyy") + "</b> " +
                "(" + d.Days + " day(s) from today).</p>" +
                "<p>Monthly rent: <b>" + d.Rent.ToString("#,##0") + "</b></p>" +
                "<p>Please contact our office to arrange renewal at your earliest convenience.</p>" +
                "<p style='margin-top:24px;color:#666'>Regards,<br/>" + Enc(company) + "</p>" +
                "</div></div>";
        }

        private static (bool ok, string err) TrySend(ApplicationDbContext db, string to, string subject, string body)
        {
            try
            {
                var comp = db.companys.FirstOrDefault();
                string host, user, pass; int port; bool ssl;
                if (comp != null && !string.IsNullOrEmpty(comp.SMTPHost) && comp.SMTPPort != null
                    && !string.IsNullOrEmpty(comp.SMTPUsername) && !string.IsNullOrEmpty(comp.SMTPPassword))
                {
                    host = comp.SMTPHost; port = (int)comp.SMTPPort.Value; user = comp.SMTPUsername;
                    pass = comp.SMTPPassword; ssl = comp.EnableSsl;
                }
                else
                {
                    host = "outlook.office365.com"; port = 587; user = "qucknet@quicknet.me";
                    pass = Environment.GetEnvironmentVariable("Smtp__Password"); ssl = true;
                }
                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                    return (false, "SMTP not configured (set Company SMTP or Smtp__Password)");

                using var msg = new MailMessage();
                msg.From = new MailAddress(user);
                msg.To.Add(to);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = true;
                using var smtp = new SmtpClient(host, port)
                {
                    EnableSsl = ssl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(user, pass)
                };
                smtp.Send(msg);
                return (true, "Sent");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        private static string Enc(string s) => string.IsNullOrEmpty(s) ? "" :
            s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        private static string Trunc(string s, int n) => string.IsNullOrEmpty(s) ? "" : (s.Length <= n ? s : s.Substring(0, n));
    }
}
