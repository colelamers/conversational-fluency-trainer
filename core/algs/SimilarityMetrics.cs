using core.algs.models;
namespace core.algs;
// -------------------------------------------------------------------------
// METRIC   | CHARACTERISTIC    | RATING / BEHAVIOR
// -------------------------------------------------------------------------
// Jaccard  | Fuzziness on Matching | Medium (semi-strict)
// Jaccard  | Fuzziness on Order  | Ignores order
// Jaccard  | Sensitivity to Length | High (based on union)
// Jaccard  | Note          | Penalizes non-overlap strongly
// -------------------------------------------------------------------------
// Cosine   | Fuzziness on Matching | High (fuzzy + vectorized)
// Cosine   | Fuzziness on Order  | Ignores order
// Cosine   | Sensitivity to Length | High (via vector length)
// Cosine   | Note          | Measures alignment, not sequence
// -------------------------------------------------------------------------
// Dice   | Fuzziness on Matching | High (fuzzy)
// Dice   | Fuzziness on Order  | Ignores order
// Dice   | Sensitivity to Length | Medium (averages lengths)
// Dice   | Note          | Counts overlap, not order
// -------------------------------------------------------------------------
// ROUGE-L  | Fuzziness on Matching | Medium (sequence-aware)
// ROUGE-L  | Fuzziness on Order  | Respects order
// ROUGE-L  | Sensitivity to Length | Medium (depends on LCS length)
// ROUGE-L  | Note          | Prefers long, ordered matches
// -------------------------------------------------------------------------
public static class 
SimilarityMetrics {
  #region Cosine Similarity

  public static double 
  CosineSimilarity(HashSet<string> set1, HashSet<string> set2) {
    int inter = 0;

    foreach (string item in set1) {
      if (set2.Contains(item)) {
        inter++;
      }
    }

    if (set1.Count == 0 || set2.Count == 0) {
      return 0.0;
    }

    return (double)inter / Math.Sqrt(set1.Count * set2.Count);
  }

  public static double 
  CosineSimilarity(List<string> a, List<string> b) {
    HashSet<string> set1 = Tokenizer.Tokenize(a);
    HashSet<string> set2 = Tokenizer.Tokenize(b);

    return CosineSimilarity(set1, set2);
  }

  public static double 
  CosineSimilarity(models.WordWindow a, models.WordWindow b) {
    HashSet<string> set1 = Tokenizer.Tokenize(a);
    HashSet<string> set2 = Tokenizer.Tokenize(b);

    return CosineSimilarity(set1, set2);
  }

  #endregion
  #region Dice Coefficient

  public static double 
  DiceCoefficient(HashSet<string> set1, HashSet<string> set2) {
    int intersection = 0;

    foreach (string word in set1) {
      if (set2.Contains(word)) {
        intersection++;
      }
    }

    int total = set1.Count + set2.Count;
    if (total == 0) {
      return 0.0;
    }

    return (2.0 * intersection) / total;
  }

  public static double 
  DiceCoefficient(List<string> a, List<string> b) {
    HashSet<string> set1 = Tokenizer.Tokenize(a);
    HashSet<string> set2 = Tokenizer.Tokenize(b);

    return DiceCoefficient(set1, set2);
  }

  public static double 
  DiceCoefficient(models.WordWindow a, models.WordWindow b) {
    HashSet<string> set1 = Tokenizer.Tokenize(a);
    HashSet<string> set2 = Tokenizer.Tokenize(b);

    return DiceCoefficient(set1, set2);
  }

  #endregion
  #region ROUGEL

  public static double 
  RougeL(List<string> a, List<string> b) {
    int row_count = a.Count;
    int column_count = b.Count;

    if (row_count == 0 || column_count == 0) {
      return 0.0;
    }

    // Longest Common Subsequence table
    int[] previous = new int[column_count + 1];
    int[] current = new int[column_count + 1];

    for (int row = 0; row < row_count; row++) {
      for (int column = 0; column < column_count; column++) {

        if (string.Equals(a[row], b[column], StringComparison.OrdinalIgnoreCase)) {
          current[column + 1] = previous[column] + 1;
        }
        else {
          current[column + 1] = Math.Max(previous[column + 1], current[column]);
        }
      }

      // Swap rows instead of copying the whole array
      int[] temp = previous;
      previous = current;
      current = temp;

      // Clear the reused row
      Array.Clear(current, 0, current.Length);
    }

    int len_lcst = previous[column_count];
    double precision = len_lcst / (double)column_count;
    double recall = len_lcst / (double)row_count;
    double precision_plus_recall = precision + recall;

    if (precision_plus_recall == 0) {
      return 0.0;
    }

    return (2.0 * precision * recall) / precision_plus_recall;
  }

  #endregion
  #region Jaccard Similarity 

  /// <summary>
  /// Skips the tokenizing part since it's assumed that is already
  /// done here </summary>
  public static double 
  JaccardSimilarity(HashSet<string> set1, HashSet<string> set2) {
    int intersection = 0;

    foreach (string word in set1) {
      if (set2.Contains(word)) {
        intersection++;
      }
    }

    int union = set1.Count + set2.Count - intersection;

    if (union == 0) {
      return 0.0;
    }

    return (double)intersection / union;
  }

  public static double 
  JaccardSimilarity(List<string> a, List<string> b) {
    HashSet<string> set1 = Tokenizer.Tokenize(a);
    HashSet<string> set2 = Tokenizer.Tokenize(b);

    return JaccardSimilarity(set1, set2);
  }

  public static double 
  JaccardSimilarity(models.WordWindow a, models.WordWindow b) {
    HashSet<string> set1 = Tokenizer.Tokenize(a);
    HashSet<string> set2 = Tokenizer.Tokenize(b);

    return JaccardSimilarity(set1, set2);
  }
  
  #endregion
  #region Consecutive Chain

  public static ConsecutiveChainResult
  FindStrictConsecutiveChain(List<int> prev_matches, List<List<int>> new_matches) {

    int longest_chain_length = 0;
    int longest_chain_start = -1;
    int longest_chain_end = -1;

    // Loop through each starting index in prev_matches
    for (int i = 0; i < prev_matches.Count; i++) {

      int prev_index = prev_matches[i];

      // Try every starting value in the current new_matches[prev_index] list
      foreach (int j in new_matches[prev_index]) {

        int current_chain_length = 1;
        int current_value = j;

        // Attempt to extend the chain as long as consecutive values continue
        for (int k = 1; k < prev_matches.Count - i; k++) {

          int next_prev_index = prev_matches[i + k];
          List<int> next_values = new_matches[next_prev_index];

          if (next_values.Contains(current_value + 1)) {
            current_value++;
            current_chain_length++;
          }
          else {
            break;
          }
        }

        if (current_chain_length > longest_chain_length) {
          longest_chain_length = current_chain_length;
          longest_chain_start = prev_matches[i];
          longest_chain_end = prev_matches[i + current_chain_length - 1];
        }
      }
    }

    return new ConsecutiveChainResult(longest_chain_start, longest_chain_end);
  }
  #endregion
}
