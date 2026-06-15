using iTextSharp.text;
using iTextSharp.text.pdf;
using QuickSoft.Models;
using System;
using System.Linq;



namespace QuickSoft.Helpers
{
    public class ITextEvents : PdfPageEventHelper
    {
        ApplicationDbContext db = new ApplicationDbContext();

        // This is the contentbyte object of the writer
        PdfContentByte cb;

        // we will put the final number of pages in a template
        PdfTemplate headerTemplate, footerTemplate;

        // this is the BaseFont we are going to use for the header / footer
        BaseFont bf = null;

        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;

        #region Fields
        private string _header;
        #endregion

        #region Properties
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }
        #endregion

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                PrintTime = DateTime.Now;
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb = writer.DirectContent;
                headerTemplate = cb.CreateTemplate(100, 100);
                footerTemplate = cb.CreateTemplate(8, 8);
            }
            catch (DocumentException de)
            {
            }
            catch (System.IO.IOException ioe)
            {
            }
        }

        public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            base.OnEndPage(writer, document);
            iTextSharp.text.Font baseFontNormal = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 11f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font baseFontBig = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
            
            var cdetails = db.companys.Select(s => new
                           {
                               CName = s.CPName,
                               CAddress = s.CPAddress,
                               CEmail = s.CPEmail,
                               CTaxRegNo = s.TRN,
                               CPhone = s.CPPhone,
                               s.CPMobile,
                               CLogo = s.CPLogo,
                           }).FirstOrDefault();
            
            Phrase p1Header = new Phrase(cdetails.CName, baseFontBig);

            //Create PdfTable object
            PdfPTable pdfTab = new PdfPTable(3);
          

            //We will have to create separate cells to include image logo and 2 separate strings
            //Row 1
            PdfPCell pdfCell1 = new PdfPCell();
            PdfPCell pdfCell2 = new PdfPCell(p1Header);
            PdfPCell pdfCell3 = new PdfPCell();

            String text = "Page " + writer.PageNumber + " of ";
            bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
           // bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\fonts\ArialUni.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            ////Add paging to footer
            {
                cb.BeginText();
                cb.SetFontAndSize(bf, 8);
                cb.SetTextMatrix(document.PageSize.GetLeft(10), document.PageSize.GetBottom(5));
                cb.ShowText(text);
                cb.EndText();
                float len = bf.GetWidthPoint(text, 8);//12
                cb.AddTemplate(footerTemplate, document.PageSize.GetLeft(10) + len, document.PageSize.GetBottom(5));
            }

            var phone = "";
            var trn = "";
            if (cdetails.CPhone != null)
            {
                phone = cdetails.CPhone;
                if (cdetails.CPMobile != null)
                {
                    phone = phone+"," + cdetails.CPMobile;
                }
            }
            else
            {
                if (cdetails.CPMobile != null)
                {
                    phone = cdetails.CPMobile;
                }
            }
            if (cdetails.CTaxRegNo != null)
            {
                trn = "\n TRN : " + cdetails.CTaxRegNo;
            }

                var comdetails = cdetails.CAddress + "\n" + phone + "\n" + cdetails.CEmail + trn;

            //Row 2
            PdfPCell pdfCell4 = new PdfPCell(new Phrase(comdetails, baseFontNormal));


            //set the alignment of all three cells and set border to 0
            pdfCell1.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell2.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell3.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell4.HorizontalAlignment = Element.ALIGN_CENTER;


            pdfCell2.VerticalAlignment = Element.ALIGN_BOTTOM;
            pdfCell3.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfCell4.VerticalAlignment = Element.ALIGN_TOP;


            pdfCell4.Colspan = 3;

            pdfCell1.Border = 0;
            pdfCell2.Border = 0;
            pdfCell3.Border = 0;
            pdfCell4.Border = 0;


            //add all three cells into PdfTable
            pdfTab.AddCell(pdfCell1);
            pdfTab.AddCell(pdfCell2);
            pdfTab.AddCell(pdfCell3);
            pdfTab.AddCell(pdfCell4);


            pdfTab.TotalWidth = document.PageSize.Width - 80f;
            pdfTab.WidthPercentage = 70;
            //pdfTab.HorizontalAlignment = Element.ALIGN_CENTER;    

            //call WriteSelectedRows of PdfTable. This writes rows from PdfWriter in PdfTable
            //first param is start row. -1 indicates there is no end row and all the rows to be included to write
            //Third and fourth param is x and y position to start writing
            pdfTab.WriteSelectedRows(0, -1, 40, document.PageSize.Height - 10, writer.DirectContent);
            //set pdfContent value


            // Move the pointer and draw line to separate header section from rest of page
           // cb.MoveTo(20, document.PageSize.Height - 40);//40,80
            // cb.LineTo(document.PageSize.Width - 40, document.PageSize.Height - 110);
            cb.Rectangle(10f, 18f, document.PageSize.Width - 20, document.PageSize.Height - 25);
            cb.Stroke();

            //Move the pointer and draw line to separate footer section from rest of page
            //cb.MoveTo(40, document.PageSize.GetBottom(30));
            //cb.LineTo(document.PageSize.Width - 40, document.PageSize.GetBottom(30));
            //cb.Stroke();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            headerTemplate.BeginText();
            headerTemplate.SetFontAndSize(bf, 12);
            headerTemplate.SetTextMatrix(0, 0);
            headerTemplate.ShowText((writer.PageNumber - 1).ToString());
            headerTemplate.EndText();

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(bf, 8);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText((writer.PageNumber).ToString());
            footerTemplate.EndText();
        }
    }
}