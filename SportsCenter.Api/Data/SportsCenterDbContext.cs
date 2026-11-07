using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Models;

namespace SportsCenter.Api.Data;

/// <summary>Надає доступ до наборів сутностей спортивного центру в базі даних.</summary>
/// <param name="options">Параметри конфігурації контексту бази даних.</param>
public class SportsCenterDbContext(DbContextOptions<SportsCenterDbContext> options)
    : DbContext(options)
{
    /// <summary>Отримує або задає набір типів занять.</summary>
    public DbSet<ActivityType> ActivityTypes { get; set; } = null!;

    /// <summary>Отримує або задає набір реєстрацій відвідувачів.</summary>
    public DbSet<Registration> Registrations { get; set; } = null!;

    /// <summary>Отримує або задає набір занять у розкладі.</summary>
    public DbSet<ClassSession> ClassSessions { get; set; } = null!;

    /// <summary>Отримує або задає набір записів про відвідуваність.</summary>
    public DbSet<Attendance> Attendances { get; set; } = null!;
}