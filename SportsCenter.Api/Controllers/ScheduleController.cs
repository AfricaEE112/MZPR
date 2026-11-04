using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;

namespace SportsCenter.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for managing class sessions in the sports center schedule.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly SportsCenterDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleController"/> class.
        /// </summary>
        /// <param name="context">The database context used to access class sessions.</param>
        public ScheduleController(SportsCenterDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a class session if its room and trainer are available at the requested time.
        /// </summary>
        /// <param name="session">The class session to create.</param>
        /// <returns>The created session, or a conflict message if the room or trainer is unavailable.</returns>
        /// <response code="201">The class session was created successfully.</response>
        /// <response code="400">The room or trainer is already booked at the requested time.</response>
        [HttpPost]
        public async Task<IActionResult> Post(ClassSession session)
        {
            bool roomOccupied = await _context.ClassSessions.AnyAsync(existing =>
                existing.Date == session.Date &&
                existing.Time == session.Time &&
                existing.Room == session.Room);

            if (roomOccupied)
            {
                return BadRequest("Аудиторія вже зайнята на цей час.");
            }

            bool trainerBusy = await _context.ClassSessions.AnyAsync(existing =>
                existing.Date == session.Date &&
                existing.Time == session.Time &&
                existing.TrainerName == session.TrainerName);

            if (trainerBusy)
            {
                return BadRequest("Тренер вже проводить заняття в цей час.");
            }

            _context.ClassSessions.Add(session);
            await _context.SaveChangesAsync();

            return Created($"/api/schedule/{session.Id}", session);
        }
    }
}