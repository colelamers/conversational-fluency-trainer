public class AppConfigModel {
  public string? AppName { get; set; }
  public string? LogLevel { get; set; }
  public int MaxThreads { get; set; }
  public AudioConfig? Audio { get; set; }
}

public class AudioConfig {
  public int DeviceIndex { get; set; }
  public int SampleRate { get; set; }
  public ComplexThingConfig? ComplexThing { get; set; }
}

public class ComplexThingConfig {
  public AConfig? A { get; set; }
}

public class AConfig {
  public int B { get; set; }
}


/*
// example.json
{
  "appName": "MyApp",
  "logLevel": "Debug",
  "maxThreads": 6,
  "audio": {
  "deviceIndex": 2,
  "sampleRate": 16000,
  "complex-thing": {
    "a" : {
      "b" : 3
    }
  }
  }
}


Example code
string path = DevPaths.ProjectRoot + "/confs/example.json";

JsonSerialization config = new JsonSerialization(path);

// typed leaf access (this is your ONLY “get”)
int b = config.GetValue<int>("audio.complex-thing.a.b");
int sampleRate = config.GetValue<int>("audio.sampleRate");
string appName = config.GetValue<string>("appName");

// full model binding (IMPORTANT: not a method called Get<T>())
AppConfigModel appConfig = JsonSerializer.Deserialize<AppConfigModel>(
  config.GetRoot(),
  new JsonSerializerOptions
  {
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
  }
);

Console.WriteLine(appConfig.AppName);
Console.WriteLine(appConfig.Audio.SampleRate);
Console.WriteLine(appConfig.Audio.ComplexThing.A.B);
*/
