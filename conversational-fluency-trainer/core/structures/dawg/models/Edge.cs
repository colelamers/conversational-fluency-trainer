namespace core.strucutres.dawg.models;

public class Edge<T>
    where T : class, IComparable<T> {
  public Node<T> Target;
  public int Count;

  public Edge(Node<T> target) {
    Target = target;
    Count = 1;
  }

  public void Increment() {
    Count = Count + 1;
  }

}
