
namespace conversational_fluency_trainer;

public static class Program {
    public static async Task Main(string[] args) {
        WhisperRunner wr = new WhisperRunner();
        await wr.RunAsync();
        Console.WriteLine("Started");
        // todo 1; we need to add a front end in somewhat besides what i have
        // currently this would have to be server side only or a full on
        // application
        // todo 1; need a loader for this. take many/large datasets, load into 
        // the dawg, and go forth
    }
}
