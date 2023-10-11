using AutoMapper;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.MappingProfiles;
public class MessageProfile : Profile
{
    public MessageProfile()
    {
        CreateMap<Message, MessageDto>().ReverseMap();

        CreateMap<MessageCreateDto, Message>().ReverseMap();
    }
}
