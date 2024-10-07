using Microsoft.EntityFrameworkCore;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Utils;

namespace PatientAnalytics.Services;

public class DatabasePopulateService(IServiceScopeFactory scopeFactory, IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        await using var context = scope.ServiceProvider.GetRequiredService<Context>();

        await context.Database.EnsureCreatedAsync(stoppingToken);

        var isMigrationNeeded = (await context.Database.GetPendingMigrationsAsync(stoppingToken)).Any();

        if (isMigrationNeeded)
        {
            await context.Database.MigrateAsync(stoppingToken);
        }

        if (!context.Users.Any(u => u.Role == "SuperAdmin"))
        {
            await CreateInitialSuperAdmin(context);
        }
    }

    private async Task CreateInitialSuperAdmin(Context context)
    {
        const string username = "superadmin";
        var password = Password.GeneratePassword();
        var hashed = Password.HashPassword(password, configuration);

        var payload = new RegistrationPayload
        {
            DateOfBirth = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Male",
            Username = username,
            FirstName = "Super",
            LastName = "Admin",
            Password = password,
            Email = "superadmin@example.com",
            Address = "123 Admin St"
        };
        var superAdmin = User.CreateUser(hashed, payload, "SuperAdmin");

        context.Users.Add(superAdmin);

        await context.SaveChangesAsync();

        Console.WriteLine($"Initial Super Admin account created with username: {username} password: {password}");
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}