using AutoMapper;
using Finamon_Data.Entities;
using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;

namespace Finamon.Service.Mapping
{
    public class ExpenseMappingProfile : Profile
    {
        public ExpenseMappingProfile()
        {
            CreateMap<Expense, ExpenseResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.BudgetName, opt => opt.MapFrom(src => src.Budget != null ? $"Budget {src.Budget.Id}" : null))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow.AddHours(7)))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow.AddHours(7)));

            CreateMap<Expense, ExpenseDetailResponse>()
                .IncludeBase<Expense, ExpenseResponse>();

            CreateMap<ExpenseRequest, Expense>();
            CreateMap<ExpenseUpdateRequest, Expense>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Budget, BudgetResponse>();
            CreateMap<Category, CategoryResponse>();
        }
    }
} 