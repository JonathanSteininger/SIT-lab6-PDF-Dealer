using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Geom;
using iText.Kernel.Colors;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Diagnostics;
using Lab1_playingcardLibrary;
using iText.IO.Image;
using iText.Kernel.Pdf.Colorspace;
using iText.Layout.Borders;

namespace forms
{
    public class PdfHandler
    {

        public void CreatePdfFromhand(PlayingHand hand, string filename)
        {
            
            Document doc = setupBasics(filename);

            PdfFont mainFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);


            Div div = new Div();
            int i = 0;
            foreach(PlayingCard card in hand.Cards)
            {

                iText.Layout.Element.Image img = new iText.Layout.Element.Image(ImageDataFactory.Create(card.GetFace, System.Drawing.Color.White));
                img.SetWidth(75);
                img.SetHeight(107);

                int cards = 27;
                img.SetFixedPosition((20) * (i%cards), (PageSize.A4.GetHeight() - 107 - 30) - (120 * (i/cards)));
                i++;
                doc.Add(img);
            }
            doc.Close();
        }

        private Document setupBasics(string filename)
        {
            WriterProperties properties = new WriterProperties();
            properties.AddXmpMetadata();
            properties.SetPdfVersion(PdfVersion.PDF_2_0);
            if(!File.Exists(filename))
            {
                string[] temp = filename.Split('/');
                StringBuilder sb = new StringBuilder();
                for( int i = 0; i < temp.Length - 1; i++ )
                {
                    sb.Append(temp[i]);
                    sb.Append("/");
                }
                string final = sb.ToString();

                if(!Directory.Exists(final)) Directory.CreateDirectory(final);
            }
            PdfWriter writer = new PdfWriter(filename, properties);

            PdfDocument pdf = new PdfDocument(writer);

            PdfDocumentInfo info = pdf.GetDocumentInfo();
            info.SetAuthor("Jonathan Steininger");
            info.SetTitle("testPdfs");
            info.SetSubject("Practice");

            return new Document(pdf, PageSize.A4);
        }
    }
}
