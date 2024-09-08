using WebApi.Api.DTO;
using WebApi.Core.Entities;
using WebApi.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _appData;

    public RoomRepository(AppDbContext appData)
    {
        _appData = appData;
    }

    public async Task<bool> AddRoomAsync(Room room)
    {
        using var transaction = await _appData.Database.BeginTransactionAsync();

        try
        {
            bool roomExists = await _appData.Rooms
                .AnyAsync(r => r.RoomSettings.RoomName == room.RoomSettings.RoomName);

            if (roomExists)
            {
                return false;
            }

            await _appData.Rooms.AddAsync(room);
            await _appData.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Room> DeleteRoomAsync(string roomHash)
    {
        using var transaction = await _appData.Database.BeginTransactionAsync();

        try
        {
            Room room = await GetRoomAsync(roomHash);

            if (room == null)
            {
                return null;
            }

            _appData.Rooms.Remove(room);
            await _appData.SaveChangesAsync();
            await transaction.CommitAsync();

            return room;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateRoomAsync(Room room)
    {
        using var transaction = await _appData.Database.BeginTransactionAsync();

        try
        {
            _appData.Rooms.Update(room);
            await _appData.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Room> GetRoomAsync(string roomHash)
    {
        return await _appData.Rooms
            .Include(r => r.ChatMessages)
            .Include(r => r.PlaylistVideos)
            .Include(r => r.Users)
            .Include(r => r.RoomSettings)
            .Include(r => r.UserPermissions)
            .Include(r => r.VideoPlayer)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);
    }

    public async Task<List<Room>> GetRoomsAsync()
    {
        return await _appData.Rooms
            .Include(r => r.Users)
            .Include(r => r.RoomSettings)
            .ToListAsync();
    }

    public async Task<IEnumerable<RoomDTO>> GetRoomsDTOAsync()
    {
        return await _appData.Rooms
            .Include(r => r.Users)
            .Include(r => r.RoomSettings)
            .Select(room => new RoomDTO(
                room.Hash,
                room.RoomSettings.RoomName,
                string.IsNullOrEmpty(room.RoomSettings.RoomPassword) ? RoomTypes.Public : RoomTypes.Private,
                room.Users.Count(),
                room.RoomSettings.MaxUsers
            ))
            .ToListAsync();
    }
}
