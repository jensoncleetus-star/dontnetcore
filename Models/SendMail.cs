using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using QuickSoft.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace QuickSoft.Models
{
    public class SendMail
    {
        ApplicationDbContext db;
        public SendMail()
        {
            db = new ApplicationDbContext();
        }

        //for send mails
        public void SendPdfMail(StringBuilder sb, string ToMail, string CcMail, string InvoiceNo, MailMessage message)
        {
            //dynamically adddded//need change
            var HeaderFooterID = 1;
            //-----------------------------
            //Response.Write(sb.ToString());
            var headefoot = db.CompanyHeaders.Find(HeaderFooterID);
    
            //headefoot = null;
            StringReader sr = new StringReader(sb.ToString());
            if ((headefoot.Header != "" || headefoot.Footer != "") && (headefoot.Header != null || headefoot.Footer != null))
            {
                //with header and footer image
                Document pdfDoc = new Document(PageSize.A4, 20, 20, 112, 90);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                    writer.ViewerPreferences = PdfWriter.PageModeUseOutlines;
                    PdfHeaderFooter PageEventHandler = new PdfHeaderFooter();
                    writer.PageEvent = PageEventHandler;

                    if (headefoot.Header != null)
                    {
                        PageEventHandler.Title = Image.GetInstance(LegacyWeb.MapPath("/uploads/companyheader/header/" + headefoot.Header));
                    }
                    if (headefoot.Footer != null)
                    {
                        PageEventHandler.FTitle = Image.GetInstance(LegacyWeb.MapPath("/uploads/companyheader/footer/" + headefoot.Footer));
                    }
                    pdfDoc.Open();
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                    pdfDoc.Close();
                    byte[] bytes = memoryStream.ToArray();
                    memoryStream.Close();
                    sendMail(ToMail, CcMail, InvoiceNo, bytes, message);
                }
            }
            else
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    //step 1
                    using (Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 85f, 10f))
                    {

                        // step 2
                        PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, memoryStream);
                        pdfWriter.PageEvent = new Helpers.ITextEvents();

                        //open the stream 
                        pdfDoc.Open();
                        XMLWorkerHelper.GetInstance().ParseXHtml(pdfWriter, pdfDoc, sr);
                        pdfDoc.Close();
                        byte[] bytes = memoryStream.ToArray();
                        memoryStream.Close();
                        sendMail(ToMail, CcMail, InvoiceNo, bytes, message);
                    }
                }

            }

        }
        public void sendMailwithoutattachment(string ToMail, string CcMail, string InvoiceNo, MailMessage message)
        {
            try
            {
                Company company = new Company();
                var comp = db.companys.FirstOrDefault();
                if (comp != null)
                {
                    company.CPName = comp.CPName;
                    company.CompanyID = comp.CompanyID;
                    company.CPEmail = comp.CPEmail;

                    if (comp.SMTPHost != null && comp.SMTPPort != null && comp.SMTPUsername != null && comp.SMTPPassword != null)
                    {
                        company.SMTPHost = comp.SMTPHost;
                        company.SMTPPort = comp.SMTPPort;
                        company.SMTPUsername = comp.SMTPUsername;
                        company.SMTPPassword = comp.SMTPPassword;
                        company.EnableSsl = comp.EnableSsl;
                    }
                    else
                    {
                        company.SMTPHost = "outlook.office365.com";
                        company.SMTPPort = 993;
                        company.SMTPUsername = "qucknet@quicknet.me";
                        company.SMTPPassword = Environment.GetEnvironmentVariable("Smtp__Password");  // SECURITY: no baked secret — owner sets Smtp__Password env/user-secret
                        company.EnableSsl = true;
                    }

                    message.From = new MailAddress(company.SMTPUsername);
                    string[] Toemail = ToMail.Split(' ');
                    foreach (string word in Toemail)
                    {
                        message.To.Add(word);

                    }
                    if (CcMail != "")
                    {
                        string[] Ccemail = CcMail.Split(' ');
                        foreach (string word in Ccemail)
                        {
                            message.CC.Add(word);

                        }
                    }
               //     message.Attachments.Add(new Attachment(new MemoryStream(bytes), company.CPName + InvoiceNo + "_" + System.DateTime.Now.ToShortDateString() + ".pdf"));
                    message.IsBodyHtml = true;

                    SmtpClient mySmtpClient = new SmtpClient();


                    mySmtpClient.Host = company.SMTPHost;
                    mySmtpClient.Port = Convert.ToInt32(company.SMTPPort);
                    mySmtpClient.EnableSsl = Convert.ToBoolean(company.EnableSsl);
                    mySmtpClient.UseDefaultCredentials = false;
                    mySmtpClient.Credentials = new System.Net.NetworkCredential(company.SMTPUsername, company.SMTPPassword);

                    //added solution for The remote certificate is invalid according to the validation procedure.
                    //ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                    //                     | SecurityProtocolType.Tls11
                    //                     | SecurityProtocolType.Tls12;
                    mySmtpClient.Send(message);

                }
            }
            catch (SmtpException ex)
            {
                string msg = "Mail cannot be sent because of the server problem:";
                msg += ex.Message;
                //log.Debug("Error: Inside catch block of Mail sending");
                //log.Error("Error msg:" + ex);
                ////log.Error("Stack trace:" + ex.StackTrace);
                //return Task.FromResult(0);
            }
        }

        private void sendMail(string ToMail, string CcMail, string InvoiceNo, byte[] bytes, MailMessage message)
        {
            try
            {
                Company company = new Company();
                var comp = db.companys.FirstOrDefault();
                if (comp != null)
                {
                    company.CPName = comp.CPName;
                    company.CompanyID = comp.CompanyID;
                    company.CPEmail = comp.CPEmail;

                    if (comp.SMTPHost != null && comp.SMTPPort != null && comp.SMTPUsername != null && comp.SMTPPassword != null)
                    {
                        company.SMTPHost = comp.SMTPHost;
                        company.SMTPPort = comp.SMTPPort;
                        company.SMTPUsername = comp.SMTPUsername;
                        company.SMTPPassword = comp.SMTPPassword;
                        company.EnableSsl = comp.EnableSsl;
                    }
                    else
                    {
                        company.SMTPHost = "outlook.office365.com";
                        company.SMTPPort = 993;
                        company.SMTPUsername = "qucknet@quicknet.me";
                        company.SMTPPassword = Environment.GetEnvironmentVariable("Smtp__Password");  // SECURITY: no baked secret — owner sets Smtp__Password env/user-secret
                        company.EnableSsl = true;
                    }

                    message.From = new MailAddress(company.SMTPUsername);
                    string[] Toemail = ToMail.Split(' ');
                    foreach (string word in Toemail)
                    {
                        message.To.Add(word);

                    }
                    if (CcMail != "")
                    {
                        string[] Ccemail = CcMail.Split(' ');
                        foreach (string word in Ccemail)
                        {
                            message.CC.Add(word);

                        }
                    }
                    message.Attachments.Add(new Attachment(new MemoryStream(bytes), company.CPName + InvoiceNo + "_" + System.DateTime.Now.ToShortDateString() + ".pdf"));
                    message.IsBodyHtml = true;

                    SmtpClient mySmtpClient = new SmtpClient();
                
                
                    mySmtpClient.Host = company.SMTPHost;
                    mySmtpClient.Port = Convert.ToInt32(company.SMTPPort);
                    mySmtpClient.EnableSsl = Convert.ToBoolean(company.EnableSsl);
                    mySmtpClient.UseDefaultCredentials = false;
                    mySmtpClient.Credentials = new System.Net.NetworkCredential(company.SMTPUsername, company.SMTPPassword);

                    //added solution for The remote certificate is invalid according to the validation procedure.
                    //ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                    //                     | SecurityProtocolType.Tls11
                    //                     | SecurityProtocolType.Tls12;
                    mySmtpClient.Send(message);

                }
            }
            catch (SmtpException ex)
            {
                string msg = "Mail cannot be sent because of the server problem:";
                msg += ex.Message;
                //log.Debug("Error: Inside catch block of Mail sending");
                //log.Error("Error msg:" + ex);
                ////log.Error("Stack trace:" + ex.StackTrace);
                //return Task.FromResult(0);
            }
        }

        //for download pdf
        public virtual byte[] DownloadPdf(StringBuilder sb, string HFCheck = "inactive")
        {                   

            //dynamically adddded//need change
            var HeaderFooterID = 1;
            var headefoot = db.CompanyHeaders.Find(HeaderFooterID);

            // headefoot = null;
            if ((headefoot.Header != "" || headefoot.Footer != "") && (headefoot.Header != null || headefoot.Footer != null))
            {
                Document pdfDoc = new Document(PageSize.A4, 20, 20, 112, 90);
                //with header and footer images
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);

                    writer.ViewerPreferences = PdfWriter.PageModeUseOutlines;
                    PdfHeaderFooter PageEventHandler = new PdfHeaderFooter();

                    writer.PageEvent = PageEventHandler;

                    if ((headefoot.Header != "" && headefoot.Header != null) && (HFCheck == "inactive"))
                    {
                        PageEventHandler.Title = Image.GetInstance(LegacyWeb.MapPath("/uploads/companyheader/header/" + headefoot.Header));
                    }
                    if ((headefoot.Footer != "" && headefoot.Footer != null) && (HFCheck == "inactive"))
                    {
                         PageEventHandler.FTitle = Image.GetInstance(LegacyWeb.MapPath("/uploads/companyheader/footer/" + headefoot.Footer));
                        //PageEventHandler.FTitle = Image.GetInstance(LegacyWeb.MapPath("/uploads/companyheader/footer/zealfooter.jpg"));
                    }

                    pdfDoc.Open();
                    StringReader sr = new StringReader(sb.ToString().Replace("<br>", "<br/>"));
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                    pdfDoc.Close();
                    byte[] bytes = memoryStream.ToArray();
                    memoryStream.Close();
                    return memoryStream.ToArray();
                }
            }
            else
            {

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    //step 1
                    using (Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 110f, 10f))
                    {

                        // step 2
                        PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, memoryStream);
                        pdfWriter.PageEvent = new Helpers.ITextEvents();

                        //open the stream 
                        pdfDoc.Open();
                        StringReader sr = new StringReader(sb.ToString());
                        XMLWorkerHelper.GetInstance().ParseXHtml(pdfWriter, pdfDoc, sr);
                        pdfDoc.Close();
                        byte[] bytes = memoryStream.ToArray();
                        memoryStream.Close();
                        return memoryStream.ToArray();

                    }
                }
            }
        }
        public string sendsms(string mobileno,  string message)
        {
            try
            {
                var config = (from a in db.companys
                              select new
                              {
                                  a.smssenderid,
                                  a.username,
                                  a.password
                              }).FirstOrDefault();
                    var client = new WebClient();
                    string url = "https://rslr.connectbind.com:8443/bulksms/bulksms?username=" +
                        config.username + "&password=" + config.password + "&type=0&dlr=1&destination=971" +
                        mobileno + "&source=" + config.smssenderid + "&message=" + message;
                    var content = client.DownloadString(url);
                    var data = content.Split('|').ToArray();
                    if (data[0] == "1701")
                    {
                    return "Success";
                    }
                    else
                    {
                    return "failed";
                    }
                }
            
            catch (Exception ex)
            {
                return "failed";
            }
        }

        // for text mail
        public void sendMail  (string ToMail, string CcMail, MailMessage message)
        {
            try
            {
                Company company = new Company();
                var comp = db.companys.FirstOrDefault();
                if (comp != null)
                {
                    company.CPName = comp.CPName;
                    company.CompanyID = comp.CompanyID;
                    company.CPEmail = comp.CPEmail;

                    if (comp.SMTPHost != null && comp.SMTPPort != null && comp.SMTPUsername != null && comp.SMTPPassword != null)
                    {
                        company.SMTPHost = comp.SMTPHost;
                        company.SMTPPort = comp.SMTPPort;
                        company.SMTPUsername = comp.SMTPUsername;
                        company.SMTPPassword = comp.SMTPPassword;
                        company.EnableSsl = comp.EnableSsl;
                    }
                    else
                    {
                        company.SMTPHost = "quicksoft.me";
                        company.SMTPPort = 25;
                        company.SMTPUsername = "app@quicksoft.me";
                        company.SMTPPassword = Environment.GetEnvironmentVariable("Smtp__Password");  // SECURITY: no baked secret — owner sets Smtp__Password env/user-secret
                        company.EnableSsl = true;
                    }

                    message.From = new MailAddress(company.SMTPUsername);
                    string[] Toemail = ToMail.Split(' ');
                    foreach (string word in Toemail)
                    {
                        message.To.Add(word);

                    }
                    if (CcMail != "")
                    {
                        string[] Ccemail = CcMail.Split(' ');
                        foreach (string word in Ccemail)
                        {
                            message.CC.Add(word);

                        }
                    }
                   
                    message.IsBodyHtml = true;

                    SmtpClient mySmtpClient = new SmtpClient();
                    mySmtpClient.Host = company.SMTPHost;
                    mySmtpClient.Port = Convert.ToInt32(company.SMTPPort);
                    mySmtpClient.EnableSsl = Convert.ToBoolean(company.EnableSsl);
                    mySmtpClient.UseDefaultCredentials = true;
                    mySmtpClient.Credentials = new System.Net.NetworkCredential(company.SMTPUsername, company.SMTPPassword);

                    //added solution for The remote certificate is invalid according to the validation procedure.
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                    mySmtpClient.Send(message);
                }
            }
            catch (SmtpException ex)
            {
                string msg = "Mail cannot be sent because of the server problem:";
                msg += ex.Message;
                //log.Debug("Error: Inside catch block of Mail sending");
                //log.Error("Error msg:" + ex);
                ////log.Error("Stack trace:" + ex.StackTrace);
                //return Task.FromResult(0);
            }
        }


        public int AdminMail(string ToMail, string Subject="", string CcMail = "")
        {
            SendMail sm = new SendMail();
            MailMessage message = new MailMessage();
            message.Subject = Subject == ""?"Tax Invoice": Subject;
            message.Body = "<p>DEAR SIR</p><p>Thank you for Contacting.</p>" +
                " <p>we are enclosing our Tax Invoice for the items / services as requested by you during our discussions.<br/></p> " +
                " <p>Looking forward to hear from you.</p>";
            sendMail(ToMail, CcMail, message);
            return 0;
        }
    }
}