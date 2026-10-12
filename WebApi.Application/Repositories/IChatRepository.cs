using WebApi.Core.Entities;
namespace WebApi.Application.Repositories;
public interface IChatRepository
{
    Task<bool> AddChatMessageAsync(string roomHash, ChatMessage chatMessage, CancellationToken cancellationToken);
}
