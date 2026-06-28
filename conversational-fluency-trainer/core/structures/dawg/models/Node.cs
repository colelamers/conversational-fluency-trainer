namespace core.strucutres.dawg.models;

public class Node {
  public string Value;
  public bool IsTerminal;
  public int Weight;

  public Dictionary<string, Edge> Children;
  public Dictionary<string, Edge> Parents;

  public Node(string value) {
    Value = value;
    Children = new Dictionary<string, Edge>();
    Parents = new Dictionary<string, Edge>();
  }

  public void AddChild(Node node) {
    if (!Children.TryGetValue(node.Value, out Edge? edge)) {
      Children[node.Value] = new Edge(node);
    }
  }

  public void AddParent(Node node) {
    if (!Parents.TryGetValue(node.Value, out Edge? edge)) {
      Parents[node.Value] = new Edge(node);
    }
  }
}
