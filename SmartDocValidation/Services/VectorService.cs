using System.Text;
using System.Text.Json;

namespace SmartDocValidation.Services
{
    public class VectorService
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly string _apiKey;
        private readonly string _indexUrl;

        public VectorService(IConfiguration config)
        {
            _apiKey = config["Pinecone:ApiKey"];
            _indexUrl = config["Pinecone:IndexUrl"];
        }

        // 🔥 Get embedding from Ollama
        public async Task<float[]> GetEmbedding(string text)
        {
            var body = new
            {
                model = "nomic-embed-text",
                prompt = text
            };

            var res = await _http.PostAsync(
                "http://localhost:11434/api/embeddings",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            );

            var result = await res.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(result);

            var embedding = json.RootElement.GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            return embedding;
        }

        // 🔥 Store in Pinecone
        public async Task AddDocument(string text)
        {
            var embedding = await GetEmbedding(text);

            var body = new
            {
                vectors = new[]
                {
                    new
                    {
                        id = Guid.NewGuid().ToString(),
                        values = embedding,
                        metadata = new { text }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_indexUrl}/vectors/upsert");
            request.Headers.Add("Api-Key", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            await _http.SendAsync(request);
        }

        // 🔥 Query Pinecone
        public async Task<string> Search(string query)
        {
            var embedding = await GetEmbedding(query);

            var body = new
            {
                vector = embedding,
                topK = 1,
                includeMetadata = true
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_indexUrl}/query");
            request.Headers.Add("Api-Key", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            var json = JsonDocument.Parse(result);

            var match = json.RootElement
                .GetProperty("matches")[0]
                .GetProperty("metadata")
                .GetProperty("text")
                .GetString();

            return match ?? "";
        }
    }
}