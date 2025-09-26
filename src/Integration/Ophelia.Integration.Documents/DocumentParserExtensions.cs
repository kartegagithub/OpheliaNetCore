using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Ophelia.Integration.Documents
{
    public static class DocumentParserService
    {
        public static string ExtractText(string fileName, byte[] fileContent)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".pdf" => ExtractPdf(fileContent),
                ".docx" => ExtractDocx(fileContent),
                ".xlsx" => ExtractXlsx(fileContent),
                ".xls" => ExtractXlsx(fileContent),
                ".txt" => ExtractTxt(fileContent),
                _ => throw new NotSupportedException($"Desteklenmeyen dosya türü: {ext}")
            };
        }

        private static string ExtractPdf(byte[] fileContent)
        {
            var sb = new StringBuilder();

            using (var reader = new PdfReader(new MemoryStream(fileContent)))
            {
                PdfDocument pdfDoc = new PdfDocument(reader);
                for (var pageNum = 1; pageNum <= pdfDoc.GetNumberOfPages(); pageNum++)
                {
                    var page = pdfDoc.GetPage(pageNum);
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string pageContent = PdfTextExtractor.GetTextFromPage(page, strategy);
                    sb.AppendLine(pageContent);
                }
            }
            return sb.ToString();
        }

        private static string ExtractDocx(byte[] fileContent)
        {
            if (fileContent == null || fileContent.Length == 0)
                throw new ArgumentException("Dosya içeriği boş olamaz.", nameof(fileContent));

            try
            {
                using var memoryStream = new MemoryStream(fileContent);
                using var wordDocument = WordprocessingDocument.Open(memoryStream, false);

                var body = wordDocument.MainDocumentPart?.Document?.Body;
                if (body == null)
                    return string.Empty;

                var textBuilder = new StringBuilder();

                foreach (var paragraph in body.Descendants<Paragraph>())
                {
                    foreach (var text in paragraph.Descendants<Text>())
                    {
                        textBuilder.Append(text.Text);
                    }
                    textBuilder.AppendLine();
                }

                return textBuilder.ToString().Trim();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Word belgesi okunurken hata oluştu: {ex.Message}", ex);
            }
        }

        private static string ExtractXlsx(byte[] fileContent)
        {
            var sb = new StringBuilder();
            using (var stream = new MemoryStream(fileContent))
            {
                var importer = new Ophelia.Data.Importer.ExcelImporter();
                var data = importer.Import(stream);
                foreach (System.Data.DataTable table in data.Tables)
                {
                    foreach (System.Data.DataRow row in table.Rows)
                    {
                        foreach (var item in row.ItemArray)
                        {
                            sb.Append(item?.ToString() + " ");
                        }
                        sb.AppendLine();
                    }
                    return sb.ToString();
                }
                data = null;
            }
            return sb.ToString();
        }

        private static string ExtractTxt(byte[] fileContent)
        {
            return Encoding.UTF8.GetString(fileContent);
        }
    }
}
