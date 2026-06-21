using System.Diagnostics;

namespace conversational_fluency_trainer.services.serilogging;

public class 
LogTimer : IDisposable {
  public 
  LogTimer(string name, string category) {
    name_ = name;
    category_ = category;
    sw_ = Stopwatch.StartNew();

    // Accesses the logger safely via the parent class path
    SeriLogging.Logger?.Information("START {Operation} [{Category}]", name_, category_);
  }

  public void Dispose() {
    sw_.Stop();
    SeriLogging.Logger?.Information("END {Operation} [{Category}] in {ElapsedMs} ms",
      name_, category_, sw_.ElapsedMilliseconds);
  }

  private readonly Stopwatch sw_;
  private readonly string name_;
  private readonly string category_;
}
