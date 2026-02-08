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
            optionsBuilder.UseSqlite("DataSource=app.db;Cache=Shared");
        }
    }
}