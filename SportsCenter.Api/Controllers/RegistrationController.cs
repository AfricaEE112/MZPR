using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;

namespace SportsCenter.Api.Controllers;

/// <summary>Надає кінцеві точки для створення та перегляду реєстрацій на заняття.</summary>
[ApiController]
[Route("[controller]")]
public class RegistrationController(SportsCenterDbContext context) : ControllerBase
{
    /// <summary>Створює підтверджену реєстрацію, якщо в занятті є вільні місця.</summary>
    /// <param name="registration">Дані реєстрації, зокрема заняття та відвідувача.</param>
    /// <returns>Створена реєстрація або повідомлення про причину відмови.</returns>
    /// <response code="200">Реєстрацію успішно створено.</response>
    /// <response code="400">Заняття досягло максимальної місткості або дані не пройшли перевірку.</response>
    /// <response code="404">Заняття із зазначеним ідентифікатором не існує.</response>
    [HttpPost]
    public async Task<IActionResult> Post(Registration registration)
    {
        // Перевіряємо, що заняття, на яке подано заявку, існує.
        var classSession = await context.ClassSessions
            .FindAsync(registration.ClassSessionId);

        if (classSession is null)
        {
            return NotFound("Class session not found.");
        }

        // Підраховуємо наявні реєстрації, щоб не перевищити місткість заняття.
        var registrationCount = await context.Registrations
            .CountAsync(item => item.ClassSessionId == registration.ClassSessionId);

        // Відхиляємо заявку, якщо наявні реєстрації вже заповнили місткість заняття.
        if (registrationCount >= classSession.Capacity)
        {
            return BadRequest("Class is full.");
        }

        // Підтверджуємо заявку та зберігаємо її після успішної перевірки місткості.
        registration.Status = "Confirmed";
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        return Ok(registration);
    }

    /// <summary>Повертає всі реєстрації зазначеного відвідувача.</summary>
    /// <param name="visitorName">Ім'я відвідувача, реєстрації якого потрібно отримати.</param>
    /// <returns>Список реєстрацій відвідувача, який може бути порожнім.</returns>
    /// <response code="200">Реєстрації успішно отримано.</response>
    [HttpGet("{visitorName}")]
    public async Task<IActionResult> Get(string visitorName)
    {
        // Відбираємо лише записи, пов'язані з указаним відвідувачем.
        var registrations = await context.Registrations
            .Where(item => item.VisitorName == visitorName)
            .ToListAsync();

        return Ok(registrations);
    }
}