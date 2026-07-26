using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed record GeneratedCampaignRegionalEventPayloadFrameIdentity
{
    public string RegionalEventId { get; init; } = string.Empty;
    public GeneratedCampaignRegionalEventReplayRouteKind RouteKind { get; init; }
    public int ReplayIndex { get; init; }
    public int SequenceIndex { get; init; }
    public string CommandIdentity { get; init; } = string.Empty;
}

public static class GeneratedCampaignRegionalEventPayloadAuthorityService
{
    public const string FrameSchema =
        "generated-regional-event-frame-v1";
    public const string HumanFactLabel =
        "regional-event-strict-replay-authority-v1";
    private const string HumanFactBase64Prefix = "base64:";

    public static GeneratedCampaignRegionalEventPayloadAuthority Create(
        string packageSha256,
        string finalStateHash,
        string inventorySha256,
        IReadOnlyList<GeneratedCampaignRegionalEventInventoryRow> inventory,
        IReadOnlyList<GeneratedCampaignRegionalEventReplaySignature> signatures,
        IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame> frames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageSha256);
        ArgumentException.ThrowIfNullOrWhiteSpace(finalStateHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(inventorySha256);
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(signatures);
        ArgumentNullException.ThrowIfNull(frames);

        var ids = inventory.Select(item => item.RegionalEventId)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        var orderedSignatures = signatures
            .OrderBy(item => item.RegionalEventId, StringComparer.Ordinal)
            .ThenBy(item => item.RouteKind)
            .ThenBy(item => item.ReplayIndex).ToList();
        var orderedFrames = frames
            .OrderBy(item => item.RegionalEventId, StringComparer.Ordinal)
            .ThenBy(item => item.RouteKind)
            .ThenBy(item => item.ReplayIndex)
            .ThenBy(item => item.SequenceIndex).ToList();
        var components =
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["inventory"] =
                    GeneratedCampaignChoiceCanonical.Hash(inventory),
                ["signatures"] =
                    GeneratedCampaignChoiceCanonical.Hash(
                        orderedSignatures),
                ["frames"] =
                    GeneratedCampaignChoiceCanonical.Hash(
                        orderedFrames),
                ["nestedCombat"] =
                    GeneratedCampaignChoiceCanonical.Hash(orderedFrames
                        .Where(item => item.NestedCombat).ToList())
            };
        var frameCounts = orderedSignatures.ToDictionary(
            SignatureKey, item => item.FrameCount,
            StringComparer.Ordinal);
        var nestedHashes = orderedSignatures.ToDictionary(
            SignatureKey, item => item.NestedCombatTraceSha256,
            StringComparer.Ordinal);
        var authority = new
            GeneratedCampaignRegionalEventPayloadAuthority
            {
                PackageSha256 = packageSha256,
                FinalStateHash = finalStateHash,
                InventorySha256 = inventorySha256,
                RegionalEventIds = ids,
                ReplaySignatures = orderedSignatures,
                ComponentSha256 = components,
                FrameCounts = frameCounts,
                NestedCombatTraceSha256 = nestedHashes,
                Passed = true
            };
        authority = authority with
        {
            AuthoritySha256 = AuthoritySha256(authority)
        };
        return Validate(authority, inventory, signatures, frames).Passed
            ? authority
            : authority with { Passed = false };
    }

    public static GeneratedCampaignRegionalEventReplayComparison Validate(
        GeneratedCampaignRegionalEventPayloadAuthority authority,
        IReadOnlyList<GeneratedCampaignRegionalEventInventoryRow> inventory,
        IReadOnlyList<GeneratedCampaignRegionalEventReplaySignature> signatures,
        IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame> frames)
    {
        ArgumentNullException.ThrowIfNull(authority);
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(signatures);
        ArgumentNullException.ThrowIfNull(frames);
        var diagnostics = new List<string>();
        var ids = inventory.Select(item => item.RegionalEventId)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        var expectedSignatureCount = ids.Count * 4;
        if (authority.SchemaVersion !=
            GeneratedCampaignRegionalEventPayloadAuthority.CurrentSchema)
            diagnostics.Add(
                "generated_regional_event.payload_authority.schema");
        if (!authority.Passed
            || authority.AuthoritySha256 != AuthoritySha256(authority))
            diagnostics.Add(
                "generated_regional_event.payload_authority.hash");
        if (!authority.RegionalEventIds.SequenceEqual(ids,
                StringComparer.Ordinal)
            || ids.Distinct(StringComparer.Ordinal).Count() != ids.Count)
            diagnostics.Add(
                "generated_regional_event.payload_authority.event_ids");
        var orderedSignatures = signatures
            .OrderBy(item => item.RegionalEventId, StringComparer.Ordinal)
            .ThenBy(item => item.RouteKind)
            .ThenBy(item => item.ReplayIndex).ToList();
        if (orderedSignatures.Count != expectedSignatureCount
            || GeneratedCampaignChoiceCanonical.Serialize(
                authority.ReplaySignatures)
            != GeneratedCampaignChoiceCanonical.Serialize(
                orderedSignatures))
            diagnostics.Add(
                "generated_regional_event.payload_authority.signatures");
        foreach (var signature in orderedSignatures)
        {
            var key = SignatureKey(signature);
            if (authority.FrameCounts.GetValueOrDefault(key) !=
                signature.FrameCount
                || authority.NestedCombatTraceSha256
                    .GetValueOrDefault(key)
                != signature.NestedCombatTraceSha256)
                diagnostics.Add(
                    "generated_regional_event.payload_authority.components");
        }
        if (authority.ComponentSha256.GetValueOrDefault("inventory")
            != GeneratedCampaignChoiceCanonical.Hash(inventory)
            || authority.ComponentSha256.GetValueOrDefault("signatures")
            != GeneratedCampaignChoiceCanonical.Hash(orderedSignatures)
            || authority.ComponentSha256.GetValueOrDefault("frames")
            != GeneratedCampaignChoiceCanonical.Hash(frames
                .OrderBy(item => item.RegionalEventId,
                    StringComparer.Ordinal)
                .ThenBy(item => item.RouteKind)
                .ThenBy(item => item.ReplayIndex)
                .ThenBy(item => item.SequenceIndex).ToList())
            || authority.ComponentSha256.GetValueOrDefault(
                "nestedCombat")
            != GeneratedCampaignChoiceCanonical.Hash(frames
                .Where(item => item.NestedCombat)
                .OrderBy(item => item.RegionalEventId,
                    StringComparer.Ordinal)
                .ThenBy(item => item.RouteKind)
                .ThenBy(item => item.ReplayIndex)
                .ThenBy(item => item.SequenceIndex).ToList()))
            diagnostics.Add(
                "generated_regional_event.payload_authority.components");
        return new GeneratedCampaignRegionalEventReplayComparison
        {
            Passed = diagnostics.Count == 0,
            Diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    public static string Serialize(
        GeneratedCampaignRegionalEventPayloadAuthority authority)
    {
        ArgumentNullException.ThrowIfNull(authority);
        return GeneratedCampaignChoiceCanonical.Serialize(authority);
    }

    public static string SerializeHumanFact(
        GeneratedCampaignRegionalEventPayloadAuthority authority)
    {
        var json = Serialize(authority);
        return HumanFactBase64Prefix
               + Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static GeneratedCampaignRegionalEventPayloadAuthority
        DeserializeHumanFact(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!value.StartsWith(HumanFactBase64Prefix,
                StringComparison.Ordinal))
            throw new JsonException(
                "generated_regional_event.payload_authority.encoding");
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(
                value[HumanFactBase64Prefix.Length..]));
            return JsonSerializer.Deserialize<
                       GeneratedCampaignRegionalEventPayloadAuthority>(
                       json, GeneratedCampaignChoiceCanonical.JsonOptions)
                   ?? throw new JsonException(
                       "generated_regional_event.payload_authority.empty");
        }
        catch (FormatException exception)
        {
            throw new JsonException(
                "generated_regional_event.payload_authority.base64",
                exception);
        }
    }

    public static string FrameCategory(
        GeneratedCampaignRegionalEventRuntimeFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        return string.Join("|",
            FrameSchema,
            frame.RegionalEventId,
            frame.RouteKind,
            frame.ReplayIndex.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            frame.SequenceIndex.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            frame.CommandSha256);
    }

    public static bool TryParseFrameCategory(
        string value,
        out GeneratedCampaignRegionalEventPayloadFrameIdentity identity)
    {
        identity = new GeneratedCampaignRegionalEventPayloadFrameIdentity();
        var parts = value.Split('|');
        if (parts.Length != 6 || parts[0] != FrameSchema
            || !Enum.TryParse<
                GeneratedCampaignRegionalEventReplayRouteKind>(
                parts[2], out var route)
            || !int.TryParse(parts[3],
                System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture,
                out var replay)
            || !int.TryParse(parts[4],
                System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture,
                out var sequence)
            || string.IsNullOrWhiteSpace(parts[1])
            || string.IsNullOrWhiteSpace(parts[5]))
            return false;
        identity = new GeneratedCampaignRegionalEventPayloadFrameIdentity
        {
            RegionalEventId = parts[1],
            RouteKind = route,
            ReplayIndex = replay,
            SequenceIndex = sequence,
            CommandIdentity = parts[5]
        };
        return true;
    }

    private static string SignatureKey(
        GeneratedCampaignRegionalEventReplaySignature signature) =>
        string.Join("|",
            signature.RegionalEventId,
            signature.RouteKind,
            signature.ReplayIndex.ToString(
                System.Globalization.CultureInfo.InvariantCulture));

    private static string AuthoritySha256(
        GeneratedCampaignRegionalEventPayloadAuthority authority) =>
        GeneratedCampaignChoiceCanonical.Hash(authority with
        {
            AuthoritySha256 = string.Empty
        });
}
