using AirAware.Models;
using Microsoft.EntityFrameworkCore;

namespace AirAware.Data;

public class AppDbContext: DbContext
{
    public AppDbContext() { }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Station> Stations { get; set; }
    public DbSet<Reading> Readings { get; set; }
    public DbSet<AqiRecord> AqiRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseNpgsql(NormalizePostgresConnectionString(connectionString));
            }
            else
            {
                // Fallback to SQLite for local development
                var dbPath = Environment.GetEnvironmentVariable("DATABASE_PATH") ?? "app.db";
                optionsBuilder.UseSqlite($"DataSource={dbPath};Cache=Shared");
            }
        }
    }

    // Render exposes PostgreSQL connection strings as URIs (postgres://user:pass@host:port/db),
    // while Npgsql expects key/value pairs. Convert when a URI is provided.
    private static string NormalizePostgresConnectionString(string connectionString)
    {
        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            SslMode = Npgsql.SslMode.Require
        };

        return builder.ConnectionString;
    }
}