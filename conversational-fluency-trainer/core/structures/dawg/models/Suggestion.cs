namespace core.strucutres.dawg.models;
public class Suggestion {
  public string Value;
  public int Weight;
  public List<string> Preview;

  public Suggestion(string value, int weight, List<string> preview) {
    Value = value;
    Weight = weight;
    Preview = preview;
  }
}
