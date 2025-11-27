using AutoMapper;
using SmartKasir.Application.DTOs;
using SmartKasir.Core.Entities;

namespace SmartKasir.Application.Mappings;

/// <summary>
/// AutoMapper profile untuk mapping entities ke DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();

        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, 
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));

        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ProductCount, 
                opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));

        // Transaction mappings
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.CashierName, 
                opt => opt.MapFrom(src => src.Cashier != null ? src.Cashier.Username : string.Empty))
            .ForMember(dest => dest.Items, 
                opt => opt.MapFrom(src => src.Items));

        // TransactionItem mappings
        CreateMap<TransactionItem, TransactionItemDto>()
            .ForMember(dest => dest.ProductName, 
                opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));
    }
}
