using AutoMapper;
using JewerlyBack.Dto;
using JewerlyBack.Models;

namespace JewerlyBack.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Catalog mappings
        CreateMap<JewelryCategory, JewelryCategoryDto>();
        CreateMap<Material, MaterialDto>();
        CreateMap<StoneType, StoneTypeDto>();
        CreateMap<JewelryBaseModel, JewelryBaseModelDto>();

        // Configuration stones and engravings
        CreateMap<JewelryConfigurationStone, ConfigurationStoneDto>()
            .ForMember(dest => dest.StoneTypeName, opt => opt.MapFrom(src => src.StoneType.Name));

        CreateMap<JewelryConfigurationEngraving, ConfigurationEngravingDto>();

        // Configuration list item - flatten navigation properties
        CreateMap<JewelryConfiguration, JewelryConfigurationListItemDto>()
            .ForMember(dest => dest.BaseModelName, opt => opt.MapFrom(src => src.BaseModel.Name))
            .ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material.Name));

        // Configuration detail - include related entities
        CreateMap<JewelryConfiguration, JewelryConfigurationDetailDto>()
            .ForMember(dest => dest.BaseModel, opt => opt.MapFrom(src => src.BaseModel))
            .ForMember(dest => dest.Material, opt => opt.MapFrom(src => src.Material))
            .ForMember(dest => dest.Stones, opt => opt.MapFrom(src => src.Stones))
            .ForMember(dest => dest.Engravings, opt => opt.MapFrom(src => src.Engravings))
            .ForMember(dest => dest.Assets, opt => opt.MapFrom(src => src.Assets));

        // Configuration create request to entity
        CreateMap<JewelryConfigurationCreateRequest, JewelryConfiguration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "draft"))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedPrice, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.BaseModel, opt => opt.Ignore())
            .ForMember(dest => dest.Material, opt => opt.Ignore())
            .ForMember(dest => dest.Stones, opt => opt.Ignore())
            .ForMember(dest => dest.Engravings, opt => opt.Ignore())
            .ForMember(dest => dest.Assets, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

        // Uploaded assets
        CreateMap<UploadedAsset, UploadedAssetDto>();

        // Orders
        CreateMap<Order, OrderListItemDto>();

        CreateMap<Order, OrderDetailDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ConfigurationName,
                opt => opt.MapFrom(src => src.Configuration.Name ?? "Unnamed Configuration"))
            .ForMember(dest => dest.PreviewImageUrl,
                opt => opt.MapFrom(src => src.Configuration.BaseModel.PreviewImageUrl));
    }
}
