using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;


namespace QuickSoft.Helpers
{
    public class PdfHeaderFooter : PdfPageEventHelper
    {
        PdfContentByte cb;
        // we will put the final number of pages in a template
        PdfTemplate template;
        // this is the BaseFont we are going to use for the header / footer
        BaseFont bf = null;
        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;
        #region Properties
        private Image _Title;
        public Image Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        private Image _FTitle;
        public Image FTitle
        {
            get { return _FTitle; }
            set { _FTitle = value; }
        }

        private string _HeaderLeft;
        public string HeaderLeft
        {
            get { return _HeaderLeft; }
            set { _HeaderLeft = value; }
        }
        private string _HeaderRight;
        public string HeaderRight
        {
            get { return _HeaderRight; }
            set { _HeaderRight = value; }
        }
        private Font _HeaderFont;
        public Font HeaderFont
        {
            get { return _HeaderFont; }
            set { _HeaderFont = value; }
        }
        private Font _FooterFont;
        public Font FooterFont
        {
            get { return _FooterFont; }
            set { _FooterFont = value; }
        }
        private string _header;
        public string header
        {
            get { return _header; }
            set { _header = value; }
        }
        private string _footer;
        public string footer
        {
            get { return _footer; }
            set { _footer = value; }
        }

        #endregion
        // we override the onOpenDocument method
        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                PrintTime = DateTime.Now;
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb = writer.DirectContent;
                template = cb.CreateTemplate(50, 50);
                //template = cb.CreateTemplate(273, 95);

            }
#pragma warning disable CS0168 // The variable 'de' is declared but never used
            catch (DocumentException de)
#pragma warning restore CS0168 // The variable 'de' is declared but never used
            {
            }
#pragma warning disable CS0168 // The variable 'ioe' is declared but never used
            catch (System.IO.IOException ioe)
#pragma warning restore CS0168 // The variable 'ioe' is declared but never used
            {
            }
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            base.OnStartPage(writer, document);
            Rectangle pageSize = document.PageSize;

            if (Title != null)
            {
                Title.SetAbsolutePosition(0, 0);
                Title.ScaleAbsolute(570, 105);
                PdfTemplate tp = cb.CreateTemplate(595, 105);
                tp.AddImage(Title);
                cb.AddTemplate(tp, 15, 842 - 110);
            }
        }
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            if (FTitle != null)
            {
                FTitle.SetAbsolutePosition(0, 0);
                FTitle.ScaleAbsolute(570, 75);
                PdfTemplate tp = cb.CreateTemplate(595, 75);
                tp.AddImage(FTitle);
                cb.AddTemplate(tp, 5, 5);
            }

            int pageN = writer.PageNumber;
            String text = "Page " + pageN + " of ";
            float len = bf.GetWidthPoint(text, 8);
            Rectangle pageSize = document.PageSize;
            cb.SetRGBColorFill(100, 100, 100);
            cb.BeginText();
            cb.SetFontAndSize(bf, 8);
            cb.SetTextMatrix(pageSize.GetLeft(20), pageSize.GetBottom(5));
            cb.ShowText(text);
            cb.EndText();
            cb.AddTemplate(template, pageSize.GetLeft(20) + len, pageSize.GetBottom(5));



            cb.BeginText();
            cb.SetFontAndSize(bf, 8);
            cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT,
                "Created On " + PrintTime.ToString(),
                pageSize.GetRight(20),
                pageSize.GetBottom(5), 0);
            cb.EndText();
        }
        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);
            template.BeginText();
            template.SetFontAndSize(bf, 8);
            template.SetTextMatrix(0, 0);
            template.ShowText("" + (writer.PageNumber));
            template.EndText();
        }
    }
}