using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Abstractions;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Providers;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Security;

namespace TaskManager.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>()
            .Configure<IConfiguration>((db, configuration) =>
                db.ConnectionString = configuration.GetConnectionString(ConnectionStringName) ?? string.Empty)
            .Validate(
                db => !string.IsNullOrWhiteSpace(db.ConnectionString),
                $"Connection string 'ConnectionStrings:{ConnectionStringName}' is not configured.")
            .ValidateOnStart();

        services.AddDbContext<AppDbContext>((provider, options) =>
            ConfigureDbContext(
                options,
                provider.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName) ?? string.Empty));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskQueryProvider, TaskQueryProvider>();
        services.AddScoped<IUserQueryProvider, UserQueryProvider>();
        services.AddScoped<ITokenRevocationStore, TokenRevocationStore>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        return services;
    }

    public static void ConfigureDbContext(DbContextOptionsBuilder options, string connectionString) =>
        options
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history"))
            .UseSnakeCaseNamingConvention();
}
