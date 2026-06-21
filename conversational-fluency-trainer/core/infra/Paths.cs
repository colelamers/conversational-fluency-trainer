using System.Reflection;
namespace core.infra;
public static class 
Paths {
  // --------------------------------------------------------------------
  // EXECUTABLE
  // --------------------------------------------------------------------

  // Linux:
  //   /home/soren/dev/MyApp/bin/Debug/net9.0/MyApp
  // Windows:
  //   C:\Dev\MyApp\bin\Debug\net9.0\MyApp.exe
  public static string 
  ExecutablePath {
    get {
      // todo 2; I'm torn on having the null check here
      if (Environment.ProcessPath == null) {
        return "";
      }

      return Environment.ProcessPath;
    }
  }

  // Linux:
  //   /home/soren/dev/MyApp/bin/Debug/net9.0/
  // Windows:
  //   C:\Dev\MyApp\bin\Debug\net9.0\
  public static string 
  ExecutableDirectory {
    get {
      return AppContext.BaseDirectory;
    }
  }

  // Linux:
  //   MyApp
  // Windows:
  //   MyApp.exe
  public static string 
  ExecutableFileName {
    get {
      return Path.GetFileName(ExecutablePath);
    }
  }

  // Linux:
  //   MyApp
  // Windows:
  //   MyApp
  public static string 
  ExecutableFileNameWithoutExtension {
    get {
      return Path.GetFileNameWithoutExtension(ExecutablePath);
    }
  }

  // --------------------------------------------------------------------
  // PROCESS
  // --------------------------------------------------------------------

  // Linux:
  //   /tmp (or wherever launched from)
  // Windows:
  //   C:\Temp
  public static string 
  WorkingDirectory {
    get {
      return Environment.CurrentDirectory;
    }
  }

  // Linux:
  //   /home/soren/dev/MyApp/bin/Debug/net9.0/
  // Windows:
  //   C:\Dev\MyApp\bin\Debug\net9.0\
  public static string 
  StartupDirectory {
    get {
      return AppDomain.CurrentDomain.BaseDirectory;
    }
  }

  // Linux:
  //   ./MyApp --debug
  // Windows:
  //   MyApp.exe --debug
  public static string 
  CommandLine {
    get {
      return Environment.CommandLine;
    }
  }

  // --------------------------------------------------------------------
  // ASSEMBLY
  // --------------------------------------------------------------------

  // Linux:
  //   /home/soren/dev/MyApp/bin/Debug/net9.0/MyApp.dll
  // Windows:
  //   C:\Dev\MyApp\bin\Debug\net9.0\MyApp.dll
  public static string 
  AssemblyLocation {
    get {
      Assembly? assembly = Assembly.GetEntryAssembly();
      if (assembly == null) {
        return "";
      }

      return assembly.Location;
    }
  }

  // Linux:
  //   /home/soren/dev/MyApp/bin/Debug/net9.0/
  // Windows:
  //   C:\Dev\MyApp\bin\Debug\net9.0\
  public static string?
  AssemblyDirectory {
    get {
      return Path.GetDirectoryName(AssemblyLocation);
    }
  }

  // --------------------------------------------------------------------
  // USER
  // --------------------------------------------------------------------

  // Linux:
  //   /home/soren
  // Windows:
  //   C:\Users\Soren
  public static string 
  UserProfile {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
  }

  // Linux:
  //   /home/soren
  // Windows:
  //   C:\Users\Soren
  public static string 
  Home {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
  }

  // Linux:
  //   /home/soren/Desktop
  // Windows:
  //   C:\Users\Soren\Desktop
  public static string 
  Desktop {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }
  }

  // Linux:
  //   /home/soren/Documents
  // Windows:
  //   C:\Users\Soren\Documents
  public static string 
  Documents {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
  }

  // Linux:
  //   /home/soren/Downloads
  // Windows:
  //   C:\Users\Soren\Downloads
  public static string 
  Downloads {
    get {
      return Path.Combine(UserProfile, "Downloads");
    }
  }

  // Linux:
  //   /home/soren/Pictures
  // Windows:
  //   C:\Users\Soren\Pictures
  public static string 
  Pictures {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    }
  }

  // Linux:
  //   /home/soren/Music
  // Windows:
  //   C:\Users\Soren\Music
  public static string 
  Music {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
    }
  }

  // Linux:
  //   /home/soren/Videos
  // Windows:
  //   C:\Users\Soren\Videos
  public static string 
  Videos {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
    }
  }

  // --------------------------------------------------------------------
  // APP DATA
  // --------------------------------------------------------------------

  // Linux:
  //   /home/soren/.config
  // Windows:
  //   C:\Users\Soren\AppData\Roaming
  public static string 
  RoamingAppData {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }
  }

  // Linux:
  //   /home/soren/.local/share
  // Windows:
  //   C:\Users\Soren\AppData\Local
  public static string 
  LocalAppData {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
  }

  // Linux:
  //   /usr/share
  // Windows:
  //   C:\ProgramData
  public static string 
  CommonApplicationData {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    }
  }

  // --------------------------------------------------------------------
  // TEMP
  // --------------------------------------------------------------------

  // Linux:
  //   /tmp
  // Windows:
  //   C:\Users\Soren\AppData\Local\Temp
  public static string 
  Temp {
    get {
      return Path.GetTempPath();
    }
  }

  // --------------------------------------------------------------------
  // SYSTEM
  // --------------------------------------------------------------------

  // Linux:
  //   /usr/share/dotnet/shared/Microsoft.NETCore.App/9.0.x/
  // Windows:
  //   C:\Program Files\dotnet\shared\Microsoft.NETCore.App\9.0.x\
  public static string 
  RuntimeDirectory {
    get {
      return System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
    }
  }

  // Linux:
  //   /
  // Windows:
  //   C:\Windows\System32
  public static string 
  SystemDirectory {
    get {
      return Environment.SystemDirectory;
    }
  }

  // Linux:
  //   (not applicable)
  // Windows:
  //   C:\Windows
  public static string 
  WindowsDirectory {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.Windows);
    }
  }

  // --------------------------------------------------------------------
  // MACHINE
  // --------------------------------------------------------------------

  // Linux:
  //   kierkegaard
  // Windows:
  //   SOREN-PC
  public static string 
  MachineName {
    get {
      return Environment.MachineName;
    }
  }

  // Linux:
  //   soren
  // Windows:
  //   Soren
  public static string 
  UserName {
    get {
      return Environment.UserName;
    }
  }

  // --------------------------------------------------------------------
  // PROJECT ROOT
  // --------------------------------------------------------------------

  // Linux:
  //   /home/soren/dev/MyApp/src/MyApp
  // Windows:
  //   C:\Dev\MyApp\src\MyApp
  public static string 
  ProjectRoot {
    get {
      DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
      while (dir != null) {
        if (dir.GetFiles("*.csproj").Length > 0) {
          return dir.FullName;
        }

        dir = dir.Parent;
      }

      return "";
    }
  }

  public static string 
  ConfsDirectory {
    get {
      string root = core.infra.Paths.SolutionRoot;
      if (String.IsNullOrWhiteSpace(root)) {
        return "";
      }

      // Points to: ~/apps/conversation-fluency-trainer/confs
      return Path.Combine(root, "confs");
    }
  }

  public static string 
  LogsDirectory {
    get {
      string root = core.infra.Paths.SolutionRoot;
      if (String.IsNullOrWhiteSpace(root)) {
        return "";
      }

      // Points to: ~/apps/conversation-fluency-trainer/logs
      string path = Path.Combine(root, "logs");
      Directory.CreateDirectory(path);
      return path;
    }
  }

  public static string
  DepsDirectory {
    get {
      string root = core.infra.Paths.SolutionRoot;
      if (String.IsNullOrWhiteSpace(root)) {
        return "";
      }

      // Points to: ~/apps/conversation-fluency-trainer/deps
      return Path.Combine(root, "deps");
    }
  }

  // Linux:
  //   /home/soren/dev/MyApp/src/MyApp/MyApp.csproj
  // Windows:
  //   C:\Dev\MyApp\src\MyApp\MyApp.csproj
  public static string 
  ProjectFile {
    get {
      string root = core.infra.Paths.ProjectRoot;
      if (String.IsNullOrWhiteSpace(root)) {
        return "";
      }

      FileInfo? file = first_or_default(new DirectoryInfo(root).GetFiles("*.csproj"), ".csproj");
      if (file == null) {
        return "";
      }

      return file.FullName;
    }
  }

  // --------------------------------------------------------------------
  // SOLUTION ROOT
  // --------------------------------------------------------------------

  // Linux:
  //   /home/soren/dev/MyApp
  // Windows:
  //   C:\Dev\MyApp
  public static string 
  SolutionRoot {
    get {
      DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
      while (dir != null) {
        // Walks up until it finds your conversational-fluency-trainer.slnx
        if (dir.GetFiles("*.slnx").Length > 0 || dir.GetFiles("*.sln").Length > 0) {
          return dir.FullName;
        }

        dir = dir.Parent;
      }

      return "";
    }
  }

  // Linux:
  //   /home/soren/dev/MyApp/MyApp.sln
  // Windows:
  //   C:\Dev\MyApp\MyApp.sln
  public static string 
    SolutionFile {
      get {
        string root = core.infra.Paths.SolutionRoot;
        if (String.IsNullOrWhiteSpace(root)) {
          return "";
        }

        DirectoryInfo? dir_info = new DirectoryInfo(root);
        FileInfo? file = first_or_default(dir_info.GetFiles("*.slnx"), ".slnx");
        
        if (file == null) {
          file = first_or_default(dir_info.GetFiles("*.sln"), ".sln");
        }

        if (file == null) {
          return "";
        }

        return file.FullName;
      }
    }

  // --------------------------------------------------------------------
  // REPOSITORY ROOT
  // --------------------------------------------------------------------

  // Linux:
  //   /home/soren/dev/MyApp
  // Windows:
  //   C:\Dev\MyApp
  // (contains .git folder)
  public static string 
  RepositoryRoot {
    get {
      DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
      while (dir != null) {
        if (Directory.Exists(Path.Combine(dir.FullName, ".git"))) {
          return dir.FullName;
        }

        dir = dir.Parent;
      }

      return "";
    }
  }

  // --------------------------------------------------------------------
  // CONFIG
  // --------------------------------------------------------------------

  // Linux:
  //   /home/soren/.config/MyApp
  // Windows:
  //   C:\Users\Soren\AppData\Roaming\MyApp
  public static string 
  ConfigDirectory {
    get {
      string path = Path.Combine(core.infra.Paths.RoamingAppData, ExecutableFileNameWithoutExtension);
      Directory.CreateDirectory(path);
      return path;
    }
  }

  // Linux:
  //   /home/soren/.config/MyApp/config.json
  // Windows:
  //   C:\Users\Soren\AppData\Roaming\MyApp\config.json
  public static string 
  ConfigFile {
    get {
      return Path.Combine(core.infra.Paths.ConfigDirectory, "config.json");
    }
  }

  // --------------------------------------------------------------------
  // DATA
  // --------------------------------------------------------------------

  // Linux:
  //   /home/soren/.local/share/MyApp
  // Windows:
  //   C:\Users\Soren\AppData\Local\MyApp
  public static string 
  DataDirectory {
    get {
      string path = Path.Combine(core.infra.Paths.LocalAppData, ExecutableFileNameWithoutExtension);
      Directory.CreateDirectory(path);
      return path;
    }
  }

  // Linux:
  //   /home/soren/.local/share/MyApp/data.db
  // Windows:
  //   C:\Users\Soren\AppData\Local\MyApp\data.db
  public static string 
  DatabaseFile {
    get {
      return Path.Combine(core.infra.Paths.DataDirectory, "data.db");
    }
  }

  // --------------------------------------------------------------------
  // LOGGING
  // --------------------------------------------------------------------

  // Linux:
  //   /home/soren/.local/share/MyApp/Logs
  // Windows:
  //   C:\Users\Soren\AppData\Local\MyApp\Logs
  public static string 
  LogsDirectoryLocal {
    get {
      string path = Path.Combine(core.infra.Paths.DataDirectory, "Logs");
      Directory.CreateDirectory(path);
      return path;
    }
  }

  // Linux:
  //   /home/soren/.local/share/MyApp/Logs/application.log
  // Windows:
  //   C:\Users\Soren\AppData\Local\MyApp\Logs\application.log
  public static string 
  LogFile {
    get {
      return Path.Combine(core.infra.Paths.LogsDirectory, "application.log");
    }
  }

  // Linux:
  //   /home/soren/.local/share/MyApp/Logs/application.log
  // Windows:
  //   C:\Users\Soren\AppData\Local\MyApp\Logs\application.log
  public static string 
  LogFileLocal {
    get {
      return Path.Combine(core.infra.Paths.LogsDirectoryLocal, "application.log");
    }
  }


  public static string 
  SystemTemp {
    get {
      return Path.GetTempPath();
    }
  }

  // Linux: /tmp (explicit)
  public static string 
  LinuxTmp {
    get {
      return "/tmp";
    }
  }

  // Linux: /var/tmp (persistent temp)
  public static string 
  LinuxVarTmp {
    get {
      return "/var/tmp";
    }
  }

  // Windows: %TEMP% resolved explicitly
  public static string 
  WindowsTemp {
    get {
      return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        + Path.DirectorySeparatorChar + "Temp";
    }
  }

  // Session temp (Linux XDG runtime dir)
  public static string 
  LinuxRuntimeTemp {
    get {
      string? runtime = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
      if (!String.IsNullOrWhiteSpace(runtime)) {
        return runtime;
      }

      return "/tmp";
    }
  }

  // Generic “best available temp”
  public static string 
  BestTemp {
    get {
      string path = Path.GetTempPath();
      if (!Directory.Exists(path)) {
        return "/tmp";
      }

      return path;
    }
  }

  private static FileInfo? 
  first_or_default(IEnumerable<FileInfo> files, string extension) {
  foreach (FileInfo file in files) {
    if (file.Extension == extension) {
      return file;
    }
  }

  return null;
}
}
