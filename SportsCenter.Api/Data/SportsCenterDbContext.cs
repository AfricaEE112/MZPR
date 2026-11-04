using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Models;

namespace SportsCenter.Api.Data;

public class SportsCenterDbContext(DbContextOptions<SportsCenterDbContext> options)
    : DbContext(options)
{
    public DbSet<ActivityType> ActivityTypes { get; set; } = null!;
    public DbSet<Registration> Registrations { get; set; } = null!;
    public DbSet<ClassSession> ClassSessions { get; set; } = null!;
}