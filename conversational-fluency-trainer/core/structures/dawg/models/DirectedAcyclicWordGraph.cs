using System;
using System.Collections.Generic;
using core.strucutres.dawg.models;
public class DirectedAcyclicWordGraph<T> where T : IComparable<T> {
  public Dictionary<T, Node<T>> NodeMap = new Dictionary<T, Node<T>>();

  public void Insert(List<T> sequence) {
    if (sequence.Count == 0) {
      return;
    }

    Node<T>? current = get_or_create(sequence[0]);

    for (int i = 1; i < sequence.Count; i++) {
      T value = sequence[i];
      Node<T>? next = find_child(current, value);

      if (next == null) {
        next = get_or_create(value);
        current.AddChild(next);
        next.AddParent(current);
      }

      current = next;
    }

    current.IsTerminal = true;
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
    if (sequence.Count == 0) {
      return false;
    }

    Node<T>? current;

    if (NodeMap.ContainsKey(sequence[0])) {
      current = NodeMap[sequence[0]];
    }
    else {
      return false;
    }

    for (int i = 1; i < sequence.Count; i++) {
      current = find_child(current, sequence[i]);

      if (current == null) {
        return false;
      }
    }

    return current.IsTerminal;
  }

  private Node<T> get_or_create(T value) {
    Node<T>? node;

    if (NodeMap.ContainsKey(value)) {
      node = NodeMap[value];
    }
    else {
      node = new Node<T>(value);
      NodeMap.Add(value, node);
    }
    
    NodeMap[value].Weight++;

    return node;
  }

  private Node<T>? find_child(Node<T> parent, T value) {
    for (int i = 0; i < parent.Children.Count; i++) {
      Node<T> child = parent.Children[i];

      if (child.Value.CompareTo(value) == 0) {
        return child;
      }
    }

    return null;
  }
}


