using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Api;

internal static class AdminCommands
{
    private static readonly Dictionary<string, Func<IServiceProvider, ILogger, CancellationToken, Task>> Commands = new()
    {
        ["migrate"] = MigrateAsync,
        ["purge-revoked-tokens"] = PurgeRevokedTokensAsync
    };

    public static bool IsAdminCommand(string[] args) => args.Length > 0 && Commands.ContainsKey(args[0]);

    public static async Task<int> RunAsync(WebApplication app, string command)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminCommand");
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            logger.LogInformation("Running admin command {Command}", command);
            await Commands[command](scope.ServiceProvider, logger, lifetime.ApplicationStopping);
            logger.LogInformation("Admin command {Command} completed", command);
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin command {Command} failed", command);
            return 1;
        }
    }

    private static async Task MigrateAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        logger.LogInformation("Pending migrations: {Count} {Migrations}", pending.Count, pending);
        await db.Database.MigrateAsync(cancellationToken);
    }

    private static async Task PurgeRevokedTokensAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var store = services.GetRequiredService<ITokenRevocationStore>();
        var now = services.GetRequiredService<TimeProvider>().GetUtcNow();
        var removed = await store.PurgeExpiredAsync(now, cancellationToken);
        logger.LogInformation("Removed {Count} expired revoked tokens", removed);
    }
}
