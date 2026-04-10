using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Services;
using System.Text;
using System.Text.Json;

namespace Pharma_Pulse.Pages
{
    [IgnoreAntiforgeryToken]
    public class AiSuggestModel : PharmacyPageModel
    {
        private readonly MedicineService _service;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public AiSuggestModel(MedicineService service, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _service = service;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult OnGet() => new JsonResult(new { ok = true });

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostAsync([FromBody] AiRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Query))
                return new JsonResult(new { reply = "Query empty hai!" });

            // ✅ DB se medicines fetch
            var medicines = _service.GetAllMedicines(CurrentPharmacyId);

            // ✅ Query se match karo
            var query = request.Query.ToLower();
            var matchedMeds = medicines
                .Where(m => m.IsActive && (
                    (!string.IsNullOrEmpty(m.MedicineName) && m.MedicineName.ToLower().Contains(query)) ||
                    (!string.IsNullOrEmpty(m.Category) && m.Category.ToLower().Contains(query))
                ))
                .Take(10)
                .Select(m => $"{m.MedicineName} (Category: {m.Category}, Stock: {m.StockUnits}, Price: ₹{m.SellingPrice})")
                .ToList();

            var medicineContext = matchedMeds.Any()
                ? "Hamare pharmacy mein available medicines:\n" + string.Join("\n", matchedMeds)
                : "Is query ke liye hamare database mein koi medicine nahi mili.";

            // ✅ Groq API call
            var apiKey = _config["GroqApiKey"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var prompt = $@"You are an AI pharmacist assistant for an Indian pharmacy.

User query: ""{request.Query}""

{medicineContext}

Instructions:
- Detect language from query (Hindi/English/Hinglish) and reply in SAME language
- If medicines found in database, recommend from those first
- Suggest top 3 medicines with: Name, dose, when to take
- Keep it concise and practical for pharmacist use
- End with: Doctor se milna zaroori hai agar symptoms 2 din se zyada rahe
- Never suggest prescription-only medicines";

            var body = new
            {
                model = "llama-3.3-70b-versatile",
                max_tokens = 500,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var jsonBody = JsonSerializer.Serialize(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
                var responseStr = await response.Content.ReadAsStringAsync();

                var json = JsonDocument.Parse(responseStr);
                var reply = json.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return new JsonResult(new { reply });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { reply = $"Error: {ex.Message}" });
            }
        }
    }

    public class AiRequest
    {
        public string Query { get; set; }
    }
}