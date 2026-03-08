using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using O_market.DTO;
using O_market.DTOs;
using O_market.Hubs;
using O_market.Models;
using O_market.Services;
using System.Security.Claims;

namespace O_market.Controllers
{
    [ApiController]
    [Route("api/messages")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _service;
        private readonly IHubContext<ChatHub> _hub;

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public MessagesController(
            IMessageService service,
            IHubContext<ChatHub> hub)
        {
            _service = service;
            _hub = hub;
        }

        // =====================================================
        // ✅ OLX-STYLE: Get chat by AdId ONLY
        // Backend resolves seller automatically
        // =====================================================
        [HttpGet("ad/{adId}")]
        public async Task<IActionResult> GetChatByAd(
            int adId,
            [FromQuery] int? otherUserId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var messages = await _service.GetMessagesForAdAsync(
                adId,
                CurrentUserId,
                page,
                pageSize,
                otherUserId
            );

            return Ok(messages);
        }

        // =====================================================
        // ✅ SEND MESSAGE (OLX STYLE)
        // Receiver resolved in service
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> Send([FromForm] MessageCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1️⃣ Save message
            var message = await _service.SendMessageAsync(dto, CurrentUserId);

            // 2️⃣ Sender username
            var senderUsername =
                User.FindFirst(ClaimTypes.Name)?.Value ?? "User";

            // 3️⃣ Realtime SignalR broadcast (per ad)
            await _hub.Clients
                .Group($"ad-{message.AdId}")
                .SendAsync("ReceiveMessage", new
                {
                    id = message.Id,
                    adId = message.AdId,
                    senderId = message.SenderId,
                    receiverId = message.ReceiverId,
                    content = message.Content,
                    sentAt = message.SentAt,
                    senderUsername,
                    attachmentUrl = message.AttachmentUrl,
                    attachmentType = message.AttachmentType
                });

            return Ok(message);
        }

        // =====================================================
        // ✅ KEEP EXISTING THREAD ENDPOINTS (OPTIONAL / ADMIN)
        // =====================================================
        [HttpGet("thread")]
        public async Task<ActionResult<PagedList<MessageResponseDto>>> GetThread(
            [FromQuery] MessageThreadDto threadDto)
        {
            var messages = await _service.GetThreadAsync(
                threadDto,
                CurrentUserId
            );

            return Ok(messages);
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> Inbox()
        {
            var inbox = await _service.GetInboxAsync(CurrentUserId);
            return Ok(inbox);
        }

        [HttpGet("thread/{adId}/{otherUserId}")]
        public async Task<ActionResult<PagedList<MessageResponseDto>>> GetThreadDirect(
            int adId,
            int otherUserId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var threadDto = new MessageThreadDto
            {
                AdId = adId,
                OtherUserId = otherUserId,
                Page = page,
                PageSize = pageSize
            };

            var messages = await _service.GetThreadAsync(
                threadDto,
                CurrentUserId
            );

            return Ok(messages);
        }
    }
}
