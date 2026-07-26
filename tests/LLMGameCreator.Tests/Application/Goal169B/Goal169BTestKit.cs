using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal168;
using LLMGameCreator.Tests.Application.Goal169;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169B;

internal static class Goal169BTestKit
{
    internal static GameProjectGeneratedCampaignRegionalEventSummary Events
    {
        get
        {
            Assert.True(Goal168TestKit.Build.Passed,
                string.Join(",", Goal168TestKit.Build.Diagnostics));
            return Assert.IsType<
                GameProjectGeneratedCampaignRegionalEventSummary>(
                Goal168TestKit.Build.GeneratedCampaignRegionalEvents);
        }
    }

    internal static GameProjectGeneratedCampaignRelationshipSummary
        Relationships => Assert.IsType<
        GameProjectGeneratedCampaignRelationshipSummary>(
        Goal168TestKit.Build.GeneratedCampaignRelationships);

    internal static GamePackageDefinition Package =>
        Goal168TestKit.Package;

    internal static GamePackageDefinition ClonePackage() =>
        Goal164TestKit.Clone(Package);

    internal static Goal169BAbsentFixture Absent
    {
        get
        {
            var profile =
                Goal169ProfileFixture.All["no-branches"];
            var overlay =
                new GeneratedCampaignRegionalEventOverlayService()
                    .Build(Goal164TestKit.Clone(
                            Goal168TestKit.Real.LaneAPackage),
                        profile.Events);
            Assert.True(overlay.Passed,
                string.Join(",", overlay.Diagnostics));
            var summary =
                new
                    GameProjectGeneratedCampaignRegionalEventQualificationService()
                    .Qualify(overlay.RegionalEventOverlayPackage,
                        overlay.Document, profile.Summary, null,
                        Goal168TestKit.Real.Runtime);
            Assert.True(summary.Passed,
                string.Join(",", summary.Diagnostics));
            return new Goal169BAbsentFixture(
                overlay.RegionalEventOverlayPackage,
                profile.Summary, summary);
        }
    }

    internal static GeneratedCampaignRegionalEventCorrelationResult
        Correlate(
            GameProjectGeneratedCampaignRegionalEventSummary? events =
                null,
            GamePackageDefinition? package = null,
            string? packageSha256 = null) =>
        GeneratedCampaignRegionalEventCorrelationService.Validate(
            package ?? Package,
            packageSha256 ?? Events.ExactPackageSha256,
            events ?? Events,
            Relationships);

    internal static GameProjectGeneratedCampaignRegionalEventSummary
        RebuildRoute(
            GameProjectGeneratedCampaignRegionalEventSummary source,
            string eventId,
            GeneratedCampaignRegionalEventReplayRouteKind route,
            int replay,
            Func<IReadOnlyList<
                    GeneratedCampaignRegionalEventRuntimeFrame>,
                IReadOnlyList<
                    GeneratedCampaignRegionalEventRuntimeFrame>>
                transform)
    {
        var original = source.RuntimeFrames.Where(item =>
            item.RegionalEventId == eventId
            && item.RouteKind == route
            && item.ReplayIndex == replay).OrderBy(item =>
            item.SequenceIndex).ToList();
        var replacement = transform(original).Select((item, index) =>
            item with { SequenceIndex = index }).ToList();
        var frames = source.RuntimeFrames.Where(item =>
                !(item.RegionalEventId == eventId
                  && item.RouteKind == route
                  && item.ReplayIndex == replay))
            .Concat(replacement)
            .OrderBy(item => item.RegionalEventId,
                StringComparer.Ordinal)
            .ThenBy(item => item.RouteKind)
            .ThenBy(item => item.ReplayIndex)
            .ThenBy(item => item.SequenceIndex).ToList();
        var qualifications = source.EventQualifications.Select(item =>
        {
            if (item.RegionalEventId != eventId)
                return item;
            var signatures = item.ReplaySignatures.Select(signature =>
                    signature.RouteKind == route
                    && signature.ReplayIndex == replay
                        ? GeneratedCampaignRegionalEventReplayService
                            .CreateSignature(eventId, route, replay,
                                replacement)
                        : signature)
                .OrderBy(signature => signature.RouteKind)
                .ThenBy(signature => signature.ReplayIndex).ToList();
            return item with
            {
                RuntimeCommandCount = frames.Count(frame =>
                    frame.RegionalEventId == eventId),
                FinalStateHash = signatures.Single(signature =>
                        signature.RouteKind ==
                        GeneratedCampaignRegionalEventReplayRouteKind
                            .RESOLUTION
                        && signature.ReplayIndex == 1)
                    .FinalStateHash,
                ReplaySignatures = signatures
            };
        }).ToList();
        var signatures = qualifications.SelectMany(item =>
                item.ReplaySignatures)
            .OrderBy(item => item.RegionalEventId,
                StringComparer.Ordinal)
            .ThenBy(item => item.RouteKind)
            .ThenBy(item => item.ReplayIndex).ToList();
        var finalStateHash = Hash(
            qualifications.Select(item => new
            {
                item.RegionalEventId,
                item.FinalStateHash,
                ResolutionSignature = item.ReplaySignatures.Single(
                    signature => signature.RouteKind ==
                                 GeneratedCampaignRegionalEventReplayRouteKind
                                     .RESOLUTION
                                 && signature.ReplayIndex == 1)
                    .SignatureSha256
            }).ToList());
        var payload =
            GeneratedCampaignRegionalEventPayloadAuthorityService.Create(
                source.ExactPackageSha256, finalStateHash,
                source.RegionalEventInventorySha256,
                source.EventInventory, signatures, frames);
        return source with
        {
            FinalStateHash = finalStateHash,
            EventQualifications = qualifications,
            ReplaySignatures = signatures,
            RuntimeFrames = frames,
            PayloadAuthority = payload
        };
    }

    internal static string PackageSha256(
        GamePackageDefinition package) =>
        Goal168TestKit.PackageSha(package);

    internal static string Hash<T>(T value) =>
        HashText(JsonSerializer.Serialize(value, ChoiceJsonOptions));

    internal static string CombatHash<T>(T value) =>
        HashText(JsonSerializer.Serialize(value, CombatJsonOptions));

    private static string HashText(string value) =>
        Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static readonly JsonSerializerOptions ChoiceJsonOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

    private static readonly JsonSerializerOptions CombatJsonOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
        };
}

internal sealed record Goal169BAbsentFixture(
    GamePackageDefinition Package,
    GameProjectGeneratedCampaignRelationshipSummary Relationships,
    GameProjectGeneratedCampaignRegionalEventSummary Events);
