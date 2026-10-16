using Microsoft.AspNetCore.Mvc;
using SportsCenter.Api;
using SportsCenter.Api.Controllers;

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
    public void Get_ReturnsOkResult_WithListOfActivityTypes()
    {
        var controller = new ActivityTypesController();

        var result = controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<ActivityType>>(okResult.Value);
    }
}