using System.Diagnostics;
using core.strucutres.dawg;
using core.strucutres.dawg.models;
using core.services.json_serializer;
using System.Linq;

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
    string processors = $"-c {config_.Whisper.Arguments.MicHardwareNumber}";

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

    core.infra.Memory.MemoryMeasure measure = new core.infra.Memory.MemoryMeasure();

    using (measure) {
        
        // 2. Load your DAWG training data inside the block
        said_words_ = LoadDawgTrainingData();
        
        List<int> records = said_words_.GetSequenceIds("Morgen").ToList();
        List<string> full_k = said_words_.GetSequence(records[17]);
        
        // 3. Manually call your method to check the size BEFORE the block ends
        long bytesUsed = measure.GetTotalMemory();
        
        // 4. Print the size in Megabytes so it's readable
        double megabytes = bytesUsed / 1024.0 / 1024.0;
        Console.WriteLine($"DAWG Memory Size: {megabytes:F2} MB ({bytesUsed:N0} bytes)");
    }
    process_.Start();

    process_.BeginOutputReadLine();
    process_.BeginErrorReadLine();
    await process_.WaitForExitAsync();
    process_.Kill(); // ensures process cleans up fully
  }

  private void
  on_output_data_received(object sender, DataReceivedEventArgs e) {
    if (!string.IsNullOrWhiteSpace(e.Data)) {

      List<string> words = core.algs.Tokenizer.CleanSplitFilterToken(
        e.Data, config_.Whisper.Filters);
      said_words_.Insert(words);

      if (words.Count > 0) {
        Console.WriteLine("STDOUT: " + e.Data);
        Console.WriteLine("DAWG suggestions:");
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
      using (FileStream stream = File.OpenRead(file)) {
        dawg.LoadFromStream(stream);
      }

      loaded++;

      Console.WriteLine($"Loaded {loaded}: {Path.GetFileName(file)}");
    }

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
