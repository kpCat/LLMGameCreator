using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed record GameProjectGeneratedWorldActivationRequest
{
    public string CompatibilityPackagePath { get; init; } = string.Empty;
    public GamePackageDefinition CompatibilityPackage { get; init; } = new();
    public SeededGeneratedProjectSourceValidationResult GeneratedSource { get; init; } = new();
    public GameProjectIdentityDocument ProjectIdentity { get; init; } = new();
    public string OutputRoot { get; init; } = string.Empty;
}

public sealed record GameProjectGeneratedWorldActivationResult
{
    public bool Passed { get; init; }
    public string CompatibilityPackageSha256 { get; init; } = string.Empty;
    public string PlayerCompositionPackagePath { get; init; } = string.Empty;
    public string PlayerCompositionPackageJson { get; init; } = string.Empty;
    public string PlayerCompositionPackageSha256 { get; init; } = string.Empty;
    public string ActivatedProjectPackagePath { get; init; } = string.Empty;
    public string ActivatedProjectPackageSha256 { get; init; } = string.Empty;
    public GamePackageDefinition PlayerCompositionPackage { get; init; } = new();
    public GamePackageDefinition ActivatedProjectPackage { get; init; } = new();
    public IReadOnlyList<string> CanonicalGameplayRecordDiff { get; init; } = [];
    public IReadOnlyList<string> ManifestDiff { get; init; } = [];
    public GameProjectGeneratedWorldActivationSummary Summary { get; init; } = new();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class GameProjectGeneratedWorldActivationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IGameRuntime _runtime;
    private readonly IRuntimeStateSerializer _stateSerializer;
    private readonly IGamePackageValidator _validator;
    private readonly GameProjectPackageIdentityOverlayService _identityOverlay;

    public GameProjectGeneratedWorldActivationService(
        IGameRuntime runtime,
        IRuntimeStateSerializer stateSerializer,
        IGamePackageValidator? validator = null,
        GameProjectPackageIdentityOverlayService? identityOverlay = null)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _stateSerializer = stateSerializer ?? throw new ArgumentNullException(nameof(stateSerializer));
        _validator = validator ?? new GamePackageValidator();
        _identityOverlay = identityOverlay ?? new GameProjectPackageIdentityOverlayService();
    }

    public GameProjectGeneratedWorldActivationResult Activate(GameProjectGeneratedWorldActivationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var diagnostics = new List<string>();
        var source = request.GeneratedSource;
        if (!source.Present || !source.Passed || source.Source is null || source.Overlay is null
            || source.GeneratedMvpPackage is null)
            return Failed("generated_activation.source_not_current");
        if (string.IsNullOrWhiteSpace(request.CompatibilityPackagePath)
            || !File.Exists(request.CompatibilityPackagePath))
            return Failed("generated_activation.compatibility_package_missing");

        var sourceRecord = source.Source;
        var compatibilityJson = File.ReadAllText(request.CompatibilityPackagePath, Encoding.UTF8);
        var compatibilitySha256 = HashFile(request.CompatibilityPackagePath);
        var generatedMaps = request.CompatibilityPackage.Game.Maps
            .Where(map => string.Equals(map.Id, sourceRecord.GeneratedStartMapId, StringComparison.Ordinal)).ToList();
        if (generatedMaps.Count != 1)
            return Failed("generated_activation.generated_start_map_missing", compatibilitySha256);
        var generatedMap = generatedMaps[0];
        if (!ValidStartPosition(request.CompatibilityPackage, generatedMap))
            return Failed("generated_activation.start_position_invalid", compatibilitySha256);

        var generatedMvpMap = source.GeneratedMvpPackage.Game.Maps
            .SingleOrDefault(map => string.Equals(map.Id, sourceRecord.GeneratedStartMapId, StringComparison.Ordinal));
        var generatedEntityIds = generatedMvpMap?.Entities.Select(entity => entity.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet(StringComparer.Ordinal) ?? [];
        if (generatedEntityIds.Count == 0
            || !generatedMap.Entities.Any(entity => generatedEntityIds.Contains(entity.Id)
                                                   && IsInteractable(request.CompatibilityPackage, entity)))
            return Failed("generated_activation.generated_interactable_missing", compatibilitySha256);

        var playerComposition = Deserialize(compatibilityJson);
        playerComposition.Manifest.StartMapId = sourceRecord.GeneratedStartMapId;
        var gameplayDiff = GameplayDiff(request.CompatibilityPackage, playerComposition);
        if (gameplayDiff.Count > 0)
            return Failed("generated_activation.gameplay_collections_changed", compatibilitySha256, gameplayDiff);
        var manifestDiff = ManifestDiff(request.CompatibilityPackage, playerComposition);
        if (!manifestDiff.SequenceEqual(["manifest.startMapId"], StringComparer.Ordinal))
            return Failed("generated_activation.manifest_diff_invalid", compatibilitySha256, manifestDiff);

        var outputRoot = Path.GetFullPath(request.OutputRoot);
        var playerCompositionPath = Confined(outputRoot, "player-composition/package.json");
        var activatedPath = Confined(outputRoot, "identity-overlaid/package.json");
        var playerCompositionJson = JsonSerializer.Serialize(playerComposition, JsonOptions) + Environment.NewLine;
        Directory.CreateDirectory(Path.GetDirectoryName(playerCompositionPath)!);
        File.WriteAllText(playerCompositionPath, playerCompositionJson, new UTF8Encoding(false));
        var playerCompositionSha256 = HashFile(playerCompositionPath);
        var identity = _identityOverlay.Overlay(playerCompositionPath, activatedPath, request.ProjectIdentity);
        var activatedJson = File.ReadAllText(activatedPath, Encoding.UTF8);
        var activatedPackage = Deserialize(activatedJson);
        if (!IdentityMatches(activatedPackage, request.ProjectIdentity)
            || !string.Equals(activatedPackage.Manifest.StartMapId, sourceRecord.GeneratedStartMapId, StringComparison.Ordinal))
            return Failed("generated_activation.project_identity_invalid", compatibilitySha256);
        var identityGameplayDiff = GameplayDiff(playerComposition, activatedPackage);
        if (identityGameplayDiff.Count > 0)
            return Failed("generated_activation.identity_changed_gameplay", compatibilitySha256, identityGameplayDiff);
        var validation = _validator.Validate(activatedPackage);
        diagnostics.AddRange(validation.Issues
            .Where(issue => issue.Severity is Domain.Validation.ValidationSeverity.Error
                or Domain.Validation.ValidationSeverity.Critical)
            .Select(issue => "generated_activation.package_invalid:" + issue.Code));
        if (diagnostics.Count > 0)
            return Failed("generated_activation.package_invalid", compatibilitySha256, diagnostics);

        var primary = Run(activatedPackage, generatedEntityIds);
        var replay = Run(activatedPackage, generatedEntityIds);
        var replayEquivalent = Equivalent(primary, replay);
        if (!replayEquivalent) diagnostics.Add("generated_activation.replay_mismatch");

        var finalSession = new UnifiedRuntimeSession { MapState = CloneMapState(primary.FinalState) };
        var saved = _stateSerializer.Serialize(finalSession);
        var restored = _stateSerializer.DeserializeUnifiedSession(saved);
        var roundtripPassed = string.Equals(HashMapState(finalSession.MapState), HashMapState(restored.MapState), StringComparison.Ordinal)
                              && string.Equals(restored.MapState.CurrentMapId, sourceRecord.GeneratedStartMapId, StringComparison.Ordinal);
        if (!roundtripPassed) diagnostics.Add("generated_activation.state_roundtrip_mismatch");

        if (!primary.StartSucceeded) diagnostics.Add("generated_activation.start_failed");
        if (!primary.MoveSucceeded) diagnostics.Add("generated_activation.move_failed");
        if (!primary.InteractSucceeded) diagnostics.Add("generated_activation.interact_failed");
        if (!primary.GeneratedInteractionObserved) diagnostics.Add("generated_activation.generated_interaction_not_observed");
        if (!primary.StateChanged) diagnostics.Add("generated_activation.state_unchanged");
        if (primary.EventSemantics.Count == 0) diagnostics.Add("generated_activation.events_missing");
        if (!string.Equals(primary.FinalState.CurrentMapId, sourceRecord.GeneratedStartMapId, StringComparison.Ordinal))
            diagnostics.Add("generated_activation.current_map_mismatch");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();

        var passed = diagnostics.Count == 0;
        var summary = new GameProjectGeneratedWorldActivationSummary
        {
            Present = true,
            Passed = passed,
            GeneratedStartMapId = sourceRecord.GeneratedStartMapId,
            GeneratedStartMapTitle = generatedMap.Name,
            StartSucceeded = primary.StartSucceeded,
            MoveSucceeded = primary.MoveSucceeded,
            InteractSucceeded = primary.InteractSucceeded,
            GeneratedInteractionObserved = primary.GeneratedInteractionObserved,
            InitialStateHash = primary.InitialStateHash,
            FinalStateHash = primary.FinalStateHash,
            ReplayFinalStateHash = replay.FinalStateHash,
            ReplayEquivalent = replayEquivalent,
            StateRoundtripPassed = roundtripPassed,
            RuntimeFrames = primary.Frames,
            HumanFacts = HumanFacts(primary, replayEquivalent, roundtripPassed),
            Diagnostics = diagnostics
        };
        return new GameProjectGeneratedWorldActivationResult
        {
            Passed = passed,
            CompatibilityPackageSha256 = compatibilitySha256,
            PlayerCompositionPackagePath = playerCompositionPath,
            PlayerCompositionPackageJson = playerCompositionJson,
            PlayerCompositionPackageSha256 = playerCompositionSha256,
            ActivatedProjectPackagePath = activatedPath,
            ActivatedProjectPackageSha256 = identity.ActivatedProjectPackageSha256,
            PlayerCompositionPackage = playerComposition,
            ActivatedProjectPackage = activatedPackage,
            CanonicalGameplayRecordDiff = gameplayDiff,
            ManifestDiff = manifestDiff,
            Summary = summary,
            Diagnostics = diagnostics
        };
    }

    internal GameProjectGeneratedRegionTravelActivationService CreateRegionTravelActivationService() =>
        new(_runtime, _stateSerializer);

    private ActivationAttempt Run(GamePackageDefinition package, IReadOnlySet<string> generatedEntityIds)
    {
        var start = _runtime.Start(package);
        var initialHash = HashMapState(start.State);
        var frames = new List<GameProjectRuntimeFrame>
        {
            Frame(0, "00_start_generated_world", "Игровой старт", "generated_activation", initialHash)
        };
        var semantics = EventSemantics("start", start.Events).ToList();
        var current = start.State;
        var move = start.Success
            ? _runtime.Execute(package, current, PlayerCommand.Move(Direction2D.Right))
            : new CommandResult { State = current, Success = false };
        current = move.State;
        var moveHash = HashMapState(current);
        frames.Add(Frame(1, "01_move_right", "Движение", "generated_activation", moveHash));
        semantics.AddRange(EventSemantics("move", move.Events));
        var interact = start.Success && move.Success
            ? _runtime.Execute(package, current, PlayerCommand.Interact())
            : new CommandResult { State = current, Success = false };
        current = interact.State;
        var finalHash = HashMapState(current);
        frames.Add(Frame(2, "02_interact", "Взаимодействие", "generated_activation", finalHash));
        semantics.AddRange(EventSemantics("interact", interact.Events));
        var generatedObserved = interact.Events.Any(runtimeEvent =>
            runtimeEvent.Type == RuntimeEventType.InteractionTriggered
            && runtimeEvent.TargetId is not null
            && generatedEntityIds.Contains(runtimeEvent.TargetId));
        return new ActivationAttempt(
            start.Success,
            move.Success,
            interact.Success,
            generatedObserved,
            !string.Equals(initialHash, finalHash, StringComparison.Ordinal),
            initialHash,
            finalHash,
            CloneMapState(current),
            frames,
            semantics);
    }

    private static bool Equivalent(ActivationAttempt left, ActivationAttempt right) =>
        left.StartSucceeded == right.StartSucceeded
        && left.MoveSucceeded == right.MoveSucceeded
        && left.InteractSucceeded == right.InteractSucceeded
        && left.GeneratedInteractionObserved == right.GeneratedInteractionObserved
        && left.FinalState.CurrentMapId == right.FinalState.CurrentMapId
        && left.FinalState.PlayerPosition.X == right.FinalState.PlayerPosition.X
        && left.FinalState.PlayerPosition.Y == right.FinalState.PlayerPosition.Y
        && left.EventSemantics.SequenceEqual(right.EventSemantics, StringComparer.Ordinal)
        && string.Equals(left.FinalStateHash, right.FinalStateHash, StringComparison.Ordinal);

    private string HashMapState(GameState state)
    {
        var json = _stateSerializer.Serialize(new UnifiedRuntimeSession { MapState = CloneMapState(state) });
        return HashText(json);
    }

    private static IEnumerable<string> EventSemantics(string step, IEnumerable<RuntimeEvent> events) =>
        events.Select(runtimeEvent => string.Join("|",
            step,
            runtimeEvent.Type.ToString(),
            runtimeEvent.TargetId ?? string.Empty,
            runtimeEvent.Message,
            string.Join(",", runtimeEvent.Args.OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => item.Key + "=" + item.Value))));

    private static IReadOnlyList<GameProjectGeneratedWorldHumanFact> HumanFacts(
        ActivationAttempt attempt,
        bool replayEquivalent,
        bool roundtripPassed) =>
    [
        Fact("Игровой старт", attempt.StartSucceeded ? "сгенерированная карта" : "не пройден"),
        Fact("Движение", attempt.MoveSucceeded ? "пройдено" : "не пройдено"),
        Fact("Взаимодействие", attempt.InteractSucceeded ? "пройдено" : "не пройдено"),
        Fact("Сгенерированное содержимое", attempt.GeneratedInteractionObserved ? "подтверждено" : "не подтверждено"),
        Fact("Повтор", replayEquivalent ? "идентичен" : "различается"),
        Fact("Сохранение состояния", roundtripPassed ? "пройдено" : "не пройдено")
    ];

    private static GameProjectGeneratedWorldHumanFact Fact(string label, string value) => new()
    {
        Label = label,
        Value = value
    };

    private static GameProjectRuntimeFrame Frame(int index, string id, string title, string category, string hash) => new()
    {
        Index = index,
        ActionId = id,
        Title = title,
        Category = category,
        StateHash = hash
    };

    private static bool ValidStartPosition(GamePackageDefinition package, MapDefinition map)
    {
        var position = map.StartPosition;
        if (position.X < 0 || position.Y < 0 || position.X >= map.Width || position.Y >= map.Height) return false;
        var tileId = map.Tiles.FirstOrDefault(tile => tile.X == position.X && tile.Y == position.Y)?.TileId
                     ?? map.DefaultTileId;
        return package.Game.TilePrototypes.Any(tile => tile.Id == tileId && tile.Walkable);
    }

    private static bool IsInteractable(GamePackageDefinition package, EntityInstanceDefinition entity) =>
        entity.Components.Any(component => component.Type == "interactable")
        || package.Game.EntityPrototypes.Any(prototype => prototype.Id == entity.PrototypeId
            && prototype.Components.Any(component => component.Type == "interactable"));

    private static IReadOnlyList<string> GameplayDiff(GamePackageDefinition left, GamePackageDefinition right) =>
        string.Equals(JsonSerializer.Serialize(left.Game, JsonOptions), JsonSerializer.Serialize(right.Game, JsonOptions),
            StringComparison.Ordinal)
        && string.Equals(JsonSerializer.Serialize(left.AssetCatalog, JsonOptions), JsonSerializer.Serialize(right.AssetCatalog, JsonOptions),
            StringComparison.Ordinal)
        && string.Equals(JsonSerializer.Serialize(left.ScriptCatalog, JsonOptions), JsonSerializer.Serialize(right.ScriptCatalog, JsonOptions),
            StringComparison.Ordinal)
        && string.Equals(JsonSerializer.Serialize(left.GeneratedContent, JsonOptions), JsonSerializer.Serialize(right.GeneratedContent, JsonOptions),
            StringComparison.Ordinal)
            ? []
            : ["gameplay_collections"];

    private static IReadOnlyList<string> ManifestDiff(GamePackageDefinition left, GamePackageDefinition right)
    {
        var result = new List<string>();
        if (!string.Equals(left.Manifest.PackageId, right.Manifest.PackageId, StringComparison.Ordinal)) result.Add("manifest.packageId");
        if (!string.Equals(left.Manifest.Title, right.Manifest.Title, StringComparison.Ordinal)) result.Add("manifest.title");
        if (!string.Equals(left.Manifest.Version, right.Manifest.Version, StringComparison.Ordinal)) result.Add("manifest.version");
        if (!string.Equals(left.Manifest.FormatVersion, right.Manifest.FormatVersion, StringComparison.Ordinal)) result.Add("manifest.formatVersion");
        if (!string.Equals(left.Manifest.Description, right.Manifest.Description, StringComparison.Ordinal)) result.Add("manifest.description");
        if (!string.Equals(left.Manifest.StartMapId, right.Manifest.StartMapId, StringComparison.Ordinal)) result.Add("manifest.startMapId");
        return result;
    }

    private static bool IdentityMatches(GamePackageDefinition package, GameProjectIdentityDocument identity) =>
        string.Equals(package.Manifest.PackageId, identity.PackageId, StringComparison.Ordinal)
        && string.Equals(package.Manifest.Title, identity.Title, StringComparison.Ordinal)
        && string.Equals(package.Manifest.Version, identity.Version, StringComparison.Ordinal)
        && string.Equals(package.Manifest.FormatVersion, identity.FormatVersion, StringComparison.Ordinal)
        && string.Equals(package.Manifest.Description ?? string.Empty, identity.Description, StringComparison.Ordinal);

    private static GameState CloneMapState(GameState state) => new()
    {
        CurrentMapId = state.CurrentMapId,
        PlayerPosition = new Position2D(state.PlayerPosition.X, state.PlayerPosition.Y),
        Mode = state.Mode,
        Flags = state.Flags.OrderBy(item => item.Key, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal)
    };

    private static GamePackageDefinition Deserialize(string json) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(json, JsonOptions)
        ?? throw new InvalidOperationException("generated_activation.package_deserialization_failed");

    private static string Confined(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.Equals(fullRoot, comparison)
            && !path.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("generated_activation.path_escape");
        return path;
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string HashText(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static GameProjectGeneratedWorldActivationResult Failed(
        string diagnostic,
        string compatibilitySha256 = "",
        IEnumerable<string>? details = null) => new()
        {
            Passed = false,
            CompatibilityPackageSha256 = compatibilitySha256,
            Summary = new GameProjectGeneratedWorldActivationSummary
            {
                Present = true,
                Passed = false,
                Diagnostics = new[] { diagnostic }.Concat(details ?? []).Distinct(StringComparer.Ordinal).ToList()
            },
            Diagnostics = new[] { diagnostic }.Concat(details ?? []).Distinct(StringComparer.Ordinal).ToList()
        };

    private sealed record ActivationAttempt(
        bool StartSucceeded,
        bool MoveSucceeded,
        bool InteractSucceeded,
        bool GeneratedInteractionObserved,
        bool StateChanged,
        string InitialStateHash,
        string FinalStateHash,
        GameState FinalState,
        IReadOnlyList<GameProjectRuntimeFrame> Frames,
        IReadOnlyList<string> EventSemantics);
}
