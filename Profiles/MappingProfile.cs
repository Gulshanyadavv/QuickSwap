// File: O_market/Profiles/MappingProfile.cs
using AutoMapper;
using O_market.DTO;
using O_market.DTOs;
using O_market.DTOs.Auth;
using O_market.Models;

namespace O_market.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserRegistrationDto, User>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Otp, opt => opt.Ignore())
                .ForMember(dest => dest.OtpExpiry, opt => opt.Ignore())
                .ForMember(dest => dest.IsVerified, opt => opt.Ignore());

            CreateMap<User, AuthResponseDto>();

            // Ad Mappings
            CreateMap<Ad, AdResponseDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.ImageUrls, opt => opt.Ignore())
                .ForMember(dest => dest.IsSellerVerified, opt => opt.MapFrom(src => src.User.IsVerified))
                .ForMember(dest => dest.PostedTimeAgo, opt => opt.Ignore())
                .ForMember(dest => dest.ShortLocation, opt => opt.Ignore())
                .ForMember(dest => dest.IsFavorited, opt => opt.Ignore())
                .ForMember(dest => dest.Highlights, opt => opt.Ignore());



            CreateMap<AdCreateDto, Ad>();
            CreateMap<AdUpdateDto, Ad>(MemberList.None);

            CreateMap<Ad, AdResponseWithDynamicDto>()
                .IncludeBase<Ad, AdResponseDto>();

            CreateMap<AdCreateWithDynamicDto, Ad>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // Category Mappings
            CreateMap<Category, CategoryResponseDto>()
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore());

            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>(MemberList.None);

            CreateMap<Category, CategoryWithFieldsDto>()
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
                .ForMember(dest => dest.DynamicFields, opt => opt.Ignore());

            // Message Mappings
            CreateMap<Message, MessageResponseDto>()
                .ForMember(dest => dest.SenderUsername, opt => opt.MapFrom(src => src.Sender.Username))
                .ForMember(dest => dest.ReceiverUsername, opt => opt.MapFrom(src => src.Receiver.Username))
                .ForMember(dest => dest.AdTitle, opt => opt.MapFrom(src => src.Ad.Title));

            CreateMap<MessageCreateDto, Message>();

            // Favorite Mappings
            CreateMap<Favorite, FavoriteResponseDto>()
                .ForMember(dest => dest.Ad, opt => opt.MapFrom(src => src.Ad));

            // Dynamic Field Mappings
            CreateMap<DynamicField, DynamicFieldDto>();
            CreateMap<DynamicField, DynamicFieldResponseDto>();

            CreateMap<DynamicFieldCreateDto, DynamicField>();
            CreateMap<DynamicFieldUpdateDto, DynamicField>(MemberList.None);
        }


    }
}