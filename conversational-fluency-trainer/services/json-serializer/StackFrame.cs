using System.Text.Json;
using System.Text.Json.Nodes;

namespace conversational_fluency_trainer.services.json_serializer;
// Define an explicit, traditional structure to hold the state
// No tuples, no hidden variables.
public struct 
StackFrame {
  public string Prefix;
  public JsonNode Node;

  public StackFrame(string prefix, JsonNode node) {
    Prefix = prefix;
    Node = node;
  }
}
