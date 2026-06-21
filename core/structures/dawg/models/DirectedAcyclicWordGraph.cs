namespace core.strucutres.dawg.models;

public class 
DirectedAcyclicWordGraph<T> where T : notnull {
  public SortedDictionary<T, Node<T>> NodeMap = new();
  public List<LinkedNode<T>> SentenceHeads = new();

  public void 
  Insert(List<T> nodes) {
    if (nodes.Count == 0) {
      return;
    }

    T node_at_zero = nodes[0];
    Node<T>? current;
    if (!NodeMap.TryGetValue(node_at_zero, out current)) {
      current = new Node<T>(node_at_zero);
      NodeMap.Add(node_at_zero, current);
    }

    LinkedNode<T> head = new(current);
    LinkedNode<T> cursor = head;

    for (int i = 1; i < nodes.Count; i++) {
      T node_at_i = nodes[i];
      Node<T>? next;

      if (!current.Children.TryGetValue(node_at_i, out next)) {
        if (!NodeMap.TryGetValue(node_at_i, out next)) {
          next = new Node<T>(node_at_i);
          NodeMap.Add(node_at_i, next);
        }
        current.AddChild(node_at_i, next);
      }

      LinkedNode<T>? next_linked = new(next);
      cursor.Next = next_linked;
      next_linked.Prev = cursor;
      cursor = next_linked;
      current = next;
    }

    Node<T>? final_node;
    if (NodeMap.TryGetValue(nodes[nodes.Count - 1], out final_node)) {
      final_node.CanBeEndOfWord = true;
    }
    
    SentenceHeads.Add(head);
  }

  public void 
  Insert(T prev, T current) {
    Node<T>? current_node;
    if (!NodeMap.TryGetValue(current, out current_node)) {
      current_node = new Node<T>(current);
      NodeMap.Add(current, current_node);
    }

    Node<T>? prev_node;
    if (NodeMap.TryGetValue(prev, out prev_node)) {
      prev_node.AddChild(current, current_node);
      current_node.AddParent(prev, prev_node);
    }
  }

  public void 
  Insert(T current) {
    Node<T>? current_node;
    if (!NodeMap.TryGetValue(current, out current_node)) {
      current_node = new Node<T>(current);
      NodeMap.Add(current, current_node);
    }
  }

  public bool 
  Contains(List<T> sequence) {
    if (sequence.Count == 0) {
      return false;
    }

    Node<T>? current;
    if (!NodeMap.TryGetValue(sequence[0], out current)) {
      return false;
    }

    for (int i = 1; i < sequence.Count; i++) {
      if (!current.Children.TryGetValue(sequence[i], out current)) {
        return false;
      }
    }
    return true;
  }
}
