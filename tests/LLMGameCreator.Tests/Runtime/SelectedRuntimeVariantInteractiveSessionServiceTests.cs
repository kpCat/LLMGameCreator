using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Runtime;

public sealed class SelectedRuntimeVariantInteractiveSessionServiceTests
{
    [Fact]
    public void ExecutesIndividualActionsAndReplaysJournalToAcceptedFinalHash()
    {
        var fixture = LoadFixture();
        var service = SelectedRuntimeVariantInteractiveSessionService.CreateDefault();
        var session = service.StartSession(fixture.Package, fixture.Start);

        Assert.True(session.AvailableActions.Count >= 10);
        Assert.True(session.AvailableActions.Count(action => action.Route == "runtime_session") >= 8);
        Assert.True(session.AvailableActions.Count(action => action.Route == "presentation_only") >= 2);
        AssertBinding(session, "harvest", "harvest_apple_tree", 8, "node/apple_tree", "HarvestResourceNode");
        AssertBinding(session, "basic_attack", "combat_round", 11, "goblin", "BasicAttack");

        Execute(service, fixture, session, "start_runtime");
        Execute(service, fixture, session, "move");
        Execute(service, fixture, session, "interact");
        var presentation = Execute(service, fixture, session, "inspect_inventory");
        Assert.False(presentation.RuntimeExecuted);
        Assert.False(presentation.RuntimeMutation);
        Assert.Equal(0, presentation.RuntimeEventCount);
        Assert.Equal(presentation.StateHashBefore, presentation.StateHashAfter);
        Execute(service, fixture, session, "open_dialogue");
        Execute(service, fixture, session, "start_or_update_quest");
        Execute(service, fixture, session, "show_inventory");
        Execute(service, fixture, session, "craft");

        var checkpoint = service.SaveCheckpoint(
            session,
            "goal144-checkpoint-before-final-systems",
            "2026-07-11T00:00:00Z");
        var harvest = Execute(service, fixture, session, "harvest");
        Assert.Equal("node/apple_tree", harvest.TargetId);
        Assert.Equal("node/apple_tree", harvest.ExecutionTargetId);
        Assert.True(harvest.ExecutionBindingValidated);
        Execute(service, fixture, session, "transaction");

        var reload = service.ReloadCheckpoint(fixture.Package, fixture.Start, checkpoint);
        Assert.True(reload.Passed, string.Join(Environment.NewLine, reload.Diagnostics));
        Assert.Equal(checkpoint.ExpectedStateHash, reload.Session.CurrentStateHash);

        session = reload.Session;
        Execute(service, fixture, session, "harvest");
        Execute(service, fixture, session, "transaction");
        Execute(service, fixture, session, "begin_encounter");
        var attack = Execute(service, fixture, session, "basic_attack");
        Assert.Equal("goblin", attack.TargetId);
        Assert.Equal("goblin", attack.ExecutionTargetId);
        Assert.True(attack.ExecutionBindingValidated);
        var final = Execute(service, fixture, session, "show_final_state");
        Assert.False(final.RuntimeExecuted);
        Assert.True(session.Completed);
        Assert.Equal(fixture.AcceptedFinalHash, session.CurrentStateHash);

        var finalCheckpoint = service.SaveCheckpoint(
            session,
            "goal144-final-journal",
            "2026-07-11T00:00:00Z");
        var fullReplay = service.ReloadCheckpoint(fixture.Package, fixture.Start, finalCheckpoint);
        Assert.True(fullReplay.Passed, string.Join(Environment.NewLine, fullReplay.Diagnostics));
        Assert.Equal(fixture.AcceptedFinalHash, fullReplay.ActualStateHash);
    }

    [Fact]
    public void RejectsInvalidActionWithoutMutationOrJournalAdvance()
    {
        var fixture = LoadFixture();
        var service = SelectedRuntimeVariantInteractiveSessionService.CreateDefault();
        var session = service.StartSession(fixture.Package, fixture.Start);
        var result = service.ExecuteAction(fixture.Package, session, new()
        {
            ActionRequestId = "goal144-invalid",
            SessionId = session.SessionId,
            ActionIndex = 0,
            ActionId = "not-a-package-action"
        });

        Assert.Equal("REJECTED", result.Status);
        Assert.Equal(result.StateHashBefore, result.StateHashAfter);
        Assert.Empty(session.ActionJournal);
        Assert.Equal(0, session.CurrentActionIndex);
    }

    [Fact]
    public void RejectsCheckpointIdentityHashAndJournalTampering()
    {
        var fixture = LoadFixture();
        var service = SelectedRuntimeVariantInteractiveSessionService.CreateDefault();
        var session = service.StartSession(fixture.Package, fixture.Start);
        Execute(service, fixture, session, "start_runtime");
        var checkpoint = service.SaveCheckpoint(session, "checkpoint", "2026-07-11T00:00:00Z");

        var wrongHash = CloneStart(fixture.Start);
        wrongHash.PackageSha256 = new string('0', 64);
        Assert.False(service.ReloadCheckpoint(fixture.Package, wrongHash, checkpoint).Passed);

        var wrongCandidate = CloneStart(fixture.Start);
        wrongCandidate.CandidateId = "minimal-map-game-balanced-baseline";
        Assert.False(service.ReloadCheckpoint(fixture.Package, wrongCandidate, checkpoint).Passed);

        var actionTamper = CloneCheckpoint(checkpoint);
        actionTamper.ActionJournal[0].ActionId = "move";
        Assert.False(service.ReloadCheckpoint(fixture.Package, fixture.Start, actionTamper).Passed);

        var targetTamper = CloneCheckpoint(checkpoint);
        targetTamper.ActionJournal[0].TargetId = "map/not-village";
        Assert.False(service.ReloadCheckpoint(fixture.Package, fixture.Start, targetTamper).Passed);
    }

    [Theory]
    [InlineData("harvest", "target")]
    [InlineData("basic_attack", "target")]
    [InlineData("harvest", "step")]
    [InlineData("harvest", "range")]
    public void RejectsTamperedDescriptorBindingWithoutMutation(string actionId, string tamperKind)
    {
        var fixture = LoadFixture();
        var service = SelectedRuntimeVariantInteractiveSessionService.CreateDefault();
        var session = service.StartSession(fixture.Package, fixture.Start);
        AdvanceTo(service, fixture, session, actionId);
        var descriptor = session.AvailableActions.Single(item => item.ActionId == actionId);
        Assert.True(descriptor.Available);
        switch (tamperKind)
        {
            case "target":
                descriptor.TargetId = actionId == "harvest"
                    ? "node/diesel_generator"
                    : "ability/basic_attack";
                break;
            case "step":
                descriptor.CanonicalStepId = "execute_transaction";
                break;
            case "range":
                descriptor.RuntimeCommandStartIndex++;
                descriptor.RuntimeCommandEndIndex++;
                break;
        }

        var before = session.CurrentStateHash;
        var journalCount = session.ActionJournal.Count;
        var actionIndex = session.CurrentActionIndex;
        var result = service.ExecuteAction(fixture.Package, session, new()
        {
            ActionRequestId = "goal144a-tampered-binding",
            SessionId = session.SessionId,
            ActionIndex = session.CurrentActionIndex,
            ActionId = actionId
        });
        Assert.Equal("REJECTED", result.Status);
        Assert.Equal(before, session.CurrentStateHash);
        Assert.Equal(journalCount, session.ActionJournal.Count);
        Assert.Equal(actionIndex, session.CurrentActionIndex);
    }

    [Fact]
    public void CheckpointReplayCountRemainsFrozenAfterReturnedSessionContinues()
    {
        var fixture = LoadFixture();
        var service = SelectedRuntimeVariantInteractiveSessionService.CreateDefault();
        var session = service.StartSession(fixture.Package, fixture.Start);
        foreach (var action in AcceptanceSequence().Take(8))
        {
            Execute(service, fixture, session, action);
        }

        var checkpoint = service.SaveCheckpoint(session, "checkpoint", "2026-07-11T00:00:00Z");
        var replay = service.ReloadCheckpoint(fixture.Package, fixture.Start, checkpoint);
        Assert.True(replay.Passed, string.Join(Environment.NewLine, replay.Diagnostics));
        Assert.Equal(8, replay.ReplayedActionCount);
        foreach (var action in AcceptanceSequence().Skip(8))
        {
            Execute(service, fixture, replay.Session, action);
        }

        Assert.Equal(13, replay.Session.ActionJournal.Count);
        Assert.Equal(8, replay.ReplayedActionCount);
    }

    private static void AdvanceTo(
        SelectedRuntimeVariantInteractiveSessionService service,
        Fixture fixture,
        SelectedRuntimeVariantInteractiveSession session,
        string actionId)
    {
        foreach (var action in AcceptanceSequence())
        {
            if (action == actionId) return;
            Execute(service, fixture, session, action);
        }
        throw new InvalidOperationException("Action was not found in Goal144 sequence: " + actionId);
    }

    private static IReadOnlyList<string> AcceptanceSequence() =>
    [
        "start_runtime", "move", "interact", "inspect_inventory", "open_dialogue",
        "start_or_update_quest", "show_inventory", "craft", "harvest", "transaction",
        "begin_encounter", "basic_attack", "show_final_state"
    ];

    private static void AssertBinding(
        SelectedRuntimeVariantInteractiveSession session,
        string actionId,
        string stepId,
        int stepIndex,
        string targetId,
        string commandKind)
    {
        var descriptor = session.AvailableActions.Single(item => item.ActionId == actionId);
        Assert.Equal(stepId, descriptor.CanonicalStepId);
        Assert.Equal(stepIndex, descriptor.CanonicalStepIndex);
        Assert.Equal(stepIndex, descriptor.RuntimeCommandStartIndex);
        Assert.Equal(stepIndex, descriptor.RuntimeCommandEndIndex);
        Assert.Equal(targetId, descriptor.TargetId);
        Assert.Equal(targetId, descriptor.ExecutionTargetId);
        Assert.Equal(commandKind, descriptor.CommandKind);
        Assert.True(descriptor.ExecutionBindingValidated);
    }

    private static SelectedRuntimeVariantInteractiveActionResult Execute(
        SelectedRuntimeVariantInteractiveSessionService service,
        Fixture fixture,
        SelectedRuntimeVariantInteractiveSession session,
        string actionId)
    {
        var result = service.ExecuteAction(fixture.Package, session, new()
        {
            ActionRequestId = "goal144-action-" + session.CurrentActionIndex.ToString("000"),
            SessionId = session.SessionId,
            ActionIndex = session.CurrentActionIndex,
            ActionId = actionId
        });
        Assert.Equal("EXECUTED", result.Status);
        Assert.True(result.CorrelationPassed);
        return result;
    }

    private static Fixture LoadFixture()
    {
        var root = ProjectRoot();
        var packagePath = Path.Combine(
            root,
            ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/package.json"
                .Replace('/', Path.DirectorySeparatorChar));
        var handoffPath = Path.Combine(
            root,
            ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/selected-runtime-variant-handoff.json"
                .Replace('/', Path.DirectorySeparatorChar));
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(
                          File.ReadAllText(packagePath),
                          options)
                      ?? throw new InvalidOperationException("Goal142 selected package did not parse.");
        using var handoff = JsonDocument.Parse(File.ReadAllText(handoffPath));
        var rootElement = handoff.RootElement;
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(packagePath))).ToLowerInvariant();
        return new Fixture(
            package,
            new SelectedRuntimeVariantInteractiveSessionStartRequest
            {
                SessionId = "goal144-selected-session",
                CandidateId = rootElement.GetProperty("candidateId").GetString()!,
                VariantKind = rootElement.GetProperty("variantKind").GetString()!,
                PackagePath = Path.GetRelativePath(root, packagePath).Replace('\\', '/'),
                PackageSha256 = hash
            },
            rootElement.GetProperty("finalStateHash").GetString()!);
    }

    private static SelectedRuntimeVariantInteractiveSessionStartRequest CloneStart(
        SelectedRuntimeVariantInteractiveSessionStartRequest source) =>
        new()
        {
            SessionId = source.SessionId,
            CandidateId = source.CandidateId,
            VariantKind = source.VariantKind,
            PackagePath = source.PackagePath,
            PackageSha256 = source.PackageSha256
        };

    private static SelectedRuntimeVariantInteractiveCheckpoint CloneCheckpoint(
        SelectedRuntimeVariantInteractiveCheckpoint source) =>
        new()
        {
            CheckpointId = source.CheckpointId,
            SessionId = source.SessionId,
            CandidateId = source.CandidateId,
            VariantKind = source.VariantKind,
            PackageSha256 = source.PackageSha256,
            ActionJournal = source.ActionJournal.Select(entry => new SelectedRuntimeVariantInteractiveJournalEntry
            {
                ActionRequestId = entry.ActionRequestId,
                SessionId = entry.SessionId,
                ActionIndex = entry.ActionIndex,
                ActionId = entry.ActionId,
                Category = entry.Category,
                Route = entry.Route,
                CommandKind = entry.CommandKind,
                TargetId = entry.TargetId,
                CanonicalStepId = entry.CanonicalStepId,
                CanonicalStepIndex = entry.CanonicalStepIndex,
                RuntimeCommandStartIndex = entry.RuntimeCommandStartIndex,
                RuntimeCommandEndIndex = entry.RuntimeCommandEndIndex,
                ExecutionTargetId = entry.ExecutionTargetId,
                ExecutionBindingValidated = entry.ExecutionBindingValidated,
                StateHashBefore = entry.StateHashBefore,
                StateHashAfter = entry.StateHashAfter,
                RuntimeExecuted = entry.RuntimeExecuted,
                RuntimeMutation = entry.RuntimeMutation,
                RuntimeEventCount = entry.RuntimeEventCount
            }).ToList(),
            RuntimeCommandExecutionCount = source.RuntimeCommandExecutionCount,
            ExpectedStateHash = source.ExpectedStateHash,
            ExpectedActionIndex = source.ExpectedActionIndex,
            MapSummary = source.MapSummary,
            InventorySummary = source.InventorySummary,
            QuestSummary = source.QuestSummary,
            CombatSummary = source.CombatSummary,
            CreatedAtUtc = source.CreatedAtUtc
        };

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }

    private sealed record Fixture(
        GamePackageDefinition Package,
        SelectedRuntimeVariantInteractiveSessionStartRequest Start,
        string AcceptedFinalHash);
}
