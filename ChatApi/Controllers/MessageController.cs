using ChatApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MessageController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/message/history?user1=1&user2=2
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] string user1, [FromQuery] string user2)
        {
            if (string.IsNullOrEmpty(user1) || string.IsNullOrEmpty(user2))
                return BadRequest("user1 and user2 required");

            var messages = await _db.Messages
                .Where(m =>
                    (m.SenderId == user1 && m.ReceiverId == user2) ||
                    (m.SenderId == user2 && m.ReceiverId == user1))
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            return Ok(messages);
        }

        // optional: mark as read
        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkRead([FromBody] MarkReadDto dto)
        {
            var msgs = await _db.Messages
                .Where(m => m.SenderId == dto.SenderId && m.ReceiverId == dto.ReceiverId && !m.IsRead)
                .ToListAsync();

            foreach (var m in msgs) m.IsRead = true;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }

    public class MarkReadDto
    {
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
    }
}
