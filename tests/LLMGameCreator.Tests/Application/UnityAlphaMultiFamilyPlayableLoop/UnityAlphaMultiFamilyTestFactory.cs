using LLMGameCreator.Application.Design.UnityAlphaMultiFamilyPlayableLoop;

namespace LLMGameCreator.Tests.Application.UnityAlphaMultiFamilyPlayableLoop;

public static class UnityAlphaMultiFamilyTestFactory
{
    public static UnityAlphaMultiFamilyPlayableLoopEvidenceService CreateService() => new();

    public static UnityAlphaMultiFamilyEvidenceResult BuildFromRepo() =>
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
