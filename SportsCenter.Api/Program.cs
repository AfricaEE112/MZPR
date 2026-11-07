using Microsoft.EntityFrameworkCore;
using SportsCenter.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Реєструємо контролери та контекст бази даних у контейнері залежностей.

builder.Services.AddControllers();
builder.Services.AddDbContext<SportsCenterDbContext>(options =>
    options.UseInMemoryDatabase("SportsCenterDb"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Підключаємо XML-документацію API, зокрема описи можливих HTTP-відповідей.
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "SportsCenter.Api.xml"));
});

var app = builder.Build();

// Вмикаємо Swagger лише в середовищі розробки.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Відображаємо маршрути контролерів як HTTP-кінцеві точки.
app.MapControllers();

app.Run();
