using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Controllers;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;
using ClassSession = SportsCenter.Api.Models.ClassSession;
using Registration = SportsCenter.Api.Models.Registration;

namespace SportsCenter.Tests;

public class RegistrationControllerTests
{
    [Fact]
    public async Task Post_ReturnsOk_WhenCapacityIsAvailable()
    {
        using var context = CreateContext();
var classSession = new ClassSession { Id = 1, Capacity = 2, MaxCapacity = 2 };
        context.ClassSessions.Add(classSession);
        context.Registrations.Add(new Registration
        {
            ClassSessionId = classSession.Id,
            VisitorName = "Existing visitor",
            RegistrationDate = DateTime.UtcNow,
            Status = "Confirmed"
        });
        await context.SaveChangesAsync();

        var controller = new RegistrationController(context);
        var registration = new Registration
        {
            ClassSessionId = classSession.Id,
            VisitorName = "New visitor",
            RegistrationDate = DateTime.UtcNow
        };

        var result = await controller.Post(registration);

        Assert.True(
            result is OkObjectResult or CreatedAtActionResult,
            $"Expected an OK or CreatedAtAction result, but got {result.GetType().Name}.");

        var savedRegistration = await context.Registrations
            .SingleAsync(item => item.VisitorName == registration.VisitorName);
        Assert.Equal("Confirmed", savedRegistration.Status);
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_WhenCapacityIsExceeded()
    {
        using var context = CreateContext();
        var classSession = new ClassSession { Id = 1, MaxCapacity = 1 };
        context.ClassSessions.Add(classSession);
        context.Registrations.Add(new Registration
        {
            ClassSessionId = classSession.Id,
            VisitorName = "Existing visitor",
            RegistrationDate = DateTime.UtcNow,
            Status = "Confirmed"
        });
        await context.SaveChangesAsync();

        var controller = new RegistrationController(context);
        var registration = new Registration
        {
            ClassSessionId = classSession.Id,
            VisitorName = "Additional visitor",
            RegistrationDate = DateTime.UtcNow
        };

        var result = await controller.Post(registration);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorMessage = Assert.IsType<string>(badRequestResult.Value);
        Assert.True(
            errorMessage.Contains("full", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("capacity", StringComparison.OrdinalIgnoreCase),
            $"Expected a capacity-related error message, but got: {errorMessage}");
    }

    [Fact]
    public async Task Get_ReturnsUserRegistrations()
    {
        using var context = CreateContext();
        context.Registrations.AddRange(
            new Registration
            {
                Id = 1,
                ClassSessionId = 1,
                VisitorName = "Alex",
                RegistrationDate = DateTime.UtcNow,
                Status = "Confirmed"
            },
            new Registration
            {
                Id = 2,
                ClassSessionId = 2,
                VisitorName = "Alex",
                RegistrationDate = DateTime.UtcNow,
                Status = "Rejected"
            },
            new Registration
            {
                Id = 3,
                ClassSessionId = 1,
                VisitorName = "Jordan",
                RegistrationDate = DateTime.UtcNow,
                Status = "Confirmed"
            });
        await context.SaveChangesAsync();

        var controller = new RegistrationController(context);

        var result = await controller.Get("Alex");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var registrations = Assert.IsAssignableFrom<IEnumerable<Registration>>(okResult.Value);
        Assert.Equal(2, registrations.Count());
        Assert.Contains(registrations, item => item.Id == 1);
        Assert.Contains(registrations, item => item.Id == 2);
        Assert.All(registrations, item => Assert.Equal("Alex", item.VisitorName));
    }

    private static SportsCenterDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SportsCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SportsCenterDbContext(options);
    }
}