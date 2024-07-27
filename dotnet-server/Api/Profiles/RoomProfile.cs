using AutoMapper;
using DotnetServer.Api.DTO;
using DotnetServer.Core.Entities;

namespace DotnetServer.Api.Profiles;

public class RoomProfile : Profile
{
    public RoomProfile()
    {
        CreateMap<Room, RoomDTO>()
            .ForMember(dest => dest.RoomHash, src => src.MapFrom(x => x.Hash))
            .ForMember(dest => dest.RoomName, src => src.MapFrom(x => x.RoomSettings.RoomName))
            .ForMember(dest => dest.RoomType, src => src.MapFrom(x => x.RoomSettings.RoomType))
            .ForMember(dest => dest.OccupiedSlots, src => src.MapFrom(x => x.Users.Count))
            .ForMember(dest => dest.TotalSlots, src => src.MapFrom(x => x.RoomSettings.MaxUsers));
    }
}

