using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

public class StringConcat {
    static double _threshold_base = .15;

    static double VERY_LOW_THRESHOLD = Math.Max(0.0, Math.Min(_threshold_base * 1, 1.0));
    static double LOW_THRESHOLD = Math.Max(0.0, Math.Min(_threshold_base * 2, 1.0));
    static double MED_THRESHOLD = Math.Max(0.0, Math.Min(_threshold_base * 3, 1.0));
    static double HIGH_THRESHOLD = Math.Max(0.0, Math.Min(_threshold_base * 4, 1.0));
    static double VERY_HIGH_THRESHOLD = Math.Max(0.0, Math.Min(_threshold_base * 5, 1.0));
    static double SIMILARITY_THRESHOLD = .75;
    static Regex ansi_escape = new Regex(@"\x1B\[[0-?]*[ -/]*[@-~]", RegexOptions.Compiled);
    static HashSet<string> whisper_fill_ins_to_skip = new HashSet<string>
    {
        "", " ", " .", " . ", " .\n", " .\x1b[2K\n",
        "(clapping)", "(explosion)", "(explosion)\x1b[2K",
        "(explosions)", "(explosions)\x1b[2K",
        "(fire crackling)", "(giggles)", "(humming)",
        "(laughing)", "(mimics whooshing)", "(water",
        "(water splashing)", "[2K]", "[BLANK_AUDIO]",
        "[BLANK_AUDIO]\x1b[2K", "[MUSIC]",
        "*sad", "music*", "[SPEAKING", "[Start",
        "*laughs*", "*Loud", "noise*", ".",
        "Device", "Device 0:", "ggml_cuda_init:",
        "init:", "main:", "whisper_backend_init_gpu:",
        "whisper_init_from_file_with_params_no_state:",
        "whisper_init_state:", "whisper_init_with_params_no_state:",
        "whisper_model_load:", "\x1b", "\x1b[2K",
        "[", "Silence", "]"
    };

    static string StripMarkdown(string reply) {
        reply = Regex.Replace(reply, @"\*\*(.*?)\*\*", "$1");
        reply = Regex.Replace(reply, @"\*(.*?)\*", "$1");
        reply = Regex.Replace(reply, @"__(.*?)__", "$1");
        reply = Regex.Replace(reply, @"^\s*\d+\.\s+", "", RegexOptions.Multiline);
        reply = Regex.Replace(reply, @"^\s*[-*]\s+", "", RegexOptions.Multiline);
        reply = Regex.Replace(reply, @"^\s*#{1,6}\s+", "", RegexOptions.Multiline);
        reply = Regex.Replace(reply, @"^\s*:\s*", "", RegexOptions.Multiline);
        return reply.Trim();
    }

    static string ParseForEndSentence(string reply) {
        reply = reply.Replace("> ", "").Replace("EOF by user", "").Trim();
        reply = StripMarkdown(reply);

        var parts = Regex.Split(reply, @"(?<=[.?!])\s+");
        var sentences = new StringBuilder();

        foreach (var p in parts) {
            var clean = p.Trim();
            if (clean.Length >= 10 && clean.Split(' ').Length >= 3 && !clean.EndsWith(":")) {
                sentences.Append(" ").Append(clean);
                if (sentences.ToString().Trim().Length > 200) {
                    break;
                }
            }
        }

        return sentences.ToString().Trim();
    }

    static void PrintOutCallResponse(string call, string response) {
        Console.WriteLine("\n----------------------------------------------");
        Console.WriteLine("Speaker: " + call);
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine("AI: " + response);
        Console.WriteLine("----------------------------------------------");
    }

    static HashSet<string> Tokenize(List<string> words) {
        var text = string.Join(" ", words).ToLower();
        text = Regex.Replace(text, @"[^\w\s]", "");
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
    }

    static double JaccardSimilarity(List<string> a, List<string> b) {
        var set1 = Tokenize(a);
        var set2 = Tokenize(b);

        var inter = set1.Intersect(set2).Count();
        var union = set1.Union(set2).Count();

        return union == 0 ? 0.0 : (double)inter / union;
    }

    static double CosineSimilarity(List<string> a, List<string> b) {
        var set1 = Tokenize(a);
        var set2 = Tokenize(b);
        int inter = 0;
        foreach (string item in set1) {
            if (set2.Contains(item)) {
                inter++;
            }
        }

        return (set1.Count == 0 || set2.Count == 0)
            ? 0.0 : (double)inter / Math.Sqrt(set1.Count * set2.Count);
    }

    static double DiceCoefficient(List<string> a, List<string> b) {
        var set1 = Tokenize(a);
        var set2 = Tokenize(b);
        int inter = 0;
        foreach (string item in set1) {
            if (set2.Contains(item)) {
                inter++;
            }
        }
        var total = set1.Count + set2.Count;

        return total == 0 ? 0.0 : (2.0 * inter) / total;
    }

    static double RougeL(List<string> a, List<string> b) {
        int rowCount = a.Count;
        int columnCount = b.Count;

        // Explicitly allocating an array of arrays (jagged array)
        int[][] longestCommonSubsequenceTable = new int[rowCount + 1][];
        for (int i = 0; i <= rowCount; i++) {
            longestCommonSubsequenceTable[i] = new int[columnCount + 1];
        }

        for (int row = 0; row < rowCount; row++) {
            for (int column = 0; column < columnCount; column++) {
                int currentRow = row + 1;
                int currentColumn = column + 1;

                string leftToken = a[row].ToLower();
                string rightToken = b[column].ToLower();

                if (leftToken == rightToken) {
                    // Explicitly grabbing from the previous row and previous column
                    int diagonalValue = longestCommonSubsequenceTable[row][column];
                    longestCommonSubsequenceTable[currentRow][currentColumn] = diagonalValue + 1;
                }
                else {
                    // Explicitly comparing the cell directly above and the cell to the exact left
                    int valueAbove = longestCommonSubsequenceTable[row][currentColumn];
                    int valueToLeft = longestCommonSubsequenceTable[currentRow][column];
                    longestCommonSubsequenceTable[currentRow][currentColumn] = Math.Max(valueAbove, valueToLeft);
                }
            }
        }

        int longestCommonSubsequenceLength = longestCommonSubsequenceTable[rowCount][columnCount];

        double precision = longestCommonSubsequenceLength / (double)Math.Max(1, columnCount);
        double recall = longestCommonSubsequenceLength / (double)Math.Max(1, rowCount);
        double precisionPlusRecall = precision + recall;

        if (precisionPlusRecall == 0) {
            return 0.0;
        }

        return (2.0 * precision * recall) / precisionPlusRecall;
    }

    static double SlidingWindowComparison(List<string> prev, List<string> cur, bool isJaccard) {
        if (prev.Count == 0 && cur.Count == 0) {
            return 0;
        }

        int prevWin = prev.Count > 1 ? Math.Min(4, prev.Count) : 1;
        int curWin = cur.Count > 1 ? Math.Min(4, cur.Count) : 1;
        int matches = 0;

        for (int i = 0; i <= prev.Count - prevWin; i++) {
            for (int j = 0; j <= cur.Count - curWin; j++) {
                double sim = isJaccard
                    ? JaccardSimilarity(prev.GetRange(i, prevWin), cur.GetRange(j, curWin))
                    : CosineSimilarity(prev.GetRange(i, prevWin), cur.GetRange(j, curWin));

                if (sim >= SIMILARITY_THRESHOLD) {
                    matches++;
                    break;
                }
            }
        }

        return matches / (double)Math.Max(1, prev.Count - prevWin + 1);
    }

    // todo; can revise this to not be "is jaccard"
    static double SlidingWindowComparisonWindow(List<string> prev, List<string> cur, bool isJaccard) {
        if (prev.Count == 0 && cur.Count == 0) { 
            return 0;
        }

        int prevWin = prev.Count > 1 ? Math.Min(4, prev.Count) : 1;
        int curWin = cur.Count > 1 ? Math.Min(4, cur.Count) : 1;
        int matches = 0;

        for (int i = 0; i <= prev.Count - prevWin; i++) {
            for (int j = 0; j <= cur.Count - curWin; j++) {
                // Pass the original lists, the starting index, and the window size explicitly
                double sim = isJaccard
                    ? JaccardSimilarityWindow(prev, i, prevWin, cur, j, curWin)
                    : CosineSimilarityWindow(prev, i, prevWin, cur, j, curWin);

                if (sim >= SIMILARITY_THRESHOLD) {
                    matches++;
                    break;
                }
            }
        }

        return matches / (double)Math.Max(1, prev.Count - prevWin + 1);
    }

    static HashSet<string> TokenizeWindow(List<string> words, int offset, int length) {
        // Explicitly building the string loop only from the specified window range
        var sb = new StringBuilder();
        for (int i = offset; i < offset + length; i++) {
            sb.Append(words[i]).Append(" ");
        }
        var text = sb.ToString().ToLower();
        text = Regex.Replace(text, @"[^\w\s]", "");
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
    }

    static double JaccardSimilarityWindow(List<string> a, int aOffset, int aLen, List<string> b, int bOffset, int bLen) {
        var set1 = TokenizeWindow(a, aOffset, aLen);
        var set2 = TokenizeWindow(b, bOffset, bLen);

        // Explicit manual intersection loop to avoid LINQ deferred execution magic
        int inter = 0;
        foreach (string item in set1) {
            if (set2.Contains(item)) {
                inter++;
            }
        }

        int union = set1.Count + set2.Count - inter;
        return union == 0 ? 0.0 : (double)inter / union;
    }

    static double CosineSimilarityWindow(List<string> a, int aOffset, int aLen, List<string> b, int bOffset, int bLen) {
        var set1 = TokenizeWindow(a, aOffset, aLen);
        var set2 = TokenizeWindow(b, bOffset, bLen);

        int inter = 0;
        foreach (string item in set1) {
            if (set2.Contains(item)) {
                inter++;
            }
        }

        return (set1.Count == 0 || set2.Count == 0)
            ? 0.0
            : (double)inter / Math.Sqrt(set1.Count * set2.Count);
    }


    static List<string> WhisperReadLine(Process whisperProc, ref List<string> prevSentence, List<string> concatList) {
        var line = whisperProc.StandardOutput.ReadLine();
        if (line == null) {
            return new List<string>();
        }

        var clean = ansi_escape.Replace(line, "").Trim();
        var newSentence = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        if (newSentence.Count <= 1) {
            return new List<string>();
        }
        if (whisper_fill_ins_to_skip.Contains(clean)) {
            return new List<string>();
        }

        // double jaccard = SlidingWindowComparison(prevSentence, newSentence, true);
        // double cosine = SlidingWindowComparison(prevSentence, newSentence, false);
        // NOTE: These eliminate the need for some linq
        double jaccard = SlidingWindowComparisonWindow(prevSentence, newSentence, true);
        double cosine = SlidingWindowComparisonWindow(prevSentence, newSentence, false);

        double dice = DiceCoefficient(prevSentence, newSentence);
        double rouge = RougeL(prevSentence, newSentence);

        bool matchData = prevSentence.Count > 0 && newSentence.Count > 0;

        bool overwrite =
            jaccard > HIGH_THRESHOLD &&
            cosine > VERY_HIGH_THRESHOLD &&
            dice > VERY_HIGH_THRESHOLD &&
            rouge > MED_THRESHOLD;

        bool append =
            jaccard > VERY_LOW_THRESHOLD &&
            cosine > LOW_THRESHOLD &&
            dice > LOW_THRESHOLD &&
            rouge > LOW_THRESHOLD;

        bool lastWordMatch = prevSentence.Count > 0 && prevSentence[prevSentence.Count - 1] == newSentence[0];

        int iIndex = concatList.Count - prevSentence.Count;

        if (!matchData) {
            concatList.AddRange(newSentence);
        }
        else if (overwrite) {
            concatList = concatList.Take(iIndex).Concat(newSentence).ToList();
        }
        else if (append) {
            concatList.AddRange(newSentence);
        }
        else if (lastWordMatch) {
            concatList = concatList.Take(iIndex)
                .Concat(prevSentence.Take(prevSentence.Count - 1))
                .Concat(newSentence)
                .ToList();
        }
        else {
            concatList.AddRange(newSentence);
        }

        prevSentence = newSentence;
        return newSentence;
    }
}
