using System;
using System.Collections.Generic;
using System.Diagnostics;
using Serilog;
using Serilog.Context;
using Serilog.Events;
namespace conversational_fluency_trainer.Services;

public static class AppLogger {
    private static ILogger logger_;
    private static readonly object lock_ = new();

    // =========================================================
    // INIT
    // =========================================================
    public static void Initialize(
        string log_file_path = "logs/app.log",
        LogEventLevel minimal_level = LogEventLevel.Information,
        bool console = true) {
        lock (lock_) {
            if (logger_ != null) {
                return;
            }

            LoggerConfiguration config = new LoggerConfiguration()
                .MinimumLevel.Is(minimal_level)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("App", AppDomain.CurrentDomain.FriendlyName)
                .WriteTo.File(
                    path: log_file_path,
                    rollingInterval: RollingInterval.Day,
                    shared: true);

            if (console) {
                config = config.WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }

            logger_ = config.CreateLogger();
            Log.Logger = logger_;
        }
    }

    // =========================================================
    // CORE LOGGING
    // =========================================================
    public static void Debug(string message, params object[] args) {
        logger_?.Debug(message, args);
    }

    public static void Info(string message, params object[] args) {
        logger_?.Information(message, args);
    }

    public static void Warn(string message, params object[] args) {
        logger_?.Warning(message, args);
    }

    public static void Error(string message, params object[] args) {
        logger_?.Error(message, args);
    }

    public static void Fatal(string message, params object[] args) {
        logger_?.Fatal(message, args);
    }

    // Exception-first variants (very useful in real systems)
    public static void Error(Exception ex, string message = "") {
        logger_?.Error(ex, message);
    }

    public static void Fatal(Exception ex, string message = "") {
        logger_?.Fatal(ex, message);
    }

    public static void Warning(Exception ex, string message = "") {
        logger_?.Warning(ex, message);
    }

    // =========================================================
    // STRUCTURED LOGGING
    // =========================================================
    public static void InfoStruct(string message, Dictionary<string, object> props) {
        using (PushProperties(props)) {
            logger_?.Information(message);
        }
    }

    public static void DebugStruct(string message, Dictionary<string, object> props) {
        using (PushProperties(props)) {
            logger_?.Debug(message);
        }
    }

    public static void ErrorStruct(string message, Dictionary<string, object> props, Exception ex = null) {
        using (PushProperties(props)) {
            if (ex != null) {
                logger_?.Error(ex, message);
            }
            else {
                logger_?.Error(message);
            }
        }
    }

    // =========================================================
    // CONTEXT ENRICHMENT
    // =========================================================
    public static IDisposable PushProperty(string key, object value) {
        return LogContext.PushProperty(key, value);
    }

    public static IDisposable PushProperties(Dictionary<string, object> props) {
        List<IDisposable> disposables = new List<IDisposable>();

        foreach (KeyValuePair<string, object> kvp in props) {
            disposables.Add(LogContext.PushProperty(kvp.Key, kvp.Value));
        }

        return new composite_disposable(disposables);
    }

    // =========================================================
    // TIMING / PERFORMANCE
    // =========================================================
    public static IDisposable Time(string operation_name, string category = "Timing") {
        return new log_timer(operation_name, category);
    }

    private class log_timer : IDisposable {
        private readonly Stopwatch sw_;
        private readonly string name_;
        private readonly string category_;

        public log_timer(string name, string category) {
            name_ = name;
            category_ = category;
            sw_ = Stopwatch.StartNew();

            logger_?.Information("START {Operation} [{Category}]", name_, category_);
        }

        public void Dispose() {
            sw_.Stop();
            logger_?.Information(
                "END {Operation} [{Category}] in {ElapsedMs} ms",
                name_,
                category_,
                sw_.ElapsedMilliseconds);
        }
    }

    // =========================================================
    // SPECIAL HELPERS
    // =========================================================
    public static void LogObject(string title, object obj) {
        logger_?.Information("{Title}: {@Object}", title, obj);
    }

    public static void LogState(string state_name, object state) {
        logger_?.Debug("STATE {StateName}: {@State}", state_name, state);
    }

    public static void LogStep(string step) {
        logger_?.Information("STEP: {Step}", step);
    }

    public static void LogTrace(string message) {
        logger_?.Verbose(message);
    }

    // =========================================================
    // INTERNAL DISPOSABLE COMPOSITE
    // =========================================================
    private class composite_disposable : IDisposable {
        private readonly IEnumerable<IDisposable> items_;

        public composite_disposable(IEnumerable<IDisposable> items) {
            items_ = items;
        }

        public void Dispose() {
            foreach (IDisposable item in items_) {
                item.Dispose();
            }
        }
    }
}
