using AutoMapper;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.MappingProfiles;
public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();

        CreateMap<ProductCreateDto, Product>().ReverseMap();
    }
}
