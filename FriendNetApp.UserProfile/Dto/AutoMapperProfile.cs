using AutoMapper;
using FriendNetApp.UserProfile.Models;

namespace FriendNetApp.UserProfile.Dto
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserInputDto, AppUser>().
                ForAllMembers(opts => opts
                    .Condition((src, dest, srcMember) => 
                        srcMember != null));
            CreateMap<AppUser, UserOutputDto>();
        }
    }
}
