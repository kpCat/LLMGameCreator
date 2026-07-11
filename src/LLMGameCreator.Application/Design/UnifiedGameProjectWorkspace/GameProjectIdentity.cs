using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public static class GameProjectIdentityVocabulary
{
    public const string SchemaVersion = "game_project_identity_v1";
    public const string RelativePath = ".llmgc/project-identity.json";
    public const string CreatedProjectPackageSource = "created_project_package";
    public const string MigratedLegacyWorkspaceSource = "migrated_legacy_workspace";
    public const string RecoveredAfterTemplateOverwriteSource = "recovered_after_template_overwrite";
}

public sealed record GameProjectIdentityDocument
{
    public string SchemaVersion { get; init; } = GameProjectIdentityVocabulary.SchemaVersion;
    public string PackageId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string FormatVersion { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; init; }
    public string Source { get; init; } = string.Empty;
    public IReadOnlyList<string> RecoveryDiagnostics { get; init; } = [];
}

public sealed class GameProjectIdentityRecoveryService
{
    public GameProjectIdentityDocument Capture(
        string projectFolder,
        GamePackageDefinition package,
        FeatureModuleCompositionDocument? legacyAuthoringDocument,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(package);
        var manifest = package.Manifest;
        if (IsComposedTemplateIdentity(manifest.PackageId, manifest.Title, manifest.Version))
        {
            if (legacyAuthoringDocument is null || !IsMeaningful(legacyAuthoringDocument.DisplayName))
            {
                throw new InvalidOperationException(
                    "Project identity recovery is ambiguous. Restore a meaningful authoring display name or provide a valid .llmgc/project-identity.json before building.");
            }

            var folderName = Path.GetFileName(Path.GetFullPath(projectFolder)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var normalizedFolder = NormalizeFolderName(folderName);
            return new GameProjectIdentityDocument
            {
                PackageId = "game/" + normalizedFolder,
                Title = legacyAuthoringDocument.DisplayName.Trim(),
                Version = "0.1.0",
                FormatVersion = string.IsNullOrWhiteSpace(manifest.FormatVersion) ? "0.1" : manifest.FormatVersion.Trim(),
                Description = IsMeaningful(legacyAuthoringDocument.Description)
                    ? legacyAuthoringDocument.Description.Trim()
                    : "Recovered game project identity.",
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                Source = GameProjectIdentityVocabulary.RecoveredAfterTemplateOverwriteSource,
                RecoveryDiagnostics =
                [
                    "Template manifest identity was detected in the opened project package.",
                    "Title was recovered from the legacy authoring document.",
                    "Package ID was recovered from the normalized project folder name.",
                    "Generated composition version was reset to 0.1.0."
                ]
            };
        }

        return new GameProjectIdentityDocument
        {
            PackageId = manifest.PackageId?.Trim() ?? string.Empty,
            Title = manifest.Title?.Trim() ?? string.Empty,
            Version = manifest.Version?.Trim() ?? string.Empty,
            FormatVersion = manifest.FormatVersion?.Trim() ?? string.Empty,
            Description = manifest.Description?.Trim() ?? string.Empty,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Source = legacyAuthoringDocument is null
                ? GameProjectIdentityVocabulary.CreatedProjectPackageSource
                : GameProjectIdentityVocabulary.MigratedLegacyWorkspaceSource
        };
    }

    public static bool IsComposedTemplateIdentity(string? packageId, string? title, string? version) =>
        string.Equals(packageId, "game/minimal-map-game", StringComparison.Ordinal)
        && string.Equals(title, "Minimal Map Game", StringComparison.Ordinal)
        && !string.IsNullOrWhiteSpace(version)
        && version.StartsWith("0.1.146-", StringComparison.Ordinal);

    private static bool IsMeaningful(string? value) => !string.IsNullOrWhiteSpace(value);

    private static string NormalizeFolderName(string value)
    {
        var normalized = Regex.Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-")
            .Trim('-');
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("Project folder name cannot be normalized into a package ID.");
        return normalized;
    }
}

public sealed class GameProjectIdentityStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly HashSet<string> AllowedSources = new(StringComparer.Ordinal)
    {
        GameProjectIdentityVocabulary.CreatedProjectPackageSource,
        GameProjectIdentityVocabulary.MigratedLegacyWorkspaceSource,
        GameProjectIdentityVocabulary.RecoveredAfterTemplateOverwriteSource
    };

    private readonly GameProjectIdentityRecoveryService _recovery;

    public GameProjectIdentityStore(GameProjectIdentityRecoveryService? recovery = null)
    {
        _recovery = recovery ?? new GameProjectIdentityRecoveryService();
    }

    public string PathFor(string projectFolder) => GameProjectFeatureModuleAuthoringService.ConfinedPath(
        projectFolder,
        GameProjectIdentityVocabulary.RelativePath);

    public GameProjectIdentityDocument LoadOrCapture(
        string projectFolder,
        GamePackageDefinition package,
        FeatureModuleCompositionDocument? legacyAuthoringDocument)
    {
        var path = PathFor(projectFolder);
        if (File.Exists(path)) return Load(path);

        var identity = _recovery.Capture(
            projectFolder,
            package,
            legacyAuthoringDocument,
            DateTimeOffset.UtcNow);
        Validate(identity);
        WriteAtomic(path, JsonSerializer.Serialize(identity, JsonOptions) + Environment.NewLine);
        return identity;
    }

    public GameProjectIdentityDocument Load(string path)
    {
        try
        {
            var identity = JsonSerializer.Deserialize<GameProjectIdentityDocument>(
                               File.ReadAllText(path, Encoding.UTF8),
                               JsonOptions)
                           ?? throw new InvalidOperationException("Project identity document is empty.");
            Validate(identity);
            return identity;
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Project identity document is invalid JSON: " + path, exception);
        }
    }

    public static void Validate(GameProjectIdentityDocument identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        var diagnostics = new List<string>();
        if (identity.SchemaVersion != GameProjectIdentityVocabulary.SchemaVersion)
            diagnostics.Add("unsupported schemaVersion");
        if (string.IsNullOrWhiteSpace(identity.PackageId)) diagnostics.Add("packageId is required");
        if (string.IsNullOrWhiteSpace(identity.Title)) diagnostics.Add("title is required");
        if (string.IsNullOrWhiteSpace(identity.Version)) diagnostics.Add("version is required");
        if (string.IsNullOrWhiteSpace(identity.FormatVersion)) diagnostics.Add("formatVersion is required");
        if (identity.CreatedAtUtc == default) diagnostics.Add("createdAtUtc is required");
        if (identity.UpdatedAtUtc == default) diagnostics.Add("updatedAtUtc is required");
        if (!AllowedSources.Contains(identity.Source)) diagnostics.Add("source is unsupported");
        if (diagnostics.Count > 0)
            throw new InvalidOperationException("Project identity validation failed: " + string.Join("; ", diagnostics));
    }

    private static void WriteAtomic(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporary, text, new UTF8Encoding(false));
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}

public sealed class GameProjectCompositionIdentityService
{
    public string Create(string packageId)
    {
        if (string.IsNullOrWhiteSpace(packageId))
            throw new InvalidOperationException("Project package ID is required for composition identity.");
        var normalized = Regex.Replace(packageId.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(normalized)) normalized = "game";
        if (normalized.Length > 96) normalized = normalized[..96].TrimEnd('-');
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(packageId.Trim())))
            .ToLowerInvariant()[..12];
        var compositionId = "project-" + normalized + "-" + hash;
        if (!FeatureModuleCompositionDocumentValidator.IsValidCompositionId(compositionId))
            throw new InvalidOperationException("Derived project composition ID is invalid: " + compositionId);
        return compositionId;
    }
}
