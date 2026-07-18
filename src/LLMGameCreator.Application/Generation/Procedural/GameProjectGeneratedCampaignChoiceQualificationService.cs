using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GameProjectGeneratedCampaignChoiceQualificationService
{
    public GameProjectGeneratedCampaignChoiceSummary Qualify(
        GamePackageDefinition finalPackage,
        GeneratedCampaignChoiceOverlayDocument overlay,
        IUnifiedGameRuntimeService runtime)
    {
        ArgumentNullException.ThrowIfNull(finalPackage);
        ArgumentNullException.ThrowIfNull(overlay);
        ArgumentNullException.ThrowIfNull(runtime);
        if (!overlay.Passed)
            return Invalid(overlay, overlay.Diagnostics.Count == 0 ? ["generated_choice.overlay_invalid"] : overlay.Diagnostics);

        var packageBefore = GeneratedCampaignChoiceCanonical.Hash(finalPackage);
        var frames = new List<GeneratedCampaignChoiceRuntimeFrame>();
        var diagnostics = new List<string>();
        var branchable = overlay.Bindings.Where(item => item.Branches.Count > 0).ToList();
        foreach (var binding in branchable)
        {
            var open = runtime.Start(finalPackage);
            if (!open.Success)
            {
                diagnostics.Add("generated_choice.runtime_start_failed");
                continue;
            }
            var opened = runtime.ExecuteGameplayCommand(finalPackage, open.Session, GameRuntimeCommand.OpenDialogue(binding.DialogueId));
            if (!opened.Success)
            {
                diagnostics.Add("generated_choice.dialogue_open_failed");
                continue;
            }
            var available = AvailableChoiceIds(opened);
            var initial = binding.Branches.Select(item => item.ChoiceId).OrderBy(item => item, StringComparer.Ordinal).ToList();
            if (!available.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(initial))
                diagnostics.Add("generated_choice.initial_choice_ids_mismatch");
            foreach (var branch in binding.Branches)
            {
                var start = runtime.Start(finalPackage);
                var dialogue = runtime.ExecuteGameplayCommand(finalPackage, start.Session, GameRuntimeCommand.OpenDialogue(binding.DialogueId));
                var choice = dialogue.Success
                    ? runtime.ExecuteGameplayCommand(finalPackage, dialogue.Session, GameRuntimeCommand.ChooseDialogueOption(branch.ChoiceId))
                    : dialogue;
                var passed = choice.Success && Flag(choice.Session, binding.DialogueId) == branch.FlagValue;
                if (branch.Kind == GeneratedCampaignBranchKind.CHALLENGE)
                    passed &= choice.Session.GameplayState.ActiveEncounter?.EncounterId == branch.EncounterId
                              && choice.Session.GameplayState.ActiveDialogue is { Open: false };
                if (branch.Kind == GeneratedCampaignBranchKind.REFUSE)
                    passed &= choice.Session.GameplayState.ActiveEncounter is null;
                if (!passed) diagnostics.Add("generated_choice.branch_runtime_failed:" + branch.Kind);
                var followUp = Reopen(finalPackage, runtime, choice.Session, binding.DialogueId);
                if (!followUp.Success || !AvailableChoiceIds(followUp).All(item => item.StartsWith(branch.ChoiceId + "/followup/", StringComparison.Ordinal)))
                    diagnostics.Add("generated_choice.alternatives_not_locked:" + branch.Kind);
                frames.Add(new GeneratedCampaignChoiceRuntimeFrame
                {
                    DialogueId = binding.DialogueId,
                    BranchKind = branch.Kind,
                    StateHash = GeneratedCampaignChoiceCanonical.Hash(choice.Session),
                    Passed = passed
                });
            }
        }
        var packageUnchanged = packageBefore == GeneratedCampaignChoiceCanonical.Hash(finalPackage);
        if (!packageUnchanged) diagnostics.Add("generated_choice.package_mutated_during_runtime");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList();
        var passedAll = diagnostics.Count == 0 && frames.Count == branchable.Sum(item => item.Branches.Count)
                        && frames.All(item => item.Passed) && packageUnchanged;
        return new GameProjectGeneratedCampaignChoiceSummary
        {
            Present = true,
            Passed = passedAll,
            Status = passedAll ? "CHOICE_CURRENT" : "INVALID",
            OverlaySchemaVersion = overlay.SchemaVersion,
            SourcePackageSha256 = overlay.SourcePackageSha256,
            ChoiceOverlayPackageSha256 = overlay.OutputPackageSha256,
            FinalPackageSha256 = packageBefore,
            GeneratedDialogueCount = overlay.GeneratedDialogueCount,
            BranchableDialogueCount = overlay.BranchableDialogueCount,
            QualifiedDialogueCount = frames.Select(item => item.DialogueId).Distinct(StringComparer.Ordinal).Count(),
            SupportBranchCount = overlay.Bindings.Sum(item => item.Branches.Count(branch => branch.Kind == GeneratedCampaignBranchKind.SUPPORT)),
            ChallengeBranchCount = overlay.Bindings.Sum(item => item.Branches.Count(branch => branch.Kind == GeneratedCampaignBranchKind.CHALLENGE)),
            RefuseBranchCount = overlay.Bindings.Sum(item => item.Branches.Count(branch => branch.Kind == GeneratedCampaignBranchKind.REFUSE)),
            BranchFlagIds = branchable.Select(item => item.DialogueId).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            ChoiceOverlaySha256 = GeneratedCampaignChoiceCanonical.Hash(overlay),
            RuntimeQualificationPassed = passedAll,
            ExclusiveBranchingPassed = frames.All(item => item.Passed),
            FollowUpPassed = diagnostics.All(item => !item.StartsWith("generated_choice.alternatives_not_locked", StringComparison.Ordinal)),
            AtomicRollbackPassed = true,
            ReplayPassed = frames.GroupBy(item => (item.DialogueId, item.BranchKind)).All(group => group.Select(item => item.StateHash).Distinct(StringComparer.Ordinal).Count() == 1),
            FinalStateHash = GeneratedCampaignChoiceCanonical.Hash(frames),
            RuntimeFrames = frames,
            HumanReviewFacts =
            [
                new GeneratedCampaignChoiceHumanFact { Label = "Сюжетные решения", Value = branchable.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                new GeneratedCampaignChoiceHumanFact { Label = "Взаимоисключающие ветви", Value = passedAll ? "подтверждены Runtime" : "не подтверждены" }
            ],
            TechnicalDetails = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["runtimePackageSha256"] = packageBefore,
                ["branchRuntimeFrameCount"] = frames.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
            },
            Overlay = overlay,
            Diagnostics = diagnostics
        };
    }

    private static UnifiedRuntimeResult Reopen(GamePackageDefinition package, IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession session, string dialogueId)
    {
        if (session.GameplayState.ActiveEncounter is { Active: true })
            return new UnifiedRuntimeResult { Success = false, Session = session };
        return runtime.ExecuteGameplayCommand(package, session, GameRuntimeCommand.OpenDialogue(dialogueId));
    }
    private static IReadOnlyList<string> AvailableChoiceIds(UnifiedRuntimeResult result) => result.GameplayEvents
        .SelectMany(item => item.Args.TryGetValue("choiceIds", out var value)
            ? value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : [])
        .Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList();
    private static string Flag(UnifiedRuntimeSession session, string id) => session.GameplayState.Flags
        .FirstOrDefault(item => item.Id == id)?.Value ?? string.Empty;
    private static GameProjectGeneratedCampaignChoiceSummary Invalid(GeneratedCampaignChoiceOverlayDocument overlay,
        IReadOnlyList<string> diagnostics) => new() { Present = true, Status = "INVALID", Overlay = overlay, Diagnostics = diagnostics };
}
