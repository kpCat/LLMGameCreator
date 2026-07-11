using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleCompositionPersistenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _workspaceRoot;
    private readonly IFeatureModuleAuthoringClock _clock;
    private readonly FeatureModuleCompositionDocumentValidator _validator;
    private readonly FeatureModuleCompositionStalenessService _staleness;

    public FeatureModuleCompositionPersistenceService(
        string workspaceRoot,
        IFeatureModuleAuthoringClock? clock = null,
        FeatureModuleCompositionDocumentValidator? validator = null,
        FeatureModuleCompositionStalenessService? staleness = null)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot)) throw new ArgumentException("workspace root is required", nameof(workspaceRoot));
        _workspaceRoot = Path.GetFullPath(workspaceRoot);
        _clock = clock ?? new SystemFeatureModuleAuthoringClock();
        _validator = validator ?? new FeatureModuleCompositionDocumentValidator();
        _staleness = staleness ?? new FeatureModuleCompositionStalenessService();
    }

    public string WorkspaceRoot => _workspaceRoot;

    public FeatureModuleCompositionDocument CreateNew(
        string compositionId,
        string displayName,
        string description,
        FeatureModuleLibrarySnapshot library,
        IReadOnlyList<string>? selectedModuleIds = null)
    {
        GuardId(compositionId);
        if (File.Exists(PathFor(compositionId))) throw new InvalidOperationException("duplicate composition ID rejected: " + compositionId);
        var now = _clock.UtcNow.ToUniversalTime();
        var selected = (selectedModuleIds ?? library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
            .Select(module => module.ModuleId).ToList()).OrderBy(id => id, StringComparer.Ordinal).ToList();
        return Normalize(new FeatureModuleCompositionDocument
        {
            CompositionId = compositionId,
            DisplayName = displayName,
            Description = description,
            SelectedModuleIds = selected,
            CatalogFingerprint = library.CatalogFingerprint,
            ModuleFingerprints = selected.Where(library.ModuleFingerprints.ContainsKey)
                .ToDictionary(id => id, id => library.ModuleFingerprints[id], StringComparer.Ordinal),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
    }

    public FeatureModuleCompositionDocument Load(string compositionId, FeatureModuleLibrarySnapshot library)
    {
        var path = PathFor(compositionId);
        if (!File.Exists(path)) throw new FileNotFoundException("saved composition was not found", path);
        FeatureModuleCompositionDocument document;
        try
        {
            document = JsonSerializer.Deserialize<FeatureModuleCompositionDocument>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
                       ?? throw new InvalidOperationException("corrupt composition rejected: " + compositionId);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("corrupt composition rejected: " + compositionId, exception);
        }
        var validation = _validator.Validate(document, library);
        if (!validation.SchemaVersionSupported || !validation.CompositionIdValid)
            throw new InvalidOperationException("saved composition validation failed: " + string.Join("; ", validation.Diagnostics));
        return Normalize(document);
    }

    public FeatureModuleCompositionDocument Save(
        FeatureModuleCompositionDocument document,
        FeatureModuleLibrarySnapshot library)
    {
        var normalized = Normalize(document);
        var validation = _validator.Validate(normalized, library);
        if (!validation.Passed)
            throw new InvalidOperationException("composition save validation failed: " + string.Join("; ", validation.Diagnostics));
        var path = PathFor(normalized.CompositionId);
        if (File.Exists(path))
        {
            var existing = ReadWithoutLibrary(path, normalized.CompositionId);
            if (existing.Revision != normalized.Revision)
                throw new InvalidOperationException("composition revision conflict rejected: " + normalized.CompositionId);
        }
        else if (normalized.Revision != 0)
            throw new InvalidOperationException("new composition revision must be zero");
        var saved = Normalize(normalized with
        {
            Revision = normalized.Revision + 1,
            UpdatedAtUtc = _clock.UtcNow.ToUniversalTime()
        });
        WriteAtomic(path, SerializeCanonical(saved));
        return saved;
    }

    public FeatureModuleCompositionDocument SaveAs(
        FeatureModuleCompositionDocument source,
        string newCompositionId,
        string newDisplayName,
        FeatureModuleLibrarySnapshot library)
    {
        GuardId(newCompositionId);
        if (File.Exists(PathFor(newCompositionId)))
            throw new InvalidOperationException("SaveAs duplicate composition ID rejected: " + newCompositionId);
        var now = _clock.UtcNow.ToUniversalTime();
        return Save(source with
        {
            CompositionId = newCompositionId,
            DisplayName = newDisplayName,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Revision = 0,
            LastMaterializedPackageSha256 = string.Empty,
            LastCompositionPackageSha256 = string.Empty,
            LastActivatedProjectPackageSha256 = string.Empty,
            LastQualifiedFinalStateHash = string.Empty,
            LastQualificationStatus = "NOT_RUN"
        }, library);
    }

    public FeatureModuleCompositionDocument Clone(
        string sourceCompositionId,
        string cloneCompositionId,
        string cloneDisplayName,
        FeatureModuleLibrarySnapshot library) =>
        SaveAs(Load(sourceCompositionId, library), cloneCompositionId, cloneDisplayName, library);

    public FeatureModuleCompositionWorkspaceIndex List(FeatureModuleLibrarySnapshot library)
    {
        if (!Directory.Exists(_workspaceRoot)) return new FeatureModuleCompositionWorkspaceIndex();
        var entries = new List<FeatureModuleCompositionWorkspaceEntry>();
        foreach (var path in Directory.EnumerateFiles(_workspaceRoot, "*" + FeatureModuleCompositionDocumentVocabulary.FileExtension)
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            var id = Path.GetFileName(path)[..^FeatureModuleCompositionDocumentVocabulary.FileExtension.Length];
            try
            {
                var document = Load(id, library);
                var stale = _staleness.Evaluate(document, library);
                entries.Add(new FeatureModuleCompositionWorkspaceEntry
                {
                    CompositionId = document.CompositionId,
                    DisplayName = document.DisplayName,
                    RelativePath = Path.GetFileName(path),
                    Revision = document.Revision,
                    Status = stale.Status,
                    Diagnostics = stale.Diagnostics
                });
            }
            catch (Exception exception) when (exception is InvalidOperationException or JsonException)
            {
                entries.Add(new FeatureModuleCompositionWorkspaceEntry
                {
                    CompositionId = id,
                    RelativePath = Path.GetFileName(path),
                    Status = "CORRUPT",
                    Corrupt = true,
                    Diagnostics = [exception.Message]
                });
            }
        }
        return new FeatureModuleCompositionWorkspaceIndex
        {
            CompositionCount = entries.Count,
            CorruptDocumentCount = entries.Count(entry => entry.Corrupt),
            Compositions = entries
        };
    }

    public void Delete(string compositionId)
    {
        var path = PathFor(compositionId);
        if (!File.Exists(path)) throw new FileNotFoundException("saved composition was not found", path);
        File.Delete(path);
    }

    public static string SerializeCanonical(FeatureModuleCompositionDocument document) =>
        JsonSerializer.Serialize(Normalize(document), JsonOptions) + "\n";

    private FeatureModuleCompositionDocument ReadWithoutLibrary(string path, string id)
    {
        try
        {
            return JsonSerializer.Deserialize<FeatureModuleCompositionDocument>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
                   ?? throw new InvalidOperationException("corrupt composition rejected: " + id);
        }
        catch (JsonException exception) { throw new InvalidOperationException("corrupt composition rejected: " + id, exception); }
    }

    private string PathFor(string compositionId)
    {
        GuardId(compositionId);
        var path = Path.GetFullPath(Path.Combine(_workspaceRoot, compositionId + FeatureModuleCompositionDocumentVocabulary.FileExtension));
        if (!IsUnder(path, _workspaceRoot)) throw new InvalidOperationException("composition workspace path escape rejected");
        return path;
    }

    private static void GuardId(string value)
    {
        if (!FeatureModuleCompositionDocumentValidator.IsValidCompositionId(value))
            throw new InvalidOperationException("invalid composition ID rejected: " + value);
    }

    private static FeatureModuleCompositionDocument Normalize(FeatureModuleCompositionDocument document) => document with
    {
        SelectedModuleIds = document.SelectedModuleIds.OrderBy(id => id, StringComparer.Ordinal).ToList(),
        ParameterValues = document.ParameterValues.OrderBy(value => value.ModuleId, StringComparer.Ordinal)
            .ThenBy(value => value.ParameterId, StringComparer.Ordinal)
            .Select(value => value with { Value = value.Value.Clone() }).ToList(),
        ModuleFingerprints = new SortedDictionary<string, string>(
            document.ModuleFingerprints.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
            StringComparer.Ordinal)
    };

    private static void WriteAtomic(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporary, text, new UTF8Encoding(false));
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static bool IsUnder(string path, string directory)
    {
        var full = Path.GetFullPath(path);
        var root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return full.Equals(root, comparison) || full.StartsWith(root + Path.DirectorySeparatorChar, comparison);
    }
}
