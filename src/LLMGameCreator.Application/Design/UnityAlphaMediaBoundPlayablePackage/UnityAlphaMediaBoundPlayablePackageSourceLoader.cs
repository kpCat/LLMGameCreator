using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;

namespace LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;

public sealed class UnityAlphaMediaBoundPlayablePackageSourceLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public UnityAlphaMediaBoundSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<UnityAlphaMediaBoundDiagnostic>();
        var refs = new List<UnityAlphaMediaBoundSourceArtifactReference>();
        var goal055Directory = MediaBoundPlayableReviewPackageEvidenceService.RelativeOutputDirectory;

        string ReadRequired(string relativeDirectory, string fileName, string sourceGoal, string artifactFamily)
        {
            var relativePath = NormalizeRelativePath(relativeDirectory, fileName);
            var path = Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(projectRoot, path);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Required Goal 056 source artifact was not found.", path);
            }

            var text = File.ReadAllText(path, Encoding.UTF8);
            refs.Add(new UnityAlphaMediaBoundSourceArtifactReference
            {
                SourceGoal = sourceGoal,
                ArtifactFamily = artifactFamily,
                ArtifactRelativePath = relativePath,
                ArtifactHash = Hash(text),
                Exists = true,
                HashMatches = true
            });
            return text;
        }

        T ReadJson<T>(string relativeDirectory, string fileName, string sourceGoal, string artifactFamily) =>
            JsonSerializer.Deserialize<T>(ReadRequired(relativeDirectory, fileName, sourceGoal, artifactFamily), JsonOptions)
            ?? throw new InvalidOperationException("Artifact JSON could not be deserialized as " + typeof(T).Name + ".");

        var goal055SourceManifest = ReadJson<MediaBoundSourceManifest>(
            goal055Directory,
            MediaBoundPlayableReviewPackageEvidenceService.SourceManifestJsonFileName,
            "Goal055",
            "source_manifest");
        var goal055ReviewPackageManifest = ReadJson<MediaBoundReviewPackageManifest>(
            goal055Directory,
            MediaBoundPlayableReviewPackageEvidenceService.ReviewPackageManifestJsonFileName,
            "Goal055",
            "review_package_manifest");
        var goal055StreamingManifest = ReadJson<StreamingAssetsMediaManifest>(
            goal055Directory,
            MediaBoundPlayableReviewPackageEvidenceService.StreamingAssetsManifestJsonFileName,
            "Goal055",
            "streaming_assets_manifest");
        var goal055UnityLoadContract = ReadJson<UnityMediaLoadContract>(
            goal055Directory,
            MediaBoundPlayableReviewPackageEvidenceService.UnityLoadContractJsonFileName,
            "Goal055",
            "unity_media_load_contract");
        var goal055UnityLoadProofs = UnityAlphaMediaBoundPlayablePackageVocabulary.FamilyIds
            .Select(familyId => ReadJson<UnityMediaLoadProof>(
                goal055Directory,
                MediaBoundPlayableReviewPackageBuilder.UnityProofFileName(familyId),
                "Goal055",
                "unity_media_load_proof"))
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ToList();
        var goal055ReportMarkdown = ReadRequired(
            goal055Directory,
            MediaBoundPlayableReviewPackageEvidenceService.ReportMarkdownFileName,
            "Goal055",
            "report");

        foreach (var sourceRef in goal055SourceManifest.SourceArtifactRefs)
        {
            if (sourceRef.SourceGoal is not ("Goal047" or "Goal054"))
            {
                continue;
            }

            var sourcePath = Path.GetFullPath(Path.Combine(projectRoot, sourceRef.ArtifactRelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(projectRoot, sourcePath);
            var exists = File.Exists(sourcePath);
            var hashMatches = false;
            if (exists)
            {
                var text = File.ReadAllText(sourcePath, Encoding.UTF8);
                hashMatches = string.Equals(Hash(text), sourceRef.ArtifactHash, StringComparison.Ordinal);
            }

            if (!exists)
            {
                diagnostics.Add(Error("goal056.source.reference_missing", sourceRef.ArtifactRelativePath, "Goal 055 source references to Goal 047/054 must resolve to physical artifacts."));
            }
            else if (!hashMatches)
            {
                diagnostics.Add(Error("goal056.source.reference_hash_mismatch", sourceRef.ArtifactRelativePath, "Goal 055 source reference hash must match the physical Goal 047/054 artifact."));
            }

            refs.Add(new UnityAlphaMediaBoundSourceArtifactReference
            {
                SourceGoal = sourceRef.SourceGoal,
                ArtifactFamily = sourceRef.ArtifactFamily,
                ArtifactRelativePath = sourceRef.ArtifactRelativePath,
                ArtifactHash = sourceRef.ArtifactHash,
                Exists = exists,
                HashMatches = hashMatches
            });
        }

        var mediaFiles = new List<UnityAlphaMediaBoundFilePayload>();
        foreach (var staged in goal055ReviewPackageManifest.StagedFiles.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal))
        {
            var goal055Relative = NormalizeRelativePath(goal055Directory, staged.StagedRelativePath);
            var path = Path.GetFullPath(Path.Combine(projectRoot, goal055Relative.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(projectRoot, path);
            if (!File.Exists(path))
            {
                diagnostics.Add(Error("goal056.source.goal055_media_missing", goal055Relative, "Goal 055 staged media file must exist before Goal 056 can copy it."));
                continue;
            }

            mediaFiles.Add(new UnityAlphaMediaBoundFilePayload
            {
                RelativePath = ToUnityStagingRelativePath(staged.StagedRelativePath),
                Bytes = File.ReadAllBytes(path)
            });
        }

        var basePayloadRoot = ResolveBaseAlphaPayloadRoot(projectRoot, diagnostics);
        var basePayloadFiles = new List<UnityAlphaMediaBoundFilePayload>();
        if (!string.IsNullOrWhiteSpace(basePayloadRoot))
        {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(projectRoot, basePayloadRoot.Replace('/', Path.DirectorySeparatorChar)), "*", SearchOption.AllDirectories)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var relativePath = Path.GetRelativePath(Path.Combine(projectRoot, basePayloadRoot.Replace('/', Path.DirectorySeparatorChar)), file).Replace('\\', '/');
                if (!IsSafeRelativePath(relativePath))
                {
                    diagnostics.Add(Error("goal056.base_payload.path_unsafe", relativePath, "Base Alpha payload paths must be safe relative paths."));
                    continue;
                }

                basePayloadFiles.Add(new UnityAlphaMediaBoundFilePayload
                {
                    RelativePath = relativePath,
                    Bytes = File.ReadAllBytes(file)
                });
            }
        }

        return new UnityAlphaMediaBoundSourceBundle
        {
            Goal055SourceManifest = goal055SourceManifest,
            Goal055ReviewPackageManifest = goal055ReviewPackageManifest,
            Goal055StreamingManifest = goal055StreamingManifest,
            Goal055UnityLoadContract = goal055UnityLoadContract,
            Goal055UnityLoadProofs = goal055UnityLoadProofs,
            Goal055ReportMarkdown = goal055ReportMarkdown,
            Goal055StagedFiles = goal055ReviewPackageManifest.StagedFiles.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList(),
            Goal055MediaFiles = mediaFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList(),
            BaseAlphaPayloadSourceRootRelativePath = basePayloadRoot,
            BaseAlphaPayloadFiles = basePayloadFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList(),
            SourceArtifactRefs = refs
                .GroupBy(item => item.SourceGoal + "|" + item.ArtifactRelativePath, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(item => SourceGoalOrder(item.SourceGoal))
                .ThenBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static string ResolveBaseAlphaPayloadRoot(string projectRoot, ICollection<UnityAlphaMediaBoundDiagnostic> diagnostics)
    {
        var candidates = new[]
        {
            ".llmgc/procedural/minimum-playable-generated-game/build-source/staging",
            ".llmgc/procedural/unity-multi-variant-playable-scenario/variants/frontier_survival/staging",
            ".llmgc/procedural/minimum-playable-generated-game/review-package/LLMGameCreatorAlpha_Data/StreamingAssets/LLMGameCreatorAlpha"
        };

        foreach (var candidate in candidates)
        {
            var full = Path.Combine(projectRoot, candidate.Replace('/', Path.DirectorySeparatorChar));
            if (Directory.Exists(full)
                && File.Exists(Path.Combine(full, "runtime", "unity-runtime-config.json"))
                && File.Exists(Path.Combine(full, "game-data", "game-package.json"))
                && File.Exists(Path.Combine(full, "assets", "asset-manifest.json")))
            {
                return candidate;
            }
        }

        diagnostics.Add(Error("goal056.base_payload.missing", "Alpha base payload", "Goal 056 requires an existing Alpha StreamingAssets base payload with runtime, game-data and assets."));
        return string.Empty;
    }

    public static string ToUnityStagingRelativePath(string goal055StagedRelativePath)
    {
        const string prefix = "review-package/StreamingAssets/LLMGameCreatorAlpha/";
        return goal055StagedRelativePath.StartsWith(prefix, StringComparison.Ordinal)
            ? goal055StagedRelativePath[prefix.Length..]
            : goal055StagedRelativePath;
    }

    private static string NormalizeRelativePath(string relativeDirectory, string fileName) =>
        (relativeDirectory.TrimEnd('/', '\\') + "/" + fileName.TrimStart('/', '\\')).Replace('\\', '/');

    private static int SourceGoalOrder(string sourceGoal) =>
        sourceGoal switch
        {
            "Goal047" => 47,
            "Goal054" => 54,
            "Goal055" => 55,
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

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static string Hash(string text) => UnityAlphaMediaBoundPlayablePackageHash.Hash(text);

    private static UnityAlphaMediaBoundDiagnostic Error(string code, string target, string message) =>
        UnityAlphaMediaBoundDiagnostic.Error(code, target, message);

    private static IReadOnlyList<UnityAlphaMediaBoundDiagnostic> SortDiagnostics(IEnumerable<UnityAlphaMediaBoundDiagnostic> diagnostics) =>
        UnityAlphaMediaBoundPlayablePackageBuilder.SortDiagnostics(diagnostics);

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
