using AutoMapper;
using Finamon.Service.ReponseModel;
using Finamon_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon.Service.Mapping
{
    public class UserRoleMapping : Profile
    {
        public UserRoleMapping()
        {
            // Mapping UserRole -> UserRoleResponseModel
            CreateMap<UserRole, UserRoleResponseModel>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
        }
    }
}
