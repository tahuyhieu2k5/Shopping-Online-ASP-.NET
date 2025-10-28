using System.Text.Json;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Shopping_Tutorial.Services.AIservices
{
    public class AIService
    {
        private readonly OpenAIClient _client;

        public AIService(IConfiguration config)
        {
            var apiKey = config["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException("API key not found. Check appsettings.json or environment variables.");

            _client = new OpenAIClient(apiKey);
        }

        // ✅ Thêm tham số products để AI hiểu dữ liệu của web
        public async Task<string> GetChatResponseAsync(string userMessage, object products = null)
        {
            var chatClient = _client.GetChatClient("gpt-4o-mini");

            // Ghép dữ liệu website (nếu có)
            string context = "";
            if (products != null)
            {
                var json = JsonSerializer.Serialize(products);
                context = $"\nDưới đây là danh sách sản phẩm từ hệ thống:\n{json}";
            }

            var response = await chatClient.CompleteChatAsync(new ChatMessage[]
            {
                new SystemChatMessage(@$"
Bạn là trợ lý AI của cửa hàng online. Hãy trả lời thân thiện và chính xác.
Nếu người dùng hỏi về sản phẩm, hãy trả lời dựa trên dữ liệu sau:
{context}
Nếu không có thông tin phù hợp, hãy nói rằng hiện bạn chưa có dữ liệu."),
                new UserChatMessage(userMessage)
            });

            return response.Value.Content[0].Text;
        }
    }
}
