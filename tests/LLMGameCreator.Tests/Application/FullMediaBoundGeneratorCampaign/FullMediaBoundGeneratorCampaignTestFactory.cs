using LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;

namespace LLMGameCreator.Tests.Application.FullMediaBoundGeneratorCampaign;

public static class FullMediaBoundGeneratorCampaignTestFactory
{
    public static FullMediaBoundGeneratorCampaignEvidenceService CreateService() => new();

    public static FullMediaBoundCampaignEvidenceResult BuildFromRepo() =>
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
