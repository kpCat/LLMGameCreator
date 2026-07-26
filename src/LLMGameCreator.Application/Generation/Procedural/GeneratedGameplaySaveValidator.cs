using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedGameplaySaveValidator
{
    private readonly string _repositoryRoot;
    private readonly IGamePackageRepository _packageRepository;
    private readonly IGamePackageValidator _packageValidator;
    private readonly SeededGeneratedProjectSourceService _sourceService;
    private readonly GeneratedWorldHistoryService _historyService;
    private readonly GeneratedWorldRegionMapBindingService _bindingService;
    private readonly GeneratedGameplayDefinitionFingerprintService _fingerprints;
    private readonly IRuntimeStateSerializer _serializer;
    private readonly IGameProjectOperationCoordinator _operationCoordinator;

    public GeneratedGameplaySaveValidator(
        string repositoryRoot,
        IGamePackageRepository packageRepository,
        IGamePackageValidator packageValidator,
        SeededGeneratedProjectSourceService sourceService,
        GeneratedWorldHistoryService historyService,
        GeneratedGameplayDefinitionFingerprintService fingerprints,
        IRuntimeStateSerializer serializer,
        IGameProjectOperationCoordinator operationCoordinator,
        GeneratedWorldRegionMapBindingService? bindingService = null)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _packageValidator = packageValidator ?? throw new ArgumentNullException(nameof(packageValidator));
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _fingerprints = fingerprints ?? throw new ArgumentNullException(nameof(fingerprints));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _operationCoordinator = operationCoordinator ?? throw new ArgumentNullException(nameof(operationCoordinator));
        _bindingService = bindingService ?? new GeneratedWorldRegionMapBindingService();
    }

    public GeneratedGameplaySaveProjectTruthResult CaptureProjectTruth(
        string projectFolder,
        GameProjectOperationLease operationLease)
    {
        var diagnostics = new List<string>();
        try
        {
            var project = Path.GetFullPath(projectFolder);
            if (!_operationCoordinator.IsCurrent(operationLease, project))
                return TruthFailed("project_operation.lease_invalid");
            var source = _sourceService.Validate(project);
            if (source is not { Present: true, Passed: true, Source: not null })
                return TruthFailed(source.Present
                    ? "generated_save.project_not_ready" : "generated_save.not_generated_project");
            var package = _packageRepository.LoadAsync(project, CancellationToken.None).GetAwaiter().GetResult();
            var packageValidation = _packageValidator.Validate(package, project);
            if (!packageValidation.IsValid) return TruthFailed("generated_save.project_not_ready");
            var authoring = new GameProjectFeatureModuleAuthoringService(
                _repositoryRoot, operationCoordinator: _operationCoordinator);
            var state = authoring.OpenProject(project, package, operationLease);
            var fingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(
                state.Document, state.Library);
            if (!fingerprint.Passed || string.IsNullOrWhiteSpace(fingerprint.Sha256))
                return TruthFailed("generated_save.project_not_ready");
            var packagePath = Confined(project, "package.json");
            var packageSha256 = HashFile(packagePath);
            if (!string.Equals(packageSha256, state.Document.LastActivatedProjectPackageSha256,
                    StringComparison.Ordinal))
                return TruthFailed("generated_save.package_changed");

            var selected = SelectHistory(project, state, packageSha256, fingerprint.Sha256);
            if (selected.Entry is null)
                return TruthFailed(selected.Diagnostic ?? "generated_save.history_not_current");
            var history = selected.Entry;
            if (history.GeneratedWorld is not { Present: true, Passed: true }
                || history.GeneratedWorldActivation is not { Present: true, Passed: true }
                || history.GeneratedRegionTravel is not { Present: true, Passed: true, ReplayEquivalent: true,
                    StateRoundtripPassed: true }
                || history.AcceptedMechanicsCompatibility is not { Passed: true })
                return TruthFailed("generated_save.travel_not_current");
            if (!string.Equals(history.GeneratedWorld.GeneratedBasePackageSha256,
                    source.Source.GeneratedBasePackageSha256, StringComparison.Ordinal)
                || !string.Equals(history.GeneratedWorld.PlanSha256, source.Source.PlanSha256,
                    StringComparison.Ordinal)
                || !string.Equals(history.GeneratedWorld.OverlaySha256,
                    source.Source.GeneratedOverlaySha256, StringComparison.Ordinal))
                return TruthFailed("generated_save.history_not_current");
            if (!string.Equals(package.Manifest.StartMapId, source.Source.GeneratedStartMapId,
                    StringComparison.Ordinal))
                return TruthFailed("generated_save.travel_not_current");
            var bindings = _bindingService.Bind(source, package);
            if (!bindings.Passed) return TruthFailed("generated_save.travel_not_current");
            var definitionInventory = _fingerprints.BuildInventory(package);
            var duplicate = definitionInventory.GroupBy(item => item.Kind + "\n" + item.Id,
                    StringComparer.Ordinal).FirstOrDefault(group => group.Count() != 1);
            if (duplicate is not null) return TruthFailed("generated_save.project_not_ready");
            var identityFingerprint = GameProjectSeedRegenerationService.IdentityFingerprint(state.Identity);
            return new GeneratedGameplaySaveProjectTruthResult
            {
                Passed = true,
                Truth = new GeneratedGameplaySaveProjectTruth
                {
                    ProjectFolder = project,
                    Identity = state.Identity,
                    IdentityFingerprint = identityFingerprint,
                    StrictGeneratedSource = source,
                    WorldId = _historyService.WorldId(project, source),
                    ActualPackage = package,
                    PackageSha256 = packageSha256,
                    CompositionPackageSha256 = state.Document.LastCompositionPackageSha256,
                    QualifiedAuthoringFingerprint = fingerprint.Sha256,
                    SelectedBuildHistoryFileName = selected.FileName,
                    SelectedBuildHistorySha256 = HashFile(selected.Path),
                    SelectedBuildHistory = selected.Entry,
                    GeneratedStartMapId = source.Source.GeneratedStartMapId,
                    GeneratedRegionMapBindings = new SortedDictionary<string, string>(
                        bindings.RegionBindings.ToDictionary(item => item.RegionId, item => item.MapId,
                            StringComparer.Ordinal), StringComparer.Ordinal),
                    DefinitionFingerprintInventory = definitionInventory
                },
                Diagnostics = diagnostics
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or JsonException or InvalidOperationException)
        {
            return TruthFailed(exception.Message.StartsWith("generated_save.", StringComparison.Ordinal)
                || exception.Message.StartsWith("project_operation.", StringComparison.Ordinal)
                ? exception.Message : "generated_save.project_not_ready:" + exception.Message);
        }
    }

    public GeneratedGameplaySaveValidationResult ValidateSession(
        GeneratedGameplaySaveProjectTruth truth,
        UnifiedRuntimeSession session)
    {
        try
        {
            var json = _serializer.Serialize(session);
            var roundtrip = _serializer.DeserializeUnifiedSession(json);
            if (!string.Equals(json, _serializer.Serialize(roundtrip), StringComparison.Ordinal))
                return Invalid("generated_save.session_roundtrip_mismatch");
            if (!string.Equals(session.GameplayState.PackageId, truth.Identity.PackageId,
                    StringComparison.Ordinal))
                return Invalid("generated_save.package_changed");
            if (!string.IsNullOrWhiteSpace(session.GameplayState.CurrentMapId)
                && !string.Equals(session.GameplayState.CurrentMapId, session.MapState.CurrentMapId,
                    StringComparison.Ordinal))
                return Invalid("generated_save.current_map_mismatch");
            if (!ValidPosition(truth.ActualPackage, session.MapState.CurrentMapId,
                    session.MapState.PlayerPosition.X, session.MapState.PlayerPosition.Y))
                return Invalid("generated_save.map_position_invalid");
            var references = _fingerprints.CaptureReferences(
                truth.ActualPackage, session, truth.DefinitionFingerprintInventory);
            return new GeneratedGameplaySaveValidationResult
            {
                Passed = references.Passed,
                Status = references.Passed
                    ? GeneratedGameplaySaveStatus.CURRENT : GeneratedGameplaySaveStatus.INVALID,
                Session = session,
                References = references,
                Diagnostics = references.Diagnostics
            };
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            return Invalid("generated_save.session_invalid:" + exception.Message);
        }
    }

    public GeneratedGameplaySaveValidationResult ValidateRevision(
        GeneratedGameplaySaveProjectTruth truth,
        GeneratedGameplaySaveRevision revision)
    {
        try
        {
            if (revision.SchemaVersion != GeneratedGameplaySaveVocabulary.RevisionSchemaVersion
                || !string.Equals(GeneratedGameplaySaveJson.RevisionSha256(revision),
                    revision.RevisionSha256, StringComparison.Ordinal)
                || !string.Equals(GeneratedGameplaySaveJson.HashText(revision.UnifiedRuntimeSessionJson),
                    revision.UnifiedRuntimeSessionSha256, StringComparison.Ordinal))
                return Invalid("generated_save.revision_hash_mismatch");
            var session = _serializer.DeserializeUnifiedSession(revision.UnifiedRuntimeSessionJson);
            if (!string.Equals(_serializer.Serialize(session), revision.UnifiedRuntimeSessionJson,
                    StringComparison.Ordinal)
                || !string.Equals(GeneratedGameplaySaveJson.HashCanonical(session.MapState),
                    revision.MapStateSha256, StringComparison.Ordinal)
                || !string.Equals(GeneratedGameplaySaveJson.HashCanonical(session.GameplayState),
                    revision.GameplayStateSha256, StringComparison.Ordinal)
                || !string.Equals(session.MapState.CurrentMapId, revision.CurrentMapId,
                    StringComparison.Ordinal))
                return Invalid("generated_save.session_hash_mismatch");
            if (!string.Equals(revision.ProjectPackageId, truth.Identity.PackageId,
                    StringComparison.Ordinal)
                || !string.Equals(revision.ProjectIdentityFingerprint, truth.IdentityFingerprint,
                    StringComparison.Ordinal))
                return Invalid("generated_save.foreign_project_identity");
            if (!string.Equals(revision.WorldId, truth.WorldId, StringComparison.Ordinal))
                return Migratable(GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED, session);

            var source = truth.StrictGeneratedSource.Source!;
            var sourceRecordSha = HashFile(Confined(truth.ProjectFolder,
                SeededGeneratedProjectVocabulary.SourceRelativePath));
            var packageRebase = !string.Equals(revision.SourceRecordSha256, sourceRecordSha,
                                    StringComparison.Ordinal)
                                || !string.Equals(revision.SourceRequestSha256,
                                    GameProjectSeedRegenerationDiffService.RequestSha256(source.GenerationRequest),
                                    StringComparison.Ordinal)
                                || !string.Equals(revision.PlanSha256, source.PlanSha256,
                                    StringComparison.Ordinal)
                                || !string.Equals(revision.OverlaySha256, source.GeneratedOverlaySha256,
                                    StringComparison.Ordinal)
                                || !string.Equals(revision.GeneratedBasePackageSha256,
                                    source.GeneratedBasePackageSha256, StringComparison.Ordinal)
                                || !string.Equals(revision.PackageSha256, truth.PackageSha256,
                                    StringComparison.Ordinal)
                                || !string.Equals(revision.CompositionPackageSha256,
                                    truth.CompositionPackageSha256, StringComparison.Ordinal)
                                || !string.Equals(revision.QualifiedAuthoringFingerprint,
                                    truth.QualifiedAuthoringFingerprint, StringComparison.Ordinal)
                                || !string.Equals(revision.SelectedBuildHistoryFileName,
                                    truth.SelectedBuildHistoryFileName, StringComparison.Ordinal)
                                || !string.Equals(revision.SelectedBuildHistorySha256,
                                    truth.SelectedBuildHistorySha256, StringComparison.Ordinal);
            if (packageRebase)
                return Migratable(GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED, session);
            var sessionValidation = ValidateSession(truth, session);
            if (!sessionValidation.Passed) return sessionValidation;
            var expected = revision.DefinitionFingerprints.OrderBy(item => item.Kind, StringComparer.Ordinal)
                .ThenBy(item => item.Id, StringComparer.Ordinal).ToList();
            var actual = sessionValidation.References!.Fingerprints.OrderBy(item => item.Kind, StringComparer.Ordinal)
                .ThenBy(item => item.Id, StringComparer.Ordinal).ToList();
            if (!string.Equals(GeneratedGameplaySaveJson.Canonical(expected),
                    GeneratedGameplaySaveJson.Canonical(actual), StringComparison.Ordinal))
                return Invalid("generated_save.definition_fingerprint_mismatch");
            return sessionValidation with { Status = GeneratedGameplaySaveStatus.CURRENT };
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException
                                           or IOException or UnauthorizedAccessException)
        {
            return Invalid("generated_save.revision_invalid:" + exception.Message);
        }
    }

    public static UnifiedRuntimeSession CloneSession(IRuntimeStateSerializer serializer, UnifiedRuntimeSession session) =>
        serializer.DeserializeUnifiedSession(serializer.Serialize(session));

    public static bool ValidPosition(GamePackageDefinition package, string mapId, int x, int y)
    {
        var maps = package.Game.Maps.Where(map => map.Id == mapId).ToList();
        if (maps.Count != 1) return false;
        var map = maps[0];
        if (x < 0 || y < 0 || x >= map.Width || y >= map.Height) return false;
        var tileId = map.Tiles.LastOrDefault(tile => tile.X == x && tile.Y == y)?.TileId
                     ?? map.DefaultTileId;
        var tiles = package.Game.TilePrototypes.Where(tile => tile.Id == tileId).ToList();
        if (tiles.Count != 1 || !tiles[0].Walkable) return false;
        return !map.Entities.Any(entity => entity.Position.X == x && entity.Position.Y == y
                                           && HasComponent(package, entity, "collidable"));
    }

    private static bool HasComponent(
        GamePackageDefinition package,
        LLMGameCreator.Domain.Definitions.EntityInstanceDefinition entity,
        string type) => entity.Components.Any(component => component.Type == type)
                        || package.Game.EntityPrototypes.SingleOrDefault(prototype =>
                            prototype.Id == entity.PrototypeId)?.Components.Any(component =>
                            component.Type == type) == true;

    private static (GameProjectBuildHistoryEntry? Entry, string FileName, string Path, string? Diagnostic)
        SelectHistory(
            string project,
            GameProjectAuthoringState state,
            string packageSha256,
            string fingerprint)
    {
        var root = Confined(project, UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot);
        if (!Directory.Exists(root)) return (null, string.Empty, string.Empty,
            "generated_save.history_not_current");
        var matches = new List<(GameProjectBuildHistoryEntry Entry, string FileName, string Path)>();
        foreach (var path in Directory.EnumerateFiles(root, "*.json", SearchOption.TopDirectoryOnly))
        {
            GameProjectBuildHistoryEntry? entry;
            try { entry = GeneratedGameplaySaveJson.Deserialize<GameProjectBuildHistoryEntry>(
                File.ReadAllText(path, Encoding.UTF8)); }
            catch (JsonException) { continue; }
            if (entry is null || entry.Status != "GREEN" || entry.AttemptStatus != "GREEN"
                || entry.GeneratedWorld is not { Present: true, Passed: true }
                || entry.GeneratedWorldActivation is not { Present: true, Passed: true }
                || entry.GeneratedRegionTravel is not { Present: true, Passed: true, ReplayEquivalent: true,
                    StateRoundtripPassed: true }
                || !string.Equals(entry.PackageSha256, packageSha256, StringComparison.Ordinal)
                || !string.Equals(entry.PackageSha256, state.Document.LastActivatedProjectPackageSha256,
                    StringComparison.Ordinal)
                || !string.Equals(entry.CompositionPackageSha256,
                    state.Document.LastCompositionPackageSha256, StringComparison.Ordinal)
                || !string.Equals(entry.FinalStateHash, state.Document.LastQualifiedFinalStateHash,
                    StringComparison.Ordinal)
                || !string.Equals(entry.QualifiedAuthoringFingerprint, fingerprint,
                    StringComparison.Ordinal)) continue;
            matches.Add((entry, Path.GetFileName(path), path));
        }
        // Stable semantic selection lets an immutable revision become CURRENT again when an
        // exact historical world/package is restored and the original GREEN proof still exists.
        var selected = matches.OrderBy(item => item.Entry.CompletedAtUtc)
            .ThenBy(item => item.FileName, StringComparer.Ordinal).FirstOrDefault();
        return selected.Entry is null
            ? (null, string.Empty, string.Empty, "generated_save.history_not_current")
            : (selected.Entry, selected.FileName, selected.Path, null);
    }

    private static GeneratedGameplaySaveProjectTruthResult TruthFailed(string diagnostic) => new()
    {
        Diagnostics = [diagnostic]
    };

    private static GeneratedGameplaySaveValidationResult Invalid(string diagnostic) => new()
    {
        Status = GeneratedGameplaySaveStatus.INVALID,
        Diagnostics = [diagnostic]
    };

    private static GeneratedGameplaySaveValidationResult Migratable(
        GeneratedGameplaySaveStatus status,
        UnifiedRuntimeSession session) => new()
    {
        Passed = true,
        Status = status,
        Session = session
    };

    private static string Confined(string root, string relative) =>
        GameProjectFeatureModuleAuthoringService.ConfinedPath(root, relative);

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
