using System.Text.Json;

namespace LLMGameCreator.Application.Design.Atlas;

public sealed class AtlasRegistryImportService
{
    private static readonly string[] KnownAtlasJsonFiles =
    [
        "capability_atlas.json",
        "reference_profiles.json",
        "artifact_contracts.json",
        "validation_pipeline.json",
        "library_growth_pipeline.json",
        "runtime_db_and_unity_export_map.json",
        "model_workflow_roles_and_prompts.json",
        "prompt_context_pack_map.json",
        "game_profile_negotiation_map.json",
        "feature_bundle_map.json",
        "generator_plan_map.json"
    ];

    public static IReadOnlyList<string> KnownAtlasFileNames { get; } =
    [
        "ATLAS_INDEX.md",
        .. KnownAtlasJsonFiles,
        "examples/test.example.json"
    ];

    private static readonly string[] ReferencePrefixes =
    [
        "profile/",
        "feature_bundle/",
        "content_overlay/",
        "validation.",
        "context_template/",
        "role/",
        "model_tier/",
        "contract_group/",
        "proposal_kind/",
        "partition/",
        "source/",
        "budget/",
        "decision_group/",
        "decision/",
        "generator_plan.",
        "library_growth.",
        "feature_selection.",
        "example/"
    ];

    private static readonly HashSet<string> ReferencePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "primary_bundle",
        "required_runtime_targets",
        "runtime_targets",
        "required_feature_bundles",
        "optional_feature_bundles",
        "selected_feature_bundles",
        "target_artifacts",
        "content_overlays",
        "optional_content_overlays",
        "expected_artifact_contract",
        "validation_gates",
        "artifact_contracts",
        "validators",
        "depends_on",
        "requires",
        "provides",
        "source_profile_id",
        "preferred_model_tier",
        "context_pack_template",
        "producer_role",
        "inputs",
        "outputs"
    };

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    public async Task<AtlasRegistryImportResult> ImportAtlasRegistryAsync(
        string repositoryRootOrAtlasRoot,
        CancellationToken cancellationToken = default)
    {
        var diagnostics = new List<AtlasDiagnostic>();
        var documents = new List<AtlasDocumentSummary>();
        var examples = new List<AtlasExampleSummary>();
        var idOwners = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        var atlasRoot = ResolveAtlasRoot(repositoryRootOrAtlasRoot);
        if (atlasRoot == null)
        {
            diagnostics.Add(CreateDiagnostic(
                AtlasDiagnosticSeverity.Error,
                AtlasDiagnosticCodes.MissingRoot,
                "generator-library/atlas folder was not found.",
                repositoryRootOrAtlasRoot));

            return BuildResult(string.Empty, documents, examples, diagnostics, idOwners);
        }

        var indexPath = Path.Combine(atlasRoot, "ATLAS_INDEX.md");
        if (!File.Exists(indexPath))
        {
            diagnostics.Add(CreateDiagnostic(
                AtlasDiagnosticSeverity.Warning,
                AtlasDiagnosticCodes.MissingKnownFile,
                "Known atlas file 'ATLAS_INDEX.md' was not found.",
                "ATLAS_INDEX.md"));
        }
        else
        {
            diagnostics.Add(CreateDiagnostic(
                AtlasDiagnosticSeverity.Info,
                AtlasDiagnosticCodes.SkippedMarkdown,
                "Atlas markdown index was found but not parsed.",
                "ATLAS_INDEX.md"));
        }

        foreach (var knownFile in KnownAtlasJsonFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var path = Path.Combine(atlasRoot, knownFile);
            if (!IsPathInsideRoot(atlasRoot, path))
            {
                diagnostics.Add(CreateDiagnostic(
                    AtlasDiagnosticSeverity.Error,
                    AtlasDiagnosticCodes.PathOutsideAtlasRoot,
                    "Atlas file path resolved outside atlas root.",
                    knownFile));
                continue;
            }

            if (!File.Exists(path))
            {
                diagnostics.Add(CreateDiagnostic(
                    AtlasDiagnosticSeverity.Warning,
                    AtlasDiagnosticCodes.MissingKnownFile,
                    $"Known atlas file '{knownFile}' was not found.",
                    knownFile));
                continue;
            }

            var summary = await LoadDocumentSummaryAsync(atlasRoot, path, diagnostics, cancellationToken).ConfigureAwait(false);
            documents.Add(summary);
            RegisterIds(summary.TopLevelIds, summary.Path, idOwners);
        }

        var examplesRoot = Path.Combine(atlasRoot, "examples");
        if (Directory.Exists(examplesRoot))
        {
            foreach (var examplePath in Directory
                         .EnumerateFiles(examplesRoot, "*.example.json", SearchOption.TopDirectoryOnly)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsPathInsideRoot(atlasRoot, examplePath))
                {
                    diagnostics.Add(CreateDiagnostic(
                        AtlasDiagnosticSeverity.Error,
                        AtlasDiagnosticCodes.PathOutsideAtlasRoot,
                        "Atlas example path resolved outside atlas root.",
                        examplePath));
                    continue;
                }

                var summary = await LoadExampleSummaryAsync(atlasRoot, examplePath, diagnostics, cancellationToken).ConfigureAwait(false);
                examples.Add(summary);

                if (!string.IsNullOrWhiteSpace(summary.ExampleId))
                {
                    RegisterId(summary.ExampleId, summary.Path, idOwners);
                }
            }
        }
        else
        {
            diagnostics.Add(CreateDiagnostic(
                AtlasDiagnosticSeverity.Warning,
                AtlasDiagnosticCodes.ExamplesRootNotFound,
                "generator-library/atlas/examples folder was not found.",
                "examples"));
        }

        AddDuplicateIdDiagnostics(idOwners, diagnostics);
        AddUnknownReferenceDiagnostics(documents, examples, idOwners.Keys, diagnostics);

        return BuildResult(atlasRoot, documents, examples, diagnostics, idOwners);
    }

    private static async Task<AtlasDocumentSummary> LoadDocumentSummaryAsync(
        string atlasRoot,
        string path,
        List<AtlasDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var relativePath = ToAtlasRelativePath(atlasRoot, path);
        try
        {
            var rawJson = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(rawJson, DocumentOptions);
            var root = document.RootElement;

            var ids = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            var references = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            CollectIdsAndReferences(root, "$", null, ids, references);

            var id = ReadString(root, "id") ?? ReadString(root, "example_id");
            var title = ReadString(root, "title");
            var purpose = ReadString(root, "purpose");

            if (string.IsNullOrWhiteSpace(id))
            {
                diagnostics.Add(CreateDiagnostic(
                    AtlasDiagnosticSeverity.Warning,
                    AtlasDiagnosticCodes.MissingIdentity,
                    "Atlas document does not declare root id or example_id.",
                    relativePath));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                diagnostics.Add(CreateDiagnostic(
                    AtlasDiagnosticSeverity.Warning,
                    AtlasDiagnosticCodes.MissingTitle,
                    "Atlas document does not declare root title.",
                    relativePath,
                    id));
            }

            if (string.IsNullOrWhiteSpace(purpose))
            {
                diagnostics.Add(CreateDiagnostic(
                    AtlasDiagnosticSeverity.Warning,
                    AtlasDiagnosticCodes.MissingPurpose,
                    "Atlas document does not declare root purpose.",
                    relativePath,
                    id));
            }

            diagnostics.Add(CreateDiagnostic(
                AtlasDiagnosticSeverity.Info,
                AtlasDiagnosticCodes.LoadedFile,
                "Atlas JSON file was loaded.",
                relativePath,
                id));

            return new AtlasDocumentSummary
            {
                Path = relativePath,
                FileName = Path.GetFileName(path),
                SchemaVersion = ReadString(root, "schema_version") ?? ReadString(root, "atlas_version"),
                Id = id,
                Title = title,
                Purpose = purpose,
                TopLevelIds = ids.ToList(),
                ReferencedIds = references.ToList(),
                Loaded = true
            };
        }
        catch (JsonException ex)
        {
            diagnostics.Add(CreateDiagnostic(
                AtlasDiagnosticSeverity.Error,
                AtlasDiagnosticCodes.InvalidJson,
                ex.Message,
                relativePath));

            return FailedDocument(relativePath, path);
        }
        catch (IOException ex)
        {
            diagnostics.Add(CreateDiagnostic(
                AtlasDiagnosticSeverity.Error,
                AtlasDiagnosticCodes.ReadFailed,
                ex.Message,
                relativePath));

            return FailedDocument(relativePath, path);
        }
    }

    private static async Task<AtlasExampleSummary> LoadExampleSummaryAsync(
        string atlasRoot,
        string path,
        List<AtlasDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var relativePath = ToAtlasRelativePath(atlasRoot, path);
        try
        {
            var rawJson = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(rawJson, DocumentOptions);
            var root = document.RootElement;

            var sourceProfileId = ReadNestedString(root, "source_profile", "id");
            var selectedFeatureBundles = ReadStringArray(root, "selected_feature_bundles");
            var targetArtifacts = ReadStringArray(root, "target_artifacts");
            var stepCount = CountArrayItems(root, "steps");
            var exampleId = ReadString(root, "example_id");

            if (stepCount == 0)
            {
                diagnostics.Add(CreateDiagnostic(
                    AtlasDiagnosticSeverity.Warning,
                    AtlasDiagnosticCodes.ExampleWithoutSteps,
                    "Atlas example has no generator plan steps.",
                    relativePath,
                    exampleId));
            }

            diagnostics.Add(CreateDiagnostic(
                AtlasDiagnosticSeverity.Info,
                AtlasDiagnosticCodes.DiscoveredExample,
                "Atlas generator plan example was discovered.",
                relativePath,
                exampleId));

            return new AtlasExampleSummary
            {
                Path = relativePath,
                ExampleId = exampleId,
                Title = ReadString(root, "title"),
                SourceProfileId = sourceProfileId,
                SelectedFeatureBundles = selectedFeatureBundles,
                TargetArtifacts = targetArtifacts,
                StepCount = stepCount
            };
        }
        catch (JsonException ex)
        {
            diagnostics.Add(CreateDiagnostic(
                AtlasDiagnosticSeverity.Error,
                AtlasDiagnosticCodes.InvalidJson,
                ex.Message,
                relativePath));

            return new AtlasExampleSummary { Path = relativePath };
        }
        catch (IOException ex)
        {
            diagnostics.Add(CreateDiagnostic(
                AtlasDiagnosticSeverity.Error,
                AtlasDiagnosticCodes.ReadFailed,
                ex.Message,
                relativePath));

            return new AtlasExampleSummary { Path = relativePath };
        }
    }

    private static AtlasDocumentSummary FailedDocument(string relativePath, string path)
    {
        return new AtlasDocumentSummary
        {
            Path = relativePath,
            FileName = Path.GetFileName(path),
            Loaded = false
        };
    }

    private static void CollectIdsAndReferences(
        JsonElement element,
        string path,
        string? propertyName,
        ISet<string> ids,
        ISet<string> references)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var propertyPath = path == "$"
                        ? "$." + property.Name
                        : path + "." + property.Name;

                    CollectIdsAndReferences(property.Value, propertyPath, property.Name, ids, references);
                }

                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    CollectIdsAndReferences(item, path + "[]", propertyName, ids, references);
                }

                break;

            case JsonValueKind.String:
                var value = element.GetString();
                if (string.IsNullOrWhiteSpace(value))
                {
                    break;
                }

                if (IsIdentityProperty(propertyName) && !IsReferenceOnlyIdentityPath(path))
                {
                    ids.Add(value.Trim());
                }
                else if (IsReferenceProperty(propertyName) && IsReferenceLike(value))
                {
                    references.Add(value.Trim());
                }

                break;
        }
    }

    private static bool IsIdentityProperty(string? propertyName)
    {
        return propertyName is not null &&
               (propertyName.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                propertyName.Equals("example_id", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsReferenceProperty(string? propertyName)
    {
        return propertyName is not null && ReferencePropertyNames.Contains(propertyName);
    }

    private static bool IsReferenceOnlyIdentityPath(string path)
    {
        return path.EndsWith(".source_profile.id", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsReferenceLike(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return false;
        }

        return ReferencePrefixes.Any(prefix => trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static void RegisterIds(
        IEnumerable<string> ids,
        string owner,
        IDictionary<string, List<string>> idOwners)
    {
        foreach (var id in ids)
        {
            RegisterId(id, owner, idOwners);
        }
    }

    private static void RegisterId(
        string? id,
        string owner,
        IDictionary<string, List<string>> idOwners)
    {
        if (string.IsNullOrWhiteSpace(id) || IsLocalStepId(id))
        {
            return;
        }

        var normalized = id.Trim();
        if (!idOwners.TryGetValue(normalized, out var owners))
        {
            owners = new List<string>();
            idOwners[normalized] = owners;
        }

        if (!owners.Contains(owner, StringComparer.OrdinalIgnoreCase))
        {
            owners.Add(owner);
        }
    }

    private static bool IsLocalStepId(string id)
    {
        return id.StartsWith("step/", StringComparison.OrdinalIgnoreCase);
    }

    private static void AddDuplicateIdDiagnostics(
        IReadOnlyDictionary<string, List<string>> idOwners,
        List<AtlasDiagnostic> diagnostics)
    {
        foreach (var pair in idOwners.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (pair.Value.Count <= 1)
            {
                continue;
            }

            diagnostics.Add(CreateDiagnostic(
                AtlasDiagnosticSeverity.Error,
                AtlasDiagnosticCodes.DuplicateId,
                $"Atlas id '{pair.Key}' appears in multiple documents: {string.Join(", ", pair.Value)}.",
                pair.Value[0],
                pair.Key));
        }
    }

    private static void AddUnknownReferenceDiagnostics(
        IReadOnlyList<AtlasDocumentSummary> documents,
        IReadOnlyList<AtlasExampleSummary> examples,
        IEnumerable<string> knownIds,
        List<AtlasDiagnostic> diagnostics)
    {
        var known = new HashSet<string>(knownIds, StringComparer.OrdinalIgnoreCase);

        foreach (var document in documents)
        {
            foreach (var reference in document.ReferencedIds.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!IsReferenceLike(reference) || known.Contains(reference))
                {
                    continue;
                }

                diagnostics.Add(CreateDiagnostic(
                    AtlasDiagnosticSeverity.Warning,
                    AtlasDiagnosticCodes.ReferenceUnknown,
                    $"Atlas reference '{reference}' was not found in loaded atlas ids.",
                    document.Path,
                    reference));
            }
        }

        foreach (var example in examples)
        {
            if (!string.IsNullOrWhiteSpace(example.SourceProfileId) &&
                IsReferenceLike(example.SourceProfileId) &&
                !known.Contains(example.SourceProfileId))
            {
                diagnostics.Add(CreateDiagnostic(
                    AtlasDiagnosticSeverity.Warning,
                    AtlasDiagnosticCodes.ExampleUnknownProfileReference,
                    $"Example source profile '{example.SourceProfileId}' was not found in loaded atlas ids.",
                    example.Path,
                    example.SourceProfileId));
            }

            foreach (var bundleId in example.SelectedFeatureBundles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!IsReferenceLike(bundleId) || known.Contains(bundleId))
                {
                    continue;
                }

                diagnostics.Add(CreateDiagnostic(
                    AtlasDiagnosticSeverity.Warning,
                    AtlasDiagnosticCodes.ReferenceUnknown,
                    $"Example feature bundle reference '{bundleId}' was not found in loaded atlas ids.",
                    example.Path,
                    bundleId));
            }
        }
    }

    private static AtlasRegistryImportResult BuildResult(
        string atlasRoot,
        IReadOnlyList<AtlasDocumentSummary> documents,
        IReadOnlyList<AtlasExampleSummary> examples,
        IReadOnlyList<AtlasDiagnostic> diagnostics,
        IReadOnlyDictionary<string, List<string>> idOwners)
    {
        var errorCount = diagnostics.Count(diagnostic => diagnostic.Severity == AtlasDiagnosticSeverity.Error);
        var warningCount = diagnostics.Count(diagnostic => diagnostic.Severity == AtlasDiagnosticSeverity.Warning);

        return new AtlasRegistryImportResult
        {
            Ok = errorCount == 0,
            AtlasRoot = atlasRoot,
            Documents = documents
                .OrderBy(document => document.Path, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Examples = examples
                .OrderBy(example => example.Path, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Diagnostics = diagnostics.ToList(),
            Summary = new AtlasRegistrySummary
            {
                DocumentCount = documents.Count,
                LoadedDocumentCount = documents.Count(document => document.Loaded),
                ExampleCount = examples.Count,
                UniqueIdCount = idOwners.Count,
                ErrorCount = errorCount,
                WarningCount = warningCount
            }
        };
    }

    private static string? ResolveAtlasRoot(string rootOrAtlasRoot)
    {
        if (string.IsNullOrWhiteSpace(rootOrAtlasRoot))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(rootOrAtlasRoot);
        var current = new DirectoryInfo(fullPath);
        if (!current.Exists)
        {
            return null;
        }

        if (IsAtlasRoot(current.FullName))
        {
            return current.FullName;
        }

        if (current.Name.Equals("generator-library", StringComparison.OrdinalIgnoreCase))
        {
            var atlasCandidate = Path.Combine(current.FullName, "atlas");
            if (IsAtlasRoot(atlasCandidate))
            {
                return atlasCandidate;
            }
        }

        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, "generator-library", "atlas");
            if (IsAtlasRoot(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return null;
    }

    private static bool IsAtlasRoot(string path)
    {
        return Directory.Exists(path) &&
               (File.Exists(Path.Combine(path, "ATLAS_INDEX.md")) ||
                File.Exists(Path.Combine(path, "capability_atlas.json")));
    }

    private static bool IsPathInsideRoot(string root, string path)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(path);
        return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static string ToAtlasRelativePath(string atlasRoot, string path)
    {
        return Path.GetRelativePath(atlasRoot, path).Replace('\\', '/');
    }

    private static string? ReadString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? ReadNestedString(JsonElement root, string objectPropertyName, string nestedPropertyName)
    {
        if (!root.TryGetProperty(objectPropertyName, out var nested) ||
            nested.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return ReadString(nested, nestedPropertyName);
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return property
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int CountArrayItems(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        return property.GetArrayLength();
    }

    private static AtlasDiagnostic CreateDiagnostic(
        string severity,
        string code,
        string message,
        string? path = null,
        string? id = null)
    {
        return new AtlasDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            Path = path,
            Id = id
        };
    }
}
