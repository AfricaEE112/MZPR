namespace SportsCenter.Api.Models
{
    public class ClassSession
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public int ActivityTypeId { get; set; }
        public ActivityType? ActivityType { get; set; } // Навігаційна властивість
        public string TrainerName { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
public int Capacity { get; set; }
public int MaxCapacity { get; set; }
public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    public string Status { get; set; } = "Заплановано";
    
    }
}