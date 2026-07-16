using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedGameplaySaveService
{
    private readonly IGameProjectOperationCoordinator _operationCoordinator;
    private readonly GeneratedGameplaySaveValidator _validator;
    private readonly GeneratedGameplaySaveStore _store;
    private readonly IRuntimeStateSerializer _serializer;
    private readonly IRuntimeSnapshotStore _legacyStore;
    private readonly SeededGeneratedProjectSourceService _sourceService;

    public GeneratedGameplaySaveService(
        IGameProjectOperationCoordinator operationCoordinator,
        GeneratedGameplaySaveValidator validator,
        GeneratedGameplaySaveStore store,
        IRuntimeStateSerializer serializer,
        IRuntimeSnapshotStore legacyStore,
        SeededGeneratedProjectSourceService sourceService)
    {
        _operationCoordinator = operationCoordinator ?? throw new ArgumentNullException(nameof(operationCoordinator));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _legacyStore = legacyStore ?? throw new ArgumentNullException(nameof(legacyStore));
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
    }

    public bool IsGeneratedProject(string projectFolder) => _sourceService.Validate(projectFolder).Present;

    public GeneratedGameplaySaveResult Save(
        string projectFolder,
        string slotName,
        UnifiedRuntimeSession session)
    {
        using var operation = _operationCoordinator.TryAcquire(projectFolder, GameProjectOperationKinds.GameplaySave);
        if (!operation.Acquired) return Failed(slotName, operation.Diagnostic);
        return Save(projectFolder, slotName, session, operation);
    }

    internal GeneratedGameplaySaveResult Save(
        string projectFolder,
        string slotName,
        UnifiedRuntimeSession session,
        GameProjectOperationLease operation)
    {
        var captured = _validator.CaptureProjectTruth(projectFolder, operation);
        if (!captured.Passed || captured.Truth is null)
            return Failed(slotName, captured.Diagnostics.FirstOrDefault() ?? "generated_save.project_not_ready");
        var truth = captured.Truth;
        var normalized = GeneratedGameplaySaveValidator.CloneSession(_serializer, session);
        if (string.IsNullOrWhiteSpace(normalized.GameplayState.PackageId))
            normalized.GameplayState.PackageId = truth.Identity.PackageId;
        if (string.IsNullOrWhiteSpace(normalized.GameplayState.CurrentMapId))
            normalized.GameplayState.CurrentMapId = normalized.MapState.CurrentMapId;
        var validation = _validator.ValidateSession(truth, normalized);
        if (!validation.Passed || validation.References is null)
            return Failed(slotName, validation.Diagnostics.FirstOrDefault() ?? "generated_save.session_invalid");

        var sessionJson = _serializer.Serialize(normalized);
        string? parent = null;
        var existing = _store.ReadSlot(projectFolder, slotName);
        if (existing.Passed)
        {
            parent = existing.Manifest!.CurrentRevisionSha256;
            var currentValidation = _validator.ValidateRevision(truth, existing.CurrentRevision!);
            if (currentValidation.Passed
                && currentValidation.Status == GeneratedGameplaySaveStatus.CURRENT
                && string.Equals(existing.CurrentRevision!.UnifiedRuntimeSessionJson, sessionJson,
                    StringComparison.Ordinal)
                && string.Equals(GeneratedGameplaySaveJson.Canonical(
                        existing.CurrentRevision.DefinitionFingerprints),
                    GeneratedGameplaySaveJson.Canonical(validation.References.Fingerprints),
                    StringComparison.Ordinal))
                return new GeneratedGameplaySaveResult
                {
                    Passed = true,
                    SlotName = existing.SlotName,
                    RevisionSha256 = existing.CurrentRevision.RevisionSha256,
                    Deduplicated = true,
                    Status = GeneratedGameplaySaveStatus.CURRENT,
                    Revision = existing.CurrentRevision,
                    Session = normalized
                };
        }
        else if (!existing.Diagnostics.Contains("generated_save.slot_missing", StringComparer.Ordinal))
            return Failed(slotName, existing.Diagnostics.FirstOrDefault() ?? "generated_save.slot_manifest_invalid");
        var source = truth.StrictGeneratedSource.Source!;
        var currentRegion = truth.GeneratedRegionMapBindings
            .SingleOrDefault(pair => pair.Value == normalized.MapState.CurrentMapId).Key ?? string.Empty;
        var revision = new GeneratedGameplaySaveRevision
        {
            ParentRevisionSha256 = parent,
            ProjectPackageId = truth.Identity.PackageId,
            ProjectIdentityFingerprint = truth.IdentityFingerprint,
            WorldId = truth.WorldId,
            SourceRecordSha256 = HashFile(Path.Combine(truth.ProjectFolder,
                SeededGeneratedProjectVocabulary.SourceRelativePath.Replace('/', Path.DirectorySeparatorChar))),
            SourceRequestSha256 = GameProjectSeedRegenerationDiffService.RequestSha256(source.GenerationRequest),
            PlanSha256 = source.PlanSha256,
            OverlaySha256 = source.GeneratedOverlaySha256,
            GeneratedBasePackageSha256 = source.GeneratedBasePackageSha256,
            PackageSha256 = truth.PackageSha256,
            CompositionPackageSha256 = truth.CompositionPackageSha256,
            QualifiedAuthoringFingerprint = truth.QualifiedAuthoringFingerprint,
            SelectedBuildHistoryFileName = truth.SelectedBuildHistoryFileName,
            SelectedBuildHistorySha256 = truth.SelectedBuildHistorySha256,
            UnifiedRuntimeSessionJson = sessionJson,
            UnifiedRuntimeSessionSha256 = GeneratedGameplaySaveJson.HashText(sessionJson),
            MapStateSha256 = GeneratedGameplaySaveJson.HashCanonical(normalized.MapState),
            GameplayStateSha256 = GeneratedGameplaySaveJson.HashCanonical(normalized.GameplayState),
            CurrentMapId = normalized.MapState.CurrentMapId,
            CurrentRegionSourceId = currentRegion,
            DefinitionFingerprints = validation.References.Fingerprints,
            GeneratedReferenceIds = validation.References.Fingerprints.Where(item => item.Generated)
                .Select(item => item.Id).Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal).ToList(),
            PortableFlagKeys = validation.References.PortableFlagKeys,
            SaveFacts = SaveFacts(truth, normalized)
        };
        revision = revision with { RevisionSha256 = GeneratedGameplaySaveJson.RevisionSha256(revision) };
        var write = _store.WriteRevision(projectFolder, slotName, revision);
        if (!write.Passed)
            return Failed(slotName, write.Diagnostics.FirstOrDefault() ?? "generated_save.store_failed");
        return new GeneratedGameplaySaveResult
        {
            Passed = true,
            SlotName = write.SlotName,
            RevisionSha256 = write.RevisionSha256,
            Deduplicated = write.Deduplicated,
            Status = GeneratedGameplaySaveStatus.CURRENT,
            Revision = revision,
            Session = normalized
        };
    }

    public GeneratedGameplaySaveResult Load(string projectFolder, string slotName)
    {
        using var operation = _operationCoordinator.TryAcquire(projectFolder, GameProjectOperationKinds.GameplayLoad);
        if (!operation.Acquired) return Failed(slotName, operation.Diagnostic);
        return Load(projectFolder, slotName, operation);
    }

    public GeneratedGameplaySaveResult LoadRevision(
        string projectFolder,
        string slotName,
        string revisionSha256)
    {
        using var operation = _operationCoordinator.TryAcquire(projectFolder, GameProjectOperationKinds.GameplayLoad);
        if (!operation.Acquired) return Failed(slotName, operation.Diagnostic);
        var captured = _validator.CaptureProjectTruth(projectFolder, operation);
        if (!captured.Passed || captured.Truth is null)
            return Failed(slotName, captured.Diagnostics.FirstOrDefault() ?? "generated_save.project_not_ready");
        var stored = _store.ReadRevision(projectFolder, slotName, revisionSha256);
        if (!stored.Passed || stored.CurrentRevision is null)
            return Failed(slotName, stored.Diagnostics.FirstOrDefault() ?? "generated_save.revision_invalid");
        return ExactRevisionResult(slotName, captured.Truth, stored.CurrentRevision);
    }

    internal GeneratedGameplaySaveResult Load(
        string projectFolder,
        string slotName,
        GameProjectOperationLease operation)
    {
        var captured = _validator.CaptureProjectTruth(projectFolder, operation);
        if (!captured.Passed || captured.Truth is null)
            return Failed(slotName, captured.Diagnostics.FirstOrDefault() ?? "generated_save.project_not_ready");
        var stored = _store.ReadSlot(projectFolder, slotName);
        if (!stored.Passed || stored.CurrentRevision is null)
            return Failed(slotName, stored.Diagnostics.FirstOrDefault() ?? "generated_save.slot_invalid");
        return ExactRevisionResult(slotName, captured.Truth, stored.CurrentRevision);
    }

    private GeneratedGameplaySaveResult ExactRevisionResult(
        string slotName,
        GeneratedGameplaySaveProjectTruth truth,
        GeneratedGameplaySaveRevision revision)
    {
        var validation = _validator.ValidateRevision(truth, revision);
        if (!validation.Passed || validation.Status != GeneratedGameplaySaveStatus.CURRENT
            || validation.Session is null)
            return new GeneratedGameplaySaveResult
            {
                SlotName = slotName,
                RevisionSha256 = revision.RevisionSha256,
                Status = validation.Status,
                Revision = revision,
                Diagnostics = validation.Diagnostics.Count > 0
                    ? validation.Diagnostics : ["generated_save.direct_stale_load_rejected"]
            };
        var exact = _serializer.Serialize(validation.Session);
        if (!string.Equals(exact, revision.UnifiedRuntimeSessionJson, StringComparison.Ordinal))
            return Failed(slotName, "generated_save.session_hash_mismatch");
        return new GeneratedGameplaySaveResult
        {
            Passed = true,
            SlotName = slotName,
            RevisionSha256 = revision.RevisionSha256,
            Status = GeneratedGameplaySaveStatus.CURRENT,
            Revision = revision,
            Session = validation.Session
        };
    }

    public GeneratedGameplaySaveListResult List(string projectFolder)
    {
        using var operation = _operationCoordinator.TryAcquire(projectFolder, GameProjectOperationKinds.GameplayLoad);
        if (!operation.Acquired) return new GeneratedGameplaySaveListResult { Diagnostics = [operation.Diagnostic] };
        return List(projectFolder, operation);
    }

    internal GeneratedGameplaySaveListResult List(
        string projectFolder,
        GameProjectOperationLease operation)
    {
        var captured = _validator.CaptureProjectTruth(projectFolder, operation);
        if (!captured.Passed || captured.Truth is null)
            return new GeneratedGameplaySaveListResult { Diagnostics = captured.Diagnostics };
        var truth = captured.Truth;
        var entries = new List<GeneratedGameplaySaveEntry>();
        var diagnostics = new List<string>();
        foreach (var slotName in _store.ListSlotNames(projectFolder))
        {
            var stored = _store.ReadSlot(projectFolder, slotName);
            if (!stored.Passed || stored.CurrentRevision is null)
            {
                entries.Add(new GeneratedGameplaySaveEntry
                {
                    SlotName = slotName,
                    Status = GeneratedGameplaySaveStatus.INVALID,
                    Diagnostics = stored.Diagnostics
                });
                diagnostics.AddRange(stored.Diagnostics);
                continue;
            }
            var validation = _validator.ValidateRevision(truth, stored.CurrentRevision);
            entries.Add(new GeneratedGameplaySaveEntry
            {
                SlotName = slotName,
                Status = validation.Status,
                CurrentRevisionSha256 = stored.CurrentRevision.RevisionSha256,
                RevisionCount = stored.Manifest?.RevisionSha256s.Count ?? 0,
                SavedWorldId = stored.CurrentRevision.WorldId,
                CurrentWorldId = truth.WorldId,
                SavedWorldTitle = Fact(stored.CurrentRevision, "Мир"),
                CurrentWorldTitle = truth.StrictGeneratedSource.RegeneratedPlan?.World.Regions.FirstOrDefault()?.Label
                                    ?? string.Empty,
                Migration = stored.CurrentRevision.Migration,
                Diagnostics = validation.Diagnostics
            });
            diagnostics.AddRange(validation.Diagnostics);
        }

        var legacy = _legacyStore.ListSnapshots(projectFolder);
        if (legacy.Success)
            entries.AddRange(legacy.SlotNames.Select(slot => new GeneratedGameplaySaveEntry
            {
                SlotName = slot,
                Status = GeneratedGameplaySaveStatus.LEGACY_RAW,
                LegacyRaw = true,
                Diagnostics = ["generated_save.legacy_raw_unverified"]
            }));
        else diagnostics.AddRange(legacy.Diagnostics.Select(item => item.Code));
        return new GeneratedGameplaySaveListResult
        {
            Passed = entries.All(entry => entry.Status != GeneratedGameplaySaveStatus.INVALID),
            Entries = entries.OrderBy(entry => entry.LegacyRaw).ThenBy(entry => entry.SlotName,
                StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).ToList()
        };
    }

    private static IReadOnlyList<GeneratedGameplaySaveFact> SaveFacts(
        GeneratedGameplaySaveProjectTruth truth,
        UnifiedRuntimeSession session)
    {
        var map = truth.ActualPackage.Game.Maps.Single(item => item.Id == session.MapState.CurrentMapId);
        return
        [
            new GeneratedGameplaySaveFact { Label = "Мир", Value =
                truth.StrictGeneratedSource.RegeneratedPlan?.World.Regions.FirstOrDefault()?.Label ?? map.Name },
            new GeneratedGameplaySaveFact { Label = "Карта", Value = map.Name },
            new GeneratedGameplaySaveFact { Label = "Состояние", Value = "сохранено" },
            new GeneratedGameplaySaveFact { Label = "Предметы", Value = session.GameplayState.Inventories
                .Sum(inventory => inventory.Stacks.Count).ToString(System.Globalization.CultureInfo.InvariantCulture) }
        ];
    }

    private static string Fact(GeneratedGameplaySaveRevision revision, string label) =>
        revision.SaveFacts.FirstOrDefault(fact => fact.Label == label)?.Value ?? string.Empty;

    private static GeneratedGameplaySaveResult Failed(string slotName, string diagnostic) => new()
    {
        SlotName = slotName,
        Diagnostics = [diagnostic]
    };

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
