using AirAware.Data;
using Microsoft.EntityFrameworkCore;

namespace AirAware;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        // Apply migrations automatically on startup
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                // Migrations are generated for PostgreSQL (production on Render).
                // For the SQLite local-dev fallback, build the schema from the model
                // directly since the relational migrations are provider-specific.
                if (context.Database.IsNpgsql())
                {
                    logger.LogInformation("Applying database migrations...");
                    context.Database.Migrate();
                    logger.LogInformation("Database migrations applied successfully.");
                }
                else
                {
                    logger.LogInformation("Ensuring local database is created...");
                    context.Database.EnsureCreated();
                    logger.LogInformation("Local database ready.");
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
                throw; // Re-throw to prevent the app from starting with a broken database
            }
        }
        
        host.Run();
    }
    
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}