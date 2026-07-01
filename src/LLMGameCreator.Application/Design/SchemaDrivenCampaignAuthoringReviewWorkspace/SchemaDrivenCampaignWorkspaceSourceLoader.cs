using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

public sealed class SchemaDrivenCampaignWorkspaceSourceLoader
{
    private sealed record SourcePlan(
        string SourceGoal,
        string ArtifactFamily,
        string SchemaGroupId,
        string RelativePath,
        IReadOnlyList<string> RequiredFields);

    private const string Goal060Root =
        ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    private const string Goal061Root =
        ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc";
    private const string Goal062Root =
        ".llmgc/procedural/goal-062-constrained-spatial-detail-generation";
    private const string Goal063Root =
        ".llmgc/procedural/goal-063-gameplay-consequence-depth-matrix";
    private const string Goal064Root =
        ".llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix";
    private const string Goal065Root =
        ".llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix";
    private const string Goal066Root =
        ".llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix";
    private const string Goal067Root =
        ".llmgc/procedural/goal-067-programmatic-narrative-quest-dialogue-event-matrix";
    private const string Goal068Root =
        ".llmgc/procedural/goal-068-combat-magic-ability-boss-encounter-matrix";
    private const string Goal069Root =
        ".llmgc/procedural/goal-069-world-event-weather-daynight-crisis-matrix";
    private const string Goal070Root =
        ".llmgc/procedural/goal-070-integrated-campaign-timeline-simulation-matrix";
    private const string Goal071Root =
        ".llmgc/procedural/goal-071-unity-alpha-interactive-campaign-player";
    private const string Goal072Root =
        ".llmgc/procedural/goal-072-generator-spine-quality-consolidation";
    private const string Goal073Root =
        ".llmgc/procedural/goal-073-source-format-p0-readability-repair";

    private static readonly IReadOnlyList<SourcePlan> SourcePlans =
    [
        Plan("Goal060", "materialized_package_inventory", "package_materialization_summary",
            Goal060Root + "/materialized-package-inventory.json", ["packages"]),
        Plan("Goal060", "package_validation_matrix", "package_materialization_summary",
            Goal060Root + "/package-validation-matrix.json", ["passed"]),
        Plan("Goal060", "runtime_consumption_matrix", "package_materialization_summary",
            Goal060Root + "/runtime-consumption-matrix.json", ["passed", "rows"]),
        Plan("Goal060", "preview_export_package_payloads", "package_materialization_summary",
            Goal060Root + "/preview-export-package-payloads.json", ["rows"]),
        Plan("Goal061", "review_package_manifest", "package_materialization_summary",
            Goal061Root + "/review-package-rc-manifest.json", ["schemaVersion"]),
        Plan("Goal061", "package_row_selection_matrix", "campaign_rows_selector",
            Goal061Root + "/package-row-selection-matrix.json", ["passed", "rows"]),
        Plan("Goal061", "save_load_replay_package_row_audit", "package_materialization_summary",
            Goal061Root + "/save-load-replay-package-row-audit.json", ["passed", "rows"]),
        Plan("Goal061", "package_media_binding_audit", "package_materialization_summary",
            Goal061Root + "/package-media-binding-audit.json", ["passed"]),
        Plan("Goal062", "spatial_detail_matrix", "spatial_detail_summary",
            Goal062Root + "/spatial-detail-matrix.json", ["passed", "rows"]),
        Plan("Goal062", "reachability_proof_matrix", "spatial_detail_summary",
            Goal062Root + "/reachability-proof-matrix.json", ["passed"]),
        Plan("Goal062", "preview_export_spatial_payload", "spatial_detail_summary",
            Goal062Root + "/preview-export-spatial-payload.json", ["rows"]),
        Plan("Goal063", "runtime_state_delta_matrix", "gameplay_consequence_summary",
            Goal063Root + "/runtime-state-delta-matrix.json", ["passed", "rows"]),
        Plan("Goal063", "gameplay_command_plan_matrix", "gameplay_consequence_summary",
            Goal063Root + "/gameplay-command-plan-matrix.json", ["passed", "rows"]),
        Plan("Goal063", "save_load_replay_audit", "gameplay_consequence_summary",
            Goal063Root + "/save-load-replay-audit.json", ["passed", "rows"]),
        Plan("Goal064", "simulation_matrix_plan", "living_world_npc_faction_summary",
            Goal064Root + "/simulation-matrix-plan.json", ["passed", "rows"]),
        Plan("Goal064", "actor_faction_catalog_summary", "living_world_npc_faction_summary",
            Goal064Root + "/actor-faction-catalog-summary.json", ["passed"]),
        Plan("Goal065", "row_plan_matrix", "economy_crafting_combat_progression_status_summary",
            Goal065Root + "/row-plan-matrix.json", ["passed", "rows"]),
        Plan("Goal065", "economy_crafting_ledger", "economy_crafting_combat_progression_status_summary",
            Goal065Root + "/economy-crafting-ledger.json", ["passed"]),
        Plan("Goal065", "combat_progression_ledger", "economy_crafting_combat_progression_status_summary",
            Goal065Root + "/combat-progression-ledger.json", ["passed"]),
        Plan("Goal065", "status_effect_ledger", "economy_crafting_combat_progression_status_summary",
            Goal065Root + "/status-effect-ledger.json", ["passed"]),
        Plan("Goal066", "settlement_row_matrix", "settlement_construction_destruction_production_summary",
            Goal066Root + "/settlement-construction-row-matrix.json", ["passed", "rows"]),
        Plan("Goal066", "settlement_production_ledger", "settlement_construction_destruction_production_summary",
            Goal066Root + "/settlement-production-ledger.json", ["passed"]),
        Plan("Goal066", "settlement_destruction_repair_ledger",
            "settlement_construction_destruction_production_summary",
            Goal066Root + "/settlement-destruction-repair-ledger.json", ["passed"]),
        Plan("Goal067", "narrative_row_matrix", "narrative_quest_dialogue_event_summary",
            Goal067Root + "/narrative-row-matrix.json", ["passed", "rows"]),
        Plan("Goal067", "quest_stage_ledger", "narrative_quest_dialogue_event_summary",
            Goal067Root + "/quest-stage-ledger.json", ["passed"]),
        Plan("Goal067", "dialogue_option_ledger", "narrative_quest_dialogue_event_summary",
            Goal067Root + "/dialogue-option-ledger.json", ["passed"]),
        Plan("Goal067", "localization_key_table", "narrative_quest_dialogue_event_summary",
            Goal067Root + "/localization-key-table.json", ["passed"]),
        Plan("Goal068", "combat_magic_row_matrix", "combat_magic_boss_summary",
            Goal068Root + "/combat-magic-row-matrix.json", ["passed", "rows"]),
        Plan("Goal068", "ability_trait_catalog", "combat_magic_boss_summary",
            Goal068Root + "/ability-trait-catalog.json", ["passed"]),
        Plan("Goal068", "boss_phase_catalog", "combat_magic_boss_summary",
            Goal068Root + "/boss-encounter-phase-catalog.json", ["passed"]),
        Plan("Goal069", "world_event_row_matrix", "weather_daynight_crisis_summary",
            Goal069Root + "/world-event-weather-daynight-row-matrix.json", ["passed", "rows"]),
        Plan("Goal069", "weather_hazard_catalog", "weather_daynight_crisis_summary",
            Goal069Root + "/weather-hazard-catalog.json", ["passed"]),
        Plan("Goal069", "crisis_event_catalog", "weather_daynight_crisis_summary",
            Goal069Root + "/crisis-event-catalog.json", ["passed"]),
        Plan("Goal070", "timeline_matrix_summary", "integrated_timeline_summary",
            Goal070Root + "/timeline-matrix-summary.json", ["passed", "rows"]),
        Plan("Goal070", "cross_system_cascade_ledger", "integrated_timeline_summary",
            Goal070Root + "/cross-system-cascade-ledger.json", ["passed"]),
        Plan("Goal070", "conflict_arbitration_ledger", "integrated_timeline_summary",
            Goal070Root + "/conflict-arbitration-ledger.json", ["passed"]),
        Plan("Goal071", "interactive_campaign_row_matrix", "interactive_campaign_action_script_summary",
            Goal071Root + "/interactive-campaign-row-matrix.json", ["passed", "rows"]),
        Plan("Goal071", "interactive_campaign_selector", "campaign_rows_selector",
            Goal071Root + "/interactive-campaign-family-seed-selector.json", ["passed", "families"]),
        Plan("Goal071", "interactive_campaign_input_script", "interactive_campaign_action_script_summary",
            Goal071Root + "/interactive-campaign-input-script.json", ["passed", "actions"]),
        Plan("Goal071", "interactive_campaign_state_transition_ledger",
            "interactive_campaign_action_script_summary",
            Goal071Root + "/interactive-campaign-state-transition-ledger.json", ["passed", "rows"]),
        Plan("Goal072", "quality_dashboard", "quality_debt_panel",
            Goal072Root + "/quality-dashboard.json", ["status", "p0Count"]),
        Plan("Goal072", "technical_debt_register", "quality_debt_panel",
            Goal072Root + "/technical-debt-register.json", ["findings"]),
        Plan("Goal073", "source_format_repair_summary", "quality_debt_panel",
            Goal073Root + "/source-format-p0-repair-summary.json", ["implementationStatus", "p0AfterCount"]),
        Plan("Goal073", "source_format_repair_report", "quality_debt_panel",
            Goal073Root + "/source-format-p0-readability-repair-report.md", ["source_format_p0_readability_repair_verification"])
    ];

    public CampaignWorkspaceSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        var refs = new List<CampaignWorkspaceSourceArtifactReference>();
        var stats = new List<CampaignWorkspaceSourceArtifactStats>();
        var jsonByPath = new SortedDictionary<string, JsonElement>(StringComparer.Ordinal);

        foreach (var plan in SourcePlans)
        {
            var read = ReadSource(projectRoot, plan);
            refs.Add(read.Reference);
            diagnostics.AddRange(read.Reference.Diagnostics);
            if (read.Root.HasValue)
            {
                jsonByPath[Normalize(plan.RelativePath)] = read.Root.Value;
            }

            stats.Add(BuildStats(read.Reference, read.Root));
        }

        var stateText = ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.json")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CONTEXT_INDEX.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var goal073Accepted = stateText.Contains(
            "source_format_p0_readability_repair_verification passed before Goal 074",
            StringComparison.Ordinal);
        var goal072Blocked = stateText.Contains("generator_spine_quality_consolidation_verification required", StringComparison.Ordinal)
            && stateText.Contains("implementationStatus=BLOCKED", StringComparison.Ordinal);
        var goal031And032Produced = stateText.Contains("semantic_pack_composition_blueprint_verification required", StringComparison.Ordinal)
            && stateText.Contains("dynamic_semantic_feature_system_verification required", StringComparison.Ordinal);

        if (!goal073Accepted)
        {
            diagnostics.Add(Error(
                "goal074.preflight.goal073_handoff_missing",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal 073 user-handoff acceptance is required before Goal 074."));
        }

        if (!goal072Blocked)
        {
            diagnostics.Add(Error(
                "goal074.preflight.goal072_blocked_not_preserved",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal 072 must remain historical BLOCKED/progress evidence."));
        }

        var packageMap = ReadGoal060PackageMap(jsonByPath);
        var rows = ReadGoal071Rows(jsonByPath, packageMap, diagnostics);
        var familyIds = rows
            .Select(item => item.FamilyId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(SchemaDrivenCampaignWorkspaceVocabulary.FamilyOrderingKey, StringComparer.Ordinal)
            .ToList();
        var seedIds = rows
            .Select(item => item.SeedId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(SchemaDrivenCampaignWorkspaceVocabulary.SeedOrderingKey, StringComparer.Ordinal)
            .ToList();

        return new CampaignWorkspaceSourceBundle
        {
            Goal073AcceptedByUserHandoff = goal073Accepted,
            Goal072RemainsHistoricalBlocked = goal072Blocked,
            Goal031And032RemainProducedForReview = goal031And032Produced,
            FamilyIds = familyIds,
            SeedIds = seedIds,
            Rows = rows,
            SourceArtifactRefs = refs.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            ArtifactStats = stats.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<CampaignWorkspaceDiagnostic> SortDiagnostics(
        IEnumerable<CampaignWorkspaceDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static SourcePlan Plan(
        string sourceGoal,
        string artifactFamily,
        string schemaGroupId,
        string relativePath,
        IReadOnlyList<string> requiredFields) =>
        new(sourceGoal, artifactFamily, schemaGroupId, relativePath, requiredFields);

    private static (CampaignWorkspaceSourceArtifactReference Reference, JsonElement? Root) ReadSource(
        string projectRoot,
        SourcePlan plan)
    {
        var normalized = Normalize(plan.RelativePath);
        var path = Resolve(projectRoot, normalized);
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        if (!File.Exists(path))
        {
            diagnostics.Add(Error("goal074.source.required_artifact_missing", normalized, "Required source artifact is missing."));
            return (Reference(plan, normalized, string.Empty, exists: false, hashMatches: false, diagnostics), null);
        }

        var bytes = File.ReadAllBytes(path);
        var text = Encoding.UTF8.GetString(bytes);
        var hash = Hash(bytes);
        var fieldsPresent = plan.RequiredFields.All(field => text.Contains(field, StringComparison.Ordinal));
        if (!fieldsPresent)
        {
            diagnostics.Add(Error(
                "goal074.source.required_field_missing",
                normalized,
                "Required source artifact field was not found."));
        }

        if (!normalized.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return (Reference(plan, normalized, hash, exists: true, hashMatches: fieldsPresent, diagnostics), null);
        }

        try
        {
            using var document = JsonDocument.Parse(bytes);
            return (Reference(plan, normalized, hash, exists: true, hashMatches: fieldsPresent, diagnostics),
                document.RootElement.Clone());
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Error("goal074.source.json_invalid", normalized, exception.Message));
            return (Reference(plan, normalized, hash, exists: true, hashMatches: false, diagnostics), null);
        }
    }

    private static CampaignWorkspaceSourceArtifactReference Reference(
        SourcePlan plan,
        string normalized,
        string hash,
        bool exists,
        bool hashMatches,
        IReadOnlyList<CampaignWorkspaceDiagnostic> diagnostics) =>
        new()
        {
            SourceGoal = plan.SourceGoal,
            ArtifactFamily = plan.ArtifactFamily,
            SchemaGroupId = plan.SchemaGroupId,
            ArtifactRelativePath = normalized,
            ArtifactHash = hash,
            Exists = exists,
            HashMatches = exists && hashMatches,
            RequiredFields = plan.RequiredFields,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static CampaignWorkspaceSourceArtifactStats BuildStats(
        CampaignWorkspaceSourceArtifactReference reference,
        JsonElement? root)
    {
        if (!root.HasValue)
        {
            return new CampaignWorkspaceSourceArtifactStats
            {
                ArtifactRelativePath = reference.ArtifactRelativePath,
                SourceGoal = reference.SourceGoal,
                SchemaGroupId = reference.SchemaGroupId,
                Passed = reference.Exists && reference.HashMatches
            };
        }

        var element = root.Value;
        var rowCount = Int(element, "rowCount");
        var itemCount = 0;
        foreach (var arrayName in new[] { "rows", "packages", "entries", "families", "items", "scenarios", "files" })
        {
            if (TryGetArray(element, arrayName, out var array))
            {
                itemCount = Math.Max(itemCount, array.GetArrayLength());
                if (rowCount == 0 && arrayName is "rows" or "packages")
                {
                    rowCount = array.GetArrayLength();
                }
            }
        }

        return new CampaignWorkspaceSourceArtifactStats
        {
            ArtifactRelativePath = reference.ArtifactRelativePath,
            SourceGoal = reference.SourceGoal,
            SchemaGroupId = reference.SchemaGroupId,
            Passed = Bool(element, "passed") || reference.HashMatches,
            RowCount = rowCount,
            ItemCount = itemCount,
            RepresentativeIds = RepresentativeIds(element)
        };
    }

    private static IReadOnlyDictionary<string, (string Path, string Hash)> ReadGoal060PackageMap(
        IReadOnlyDictionary<string, JsonElement> jsonByPath)
    {
        var result = new SortedDictionary<string, (string Path, string Hash)>(StringComparer.Ordinal);
        if (!jsonByPath.TryGetValue(Goal060Root + "/materialized-package-inventory.json", out var inventory)
            || !TryGetArray(inventory, "packages", out var packages))
        {
            return result;
        }

        foreach (var package in packages.EnumerateArray())
        {
            var rowId = Text(package, "rowId");
            if (!string.IsNullOrWhiteSpace(rowId))
            {
                result[rowId] = (Text(package, "packageRelativePath"), Text(package, "packageHash"));
            }
        }

        return result;
    }

    private static IReadOnlyList<CampaignWorkspaceSourceRow> ReadGoal071Rows(
        IReadOnlyDictionary<string, JsonElement> jsonByPath,
        IReadOnlyDictionary<string, (string Path, string Hash)> packageMap,
        List<CampaignWorkspaceDiagnostic> diagnostics)
    {
        if (!jsonByPath.TryGetValue(Goal071Root + "/interactive-campaign-row-matrix.json", out var matrix)
            || !TryGetArray(matrix, "rows", out var rowsElement))
        {
            diagnostics.Add(Error(
                "goal074.source.goal071_rows_missing",
                Goal071Root + "/interactive-campaign-row-matrix.json",
                "Goal 071 interactive campaign rows are required."));
            return [];
        }

        var rows = new List<CampaignWorkspaceSourceRow>();
        foreach (var row in rowsElement.EnumerateArray())
        {
            var rowId = Text(row, "rowId");
            if (string.IsNullOrWhiteSpace(rowId))
            {
                continue;
            }

            packageMap.TryGetValue(rowId, out var package);
            rows.Add(new CampaignWorkspaceSourceRow
            {
                RowId = rowId,
                FamilyId = Text(row, "familyId"),
                SeedId = Text(row, "seedId"),
                PackageRelativePath = package.Path ?? string.Empty,
                PackageHash = package.Hash ?? string.Empty,
                InteractiveRowHash = Text(row, "rowHash"),
                InitialStateHash = Text(row, "initialStateHash"),
                FinalStateHash = Text(row, "finalStateHash"),
                ActionCount = TryGetArray(row, "actions", out var actions) ? actions.GetArrayLength() : 0,
                StateChanging = Bool(row, "stateChanging"),
                SaveLoadReplayPassed = Bool(row, "saveLoadReplayPassed"),
                SourceRefs = ReadActionSourceRefs(row)
            });
        }

        var ordered = rows
            .OrderBy(item => SchemaDrivenCampaignWorkspaceVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SchemaDrivenCampaignWorkspaceVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ToList();
        if (ordered.Count != 9 || ordered.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() != 9)
        {
            diagnostics.Add(Error(
                "goal074.source.row_count_invalid",
                "Goal071:interactive-campaign-row-matrix",
                "Goal 074 requires nine unique family/seed source rows."));
        }

        return ordered;
    }

    private static IReadOnlyList<string> ReadActionSourceRefs(JsonElement row)
    {
        if (!TryGetArray(row, "actions", out var actions))
        {
            return [];
        }

        return actions.EnumerateArray()
            .Select(action => Text(action, "sourceRef"))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> RepresentativeIds(JsonElement root)
    {
        foreach (var arrayName in new[] { "rows", "packages", "entries", "families", "items", "scenarios" })
        {
            if (!TryGetArray(root, arrayName, out var array))
            {
                continue;
            }

            return array.EnumerateArray()
                .Select(item => Text(item, "rowId"))
                .Concat(array.EnumerateArray().Select(item => Text(item, "id")))
                .Concat(array.EnumerateArray().Select(item => Text(item, "entryId")))
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .Take(5)
                .ToList();
        }

        return [];
    }

    private static bool TryGetArray(JsonElement element, string propertyName, out JsonElement array)
    {
        if (element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propertyName, out array)
            && array.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        array = default;
        return false;
    }

    private static string Text(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool Bool(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True;

    private static int Int(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static string ReadOptionalText(string projectRoot, string relativePath)
    {
        var path = Resolve(projectRoot, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Resolve(string projectRoot, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, path);
        return path;
    }

    private static string Normalize(string path) =>
        path.Replace('\\', '/').TrimStart('/');

    private static void EnsureContained(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + normalizedPath);
        }
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            _ => 2
        };

    private static string Hash(byte[] bytes) =>
        SchemaDrivenCampaignWorkspaceHash.Sha256(bytes);

    private static CampaignWorkspaceDiagnostic Error(string code, string target, string message) =>
        CampaignWorkspaceDiagnostic.Error(code, target, message);
}
