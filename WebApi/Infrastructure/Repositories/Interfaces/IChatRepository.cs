using WebApi.Core.Entities;

namespace WebApi.Infrastructure.Repositories;

public interface IChatRepository
{
    Task<bool> AddChatMessageAsync(string roomHash, ChatMessage chatMessage);
}