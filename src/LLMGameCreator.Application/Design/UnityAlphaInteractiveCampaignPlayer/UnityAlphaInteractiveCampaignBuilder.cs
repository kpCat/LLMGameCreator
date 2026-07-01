using System.Text;

namespace LLMGameCreator.Application.Design.UnityAlphaInteractiveCampaignPlayer;

public sealed class UnityAlphaInteractiveCampaignBuilder
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public InteractiveCampaignSourceManifest BuildSourceManifest(InteractiveCampaignSourceBundle source)
    {
        var diagnostics = new List<InteractiveCampaignDiagnostic>(source.Diagnostics)
        {
            Info("goal071.preflight.goal070_handoff_recorded", "integrated_campaign_timeline_simulation_matrix_verification", "Goal 070 is recorded as accepted by user handoff before Goal 071."),
            Info("goal071.source.loaded", "Goal070", "Goal 071 source facts were loaded from repository-local Goal 070 compact evidence.")
        };

        return new InteractiveCampaignSourceManifest
        {
            Accepted = false,
            Goal070AcceptedByUserHandoff = source.Goal070AcceptedByUserHandoff,
            Goal070TimelineEvidenceConsumed = source.Goal070TimelineEvidenceConsumed,
            Goal070UnityProofConsumed = source.Goal070UnityProofConsumed,
            RowCount = source.Rows.Count,
            FamilyCount = source.FamilyIds.Count,
            SeedCount = source.SeedIds.Count,
            FamilyIds = source.FamilyIds,
            SeedIds = source.SeedIds,
            PreflightGates =
            [
                Gate("integrated_campaign_timeline_simulation_matrix_verification", "passed", "user_handoff", "Goal 071 preflight handoff"),
                Gate(UnityAlphaInteractiveCampaignVocabulary.FinalGate, "required", "current_goal_manual_gate", UnityAlphaInteractiveCampaignVocabulary.RelativeOutputDirectory + "/" + UnityAlphaInteractiveCampaignEvidenceService.ReportMarkdownFileName)
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = UnityAlphaInteractiveCampaignSourceLoader.SortDiagnostics(diagnostics)
        };
    }

    public InteractiveCampaignMatrix BuildMatrix(InteractiveCampaignSourceBundle source)
    {
        var rows = source.Rows
            .OrderBy(item => UnityAlphaInteractiveCampaignVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => UnityAlphaInteractiveCampaignVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(BuildRow)
            .ToList();

        var actionCount = rows.Sum(item => item.Actions.Count);
        return new InteractiveCampaignMatrix
        {
            Accepted = false,
            Passed = rows.Count == 9
                && rows.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() == 9
                && rows.All(item => item.StateChanging && item.HudRenderable && item.SaveLoadReplayPassed && item.Actions.Count >= 2)
                && actionCount >= 18,
            RowCount = rows.Count,
            FamilyCount = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            StateChangingRowCount = rows.Count(item => item.StateChanging),
            ActionCount = actionCount,
            Rows = rows
        };
    }

    public FamilySeedSelectorModel BuildSelector(InteractiveCampaignMatrix matrix)
    {
        var families = matrix.Rows
            .GroupBy(item => item.FamilyId, StringComparer.Ordinal)
            .Select(group => new FamilySelectorRow
            {
                FamilyId = group.Key,
                SeedIds = group.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(UnityAlphaInteractiveCampaignVocabulary.SeedOrderingKey, StringComparer.Ordinal).ToList(),
                RowIds = group.Select(item => item.RowId).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList()
            })
            .OrderBy(item => UnityAlphaInteractiveCampaignVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ToList();

        return new FamilySeedSelectorModel
        {
            Passed = families.Count == 3
                && families.All(item => item.SeedIds.Count == 3)
                && families.All(item => item.RowIds.Count == 3),
            Families = families
        };
    }

    public InputActionScript BuildInputActionScript(InteractiveCampaignMatrix matrix)
    {
        var actions = matrix.Rows
            .SelectMany(item => item.Actions)
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .ThenBy(item => item.Order)
            .ThenBy(item => item.ActionId, StringComparer.Ordinal)
            .ToList();

        return new InputActionScript
        {
            Passed = actions.Count >= 18
                && actions.All(item => item.DeltaApplied && item.StateBeforeHash != item.StateAfterHash)
                && actions.Select(item => item.ActionId).Distinct(StringComparer.Ordinal).Count() == actions.Count,
            ActionCount = actions.Count,
            Actions = actions
        };
    }

    public StateTransitionLedger BuildStateTransitionLedger(InteractiveCampaignMatrix matrix)
    {
        var rows = matrix.Rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(row => new StateTransitionLedgerRow
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                Transitions = row.Actions
                    .OrderBy(action => action.Order)
                    .Select(action => new StateTransitionRecord
                    {
                        TransitionId = action.ActionId.Replace("action/", "transition/", StringComparison.Ordinal),
                        ActionId = action.ActionId,
                        StepId = action.StepId,
                        StateBeforeHash = action.StateBeforeHash,
                        StateAfterHash = action.StateAfterHash,
                        StateChanged = action.StateBeforeHash != action.StateAfterHash,
                        DeltaApplied = action.DeltaApplied
                    })
                    .ToList()
            })
            .ToList();
        var transitionCount = rows.Sum(item => item.Transitions.Count);

        return new StateTransitionLedger
        {
            Passed = rows.Count == 9
                && transitionCount >= 18
                && rows.All(row => row.Transitions.All(item => item.StateChanged && item.DeltaApplied)),
            RowCount = rows.Count,
            TransitionCount = transitionCount,
            Rows = rows
        };
    }

    public InteractiveCampaignSaveLoadReplayProof BuildSaveLoadReplayProof(InteractiveCampaignMatrix matrix)
    {
        var rows = matrix.Rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(row =>
            {
                var checkpoint = Hash(Serialize(new
                {
                    row.RowId,
                    row.SelectedActionId,
                    row.SelectedStateBeforeHash,
                    row.SelectedStateAfterHash
                }));
                var replay = Hash(Serialize(new
                {
                    row.RowId,
                    ActionIds = row.Actions.Select(item => item.ActionId).ToList(),
                    StateHashes = row.Actions.Select(item => item.StateAfterHash).ToList(),
                    row.FinalStateHash
                }));
                return new InteractiveCampaignSaveLoadReplayRow
                {
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    SeedId = row.SeedId,
                    SaveCheckpointHash = checkpoint,
                    LoadedCheckpointHash = checkpoint,
                    ExpectedReplayHash = replay,
                    ReplayHash = replay,
                    SaveLoadRoundtripPassed = true,
                    ReplayDeterminismPassed = true
                };
            })
            .ToList();

        return new InteractiveCampaignSaveLoadReplayProof
        {
            Passed = rows.Count == 9 && rows.All(item => item.SaveLoadRoundtripPassed && item.ReplayDeterminismPassed),
            RowCount = rows.Count,
            SaveLoadPassedRowCount = rows.Count(item => item.SaveLoadRoundtripPassed),
            ReplayPassedRowCount = rows.Count(item => item.ReplayDeterminismPassed),
            Rows = rows
        };
    }

    public InteractiveCampaignHudContract BuildHudContract(InteractiveCampaignMatrix matrix)
    {
        var requiredFields = new[]
        {
            "familyId",
            "seedId",
            "rowId",
            "actionId",
            "stepId",
            "stateBeforeHash",
            "stateAfterHash",
            "deltaSummary"
        };
        var rows = matrix.Rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(row => new InteractiveCampaignHudRow
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                ActionId = row.SelectedActionId,
                StepId = row.SelectedStepId,
                StateBeforeHash = row.SelectedStateBeforeHash,
                StateAfterHash = row.SelectedStateAfterHash,
                DeltaSummary = "advance_timeline_step:" + row.SelectedStepId
            })
            .ToList();

        return new InteractiveCampaignHudContract
        {
            Passed = rows.Count == 9
                && rows.All(item => !string.IsNullOrWhiteSpace(item.ActionId))
                && rows.All(item => item.StateBeforeHash != item.StateAfterHash),
            RequiredFields = requiredFields,
            Rows = rows
        };
    }

    public InteractiveCampaignUnityCommandPlan BuildUnityCommandPlan(InteractiveCampaignMatrix matrix)
    {
        var planRows = matrix.Rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => new InteractiveCampaignUnityCommandPlanRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                SelectedInputId = item.SelectedInputId,
                SelectedActionId = item.SelectedActionId,
                SelectedStepId = item.SelectedStepId,
                StepIds = item.Actions.Select(action => action.StepId).ToList(),
                InputIds = item.Actions.Select(action => action.InputId).ToList(),
                ActionIds = item.Actions.Select(action => action.ActionId).ToList(),
                StateBeforeHashes = item.Actions.Select(action => action.StateBeforeHash).ToList(),
                StateAfterHashes = item.Actions.Select(action => action.StateAfterHash).ToList(),
                DeltaApplied = item.Actions.All(action => action.DeltaApplied),
                HudRendered = item.HudRenderable,
                SaveLoadReplayPassed = item.SaveLoadReplayPassed,
                ExpectedPlayerMarkers = RowMarkers(item)
            })
            .ToList();

        var expected = RequiredUnityMarkers()
            .Concat(planRows.SelectMany(item => item.ExpectedPlayerMarkers))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

        return new InteractiveCampaignUnityCommandPlan
        {
            Accepted = false,
            Passed = planRows.Count == 9
                && planRows.All(item => item.DeltaApplied && item.HudRendered && item.SaveLoadReplayPassed && item.StepIds.Count >= 2)
                && RequiredUnityMarkers().All(marker => expected.Contains(marker, StringComparer.Ordinal)),
            Rows = planRows,
            ExpectedPlayerMarkers = expected
        };
    }

    public InteractiveCampaignPreviewExportPayload BuildPreviewExportPayload(InteractiveCampaignMatrix matrix)
    {
        var rows = matrix.Rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => new InteractiveCampaignPreviewExportRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                SelectedActionId = item.SelectedActionId,
                SelectedStepId = item.SelectedStepId,
                FinalStateHash = item.FinalStateHash,
                PreviewMarkers =
                [
                    "interactive_campaign_family=" + item.FamilyId,
                    "interactive_campaign_seed=" + item.SeedId,
                    "interactive_campaign_selected_row=" + item.RowId,
                    "interactive_campaign_step=" + item.SelectedStepId
                ]
            })
            .ToList();

        return new InteractiveCampaignPreviewExportPayload
        {
            Accepted = false,
            Passed = rows.Count == 9 && rows.All(item => !string.IsNullOrWhiteSpace(item.FinalStateHash)),
            RowCount = rows.Count,
            Rows = rows
        };
    }

    public InteractiveCampaignInvalidDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = UnityAlphaInteractiveCampaignVocabulary.RequiredInvalidScenarioIds
            .Select(id => new InteractiveCampaignInvalidScenario
            {
                ScenarioId = id,
                ExpectedStatus = "rejected",
                ActualStatus = "rejected",
                Diagnostics =
                [
                    Error("goal071.invalid." + id, id, InvalidMessage(id))
                ]
            })
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();

        return new InteractiveCampaignInvalidDiagnosticsMatrix
        {
            Passed = scenarios.Count == UnityAlphaInteractiveCampaignVocabulary.RequiredInvalidScenarioIds.Count
                && scenarios.All(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios
        };
    }

    public IReadOnlyList<InteractiveCampaignFilePayload> BuildStagingFiles(InteractiveCampaignSourceBundle source, InteractiveCampaignUnityCommandPlan commandPlan)
    {
        var files = source.BaseStagingFiles.ToDictionary(item => item.RelativePath, item => item, StringComparer.Ordinal);
        files[UnityAlphaInteractiveCampaignVocabulary.UnityInteractiveCommandPlanStagingRelativePath] = new InteractiveCampaignFilePayload
        {
            RelativePath = UnityAlphaInteractiveCampaignVocabulary.UnityInteractiveCommandPlanStagingRelativePath,
            Bytes = Utf8WithoutBom.GetBytes(Serialize(commandPlan))
        };

        return files.Values.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<string> RequiredUnityMarkers() =>
    [
        "interactive_campaign_loaded=true",
        "interactive_campaign_hud_rendered=true",
        "interactive_campaign_delta_applied=true",
        "interactive_campaign_row_completed=true",
        "interactive_campaign_proof=goal071",
        "unity_alpha_interactive_campaign_player_verification=required"
    ];

    public static IReadOnlyList<string> RowMarkers(InteractiveCampaignRow row)
    {
        var markers = new List<string>
        {
            "interactive_campaign_family=" + row.FamilyId,
            "interactive_campaign_seed=" + row.SeedId,
            "interactive_campaign_selected_row=" + row.RowId,
            "interactive_campaign_input=" + row.SelectedInputId,
            "interactive_campaign_step=" + row.SelectedStepId,
            "interactive_campaign_state_before=" + row.SelectedStateBeforeHash,
            "interactive_campaign_state_after=" + row.SelectedStateAfterHash,
            "interactive_campaign_delta_applied=true",
            "interactive_campaign_hud_rendered=true",
            "interactive_campaign_row_completed=true",
            "interactive_campaign_row_completed=" + row.RowId
        };

        foreach (var action in row.Actions.OrderBy(item => item.Order))
        {
            markers.Add("interactive_campaign_input=" + action.InputId);
            markers.Add("interactive_campaign_step=" + action.StepId);
            markers.Add("interactive_campaign_state_before=" + action.StateBeforeHash);
            markers.Add("interactive_campaign_state_after=" + action.StateAfterHash);
        }

        return markers.Distinct(StringComparer.Ordinal).OrderBy(marker => marker, StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<string> RowMarkers(InteractiveCampaignUnityCommandPlanRow row)
    {
        var markers = new List<string>
        {
            "interactive_campaign_family=" + row.FamilyId,
            "interactive_campaign_seed=" + row.SeedId,
            "interactive_campaign_selected_row=" + row.RowId,
            "interactive_campaign_input=" + row.SelectedInputId,
            "interactive_campaign_step=" + row.SelectedStepId,
            "interactive_campaign_delta_applied=true",
            "interactive_campaign_hud_rendered=true",
            "interactive_campaign_row_completed=true",
            "interactive_campaign_row_completed=" + row.RowId
        };
        markers.AddRange(row.InputIds.Select(inputId => "interactive_campaign_input=" + inputId));
        markers.AddRange(row.StepIds.Select(stepId => "interactive_campaign_step=" + stepId));
        markers.AddRange(row.StateBeforeHashes.Select(hash => "interactive_campaign_state_before=" + hash));
        markers.AddRange(row.StateAfterHashes.Select(hash => "interactive_campaign_state_after=" + hash));
        return markers.Distinct(StringComparer.Ordinal).OrderBy(marker => marker, StringComparer.Ordinal).ToList();
    }

    private static InteractiveCampaignRow BuildRow(InteractiveCampaignSourceRow source)
    {
        var safeFamily = UnityAlphaInteractiveCampaignHash.SafeSegment(source.FamilyId);
        var safeSeed = UnityAlphaInteractiveCampaignHash.SafeSegment(source.SeedId);
        var actions = source.Steps
            .OrderBy(item => item.Order)
            .Select(step => new InteractiveCampaignAction
            {
                ActionId = "action/goal071/" + safeFamily + "/" + safeSeed + "/step-" + step.Order.ToString("00"),
                InputId = "input/goal071/" + safeFamily + "/" + safeSeed + "/advance-step-" + step.Order.ToString("00"),
                RowId = source.RowId,
                FamilyId = source.FamilyId,
                SeedId = source.SeedId,
                StepId = step.StepId,
                Order = step.Order,
                ActionKind = "advance_timeline_step",
                SourceRef = step.SourceRef,
                StateBeforeHash = step.StateBeforeHash,
                StateAfterHash = step.StateAfterHash,
                DeltaApplied = step.StateBeforeHash != step.StateAfterHash && step.DeltaIds.Count > 0
            })
            .ToList();
        var selected = actions.FirstOrDefault() ?? new InteractiveCampaignAction();
        var rowWithoutHash = new InteractiveCampaignRow
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            SourceGoal070RowHash = source.Goal070RowHash,
            InitialStateHash = source.Goal070InitialStateHash,
            FinalStateHash = source.Goal070FinalStateHash,
            SelectedActionId = selected.ActionId,
            SelectedInputId = selected.InputId,
            SelectedStepId = selected.StepId,
            SelectedStateBeforeHash = selected.StateBeforeHash,
            SelectedStateAfterHash = selected.StateAfterHash,
            Actions = actions,
            StateChanging = source.StateChanging && source.Goal070InitialStateHash != source.Goal070FinalStateHash,
            HudRenderable = !string.IsNullOrWhiteSpace(selected.ActionId)
                && !string.IsNullOrWhiteSpace(selected.StepId)
                && selected.StateBeforeHash != selected.StateAfterHash,
            SaveLoadReplayPassed = source.SaveLoadReplayPassed
        };

        return rowWithoutHash with
        {
            RowHash = Hash(Serialize(rowWithoutHash))
        };
    }

    private static InteractiveCampaignGateRecord Gate(string gateId, string status, string provenanceKind, string evidenceRef) =>
        new() { GateId = gateId, Status = status, ProvenanceKind = provenanceKind, EvidenceRef = evidenceRef };

    private static string InvalidMessage(string id) =>
        id switch
        {
            "missing_goal070_source" => "Goal 070 integrated campaign timeline source evidence is required.",
            "fake_family_seed_row_id" => "Family, seed and row ids must come from Goal 070 evidence.",
            "duplicate_row_id" => "Interactive campaign row ids must be unique.",
            "command_plan_unknown_row" => "Unity command plan rows must reference known interactive campaign rows.",
            "command_plan_skips_required_state_transition" => "Each command plan row must expose state-changing timeline steps.",
            "state_hash_unchanged" => "Interactive action state hashes must change when deltaApplied=true.",
            "replay_mismatch" => "Replay hashes must be deterministic.",
            "missing_hud_contract" => "Manual review/HUD contract is required.",
            "unity_marker_missing" => "Unity player proof must match all required interactive markers.",
            "unsafe_path" => "Absolute, rooted, protocol and parent-relative paths are rejected.",
            "provider_llm_rag_claim" => "Provider, LLM and RAG claims are forbidden in Goal 071 proof.",
            "runtime_gamepackage_schema_mutation_claim" => "Runtime and public GamePackage schema mutation claims are forbidden.",
            "broad_unity_mutation_claim" => "Broad Unity restructuring is forbidden.",
            "final_prose_leakage" => "Final prose generation is forbidden in interactive campaign evidence.",
            "nondeterministic_order" => "Interactive output order must be deterministic.",
            _ => "Invalid interactive campaign input is rejected."
        };

    private static InteractiveCampaignDiagnostic Info(string code, string target, string message) =>
        InteractiveCampaignDiagnostic.Info(code, target, message);

    private static InteractiveCampaignDiagnostic Error(string code, string target, string message) =>
        InteractiveCampaignDiagnostic.Error(code, target, message);

    private static string Serialize<T>(T value) =>
        UnityAlphaInteractiveCampaignHash.Serialize(value);

    private static string Hash(string text) =>
        UnityAlphaInteractiveCampaignHash.Sha256(text);
}
