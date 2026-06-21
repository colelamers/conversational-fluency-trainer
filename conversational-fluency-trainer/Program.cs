using conversational_fluency_trainer.services;

namespace conversational_fluency_trainer;

public static class Program {
    public static async Task Main(string[] args) {
        WhisperRunner wr = new WhisperRunner();
        await wr.RunAsync();
        Console.WriteLine("Started");
    }
}
