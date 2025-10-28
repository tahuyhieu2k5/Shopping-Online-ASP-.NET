using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Services.AIservices;

namespace Shopping_Tutorial.Controllers
{
    public class ChatController : Controller
    {
        private readonly AIService _ai;
        private readonly DataContext _db;

        public ChatController(AIService ai, DataContext db)
        {
            _ai = ai;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ChatRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Message required");

            // Lấy dữ liệu thật từ database (ví dụ: bảng Sản phẩm)
            var products = await _db.Products
                .Select(p => new { p.Name, p.Price, p.Description })
                .Take(10)
                .ToListAsync();

            var reply = await _ai.GetChatResponseAsync(dto.Message, products);
            return Ok(new { reply });
        }
    }

    public class ChatRequestDto
    {
        public string Message { get; set; }
    }
}
