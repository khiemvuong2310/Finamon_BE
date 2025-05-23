using AutoMapper;
using Finamon_Data.Entities;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;

namespace Finamon.Service.Mapping
{
    public class CommentMappingProfile : Profile
    {
        public CommentMappingProfile()
        {
            CreateMap<Comment, CommentResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                .ForMember(dest => dest.BlogTitle, opt => opt.MapFrom(src => src.Blog != null ? src.Blog.Title : null));

            CreateMap<CommentRequest, Comment>();
            CreateMap<CommentUpdateRequest, Comment>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content)); // Ensure only content is mapped for update
        }
    }
} 