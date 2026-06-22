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
  TokenizeClean(string words) {
    HashSet<string> result = new();

    foreach (string tkn in CleanAndSplitToken(words)) {
      result.Add(tkn);
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
  Tokenize(models.WordWindow window) {
    HashSet<string> result = new();

    for (int i = window.Offset; i < window.Offset + window.Length; i++) {
      // Leverage your high-performance CleanToken method here
      string token = CleanToken(window.Words[i]);
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

  // todo 2; could probably just make this return a hashset too
  public static List<string>
  CleanAndSplitToken(string word) {
    List<string> tokens = new();
    Span<char> buffer = stackalloc char[word.Length];
    int pos = 0;

    foreach (char c in word) {
      if (char.IsWhiteSpace(c)) {
        if (pos > 0) {
          tokens.Add(new string(buffer.Slice(0, pos)));
          pos = 0; // Reset for next token
        }
      }
      else if (char.IsLetterOrDigit(c)) {
        buffer[pos++] = char.ToLowerInvariant(c);
      }
    }

    if (pos > 0) {
      tokens.Add(new string(buffer.Slice(0, pos)));
    }

    return tokens;
  }

  public static List<string>
  CleanSplitFilterToken(string word, HashSet<string> filter) {
    List<string> tokens = new();
    Span<char> buffer = stackalloc char[word.Length];
    int pos = 0;

    foreach (char c in word) {
      if (char.IsWhiteSpace(c)) {
        if (pos > 0) {
          add_clean_token(buffer.Slice(0, pos), tokens, filter);
          pos = 0;
        }

        continue;
      }

      buffer[pos++] = char.ToLowerInvariant(c);
    }

    if (pos > 0) {
      add_clean_token(buffer.Slice(0, pos), tokens, filter);
    }

    return tokens;
  }

  private static void
  add_clean_token(ReadOnlySpan<char> raw, List<string> tokens, HashSet<string> filter) {
    string token = new string(raw);
    foreach (string bad in filter) {
      if (!string.IsNullOrWhiteSpace(bad)) {
        token = token.Replace(bad, "", StringComparison.OrdinalIgnoreCase);
      }
    }

    Span<char> cleaned = stackalloc char[token.Length];
    int pos = 0;

    foreach (char c in token) {
      if (char.IsLetterOrDigit(c)) {
        cleaned[pos++] = char.ToLowerInvariant(c);
      }
    }

    token = new string(cleaned.Slice(0, pos));

    if (token.Length == 0) {
      return;
    }

    if (!filter.Contains(token)) {
      tokens.Add(token);
    }
  }


}
