using core.strucutres.dawg.models;

namespace core.strucutres.dawg;

public class
DirectedAcyclicWordGraph {
  private Dictionary<string, Node> node_map_ = new();
  private Dictionary<int, string> id_to_value_ = new();
  private Dictionary<int, HashSet<int>> next_ = new();
  private Dictionary<string, HashSet<int>> word_sequence_ids_ = new();
  private int next_sequence_id_;
  private int next_id_ = 0;

  public
  DirectedAcyclicWordGraph() {
  }

  public void
  LoadFromStream(Stream stream) {
    if (stream == null) {
      throw new ArgumentNullException(nameof(stream));
    }

    using (StreamReader reader = new StreamReader(stream)) {
      while (!reader.EndOfStream) {
        string? line = reader.ReadLine();

        if (string.IsNullOrWhiteSpace(line)) {
          continue;
        }

        List<string> tokens = core.algs.Tokenizer.CleanAndSplitToken(line);
        Insert(tokens);
      }
    }
  }

  public int
  Insert(List<string> sequence) {
    if (sequence == null || sequence.Count == 0) {
      return -1;
    }

    int sequence_id = next_sequence_id_;
    next_sequence_id_++;

    Node? previous_node = null;

    for (int i = 0; i < sequence.Count; i++) {
      Node? current_node = get_or_create(sequence[i]);

      register_sequence_membership(sequence[i], sequence_id);

      if (previous_node != null) {
        add_edge(previous_node.Id, current_node.Id);
      }

      if (i == sequence.Count - 1) {
        current_node.IsTerminal = true;
      }

      previous_node = current_node;
    }

    return sequence_id;
  }

  private void
  register_sequence_membership(string value, int sequence_id) {
    HashSet<int>? ids = null;

    if (!word_sequence_ids_.TryGetValue(value, out ids)) {
      ids = new HashSet<int>();
      word_sequence_ids_[value] = ids;
    }

    ids.Add(sequence_id);
  }

  public Node?
  GetNode(string value) {
    Node? node = null;

    if (node_map_.TryGetValue(value, out node)) {
      return node;
    }
    return null;
  }

  public bool
  CheckSequence(List<string> sequence) {
    if (sequence == null || sequence.Count == 0) {
      return false;
    }

    Node? current_node = null;

    if (!node_map_.TryGetValue(sequence[0], out current_node)) {
      return false;
    }

    for (int i = 1; i < sequence.Count; i++) {
      Node? next_node = null;

      if (!node_map_.TryGetValue(sequence[i], out next_node)) {
        return false;
      }

      if (!HasEdge(current_node.Value, next_node.Value)) {
        return false;
      }

      current_node = next_node;
    }

    return current_node.IsTerminal;
  }

  public bool
  HasEdge(string from_value, string to_value) {
    Node? from_node = null;
    Node? to_node = null;

    if (!node_map_.TryGetValue(from_value, out from_node)) {
      return false;
    }

    if (!node_map_.TryGetValue(to_value, out to_node)) {
      return false;
    }

    HashSet<int>? edges = null;
    if (!next_.TryGetValue(from_node.Id, out edges)) {
      return false;
    }

    return edges.Contains(to_node.Id);
  }

  public HashSet<int> 
  GetSequenceIds(string value) {
    HashSet<int>? ids = null;

    if (word_sequence_ids_.TryGetValue(value, out ids)) {
      return ids;
    }

    return new HashSet<int>();
  }

  public List<string>
  GetSequence(HashSet<int> ids) {
    List<string> sequence = new();
    foreach (int k_item in ids) {
      string? str_val;
      if (id_to_value_.TryGetValue(k_item, out str_val)) {
        
      }
      sequence.Add(id_to_value_[k_item]);
    }
    return sequence;
  }

  public List<string>
  GetNext(string value) {
    List<string> result = new List<string>();

    Node? node = null;

    if (!node_map_.TryGetValue(value, out node)) {
      return result;
    }

    HashSet<int>? edges = null;

    if (!next_.TryGetValue(node.Id, out edges)) {
      return result;
    }

    foreach (int id in edges) {
      result.Add(id_to_value_[id]);
    }

    return result;
  }

  private Node
  get_or_create(string value) {
    Node? node = null;

    if (!node_map_.TryGetValue(value, out node)) {
      node = new Node(next_id_, value);
      node_map_[value] = node;
      id_to_value_[next_id_] = value;
      next_id_++;
    }

    return node;
  }

  private void
  add_edge(int from_id, int to_id) {
    HashSet<int>? edges = null;

    if (!next_.TryGetValue(from_id, out edges)) {
      edges = new HashSet<int>();
      next_[from_id] = edges;
    }

    edges.Add(to_id);
  }
}
