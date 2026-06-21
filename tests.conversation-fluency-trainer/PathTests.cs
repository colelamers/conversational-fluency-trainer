using core.infra;

namespace tests.conversation_fluency_trainer;

[TestClass]
public class PathsTests
{
    // MSTest automatically injects TestContext if a public property exists.
    // This makes TestContext.WriteLine display natively in VS Code / VS test logs.
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void VerifyDependenciesDirectoryExistsAndIsNamedCorrectly()
    {
        // Act
        string deps_path = Paths.DepsDirectory;

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(deps_path), "DepsDirectory resolved to an empty string.");
        Assert.IsTrue(Directory.Exists(deps_path), $"The dependencies folder path was expected but not found at: {deps_path}");

        // Confirm name explicitly (paranoid check)
        DirectoryInfo dir_info = new(deps_path);
        Assert.AreEqual("deps", dir_info.Name);
    }

    [TestMethod]
    public void ExecutablePaths_ShouldReturnValidPopulatedStrings()
    {
        // Act & Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.ExecutablePath));
        Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.ExecutableDirectory));
        Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.ExecutableFileName));
        Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.ExecutableFileNameWithoutExtension));

        // When running tests, the executable is usually the dotnet test host process
        Assert.IsTrue(Directory.Exists(Paths.ExecutableDirectory), $"Dir missing: {Paths.ExecutableDirectory}");
    }

    [TestMethod]
    public void WorkingAndStartupDirectories_ShouldExistOnDisk()
    {
        Assert.IsTrue(Directory.Exists(Paths.WorkingDirectory));
        Assert.IsTrue(Directory.Exists(Paths.StartupDirectory));
        Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.CommandLine));
    }

    [TestMethod]
    public void UserProfileAndHome_ShouldMatchAndExist()
    {
        Assert.IsTrue(Directory.Exists(Paths.UserProfile));
        Assert.IsTrue(Directory.Exists(Paths.Home));
        Assert.AreEqual(Paths.UserProfile, Paths.Home);
    }

  [TestMethod]
  public void UserSpecialFolders_ShouldBeValidOrPlatformConditional()
  {
      // Desktop, Documents, and Downloads should resolve to real locations on dev boxes
      Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.Desktop));
      Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.Documents));
      Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.Downloads));
      Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.Pictures));
      
      TestContext.WriteLine($"SystemDirectory: '{Paths.SystemDirectory}'");
      
      // Windows-specific path checks
      if (OperatingSystem.IsWindows())
      {
          // System directory is guaranteed to always exist on Windows
          Assert.IsTrue(Directory.Exists(Paths.SystemDirectory), "SystemDirectory should exist on Windows.");
          Assert.IsTrue(Directory.Exists(Paths.WindowsDirectory));
          Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.WindowsTemp));
      }
      else
      {
          // On Linux/macOS, verify that it gracefully evaluates to an empty string
          Assert.IsTrue(string.IsNullOrEmpty(Paths.SystemDirectory), "SystemDirectory should be empty on non-Windows platforms.");
      }
  }

    [TestMethod]
    public void SolutionRootAndFile_ShouldResolveCorrectly()
    {
        // Act
        string solution_root = Paths.SolutionRoot;
        string solution_file = Paths.SolutionFile;
        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(solution_root), "Solution root failed to resolve.");
        Assert.IsTrue(Directory.Exists(solution_root), $"Solution root directory doesn't exist: {solution_root}");
        
        Assert.IsFalse(string.IsNullOrWhiteSpace(solution_file), "Solution file target returned empty string.");
        Assert.IsTrue(File.Exists(solution_file), $"Target solution file was not found on disk: {solution_file}");

        // Ensure it found your custom modern solution file format
        bool is_valid_extension = solution_file.EndsWith(".slnx") || solution_file.EndsWith(".sln");
        Assert.IsTrue(is_valid_extension, "File extension must be .slnx or .sln");
    }

    [TestMethod]
    public void SolutionLevelAssets_ShouldExistAndBeNamedCorrectly()
    {
        // Act
        string deps = Paths.DepsDirectory;
        string confs = Paths.ConfsDirectory;
        string logs = Paths.LogsDirectory;

        // Assert they are accurately resolved roots
        Assert.IsTrue(Directory.Exists(deps), $"Deps directory missing at: {deps}");
        Assert.IsTrue(Directory.Exists(confs), $"Confs directory missing at: {confs}");
        Assert.IsTrue(Directory.Exists(logs), $"Logs directory missing at: {logs}");

        // End-folder identity validation
        Assert.AreEqual("deps", Path.GetFileName(deps.TrimEnd(Path.DirectorySeparatorChar)));
        Assert.AreEqual("confs", Path.GetFileName(confs.TrimEnd(Path.DirectorySeparatorChar)));
        Assert.AreEqual("logs", Path.GetFileName(logs.TrimEnd(Path.DirectorySeparatorChar)));
    }

    [TestMethod]
    public void MachineAndUserIdentity_ShouldBePopulated()
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.MachineName));
        Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.UserName));
    }

    [TestMethod]
    public void TempPaths_ShouldReturnValidWriteablePaths()
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.Temp));
        Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.BestTemp));
        Assert.IsTrue(Directory.Exists(Paths.BestTemp));

        if (OperatingSystem.IsLinux())
        {
            Assert.AreEqual("/tmp", Paths.LinuxTmp);
            Assert.AreEqual("/var/tmp", Paths.LinuxVarTmp);
            Assert.IsFalse(string.IsNullOrWhiteSpace(Paths.LinuxRuntimeTemp));
        }
    }
}
