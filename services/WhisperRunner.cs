using System.Diagnostics;
namespace conversational_fluency_trainer.services;

public class WhisperRunner {
    Process process_ = new Process();
    readonly static string WHISPER_PATH = Path.Combine(
        core.infra.Paths.ProjectRoot, "dependencies/whisper-stream");
    readonly static string MODEL_PATH = Path.Combine(
        core.infra.Paths.ProjectRoot, "dependencies/ggml-large-v3-turbo-q5_0.bin");

    public async Task RunAsync() {
        ProcessStartInfo psi = new () {
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
        process_.OutputDataReceived += OnOutputDataReceived;
        process_.ErrorDataReceived += OnErrorDataReceived;

        process_.Start();

        process_.BeginOutputReadLine();
        process_.BeginErrorReadLine();

        await process_.WaitForExitAsync();
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e) {
        if (!string.IsNullOrWhiteSpace(e.Data)) {
            Console.WriteLine("STDOUT: " + e.Data);
        }
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e) {
        if (!string.IsNullOrWhiteSpace(e.Data)) {
            Console.WriteLine("STDERR: " + e.Data);
        }
    }
}
