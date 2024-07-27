using AutoMapper;
using DotnetServer.Api.DTO;
using DotnetServer.Core.Entities;

namespace DotnetServer.Api.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDTO>()
            .ForMember(dest => dest.Username, src => src.MapFrom(x => x.Username))
            .ForMember(dest => dest.IsAdmin, src => src.MapFrom(x => x.IsAdmin));
    }
}

