using AirAware.Models;
using Microsoft.EntityFrameworkCore;

namespace AirAware.Data;

public class AppDbContext: DbContext
{
    public DbSet<Station> Stations { get; set; }
    public DbSet<Reading> Readings { get; set; }
    public DbSet<AqiRecord> AqiRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => 
        optionsBuilder.UseSqlite("DataSource=app.db;Cache=Shared");
}