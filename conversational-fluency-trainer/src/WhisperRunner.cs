using System.Diagnostics;
using core.strucutres.dawg;
using core.strucutres.dawg.models;
using core.services.json_serializer;
namespace conversational_fluency_trainer;

public class
WhisperRunner {
  public WhisperRunner() {
    string k_dir = core.infra.Paths.ConfsDirectory;
    config_ = new JsonSerializer($"{k_dir}/WhisperConfig.json")
      .Deserialize<config_models.AppConfig>();
  }
  public async Task
  RunAsync() {
    
    string k_deps = core.infra.Paths.DepsDirectory;
    string model = $"-m \"{k_deps}/{config_.Whisper.FileNames.Model}\"";
    string threads = $"-t {config_.Whisper.Arguments.Threads}";
    string language = $"-l {config_.Whisper.Arguments.Language}";
    string processors = $"-c {config_.Whisper.Arguments.Processors}";

    ProcessStartInfo psi = new() {
      FileName = $"{k_deps}/{config_.Whisper.FileNames.Executable}",
      Arguments = $"{model} {threads} {language} {processors}",
      RedirectStandardOutput = config_.Whisper.Process.RedirectOutput,
      RedirectStandardError = config_.Whisper.Process.RedirectError,
      RedirectStandardInput = config_.Whisper.Process.RedirectInput,
      UseShellExecute = config_.Whisper.Process.UseShellExecute,
      CreateNoWindow = config_.Whisper.Process.CreateNoWindow,
      WorkingDirectory = Path.GetDirectoryName(config_.Whisper.FileNames.Executable)
    };

    process_.StartInfo = psi;
    process_.OutputDataReceived += on_output_data_received;
    process_.ErrorDataReceived += on_error_data_received;
    // Note: there is a problem running this in debug move that it holds onto
    // the process in memory and you need to force kill the task to free up 
    // the gpu vram.
    said_words_ = LoadDawgTrainingData();
    process_.Start();

    process_.BeginOutputReadLine();
    process_.BeginErrorReadLine();
    await process_.WaitForExitAsync();
    process_.Kill(); // ensures process cleans up fully
  }

  private void
  on_output_data_received(object sender, DataReceivedEventArgs e) {
    if (!string.IsNullOrWhiteSpace(e.Data)) {
      List<string> words = core.algs.Tokenizer.CleanSplitFilterToken(e.Data, config_.Whisper.Filters);
      said_words_.Insert(words);
      if (words.Count > 0) {
        Console.WriteLine("STDOUT: " + e.Data);
        List<Suggestion> suggestions = said_words_.GetSuggestions(words, 5, 5);
        Console.WriteLine("DAWG suggestions:");
        foreach (Suggestion item in suggestions) {
          Console.WriteLine("\t\t" + item.Value + " (" + item.Weight + ") - " + string.Join(" ", item.Preview));
        }
      }
    }
  }

  public static DirectedAcyclicWordGraph
  LoadDawgTrainingData() {
    DirectedAcyclicWordGraph dawg = new();

    string directory = "/var/tmp/de/";

    if (!Directory.Exists(directory)) {
      throw new DirectoryNotFoundException(directory);
    }

    int loaded = 0;

    foreach (string file in Directory.EnumerateFiles(directory, "*.txt")) {
      using FileStream stream = File.OpenRead(file);

      dawg.LoadFromStream(stream);
      loaded++;

      Console.WriteLine($"Loaded {loaded}: {Path.GetFileName(file)}");
    }

    Console.WriteLine($"Training complete. Files: {loaded}, Nodes: {dawg.NodeMap.Count}");
    return dawg;
  }

  private void
  on_error_data_received(object sender, DataReceivedEventArgs e) {
    if (!string.IsNullOrWhiteSpace(e.Data)) {
      Console.WriteLine("STDERR: " + e.Data);
    }
  }

  private Process process_ = new Process();
  private config_models.AppConfig config_;
  private DirectedAcyclicWordGraph said_words_ = new();
  // todo 3; these are gonna need a unit test
  // todo 2; make global file of consts so unit tests are consistent and paths
  //         stable/findable
}
