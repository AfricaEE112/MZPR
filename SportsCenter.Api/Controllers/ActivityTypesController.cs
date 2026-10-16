using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Data;

namespace SportsCenter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ActivityTypesController : ControllerBase
{
    private readonly SportsCenterDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityTypesController"/> class.
    /// </summary>
    /// <param name="context">The database context used to access activity types.</param>
    public ActivityTypesController(SportsCenterDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all activity types.
    /// </summary>
    /// <returns>An HTTP response containing the activity types.</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var activityTypes = await _context.ActivityTypes.ToListAsync();
        return Ok(activityTypes);
    }

    /// <summary>
    /// Creates a new activity type.
    /// </summary>
    /// <param name="activityType">The activity type to create.</param>
    /// <returns>An HTTP response containing the created activity type.</returns>
    [HttpPost]
    public async Task<IActionResult> Post(ActivityType activityType)
    {
        _context.ActivityTypes.Add(activityType);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = activityType.Id }, activityType);
    }

    /// <summary>
    /// Updates an existing activity type.
    /// </summary>
    /// <param name="id">The identifier of the activity type to update.</param>
    /// <param name="activityType">The updated activity type data.</param>
    /// <returns>An HTTP response indicating whether the update succeeded.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, ActivityType activityType)
    {
        if (id != activityType.Id)
        {
            return BadRequest();
        }

        var existingActivityType = await _context.ActivityTypes.FindAsync(id);
        if (existingActivityType is null)
        {
            return NotFound();
        }

        existingActivityType.Name = activityType.Name;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing activity type.
    /// </summary>
    /// <param name="id">The identifier of the activity type to delete.</param>
    /// <returns>An HTTP response indicating whether the deletion succeeded.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var activityType = await _context.ActivityTypes.FindAsync(id);
        if (activityType is null)
        {
            return NotFound();
        }

        _context.ActivityTypes.Remove(activityType);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
