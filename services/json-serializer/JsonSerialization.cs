using System.Text.Json;
using System.Text.Json.Nodes;
namespace conversational_fluency_trainer.services.json_serializer;

public class 
JsonSerializer {
  public 
  JsonSerializer(string path) {
    string json = File.ReadAllText(path);
    JsonNode? parsed = JsonNode.Parse(json);

    if (parsed == null) {
      throw new InvalidOperationException("Invalid JSON config file.");
    }

    root_ = parsed;
  }

  public 
  JsonSerializer(JsonNode root) {
    if (root == null) {
      throw new ArgumentNullException(nameof(root));
    }

    root_ = root;
  }

  public JsonNode 
  GetRoot() {
    return root_;
  }

  public JsonNode 
  GetNode(string path) {
    return resolve(path);
  }

  public T 
  GetValue<T>(string path) {
    JsonNode node = resolve(path);
    T? value = node.Deserialize<T>(OPTIONS);

    if (value == null) {
      throw new InvalidOperationException("Conversion failed.");
    }

    return value;
  }

  public void 
  SetValue(string path, JsonNode value) {
    if (string.IsNullOrWhiteSpace(path)) {
      throw new ArgumentException("Path cannot be empty.");
    }

    string[] parts = path.Split('.');
    JsonNode current = root_;

    for (int i = 0; i < parts.Length - 1; i++) {
      if (current is not JsonObject obj) {
        throw new InvalidOperationException("Invalid path structure.");
      }

      JsonNode? next = null;
      bool exists = obj.TryGetPropertyValue(parts[i], out next);

      if (!exists || next == null) {
        next = new JsonObject();
        obj[parts[i]] = next;
      }

      current = next;
    }

    bool is_match = current is JsonObject;
    if (!is_match) {
      throw new InvalidOperationException("Invalid final path structure.");
    }

    JsonObject? final_obj = (JsonObject)current;
    final_obj[parts[parts.Length - 1]] = value;
  }

  public IEnumerable<string> 
  GetKeys(string path) {
    JsonNode node = resolve(path);

    if (node is not JsonObject obj) {
      yield break;
    }

    foreach (KeyValuePair<string, JsonNode?> kv in obj) {
      yield return kv.Key;
    }
  }

  public IEnumerable<string> 
  GetAllKeysDeep(string path) {
    JsonNode start = resolve(path);

    // Use the explicit struct instead of a tuple
    Stack<StackFrame> stack = new Stack<StackFrame>();
    stack.Push(new StackFrame("", start));

    while (stack.Count > 0) {
      // Pop a single, explicit object. No multi-value unpacking/deconstruction!
      StackFrame current_frame = stack.Pop();
      string prefix = current_frame.Prefix;
      JsonNode node = current_frame.Node;

      // Traditional explicit type check instead of 'is JsonObject obj'
      if (node is JsonObject) {
        JsonObject obj = (JsonObject)node;

        foreach (KeyValuePair<string, JsonNode?> kv in obj) {
          string full_key;

          if (string.IsNullOrEmpty(prefix)) {
            full_key = kv.Key;
          }
          else {
            full_key = prefix + "." + kv.Key;
          }

          yield return full_key;

          if (kv.Value != null) {
            stack.Push(new StackFrame(full_key, kv.Value));
          }
        }
      }
    }
  }

  private JsonNode 
  resolve(string path) {
    if (string.IsNullOrWhiteSpace(path)) {
      return root_;
    }

    string[] parts = path.Split('.');
    JsonNode current = root_;

    for (int i = 0; i < parts.Length; i++) {
      if (current is not JsonObject obj) {
        throw new InvalidOperationException(
          $"Path traversal failed at '{parts[i]}'."
        );
      }
      JsonNode? next;
      bool exists = obj.TryGetPropertyValue(parts[i], out next);

      if (!exists || next == null) {
        throw new KeyNotFoundException($"Path not found: {path}");
      }

      current = next;
    }

    return current;
  }
  
  private JsonNode root_;
  private static readonly JsonSerializerOptions OPTIONS = new JsonSerializerOptions {
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    WriteIndented = true
  };
}
