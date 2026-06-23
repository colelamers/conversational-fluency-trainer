namespace core.strucutres.dawg.models;
public class Suggestion<T> {
  public T Value;
  public int Weight;
  public List<T> Preview;

  public Suggestion(
      T value,
      int weight,
      List<T> preview
  ) {
    Value = value;
    Weight = weight;
    Preview = preview;
  }
}
