using AutoMapper;
using ChatAppModels;
using ChatBackend.ViewModels;

namespace ChatBackend
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<Message, MessageModel>().ReverseMap();
            CreateMap<User, UserModel>().ReverseMap();
            CreateMap<User, AuthModel>().ReverseMap();
            CreateMap<User, RegisterModel>().ReverseMap();
        }
    }
}