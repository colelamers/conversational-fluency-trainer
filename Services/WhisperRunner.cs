using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class WhisperRunner {
    Process process_;

    string whisperPath = Path.Combine(DevPaths.ProjectRoot, "dependencies/whisper-stream");
    string modelPath = Path.Combine(DevPaths.ProjectRoot, "dependencies/ggml-large-v3-turbo-q5_0.bin");

    public async Task RunAsync() {
        var psi = new ProcessStartInfo {
            FileName = whisperPath,
            Arguments = $"-m \"{modelPath}\" -t 8 -l de -c 1",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(whisperPath)
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
