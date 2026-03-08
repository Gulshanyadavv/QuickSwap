using O_market.DTO;
using O_market.DTOs;
using O_market.Models;

namespace O_market.Services
{
    public interface IMessageService
    {
        Task<PagedList<MessageResponseDto>> GetThreadAsync(
            MessageThreadDto threadDto,
            int currentUserId
        );

        Task<MessageResponseDto> SendMessageAsync(
            MessageCreateDto dto,
            int senderId
        );

        Task<PagedList<MessageResponseDto>> GetMessagesForAdAsync(
            int adId,
            int currentUserId,
            int page,
            int pageSize,
            int? otherUserId = null
        );

        Task<List<InboxChatDto>> GetInboxAsync(int currentUserId);

    }
}
