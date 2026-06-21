using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace conversational_fluency_trainer.services.serilogging;

public static class 
SeriLogging {
  public static ILogger? Logger { get { return logger_; } }

  // Defaults can be :
  // LogEventLevel.Information
  // true
  public static void 
  Initialize(LogEventLevel min_level, bool console) {
    lock (LOCK) {
      if (logger_ != null) {
        return;
      }

      LoggerConfiguration config = new LoggerConfiguration()
        .MinimumLevel.Is(min_level)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("App", AppDomain.CurrentDomain.FriendlyName)
        .WriteTo.File(
            path: "logs/app.log",
            rollingInterval: RollingInterval.Day,
            shared: true);

      if (console) {
        config = config.WriteTo.Console(
            outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] " +
            "{Message:lj} {Properties:j}{NewLine}{Exception}");
      }

      Log.Logger = config.CreateLogger();
      logger_ = Log.Logger;
    }
  }

  public static void 
  Debug(string message, params object[] args) {
    logger_?.Debug(message, args);
  }

  public static void 
  Info(string message, params object[] args) {
    logger_?.Information(message, args);
  }

  public static void 
  Warn(string message, params object[] args) {
    logger_?.Warning(message, args);
  }

  public static void 
  Error(string message, params object[] args) {
    logger_?.Error(message, args);
  }

  public static void 
  Fatal(string message, params object[] args) {
    logger_?.Fatal(message, args);
  }

  // Exception-first variants (very useful in real systems)
  public static void 
  Error(Exception ex, string message = "") {
    logger_?.Error(ex, message);
  }

  public static void 
  Fatal(Exception ex, string message = "") {
    logger_?.Fatal(ex, message);
  }

  public static void 
  Warning(Exception ex, string message = "") {
    logger_?.Warning(ex, message);
  }

  public static void 
  InfoStruct(string message, Dictionary<string, object> props) {
    using (PushProperties(props)) {
      logger_?.Information(message);
    }
  }

  public static void 
  DebugStruct(string message, Dictionary<string, object> props) {
    using (PushProperties(props)) {
      logger_?.Debug(message);
    }
  }

  public static void 
  ErrorStruct(string message, Dictionary<string, object> props, Exception ex) {
    using (PushProperties(props)) {
      if (ex != null) {
        logger_?.Error(ex, message);
      }
      else {
        logger_?.Error(message);
      }
    }
  }

  public static void 
  LogObject(string title, object obj) {
    logger_?.Information("{Title}: {@Object}", title, obj);
  }

  public static void 
  LogState(string state_name, object state) {
    logger_?.Debug("STATE {StateName}: {@State}", state_name, state);
  }

  public static void 
  LogStep(string step) {
    logger_?.Information("STEP: {Step}", step);
  }

  public static void 
  LogTrace(string message) {
    logger_?.Verbose(message);
  }

  public static IDisposable 
  PushProperty(string key, object value) {
    return LogContext.PushProperty(key, value);
  }

  public static IDisposable 
  PushProperties(Dictionary<string, object> props) {
    List<IDisposable> disposables = new List<IDisposable>();

    foreach (KeyValuePair<string, object> kvp in props) {
      disposables.Add(LogContext.PushProperty(kvp.Key, kvp.Value));
    }

    return new CompositeDisposable(disposables);
  }

  public static IDisposable 
  Time(string operation_name, string category) {
    if (string.IsNullOrWhiteSpace(category)) {
      category = "Timing";
    }
    return new LogTimer(operation_name, category);
  }

  private static ILogger? logger_ { get; set; }
  private static readonly object LOCK = new();
}
