using AutoMapper;
using Finamon.Service.ReponseModel;
using Finamon_Data.Entities;
using System;

namespace Finamon.Service.Mapping
{
    public class BlogMappingProfile : Profile
    {
        public BlogMappingProfile()
        {
            CreateMap<Blog, BlogResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title != null ? src.Title.Trim() : string.Empty))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content != null ? src.Content.Trim() : string.Empty))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate ?? DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));

            CreateMap<PagedBlogResponse, PagedBlogResponse>();
        }
    }
} 