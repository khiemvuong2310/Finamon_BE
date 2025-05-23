using AutoMapper;
using Finamon_Data.Entities;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;

namespace Finamon.Service.Mapping
{
    public class ImageMappingProfile : Profile
    {
        public ImageMappingProfile()
        {
            CreateMap<Image, ImageResponse>();
            CreateMap<ImageRequest, Image>();
        }
    }
} 