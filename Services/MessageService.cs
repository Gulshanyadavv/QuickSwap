using AutoMapper;
using Microsoft.EntityFrameworkCore;
using O_market.DTO;
using O_market.DTOs;
using O_market.Models;
using O_market.Repositories;
using O_market.Services;

namespace O_market.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _repo;
        private readonly IMapper _mapper;
        private readonly OlxdbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public MessageService(
            IMessageRepository repo,
            IMapper mapper,
            OlxdbContext context,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _repo = repo;
            _mapper = mapper;
            _context = context;
            _env = env;
        }

        public async Task<PagedList<MessageResponseDto>> GetThreadAsync(MessageThreadDto threadDto, int currentUserId)
        {
            var pagedMessages = await _repo.GetThreadAsync(threadDto, currentUserId);
            var dtos = _mapper.Map<List<MessageResponseDto>>(pagedMessages.Items);

            foreach (var dto in dtos)
            {
                // FIXED: Use loaded entities from repo (Includes ensure Sender/Receiver/Ad populated)
                var messageEntity = pagedMessages.Items.First(m => m.Id == dto.Id);
                dto.SenderUsername = messageEntity.Sender?.Username ?? "Unknown Sender";
                dto.ReceiverUsername = messageEntity.Receiver?.Username ?? "Unknown Receiver";
                if (dto.AdId.HasValue)
                {
                    dto.AdTitle = messageEntity.Ad?.Title ?? "No Ad";  // From Include in repo
                }
            }

            return new PagedList<MessageResponseDto>(dtos, pagedMessages.TotalCount, pagedMessages.Page, pagedMessages.PageSize);
        }
        public async Task<PagedList<MessageResponseDto>> GetMessagesForAdAsync(
            int adId,
            int currentUserId,
            int page,
            int pageSize,
            int? otherUserId = null)
        {
            // 1️⃣ Get ad (seller)
            var ad = await _context.Ads
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == adId);

            if (ad == null)
                throw new ArgumentException("Ad not found");

            var sellerId = ad.UserId;

            // 2️⃣ Resolve 'Other User'
            // If explicit otherUserId provided (Seller perspective opening a specific conversation), use it.
            // If NOT provided AND current user is NOT the seller, then the 'other user' is the seller (Buyer perspective).
            // If NOT provided AND current user IS the seller, then we have an ambiguous request (return empty or error).

            var resolvedOtherId = otherUserId ?? (currentUserId == sellerId ? null : sellerId);

            if (resolvedOtherId == null && currentUserId == sellerId)
            {
                // Ambiguous seller request - return empty list instead of crashing or leaking other threads
                return new PagedList<MessageResponseDto>(new List<MessageResponseDto>(), 0, page, pageSize);
            }

            // 3️⃣ Reuse existing thread logic
            var threadDto = new MessageThreadDto
            {
                AdId = adId,
                OtherUserId = resolvedOtherId ?? 0, // Fallback to 0 if null, though handled above
                Page = page,
                PageSize = pageSize
            };

            return await GetThreadAsync(threadDto, currentUserId);
        }



        public async Task<MessageResponseDto> SendMessageAsync(MessageCreateDto dto, int senderId)
        {
            if (senderId == dto.ReceiverId) throw new ArgumentException("Cannot message self.");

            var message = _mapper.Map<Message>(dto);
            message.SenderId = senderId;
            message.SentAt = DateTime.Now; // Fixed: Use local time as requested by user

            // 📂 Handle Attachment
            if (dto.Attachment != null)
            {
                var folderPath = Path.Combine(_env.WebRootPath, "uploads", "chat");
                Directory.CreateDirectory(folderPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Attachment.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Attachment.CopyToAsync(stream);
                }

                message.AttachmentUrl = $"/uploads/chat/{fileName}";
                var ext = Path.GetExtension(dto.Attachment.FileName).ToLower();
                message.AttachmentType = (ext == ".jpg" || ext == ".png" || ext == ".jpeg" || ext == ".gif") ? "image" : "file";
            }

            var sent = await _repo.SendAsync(message);

            // Reload with full details
            var fullMessage = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Ad)
                .FirstAsync(m => m.Id == sent.Id);

            return _mapper.Map<MessageResponseDto>(fullMessage);
        }

        public async Task<List<InboxChatDto>> GetInboxAsync(int currentUserId)
        {
            return await _repo.GetInboxAsync(currentUserId);
        }
    }
}