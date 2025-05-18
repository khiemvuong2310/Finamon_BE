using Finamon.Repo.Repositories;
using Finamon.Repo.UnitOfWork;
using Finamon.Service.Interfaces;
using Finamon.Service.Mapping;
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

            ////Add DbContext
            //services.AddDbContext<AppDbContext>(options =>
            //{
            //    var connectionString = configuration.GetConnectionString("DefaultConnection");
            //    var serverVersion = new MySqlServerVersion(new Version(8, 0, 2));
            //    options.UseMySql(connectionString, serverVersion);
            //});

            //Add Scoped
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IExpenseService, ExpenseService>();

            // Thêm AutoMapper vào dịch vụ
            services.AddAutoMapper(typeof(UserMappingProfile));
            services.AddAutoMapper(typeof(UserRoleMapping));
            services.AddAutoMapper(typeof(ExpenseMappingProfile));
        }
    }
}
