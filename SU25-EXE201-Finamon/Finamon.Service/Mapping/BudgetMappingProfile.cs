using AutoMapper;
using Finamon_Data.Entities;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;

namespace Finamon.Service.Mapping
{
    public class BudgetMappingProfile : Profile
    {
        public BudgetMappingProfile()
        {
            // Budget mappings
            CreateMap<Budget, BudgetResponse>();
            CreateMap<BudgetRequestModel, Budget>();

            // BudgetDetail mappings
            CreateMap<BudgetDetail, BudgetDetailResponse>()
                .ForMember(dest => dest.BudgetName, opt => opt.MapFrom(src => src.Budget.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<BudgetDetailRequestModel, BudgetDetail>();

            // BudgetAlert mappings
            CreateMap<BudgetAlert, BudgetAlertResponse>();
            CreateMap<BudgetAlertRequestModel, BudgetAlert>();
        }
    }
}
