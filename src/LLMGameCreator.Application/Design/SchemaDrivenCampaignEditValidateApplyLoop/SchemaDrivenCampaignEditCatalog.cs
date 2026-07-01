namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignEditCatalog
{
    private static readonly IReadOnlyList<string> ManualFieldRotation =
    [
        "gameplay_consequence_summary.consequence_intensity",
        "settlement_construction_destruction_production_summary.production_focus",
        "combat_magic_boss_summary.status_pressure"
    ];

    private static readonly IReadOnlyList<string> AutoFieldRotation =
    [
        "living_world_npc_faction_summary.faction_pressure",
        "narrative_quest_dialogue_event_summary.event_intent",
        "weather_daynight_crisis_summary.crisis_pressure"
    ];

    public EditableSchemaFieldCatalog BuildFieldCatalog() =>
        new()
        {
            Passed = true,
            Fields = Fields(),
            FieldCount = Fields().Count
        };

    public ChangeSetCatalog BuildChangeSetCatalog(
        CampaignEditSourceBundle source,
        EditableSchemaFieldCatalog catalog)
    {
        var fields = catalog.Fields.ToDictionary(field => field.FieldId, StringComparer.Ordinal);
        var candidates = new List<CampaignChangeSetCandidate>();
        foreach (var row in source.Rows)
        {
            var seedIndex = SeedIndex(row.SeedId);
            candidates.Add(BuildCandidate(row, fields[ManualFieldRotation[seedIndex]], "manual"));
            candidates.Add(BuildCandidate(row, fields[AutoFieldRotation[seedIndex]], "auto_suggestion"));
        }

        var ordered = candidates
            .OrderBy(item => SchemaDrivenCampaignEditVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SchemaDrivenCampaignEditVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ThenBy(item => item.CandidateKind, StringComparer.Ordinal)
            .ThenBy(item => item.FieldId, StringComparer.Ordinal)
            .ToList();

        return new ChangeSetCatalog
        {
            Passed = ordered.Count == 18,
            RowCount = source.Rows.Count,
            CandidateCount = ordered.Count,
            ManualCandidateCount = ordered.Count(item => item.CandidateKind == "manual"),
            AutoSuggestionCandidateCount = ordered.Count(item => item.CandidateKind == "auto_suggestion"),
            Candidates = ordered
        };
    }

    public static IReadOnlyDictionary<string, string> BuildInitialValues(
        CampaignEditSourceRow row,
        EditableSchemaFieldCatalog catalog)
    {
        var familyIndex = FamilyIndex(row.FamilyId);
        var seedIndex = SeedIndex(row.SeedId);
        return catalog.Fields
            .OrderBy(field => field.FieldId, StringComparer.Ordinal)
            .Select((field, index) => new
            {
                field.FieldId,
                Value = field.AllowedValues[(familyIndex + seedIndex + index) % field.AllowedValues.Count]
            })
            .ToDictionary(item => item.FieldId, item => item.Value, StringComparer.Ordinal);
    }

    private static CampaignChangeSetCandidate BuildCandidate(
        CampaignEditSourceRow row,
        EditableSchemaField field,
        string kind)
    {
        var values = BuildInitialValues(row, new EditableSchemaFieldCatalog { Fields = Fields() });
        var beforeValue = values[field.FieldId];
        var proposedValue = NextValue(field, beforeValue, kind == "manual" ? 1 : 2);
        var candidateId = kind + "-" + SchemaDrivenCampaignEditHash.SafeSegment(row.RowId)
            + "-" + SchemaDrivenCampaignEditHash.SafeSegment(field.FieldId);

        return new CampaignChangeSetCandidate
        {
            CandidateId = candidateId,
            CandidateKind = kind,
            CandidateState = "candidate",
            ValidatedBeforeApply = true,
            RowId = row.RowId,
            FamilyId = row.FamilyId,
            SeedId = row.SeedId,
            SourceFamilyId = row.FamilyId,
            FieldId = field.FieldId,
            FieldDomain = field.DomainId,
            ProposedValueKind = field.ValueShape,
            BeforeValue = beforeValue,
            ProposedValue = proposedValue,
            ProvenanceKind = kind == "manual" ? "manual_user" : "deterministic_auto_suggestion",
            EvidenceRef = SchemaDrivenCampaignEditVocabulary.RelativeOutputDirectory + "/change-set-catalog.json",
            RollbackTargetRowId = row.RowId,
            ExpectedBeforeHash = HashRow(row, values),
            ExpectedAfterHash = HashRow(row, values.Set(field.FieldId, proposedValue))
        };
    }

    private static string NextValue(EditableSchemaField field, string beforeValue, int offset)
    {
        var index = IndexOf(field.AllowedValues, beforeValue);
        if (index < 0)
        {
            return field.AllowedValues[0];
        }

        return field.AllowedValues[(index + offset) % field.AllowedValues.Count];
    }

    private static IReadOnlyList<EditableSchemaField> Fields() =>
    [
        Field(
            "gameplay_consequence_summary.consequence_intensity",
            "gameplay_consequence_summary",
            "gameplay_consequence",
            ["low", "medium", "high"]),
        Field(
            "living_world_npc_faction_summary.faction_pressure",
            "living_world_npc_faction_summary",
            "living_world_faction",
            ["neutral", "tense", "allied"]),
        Field(
            "settlement_construction_destruction_production_summary.production_focus",
            "settlement_construction_destruction_production_summary",
            "settlement_production",
            ["repair", "defense", "growth"]),
        Field(
            "narrative_quest_dialogue_event_summary.event_intent",
            "narrative_quest_dialogue_event_summary",
            "narrative_event_intent",
            ["investigate", "rescue", "trade"]),
        Field(
            "combat_magic_boss_summary.status_pressure",
            "combat_magic_boss_summary",
            "combat_magic_status",
            ["burn", "freeze", "shock"]),
        Field(
            "weather_daynight_crisis_summary.crisis_pressure",
            "weather_daynight_crisis_summary",
            "weather_crisis_pressure",
            ["calm", "storm", "crisis"])
    ];

    private static EditableSchemaField Field(
        string fieldId,
        string groupId,
        string domainId,
        IReadOnlyList<string> allowedValues) =>
        new()
        {
            FieldId = fieldId,
            SchemaGroupId = groupId,
            DomainId = domainId,
            ValueShape = "enum",
            AllowedValues = allowedValues,
            Editable = true,
            FinalProseAllowed = false,
            SourcePath = "goal074.dynamicSchema.groups." + groupId
        };

    private static int FamilyIndex(string familyId) =>
        IndexOf(SchemaDrivenCampaignEditVocabulary.FamilyIds, familyId) is var index && index >= 0 ? index : 0;

    private static int SeedIndex(string seedId) =>
        IndexOf(SchemaDrivenCampaignEditVocabulary.SeedIds, seedId) is var index && index >= 0 ? index : 0;

    private static int IndexOf(IReadOnlyList<string> values, string value)
    {
        for (var index = 0; index < values.Count; index++)
        {
            if (string.Equals(values[index], value, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    public static string HashRow(CampaignEditSourceRow row, IReadOnlyDictionary<string, string> values) =>
        SchemaDrivenCampaignEditHash.Sha256(SchemaDrivenCampaignEditHash.Serialize(new
        {
            row.RowId,
            row.FamilyId,
            row.SeedId,
            row.PackageHash,
            row.InteractiveRowHash,
            values = values.OrderBy(item => item.Key, StringComparer.Ordinal)
        }));
}

internal static class CampaignEditDictionaryExtensions
{
    public static IReadOnlyDictionary<string, string> Set(
        this IReadOnlyDictionary<string, string> source,
        string key,
        string value)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in source)
        {
            result[pair.Key] = pair.Value;
        }

        result[key] = value;

        return result;
    }
}
