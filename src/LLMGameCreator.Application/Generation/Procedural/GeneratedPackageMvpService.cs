using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;
using DomainFormulaDefinition = LLMGameCreator.Domain.Definitions.FormulaDefinition;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedPackageMvpService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/generated-package-mvp";
    public const string PackageJsonFileName = "package.json";
    public const string ReportJsonFileName = "generated-package-mvp-report.json";
    public const string ReportMarkdownFileName = "generated-package-mvp-report.md";
    public const string RuntimeBootstrapReportJsonFileName = "runtime-bootstrap-report.json";
    public const string RuntimeBootstrapReportMarkdownFileName = "runtime-bootstrap-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IGamePackageValidator _validator;
    private readonly GeneratedPackageMvpMarkdownRenderer _markdownRenderer;

    public GeneratedPackageMvpService(
        IGamePackageValidator? validator = null,
        GeneratedPackageMvpMarkdownRenderer? markdownRenderer = null)
    {
        _validator = validator ?? new GamePackageValidator();
        _markdownRenderer = markdownRenderer ?? new GeneratedPackageMvpMarkdownRenderer();
    }

    public GeneratedPackageMvpResult Generate(GeneratedPackageMvpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var diagnostics = new List<GeneratedPackageMvpDiagnostic>();
        AddInputDiagnostics(request, diagnostics);

        var source = BuildSourceMetadata(request);
        var mappedRecords = new List<GeneratedPackageMappedRecord>();
        var package = BuildPackage(request, source, mappedRecords, diagnostics);
        var packageJson = JsonSerializer.Serialize(package, JsonOptions);
        var packageHash = ComputeHash(packageJson);

        package.GeneratedContent.AppliedArtifacts.Add(new GeneratedContentArtifactProvenance
        {
            ArtifactId = "generated_package_mvp/" + ShortHash(packageHash),
            ContractId = "generated_package_mvp_v1",
            ArtifactKind = "generated_package_mvp",
            CapabilitySelectionId = "generated_package_mvp",
            GeneratedAt = string.Empty,
            AuditId = source.PlanId,
            AppliedAt = string.Empty,
            ContentHash = packageHash,
            MappingResult = "deterministic_mvp_mapping"
        });
        package.GeneratedContent.AppliedArtifacts = package.GeneratedContent.AppliedArtifacts
            .OrderBy(item => item.ArtifactId, StringComparer.Ordinal)
            .ToList();

        packageJson = JsonSerializer.Serialize(package, JsonOptions);
        packageHash = ComputeHash(packageJson);

        var validationReport = _validator.Validate(package);
        var validationIssues = validationReport.Issues
            .OrderBy(item => item.Severity)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.TargetId, StringComparer.Ordinal)
            .Select(ToValidationIssue)
            .ToList();
        foreach (var issue in validationIssues.Where(issue => issue.Severity is nameof(ValidationSeverity.Error) or nameof(ValidationSeverity.Critical)))
        {
            diagnostics.Add(Diagnostic("error", "generated_package_mvp.package_validation_failed", issue.TargetId, issue.Code + ": " + issue.Message));
        }

        var runtimeBootstrap = BuildRuntimeBootstrapReport(package, validationReport, diagnostics);
        diagnostics.Add(Diagnostic("info", "generated_package_mvp.no_external_execution", "generation", "No LLM, provider, Lua, Unity, media or external runtime execution was invoked."));

        var sortedDiagnostics = SortDiagnostics(diagnostics);
        runtimeBootstrap = runtimeBootstrap with
        {
            Diagnostics = SortDiagnostics(runtimeBootstrap.Diagnostics.Concat(sortedDiagnostics.Where(item => item.Code.StartsWith("runtime_bootstrap.", StringComparison.Ordinal))))
        };

        var report = new GeneratedPackageMvpReport
        {
            Source = source,
            PackageId = package.Manifest.PackageId,
            PackageTitle = package.Manifest.Title,
            PackageHash = packageHash,
            StableSummary = BuildStableSummary(package),
            HasErrors = validationIssues.Any(item => item.Severity is nameof(ValidationSeverity.Error) or nameof(ValidationSeverity.Critical))
                        || sortedDiagnostics.Any(item => item.Severity == "error"),
            DiagnosticCount = sortedDiagnostics.Count,
            MappedRecords = mappedRecords
                .OrderBy(item => item.SourceKind, StringComparer.Ordinal)
                .ThenBy(item => item.SourceId, StringComparer.Ordinal)
                .ThenBy(item => item.PackageKind, StringComparer.Ordinal)
                .ThenBy(item => item.PackageId, StringComparer.Ordinal)
                .ToList(),
            ValidationIssues = validationIssues,
            RuntimeBootstrap = runtimeBootstrap,
            Diagnostics = sortedDiagnostics
        };

        var reportJson = JsonSerializer.Serialize(report, JsonOptions);
        var reportMarkdown = _markdownRenderer.RenderReport(report);
        var runtimeBootstrapReportJson = JsonSerializer.Serialize(runtimeBootstrap, JsonOptions);
        var runtimeBootstrapReportMarkdown = _markdownRenderer.RenderRuntimeBootstrapReport(runtimeBootstrap);

        return new GeneratedPackageMvpResult
        {
            Package = package,
            Report = report,
            RuntimeBootstrapReport = runtimeBootstrap,
            PackageJson = packageJson,
            ReportJson = reportJson,
            ReportMarkdown = reportMarkdown,
            RuntimeBootstrapReportJson = runtimeBootstrapReportJson,
            RuntimeBootstrapReportMarkdown = runtimeBootstrapReportMarkdown,
            Diagnostics = sortedDiagnostics
        };
    }

    public async Task<GeneratedPackageMvpWriteResult> WriteAsync(
        string projectRootPath,
        GeneratedPackageMvpResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "generated-package-mvp"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var packageJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, PackageJsonFileName));
        var reportJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportJsonFileName));
        var reportMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportMarkdownFileName));
        var runtimeBootstrapReportJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, RuntimeBootstrapReportJsonFileName));
        var runtimeBootstrapReportMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, RuntimeBootstrapReportMarkdownFileName));
        EnsureContained(outputDirectory, packageJsonPath);
        EnsureContained(outputDirectory, reportJsonPath);
        EnsureContained(outputDirectory, reportMarkdownPath);
        EnsureContained(outputDirectory, runtimeBootstrapReportJsonPath);
        EnsureContained(outputDirectory, runtimeBootstrapReportMarkdownPath);

        await File.WriteAllTextAsync(packageJsonPath, result.PackageJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(runtimeBootstrapReportJsonPath, result.RuntimeBootstrapReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(runtimeBootstrapReportMarkdownPath, result.RuntimeBootstrapReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new GeneratedPackageMvpWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            PackageJsonPath = packageJsonPath,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            RuntimeBootstrapReportJsonPath = runtimeBootstrapReportJsonPath,
            RuntimeBootstrapReportMarkdownPath = runtimeBootstrapReportMarkdownPath
        };
    }

    private static GamePackageDefinition BuildPackage(
        GeneratedPackageMvpRequest request,
        GeneratedPackageMvpSourceMetadata source,
        ICollection<GeneratedPackageMappedRecord> mappedRecords,
        ICollection<GeneratedPackageMvpDiagnostic> diagnostics)
    {
        var plan = request.SourcePlan;
        var seedSegment = IdSegment(string.IsNullOrWhiteSpace(source.Seed) ? "missing_seed" : source.Seed);
        var modeSegment = IdSegment(string.IsNullOrWhiteSpace(source.Mode) ? "unknown_mode" : source.Mode);
        var identityHash = ShortHash(ComputeHash(seedSegment + "\n" + modeSegment + "\n" + source.PlanHash + "\n" + source.RulePackHash + "\n" + source.TinyLoopStateHash));
        var packageId = "game/generated_mvp_" + modeSegment + "_" + identityHash;
        var startRegion = plan?.World.Regions.OrderBy(item => item.RegionId, StringComparer.Ordinal).FirstOrDefault();
        var startMapId = startRegion is null ? "map/generated_mvp_start" : PackageMapId(startRegion.RegionId);

        var package = new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = packageId,
                Title = "Generated MVP " + modeSegment + " " + identityHash,
                Version = "0.1.0",
                FormatVersion = "0.1",
                StartMapId = startMapId,
                Description = "Deterministic generated package MVP assembled from procedural sidecars."
            },
            Game = new GameDefinition
            {
                TilePrototypes =
                [
                    new TilePrototypeDefinition { Id = "tile/generated_floor", Name = "Generated Floor", Walkable = true, MovementCost = 1 },
                    new TilePrototypeDefinition { Id = "tile/generated_blocker", Name = "Generated Blocker", Walkable = false, MovementCost = 1 }
                ],
                EntityPrototypes =
                [
                    new EntityPrototypeDefinition
                    {
                        Id = "entity_prototype/generated_actor",
                        Name = "Generated Actor",
                        Components = [new ComponentDefinition { Type = "interactable", Args = SortedArgs(("text", "Generated actor interaction.")) }]
                    },
                    new EntityPrototypeDefinition
                    {
                        Id = "entity_prototype/generated_cache",
                        Name = "Generated Cache",
                        Components = [new ComponentDefinition { Type = "interactable", Args = SortedArgs(("text", "Generated cache interaction.")) }]
                    }
                ]
            }
        };

        if (plan is null)
        {
            package.Game.Maps.Add(FallbackMap(startMapId));
            diagnostics.Add(Diagnostic("error", "generated_package_mvp.source_plan_missing", "sourcePlan", "Generated plan was not supplied; fallback package contains only a minimal start map."));
            return package;
        }

        package.GeneratedContent.Profile = BuildGeneratedProfile(plan, source);
        package.Game.Maps = BuildMaps(plan, mappedRecords, diagnostics);
        package.Manifest.StartMapId = package.Game.Maps.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault()?.Id ?? startMapId;
        if (package.Game.Maps.Count == 0)
        {
            package.Game.Maps.Add(FallbackMap(startMapId));
            diagnostics.Add(Diagnostic("error", "generated_package_mvp.no_regions", "sourcePlan.world.regions", "Generated plan had no regions; fallback map was emitted."));
        }

        package.Game.Factions = BuildFactions(plan, mappedRecords);
        package.Game.Items = BuildItems(plan, request.TinyLoopResult, mappedRecords, diagnostics);
        package.Game.Resources = BuildResources(plan, mappedRecords);
        package.Game.Abilities = BuildAbilities(request.RulePack, mappedRecords);
        package.Game.Encounters = BuildEncounters(plan, request.RulePack, mappedRecords, diagnostics);
        package.Game.Quests = BuildQuests(plan, mappedRecords, diagnostics);
        package.Game.Dialogues = BuildDialogues(plan, mappedRecords, diagnostics);
        package.Game.Interactions = BuildInteractions(plan, mappedRecords);
        package.Game.Formulas = BuildDomainFormulas(request.RulePack, mappedRecords);
        package.GeneratedContent = BuildGeneratedContent(plan, package, request, source, mappedRecords);

        AddMapEntities(package, plan, mappedRecords);
        AddUnmappedDiagnostics(plan, request.RulePack, diagnostics);
        return SortPackage(package);
    }

    private static GeneratedPackageMvpSourceMetadata BuildSourceMetadata(GeneratedPackageMvpRequest request) => new()
    {
        PlanId = request.SourcePlan?.PlanId ?? string.Empty,
        PlanHash = request.SourcePlan?.Metadata.DeterministicHash ?? string.Empty,
        RulePackId = request.RulePack?.Metadata.RulePackId ?? string.Empty,
        RulePackHash = request.RulePack?.Metadata.DeterministicHash ?? string.Empty,
        TinyLoopStateHash = request.TinyLoopResult?.State.DeterministicHash ?? string.Empty,
        Seed = request.SourcePlan?.Metadata.Seed ?? string.Empty,
        Mode = request.SourcePlan?.Metadata.Mode ?? string.Empty
    };

    private static GeneratedGameProfileDefinition BuildGeneratedProfile(
        ProceduralGeneratedGamePlan plan,
        GeneratedPackageMvpSourceMetadata source) => new()
    {
        Title = "Generated Package MVP",
        Description = "Minimal generated package mapped from procedural plan, rule pack and tiny runtime loop.",
        Genre = "procedural_prototype",
        Tone = string.Join(",", plan.Profile.StyleHintIds.Where(id => id.StartsWith("tone/", StringComparison.Ordinal)).OrderBy(id => id, StringComparer.Ordinal)),
        PresentationMode = "presentation_mode/top_down_2d",
        WorldTopology = plan.World.TopologyVariantId,
        ActorModel = plan.Profile.VariantIds.FirstOrDefault(id => id.StartsWith("actor_model/", StringComparison.Ordinal)) ?? "actor_model/single_player_character",
        CombatModel = plan.Profile.VariantIds.FirstOrDefault(id => id.StartsWith("combat_model/", StringComparison.Ordinal)) ?? "combat_model/turn_based",
        CoreLoop = ["explore_generated_region", "inspect_generated_actor", "resolve_generated_encounter", "advance_generated_quest"],
        Pillars = ["deterministic_seeded_content", "validator_clean_package", "runtime_bootstrap_evidence"],
        SourceContextJson = JsonSerializer.Serialize(source, JsonOptions)
    };

    private static List<MapDefinition> BuildMaps(
        ProceduralGeneratedGamePlan plan,
        ICollection<GeneratedPackageMappedRecord> mappedRecords,
        ICollection<GeneratedPackageMvpDiagnostic> diagnostics)
    {
        var maps = new List<MapDefinition>();
        foreach (var region in plan.World.Regions.OrderBy(item => item.RegionId, StringComparer.Ordinal))
        {
            var map = new MapDefinition
            {
                Id = PackageMapId(region.RegionId),
                Name = region.Label,
                Width = 5,
                Height = 5,
                DefaultTileId = "tile/generated_floor",
                StartPosition = new Position2D(1, 1),
                Tiles =
                [
                    new TileOverrideDefinition { X = 4, Y = 4, TileId = "tile/generated_blocker" }
                ]
            };
            maps.Add(map);
            mappedRecords.Add(Mapped("region", region.RegionId, "map", map.Id, "Region mapped to a minimal finite map."));
            mappedRecords.Add(Mapped("region", region.RegionId, "generated_region", region.RegionId, "Region preserved in GeneratedContent."));
        }

        if (maps.Count < 2)
        {
            diagnostics.Add(Diagnostic("warning", "generated_package_mvp.region_count_below_target", "sourcePlan.world.regions", "Fewer than two generated regions were available for map/location output."));
        }

        return maps;
    }

    private static List<FactionDefinition> BuildFactions(ProceduralGeneratedGamePlan plan, ICollection<GeneratedPackageMappedRecord> mappedRecords) =>
        plan.Factions
            .OrderBy(item => item.FactionId, StringComparer.Ordinal)
            .Select(faction =>
            {
                mappedRecords.Add(Mapped("faction", faction.FactionId, "faction", faction.FactionId, "Faction seed mapped to narrative faction definition."));
                return new FactionDefinition
                {
                    Id = faction.FactionId,
                    Name = faction.Label,
                    Description = "Generated faction motive: " + faction.MotiveHintId,
                    Kind = "generated_faction",
                    DefaultReputation = 0,
                    MinReputation = -100,
                    MaxReputation = 100,
                    Tags = [faction.MotiveHintId],
                    Metadata = SortedArgs(("sourceRegionId", faction.HomeRegionId))
                };
            })
            .ToList();

    private static List<ItemDefinition> BuildItems(
        ProceduralGeneratedGamePlan plan,
        TinyGeneratedRuntimeLoopResult? tinyLoop,
        ICollection<GeneratedPackageMappedRecord> mappedRecords,
        ICollection<GeneratedPackageMvpDiagnostic> diagnostics)
    {
        var grantedItems = new HashSet<string>(tinyLoop?.State.InventoryItemCounts.Keys ?? Array.Empty<string>(), StringComparer.Ordinal);
        var items = plan.ItemResourceSeeds
            .OrderBy(item => item.ItemSeedId, StringComparer.Ordinal)
            .Select(seed =>
            {
                var itemId = PackageItemId(seed.ItemSeedId);
                mappedRecords.Add(Mapped("item_seed", seed.ItemSeedId, "item", itemId, "Item/resource seed mapped to package item definition."));
                return new ItemDefinition
                {
                    Id = itemId,
                    Name = TitleFromId(seed.ItemSeedId),
                    Description = "Generated item affordance: " + seed.AffordanceHintId,
                    Kind = "generated_resource",
                    MaxStack = 20,
                    QuestItem = grantedItems.Contains(seed.ItemSeedId),
                    Tags = [seed.AffordanceHintId],
                    Metadata = SortedArgs(("sourceItemSeedId", seed.ItemSeedId), ("sourceResourceKindId", seed.ResourceKindId), ("sourceRegionId", seed.RegionId))
                };
            })
            .ToList();

        if (items.Count == 0)
        {
            diagnostics.Add(Diagnostic("warning", "generated_package_mvp.no_items", "sourcePlan.itemResourceSeeds", "No item/resource seeds were available for package item output."));
        }

        return items;
    }

    private static List<ResourceDefinition> BuildResources(
        ProceduralGeneratedGamePlan plan,
        ICollection<GeneratedPackageMappedRecord> mappedRecords) =>
        new[]
            {
                new ResourceDefinition
                {
                    Id = "resource/health",
                    Name = "Health",
                    Kind = "vital",
                    MinValue = 0,
                    MaxValue = 100,
                    DefaultValue = 30,
                    Tags = ["runtime_bootstrap"]
                }
            }
            .Concat(plan.ItemResourceSeeds
            .OrderBy(item => item.ResourceKindId, StringComparer.Ordinal)
            .GroupBy(item => item.ResourceKindId, StringComparer.Ordinal)
            .Select(group =>
            {
                var resourceId = group.Key;
                mappedRecords.Add(Mapped("resource_kind", resourceId, "resource", resourceId, "Resource kind mapped to package resource definition."));
                return new ResourceDefinition
                {
                    Id = resourceId,
                    Name = TitleFromId(resourceId),
                    Kind = "generated_resource",
                    MinValue = 0,
                    MaxValue = 100,
                    DefaultValue = 0,
                    Tags = ["generated"],
                    Metadata = SortedArgs(("sourceItemSeedIds", string.Join(",", group.Select(item => item.ItemSeedId).OrderBy(id => id, StringComparer.Ordinal))))
                };
            }))
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .ToList();

    private static List<AbilityDefinition> BuildAbilities(
        FormulaEffectActionRulePack? rulePack,
        ICollection<GeneratedPackageMappedRecord> mappedRecords)
    {
        if (rulePack is null)
        {
            return [];
        }

        return rulePack.Actions
            .OrderBy(item => item.ActionId, StringComparer.Ordinal)
            .Select(action =>
            {
                var abilityId = "ability/" + IdSegment(action.ActionId);
                mappedRecords.Add(Mapped("action", action.ActionId, "ability", abilityId, "Rule-pack action mapped to a data-only package ability."));
                return new AbilityDefinition
                {
                    Id = abilityId,
                    Name = TitleFromId(action.ActionId),
                    Kind = "generated_action",
                    Power = Math.Max(1, action.EffectIds.Count),
                    Range = 1,
                    Tags = [action.ActionType],
                    Metadata = SortedArgs(("sourceActionId", action.ActionId), ("effectIds", string.Join(",", action.EffectIds.OrderBy(id => id, StringComparer.Ordinal))))
                };
            })
            .ToList();
    }

    private static List<EncounterDefinition> BuildEncounters(
        ProceduralGeneratedGamePlan plan,
        FormulaEffectActionRulePack? rulePack,
        ICollection<GeneratedPackageMappedRecord> mappedRecords,
        ICollection<GeneratedPackageMvpDiagnostic> diagnostics)
    {
        var abilities = rulePack?.Actions.Select(action => "ability/" + IdSegment(action.ActionId)).OrderBy(id => id, StringComparer.Ordinal).ToList() ?? [];
        var encounters = plan.EncounterSeeds
            .OrderBy(item => item.EncounterSeedId, StringComparer.Ordinal)
            .Select(seed =>
            {
                var encounterId = PackageEncounterId(seed.EncounterSeedId);
                mappedRecords.Add(Mapped("encounter_seed", seed.EncounterSeedId, "encounter", encounterId, "Encounter seed mapped to package encounter definition."));
                return new EncounterDefinition
                {
                    Id = encounterId,
                    Name = TitleFromId(seed.EncounterSeedId),
                    Kind = "generated_encounter",
                    Participants = BuildEncounterParticipants(plan, seed, abilities),
                    Rewards = seed.RewardItemSeedIds.OrderBy(id => id, StringComparer.Ordinal)
                        .Select(id => new OutputDefinition { Kind = "item", Id = PackageItemId(id), Amount = 1 })
                        .ToList(),
                    DefaultSeed = StableInt(seed.EncounterSeedId),
                    Tags = ["generated"],
                    Metadata = SortedArgs(("sourceEncounterSeedId", seed.EncounterSeedId), ("sourceRegionId", seed.RegionId))
                };
            })
            .ToList();

        if (encounters.Count == 0)
        {
            diagnostics.Add(Diagnostic("warning", "generated_package_mvp.no_encounters", "sourcePlan.encounterSeeds", "No encounter seeds were available for package encounter output."));
        }

        return encounters;
    }

    private static List<EncounterParticipantDefinition> BuildEncounterParticipants(
        ProceduralGeneratedGamePlan plan,
        ProceduralEncounterSeed encounter,
        IReadOnlyList<string> abilities)
    {
        var participants = new List<EncounterParticipantDefinition>
        {
            new()
            {
                Id = "participant/player",
                Name = "Player",
                Kind = "player",
                Team = "player",
                Abilities = abilities.Take(1).ToList(),
                Resources = [new OutputDefinition { Kind = "resource", Id = "resource/health", Amount = 30 }]
            }
        };

        foreach (var actorId in encounter.ActorSeedIds.OrderBy(id => id, StringComparer.Ordinal).Take(2))
        {
            var actor = plan.ActorSeeds.FirstOrDefault(item => item.ActorSeedId == actorId);
            participants.Add(new EncounterParticipantDefinition
            {
                Id = "participant/" + IdSegment(actorId),
                Name = TitleFromId(actorId),
                Kind = "generated_actor",
                Team = "generated",
                EntityPrototypeId = "entity_prototype/generated_actor",
                FactionId = actor?.FactionId,
                Abilities = abilities.Take(1).ToList(),
                Resources = [new OutputDefinition { Kind = "resource", Id = "resource/health", Amount = 12 }]
            });
        }

        return participants.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
    }

    private static List<QuestDefinition> BuildQuests(
        ProceduralGeneratedGamePlan plan,
        ICollection<GeneratedPackageMappedRecord> mappedRecords,
        ICollection<GeneratedPackageMvpDiagnostic> diagnostics)
    {
        var quests = plan.QuestEventSeeds
            .OrderBy(item => item.QuestEventSeedId, StringComparer.Ordinal)
            .Select(seed =>
            {
                var questId = PackageQuestId(seed.QuestEventSeedId);
                var encounterId = PackageEncounterId(seed.TargetEncounterSeedId);
                var itemId = PackageItemId(seed.RequiredItemSeedId);
                mappedRecords.Add(Mapped("quest_event_seed", seed.QuestEventSeedId, "quest", questId, "Quest/event seed mapped to package quest definition."));
                return new QuestDefinition
                {
                    Id = questId,
                    Title = TitleFromId(seed.QuestEventSeedId),
                    Description = "Generated quest/event mapped from procedural seed.",
                    Kind = "generated_quest",
                    AutoStart = true,
                    Objectives =
                    [
                        new QuestObjectiveDefinition
                        {
                            Id = "objective/" + IdSegment(seed.QuestEventSeedId) + "_encounter",
                            Kind = "complete_encounter",
                            TargetId = encounterId,
                            RequiredAmount = 1
                        },
                        new QuestObjectiveDefinition
                        {
                            Id = "objective/" + IdSegment(seed.QuestEventSeedId) + "_item",
                            Kind = "has_item",
                            TargetId = itemId,
                            RequiredAmount = 1
                        }
                    ],
                    Rewards = [new OutputDefinition { Kind = "reputation", Id = seed.SourceFactionId, Amount = 1 }],
                    Stages =
                    [
                        new QuestStageDefinition
                        {
                            Id = "stage/" + IdSegment(seed.QuestEventSeedId) + "_start",
                            Text = "Resolve the generated encounter and secure the generated item."
                        }
                    ],
                    Tags = ["generated"],
                    Metadata = SortedArgs(("sourceQuestEventSeedId", seed.QuestEventSeedId), ("sourceRegionId", seed.RegionId))
                };
            })
            .ToList();

        if (quests.Count == 0)
        {
            diagnostics.Add(Diagnostic("warning", "generated_package_mvp.no_quests", "sourcePlan.questEventSeeds", "No quest/event seeds were available for package quest output."));
        }

        return quests;
    }

    private static List<DialogueDefinition> BuildDialogues(
        ProceduralGeneratedGamePlan plan,
        ICollection<GeneratedPackageMappedRecord> mappedRecords,
        ICollection<GeneratedPackageMvpDiagnostic> diagnostics)
    {
        var dialogues = plan.ActorSeeds
            .OrderBy(item => item.ActorSeedId, StringComparer.Ordinal)
            .Take(2)
            .Select(actor =>
            {
                var dialogueId = PackageDialogueId(actor.ActorSeedId);
                mappedRecords.Add(Mapped("actor_seed", actor.ActorSeedId, "dialogue", dialogueId, "Actor seed mapped to a minimal dialogue definition."));
                return new DialogueDefinition
                {
                    Id = dialogueId,
                    Title = TitleFromId(actor.ActorSeedId),
                    StartNodeId = "start",
                    Nodes =
                    [
                        new DialogueNodeDefinition
                        {
                            Id = "start",
                            SpeakerId = PackageActorEntityId(actor.ActorSeedId),
                            Text = "This generated actor marks the package MVP path.",
                            Choices = [new DialogueChoiceDefinition { Id = "close", Text = "Continue.", CloseDialogue = true }]
                        }
                    ],
                    Tags = ["generated"],
                    Metadata = SortedArgs(("sourceActorSeedId", actor.ActorSeedId), ("sourceRegionId", actor.RegionId))
                };
            })
            .ToList();

        if (dialogues.Count == 0)
        {
            diagnostics.Add(Diagnostic("warning", "generated_package_mvp.no_dialogues", "sourcePlan.actorSeeds", "No actor seeds were available for dialogue output."));
        }

        return dialogues;
    }

    private static List<InteractionDefinition> BuildInteractions(
        ProceduralGeneratedGamePlan plan,
        ICollection<GeneratedPackageMappedRecord> mappedRecords)
    {
        var interactions = new List<InteractionDefinition>();
        foreach (var actor in plan.ActorSeeds.OrderBy(item => item.ActorSeedId, StringComparer.Ordinal).Take(2))
        {
            var interactionId = PackageInteractionId(actor.ActorSeedId);
            mappedRecords.Add(Mapped("actor_seed", actor.ActorSeedId, "interaction", interactionId, "Actor seed mapped to inspect interaction."));
            interactions.Add(new InteractionDefinition
            {
                Id = interactionId,
                Kind = "inspect",
                Metadata = SortedArgs(("dialogue_id", PackageDialogueId(actor.ActorSeedId)), ("sourceActorSeedId", actor.ActorSeedId))
            });
        }

        return interactions;
    }

    private static List<DomainFormulaDefinition> BuildDomainFormulas(
        FormulaEffectActionRulePack? rulePack,
        ICollection<GeneratedPackageMappedRecord> mappedRecords)
    {
        if (rulePack is null)
        {
            return [];
        }

        return rulePack.Formulas
            .OrderBy(item => item.FormulaId, StringComparer.Ordinal)
            .Select(formula =>
            {
                mappedRecords.Add(Mapped("formula", formula.FormulaId, "formula", formula.FormulaId, "Rule-pack formula preserved as package formula definition."));
                return new DomainFormulaDefinition
                {
                    Id = formula.FormulaId,
                    Expression = formula.Expression,
                    ResultType = formula.ResultType,
                    Description = "Generated MVP formula from rule pack."
                };
            })
            .ToList();
    }

    private static GeneratedContentDefinition BuildGeneratedContent(
        ProceduralGeneratedGamePlan plan,
        GamePackageDefinition package,
        GeneratedPackageMvpRequest request,
        GeneratedPackageMvpSourceMetadata source,
        ICollection<GeneratedPackageMappedRecord> mappedRecords) => new()
    {
        Profile = package.GeneratedContent.Profile,
        Regions = plan.World.Regions.OrderBy(item => item.RegionId, StringComparer.Ordinal).Select(region => new GeneratedRegionDefinition
        {
            SourceId = region.RegionId,
            Title = region.Label,
            Description = "Generated region mood: " + region.MoodHintId,
            SceneIds = [PackageMapId(region.RegionId)]
        }).ToList(),
        Scenes = package.Game.Maps.OrderBy(item => item.Id, StringComparer.Ordinal).Select(map => new GeneratedSceneDefinition
        {
            SourceId = map.Id,
            PackageMapId = map.Id,
            Title = map.Name,
            Description = "Minimal generated package MVP scene.",
            Purpose = "runtime_bootstrap"
        }).ToList(),
        Npcs = plan.ActorSeeds.OrderBy(item => item.ActorSeedId, StringComparer.Ordinal).Select(actor => new GeneratedNpcDefinition
        {
            SourceId = actor.ActorSeedId,
            Name = TitleFromId(actor.ActorSeedId),
            Description = actor.RoleHintId,
            RegionId = actor.RegionId,
            SceneId = PackageMapId(actor.RegionId)
        }).ToList(),
        Items = plan.ItemResourceSeeds.OrderBy(item => item.ItemSeedId, StringComparer.Ordinal).Select(item => new GeneratedItemDefinition
        {
            SourceId = item.ItemSeedId,
            Name = TitleFromId(item.ItemSeedId),
            Description = item.AffordanceHintId
        }).ToList(),
        Encounters = plan.EncounterSeeds.OrderBy(item => item.EncounterSeedId, StringComparer.Ordinal).Select(encounter => new GeneratedEncounterDefinition
        {
            SourceId = encounter.EncounterSeedId,
            Title = TitleFromId(encounter.EncounterSeedId),
            Description = "Generated encounter mapped to package encounter.",
            RegionId = encounter.RegionId,
            SceneId = PackageMapId(encounter.RegionId),
            NpcIds = encounter.ActorSeedIds.OrderBy(id => id, StringComparer.Ordinal).Select(PackageActorEntityId).ToList()
        }).ToList(),
        Quests = plan.QuestEventSeeds.OrderBy(item => item.QuestEventSeedId, StringComparer.Ordinal).Select(quest => new GeneratedQuestSeedDefinition
        {
            SourceId = quest.QuestEventSeedId,
            PackageQuestId = PackageQuestId(quest.QuestEventSeedId),
            Title = TitleFromId(quest.QuestEventSeedId),
            Description = "Generated quest/event mapped to package quest.",
            Steps = ["enter_region", "resolve_encounter", "collect_item"],
            Objectives = [PackageEncounterId(quest.TargetEncounterSeedId), PackageItemId(quest.RequiredItemSeedId)]
        }).ToList(),
        Mechanics = request.RulePack?.Actions.OrderBy(item => item.ActionId, StringComparer.Ordinal).Select(action => new GeneratedMechanicDefinition
        {
            SourceId = action.ActionId,
            PackageAbilityId = "ability/" + IdSegment(action.ActionId),
            Name = TitleFromId(action.ActionId),
            Description = action.Purpose,
            Tags = [action.ActionType]
        }).ToList() ?? [],
        PreservedArtifacts =
        [
            new PreservedGeneratedArtifactDefinition
            {
                ArtifactId = source.PlanId,
                ContractId = "procedural_generated_game_plan_v1",
                ArtifactKind = "generated_game_plan",
                Reason = "source_provenance",
                RawJson = JsonSerializer.Serialize(new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["planHash"] = source.PlanHash,
                    ["rulePackHash"] = source.RulePackHash,
                    ["tinyLoopStateHash"] = source.TinyLoopStateHash,
                    ["mappedRecords"] = mappedRecords.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }, JsonOptions)
            }
        ]
    };

    private static void AddMapEntities(
        GamePackageDefinition package,
        ProceduralGeneratedGamePlan plan,
        ICollection<GeneratedPackageMappedRecord> mappedRecords)
    {
        var mapsByRegion = package.Game.Maps.ToDictionary(item => item.Id, StringComparer.Ordinal);
        foreach (var actor in plan.ActorSeeds.OrderBy(item => item.ActorSeedId, StringComparer.Ordinal))
        {
            if (!mapsByRegion.TryGetValue(PackageMapId(actor.RegionId), out var map))
            {
                continue;
            }

            var entityId = PackageActorEntityId(actor.ActorSeedId);
            map.Entities.Add(new EntityInstanceDefinition
            {
                Id = entityId,
                PrototypeId = "entity_prototype/generated_actor",
                Position = new Position2D(2, 1),
                Components =
                [
                    new ComponentDefinition
                    {
                        Type = "interactable",
                        Args = SortedArgs(("dialogueId", PackageDialogueId(actor.ActorSeedId)), ("interactionId", PackageInteractionId(actor.ActorSeedId)))
                    }
                ]
            });
            mappedRecords.Add(Mapped("actor_seed", actor.ActorSeedId, "entity", entityId, "Actor seed mapped to interactable map entity."));
        }

        foreach (var item in plan.ItemResourceSeeds.OrderBy(item => item.ItemSeedId, StringComparer.Ordinal))
        {
            if (!mapsByRegion.TryGetValue(PackageMapId(item.RegionId), out var map))
            {
                continue;
            }

            var entityId = "entity/" + IdSegment(item.ItemSeedId);
            map.Entities.Add(new EntityInstanceDefinition
            {
                Id = entityId,
                PrototypeId = "entity_prototype/generated_cache",
                Position = new Position2D(3, 1),
                Components = [new ComponentDefinition { Type = "interactable", Args = SortedArgs(("text", "Generated item cache: " + PackageItemId(item.ItemSeedId))) }]
            });
            mappedRecords.Add(Mapped("item_seed", item.ItemSeedId, "entity", entityId, "Item seed mapped to inspectable cache entity."));
        }

        foreach (var map in package.Game.Maps)
        {
            map.Entities = map.Entities
                .GroupBy(item => item.Id, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(item => item.Id, StringComparer.Ordinal)
                .ToList();
        }
    }

    private static GeneratedPackageRuntimeBootstrapReport BuildRuntimeBootstrapReport(
        GamePackageDefinition package,
        ValidationReport validationReport,
        ICollection<GeneratedPackageMvpDiagnostic> outerDiagnostics)
    {
        var diagnostics = new List<GeneratedPackageMvpDiagnostic>
        {
            Diagnostic("info", "runtime_bootstrap.application_layer_adapter", package.Manifest.PackageId, "Application cannot instantiate Runtime project services without adding a new project dependency; bootstrap evidence uses existing package contracts.")
        };
        if (!validationReport.IsValid)
        {
            diagnostics.Add(Diagnostic("error", "runtime_bootstrap.validation_blocked", package.Manifest.PackageId, "Runtime bootstrap evidence is limited because package validation failed."));
        }

        var startMap = package.Game.Maps.FirstOrDefault(item => item.Id == package.Manifest.StartMapId)
                       ?? package.Game.Maps.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault();
        var initialStateCreated = validationReport.IsValid && startMap is not null && !string.IsNullOrWhiteSpace(package.Manifest.PackageId);
        var mapRuntimeStarted = startMap is not null;
        var moveCommandSucceeded = CanMoveRight(package, startMap);
        var interactCommandObserved = moveCommandSucceeded && startMap is not null && HasAdjacentInteractable(package, startMap, startMap.StartPosition.X + 1, startMap.StartPosition.Y);

        if (!mapRuntimeStarted)
        {
            diagnostics.Add(Diagnostic("error", "runtime_bootstrap.map_start_failed", package.Manifest.StartMapId, "Generated package has no start map."));
        }

        if (!moveCommandSucceeded)
        {
            diagnostics.Add(Diagnostic("warning", "runtime_bootstrap.move_failed", package.Manifest.StartMapId, "Bootstrap adapter could not prove one walkable move from the start position."));
        }

        if (!interactCommandObserved)
        {
            diagnostics.Add(Diagnostic("warning", "runtime_bootstrap.interact_not_observed", package.Manifest.StartMapId, "Bootstrap adapter did not find an adjacent interactable entity after the first move."));
        }

        foreach (var diagnostic in diagnostics)
        {
            outerDiagnostics.Add(diagnostic);
        }

        var eventTypes = new SortedSet<string>(StringComparer.Ordinal);
        if (mapRuntimeStarted)
        {
            eventTypes.Add("GameStarted");
        }

        if (moveCommandSucceeded)
        {
            eventTypes.Add("PlayerMoved");
        }

        if (interactCommandObserved)
        {
            eventTypes.Add("InteractionTriggered");
        }

        return new GeneratedPackageRuntimeBootstrapReport
        {
            ValidationPassed = validationReport.IsValid,
            InitialStateCreated = initialStateCreated,
            MapRuntimeStarted = mapRuntimeStarted,
            MoveCommandSucceeded = moveCommandSucceeded,
            InteractCommandObserved = interactCommandObserved,
            StartMapId = package.Manifest.StartMapId,
            CurrentMapId = startMap?.Id ?? string.Empty,
            PlayerEntityId = "player",
            RuntimeSummary = BuildRuntimeSummary(initialStateCreated, mapRuntimeStarted, moveCommandSucceeded, interactCommandObserved),
            EventTypes = eventTypes.ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static void AddInputDiagnostics(GeneratedPackageMvpRequest request, ICollection<GeneratedPackageMvpDiagnostic> diagnostics)
    {
        if (request.SourcePlan is null)
        {
            diagnostics.Add(Diagnostic("error", "generated_package_mvp.source_plan_missing", "sourcePlan", "Source generated plan was not supplied."));
        }

        if (request.RulePack is null)
        {
            diagnostics.Add(Diagnostic("warning", "generated_package_mvp.rule_pack_missing", "rulePack", "Rule pack was not supplied; mechanics/formulas will be limited."));
        }

        if (request.RulePackValidationReport is null)
        {
            diagnostics.Add(Diagnostic("warning", "generated_package_mvp.rule_pack_validation_missing", "rulePackValidationReport", "Rule pack validation report was not supplied."));
        }
        else if (request.RulePackValidationReport.HasErrors)
        {
            diagnostics.Add(Diagnostic("error", "generated_package_mvp.rule_pack_validation_failed", request.RulePackValidationReport.RulePackId, "Rule pack validation report contains errors."));
        }

        if (request.TinyLoopResult is null)
        {
            diagnostics.Add(Diagnostic("warning", "generated_package_mvp.tiny_loop_missing", "tinyLoopResult", "Tiny runtime loop result was not supplied; runtime provenance will be limited."));
        }
        else if (request.TinyLoopResult.Report.HasErrors)
        {
            diagnostics.Add(Diagnostic("warning", "generated_package_mvp.tiny_loop_has_errors", request.TinyLoopResult.Report.StateHash, "Tiny runtime loop report contains errors."));
        }
    }

    private static void AddUnmappedDiagnostics(
        ProceduralGeneratedGamePlan plan,
        FormulaEffectActionRulePack? rulePack,
        ICollection<GeneratedPackageMvpDiagnostic> diagnostics)
    {
        foreach (var connection in plan.World.Connections.OrderBy(item => item.ConnectionId, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("info", "generated_package_mvp.connection_report_only", connection.ConnectionId, "Region connection is report/provenance-only in this package MVP; map portals are deferred."));
        }

        foreach (var effect in (rulePack?.Effects ?? Array.Empty<EffectDefinition>())
                     .Where(item => item.EffectType is "effect/set_flag" or "effect/adjust_reputation" or "effect/advance_quest_event")
                     .OrderBy(item => item.EffectId, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("info", "generated_package_mvp.rule_effect_report_only", effect.EffectId, "Rule-pack effect remains report/provenance-only until richer runtime mapping is explicitly selected."));
        }
    }

    private static GamePackageDefinition SortPackage(GamePackageDefinition package)
    {
        package.Game.TilePrototypes = package.Game.TilePrototypes.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.EntityPrototypes = package.Game.EntityPrototypes.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Maps = package.Game.Maps.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Items = package.Game.Items.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Resources = package.Game.Resources.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Abilities = package.Game.Abilities.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Encounters = package.Game.Encounters.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Quests = package.Game.Quests.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Dialogues = package.Game.Dialogues.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Factions = package.Game.Factions.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Formulas = package.Game.Formulas.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Interactions = package.Game.Interactions.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        return package;
    }

    private static MapDefinition FallbackMap(string mapId) => new()
    {
        Id = mapId,
        Name = "Generated MVP Start",
        Width = 3,
        Height = 3,
        DefaultTileId = "tile/generated_floor",
        StartPosition = new Position2D(1, 1)
    };

    private static string BuildStableSummary(GamePackageDefinition package) =>
        string.Join("; ", new[]
        {
            $"maps={package.Game.Maps.Count}",
            $"items={package.Game.Items.Count}",
            $"resources={package.Game.Resources.Count}",
            $"factions={package.Game.Factions.Count}",
            $"encounters={package.Game.Encounters.Count}",
            $"quests={package.Game.Quests.Count}",
            $"dialogues={package.Game.Dialogues.Count}",
            $"generatedRecords={package.GeneratedContent.Regions.Count + package.GeneratedContent.Npcs.Count + package.GeneratedContent.Items.Count + package.GeneratedContent.Encounters.Count + package.GeneratedContent.Quests.Count}"
        });

    private static bool CanMoveRight(GamePackageDefinition package, MapDefinition? map)
    {
        if (map is null)
        {
            return false;
        }

        var nextX = map.StartPosition.X + 1;
        var nextY = map.StartPosition.Y;
        if (nextX < 0 || nextY < 0 || nextX >= map.Width || nextY >= map.Height)
        {
            return false;
        }

        var tileId = map.Tiles.FirstOrDefault(tile => tile.X == nextX && tile.Y == nextY)?.TileId ?? map.DefaultTileId;
        return package.Game.TilePrototypes.Any(tile => tile.Id == tileId && tile.Walkable);
    }

    private static bool HasAdjacentInteractable(GamePackageDefinition package, MapDefinition map, int playerX, int playerY) =>
        map.Entities.Any(entity => Math.Abs(entity.Position.X - playerX) + Math.Abs(entity.Position.Y - playerY) == 1 && HasInteractable(package, entity));

    private static bool HasInteractable(GamePackageDefinition package, EntityInstanceDefinition entity)
    {
        if (entity.Components.Any(component => component.Type == "interactable"))
        {
            return true;
        }

        return package.Game.EntityPrototypes
            .FirstOrDefault(prototype => prototype.Id == entity.PrototypeId)
            ?.Components.Any(component => component.Type == "interactable") == true;
    }

    private static string BuildRuntimeSummary(bool initialStateCreated, bool mapRuntimeStarted, bool moveCommandSucceeded, bool interactCommandObserved) =>
        string.Join("; ", new[]
        {
            $"initialStateCreated={initialStateCreated.ToString().ToLowerInvariant()}",
            $"mapRuntimeStarted={mapRuntimeStarted.ToString().ToLowerInvariant()}",
            $"moveCommandSucceeded={moveCommandSucceeded.ToString().ToLowerInvariant()}",
            $"interactCommandObserved={interactCommandObserved.ToString().ToLowerInvariant()}"
        });

    private static GeneratedPackageMvpValidationIssue ToValidationIssue(ValidationIssue issue) => new()
    {
        Severity = issue.Severity.ToString(),
        Code = issue.Code,
        Message = issue.Message,
        TargetId = issue.TargetId ?? string.Empty,
        Category = issue.Category ?? string.Empty
    };

    private static GeneratedPackageMappedRecord Mapped(string sourceKind, string sourceId, string packageKind, string packageId, string note) => new()
    {
        SourceKind = sourceKind,
        SourceId = sourceId,
        PackageKind = packageKind,
        PackageId = packageId,
        MappingNote = note
    };

    private static IReadOnlyList<GeneratedPackageMvpDiagnostic> SortDiagnostics(IEnumerable<GeneratedPackageMvpDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static GeneratedPackageMvpDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static string PackageMapId(string regionId) => "map/" + IdSegment(regionId);
    private static string PackageItemId(string itemSeedId) => "item/" + IdSegment(itemSeedId);
    private static string PackageEncounterId(string encounterSeedId) => "encounter/" + IdSegment(encounterSeedId);
    private static string PackageQuestId(string questEventSeedId) => "quest/" + IdSegment(questEventSeedId);
    private static string PackageDialogueId(string actorSeedId) => "dialogue/" + IdSegment(actorSeedId);
    private static string PackageInteractionId(string actorSeedId) => "interaction/" + IdSegment(actorSeedId);
    private static string PackageActorEntityId(string actorSeedId) => "entity/" + IdSegment(actorSeedId);

    private static string IdSegment(string id)
    {
        var normalized = id.Replace('/', '_').Trim('_').ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            builder.Append(character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-' ? character : '_');
        }

        var segment = builder.ToString();
        while (segment.Contains("__", StringComparison.Ordinal))
        {
            segment = segment.Replace("__", "_", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(segment) ? "generated" : segment;
    }

    private static string TitleFromId(string id)
    {
        var segment = IdSegment(id);
        var words = segment.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word is not "actor" and not "seed" and not "quest" and not "event" and not "encounter" and not "item")
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..]);
        var title = string.Join(" ", words);
        return string.IsNullOrWhiteSpace(title) ? "Generated" : title;
    }

    private static Dictionary<string, string> SortedArgs(params (string Key, string Value)[] values) =>
        values
            .Where(item => !string.IsNullOrWhiteSpace(item.Value))
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);

    private static int StableInt(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return BitConverter.ToInt32(hash, 0) & 0x7FFFFFFF;
    }

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ShortHash(string hash) => hash.Length <= 12 ? hash : hash[..12];

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Generated package MVP output path must stay under the project root.");
        }
    }
}
