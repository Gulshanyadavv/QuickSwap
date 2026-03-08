using Microsoft.EntityFrameworkCore;
using O_market.DTO;
using O_market.DTOs;  // FIXED: Changed from O_market.DTO to DTOs
using O_market.Models;
using O_market.Repositories;

namespace O_market.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly OlxdbContext _context;

        public MessageRepository(OlxdbContext context) { _context = context; }

        public async Task<PagedList<Message>> GetThreadAsync(MessageThreadDto threadDto, int currentUserId)
        {
            var query = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Ad)
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == threadDto.OtherUserId) ||
                            (m.SenderId == threadDto.OtherUserId && m.ReceiverId == currentUserId));

            if (threadDto.AdId.HasValue) query = query.Where(m => m.AdId == threadDto.AdId);

            var total = await query.CountAsync();
            var messages = await query.OrderBy(m => m.SentAt)
                .Skip((threadDto.Page - 1) * threadDto.PageSize)
                .Take(threadDto.PageSize)
                .ToListAsync();

            return new PagedList<Message>(messages, total, threadDto.Page, threadDto.PageSize);
        }

        public async Task<Message> SendAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<bool> ExistsBetweenUsersAsync(int user1Id, int user2Id, int? adId = null)
        {
            var query = _context.Messages.Where(m =>
                ((m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                 (m.SenderId == user2Id && m.ReceiverId == user1Id)) &&
                (!adId.HasValue || m.AdId == adId));

            return await query.AnyAsync();
        }

        public async Task<List<InboxChatDto>> GetInboxAsync(int currentUserId)
        {
            // 1️⃣ Get raw IDs for unique conversations (AdId + the other person)
            // We pull this to memory handle the GroupBy reliably for SQL Server
            var rawMessages = await _context.Messages
                .Where(m => (m.SenderId == currentUserId || m.ReceiverId == currentUserId) && m.AdId != null)
                .Select(m => new {
                    m.Id,
                    m.AdId,
                    OtherUserId = m.SenderId == currentUserId ? m.ReceiverId : m.SenderId,
                    m.SentAt
                })
                .ToListAsync();

            if (!rawMessages.Any()) return new List<InboxChatDto>();

            // 2️⃣ Find the latest message ID for each unique Chat pair (Ad + OtherPerson)
            var latestIds = rawMessages
                .GroupBy(x => new { x.AdId, x.OtherUserId })
                .Select(g => g.OrderByDescending(x => x.SentAt).First().Id)
                .ToList();

            // 3️⃣ Hydrate full objects for those specific messages
            var conversations = await _context.Messages
                .Include(m => m.Ad)
                    .ThenInclude(a => a.User)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => latestIds.Contains(m.Id))
                .ToListAsync();

            // 4️⃣ Mapping
            var result = conversations.Select(m => new InboxChatDto
            {
                AdId = m.AdId ?? 0,
                AdTitle = m.Ad?.Title ?? "Deleted Ad",
                SellerName = m.Ad?.User?.Username ?? "Unknown",
                OtherUserId = m.SenderId == currentUserId ? m.ReceiverId : m.SenderId,
                OtherUsername = m.SenderId == currentUserId ? (m.Receiver?.Username ?? "Unknown") : (m.Sender?.Username ?? "Unknown"),
                LastMessage = m.Content,
                LastMessageAt = m.SentAt ?? DateTime.Now
            })
            .OrderByDescending(x => x.LastMessageAt)
            .ToList();

            return result;
        }
    }

}