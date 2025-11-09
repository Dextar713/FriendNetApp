using AutoMapper;
using FriendNetApp.UserProfile.Models;

namespace FriendNetApp.UserProfile.Dto
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AppUser, UserInputDto>().ReverseMap();
            CreateMap<AppUser, UserOutputDto>().ReverseMap();
        }
    }
}
