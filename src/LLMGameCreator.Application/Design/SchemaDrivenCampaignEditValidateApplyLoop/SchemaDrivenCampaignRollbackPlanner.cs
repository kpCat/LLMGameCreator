namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignRollbackPlanner
{
    public PreviewExportRefreshPayload BuildPreviewExportRefreshPayload(RowBeforeAfterDiffMatrix diffMatrix)
    {
        var rows = diffMatrix.Rows
            .OrderBy(row => SchemaDrivenCampaignEditVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => SchemaDrivenCampaignEditVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row => new PreviewExportChangedRow
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                RefreshKey = "goal075.refresh." + row.FamilyId + "." + row.SeedId,
                AfterHash = row.AfterHash,
                ChangedFieldIds = row.ChangedFields
                    .Select(field => field.FieldId)
                    .OrderBy(fieldId => fieldId, StringComparer.Ordinal)
                    .ToList()
            })
            .ToList();

        return new PreviewExportRefreshPayload
        {
            Passed = rows.Count == 9 && rows.All(row => row.ChangedFieldIds.Count > 0),
            ChangedRowCount = rows.Count,
            ChangedRowIds = rows.Select(row => row.RowId).ToList(),
            ChangedDomains = rows
                .SelectMany(row => row.ChangedFieldIds)
                .Select(FieldDomainFromId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(domain => domain, StringComparer.Ordinal)
                .ToList(),
            Rows = rows
        };
    }

    private static string FieldDomainFromId(string fieldId) =>
        fieldId switch
        {
            "gameplay_consequence_summary.consequence_intensity" => "gameplay_consequence",
            "living_world_npc_faction_summary.faction_pressure" => "living_world_faction",
            "settlement_construction_destruction_production_summary.production_focus" => "settlement_production",
            "narrative_quest_dialogue_event_summary.event_intent" => "narrative_event_intent",
            "combat_magic_boss_summary.status_pressure" => "combat_magic_status",
            "weather_daynight_crisis_summary.crisis_pressure" => "weather_crisis_pressure",
            _ => "unknown"
        };
}
