using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.WinForms.Pages.StrictLlmArtifacts;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class ContentLanguagePolicySmokeTests
{
    [Fact]
    public async Task ContentLanguagePolicyProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectFolder = ResolveProjectFolder(temp.Path);
        var policyService = new ContentLanguagePolicyService();
        var saved = await policyService.SaveAsync(projectFolder, ContentLanguagePolicy.CreateDefault());
        var loaded = await policyService.LoadAsync(projectFolder);
        var request = new StrictLlmArtifactsPresenter().BuildRequest(new StrictLlmArtifactsViewState
        {
            SelectedProfileId = "unused-no-provider",
            SelectedContractIds = ["quest_pack_v1"],
            SelectedContentLanguage = loaded.Policy.ContentLanguage
        });
        var catalog = new GeneratorPlanStrictLlmArtifactContractCatalog();
        Assert.True(catalog.TryGet("quest_pack_v1", out var contract));
        var prompt = new GeneratorPlanStrictLlmArtifactPromptBuilder().Build(
            contract,
            new GeneratorPlanCapabilitySelection { SelectionId = "selection/content-language-smoke" },
            request);
        var diagnostics = new ContentLanguageDiagnosticService().Inspect(
            """{"quests":[{"id":"quest/market-watch","title":"Watch the Busy Market","objectives":["Talk to every market guard"]}]}""",
            request.ContentLanguage);

        Assert.True(saved.IsProjectPersisted);
        Assert.Equal(ContentLanguageCodes.Russian, request.ContentLanguage);
        Assert.Contains("player-facing game content in Russian", prompt.CombinedText);
        Assert.Contains("Keep technical ids in ASCII/kebab_case", prompt.CombinedText);
        Assert.Equal(ContentLanguagePolicy.AsciiKebabCaseTechnicalIdPolicy, loaded.Policy.TechnicalIdPolicy);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == ContentLanguageDiagnosticService.ObviousEnglishProseWarning);
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
