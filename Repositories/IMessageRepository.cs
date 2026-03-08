using O_market.DTO;
using O_market.DTOs;
using O_market.Models;

namespace O_market.Repositories
{
    public interface IMessageRepository
    {
        Task<PagedList<Message>> GetThreadAsync(MessageThreadDto threadDto, int currentUserId);
        Task<Message> SendAsync(Message message);
        Task<bool> ExistsBetweenUsersAsync(int user1Id, int user2Id, int? adId = null);
        Task<List<InboxChatDto>> GetInboxAsync(int currentUserId);
    }
}