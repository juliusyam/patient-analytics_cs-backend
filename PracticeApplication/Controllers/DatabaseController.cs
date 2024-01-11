using Microsoft.AspNetCore.Mvc;
using PracticeApplication.Middleware;

namespace PracticeApplication.Controllers;

[Tags("Database (Development Only)")]
[ApiController]
[Route("/database")]
public class DatabaseController
{
    private readonly IHostEnvironment HostEnvironment;

    public DatabaseController(IHostEnvironment hostEnvironment)
    {
        HostEnvironment = hostEnvironment;
    }
    
    [HttpGet("wipe", Name = "WipeDatabase")]
    public void WipeDatabase(IConfiguration configuration)
    {
        if (!HostEnvironment.IsDevelopment()) throw new HttpStatusCodeException(StatusCodes.Status404NotFound, "");
        SqliteConnectionAccess.ResetDatabase(configuration.GetConnectionString("PracticeApplicationContext"));
    }
}