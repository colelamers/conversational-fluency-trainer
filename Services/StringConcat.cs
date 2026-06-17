using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
namespace conversational_fluency_trainer.Services;

public class StringConcat {
    const double SIMILARITY_THRESHOLD = .75;
    const double THRESHOLD_BASE = .15;
    static readonly double VERY_LOW_THRESHOLD = Math.Clamp(THRESHOLD_BASE * 1, 0.0, 1.0);
    static readonly double LOW_THRESHOLD = Math.Clamp(THRESHOLD_BASE * 2, 0.0, 1.0);
    static readonly double MED_THRESHOLD = Math.Clamp(THRESHOLD_BASE * 3, 0.0, 1.0);
    static readonly double HIGH_THRESHOLD = Math.Clamp(THRESHOLD_BASE * 4, 0.0, 1.0);
    static readonly double VERY_HIGH_THRESHOLD = Math.Clamp(THRESHOLD_BASE * 5, 0.0, 1.0);
    static Regex ansi_escape_ = new Regex(@"\x1B\[[0-?]*[ -/]*[@-~]", RegexOptions.Compiled);
    static HashSet<string> whisper_fill_ins_to_skip_ = new HashSet<string>
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

    static string 
    StripMarkdown(string reply) {
        reply = Regex.Replace(reply, @"\*\*(.*?)\*\*", "$1");
        reply = Regex.Replace(reply, @"\*(.*?)\*", "$1");
        reply = Regex.Replace(reply, @"__(.*?)__", "$1");
        reply = Regex.Replace(reply, @"^\s*\d+\.\s+", "", RegexOptions.Multiline);
        reply = Regex.Replace(reply, @"^\s*[-*]\s+", "", RegexOptions.Multiline);
        reply = Regex.Replace(reply, @"^\s*#{1,6}\s+", "", RegexOptions.Multiline);
        reply = Regex.Replace(reply, @"^\s*:\s*", "", RegexOptions.Multiline);
        return reply.Trim();
    }

    static string 
    ParseForEndSentence(string reply) {
        reply = reply.Replace("> ", "").Replace("EOF by user", "").Trim();
        reply = StripMarkdown(reply);

        string[] parts = Regex.Split(reply, @"(?<=[.?!])\s+");
        StringBuilder sentences = new StringBuilder();

        foreach (string p in parts) {
            string clean = p.Trim();
            if (clean.Length >= 10 && clean.Split(' ').Length >= 3 && !clean.EndsWith(":")) {
                sentences.Append(" ").Append(clean);
                if (sentences.ToString().Trim().Length > 200) {
                    break;
                }
            }
        }

        return sentences.ToString().Trim();
    }

    static void 
    PrintOutCallResponse(string call, string response) {
        Console.WriteLine("\n----------------------------------------------");
        Console.WriteLine("Speaker: " + call);
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine("AI: " + response);
        Console.WriteLine("----------------------------------------------");
    }

    public static HashSet<string> 
    Tokenize(List<string> words) {
        string text = string.Join(" ", words).ToLower();
        text = Regex.Replace(text, @"[^\w\s]", "");
        return new HashSet<string>(
            text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        );
    }

    public static double 
    JaccardSimilarity(List<string> a, List<string> b) {
        HashSet<string> set1 = Tokenize(a);
        HashSet<string> set2 = Tokenize(b);

        HashSet<string> inter_set = new HashSet<string>(set1);
        inter_set.IntersectWith(set2);

        HashSet<string> union_set = new HashSet<string>(set1);
        union_set.UnionWith(set2);

        int inter = inter_set.Count;
        int union = union_set.Count;

        return union == 0 ? 0.0 : (double)inter / union;
    }

    public static double 
    CosineSimilarity(List<string> a, List<string> b) {
        HashSet<string> set1 = Tokenize(a);
        HashSet<string> set2 = Tokenize(b);
        int inter = 0;
        foreach (string item in set1) {
            if (set2.Contains(item)) {
                inter++;
            }
        }

        return (set1.Count == 0 || set2.Count == 0)
            ? 0.0 : (double)inter / Math.Sqrt(set1.Count * set2.Count);
    }

    public static double 
    DiceCoefficient(List<string> a, List<string> b) {
        HashSet<string> set1 = Tokenize(a);
        HashSet<string> set2 = Tokenize(b);
        int inter = 0;
        foreach (string item in set1) {
            if (set2.Contains(item)) {
                inter++;
            }
        }
        int total = set1.Count + set2.Count;

        return total == 0 ? 0.0 : (2.0 * inter) / total;
    }

    public static double 
    RougeL(List<string> a, List<string> b) {
        int row_count = a.Count;
        int column_count = b.Count;

        // Explicitly allocating an array of arrays (jagged array)
        // Longest Common Subsequence Table
        int[][] lcst = new int[row_count + 1][];
        for (int i = 0; i <= row_count; i++) {
            lcst[i] = new int[column_count + 1];
        }

        for (int row = 0; row < row_count; row++) {
            for (int column = 0; column < column_count; column++) {
                int current_row = row + 1;
                int current_column = column + 1;

                string left_token = a[row].ToLower();
                string right_token = b[column].ToLower();

                if (left_token == right_token) {
                    // Explicitly grabbing from the previous row and previous column
                    int diagonal_value = lcst[row][column];
                    lcst[current_row][current_column] = diagonal_value + 1;
                }
                else {
                    // Explicitly comparing the cell directly above and the cell to the exact left
                    lcst[current_row][current_column] = Math.Max(
                        lcst[row][current_column], lcst[current_row][column]);
                }
            }
        }

        int len_lcst = lcst[row_count][column_count];

        double precision = len_lcst / (double)Math.Max(1, column_count);
        double recall = len_lcst / (double)Math.Max(1, row_count);
        double precision_plus_recall = precision + recall;

        if (precision_plus_recall == 0) {
            return 0.0;
        }

        return (2.0 * precision * recall) / precision_plus_recall;
    }

    static double 
    SlidingWindowComparison(List<string> prev, List<string> cur, bool is_jaccard) {
        if (prev.Count == 0 && cur.Count == 0) {
            return 0;
        }

        int prev_win;
        if (prev.Count > 1) {
            prev_win = Math.Min(4, prev.Count);
        }
        else {
            prev_win = 1;
        }

        int cur_win;
        if (cur.Count > 1) {
            cur_win = Math.Min(4, cur.Count);
        }
        else {
            cur_win = 1;
        }

        int matches = 0;

        for (int i = 0; i <= prev.Count - prev_win; i++) {
            for (int j = 0; j <= cur.Count - cur_win; j++) {
                double sim;

                if (is_jaccard) {
                    sim = JaccardSimilarity(prev.GetRange(i, prev_win), cur.GetRange(j, cur_win));
                }
                else {
                    sim = CosineSimilarity(prev.GetRange(i, prev_win), cur.GetRange(j, cur_win));
                }

                if (sim >= SIMILARITY_THRESHOLD) {
                    matches++;
                    break;
                }
            }
        }

        double denominator = Math.Max(1, prev.Count - prev_win + 1);
        return matches / denominator;
    }

    // todo; can revise this to not be "is jaccard"
    static double 
    SlidingWindowComparisonWindow(List<string> prev, List<string> cur, bool isJaccard) {
        if (prev.Count == 0 && cur.Count == 0) {
            return 0;
        }

        int prev_win;
        if (prev.Count > 1) {
            prev_win = Math.Min(4, prev.Count);
        }
        else {
            prev_win = 1;
        }

        int cur_win;
        if (cur.Count > 1) {
            cur_win = Math.Min(4, cur.Count);
        }
        else {
            cur_win = 1;
        }

        int matches = 0;

        for (int i = 0; i <= prev.Count - prev_win; i++) {
            for (int j = 0; j <= cur.Count - cur_win; j++) {
                double sim;

                if (isJaccard) {
                    sim = JaccardSimilarityWindow(prev, i, prev_win, cur, j, cur_win);
                }
                else {
                    sim = CosineSimilarityWindow(prev, i, prev_win, cur, j, cur_win);
                }

                if (sim >= SIMILARITY_THRESHOLD) {
                    matches++;
                    break;
                }
            }
        }

        double denominator = Math.Max(1, prev.Count - prev_win + 1);
        return matches / denominator;
    }

    static HashSet<string> 
    TokenizeWindow(List<string> words, int offset, int length) {
        // Explicitly building the string loop only from the specified window range
        StringBuilder sb = new StringBuilder();
        for (int i = offset; i < offset + length; i++) {
            sb.Append(words[i]).Append(" ");
        }
        string text = sb.ToString().ToLower();
        text = Regex.Replace(text, @"[^\w\s]", "");
        return new HashSet<string>(
            text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        );
    }

    static double 
    JaccardSimilarityWindow(List<string> a, int a_offset, int a_len, 
                            List<string> b, int b_offset, int b_len) {
        HashSet<string> set1 = TokenizeWindow(a, a_offset, a_len);
        HashSet<string> set2 = TokenizeWindow(b, b_offset, b_len);

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

    static double 
    CosineSimilarityWindow(List<string> a, int a_offset, int a_len, 
                           List<string> b, int b_offset, int b_len) {
        HashSet<string> set1 = TokenizeWindow(a, a_offset, a_len);
        HashSet<string> set2 = TokenizeWindow(b, b_offset, b_len);

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


    static List<string>
    WhisperReadLine(Process whisperProc, ref List<string> prev_sentence, List<string> concat_list) {
        string? line = whisperProc.StandardOutput.ReadLine();
        if (line == null) {
            return new List<string>();
        }

        string clean = ansi_escape_.Replace(line, "").Trim();
        List<string> new_sentence = new List<string>(
            clean.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        );
        if (new_sentence.Count <= 1) {
            return new List<string>();
        }

        if (whisper_fill_ins_to_skip_.Contains(clean)) {
            return new List<string>();
        }

        // double jaccard = SlidingWindowComparison(prev_sentence, newSentence, true);
        // double cosine = SlidingWindowComparison(prev_sentence, newSentence, false);
        // NOTE: These eliminate the need for some linq
        double jaccard = SlidingWindowComparisonWindow(prev_sentence, new_sentence, true);
        double cosine = SlidingWindowComparisonWindow(prev_sentence, new_sentence, false);

        double dice = DiceCoefficient(prev_sentence, new_sentence);
        double rouge = RougeL(prev_sentence, new_sentence);
        // todo; revise all these bools
        bool match_data = prev_sentence.Count > 0 && new_sentence.Count > 0;
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

        // todo; revise this
        bool last_word_match = prev_sentence.Count > 0 && prev_sentence[prev_sentence.Count - 1] == new_sentence[0];

        int idx = concat_list.Count - prev_sentence.Count;

        if (!match_data) {
            // todo; i heavily dislike this linq
            concat_list.AddRange(new_sentence);
        }
        else if (overwrite) {
            concat_list.RemoveRange(idx, concat_list.Count - idx);
            concat_list.AddRange(new_sentence);
        }
        else if (append) {
            // todo; i heavily dislike this linq
            concat_list.AddRange(new_sentence);
        }
        else if (last_word_match) {
            concat_list.RemoveRange(idx, concat_list.Count - idx);
            for (int i = 0; i < prev_sentence.Count - 1; i++) {
                concat_list.Add(prev_sentence[i]);
            }

            concat_list.AddRange(new_sentence);
        }
        else {
            // todo; i heavily dislike this linq
            concat_list.AddRange(new_sentence);
        }

        prev_sentence = new_sentence;
        return new_sentence;
    }
}
