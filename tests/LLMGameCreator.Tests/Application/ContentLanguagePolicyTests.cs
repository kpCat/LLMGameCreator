using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.WinForms.Pages.StrictLlmArtifacts;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class ContentLanguagePolicyTests
{
    [Fact]
    public void DefaultPolicySupportsRequiredLanguagesAndRussianPromptInstruction()
    {
        var policy = ContentLanguagePolicy.CreateDefault();
        var provider = new ContentLanguagePromptInstructionProvider();

        Assert.Equal(ContentLanguageCodes.Russian, policy.ContentLanguage);
        Assert.Equal(["ru", "uk", "en"], ContentLanguageCodes.Supported);
        Assert.Equal(ContentLanguagePolicy.AsciiKebabCaseTechnicalIdPolicy, policy.TechnicalIdPolicy);
        var instruction = provider.GetInstruction(policy.ContentLanguage);
        Assert.Contains("player-facing game content in Russian", instruction);
        Assert.Contains("ASCII/kebab_case", instruction);
        Assert.Contains("Do not translate ids", instruction);
    }

    [Fact]
    public void PresenterRequestAndPromptUseSelectedContentLanguage()
    {
        var state = new StrictLlmArtifactsViewState
        {
            SelectedProfileId = "local",
            SelectedContractIds = ["game_profile_v1"],
            SelectedContentLanguage = ContentLanguageCodes.Ukrainian
        };
        var request = new StrictLlmArtifactsPresenter().BuildRequest(state);
        var catalog = new GeneratorPlanStrictLlmArtifactContractCatalog();
        Assert.True(catalog.TryGet("game_profile_v1", out var contract));

        var prompt = new GeneratorPlanStrictLlmArtifactPromptBuilder().Build(
            contract,
            new GeneratorPlanCapabilitySelection { SelectionId = "selection/content-language" },
            request);

        Assert.Equal(ContentLanguageCodes.Ukrainian, request.ContentLanguage);
        Assert.Contains("player-facing game content in Ukrainian", prompt.CombinedText);
        Assert.Contains("Keep technical ids in ASCII/kebab_case", prompt.CombinedText);
    }

    [Fact]
    public void DiagnosticsWarnForEnglishPlayerTextButIgnoreTechnicalIds()
    {
        var service = new ContentLanguageDiagnosticService();
        var englishText = """
        {
          "artifact_kind": "quest_pack_v1",
          "quests": [{
            "id": "quest/find-lost-medicine",
            "title": "Find the Lost Medicine",
            "steps": ["Search the abandoned clinic", "Return to the village healer"]
          }]
        }
        """;
        var idsOnly = """
        {
          "artifact_kind": "quest_pack_v1",
          "quest_id": "quest/find-lost-medicine",
          "scene_ids": ["scene/abandoned-clinic"]
        }
        """;

        Assert.Contains(service.Inspect(englishText, ContentLanguageCodes.Russian),
            diagnostic => diagnostic.Code == ContentLanguageDiagnosticService.ObviousEnglishProseWarning);
        Assert.Empty(service.Inspect(idsOnly, ContentLanguageCodes.Russian));
    }

    [Fact]
    public async Task ProjectPolicyRoundTripsThroughProjectSettingsFolder()
    {
        using var temp = new TempDirectory();
        var service = new ContentLanguagePolicyService();
        var saved = await service.SaveAsync(temp.Path, new ContentLanguagePolicy
        {
            ContentLanguage = ContentLanguageCodes.Ukrainian
        });
        var loaded = await service.LoadAsync(temp.Path);

        Assert.True(saved.IsProjectPersisted);
        Assert.True(File.Exists(Path.Combine(temp.Path, ".llmgc", "settings", "content-language-policy.json")));
        Assert.True(loaded.IsProjectPersisted);
        Assert.Equal(ContentLanguageCodes.Ukrainian, loaded.Policy.ContentLanguage);
        Assert.Equal(ContentLanguagePolicy.AsciiKebabCaseTechnicalIdPolicy, loaded.Policy.TechnicalIdPolicy);
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
