using LLMGameCreator.Application.Design.FullGeneratorVariabilityRegressionMatrix;

namespace LLMGameCreator.Tests.Application.FullGeneratorVariabilityRegressionMatrix;

public static class FullGeneratorVariabilityRegressionMatrixTestFactory
{
    public static FullGeneratorVariabilityMatrixEvidenceService CreateService() => new();

    public static FullGeneratorVariabilityEvidenceResult BuildFromRepo() =>
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
