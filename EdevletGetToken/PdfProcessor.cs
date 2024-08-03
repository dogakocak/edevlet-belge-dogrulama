using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdevletGetToken
{
    using iText.Kernel.Pdf;
    using iText.Kernel.Pdf.Canvas.Parser;
    using iText.Kernel.Pdf.Canvas.Parser.Listener;
    using System.IO;

    public class PdfProcessor
    {
        public static string ExtractTextFromPdf(byte[] pdfBytes)
        {
            using (var pdfStream = new MemoryStream(pdfBytes))
            {
                using (var pdfReader = new PdfReader(pdfStream))
                {
                    using (var pdfDocument = new PdfDocument(pdfReader))
                    {
                        var strategy = new SimpleTextExtractionStrategy();
                        var pdfText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(1), strategy);
                        return pdfText;
                    }
                }
            }
        }

        public static bool ValidatePdf(string pdfText)
        {
            return !pdfText.Contains("/belge-dogrulama?islem=dogrulama");
        }
    }

}
