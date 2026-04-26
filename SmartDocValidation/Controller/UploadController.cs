using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartDocValidation.Agents;
using SmartDocValidation.Services;

namespace SmartDocValidation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly PdfService _pdfService;
        private readonly OcrService _ocrService;

        private readonly LlmService _llmService;

        private readonly ExcelService _excelService;
        private readonly ResolutionAgent _resolutionAgent;
        private readonly MailAgent _mailAgent;

        public UploadController(
            PdfService pdfService,
            OcrService ocrService,
            LlmService llmService,
            ExcelService excelService,
            ResolutionAgent resolutionAgent,
            MailAgent mailAgent
        )
        {
            _pdfService = pdfService;
            _ocrService = ocrService;
            _llmService = llmService;
            _excelService = excelService;
            _resolutionAgent = resolutionAgent;
            _mailAgent = mailAgent;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // Save uploaded file
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Step 1: PDF → Images
            var images = _pdfService.ConvertPdfToImages(filePath);

            // Step 2: Images → OCR Text
            var text = _ocrService.ExtractText(images);
            var excelData = _excelService.ReadExcel("data.xlsx");
            var structured = await _llmService.ExtractFields(text);
            Dictionary<string, object>? extractedDict = null;

            if (structured is JsonElement jsonElement)
            {
                extractedDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    jsonElement.GetRawText()
                );
            }
            else if (structured is string str)
            {
                try
                {
                    extractedDict = JsonSerializer.Deserialize<Dictionary<string, object>>(str);
                }
                catch
                {
                    return Ok(new { error = "Invalid JSON string from LLM", structured });
                }
            }

            if (extractedDict == null)
            {
                return Ok(new { error = "Failed to convert structured data", structured });
            }

            var result = await _resolutionAgent.Resolve(extractedDict, excelData);
            if (result is not null)
            {
                var status = result.GetType().GetProperty("status")?.GetValue(result)?.ToString();

                string subject =
                    status == "SUCCESS"
                        ? "Document Validation Success"
                        : "Document Validation Failed";

                await _mailAgent.SendMail(
                    subject,
                    $@"
                    Document Validation Result

                    Status: {status}

                    Policy Number: {extractedDict["policy_number"]}
                    Name: {extractedDict["name"]}
                    Email: {extractedDict["email"]}
                    Company: {extractedDict["company_name"]}

                    Result Details:
                    {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}
                    "
                );
            }
            return Ok(
                new
                {
                    rawText = text,
                    structuredData = structured,
                    excelData = excelData,
                    resolutionResult = result,
                }
            );
        }

        public class ExtractedData
        {
            public string policy_number { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public string company_name { get; set; }
        }
    }
}
