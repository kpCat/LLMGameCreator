using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal167;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal167ChoicePreviewJournalTests
{
    [Fact]
    public void Behavioral_preview_availability_is_exactly_runtime_backed()
    {
        var fixture = PreviewFixture.Create(GeneratedCampaignBranchKind.SUPPORT);
        Assert.Equal(fixture.InitialChoiceIds, fixture.Preview.RuntimeAvailableChoiceIds);
        Assert.Equal(fixture.InitialChoiceIds,
            fixture.Preview.Options.Where(item => item.Enabled).Select(item => item.TechnicalChoiceId)
                .OrderBy(item => item, StringComparer.Ordinal));
    }

    [Fact]
    public void Behavioral_preview_does_not_mutate_original_session()
    {
        var fixture = PreviewFixture.Create(GeneratedCampaignBranchKind.SUPPORT);
        Assert.Equal(fixture.SessionBeforePreview, Goal164TestKit.Canonical(fixture.Opened.Session));
    }

    [Fact]
    public void Behavioral_preview_does_not_mutate_package()
    {
        var fixture = PreviewFixture.Create(GeneratedCampaignBranchKind.SUPPORT);
        Assert.Equal(fixture.PackageBeforePreview, Goal164TestKit.Canonical(fixture.Package));
    }

    [Fact]
    public void Behavioral_support_preview_uses_observed_positive_reputation_delta()
    {
        var fixture = PreviewFixture.Create(GeneratedCampaignBranchKind.SUPPORT);
        var option = fixture.Option;
        Assert.Equal(fixture.Branch.ReputationAmount, option.ObservedReputationDelta);
        Assert.Contains(option.ConsequencePreview, item => item.StartsWith("Репутация: +", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_support_preview_uses_observed_flag_and_quest_state()
    {
        var option = PreviewFixture.Create(GeneratedCampaignBranchKind.SUPPORT).Option;
        Assert.Equal("SUPPORT", option.ObservedFlagValue);
        Assert.Equal("active", option.ObservedQuestState);
        Assert.Contains("QuestStarted", option.RuntimeEventTypes);
    }

    [Fact]
    public void Behavioral_challenge_preview_uses_observed_encounter()
    {
        var fixture = PreviewFixture.Create(GeneratedCampaignBranchKind.CHALLENGE);
        Assert.Equal(fixture.Branch.EncounterId, fixture.Option.ObservedEncounterId);
        Assert.Contains("EncounterStarted", fixture.Option.RuntimeEventTypes);
    }

    [Fact]
    public void Behavioral_refuse_preview_uses_observed_negative_reputation_only()
    {
        var option = PreviewFixture.Create(GeneratedCampaignBranchKind.REFUSE).Option;
        Assert.True(option.ObservedReputationDelta < 0);
        Assert.Equal(string.Empty, option.ObservedQuestState);
        Assert.Equal(string.Empty, option.ObservedEncounterId);
    }

    [Fact]
    public void Behavioral_preview_ignores_tampered_consequence_metadata()
    {
        var fixture = PreviewFixture.Create(GeneratedCampaignBranchKind.SUPPORT, package =>
        {
            var choice = package.Game.Dialogues.SelectMany(item => item.Nodes).SelectMany(item => item.Choices)
                .Single(item => item.Id == "generatedChoice/support");
            choice.Metadata["generatedChoiceReputationAmount"] = "9999";
        });
        Assert.Equal(fixture.Branch.ReputationAmount, fixture.Option.ObservedReputationDelta);
        Assert.DoesNotContain(fixture.Option.ConsequencePreview, item => item.Contains("9999", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_support_and_refuse_preview_survive_zero_generated_encounters()
    {
        var fixture = PreviewFixture.Create(GeneratedCampaignBranchKind.SUPPORT, package =>
        {
            package.Game.Encounters = [];
            package.GeneratedContent.Encounters = [];
        });
        Assert.True(fixture.Preview.Options.Single(item => item.TechnicalChoiceId == "generatedChoice/support").Enabled);
        Assert.True(fixture.Preview.Options.Single(item => item.TechnicalChoiceId == "generatedChoice/refuse").Enabled);
    }

    [Fact]
    public void Behavioral_support_journal_is_flag_and_state_backed()
    {
        var fixture = ExecutedFixture.Create(GeneratedCampaignBranchKind.SUPPORT);
        var row = Assert.Single(new GeneratedCampaignDecisionJournalService()
            .Project(fixture.Package, fixture.Result.Session).Decisions);
        Assert.Contains("Флаг ветви: SUPPORT", row.Consequence);
        Assert.Contains("Задание: active", row.Consequence);
        Assert.True(row.AlternativesLocked);
    }

    [Fact]
    public void Behavioral_support_active_journal_exposes_followup()
    {
        var fixture = ExecutedFixture.Create(GeneratedCampaignBranchKind.SUPPORT);
        var row = Assert.Single(new GeneratedCampaignDecisionJournalService()
            .Project(fixture.Package, fixture.Result.Session).Decisions);
        Assert.Equal(GeneratedCampaignDecisionStatus.FollowUpAvailable, row.Status);
    }

    [Fact]
    public void Behavioral_challenge_journal_waits_while_encounter_is_active()
    {
        var fixture = ExecutedFixture.Create(GeneratedCampaignBranchKind.CHALLENGE);
        var row = Assert.Single(new GeneratedCampaignDecisionJournalService()
            .Project(fixture.Package, fixture.Result.Session).Decisions);
        Assert.Equal(GeneratedCampaignDecisionStatus.Chosen, row.Status);
    }

    [Fact]
    public void Behavioral_challenge_journal_exposes_followup_after_flee()
    {
        var fixture = ExecutedFixture.Create(GeneratedCampaignBranchKind.CHALLENGE);
        var fled = fixture.Runtime.ExecuteGameplayCommand(fixture.Package, fixture.Result.Session,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.FleeEncounter });
        var row = Assert.Single(new GeneratedCampaignDecisionJournalService()
            .Project(fixture.Package, fled.Session).Decisions);
        Assert.Equal(GeneratedCampaignDecisionStatus.FollowUpAvailable, row.Status);
    }

    [Fact]
    public void Behavioral_invalid_flag_does_not_create_ghost_journal_row()
    {
        var fixture = ExecutedFixture.Create(GeneratedCampaignBranchKind.REFUSE);
        fixture.Result.Session.GameplayState.Flags.Single(item => item.Id == fixture.Binding.DialogueId).Value = "GHOST";
        Assert.Empty(new GeneratedCampaignDecisionJournalService()
            .Project(fixture.Package, fixture.Result.Session).Decisions);
    }

    [Fact]
    public void Behavioral_support_consequences_include_decision_lock_and_followup()
    {
        var outcome = ExecutedFixture.Create(GeneratedCampaignBranchKind.SUPPORT).Outcome;
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Decision);
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.BranchLocked);
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.BranchFollowUp);
    }

    [Fact]
    public void Behavioral_active_challenge_consequences_do_not_claim_followup()
    {
        var outcome = ExecutedFixture.Create(GeneratedCampaignBranchKind.CHALLENGE).Outcome;
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Decision);
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.BranchLocked);
        Assert.DoesNotContain(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.BranchFollowUp);
    }

    [Fact]
    public void Behavioral_refuse_consequences_are_state_backed()
    {
        var outcome = ExecutedFixture.Create(GeneratedCampaignBranchKind.REFUSE).Outcome;
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Reputation
            && item.Delta.StartsWith("-", StringComparison.Ordinal));
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Decision);
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.BranchFollowUp);
    }

    [Fact]
    public void Behavioral_preview_before_after_hashes_are_runtime_state_hashes()
    {
        var fixture = PreviewFixture.Create(GeneratedCampaignBranchKind.SUPPORT);
        Assert.NotEqual(fixture.Option.BeforeStateHash, fixture.Option.AfterStateHash);
    }
}

internal sealed record PreviewFixture(
    LLMGameCreator.GamePackage.GamePackageDefinition Package,
    IUnifiedGameRuntimeService Runtime,
    GeneratedCampaignChoiceBinding Binding,
    GeneratedCampaignChoiceBranch Branch,
    UnifiedRuntimeResult Opened,
    GeneratedCampaignDialogueChoicePreview Preview,
    GeneratedCampaignChoiceOption Option,
    IReadOnlyList<string> InitialChoiceIds,
    string SessionBeforePreview,
    string PackageBeforePreview)
{
    public static PreviewFixture Create(
        GeneratedCampaignBranchKind kind,
        Action<LLMGameCreator.GamePackage.GamePackageDefinition>? mutate = null)
    {
        var build = Goal164TestKit.AllSelectable;
        var package = Goal164TestKit.Clone(build.Package);
        mutate?.Invoke(package);
        var choices = Assert.IsType<GameProjectGeneratedCampaignChoiceSummary>(build.Build.GeneratedCampaignChoices);
        var binding = choices.Overlay!.Bindings.First(item => item.Branches.Any(branch => branch.Kind == kind));
        var branch = binding.Branches.Single(item => item.Kind == kind);
        var runtime = build.Runtime;
        var started = runtime.Start(package);
        Assert.True(started.Success);
        var opened = runtime.ExecuteGameplayCommand(package, started.Session,
            GameRuntimeCommand.OpenDialogue(binding.DialogueId));
        Assert.True(opened.Success);
        var initialIds = opened.GameplayEvents.Single(item => item.Type == GameRuntimeEventType.DialogueOpened)
            .Args["choiceIds"].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        var sessionBefore = Goal164TestKit.Canonical(opened.Session);
        var packageBefore = Goal164TestKit.Canonical(package);
        var preview = Assert.IsType<GeneratedCampaignDialogueChoicePreview>(
            new GeneratedCampaignDialogueChoicePreviewService(runtime).Preview(package, opened.Session));
        var option = preview.Options.Single(item => item.TechnicalChoiceId == branch.ChoiceId);
        return new PreviewFixture(package, runtime, binding, branch, opened, preview, option, initialIds,
            sessionBefore, packageBefore);
    }
}

internal sealed record ExecutedFixture(
    LLMGameCreator.GamePackage.GamePackageDefinition Package,
    IUnifiedGameRuntimeService Runtime,
    GeneratedCampaignChoiceBinding Binding,
    GeneratedCampaignChoiceBranch Branch,
    UnifiedRuntimeSession Before,
    UnifiedRuntimeResult Result,
    GeneratedCampaignActionOutcome Outcome)
{
    public static ExecutedFixture Create(GeneratedCampaignBranchKind kind)
    {
        var preview = PreviewFixture.Create(kind);
        var before = Goal164TestKit.Clone(preview.Opened.Session);
        var result = preview.Runtime.ExecuteGameplayCommand(preview.Package, preview.Opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(preview.Branch.ChoiceId));
        Assert.True(result.Success, string.Join(",", result.Diagnostics.Select(item => item.Code)));
        var outcome = new GeneratedCampaignConsequenceProjector().ProjectAction(
            preview.Package,
            before,
            result.Session,
            [],
            result.GameplayEvents,
            new GeneratedCampaignAction
            {
                Kind = GeneratedCampaignActionKind.ChooseDialogue,
                Title = preview.Option.Title,
                TechnicalChoiceId = preview.Branch.ChoiceId
            },
            [],
            [],
            true,
            []);
        return new ExecutedFixture(preview.Package, preview.Runtime, preview.Binding, preview.Branch,
            before, result, outcome);
    }
}
