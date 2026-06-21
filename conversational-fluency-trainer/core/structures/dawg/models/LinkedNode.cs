namespace core.strucutres.dawg.models;

public class
LinkedNode<T> where T : notnull {
  public Node<T>? DawgNode;
  public LinkedNode<T>? Prev;
  public LinkedNode<T>? Next;

  public 
  LinkedNode(Node<T>? node) {
    DawgNode = node;
  }
}
