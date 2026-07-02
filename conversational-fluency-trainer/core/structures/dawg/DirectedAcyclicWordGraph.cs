using core.strucutres.dawg.models;

namespace core.strucutres.dawg;

public class
DirectedAcyclicWordGraph {
  private Dictionary<string, Node> node_map_ = new();
  private Dictionary<int, string> id_to_value_ = new();
  private Dictionary<int, HashSet<int>> next_ = new();
  private Dictionary<int, List<int>> sequence_node_ids_ = new();
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

    List<int> node_ids = new List<int>();
    Node? previous_node = null;

    for (int i = 0; i < sequence.Count; i++) {
      Node? current_node = get_or_create(sequence[i]);

      node_ids.Add(current_node.Id);

      if (i == 0) {
        current_node.IsSequenceStart = true;
      }

      if (previous_node != null) {
        add_edge(previous_node.Id, current_node.Id);
      }

      if (i == sequence.Count - 1) {
        current_node.IsTerminal = true;
      }

      previous_node = current_node;
    }

    sequence_node_ids_[sequence_id] = node_ids;

    return sequence_id;
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
    HashSet<int> result = new HashSet<int>();

    Node? node = null;

    if (!node_map_.TryGetValue(value.ToLower(), out node)) {
      return result;
    }

    foreach (KeyValuePair<int, List<int>> entry in sequence_node_ids_) {
      if (entry.Value.Contains(node.Id)) {
        result.Add(entry.Key);
      }
    }

    return result;
  }

  public List<string>
  GetSequence(int sequence_id) {
    List<int>? node_ids = null;

    if (!sequence_node_ids_.TryGetValue(sequence_id, out node_ids)) {
      return new List<string>();
    }

    return resolve_node_ids(node_ids);
  }

  public List<string>
  GetSequence(string value, int sequence_id) {
    Node? start_node = null;

    if (!node_map_.TryGetValue(value, out start_node)) {
      return new List<string>();
    }

    List<int>? node_ids = null;

    if (!sequence_node_ids_.TryGetValue(sequence_id, out node_ids)) {
      return new List<string>();
    }

    int start_index = node_ids.IndexOf(start_node.Id);

    if (start_index < 0) {
      return new List<string>();
    }

    List<int> tail_ids = node_ids.GetRange(start_index, node_ids.Count - start_index);

    return resolve_node_ids(tail_ids);
  }

  private List<string>
  resolve_node_ids(List<int> node_ids) {
    List<string> result = new List<string>();

    foreach (int node_id in node_ids) {
      if (id_to_value_.TryGetValue(node_id, out string? word)) {
        result.Add(word);
      }
    }

    return result;
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
