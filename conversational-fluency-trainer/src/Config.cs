namespace conversational_fluency_trainer.config_models;

public class AppConfig {
  public WhisperConfig Whisper { get; set; } = new();
  public DawgConfig Dawg { get; set; } = new();
}

public class WhisperConfig {
  public WhisperPaths FileNames { get; set; } = new();

  public WhisperArguments Arguments { get; set; } = new();
  public WhisperProcess Process { get; set; } = new();

  public HashSet<string> Filters { get; set; } = new();
}

public class WhisperPaths {
  public string Executable { get; set; } = "";
  public string Model { get; set; } = "";
}

public class WhisperArguments {
  public int Threads { get; set; }
  public string Language { get; set; } = "";
  public int Processors { get; set; }
}


public class WhisperProcess {
  public bool RedirectOutput { get; set; }
  public bool RedirectError { get; set; }
  public bool RedirectInput { get; set; }

  public bool UseShellExecute { get; set; }
  public bool CreateNoWindow { get; set; }
}


public class DawgConfig {
  public string TrainingDirectory { get; set; } = "";
}
