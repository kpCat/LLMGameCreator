using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;
using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;
using LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;

namespace LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;

public sealed class MediaBoundPlayableReviewPackageSourceLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public MediaBoundSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var refs = new List<MediaBoundSourceArtifactReference>();
        var diagnostics = new List<MediaBoundDiagnostic>();

        string ReadRequired(string relativeDirectory, string fileName, string sourceGoal, string artifactFamily, string summary)
        {
            var relativePath = NormalizeRelativePath(relativeDirectory, fileName);
            var path = Path.GetFullPath(Path.Combine(projectRoot, relativeDirectory.Replace('/', Path.DirectorySeparatorChar), fileName));
            EnsureContained(projectRoot, path);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Required Goal 055 source artifact was not found.", path);
            }

            var text = File.ReadAllText(path, Encoding.UTF8);
            refs.Add(Ref(sourceGoal, artifactFamily, fileName, relativePath, text, summary));
            return text;
        }

        T ReadJson<T>(string relativeDirectory, string fileName, string sourceGoal, string artifactFamily, string summary) =>
            JsonSerializer.Deserialize<T>(ReadRequired(relativeDirectory, fileName, sourceGoal, artifactFamily, summary), JsonOptions)
            ?? throw new InvalidOperationException("Artifact JSON could not be deserialized as " + typeof(T).Name + ".");

        var goal047Directory = FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory;
        var goal053Directory = MediaAssetCampaignEvidenceService.RelativeOutputDirectory;
        var goal054Directory = MediaMaterializationReviewPackageEvidenceService.RelativeOutputDirectory;

        var goal047SourceManifest = ReadJson<FullGeneratorDryRunManifest>(
            goal047Directory,
            FullGeneratorWithoutMediaDryRunEvidenceService.SourceManifestJsonFileName,
            "Goal047",
            "dry_run_source_manifest",
            "Goal 047 full-generator dry-run source manifest.");
        var familyDryRuns = new List<FullGeneratorFamilyDryRunRecord>();
        foreach (var fileName in new[]
                 {
                     FullGeneratorWithoutMediaDryRunEvidenceService.MapPanelFamilyDryRunJsonFileName,
                     FullGeneratorWithoutMediaDryRunEvidenceService.SurvivalFamilyDryRunJsonFileName,
                     FullGeneratorWithoutMediaDryRunEvidenceService.GridDungeonFamilyDryRunJsonFileName
                 })
        {
            familyDryRuns.Add(ReadJson<FullGeneratorFamilyDryRunRecord>(
                goal047Directory,
                fileName,
                "Goal047",
                "family_dry_run",
                "Goal 047 family dry-run record."));
        }

        var goal053SourceManifest = ReadJson<MediaCampaignSourceManifest>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.SourceManifestJsonFileName,
            "Goal053",
            "media_campaign_source_manifest",
            "Goal 053 media campaign source facts.");
        var goal053BindingManifest = ReadJson<MediaBindingManifest>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.BindingManifestJsonFileName,
            "Goal053",
            "media_binding_manifest",
            "Goal 053 media binding manifest.");
        var goal053LicenseLedger = ReadJson<MediaLicenseProvenanceLedger>(
            goal053Directory,
            MediaAssetCampaignEvidenceService.LicenseProvenanceLedgerJsonFileName,
            "Goal053",
            "media_license_provenance_ledger",
            "Goal 053 media license/provenance ledger.");

        var goal054SourceManifest = ReadJson<MediaMaterializationSourceManifest>(
            goal054Directory,
            MediaMaterializationReviewPackageEvidenceService.SourceManifestJsonFileName,
            "Goal054",
            "media_materialization_source_manifest",
            "Goal 054 source manifest.");
        var goal054Inventory = ReadJson<MaterializedMediaInventory>(
            goal054Directory,
            MediaMaterializationReviewPackageEvidenceService.InventoryJsonFileName,
            "Goal054",
            "materialized_media_inventory",
            "Goal 054 physical media inventory.");
        var goal054LicenseLedger = ReadJson<MediaProvenanceLicenseLedger>(
            goal054Directory,
            MediaMaterializationReviewPackageEvidenceService.LicenseLedgerJsonFileName,
            "Goal054",
            "media_provenance_license_ledger",
            "Goal 054 materialized media provenance ledger.");
        var goal054BindingValidation = ReadJson<MediaBindingValidation>(
            goal054Directory,
            MediaMaterializationReviewPackageEvidenceService.BindingValidationJsonFileName,
            "Goal054",
            "media_binding_validation",
            "Goal 054 media binding validation.");
        var goal054ReviewPackageManifest = ReadJson<MediaReviewPackageManifest>(
            goal054Directory,
            MediaMaterializationReviewPackageEvidenceService.ReviewPackageManifestJsonFileName,
            "Goal054",
            "media_review_package_manifest",
            "Goal 054 review package manifest.");
        var goal054PreviewPayloads = ReadJson<LLMGameCreator.Application.Design.MediaMaterializationReviewPackage.PreviewExportMediaPayloads>(
            goal054Directory,
            MediaMaterializationReviewPackageEvidenceService.PreviewExportPayloadsJsonFileName,
            "Goal054",
            "preview_export_media_payloads",
            "Goal 054 preview/export media-bound payloads.");
        var goal054Report = ReadRequired(
            goal054Directory,
            MediaMaterializationReviewPackageEvidenceService.ReportMarkdownFileName,
            "Goal054",
            "media_materialization_review_package_report",
            "Goal 054 compact report.");

        var physicalFiles = new List<Goal054PhysicalMediaSource>();
        foreach (var file in goal054Inventory.Files.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var relativePath = NormalizeRelativePath(goal054Directory, file.RelativePath);
            var physicalPath = Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(projectRoot, physicalPath);
            if (!File.Exists(physicalPath))
            {
                diagnostics.Add(MediaBoundDiagnostic.Error("goal055.source.goal054_physical_missing", file.RelativePath, "Goal 054 physical media file is missing."));
                continue;
            }

            var bytes = File.ReadAllBytes(physicalPath);
            physicalFiles.Add(new Goal054PhysicalMediaSource
            {
                InventoryRecord = file,
                SourceRelativePath = relativePath,
                Bytes = bytes,
                ActualSha256 = MediaBoundPlayableReviewPackageHash.Hash(bytes)
            });
        }

        return new MediaBoundSourceBundle
        {
            Goal047SourceManifest = goal047SourceManifest,
            Goal047FamilyDryRuns = familyDryRuns.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal).ToList(),
            Goal053SourceManifest = goal053SourceManifest,
            Goal053BindingManifest = goal053BindingManifest,
            Goal053LicenseLedger = goal053LicenseLedger,
            Goal054SourceManifest = goal054SourceManifest,
            Goal054Inventory = goal054Inventory,
            Goal054LicenseLedger = goal054LicenseLedger,
            Goal054BindingValidation = goal054BindingValidation,
            Goal054ReviewPackageManifest = goal054ReviewPackageManifest,
            Goal054PreviewPayloads = goal054PreviewPayloads,
            Goal054ReportMarkdown = goal054Report,
            Goal054PhysicalMediaFiles = physicalFiles.OrderBy(item => FamilyOrderingKey(item.InventoryRecord.FamilyId), StringComparer.Ordinal).ThenBy(item => SlotOrder(item.InventoryRecord.MediaSlotId)).ThenBy(item => item.InventoryRecord.RelativePath, StringComparer.Ordinal).ToList(),
            SourceArtifactRefs = refs
                .OrderBy(item => SourceGoalOrder(item.SourceGoal))
                .ThenBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = diagnostics
        };
    }

    private static MediaBoundSourceArtifactReference Ref(
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
            ArtifactHash = MediaBoundPlayableReviewPackageHash.Hash(text),
            Summary = summary
        };

    private static string NormalizeRelativePath(string relativeDirectory, string fileName) =>
        (relativeDirectory.TrimEnd('/', '\\') + "/" + fileName.TrimStart('/', '\\')).Replace('\\', '/');

    private static int SourceGoalOrder(string sourceGoal) =>
        sourceGoal switch
        {
            "Goal047" => 47,
            "Goal053" => 53,
            "Goal054" => 54,
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

    private static int SlotOrder(string slotId) =>
        slotId switch
        {
            "world_key_art" => 1,
            "npc_portrait" => 2,
            "ui_panel_skin" => 3,
            "sfx_interaction" => 4,
            "export_placeholder_bundle" => 5,
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
}
