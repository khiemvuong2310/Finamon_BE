using AutoMapper;
using Finamon_Data.Entities;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;

namespace Finamon.Service.Mapping
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            // Map from Category to CategoryResponse
            CreateMap<Category, CategoryResponse>()
                .ForMember(dest => dest.Expenses, opt => opt.MapFrom(src => src.Expenses))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            // Map from CategoryRequestModel to Category
            CreateMap<CategoryRequestModel, Category>()
                .ForMember(dest => dest.Expenses, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
        }
    }
} 