namespace SportsCenter.Api.Models;

/// <summary>Представляє запис про присутність відвідувача на занятті.</summary>
public class Attendance
{
    /// <summary>Отримує або задає унікальний ідентифікатор запису відвідуваності.</summary>
    public int Id { get; set; }

    /// <summary>Отримує або задає ідентифікатор заняття.</summary>
    public int ClassSessionId { get; set; }

    /// <summary>Отримує або задає ідентифікатор реєстрації відвідувача.</summary>
    public int RegistrationId { get; set; }

    /// <summary>Отримує або задає ознаку присутності відвідувача.</summary>
    public bool IsPresent { get; set; } = false;
}