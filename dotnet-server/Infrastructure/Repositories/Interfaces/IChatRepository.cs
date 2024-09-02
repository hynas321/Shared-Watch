using DotnetServer.Core.Entities;

namespace DotnetServer.Infrastructure.Repositories;

public interface IChatRepository
{
    Task<bool> AddChatMessageAsync(string roomHash, ChatMessage chatMessage);
}