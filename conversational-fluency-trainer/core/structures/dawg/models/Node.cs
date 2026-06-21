using System.Runtime.CompilerServices;

namespace core.strucutres.dawg.models;

public class 
Node<T> where T : notnull {
  public T Key;
  public bool CanBeEndOfWord = false;
  public int TraversalWeight = 0;

  public SortedDictionary<T, Node<T>> Children = new();
  public SortedDictionary<T, Node<T>> Parents = new();

  public Node(T key) {
    Key = key;
  }

  public void 
  AddChild(T child, Node<T> node) {
    TraversalWeight = TraversalWeight + 1;
    Children.Add(child, node);
  }

  public void
   AddParent(T parent, Node<T> node) {
    Parents.Add(parent, node);
  }
}
