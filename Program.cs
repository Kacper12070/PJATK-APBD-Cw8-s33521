using cwiczenia8.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();