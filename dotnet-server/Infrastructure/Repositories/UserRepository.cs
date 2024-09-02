using DotnetServer.Api.DTO;
using DotnetServer.Core.Entities;
using DotnetServer.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotnetServer.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddUserAsync(string roomHash, User user)
    {
        var room = await _context.Rooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);

        if (room == null)
        {
            return false;
        }

        room.Users.Add(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<User> DeleteUserAsync(string roomHash, string authorizationToken)
    {
        var room = await _context.Rooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);

        if (room == null)
        {
            return null;
        }

        var user = room.Users.FirstOrDefault(u => u.AuthorizationToken == authorizationToken);
        if (user == null)
        {
            return null;
        }

        room.Users.Remove(user);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<(User user, string roomHash)> DeleteUserAsync(string connectionId)
    {
        var room = await _context.Rooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Users.Any(u => u.SignalRConnectionId == connectionId));

        if (room != null)
        {
            var user = room.Users.FirstOrDefault(u => u.SignalRConnectionId == connectionId);
            if (user != null)
            {
                room.Users.Remove(user);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return (user, room.Hash);
            }
        }

        return (null, null);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User> GetUserByAuthorizationTokenAsync(string roomHash, string authorizationToken)
    {
        var room = await _context.Rooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);

        return room?.Users.FirstOrDefault(u => u.AuthorizationToken == authorizationToken);
    }

    public async Task<User> GetUserByAuthorizationTokenAsync(string authorizationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.AuthorizationToken == authorizationToken);

        return user;
    }

    public async Task<User> GetUserByUsernameAsync(string roomHash, string username)
    {
        var room = await _context.Rooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);

        return room?.Users.FirstOrDefault(u => u.Username == username);
    }

    public async Task<User> GetUserByConnectionIdAsync(string connectionId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.SignalRConnectionId == connectionId);
    }

    public async Task<IEnumerable<UserDTO>> GetUsersDTOAsync(string roomHash)
    {
        var room = await _context.Rooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);

        return room?.Users.Select(user => new UserDTO(user.Username, user.IsAdmin)) ?? Enumerable.Empty<UserDTO>();
    }
}
