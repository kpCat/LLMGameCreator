using LLMGameCreator.Application.Design.Semantics;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Application.Semantics;

public sealed class SemanticGuidedCompositionAcceptanceTests
{
    [Fact]
    public async Task BuildsStableAcceptedSemanticGuidedCompositionArtifacts()
    {
        using var temp = new TempDirectory();
        var service = CreateService();

        var first = service.Build(temp.Path);
        var second = service.Build(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted);
        Assert.Equal("semantic_guided_composition_artifact_verification", first.Report.ManualGate);
        Assert.True(first.Report.Goal004RuntimeEvidencePreserved);
        Assert.False(first.Report.SemanticSelectedIdsExecutedInRuntime);
        Assert.Equal("semantic_selection_is_generator_level_goal004_runtime_evidence_is_independent_regression", first.Report.RuntimeEvidenceSource);
        Assert.True(first.Report.ExpectedValidScenariosAccepted);
        Assert.True(first.Report.ExpectedInvalidScenariosRejectedByErrors);
        Assert.True(first.Report.RepeatedRunStable);
        Assert.True(first.Report.MultiSeedNoDanglingReferences);
        Assert.True(first.Report.MeaningfulValidVariantCount >= 3);
        Assert.False(first.Report.ExternalExecution.LlmExecuted);
        Assert.False(first.Report.ExternalExecution.RagExecuted);
        Assert.False(first.Report.ExternalExecution.ProviderExecuted);
        Assert.False(first.Report.ExternalExecution.LuaExecuted);
        Assert.False(first.Report.ExternalExecution.UnityExecuted);
        Assert.False(first.Report.ExternalExecution.MediaExecuted);

        var validSelections = first.Report.Scenarios
            .Where(item => item.ExpectedValid)
            .Select(item => (item.SelectedQuestPatternId, item.SelectedDialogueIntentId, item.SelectedInteractionPatternId))
            .Distinct()
            .ToList();
        Assert.True(validSelections.Count >= 3);

        var overlay = first.Report.Scenarios.Single(item => item.ScenarioId == "core_genre_project_overlay");
        Assert.Equal("quest_pattern/two_step_sequence", overlay.SelectedQuestPatternId);
        Assert.Equal("dialogue/completion_response/default", overlay.SelectedDialogueIntentId);
        Assert.Equal("interaction/use_reward_on_contact", overlay.SelectedInteractionPatternId);

        var candidate = first.Report.Scenarios.Single(item => item.ScenarioId == "candidate_quarantine");
        Assert.True(candidate.Accepted);
        Assert.False(candidate.CandidateLeakageDetected);
        Assert.True(candidate.QuarantinedTermCount >= 1);

        var invalid = first.Report.Scenarios.Single(item => item.ScenarioId == "invalid_conflict_rejection");
        Assert.False(invalid.Accepted);
        Assert.Contains(invalid.Diagnostics, item => item.Code == "semantic_guided.excludes_conflict");

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.Contains("\"accepted\": true", await File.ReadAllTextAsync(write.ReportJsonPath));
    }

    [Fact]
    public void ExpectedInvalidScenarioWithoutRealErrorFailsOverallMatrix()
    {
        using var temp = new TempDirectory();
        WriteReferencePackSet(temp.Path, ConflictKind.None);
        var service = CreateService();

        var result = service.Build(temp.Path, temp.Path);
        var invalid = result.Report.Scenarios.Single(item => item.ScenarioId == "invalid_conflict_rejection");

        Assert.False(result.Report.Accepted);
        Assert.True(invalid.Accepted);
        Assert.False(invalid.ExpectationMatched);
        Assert.False(result.Report.ExpectedInvalidScenariosRejectedByErrors);
        Assert.DoesNotContain(invalid.Diagnostics, item => item.Severity == SemanticDiagnosticSeverity.Error);
    }

    [Fact]
    public void ExcludesConflictMakesActualScenarioAcceptanceFalse()
    {
        using var temp = new TempDirectory();
        WriteReferencePackSet(temp.Path, ConflictKind.Excludes);
        var service = CreateService();

        var result = service.Build(temp.Path, temp.Path);
        var invalid = result.Report.Scenarios.Single(item => item.ScenarioId == "invalid_conflict_rejection");

        Assert.False(invalid.Accepted);
        Assert.True(invalid.ExpectationMatched);
        Assert.Contains(invalid.Diagnostics, item => item.Code == "semantic_guided.excludes_conflict");
    }

    [Fact]
    public void ForbiddenToneConflictMakesActualScenarioAcceptanceFalse()
    {
        using var temp = new TempDirectory();
        WriteReferencePackSet(temp.Path, ConflictKind.ForbiddenInTone);
        var service = CreateService();

        var result = service.Build(temp.Path, temp.Path);
        var invalid = result.Report.Scenarios.Single(item => item.ScenarioId == "invalid_conflict_rejection");

        Assert.False(invalid.Accepted);
        Assert.True(invalid.ExpectationMatched);
        Assert.Contains(invalid.Diagnostics, item => item.Code == "semantic_guided.forbidden_tone_conflict");
    }

    [Fact]
    public void RequiresWithoutActiveTargetMakesActualScenarioAcceptanceFalse()
    {
        using var temp = new TempDirectory();
        WriteReferencePackSet(temp.Path, ConflictKind.RequiresExternal);
        var service = CreateService();

        var result = service.Build(temp.Path, temp.Path);
        var invalid = result.Report.Scenarios.Single(item => item.ScenarioId == "invalid_conflict_rejection");

        Assert.False(invalid.Accepted);
        Assert.True(invalid.ExpectationMatched);
        Assert.Contains(invalid.Diagnostics, item => item.Code == "semantic_guided.requires_unsatisfied");
    }

    [Fact]
    public void CompatibleAndPreferenceRelationsDoNotCreateFalseCompositionErrors()
    {
        using var temp = new TempDirectory();
        WriteReferencePackSet(temp.Path, ConflictKind.None, includeCompatibleRelation: true);
        var service = CreateService();

        var result = service.Build(temp.Path, temp.Path);
        var valid = result.Report.Scenarios.Single(item => item.ScenarioId == "core_plus_wildland_frontier");

        Assert.True(valid.Accepted);
        Assert.DoesNotContain(valid.Diagnostics, item => item.Severity == SemanticDiagnosticSeverity.Error);
    }

    [Fact]
    public void MalformedReferencePackProducesReportDiagnosticInsteadOfThrowing()
    {
        using var temp = new TempDirectory();
        WriteReferencePackSet(temp.Path, ConflictKind.Excludes);
        File.WriteAllText(Path.Combine(temp.Path, "malformed.semantic-pack.json"), "{ nope");
        var service = CreateService();

        var result = service.Build(temp.Path, temp.Path);

        Assert.False(result.Report.Accepted);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "semantic_layer.pack_json_malformed");
        Assert.Contains(result.Report.Scenarios, item => item.ScenarioId == "core_plus_wildland_frontier");
    }

    private static SemanticGuidedCompositionAcceptanceService CreateService()
    {
        var serializer = new RuntimeStateSerializer();
        var goal004 = new QuestDialogInteractionFamilyAcceptanceService(
            visiblePreviewService: new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()),
            runtimeBackedStateAcceptanceService: new RuntimeBackedMicrogameStateAcceptanceService(
                serializer,
                new RuntimeSnapshotStore(serializer)));

        return new SemanticGuidedCompositionAcceptanceService(goal004Service: goal004);
    }

    private static void WriteReferencePackSet(
        string root,
        ConflictKind conflictKind,
        bool includeCompatibleRelation = false)
    {
        var packs = new[]
        {
            Pack(
                "core/base",
                SemanticLayerKinds.Core,
                [
                    Term("tone/hopeful", "tone", "Hopeful"),
                    Term("tone/tense", "tone", "Tense"),
                    Term("item_affordance/quest_item", "item_affordance", "Quest item"),
                    Term("entity_role/contact", "entity_role", "Contact")
                ],
                includeCompatibleRelation
                    ? [Relation("relation/core/quest_item/compatible/contact", "item_affordance/quest_item", SemanticRelationKinds.CompatibleWith, "entity_role/contact")]
                    : []),
            Pack(
                "genre/wildland_frontier",
                SemanticLayerKinds.Genre,
                [
                    Term("quest_motif/recover_cache", "quest_motif", "Recover cache"),
                    Term("tone/urgent", "tone", "Urgent"),
                    Term("location_mood/overgrown_ruins", "location_mood", "Overgrown ruins")
                ],
                [
                    Relation("relation/wildland/recover_cache/prefers/recover_from_encounter", "quest_motif/recover_cache", SemanticRelationKinds.PrefersQuestPattern, "quest_pattern/recover_from_encounter"),
                    Relation("relation/wildland/urgent/prefers/warn", "tone/urgent", SemanticRelationKinds.PrefersDialogueIntent, "dialogue/warn_threaten/default"),
                    Relation("relation/wildland/ruins/prefers/resolve_challenge", "location_mood/overgrown_ruins", SemanticRelationKinds.PrefersInteractionFamily, "interaction/resolve_challenge")
                ]),
            Pack(
                "genre/gothic_mystery",
                SemanticLayerKinds.Genre,
                [
                    Term("quest_motif/investigate_marker", "quest_motif", "Investigate marker"),
                    Term("tone/bleak", "tone", "Bleak"),
                    Term("location_mood/haunted_marker", "location_mood", "Haunted marker")
                ],
                [
                    Relation("relation/gothic/investigate_marker/prefers/interact_object", "quest_motif/investigate_marker", SemanticRelationKinds.PrefersQuestPattern, "quest_pattern/interact_with_object"),
                    Relation("relation/gothic/bleak/prefers/ask_about_quest", "tone/bleak", SemanticRelationKinds.PrefersDialogueIntent, "dialogue/ask_about_quest/default"),
                    Relation("relation/gothic/haunted_marker/prefers/inspect_marker", "location_mood/haunted_marker", SemanticRelationKinds.PrefersInteractionFamily, "interaction/inspect_marker")
                ]),
            Pack(
                "genre/trade_caravan",
                SemanticLayerKinds.Genre,
                [
                    Term("quest_motif/deliver_token", "quest_motif", "Deliver token"),
                    Term("tone/pragmatic", "tone", "Pragmatic"),
                    Term("entity_role/broker", "entity_role", "Broker")
                ],
                [
                    Relation("relation/trade/deliver_token/prefers/deliver_reward", "quest_motif/deliver_token", SemanticRelationKinds.PrefersQuestPattern, "quest_pattern/deliver_reward_token"),
                    Relation("relation/trade/pragmatic/prefers/bargain", "tone/pragmatic", SemanticRelationKinds.PrefersDialogueIntent, "dialogue/bargain_reward/default"),
                    Relation("relation/trade/broker/prefers/talk_contact", "entity_role/broker", SemanticRelationKinds.PrefersInteractionFamily, "interaction/talk_contact")
                ]),
            Pack(
                "project/sky_lantern_outpost",
                SemanticLayerKinds.Project,
                [
                    Term("quest_motif/recover_cache", "quest_motif", "Recover lantern cache"),
                    Term("tone/ceremonial", "tone", "Ceremonial")
                ],
                [
                    Relation("relation/project/lantern_cache/prefers/two_step_sequence", "quest_motif/recover_cache", SemanticRelationKinds.PrefersQuestPattern, "quest_pattern/two_step_sequence"),
                    Relation("relation/project/ceremonial/prefers/completion_response", "tone/ceremonial", SemanticRelationKinds.PrefersDialogueIntent, "dialogue/completion_response/default"),
                    Relation("relation/project/lantern_cache/prefers/use_reward", "quest_motif/recover_cache", SemanticRelationKinds.PrefersInteractionFamily, "interaction/use_reward_on_contact")
                ]),
            Pack(
                "imported_candidate/rumor_candidates",
                SemanticLayerKinds.ImportedCandidate,
                [Term("quest_motif/whispered_prophecy", "quest_motif", "Whispered prophecy", SemanticTermStatuses.Candidate)],
                [Relation("relation/candidate/prophecy/prefers/fetch_cache", "quest_motif/whispered_prophecy", SemanticRelationKinds.PrefersQuestPattern, "quest_pattern/fetch_item_cache", SemanticTermStatuses.Candidate)]),
            Pack(
                "llm_candidate/unused_suggestions",
                SemanticLayerKinds.LlmCandidate,
                [Term("item_affordance/dream_key", "item_affordance", "Dream key", SemanticTermStatuses.Candidate)],
                [Relation("relation/llm_candidate/dream_key/prefers/use_reward", "item_affordance/dream_key", SemanticRelationKinds.PrefersInteractionFamily, "interaction/use_reward_on_contact", SemanticTermStatuses.Candidate)]),
            Pack(
                "project/conflicting_overlay",
                SemanticLayerKinds.Project,
                [Term("tone/hopeful", "tone", "Hopeful")],
                BuildConflictRelations(conflictKind))
        };

        foreach (var pack in packs)
        {
            var fileName = pack.LayerId.Replace('/', '-') + ".semantic-pack.json";
            File.WriteAllText(Path.Combine(root, fileName), JsonSerializer.Serialize(pack, JsonOptions));
        }
    }

    private static IReadOnlyList<SemanticLayerRelationDeclaration> BuildConflictRelations(ConflictKind kind) =>
        kind switch
        {
            ConflictKind.Excludes =>
            [
                Relation("relation/conflict/hopeful/excludes/bleak", "tone/hopeful", SemanticRelationKinds.Excludes, "tone/bleak")
            ],
            ConflictKind.ForbiddenInTone =>
            [
                Relation("relation/conflict/hopeful/forbidden/bleak", "tone/hopeful", SemanticRelationKinds.ForbiddenInTone, "tone/bleak")
            ],
            ConflictKind.RequiresExternal =>
            [
                Relation("relation/conflict/hopeful/requires/external", "tone/hopeful", SemanticRelationKinds.Requires, "quest_pattern/fetch_item_cache")
            ],
            _ => []
        };

    private static SemanticLayerPack Pack(
        string layerId,
        string layerKind,
        IReadOnlyList<SemanticLayerTermDeclaration> terms,
        IReadOnlyList<SemanticLayerRelationDeclaration> relations) => new()
        {
            LayerId = layerId,
            LayerKind = layerKind,
            Source = "unit-test",
            Terms = terms,
            Relations = relations
        };

    private static SemanticLayerTermDeclaration Term(
        string termId,
        string kind,
        string label,
        string status = SemanticTermStatuses.Known) => new()
        {
            TermId = termId,
            Kind = kind,
            Label = label,
            Status = status
        };

    private static SemanticLayerRelationDeclaration Relation(
        string relationId,
        string source,
        string kind,
        string target,
        string status = SemanticTermStatuses.Known) => new()
        {
            RelationId = relationId,
            SourceTermId = source,
            RelationKind = kind,
            TargetTermId = target,
            Status = status
        };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private enum ConflictKind
    {
        None,
        Excludes,
        ForbiddenInTone,
        RequiresExternal
    }

    private sealed class DefaultRuntimeAdapter : IVisibleGeneratedPlayableRuntimeAdapter
    {
        public VisibleGeneratedPlayableRuntimeAttempt Run(GamePackageDefinition package)
        {
            var runtime = new DefaultGameRuntime();
            var start = runtime.Start(package);
            var eventTypes = new SortedSet<string>(start.Events.Select(item => item.Type.ToString()), StringComparer.Ordinal);
            var commandAttempts = new List<VisibleGeneratedPlayableRuntimeCommandAttempt>();
            var currentState = start.State;

            if (start.Success)
            {
                var move = runtime.Execute(package, currentState, PlayerCommand.Move(Direction2D.Right));
                currentState = move.State;
                commandAttempts.Add(ToAttempt("01_move_right", "move/right", move));
                foreach (var eventType in move.Events.Select(item => item.Type.ToString()))
                {
                    eventTypes.Add(eventType);
                }

                var interact = runtime.Execute(package, currentState, PlayerCommand.Interact());
                currentState = interact.State;
                commandAttempts.Add(ToAttempt("02_interact", "interact", interact));
                foreach (var eventType in interact.Events.Select(item => item.Type.ToString()))
                {
                    eventTypes.Add(eventType);
                }
            }

            return new VisibleGeneratedPlayableRuntimeAttempt
            {
                RuntimeStartAttempted = true,
                RuntimeStartSucceeded = start.Success,
                StartMapId = package.Manifest.StartMapId,
                CurrentMapId = currentState.CurrentMapId,
                PlayerStartPosition = new VisibleGeneratedPlayablePosition
                {
                    X = start.State.PlayerPosition.X,
                    Y = start.State.PlayerPosition.Y
                },
                PlayerCurrentPosition = new VisibleGeneratedPlayablePosition
                {
                    X = currentState.PlayerPosition.X,
                    Y = currentState.PlayerPosition.Y
                },
                CommandAttempts = commandAttempts,
                EventTypes = eventTypes.ToList()
            };
        }

        private static VisibleGeneratedPlayableRuntimeCommandAttempt ToAttempt(
            string commandId,
            string commandType,
            CommandResult result) => new()
        {
            CommandId = commandId,
            CommandType = commandType,
            Succeeded = result.Success,
            CurrentMapId = result.State.CurrentMapId,
            PlayerPosition = new VisibleGeneratedPlayablePosition
            {
                X = result.State.PlayerPosition.X,
                Y = result.State.PlayerPosition.Y
            },
            EventTypes = result.Events.Select(item => item.Type.ToString()).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            EventTargets = result.Events.Select(item => item.TargetId ?? string.Empty).Where(value => !string.IsNullOrWhiteSpace(value)).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            EventMessages = result.Events.Select(item => item.Message).Where(value => !string.IsNullOrWhiteSpace(value)).OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
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
