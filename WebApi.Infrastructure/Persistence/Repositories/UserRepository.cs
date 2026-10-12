using WebApi.Infrastructure.Persistence;
using WebApi.Application.DTO;
using WebApi.Application.Repositories;
using WebApi.Core.Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Constants;
using WebApi.Application.Services;

namespace WebApi.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;
    private readonly IHubConnectionRegistry _hubConnectionRegistry;

    public UserRepository(AppDbContext dbContext, IHubConnectionRegistry hubConnectionRegistry)
    {
        _dbContext = dbContext;
        _hubConnectionRegistry = hubConnectionRegistry;
    }

    public async Task<bool> AddUserAsync(string roomHash, User user, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Hash == roomHash, cancellationToken);

        if (room is null)
        {
            return false;
        }

        room.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<User?> DeleteUserByConnectionIdAsync(string roomHash, string connectionId, CancellationToken cancellationToken)
    {
        var username = _hubConnectionRegistry.GetUserIdByConnectionId(roomHash, connectionId);
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        var room = await _dbContext.Rooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Hash == roomHash, cancellationToken);

        if (room is null)
        {
            return null;
        }

        var user = room.Users?.FirstOrDefault(u => u.Username == username);
        if (user is null)
        {
            return null;
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<User?> DeleteUserByUsernameAsync(string roomHash, string username, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Hash == roomHash, cancellationToken);

        if (room is null)
        {
            return null;
        }

        var user = room.Users.FirstOrDefault(u => u.Username == username);
        if (user is null)
        {
            return null;
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<User?> GetUserAsync(string roomHash, string username, CancellationToken cancellationToken)
    {
        return await _dbContext.Rooms
            .Where(r => r.Hash == roomHash)
            .SelectMany(r => r.Users)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<IEnumerable<UserDTO>> GetUsersDTOAsync(string roomHash, CancellationToken cancellationToken)
    {
        return await _dbContext.Rooms
            .Where(r => r.Hash == roomHash)
            .SelectMany(r => r.Users, (r, u) => new UserDTO(
                u.Username,
                u.Role == Role.Admin
            ))
            .ToListAsync(cancellationToken);
    }
}



