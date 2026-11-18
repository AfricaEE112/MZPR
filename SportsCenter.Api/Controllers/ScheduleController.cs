using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;

namespace SportsCenter.Api.Controllers;

/// <summary>Надає кінцеві точки для керування заняттями у розкладі спортивного центру.</summary>
[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly SportsCenterDbContext _context;

    /// <summary>Створює екземпляр контролера розкладу.</summary>
    /// <param name="context">Контекст бази даних для доступу до занять.</param>
    public ScheduleController(SportsCenterDbContext context)
    {
        _context = context;
    }

    /// <summary>Створює заняття, якщо аудиторія та тренер вільні протягом усього часу заняття.</summary>
    /// <param name="session">Дані заняття, яке потрібно додати до розкладу.</param>
    /// <returns>Створене заняття або повідомлення про причину відмови.</returns>
    /// <remarks>
    /// Тривалість береться з виду заняття. Якщо місткість не вказано, використовується ліміт місць виду заняття.
    /// Скасовані заняття не блокують аудиторію і тренера.
    /// </remarks>
    /// <response code="201">Заняття успішно додано до розкладу.</response>
    /// <response code="400">Вид заняття не існує, або аудиторія чи тренер уже зайняті в цей час.</response>
    [HttpPost]
    public async Task<IActionResult> Post(ClassSession session)
    {
        // Без виду заняття невідома тривалість, тож перевірити накладання неможливо.
        var activityType = await _context.ActivityTypes.FindAsync(session.ActivityTypeId);
        if (activityType is null)
        {
            return BadRequest("Вид заняття не знайдено.");
        }

        // Якщо адміністратор не вказав місткість, беремо ліміт місць із виду заняття.
        if (session.Capacity <= 0)
        {
            session.Capacity = activityType.MaxParticipants;
        }

        var newStart = session.Time;
        var newEnd = session.Time + TimeSpan.FromMinutes(activityType.DurationMinutes);

        // Беремо лише актуальні (не скасовані) заняття того самого дня разом із їхнім видом,
        // щоб знати тривалість кожного.
        var sameDaySessions = await _context.ClassSessions
            .Include(existing => existing.ActivityType)
            .Where(existing => existing.Date == session.Date && existing.Status != SessionStatus.Cancelled)
            .ToListAsync();

        var overlapping = sameDaySessions
            .Where(existing => Overlaps(existing, newStart, newEnd))
            .ToList();

        // Не дозволяємо призначити два заняття в одну аудиторію одночасно.
        if (overlapping.Any(existing => existing.Room == session.Room))
        {
            return BadRequest("Аудиторія вже зайнята на цей час.");
        }

        // Тренер не може проводити два заняття одночасно.
        if (overlapping.Any(existing => existing.TrainerName == session.TrainerName))
        {
            return BadRequest("Тренер вже проводить заняття в цей час.");
        }

        // Нове заняття завжди створюється як заплановане, незалежно від того, що надіслав клієнт.
        session.Status = SessionStatus.Scheduled;
        _context.ClassSessions.Add(session);
        await _context.SaveChangesAsync();

        return Created($"/api/schedule/{session.Id}", session);
    }

    /// <summary>Визначає, чи перетинається наявне заняття з проміжком часу [start, end).</summary>
    /// <param name="existing">Заняття, яке вже є в розкладі.</param>
    /// <param name="start">Початок нового заняття.</param>
    /// <param name="end">Кінець нового заняття.</param>
    /// <returns><c>true</c>, якщо заняття накладаються.</returns>
    private static bool Overlaps(ClassSession existing, TimeSpan start, TimeSpan end)
    {
        var existingStart = existing.Time;
        var existingEnd = existing.Time + TimeSpan.FromMinutes(existing.ActivityType?.DurationMinutes ?? 0);

        // Однаковий час початку — завжди конфлікт. Інакше два проміжки перетинаються,
        // коли кожен починається раніше, ніж закінчується інший. Заняття «впритул» (11:00 після 10:00–11:00) дозволені.
        return existingStart == start || (existingStart < end && start < existingEnd);
    }
}
