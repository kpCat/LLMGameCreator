using System.Text.Json;
using System.Text;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161;

[Collection(Goal160Collection.Name)]
public sealed class Goal161StandaloneAndPortabilityTests
{
    [Fact]
    public void Behavioral_migration_facts_append_to_standalone_human_review_request()
    {
        if (!Goal161StandaloneState.Enabled) return;
        var facts = Goal161StandaloneState.Value.PreStandaloneFacts;
        Assert.Contains(facts, item => item.Label == "Игровое сохранение" && item.Value == "перенесено");
        Assert.Contains(facts, item => item.Label == "Позиция" && item.Value == "сброшена на старт");
        Assert.Contains(facts, item => item.Label == "Проверка после загрузки" && item.Value == "пройдена");
    }

    [Fact]
    public void Behavioral_exactly_one_cached_hidden_smoke_runs_after_migration()
    {
        Assert.NotEmpty(Goal157TestKit.CompleteHostCaches());
        if (!Goal161StandaloneState.Enabled) return;
        var standalone = Goal161StandaloneState.Value.Standalone;
        Assert.Equal("GREEN", standalone.Status);
        Assert.True(standalone.LaunchSmokePassed);
        Assert.Equal(standalone.SelfCheckTotalCount, standalone.SelfCheckPassedCount);
        Assert.True(standalone.SelfCheckTotalCount > 0);
    }

    [Fact]
    public void Behavioral_cached_host_is_reused_without_rebuild_and_unity_editor_starts_zero()
    {
        if (!Goal161StandaloneState.Enabled) return;
        var state = Goal161StandaloneState.Value;
        Assert.True(state.Standalone.HostReused);
        Assert.False(state.Standalone.HostRebuilt);
        Assert.Equal(0, state.UnityEditorStarts);
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
    }

    [Fact]
    public void Behavioral_actual_payload_contains_save_migration_travel_and_accepted_facts()
    {
        if (!Goal161StandaloneState.Enabled) return;
        var state = Goal161StandaloneState.Value;
        using var model = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(state.PayloadRoot, "player-adapter-model.json")));
        var facts = model.RootElement.GetProperty("humanReviewFacts").EnumerateArray()
            .Select(item => (Label: item.GetProperty("label").GetString() ?? string.Empty,
                Value: item.GetProperty("value").GetString() ?? string.Empty)).ToList();
        Assert.Contains(facts, item => item.Label == "Игровое сохранение" && item.Value == "перенесено");
        Assert.All(state.Snapshot.GeneratedRegionTravel!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.All(state.Snapshot.AcceptedMechanics!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
    }

    [Fact]
    public void Behavioral_all_selectable_release_candidate_is_current_after_standalone()
    {
        if (!Goal161StandaloneState.Enabled) return;
        var snapshot = Goal161StandaloneState.Value.Snapshot;
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.True(snapshot.AcceptedMechanics?.Passed);
    }

    [Fact]
    public void Behavioral_core_only_portable_save_truth_restores_without_false_rc_readiness()
    {
        if (!Goal161StandaloneState.Enabled) return;
        var state = Goal161StandaloneState.Value;
        Assert.Equal(state.UnityProcessesBeforePortable,
            state.UnityProcessesAfterCorePortable);
        Assert.Equal("TRAVEL_CURRENT", state.CorePortableSnapshot.GeneratedWorld?.Status);
        Assert.False(state.CorePortableSnapshot.AcceptedMechanics?.Passed);
        Assert.NotEmpty(state.CorePortableSnapshot.AcceptedMechanics?.MissingFactKinds ?? []);
        Assert.DoesNotContain(state.CorePortableSnapshot.ReleaseCandidateConfigurationStatus,
            new[] { "READY", "CURRENT", "BUILD_GREEN_STANDALONE_PENDING" });
        Assert.Equal(1, state.CorePortableSnapshot.GeneratedGameplaySaves?.SlotCount);
        Assert.Equal(2, state.CorePortableRevisionCount);
    }

    [Fact]
    public void Behavioral_all_selectable_portable_copy_restores_slots_revisions_migration_and_rc_without_execution()
    {
        if (!Goal161StandaloneState.Enabled) return;
        var state = Goal161StandaloneState.Value;
        Assert.Equal(state.UnityProcessesBeforePortable,
            state.UnityProcessesAfterAllSelectablePortable);
        Assert.Equal("TRAVEL_CURRENT", state.AllSelectablePortableSnapshot.GeneratedWorld?.Status);
        Assert.True(state.AllSelectablePortableSnapshot.GeneratedWorldActivation?.Passed);
        Assert.True(state.AllSelectablePortableSnapshot.GeneratedRegionTravel?.Passed);
        Assert.Equal("CURRENT", state.AllSelectablePortableSnapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal(1, state.AllSelectablePortableSnapshot.GeneratedGameplaySaves?.SlotCount);
        Assert.Equal(GeneratedGameplaySaveStatus.CURRENT,
            Assert.Single(state.AllSelectablePortableSnapshot.GeneratedGameplaySaves!.Entries,
                item => !item.LegacyRaw).Status);
        Assert.Equal(state.AllSelectableRevisionCount, state.AllSelectablePortableRevisionCount);
    }
}

internal static class Goal161StandaloneState
{
    public static bool Enabled => string.Equals(
        Environment.GetEnvironmentVariable("LLMGC_GOAL161_RUN_SMOKE"), "true",
        StringComparison.OrdinalIgnoreCase);

    private static readonly Lazy<Goal161StandaloneFixture> Fixture = new(Goal161StandaloneFixture.Create);
    public static Goal161StandaloneFixture Value => Fixture.Value;
}

internal sealed record Goal161StandaloneFixture(
    GeneratedProject Project,
    GeneratedGameplaySaveMigrationResult Migration,
    IReadOnlyList<GeneratedGameplaySaveFact> PreStandaloneFacts,
    ProjectStandaloneBuildResult Standalone,
    UnifiedGameProjectWorkspaceSnapshot Snapshot,
    string PayloadRoot,
    int UnityEditorStarts,
    int UnityProcessesBeforePortable,
    int UnityProcessesAfterAllSelectablePortable,
    int UnityProcessesAfterCorePortable,
    UnifiedGameProjectWorkspaceSnapshot AllSelectablePortableSnapshot,
    UnifiedGameProjectWorkspaceSnapshot CorePortableSnapshot,
    int AllSelectableRevisionCount,
    int AllSelectablePortableRevisionCount,
    int CorePortableRevisionCount)
{
    public static Goal161StandaloneFixture Create()
    {
        if (!Goal161StandaloneState.Enabled)
            throw new InvalidOperationException("Goal161 standalone proof requires LLMGC_GOAL161_RUN_SMOKE=true.");
        var project = Goal156TestKit.Copy(Goal161MigrationState.Value.Project, "goal161-standalone-proof");
        var bundle = Goal161WorldBundle.Create(project.Path);
        var preview = bundle.Controller.PreviewGeneratedGameplaySaveMigration("campaign");
        Assert.True(preview.Passed, string.Join(Environment.NewLine, preview.Diagnostics));
        var migration = bundle.Controller.ApplyGeneratedGameplaySaveMigration(preview);
        Assert.True(migration.Passed, string.Join(Environment.NewLine, migration.Diagnostics));
        var preStandalone = bundle.Controller.Snapshot();
        var facts = GeneratedGameplaySavesSummaryService.StandaloneHumanFacts(
            preStandalone.GeneratedGameplaySaves);
        Assert.NotEmpty(facts);
        var caches = Goal157TestKit.CompleteHostCaches();
        var hostBefore = caches.ToDictionary(path => path, Goal157TestKit.TreeHashes,
            StringComparer.OrdinalIgnoreCase);
        var goal142Before = Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath);
        var goal148 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Games", "goal148-manual");
        var goal148Before = Goal157TestKit.TreeHashes(goal148);
        var unityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        var standalone = bundle.Controller.BuildWindowsStandalone();
        WriteAttemptCapture(standalone);
        var unityAfter = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        Assert.Equal("GREEN", standalone.Status);
        var snapshot = bundle.Controller.Snapshot();
        var allCount = bundle.Saves.Store.ReadSlot(project.Path, "campaign").Revisions.Count;
        var beforePortable = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        using var portableAll = Goal156TestKit.Copy(project, "goal161-portable-all");
        var portableAllBundle = Goal161WorldBundle.Create(portableAll.Path);
        var portableAllSnapshot = portableAllBundle.Controller.Snapshot();
        var portableAllCount = portableAllBundle.Saves.Store.ReadSlot(portableAll.Path, "campaign").Revisions.Count;
        var afterAll = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        using var portableCore = Goal156TestKit.Copy(Goal161CoreProfileState.Value.Project,
            "goal161-portable-core");
        var portableCoreBundle = Goal161WorldBundle.Create(portableCore.Path);
        var portableCoreSnapshot = portableCoreBundle.Controller.Snapshot();
        var coreCount = portableCoreBundle.Saves.Store.ReadSlot(portableCore.Path, "core").Revisions.Count;
        var afterCore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        var result = new Goal161StandaloneFixture(
            project, migration, facts, standalone, snapshot, Goal157TestKit.RealPayloadRoot(standalone),
            Math.Max(0, unityAfter - unityBefore), beforePortable, afterAll, afterCore,
            portableAllSnapshot, portableCoreSnapshot, allCount, portableAllCount, coreCount);
        WriteCapture(result,
            hostBefore.TryGetValue(Goal157TestKit.HostRoot(standalone.HostCacheKey), out var expectedHost)
            && expectedHost.SequenceEqual(Goal157TestKit.TreeHashes(
                Goal157TestKit.HostRoot(standalone.HostCacheKey))),
            goal142Before == Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath),
            goal148Before.SequenceEqual(Goal157TestKit.TreeHashes(goal148)));
        return result;
    }

    private static void WriteCapture(
        Goal161StandaloneFixture state,
        bool hostFileSetHashUnchanged,
        bool goal142SourceByteIdentical,
        bool sourceGoal148ByteIdentical)
    {
        var path = Environment.GetEnvironmentVariable("LLMGC_GOAL161_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path)) return;
        var migrationState = Goal161MigrationState.Value;
        var core = Goal161CoreProfileState.Value;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            schemaVersion = "goal161_cached_hidden_standalone_smoke_v1",
            status = "GREEN",
            hiddenSmokeInvocationCount = 1,
            unityProcessStartCount = state.UnityEditorStarts,
            state.Standalone.HostCacheKey,
            state.Standalone.HostReused,
            state.Standalone.HostRebuilt,
            hiddenSmokePassed = state.Standalone.LaunchSmokePassed,
            payloadSelfCheckPassed = state.Standalone.PayloadSelfCheckPassed,
            legacyHostParserCompatibilityPassed =
                state.Standalone.LegacyHostParserCompatibilityPassed,
            smokeExitCode = state.Standalone.SmokeExitCode,
            smokeMarkerText = state.Standalone.SmokeMarkerText,
            smokeMarkerPath = state.Standalone.SmokeMarkerPath,
            playerLogPath = state.Standalone.PlayerLogPath,
            playerLogPresent = state.Standalone.PlayerLogPresent,
            playerLogRelevantLines = state.Standalone.PlayerLogRelevantLines,
            namedSmokeFailure = state.Standalone.NamedSmokeFailure,
            standaloneSelfChecksPassed = state.Standalone.SelfCheckTotalCount > 0
                                         && state.Standalone.SelfCheckTotalCount
                                         == state.Standalone.SelfCheckPassedCount,
            outputLocationKind = state.Standalone.OutputLocationKind,
            outputProjectToken = state.Standalone.OutputProjectToken,
            outputFolder = state.Standalone.OutputFolder,
            executablePath = state.Standalone.ExecutablePath,
            buildManifestPath = state.Standalone.BuildManifestPath,
            maximumPlayerPathLength = state.Standalone.MaximumPlayerPathLength,
            playerPathBudgetLimit = state.Standalone.PlayerPathBudgetLimit,
            playerPathBudgetPassed = state.Standalone.PlayerPathBudgetPassed,
            priorSuccessfulOutputPreserved = state.Standalone.PriorSuccessfulOutputPreserved,
            hostFileSetHashUnchanged,
            goal142SourceByteIdentical,
            sourceGoal148ByteIdentical,
            sourceRevisionSha256 = state.Migration.SourceRevisionSha256,
            migratedRevisionSha256 = state.Migration.MigratedRevisionSha256,
            sourceWorldId = state.Migration.Revision?.Migration?.SourceWorldId,
            targetWorldId = state.Migration.Revision?.Migration?.TargetWorldId,
            mapResetPassed = state.Migration.Revision?.Migration?.MapReset == true,
            preservedReferenceCount = state.Migration.Revision?.Migration?.PreservedDefinitionIds.Count ?? 0,
            droppedReferenceCount = state.Migration.Revision?.Migration?.DroppedDefinitionIds.Count ?? 0,
            saveRevisionCount = state.AllSelectableRevisionCount,
            actualPayloadSaveMigrationFactsPassed = PayloadHasFact(state.PayloadRoot,
                "Игровое сохранение", "перенесено"),
            actualPayloadAcceptedFactsPassed = state.Snapshot.AcceptedMechanics!.HumanFacts.All(
                fact => PayloadHasFact(state.PayloadRoot, fact.Label, fact.Value)),
            actualPayloadTravelFactsPassed = state.Snapshot.GeneratedRegionTravel!.HumanFacts.All(
                fact => PayloadHasFact(state.PayloadRoot, fact.Label, fact.Value)),
            allSelectableReleaseCandidateCurrent =
                state.Snapshot.ReleaseCandidateConfigurationStatus == "CURRENT"
                && state.Snapshot.ReleaseCandidateRecordConfigurationStatus == "CURRENT",
            portableAllSelectablePassed =
                state.AllSelectablePortableSnapshot.ReleaseCandidateConfigurationStatus == "CURRENT"
                && state.AllSelectablePortableRevisionCount == state.AllSelectableRevisionCount,
            portableCoreOnlyPassed = state.CorePortableSnapshot.AcceptedMechanics?.Passed == false
                                     && state.CorePortableRevisionCount == 2,
            coreOnlyNoFalseRcReady = !new[] { "READY", "CURRENT", "BUILD_GREEN_STANDALONE_PENDING" }
                .Contains(state.CorePortableSnapshot.ReleaseCandidateConfigurationStatus,
                    StringComparer.Ordinal),
            allSelectableRegenerationCommitPassed = migrationState.Regenerated.Applied,
            allSelectableRollbackCommitPassed = migrationState.Rollback.Applied,
            coreOnlyRegenerationCommitPassed = core.Regenerated.Applied,
            coreOnlyRollbackCommitPassed = core.RolledBack.Applied,
            postMigrationRuntimeMovePassed = migrationState.FirstRouteResults.Any(result =>
                result.MapEvents.Any(item => item.Type == RuntimeEventType.PlayerMoved)),
            postMigrationTravelPassed = migrationState.FirstRouteResults.Any(result =>
                result.MapEvents.Any(item => item.Type == RuntimeEventType.MapChanged)),
            postMigrationDestinationInteractionPassed = migrationState.FirstRouteResults[^1].MapEvents.Any(
                item => item.Type == RuntimeEventType.InteractionTriggered),
            postMigrationReplayEquivalent = migrationState.FirstRouteSessionJson
                                            == migrationState.ReplayRouteSessionJson,
            originalWorldRestored = migrationState.OriginalRevisionAfterRollback.Passed,
            saveTreeUnchangedDuringWorldChanges =
                migrationState.SaveTreeBeforeRegeneration.SequenceEqual(
                    migrationState.SaveTreeAfterRegeneration)
                && migrationState.SaveTreeBeforeRollback.SequenceEqual(
                    migrationState.SaveTreeAfterRollback)
        }, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine,
            new UTF8Encoding(false));
    }

    private static void WriteAttemptCapture(ProjectStandaloneBuildResult standalone)
    {
        var path = Environment.GetEnvironmentVariable("LLMGC_GOAL161_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            schemaVersion = "goal161_cached_hidden_standalone_smoke_v2",
            standalone.Status,
            standalone.Stage,
            standalone.Diagnostics,
            standalone.PublicationStage,
            standalone.PublicationDiagnostic,
            standalone.OutputFolder,
            standalone.OutputRunDirectoryName,
            standalone.CurrentPointerPath,
            standalone.CurrentPointerSha256,
            standalone.RunStatusPath,
            pointerPresent = !string.IsNullOrWhiteSpace(standalone.CurrentPointerPath)
                             && File.Exists(standalone.CurrentPointerPath)
        }, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine, new UTF8Encoding(false));
    }

    private static bool PayloadHasFact(string payloadRoot, string label, string value)
    {
        using var model = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(payloadRoot, "player-adapter-model.json"), Encoding.UTF8));
        return model.RootElement.GetProperty("humanReviewFacts").EnumerateArray().Any(item =>
            item.GetProperty("label").GetString() == label
            && item.GetProperty("value").GetString() == value);
    }
}
