using System.Text.Json;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using TaskManager.Api.Logging;

namespace TaskManager.Tests.Unit;

public sealed class JsonLogFormatterTests
{
    [Fact]
    public void Format_WritesSchemaFieldsAsJson()
    {
        var userId = Guid.NewGuid();

        var line = Log(logger => logger.LogInformation(new EventId(1001, "user.registered"), "User {UserId} registered", userId));

        using var json = JsonDocument.Parse(line);
        var root = json.RootElement;
        Assert.True(root.GetProperty("timestamp").TryGetDateTimeOffset(out _));
        Assert.Equal("info", root.GetProperty("level").GetString());
        Assert.Equal($"User {userId} registered", root.GetProperty("message").GetString());
        Assert.Equal("test-service", root.GetProperty("service.name").GetString());
        Assert.Equal("1.2.3", root.GetProperty("service.version").GetString());
        Assert.Equal("user.registered", root.GetProperty("event.name").GetString());
        Assert.Equal(userId.ToString(), root.GetProperty("user.id").GetString());
        Assert.Equal("tests", root.GetProperty("logger").GetString());
        Assert.False(root.TryGetProperty("UserId", out _));
        Assert.False(root.TryGetProperty("EventId", out _));
        Assert.False(root.TryGetProperty("task.id", out _));
    }

    [Fact]
    public void Format_MapsLevelsAndKeepsUnknownProperties()
    {
        var line = Log(logger => logger.LogWarning("Purged {Count} tokens", 5));

        using var json = JsonDocument.Parse(line);
        var root = json.RootElement;
        Assert.Equal("warn", root.GetProperty("level").GetString());
        Assert.Equal(5, root.GetProperty("Count").GetInt32());
    }

    [Fact]
    public void Format_WritesOneLinePerEvent()
    {
        var output = Log(logger =>
        {
            logger.LogInformation("first");
            logger.LogError(new InvalidOperationException("boom"), "second");
        });

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        using var error = JsonDocument.Parse(lines[1]);
        Assert.Equal("error", error.RootElement.GetProperty("level").GetString());
        Assert.Contains("boom", error.RootElement.GetProperty("exception").GetString());
    }

    private static string Log(Action<Microsoft.Extensions.Logging.ILogger> write)
    {
        var output = new StringWriter();
        using (var serilog = new LoggerConfiguration()
                   .Enrich.WithProperty("ServiceName", "test-service")
                   .Enrich.WithProperty("ServiceVersion", "1.2.3")
                   .WriteTo.Sink(new TextSink(output))
                   .CreateLogger())
        using (var factory = new SerilogLoggerFactory(serilog))
        {
            write(factory.CreateLogger("tests"));
        }

        return output.ToString().Trim();
    }

    private sealed class TextSink(TextWriter output) : ILogEventSink
    {
        private readonly Serilog.Templates.ExpressionTemplate _formatter = LoggingSetup.CreateFormatter();

        public void Emit(LogEvent logEvent) => _formatter.Format(logEvent, output);
    }
}
