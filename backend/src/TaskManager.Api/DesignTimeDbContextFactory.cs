using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Api;

internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Database=design_time";

        var options = new DbContextOptionsBuilder<AppDbContext>();
        DependencyInjection.ConfigureDbContext(options, connectionString);
        return new AppDbContext(options.Options);
    }
}
