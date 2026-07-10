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
        Execute(service, fixture, session, "harvest");
        Execute(service, fixture, session, "transaction");

        var reload = service.ReloadCheckpoint(fixture.Package, fixture.Start, checkpoint);
        Assert.True(reload.Passed, string.Join(Environment.NewLine, reload.Diagnostics));
        Assert.Equal(checkpoint.ExpectedStateHash, reload.Session.CurrentStateHash);

        session = reload.Session;
        Execute(service, fixture, session, "harvest");
        Execute(service, fixture, session, "transaction");
        Execute(service, fixture, session, "begin_encounter");
        Execute(service, fixture, session, "basic_attack");
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

        checkpoint.ActionJournal[0].ActionId = "move";
        Assert.False(service.ReloadCheckpoint(fixture.Package, fixture.Start, checkpoint).Passed);
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
