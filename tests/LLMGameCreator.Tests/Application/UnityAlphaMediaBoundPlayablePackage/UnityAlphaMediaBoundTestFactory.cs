using LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;

namespace LLMGameCreator.Tests.Application.UnityAlphaMediaBoundPlayablePackage;

public static class UnityAlphaMediaBoundTestFactory
{
    public static UnityAlphaMediaBoundPlayablePackageEvidenceService CreateService() => new();

    public static UnityAlphaMediaBoundEvidenceResult BuildFromRepo() =>
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
