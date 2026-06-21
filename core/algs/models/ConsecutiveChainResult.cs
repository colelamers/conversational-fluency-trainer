namespace core.algs.models;

public readonly struct 
ConsecutiveChainResult {
  public int Lower { get; }
  public int Upper { get; }

  public ConsecutiveChainResult(int lower, int upper) {
    Lower = lower;
    Upper = upper;
  }
}
