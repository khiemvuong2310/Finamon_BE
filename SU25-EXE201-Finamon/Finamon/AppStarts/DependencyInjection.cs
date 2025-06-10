using Finamon.Repo.Repositories;
using Finamon.Repo.UnitOfWork;
using Finamon.Service.Interfaces;
using Finamon.Service.Mapping;
using Finamon.Service.RequestModel;
using Finamon.Service.Services;
using Finamon_Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Clients;

namespace Finamon.AppStarts
{
    public static class DependencyInjection
    {
        public static void InstallService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            services.AddSingleton<ITwilioRestClient>(new TwilioRestClient("ACCOUNT_SID", "AUTH_TOKEN"));

            //Add Scoped
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IBudgetService, BudgetService>();
            services.AddScoped<IBudgetAlertService, BudgetAlertService>();
            services.AddScoped<IBudgetCategoryService, BudgetCategoryService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IMembershipService, MembershipService>();
            services.AddScoped<IKeywordService, KeywordService>();
            services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
            services.AddScoped<ICategoryAlertService, CategoryAlertService>();

            // Thêm AutoMapper vào dịch vụ
            services.AddAutoMapper(typeof(UserMappingProfile));
            services.AddAutoMapper(typeof(UserRoleMapping));
            services.AddAutoMapper(typeof(ExpenseMappingProfile));
            services.AddAutoMapper(typeof(BudgetMappingProfile));
            services.AddAutoMapper(typeof(CategoryMappingProfile));
            services.AddAutoMapper(typeof(BlogMappingProfile));
            services.AddAutoMapper(typeof(ReportMappingProfile));
            services.AddAutoMapper(typeof(CommentMappingProfile));
            services.AddAutoMapper(typeof(MembershipProfile));
            services.AddAutoMapper(typeof(KeywordProfile));
        }
    }
}
