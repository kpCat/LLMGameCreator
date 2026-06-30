using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;
using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;

namespace LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;

public sealed class MediaMaterializationReviewPackageSourceLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public MediaMaterializationSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var refs = new List<MediaMaterializationSourceArtifactReference>();
        var diagnostics = new List<MediaMaterializationDiagnostic>();

        string ReadRequired(string relativeDirectory, string fileName, string sourceGoal, string artifactFamily, string summary)
        {
            var relativePath = NormalizeRelativePath(relativeDirectory, fileName);
            var path = Path.GetFullPath(Path.Combine(projectRoot, relativeDirectory.Replace('/', Path.DirectorySeparatorChar), fileName));
            EnsureContained(projectRoot, path);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Required Goal 054 source artifact was not found.", path);
            }

            var text = File.ReadAllText(path, Encoding.UTF8);
            refs.Add(Ref(sourceGoal, artifactFamily, fileName, relativePath, text, summary));
            return text;
        }

        T ReadJson<T>(string relativeDirectory, string fileName, string sourceGoal, string artifactFamily, string summary) =>
            JsonSerializer.Deserialize<T>(ReadRequired(relativeDirectory, fileName, sourceGoal, artifactFamily, summary), JsonOptions)
            ?? throw new InvalidOperationException("Artifact JSON could not be deserialized as " + typeof(T).Name + ".");

        var goal053Directory = MediaAssetCampaignEvidenceService.RelativeOutputDirectory;
        var goal047Directory = FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory;

        var goal053SourceManifest = ReadJson<MediaCampaignSourceManifest>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.SourceManifestJsonFileName,
            "Goal053",
            "media_campaign_source_manifest",
            "Goal 053 source manifest and family source facts.");
        var goal053RequestQueue = ReadJson<MediaRequestQueue>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.RequestQueueJsonFileName,
            "Goal053",
            "media_request_queue",
            "Goal 053 media request queue.");
        var goal053LicenseLedger = ReadJson<MediaLicenseProvenanceLedger>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.LicenseProvenanceLedgerJsonFileName,
            "Goal053",
            "media_license_provenance_ledger",
            "Goal 053 license/provenance policy ledger.");
        var goal053CandidateQuarantine = ReadJson<MediaCandidateQuarantine>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.CandidateQuarantineJsonFileName,
            "Goal053",
            "media_candidate_quarantine",
            "Goal 053 candidate quarantine matrix.");
        var goal053ReviewLedger = ReadJson<MediaReviewPromotionLedger>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.ReviewPromotionLedgerJsonFileName,
            "Goal053",
            "media_review_promotion_ledger",
            "Goal 053 review/promotion ledger.");
        var goal053FixtureInventory = ReadJson<MediaFixtureFileInventory>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.FixtureInventoryJsonFileName,
            "Goal053",
            "media_fixture_file_inventory",
            "Goal 053 deterministic fixture inventory.");
        var goal053BindingManifest = ReadJson<MediaBindingManifest>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.BindingManifestJsonFileName,
            "Goal053",
            "media_binding_manifest",
            "Goal 053 promoted media binding manifest.");
        var goal053PreviewExportPayloads = ReadJson<LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration.PreviewExportMediaPayloads>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.PreviewExportMediaPayloadsJsonFileName,
            "Goal053",
            "preview_export_media_payloads",
            "Goal 053 preview/export media payload proof.");
        var goal053InvalidMatrix = ReadJson<InvalidMediaDiagnosticsMatrix>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.InvalidMatrixJsonFileName,
            "Goal053",
            "invalid_media_diagnostics_matrix",
            "Goal 053 invalid/fake/leak matrix.");
        var goal053Report = ReadRequired(
            goal053Directory,
            MediaAssetCampaignEvidenceService.ReportMarkdownFileName,
            "Goal053",
            "media_asset_campaign_orchestration_report",
            "Goal 053 compact report.");

        var goal047SourceManifest = ReadJson<FullGeneratorDryRunManifest>(
            goal047Directory,
            FullGeneratorWithoutMediaDryRunEvidenceService.SourceManifestJsonFileName,
            "Goal047",
            "dry_run_source_manifest",
            "Goal 047 source manifest.");

        var familyDryRuns = new List<FullGeneratorFamilyDryRunRecord>();
        foreach (var fileName in new[]
                 {
                     FullGeneratorWithoutMediaDryRunEvidenceService.MapPanelFamilyDryRunJsonFileName,
                     FullGeneratorWithoutMediaDryRunEvidenceService.SurvivalFamilyDryRunJsonFileName,
                     FullGeneratorWithoutMediaDryRunEvidenceService.GridDungeonFamilyDryRunJsonFileName
                 })
        {
            var path = Path.GetFullPath(Path.Combine(projectRoot, goal047Directory.Replace('/', Path.DirectorySeparatorChar), fileName));
            if (!File.Exists(path))
            {
                diagnostics.Add(MediaMaterializationDiagnostic.Warning("goal054.source.goal047_family_missing", fileName, "Goal 047 family dry-run artifact is missing; family source facts will be incomplete."));
                continue;
            }

            familyDryRuns.Add(ReadJson<FullGeneratorFamilyDryRunRecord>(
                goal047Directory,
                fileName,
                "Goal047",
                "family_dry_run",
                "Goal 047 family dry-run record."));
        }

        return new MediaMaterializationSourceBundle
        {
            Goal053SourceManifest = goal053SourceManifest,
            Goal053RequestQueue = goal053RequestQueue,
            Goal053LicenseLedger = goal053LicenseLedger,
            Goal053CandidateQuarantine = goal053CandidateQuarantine,
            Goal053ReviewLedger = goal053ReviewLedger,
            Goal053FixtureInventory = goal053FixtureInventory,
            Goal053BindingManifest = goal053BindingManifest,
            Goal053PreviewExportPayloads = goal053PreviewExportPayloads,
            Goal053InvalidMatrix = goal053InvalidMatrix,
            Goal053ReportMarkdown = goal053Report,
            Goal047SourceManifest = goal047SourceManifest,
            Goal047FamilyDryRuns = familyDryRuns.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal).ToList(),
            SourceArtifactRefs = refs
                .OrderBy(item => SourceGoalOrder(item.SourceGoal))
                .ThenBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = diagnostics
        };
    }

    private static MediaMaterializationSourceArtifactReference Ref(
        string sourceGoal,
        string artifactFamily,
        string fileName,
        string relativePath,
        string text,
        string summary) =>
        new()
        {
            SourceGoal = sourceGoal,
            EvidenceRef = $"{sourceGoal.ToLowerInvariant()}/{artifactFamily}/{Path.GetFileNameWithoutExtension(fileName)}",
            ArtifactFamily = artifactFamily,
            ArtifactFileName = fileName,
            ArtifactRelativePath = relativePath,
            ArtifactHash = MediaMaterializationReviewPackageHash.Hash(text),
            Summary = summary
        };

    private static string NormalizeRelativePath(string relativeDirectory, string fileName) =>
        (relativeDirectory.TrimEnd('/', '\\') + "/" + fileName).Replace('\\', '/');

    private static int SourceGoalOrder(string sourceGoal) =>
        sourceGoal switch
        {
            "Goal047" => 47,
            "Goal053" => 53,
            _ => 999
        };

    private static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}
