using Finamon.AppStarts;
using Finamon_Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Text.Json.Serialization;
using DotNetEnv;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http.Features; // Thêm dòng này


namespace Finamon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 1. Load environment variables
            Env.Load();

            var builder = WebApplication.CreateBuilder(args);

            // 2. Copy email templates to output directory
            var templatesSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
            var templatesDestPath = Path.Combine(builder.Environment.ContentRootPath, "Templates");

            if (!Directory.Exists(templatesDestPath))
            {
                Directory.CreateDirectory(templatesDestPath);
                foreach (var file in Directory.GetFiles(templatesSourcePath, "*.html"))
                {
                    File.Copy(file, Path.Combine(templatesDestPath, Path.GetFileName(file)), true);
                }
            }

            // 3. Add services to the container
            builder.Services.InstallService(builder.Configuration);
            builder.Services.ConfigureAuthService(builder.Configuration);

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = null;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = builder.Configuration["DB_CONNECTION_STRING"];
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 2));
                options.UseMySql(connectionString, serverVersion);
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "https://finamon.pages.dev")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Upload config
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; // 100 MB
            });

            // Swagger & OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Finamon API",
                    Version = "v1"
                });

                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };
                c.AddSecurityDefinition("Bearer", securitySchema);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securitySchema, new[] { "Bearer" } }
            });
            });

            // 4. Firebase configuration
            var firebaseConfig = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");
            if (!string.IsNullOrEmpty(firebaseConfig))
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromJson(firebaseConfig),
                    ProjectId = "pawfund-e7fdd"
                });
            }
            else
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("firebase-adminsdk.json"),
                    ProjectId = "pawfund-e7fdd"
                });
            }

            // 5. Kestrel server config
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5296);
            });

            // 6. Build application
            var app = builder.Build();

            // 7. Configure middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Finamon API v1");
                });
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler();

            app.UseHttpsRedirection();
            app.UseCors("AllowReactApp");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // 8. Run the application
            app.Run();
        }
    }
}
