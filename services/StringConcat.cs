using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using core.algs;
namespace conversational_fluency_trainer.services;

public class StringConcat {
    /*
        // todo 1; implement this cache at some point to speed up the redundant
        // iterations

        readonly struct TokenWindow
        {
            public int StartIndex { get; init; }
            public int WordCount { get; init; }

            public TokenWindow(int startIndex, int wordCount)
            {
                StartIndex = startIndex;
                WordCount = wordCount;
            }
        }

        Then:

        Dictionary<TokenWindow, HashSet<string>> cache = new();

        When you read:

        cache[new TokenWindow(5, 4)];
    */
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
                    sim = SimilarityMetrics.JaccardSimilarity(prev.GetRange(i, prev_win), cur.GetRange(j, cur_win));
                }
                else {
                    sim = SimilarityMetrics.CosineSimilarity(prev.GetRange(i, prev_win), cur.GetRange(j, cur_win));
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
                    sim = SimilarityMetrics.JaccardSimilarityWindow(prev, i, prev_win, cur, j, cur_win);
                }
                else {
                    sim = SimilarityMetrics.CosineSimilarityWindow(prev, i, prev_win, cur, j, cur_win);
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




    static List<string>
    WhisperReadLine(Process whisperProc, ref List<string> prev_sentence, List<string> concat_list) 
    {
        string? line = whisperProc.StandardOutput.ReadLine();
        if (line == null) {
            return new List<string>();
        }

        string clean = ansi_escape_.Replace(line, "").Trim();
        List<string> new_sentence = new(clean.Split(' ', StringSplitOptions.RemoveEmptyEntries)
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
        double dice = SimilarityMetrics.DiceCoefficient(prev_sentence, new_sentence);
        double rouge = SimilarityMetrics.RougeL(prev_sentence, new_sentence);
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
