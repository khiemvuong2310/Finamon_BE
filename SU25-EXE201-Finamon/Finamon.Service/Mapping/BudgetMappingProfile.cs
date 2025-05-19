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

            // Category mappings
            CreateMap<Category, CategoryResponse>();
            CreateMap<CategoryRequestModel, Category>();

            // BudgetAlert mappings
            CreateMap<BudgetAlert, BudgetAlertResponse>();
            CreateMap<BudgetAlertRequestModel, BudgetAlert>();
        }
    }
}
