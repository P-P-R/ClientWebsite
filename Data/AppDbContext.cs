using Microsoft.EntityFrameworkCore;

namespace CityStyle.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Gör det omöjligt att ha två bokningar på samma datum + tid
        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.Date, b.Time })
            .IsUnique();
    }
}
