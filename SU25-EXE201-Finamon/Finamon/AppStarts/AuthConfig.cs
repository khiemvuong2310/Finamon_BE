using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Finamon.AppStarts
{
    public static class AuthConfig
    {
        public static void ConfigureAuthService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            var secrectKey = configuration["Jwt:Key"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrectKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            })
            //.AddCookie()
            //.AddGoogle(GoogleDefaults.AuthenticationScheme, googleOptions =>
            //{
            //    IConfigurationSection googleAuthNSection = configuration.GetSection("GoogleKeys");

            //    googleOptions.ClientId = googleAuthNSection["ClientId"];
            //    googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];

            //    // Optional: Set a custom callback path if needed
            //    googleOptions.CallbackPath = "/dang-nhap-tu-google";
            //}) 
            ;
            services.AddAuthorization();
        }
    }
}
