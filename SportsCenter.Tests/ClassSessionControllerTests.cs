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
		var session = new ClassSession { Status = SessionStatus.Scheduled };
		context.ClassSessions.Add(session);
		await context.SaveChangesAsync();

		var controller = new ClassSessionController(context);
		var result = await controller.Cancel(session.Id);

		Assert.IsType<OkResult>(result);
		var updatedSession = await context.ClassSessions.FindAsync(session.Id);
		Assert.NotNull(updatedSession);
		Assert.Equal(SessionStatus.Cancelled, updatedSession.Status);
	}

	/// <summary>Перевіряє, що запис про присутність створюється з переданими даними.</summary>
	[Fact]
	public async Task MarkAttendance_CreatesAttendanceRecord()
	{
		await using var context = CreateContext();
		var session = new ClassSession { Status = SessionStatus.Scheduled };
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

	/// <summary>Перевіряє, що скасування неіснуючого заняття повертає 404.</summary>
	[Fact]
	public async Task CancelSession_ReturnsNotFound_WhenSessionDoesNotExist()
	{
		await using var context = CreateContext();
		var controller = new ClassSessionController(context);

		var result = await controller.Cancel(42);

		Assert.IsType<NotFoundResult>(result);
	}

	/// <summary>Перевіряє, що відмітити присутність на неіснуючому занятті не можна.</summary>
	[Fact]
	public async Task MarkAttendance_ReturnsNotFound_WhenSessionDoesNotExist()
	{
		await using var context = CreateContext();
		var controller = new ClassSessionController(context);
		var request = new ClassSessionController.RecordAttendanceRequest { RegistrationId = 1, IsPresent = true };

		var result = await controller.RecordAttendance(42, request);

		Assert.IsType<NotFoundObjectResult>(result);
		Assert.False(await context.Attendances.AnyAsync());
	}

	/// <summary>Перевіряє, що не можна відмітити учасника, зареєстрованого на інше заняття.</summary>
	[Fact]
	public async Task MarkAttendance_ReturnsBadRequest_WhenRegistrationBelongsToAnotherSession()
	{
		await using var context = CreateContext();
		var session = new ClassSession();
		var otherSession = new ClassSession();
		context.ClassSessions.AddRange(session, otherSession);
		await context.SaveChangesAsync();

		var registration = new Registration { ClassSessionId = otherSession.Id, VisitorName = "Alex" };
		context.Registrations.Add(registration);
		await context.SaveChangesAsync();

		var controller = new ClassSessionController(context);
		var request = new ClassSessionController.RecordAttendanceRequest { RegistrationId = registration.Id, IsPresent = true };

		var result = await controller.RecordAttendance(session.Id, request);

		Assert.IsType<BadRequestObjectResult>(result);
		Assert.False(await context.Attendances.AnyAsync());
	}

	/// <summary>Перевіряє, що повторна відмітка оновлює наявний запис, а не створює дублікат.</summary>
	[Fact]
	public async Task MarkAttendance_UpdatesExistingRecord_InsteadOfCreatingDuplicate()
	{
		await using var context = CreateContext();
		var session = new ClassSession();
		context.ClassSessions.Add(session);
		await context.SaveChangesAsync();

		var registration = new Registration { ClassSessionId = session.Id, VisitorName = "Alex" };
		context.Registrations.Add(registration);
		await context.SaveChangesAsync();

		var controller = new ClassSessionController(context);
		await controller.RecordAttendance(session.Id,
			new ClassSessionController.RecordAttendanceRequest { RegistrationId = registration.Id, IsPresent = true });

		var result = await controller.RecordAttendance(session.Id,
			new ClassSessionController.RecordAttendanceRequest { RegistrationId = registration.Id, IsPresent = false });

		Assert.IsType<OkResult>(result);
		var attendance = await context.Attendances.SingleAsync();
		Assert.False(attendance.IsPresent);
	}

	/// <summary>Перевіряє, що звіт відхиляє період, у якому початок пізніший за кінець.</summary>
	[Fact]
	public async Task GetReport_ReturnsBadRequest_WhenStartDateIsAfterEndDate()
	{
		await using var context = CreateContext();
		var controller = new ClassSessionController(context);

		var result = await controller.GetReport(new DateTime(2026, 12, 31), new DateTime(2026, 12, 1));

		Assert.IsType<BadRequestObjectResult>(result);
	}

	/// <summary>Перевіряє, що скасовані заняття не враховуються у місткості та відвідуваності, а рахуються окремо.</summary>
	[Fact]
	public async Task GetReport_ExcludesCancelledSessions()
	{
		await using var context = CreateContext();
		var held = new ClassSession { Date = new DateTime(2026, 12, 1), Capacity = 10 };
		var cancelled = new ClassSession { Date = new DateTime(2026, 12, 2), Capacity = 20, Status = SessionStatus.Cancelled };
		var outOfRange = new ClassSession { Date = new DateTime(2027, 1, 15), Capacity = 30 };
		context.ClassSessions.AddRange(held, cancelled, outOfRange);
		await context.SaveChangesAsync();

		context.Attendances.AddRange(
			new Attendance { ClassSessionId = held.Id, RegistrationId = 1, IsPresent = true },
			new Attendance { ClassSessionId = held.Id, RegistrationId = 2, IsPresent = false },
			new Attendance { ClassSessionId = cancelled.Id, RegistrationId = 3, IsPresent = true });
		await context.SaveChangesAsync();

		var controller = new ClassSessionController(context);

		var result = await controller.GetReport(new DateTime(2026, 12, 1), new DateTime(2026, 12, 31));

		var okResult = Assert.IsType<OkObjectResult>(result);
		var report = okResult.Value!;
		Assert.Equal(1, ReadProperty<int>(report, "TotalSessions"));
		Assert.Equal(1, ReadProperty<int>(report, "CancelledSessions"));
		Assert.Equal(10, ReadProperty<int>(report, "TotalCapacity"));
		Assert.Equal(1, ReadProperty<int>(report, "TotalActualAttendees"));
	}

	/// <summary>Читає властивість анонімного об'єкта звіту через рефлексію.</summary>
	/// <typeparam name="T">Очікуваний тип значення.</typeparam>
	/// <param name="source">Об'єкт, з якого читається значення.</param>
	/// <param name="name">Назва властивості.</param>
	/// <returns>Значення властивості.</returns>
	private static T ReadProperty<T>(object source, string name)
	{
		var property = source.GetType().GetProperty(name);
		Assert.True(property is not null, $"У звіті немає властивості {name}.");
		return Assert.IsType<T>(property!.GetValue(source));
	}
}
