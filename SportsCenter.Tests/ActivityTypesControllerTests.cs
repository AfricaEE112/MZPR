using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api;
using SportsCenter.Api.Controllers;
using SportsCenter.Api.Data;

namespace SportsCenter.Tests;

/// <summary>
/// Contains tests for the <see cref="ActivityTypesController"/>.
/// </summary>
public class ActivityTypesControllerTests
{
    /// <summary>
    /// Verifies that <see cref="ActivityTypesController.Get"/> returns an OK result
    /// containing a list of activity types.
    /// </summary>
    [Fact]
    public async Task Get_ReturnsOkResult_WithListOfActivityTypes()
    {
        using var context = CreateContext();
        var controller = new ActivityTypesController(context);

        var result = await controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<ActivityType>>(okResult.Value);
    }

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

    private static SportsCenterDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SportsCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SportsCenterDbContext(options);
    }
}