using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Controllers;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;

namespace SportsCenter.Tests;

/// <summary>Містить тести перевірок доступності аудиторій і тренерів у розкладі.</summary>
public class ScheduleControllerTests
{
    private static readonly DateTime Day = new(2026, 12, 1);

    /// <summary>Створює ізольовану базу в пам'яті з одним видом заняття тривалістю 60 хвилин.</summary>
    /// <returns>Контекст бази даних для тесту.</returns>
    private static async Task<SportsCenterDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<SportsCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new SportsCenterDbContext(options);
        context.ActivityTypes.Add(new ActivityType { Id = 1, Name = "Yoga", DurationMinutes = 60, MaxParticipants = 15 });
        await context.SaveChangesAsync();
        return context;
    }

    /// <summary>Створює заняття виду «Yoga» з указаними параметрами.</summary>
    private static ClassSession Session(int hour, int minute, string room, string trainer) => new()
    {
        Date = Day,
        Time = new TimeSpan(hour, minute, 0),
        Room = room,
        TrainerName = trainer,
        ActivityTypeId = 1,
        Capacity = 10
    };

    /// <summary>Перевіряє, що заняття не додається до зайнятої аудиторії.</summary>
    [Fact]
    public async Task Post_ReturnsBadRequest_WhenRoomIsOccupied()
    {
        await using var context = await CreateContextAsync();
        context.ClassSessions.Add(Session(10, 0, "Зал 1", "Олег"));
        await context.SaveChangesAsync();
        var controller = new ScheduleController(context);

        var result = await controller.Post(Session(10, 0, "Зал 1", "Іван"));

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Аудиторія вже зайнята на цей час.", badRequestResult.Value);
    }

    /// <summary>Перевіряє, що заняття не додається, якщо тренер уже зайнятий.</summary>
    [Fact]
    public async Task Post_ReturnsBadRequest_WhenTrainerIsBusy()
    {
        await using var context = await CreateContextAsync();
        context.ClassSessions.Add(Session(10, 0, "Зал 1", "Олег"));
        await context.SaveChangesAsync();
        var controller = new ScheduleController(context);

        var result = await controller.Post(Session(10, 0, "Зал 2", "Олег"));

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Тренер вже проводить заняття в цей час.", badRequestResult.Value);
    }

    /// <summary>Перевіряє, що заняття, яке починається посеред іншого, вважається конфліктом.</summary>
    [Fact]
    public async Task Post_ReturnsBadRequest_WhenSessionsOverlapInTime()
    {
        await using var context = await CreateContextAsync();
        context.ClassSessions.Add(Session(10, 0, "Зал 1", "Олег")); // 10:00–11:00
        await context.SaveChangesAsync();
        var controller = new ScheduleController(context);

        var result = await controller.Post(Session(10, 30, "Зал 1", "Іван")); // 10:30–11:30

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Аудиторія вже зайнята на цей час.", badRequestResult.Value);
    }

    /// <summary>Перевіряє, що заняття можна поставити одразу після завершення попереднього.</summary>
    [Fact]
    public async Task Post_ReturnsCreated_WhenSessionStartsAfterPreviousEnds()
    {
        await using var context = await CreateContextAsync();
        context.ClassSessions.Add(Session(10, 0, "Зал 1", "Олег")); // 10:00–11:00
        await context.SaveChangesAsync();
        var controller = new ScheduleController(context);

        var result = await controller.Post(Session(11, 0, "Зал 1", "Олег"));

        Assert.IsType<CreatedResult>(result);
    }

    /// <summary>Перевіряє, що скасоване заняття не блокує аудиторію і тренера.</summary>
    [Fact]
    public async Task Post_ReturnsCreated_WhenConflictingSessionIsCancelled()
    {
        await using var context = await CreateContextAsync();
        var cancelled = Session(10, 0, "Зал 1", "Олег");
        cancelled.Status = SessionStatus.Cancelled;
        context.ClassSessions.Add(cancelled);
        await context.SaveChangesAsync();
        var controller = new ScheduleController(context);

        var result = await controller.Post(Session(10, 0, "Зал 1", "Олег"));

        Assert.IsType<CreatedResult>(result);
    }

    /// <summary>Перевіряє, що не можна створити заняття неіснуючого виду.</summary>
    [Fact]
    public async Task Post_ReturnsBadRequest_WhenActivityTypeDoesNotExist()
    {
        await using var context = await CreateContextAsync();
        var controller = new ScheduleController(context);
        var session = Session(10, 0, "Зал 1", "Олег");
        session.ActivityTypeId = 99;

        var result = await controller.Post(session);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.False(await context.ClassSessions.AnyAsync());
    }

    /// <summary>Перевіряє, що без указаної місткості заняття бере ліміт місць із виду заняття.</summary>
    [Fact]
    public async Task Post_UsesActivityTypeLimit_WhenCapacityIsNotSet()
    {
        await using var context = await CreateContextAsync();
        var controller = new ScheduleController(context);
        var session = Session(10, 0, "Зал 1", "Олег");
        session.Capacity = 0;

        var result = await controller.Post(session);

        Assert.IsType<CreatedResult>(result);
        var saved = await context.ClassSessions.SingleAsync();
        Assert.Equal(15, saved.Capacity);
    }
}
