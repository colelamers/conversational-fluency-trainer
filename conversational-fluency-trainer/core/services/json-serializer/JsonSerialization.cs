using System.Text.Json;
using System.Text.Json.Nodes;
using core.services.json_serializer.models;

namespace core.services.json_serializer;

public class 
JsonSerializer {

  /// <summary>
  /// Loads a JSON document from a file.
  ///
  /// </summary>
  /// <param name="path">
  /// Path to a JSON file.
  /// </param>
  /// <exception cref="InvalidOperationException">
  /// Thrown if the file does not contain valid JSON.
  /// </exception>
  ///
  /// <example>
  /// <code>
  /// JsonSerializer json = new JsonSerializer("config.json");
  /// </code>
  /// </example>
  public 
  JsonSerializer(string path) {
    string json = File.ReadAllText(path);
    JsonNode? parsed = JsonNode.Parse(json);

    if (parsed == null) {
      throw new InvalidOperationException("Invalid JSON config file.");
    }

    root_ = parsed;
  }

  /// <summary>
  /// Wraps an existing <see cref="JsonNode"/>.
  ///
  /// </summary>
  /// <param name="root">
  /// Root node of the JSON document.
  /// </param>
  /// <exception cref="ArgumentNullException">
  /// Thrown if <paramref name="root"/> is null.
  /// </exception>
  ///
  /// <example>
  /// <code>
  /// JsonNode node = JsonNode.Parse("{ \"value\": 42 }")!;
  ///
  /// JsonSerializer json = new JsonSerializer(node);
  /// </code>
  /// </example>
  public 
  JsonSerializer(JsonNode root) {
    if (root == null) {
      throw new ArgumentNullException(nameof(root));
    }

    root_ = root;
  }

  /// <summary>
  /// Deserializes the entire JSON document into a strongly typed object.
  ///
  /// </summary>
  /// <typeparam name="T">
  /// Target object type.
  /// </typeparam>
  /// <returns>
  /// The JSON document converted into <typeparamref name="T"/>.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown if deserialization fails.
  /// </exception>
  ///
  /// <example>
  /// <code>
  /// AppConfig config = json.Deserialize&lt;AppConfig&gt;();
  ///
  /// Console.WriteLine(config.Whisper.Model);
  /// </code>
  /// </example>
  public T
  Deserialize<T>() {
    T? value = root_.Deserialize<T>(OPTIONS);

    if (value == null) {
      throw new InvalidOperationException(
        $"Failed to deserialize JSON into {typeof(T).Name}");
    }

    return value;
  }

  /// <summary>
  /// Returns the root JSON node.
  ///
  /// </summary>
  /// <returns>
  /// The root <see cref="JsonNode"/>.
  /// </returns>
  ///
  /// <example>
  /// <code>
  /// JsonNode root = json.GetRoot();
  /// </code>
  /// </example>
  public JsonNode 
  GetRoot() {
    return root_;
  }

  /// <summary>
  /// Retrieves a JSON node using a dot-separated path.
  ///
  /// </summary>
  /// <param name="path">
  /// Dot-separated property path.
  ///
  /// Examples:
  /// "graphics"
  /// "graphics.window"
  /// "graphics.window.width"
  /// </param>
  /// <returns>
  /// The matching <see cref="JsonNode"/>.
  /// </returns>
  /// <exception cref="KeyNotFoundException">
  /// Thrown if the path does not exist.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown if traversal encounters a non-object node.
  /// </exception>
  ///
  /// <example>
  /// <code>
  /// JsonNode node = json.GetNode("graphics.window");
  /// </code>
  /// </example>
  public JsonNode 
  GetNode(string path) {
    return resolve(path);
  }

  /// <summary>
  /// Retrieves and deserializes a value from the JSON document.
  ///
  /// </summary>
  /// <typeparam name="T">
  /// Destination type.
  /// </typeparam>
  /// <param name="path">
  /// Dot-separated property path.
  /// </param>
  /// <returns>
  /// The value converted to <typeparamref name="T"/>.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown if the value cannot be converted.
  /// </exception>
  ///
  /// <example>
  /// <code>
  /// int width = json.GetValue<int>("graphics.window.width");
  ///
  /// bool fullscreen = json.GetValue<bool>("graphics.fullscreen");
  ///
  /// string title = json.GetValue<string>("window.title");
  /// </code>
  /// </example>
  public T 
  GetValue<T>(string path) {
    JsonNode node = resolve(path);
    T? value = node.Deserialize<T>(OPTIONS);

    if (value == null) {
      throw new InvalidOperationException("Conversion failed.");
    }

    return value;
  }

  /// <summary>
  /// Creates or replaces a value at the specified path.
  ///
  /// Missing intermediate objects are automatically created.
  ///
  /// </summary>
  /// <param name="path">
  /// Dot-separated property path.
  /// </param>
  /// <param name="value">
  /// Value to store.
  /// </param>
  /// <exception cref="ArgumentException">
  /// Thrown if the path is empty.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown if part of the path is not a JSON object.
  /// </exception>
  ///
  /// <example>
  /// <code>
  /// json.SetValue(
  ///     "graphics.window.width",
  ///     JsonValue.Create(2560)!);
  ///
  /// json.SetValue(
  ///     "graphics.window.title",
  ///     JsonValue.Create("My Game")!);
  /// </code>
  /// </example>
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

      JsonNode? next;
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

  /// <summary>
  /// Returns the immediate child property names of a JSON object.
  ///
  /// </summary>
  /// <param name="path">
  /// Path to the JSON object.
  /// </param>
  /// <returns>
  /// A collection containing the object's direct property names.
  /// Returns an empty collection if the node is not an object.
  /// </returns>
  ///
  /// <example>
  /// Given:
  ///
  /// <code>
  /// {
  ///   "window": {
  ///     "width": 1920,
  ///     "height": 1080
  ///   }
  /// }
  /// </code>
  ///
  /// <code>
  /// IEnumerable<string> keys = json.GetKeys("window");
  ///
  /// // width
  /// // height
  /// </code>
  /// </example>
  public IEnumerable<string> 
  GetKeys(string path) {
    JsonNode node = resolve(path);

    List<string> keys = new List<string>();
    if (node is JsonObject == false) {
      return keys;
    }

    JsonObject obj = (JsonObject)node;
    foreach (KeyValuePair<string, JsonNode?> kv in obj) {
      keys.Add(kv.Key);
    }

    return keys;
  }

  /// <summary>
  /// Returns every nested property name beneath the specified object.
  ///
  /// Keys are returned as dot-separated paths relative to the supplied path.
  ///
  /// </summary>
  /// <param name="path">
  /// Starting object path.
  /// </param>
  /// <returns>
  /// A collection of every nested property path.
  /// </returns>
  ///
  /// <example>
  /// Given:
  ///
  /// <code>
  /// {
  ///   "graphics": {
  ///     "window": {
  ///       "width": 1920,
  ///       "height": 1080
  ///     },
  ///     "vsync": true
  ///   }
  /// }
  /// </code>
  ///
  /// <code>
  /// foreach (string key in json.GetAllKeysDeep("graphics"))
  /// {
  ///     Console.WriteLine(key);
  /// }
  ///
  /// // Output:
  /// // window
  /// // vsync
  /// // window.width
  /// // window.height
  /// </code>
  /// </example>
  public IEnumerable<string> 
  GetAllKeysDeep(string path) {
    JsonNode start = resolve(path);

    // 1. Eager allocation: This list stays entirely on the local stack scope
    List<string> results = new List<string>();

    Stack<StackFrame> stack = new Stack<StackFrame>();
    stack.Push(new StackFrame("", start));

    while (stack.Count > 0) {
      StackFrame current_frame = stack.Pop();
      string prefix = current_frame.Prefix;
      JsonNode node = current_frame.Node;

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

          results.Add(full_key);

          if (kv.Value != null) {
            stack.Push(new StackFrame(full_key, kv.Value));
          }
        }
      }
    }

    return results;
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
