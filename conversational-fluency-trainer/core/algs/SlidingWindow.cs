using core.algs.models;
namespace core.algs;

public static class
SlidingWindow {
  private static double 
  sliding_window_comparison(List<string> prev, List<string> cur, bool is_jaccard) {
    if (prev.Count == 0 && cur.Count == 0) {
      return 0;
    }

    int prev_win;
    if (prev.Count > 1) {
      prev_win = Math.Min(MIN_WINDOW_SIZE, prev.Count);
    }
    else {
      prev_win = 1;
    }

    int cur_win;
    if (cur.Count > 1) {
      cur_win = Math.Min(MIN_WINDOW_SIZE, cur.Count);
    }
    else {
      cur_win = 1;
    }

    int matches = 0;

    for (int i = 0; i <= prev.Count - prev_win; i++) {
      for (int j = 0; j <= cur.Count - cur_win; j++) {

        WordWindow prev_window = new (prev, i, prev_win);
        WordWindow cur_window = new (cur, j, cur_win);

        double sim;
        // todo 2; fix this. should not have an if like that
        if (is_jaccard) {
          sim = SimilarityMetrics.JaccardSimilarity(prev_window, cur_window);
        }
        else {
          sim = SimilarityMetrics.CosineSimilarity(prev_window, cur_window);
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

  private const double SIMILARITY_THRESHOLD = .75;
  private const double THRESHOLD_BASE = .15;
  private const int MIN_WINDOW_SIZE = 4;
  private static readonly double VERY_LOW_THRESHOLD = Math.Clamp(THRESHOLD_BASE * 1, 0.0, 1.0);
  private static readonly double LOW_THRESHOLD = Math.Clamp(THRESHOLD_BASE * 2, 0.0, 1.0);
  private static readonly double MED_THRESHOLD = Math.Clamp(THRESHOLD_BASE * 3, 0.0, 1.0);
  private static readonly double HIGH_THRESHOLD = Math.Clamp(THRESHOLD_BASE * 4, 0.0, 1.0);
  private static readonly double VERY_HIGH_THRESHOLD = Math.Clamp(THRESHOLD_BASE * 5, 0.0, 1.0);
}
