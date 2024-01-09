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

var sqliteConnection = new SqliteConnection("Data Source=PracticeApplication.db");

sqliteConnection.Open();

var createUserTable =
    "CREATE TABLE IF NOT EXISTS Users(Id INTEGER NOT NULL UNIQUE, DateOfBirth TEXT NOT NULL, Gender TEXT NOT NULL, Email TEXT NOT NULL UNIQUE, Address INTEGER, DateCreated TEXT NOT NULL, DateEdited TEXT, PRIMARY KEY(Id AUTOINCREMENT));";

var createPatientTable =
    "CREATE TABLE IF NOT EXISTS Patients(Id INTEGER NOT NULL UNIQUE, DoctorId INTEGER NOT NULL, DateOfBirth TEXT NOT NULL, Gender TEXT NOT NULL, Email TEXT NOT NULL UNIQUE, Address INTEGER, DateCreated TEXT NOT NULL, DateEdited TEXT, PRIMARY KEY(Id AUTOINCREMENT));";


SqliteCommand sqliteCommand = sqliteConnection.CreateCommand();
sqliteCommand.CommandText = createUserTable;
sqliteCommand.ExecuteNonQuery();

sqliteCommand.CommandText = createPatientTable;
sqliteCommand.ExecuteNonQuery();

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