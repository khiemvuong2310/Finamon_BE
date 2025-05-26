using AutoMapper;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon_Data.Entities;

namespace Finamon.Service.Mapping
{
    public class KeywordProfile : Profile
    {
        public KeywordProfile()
        {
            CreateMap<CreateKeywordRequest, Keyword>();
            CreateMap<UpdateKeywordRequest, Keyword>();
            CreateMap<Keyword, KeywordResponse>();
        }
    }
}