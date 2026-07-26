using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal161;
using LLMGameCreator.Tests.Application.Goal162;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal167;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal167HistoryRegenerationTests
{
    [Fact]
    public void Behavioral_v7_primary_truth_belongs_to_regional_event_route_while_earlier_summaries_stay_exact()
    {
        var build = Goal164TestKit.AllSelectable.Build;

        Assert.Equal(GameProjectBuildHistoryReader.SchemaVersionV7,
            ReadHistory(build.BuildHistoryPath).SchemaVersion);
        Assert.Equal(build.FinalStateHash,
            build.GeneratedCampaignRegionalEvents?.FinalStateHash);
        Assert.NotEqual(build.FinalStateHash,
            build.GeneratedCampaignRelationships?.FinalStateHash);
        Assert.NotEqual(build.FinalStateHash,
            build.GeneratedCampaignChoices?.FinalStateHash);
        Assert.NotEqual(build.FinalStateHash, build.GeneratedEncounterCombat?.FinalStateHash);
        Assert.Equal(build.PackageSha256, build.GeneratedEncounterCombat?.ExactPackageSha256);
    }

    [Fact]
    public void Behavioral_regeneration_seal_contains_choice_summary_overlay_and_flag_inventory()
    {
        var seal = Goal164RegenerationState.Value.Seal;

        Assert.Equal(64, seal.GeneratedCampaignChoiceSummarySha256.Length);
        Assert.Equal(64, seal.GeneratedCampaignChoiceOverlaySha256.Length);
        Assert.Equal(64, seal.GeneratedCampaignChoiceFlagInventorySha256.Length);
    }

    [Fact]
    public void Behavioral_regeneration_and_rollback_both_restore_choice_current()
    {
        var state = Goal164RegenerationState.Value;

        Assert.True(state.Applied.Applied, string.Join(",", state.Applied.Diagnostics));
        Assert.Equal("CHOICE_CURRENT", state.AfterRegeneration.GeneratedCampaignChoices?.Status);
        Assert.True(state.RolledBack.Applied, string.Join(",", state.RolledBack.Diagnostics));
        Assert.Equal("CHOICE_CURRENT", state.AfterRollback.GeneratedCampaignChoices?.Status);
    }

    [Fact]
    public void Behavioral_genuine_v4_projects_choices_pending_and_campaign_not_ready()
    {
        var state = Goal167V4State.Value;

        Assert.Equal("CAMPAIGN_CURRENT", state.Snapshot.GeneratedEncounterCombat?.Status);
        Assert.Equal("CHOICES_PENDING", state.Snapshot.GeneratedCampaignChoices?.Status);
        Assert.Equal(GeneratedCampaignSessionStatus.PROJECT_NOT_READY, state.Capture.Status);
        Assert.Contains("campaign.generated_choices_not_current", state.Capture.Diagnostics);
    }

    [Fact]
    public void Behavioral_genuine_v4_projects_collect_and_play_action()
    {
        var presentation = Goal162ProjectsTestKit.Presentation(Goal167V4State.Value.Snapshot);

        Assert.True(presentation.Enabled);
        Assert.False(presentation.Current);
        Assert.Equal("Собрать и играть", presentation.Title);
    }

    [Fact]
    public void Behavioral_zero_encounter_profile_qualifies_support_and_refuse_without_challenge()
    {
        var state = Goal167ZeroEncounterState.Value;

        Assert.True(state.Binding.Passed, string.Join(",", state.Binding.Diagnostics));
        Assert.All(state.Binding.Bindings.SelectMany(item => item.Branches),
            branch => Assert.NotEqual(GeneratedCampaignBranchKind.CHALLENGE, branch.Kind));
        Assert.True(state.Choices.Passed, string.Join(",", state.Choices.Diagnostics));
        Assert.Equal("CHOICE_CURRENT", state.Choices.Status);
        Assert.True(state.Choices.SupportBranchCount > 0);
        Assert.True(state.Choices.RefuseBranchCount > 0);
        Assert.Equal(0, state.Choices.ChallengeBranchCount);
    }

    [Fact]
    public void Behavioral_zero_encounter_choice_profile_does_not_claim_current_without_later_slices()
    {
        var state = Goal167ZeroEncounterState.Value;

        Assert.Null(state.Combat);
        Assert.Equal("TRAVEL_CURRENT", state.World.Status);
    }

    [Theory]
    [InlineData("summary")]
    [InlineData("overlay")]
    [InlineData("flag-inventory")]
    public void Behavioral_choice_seal_rejects_summary_overlay_and_flag_inventory_tamper(string kind)
    {
        using var fixture = CandidateSealFixture.Create();
        var overlay = kind == "summary" ? null : new GeneratedCampaignChoiceOverlayDocument
        {
            Passed = true,
            OutputPackageSha256 = new string('d', 64),
            FlagInventory = kind == "flag-inventory"
                ? [new GeneratedCampaignChoiceFlagInventoryRow
                {
                    DialogueId = "dialogue/tampered",
                    SupportedBranchKinds = [GeneratedCampaignBranchKind.SUPPORT]
                }]
                : []
        };
        var tampered = fixture.Build with
        {
            GeneratedCampaignChoices = new GameProjectGeneratedCampaignChoiceSummary
            {
                Present = true,
                Passed = true,
                Status = "CHOICE_CURRENT",
                FinalPackageSha256 = fixture.Build.PackageSha256,
                Overlay = overlay
            }
        };
        var result = fixture.Service.Verify(fixture.Root, fixture.Seal, tampered,
            fixture.Snapshot, fixture.Diff, fixture.Authoring);

        Assert.False(result.Passed);
        Assert.Contains("regeneration.candidate_choice_changed", result.Diagnostics);
    }

    private static GameProjectBuildHistoryEntry ReadHistory(string path) =>
        JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
}

internal static class Goal167V4State
{
    private static readonly Lazy<Goal167V4Fixture> Fixture = new(Create);
    public static Goal167V4Fixture Value => Fixture.Value;

    private static Goal167V4Fixture Create()
    {
        var source = Goal164TestKit.AllSelectable;
        var project = Goal156TestKit.Copy(source.Project, "goal167-genuine-v4");
        var historyRoot = Path.Combine(project.Path,
            UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));
        foreach (var path in Directory.EnumerateFiles(historyRoot, "*.json")) File.Delete(path);
        var v5 = JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(
            File.ReadAllText(source.Build.BuildHistoryPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var v4 = v5 with
        {
            SchemaVersion = GameProjectBuildHistoryReader.SchemaVersionV4,
            FinalStateHash = v5.GeneratedEncounterCombat!.FinalStateHash,
            GeneratedCampaignChoices = null
        };
        File.WriteAllText(Path.Combine(historyRoot, "genuine-v4.json"),
            JsonSerializer.Serialize(v4, new JsonSerializerOptions { WriteIndented = true }));

        var authoringRoot = Path.Combine(project.Path,
            UnifiedGameProjectWorkspaceVocabulary.AuthoringRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));
        var authoringPath = Directory.EnumerateFiles(authoringRoot,
            "*" + FeatureModuleCompositionDocumentVocabulary.FileExtension).Single();
        var authoring = JsonSerializer.Deserialize<FeatureModuleCompositionDocument>(
            File.ReadAllText(authoringPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })! with
        {
            LastQualifiedFinalStateHash = v4.FinalStateHash
        };
        File.WriteAllText(authoringPath,
            JsonSerializer.Serialize(authoring, new JsonSerializerOptions { WriteIndented = true }));

        var controller = Goal156TestKit.OpenWorkspace(project.Path);
        var snapshot = controller.Snapshot();
        var current = new CurrentGamePackageService(Goal156TestKit.Repository);
        current.LoadAsync(project.Path, CancellationToken.None).GetAwaiter().GetResult();
        var saves = Goal161ServiceBundle.Create(source: Goal156TestKit.SourceService);
        var capture = new GeneratedCampaignSessionTruthService(
            current, saves.Validator, saves.Coordinator).Capture();
        return new Goal167V4Fixture(project, snapshot, capture);
    }
}

internal sealed record Goal167V4Fixture(
    GeneratedProject Project,
    UnifiedGameProjectWorkspaceSnapshot Snapshot,
    (GeneratedCampaignSessionStatus Status, GeneratedCampaignProjectTruth? Truth,
        IReadOnlyList<string> Diagnostics) Capture);

internal static class Goal167ZeroEncounterState
{
    private static readonly Lazy<Goal167ZeroEncounterFixture> Fixture = new(Create);
    public static Goal167ZeroEncounterFixture Value => Fixture.Value;

    private static Goal167ZeroEncounterFixture Create()
    {
        var build = Goal164TestKit.AllSelectable;
        var mvp = Goal164TestKit.Clone(build.Source.GeneratedMvpPackage!);
        mvp.GeneratedContent.Encounters = [];
        var strict = build.Source with
        {
            Source = build.Source.Source! with
            {
                Counts = build.Source.Source.Counts with { Encounters = 0 }
            },
            GeneratedMvpPackage = mvp,
            RegeneratedPlan = build.Source.RegeneratedPlan! with { EncounterSeeds = [] }
        };
        var package = Goal164TestKit.Clone(build.LaneAPackage);
        package.Game.Encounters = [];
        package.GeneratedContent.Encounters = [];
        foreach (var quest in package.Game.Quests.Where(item =>
                     package.GeneratedContent.Quests.Any(generated =>
                         generated.PackageQuestId == item.Id)))
            quest.Objectives = [];
        var binding = new GeneratedCampaignChoiceBindingService().Bind(strict, package);
        var overlay = new GeneratedCampaignChoiceOverlayService().Build(package, binding);
        Assert.True(overlay.Passed, string.Join(",", overlay.Diagnostics));
        var choices = new GameProjectGeneratedCampaignChoiceQualificationService().Qualify(
            overlay.ChoiceOverlayPackage, overlay.Document, build.Runtime);
        var world = new GameProjectGeneratedWorldSummaryService().Restore(
            strict,
            build.Build.GeneratedWorld,
            matchesCurrentAuthoring: true,
            build.Build.GeneratedWorldActivation,
            build.Build.GeneratedRegionTravel,
            combat: null,
            choices)!;
        return new Goal167ZeroEncounterFixture(binding, choices, null, world);
    }
}

internal sealed record Goal167ZeroEncounterFixture(
    GeneratedCampaignChoiceBindingResult Binding,
    GameProjectGeneratedCampaignChoiceSummary Choices,
    GameProjectGeneratedEncounterCombatSummary? Combat,
    GameProjectGeneratedWorldSummary World);
