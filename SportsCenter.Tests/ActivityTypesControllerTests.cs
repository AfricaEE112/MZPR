using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api;
using SportsCenter.Api.Controllers;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;
using Xunit;

namespace SportsCenter.Tests;

/// <summary>Містить тести для <see cref="ActivityTypesController"/>.</summary>
public class ActivityTypesControllerTests
{
    /// <summary>Перевіряє, що <see cref="ActivityTypesController.Get"/> повертає успішну відповідь зі списком типів занять.</summary>
    [Fact]
    public async Task Get_ReturnsOkResult_WithListOfActivityTypes()
    {
        using var context = CreateContext();
        var controller = new ActivityTypesController(context);

        var result = await controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<ActivityType>>(okResult.Value);
    }

    /// <summary>Перевіряє, що створення коректного типу заняття повертає відповідь із результатом створення.</summary>
    [Fact]
    public async Task Post_ReturnsCreatedAtActionResult_WhenGivenValidActivityType()
    {
        using var context = CreateContext();
        var controller = new ActivityTypesController(context);
        var activityType = new ActivityType { Id = 1, Name = "Yoga" };

        var result = await controller.Post(activityType);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Same(activityType, createdAtActionResult.Value);
    }

    /// <summary>Перевіряє, що успішне оновлення типу заняття повертає відповідь без вмісту.</summary>
    [Fact]
    public async Task Put_ReturnsNoContent_WhenUpdateIsSuccessful()
    {
        using var context = CreateContext();
        var activityType = new ActivityType { Id = 1, Name = "Yoga" };
        context.ActivityTypes.Add(activityType);
        await context.SaveChangesAsync();

        var controller = new ActivityTypesController(context);
        var updatedActivityType = new ActivityType { Id = 1, Name = "Pilates" };

        var result = await controller.Put(activityType.Id, updatedActivityType);

        Assert.IsType<NoContentResult>(result);
    }

    /// <summary>Перевіряє, що успішне видалення типу заняття прибирає запис із бази даних.</summary>
    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleteIsSuccessful()
    {
        using var context = CreateContext();
        var activityType = new ActivityType { Id = 1, Name = "Yoga" };
        context.ActivityTypes.Add(activityType);
        await context.SaveChangesAsync();

        var controller = new ActivityTypesController(context);

        var result = await controller.Delete(activityType.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.False(await context.ActivityTypes.AnyAsync(item => item.Id == activityType.Id));
    }

    /// <summary>Створює контекст бази даних у пам'яті з унікальною назвою для ізоляції тесту.</summary>
    /// <returns>Контекст бази даних для тесту.</returns>
    private static SportsCenterDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SportsCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SportsCenterDbContext(options);
    }
}