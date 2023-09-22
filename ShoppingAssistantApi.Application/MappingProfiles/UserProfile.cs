using AutoMapper;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.MappingProfiles;
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
    }
}
