using System;
using System.Collections.Generic;
using core.strucutres.dawg.models;

namespace core.strucutres.dawg;

public class DirectedAcyclicWordGraph<T>
    where T : class, IComparable<T> {
  public Dictionary<T, Node<T>> NodeMap;

  public DirectedAcyclicWordGraph() {
    NodeMap = new Dictionary<T, Node<T>>();
  }

  public void Insert(List<T> sequence) {
    if (sequence == null) {
      throw new ArgumentNullException(nameof(sequence));
    }

    if (sequence.Count == 0) {
      return;
    }

    Node<T> current = get_or_create(sequence[0]);

    int i = 1;
    while (i < sequence.Count) {
      T value = sequence[i];
      Node<T> next = get_or_create(value);

      current.AddChild(next);
      next.AddParent(current);

      current = next;
      i = i + 1;
    }

    current.IsTerminal = true;
  }

  private Node<T>? choose_next(Node<T> node) {
    int total = 0;

    foreach (Edge<T> edge in node.Children.Values) {
      total += edge.Count;
    }

    if (total == 0) {
      return null;
    }

    int roll = random_.Next(total);
    int current = 0;

    foreach (Edge<T> edge in node.Children.Values) {
      current += edge.Count;

      if (roll < current) {
        return edge.Target;
      }
    }

    return null;
  }


  public List<T> Walk(T start, int steps) {
    List<T> output = new List<T>();

    if (!NodeMap.TryGetValue(start, out Node<T>? current)) {
      return output;
    }

    output.Add(current.Value);

    int i = 0;
    while (i < steps) {
      Node<T>? next = choose_next(current);

      if (next == null) {
        break;
      }

      output.Add(next.Value);
      current = next;
      i = i + 1;
    }

    return output;
  }

  public void Add(T value) {
    get_or_create(value);
  }

  public void Add(T parent, T child) {
    Node<T> parent_node = get_or_create(parent);
    Node<T> child_node = get_or_create(child);

    parent_node.AddChild(child_node);
    child_node.AddParent(parent_node);
  }

  public bool Contains(List<T> sequence) {
    if (sequence == null) {
      throw new ArgumentNullException(nameof(sequence));
    }

    if (sequence.Count == 0) {
      return false;
    }

    if (!NodeMap.TryGetValue(sequence[0], out Node<T>? current)) {
      return false;
    }

    int i = 1;
    while (i < sequence.Count) {
      T value = sequence[i];

      if (!current.Children.TryGetValue(value, out Edge<T>? edge)) {
        return false;
      }

      current = edge.Target;
      i = i + 1;
    }

    return current.IsTerminal;
  }

  private Node<T> get_or_create(T value) {
    if (!NodeMap.TryGetValue(value, out Node<T>? node)) {
      node = new Node<T>(value);
      NodeMap[value] = node;
    }

    node.Weight = node.Weight + 1;
    return node;
  }

  private readonly Random random_ = new Random();
}
