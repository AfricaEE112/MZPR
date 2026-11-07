namespace SportsCenter.Api;

/// <summary>Містить прогноз погоди для певної дати.</summary>
public class WeatherForecast
{
    /// <summary>Отримує або задає дату прогнозу.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Отримує або задає температуру у градусах Цельсія.</summary>
    public int TemperatureC { get; set; }

    /// <summary>Обчислює температуру у градусах Фаренгейта за значенням у Цельсіях.</summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>Отримує або задає текстовий опис погоди, якщо він наявний.</summary>
    public string? Summary { get; set; }
}
