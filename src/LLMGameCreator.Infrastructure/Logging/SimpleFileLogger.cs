using Microsoft.Extensions.Logging;

namespace LLMGameCreator.Infrastructure.Logging;

public sealed class SimpleFileLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;
    private readonly object _syncRoot = new object();

    public SimpleFileLoggerProvider(string logFilePath)
    {
        _logFilePath = logFilePath;
        var directory = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SimpleFileLogger(categoryName, _logFilePath, _syncRoot);
    }

    public void Dispose()
    {
    }
}

internal sealed class SimpleFileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logFilePath;
    private readonly object _syncRoot;

    public SimpleFileLogger(string categoryName, string logFilePath, object syncRoot)
    {
        _categoryName = categoryName;
        _logFilePath = logFilePath;
        _syncRoot = syncRoot;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var line = $"{DateTimeOffset.Now:O} [{logLevel}] {_categoryName}: {formatter(state, exception)}";
        if (exception != null)
        {
            line += Environment.NewLine + exception;
        }

        lock (_syncRoot)
        {
            File.AppendAllText(_logFilePath, line + Environment.NewLine);
        }
    }
}
