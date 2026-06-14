using AirAware.Data;
using AirAware.Middleware;
using AirAware.Services;
using Microsoft.OpenApi.Models;

namespace AirAware;

public class Startup
{
    private const string CorsPolicyName = "FrontendPolicy";

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddScoped<IAqiCalculator, AqiCalculator>();
        services.AddDbContext<AppDbContext>();

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                var origins = Configuration.GetValue<string>("AllowedOrigins")?.Split(
                        ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    ?? Array.Empty<string>();

                if (origins.Length > 0)
                    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
                else
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "AirAware API", Version = "v1" });

            var apiKeyScheme = new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "X-API-KEY",
                Type = SecuritySchemeType.ApiKey,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            };

            c.AddSecurityDefinition("ApiKey", apiKeyScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { apiKeyScheme, Array.Empty<string>() }
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        if (!env.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AirAware API v1"));
        }

        app.UseCors(CorsPolicyName);

        app.UseMiddleware<ApiKeyAuthMiddleware>();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}"
            );
        });
    }
}