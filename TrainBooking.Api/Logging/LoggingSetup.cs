using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace TrainBooking.Api.Logging;

/// <summary>
/// Configures logging: readable lines on the console, and JSON lines in a size-limited file
/// (settings: <c>FileLogging</c>) that Logstash can read.
/// Request and response bodies are never logged, so passenger data stays out of the logs.
/// </summary>
public static class LoggingSetup
{
    private const string DatabaseCommandSource = "Microsoft.EntityFrameworkCore.Database.Command";
    private const string MetricsPath = "/metrics";

    public static WebApplicationBuilder AddAppLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            // Tells apart the API copies that run behind the load balancer (setting: Instance:Name).
            var instanceName = context.Configuration["Instance:Name"];
            if (string.IsNullOrWhiteSpace(instanceName))
                instanceName = Environment.MachineName;

            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "TrainBooking.Api")
                .Enrich.WithProperty("Instance", instanceName)
                .WriteTo.Console();

            AddFile(configuration, context.Configuration, context.HostingEnvironment.ContentRootPath);
        });

        return builder;
    }

    /// <summary>
    /// Logs one line per request: method, path (without query string), status code and duration.
    /// </summary>
    public static WebApplication UseAppRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options => options.GetLevel = (httpContext, _, exception) =>
        {
            // Prometheus scrapes this every few seconds; it would drown everything else.
            if (httpContext.Request.Path.StartsWithSegments(MetricsPath))
                return LogEventLevel.Verbose;

            if (exception is not null || httpContext.Response.StatusCode >= 500)
                return LogEventLevel.Error;

            return httpContext.Response.StatusCode >= 400 ? LogEventLevel.Warning : LogEventLevel.Information;
        });

        return app;
    }

    private static void AddFile(
        LoggerConfiguration configuration,
        IConfiguration settings,
        string contentRootPath)
    {
        // An empty path turns the file off.
        var path = settings["FileLogging:Path"];
        if (string.IsNullOrWhiteSpace(path))
            return;

        // Relative to the project folder, so the result does not depend on where the API is started from.
        var fullPath = Path.GetFullPath(path, contentRootPath);
        var fileSizeLimitMb = settings.GetValue("FileLogging:FileSizeLimitMb", 10);
        var retainedFiles = settings.GetValue("FileLogging:RetainedFiles", 7);

        configuration.WriteTo.Logger(file => file
            // Every SQL statement is logged at Information; keep those on the console only.
            .Filter.ByExcluding(IsRoutineDatabaseCommand)
            .WriteTo.File(
                new RenderedCompactJsonFormatter(),
                fullPath,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: fileSizeLimitMb * 1024L * 1024L,
                retainedFileCountLimit: retainedFiles));
    }

    private static bool IsRoutineDatabaseCommand(LogEvent logEvent)
    {
        return logEvent.Level < LogEventLevel.Warning
            && logEvent.Properties.TryGetValue(Constants.SourceContextPropertyName, out var source)
            && source is ScalarValue { Value: DatabaseCommandSource };
    }
}
