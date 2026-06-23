using System;
using System.Collections.Generic;
using core.strucutres.dawg.models;

namespace core.strucutres.dawg;

public class DirectedAcyclicWordGraph {
  public Dictionary<string, Node> NodeMap;

  public DirectedAcyclicWordGraph() {
    NodeMap = new Dictionary<string, Node>();
  }

public void LoadFromStream(Stream stream) {
  if (stream == null) {
    throw new ArgumentNullException(nameof(stream));
  }

  using StreamReader reader = new StreamReader(stream);

  while (!reader.EndOfStream) {
    string? line = reader.ReadLine();

    if (string.IsNullOrWhiteSpace(line)) {
      continue;
    }

    List<string> tokens =
      core.algs.Tokenizer.CleanAndSplitToken(line);

    Insert(tokens);
  }
}

  public void
  Insert(List<string> sequence) {
    if (sequence == null) {
      throw new ArgumentNullException(nameof(sequence));
    }

    if (sequence.Count == 0) {
      return;
    }

    Node current = get_or_create(sequence[0]);

    int i = 1;
    while (i < sequence.Count) {
      string value = sequence[i];
      Node next = get_or_create(value);

      current.AddChild(next);
      next.AddParent(current);

      current = next;
      i = i + 1;
    }

    current.IsTerminal = true;
  }

  public List<string>
  WalkFrom(List<string> sequence, int length) {
    List<string> output = new List<string>();

    if (sequence == null || sequence.Count == 0) {
      return output;
    }

    Node? current;

    if (!NodeMap.TryGetValue(sequence[0], out current)) {
      return output;
    }

    int i = 1;

    while (i < sequence.Count) {
      string value = sequence[i];

      if (!current.Children.TryGetValue(value, out Edge? edge)) {
        return output;
      }

      current = edge.Target;
      i++;
    }

    output.Add(current.Value);

    int step = 0;

    while (step < length) {
      Node? next = choose_next(current);

      if (next == null) {
        break;
      }

      output.Add(next.Value);
      current = next;

      step++;
    }

    return output;
  }

  public List<string>
  WalkRandom(int min, int max) {
    List<string> output = new();

    string? start = GetRandomStart();

    if (start == null) {
      return output;
    }

    Node current = NodeMap[start];

    output.Add(current.Value);
    int length = random_.Next(min, max + 1);
    int i = 0;

    while (i < length) {
      Node? next = choose_next(current);

      if (next == null) {
        break;
      }

      output.Add(next.Value);
      current = next;

      i++;
    }

    return output;
  }

  public string?
  GetRandomStart() {
    int total = 0;

    foreach (Node node in NodeMap.Values) {
      total += node.Weight;
    }

    if (total == 0) {
      return null;
    }

    int roll = random_.Next(total);

    foreach (KeyValuePair<string, Node> pair in NodeMap) {
      roll -= pair.Value.Weight;

      if (roll < 0) {
        return pair.Key;
      }
    }

    return null;
  }

  private Node?
  choose_next(Node node) {
    int total = 0;

    foreach (Edge edge in node.Children.Values) {
      total += edge.Count;
    }

    if (total == 0) {
      return null;
    }

    int roll = random_.Next(total);
    int current = 0;

    foreach (Edge edge in node.Children.Values) {
      current += edge.Count;

      if (roll < current) {
        return edge.Target;
      }
    }

    return null;
  }


  public List<string>
  Walk(string start, int steps) {
    List<string> output = new List<string>();

    if (!NodeMap.TryGetValue(start, out Node? current)) {
      return output;
    }

    output.Add(current.Value);

    int i = 0;
    while (i < steps) {
      Node? next = choose_next(current);

      if (next == null) {
        break;
      }

      output.Add(next.Value);
      current = next;
      i = i + 1;
    }

    return output;
  }

  private Edge?
  get_best_edge(Node node) {
    Edge? best = null;
    int best_weight = 0;

    foreach (Edge next in node.Children.Values) {
      if (next.Count > best_weight) {
        best = next;
        best_weight = next.Count;
      }
    }

    return best;
  }

  private int
  compare_suggestions(Suggestion a, Suggestion b) {
    return b.Weight.CompareTo(a.Weight);
  }

  public List<Suggestion>
  GetSuggestions(List<string> sequence, int amount, int preview_length) {
    List<Suggestion> suggestions = new();

    if (sequence == null || sequence.Count == 0) {
      return suggestions;
    }

    Node? current;

    if (!NodeMap.TryGetValue(sequence[0], out current)) {
      return suggestions;
    }

    int i = 1;
    while (i < sequence.Count) {
      if (!current.Children.TryGetValue(sequence[i], out Edge? edge)) {
        return suggestions;
      }

      current = edge.Target;
      i++;
    }

    foreach (Edge edge in current.Children.Values) {
      List<string> preview = new();
      Node walk = edge.Target;
      int count = 0;

      while (count < preview_length) {
        Edge? best = get_best_edge(walk);

        if (best == null) {
          break;
        }

        preview.Add(best.Target.Value);

        walk = best.Target;

        count++;
      }

      suggestions.Add(new Suggestion(edge.Target.Value, edge.Count, preview));
    }

    suggestions.Sort(compare_suggestions);

    if (suggestions.Count > amount) {
      suggestions.RemoveRange(amount, suggestions.Count - amount);
    }

    return suggestions;
  }

  public void
  Add(string value) {
    get_or_create(value);
  }

  public void
  Add(string parent, string child) {
    Node parent_node = get_or_create(parent);
    Node child_node = get_or_create(child);

    parent_node.AddChild(child_node);
    child_node.AddParent(parent_node);
  }

  public bool
  Contains(List<string> sequence) {
    if (sequence == null) {
      throw new ArgumentNullException(nameof(sequence));
    }

    if (sequence.Count == 0) {
      return false;
    }

    if (!NodeMap.TryGetValue(sequence[0], out Node? current)) {
      return false;
    }

    int i = 1;
    while (i < sequence.Count) {
      string value = sequence[i];

      if (!current.Children.TryGetValue(value, out Edge? edge)) {
        return false;
      }

      current = edge.Target;
      i = i + 1;
    }

    return current.IsTerminal;
  }

  private Node
  get_or_create(string value) {
    if (!NodeMap.TryGetValue(value, out Node? node)) {
      node = new Node(value);
      NodeMap[value] = node;
    }

    node.Weight = node.Weight + 1;
    return node;
  }

  private readonly Random random_ = new Random();
}
