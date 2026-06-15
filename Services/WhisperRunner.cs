using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class WhisperRunner
{
    private Process _process;

    string whisperPath = Path.Combine(DevPaths.ProjectRoot, "dependencies/whisper-stream");
    string modelPath = Path.Combine(DevPaths.ProjectRoot, "dependencies/ggml-large-v3-turbo-q5_0.bin");

    public async Task RunAsync()
    {
        var psi = new ProcessStartInfo
        {
            FileName = whisperPath,
            Arguments = $"-m \"{modelPath}\" -t 8 -l de -c 1",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(whisperPath)
        };

        _process = new Process();
        _process.StartInfo = psi;
        _process.OutputDataReceived += OnOutputDataReceived;
        _process.ErrorDataReceived += OnErrorDataReceived;

        _process.Start();

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        await _process.WaitForExitAsync();
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!String.IsNullOrWhiteSpace(e.Data))
        {
            Console.WriteLine("STDOUT: " + e.Data);
        }
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!String.IsNullOrWhiteSpace(e.Data))
        {
            Console.WriteLine("STDERR: " + e.Data);
        }
    }
}
