using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedProjectOverlayService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly IReadOnlyList<(string Parent, string Collection, string Key)> Collections =
    [
        ("game", "tilePrototypes", "id"),
        ("game", "entityPrototypes", "id"),
        ("game", "maps", "id"),
        ("game", "items", "id"),
        ("game", "resources", "id"),
        ("game", "statuses", "id"),
        ("game", "recipes", "id"),
        ("game", "lootTables", "id"),
        ("game", "transactions", "id"),
        ("game", "resourceNetworks", "id"),
        ("game", "resourceNodes", "id"),
        ("game", "inventories", "id"),
        ("game", "equipmentSlots", "id"),
        ("game", "abilities", "id"),
        ("game", "stats", "id"),
        ("game", "progressions", "id"),
        ("game", "encounters", "id"),
        ("game", "quests", "id"),
        ("game", "dialogues", "id"),
        ("game", "factions", "id"),
        ("game", "formulas", "id"),
        ("game", "interactions", "id"),
        ("assetCatalog", "assets", "id"),
        ("assetCatalog", "contracts", "id"),
        ("assetCatalog", "generationRequests", "id"),
        ("scriptCatalog", "scripts", "id"),
        ("scriptCatalog", "generators", "id"),
        ("generatedContent", "scenes", "sourceId"),
        ("generatedContent", "regions", "sourceId"),
        ("generatedContent", "npcs", "sourceId"),
        ("generatedContent", "items", "sourceId"),
        ("generatedContent", "dialogues", "sourceId"),
        ("generatedContent", "encounters", "sourceId"),
        ("generatedContent", "quests", "sourceId"),
        ("generatedContent", "mechanics", "sourceId"),
        ("generatedContent", "appliedArtifacts", "artifactId"),
        ("generatedContent", "preservedArtifacts", "artifactId")
    ];

    private readonly IGamePackageValidator _validator;

    public GeneratedProjectOverlayService(IGamePackageValidator? validator = null)
    {
        _validator = validator ?? new GamePackageValidator();
    }

    public string NamespaceGeneratedPackage(string generatedMvpPackageJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedMvpPackageJson);
        var root = JsonNode.Parse(generatedMvpPackageJson)?.AsObject()
                   ?? throw new InvalidOperationException("generated_overlay.mvp_invalid_json");
        var replacements = new Dictionary<string, string>(StringComparer.Ordinal);
        var sourceByNamespacedId = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var collection in Collections)
        {
            var parent = root[collection.Parent]?.AsObject()
                         ?? throw new InvalidOperationException("generated_overlay.collection_parent_missing:" + collection.Parent);
            var records = GetOrCreateArray(parent, collection.Collection);
            foreach (var record in records)
            {
                var id = RequireId(record, collection.Key, collection.Parent + "." + collection.Collection);
                var namespaced = IsGeneratedNamespace(id) ? id : "generated/" + id;
                if (sourceByNamespacedId.TryGetValue(namespaced, out var existingSource)
                    && !string.Equals(existingSource, id, StringComparison.Ordinal))
                    throw new InvalidOperationException("generated_overlay.namespace_collision:" + namespaced);
                sourceByNamespacedId[namespaced] = id;
                if (!string.Equals(id, namespaced, StringComparison.Ordinal)) replacements[id] = namespaced;
            }
        }
        ReplaceExactStrings(root, replacements);
        return root.ToJsonString(JsonOptions);
    }

    public GeneratedProjectOverlayResult Build(
        string goal142BaselinePackageJson,
        string goal142BaselinePackageSha256,
        string generatedMvpPackageJson,
        ProceduralGeneratedGamePlan plan)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(goal142BaselinePackageJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(goal142BaselinePackageSha256);
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedMvpPackageJson);
        ArgumentNullException.ThrowIfNull(plan);

        var baselineRoot = JsonNode.Parse(goal142BaselinePackageJson)?.AsObject()
                           ?? throw new InvalidOperationException("generated_overlay.baseline_invalid_json");
        var generatedRoot = JsonNode.Parse(generatedMvpPackageJson)?.AsObject()
                            ?? throw new InvalidOperationException("generated_overlay.mvp_invalid_json");
        var mergedRoot = baselineRoot.DeepClone().AsObject();
        PreserveGeneratedProfile(generatedRoot);

        var baselineRecords = new List<GeneratedProjectRecordFingerprint>();
        var generatedRecords = new List<GeneratedProjectRecordFingerprint>();
        var deduplicated = new List<string>();
        var additiveCount = 0;

        foreach (var collection in Collections)
        {
            MergeCollection(
                baselineRoot,
                generatedRoot,
                mergedRoot,
                collection.Parent,
                collection.Collection,
                collection.Key,
                baselineRecords,
                generatedRecords,
                deduplicated,
                ref additiveCount);
        }

        var baselineManifest = baselineRoot["manifest"]
                               ?? throw new InvalidOperationException("generated_overlay.baseline_manifest_missing");
        var mergedManifest = mergedRoot["manifest"]
                             ?? throw new InvalidOperationException("generated_overlay.merged_manifest_missing");
        if (!CanonicalEquals(baselineManifest, mergedManifest))
            throw new InvalidOperationException("generated_overlay.baseline_manifest_changed");

        var baselinePackage = Deserialize(goal142BaselinePackageJson);
        var generatedPackage = Deserialize(generatedMvpPackageJson);
        var generatedBaseJson = mergedRoot.ToJsonString(JsonOptions);
        var generatedBase = Deserialize(generatedBaseJson);
        var preservationDiagnostics = ValidateRecordFingerprints(generatedBaseJson, baselineRecords)
            .Concat(ValidateRecordFingerprints(generatedBaseJson, generatedRecords))
            .ToList();
        var packageValidation = _validator.Validate(generatedBase);
        var validationDiagnostics = packageValidation.Issues
            .Where(issue => issue.Severity is Domain.Validation.ValidationSeverity.Error
                or Domain.Validation.ValidationSeverity.Critical)
            .Select(issue => issue.Code + ":" + issue.TargetId)
            .ToList();
        var diagnostics = preservationDiagnostics.Concat(validationDiagnostics)
            .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        var baselinePreserved = preservationDiagnostics.All(item => !item.StartsWith("generated_overlay.baseline", StringComparison.Ordinal));
        var referencesValid = validationDiagnostics.Count == 0;
        var generatedStartMapId = generatedPackage.Manifest.StartMapId;
        if (!generatedBase.Game.Maps.Any(map => string.Equals(map.Id, generatedStartMapId, StringComparison.Ordinal)))
        {
            diagnostics.Add("generated_overlay.generated_start_map_missing:" + generatedStartMapId);
            referencesValid = false;
        }

        var document = new GeneratedProjectOverlayDocument
        {
            Goal142BaselinePackageSha256 = goal142BaselinePackageSha256,
            GeneratedMvpPackageSha256 = HashText(generatedMvpPackageJson),
            GeneratedBasePackageSha256 = HashText(generatedBaseJson),
            BaselineManifestSha256 = HashCanonical(baselineManifest),
            BaselineStartMapId = baselinePackage.Manifest.StartMapId,
            GeneratedStartMapId = generatedStartMapId,
            BaselineRecordCount = baselineRecords.Count,
            GeneratedRecordCount = generatedRecords.Count,
            AdditiveRecordCount = additiveCount,
            DeduplicatedRecordCount = deduplicated.Count,
            BaselineRecords = SortFingerprints(baselineRecords),
            GeneratedRecords = SortFingerprints(generatedRecords),
            DeduplicatedRecordKeys = deduplicated.OrderBy(value => value, StringComparer.Ordinal).ToList(),
            BaselineDefinitionsPreserved = baselinePreserved,
            GeneratedRecordsAdditive = generatedRecords.Count == additiveCount + deduplicated.Count,
            GeneratedReferencesValid = referencesValid
        };
        var overlayJson = JsonSerializer.Serialize(document, JsonOptions);
        var passed = document.BaselineDefinitionsPreserved
                     && document.GeneratedRecordsAdditive
                     && document.GeneratedReferencesValid
                     && string.Equals(generatedBase.Manifest.StartMapId, baselinePackage.Manifest.StartMapId, StringComparison.Ordinal)
                     && diagnostics.Count == 0;
        return new GeneratedProjectOverlayResult
        {
            Document = document,
            GeneratedBasePackage = generatedBase,
            OverlayJson = overlayJson,
            GeneratedBasePackageJson = generatedBaseJson,
            Diagnostics = diagnostics,
            Passed = passed
        };
    }

    public IReadOnlyList<string> ValidatePackageRecords(
        GamePackageDefinition package,
        GeneratedProjectOverlayDocument overlay,
        bool includeBaseline)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(overlay);
        var json = JsonSerializer.Serialize(package, JsonOptions);
        var expected = includeBaseline
            ? overlay.BaselineRecords.Concat(overlay.GeneratedRecords)
            : overlay.GeneratedRecords;
        return ValidateRecordFingerprints(json, expected).ToList();
    }

    public IReadOnlyList<string> ValidatePackageRecords(
        string packageJson,
        GeneratedProjectOverlayDocument overlay,
        bool includeBaseline)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageJson);
        ArgumentNullException.ThrowIfNull(overlay);
        var expected = includeBaseline
            ? overlay.BaselineRecords.Concat(overlay.GeneratedRecords)
            : overlay.GeneratedRecords;
        return ValidateRecordFingerprints(packageJson, expected).ToList();
    }

    private static void PreserveGeneratedProfile(JsonObject generatedRoot)
    {
        var generatedContent = generatedRoot["generatedContent"]?.AsObject();
        var profile = generatedContent?["profile"];
        if (generatedContent is null || profile is null) return;
        var preserved = generatedContent["preservedArtifacts"]?.AsArray();
        if (preserved is null)
        {
            preserved = [];
            generatedContent["preservedArtifacts"] = preserved;
        }
        preserved.Add(new JsonObject
        {
            ["artifactId"] = "seeded_generated_project/generated_mvp_profile",
            ["contractId"] = "seeded_generated_project_source_v1",
            ["artifactKind"] = "generated_profile",
            ["reason"] = "Generated MVP profile retained additively while Goal142 baseline profile remains the composition anchor.",
            ["rawJson"] = Canonical(profile)
        });
    }

    private static void MergeCollection(
        JsonObject baselineRoot,
        JsonObject generatedRoot,
        JsonObject mergedRoot,
        string parentName,
        string collectionName,
        string keyName,
        ICollection<GeneratedProjectRecordFingerprint> baselineRecords,
        ICollection<GeneratedProjectRecordFingerprint> generatedRecords,
        ICollection<string> deduplicated,
        ref int additiveCount)
    {
        var path = parentName + "." + collectionName;
        var baselineParent = baselineRoot[parentName]?.AsObject()
                             ?? throw new InvalidOperationException("generated_overlay.collection_parent_missing:" + parentName);
        var generatedParent = generatedRoot[parentName]?.AsObject()
                              ?? throw new InvalidOperationException("generated_overlay.collection_parent_missing:" + parentName);
        var mergedParent = mergedRoot[parentName]?.AsObject()
                           ?? throw new InvalidOperationException("generated_overlay.collection_parent_missing:" + parentName);
        var baselineArray = GetOrCreateArray(baselineParent, collectionName);
        var generatedArray = GetOrCreateArray(generatedParent, collectionName);
        var mergedArray = GetOrCreateArray(mergedParent, collectionName);

        var existing = new Dictionary<string, JsonNode>(StringComparer.Ordinal);
        foreach (var record in baselineArray)
        {
            var id = RequireId(record, keyName, path);
            if (!existing.TryAdd(id, record!))
                throw new InvalidOperationException("generated_overlay.baseline_duplicate:" + path + ":" + id);
            baselineRecords.Add(Fingerprint(path, id, record!));
        }

        foreach (var record in generatedArray)
        {
            var id = RequireId(record, keyName, path);
            generatedRecords.Add(Fingerprint(path, id, record!));
            if (existing.TryGetValue(id, out var baselineRecord))
            {
                if (!CanonicalEquals(baselineRecord, record!))
                    throw new InvalidOperationException("generated_overlay.id_collision:" + path + ":" + id);
                deduplicated.Add(path + ":" + id);
                continue;
            }
            existing.Add(id, record!);
            mergedArray.Add(record!.DeepClone());
            additiveCount++;
        }

        // Baseline order is a composition contract: existing FeatureModule mutation paths are index-based.
        // Generated records are appended in their already deterministic source order.
    }

    private static IEnumerable<string> ValidateRecordFingerprints(
        string packageJson,
        IEnumerable<GeneratedProjectRecordFingerprint> expected)
    {
        var root = JsonNode.Parse(packageJson)?.AsObject();
        if (root is null)
        {
            yield return "generated_overlay.package_invalid_json";
            yield break;
        }
        foreach (var item in expected)
        {
            var segments = item.CollectionPath.Split('.', 2);
            var collection = root[segments[0]]?[segments[1]]?.AsArray();
            var record = collection?.FirstOrDefault(node => RecordId(node) == item.RecordId);
            if (record is null)
            {
                yield return "generated_overlay.record_missing:" + item.CollectionPath + ":" + item.RecordId;
                continue;
            }
            if (!string.Equals(HashCanonical(record), item.CanonicalSha256, StringComparison.Ordinal))
                yield return "generated_overlay.record_changed:" + item.CollectionPath + ":" + item.RecordId;
        }
    }

    private static string RecordId(JsonNode? record)
    {
        if (record is not JsonObject obj) return string.Empty;
        foreach (var key in new[] { "id", "sourceId", "artifactId" })
        {
            var value = obj[key]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(value)) return value;
        }
        return string.Empty;
    }

    private static JsonArray GetOrCreateArray(JsonObject parent, string propertyName)
    {
        if (parent[propertyName] is JsonArray existing) return existing;
        if (parent[propertyName] is not null)
            throw new InvalidOperationException("generated_overlay.collection_invalid:" + propertyName);
        var created = new JsonArray();
        parent[propertyName] = created;
        return created;
    }

    private static bool IsGeneratedNamespace(string id) =>
        id.StartsWith("generated/", StringComparison.Ordinal)
        || id.StartsWith("seeded_generated_project/", StringComparison.Ordinal);

    private static void ReplaceExactStrings(JsonNode node, IReadOnlyDictionary<string, string> replacements)
    {
        if (node is JsonObject obj)
        {
            foreach (var property in obj.ToList())
            {
                if (property.Value is JsonValue value
                    && value.TryGetValue<string>(out var text)
                    && replacements.TryGetValue(text, out var replacement))
                    obj[property.Key] = replacement;
                else if (property.Value is not null)
                    ReplaceExactStrings(property.Value, replacements);
            }
            return;
        }
        if (node is not JsonArray array) return;
        for (var index = 0; index < array.Count; index++)
        {
            if (array[index] is JsonValue value
                && value.TryGetValue<string>(out var text)
                && replacements.TryGetValue(text, out var replacement))
                array[index] = replacement;
            else if (array[index] is not null)
                ReplaceExactStrings(array[index]!, replacements);
        }
    }

    private static string RequireId(JsonNode? record, string keyName, string path)
    {
        var id = record?[keyName]?.GetValue<string>()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidOperationException("generated_overlay.id_missing:" + path);
        return id;
    }

    private static GeneratedProjectRecordFingerprint Fingerprint(string path, string id, JsonNode record) => new()
    {
        CollectionPath = path,
        RecordId = id,
        CanonicalSha256 = HashCanonical(record)
    };

    private static IReadOnlyList<GeneratedProjectRecordFingerprint> SortFingerprints(
        IEnumerable<GeneratedProjectRecordFingerprint> values) => values
        .OrderBy(value => value.CollectionPath, StringComparer.Ordinal)
        .ThenBy(value => value.RecordId, StringComparer.Ordinal).ToList();

    private static bool CanonicalEquals(JsonNode left, JsonNode right) =>
        string.Equals(Canonical(left), Canonical(right), StringComparison.Ordinal);

    private static string HashCanonical(JsonNode node) => HashText(Canonical(node));

    private static string Canonical(JsonNode node) => CanonicalNode(node).ToJsonString(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    });

    private static JsonNode CanonicalNode(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            var result = new JsonObject();
            foreach (var property in obj.OrderBy(property => property.Key, StringComparer.Ordinal))
                result[property.Key] = property.Value is null ? null : CanonicalNode(property.Value);
            return result;
        }
        if (node is JsonArray array)
        {
            var result = new JsonArray();
            foreach (var item in array) result.Add(item is null ? null : CanonicalNode(item));
            return result;
        }
        return node.DeepClone();
    }

    private static GamePackageDefinition Deserialize(string json) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(json, JsonOptions)
        ?? throw new InvalidOperationException("generated_overlay.package_deserialization_failed");

    internal static string HashText(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}
