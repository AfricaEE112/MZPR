using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;

namespace SportsCenter.Api.Controllers;

/// <summary>
/// Надає кінцеві точки для перегляду та керування типами занять.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ActivityTypesController : ControllerBase
{
    private readonly SportsCenterDbContext _context;

    /// <summary>
    /// Створює екземпляр контролера типів занять.
    /// </summary>
    /// <param name="context">Контекст бази даних для доступу до типів занять.</param>
    public ActivityTypesController(SportsCenterDbContext context)
    {
        _context = context;
    }

    /// <summary>Повертає всі доступні типи занять.</summary>
    /// <returns>HTTP-відповідь зі списком типів занять.</returns>
    /// <response code="200">Список типів занять успішно отримано.</response>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // Завантажуємо повний перелік типів занять із бази даних.
        var activityTypes = await _context.ActivityTypes.ToListAsync();
        return Ok(activityTypes);
    }

    /// <summary>Створює новий тип заняття.</summary>
    /// <param name="activityType">Дані типу заняття, який потрібно створити.</param>
    /// <returns>HTTP-відповідь із даними створеного типу заняття.</returns>
    /// <response code="201">Тип заняття успішно створено.</response>
    /// <response code="400">Назва порожня, тривалість або ліміт місць некоректні.</response>
    [HttpPost]
    public async Task<IActionResult> Post(ActivityType activityType)
    {
        // Перевіряємо критерії приймання SCRUM-10 до збереження.
        var validationError = Validate(activityType);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        // Додаємо новий тип до контексту та зберігаємо його в базі даних.
        _context.ActivityTypes.Add(activityType);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = activityType.Id }, activityType);
    }

    /// <summary>Оновлює наявний тип заняття.</summary>
    /// <param name="id">Ідентифікатор типу заняття, який потрібно оновити.</param>
    /// <param name="activityType">Оновлені дані типу заняття.</param>
    /// <returns>HTTP-відповідь про результат оновлення.</returns>
    /// <response code="204">Тип заняття успішно оновлено.</response>
    /// <response code="400">Ідентифікатори не збігаються або дані типу заняття некоректні.</response>
    /// <response code="404">Тип заняття із зазначеним ідентифікатором не знайдено.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, ActivityType activityType)
    {
        // Переконуємося, що клієнт оновлює саме той запис, який указано в маршруті.
        if (id != activityType.Id)
        {
            return BadRequest();
        }

        // Оновлені дані мають відповідати тим самим правилам, що й під час створення.
        var validationError = Validate(activityType);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        // Перевіряємо наявність запису перед застосуванням змін.
        var existingActivityType = await _context.ActivityTypes.FindAsync(id);
        if (existingActivityType is null)
        {
            return NotFound();
        }

        // Переносимо нові значення на знайдений запис і зберігаємо оновлення.
        existingActivityType.Name = activityType.Name;
        existingActivityType.DurationMinutes = activityType.DurationMinutes;
        existingActivityType.MaxParticipants = activityType.MaxParticipants;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Видаляє наявний тип заняття.</summary>
    /// <param name="id">Ідентифікатор типу заняття, який потрібно видалити.</param>
    /// <returns>HTTP-відповідь про результат видалення.</returns>
    /// <response code="204">Тип заняття успішно видалено.</response>
    /// <response code="404">Тип заняття із зазначеним ідентифікатором не знайдено.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Якщо запису немає, неможливо виконати видалення.
        var activityType = await _context.ActivityTypes.FindAsync(id);
        if (activityType is null)
        {
            return NotFound();
        }

        // Видаляємо знайдений тип і фіксуємо зміни в базі даних.
        _context.ActivityTypes.Remove(activityType);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Перевіряє дані типу заняття за правилами SCRUM-10.</summary>
    /// <param name="activityType">Тип заняття для перевірки.</param>
    /// <returns>Текст помилки або <c>null</c>, якщо дані коректні.</returns>
    /// <remarks>
    /// Атрибути [Required] і [Range] спрацьовують автоматично лише в HTTP-конвеєрі,
    /// тому та сама перевірка дублюється тут — вона працює і під час прямого виклику в юніт-тестах.
    /// </remarks>
    private static string? Validate(ActivityType activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType.Name))
        {
            return "Назва виду заняття є обов'язковою.";
        }

        if (activityType.DurationMinutes <= 0)
        {
            return "Тривалість заняття має бути більшою за нуль.";
        }

        if (activityType.MaxParticipants <= 0)
        {
            return "Ліміт місць має бути більшим за нуль.";
        }

        return null;
    }
}
