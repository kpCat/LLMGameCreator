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
        GameProjectGeneratedWorldActivationSummary? activation = null)
    {
        ArgumentNullException.ThrowIfNull(validation);
        ArgumentNullException.ThrowIfNull(compositionPackage);
        ArgumentNullException.ThrowIfNull(activatedPackage);
        if (!validation.Present || !validation.Passed || validation.Source is null || validation.Overlay is null)
            return ProjectSource(validation) ?? new GameProjectGeneratedWorldSummary();
        var diagnostics = _overlayService.ValidatePackageRecords(compositionPackage, validation.Overlay, includeBaseline: false)
            .Concat(_overlayService.ValidatePackageRecords(activatedPackage, validation.Overlay, includeBaseline: false))
            .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        var status = diagnostics.Count > 0
            ? "INVALID"
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
        GameProjectGeneratedWorldActivationSummary? activation = null)
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
        var status = matchesCurrentAuthoring ? "BUILD_CURRENT" : "LAST_SUCCESS";
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

    public static string FormatCard(
        GameProjectGeneratedWorldSummary summary,
        GameProjectGeneratedWorldActivationSummary? activation = null)
    {
        ArgumentNullException.ThrowIfNull(summary);
        var status = summary.Status switch
        {
            "SOURCE_READY" => "Источник готов; сборка ещё не запускалась",
            "BUILD_CURRENT" => "Сборка соответствует текущим настройкам",
            "LAST_SUCCESS" => "Показана последняя успешная сборка",
            _ => "Источник генерации повреждён или не подтверждён"
        };
        var rows = summary.HumanFacts
            .Concat(activation is { Present: true, Passed: true } ? activation.HumanFacts : [])
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
