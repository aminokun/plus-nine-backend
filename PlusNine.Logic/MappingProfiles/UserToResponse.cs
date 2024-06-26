﻿using AutoMapper;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Responses;

namespace PlusNine.Logic.MappingProfiles
{

    public class UserToResponse : Profile
    {
        public UserToResponse()
        {
            CreateMap<User, GetUserResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.Token))
                .ForMember(dest => dest.TokenCreated, opt => opt.MapFrom(src => src.TokenCreated))
                .ForMember(dest => dest.TokenExpires, opt => opt.MapFrom(src => src.TokenExpires));
        }
    }
}
