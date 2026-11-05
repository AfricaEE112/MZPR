namespace SportsCenter.Api.Models;

public class Attendance
{
    public int Id { get; set; }

    public int ClassSessionId { get; set; }

    public int RegistrationId { get; set; }

    public bool IsPresent { get; set; } = false;
}