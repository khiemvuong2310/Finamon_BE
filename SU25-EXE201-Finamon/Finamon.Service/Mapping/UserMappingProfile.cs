using AutoMapper;
using Finamon_Data.Entities;
using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;

namespace Finamon.Service.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => 
                    string.Join(", ", src.UserRoles.Where(ur => ur.Status).Select(ur => ur.Role.Name))))
                .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src => 
                    src.UserRoles.Where(ur => ur.Status).Select(ur => new UserRoleResponseNameModel { RoleName = ur.Role.Name })));

            CreateMap<User, UserDetailResponse>()
                .IncludeBase<User, UserResponse>()
                .ForMember(dest => dest.TotalExpenses, opt => opt.MapFrom(src => src.Expenses.Count))
                .ForMember(dest => dest.TotalExpenseAmount, opt => opt.MapFrom(src => src.Expenses.Sum(e => e.Amount)))
                .ForMember(dest => dest.TotalReports, opt => opt.MapFrom(src => src.Reports.Count))
                .ForMember(dest => dest.TotalChatSessions, opt => opt.MapFrom(src => src.ChatSessions.Count))
                .ForMember(dest => dest.HasActiveMembership, opt => opt.MapFrom(src => 
                    src.UserMemberships.Any(um => um.EndDate > DateTime.Now)));

            CreateMap<UserRequest, User>();
            CreateMap<UserUpdateRequest, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
} 