namespace SportsCenter.Api.Models
{
    /// <summary>Представляє заняття в розкладі спортивного центру.</summary>
    public class ClassSession
    {
        /// <summary>Отримує або задає унікальний ідентифікатор заняття.</summary>
        public int Id { get; set; }

        /// <summary>Отримує або задає дату проведення заняття.</summary>
        public DateTime Date { get; set; }

        /// <summary>Отримує або задає час початку заняття.</summary>
        public TimeSpan Time { get; set; }

        /// <summary>Отримує або задає ідентифікатор типу заняття.</summary>
        public int ActivityTypeId { get; set; }

        /// <summary>Отримує або задає пов'язаний тип заняття, якщо його завантажено.</summary>
        public ActivityType? ActivityType { get; set; }

        /// <summary>Отримує або задає ім'я тренера, який проводить заняття.</summary>
        public string TrainerName { get; set; } = string.Empty;

        /// <summary>Отримує або задає назву аудиторії для заняття.</summary>
        public string Room { get; set; } = string.Empty;
        /// <summary>Отримує або задає місткість, яку використовує перевірка реєстрації.</summary>
        public int Capacity { get; set; }

        /// <summary>Отримує або задає максимальну місткість заняття.</summary>
        public int MaxCapacity { get; set; }

        /// <summary>Отримує або задає колекцію реєстрацій на заняття.</summary>
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

        /// <summary>Отримує або задає поточний стан заняття.</summary>
        public string Status { get; set; } = "Заплановано";
    }
}