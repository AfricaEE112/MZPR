using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Data;

// Дозволяє Npgsql коректно працювати з DateTime без суворої прив'язки до UTC
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Реєструємо контролери та контекст бази даних PostgreSQL
builder.Services.AddControllers();
builder.Services.AddDbContext<SportsCenterDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "SportsCenter.Api.xml");
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Автоматично створюємо таблиці в PostgreSQL при першому запуску
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SportsCenterDbContext>();
    db.Database.EnsureCreated();
}

// Вмикаємо Swagger у середовищі розробки (у docker-compose передаємо Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
