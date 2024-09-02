using DotnetServer.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetServer.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddChatMessageAsync(string roomHash, ChatMessage chatMessage)
    {
        Room room = await _context.Rooms
            .Include(r => r.ChatMessages)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);

        if (room == null)
        {
            return false;
        }

        room.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        return true;
    }
}