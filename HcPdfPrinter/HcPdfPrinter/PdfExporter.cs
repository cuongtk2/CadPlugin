using System.Collections.Generic;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace HcPdfPrinter
{
    /// <summary>Ghép các trang trong session thành một file PDF, mỗi trang có bookmark theo title.</summary>
    internal static class PdfExporter
    {
        public static void Export(IEnumerable<PageItem> pages, string outPath)
        {
            var sources = new Dictionary<string, PdfDocument>();
            try
            {
                var output = new PdfDocument();
                string? firstTitle = null;

                foreach (var p in pages)
                {
                    if (!sources.TryGetValue(p.SourcePdf, out var src))
                    {
                        src = PdfReader.Open(p.SourcePdf, PdfDocumentOpenMode.Import);
                        sources[p.SourcePdf] = src;
                    }

                    PdfPage page = output.AddPage(src.Pages[p.PageIndex]);
                    if (p.Rotation != 0)
                        page.Rotate = (page.Rotate + p.Rotation) % 360;

                    output.Outlines.Add(p.Title, page, true);
                    firstTitle ??= p.Title;
                }

                output.Info.Title = firstTitle ?? "";
                output.Info.Creator = "HcPdfPrinter";
                output.Save(outPath);
            }
            finally
            {
                foreach (var d in sources.Values) d.Dispose();
            }
        }
    }
}
