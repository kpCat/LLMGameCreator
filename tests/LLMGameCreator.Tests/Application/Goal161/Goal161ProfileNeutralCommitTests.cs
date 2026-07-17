using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal159;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161;

[Collection(Goal160Collection.Name)]
public sealed class Goal161ProfileNeutralCommitTests
{
    [Fact]
    public void Behavioral_all_selectable_regeneration_apply_remains_green()
    {
        var state = Goal161MigrationState.Value;
        Assert.True(state.Regenerated.Applied);
        Assert.Equal("GREEN", state.Regenerated.Status);
        Assert.True(state.Regenerated.AuthoritativeSnapshot?.AcceptedMechanics?.Passed);
    }

    [Fact]
    public void Behavioral_all_selectable_history_rollback_remains_green()
    {
        var state = Goal161MigrationState.Value;
        Assert.True(state.Rollback.Applied);
        Assert.Equal("GREEN", state.Rollback.Status);
        Assert.True(state.Rollback.AuthoritativeSnapshot?.AcceptedMechanics?.Passed);
    }

    [Fact]
    public void Behavioral_real_core_only_regeneration_candidate_is_campaign_current()
    {
        var state = Goal161CoreProfileState.Value;
        Assert.Equal("GREEN", state.RegenerationPreview.Status);
        Assert.Equal("CAMPAIGN_CURRENT", state.RegenerationPreview.CandidateSnapshot?.GeneratedWorld?.Status);
        Assert.True(state.RegenerationPreview.CandidateSnapshot?.AcceptedMechanicsCompatibility?.Passed);
    }

    [Fact]
    public void Behavioral_real_core_only_regeneration_semantic_commit_succeeds()
    {
        var result = Goal161CoreProfileState.Value.Regenerated;
        Assert.True(result.Applied, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal("GREEN", result.Status);
        Assert.Equal("CAMPAIGN_CURRENT", result.AuthoritativeSnapshot?.GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_real_core_only_rollback_candidate_is_campaign_current()
    {
        var preview = Goal161CoreProfileState.Value.RollbackPreview;
        Assert.Equal("GREEN", preview.Status);
        Assert.Equal("CAMPAIGN_CURRENT", preview.CandidateSnapshot?.GeneratedWorld?.Status);
        Assert.True(preview.CandidateSnapshot?.AcceptedMechanicsCompatibility?.Passed);
    }

    [Fact]
    public void Behavioral_real_core_only_rollback_semantic_commit_succeeds()
    {
        var result = Goal161CoreProfileState.Value.RolledBack;
        Assert.True(result.Applied, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal("GREEN", result.Status);
        Assert.Equal("CAMPAIGN_CURRENT", result.AuthoritativeSnapshot?.GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_core_only_accepted_mechanics_remains_false_and_incomplete()
    {
        var state = Goal161CoreProfileState.Value;
        Assert.False(state.Regenerated.AuthoritativeSnapshot?.AcceptedMechanics?.Passed);
        Assert.NotEmpty(state.Regenerated.AuthoritativeSnapshot?.AcceptedMechanics?.MissingFactKinds ?? []);
        Assert.False(state.RolledBack.AuthoritativeSnapshot?.AcceptedMechanics?.Passed);
        Assert.NotEmpty(state.RolledBack.AuthoritativeSnapshot?.AcceptedMechanics?.MissingFactKinds ?? []);
    }

    [Fact]
    public void Behavioral_core_only_rc_never_claims_ready_pending_or_current()
    {
        var state = Goal161CoreProfileState.Value;
        AssertCoreRc(state.RegenerationPreview.CandidateSnapshot);
        AssertCoreRc(state.Regenerated.AuthoritativeSnapshot);
        AssertCoreRc(state.RollbackPreview.CandidateSnapshot);
        AssertCoreRc(state.RolledBack.AuthoritativeSnapshot);
    }

    [Fact]
    public void Behavioral_custom_partial_selection_exact_sealed_summary_validates()
    {
        using var fixture = CandidateSealFixture.Create(CustomSnapshot());
        var result = fixture.Verify();
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal("custom_partial", result.Seal?.MechanicsProfileId);
        Assert.Equal(GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(
            fixture.Snapshot.AcceptedMechanics), result.Seal?.AcceptedMechanicsSummarySha256);
    }

    [Fact]
    public void Behavioral_tampered_accepted_mechanics_summary_fails_seal_validation()
    {
        using var fixture = CandidateSealFixture.Create(CustomSnapshot());
        var snapshot = fixture.Snapshot with
        {
            AcceptedMechanics = fixture.Snapshot.AcceptedMechanics! with { Passed = true }
        };
        var result = fixture.Service.Verify(fixture.Root, fixture.Seal, fixture.Build, snapshot,
            fixture.Diff, fixture.Authoring);
        Assert.Contains("regeneration.candidate_history_changed", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_tampered_compatibility_summary_fails_seal_validation()
    {
        using var fixture = CandidateSealFixture.Create(CustomSnapshot());
        var snapshot = fixture.Snapshot with
        {
            AcceptedMechanicsCompatibility = fixture.Snapshot.AcceptedMechanicsCompatibility! with
            {
                CompatibilityFinalStateHash = new string('f', 64)
            }
        };
        var result = fixture.Service.Verify(fixture.Root, fixture.Seal, fixture.Build, snapshot,
            fixture.Diff, fixture.Authoring);
        Assert.Contains("regeneration.candidate_history_changed", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_candidate_rc_status_mismatch_fails_seal_validation()
    {
        using var fixture = CandidateSealFixture.Create(CustomSnapshot());
        var snapshot = fixture.Snapshot with { ReleaseCandidateConfigurationStatus = "CURRENT" };
        var result = fixture.Service.Verify(fixture.Root, fixture.Seal, fixture.Build, snapshot,
            fixture.Diff, fixture.Authoring);
        Assert.Contains("regeneration.candidate_tampered", result.Diagnostics);
    }

    private static UnifiedGameProjectWorkspaceSnapshot CustomSnapshot()
    {
        var accepted = new GameProjectAcceptedMechanicsSummary
        {
            Present = true,
            Passed = false,
            SelectedMechanicCount = 3,
            MissingFactKinds = ["status"],
            QualificationFinalStateHash = new string('a', 64)
        };
        return new UnifiedGameProjectWorkspaceSnapshot
        {
            GeneratedWorld = new GameProjectGeneratedWorldSummary
            {
                Present = true,
                Passed = true,
                Status = "TRAVEL_CURRENT",
                MechanicsProfileId = "custom_partial",
                PlanSha256 = new string('1', 64),
                OverlaySha256 = new string('2', 64),
                GeneratedBasePackageSha256 = new string('3', 64)
            },
            AcceptedMechanics = accepted,
            AcceptedMechanicsCompatibility = new GameProjectAcceptedMechanicsCompatibilityResult
            {
                Passed = true,
                CompatibilityFinalStateHash = new string('a', 64),
                AcceptedMechanics = accepted
            },
            ReleaseCandidateRecordConfigurationStatus = "ABSENT",
            ReleaseCandidateConfigurationStatus = "ABSENT"
        };
    }

    private static void AssertCoreRc(UnifiedGameProjectWorkspaceSnapshot? snapshot)
    {
        Assert.NotNull(snapshot);
        Assert.DoesNotContain(snapshot!.ReleaseCandidateRecordConfigurationStatus,
            new[] { "READY", "CURRENT", "BUILD_GREEN_STANDALONE_PENDING" });
        Assert.DoesNotContain(snapshot.ReleaseCandidateConfigurationStatus,
            new[] { "READY", "CURRENT", "BUILD_GREEN_STANDALONE_PENDING" });
    }
}

internal static class Goal161CoreProfileState
{
    private static readonly Lazy<Goal161CoreProfileFixture> Fixture = new(Goal161CoreProfileFixture.Create);
    public static Goal161CoreProfileFixture Value => Fixture.Value;
}

internal sealed record Goal161CoreProfileFixture(
    GeneratedProject Project,
    Goal161WorldBundle Bundle,
    GeneratedGameplaySaveResult Saved,
    GameProjectSeedRegenerationPreview RegenerationPreview,
    GameProjectSeedRegenerationResult Regenerated,
    GeneratedGameplaySaveMigrationResult Migrated,
    GameProjectGeneratedWorldRollbackPreview RollbackPreview,
    GameProjectGeneratedWorldRollbackResult RolledBack)
{
    public static Goal161CoreProfileFixture Create()
    {
        var project = Goal156TestKit.Copy(Goal157BuildState.Value.CoreProject, "goal161-core-profile");
        var bundle = Goal161WorldBundle.Create(project.Path);
        var originalWorldId = bundle.Controller.ReadGeneratedWorldHistory().CurrentWorldId;
        var package = Goal156TestKit.Load(project.Path);
        var saved = bundle.Saves.Save.Save(project.Path, "core", bundle.Saves.Runtime.Start(package).Session);
        Assert.True(saved.Passed, string.Join(Environment.NewLine, saved.Diagnostics));
        var request = bundle.Controller.CreateGeneratedWorldRegenerationRequest(
            Goal159TestKit.ChangedRequest(bundle.Controller.Snapshot(), "goal161-core-regenerated"));
        var preview = bundle.Controller.PreviewGeneratedWorldRegeneration(request);
        Assert.Equal("GREEN", preview.Status);
        var regenerated = bundle.Controller.ApplyGeneratedWorldRegeneration(request, preview);
        Assert.True(regenerated.Applied, string.Join(Environment.NewLine, regenerated.Diagnostics));
        var migrationPreview = bundle.Saves.Migration.Preview(project.Path, "core");
        Assert.True(migrationPreview.Passed, string.Join(Environment.NewLine, migrationPreview.Diagnostics));
        var migrated = bundle.Saves.Migration.Apply(new GeneratedGameplaySaveMigrationApplyRequest
        {
            ProjectFolder = project.Path,
            SlotName = migrationPreview.SlotName,
            SourceRevisionSha256 = migrationPreview.SourceRevisionSha256,
            CandidateSessionSha256 = migrationPreview.CandidateSessionSha256
        });
        Assert.True(migrated.Passed, string.Join(Environment.NewLine, migrated.Diagnostics));
        var rollbackRequest = bundle.Controller.CreateGeneratedWorldRollbackRequest(originalWorldId);
        var rollbackPreview = bundle.Controller.PreviewGeneratedWorldRollback(rollbackRequest);
        Assert.Equal("GREEN", rollbackPreview.Status);
        var rolledBack = bundle.Controller.ApplyGeneratedWorldRollback(rollbackRequest, rollbackPreview);
        Assert.True(rolledBack.Applied, string.Join(Environment.NewLine, rolledBack.Diagnostics));
        return new Goal161CoreProfileFixture(project, bundle, saved, preview, regenerated, migrated,
            rollbackPreview, rolledBack);
    }
}
