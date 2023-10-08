using AutoMapper;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.MappingProfiles;
public class WishlistProfile : Profile
{
    public WishlistProfile()
    {
        CreateMap<Wishlist, WishlistDto>().ReverseMap();

        CreateMap<WishlistCreateDto, Wishlist>().ReverseMap();
    }
}
