using AutoMapper;
using FriendNetApp.SocialService.Models;

namespace FriendNetApp.SocialService.Dto
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<MatchDto, Match>().ReverseMap();
            CreateMap<FriendshipDto, Friendship>().ReverseMap();
        }
    }
}
