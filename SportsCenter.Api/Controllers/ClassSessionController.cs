using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;

namespace SportsCenter.Api.Controllers;

[ApiController]
[Route("[controller]")]
/// <summary>
/// Provides endpoints for managing class sessions, attendance, and session reports.
/// </summary>
public class ClassSessionController : ControllerBase
{
	private readonly SportsCenterDbContext _context;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassSessionController"/> class.
	/// </summary>
	/// <param name="context">The database context used to access class sessions and attendance records.</param>
	public ClassSessionController(SportsCenterDbContext context)
	{
		_context = context;
	}

	// SCRUM-27
	/// <summary>
	/// Cancels the specified class session.
	/// </summary>
	/// <param name="id">The identifier of the class session to cancel.</param>
	/// <returns>An action result indicating whether the session was canceled.</returns>
	/// <response code="200">The session was canceled.</response>
	/// <response code="404">No session with the specified identifier was found.</response>
	[HttpPut("{id}/cancel")]
	public async Task<IActionResult> Cancel(int id)
	{
		var session = await _context.ClassSessions.FindAsync(id);
		if (session is null)
		{
			return NotFound();
		}

		session.Status = "Скасовано";
		await _context.SaveChangesAsync();
		return Ok();
	}

	// SCRUM-28
	/// <summary>
	/// Records an attendee's attendance for a class session.
	/// </summary>
	/// <param name="classSessionId">The identifier of the class session.</param>
	/// <param name="request">The registration identifier and whether the attendee was present.</param>
	/// <returns>An action result indicating that the attendance record was saved.</returns>
	/// <response code="200">The attendance record was saved.</response>
	/// <response code="400">The request body is missing or invalid.</response>
	[HttpPost("{classSessionId}/attendance")]
	public async Task<IActionResult> RecordAttendance(
		int classSessionId,
		[FromBody] RecordAttendanceRequest request)
	{
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

	// SCRUM-28
	/// <summary>
	/// Gets aggregate session capacity and attendance for the specified date range.
	/// </summary>
	/// <param name="startDate">The inclusive start of the date range.</param>
	/// <param name="endDate">The inclusive end of the date range.</param>
	/// <returns>A report containing the date range, session count, total capacity, and actual attendee count.</returns>
	/// <response code="200">The report was generated.</response>
	/// <response code="400">The start date is later than the end date.</response>
	[HttpGet("report")]
	public async Task<IActionResult> GetReport(
		[FromQuery] DateTime startDate,
		[FromQuery] DateTime endDate)
	{
		if (startDate > endDate)
		{
			return BadRequest("startDate must be earlier than or equal to endDate.");
		}

		var sessions = _context.ClassSessions
			.Where(session => session.Date >= startDate && session.Date <= endDate);

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

	/// <summary>
	/// Specifies the attendance details to record for a class session.
	/// </summary>
	public sealed class RecordAttendanceRequest
	{
		/// <summary>Gets or sets the identifier of the attendee's registration.</summary>
		public int RegistrationId { get; set; }

		/// <summary>Gets or sets whether the attendee was present.</summary>
		public bool IsPresent { get; set; }
	}
}
