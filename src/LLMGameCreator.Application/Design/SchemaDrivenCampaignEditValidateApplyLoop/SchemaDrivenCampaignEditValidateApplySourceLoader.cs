using System.Text;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignEditValidateApplySourceLoader
{
    private const string Goal074Root =
        ".llmgc/procedural/goal-074-schema-driven-campaign-authoring-review-workspace";

    private static readonly IReadOnlyList<string> Goal074ArtifactFiles =
    [
        "workspace-source-manifest.json",
        "campaign-row-selector.json",
        "dynamic-authoring-schema.json",
        "ui-binding-contract.json",
        "workspace-validation-dashboard.json",
        "review-provenance-ledger.json",
        "authoring-action-plan.json",
        "quality-gate-scan.json",
        "winforms-control-inventory.json",
        "invalid-diagnostics-matrix.json",
        "schema-driven-campaign-authoring-review-workspace-report.md"
    ];

    private readonly SchemaDrivenCampaignWorkspaceEvidenceService _workspaceService = new();

    public CampaignEditSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var workspace = _workspaceService.Build(projectRoot);
        var workspaceSourceRows = new SchemaDrivenCampaignWorkspaceSourceLoader()
            .Load(projectRoot)
            .Rows
            .ToDictionary(row => row.RowId, StringComparer.Ordinal);
        var diagnostics = new List<CampaignEditDiagnostic>();
        var sourceArtifacts = Goal074ArtifactFiles
            .Select(fileName => ReadSourceArtifact(projectRoot, fileName))
            .OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
            .ToList();

        foreach (var artifact in sourceArtifacts.Where(item => !item.Exists))
        {
            diagnostics.Add(CampaignEditDiagnostic.Error(
                "goal075.source.goal074_artifact_missing",
                artifact.ArtifactRelativePath,
                "Goal 075 consumes the Goal 074 workspace evidence and requires this artifact."));
        }

        var stateText = ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.json")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CONTEXT_INDEX.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var goal074Accepted = stateText.Contains(
            "schema_driven_campaign_authoring_review_workspace_verification passed before Goal 075",
            StringComparison.Ordinal);
        var goal072Blocked = stateText.Contains("generator_spine_quality_consolidation_verification required", StringComparison.Ordinal)
            && stateText.Contains("implementationStatus=BLOCKED", StringComparison.Ordinal);
        var goal031And032Produced = stateText.Contains("semantic_pack_composition_blueprint_verification required", StringComparison.Ordinal)
            && stateText.Contains("dynamic_semantic_feature_system_verification required", StringComparison.Ordinal);

        if (!goal074Accepted)
        {
            diagnostics.Add(CampaignEditDiagnostic.Error(
                "goal075.preflight.goal074_handoff_missing",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal 074 user-handoff acceptance is required before Goal 075."));
        }

        if (!goal072Blocked)
        {
            diagnostics.Add(CampaignEditDiagnostic.Error(
                "goal075.preflight.goal072_blocked_not_preserved",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal 072 must remain historical BLOCKED evidence."));
        }

        if (!goal031And032Produced)
        {
            diagnostics.Add(CampaignEditDiagnostic.Error(
                "goal075.preflight.goal031_goal032_state_missing",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal 031 and Goal 032 must remain produced-for-review/not passed."));
        }

        var rows = workspace.RowSelector.Rows
            .OrderBy(row => SchemaDrivenCampaignEditVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => SchemaDrivenCampaignEditVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row => new CampaignEditSourceRow
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                PackageRelativePath = row.PackageRelativePath,
                InteractiveRowHash = row.InteractiveRowHash,
                InitialStateHash = SourceRow(workspaceSourceRows, row.RowId).InitialStateHash,
                FinalStateHash = SourceRow(workspaceSourceRows, row.RowId).FinalStateHash,
                PackageHash = SourceRow(workspaceSourceRows, row.RowId).PackageHash,
                StateChanging = row.StateChanging,
                SaveLoadReplayPassed = row.SaveLoadReplayPassed,
                SourceRefs = SourceRow(workspaceSourceRows, row.RowId).SourceRefs
            })
            .ToList();

        if (rows.Count != 9)
        {
            diagnostics.Add(CampaignEditDiagnostic.Error(
                "goal075.source.row_count_invalid",
                "goal074.rowSelector.rows",
                "Goal 075 requires the nine Goal 074 family/seed rows."));
        }

        return new CampaignEditSourceBundle
        {
            Goal074AcceptedByUserHandoff = goal074Accepted,
            Goal072RemainsHistoricalBlocked = goal072Blocked,
            Goal031And032RemainProducedForReview = goal031And032Produced,
            FamilyIds = rows
                .Select(row => row.FamilyId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(SchemaDrivenCampaignEditVocabulary.FamilyOrderingKey, StringComparer.Ordinal)
                .ToList(),
            SeedIds = rows
                .Select(row => row.SeedId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(SchemaDrivenCampaignEditVocabulary.SeedOrderingKey, StringComparer.Ordinal)
                .ToList(),
            Rows = rows,
            SourceArtifacts = sourceArtifacts,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public CampaignEditSourceManifest BuildSourceManifest(CampaignEditSourceBundle source) =>
        new()
        {
            Accepted = false,
            Goal074AcceptedByUserHandoff = source.Goal074AcceptedByUserHandoff,
            Goal072RemainsHistoricalBlocked = source.Goal072RemainsHistoricalBlocked,
            Goal031And032RemainProducedForReview = source.Goal031And032RemainProducedForReview,
            RowCount = source.Rows.Count,
            FamilyCount = source.FamilyIds.Count,
            SeedCount = source.SeedIds.Count,
            SourceArtifacts = source.SourceArtifacts,
            Diagnostics = source.Diagnostics
        };

    public static IReadOnlyList<CampaignEditDiagnostic> SortDiagnostics(
        IEnumerable<CampaignEditDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static CampaignEditSourceArtifactReference ReadSourceArtifact(string projectRoot, string fileName)
    {
        var relativePath = Goal074Root + "/" + fileName;
        var path = Resolve(projectRoot, relativePath);
        if (!File.Exists(path))
        {
            return new CampaignEditSourceArtifactReference
            {
                SourceGoal = "Goal074",
                ArtifactFamily = Path.GetFileNameWithoutExtension(fileName),
                ArtifactRelativePath = relativePath,
                Exists = false
            };
        }

        var bytes = File.ReadAllBytes(path);
        return new CampaignEditSourceArtifactReference
        {
            SourceGoal = "Goal074",
            ArtifactFamily = Path.GetFileNameWithoutExtension(fileName),
            ArtifactRelativePath = relativePath,
            ArtifactHash = SchemaDrivenCampaignEditHash.Sha256(bytes),
            Exists = true
        };
    }

    private static CampaignWorkspaceSourceRow SourceRow(
        IReadOnlyDictionary<string, CampaignWorkspaceSourceRow> rows,
        string rowId) =>
        rows.TryGetValue(rowId, out var row) ? row : new CampaignWorkspaceSourceRow();

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
}
