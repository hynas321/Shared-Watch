using System.Threading;
using System.Threading.Tasks;
using WebApi.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _dbContext;

    public ChatRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AddChatMessageAsync(string roomHash, ChatMessage chatMessage, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.ChatMessages)
            .FirstOrDefaultAsync(r => r.Hash == roomHash, cancellationToken);

        if (room == null)
        {
            return false;
        }

        room.ChatMessages.Add(chatMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
