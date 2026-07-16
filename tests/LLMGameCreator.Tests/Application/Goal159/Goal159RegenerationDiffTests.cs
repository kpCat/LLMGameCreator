using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal159;

[Collection(Goal156Collection.Name)]
public sealed class Goal159RegenerationDiffTests
{
    [Fact]
    public void Behavioral_different_seed_produces_meaningful_gameplay_diff()
    {
        var diff = Goal159SuccessState.Value.Preview.Diff!;

        Assert.True(diff.GameplayChanged);
        Assert.NotEqual(diff.OldSourceRequestSha256, diff.NewSourceRequestSha256);
        Assert.NotEqual(diff.OldPlanSha256, diff.NewPlanSha256);
        Assert.True(diff.AddedRecordCount + diff.RemovedRecordCount + diff.ChangedRecordCount > 0);
    }

    [Fact]
    public void Behavioral_changed_mode_produces_meaningful_diff()
    {
        var current = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path);
        using var candidate = Goal159TestKit.CreateArtifacts(new SeededGeneratedProjectGenerationRequest
        {
            Seed = current.ResolvedGenerationOptions!.Seed,
            Mode = ProceduralGameGenerationModes.FullySeededWorld,
            PresetId = current.ResolvedGenerationOptions.PresetId
        });

        var diff = new GameProjectSeedRegenerationDiffService().Compare(
            current, candidate.Validation, true, true);

        Assert.True(diff.GameplayChanged);
        Assert.NotEqual(diff.OldMode, diff.NewMode);
        Assert.NotEqual(diff.OldPlanSha256, diff.NewPlanSha256);
    }

    [Fact]
    public void Behavioral_changed_preset_without_overrides_changes_resolved_options_and_diff()
    {
        var current = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path);
        using var candidate = Goal159TestKit.CreateArtifacts(new SeededGeneratedProjectGenerationRequest
        {
            Seed = current.ResolvedGenerationOptions!.Seed,
            Mode = current.ResolvedGenerationOptions.Mode,
            PresetId = "recover_resource"
        });

        var diff = new GameProjectSeedRegenerationDiffService().Compare(
            current, candidate.Validation, true, true);

        Assert.True(diff.GameplayChanged);
        Assert.NotEqual(diff.OldPresetId, diff.NewPresetId);
        Assert.False(candidate.Validation.ResolvedGenerationOptions!.StyleOverridesApplied);
        Assert.NotEqual(current.ResolvedGenerationOptions.CompactStyleHintIds,
            candidate.Validation.ResolvedGenerationOptions.CompactStyleHintIds);
    }

    [Fact]
    public void Behavioral_record_counts_match_canonical_collection_comparison()
    {
        var diff = Goal159SuccessState.Value.Preview.Diff!;

        Assert.Equal(diff.AddedRecordCount, diff.AddedByCollection.Values.Sum());
        Assert.Equal(diff.RemovedRecordCount, diff.RemovedByCollection.Values.Sum());
        Assert.Equal(diff.ChangedRecordCount, diff.ChangedByCollection.Values.Sum());
        Assert.True(diff.UnchangedRecordCount >= 0);
    }

    [Fact]
    public void Behavioral_diff_reflects_preserved_authoring_and_identity()
    {
        var diff = Goal159SuccessState.Value.Preview.Diff!;

        Assert.True(diff.AuthoringPreserved);
        Assert.True(diff.ProjectIdentityPreserved);
        Assert.DoesNotContain("regeneration.authoring_not_preserved", diff.Diagnostics);
        Assert.DoesNotContain("regeneration.identity_not_preserved", diff.Diagnostics);
    }

    [Fact]
    public void Behavioral_diff_uses_data_derived_counts_without_fixed_content_assumption()
    {
        var fixture = Goal159SuccessState.Value;
        var diff = fixture.Preview.Diff!;

        Assert.Equal(fixture.Preview.CandidateSourceSummary,
            fixture.Source.ResolvedGenerationOptions!.StableSummary);
        Assert.Equal(fixture.Source.Source!.Counts.Regions, diff.NewCounts.Regions);
        Assert.Equal(fixture.Source.Source.Counts.Factions, diff.NewCounts.Factions);
        Assert.Equal(fixture.Source.Source.Counts.Actors, diff.NewCounts.Actors);
        Assert.Equal(fixture.Source.Source.Counts.ItemsAndResources, diff.NewCounts.ItemsAndResources);
    }

    [Fact]
    public void Behavioral_diff_contains_old_and_new_world_titles()
    {
        var diff = Goal159SuccessState.Value.Preview.Diff!;

        Assert.False(string.IsNullOrWhiteSpace(diff.OldStartRegionTitle));
        Assert.False(string.IsNullOrWhiteSpace(diff.NewStartRegionTitle));
        Assert.False(string.IsNullOrWhiteSpace(diff.OldTravelDestinationTitle));
        Assert.False(string.IsNullOrWhiteSpace(diff.NewTravelDestinationTitle));
    }

    [Fact]
    public void Behavioral_request_hash_normalizes_override_order_and_newlines()
    {
        var first = GameProjectSeedRegenerationDiffService.RequestSha256(new()
        {
            Seed = " seed ", Mode = GenerationPresetOptionsService.DefaultMode,
            PresetId = GenerationPresetOptionsService.DefaultPresetId,
            CompactStyleHintIds = ["theme/trade", "tone/mysterious"]
        });
        var second = GameProjectSeedRegenerationDiffService.RequestSha256(new()
        {
            Seed = "seed", Mode = GenerationPresetOptionsService.DefaultMode,
            PresetId = GenerationPresetOptionsService.DefaultPresetId,
            CompactStyleHintIds = ["tone/mysterious", "theme/trade", "theme/trade"]
        });

        Assert.Equal(first, second);
    }
}
