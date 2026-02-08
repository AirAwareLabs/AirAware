using AirAware.Data;
using Microsoft.EntityFrameworkCore;

namespace AirAware;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddDbContext<AppDbContext>();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext dbContext)
    {
        // Apply any pending migrations on startup
        dbContext.Database.Migrate();
        
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}"
            );
        });
    }
}