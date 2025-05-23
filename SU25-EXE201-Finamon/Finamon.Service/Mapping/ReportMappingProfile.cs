using AutoMapper;
using Finamon_Data.Entities;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;

namespace Finamon.Service.Mapping
{
    public class ReportMappingProfile : Profile
    {
        public ReportMappingProfile()
        {
            // Map từ Report entity sang ReportResponse
            CreateMap<Report, ReportResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

            // Map từ ReportRequestModel sang Report entity
            CreateMap<ReportRequestModel, Report>();
        }
    }
} 