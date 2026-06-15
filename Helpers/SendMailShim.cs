using System;
using System.Net.Mail;

namespace QuickSoft.Models
{
    // Minimal port of the legacy SendMail's plain email path (the iText PDF overloads in the
    // original SendMail.cs are excluded for now and ported in a later wave). Common.cs only
    // calls new SendMail() + sendMail(to, cc, message).
    public class SendMail
    {
        public SendMail() { }

        public void sendMail(string ToMail, string CcMail, MailMessage message)
        {
            try
            {
                if (message == null) return;
                if (!string.IsNullOrEmpty(ToMail)) message.To.Add(ToMail);
                if (!string.IsNullOrEmpty(CcMail)) message.CC.Add(CcMail);
                if (message.From == null) message.From = new MailAddress("info@quicknet.me");
                using var smtp = new SmtpClient("smtp.quicknet.me", 25) { EnableSsl = true };
                smtp.Send(message);
            }
            catch
            {
                // best-effort send; SMTP settings wired properly in a later wave
            }
        }

        public void sendMailwithoutattachment(string ToMail, string CcMail, string InvoiceNo, MailMessage message) => sendMail(ToMail, CcMail, message);

        // PDF: the legacy generatePdf(id) returns XHTML in a StringBuilder; iTextSharp's XMLWorker turned
        // that XHTML into a PDF. The .NET 10 successor is iText 7 pdfHTML (HtmlConverter.ConvertToPdf).
        public byte[] DownloadPdf(System.Text.StringBuilder sb, string status) => HtmlToPdf(sb);
        public byte[] DownloadPdf(System.Text.StringBuilder sb) => HtmlToPdf(sb);

        public void SendPdfMail(System.Text.StringBuilder sb, string ToMail, string CcMail, string InvoiceNo, MailMessage message)
        {
            try
            {
                var pdf = HtmlToPdf(sb);
                if (message != null && pdf.Length > 0)
                {
                    var name = (string.IsNullOrEmpty(InvoiceNo) ? "document" : InvoiceNo) + ".pdf";
                    message.Attachments.Add(new Attachment(new System.IO.MemoryStream(pdf), name, "application/pdf"));
                }
                sendMail(ToMail, CcMail, message);
            }
            catch { /* best-effort */ }
        }

        private static byte[] HtmlToPdf(System.Text.StringBuilder sb)
        {
            if (sb == null || sb.Length == 0) { System.Console.Error.WriteLine("[PDF] empty html (generatePdf returned nothing)"); return System.Array.Empty<byte>(); }
            try
            {
                using var ms = new System.IO.MemoryStream();
                iText.Html2pdf.HtmlConverter.ConvertToPdf(sb.ToString(), ms);
                var bytes = ms.ToArray(); // ConvertToPdf closes the stream; ToArray still works, but ms.Length would throw
                System.Console.Error.WriteLine("[PDF] generated " + bytes.Length + " bytes from " + sb.Length + " html chars");
                return bytes;
            }
            catch (System.Exception ex) { System.Console.Error.WriteLine("[PDF] iText ERROR: " + ex.ToString().Substring(0, System.Math.Min(500, ex.ToString().Length))); return System.Array.Empty<byte>(); }
        }
    }

    // Compat for EF6 DbFunctions.AddDays (EF Core has no EF.Functions.AddDays; client-evaluated).
    public static class DbFunctionsCompat
    {
        public static DateTime? AddDays(DateTime? date, int? days) => date?.AddDays(days ?? 0);
        public static DateTime AddDays(DateTime date, int days) => date.AddDays(days);
        public static DateTime? AddMinutes(DateTime? date, int? minutes) => date?.AddMinutes(minutes ?? 0);
        public static DateTime? AddMinutes(DateTime? date, double? minutes) => date?.AddMinutes(minutes ?? 0);
        public static DateTime? AddHours(DateTime? date, int? hours) => date?.AddHours(hours ?? 0);
        public static DateTime? AddHours(DateTime? date, double? hours) => date?.AddHours(hours ?? 0);
    }
}
