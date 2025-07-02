using AutoMapper;
using Finamon.Service.ReponseModel;
using Finamon_Data.Entities;

namespace Finamon.Service.Mapping
{
    public class SiteAnalyticProfile : Profile
    {
        public SiteAnalyticProfile()
        {
            CreateMap<SiteAnalytic, SiteAnalyticResponse>();
        }
    }
} 