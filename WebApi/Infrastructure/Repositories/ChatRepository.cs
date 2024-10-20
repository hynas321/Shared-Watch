using WebApi.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddChatMessageAsync(string roomHash, ChatMessage chatMessage)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var room = await _context.Rooms
                .Include(r => r.ChatMessages)
                .FirstOrDefaultAsync(r => r.Hash == roomHash);

            if (room == null)
            {
                return false;
            }

            room.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
