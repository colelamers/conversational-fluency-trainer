using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;

public class JsonSerialization
{
    private JsonNode root_;

    private static readonly JsonSerializerOptions options_ = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true
    };

    // =========================================================
    // CONSTRUCTION
    // =========================================================

    public JsonSerialization(string path)
    {
        string json = File.ReadAllText(path);
        JsonNode parsed = JsonNode.Parse(json);

        if (parsed == null)
        {
            throw new InvalidOperationException("Invalid JSON config file.");
        }

        root_ = parsed;
    }

    public JsonSerialization(JsonNode root)
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        root_ = root;
    }

    public JsonNode GetRoot()
    {
        return root_;
    }

    public JsonNode GetNode(string path)
    {
        return resolve(path);
    }

    public T GetValue<T>(string path)
    {
        JsonNode node = resolve(path);

        T value = node.Deserialize<T>(options_);

        if (value == null)
        {
            throw new InvalidOperationException("Conversion failed.");
        }

        return value;
    }

    public void SetValue(string path, JsonNode value)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty.");
        }

        string[] parts = path.Split('.');
        JsonNode current = root_;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (current is not JsonObject obj)
            {
                throw new InvalidOperationException("Invalid path structure.");
            }

            bool exists = obj.TryGetPropertyValue(parts[i], out JsonNode next);

            if (!exists || next == null)
            {
                next = new JsonObject();
                obj[parts[i]] = next;
            }

            current = next;
        }

        if (current is not JsonObject finalObj)
        {
            throw new InvalidOperationException("Invalid final path structure.");
        }

        finalObj[parts[parts.Length - 1]] = value;
    }

    // =========================================================
    // INSPECTION
    // =========================================================

    public IEnumerable<string> GetKeys(string path)
    {
        JsonNode node = resolve(path);

        if (node is not JsonObject obj)
        {
            yield break;
        }

        foreach (KeyValuePair<string, JsonNode> kv in obj)
        {
            yield return kv.Key;
        }
    }

    public IEnumerable<string> GetAllKeysDeep(string path)
    {
        JsonNode start = resolve(path);

        Stack<(string prefix, JsonNode node)> stack =
            new Stack<(string prefix, JsonNode node)>();

        stack.Push(("", start));

        while (stack.Count > 0)
        {
            (string prefix, JsonNode node) = stack.Pop();

            if (node is JsonObject obj)
            {
                foreach (KeyValuePair<string, JsonNode> kv in obj)
                {
                    string fullKey;

                    if (string.IsNullOrEmpty(prefix))
                    {
                        fullKey = kv.Key;
                    }
                    else
                    {
                        fullKey = prefix + "." + kv.Key;
                    }

                    yield return fullKey;

                    if (kv.Value != null)
                    {
                        stack.Push((fullKey, kv.Value));
                    }
                }
            }
        }
    }

    // =========================================================
    // INTERNAL RESOLVER
    // =========================================================

    private JsonNode resolve(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return root_;
        }

        string[] parts = path.Split('.');
        JsonNode current = root_;

        for (int i = 0; i < parts.Length; i++)
        {
            if (current is not JsonObject obj)
            {
                throw new InvalidOperationException(
                    $"Path traversal failed at '{parts[i]}'."
                );
            }

            bool exists = obj.TryGetPropertyValue(parts[i], out JsonNode next);

            if (!exists || next == null)
            {
                throw new KeyNotFoundException($"Path not found: {path}");
            }

            current = next;
        }

        return current;
    }
}
