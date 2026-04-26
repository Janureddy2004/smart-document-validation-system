using System.Diagnostics;

public class OcrService
{
    public string ExtractText(List<string> imagePaths)
    {
        string finalText = "";

        foreach (var imagePath in imagePaths)
        {
            var outputFile = Path.GetFileNameWithoutExtension(imagePath);

            var process = new Process();
            process.StartInfo.FileName = "tesseract";
            process.StartInfo.Arguments = $"{imagePath} {outputFile} -l eng";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            process.WaitForExit();

            var textFile = outputFile + ".txt";

            if (File.Exists(textFile))
            {
                finalText += File.ReadAllText(textFile) + "\n";
                File.Delete(textFile); // cleanup
            }
        }

        return finalText;
    }
}