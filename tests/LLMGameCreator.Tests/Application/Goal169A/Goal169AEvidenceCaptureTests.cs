using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal168;
using LLMGameCreator.Tests.Application.Goal169;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169A;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169AEvidenceCaptureTests
{
    [Fact]
    public void Behavioral_typed_goal169a_capture_is_strict_and_complete()
    {
        var events =
            Assert.IsType<GameProjectGeneratedCampaignRegionalEventSummary>(
                Goal168TestKit.Build.GeneratedCampaignRegionalEvents);
        var relationships =
            Assert.IsType<
                GameProjectGeneratedCampaignRelationshipSummary>(
                Goal168TestKit.Build.GeneratedCampaignRelationships);
        var correlation =
            GeneratedCampaignRegionalEventCorrelationService.Validate(
                events.ExactPackageSha256, events, relationships);
        Assert.True(correlation.Passed,
            string.Join(",", correlation.Diagnostics));
        var migration = Goal169SaveMigrationState.Value;
        var explicitMoveCount = events.RuntimeFrames.Count(item =>
            item.CommandType.StartsWith("Move.",
                StringComparison.Ordinal));
        var bareDirectionCount = events.RuntimeFrames.Count(item =>
            item.CommandType is "Up" or "Down" or "Left"
            or "Right");
        Assert.True(explicitMoveCount > 0);
        Assert.Equal(0, bareDirectionCount);

        var path = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL169A_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path))
            return;
        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            status = "GREEN",
            events.StrictProofSchemaVersion,
            packageSha256 = events.ExactPackageSha256,
            events.FinalStateHash,
            relationships.RelationshipBranchMatrixSha256,
            relationships.ArcQuestCount,
            relationships.QualifiedArcQuestCount,
            events.EventCount,
            events.QualifiedEventCount,
            inventoryCount = events.EventInventory.Count,
            bindingCount = events.Overlay!.Bindings.Count,
            qualificationCount =
                events.EventQualifications.Count,
            runtimeFrameCount = events.RuntimeFrames.Count,
            replaySignatureCount =
                events.ReplaySignatures.Count,
            lockedReplaySignatureCount =
                events.ReplaySignatures.Count(item =>
                    item.RouteKind ==
                    GeneratedCampaignRegionalEventReplayRouteKind
                        .LOCKED_PROBE),
            resolutionReplaySignatureCount =
                events.ReplaySignatures.Count(item =>
                    item.RouteKind ==
                    GeneratedCampaignRegionalEventReplayRouteKind
                        .RESOLUTION),
            explicitMoveCount,
            bareDirectionCount,
            strictCorrelationPassed = correlation.Passed,
            correlationDiagnostics = correlation.Diagnostics,
            replaySignatures = events.ReplaySignatures,
            eventInventory = events.EventInventory,
            challengeRegionDerivations = events.EventInventory
                .Where(item => item.EventKind ==
                    GeneratedCampaignRegionalEventKind
                        .CHALLENGE_AFTERMATH)
                .Select(item => new
                {
                    item.RegionalEventId,
                    item.RegionId,
                    item.MapId,
                    item.ChallengeEncounterId,
                    item.ChallengeEncounterSourceId,
                    item.TargetRegionDerivation,
                    item.TargetRegionFingerprint
                }).ToList(),
            compatibleMigrationFacts =
                migration.CompatiblePreview.RegionalEventFacts,
            incompatibleMigrationFacts =
                migration.IncompatiblePreview.RegionalEventFacts
        }, new JsonSerializerOptions { WriteIndented = true })
            + Environment.NewLine, new UTF8Encoding(false));
    }
}
