namespace core.algs;
public static class SimilarityMetrics {

    public static double 
    CosineSimilarity(List<string> a, List<string> b) {
        HashSet<string> set1 = Tokenizer.Tokenize(a);
        HashSet<string> set2 = Tokenizer.Tokenize(b);
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
    DiceCoefficient(List<string> a, List<string> b) {
        HashSet<string> set1 = Tokenizer.Tokenize(a);
        HashSet<string> set2 = Tokenizer.Tokenize(b);
        int inter = 0;

        foreach (string item in set1) {
            if (set2.Contains(item)) {
                inter++;
            }
        }

        int total = set1.Count + set2.Count;
        if (total == 0) {
            return 0.0;
        }

        return (2.0 * inter) / total;
    }

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

    public static double 
    CosineSimilarityWindow(List<string> a, int a_offset, int a_len, 
                           List<string> b, int b_offset, int b_len) {
        HashSet<string> set1 = Tokenizer.TokenizeWindow(a, a_offset, a_len);
        HashSet<string> set2 = Tokenizer.TokenizeWindow(b, b_offset, b_len);

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

        int inter = 0;

        foreach (string item in set1) {
            if (set2.Contains(item)) {
                inter++;
            }
        }

        int union = set1.Count + set2.Count - inter;

        if (union == 0) {
            return 0.0;
        }

        return (double)inter / union;
    }

    public static double 
    JaccardSimilarity(models.WordWindow a, models.WordWindow b) {
        HashSet<string> set1 = Tokenizer.Tokenize(a);
        HashSet<string> set2 = Tokenizer.Tokenize(b);

        int inter = 0;

        foreach (string item in set1) {
            if (set2.Contains(item)) {
                inter++;
            }
        }

        int union = set1.Count + set2.Count - inter;

        if (union == 0) {
            return 0.0;
        }

        return (double)inter / union;
    }
}
