namespace core.strucutres.dawg.models;

public class Edge {
  public Node Target;
  public int Count;

  public Edge(Node target) {
    Target = target;
    Count = 1;
  }

  public void Increment() {
    Count = Count + 1;
  }

}
