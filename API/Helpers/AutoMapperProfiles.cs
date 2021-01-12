using System;
using System.Linq;
using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles(IHttpContextAccessor httpContextAccessor)
        {
            //var username = GetUserName(httpContextAccessor);

            CreateMap<AppUser, MemberDto>()
                .ForMember(
                    dest => dest.PhotoUrl,
                    opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(
                    dest => dest.Age,
                    opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge())
                )
                .ForMember(
                    dest => dest.Photos,
                    opt => opt.MapFrom(u =>u.Photos.Where(p => p.IsModerated /*u.UserName == username*/))
                );

            CreateMap<Photo, PhotoDto>();
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();

            CreateMap<Message, MessageDto>()
                .ForMember(
                    dest => dest.SenderPhotoUrl,
                    opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(
                    dest => dest.RecipientPhotoUrl,
                    opt => opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));

            CreateMap<Photo, UnmoderatedPhotoDto>()
                .ForMember(
                    dest => dest.UserName,
                    opt => opt.MapFrom(src => src.AppUser.UserName)
                );

           // CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
        }

        private string GetUserName(IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}