using LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;

namespace LLMGameCreator.Tests.Application.MediaMaterializationReviewPackage;

public static class MediaMaterializationReviewPackageTestFactory
{
    public static MediaMaterializationReviewPackageEvidenceService CreateService() => new();

    public static MediaMaterializationReviewPackageEvidenceResult BuildFromRepo() =>
        CreateService().Build(FindRepoRoot());

    public static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}

public sealed class TempMediaMaterializationProject : IDisposable
{
    public TempMediaMaterializationProject()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
