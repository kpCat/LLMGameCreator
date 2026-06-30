using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;
using LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;

namespace LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;

public sealed class MediaAssetCampaignSourceLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public MediaCampaignSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var refs = new List<MediaCampaignSourceArtifactReference>();
        var artifactHashes = new SortedDictionary<string, string>(StringComparer.Ordinal);

        string Read(
            string relativeDirectory,
            string fileName,
            string sourceGoal,
            string artifactFamily,
            string summary)
        {
            var relativePath = NormalizeRelativePath(relativeDirectory, fileName);
            var path = Path.GetFullPath(Path.Combine(
                projectRoot,
                relativeDirectory.Replace('/', Path.DirectorySeparatorChar),
                fileName));
            EnsureContained(projectRoot, path);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Required Goal 053 source artifact was not found.", path);
            }

            var text = File.ReadAllText(path, Encoding.UTF8);
            var hash = MediaAssetCampaignHash.Hash(text);
            artifactHashes[relativePath] = hash;
            refs.Add(new MediaCampaignSourceArtifactReference
            {
                SourceGoal = sourceGoal,
                EvidenceRef = EvidenceRef(sourceGoal, artifactFamily, fileName),
                ArtifactFamily = artifactFamily,
                ArtifactFileName = fileName,
                ArtifactRelativePath = relativePath,
                ArtifactHash = hash,
                Summary = summary
            });

            return text;
        }

        var goal047 = ReadGoal047(Read);
        var goal043CatalogText = Read(
            MultiFamilyGeneratedTemplateEvidenceService.RelativeOutputDirectory,
            MultiFamilyGeneratedTemplateEvidenceService.CatalogJsonFileName,
            "Goal043",
            "family_template_catalog",
            "Goal 043 family catalog for family/scenario/style ids.");
        var goal040MetamodulePayloadText = Read(
            ChunkedRuntimePreviewExportEvidenceService.RelativeOutputDirectory,
            ChunkedRuntimePreviewExportEvidenceService.MetamodulePayloadJsonFileName,
            "Goal040",
            "chunked_preview_payload",
            "Goal 040 metamodule preview/export payload for world-scale media-volume stress facts.");

        return new MediaCampaignSourceBundle
        {
            Goal047Manifest = goal047.Manifest,
            Goal047ReviewLedger = goal047.ReviewLedger,
            Goal047FamilyDryRuns = goal047.FamilyDryRuns,
            Goal047RuntimePreviewMatrix = goal047.RuntimePreview,
            Goal047ExportProfileMatrix = goal047.ExportProfiles,
            Goal047PackageSummary = goal047.PackageSummary,
            Goal047OneClickSummary = goal047.OneClickSummary,
            Goal043Catalog = ReadJson<FamilyTemplateCatalog>(goal043CatalogText),
            Goal040MetamodulePayload = ReadJson<ChunkedPreviewPayload>(goal040MetamodulePayloadText),
            SourceArtifactRefs = refs
                .OrderBy(item => SourceGoalOrder(item.SourceGoal))
                .ThenBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
                .ThenBy(item => item.EvidenceRef, StringComparer.Ordinal)
                .ToList(),
            ArtifactHashByRelativePath = artifactHashes
        };
    }

    private static Goal047ReadResult ReadGoal047(Func<string, string, string, string, string, string> read)
    {
        var manifest = ReadJson<FullGeneratorDryRunManifest>(read(
            FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory,
            FullGeneratorWithoutMediaDryRunEvidenceService.SourceManifestJsonFileName,
            "Goal047",
            "dry_run_source_manifest",
            "Goal 047 source manifest and accepted-family facts."));
        var review = ReadJson<FullGeneratorReviewPromotionLedger>(read(
            FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory,
            FullGeneratorWithoutMediaDryRunEvidenceService.ReviewPromotionLedgerJsonFileName,
            "Goal047",
            "review_promotion_ledger",
            "Goal 047 review/promotion lifecycle facts."));
        var runtime = ReadJson<FullGeneratorRuntimePreviewValidationMatrix>(read(
            FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory,
            FullGeneratorWithoutMediaDryRunEvidenceService.RuntimePreviewValidationMatrixJsonFileName,
            "Goal047",
            "runtime_preview_validation_matrix",
            "Goal 047 runtime preview payload validation facts."));
        var export = ReadJson<FullGeneratorExportProfileSelectionMatrix>(read(
            FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory,
            FullGeneratorWithoutMediaDryRunEvidenceService.ExportProfileSelectionMatrixJsonFileName,
            "Goal047",
            "export_profile_selection_matrix",
            "Goal 047 export profile facts."));
        var package = ReadJson<FullGeneratorPackageCompatibilitySummary>(read(
            FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory,
            FullGeneratorWithoutMediaDryRunEvidenceService.PackageCompatibilitySummaryJsonFileName,
            "Goal047",
            "package_compatibility_summary",
            "Goal 047 generated target/package compatibility facts."));
        var oneClick = ReadJson<FullGeneratorOneClickDryRunSummary>(read(
            FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory,
            FullGeneratorWithoutMediaDryRunEvidenceService.OneClickDryRunSummaryJsonFileName,
            "Goal047",
            "one_click_dry_run_summary",
            "Goal 047 one-click dry-run summary."));
        read(
            FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory,
            FullGeneratorWithoutMediaDryRunEvidenceService.InvalidFakeLeakMatrixJsonFileName,
            "Goal047",
            "invalid_fake_leak_matrix",
            "Goal 047 invalid/fake/leak source matrix.");
        read(
            FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory,
            FullGeneratorWithoutMediaDryRunEvidenceService.ReportMarkdownFileName,
            "Goal047",
            "full_generator_without_media_report",
            "Goal 047 compact report.");

        var familyDryRuns = new List<FullGeneratorFamilyDryRunRecord>
        {
            ReadJson<FullGeneratorFamilyDryRunRecord>(read(
                FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory,
                FullGeneratorWithoutMediaDryRunEvidenceService.MapPanelFamilyDryRunJsonFileName,
                "Goal047",
                "family_dry_run",
                "Goal 047 map_panel_rpg dry-run record.")),
            ReadJson<FullGeneratorFamilyDryRunRecord>(read(
                FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory,
                FullGeneratorWithoutMediaDryRunEvidenceService.SurvivalFamilyDryRunJsonFileName,
                "Goal047",
                "family_dry_run",
                "Goal 047 survival_sandbox dry-run record.")),
            ReadJson<FullGeneratorFamilyDryRunRecord>(read(
                FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory,
                FullGeneratorWithoutMediaDryRunEvidenceService.GridDungeonFamilyDryRunJsonFileName,
                "Goal047",
                "family_dry_run",
                "Goal 047 first_person_grid_dungeon dry-run record."))
        };

        return new Goal047ReadResult(manifest, review, runtime, export, package, oneClick, familyDryRuns);
    }

    private static T ReadJson<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, JsonOptions)
        ?? throw new InvalidOperationException("Artifact JSON could not be deserialized as " + typeof(T).Name + ".");

    private static string EvidenceRef(string sourceGoal, string artifactFamily, string fileName) =>
        $"{sourceGoal.ToLowerInvariant()}/{artifactFamily}/{Path.GetFileNameWithoutExtension(fileName)}";

    private static string NormalizeRelativePath(string relativeDirectory, string fileName) =>
        (relativeDirectory.TrimEnd('/', '\\') + "/" + fileName).Replace('\\', '/');

    private static int SourceGoalOrder(string sourceGoal) =>
        sourceGoal switch
        {
            "Goal040" => 40,
            "Goal043" => 43,
            "Goal047" => 47,
            _ => 999
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

    private sealed record Goal047ReadResult(
        FullGeneratorDryRunManifest Manifest,
        FullGeneratorReviewPromotionLedger ReviewLedger,
        FullGeneratorRuntimePreviewValidationMatrix RuntimePreview,
        FullGeneratorExportProfileSelectionMatrix ExportProfiles,
        FullGeneratorPackageCompatibilitySummary PackageSummary,
        FullGeneratorOneClickDryRunSummary OneClickSummary,
        IReadOnlyList<FullGeneratorFamilyDryRunRecord> FamilyDryRuns);
}
