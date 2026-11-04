using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Controllers;
using SportsCenter.Api.Data;
using SportsCenter.Api.Models;

namespace SportsCenter.Tests
{
    public class ScheduleControllerTests
    {
        private DbContextOptions<SportsCenterDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<SportsCenterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenRoomIsOccupied()
        {
            var options = CreateNewContextOptions();
            using var context = new SportsCenterDbContext(options);
            context.ClassSessions.Add(new ClassSession 
            { Id = 1, Date = new DateTime(2026, 12, 1), Time = new TimeSpan(10, 0, 0), Room = "Зал 1", TrainerName = "Олег", ActivityTypeId = 1 });
            await context.SaveChangesAsync();

            var controller = new ScheduleController(context);
            var newSession = new ClassSession 
            { Id = 2, Date = new DateTime(2026, 12, 1), Time = new TimeSpan(10, 0, 0), Room = "Зал 1", TrainerName = "Іван", ActivityTypeId = 1 };

            var result = await controller.Post(newSession);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Аудиторія вже зайнята на цей час.", badRequestResult.Value);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenTrainerIsBusy()
        {
            var options = CreateNewContextOptions();
            using var context = new SportsCenterDbContext(options);
            context.ClassSessions.Add(new ClassSession 
            { Id = 1, Date = new DateTime(2026, 12, 1), Time = new TimeSpan(10, 0, 0), Room = "Зал 1", TrainerName = "Олег", ActivityTypeId = 1 });
            await context.SaveChangesAsync();

            var controller = new ScheduleController(context);
            var newSession = new ClassSession 
            { Id = 2, Date = new DateTime(2026, 12, 1), Time = new TimeSpan(10, 0, 0), Room = "Зал 2", TrainerName = "Олег", ActivityTypeId = 1 };

            var result = await controller.Post(newSession);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Тренер вже проводить заняття в цей час.", badRequestResult.Value);
        }
    }
}