namespace SportsCenter.Api.Models;

/// <summary>Представляє реєстрацію відвідувача на заняття.</summary>
public class Registration
{
    /// <summary>Отримує або задає унікальний ідентифікатор реєстрації.</summary>
    public int Id { get; set; }

    /// <summary>Отримує або задає ідентифікатор заняття, на яке зареєстровано відвідувача.</summary>
    public int ClassSessionId { get; set; }

    /// <summary>Отримує або задає ім'я відвідувача.</summary>
    public string VisitorName { get; set; } = string.Empty;

    /// <summary>Отримує або задає дату й час створення реєстрації.</summary>
    public DateTime RegistrationDate { get; set; }

    /// <summary>Отримує або задає стан реєстрації.</summary>
    public string Status { get; set; } = string.Empty;
}