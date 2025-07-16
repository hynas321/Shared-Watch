using WebApi.Api.DTO;
using WebApi.Core.Entities;
using WebApi.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _dbContext;

    public RoomRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AddRoomAsync(Room room, CancellationToken cancellationToken)
    {
        bool roomExists = await _dbContext.Rooms
            .AnyAsync(r => r.RoomSettings.RoomName == room.RoomSettings.RoomName, cancellationToken);

        if (roomExists)
        {
            return false;
        }

        await _dbContext.Rooms.AddAsync(room, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<Room> DeleteRoomAsync(string roomHash, CancellationToken cancellationToken)
    {
        Room room = await GetRoomAsync(roomHash, cancellationToken);

        if (room is null)
        {
            return null;
        }

        _dbContext.Rooms.Remove(room);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return room;
    }

    public async Task<bool> UpdateRoomAsync(Room room, CancellationToken cancellationToken)
    {
        _dbContext.Rooms.Update(room);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<Room> GetRoomAsync(string roomHash, CancellationToken cancellationToken)
    {
        return await _dbContext.Rooms
            .Include(r => r.ChatMessages)
            .Include(r => r.PlaylistVideos)
            .Include(r => r.Users)
            .Include(r => r.RoomSettings)
            .Include(r => r.UserPermissions)
            .FirstOrDefaultAsync(r => r.Hash == roomHash, cancellationToken);
    }

    public async Task<Room> GetRoomByNameAsync(string roomName, CancellationToken cancellationToken)
    {
        return await _dbContext.Rooms
            .FirstOrDefaultAsync(r => r.RoomSettings.RoomName == roomName, cancellationToken);
    }

    public async Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Rooms
            .Include(r => r.Users)
            .Include(r => r.RoomSettings)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RoomDTO>> GetRoomsDTOAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Rooms
            .Select(r => new RoomDTO(
                r.Hash,
                r.RoomSettings.RoomName,
                string.IsNullOrEmpty(r.RoomSettings.RoomPassword) ? RoomTypes.Public : RoomTypes.Private,
                r.Users.Count,
                r.RoomSettings.MaxUsers
            ))
            .ToListAsync(cancellationToken);
    }
}
