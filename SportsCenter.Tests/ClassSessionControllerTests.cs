using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Controllers;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;
using Xunit;

namespace SportsCenter.Tests;

/// <summary>Містить тести кінцевих точок керування заняттями та відвідуваністю.</summary>
public class ClassSessionControllerTests
{
	/// <summary>Створює контекст бази даних у пам'яті з унікальною назвою для ізоляції тесту.</summary>
	/// <returns>Контекст бази даних для тесту.</returns>
	private static SportsCenterDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<SportsCenterDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		return new SportsCenterDbContext(options);
	}

	/// <summary>Перевіряє, що скасування заняття змінює його стан на «Скасовано».</summary>
	[Fact]
	public async Task CancelSession_ChangesStatusToCanceled()
	{
		await using var context = CreateContext();
		var session = new ClassSession { Status = "Заплановано" };
		context.ClassSessions.Add(session);
		await context.SaveChangesAsync();

		var controller = new ClassSessionController(context);
		var result = await controller.Cancel(session.Id);

		Assert.IsType<OkResult>(result);
		var updatedSession = await context.ClassSessions.FindAsync(session.Id);
		Assert.NotNull(updatedSession);
		Assert.Equal("Скасовано", updatedSession.Status);
	}

	/// <summary>Перевіряє, що запис про присутність створюється з переданими даними.</summary>
	[Fact]
	public async Task MarkAttendance_CreatesAttendanceRecord()
	{
		await using var context = CreateContext();
		var session = new ClassSession { Status = "Заплановано" };
		context.ClassSessions.Add(session);
		await context.SaveChangesAsync();

		var registration = new Registration { ClassSessionId = session.Id };
		context.Registrations.Add(registration);
		await context.SaveChangesAsync();

		var controller = new ClassSessionController(context);
		var request = new ClassSessionController.RecordAttendanceRequest 
{ 
    RegistrationId = registration.Id, 
    IsPresent = true 
};
var result = await controller.RecordAttendance(session.Id, request);
		Assert.IsType<OkResult>(result);
		var attendance = await context.Attendances
			.SingleOrDefaultAsync(a => a.RegistrationId == registration.Id);
		Assert.NotNull(attendance);
		Assert.True(attendance.IsPresent);
	}
}
