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
    }
}