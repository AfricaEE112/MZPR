using Microsoft.EntityFrameworkCore;
using SportsCenter.Api;
using SportsCenter.Api.Models;

namespace SportsCenter.Api.Data;

public class SportsCenterDbContext : DbContext
{
    public SportsCenterDbContext(DbContextOptions<SportsCenterDbContext> options)
        : base(options)
    {
    }

    public DbSet<ActivityType> ActivityTypes { get; set; } = null!;

    public DbSet<ClassSession> ClassSessions { get; set; } = null!;
    public DbSet<Registration> Registrations { get; set; } = null!;
}