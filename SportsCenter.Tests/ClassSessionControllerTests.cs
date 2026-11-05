using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Controllers;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;
using Xunit;

namespace SportsCenter.Tests;

public class ClassSessionControllerTests
{
	private static SportsCenterDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<SportsCenterDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		return new SportsCenterDbContext(options);
	}

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
