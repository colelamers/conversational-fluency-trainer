namespace core.strucutres.dawg.models;

public class Node<T> where T : class, IComparable<T> {
  public T Value;
  public bool IsTerminal;
  public int Weight;

  public Dictionary<T, Edge<T>> Children;
  public Dictionary<T, Edge<T>> Parents;

  public Node(T value) {
    Value = value;
    Children = new Dictionary<T, Edge<T>>();
    Parents = new Dictionary<T, Edge<T>>();
  }

  public void AddChild(Node<T> node) {
    if (Children.TryGetValue(node.Value, out Edge<T>? edge)) {
      edge.Increment();
    }
    else {
      Children[node.Value] = new Edge<T>(node);
    }
  }

  public void AddParent(Node<T> node) {
    if (Parents.TryGetValue(node.Value, out Edge<T>? edge)) {
      edge.Increment();
    }
    else {
      Parents[node.Value] = new Edge<T>(node);
    }
  }
}
