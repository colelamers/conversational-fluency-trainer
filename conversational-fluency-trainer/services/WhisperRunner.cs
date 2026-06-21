using System.Diagnostics;
namespace conversational_fluency_trainer.services;

public class 
WhisperRunner {
  public async Task 
  RunAsync() {
    ProcessStartInfo psi = new() {
      FileName = WHISPER_PATH,
      Arguments = $"-m \"{MODEL_PATH}\" -t 8 -l de -c 1",
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      RedirectStandardInput = false,
      UseShellExecute = false,
      CreateNoWindow = true,
      WorkingDirectory = Path.GetDirectoryName(WHISPER_PATH)
    };

    process_.StartInfo = psi;
    process_.OutputDataReceived += on_output_data_received;
    process_.ErrorDataReceived += on_error_data_received;

    process_.Start();

    process_.BeginOutputReadLine();
    process_.BeginErrorReadLine();

    await process_.WaitForExitAsync();
  }

  private void 
  on_output_data_received(object sender, DataReceivedEventArgs e) {
    if (!string.IsNullOrWhiteSpace(e.Data)) {
      Console.WriteLine("STDOUT: " + e.Data);
    }
  }

  private void 
  on_error_data_received(object sender, DataReceivedEventArgs e) {
    if (!string.IsNullOrWhiteSpace(e.Data)) {
      Console.WriteLine("STDERR: " + e.Data);
    }
  }

  private Process process_ = new Process();
  // todo 3; these are gonna need a unit test
  // todo 2; make global file of consts so unit tests are consistent and paths
  //         stable/findable
  private readonly static string WHISPER_PATH = Path.Combine(
    core.infra.Paths.DepsDirectory, "whisper-stream");
  private readonly static string MODEL_PATH = Path.Combine(
    core.infra.Paths.DepsDirectory, "ggml-large-v3-turbo-q5_0.bin");

  private readonly static HashSet<string> WHISPER_FILL_INS_TO_SKIP = new HashSet<string>
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
}
