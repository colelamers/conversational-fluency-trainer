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

  public void
  Insert(List<T> sequence) {
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

  public List<T>
  WalkFrom(List<T> sequence, int length) {
    List<T> output = new List<T>();

    if (sequence == null || sequence.Count == 0) {
      return output;
    }

    Node<T>? current;

    if (!NodeMap.TryGetValue(sequence[0], out current)) {
      return output;
    }

    int i = 1;

    while (i < sequence.Count) {
      T value = sequence[i];

      if (!current.Children.TryGetValue(value, out Edge<T>? edge)) {
        return output;
      }

      current = edge.Target;
      i++;
    }

    output.Add(current.Value);

    int step = 0;

    while (step < length) {
      Node<T>? next = choose_next(current);

      if (next == null) {
        break;
      }

      output.Add(next.Value);
      current = next;

      step++;
    }

    return output;
  }

  public List<T>
  WalkRandom(int min, int max) {
    List<T> output = new List<T>();

    T? start = GetRandomStart();

    if (start == null) {
      return output;
    }

    Node<T> current = NodeMap[start];

    output.Add(current.Value);

    int length = random_.Next(min, max + 1);

    int i = 0;

    while (i < length) {
      Node<T>? next = choose_next(current);

      if (next == null) {
        break;
      }

      output.Add(next.Value);
      current = next;

      i++;
    }

    return output;
  }

  public T?
  GetRandomStart() {
    int total = 0;

    foreach (Node<T> node in NodeMap.Values) {
      total += node.Weight;
    }

    if (total == 0) {
      return null;
    }

    int roll = random_.Next(total);

    foreach (KeyValuePair<T, Node<T>> pair in NodeMap) {
      roll -= pair.Value.Weight;

      if (roll < 0) {
        return pair.Key;
      }
    }

    return null;
  }

  private Node<T>?
  choose_next(Node<T> node) {
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


  public List<T>
  Walk(T start, int steps) {
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

  private Edge<T>? get_best_edge(Node<T> node) {
    Edge<T>? best = null;
    int best_weight = 0;

    foreach (Edge<T> next in node.Children.Values) {
      if (next.Count > best_weight) {
        best = next;
        best_weight = next.Count;
      }
    }

    return best;
  }

  private int compare_suggestions(Suggestion<T> a, Suggestion<T> b) {
    return b.Weight.CompareTo(a.Weight);
  }

  public List<Suggestion<T>> GetSuggestions(
      List<T> sequence,
      int amount,
      int preview_length
  ) {
    List<Suggestion<T>> suggestions = new();

    if (sequence == null || sequence.Count == 0) {
      return suggestions;
    }

    Node<T>? current;

    if (!NodeMap.TryGetValue(sequence[0], out current)) {
      return suggestions;
    }

    int i = 1;
    while (i < sequence.Count) {
      if (!current.Children.TryGetValue(sequence[i], out Edge<T>? edge)) {
        return suggestions;
      }

      current = edge.Target;
      i++;
    }

    foreach (Edge<T> edge in current.Children.Values) {
      List<T> preview = new();
      Node<T> walk = edge.Target;
      int count = 0;

      while (count < preview_length) {
        Edge<T>? best = get_best_edge(walk);

        if (best == null) {
          break;
        }

        preview.Add(best.Target.Value);

        walk = best.Target;

        count++;
      }

      suggestions.Add(new Suggestion<T>(edge.Target.Value, edge.Count, preview));
    }

    suggestions.Sort(compare_suggestions);

    if (suggestions.Count > amount) {
      suggestions.RemoveRange(amount, suggestions.Count - amount);
    }

    return suggestions;
  }

  public void
  Add(T value) {
    get_or_create(value);
  }

  public void
  Add(T parent, T child) {
    Node<T> parent_node = get_or_create(parent);
    Node<T> child_node = get_or_create(child);

    parent_node.AddChild(child_node);
    child_node.AddParent(parent_node);
  }

  public bool
  Contains(List<T> sequence) {
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

  private Node<T>
  get_or_create(T value) {
    if (!NodeMap.TryGetValue(value, out Node<T>? node)) {
      node = new Node<T>(value);
      NodeMap[value] = node;
    }

    node.Weight = node.Weight + 1;
    return node;
  }

  private readonly Random random_ = new Random();
}
