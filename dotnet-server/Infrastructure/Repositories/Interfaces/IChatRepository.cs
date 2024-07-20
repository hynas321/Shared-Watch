using DotnetServer.Core.Entities;

namespace DotnetServer.Infrastructure.Repositories;

public interface IChatRepository
{
    bool AddChatMessage(string roomHash, ChatMessage chatMessage);
}