using Microsoft.AspNetCore.Mvc;
using PatientAnalytics.Middleware;
using PatientAnalytics.Services;

namespace PatientAnalytics.Controllers;

[Tags("Database (Development Only)")]
[ApiController]
[Route("/database")]
public class DatabaseController
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly UserService _userService;

    public DatabaseController(IHostEnvironment hostEnvironment, UserService userService)
    {
        _hostEnvironment = hostEnvironment;
        _userService = userService;
    }
    
    [HttpGet("wipe", Name = "WipeDatabase")]
    public async Task WipeDatabase(IConfiguration configuration)
    {
        if (!_hostEnvironment.IsDevelopment()) throw new HttpStatusCodeException(StatusCodes.Status404NotFound, "");
        await SqliteConnectionAccess.ResetDatabase(_userService, configuration.GetConnectionString("PatientAnalyticsContext"));
    }
}