using System.Security.Cryptography;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedGameplaySaveMigrationService
{
    private readonly IGameProjectOperationCoordinator _operationCoordinator;
    private readonly GeneratedGameplaySaveValidator _validator;
    private readonly GeneratedGameplaySaveStore _store;
    private readonly GeneratedGameplayDefinitionFingerprintService _fingerprints;
    private readonly IRuntimeStateSerializer _serializer;
    private readonly Dictionary<string, CachedPreview> _previews = new(StringComparer.Ordinal);

    public GeneratedGameplaySaveMigrationService(
        IGameProjectOperationCoordinator operationCoordinator,
        GeneratedGameplaySaveValidator validator,
        GeneratedGameplaySaveStore store,
        GeneratedGameplayDefinitionFingerprintService fingerprints,
        IRuntimeStateSerializer serializer)
    {
        _operationCoordinator = operationCoordinator ?? throw new ArgumentNullException(nameof(operationCoordinator));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _fingerprints = fingerprints ?? throw new ArgumentNullException(nameof(fingerprints));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public GeneratedGameplaySaveMigrationPreview Preview(string projectFolder, string slotName)
    {
        using var operation = _operationCoordinator.TryAcquire(
            projectFolder, GameProjectOperationKinds.GameplaySaveMigration);
        if (!operation.Acquired) return PreviewFailed(slotName, operation.Diagnostic);
        var capture = _validator.CaptureProjectTruth(projectFolder, operation);
        if (!capture.Passed || capture.Truth is null)
            return PreviewFailed(slotName,
                capture.Diagnostics.FirstOrDefault() ?? "generated_save.project_not_ready");
        var stored = _store.ReadSlot(projectFolder, slotName);
        if (!stored.Passed || stored.CurrentRevision is null)
            return PreviewFailed(slotName,
                stored.Diagnostics.FirstOrDefault() ?? "generated_save.slot_invalid");
        var validation = _validator.ValidateRevision(capture.Truth, stored.CurrentRevision);
        if (!validation.Passed || validation.Session is null
            || validation.Status is not GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED
                and not GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED)
            return PreviewFailed(slotName,
                validation.Diagnostics.FirstOrDefault() ?? "generated_save.migration_not_required",
                stored.CurrentRevision.RevisionSha256, validation.Status);
        var candidate = BuildCandidate(
            capture.Truth, stored.CurrentRevision, validation.Session, validation.Status);
        if (!candidate.Preview.Passed) return candidate.Preview with { SlotName = slotName };
        var preview = candidate.Preview with { SlotName = slotName };
        _previews[CacheKey(projectFolder, slotName, stored.CurrentRevision.RevisionSha256)] = new CachedPreview
        {
            ProjectFolder = Path.GetFullPath(projectFolder),
            Preview = preview,
            TargetTruthSha256 = TruthSha256(capture.Truth),
            CandidateSessionJson = _serializer.Serialize(candidate.Session),
            Migration = candidate.Migration
        };
        return preview;
    }

    public GeneratedGameplaySaveMigrationResult Apply(GeneratedGameplaySaveMigrationApplyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var operation = _operationCoordinator.TryAcquire(
            request.ProjectFolder, GameProjectOperationKinds.GameplaySaveMigration);
        if (!operation.Acquired) return ApplyFailed(request, operation.Diagnostic);
        var key = CacheKey(request.ProjectFolder, request.SlotName, request.SourceRevisionSha256);
        if (!_previews.TryGetValue(key, out var cached)
            || !string.Equals(cached.Preview.CandidateSessionSha256,
                request.CandidateSessionSha256, StringComparison.Ordinal))
            return ApplyFailed(request, "generated_save.migration_preview_mismatch");
        var capture = _validator.CaptureProjectTruth(request.ProjectFolder, operation);
        if (!capture.Passed || capture.Truth is null)
            return ApplyFailed(request,
                capture.Diagnostics.FirstOrDefault() ?? "generated_save.project_not_ready");
        if (!string.Equals(TruthSha256(capture.Truth), cached.TargetTruthSha256,
                StringComparison.Ordinal))
            return ApplyFailed(request, "generated_save.migration_target_changed");
        var stored = _store.ReadSlot(request.ProjectFolder, request.SlotName);
        if (!stored.Passed || stored.CurrentRevision is null
            || !string.Equals(stored.CurrentRevision.RevisionSha256,
                request.SourceRevisionSha256, StringComparison.Ordinal))
            return ApplyFailed(request, "generated_save.migration_source_changed");
        var sourceValidation = _validator.ValidateRevision(capture.Truth, stored.CurrentRevision);
        if (!sourceValidation.Passed || sourceValidation.Session is null
            || sourceValidation.Status != cached.Preview.SourceStatus)
            return ApplyFailed(request, "generated_save.migration_source_changed");
        var recomputed = BuildCandidate(capture.Truth, stored.CurrentRevision,
            sourceValidation.Session, sourceValidation.Status);
        var recomputedPreview = recomputed.Preview with { SlotName = request.SlotName };
        if (!recomputed.Preview.Passed
            || !string.Equals(recomputed.Preview.CandidateSessionSha256,
                request.CandidateSessionSha256, StringComparison.Ordinal)
            || !string.Equals(GeneratedGameplaySaveJson.Canonical(recomputedPreview),
                GeneratedGameplaySaveJson.Canonical(cached.Preview), StringComparison.Ordinal)
            || !string.Equals(_serializer.Serialize(recomputed.Session), cached.CandidateSessionJson,
                StringComparison.Ordinal)
            || !string.Equals(GeneratedGameplaySaveJson.Canonical(recomputed.Migration),
                GeneratedGameplaySaveJson.Canonical(cached.Migration), StringComparison.Ordinal))
            return ApplyFailed(request, "generated_save.migration_preview_mismatch");

        var candidateValidation = _validator.ValidateSession(capture.Truth, recomputed.Session);
        if (!candidateValidation.Passed || candidateValidation.References is null)
            return ApplyFailed(request,
                candidateValidation.Diagnostics.FirstOrDefault() ?? "generated_save.migration_candidate_invalid");
        var truth = capture.Truth;
        var source = truth.StrictGeneratedSource.Source!;
        var sessionJson = _serializer.Serialize(recomputed.Session);
        var currentRegion = truth.GeneratedRegionMapBindings
            .SingleOrDefault(pair => pair.Value == recomputed.Session.MapState.CurrentMapId).Key ?? string.Empty;
        var revision = new GeneratedGameplaySaveRevision
        {
            ParentRevisionSha256 = stored.CurrentRevision.RevisionSha256,
            Migration = recomputed.Migration,
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
            MapStateSha256 = GeneratedGameplaySaveJson.HashCanonical(recomputed.Session.MapState),
            GameplayStateSha256 = GeneratedGameplaySaveJson.HashCanonical(recomputed.Session.GameplayState),
            CurrentMapId = recomputed.Session.MapState.CurrentMapId,
            CurrentRegionSourceId = currentRegion,
            DefinitionFingerprints = candidateValidation.References.Fingerprints,
            GeneratedReferenceIds = candidateValidation.References.Fingerprints.Where(item => item.Generated)
                .Select(item => item.Id).Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal).ToList(),
            PortableFlagKeys = candidateValidation.References.PortableFlagKeys,
            SaveFacts = MigrationFacts(recomputed.Migration)
        };
        revision = revision with { RevisionSha256 = GeneratedGameplaySaveJson.RevisionSha256(revision) };
        var write = _store.WriteRevision(request.ProjectFolder, request.SlotName, revision);
        if (!write.Passed)
            return ApplyFailed(request,
                write.Diagnostics.FirstOrDefault() ?? "generated_save.migration_write_failed");
        var current = _validator.ValidateRevision(truth, revision);
        if (!current.Passed || current.Status != GeneratedGameplaySaveStatus.CURRENT
            || current.Session is null)
            return ApplyFailed(request,
                current.Diagnostics.FirstOrDefault() ?? "generated_save.migration_post_validation_failed");
        _previews.Remove(key);
        return new GeneratedGameplaySaveMigrationResult
        {
            Passed = true,
            SlotName = request.SlotName,
            SourceRevisionSha256 = request.SourceRevisionSha256,
            MigratedRevisionSha256 = revision.RevisionSha256,
            Preview = recomputedPreview,
            Revision = revision,
            Session = current.Session
        };
    }

    private CandidateBuild BuildCandidate(
        GeneratedGameplaySaveProjectTruth truth,
        GeneratedGameplaySaveRevision sourceRevision,
        UnifiedRuntimeSession sourceSession,
        GeneratedGameplaySaveStatus sourceStatus)
    {
        var session = GeneratedGameplaySaveValidator.CloneSession(_serializer, sourceSession);
        var preserved = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var dropped = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var preservedIds = new HashSet<string>(StringComparer.Ordinal);
        var droppedIds = new HashSet<string>(StringComparer.Ordinal);
        var reasons = new HashSet<string>(StringComparer.Ordinal);
        var sourceByKey = sourceRevision.DefinitionFingerprints.GroupBy(Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
        var targetByKey = truth.DefinitionFingerprintInventory.GroupBy(Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);

        bool Portable(string kind, string id, bool generatedAllowed = true)
        {
            var key = kind + "\n" + id;
            return sourceByKey.TryGetValue(key, out var source)
                   && targetByKey.TryGetValue(key, out var target)
                   && (generatedAllowed || !source.Generated)
                   && string.Equals(source.CanonicalSha256, target.CanonicalSha256, StringComparison.Ordinal);
        }

        void Preserved(string kind, string id)
        {
            preserved[kind] = preserved.GetValueOrDefault(kind) + 1;
            preservedIds.Add(kind + ":" + id);
        }

        void Dropped(string kind, string id, string reason)
        {
            dropped[kind] = dropped.GetValueOrDefault(kind) + 1;
            droppedIds.Add(kind + ":" + id);
            reasons.Add(kind + ":" + id + ":" + reason);
        }

        bool MapReset;
        if (sourceStatus == GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED
            || !Portable("map", session.MapState.CurrentMapId)
            || !GeneratedGameplaySaveValidator.ValidPosition(truth.ActualPackage,
                session.MapState.CurrentMapId, session.MapState.PlayerPosition.X,
                session.MapState.PlayerPosition.Y))
        {
            var start = truth.ActualPackage.Game.Maps.Single(map => map.Id == truth.GeneratedStartMapId);
            if (!string.IsNullOrWhiteSpace(session.MapState.CurrentMapId))
                Dropped("map", session.MapState.CurrentMapId, "map_reset_to_current_generated_start");
            session.MapState.CurrentMapId = start.Id;
            session.MapState.PlayerPosition = new LLMGameCreator.Domain.Definitions.Position2D(
                start.StartPosition.X, start.StartPosition.Y);
            session.MapState.Mode = "map";
            session.GameplayState.CurrentMapId = start.Id;
            MapReset = true;
        }
        else
        {
            Preserved("map", session.MapState.CurrentMapId);
            session.GameplayState.CurrentMapId = session.MapState.CurrentMapId;
            MapReset = false;
        }

        foreach (var inventory in session.GameplayState.Inventories)
        {
            inventory.Stacks = inventory.Stacks.Where(stack =>
            {
                if (Portable("item", stack.ItemId))
                {
                    Preserved("item", stack.ItemId);
                    stack.Metadata = FilterDictionary(stack.Metadata, sourceRevision, Portable, Dropped);
                    return true;
                }
                Dropped("item", stack.ItemId, "definition_missing_or_changed");
                return false;
            }).ToList();
            inventory.Metadata = FilterDictionary(inventory.Metadata, sourceRevision, Portable, Dropped);
        }
        foreach (var equipment in session.GameplayState.Equipment)
        {
            equipment.Slots = equipment.Slots.Where(slot =>
            {
                var slotPortable = !sourceByKey.ContainsKey("equipment_slot\n" + slot.SlotId)
                                   || Portable("equipment_slot", slot.SlotId);
                var itemPortable = string.IsNullOrWhiteSpace(slot.ItemId)
                                   || Portable("item", slot.ItemId);
                if (slotPortable && itemPortable)
                {
                    if (!string.IsNullOrWhiteSpace(slot.ItemId)) Preserved("item", slot.ItemId);
                    slot.Metadata = FilterDictionary(slot.Metadata, sourceRevision, Portable, Dropped);
                    return true;
                }
                Dropped("equipment", slot.SlotId,
                    slotPortable ? "item_definition_missing_or_changed" : "slot_definition_missing_or_changed");
                return false;
            }).ToList();
            equipment.Metadata = FilterDictionary(equipment.Metadata, sourceRevision, Portable, Dropped);
        }
        session.GameplayState.Resources = session.GameplayState.Resources.Where(item =>
            Keep("resource", item.ResourceId, Portable, Preserved, Dropped)).ToList();
        session.GameplayState.Stats = session.GameplayState.Stats.Where(item =>
            Keep("stat", item.StatId, Portable, Preserved, Dropped)).ToList();
        session.GameplayState.Progressions = session.GameplayState.Progressions.Where(item =>
        {
            var keep = Keep("progression", item.ProgressionId, Portable, Preserved, Dropped);
            if (keep) item.Metadata = FilterDictionary(item.Metadata, sourceRevision, Portable, Dropped);
            return keep;
        }).ToList();
        session.GameplayState.Factions = session.GameplayState.Factions.Where(item =>
        {
            var keep = Keep("faction", item.FactionId, Portable, Preserved, Dropped);
            if (keep) item.Metadata = FilterDictionary(item.Metadata, sourceRevision, Portable, Dropped);
            return keep;
        }).ToList();

        session.GameplayState.QuestStates = session.GameplayState.QuestStates
            .Where(pair => KeepQuest(pair.Key, sourceByKey, Portable, Preserved, Dropped))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        session.GameplayState.Quests = session.GameplayState.Quests.Where(quest =>
        {
            var keep = KeepQuest(quest.QuestId, sourceByKey, Portable, Preserved, Dropped);
            if (keep)
            {
                quest.Metadata = FilterDictionary(quest.Metadata, sourceRevision, Portable, Dropped);
                foreach (var objective in quest.Objectives)
                    objective.Metadata = FilterDictionary(objective.Metadata, sourceRevision, Portable, Dropped);
            }
            return keep;
        }).ToList();
        session.GameplayState.Statuses = session.GameplayState.Statuses.Where(status =>
        {
            if (status.RemainingTicks is not null)
            {
                Dropped("status", status.StatusId, "transient_status_reset");
                return false;
            }
            var targetPortable = GeneratedGameplayDefinitionFingerprintService.IsStructural(status.TargetId)
                                 || sourceRevision.DefinitionFingerprints.Where(item => item.Id == status.TargetId)
                                     .All(item => Portable(item.Kind, item.Id));
            if (Portable("status", status.StatusId) && targetPortable)
            {
                Preserved("status", status.StatusId);
                status.Metadata = FilterDictionary(status.Metadata, sourceRevision, Portable, Dropped);
                return true;
            }
            Dropped("status", status.StatusId, "definition_or_target_missing_or_changed");
            return false;
        }).ToList();

        if (session.GameplayState.ActiveEncounter is { } encounter)
            Dropped("encounter", encounter.EncounterId, "transient_encounter_reset");
        if (session.GameplayState.ActiveDialogue is { } dialogue)
            Dropped("dialogue", dialogue.DialogueId, "transient_dialogue_reset");
        if (session.GameplayState.Tick != 0) Dropped("tick", "runtime_tick", "transient_tick_reset");
        if (session.MapEvents.Count > 0) Dropped("map_event", "events", "transient_events_reset");
        if (session.GameplayEvents.Count > 0)
            Dropped("gameplay_event", "events", "transient_events_reset");
        session.GameplayState.ActiveEncounter = null;
        session.GameplayState.ActiveDialogue = null;
        session.GameplayState.Tick = 0;
        session.MapEvents = [];
        session.GameplayEvents = [];
        session.MapState.Flags = FilterDictionary(
            session.MapState.Flags, sourceRevision, Portable, Dropped);
        session.GameplayState.Flags = session.GameplayState.Flags.Where(flag =>
            PairPortable(flag.Id, flag.Value, sourceRevision, Portable, Dropped)).ToList();
        session.GameplayState.Metadata = FilterDictionary(
            session.GameplayState.Metadata, sourceRevision, Portable, Dropped);
        session.Metadata = FilterDictionary(session.Metadata, sourceRevision, Portable, Dropped);
        session.GameplayState.PackageId = truth.Identity.PackageId;

        var candidateJson = _serializer.Serialize(session);
        var migration = new GeneratedGameplaySaveMigration
        {
            SourceRevisionSha256 = sourceRevision.RevisionSha256,
            SourceWorldId = sourceRevision.WorldId,
            TargetWorldId = truth.WorldId,
            SourcePackageSha256 = sourceRevision.PackageSha256,
            TargetPackageSha256 = truth.PackageSha256,
            MapReset = MapReset,
            PreservedCounts = preserved,
            DroppedCounts = dropped,
            PreservedDefinitionIds = preservedIds.OrderBy(value => value, StringComparer.Ordinal).ToList(),
            DroppedDefinitionIds = droppedIds.OrderBy(value => value, StringComparer.Ordinal).ToList(),
            DroppedReasons = reasons.OrderBy(value => value, StringComparer.Ordinal).ToList()
        };
        var validation = _validator.ValidateSession(truth, session);
        var preview = new GeneratedGameplaySaveMigrationPreview
        {
            SourceRevisionSha256 = sourceRevision.RevisionSha256,
            SourceStatus = sourceStatus,
            SourceWorldId = sourceRevision.WorldId,
            TargetWorldId = truth.WorldId,
            SourcePackageSha256 = sourceRevision.PackageSha256,
            TargetPackageSha256 = truth.PackageSha256,
            MapReset = MapReset,
            PreservedCountsByKind = preserved,
            DroppedCountsByKind = dropped,
            PreservedDefinitionIds = migration.PreservedDefinitionIds,
            DroppedDefinitionIds = migration.DroppedDefinitionIds,
            DroppedReasons = migration.DroppedReasons,
            CandidateSessionSha256 = GeneratedGameplaySaveJson.HashText(candidateJson),
            CandidateMapStateSha256 = GeneratedGameplaySaveJson.HashCanonical(session.MapState),
            CandidateGameplayStateSha256 = GeneratedGameplaySaveJson.HashCanonical(session.GameplayState),
            Diagnostics = validation.Diagnostics,
            Passed = validation.Passed
        };
        return new CandidateBuild(session, migration, preview);
    }

    private static bool Keep(
        string kind,
        string id,
        Func<string, string, bool, bool> portable,
        Action<string, string> preserved,
        Action<string, string, string> dropped)
    {
        if (portable(kind, id, true))
        {
            preserved(kind, id);
            return true;
        }
        dropped(kind, id, "definition_missing_or_changed");
        return false;
    }

    private static bool KeepQuest(
        string id,
        IReadOnlyDictionary<string, GeneratedGameplayDefinitionFingerprint> sourceByKey,
        Func<string, string, bool, bool> portable,
        Action<string, string> preserved,
        Action<string, string, string> dropped)
    {
        var generated = sourceByKey.TryGetValue("quest\n" + id, out var source) && source.Generated;
        if (!generated && portable("quest", id, false))
        {
            preserved("quest", id);
            return true;
        }
        dropped("quest", id, generated ? "generated_world_bound_quest" : "definition_missing_or_changed");
        return false;
    }

    private static Dictionary<string, string> FilterDictionary(
        IReadOnlyDictionary<string, string> source,
        GeneratedGameplaySaveRevision revision,
        Func<string, string, bool, bool> portable,
        Action<string, string, string> dropped)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in source)
        {
            if (PairPortable(pair.Key, pair.Value, revision, portable, dropped))
                result[pair.Key] = pair.Value;
        }
        return result;
    }

    private static bool PairPortable(
        string key,
        string value,
        GeneratedGameplaySaveRevision revision,
        Func<string, string, bool, bool> portable,
        Action<string, string, string> dropped)
    {
        foreach (var scalar in new[] { key, value })
        {
            var definitions = revision.DefinitionFingerprints.Where(item => item.Id == scalar).ToList();
            if (definitions.Count > 0 && definitions.Any(item => !portable(item.Kind, item.Id, true)))
            {
                dropped("metadata", key, "references_dropped_definition:" + scalar);
                return false;
            }
            if (revision.GeneratedReferenceIds.Contains(scalar, StringComparer.Ordinal)
                && definitions.Count == 0)
            {
                dropped("metadata", key, "references_unresolved_generated_definition:" + scalar);
                return false;
            }
        }
        return true;
    }

    private static IReadOnlyList<GeneratedGameplaySaveFact> MigrationFacts(
        GeneratedGameplaySaveMigration migration) =>
    [
        new GeneratedGameplaySaveFact { Label = "Игровое сохранение", Value = "перенесено" },
        new GeneratedGameplaySaveFact { Label = "Мир сохранения", Value = "текущий" },
        new GeneratedGameplaySaveFact { Label = "Позиция", Value = migration.MapReset
            ? "сброшена на старт" : "сохранена" },
        new GeneratedGameplaySaveFact { Label = "Сохранено данных", Value = migration.PreservedCounts.Values.Sum()
            .ToString(System.Globalization.CultureInfo.InvariantCulture) },
        new GeneratedGameplaySaveFact { Label = "Сброшено данных", Value = migration.DroppedCounts.Values.Sum()
            .ToString(System.Globalization.CultureInfo.InvariantCulture) },
        new GeneratedGameplaySaveFact { Label = "Проверка после загрузки", Value = "пройдена" }
    ];

    private static string TruthSha256(GeneratedGameplaySaveProjectTruth truth) =>
        GeneratedGameplaySaveJson.HashCanonical(new
        {
            truth.IdentityFingerprint,
            truth.WorldId,
            truth.PackageSha256,
            truth.CompositionPackageSha256,
            truth.QualifiedAuthoringFingerprint,
            truth.SelectedBuildHistoryFileName,
            truth.SelectedBuildHistorySha256,
            truth.GeneratedStartMapId,
            truth.DefinitionFingerprintInventory
        });

    private static string CacheKey(string project, string slot, string revision) =>
        Path.GetFullPath(project).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        + "\n" + slot.Trim() + "\n" + revision;

    private static string Key(GeneratedGameplayDefinitionFingerprint fingerprint) =>
        fingerprint.Kind + "\n" + fingerprint.Id;

    private static GeneratedGameplaySaveMigrationPreview PreviewFailed(
        string slot,
        string diagnostic,
        string revision = "",
        GeneratedGameplaySaveStatus status = GeneratedGameplaySaveStatus.INVALID) => new()
    {
        SlotName = slot,
        SourceRevisionSha256 = revision,
        SourceStatus = status,
        Diagnostics = [diagnostic]
    };

    private static GeneratedGameplaySaveMigrationResult ApplyFailed(
        GeneratedGameplaySaveMigrationApplyRequest request,
        string diagnostic) => new()
    {
        SlotName = request.SlotName,
        SourceRevisionSha256 = request.SourceRevisionSha256,
        Diagnostics = [diagnostic]
    };

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private sealed record CandidateBuild(
        UnifiedRuntimeSession Session,
        GeneratedGameplaySaveMigration Migration,
        GeneratedGameplaySaveMigrationPreview Preview);

    private sealed record CachedPreview
    {
        public string ProjectFolder { get; init; } = string.Empty;
        public GeneratedGameplaySaveMigrationPreview Preview { get; init; } = new();
        public string TargetTruthSha256 { get; init; } = string.Empty;
        public string CandidateSessionJson { get; init; } = string.Empty;
        public GeneratedGameplaySaveMigration Migration { get; init; } = new();
    }
}
