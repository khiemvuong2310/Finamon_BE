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

            // BudgetCategory mappings
            CreateMap<BudgetCategory, BudgetCategoryResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<BudgetCategoryRequestModel, BudgetCategory>();

            // BudgetAlert mappings
            CreateMap<BudgetAlert, BudgetAlertResponse>();
            CreateMap<BudgetAlertRequestModel, BudgetAlert>();
        }
    }
}
