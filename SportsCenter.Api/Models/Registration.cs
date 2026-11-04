namespace SportsCenter.Api.Models;

public class Registration
{
    public int Id { get; set; }

    public int ClassSessionId { get; set; }

    public string VisitorName { get; set; } = string.Empty;

    public DateTime RegistrationDate { get; set; }

    public string Status { get; set; } = string.Empty;
}