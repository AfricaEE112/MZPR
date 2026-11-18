using System.ComponentModel.DataAnnotations;

namespace SportsCenter.Api.Models;

/// <summary>Представляє тип заняття, доступний у спортивному центрі.</summary>
public class ActivityType
{
    /// <summary>Отримує або задає унікальний ідентифікатор типу заняття.</summary>
    public int Id { get; set; }

    /// <summary>Отримує або задає назву типу заняття. Поле обов'язкове.</summary>
    [Required(ErrorMessage = "Назва виду заняття є обов'язковою.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Отримує або задає тривалість заняття у хвилинах.</summary>
    [Range(1, 600, ErrorMessage = "Тривалість має бути від 1 до 600 хвилин.")]
    public int DurationMinutes { get; set; }

    /// <summary>Отримує або задає максимальну кількість учасників заняття.</summary>
    [Range(1, 1000, ErrorMessage = "Ліміт місць має бути від 1 до 1000.")]
    public int MaxParticipants { get; set; }
}
