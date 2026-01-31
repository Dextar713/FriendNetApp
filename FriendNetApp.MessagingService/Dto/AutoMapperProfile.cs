using AutoMapper;
using FriendNetApp.MessagingService.Models;

namespace FriendNetApp.MessagingService.Dto
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<MessageDto, Message>().ReverseMap();
            CreateMap<ChatDto, Chat>();
            CreateMap<UserDto, UserReplica>().ReverseMap();
            CreateMap<Chat, ChatDto>()
                .ForMember(d => d.User1, opt => opt.MapFrom(s => s.User1))
                .ForMember(d => d.User2, opt => opt.MapFrom(s => s.User2))
                .ForMember(d => d.Messages, opt => opt.MapFrom(s => s.Messages));
        }
    }

}
