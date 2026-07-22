using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal159;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal164;

[Collection(Goal160Collection.Name)]
public sealed class Goal164RegenerationRollbackTests
{
    [Fact]
    public void Behavioral_regeneration_candidate_rebuilds_campaign_current_combat()
    {
        var state = Goal164RegenerationState.Value;

        Assert.Equal("GREEN", state.Preview.Status);
        Assert.Equal("CAMPAIGN_CURRENT", state.Preview.CandidateBuild?.GeneratedEncounterCombat?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", state.Preview.CandidateSnapshot?.GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_regeneration_candidate_seal_contains_combat_hashes()
    {
        var seal = Goal164RegenerationState.Value.Seal;

        Assert.Equal(64, seal.GeneratedEncounterCombatSummarySha256.Length);
        Assert.Equal(64, seal.GeneratedEncounterCombatOverlaySha256.Length);
        Assert.False(string.IsNullOrWhiteSpace(seal.GeneratedEncounterCombatContractId));
    }

    [Fact]
    public void Behavioral_regeneration_candidate_primary_hash_is_combat_overlay_hash()
    {
        var build = Goal164RegenerationState.Value.Preview.CandidateBuild!;

        Assert.Equal(build.PackageSha256, build.GeneratedEncounterCombat?.Overlay?.OutputPackageSha256);
        Assert.Equal(build.PackageSha256, build.GeneratedEncounterCombat?.ExactPackageSha256);
    }

    [Fact]
    public void Behavioral_regeneration_apply_commits_v5_current_history()
    {
        var state = Goal164RegenerationState.Value;

        Assert.True(state.Applied.Applied, string.Join(",", state.Applied.Diagnostics));
        Assert.Equal("CAMPAIGN_CURRENT", state.AfterRegeneration.GeneratedEncounterCombat?.Status);
        Assert.Equal("CHOICE_CURRENT", state.AfterRegeneration.GeneratedCampaignChoices?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", state.AfterRegeneration.GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_regeneration_changes_world_but_not_contract_source_authority()
    {
        var state = Goal164RegenerationState.Value;

        Assert.NotEqual(state.OriginalWorldId, state.RegeneratedWorldId);
        Assert.NotEqual(state.Build.Contract.Contract?.ContractId,
            state.AfterRegeneration.GeneratedEncounterCombat?.ContractId);
        Assert.Equal(64,
            state.AfterRegeneration.GeneratedEncounterCombat?.ContractSourcePackageSha256.Length);
        Assert.NotEqual(state.AfterRegeneration.GeneratedEncounterCombat?.ExactPackageSha256,
            state.AfterRegeneration.GeneratedEncounterCombat?.ContractSourcePackageSha256);
    }

    [Fact]
    public void Behavioral_history_rollback_candidate_rebuilds_current_combat_contract()
    {
        var state = Goal164RegenerationState.Value;

        Assert.Equal("GREEN", state.RollbackPreview.Status);
        Assert.Equal("CAMPAIGN_CURRENT",
            state.RollbackPreview.CandidateBuild?.GeneratedEncounterCombat?.Status);
    }

    [Fact]
    public void Behavioral_history_rollback_apply_restores_campaign_current_world()
    {
        var state = Goal164RegenerationState.Value;

        Assert.True(state.RolledBack.Applied, string.Join(",", state.RolledBack.Diagnostics));
        Assert.Equal(state.OriginalWorldId, state.RolledBackWorldId);
        Assert.Equal("CAMPAIGN_CURRENT", state.AfterRollback.GeneratedEncounterCombat?.Status);
    }
}

internal static class Goal164RegenerationState
{
    private static readonly Lazy<Goal164RegenerationFixture> Fixture = new(Create);
    public static Goal164RegenerationFixture Value => Fixture.Value;

    private static Goal164RegenerationFixture Create()
    {
        var build = Goal164BuildFixture.Create(coreOnly: false);
        var original = build.Controller.Snapshot();
        var originalWorldId = build.Source.RegeneratedPlan!.World.WorldId;
        var rollbackTargetWorldId = build.Controller.ReadGeneratedWorldHistory().CurrentWorldId;
        var request = build.Controller.CreateGeneratedWorldRegenerationRequest(
            Goal159TestKit.ChangedRequest(original, "goal164-regenerated-combat"));
        var preview = build.Controller.PreviewGeneratedWorldRegeneration(request);
        Assert.True(string.Equals("GREEN", preview.Status, StringComparison.Ordinal),
            string.Join(Environment.NewLine, preview.Diagnostics));
        var sealPath = Path.Combine(preview.CandidateRoot,
            GameProjectSeedRegenerationVocabulary.CandidateSealRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var seal = JsonSerializer.Deserialize<GameProjectSeedRegenerationCandidateSeal>(
            File.ReadAllText(sealPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var applied = build.Controller.ApplyGeneratedWorldRegeneration(request, preview);
        Assert.True(applied.Applied, string.Join(Environment.NewLine, applied.Diagnostics));
        var afterRegeneration = build.Controller.Snapshot();
        var regeneratedWorldId = build.SourceService.Validate(build.Project.Path).RegeneratedPlan!.World.WorldId;
        var rollbackRequest = build.Controller.CreateGeneratedWorldRollbackRequest(rollbackTargetWorldId);
        var rollbackPreview = build.Controller.PreviewGeneratedWorldRollback(rollbackRequest);
        Assert.Equal("GREEN", rollbackPreview.Status);
        var rolledBack = build.Controller.ApplyGeneratedWorldRollback(rollbackRequest, rollbackPreview);
        Assert.True(rolledBack.Applied, string.Join(Environment.NewLine, rolledBack.Diagnostics));
        var afterRollback = build.Controller.Snapshot();
        var rolledBackWorldId = build.SourceService.Validate(build.Project.Path).RegeneratedPlan!.World.WorldId;
        return new Goal164RegenerationFixture(build, originalWorldId, request, preview, seal, applied,
            afterRegeneration, regeneratedWorldId, rollbackPreview, rolledBack, afterRollback,
            rolledBackWorldId);
    }
}

internal sealed record Goal164RegenerationFixture(
    Goal164BuildFixture Build,
    string OriginalWorldId,
    GameProjectSeedRegenerationRequest Request,
    GameProjectSeedRegenerationPreview Preview,
    GameProjectSeedRegenerationCandidateSeal Seal,
    GameProjectSeedRegenerationResult Applied,
    UnifiedGameProjectWorkspaceSnapshot AfterRegeneration,
    string RegeneratedWorldId,
    GameProjectGeneratedWorldRollbackPreview RollbackPreview,
    GameProjectGeneratedWorldRollbackResult RolledBack,
    UnifiedGameProjectWorkspaceSnapshot AfterRollback,
    string RolledBackWorldId);
