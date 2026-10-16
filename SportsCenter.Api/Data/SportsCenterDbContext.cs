using Microsoft.EntityFrameworkCore;
using SportsCenter.Api;

namespace SportsCenter.Api.Data;

public class SportsCenterDbContext : DbContext
{
    public SportsCenterDbContext(DbContextOptions<SportsCenterDbContext> options)
        : base(options)
    {
    }

    public DbSet<ActivityType> ActivityTypes { get; set; } = null!;
}