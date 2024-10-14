using Microsoft.EntityFrameworkCore;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Utils;

namespace PatientAnalytics.Services;

public class DatabasePopulateService(IServiceScopeFactory scopeFactory, IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        await using var context = scope.ServiceProvider.GetRequiredService<Context>();

        await ConductDatabaseMigrations(context, cancellationToken);
        
        await CreateInitialSuperAdmin(context, cancellationToken);
    }

    private static async Task ConductDatabaseMigrations(Context context, CancellationToken cancellationToken)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var isMigrationNeeded = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).Any();

        if (isMigrationNeeded)
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
    }
    
    private async Task CreateInitialSuperAdmin(Context context, CancellationToken cancellationToken)
    {
        const string username = "superadmin";
        
        var superAdminExists = await context.Users.AnyAsync(
            u => u.Username == username,
            cancellationToken);

        if (superAdminExists) return;
        
        // TODO: Remove Password generation and pass this as an environment variable
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

        await context.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"Initial Super Admin account created with username: {username} password: {password}");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}