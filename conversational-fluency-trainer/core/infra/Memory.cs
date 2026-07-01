namespace core.infra;

public class Memory {
  public struct MemoryMeasure : IDisposable {
    private readonly long before_;

    public MemoryMeasure() {
      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();
      before_ = GC.GetTotalMemory(true);
    }

    // Dispose captures the final memory delta right as the 'using' block ends
    public void Dispose() {
    }

    public long GetTotalMemory() {
      return GC.GetTotalMemory(true) - before_;
    }
  }
}
