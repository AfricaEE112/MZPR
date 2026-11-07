using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;

namespace SportsCenter.Api.Controllers;

/// <summary>
/// Надає кінцеві точки для керування заняттями, відвідуваністю та звітами.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ClassSessionController : ControllerBase
{
	private readonly SportsCenterDbContext _context;

	/// <summary>Створює екземпляр контролера занять.</summary>
	/// <param name="context">Контекст бази даних для доступу до занять і записів відвідуваності.</param>
	public ClassSessionController(SportsCenterDbContext context)
	{
		_context = context;
	}

	// Реалізація вимоги SCRUM-27.
	/// <summary>Скасовує заняття із зазначеним ідентифікатором.</summary>
	/// <param name="id">Ідентифікатор заняття, яке потрібно скасувати.</param>
	/// <returns>Результат операції скасування заняття.</returns>
	/// <response code="200">Заняття успішно скасовано.</response>
	/// <response code="404">Заняття із зазначеним ідентифікатором не знайдено.</response>
	[HttpPut("{id}/cancel")]
	public async Task<IActionResult> Cancel(int id)
	{
		// Знаходимо заняття; відсутній запис не можна позначити скасованим.
		var session = await _context.ClassSessions.FindAsync(id);
		if (session is null)
		{
			return NotFound();
		}

		// Зміна статусу зберігає заняття в розкладі, але позначає його скасованим.
		session.Status = "Скасовано";
		await _context.SaveChangesAsync();
		return Ok();
	}

	// Реалізація вимоги SCRUM-28.
	/// <summary>Зберігає відомості про присутність учасника на занятті.</summary>
	/// <param name="classSessionId">Ідентифікатор заняття.</param>
	/// <param name="request">Ідентифікатор реєстрації та ознака присутності учасника.</param>
	/// <returns>Результат збереження запису про відвідуваність.</returns>
	/// <response code="200">Запис про відвідуваність збережено.</response>
	/// <response code="400">Тіло запиту відсутнє або не пройшло перевірку моделі.</response>
	[HttpPost("{classSessionId}/attendance")]
	public async Task<IActionResult> RecordAttendance(
		int classSessionId,
		[FromBody] RecordAttendanceRequest request)
	{
		// Пов'язуємо відвідуваність із заняттям і реєстрацією, переданими в запиті.
		var attendance = new Attendance
		{
			ClassSessionId = classSessionId,
			RegistrationId = request.RegistrationId,
			IsPresent = request.IsPresent
		};

		_context.Attendances.Add(attendance);
		await _context.SaveChangesAsync();
		return Ok();
	}

	// Реалізація вимоги SCRUM-28.
	/// <summary>Формує зведений звіт про місткість і відвідуваність занять за період.</summary>
	/// <param name="startDate">Початок періоду включно.</param>
	/// <param name="endDate">Кінець періоду включно.</param>
	/// <returns>Звіт із межами періоду, кількістю занять, загальною місткістю та кількістю присутніх.</returns>
	/// <response code="200">Звіт успішно сформовано.</response>
	/// <response code="400">Дата початку пізніша за дату завершення.</response>
	[HttpGet("report")]
	public async Task<IActionResult> GetReport(
		[FromQuery] DateTime startDate,
		[FromQuery] DateTime endDate)
	{
		// Відхиляємо некоректний період до виконання запитів до бази даних.
		if (startDate > endDate)
		{
			return BadRequest("startDate must be earlier than or equal to endDate.");
		}

		// Обмежуємо звіт заняттями, дата яких потрапляє в заданий включний діапазон.
		var sessions = _context.ClassSessions
			.Where(session => session.Date >= startDate && session.Date <= endDate);

		// Обчислюємо кількість занять, сумарну місткість і число присутніх учасників.
		var totalSessions = await sessions.CountAsync();
		var totalCapacity = await sessions.SumAsync(session => (int?)session.Capacity) ?? 0;
		var sessionIds = sessions.Select(session => session.Id);
		var totalActualAttendees = await _context.Attendances
			.CountAsync(attendance => sessionIds.Contains(attendance.ClassSessionId) && attendance.IsPresent);

		return Ok(new
		{
			StartDate = startDate,
			EndDate = endDate,
			TotalSessions = totalSessions,
			TotalCapacity = totalCapacity,
			TotalActualAttendees = totalActualAttendees
		});
	}

	/// <summary>Містить дані про відвідуваність, які потрібно зберегти для заняття.</summary>
	public sealed class RecordAttendanceRequest
	{
		/// <summary>Отримує або задає ідентифікатор реєстрації учасника.</summary>
		public int RegistrationId { get; set; }

		/// <summary>Отримує або задає ознаку присутності учасника.</summary>
		public bool IsPresent { get; set; }
	}
}
