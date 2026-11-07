namespace SportsCenter.Api;

/// <summary>Представляє тип заняття, доступний у спортивному центрі.</summary>
public class ActivityType
{
    /// <summary>Отримує або задає унікальний ідентифікатор типу заняття.</summary>
    public int Id { get; set; }

    /// <summary>Отримує або задає назву типу заняття.</summary>
    public string Name { get; set; } = string.Empty;
}