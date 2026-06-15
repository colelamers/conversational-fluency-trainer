using System;
using System.Collections.Generic;
using System.Diagnostics;
using Serilog;
using Serilog.Context;
using Serilog.Events;

public static class AppLogger
{
    private static ILogger _logger;
    private static readonly object _lock = new();

    // =========================================================
    // INIT
    // =========================================================
    public static void Initialize(
        string logFilePath = "logs/app.log",
        LogEventLevel minimumLevel = LogEventLevel.Information,
        bool console = true)
    {
        lock (_lock)
        {
            if (_logger != null)
                return;

            var config = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLevel)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("App", AppDomain.CurrentDomain.FriendlyName)
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    shared: true);

            if (console)
            {
                config = config.WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }

            _logger = config.CreateLogger();
            Log.Logger = _logger;
        }
    }

    // =========================================================
    // CORE LOGGING
    // =========================================================
    public static void Debug(string message, params object[] args)
        => _logger?.Debug(message, args);

    public static void Info(string message, params object[] args)
        => _logger?.Information(message, args);

    public static void Warn(string message, params object[] args)
        => _logger?.Warning(message, args);

    public static void Error(string message, params object[] args)
        => _logger?.Error(message, args);

    public static void Fatal(string message, params object[] args)
        => _logger?.Fatal(message, args);

    // Exception-first variants (very useful in real systems)
    public static void Error(Exception ex, string message = "")
        => _logger?.Error(ex, message);

    public static void Fatal(Exception ex, string message = "")
        => _logger?.Fatal(ex, message);

    public static void Warning(Exception ex, string message = "")
        => _logger?.Warning(ex, message);

    // =========================================================
    // STRUCTURED LOGGING
    // =========================================================
    public static void InfoStruct(string message, Dictionary<string, object> props)
    {
        using (PushProperties(props))
        {
            _logger?.Information(message);
        }
    }

    public static void DebugStruct(string message, Dictionary<string, object> props)
    {
        using (PushProperties(props))
        {
            _logger?.Debug(message);
        }
    }

    public static void ErrorStruct(string message, Dictionary<string, object> props, Exception ex = null)
    {
        using (PushProperties(props))
        {
            if (ex != null)
                _logger?.Error(ex, message);
            else
                _logger?.Error(message);
        }
    }

    // =========================================================
    // CONTEXT ENRICHMENT
    // =========================================================
    public static IDisposable PushProperty(string key, object value)
    {
        return LogContext.PushProperty(key, value);
    }

    public static IDisposable PushProperties(Dictionary<string, object> props)
    {
        var disposables = new List<IDisposable>();

        foreach (var kvp in props)
        {
            disposables.Add(LogContext.PushProperty(kvp.Key, kvp.Value));
        }

        return new CompositeDisposable(disposables);
    }

    // =========================================================
    // TIMING / PERFORMANCE
    // =========================================================
    public static IDisposable Time(string operationName, string category = "Timing")
    {
        return new LogTimer(operationName, category);
    }

    private class LogTimer : IDisposable
    {
        private readonly Stopwatch _sw;
        private readonly string _name;
        private readonly string _category;

        public LogTimer(string name, string category)
        {
            _name = name;
            _category = category;
            _sw = Stopwatch.StartNew();

            _logger?.Information("START {Operation} [{Category}]", _name, _category);
        }

        public void Dispose()
        {
            _sw.Stop();
            _logger?.Information(
                "END {Operation} [{Category}] in {ElapsedMs} ms",
                _name,
                _category,
                _sw.ElapsedMilliseconds);
        }
    }

    // =========================================================
    // SPECIAL HELPERS
    // =========================================================
    public static void LogObject(string title, object obj)
    {
        _logger?.Information("{Title}: {@Object}", title, obj);
    }

    public static void LogState(string stateName, object state)
    {
        _logger?.Debug("STATE {StateName}: {@State}", stateName, state);
    }

    public static void LogStep(string step)
    {
        _logger?.Information("STEP: {Step}", step);
    }

    public static void LogTrace(string message)
    {
        _logger?.Verbose(message);
    }

    // =========================================================
    // INTERNAL DISPOSABLE COMPOSITE
    // =========================================================
    private class CompositeDisposable : IDisposable
    {
        private readonly IEnumerable<IDisposable> _items;

        public CompositeDisposable(IEnumerable<IDisposable> items)
        {
            _items = items;
        }

        public void Dispose()
        {
            foreach (var item in _items)
                item.Dispose();
        }
    }
}
