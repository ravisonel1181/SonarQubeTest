using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NEHGSWebApiCore.Application.Common.Mappings;
using NEHGSWebApiCore.Application.Interfaces.Repositories;
using NEHGSWebApiCore.Application.Interfaces.Security;
using NEHGSWebApiCore.Application.Interfaces.Services;
using NEHGSWebApiCore.Application.Services;
using NEHGSWebApiCore.Domain.Entities;
using NEHGSWebApiCore.Infrastructure.BackgroundTask;
using NEHGSWebApiCore.Infrastructure.Data;
using NEHGSWebApiCore.Infrastructure.Repositories;
using NEHGSWebApiCore.Infrastructure.Security;
using Serilog;

namespace NEHGSWebApiCore.Api.Middleware
{
    public static class ServiceExtensionsMiddleware
    {
        // Register Services
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Database
            services.AddDbContext<ApplicationDbContext>(
                options =>
                {
                    options.UseSqlServer(
                        configuration.GetConnectionString(
                            "DefaultConnection"));
                });

            // Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // JWT tokens
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["JwtToken:Issuer"],
                        ValidAudience = configuration["JwtToken:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["JwtToken:SecretKey"]))
                    };
            });

            // Mapster
            MappingConfig.RegisterMappings();

            services.AddSingleton<IProgressService, ProgressService>();
            services.AddSingleton<ITokenRevocationService, TokenRevocationService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<ImportBackgroundService>();

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
            services.AddScoped<IImportService, ImportService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFileStorageService, FileStorageService>();

            return services;
        }

        // Middleware Pipeline
        public static WebApplication UseApplicationMiddleware(this WebApplication app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}
