namespace core.strucutres.dawg.models;

public class 
Node<T> where T : IComparable<T> {
  public T Value;
  public bool IsTerminal = false;
  public int Weight = 0;
  public List<Node<T>> Children = new List<Node<T>>();
  public List<Node<T>> Parents = new List<Node<T>>();

  public Node(T value) {
    Value = value;
  }

  public void AddChild(Node<T> node) {
    for (int i = 0; i < Children.Count; i++) {
      if (Children[i].Value.CompareTo(node.Value) == 0) {
        return;
      }
    }

    Children.Add(node);
  }

  public void AddParent(Node<T> node) {
    for (int i = 0; i < Parents.Count; i++) {
      if (Parents[i].Value.CompareTo(node.Value) == 0) {
        return;
      }
    }

    Parents.Add(node);
  }
}
