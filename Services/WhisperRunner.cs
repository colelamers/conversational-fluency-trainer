using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
namespace conversational_fluency_trainer.Services;

public class WhisperRunner {
    Process process_;

    string whisper_path_ = Path.Combine(DevPaths.ProjectRoot, "dependencies/whisper-stream");
    string model_path_ = Path.Combine(DevPaths.ProjectRoot, "dependencies/ggml-large-v3-turbo-q5_0.bin");

    public async Task RunAsync() {
        ProcessStartInfo psi = new () {
            FileName = whisper_path_,
            Arguments = $"-m \"{model_path_}\" -t 8 -l de -c 1",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(whisper_path_)
        };

        process_ = new Process();
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
