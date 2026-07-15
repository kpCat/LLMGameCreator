using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed record GameProjectGeneratedRegionTravelActivationRequest
{
    public SeededGeneratedProjectSourceValidationResult GeneratedSource { get; init; } = new();
    public GamePackageDefinition PlayerPackage { get; init; } = new();
}

public sealed record GameProjectGeneratedRegionTravelActivationResult
{
    public bool Passed { get; init; }
    public GeneratedWorldTravelRoutePlan RoutePlan { get; init; } = new();
    public GameProjectGeneratedRegionTravelSummary Summary { get; init; } = new();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class GameProjectGeneratedRegionTravelActivationService
{
    private readonly IGameRuntime _runtime;
    private readonly IRuntimeStateSerializer _stateSerializer;
    private readonly GeneratedWorldTravelRoutePlanner _planner;

    public GameProjectGeneratedRegionTravelActivationService(
        IGameRuntime runtime,
        IRuntimeStateSerializer stateSerializer,
        GeneratedWorldTravelRoutePlanner? planner = null)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _stateSerializer = stateSerializer ?? throw new ArgumentNullException(nameof(stateSerializer));
        _planner = planner ?? new GeneratedWorldTravelRoutePlanner();
    }

    public GameProjectGeneratedRegionTravelActivationResult Activate(
        GameProjectGeneratedRegionTravelActivationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var plan = _planner.Plan(request.GeneratedSource, request.PlayerPackage);
        if (!plan.Passed)
            return Failed(plan, plan.Diagnostics.Count > 0
                ? plan.Diagnostics
                : ["generated_travel.route_planning_failed"]);

        var primary = Run(request.PlayerPackage, plan);
        var replay = Run(request.PlayerPackage, plan);
        var diagnostics = primary.Diagnostics.Concat(replay.Diagnostics.Select(item => "replay:" + item)).ToList();
        var replayEquivalent = Equivalent(primary, replay);
        if (!replayEquivalent) diagnostics.Add("generated_travel.replay_mismatch");

        var finalSession = new UnifiedRuntimeSession { MapState = Clone(primary.FinalState) };
        var serialized = _stateSerializer.Serialize(finalSession);
        UnifiedRuntimeSession restored;
        try
        {
            restored = _stateSerializer.DeserializeUnifiedSession(serialized);
        }
        catch (Exception exception)
        {
            diagnostics.Add("generated_travel.state_roundtrip_failed:" + exception.GetType().Name);
            restored = new UnifiedRuntimeSession();
        }
        var roundtripPassed = ExactState(primary.FinalState, restored.MapState)
                              && string.Equals(HashState(primary.FinalState), HashState(restored.MapState),
                                  StringComparison.Ordinal);
        if (!roundtripPassed) diagnostics.Add("generated_travel.state_roundtrip_mismatch");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList();

        var passed = diagnostics.Count == 0
                     && primary.OriginInteractionObserved
                     && primary.GateInteractionsPassed
                     && primary.DestinationInteractionObserved
                     && primary.TransitionCount == plan.ConnectionIds.Count
                     && replayEquivalent
                     && roundtripPassed;
        var summary = new GameProjectGeneratedRegionTravelSummary
        {
            Present = true,
            Passed = passed,
            OriginRegionId = plan.OriginRegionId,
            OriginRegionTitle = plan.OriginRegionTitle,
            OriginMapId = plan.OriginMapId,
            OriginMapTitle = plan.OriginMapTitle,
            DestinationRegionId = plan.DestinationRegionId,
            DestinationRegionTitle = plan.DestinationRegionTitle,
            DestinationMapId = plan.DestinationMapId,
            DestinationMapTitle = plan.DestinationMapTitle,
            ConnectionIds = plan.ConnectionIds,
            TransitionCount = primary.TransitionCount,
            VisitedRegionIds = primary.VisitedRegionIds,
            VisitedMapIds = primary.VisitedMapIds,
            MovementCommandCount = plan.MovementCommandCount,
            OriginInteractionObserved = primary.OriginInteractionObserved,
            TravelGateInteractionsPassed = primary.GateInteractionsPassed,
            DestinationInteractionObserved = primary.DestinationInteractionObserved,
            InitialStateHash = primary.InitialStateHash,
            FinalStateHash = primary.FinalStateHash,
            ReplayFinalStateHash = replay.FinalStateHash,
            ReplayEquivalent = replayEquivalent,
            StateRoundtripPassed = roundtripPassed,
            RuntimeFrames = primary.Frames,
            HumanFacts = HumanFacts(plan, primary, replayEquivalent, roundtripPassed),
            Diagnostics = diagnostics
        };
        return new GameProjectGeneratedRegionTravelActivationResult
        {
            Passed = passed,
            RoutePlan = plan,
            Summary = summary,
            Diagnostics = diagnostics
        };
    }

    private RouteAttempt Run(GamePackageDefinition package, GeneratedWorldTravelRoutePlan plan)
    {
        var diagnostics = new List<string>();
        var start = _runtime.Start(package);
        var current = start.State;
        if (!start.Success
            || !string.Equals(current.CurrentMapId, plan.OriginMapId, StringComparison.Ordinal))
            diagnostics.Add("generated_travel.start_failed");
        var initialHash = HashState(current);
        var frames = new List<GameProjectRuntimeFrame>
        {
            Frame(0, "00_generated_start", "Игровой старт в сгенерированном регионе",
                "generated_start", initialHash)
        };
        var observations = new List<string>
        {
            Observation("start", start.Success, current, start.Events)
        };
        var visitedRegions = new List<string> { plan.OriginRegionId };
        var visitedMaps = new List<string> { plan.OriginMapId };
        var originObserved = false;
        var destinationObserved = false;
        var gatesPassed = true;
        var transitionCount = 0;

        for (var index = 0; index < plan.Actions.Count; index++)
        {
            var action = plan.Actions[index];
            var result = _runtime.Execute(package, current, action.Command);
            current = result.State;
            var prefix = "generated_travel.action_" + index;
            if (!result.Success) diagnostics.Add(prefix + ".failed");
            if (!string.Equals(current.CurrentMapId, action.ExpectedMapId, StringComparison.Ordinal)
                || current.PlayerPosition.X != action.ExpectedX
                || current.PlayerPosition.Y != action.ExpectedY)
                diagnostics.Add(prefix + ".state_mismatch");

            switch (action.Kind)
            {
                case GeneratedWorldTravelPlannedActionKind.Move:
                    break;
                case GeneratedWorldTravelPlannedActionKind.OriginInteraction:
                    originObserved = CorrelatedInteraction(result.Events, action.TargetEntityId);
                    if (!originObserved) diagnostics.Add("generated_travel.origin_interaction_missing");
                    break;
                case GeneratedWorldTravelPlannedActionKind.DestinationInteraction:
                    destinationObserved = CorrelatedInteraction(result.Events, action.TargetEntityId);
                    if (!destinationObserved) diagnostics.Add("generated_travel.destination_interaction_missing");
                    break;
                case GeneratedWorldTravelPlannedActionKind.GateInteraction:
                    var gatePassed = CorrelatedInteraction(result.Events, action.TargetEntityId)
                                     && CorrelatedMapChanged(result.Events, action);
                    gatesPassed &= gatePassed;
                    if (!gatePassed) diagnostics.Add("generated_travel.map_change_correlation_failed:" + action.ConnectionId);
                    if (gatePassed)
                    {
                        transitionCount++;
                        visitedRegions.Add(action.ToRegionId);
                        visitedMaps.Add(action.DestinationMapId);
                    }
                    break;
            }

            var category = Category(action.Kind);
            frames.Add(Frame(
                frames.Count,
                TechnicalActionId(index, action),
                Title(action.Kind),
                category,
                HashState(current)));
            observations.Add(Observation(index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                result.Success, current, result.Events));
        }

        return new RouteAttempt(
            originObserved,
            gatesPassed,
            destinationObserved,
            transitionCount,
            initialHash,
            HashState(current),
            Clone(current),
            visitedRegions,
            visitedMaps,
            frames,
            observations,
            diagnostics);
    }

    private static bool CorrelatedInteraction(IEnumerable<RuntimeEvent> events, string targetEntityId) =>
        events.Any(runtimeEvent => runtimeEvent.Type == RuntimeEventType.InteractionTriggered
                                   && string.Equals(runtimeEvent.TargetId, targetEntityId, StringComparison.Ordinal));

    private static bool CorrelatedMapChanged(
        IReadOnlyCollection<RuntimeEvent> events,
        GeneratedWorldTravelPlannedAction action)
    {
        var changed = events.Where(runtimeEvent => runtimeEvent.Type == RuntimeEventType.MapChanged).ToList();
        if (changed.Count != 1 || !string.Equals(changed[0].TargetId, action.DestinationMapId, StringComparison.Ordinal))
            return false;
        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [MapTransitionInteractionContract.ConnectionIdKey] = action.ConnectionId,
            [MapTransitionInteractionContract.SourceMapIdKey] = action.SourceMapId,
            [MapTransitionInteractionContract.DestinationMapIdKey] = action.DestinationMapId,
            [MapTransitionInteractionContract.DestinationXKey] = action.DestinationX.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            [MapTransitionInteractionContract.DestinationYKey] = action.DestinationY.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            [MapTransitionInteractionContract.FromRegionIdKey] = action.FromRegionId,
            [MapTransitionInteractionContract.ToRegionIdKey] = action.ToRegionId
        };
        return changed[0].Args.Count == expected.Count
               && expected.All(pair => changed[0].Args.TryGetValue(pair.Key, out var value)
                                       && string.Equals(value, pair.Value, StringComparison.Ordinal));
    }

    private static bool Equivalent(RouteAttempt left, RouteAttempt right) =>
        left.OriginInteractionObserved == right.OriginInteractionObserved
        && left.GateInteractionsPassed == right.GateInteractionsPassed
        && left.DestinationInteractionObserved == right.DestinationInteractionObserved
        && left.TransitionCount == right.TransitionCount
        && left.VisitedRegionIds.SequenceEqual(right.VisitedRegionIds, StringComparer.Ordinal)
        && left.VisitedMapIds.SequenceEqual(right.VisitedMapIds, StringComparer.Ordinal)
        && left.Observations.SequenceEqual(right.Observations, StringComparer.Ordinal)
        && string.Equals(left.FinalStateHash, right.FinalStateHash, StringComparison.Ordinal);

    private string HashState(GameState state) => Hash(_stateSerializer.Serialize(
        new UnifiedRuntimeSession { MapState = Clone(state) }));

    private static bool ExactState(GameState left, GameState right) =>
        string.Equals(left.CurrentMapId, right.CurrentMapId, StringComparison.Ordinal)
        && left.PlayerPosition.X == right.PlayerPosition.X
        && left.PlayerPosition.Y == right.PlayerPosition.Y
        && string.Equals(left.Mode, right.Mode, StringComparison.Ordinal)
        && left.Flags.Count == right.Flags.Count
        && left.Flags.OrderBy(item => item.Key, StringComparer.Ordinal)
            .SequenceEqual(right.Flags.OrderBy(item => item.Key, StringComparer.Ordinal));

    private static GameState Clone(GameState state) => new()
    {
        CurrentMapId = state.CurrentMapId,
        PlayerPosition = new LLMGameCreator.Domain.Definitions.Position2D(
            state.PlayerPosition.X, state.PlayerPosition.Y),
        Mode = state.Mode,
        Flags = state.Flags.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal)
    };

    private static string Observation(
        string step,
        bool success,
        GameState state,
        IEnumerable<RuntimeEvent> events) => string.Join("|",
        step,
        success,
        state.CurrentMapId,
        state.PlayerPosition.X,
        state.PlayerPosition.Y,
        state.Mode,
        string.Join(";", events.Select(runtimeEvent => string.Join("/",
            runtimeEvent.Type,
            runtimeEvent.TargetId ?? string.Empty,
            runtimeEvent.Message,
            string.Join(",", runtimeEvent.Args.OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => item.Key + "=" + item.Value))))));

    private static IReadOnlyList<GameProjectGeneratedWorldHumanFact> HumanFacts(
        GeneratedWorldTravelRoutePlan plan,
        RouteAttempt attempt,
        bool replayEquivalent,
        bool roundtripPassed) =>
    [
        Fact("Начальный регион", plan.OriginRegionTitle),
        Fact("Взаимодействие в начальном регионе", attempt.OriginInteractionObserved ? "подтверждено" : "не подтверждено"),
        Fact("Переход между регионами", attempt.GateInteractionsPassed ? "подтверждён" : "не подтверждён"),
        Fact("Посещено регионов", attempt.VisitedRegionIds.Distinct(StringComparer.Ordinal).Count().ToString(
            System.Globalization.CultureInfo.InvariantCulture)),
        Fact("Регион назначения", plan.DestinationRegionTitle),
        Fact("Взаимодействие после перехода", attempt.DestinationInteractionObserved ? "подтверждено" : "не подтверждено"),
        Fact("Повтор маршрута", replayEquivalent ? "идентичен" : "различается"),
        Fact("Сохранение состояния", roundtripPassed ? "пройдено" : "не пройдено")
    ];

    private static GameProjectGeneratedWorldHumanFact Fact(string label, string value) => new()
    {
        Label = label,
        Value = value
    };

    private static GameProjectRuntimeFrame Frame(
        int index,
        string actionId,
        string title,
        string category,
        string stateHash) => new()
        {
            Index = index,
            ActionId = actionId,
            Title = title,
            Category = category,
            StateHash = stateHash
        };

    private static string TechnicalActionId(int index, GeneratedWorldTravelPlannedAction action) =>
        index.ToString("D3", System.Globalization.CultureInfo.InvariantCulture) + "_" + action.Kind + "_"
        + (action.ConnectionId.Length > 0 ? action.ConnectionId : action.TargetEntityId);

    private static string Category(GeneratedWorldTravelPlannedActionKind kind) => kind switch
    {
        GeneratedWorldTravelPlannedActionKind.OriginInteraction => "generated_origin_interaction",
        GeneratedWorldTravelPlannedActionKind.GateInteraction => "generated_travel",
        GeneratedWorldTravelPlannedActionKind.DestinationInteraction => "generated_destination_interaction",
        _ => "generated_navigation"
    };

    private static string Title(GeneratedWorldTravelPlannedActionKind kind) => kind switch
    {
        GeneratedWorldTravelPlannedActionKind.OriginInteraction => "Взаимодействие в начальном регионе",
        GeneratedWorldTravelPlannedActionKind.GateInteraction => "Переход между регионами",
        GeneratedWorldTravelPlannedActionKind.DestinationInteraction => "Взаимодействие после перехода",
        _ => "Движение по маршруту"
    };

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static GameProjectGeneratedRegionTravelActivationResult Failed(
        GeneratedWorldTravelRoutePlan plan,
        IReadOnlyList<string> diagnostics) => new()
        {
            Passed = false,
            RoutePlan = plan,
            Summary = new GameProjectGeneratedRegionTravelSummary
            {
                Present = true,
                Passed = false,
                Diagnostics = diagnostics
            },
            Diagnostics = diagnostics
        };

    private sealed record RouteAttempt(
        bool OriginInteractionObserved,
        bool GateInteractionsPassed,
        bool DestinationInteractionObserved,
        int TransitionCount,
        string InitialStateHash,
        string FinalStateHash,
        GameState FinalState,
        IReadOnlyList<string> VisitedRegionIds,
        IReadOnlyList<string> VisitedMapIds,
        IReadOnlyList<GameProjectRuntimeFrame> Frames,
        IReadOnlyList<string> Observations,
        IReadOnlyList<string> Diagnostics);
}
