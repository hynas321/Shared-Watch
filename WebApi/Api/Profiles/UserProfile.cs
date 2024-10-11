using AutoMapper;
using WebApi.Api.DTO;
using WebApi.Application.Constants;
using WebApi.Core.Entities;

namespace WebApi.Api.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDTO>()
            .ForMember(dest => dest.Username, src => src.MapFrom(x => x.Username))
            .ForMember(dest => dest.IsAdmin, src => src.MapFrom(x => x.Role == Role.Admin));
    }
}

