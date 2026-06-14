using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanCapabilitySelectionAtlasReader
{
    private static readonly string[] RequiredAtlasFiles =
    [
        "game_form_factor_taxonomy.json",
        "game_system_variant_taxonomy.json",
        "feature_bundle_map.json",
        "capability_atlas.json",
        "artifact_contracts.json"
    ];

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    public async Task<GeneratorPlanCapabilitySelectionAtlas> LoadAsync(
        string atlasRootPath,
        CancellationToken cancellationToken = default)
    {
        var diagnostics = new List<GeneratorPlanCapabilitySelectionDiagnostic>();
        var atlasRoot = ResolveAtlasRoot(atlasRootPath);
        if (atlasRoot == null)
        {
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanCapabilitySelectionDiagnosticCodes.AtlasRootMissing,
                "generator-library/atlas folder was not found.",
                string.IsNullOrWhiteSpace(atlasRootPath) ? "atlas_root" : atlasRootPath.Trim()));

            return new GeneratorPlanCapabilitySelectionAtlas { Diagnostics = diagnostics };
        }

        var documents = new Dictionary<string, JsonDocument>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var file in RequiredAtlasFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var path = Path.Combine(atlasRoot, file);
                if (!IsPathInsideRoot(atlasRoot, path) || !File.Exists(path))
                {
                    diagnostics.Add(Diagnostic(
                        GeneratorPlanPreviewDiagnosticSeverity.Error,
                        GeneratorPlanCapabilitySelectionDiagnosticCodes.AtlasFileMissing,
                        $"Required atlas file '{file}' was not found.",
                        file));
                    continue;
                }

                try
                {
                    var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
                    documents[file] = JsonDocument.Parse(json, DocumentOptions);
                }
                catch (JsonException ex)
                {
                    diagnostics.Add(Diagnostic(
                        GeneratorPlanPreviewDiagnosticSeverity.Error,
                        GeneratorPlanCapabilitySelectionDiagnosticCodes.AtlasInvalidJson,
                        ex.Message,
                        file));
                }
                catch (IOException ex)
                {
                    diagnostics.Add(Diagnostic(
                        GeneratorPlanPreviewDiagnosticSeverity.Error,
                        GeneratorPlanCapabilitySelectionDiagnosticCodes.AtlasFileMissing,
                        ex.Message,
                        file));
                }
            }

            if (diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error))
            {
                return new GeneratorPlanCapabilitySelectionAtlas
                {
                    AtlasRootPath = atlasRoot,
                    Diagnostics = diagnostics
                };
            }

            var formFactor = documents["game_form_factor_taxonomy.json"].RootElement;
            var systemTaxonomy = documents["game_system_variant_taxonomy.json"].RootElement;
            var featureBundles = documents["feature_bundle_map.json"].RootElement;
            var capabilityAtlas = documents["capability_atlas.json"].RootElement;
            var artifactContracts = documents["artifact_contracts.json"].RootElement;

            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Info,
                GeneratorPlanCapabilitySelectionDiagnosticCodes.Loaded,
                "Capability selection atlas files were loaded.",
                atlasRoot));

            return new GeneratorPlanCapabilitySelectionAtlas
            {
                AtlasRootPath = atlasRoot,
                PresentationModes = ReadOptions(formFactor, "presentation_modes"),
                WorldTopologies = ReadOptions(systemTaxonomy, "world_topologies"),
                ActorModels = ReadOptions(systemTaxonomy, "actor_models"),
                InventoryModels = ReadOptions(systemTaxonomy, "inventory_models"),
                CombatModels = ReadOptions(systemTaxonomy, "combat_models"),
                ProgressionModels = ReadOptions(systemTaxonomy, "progression_models"),
                PathfindingProfiles = ReadOptions(systemTaxonomy, "pathfinding_profiles"),
                NpcBehaviorModels = ReadOptions(systemTaxonomy, "npc_behavior_models"),
                FeatureBundles = ReadFeatureBundles(featureBundles, capabilityAtlas),
                RuntimeTargets = ReadRuntimeTargets(capabilityAtlas),
                Capabilities = ReadCapabilities(capabilityAtlas),
                ArtifactContracts = ReadArtifactContracts(artifactContracts, capabilityAtlas),
                Diagnostics = diagnostics
            };
        }
        finally
        {
            foreach (var document in documents.Values)
            {
                document.Dispose();
            }
        }
    }

    public string DiscoverAtlasRoot()
    {
        return ResolveAtlasRoot(string.Empty) ?? string.Empty;
    }

    private static IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> ReadOptions(JsonElement root, string propertyName)
    {
        if (!TryGetArray(root, propertyName, out var array))
        {
            return Array.Empty<GeneratorPlanCapabilitySelectionAtlasOption>();
        }

        return array
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Object)
            .Select(item => new GeneratorPlanCapabilitySelectionAtlasOption
            {
                Id = ReadString(item, "id"),
                Title = ReadString(item, "title"),
                Purpose = ReadString(item, "purpose"),
                RequiredArtifactContracts = ReadStringArray(item, "required_artifact_contracts"),
                RequiredValidators = ReadStringArray(item, "required_validators"),
                CompatibleWith = ReadStringArray(item, "compatible_with"),
                IncompatibleWith = ReadStringArray(item, "incompatible_with"),
                AllowedWorldTopologies = ReadStringArray(item, "allowed_world_topologies"),
                RecommendedActorModels = ReadStringArray(item, "recommended_actor_models"),
                RecommendedCombatModels = ReadStringArray(item, "recommended_combat_models")
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.Id))
            .OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<GeneratorPlanCapabilitySelectionFeatureBundle> ReadFeatureBundles(JsonElement featureBundleMap, JsonElement capabilityAtlas)
    {
        var bundles = new Dictionary<string, GeneratorPlanCapabilitySelectionFeatureBundle>(StringComparer.OrdinalIgnoreCase);

        foreach (var bundle in ReadFeatureBundleArray(featureBundleMap, "feature_bundles"))
        {
            bundles[bundle.Id] = bundle;
        }

        foreach (var bundle in ReadFeatureBundleArray(capabilityAtlas, "feature_bundles"))
        {
            if (!bundles.ContainsKey(bundle.Id))
            {
                bundles[bundle.Id] = bundle;
            }
        }

        return bundles.Values
            .OrderBy(bundle => bundle.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(bundle => bundle.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<GeneratorPlanCapabilitySelectionFeatureBundle> ReadFeatureBundleArray(JsonElement root, string propertyName)
    {
        if (!TryGetArray(root, propertyName, out var array))
        {
            return Array.Empty<GeneratorPlanCapabilitySelectionFeatureBundle>();
        }

        return array
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Object)
            .Select(item => new GeneratorPlanCapabilitySelectionFeatureBundle
            {
                Id = ReadString(item, "id"),
                Title = ReadString(item, "title"),
                Domain = ReadString(item, "domain"),
                Category = ReadString(item, "category"),
                Purpose = ReadString(item, "purpose"),
                Requires = ReadStringArray(item, "requires"),
                Provides = ReadStringArray(item, "provides"),
                ArtifactContracts = ReadStringArray(item, "artifact_contracts", "outputs", "output_contracts"),
                Validators = ReadStringArray(item, "validators"),
                RuntimeTargets = ReadStringArray(item, "runtime_targets"),
                PromptContextTemplates = ReadStringArray(item, "prompt_context_templates"),
                FutureModuleGaps = ReadStringArray(item, "future_module_gaps"),
                IncompatibleWith = ReadStringArray(item, "incompatible_with"),
                RecommendedWith = ReadStringArray(item, "recommended_with")
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.Id))
            .ToList();
    }

    private static IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> ReadRuntimeTargets(JsonElement capabilityAtlas)
    {
        return ReadOptions(capabilityAtlas, "runtime_targets");
    }

    private static IReadOnlyList<GeneratorPlanCapabilitySelectionCapability> ReadCapabilities(JsonElement capabilityAtlas)
    {
        if (!TryGetArray(capabilityAtlas, "domains", out var domains))
        {
            return Array.Empty<GeneratorPlanCapabilitySelectionCapability>();
        }

        var capabilities = new List<GeneratorPlanCapabilitySelectionCapability>();
        foreach (var domain in domains.EnumerateArray())
        {
            if (!TryGetArray(domain, "capabilities", out var capabilityArray))
            {
                continue;
            }

            capabilities.AddRange(capabilityArray
                .EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Object)
                .Select(item => new GeneratorPlanCapabilitySelectionCapability
                {
                    Id = ReadString(item, "id"),
                    Title = ReadString(item, "title"),
                    Provides = ReadStringArray(item, "provides"),
                    DependsOn = ReadStringArray(item, "depends_on"),
                    OutputContracts = ReadStringArray(item, "output_contracts"),
                    Validators = ReadStringArray(item, "validators"),
                    RuntimeTargets = ReadStringArray(item, "runtime_targets")
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.Id)));
        }

        return capabilities
            .OrderBy(capability => capability.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(capability => capability.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<GeneratorPlanCapabilitySelectionArtifactContract> ReadArtifactContracts(JsonElement artifactContracts, JsonElement capabilityAtlas)
    {
        var contracts = new Dictionary<string, GeneratorPlanCapabilitySelectionArtifactContract>(StringComparer.OrdinalIgnoreCase);

        foreach (var contract in ReadArtifactContractArray(artifactContracts, "contracts"))
        {
            contracts[contract.Id] = contract;
        }

        foreach (var contract in ReadArtifactContractArray(capabilityAtlas, "artifact_contracts"))
        {
            if (!contracts.ContainsKey(contract.Id))
            {
                contracts[contract.Id] = contract;
            }
        }

        return contracts.Values
            .OrderBy(contract => contract.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(contract => contract.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<GeneratorPlanCapabilitySelectionArtifactContract> ReadArtifactContractArray(JsonElement root, string propertyName)
    {
        if (!TryGetArray(root, propertyName, out var array))
        {
            return Array.Empty<GeneratorPlanCapabilitySelectionArtifactContract>();
        }

        return array
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Object)
            .Select(item => new GeneratorPlanCapabilitySelectionArtifactContract
            {
                Id = ReadString(item, "id"),
                Title = ReadString(item, "title"),
                Purpose = ReadString(item, "purpose"),
                RequiredValidators = ReadStringArray(item, "required_validators", "validation_levels")
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.Id))
            .ToList();
    }

    private static string? ResolveAtlasRoot(string rootOrAtlasRoot)
    {
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(rootOrAtlasRoot))
        {
            candidates.Add(rootOrAtlasRoot.Trim());
        }

        candidates.Add(Environment.CurrentDirectory);
        candidates.Add(AppContext.BaseDirectory);

        foreach (var candidate in candidates)
        {
            var resolved = ResolveFromCandidate(candidate);
            if (resolved != null)
            {
                return resolved;
            }
        }

        return null;
    }

    private static string? ResolveFromCandidate(string candidatePath)
    {
        if (string.IsNullOrWhiteSpace(candidatePath))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(candidatePath);
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
            var atlasCandidate = Path.Combine(current.FullName, "generator-library", "atlas");
            if (IsAtlasRoot(atlasCandidate))
            {
                return atlasCandidate;
            }

            current = current.Parent;
        }

        return null;
    }

    private static bool IsAtlasRoot(string path)
    {
        return Directory.Exists(path) &&
               RequiredAtlasFiles.All(file => File.Exists(Path.Combine(path, file)));
    }

    private static bool IsPathInsideRoot(string root, string path)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(path);
        return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetArray(JsonElement root, string propertyName, out JsonElement array)
    {
        return root.TryGetProperty(propertyName, out array) && array.ValueKind == JsonValueKind.Array;
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return string.Empty;
        }

        return property.GetString()?.Trim() ?? string.Empty;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
            {
                continue;
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

        return Array.Empty<string>();
    }

    private static GeneratorPlanCapabilitySelectionDiagnostic Diagnostic(
        string severity,
        string code,
        string message,
        string target)
    {
        return new GeneratorPlanCapabilitySelectionDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            Target = target
        };
    }
}
