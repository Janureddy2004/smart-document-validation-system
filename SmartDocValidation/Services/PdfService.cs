using Docnet.Core;
using Docnet.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SmartDocValidation.Services
{
    public class PdfService
    {
        public List<string> ConvertPdfToImages(string pdfPath)
        {
            var imagePaths = new List<string>();

            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "pdf-images");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            using var docReader = DocLib.Instance.GetDocReader(pdfPath, new PageDimensions(1080, 1920));

            int pageCount = docReader.GetPageCount();

            for (int i = 0; i < pageCount; i++)
            {
                using var pageReader = docReader.GetPageReader(i);

                var rawBytes = pageReader.GetImage(); // BGRA format
                int width = pageReader.GetPageWidth();
                int height = pageReader.GetPageHeight();

                using var image = Image.LoadPixelData<Bgra32>(rawBytes, width, height);

                var imagePath = Path.Combine(outputDir, $"page_{i}.png");
                image.Save(imagePath);

                imagePaths.Add(imagePath);
            }

            return imagePaths;
        }
    }
}