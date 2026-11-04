using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;

namespace SportsCenter.Api.Controllers;

/// <summary>Provides endpoints for creating and retrieving class registrations.</summary>
[ApiController]
[Route("[controller]")]
public class RegistrationController(SportsCenterDbContext context) : ControllerBase
{
    /// <summary>Creates a confirmed registration if the requested class session has available capacity.</summary>
    /// <param name="registration">The registration details, including the class session and visitor.</param>
    /// <returns>The created registration, or an error describing why it could not be created.</returns>
    /// <response code="200">The registration was created successfully.</response>
    /// <response code="400">The class session has reached its maximum capacity.</response>
    /// <response code="404">The specified class session does not exist.</response>
    [HttpPost]
    public async Task<IActionResult> Post(Registration registration)
    {
        var classSession = await context.ClassSessions
            .FindAsync(registration.ClassSessionId);

        if (classSession is null)
        {
            return NotFound("Class session not found.");
        }

        var registrationCount = await context.Registrations
            .CountAsync(item => item.ClassSessionId == registration.ClassSessionId);

        // Reject the registration when existing registrations have filled the session capacity.
        if (registrationCount >= classSession.Capacity)
        {
            return BadRequest("Class is full.");
        }

        registration.Status = "Confirmed";
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        return Ok(registration);
    }

    /// <summary>Retrieves all registrations belonging to the specified visitor.</summary>
    /// <param name="visitorName">The name of the visitor whose registrations are requested.</param>
    /// <returns>The visitor's registrations, which may be an empty list.</returns>
    /// <response code="200">The registrations were retrieved successfully.</response>
    [HttpGet("{visitorName}")]
    public async Task<IActionResult> Get(string visitorName)
    {
        var registrations = await context.Registrations
            .Where(item => item.VisitorName == visitorName)
            .ToListAsync();

        return Ok(registrations);
    }
}