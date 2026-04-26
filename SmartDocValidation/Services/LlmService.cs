using System.Text;
using System.Text.Json;

namespace SmartDocValidation.Services
{
    public class LlmService
    {
        private readonly HttpClient _http = new HttpClient();

        public async Task<object> ExtractFields(string ocrText)
        {
            var prompt = $@"
                You are an API that extracts structured data.

                Return ONLY valid JSON. No explanation.

                Format strictly like this:
                {{
                ""policy_number"": string or null,
                ""name"": string or null,
                ""email"": string or null,
                ""company_name"": string or null
                }}

                OCR Text:
                {ocrText}
            ";

            var body = new
            {
                model = "llama3",
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = 0   // 🔥 forces deterministic output
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync("http://localhost:11434/api/generate", content);
            var result = await response.Content.ReadAsStringAsync();

            // 🔥 Step 1: Parse Ollama response
            var json = JsonDocument.Parse(result);
            var clean = json.RootElement.GetProperty("response").GetString();

            // 🔥 Remove markdown
            clean = clean.Replace("```", "").Trim();

            // 🔥 Extract only JSON part (IMPORTANT)
            int start = clean.IndexOf('{');
            int end = clean.LastIndexOf('}');

            if (start != -1 && end != -1)
            {
                clean = clean.Substring(start, end - start + 1);
            }

            // 🔥 Try parsing safely
            try
            {
                var structuredObj = JsonSerializer.Deserialize<object>(clean);
                return structuredObj!;
            }
            catch
            {
                // 🔥 Try manual extraction (backup)
                var structured = new Dictionary<string, string?>();

                if (clean.Contains("Policy Number"))
                    structured["policy_number"] = Extract(clean, "Policy Number");

                if (clean.Contains("Name"))
                    structured["name"] = Extract(clean, "Name");

                if (clean.Contains("Email"))
                    structured["email"] = Extract(clean, "Email");

                if (clean.Contains("Company"))
                    structured["company_name"] = Extract(clean, "Company");

                return structured;
            }
        }
        private string? Extract(string text, string key)
        {
            var lines = text.Split('\n');

            foreach (var line in lines)
            {
                if (line.ToLower().Contains(key.ToLower()))
                {
                    var parts = line.Split(':');
                    if (parts.Length > 1)
                        return parts[1].Trim();
                }
            }
            return null;
        }
    }

}