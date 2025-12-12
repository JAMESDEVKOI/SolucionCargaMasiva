using Auth.Application.Abstractions.Interfaces.Caching;
using Auth.Application.Abstractions.Interfaces.Common;
using Auth.Application.Abstractions.Interfaces.Data;
using Auth.Application.Abstractions.Interfaces.Identity;
using Auth.Application.Abstractions.Interfaces.Repositories;
using Auth.Application.Abstractions.Interfaces.Sessions;
using Auth.Domain.Interface;
using Auth.Domain.User;
using Auth.Infrastructure.Caching.Redis;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Identity.Authentication.Cookie;
using Auth.Infrastructure.Identity.Authentication.Jwt;
using Auth.Infrastructure.Identity.Authorization;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Persistence.Interceptors;
using Auth.Infrastructure.Persistence.Repositories;
using Auth.Infrastructure.Services;
using Auth.Infrastructure.Sessions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

namespace Auth.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddPersistence(configuration);
            services.AddCaching(configuration);
            services.AddAuthentication(configuration);
            services.AddAuthorizationPolicies();
            services.AddServices();
            services.AddRepositories();

            return services;
        }

        private static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Database")
                ?? throw new InvalidOperationException("Database connection string is not configured");

            services.AddSingleton<UpdateAuditableEntitiesInterceptor>();
            services.AddSingleton<PublishDomainEventsInterceptor>();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var auditableInterceptor = sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
                var domainEventsInterceptor = sp.GetRequiredService<PublishDomainEventsInterceptor>();

                options.UseNpgsql(connectionString)
                    //.UseSnakeCaseNamingConvention()
                    .AddInterceptors(auditableInterceptor, domainEventsInterceptor);
            });

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

            services.AddSingleton<ISqlConnectionFactory>(_ =>
                new SqlConnectionFactory(connectionString));

            return services;
        }

        private static IServiceCollection AddCaching(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var redisSettings = new RedisSettings();
            configuration.GetSection(nameof(RedisSettings)).Bind(redisSettings);

            services.AddSingleton(redisSettings);

            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisSettings.ConnectionString));

            services.AddSingleton<ICacheService, RedisCacheService>();

            return services;
        }

        private static IServiceCollection AddAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<JwtSettings>(
                configuration.GetSection(nameof(JwtSettings)));

            services.Configure<CookieSettings>(
                configuration.GetSection(nameof(CookieSettings)));

            var jwtSettings = configuration
                .GetSection(nameof(JwtSettings))
                .Get<JwtSettings>() ?? new JwtSettings();

            var cookieSettings = configuration
                .GetSection(nameof(CookieSettings))
                .Get<CookieSettings>() ?? new CookieSettings();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[cookieSettings.AccessTokenCookieName];
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddScoped<IJwtProvider, JwtTokenProvider>();
            services.AddScoped<ICookieAuthenticationProvider, CookieAuthenticationProvider>();

            return services;
        }

        private static IServiceCollection AddAuthorizationPolicies(
            this IServiceCollection services)
        {
            services.AddAuthorization();

            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

            return services;
        }

        private static IServiceCollection AddServices(
            this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<ISessionManager, SessionManager>();
            services.AddSingleton<IRefreshTokenService, RefreshTokenService>();

            return services;
        }

        private static IServiceCollection AddRepositories(
            this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserQueryRepository, UserQueryRepository>();

            return services;
        }
    }
}
