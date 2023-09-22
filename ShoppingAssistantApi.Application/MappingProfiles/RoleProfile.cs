using AutoMapper;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.MappingProfiles;
public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<Role, RoleDto>().ReverseMap();

        CreateMap<RoleCreateDto, Role>().ReverseMap();
    }
}