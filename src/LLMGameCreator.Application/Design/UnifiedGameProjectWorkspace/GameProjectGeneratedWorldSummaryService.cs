using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public sealed class GameProjectGeneratedWorldSummaryService
{
    private readonly GeneratedProjectOverlayService _overlayService;
    private readonly GenerationPresetOptionsService _presetOptions;

    public GameProjectGeneratedWorldSummaryService(
        GeneratedProjectOverlayService? overlayService = null,
        GenerationPresetOptionsService? presetOptions = null)
    {
        _overlayService = overlayService ?? new GeneratedProjectOverlayService();
        _presetOptions = presetOptions ?? new GenerationPresetOptionsService();
    }

    public GameProjectGeneratedWorldSummary? ProjectSource(
        SeededGeneratedProjectSourceValidationResult validation)
    {
        ArgumentNullException.ThrowIfNull(validation);
        if (!validation.Present) return null;
        if (!validation.Passed || validation.Source is null) return new GameProjectGeneratedWorldSummary
        {
            Present = true,
            Passed = false,
            Status = "INVALID",
            Diagnostics = validation.Diagnostics
        };
        return Build(validation, "SOURCE_READY", packageContentPreserved: false, []);
    }

    public GameProjectGeneratedWorldSummary BuildCurrent(
        SeededGeneratedProjectSourceValidationResult validation,
        GamePackageDefinition compositionPackage,
        GamePackageDefinition activatedPackage,
        GameProjectGeneratedWorldActivationSummary? activation = null,
        GeneratedWorldTravelOverlayDocument? travelOverlay = null,
        GameProjectGeneratedRegionTravelSummary? travel = null,
        GameProjectGeneratedEncounterCombatSummary? combat = null,
        GameProjectGeneratedCampaignChoiceSummary? choices = null,
        GameProjectGeneratedCampaignRelationshipSummary? relationships = null,
        GameProjectGeneratedCampaignRegionalEventSummary? regionalEvents = null)
    {
        ArgumentNullException.ThrowIfNull(validation);
        ArgumentNullException.ThrowIfNull(compositionPackage);
        ArgumentNullException.ThrowIfNull(activatedPackage);
        if (!validation.Present || !validation.Passed || validation.Source is null || validation.Overlay is null)
            return ProjectSource(validation) ?? new GameProjectGeneratedWorldSummary();
        var travelReady = travelOverlay is
                          {
                              ControlledDeltaPassed: true,
                              GatePlacementPassed: true,
                              ConnectionCount: > 0,
                              GateCount: > 0
                          }
                          && travel is { Present: true, Passed: true };
        var diagnostics = _overlayService.ValidatePackageRecords(compositionPackage, validation.Overlay, includeBaseline: false)
            .Concat(_overlayService.ValidatePackageRecords(activatedPackage, validation.Overlay, includeBaseline: false))
            .Where(diagnostic => !travelReady || !AuthorizedTravelMapChange(diagnostic, travelOverlay!))
            .Where(diagnostic => combat is not { Present: true, Passed: true }
                                 || !AuthorizedCombatEncounterChange(diagnostic, combat))
            .Where(diagnostic => choices is not { Present: true, Passed: true }
                                 || !AuthorizedChoiceDialogueChange(diagnostic, choices))
            .Where(diagnostic => relationships is not { Present: true, Passed: true }
                                 || !AuthorizedRelationshipChange(diagnostic, relationships))
            .Where(diagnostic => regionalEvents is not
                                 { Present: true, Passed: true }
                                 || !AuthorizedRegionalEventChange(
                                     diagnostic, regionalEvents))
            .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        var combatReady = validation.Source.Counts.Encounters == 0
                          || combat is { Present: true, Passed: true, Status: "CAMPAIGN_CURRENT" };
        var choicesReady = choices is { Present: true, Passed: true, Status: "CHOICE_CURRENT" };
        var relationshipsReady = relationships is null
                                 || relationships is { Present: false, Passed: true, Status: "ABSENT" }
                                 || relationships is { Present: true, Passed: true, Status: "RELATIONSHIPS_CURRENT" };
        var regionalEventsReady = regionalEvents is
            { Present: false, Passed: true, Status: "ABSENT" }
            or
            {
                Present: true,
                Passed: true,
                Status: "REGIONAL_EVENTS_CURRENT"
            };
        var status = diagnostics.Count > 0
            ? "INVALID"
            : combatReady && choicesReady && relationshipsReady
              && regionalEventsReady && travelReady
              && activation is { Present: true, Passed: true }
                ? "CAMPAIGN_CURRENT"
            : travelReady && activation is { Present: true, Passed: true }
                ? "BUILD_CURRENT"
            : activation is { Present: true, Passed: true }
                ? "BUILD_CURRENT"
                : "SOURCE_READY";
        return Build(validation, status,
            packageContentPreserved: diagnostics.Count == 0, diagnostics);
    }

    public GameProjectGeneratedWorldSummary? Restore(
        SeededGeneratedProjectSourceValidationResult validation,
        GameProjectGeneratedWorldSummary? lastSuccessful,
        bool matchesCurrentAuthoring,
        GameProjectGeneratedWorldActivationSummary? activation = null,
        GameProjectGeneratedRegionTravelSummary? travel = null,
        GameProjectGeneratedEncounterCombatSummary? combat = null,
        GameProjectGeneratedCampaignChoiceSummary? choices = null,
        GameProjectGeneratedCampaignRelationshipSummary? relationships = null,
        GameProjectGeneratedCampaignRegionalEventSummary?
            regionalEvents = null)
    {
        var source = ProjectSource(validation);
        if (source is null || !source.Passed || lastSuccessful is not { Present: true, Passed: true }) return source;
        var sourceMatches = string.Equals(source.SourceRequestSha256, lastSuccessful.SourceRequestSha256, StringComparison.Ordinal)
                            && string.Equals(source.PlanSha256, lastSuccessful.PlanSha256, StringComparison.Ordinal)
                            && string.Equals(source.OverlaySha256, lastSuccessful.OverlaySha256, StringComparison.Ordinal)
                            && string.Equals(source.GeneratedBasePackageSha256, lastSuccessful.GeneratedBasePackageSha256, StringComparison.Ordinal);
        if (!sourceMatches) return source with
        {
            Passed = false,
            Status = "INVALID",
            Diagnostics = ["generated_summary.history_source_mismatch"]
        };
        if (activation is not { Present: true, Passed: true }) return source;
        var choicesCurrent = choices is
            { Present: true, Passed: true, Status: "CHOICE_CURRENT" };
        var combatCurrent = source.EncounterCount == 0
                            || combat is
                            {
                                Present: true,
                                Passed: true,
                                Status: "CAMPAIGN_CURRENT"
                            };
        var relationshipsPending = relationships is
            { Present: true, Passed: false, Status: "RELATIONSHIPS_PENDING" };
        var regionalEventsPending = regionalEvents is
            {
                Passed: false,
                Status: "REGIONAL_EVENTS_PENDING"
            };
        var status = choicesCurrent && relationshipsPending
                     && combatCurrent
                     && travel is { Present: true, Passed: true }
            ? matchesCurrentAuthoring
                ? "RELATIONSHIPS_PENDING"
                : "LAST_SUCCESS"
            : choicesCurrent && regionalEventsPending
              && (relationships is
                  {
                      Passed: true,
                      Status: "RELATIONSHIPS_CURRENT"
                  })
              && combatCurrent
              && travel is { Present: true, Passed: true }
                ? matchesCurrentAuthoring
                    ? "REGIONAL_EVENTS_PENDING"
                    : "LAST_SUCCESS"
            : choicesCurrent
                     && (relationships is null
                         || relationships is
                         {
                             Present: false,
                             Passed: true,
                             Status: "ABSENT"
                         }
                         || relationships is
                         {
                             Present: true,
                             Passed: true,
                             Status: "RELATIONSHIPS_CURRENT"
                         })
                     && regionalEvents is
                     {
                         Passed: true,
                         Status: "REGIONAL_EVENTS_CURRENT" or "ABSENT"
                     }
                     && (source.EncounterCount == 0
                         || combat is { Present: true, Passed: true, Status: "CAMPAIGN_CURRENT" })
            ? matchesCurrentAuthoring ? "CAMPAIGN_CURRENT" : "LAST_SUCCESS"
            : travel is { Present: true, Passed: true }
            ? matchesCurrentAuthoring ? "TRAVEL_CURRENT" : "LAST_SUCCESS"
            : matchesCurrentAuthoring
                ? string.Equals(lastSuccessful.Status, "START_CURRENT", StringComparison.Ordinal)
                    ? "START_CURRENT"
                    : "BUILD_CURRENT"
                : "LAST_SUCCESS";
        return lastSuccessful with
        {
            Status = status,
            HumanFacts = BuildHumanFacts(lastSuccessful),
            Diagnostics = validation.Diagnostics
        };
    }

    public IReadOnlyList<StandaloneHumanReviewFact> StandaloneHumanFacts(
        GameProjectGeneratedWorldSummary? summary) => summary is { Present: true, Passed: true }
        ? summary.HumanFacts.Select(fact => new StandaloneHumanReviewFact
        {
            Label = fact.Label,
            Value = fact.Value
        }).ToList()
        : [];

    public static IReadOnlyList<StandaloneHumanReviewFact> StandaloneActivationHumanFacts(
        GameProjectGeneratedWorldActivationSummary? summary) => summary is { Present: true, Passed: true }
        ? summary.HumanFacts.Select(fact => new StandaloneHumanReviewFact
        {
            Label = fact.Label,
            Value = fact.Value
        }).ToList()
        : [];

    public static IReadOnlyList<StandaloneHumanReviewFact> StandaloneTravelHumanFacts(
        GameProjectGeneratedRegionTravelSummary? summary) => summary is { Present: true, Passed: true }
        ? summary.HumanFacts.Select(fact => new StandaloneHumanReviewFact
        {
            Label = fact.Label,
            Value = fact.Value
        }).ToList()
        : [];

    public static IReadOnlyList<StandaloneHumanReviewFact> StandaloneCombatHumanFacts(
        GameProjectGeneratedEncounterCombatSummary? summary) => summary is { Present: true, Passed: true }
        ? summary.HumanReviewFacts.Select(fact => new StandaloneHumanReviewFact
        {
            Label = fact.Label,
            Value = fact.Value
        }).ToList()
        : [];

    public static IReadOnlyList<StandaloneHumanReviewFact> StandaloneChoiceHumanFacts(
        GameProjectGeneratedCampaignChoiceSummary? summary) => summary is { Present: true, Passed: true }
        ? summary.HumanReviewFacts.Select(fact => new StandaloneHumanReviewFact
        {
            Label = fact.Label,
            Value = fact.Value
        }).ToList()
        : [];

    public static IReadOnlyList<StandaloneHumanReviewFact> StandaloneRelationshipHumanFacts(
        GameProjectGeneratedCampaignRelationshipSummary? summary) =>
        summary is { Present: true, Passed: true }
            ? summary.HumanReviewFacts.Select(fact => new StandaloneHumanReviewFact
            {
                Label = fact.Label,
                Value = fact.Value
            }).ToList()
            : [];

    public static IReadOnlyList<StandaloneHumanReviewFact>
        StandaloneRegionalEventHumanFacts(
            GameProjectGeneratedCampaignRegionalEventSummary? summary) =>
        summary is { Passed: true }
            ? summary.HumanReviewFacts.Select(fact =>
                new StandaloneHumanReviewFact
                {
                    Label = fact.Label,
                    Value = fact.Value
                }).ToList()
            : [];

    private static bool AuthorizedTravelMapChange(
        string diagnostic,
        GeneratedWorldTravelOverlayDocument overlay)
    {
        const string prefix = "generated_overlay.record_changed:game.maps:";
        if (!diagnostic.StartsWith(prefix, StringComparison.Ordinal)) return false;
        var mapId = diagnostic[prefix.Length..];
        return overlay.MapFingerprintsAfter.Any(item =>
            string.Equals(item.RecordId, mapId, StringComparison.Ordinal));
    }

    private static bool AuthorizedCombatEncounterChange(
        string diagnostic,
        GameProjectGeneratedEncounterCombatSummary combat)
    {
        const string prefix = "generated_overlay.record_changed:game.encounters:";
        if (!diagnostic.StartsWith(prefix, StringComparison.Ordinal)) return false;
        var encounterId = diagnostic[prefix.Length..];
        return combat.Overlay?.EncounterFingerprintsAfter.Any(item =>
            string.Equals(item.EncounterId, encounterId, StringComparison.Ordinal)) == true;
    }

    private static bool AuthorizedChoiceDialogueChange(
        string diagnostic,
        GameProjectGeneratedCampaignChoiceSummary choices)
    {
        const string prefix = "generated_overlay.record_changed:game.dialogues:";
        if (!diagnostic.StartsWith(prefix, StringComparison.Ordinal)) return false;
        var dialogueId = diagnostic[prefix.Length..];
        return choices.Overlay?.DialogueFingerprintsAfter.Any(item =>
            string.Equals(item.DialogueId, dialogueId, StringComparison.Ordinal)) == true;
    }

    private static bool AuthorizedRelationshipChange(
        string diagnostic,
        GameProjectGeneratedCampaignRelationshipSummary relationships)
    {
        const string dialoguePrefix = "generated_overlay.record_changed:game.dialogues:";
        const string questPrefix = "generated_overlay.record_changed:game.quests:";
        if (diagnostic.StartsWith(dialoguePrefix, StringComparison.Ordinal))
        {
            var dialogueId = diagnostic[dialoguePrefix.Length..];
            return relationships.Overlay?.FingerprintsAfter.Any(item =>
                item.CollectionPath == "game.dialogues"
                && item.DefinitionId == dialogueId) == true;
        }
        if (diagnostic.StartsWith(questPrefix, StringComparison.Ordinal))
        {
            var questId = diagnostic[questPrefix.Length..];
            return relationships.Overlay?.FingerprintsAfter.Any(item =>
                item.CollectionPath == "game.quests"
                && item.DefinitionId == questId) == true;
        }
        return false;
    }

    private static bool AuthorizedRegionalEventChange(
        string diagnostic,
        GameProjectGeneratedCampaignRegionalEventSummary regionalEvents)
    {
        const string mapPrefix =
            "generated_overlay.record_changed:game.maps:";
        if (!diagnostic.StartsWith(mapPrefix,
                StringComparison.Ordinal))
            return false;
        var mapId = diagnostic[mapPrefix.Length..];
        return regionalEvents.Overlay?.Bindings.Any(item =>
            item.MapId == mapId) == true;
    }

    public static string FormatCard(
        GameProjectGeneratedWorldSummary summary,
        GameProjectGeneratedWorldActivationSummary? activation = null,
        GameProjectGeneratedRegionTravelSummary? travel = null,
        GameProjectGeneratedEncounterCombatSummary? combat = null)
    {
        ArgumentNullException.ThrowIfNull(summary);
        var status = summary.Status switch
        {
            "SOURCE_READY" => "Источник готов; сборка ещё не запускалась",
            "START_CURRENT" or "BUILD_CURRENT" => "Игровой старт проверен; переходы ещё не подтверждены",
            "TRAVEL_CURRENT" => "Сгенерированный маршрут проверен",
            "CAMPAIGN_CURRENT" => "Сгенерированная кампания и бои проверены",
            "LAST_SUCCESS" => "Показана последняя успешная сборка",
            _ => "Источник генерации повреждён или не подтверждён"
        };
        var rows = summary.HumanFacts
            .Concat(activation is { Present: true, Passed: true } ? activation.HumanFacts : [])
            .Concat(travel is { Present: true, Passed: true } ? travel.HumanFacts : [])
            .Concat(combat is { Present: true, Passed: true }
                ? combat.HumanReviewFacts.Select(fact => new GameProjectGeneratedWorldHumanFact
                {
                    Label = fact.Label,
                    Value = fact.Value
                })
                : [])
            .Select(fact => fact.Label + "    " + fact.Value)
            .Append("Статус сборки    " + status);
        return "Сгенерированный мир" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, rows);
    }

    private GameProjectGeneratedWorldSummary Build(
        SeededGeneratedProjectSourceValidationResult validation,
        string status,
        bool packageContentPreserved,
        IReadOnlyList<string> diagnostics)
    {
        var source = validation.Source ?? throw new InvalidOperationException("generated source is required");
        var mapTitle = validation.GeneratedBasePackage?.Game.Maps
            .SingleOrDefault(map => map.Id == source.GeneratedStartMapId)?.Name ?? source.GeneratedStartMapId;
        var summary = new GameProjectGeneratedWorldSummary
        {
            Present = true,
            Passed = status != "INVALID",
            Status = status,
            Seed = source.Seed,
            Mode = source.Mode,
            PresetId = source.PresetId,
            MechanicsProfileId = source.MechanicsProfileId,
            SourceRequestSha256 = SeededGeneratedProjectSourceService.HashText(string.Join("\n", new[]
            {
                source.Seed,
                source.Mode,
                source.PresetId,
                string.Join("|", source.StyleHintIds.OrderBy(value => value, StringComparer.Ordinal)),
                string.Join("|", source.VariantIds.OrderBy(value => value, StringComparer.Ordinal))
            })),
            PlanSha256 = source.PlanSha256,
            OverlaySha256 = source.GeneratedOverlaySha256,
            GeneratedBasePackageSha256 = source.GeneratedBasePackageSha256,
            RegionCount = source.Counts.Regions,
            FactionCount = source.Counts.Factions,
            ActorCount = source.Counts.Actors,
            ItemResourceCount = source.Counts.ItemsAndResources,
            EncounterCount = source.Counts.Encounters,
            QuestEventCount = source.Counts.QuestEvents,
            GeneratedStartMapTitle = mapTitle,
            TinyLoopPassed = source.TinyLoop.Passed,
            TinyLoopStepCount = source.TinyLoop.StepCount,
            TinyLoopInitialStateHash = source.TinyLoop.InitialStateHash,
            TinyLoopFinalStateHash = source.TinyLoop.FinalStateHash,
            RewardOrCostObserved = source.TinyLoop.RewardOrCostObserved,
            StateChangeObserved = source.TinyLoop.StateChangeObserved,
            PackageContentPreserved = packageContentPreserved,
            Diagnostics = diagnostics
        };
        return summary with { HumanFacts = BuildHumanFacts(summary) };
    }

    private IReadOnlyList<GameProjectGeneratedWorldHumanFact> BuildHumanFacts(GameProjectGeneratedWorldSummary summary)
    {
        var preset = _presetOptions.GetPresets().SingleOrDefault(item => item.PresetId == summary.PresetId)?.Title
                     ?? summary.PresetId;
        return
        [
            Fact("Seed", summary.Seed),
            Fact("Режим генерации", ModeTitle(summary.Mode)),
            Fact("Пресет", preset),
            Fact("Профиль", ProfileTitle(summary.MechanicsProfileId)),
            Fact("Регионы", summary.RegionCount.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            Fact("Фракции", summary.FactionCount.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            Fact("Персонажи", summary.ActorCount.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            Fact("Предметы и ресурсы", summary.ItemResourceCount.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            Fact("Столкновения", summary.EncounterCount.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            Fact("Задания и события", summary.QuestEventCount.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            Fact("Сгенерированный цикл", summary.TinyLoopPassed && summary.RewardOrCostObserved && summary.StateChangeObserved
                ? "пройден; награда/затрата и изменение состояния подтверждены"
                : "не подтверждён")
        ];
    }

    private static string ModeTitle(string mode) => mode switch
    {
        ProceduralGameGenerationModes.AuthoredSmallWorld => "Авторский компактный мир",
        ProceduralGameGenerationModes.SemiProceduralRegions => "Полупроцедурные регионы",
        ProceduralGameGenerationModes.FullySeededWorld => "Полностью генерируемый мир",
        _ => "Неизвестный режим"
    };

    private static string ProfileTitle(string profile) => profile switch
    {
        GeneratedProjectMechanicsProfiles.AllSelectableDefaults => "Все доступные механики",
        GeneratedProjectMechanicsProfiles.CoreOnly => "Только обязательные",
        _ => "Неизвестный профиль"
    };

    private static GameProjectGeneratedWorldHumanFact Fact(string label, string value) => new()
    {
        Label = label,
        Value = value
    };
}
