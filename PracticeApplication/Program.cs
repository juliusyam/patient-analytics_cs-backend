using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using PracticeApplication.Middleware;
using PracticeApplication.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(opt =>
    opt.UseSqlite("Data Source=PracticeApplication.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpStatusCodeExceptionMiddleware();

app.UseExceptionHandler(new ExceptionHandlerOptions()
{
    AllowStatusCode404Response = true,
    ExceptionHandlingPath = "/error"
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();