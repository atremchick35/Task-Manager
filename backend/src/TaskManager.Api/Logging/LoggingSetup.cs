using System.Reflection;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using TaskManager.Api.Auth;

namespace TaskManager.Api.Logging;

internal static class LoggingSetup
{
    public const string DefaultServiceName = "task-manager-api";

    public const string JsonTemplate =
        "{ {" +
        "timestamp: UtcDateTime(@t), " +
        "level: if @l = 'Information' then 'info' " +
        "else if @l = 'Warning' then 'warn' " +
        "else if @l = 'Error' then 'error' " +
        "else if @l = 'Fatal' then 'fatal' " +
        "else if @l = 'Debug' then 'debug' " +
        "else 'trace', " +
        "message: @m, " +
        "'service.name': ServiceName, " +
        "'service.version': ServiceVersion, " +
        "'deployment.environment': EnvironmentName, " +
        "'host.name': HostName, " +
        "logger: SourceContext, " +
        "'event.name': EventId.Name, " +
        "'event.reason': Reason, " +
        "'user.id': UserId, " +
        "'task.id': TaskId, " +
        "'task.status': TaskStatus, " +
        "'task.previous_status': PreviousStatus, " +
        "'task.priority': TaskPriority, " +
        "'http.request.method': RequestMethod, " +
        "'http.route': HttpRoute, " +
        "'url.path': RequestPath, " +
        "'http.response.status_code': StatusCode, " +
        "'http.duration_ms': Round(Elapsed, 2), " +
        "'client.address': ClientAddress, " +
        "'error.type': ErrorType, " +
        "'error.message': ErrorMessage, " +
        "trace_id: @tr, " +
        "span_id: @sp, " +
        "exception: @x, " +
        "..rest()" +
        "} }\n";

    public static ExpressionTemplate CreateFormatter() => new(JsonTemplate);

    public static WebApplicationBuilder AddStructuredLogging(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var environmentName = builder.Environment.EnvironmentName;

        builder.Services.AddSerilog(logger => logger
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", configuration["Service:Name"] ?? DefaultServiceName)
            .Enrich.WithProperty("ServiceVersion", configuration["Service:Version"] ?? GetAssemblyVersion())
            .Enrich.WithProperty("EnvironmentName", environmentName)
            .Enrich.WithProperty("HostName", Environment.MachineName)
            .WriteTo.Console(CreateFormatter()));

        return builder;
    }

    public static WebApplication UseStructuredRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0} ms";
            options.GetLevel = GetRequestLevel;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                if (httpContext.GetEndpoint() is RouteEndpoint { RoutePattern.RawText: { } route })
                    diagnosticContext.Set("HttpRoute", route.StartsWith('/') ? route : "/" + route);

                diagnosticContext.Set("ClientAddress", GetClientAddress(httpContext));

                if (httpContext.User.Identity?.IsAuthenticated == true)
                    diagnosticContext.Set("UserId", httpContext.User.GetUserId());
            };
        });

        return app;
    }

    private static LogEventLevel GetRequestLevel(HttpContext httpContext, double elapsed, Exception? exception)
    {
        if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            return LogEventLevel.Error;

        if (httpContext.Request.Path.StartsWithSegments("/health"))
            return LogEventLevel.Verbose;

        return httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest
            ? LogEventLevel.Warning
            : LogEventLevel.Information;
    }

    private static string? GetClientAddress(HttpContext httpContext)
    {
        var realIp = httpContext.Request.Headers["X-Real-IP"].ToString();
        return string.IsNullOrEmpty(realIp) ? httpContext.Connection.RemoteIpAddress?.ToString() : realIp;
    }

    private static string GetAssemblyVersion() =>
        typeof(LoggingSetup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "unknown";
}
