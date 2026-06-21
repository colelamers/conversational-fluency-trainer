using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using core.infra;

namespace tests.conversation_fluency_trainer;

[TestClass]
public class ProjectTests
{
    [TestMethod]
    public void CriticalModelAsset_ShouldBePresentInDeps()
    {
        // Act
        string deps_path = Paths.DepsDirectory;
        string model_file = Path.Combine(deps_path, "ggml-large-v3-turbo-q5_0.bin");

        // Assert that critical large assets are present inside it
        Assert.IsTrue(File.Exists(model_file), $"Expected model file is missing from deps directory: {model_file}");
    }
}
