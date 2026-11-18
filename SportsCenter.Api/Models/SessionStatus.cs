namespace SportsCenter.Api.Models;

/// <summary>Містить допустимі значення стану заняття.</summary>
/// <remarks>Константи замість «магічних рядків» захищають від одруківок у різних частинах коду.</remarks>
public static class SessionStatus
{
    /// <summary>Заняття заплановане й відбудеться.</summary>
    public const string Scheduled = "Заплановано";

    /// <summary>Заняття скасоване адміністратором.</summary>
    public const string Cancelled = "Скасовано";
}
