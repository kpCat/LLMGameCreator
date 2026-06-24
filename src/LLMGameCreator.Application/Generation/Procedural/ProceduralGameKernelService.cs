using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.Semantics;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class ProceduralGameKernelService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural";
    public const string GeneratedPlanJsonFileName = "generated-game-plan.json";
    public const string GeneratedPlanMarkdownFileName = "generated-game-plan.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly ProceduralGamePlanMarkdownRenderer _markdownRenderer;

    public ProceduralGameKernelService(ProceduralGamePlanMarkdownRenderer? markdownRenderer = null)
    {
        _markdownRenderer = markdownRenderer ?? new ProceduralGamePlanMarkdownRenderer();
    }

    public ProceduralGameKernelResult Generate(ProceduralGameKernelRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var diagnostics = new List<ProceduralGameDiagnostic>();
        var seed = NormalizeSeed(request.Seed, diagnostics);
        var mode = NormalizeMode(request.Mode, diagnostics);
        var variantIds = NormalizeVariantIds(request.SelectedVariantIds, mode, diagnostics);
        var styleHintIds = NormalizeStyleHintIds(request.CompactStyleHintIds, request.SemanticCatalog, diagnostics);
        var rng = new StableProceduralRandom(StableHashToUInt64(seed, mode, variantIds, styleHintIds));

        var world = BuildWorld(seed, mode, variantIds, styleHintIds, rng);
        var factions = BuildFactions(world, styleHintIds, rng);
        var actors = BuildActors(world, factions, styleHintIds, rng);
        var items = BuildItems(world, styleHintIds, rng);
        var placeholders = BuildPlaceholders();
        var encounters = BuildEncounters(world, factions, actors, items, placeholders, rng);
        var questEvents = BuildQuestEvents(world, factions, items, encounters, placeholders, rng);

        diagnostics.Add(new ProceduralGameDiagnostic
        {
            Severity = "info",
            Code = "procedural_kernel.slice030_required",
            Target = "formulaEffectActionPlaceholders",
            Message = "Formula, effect, action and reward behavior is represented as placeholders for Product Slice 030."
        });
        diagnostics.Add(new ProceduralGameDiagnostic
        {
            Severity = "info",
            Code = "procedural_kernel.no_external_execution",
            Target = "generation",
            Message = "No LLM, provider, Lua, Unity, media or runtime execution was invoked."
        });

        var stableSummary = BuildStableSummary(world, factions, actors, items, encounters, questEvents);
        var deterministicHash = ComputeHash(string.Join("\n", new[]
        {
            seed,
            mode,
            string.Join("|", variantIds),
            string.Join("|", styleHintIds),
            stableSummary
        }));

        var planWithoutMarkdown = new ProceduralGeneratedGamePlan
        {
            Metadata = new ProceduralGenerationMetadata
            {
                Seed = seed,
                Mode = mode,
                DeterministicHash = deterministicHash,
                StableSummary = stableSummary
            },
            Profile = new ProceduralGenerationProfile
            {
                VariantIds = variantIds,
                StyleHintIds = styleHintIds
            },
            World = world,
            Factions = factions,
            ActorSeeds = actors,
            ItemResourceSeeds = items,
            EncounterSeeds = encounters,
            QuestEventSeeds = questEvents,
            FormulaEffectActionPlaceholders = placeholders,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var markdown = _markdownRenderer.Render(planWithoutMarkdown);
        var plan = planWithoutMarkdown with { MarkdownSummary = markdown };
        var json = JsonSerializer.Serialize(plan, JsonOptions);

        return new ProceduralGameKernelResult
        {
            Plan = plan,
            Json = json,
            Markdown = markdown,
            Diagnostics = plan.Diagnostics
        };
    }

    public async Task<ProceduralGameKernelWriteResult> WriteAsync(
        string projectRootPath,
        ProceduralGameKernelResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.GetFullPath(Path.Combine(outputDirectory, GeneratedPlanJsonFileName));
        var markdownPath = Path.GetFullPath(Path.Combine(outputDirectory, GeneratedPlanMarkdownFileName));
        EnsureContained(outputDirectory, jsonPath);
        EnsureContained(outputDirectory, markdownPath);

        await File.WriteAllTextAsync(jsonPath, result.Json, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.Markdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new ProceduralGameKernelWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            JsonPath = jsonPath,
            MarkdownPath = markdownPath
        };
    }

    private static string NormalizeSeed(string seed, ICollection<ProceduralGameDiagnostic> diagnostics)
    {
        var normalized = (seed ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            return normalized;
        }

        diagnostics.Add(new ProceduralGameDiagnostic
        {
            Severity = "warning",
            Code = "procedural_kernel.invalid_seed",
            Target = "seed",
            Message = "Seed was empty and was replaced with deterministic fallback seed 'default_seed'."
        });
        return "default_seed";
    }

    private static string NormalizeMode(string mode, ICollection<ProceduralGameDiagnostic> diagnostics)
    {
        if (TryNormalizeSegment(mode, out var normalized) && ProceduralGameGenerationModes.Supported.Contains(normalized))
        {
            return normalized;
        }

        diagnostics.Add(new ProceduralGameDiagnostic
        {
            Severity = "warning",
            Code = "procedural_kernel.invalid_mode",
            Target = "mode",
            Message = $"Mode '{mode}' is not supported and was replaced with '{ProceduralGameGenerationModes.AuthoredSmallWorld}'."
        });
        return ProceduralGameGenerationModes.AuthoredSmallWorld;
    }

    private static IReadOnlyList<string> NormalizeVariantIds(
        IReadOnlyList<string> selectedVariantIds,
        string mode,
        ICollection<ProceduralGameDiagnostic> diagnostics)
    {
        var ids = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var id in selectedVariantIds)
        {
            if (TryNormalizeId(id, out var normalized))
            {
                ids.Add(normalized);
            }
            else if (!string.IsNullOrWhiteSpace(id))
            {
                diagnostics.Add(new ProceduralGameDiagnostic
                {
                    Severity = "warning",
                    Code = "procedural_kernel.invalid_variant_id",
                    Target = id,
                    Message = "Selected variant id was unsafe and was skipped."
                });
            }
        }

        AddDefaultVariant(ids, "world_topology/", mode == ProceduralGameGenerationModes.FullySeededWorld
            ? "world_topology/infinite_chunks"
            : "world_topology/region_graph");
        AddDefaultVariant(ids, "chunk_streaming/", mode == ProceduralGameGenerationModes.FullySeededWorld
            ? "chunk_streaming/generated_on_demand"
            : "chunk_streaming/none");
        AddDefaultVariant(ids, "actor_model/", "actor_model/single_player_character");
        AddDefaultVariant(ids, "inventory_model/", "inventory_model/list_inventory");
        AddDefaultVariant(ids, "combat_model/", "combat_model/turn_based");
        AddDefaultVariant(ids, "progression_model/", "progression_model/reputation_tracks");
        AddDefaultVariant(ids, "pathfinding/", mode == ProceduralGameGenerationModes.FullySeededWorld
            ? "pathfinding/chunk_aware_pathfinding"
            : "pathfinding/region_graph");
        AddDefaultVariant(ids, "npc_behavior/", "npc_behavior/faction_driven");

        return ids.ToList();
    }

    private static void AddDefaultVariant(ISet<string> ids, string prefix, string defaultId)
    {
        if (!ids.Any(id => id.StartsWith(prefix, StringComparison.Ordinal)))
        {
            ids.Add(defaultId);
        }
    }

    private static IReadOnlyList<string> NormalizeStyleHintIds(
        IReadOnlyList<string> compactStyleHintIds,
        SemanticCatalog? semanticCatalog,
        ICollection<ProceduralGameDiagnostic> diagnostics)
    {
        var ids = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var id in compactStyleHintIds)
        {
            AddStyleHint(id, "styleHints", ids, diagnostics);
        }

        if (semanticCatalog is not null)
        {
            foreach (var term in semanticCatalog.Terms
                         .Where(IsGenerationHintTerm)
                         .OrderBy(term => term.TermId, StringComparer.Ordinal)
                         .Take(24))
            {
                AddStyleHint(term.TermId, "semanticCatalog", ids, diagnostics);
            }
        }

        if (ids.Count == 0)
        {
            ids.Add("theme/exploration");
            ids.Add("tone/mysterious");
        }

        return ids.Take(24).ToList();
    }

    private static bool IsGenerationHintTerm(SemanticCatalogTerm term) =>
        (term.Status is SemanticTermStatuses.Known or SemanticTermStatuses.Candidate) &&
        (term.Kind is SemanticTermKinds.Theme or SemanticTermKinds.Tone or SemanticTermKinds.Biome
            or SemanticTermKinds.Faction or SemanticTermKinds.NpcArchetype or SemanticTermKinds.QuestMotif
            or SemanticTermKinds.ItemAffordance or SemanticTermKinds.LocationMood or SemanticTermKinds.EntityRole);

    private static void AddStyleHint(
        string id,
        string target,
        ISet<string> ids,
        ICollection<ProceduralGameDiagnostic> diagnostics)
    {
        if (TryNormalizeId(id, out var normalized))
        {
            ids.Add(normalized);
            return;
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        diagnostics.Add(new ProceduralGameDiagnostic
        {
            Severity = "warning",
            Code = "procedural_kernel.invalid_style_hint_id",
            Target = target,
            Message = $"Style hint id '{id}' was unsafe and was skipped."
        });
    }

    private static ProceduralWorldPlan BuildWorld(
        string seed,
        string mode,
        IReadOnlyList<string> variantIds,
        IReadOnlyList<string> styleHintIds,
        StableProceduralRandom rng)
    {
        var topology = variantIds.FirstOrDefault(id => id.StartsWith("world_topology/", StringComparison.Ordinal))
                       ?? "world_topology/region_graph";
        var regionCount = mode switch
        {
            ProceduralGameGenerationModes.AuthoredSmallWorld => 4,
            ProceduralGameGenerationModes.SemiProceduralRegions => 5,
            _ => 6
        };
        var labels = PickDistinct(RegionLabels, regionCount, rng);
        var moods = PickCycle(StyleHintsByPrefix(styleHintIds, "location_mood/"), regionCount, rng, LocationMoodFallbacks);
        var themes = PickCycle(StyleHintsByPrefix(styleHintIds, "theme/"), regionCount, rng, ThemeFallbacks);
        var regions = labels.Select((label, index) => new ProceduralRegionSeed
            {
                RegionId = $"region/{NormalizeLabelSegment(label)}",
                Label = label,
                MoodHintId = moods[index],
                Tags = [themes[index], mode]
            })
            .OrderBy(region => region.RegionId, StringComparer.Ordinal)
            .ToList();

        var connections = new List<ProceduralRegionConnection>();
        for (var index = 1; index < regions.Count; index++)
        {
            connections.Add(Connection(regions[0], regions[index], index));
        }

        for (var index = 1; index < regions.Count - 1; index++)
        {
            connections.Add(Connection(regions[index], regions[index + 1], index + regions.Count));
        }

        return new ProceduralWorldPlan
        {
            WorldId = $"world/{NormalizeLabelSegment(seed)}",
            TopologyVariantId = topology,
            Regions = regions,
            Connections = connections
                .OrderBy(connection => connection.ConnectionId, StringComparer.Ordinal)
                .ToList()
        };

        static ProceduralRegionConnection Connection(ProceduralRegionSeed from, ProceduralRegionSeed to, int index) => new()
        {
            ConnectionId = $"connection/{from.RegionId[(from.RegionId.LastIndexOf('/') + 1)..]}__{to.RegionId[(to.RegionId.LastIndexOf('/') + 1)..]}",
            FromRegionId = from.RegionId,
            ToRegionId = to.RegionId,
            GateRequirementPlaceholderId = index % 2 == 0
                ? "requirement/open_route"
                : "requirement/faction_access"
        };
    }

    private static IReadOnlyList<ProceduralFactionSeed> BuildFactions(
        ProceduralWorldPlan world,
        IReadOnlyList<string> styleHintIds,
        StableProceduralRandom rng)
    {
        var labels = PickDistinct(FactionLabels, 3, rng);
        var motives = PickCycle(StyleHintsByPrefix(styleHintIds, "quest_motif/"), labels.Count, rng, QuestMotifFallbacks);
        return labels.Select((label, index) => new ProceduralFactionSeed
            {
                FactionId = $"faction/{NormalizeLabelSegment(label)}",
                Label = label,
                HomeRegionId = world.Regions[index % world.Regions.Count].RegionId,
                MotiveHintId = motives[index]
            })
            .OrderBy(faction => faction.FactionId, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<ProceduralActorSeed> BuildActors(
        ProceduralWorldPlan world,
        IReadOnlyList<ProceduralFactionSeed> factions,
        IReadOnlyList<string> styleHintIds,
        StableProceduralRandom rng)
    {
        var archetypes = PickCycle(StyleHintsByPrefix(styleHintIds, "npc_archetype/"), 6, rng, ActorArchetypeFallbacks);
        var roles = PickCycle(StyleHintsByPrefix(styleHintIds, "entity_role/"), 6, rng, ActorRoleFallbacks);
        return Enumerable.Range(0, 6)
            .Select(index => new ProceduralActorSeed
            {
                ActorSeedId = $"actor_seed/{NormalizeLabelSegment(archetypes[index])}_{index + 1:D2}",
                ArchetypeId = archetypes[index],
                FactionId = factions[index % factions.Count].FactionId,
                RegionId = world.Regions[(index + 1) % world.Regions.Count].RegionId,
                RoleHintId = roles[index]
            })
            .OrderBy(actor => actor.ActorSeedId, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<ProceduralItemResourceSeed> BuildItems(
        ProceduralWorldPlan world,
        IReadOnlyList<string> styleHintIds,
        StableProceduralRandom rng)
    {
        var resources = PickDistinct(ResourceKinds, 5, rng);
        var affordances = PickCycle(StyleHintsByPrefix(styleHintIds, "item_affordance/"), resources.Count, rng, ItemAffordanceFallbacks);
        return resources.Select((resource, index) => new ProceduralItemResourceSeed
            {
                ItemSeedId = $"item_seed/{NormalizeLabelSegment(resource)}",
                ResourceKindId = $"resource/{NormalizeLabelSegment(resource)}",
                RegionId = world.Regions[index % world.Regions.Count].RegionId,
                AffordanceHintId = affordances[index]
            })
            .OrderBy(item => item.ItemSeedId, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<ProceduralFormulaEffectActionPlaceholder> BuildPlaceholders() =>
    [
        new()
        {
            PlaceholderId = "requirement/open_route",
            Kind = "requirement",
            Summary = "Check whether a region connection is open."
        },
        new()
        {
            PlaceholderId = "requirement/faction_access",
            Kind = "requirement",
            Summary = "Check faction reputation or story access."
        },
        new()
        {
            PlaceholderId = "action/resolve_encounter",
            Kind = "action",
            Summary = "Resolve an encounter into state changes."
        },
        new()
        {
            PlaceholderId = "reward/quest_progress",
            Kind = "reward",
            Summary = "Advance quest/event state and grant a resource."
        }
    ];

    private static IReadOnlyList<ProceduralEncounterSeed> BuildEncounters(
        ProceduralWorldPlan world,
        IReadOnlyList<ProceduralFactionSeed> factions,
        IReadOnlyList<ProceduralActorSeed> actors,
        IReadOnlyList<ProceduralItemResourceSeed> items,
        IReadOnlyList<ProceduralFormulaEffectActionPlaceholder> placeholders,
        StableProceduralRandom rng)
    {
        var labels = PickDistinct(EncounterLabels, 4, rng);
        return labels.Select((label, index) =>
            {
                var region = world.Regions[index % world.Regions.Count];
                var factionPair = factions.Skip(index % factions.Count).Concat(factions).Take(2).Select(faction => faction.FactionId).OrderBy(id => id, StringComparer.Ordinal).ToList();
                var actorPair = actors.Where(actor => factionPair.Contains(actor.FactionId, StringComparer.Ordinal)).Take(2).Select(actor => actor.ActorSeedId).OrderBy(id => id, StringComparer.Ordinal).ToList();
                return new ProceduralEncounterSeed
                {
                    EncounterSeedId = $"encounter_seed/{NormalizeLabelSegment(label)}",
                    RegionId = region.RegionId,
                    FactionIds = factionPair,
                    ActorSeedIds = actorPair.Count == 0 ? [actors[index % actors.Count].ActorSeedId] : actorPair,
                    RewardItemSeedIds = [items[index % items.Count].ItemSeedId],
                    ActionPlaceholderId = placeholders.First(item => item.Kind == "action").PlaceholderId
                };
            })
            .OrderBy(encounter => encounter.EncounterSeedId, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<ProceduralQuestEventSeed> BuildQuestEvents(
        ProceduralWorldPlan world,
        IReadOnlyList<ProceduralFactionSeed> factions,
        IReadOnlyList<ProceduralItemResourceSeed> items,
        IReadOnlyList<ProceduralEncounterSeed> encounters,
        IReadOnlyList<ProceduralFormulaEffectActionPlaceholder> placeholders,
        StableProceduralRandom rng)
    {
        var labels = PickDistinct(QuestEventLabels, 3, rng);
        return labels.Select((label, index) => new ProceduralQuestEventSeed
            {
                QuestEventSeedId = $"quest_event_seed/{NormalizeLabelSegment(label)}",
                RegionId = world.Regions[(index + 1) % world.Regions.Count].RegionId,
                SourceFactionId = factions[index % factions.Count].FactionId,
                TargetEncounterSeedId = encounters[index % encounters.Count].EncounterSeedId,
                RequiredItemSeedId = items[(index + 1) % items.Count].ItemSeedId,
                RewardPlaceholderId = placeholders.First(item => item.Kind == "reward").PlaceholderId
            })
            .OrderBy(quest => quest.QuestEventSeedId, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> StyleHintsByPrefix(IReadOnlyList<string> styleHintIds, string prefix) =>
        styleHintIds.Where(id => id.StartsWith(prefix, StringComparison.Ordinal)).OrderBy(id => id, StringComparer.Ordinal).ToList();

    private static IReadOnlyList<string> PickDistinct(IReadOnlyList<string> source, int count, StableProceduralRandom rng)
    {
        var pool = source.ToList();
        var result = new List<string>(count);
        while (result.Count < count && pool.Count > 0)
        {
            var index = rng.Next(pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    private static IReadOnlyList<string> PickCycle(
        IReadOnlyList<string> preferred,
        int count,
        StableProceduralRandom rng,
        IReadOnlyList<string> fallback)
    {
        var source = preferred.Count > 0 ? preferred : fallback;
        var start = rng.Next(source.Count);
        return Enumerable.Range(0, count)
            .Select(index => source[(start + index) % source.Count])
            .ToList();
    }

    private static string BuildStableSummary(
        ProceduralWorldPlan world,
        IReadOnlyList<ProceduralFactionSeed> factions,
        IReadOnlyList<ProceduralActorSeed> actors,
        IReadOnlyList<ProceduralItemResourceSeed> items,
        IReadOnlyList<ProceduralEncounterSeed> encounters,
        IReadOnlyList<ProceduralQuestEventSeed> questEvents) =>
        string.Join("; ", new[]
        {
            $"world={world.WorldId}",
            $"regions={world.Regions.Count}",
            $"connections={world.Connections.Count}",
            $"factions={factions.Count}",
            $"actors={actors.Count}",
            $"items={items.Count}",
            $"encounters={encounters.Count}",
            $"questEvents={questEvents.Count}"
        });

    private static IReadOnlyList<ProceduralGameDiagnostic> SortDiagnostics(IEnumerable<ProceduralGameDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static ulong StableHashToUInt64(
        string seed,
        string mode,
        IReadOnlyList<string> variantIds,
        IReadOnlyList<string> styleHintIds)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", new[]
        {
            seed,
            mode,
            string.Join("|", variantIds),
            string.Join("|", styleHintIds)
        })));
        return BitConverter.ToUInt64(hash, 0);
    }

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string NormalizeLabelSegment(string value)
    {
        if (TryNormalizeId(value, out var normalized))
        {
            return normalized.Replace('/', '_');
        }

        var builder = new StringBuilder();
        foreach (var character in value.ToLowerInvariant())
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(character);
            }
            else if (char.IsWhiteSpace(character) || character is '-' or '_')
            {
                builder.Append('_');
            }
        }

        var segment = builder.ToString().Trim('_');
        while (segment.Contains("__", StringComparison.Ordinal))
        {
            segment = segment.Replace("__", "_", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(segment) ? "generated" : segment;
    }

    private static bool TryNormalizeId(string value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.Contains('\\') || value.Contains(':'))
        {
            return false;
        }

        var candidate = NormalizeSpaces(value.Trim().ToLowerInvariant());
        if (candidate.Length == 0 || candidate.Length > 128 || candidate.StartsWith('/') || candidate.EndsWith('/'))
        {
            return false;
        }

        var segments = candidate.Split('/', StringSplitOptions.None);
        if (segments.Any(segment => string.IsNullOrWhiteSpace(segment) || segment is "." or "..") ||
            candidate.Any(character => !IsSafeIdCharacter(character)))
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeSegment(string value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || value.Length > 80 || value.Contains('/') || value.Contains('\\') || value.Contains(':'))
        {
            return false;
        }

        var candidate = NormalizeSpaces(value.Trim().ToLowerInvariant());
        if (candidate.Length == 0 || candidate is "." or ".." ||
            candidate.Any(character => !IsSafeSegmentCharacter(character)))
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    private static string NormalizeSpaces(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousUnderscore = false;
        foreach (var character in value)
        {
            var normalized = char.IsWhiteSpace(character) ? '_' : character;
            if (normalized == '_' && previousUnderscore)
            {
                continue;
            }

            builder.Append(normalized);
            previousUnderscore = normalized == '_';
        }

        return builder.ToString().Trim('_');
    }

    private static bool IsSafeIdCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-' or '/';

    private static bool IsSafeSegmentCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-';

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Procedural output path must stay under the project root.");
        }
    }

    private sealed class StableProceduralRandom
    {
        private ulong _state;

        public StableProceduralRandom(ulong seed)
        {
            _state = seed == 0 ? 0x9E3779B97F4A7C15UL : seed;
        }

        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            }

            _state += 0x9E3779B97F4A7C15UL;
            var value = _state;
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            value ^= value >> 31;
            return (int)(value % (uint)maxExclusive);
        }
    }

    private static readonly IReadOnlyList<string> RegionLabels =
    [
        "Ash Gate", "Glass Fen", "Salt Orchard", "Lantern Fold", "Iron Meadow", "Blue Ditch",
        "Moth Harbor", "Cinder Road", "Quiet Spire", "Drift Garden", "Tangle Mill", "Frost Canal"
    ];

    private static readonly IReadOnlyList<string> FactionLabels =
    [
        "Ember Wardens", "Canal Syndicate", "Lantern Keepers", "Glass Foragers", "Moth Court",
        "Roadbound Surveyors", "Iron Choir", "Salt Orchard League"
    ];

    private static readonly IReadOnlyList<string> ResourceKinds =
    [
        "signal_ink", "field_ration", "route_key", "bright_salt", "repair_fiber",
        "trade_charm", "weathered_relic", "camp_lantern", "iron_seed"
    ];

    private static readonly IReadOnlyList<string> EncounterLabels =
    [
        "border_toll", "lost_signal", "market_argument", "broken_bridge",
        "night_watch", "smuggled_relic", "contested_cache", "silent_patrol"
    ];

    private static readonly IReadOnlyList<string> QuestEventLabels =
    [
        "secure_route", "recover_cache", "broker_truce", "restore_signal", "map_safe_passage", "settle_debt"
    ];

    private static readonly IReadOnlyList<string> ThemeFallbacks =
    [
        "theme/exploration", "theme/survival", "theme/trade", "theme/mystery"
    ];

    private static readonly IReadOnlyList<string> LocationMoodFallbacks =
    [
        "location_mood/safe", "location_mood/dangerous", "location_mood/isolated", "location_mood/ruined"
    ];

    private static readonly IReadOnlyList<string> QuestMotifFallbacks =
    [
        "quest_motif/escort", "quest_motif/recover_lost_resource", "quest_motif/faction_truce"
    ];

    private static readonly IReadOnlyList<string> ActorArchetypeFallbacks =
    [
        "npc_archetype/scout", "npc_archetype/trader", "npc_archetype/warden",
        "npc_archetype/forager", "npc_archetype/messenger", "npc_archetype/mechanic"
    ];

    private static readonly IReadOnlyList<string> ActorRoleFallbacks =
    [
        "entity_role/guide", "entity_role/rival", "entity_role/vendor", "entity_role/quest_giver"
    ];

    private static readonly IReadOnlyList<string> ItemAffordanceFallbacks =
    [
        "item_affordance/tradable", "item_affordance/quest_item", "item_affordance/consumable",
        "item_affordance/craft_material"
    ];
}
