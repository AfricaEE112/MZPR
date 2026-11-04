namespace SportsCenter.Api.Models;

public class ClassSession
{
    public int Id { get; set; }

    public int ActivityTypeId { get; set; }

    public DateTime StartTime { get; set; }

    public int MaxCapacity { get; set; }

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}