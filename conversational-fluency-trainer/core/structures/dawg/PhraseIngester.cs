using core.strucutres.dawg.models;
namespace core.strucutres.dawg;

public class 
PhraseIngester<T> where T : notnull {
  private DirectedAcyclicWordGraph<T> dawg_;

  public PhraseIngester(DirectedAcyclicWordGraph<T> dawg) {
    dawg_ = dawg;
  }

  /// <summary>
  /// Processes a clean, validated sequence of tokens directly into the DAWG state machine.
  /// </summary>
  public void 
  CommitStreamingPhrase(List<T> tokens) {
    if (tokens.Count < 1) {
      return;
    }

    // Direct ingestion utilizes the structural deduplication of the DAWG automatically
    dawg_.Insert(tokens);
    
    // Boost structural traversal weight along the exact sequence path
    Node<T>? current;
    if (dawg_.NodeMap.TryGetValue(tokens[0], out current) == true) {
      for (int i = 1; i < tokens.Count; i++) {
        Node<T>? next;
        if (current.Children.TryGetValue(tokens[i], out next) == true) {
          current.TraversalWeight = current.TraversalWeight + 1;
          current = next;
        }
      }
    }
  }
}
