using DietiEstate.Shared.Constants;
using DietiEstate.Shared.Enums;
using DietiEstate.WebApi.Configs;
using DietiEstate.WebApi.Data;
using DietiEstate.WebApi.Data.Seeders;
using DietiEstate.WebApi.Handlers;
using DietiEstate.WebApi.Middlewares;
using DietiEstate.WebApi.Repositories.Implementations;
using DietiEstate.WebApi.Repositories.Interfaces;
using DietiEstate.WebApi.Services.Implementations;
using DietiEstate.WebApi.Services.Interfaces;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace DietiEstate.WebApi;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Env.Load();
        
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);

        ConfigureAuthentication(builder);
        ConfigureAuthorization(builder);
        
        var app = builder.Build();
        await ConfigureApplicationAsync(app);

        await app.RunAsync();
    }
    
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<DietiEstateDbContext>(options =>
        {
            options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING"), dboptions =>
            {
                dboptions.MapEnum<UserRole>("user_role")
                    .EnableRetryOnFailure();
                dboptions.MapEnum<WorkItemStatus>("work_item_status")
                    .EnableRetryOnFailure();
                dboptions.MapEnum<WorkItemType>("work_item_type")
                    .EnableRetryOnFailure();
                dboptions.EnableRetryOnFailure(0);
            });
        }, ServiceLifetime.Transient);

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        });
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")!));
        
        builder.Services.AddControllers()
            .AddNewtonsoftJson();
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<JwtSecuritySchemeTransformer>();
        });
        
        builder.Services.AddScoped<DatabaseSeeder>();
        
        builder.Services.AddScoped<IListingRepository, ListingRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserVerificationRepository, UserVerificationRepository>();
        
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IPasswordService, BCryptPasswordService>();
        builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
        builder.Services.AddScoped<IUserSessionService, RedisSessionService>();
        builder.Services.AddScoped<IUserService, UserService>();
        
        builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
        
        builder.Services.Configure<AuthConfig>(
            builder.Configuration.GetSection("Authentication"));
        
        builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
    }

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        
        var jwtConfig = new JwtConfiguration(
            Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "secret",
            Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "secret",
            Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "secret",
            int.Parse(Environment.GetEnvironmentVariable("JWT_ID_MINUTES_EXPIRY") ?? "60"),
            int.Parse(Environment.GetEnvironmentVariable("JWT_ACCESS_MINUTES_EXPIRY") ?? "15"),
            int.Parse(Environment.GetEnvironmentVariable("JWT_REFRESH_DAYS_EXPIRY") ?? "30")
        );
        builder.Services.AddSingleton(jwtConfig);
        
        var authConfig = builder.Configuration.GetSection("Authentication").Get<AuthConfig>();
        if (authConfig?.BypassAuth == true)
        {
            builder.Services.AddAuthentication("BypassAuth")
                .AddScheme<AuthenticationSchemeOptions, BypassAuthHandler>("BypassAuth", null);
        }
        else
        {
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(jwtOptions =>
                {
                    jwtOptions.TokenValidationParameters = jwtConfig.GetTokenValidationParameters();
                    jwtOptions.MapInboundClaims = false;
                });
        }
    }
    
    private static void ConfigureAuthorization(WebApplicationBuilder builder)
    {
        var authConfig = builder.Configuration.GetSection("Authentication").Get<AuthConfig>();
        if (authConfig?.BypassAuth == true)
        {
            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("ReadListing", policy =>
                    policy.RequireAssertion( _ => true))
                .AddPolicy("WriteListing", policy =>
                    policy.RequireAssertion( _ => true))
                .AddPolicy("DeleteListing", policy =>
                    policy.RequireAssertion( _ => true))
                .AddPolicy("ReadAgent", policy =>
                    policy.RequireAssertion( _ => true))
                .AddPolicy("WriteAgent", policy =>
                    policy.RequireAssertion( _ => true))
                .AddPolicy("DeleteAgent", policy =>
                    policy.RequireAssertion( _ => true))
                .AddPolicy("ReadSupportAdmin", policy =>
                    policy.RequireAssertion( _ => true))
                .AddPolicy("WriteSupportAdmin", policy =>
                    policy.RequireAssertion( _ => true))
                .AddPolicy("DeleteSupportAdmin", policy =>
                    policy.RequireAssertion( _ => true));
        }
        else
        {
            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("SystemAdmin", policy =>
                    policy.RequireClaim("scope", UserScope.SystemAdmin))
                .AddPolicy("ReadListing", policy =>
                    policy.RequireClaim("scope", UserScope.ReadListing))
                .AddPolicy("WriteListing", policy =>
                    policy.RequireClaim("scope", UserScope.WriteListing))
                .AddPolicy("DeleteListing", policy =>
                    policy.RequireClaim("scope", UserScope.DeleteListing))
                .AddPolicy("ReadAgent", policy =>
                    policy.RequireClaim("scope", UserScope.ReadAgent))
                .AddPolicy("WriteAgent", policy =>
                    policy.RequireClaim("scope", UserScope.WriteAgent))
                .AddPolicy("DeleteAgent", policy =>
                    policy.RequireClaim("scope", UserScope.DeleteAgent))
                .AddPolicy("ReadSupportAdmin", policy =>
                    policy.RequireClaim("scope", UserScope.ReadSupportAdmin))
                .AddPolicy("WriteSupportAdmin", policy =>
                    policy.RequireClaim("scope", UserScope.WriteSupportAdmin))
                .AddPolicy("DeleteSupportAdmin", policy =>
                    policy.RequireClaim("scope", UserScope.DeleteSupportAdmin));
        }
    }
    
    private static async Task ConfigureApplicationAsync(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        if (!app.Environment.IsStaging())
        {
            var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DietiEstateDbContext>();
            await dbContext.Database.MigrateAsync();

            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
        }
        
        //app.UseHttpsRedirection();
        app.UseRouting();
        app.UseMiddleware<UserSessionAuthMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
