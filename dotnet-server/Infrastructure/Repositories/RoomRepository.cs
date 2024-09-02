using DotnetServer.Api.DTO;
using DotnetServer.Core.Entities;
using DotnetServer.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetServer.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _appData;

    public RoomRepository(AppDbContext appData)
    {
        _appData = appData;
    }

    public async Task<bool> AddRoomAsync(Room room)
    {
        bool roomExists = await _appData.Rooms
            .AnyAsync(r => r.RoomSettings.RoomName == room.RoomSettings.RoomName);

        if (roomExists)
        {
            return false;
        }

        await _appData.Rooms.AddAsync(room);
        await _appData.SaveChangesAsync();

        return true;
    }

    public async Task<Room> DeleteRoomAsync(string roomHash)
    {
        Room room = await GetRoomAsync(roomHash);

        if (room == null)
        {
            return null;
        }

        _appData.Rooms.Remove(room);
        await _appData.SaveChangesAsync();

        return room;
    }

    public async Task<bool> UpdateRoomAsync(Room room)
    {
        _appData.Rooms.Update(room);
        await _appData.SaveChangesAsync();
        return true;
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
                room.RoomSettings.RoomPassword == "" ? RoomTypes.Public : RoomTypes.Private,
                room.Users.Count(),
                room.RoomSettings.MaxUsers
            ))
            .ToListAsync();
    }
}