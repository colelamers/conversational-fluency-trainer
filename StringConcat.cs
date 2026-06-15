using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

public class StrincConcat
{
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

    static string StripMarkdown(string reply)
    {
        reply = Regex.Replace(reply, @"\*\*(.*?)\*\*", "$1");
        reply = Regex.Replace(reply, @"\*(.*?)\*", "$1");
        reply = Regex.Replace(reply, @"__(.*?)__", "$1");
        reply = Regex.Replace(reply, @"^\s*\d+\.\s+", "", RegexOptions.Multiline);
        reply = Regex.Replace(reply, @"^\s*[-*]\s+", "", RegexOptions.Multiline);
        reply = Regex.Replace(reply, @"^\s*#{1,6}\s+", "", RegexOptions.Multiline);
        reply = Regex.Replace(reply, @"^\s*:\s*", "", RegexOptions.Multiline);
        return reply.Trim();
    }

    static string ParseForEndSentence(string reply)
    {
        reply = reply.Replace("> ", "").Replace("EOF by user", "").Trim();
        reply = StripMarkdown(reply);

        var parts = Regex.Split(reply, @"(?<=[.?!])\s+");
        var sentences = new StringBuilder();

        foreach (var p in parts)
        {
            var clean = p.Trim();
            if (clean.Length >= 10 && clean.Split(' ').Length >= 3 && !clean.EndsWith(":"))
            {
                sentences.Append(" ").Append(clean);
                if (sentences.ToString().Trim().Length > 200)
                    break;
            }
        }

        return sentences.ToString().Trim();
    }

    static void PrintOutCallResponse(string call, string response)
    {
        Console.WriteLine("\n----------------------------------------------");
        Console.WriteLine("Speaker: " + call);
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine("AI: " + response);
        Console.WriteLine("----------------------------------------------");
    }

    static HashSet<string> Tokenize(List<string> words)
    {
        var text = string.Join(" ", words).ToLower();
        text = Regex.Replace(text, @"[^\w\s]", "");
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
    }

    static double JaccardSimilarity(List<string> a, List<string> b)
    {
        var set1 = Tokenize(a);
        var set2 = Tokenize(b);

        var inter = set1.Intersect(set2).Count();
        var union = set1.Union(set2).Count();

        return union == 0 ? 0.0 : (double)inter / union;
    }

    static double CosineSimilarity(List<string> a, List<string> b)
    {
        var set1 = Tokenize(a);
        var set2 = Tokenize(b);
        var inter = set1.Intersect(set2).Count();

        return (set1.Count == 0 || set2.Count == 0)
            ? 0.0
            : (double)inter / Math.Sqrt(set1.Count * set2.Count);
    }

    static double DiceCoefficient(List<string> a, List<string> b)
    {
        var set1 = Tokenize(a);
        var set2 = Tokenize(b);
        var inter = set1.Intersect(set2).Count();
        var total = set1.Count + set2.Count;

        return total == 0 ? 0.0 : (2.0 * inter) / total;
    }

    static double RougeL(List<string> a, List<string> b)
    {
        int m = a.Count;
        int n = b.Count;

        int[,] dp = new int[m + 1, n + 1];

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (a[i].ToLower() == b[j].ToLower()) 
                {
                    dp[i + 1, j + 1] = dp[i, j] + 1;
                }
                else 
                {
                    dp[i + 1, j + 1] = Math.Max(dp[i, j + 1], dp[i + 1, j]);
                }
            }
        }

        int lcs = dp[m, n];

        double precision = lcs / (double)Math.Max(1, n);
        double recall = lcs / (double)Math.Max(1, m);
        double f1 = precision + recall;

        return f1 > 0 ? (2 * precision * recall) / f1 : 0.0;
    }

    static double SlidingWindowComparison(List<string> prev, List<string> cur, bool isJaccard)
    {
        if (prev.Count == 0 && cur.Count == 0)
            return 0;

        int prevWin = prev.Count > 1 ? Math.Min(4, prev.Count) : 1;
        int curWin = cur.Count > 1 ? Math.Min(4, cur.Count) : 1;
        int matches = 0;

        for (int i = 0; i <= prev.Count - prevWin; i++)
        {
            for (int j = 0; j <= cur.Count - curWin; j++)
            {
                double sim = isJaccard
                    ? JaccardSimilarity(prev.GetRange(i, prevWin), cur.GetRange(j, curWin))
                    : CosineSimilarity(prev.GetRange(i, prevWin), cur.GetRange(j, curWin));

                if (sim >= 0.75)
                {
                    matches++;
                    break;
                }
            }
        }

        return matches / (double)Math.Max(1, prev.Count - prevWin + 1);
    }

    static List<string> WhisperReadLine(Process whisperProc, ref List<string> prevSentence, List<string> concatList)
    {
        var line = whisperProc.StandardOutput.ReadLine();
        if (line == null) return new List<string>();

        var clean = ansi_escape.Replace(line, "").Trim();
        var newSentence = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        if (newSentence.Count <= 1) return new List<string>();
        if (whisper_fill_ins_to_skip.Contains(clean)) return new List<string>();

        var jaccard = SlidingWindowComparison(prevSentence, newSentence, true);
        var cosine = SlidingWindowComparison(prevSentence, newSentence, false);
        var dice = DiceCoefficient(prevSentence, newSentence);
        var rouge = RougeL(prevSentence, newSentence);

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

        bool lastWordMatch =
            prevSentence.Count > 0 &&
            prevSentence[^1] == newSentence[0];

        int iIndex = concatList.Count - prevSentence.Count;

        if (!matchData)
        {
            concatList.AddRange(newSentence);
        }
        else if (overwrite)
        {
            concatList = concatList.Take(iIndex).Concat(newSentence).ToList();
        }
        else if (append)
        {
            concatList.AddRange(newSentence);
        }
        else if (lastWordMatch)
        {
            concatList = concatList.Take(iIndex)
                .Concat(prevSentence.Take(prevSentence.Count - 1))
                .Concat(newSentence)
                .ToList();
        }
        else
        {
            concatList.AddRange(newSentence);
        }

        prevSentence = newSentence;
        return newSentence;
    }
}
