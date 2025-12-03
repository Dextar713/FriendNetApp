using AutoMapper;
using FriendNetApp.MessagingService.Models;

namespace FriendNetApp.MessagingService.Dto
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<MessageDto, Message>().ReverseMap();
            CreateMap<ChatDto, Chat>().ReverseMap();
            CreateMap<UserDto, UserReplica>().ReverseMap();
        }
    }

}
