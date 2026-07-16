using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GameProjectSeedRegenerationDiffService
{
    private static readonly JsonSerializerOptions HashJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public GameProjectSeedRegenerationDiff Compare(
        SeededGeneratedProjectSourceValidationResult current,
        SeededGeneratedProjectSourceValidationResult candidate,
        bool authoringPreserved,
        bool projectIdentityPreserved)
    {
        Require(current);
        Require(candidate);
        var oldSource = current.Source!;
        var newSource = candidate.Source!;
        var oldRecords = Index(current.Overlay!.GeneratedRecords);
        var newRecords = Index(candidate.Overlay!.GeneratedRecords);
        var added = newRecords.Keys.Except(oldRecords.Keys, StringComparer.Ordinal).ToList();
        var removed = oldRecords.Keys.Except(newRecords.Keys, StringComparer.Ordinal).ToList();
        var common = oldRecords.Keys.Intersect(newRecords.Keys, StringComparer.Ordinal).ToList();
        var changed = common.Where(key => !string.Equals(oldRecords[key].CanonicalSha256,
            newRecords[key].CanonicalSha256, StringComparison.Ordinal)).ToList();
        var unchanged = common.Count - changed.Count;
        var oldPlan = current.RegeneratedPlan!;
        var newPlan = candidate.RegeneratedPlan!;
        var diagnostics = new List<string>();
        var gameplayChanged = !string.Equals(oldSource.PlanSha256, newSource.PlanSha256, StringComparison.Ordinal)
                              || !string.Equals(oldSource.GeneratedOverlaySha256,
                                  newSource.GeneratedOverlaySha256, StringComparison.Ordinal)
                              || !string.Equals(oldSource.GeneratedBasePackageSha256,
                                  newSource.GeneratedBasePackageSha256, StringComparison.Ordinal)
                              || added.Count + removed.Count + changed.Count > 0;
        if (!gameplayChanged) diagnostics.Add("regeneration.no_gameplay_change");
        if (!authoringPreserved) diagnostics.Add("regeneration.authoring_not_preserved");
        if (!projectIdentityPreserved) diagnostics.Add("regeneration.identity_not_preserved");
        return new GameProjectSeedRegenerationDiff
        {
            OldSeed = oldSource.Seed,
            NewSeed = newSource.Seed,
            OldMode = oldSource.Mode,
            NewMode = newSource.Mode,
            OldPresetId = oldSource.PresetId,
            NewPresetId = newSource.PresetId,
            OldSourceRequestSha256 = RequestSha256(oldSource.GenerationRequest),
            NewSourceRequestSha256 = RequestSha256(newSource.GenerationRequest),
            OldPlanSha256 = oldSource.PlanSha256,
            NewPlanSha256 = newSource.PlanSha256,
            OldOverlaySha256 = oldSource.GeneratedOverlaySha256,
            NewOverlaySha256 = newSource.GeneratedOverlaySha256,
            OldGeneratedBaseSha256 = oldSource.GeneratedBasePackageSha256,
            NewGeneratedBaseSha256 = newSource.GeneratedBasePackageSha256,
            OldCounts = Counts(oldSource.Counts),
            NewCounts = Counts(newSource.Counts),
            AddedRecordCount = added.Count,
            RemovedRecordCount = removed.Count,
            ChangedRecordCount = changed.Count,
            UnchangedRecordCount = unchanged,
            AddedByCollection = Group(added, newRecords),
            RemovedByCollection = Group(removed, oldRecords),
            ChangedByCollection = Group(changed, newRecords),
            OldStartRegionTitle = StartTitle(oldPlan),
            NewStartRegionTitle = StartTitle(newPlan),
            OldTravelDestinationTitle = DestinationTitle(oldPlan),
            NewTravelDestinationTitle = DestinationTitle(newPlan),
            GameplayChanged = gameplayChanged,
            AuthoringPreserved = authoringPreserved,
            ProjectIdentityPreserved = projectIdentityPreserved,
            Diagnostics = diagnostics
        };
    }

    public static string RequestSha256(SeededGeneratedProjectGenerationRequest request)
    {
        var normalized = SeededGeneratedProjectSourceService.NormalizeRequest(request);
        return SeededGeneratedProjectSourceService.HashText(JsonSerializer.Serialize(normalized, HashJsonOptions));
    }

    private static void Require(SeededGeneratedProjectSourceValidationResult source)
    {
        if (source is not { Present: true, Passed: true, Source: not null, Overlay: not null, RegeneratedPlan: not null })
            throw new InvalidOperationException("regeneration.diff_source_invalid");
    }

    private static Dictionary<string, GeneratedProjectRecordFingerprint> Index(
        IReadOnlyList<GeneratedProjectRecordFingerprint> records) => records.ToDictionary(
        item => item.CollectionPath + "|" + item.RecordId,
        StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, int> Group(
        IEnumerable<string> keys,
        IReadOnlyDictionary<string, GeneratedProjectRecordFingerprint> records) =>
        new SortedDictionary<string, int>(keys.Select(key => records[key].CollectionPath)
            .GroupBy(value => value, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal), StringComparer.Ordinal);

    private static GameProjectSeedRegenerationCollectionCounts Counts(GeneratedProjectCounts counts) => new()
    {
        Regions = counts.Regions,
        Factions = counts.Factions,
        Actors = counts.Actors,
        ItemsAndResources = counts.ItemsAndResources,
        Encounters = counts.Encounters,
        QuestEvents = counts.QuestEvents
    };

    private static string StartTitle(ProceduralGeneratedGamePlan plan) =>
        plan.World.Regions.FirstOrDefault()?.Label ?? string.Empty;

    private static string DestinationTitle(ProceduralGeneratedGamePlan plan)
    {
        var destination = plan.World.Connections.FirstOrDefault()?.ToRegionId;
        return plan.World.Regions.SingleOrDefault(region => region.RegionId == destination)?.Label
               ?? plan.World.Regions.Skip(1).FirstOrDefault()?.Label
               ?? string.Empty;
    }
}
