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
            Env.Load(); // Tải biến môi trường từ file .env
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.InstallService(builder.Configuration);
            builder.Services.ConfigureAuthService(builder.Configuration);

            //This ensures that users can only access their own information through the GetUserByIdAsync endpoint.
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = null;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddDbContext<AppDbContext>(options => {
                var connectionString = builder.Configuration["DB_CONNECTION_STRING"];
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 2));
                options.UseMySql(connectionString, serverVersion);
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:5173","https://finamon.pages.dev")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer(); // Thay v� AddOpenApi()

            //Add button Authorize 
            builder.Services.AddSwaggerGen(c =>
            {
                //c.OperationFilter<SnakecasingParameOperationFilter>();
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Finamon API",
                    Version = "v1"
                });

                //c.CustomSchemaIds(type => type.FullName); // 🔥 Thêm dòng này

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
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
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
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securitySchema, new string[] { "Bearer" } } });
            });


            // ============= FIREBASE CONFIG (SỬA LẠI) =============
            var firebaseConfig = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");
            if (!string.IsNullOrEmpty(firebaseConfig))
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(firebaseConfig),
                    ProjectId = "pawfund-e7fdd" // Thêm ProjectId
                });
            }
            else
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("firebase-adminsdk.json"),
                    ProjectId = "pawfund-e7fdd" // Thêm ProjectId
                });
            }
            // File upload config (đã có)
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; // 100 MB
            });

            builder.WebHost.ConfigureKestrel(options => {
                options.ListenAnyIP(5296); // Binds to all IPs
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Finamon API v1");
                });
            }
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseExceptionHandler();

            app.UseHttpsRedirection();

            app.UseCors("AllowReactApp");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
