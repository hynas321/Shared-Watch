using WebApi.Api.DTO;
using WebApi.Core.Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Constants;
using WebApi.Api.SignalR.Interfaces;

namespace WebApi.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;
    private readonly IHubConnectionMapper _hubConnectionMapper;

    public UserRepository(AppDbContext dbContext, IHubConnectionMapper hubConnectionMapper)
    {
        _dbContext = dbContext;
        _hubConnectionMapper = hubConnectionMapper;
    }

    public async Task<bool> AddUserAsync(string roomHash, User user)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var room = await _dbContext.Rooms
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Hash == roomHash);

            if (room == null)
            {
                return false;
            }

            room.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<User> DeleteUserByConnectionIdAsync(string roomHash, string connectionId)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var username = _hubConnectionMapper.GetUserIdByConnectionId(connectionId);

            var room = await _dbContext.Rooms
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Hash == roomHash);

            if (room == null)
            {
                return null;
            }

            var user = room.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return null;
            }

            room.Users.Remove(user);
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return user;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<User> DeleteUserByUsernameAsync(string roomHash, string username)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var room = await _dbContext.Rooms
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Hash == roomHash);

            if (room == null)
            {
                return null;
            }

            var user = room.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return null;
            }

            room.Users.Remove(user);
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return user;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<User> GetUserAsync(string roomHash, string username)
    {
        return await _dbContext.Rooms
            .Include(r => r.Users)
            .Where(r => r.Hash == roomHash)
            .SelectMany(r => r.Users)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<UserDTO>> GetUsersDTOAsync(string roomHash)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);

        return room?.Users.Select(user => new UserDTO(user.Username, user.Role == Role.Admin)) ?? Enumerable.Empty<UserDTO>();
    }
}
