namespace core.algs;

public static class 
Tokenizer {
  public static HashSet<string>
  Tokenize(List<string> words) {
    HashSet<string> result = new();

    foreach (string word in words) {
      if (word.Length > 0) {
        result.Add(word);
      }
    }

    return result;
  }

  public static HashSet<string>
  TokenizeClean(List<string> words) {
    HashSet<string> result = new();

    foreach (string word in words) {
      string token = CleanToken(word);

      if (token.Length > 0) {
        result.Add(token);
      }
    }

    return result;
  }

  public static HashSet<string>
  Tokenize(models.WordWindow window)
  {
    HashSet<string> result = new();

    for (int i = window.Offset; i < window.Offset + window.Length; i++) {
      string token = window.Words[i]
        .ToLowerInvariant()
        .Trim('.', ',', '?', '!', ':', ';', '"', '\'');

      if (token.Length > 0) {
        result.Add(token);
      }
    }

    return result;
  }

  public static string 
  CleanToken(string word) {
    Span<char> buffer = stackalloc char[word.Length];
    int pos = 0;

    foreach (char c in word) {
      if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)) {
        buffer[pos++] = char.ToLowerInvariant(c);
      }
    }

    return new string(buffer.Slice(0, pos));
  }
}
