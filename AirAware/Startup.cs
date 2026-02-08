using AirAware.Data;
using AirAware.Services;
using AirAware.Utils;

namespace AirAware;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddScoped<IAqiCalculator, AqiCalculator>();
        services.AddDbContext<AppDbContext>();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        Logger.LogApplicationStartup(logger, env.EnvironmentName);
        
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            Logger.LogConfiguration(logger, "Environment", "Development mode enabled with exception page");
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}"
            );
        });
        
        Logger.LogConfiguration(logger, "Routing", "Endpoints configured successfully");
    }
}