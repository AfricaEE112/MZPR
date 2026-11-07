using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;

namespace SportsCenter.Api.Controllers
{
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

        /// <summary>Створює заняття, якщо аудиторія та тренер вільні у вказаний час.</summary>
        /// <param name="session">Дані заняття, яке потрібно додати до розкладу.</param>
        /// <returns>Створене заняття або повідомлення про зайнятість аудиторії чи тренера.</returns>
        /// <response code="201">Заняття успішно додано до розкладу.</response>
        /// <response code="400">Аудиторія або тренер уже зайняті у вказаний час, або дані не пройшли перевірку.</response>
        [HttpPost]
        public async Task<IActionResult> Post(ClassSession session)
        {
            // Не дозволяємо призначити два заняття в одну аудиторію одночасно.
            bool roomOccupied = await _context.ClassSessions.AnyAsync(existing =>
                existing.Date == session.Date &&
                existing.Time == session.Time &&
                existing.Room == session.Room);

            if (roomOccupied)
            {
                return BadRequest("Аудиторія вже зайнята на цей час.");
            }

            // Тренер не може проводити два заняття в один і той самий час.
            bool trainerBusy = await _context.ClassSessions.AnyAsync(existing =>
                existing.Date == session.Date &&
                existing.Time == session.Time &&
                existing.TrainerName == session.TrainerName);

            if (trainerBusy)
            {
                return BadRequest("Тренер вже проводить заняття в цей час.");
            }

            // Зберігаємо заняття лише після перевірки обох обмежень розкладу.
            _context.ClassSessions.Add(session);
            await _context.SaveChangesAsync();

            return Created($"/api/schedule/{session.Id}", session);
        }
    }
}