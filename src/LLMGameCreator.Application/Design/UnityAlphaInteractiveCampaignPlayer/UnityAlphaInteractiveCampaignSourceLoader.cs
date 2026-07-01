using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.UnityAlphaInteractiveCampaignPlayer;

public sealed class UnityAlphaInteractiveCampaignSourceLoader
{
    private const string Goal070Root = UnityAlphaInteractiveCampaignVocabulary.Goal070RelativeOutputDirectory;

    public InteractiveCampaignSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<InteractiveCampaignDiagnostic>();
        var sourceRefs = new List<InteractiveCampaignSourceArtifactReference>();

        if (!Directory.Exists(Resolve(projectRoot, Goal070Root)))
        {
            diagnostics.Add(Error("goal071.source.goal070_folder_missing", Goal070Root, "Goal 070 source artifact folder is required."));
        }

        var sourceManifest = ReadRoot(projectRoot, Goal070Root + "/source-manifest.json", "Goal070", "source_manifest", ["accepted", "rowCount", "preflightGates"], sourceRefs, diagnostics);
        var matrix = ReadRoot(projectRoot, Goal070Root + "/timeline-matrix-summary.json", "Goal070", "timeline_matrix_summary", ["passed", "rowCount", "rows"], sourceRefs, diagnostics);
        _ = ReadRoot(projectRoot, Goal070Root + "/save-load-replay-audit.json", "Goal070", "save_load_replay_audit", ["passed", "rows"], sourceRefs, diagnostics);
        _ = ReadRoot(projectRoot, Goal070Root + "/unity-command-plan.json", "Goal070", "unity_command_plan", ["passed", "rows", "expectedPlayerMarkers"], sourceRefs, diagnostics);
        var unityProof = ReadRoot(projectRoot, Goal070Root + "/unity-player-proof-summary.json", "Goal070", "unity_player_proof_summary", ["passed", "playerExecuted", "provenRowCount", "matchedMarkers"], sourceRefs, diagnostics);
        AddReportRef(projectRoot, sourceRefs, diagnostics);

        var rows = ReadTimelineRows(matrix, diagnostics)
            .OrderBy(item => UnityAlphaInteractiveCampaignVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => UnityAlphaInteractiveCampaignVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ToList();

        var handoffText = ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.json")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CONTEXT_INDEX.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var goal070Accepted = handoffText.Contains("integrated_campaign_timeline_simulation_matrix_verification passed before Goal 071", StringComparison.Ordinal);
        if (!goal070Accepted)
        {
            diagnostics.Add(Error("goal071.preflight.goal070_handoff_missing", "docs/CURRENT_GENERATOR_STATE.*", "Goal 070 user-handoff acceptance must be recorded before Goal 071."));
        }

        var duplicateRows = rows
            .GroupBy(item => item.RowId, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        foreach (var rowId in duplicateRows)
        {
            diagnostics.Add(Error("goal071.source.duplicate_row_id", rowId, "Goal 071 source row ids must be unique."));
        }

        foreach (var row in rows)
        {
            if (!UnityAlphaInteractiveCampaignVocabulary.FamilyIds.Contains(row.FamilyId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal071.source.fake_family_id", row.FamilyId, "Goal 071 accepts only the three proven family ids from Goal 070."));
            }

            if (!UnityAlphaInteractiveCampaignVocabulary.SeedIds.Contains(row.SeedId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal071.source.fake_seed_id", row.SeedId, "Goal 071 accepts only seed_alpha, seed_beta and seed_gamma from Goal 070."));
            }

            if (!row.StateChanging || row.Goal070InitialStateHash == row.Goal070FinalStateHash)
            {
                diagnostics.Add(Error("goal071.source.goal070_row_not_state_changing", row.RowId, "Goal 070 source row must have a state-changing final state."));
            }

            if (row.Steps.Count < 2 || row.Steps.Any(step => step.StateBeforeHash == step.StateAfterHash))
            {
                diagnostics.Add(Error("goal071.source.timeline_steps_missing", row.RowId, "Goal 071 requires state-changing multi-step timeline source rows."));
            }
        }

        var familyIds = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).OrderBy(UnityAlphaInteractiveCampaignVocabulary.FamilyOrderingKey, StringComparer.Ordinal).ToList();
        var seedIds = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(UnityAlphaInteractiveCampaignVocabulary.SeedOrderingKey, StringComparer.Ordinal).ToList();
        var goal070TimelineConsumed = rows.Count == 9
            && familyIds.Count == 3
            && seedIds.Count == 3
            && duplicateRows.Count == 0
            && rows.All(item => item.StateChanging && item.SaveLoadReplayPassed && item.Steps.Count >= 2);
        var goal070UnityConsumed = Bool(unityProof, "passed")
            && Bool(unityProof, "playerExecuted")
            && Int(unityProof, "provenRowCount") == 9
            && ReadStringArray(unityProof, "matchedMarkers").Contains("review_package_proof=goal070", StringComparer.Ordinal);

        return new InteractiveCampaignSourceBundle
        {
            Goal070AcceptedByUserHandoff = goal070Accepted,
            Goal070TimelineEvidenceConsumed = goal070TimelineConsumed,
            Goal070UnityProofConsumed = goal070UnityConsumed,
            FamilyIds = familyIds,
            SeedIds = seedIds,
            Rows = rows,
            BaseStagingFiles = LoadGoal070StagingFiles(projectRoot, diagnostics),
            SourceArtifactRefs = sourceRefs.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<InteractiveCampaignDiagnostic> SortDiagnostics(IEnumerable<InteractiveCampaignDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    public static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static JsonElement ReadRoot(
        string projectRoot,
        string relativePath,
        string sourceGoal,
        string artifactFamily,
        IReadOnlyList<string> requiredFields,
        List<InteractiveCampaignSourceArtifactReference> refs,
        List<InteractiveCampaignDiagnostic> diagnostics)
    {
        refs.Add(FileRef(projectRoot, sourceGoal, artifactFamily, relativePath, requiredFields));
        var path = Resolve(projectRoot, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(Error("goal071.source.required_artifact_missing", relativePath, "Required Goal 070 artifact is missing."));
            return default;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            return document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Error("goal071.source.json_invalid", relativePath, exception.Message));
            return default;
        }
    }

    private static InteractiveCampaignSourceArtifactReference FileRef(
        string projectRoot,
        string sourceGoal,
        string artifactFamily,
        string relativePath,
        IReadOnlyList<string> requiredFields)
    {
        var normalized = Normalize(relativePath);
        var path = Resolve(projectRoot, normalized);
        var exists = File.Exists(path);
        var diagnostics = new List<InteractiveCampaignDiagnostic>();
        var hash = string.Empty;
        var fieldsPresent = requiredFields.Count == 0;
        if (!exists)
        {
            diagnostics.Add(Error("goal071.source.required_artifact_missing", normalized, "Required Goal 070 artifact is missing."));
        }
        else
        {
            var bytes = File.ReadAllBytes(path);
            hash = UnityAlphaInteractiveCampaignHash.Sha256(bytes);
            if (requiredFields.Count > 0)
            {
                try
                {
                    using var document = JsonDocument.Parse(bytes);
                    fieldsPresent = requiredFields.All(field => document.RootElement.TryGetProperty(field, out _));
                    if (!fieldsPresent)
                    {
                        diagnostics.Add(Error("goal071.source.required_field_missing", normalized, "Required top-level source fields are missing."));
                    }
                }
                catch (JsonException exception)
                {
                    diagnostics.Add(Error("goal071.source.json_invalid", normalized, exception.Message));
                }
            }
        }

        return new InteractiveCampaignSourceArtifactReference
        {
            SourceGoal = sourceGoal,
            ArtifactFamily = artifactFamily,
            ArtifactRelativePath = normalized,
            ArtifactHash = hash,
            Exists = exists,
            HashMatches = exists && fieldsPresent && !string.IsNullOrWhiteSpace(hash),
            RequiredFields = requiredFields,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static void AddReportRef(
        string projectRoot,
        List<InteractiveCampaignSourceArtifactReference> sourceRefs,
        List<InteractiveCampaignDiagnostic> diagnostics)
    {
        var root = Resolve(projectRoot, Goal070Root);
        var match = Directory.Exists(root)
            ? Directory.EnumerateFiles(root, "*report.md", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.Ordinal)
                .FirstOrDefault()
            : null;
        if (string.IsNullOrWhiteSpace(match))
        {
            diagnostics.Add(Error("goal071.source.goal070_report_missing", Goal070Root + "/*report.md", "Goal 070 compact report is required."));
            sourceRefs.Add(FileRef(projectRoot, "Goal070", "report", Goal070Root + "/*report.md", []));
            return;
        }

        sourceRefs.Add(FileRef(projectRoot, "Goal070", "report", Normalize(Path.GetRelativePath(projectRoot, match)), []));
    }

    private static IReadOnlyList<InteractiveCampaignSourceRow> ReadTimelineRows(JsonElement matrix, List<InteractiveCampaignDiagnostic> diagnostics)
    {
        if (matrix.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return [];
        }

        if (!matrix.TryGetProperty("rows", out var rowsElement) || rowsElement.ValueKind != JsonValueKind.Array)
        {
            diagnostics.Add(Error("goal071.source.goal070_rows_missing", "timeline-matrix-summary.json#rows", "Goal 070 row matrix is missing."));
            return [];
        }

        var rows = new List<InteractiveCampaignSourceRow>();
        foreach (var rowElement in rowsElement.EnumerateArray())
        {
            var rowId = Text(rowElement, "rowId");
            if (string.IsNullOrWhiteSpace(rowId))
            {
                diagnostics.Add(Error("goal071.source.row_id_missing", "timeline-matrix-summary.json", "Goal 070 row id is required."));
                continue;
            }

            var steps = ReadSteps(rowElement, rowId, diagnostics);
            var replay = rowElement.TryGetProperty("saveLoadReplayProof", out var replayElement) ? replayElement : default;
            rows.Add(new InteractiveCampaignSourceRow
            {
                RowId = rowId,
                FamilyId = Text(rowElement, "familyId"),
                SeedId = Text(rowElement, "seedId"),
                Goal070RowHash = Text(rowElement, "rowHash"),
                Goal070InitialStateHash = NestedText(rowElement, "initialState", "stateHash"),
                Goal070FinalStateHash = NestedText(replay, "finalStateHash"),
                UpstreamRefs = ReadStringArray(rowElement, "upstreamRefs"),
                Steps = steps,
                StateChanging = Bool(rowElement, "stateChanging"),
                SaveLoadReplayPassed = Bool(replay, "saveLoadRoundtripPassed") && Bool(replay, "replayDeterminismPassed")
            });
        }

        return rows;
    }

    private static IReadOnlyList<InteractiveCampaignSourceStep> ReadSteps(JsonElement rowElement, string rowId, List<InteractiveCampaignDiagnostic> diagnostics)
    {
        if (!rowElement.TryGetProperty("ticks", out var ticks) || ticks.ValueKind != JsonValueKind.Array)
        {
            diagnostics.Add(Error("goal071.source.goal070_ticks_missing", rowId, "Goal 070 row ticks are required for interactive step transitions."));
            return [];
        }

        return ticks.EnumerateArray()
            .Select(tick => new InteractiveCampaignSourceStep
            {
                StepId = Text(tick, "tickId"),
                Order = Int(tick, "order"),
                SourceRef = Text(tick, "sourceRef"),
                StateBeforeHash = NestedText(tick, "beforeState", "stateHash"),
                StateAfterHash = NestedText(tick, "afterState", "stateHash"),
                DeltaIds = ReadDeltaIds(tick)
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.StepId))
            .OrderBy(item => item.Order)
            .ThenBy(item => item.StepId, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> ReadDeltaIds(JsonElement tick)
    {
        if (!tick.TryGetProperty("deltas", out var deltas) || deltas.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return deltas.EnumerateArray()
            .Select(delta => Text(delta, "deltaId"))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<InteractiveCampaignFilePayload> LoadGoal070StagingFiles(string projectRoot, List<InteractiveCampaignDiagnostic> diagnostics)
    {
        var root = Resolve(projectRoot, Goal070Root + "/" + UnityAlphaInteractiveCampaignVocabulary.StagingRoot);
        if (!Directory.Exists(root))
        {
            diagnostics.Add(Error("goal071.source.goal070_staging_missing", Goal070Root + "/staging", "Goal 070 staging files are required for Goal 071 Unity Alpha proof."));
            return [];
        }

        var files = new List<InteractiveCampaignFilePayload>();
        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal))
        {
            var relative = Normalize(Path.GetRelativePath(root, path));
            if (!IsSafeRelativePath(relative))
            {
                diagnostics.Add(Error("goal071.source.unsafe_staging_path", relative, "Goal 070 staging path is not safe to reuse."));
                continue;
            }

            files.Add(new InteractiveCampaignFilePayload
            {
                RelativePath = relative,
                Bytes = File.ReadAllBytes(path)
            });
        }

        return files;
    }

    private static string ReadOptionalText(string projectRoot, string relativePath)
    {
        var path = Resolve(projectRoot, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Resolve(string projectRoot, string relativePath) =>
        Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Normalize(string path) =>
        path.Replace('\\', '/');

    private static string Text(JsonElement element, string propertyName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return string.Empty;
        }

        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string NestedText(JsonElement element, string objectProperty, string valueProperty)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return string.Empty;
        }

        return element.TryGetProperty(objectProperty, out var nested) ? Text(nested, valueProperty) : string.Empty;
    }

    private static string NestedText(JsonElement element, string valueProperty) =>
        Text(element, valueProperty);

    private static int Int(JsonElement element, string propertyName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return 0;
        }

        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
            ? value
            : 0;
    }

    private static bool Bool(JsonElement element, string propertyName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return false;
        }

        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.True;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
            || !element.TryGetProperty(propertyName, out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return array.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            _ => 2
        };

    private static InteractiveCampaignDiagnostic Error(string code, string target, string message) =>
        InteractiveCampaignDiagnostic.Error(code, target, message);
}
