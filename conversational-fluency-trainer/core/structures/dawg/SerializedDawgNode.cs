namespace core.strucutres.dawg;

/// <summary>
/// Flattened representation optimized for structured JSON serialization.
/// </summary>
public class 
SerializedDawgNode {
  public string Key { get; set; }
  public bool CanBeEndOfWord { get; set; }
  public int TraversalWeight { get; set; }
  public List<string> ChildrenKeys { get; set; }
  public 
  SerializedDawgNode() {
    Key = "";
    ChildrenKeys = new List<string>();
  }
}
