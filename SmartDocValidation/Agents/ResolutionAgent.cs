using System.Text;
using System.Text.Json;
using SmartDocValidation.Services;

namespace SmartDocValidation.Agents
{
    public class ResolutionAgent
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly VectorService _vectorService;

        public ResolutionAgent(VectorService vectorService)
        {
            _vectorService = vectorService;
        }

        public async Task<object> Resolve(
            Dictionary<string, object> extracted,
            List<Dictionary<string, string>> excelData
        )
        {
            if (extracted == null)
            {
                return new { status = "FAIL", reason = "Extracted data is null" };
            }

            if (!extracted.ContainsKey("policy_number"))
            {
                return new { status = "FAIL", reason = "Policy number missing in extracted data" };
            }

            var policyNumber = extracted["policy_number"]?.ToString();
            var row = excelData.FirstOrDefault(x => x["PolicyNumber"] == policyNumber);

            if (row == null)
            {
                return new { status = "FAIL", reason = "Policy not found" };
            }

            // 🔥 STEP 2: Query vector DB

            string policy = extracted.ContainsKey("policy_number")
                ? extracted["policy_number"]?.ToString()
                : "";
            string name = extracted.ContainsKey("name") ? extracted["name"]?.ToString() : "";
            string email = extracted.ContainsKey("email") ? extracted["email"]?.ToString() : "";
            string company = extracted.ContainsKey("company_name")
                ? extracted["company_name"]?.ToString()
                : "";

            // 🔥 vector query
            var query = $"Policy {policy} {name}";
            var context = await _vectorService.Search(query);

            // 🔥 exact match
            bool exactMatch =
                name == row["Name"] && email == row["Email"] && company == row["Company"];

            if (exactMatch)
            {
                return new
                {
                    status = "SUCCESS",
                    type = "Exact Match",
                    contextUsed = context,
                };
            }

            var prompt =
                $@"
                You are a validation agent.

                Extracted Data:
                {JsonSerializer.Serialize(extracted)}

                Expected Data:
                {JsonSerializer.Serialize(row)}

                Retrieved Context (from vector DB):
                {context}

                Tasks:
                1. Check if extracted data matches expected
                2. If mismatch, suggest correct values from context

                Rules:
                - Minor spelling differences are OK
                - Email must match exactly

                Return JSON:
                {{
                ""match"": true/false,
                ""reason"": ""short explanation"",
                ""suggested_corrections"": {{
                    ""name"": ""corrected or null"",
                    ""email"": ""corrected or null"",
                    ""company_name"": ""corrected or null""
                }}
                }}";

            var body = new
            {
                model = "llama3",
                prompt = prompt,
                stream = false,
                options = new { temperature = 0 },
            };

            var response = await _http.PostAsync(
                "http://localhost:11434/api/generate",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            );

            var result = await response.Content.ReadAsStringAsync();

            var json = JsonDocument.Parse(result);
            var clean = json.RootElement.GetProperty("response").GetString();

            clean = clean.Replace("```", "").Trim();

            int start = clean.IndexOf('{');
            int end = clean.LastIndexOf('}');

            if (start != -1 && end != -1)
                clean = clean.Substring(start, end - start + 1);

            try
            {
                var decision = JsonSerializer.Deserialize<object>(clean);
                return new
                {
                    status = "LLM_VALIDATED",
                    contextUsed = context,
                    decision,
                };
            }
            catch
            {
                return new
                {
                    status = "MISMATCH",
                    contextUsed = context,
                    raw = clean,
                };
            }
        }
    }
}
