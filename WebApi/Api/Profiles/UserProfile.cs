﻿using AutoMapper;
using WebApi.Api.DTO;
using WebApi.Core.Entities;

namespace WebApi.Api.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDTO>()
            .ForMember(dest => dest.Username, src => src.MapFrom(x => x.Username));
    }
}

