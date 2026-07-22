using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal167;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal167SaveMigrationTests
{
    [Fact]
    public void Behavioral_exact_save_and_continue_preserve_branch_flag_reputation_and_journal()
    {
        var state = Goal167SaveMigrationState.Value;

        Assert.True(state.Saved.Passed, string.Join(",", state.Saved.Diagnostics));
        Assert.True(state.Loaded.Passed, string.Join(",", state.Loaded.Diagnostics));
        Assert.Equal("SUPPORT", Flag(state.Loaded.Session!, state.DialogueId));
        Assert.Equal(state.SupportResult.Session.GameplayState.Factions
                .Select(item => (item.FactionId, item.Reputation)),
            state.Loaded.Session!.GameplayState.Factions
                .Select(item => (item.FactionId, item.Reputation)));
        Assert.Single(new GeneratedCampaignDecisionJournalService()
            .Project(state.Build.Package, state.Loaded.Session).Decisions);
    }

    [Fact]
    public void Behavioral_exact_continue_keeps_initial_alternatives_locked()
    {
        var state = Goal167SaveMigrationState.Value;
        var opened = state.Build.Runtime.ExecuteGameplayCommand(state.Build.Package,
            state.Loaded.Session!, GameRuntimeCommand.OpenDialogue(state.DialogueId));

        Assert.True(opened.Success, string.Join(",", opened.Diagnostics.Select(item => item.Code)));
        var choiceIds = opened.GameplayEvents.Single(item => item.Type == GameRuntimeEventType.DialogueOpened)
            .Args["choiceIds"].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.DoesNotContain("generatedChoice/support", choiceIds);
        Assert.DoesNotContain("generatedChoice/challenge", choiceIds);
        Assert.DoesNotContain("generatedChoice/refuse", choiceIds);
        Assert.Single(choiceIds);
    }

    [Fact]
    public void Behavioral_pre_choice_save_remains_unchosen_with_empty_journal()
    {
        var state = Goal167SaveMigrationState.Value;

        Assert.True(state.PreChoiceLoaded.Passed, string.Join(",", state.PreChoiceLoaded.Diagnostics));
        Assert.DoesNotContain(state.PreChoiceLoaded.Session!.GameplayState.Flags,
            item => item.Id == state.DialogueId && !string.IsNullOrWhiteSpace(item.Value));
        Assert.Empty(new GeneratedCampaignDecisionJournalService()
            .Project(state.Build.Package, state.PreChoiceLoaded.Session).Decisions);
    }

    [Fact]
    public void Behavioral_package_rebase_preview_is_explicit_and_zero_write()
    {
        var state = Goal167SaveMigrationState.Value;

        Assert.Equal(GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED,
            state.CompatiblePreview.SourceStatus);
        Assert.True(state.CompatiblePreview.Passed, string.Join(",", state.CompatiblePreview.Diagnostics));
        Assert.False(string.IsNullOrWhiteSpace(state.CompatiblePreview.CandidateSessionSha256));
    }

    [Fact]
    public void Behavioral_compatible_dialogue_migration_preserves_branch_flag()
    {
        var state = Goal167SaveMigrationState.Value;

        Assert.True(state.CompatibleApplied.Passed,
            string.Join(",", state.CompatibleApplied.Diagnostics));
        Assert.Equal("SUPPORT", Flag(state.CompatibleApplied.Session!, state.DialogueId));
        Assert.Single(new GeneratedCampaignDecisionJournalService()
            .Project(state.Build.Package, state.CompatibleApplied.Session!).Decisions);
    }

    [Fact]
    public void Behavioral_incompatible_dialogue_migration_drops_flag_without_ghost_journal()
    {
        var state = Goal167SaveMigrationState.Value;

        Assert.True(state.IncompatiblePreview.Passed,
            string.Join(",", state.IncompatiblePreview.Diagnostics));
        Assert.True(state.IncompatibleApplied.Passed,
            string.Join(",", state.IncompatibleApplied.Diagnostics));
        Assert.DoesNotContain(state.IncompatibleApplied.Session!.GameplayState.Flags,
            item => item.Id == state.DialogueId);
        Assert.Empty(new GeneratedCampaignDecisionJournalService()
            .Project(state.Build.Package, state.IncompatibleApplied.Session).Decisions);
    }

    [Fact]
    public void Behavioral_migration_resets_transient_dialogue_and_keeps_runtime_usable()
    {
        var state = Goal167SaveMigrationState.Value;

        Assert.Null(state.CompatibleApplied.Session!.GameplayState.ActiveDialogue);
        Assert.Null(state.CompatibleApplied.Session.GameplayState.ActiveEncounter);
        var opened = state.Build.Runtime.ExecuteGameplayCommand(state.Build.Package,
            state.CompatibleApplied.Session, GameRuntimeCommand.OpenDialogue(state.DialogueId));
        Assert.True(opened.Success, string.Join(",", opened.Diagnostics.Select(item => item.Code)));
    }

    private static string Flag(UnifiedRuntimeSession session, string dialogueId) =>
        session.GameplayState.Flags.Single(item => item.Id == dialogueId).Value;
}

internal static class Goal167SaveMigrationState
{
    private static readonly Lazy<Goal167SaveMigrationFixture> Fixture = new(Create);
    public static Goal167SaveMigrationFixture Value => Fixture.Value;

    private static Goal167SaveMigrationFixture Create()
    {
        var build = Goal164TestKit.AllSelectable;
        var executed = ExecutedFixture.Create(GeneratedCampaignBranchKind.SUPPORT);
        var dialogueId = executed.Binding.DialogueId;
        var saved = build.Saves.Save.Save(build.Project.Path, "goal167-support", executed.Result.Session);
        Assert.True(saved.Passed, string.Join(",", saved.Diagnostics));
        var loaded = build.Saves.Save.Load(build.Project.Path, "goal167-support");

        var started = build.Runtime.Start(build.Package);
        Assert.True(started.Success);
        var preChoiceSaved = build.Saves.Save.Save(
            build.Project.Path, "goal167-pre-choice", started.Session);
        Assert.True(preChoiceSaved.Passed, string.Join(",", preChoiceSaved.Diagnostics));
        var preChoiceLoaded = build.Saves.Save.Load(build.Project.Path, "goal167-pre-choice");

        var compatibleSource = saved.Revision! with
        {
            ParentRevisionSha256 = null,
            PackageSha256 = new string('a', 64),
            RevisionSha256 = string.Empty
        };
        var compatibleWrite = build.Saves.Store.WriteRevision(
            build.Project.Path, "goal167-compatible-rebase", compatibleSource);
        Assert.True(compatibleWrite.Passed, string.Join(",", compatibleWrite.Diagnostics));
        var compatiblePreview = build.Saves.Migration.Preview(
            build.Project.Path, "goal167-compatible-rebase");
        var compatibleApplied = build.Saves.Migration.Apply(new GeneratedGameplaySaveMigrationApplyRequest
        {
            ProjectFolder = build.Project.Path,
            SlotName = "goal167-compatible-rebase",
            SourceRevisionSha256 = compatiblePreview.SourceRevisionSha256,
            CandidateSessionSha256 = compatiblePreview.CandidateSessionSha256
        });

        var incompatibleFingerprints = saved.Revision.DefinitionFingerprints.Select(item =>
            item.Id == dialogueId ? item with { CanonicalSha256 = new string('b', 64) } : item).ToList();
        Assert.Contains(incompatibleFingerprints,
            item => item.Id == dialogueId && item.CanonicalSha256 == new string('b', 64));
        var incompatibleSource = saved.Revision with
        {
            ParentRevisionSha256 = null,
            PackageSha256 = new string('c', 64),
            DefinitionFingerprints = incompatibleFingerprints,
            RevisionSha256 = string.Empty
        };
        var incompatibleWrite = build.Saves.Store.WriteRevision(
            build.Project.Path, "goal167-incompatible-rebase", incompatibleSource);
        Assert.True(incompatibleWrite.Passed, string.Join(",", incompatibleWrite.Diagnostics));
        var incompatiblePreview = build.Saves.Migration.Preview(
            build.Project.Path, "goal167-incompatible-rebase");
        var incompatibleApplied = build.Saves.Migration.Apply(new GeneratedGameplaySaveMigrationApplyRequest
        {
            ProjectFolder = build.Project.Path,
            SlotName = "goal167-incompatible-rebase",
            SourceRevisionSha256 = incompatiblePreview.SourceRevisionSha256,
            CandidateSessionSha256 = incompatiblePreview.CandidateSessionSha256
        });

        return new Goal167SaveMigrationFixture(build, dialogueId, executed.Result, saved, loaded,
            preChoiceLoaded, compatiblePreview, compatibleApplied,
            incompatiblePreview, incompatibleApplied);
    }
}

internal sealed record Goal167SaveMigrationFixture(
    Goal164BuildFixture Build,
    string DialogueId,
    UnifiedRuntimeResult SupportResult,
    GeneratedGameplaySaveResult Saved,
    GeneratedGameplaySaveResult Loaded,
    GeneratedGameplaySaveResult PreChoiceLoaded,
    GeneratedGameplaySaveMigrationPreview CompatiblePreview,
    GeneratedGameplaySaveMigrationResult CompatibleApplied,
    GeneratedGameplaySaveMigrationPreview IncompatiblePreview,
    GeneratedGameplaySaveMigrationResult IncompatibleApplied);
