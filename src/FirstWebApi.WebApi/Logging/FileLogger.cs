using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FirstWebApi.WebApi.Logging;

public class FileLoggerConfiguration
{
    public string Path { get; set; } = "logs/app-.log";
    public string Format { get; set; } = "Json";
}

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly string _format;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    public FileLoggerProvider(IOptions<FileLoggerConfiguration> options, IHttpContextAccessor httpContextAccessor)
    {
        var config = options.Value;
        _format = config.Format;
        _httpContextAccessor = httpContextAccessor;
        var dir = System.IO.Path.GetDirectoryName(config.Path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        _filePath = config.Path;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _filePath, _format, _httpContextAccessor));
    }

    public void Dispose() => _loggers.Clear();
}

public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _filePath;
    private readonly string _format;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly object Lock = new();

    public FileLogger(string categoryName, string filePath, string format, IHttpContextAccessor httpContextAccessor)
    {
        _categoryName = categoryName;
        _filePath = filePath;
        _format = format;
        _httpContextAccessor = httpContextAccessor;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var logRecord = new Dictionary<string, object?>
        {
            ["Timestamp"] = DateTime.UtcNow.ToString("O"),
            ["Level"] = logLevel.ToString(),
            ["Category"] = _categoryName,
            ["Message"] = formatter(state, exception)
        };

        if (exception != null)
            logRecord["Exception"] = exception.ToString();

        var traceId = _httpContextAccessor.HttpContext?.TraceIdentifier;
        if (!string.IsNullOrEmpty(traceId))
            logRecord["TraceId"] = traceId;

        if (state is IEnumerable<KeyValuePair<string, object?>> structure)
        {
            foreach (var kv in structure)
            {
                if (kv.Key != "{OriginalFormat}")
                {
                    try
                    {
                        _ = JsonSerializer.Serialize(kv.Value);
                        logRecord[kv.Key] = kv.Value;
                    }
                    catch
                    {
                        logRecord[kv.Key] = kv.Value?.ToString();
                    }
                }
            }
        }

        var fileName = _filePath.Replace(".log", $"-{DateTime.UtcNow:yyyy-MM-dd}.log");

        string logLine;
        try
        {
            logLine = _format == "Json"
                ? JsonSerializer.Serialize(logRecord)
                : $"[{logRecord["Timestamp"]}] {logRecord["Level"]}: {logRecord["Message"]}";
        }
        catch
        {
            logLine =
                $"[{logRecord["Timestamp"]}] {logRecord["Level"]}: {logRecord["Message"]}";
        }

        lock (Lock)
        {
            File.AppendAllText(fileName, logLine + Environment.NewLine);
        }
    }
}
