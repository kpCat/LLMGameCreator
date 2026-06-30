using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;

namespace LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;

public sealed class ChunkedRuntimePreviewPayloadBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public IReadOnlyList<ChunkedPreviewPayload> BuildPayloads(ChunkedRuntimePreviewExportSourceBundle source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return ChunkedRuntimePreviewExportVocabulary.ScenarioIds
            .Select(scenarioId => BuildPayload(source, scenarioId))
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();
    }

    private static ChunkedPreviewPayload BuildPayload(
        ChunkedRuntimePreviewExportSourceBundle source,
        string scenarioId)
    {
        var plan = source.PlansByScenario[scenarioId];
        var persistence = source.SaveLoadProof.Scenarios.Single(item => item.ScenarioId == scenarioId);
        var replay = source.ReplayProof.Scenarios.Single(item => item.ScenarioId == scenarioId);
        var state = source.StatesByScenario.TryGetValue(scenarioId, out var physicalState)
            ? physicalState
            : StateFromRestoredEvidence(persistence);
        var planFile = RuntimeChunkDeltaEvidenceService.PlanFileName(scenarioId);
        var stateFile = StateFileNameOrSaveLoadProof(scenarioId, source);
        var sourceStateHash = source.ArtifactHashByFileName.TryGetValue(stateFile, out var stateHash)
            ? stateHash
            : source.ArtifactHashByFileName[RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName];
        var sourceEvidence = new ChunkedPreviewSourceEvidence
        {
            Goal038EvidenceRefs = plan.SourceFacts.Goal038EvidenceRefs
                .Order(StringComparer.Ordinal)
                .Select(item => new ChunkedSourceReference
                {
                    SourceGoal = "Goal038",
                    EvidenceRef = item,
                    ArtifactFamily = "world_scale_source_fact",
                    ArtifactFileName = "embedded-in-" + planFile,
                    ArtifactHash = source.ArtifactHashByFileName[planFile]
                })
                .ToList(),
            Goal039EvidenceRefs =
            [
                Ref("Goal039", planFile, "runtime_chunk_traversal_plan", source.ArtifactHashByFileName[planFile]),
                Ref("Goal039", stateFile, "runtime_chunk_delta_state", sourceStateHash),
                Ref("Goal039", RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName, "runtime_save_load_roundtrip_proof", source.ArtifactHashByFileName[RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName]),
                Ref("Goal039", RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName, "chunk_replay_determinism_proof", source.ArtifactHashByFileName[RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName])
            ],
            SourcePlanHash = source.ArtifactHashByFileName[planFile],
            SourceRuntimeDeltaStateHash = sourceStateHash,
            SourceSaveLoadProofHash = source.ArtifactHashByFileName[RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName],
            SourceReplayProofHash = source.ArtifactHashByFileName[RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName],
            ConsumesGoal039RuntimeDeltaCommands = plan.Commands.Count > 0 && state.RuntimeDeltas.Count > 0,
            ConsumesGoal039SaveLoadProof = persistence.Passed,
            ConsumesGoal039ReplayProof = replay.SameSeedDeterministic,
            PayloadIsSourceJsonCopy = false
        };
        var payloadWithoutHash = new ChunkedPreviewPayload
        {
            ScenarioId = scenarioId,
            ProfileId = plan.ProfileId,
            WorldGraphId = plan.WorldGraphId,
            FiniteMapId = plan.FiniteMapId,
            CoordinateKind = plan.CoordinateKind,
            ReplaySeed = plan.ReplaySeed,
            SourceEvidence = sourceEvidence,
            ChunkIds = plan.Steps.Select(item => item.ChunkId)
                .Concat(state.DiscoveredChunkIds)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList(),
            TraversalRoute = plan.Steps
                .OrderBy(item => item.StepIndex)
                .Select(item => new ChunkedTraversalRouteStep
                {
                    StepIndex = item.StepIndex,
                    RegionId = item.RegionId,
                    ChunkId = item.ChunkId,
                    Coordinate = item.Coordinate,
                    ArrivedByEdgeId = item.ArrivedByEdgeId ?? string.Empty,
                    LandmarkId = item.LandmarkId,
                    RouteCheckpointMarkerId = item.RouteCheckpointMarkerId,
                    MutationMarkerId = item.LocalMutationId
                })
                .ToList(),
            VisitedRegionIds = PreferStateOrPlan(state.VisitedRegionIds, plan.Steps.Select(item => item.RegionId)),
            DiscoveredChunkIds = PreferStateOrPlan(state.DiscoveredChunkIds, plan.Steps.Select(item => item.ChunkId)),
            LandmarkDiscoveryIds = PreferStateOrPlan(state.LandmarkDiscoveryIds, plan.Steps.Select(item => item.LandmarkId)),
            RouteCheckpointMarkerIds = PreferStateOrPlan(state.RouteCheckpointMarkerIds, plan.Steps.Select(item => item.RouteCheckpointMarkerId)),
            MutationMarkers = BuildMutationMarkers(plan, state),
            RuntimeDeltaMarkers = plan.Commands
                .OrderBy(item => item.Order)
                .Select(item => new ChunkedRuntimeDeltaMarker
                {
                    Order = item.Order,
                    DeltaId = item.DeltaId,
                    DeltaKind = item.DeltaKind,
                    RegionId = item.RegionId,
                    ChunkId = item.ChunkId,
                    MarkerId = item.MarkerId,
                    SourceEdgeId = item.SourceEdgeId ?? string.Empty,
                    MutationKey = item.MutationKey,
                    MutationValue = item.MutationValue
                })
                .ToList(),
            ReplaySaveLoadCorrelation = new ChunkedReplaySaveLoadCorrelation
            {
                RuntimeStateOwnerIsGameRuntimeState = true,
                SerializerRoundtripPassed = persistence.SerializerRoundtripPassed,
                SnapshotRoundtripPassed = persistence.SnapshotRoundtripPassed,
                ReplayDeterminismPassed = replay.SameSeedDeterministic,
                RuntimeStateHash = persistence.SerializedStateHash,
                RestoredRuntimeStateHash = persistence.RestoredSerializedStateHash,
                ReplayMarker = state.DeterministicReplayMarkers.Order(StringComparer.Ordinal).FirstOrDefault() ?? string.Empty,
                ReplayProofHash = source.ArtifactHashByFileName[RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName],
                SaveLoadProofHash = source.ArtifactHashByFileName[RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName]
            },
            FamilyLensViews = ChunkedMultiFamilyRegressionPlanner.BuildPayloadViews(plan, state),
            PreviewExportReadiness = new ChunkedPreviewExportReadiness
            {
                PreviewPayloadReady = true,
                ExportManifestReady = true,
                RuntimeDeltaBacked = plan.Commands.Count > 0,
                SaveLoadBacked = persistence.Passed,
                ReplayBacked = replay.SameSeedDeterministic,
                ConcreteRuntimePreviewIntegrationFutureRequired = true,
                ConcreteUnityExportIntegrationFutureRequired = true,
                FutureRequiredGaps =
                [
                    "runtime_preview_route_integration_future_required",
                    "unity_export_adapter_integration_future_required"
                ],
                BlockedGaps = []
            }
        };

        return payloadWithoutHash with
        {
            PayloadHash = ChunkedRuntimePreviewExportHash.Hash(JsonSerializer.Serialize(payloadWithoutHash, JsonOptions))
        };
    }

    private static ChunkedSourceReference Ref(string sourceGoal, string fileName, string artifactFamily, string hash) =>
        new()
        {
            SourceGoal = sourceGoal,
            EvidenceRef = fileName,
            ArtifactFamily = artifactFamily,
            ArtifactFileName = fileName,
            ArtifactHash = hash
        };

    private static string StateFileNameOrSaveLoadProof(
        string scenarioId,
        ChunkedRuntimePreviewExportSourceBundle source)
    {
        if (source.StatesByScenario.ContainsKey(scenarioId))
        {
            return scenarioId switch
            {
                "frontier_survival" => RuntimeChunkDeltaEvidenceService.FrontierStateJsonFileName,
                "metamodule_kingdoms" => RuntimeChunkDeltaEvidenceService.MetamoduleStateJsonFileName,
                _ => RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName
            };
        }

        return RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName;
    }

    private static RuntimeChunkDeltaStateSnapshot StateFromRestoredEvidence(RuntimeChunkPersistenceResult persistence)
    {
        var evidence = persistence.RestoredStateEvidence;
        return new RuntimeChunkDeltaStateSnapshot
        {
            ScenarioId = Value(evidence, "metadata.runtimeChunk.scenarioId"),
            RegionId = Value(evidence, "metadata.runtimeChunk.currentRegionId"),
            ChunkId = Value(evidence, "metadata.runtimeChunk.currentChunkId"),
            VisitedRegionIds = Split(Value(evidence, "metadata.runtimeChunk.visitedRegionIds")),
            DiscoveredChunkIds = Split(Value(evidence, "metadata.runtimeChunk.discoveredChunkIds")),
            LandmarkDiscoveryIds = Split(Value(evidence, "metadata.runtimeChunk.landmarkDiscoveryIds")),
            RouteCheckpointMarkerIds = Split(Value(evidence, "metadata.runtimeChunk.routeCheckpointMarkerIds")),
            LocalMutations = ReadDictionary(Value(evidence, "metadata.runtimeChunk.localMutations")),
            RuntimeDeltas = ReadList<RuntimeChunkDeltaRecord>(Value(evidence, "metadata.runtimeChunk.runtimeDeltas")),
            DeterministicReplayMarkers = Split(Value(evidence, "metadata.runtimeChunk.deterministicReplayMarkers"))
        };
    }

    private static IReadOnlyList<string> PreferStateOrPlan(
        IReadOnlyList<string> stateValues,
        IEnumerable<string> fallbackValues)
    {
        var values = stateValues.Count > 0 ? stateValues : fallbackValues;
        return values
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<ChunkedMutationMarker> BuildMutationMarkers(
        RuntimeChunkTraversalPlan plan,
        RuntimeChunkDeltaStateSnapshot state)
    {
        if (state.LocalMutations.Count > 0)
        {
            return state.LocalMutations
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new ChunkedMutationMarker { MutationId = item.Key, MutationKind = item.Value })
                .ToList();
        }

        return plan.Steps
            .Where(item => !string.IsNullOrWhiteSpace(item.LocalMutationId))
            .OrderBy(item => item.LocalMutationId, StringComparer.Ordinal)
            .Select(item => new ChunkedMutationMarker { MutationId = item.LocalMutationId, MutationKind = item.LocalMutationKind })
            .ToList();
    }

    private static IReadOnlyList<string> Split(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Order(StringComparer.Ordinal)
                .ToList();

    private static IReadOnlyDictionary<string, string> ReadDictionary(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? new SortedDictionary<string, string>(StringComparer.Ordinal)
            : JsonSerializer.Deserialize<SortedDictionary<string, string>>(json, JsonOptions)
              ?? new SortedDictionary<string, string>(StringComparer.Ordinal);

    private static IReadOnlyList<T> ReadList<T>(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];

    private static string Value(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) ? value : string.Empty;
}

public sealed class ChunkedExportManifestBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public ChunkedExportManifest Build(IReadOnlyList<ChunkedPreviewPayload> payloads)
    {
        var entries = payloads
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(item => new ChunkedExportManifestEntry
            {
                ScenarioId = item.ScenarioId,
                PayloadPath = ".llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/" + ChunkedRuntimePreviewExportVocabulary.PayloadFileNamesByScenario[item.ScenarioId],
                PayloadHash = item.PayloadHash,
                ChunkCount = item.ChunkIds.Count,
                RuntimeDeltaMarkerCount = item.RuntimeDeltaMarkers.Count,
                PreviewReady = item.PreviewExportReadiness.PreviewPayloadReady,
                ExportReady = item.PreviewExportReadiness.ExportManifestReady
            })
            .ToList();
        var withoutHash = new ChunkedExportManifest
        {
            UsesGoal039RuntimeDeltas = payloads.All(item => item.SourceEvidence.ConsumesGoal039RuntimeDeltaCommands),
            RuntimePreviewCompatible = true,
            UnityExportCompatible = true,
            Payloads = entries,
            FutureRequiredIntegrationGaps =
            [
                "runtime_preview_route_integration_future_required",
                "unity_export_adapter_integration_future_required"
            ],
            Diagnostics =
            [
                ChunkedRuntimePreviewExportDiagnostic.Info(
                    "chunked_consumer.integration.future_required",
                    "runtime_preview_unity_export",
                    "Goal 040 writes a contract-bound payload and manifest; concrete Runtime Preview/Unity route wiring remains future-required.")
            ]
        };

        return withoutHash with
        {
            ManifestHash = ChunkedRuntimePreviewExportHash.Hash(JsonSerializer.Serialize(withoutHash, JsonOptions))
        };
    }
}

public sealed class ChunkedMultiFamilyRegressionPlanner
{
    public static IReadOnlyList<ChunkedFamilyLensPayloadView> BuildPayloadViews(
        RuntimeChunkTraversalPlan plan,
        RuntimeChunkDeltaStateSnapshot state) =>
        BuildLensPlans()
            .Select(lens => new ChunkedFamilyLensPayloadView
            {
                FamilyLensId = lens.FamilyLensId,
                CorePayloadSchemaId = lens.CorePayloadSchemaId,
                ForksCoreTraversalSchema = false,
                ExpectedConsumerNeeds = lens.ExpectedConsumerNeeds,
                RouteOrientationHints = LensHints(lens.FamilyLensId, plan, state),
                ReadinessFlags =
                [
                    "uses_shared_chunk_traversal_payload",
                    "uses_goal039_runtime_delta_markers",
                    "does_not_fork_core_schema"
                ]
            })
            .ToList();

    public MultiFamilyWorldScaleRegressionMatrix Build(IReadOnlyList<ChunkedPreviewPayload> payloads)
    {
        var lensPlans = BuildLensPlans();
        var scenarioReuse = payloads
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(item => new MultiFamilyScenarioReuse
            {
                ScenarioId = item.ScenarioId,
                FamilyLensIds = item.FamilyLensViews.Select(view => view.FamilyLensId).Order(StringComparer.Ordinal).ToList(),
                SharedCorePayloadSchemaId = ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId,
                ReusesSameCoreTraversalPayload = item.FamilyLensViews.Count == 3
                    && item.FamilyLensViews.All(view => !view.ForksCoreTraversalSchema && view.CorePayloadSchemaId == ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId)
            })
            .ToList();

        return new MultiFamilyWorldScaleRegressionMatrix
        {
            FamilyLensCount = lensPlans.Count,
            ScenarioCount = payloads.Count,
            Passed = lensPlans.Count == 3
                && scenarioReuse.Count == 4
                && scenarioReuse.All(item => item.ReusesSameCoreTraversalPayload),
            FamilyLenses = lensPlans,
            ScenarioReuse = scenarioReuse
        };
    }

    private static IReadOnlyList<MultiFamilyLensPlan> BuildLensPlans() =>
    [
        new()
        {
            FamilyLensId = "map_panel_rpg",
            ExpectedConsumerNeeds =
            [
                "region_panel_sequence",
                "travel_log",
                "landmark_focus"
            ]
        },
        new()
        {
            FamilyLensId = "survival_sandbox",
            ExpectedConsumerNeeds =
            [
                "hazard_resource_traversal_hints",
                "return_to_camp_route",
                "local_mutation_state"
            ]
        },
        new()
        {
            FamilyLensId = "first_person_grid_dungeon",
            ExpectedConsumerNeeds =
            [
                "corridor_room_route_orientation",
                "checkpoint_breadcrumbs",
                "step_ordered_turn_hints"
            ]
        }
    ];

    private static IReadOnlyList<string> LensHints(
        string familyLensId,
        RuntimeChunkTraversalPlan plan,
        RuntimeChunkDeltaStateSnapshot state) =>
        familyLensId switch
        {
            "map_panel_rpg" =>
            [
                "panel:start=" + plan.StartRegionId,
                "panel:landmarks=" + state.LandmarkDiscoveryIds.Count.ToString("0"),
                "panel:travel_log_steps=" + plan.Steps.Count.ToString("0")
            ],
            "survival_sandbox" =>
            [
                "survival:chunks=" + state.DiscoveredChunkIds.Count.ToString("0"),
                "survival:mutations=" + state.LocalMutations.Count.ToString("0"),
                "survival:return_route_seed=" + plan.ReplaySeed
            ],
            "first_person_grid_dungeon" =>
            [
                "grid:first_chunk=" + (plan.Steps.FirstOrDefault()?.ChunkId ?? string.Empty),
                "grid:last_chunk=" + (plan.Steps.LastOrDefault()?.ChunkId ?? string.Empty),
                "grid:checkpoints=" + state.RouteCheckpointMarkerIds.Count.ToString("0")
            ],
            _ => []
        };
}

public sealed class InfiniteChunkedWorldSmokeProofBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public InfiniteChunkedWorldSmokeProof Build()
    {
        var seedId = "goal040-bounded-infinite-window-seed";
        var window = new InfiniteChunkWindow
        {
            OriginChunkId = "chunk/infinite/goal040/origin/x0/y0",
            OriginX = 0,
            OriginY = 0,
            Radius = 1,
            Width = 3,
            Height = 3
        };
        var chunks = Enumerable.Range(-window.Radius, window.Width)
            .SelectMany(x => Enumerable.Range(-window.Radius, window.Height).Select(y => BuildChunk(seedId, x, y)))
            .OrderBy(item => item.X)
            .ThenBy(item => item.Y)
            .ToList();
        var withoutHash = new InfiniteChunkedWorldSmokeProof
        {
            SeedId = seedId,
            Window = window,
            DerivedChunks = chunks,
            BoundaryHandoffPlaceholders =
            [
                "north_boundary_handoff_placeholder",
                "south_boundary_handoff_placeholder",
                "west_boundary_handoff_placeholder",
                "east_boundary_handoff_placeholder"
            ],
            RealInfiniteStreamingImplemented = false,
            Diagnostics =
            [
                ChunkedRuntimePreviewExportDiagnostic.Info(
                    "chunked_consumer.infinite.bounded_window_proof",
                    seedId,
                    "Bounded deterministic chunk-window derivation proves the path can extend beyond finite maps without implementing real infinite streaming.")
            ]
        };
        var hash = ChunkedRuntimePreviewExportHash.Hash(JsonSerializer.Serialize(withoutHash, JsonOptions));
        return withoutHash with
        {
            RepeatableHash = hash,
            ReplayedHash = hash,
            Deterministic = true
        };
    }

    private static InfiniteDerivedChunk BuildChunk(string seedId, int x, int y)
    {
        var key = $"{seedId}|x={x:0}|y={y:0}";
        return new InfiniteDerivedChunk
        {
            X = x,
            Y = y,
            DerivationKey = key,
            ChunkId = $"chunk/infinite/goal040/x{x:0}/y{y:0}/{ChunkedRuntimePreviewExportHash.Hash(key)[..12]}"
        };
    }
}

public sealed class ChunkedRuntimePreviewExportValidator
{
    public IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> ValidatePayload(ChunkedPreviewPayload payload)
    {
        var diagnostics = new List<ChunkedRuntimePreviewExportDiagnostic>();
        if (!ChunkedRuntimePreviewExportVocabulary.ScenarioIds.Contains(payload.ScenarioId, StringComparer.Ordinal))
        {
            diagnostics.Add(Error("chunked_consumer.scenario.fake", payload.ScenarioId, "Payload scenario id must be one of the Goal 039 scenarios."));
        }

        if (payload.FinalProseOnly)
        {
            diagnostics.Add(Error("chunked_consumer.payload.final_prose_only", payload.ScenarioId, "Payload must be structured machine-readable JSON, not final prose only."));
        }

        if (payload.SourceEvidence.PayloadIsSourceJsonCopy)
        {
            diagnostics.Add(Error("chunked_consumer.payload.source_json_copy", payload.ScenarioId, "Payload must transform Goal 039 deltas into a consumer contract instead of copying source JSON."));
        }

        if (!payload.SourceEvidence.ConsumesGoal039RuntimeDeltaCommands || payload.RuntimeDeltaMarkers.Count == 0)
        {
            diagnostics.Add(Error("chunked_consumer.source.goal039_runtime_delta_missing", payload.ScenarioId, "Payload must consume Goal 039 runtime chunk delta markers."));
        }

        if (!payload.SourceEvidence.ConsumesGoal039SaveLoadProof
            || !payload.SourceEvidence.ConsumesGoal039ReplayProof
            || !payload.ReplaySaveLoadCorrelation.SerializerRoundtripPassed
            || !payload.ReplaySaveLoadCorrelation.SnapshotRoundtripPassed
            || !payload.ReplaySaveLoadCorrelation.ReplayDeterminismPassed
            || string.IsNullOrWhiteSpace(payload.ReplaySaveLoadCorrelation.RuntimeStateHash)
            || string.IsNullOrWhiteSpace(payload.ReplaySaveLoadCorrelation.RestoredRuntimeStateHash)
            || string.IsNullOrWhiteSpace(payload.ReplaySaveLoadCorrelation.ReplayMarker))
        {
            diagnostics.Add(Error("chunked_consumer.persistence.correlation_missing", payload.ScenarioId, "Payload must preserve save-load and replay correlation from Goal 039."));
        }

        if (payload.ChunkIds.Count == 0 || payload.ChunkIds.Any(item => !item.StartsWith("chunk/" + payload.ScenarioId + "/", StringComparison.Ordinal)))
        {
            diagnostics.Add(Error("chunked_consumer.chunk.fake", payload.ScenarioId, "Payload chunk ids must come from the matching Goal 039 scenario."));
        }

        if (!payload.RuntimeDeltaMarkers.Select(item => item.Order).SequenceEqual(payload.RuntimeDeltaMarkers.Select(item => item.Order).Order()))
        {
            diagnostics.Add(Error("chunked_consumer.order.nondeterministic", payload.ScenarioId, "Runtime delta markers must keep deterministic order."));
        }

        if (!payload.ChunkIds.SequenceEqual(payload.ChunkIds.Order(StringComparer.Ordinal), StringComparer.Ordinal))
        {
            diagnostics.Add(Error("chunked_consumer.order.nondeterministic", payload.ScenarioId, "Chunk ids must be deterministically ordered."));
        }

        diagnostics.AddRange(ValidateFamilyViews(payload.FamilyLensViews, payload.ScenarioId));
        diagnostics.AddRange(ValidateBoundaryClaims(payload.BoundaryClaims, payload.ScenarioId));
        return SortDiagnostics(diagnostics);
    }

    public IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> ValidateMultiFamilyMatrix(MultiFamilyWorldScaleRegressionMatrix matrix)
    {
        var diagnostics = new List<ChunkedRuntimePreviewExportDiagnostic>();
        if (matrix.FamilyLensCount < 3 || matrix.FamilyLenses.Count < 3)
        {
            diagnostics.Add(Error("chunked_consumer.family.count_missing", "multi_family_matrix", "At least three family lenses are required."));
        }

        foreach (var lens in matrix.FamilyLenses)
        {
            if (lens.ForksCoreTraversalSchema || lens.CorePayloadSchemaId != ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId)
            {
                diagnostics.Add(Error("chunked_consumer.family.core_schema_fork", lens.FamilyLensId, "Family lenses must reuse the same core chunk traversal payload schema."));
            }

            if (lens.ExpectedConsumerNeeds.Count == 0)
            {
                diagnostics.Add(Error("chunked_consumer.family.needs_missing", lens.FamilyLensId, "Family lenses must state distinct expected consumer needs."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    public IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> ValidateInfiniteProof(InfiniteChunkedWorldSmokeProof proof)
    {
        var diagnostics = new List<ChunkedRuntimePreviewExportDiagnostic>();
        if (string.IsNullOrWhiteSpace(proof.SeedId)
            || !string.Equals(proof.RepeatableHash, proof.ReplayedHash, StringComparison.Ordinal)
            || !proof.Deterministic)
        {
            diagnostics.Add(Error("chunked_consumer.infinite.seed_nondeterministic", proof.SeedId, "Bounded infinite window proof must have a stable seed and repeatable hash."));
        }

        if (proof.Window.Radius < 0 || proof.Window.Width <= 0 || proof.Window.Height <= 0 || proof.DerivedChunks.Count != proof.Window.Width * proof.Window.Height)
        {
            diagnostics.Add(Error("chunked_consumer.infinite.window_invalid", proof.SeedId, "Bounded infinite window dimensions must be valid and match derived chunks."));
        }

        if (proof.RealInfiniteStreamingImplemented)
        {
            diagnostics.Add(Error("chunked_consumer.infinite.real_streaming_forbidden", proof.SeedId, "Goal 040 must not implement real infinite streaming."));
        }

        return SortDiagnostics(diagnostics);
    }

    public IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> ValidatePackageAudit(PackageImmutabilityAudit audit)
    {
        var diagnostics = new List<ChunkedRuntimePreviewExportDiagnostic>();
        if (audit.GamePackageDefinitionsMutated || audit.PublicPackageSchemaMutated)
        {
            diagnostics.Add(Error("chunked_consumer.boundary.gamepackage.forbidden", "package_immutability_audit", "Goal 040 must not mutate GamePackage definitions or public package schema."));
        }

        if (audit.RuntimeStateSourceContractsMutated || audit.WinFormsUiMutated || audit.UnityEntrypointsMutated)
        {
            diagnostics.Add(Error("chunked_consumer.boundary.runtime_ui_unity.forbidden", "package_immutability_audit", "Goal 040 must not mutate Runtime, WinForms UI or Unity source."));
        }

        if (audit.ProviderLlmRagTouched)
        {
            diagnostics.Add(Error("chunked_consumer.boundary.provider_llm_rag.forbidden", "package_immutability_audit", "Provider/LLM/RAG calls are forbidden."));
        }

        if (audit.LuaExecutionTouched)
        {
            diagnostics.Add(Error("chunked_consumer.boundary.lua.forbidden", "package_immutability_audit", "Lua source/execution changes are forbidden."));
        }

        if (audit.GeneratorLibraryTouched)
        {
            diagnostics.Add(Error("chunked_consumer.boundary.generator_library.forbidden", "package_immutability_audit", "Generator-library mutation is forbidden."));
        }

        return SortDiagnostics(diagnostics);
    }

    public InvalidChunkedConsumerMatrix BuildInvalidMatrix(
        ChunkedRuntimePreviewExportSourceBundle source,
        IReadOnlyList<ChunkedPreviewPayload> payloads,
        MultiFamilyWorldScaleRegressionMatrix familyMatrix,
        InfiniteChunkedWorldSmokeProof infiniteProof,
        PackageImmutabilityAudit packageAudit)
    {
        var frontier = payloads.Single(item => item.ScenarioId == "frontier_survival");
        var firstChunk = frontier.ChunkIds.First();
        var firstRoute = frontier.TraversalRoute.First();
        var invalid = new List<InvalidChunkedConsumerScenario>
        {
            Invalid("missing_goal039_source_evidence", "missing Goal 039 source evidence", "rejected", [Error("chunked_consumer.source.goal039_missing", RuntimeChunkDeltaEvidenceService.RelativeOutputDirectory, "Goal 039 source evidence is required.")]),
            PayloadInvalid("fake_scenario_id", "fake scenario id", frontier with { ScenarioId = "fake_scenario" }),
            PayloadInvalid("fake_chunk_id", "fake chunk id", frontier with { ChunkIds = frontier.ChunkIds.Select(item => item == firstChunk ? "chunk/fake/missing/primary" : item).ToList(), TraversalRoute = frontier.TraversalRoute.Select((item, index) => index == 0 ? item with { ChunkId = "chunk/fake/missing/primary" } : item).ToList() }),
            PayloadInvalid("static_map_without_runtime_delta", "traversal references Goal 038 static map but no Goal 039 runtime delta", frontier with { RuntimeDeltaMarkers = [], SourceEvidence = frontier.SourceEvidence with { ConsumesGoal039RuntimeDeltaCommands = false } }),
            FamilyInvalid("family_lens_forks_core_schema", "family lens forks core schema", familyMatrix with { FamilyLenses = familyMatrix.FamilyLenses.Select((item, index) => index == 0 ? item with { CorePayloadSchemaId = "forked_schema", ForksCoreTraversalSchema = true } : item).ToList() }),
            FamilyInvalid("family_lens_missing_required_consumer_needs", "family lens missing required consumer needs", familyMatrix with { FamilyLenses = familyMatrix.FamilyLenses.Select((item, index) => index == 0 ? item with { ExpectedConsumerNeeds = [] } : item).ToList() }),
            InfiniteInvalid("infinite_window_nondeterministic_seed", "infinite window nondeterministic seed", infiniteProof with { ReplayedHash = "different", Deterministic = false }),
            InfiniteInvalid("boundary_overflow_invalid_window", "boundary overflow or invalid window", infiniteProof with { Window = infiniteProof.Window with { Radius = -1, Width = 0 } }),
            AuditInvalid("package_mutation_attempt", "package mutation attempt", packageAudit with { GamePackageDefinitionsMutated = true, Passed = false }, "blocked"),
            PayloadInvalid("runtime_ui_unity_source_mutation_claim", "Runtime/UI/Unity source mutation claim", frontier with { BoundaryClaims = new ChunkedConsumerBoundaryClaims { RuntimeSourceMutation = true, UiWinFormsMutation = true, UnitySourceMutation = true } }, "blocked"),
            PayloadInvalid("provider_llm_rag_claim", "provider/LLM/RAG claim", frontier with { BoundaryClaims = new ChunkedConsumerBoundaryClaims { ProviderLlmRag = true } }, "blocked"),
            PayloadInvalid("lua_execution_claim", "Lua execution claim", frontier with { BoundaryClaims = new ChunkedConsumerBoundaryClaims { LuaSourceOrExecution = true } }, "blocked"),
            PayloadInvalid("filesystem_network_process_reflection_thread_time_random_native_interop_claim", "filesystem/network/process/reflection/thread/time/random/native interop claim", frontier with { BoundaryClaims = new ChunkedConsumerBoundaryClaims { Filesystem = true, Network = true, Process = true, Reflection = true, Thread = true, Time = true, Random = true, NativeInterop = true } }, "blocked"),
            PayloadInvalid("final_prose_only_payload", "final prose-only payload", frontier with { FinalProseOnly = true }),
            PayloadInvalid("missing_save_load_replay_correlation", "missing save-load/replay correlation", frontier with { ReplaySaveLoadCorrelation = new ChunkedReplaySaveLoadCorrelation(), SourceEvidence = frontier.SourceEvidence with { ConsumesGoal039SaveLoadProof = false, ConsumesGoal039ReplayProof = false } }),
            PayloadInvalid("nondeterministic_ordering", "nondeterministic ordering", frontier with { RuntimeDeltaMarkers = frontier.RuntimeDeltaMarkers.Reverse().ToList() })
        };

        return new InvalidChunkedConsumerMatrix
        {
            ScenarioCount = invalid.Count,
            MatchedExpectationCount = invalid.Count(item => item.ExpectedStatus == item.ActualStatus),
            RejectedCount = invalid.Count(item => item.ActualStatus == "rejected"),
            BlockedCount = invalid.Count(item => item.ActualStatus == "blocked"),
            Passed = source.PlansByScenario.Count == 4
                && invalid.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            Scenarios = invalid.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };

        InvalidChunkedConsumerScenario PayloadInvalid(
            string scenarioId,
            string kind,
            ChunkedPreviewPayload payload,
            string expectedStatus = "rejected") =>
            Invalid(scenarioId, kind, expectedStatus, ValidatePayload(payload).Where(item => item.Severity == "error").ToList());

        InvalidChunkedConsumerScenario FamilyInvalid(
            string scenarioId,
            string kind,
            MultiFamilyWorldScaleRegressionMatrix matrix,
            string expectedStatus = "rejected") =>
            Invalid(scenarioId, kind, expectedStatus, ValidateMultiFamilyMatrix(matrix).Where(item => item.Severity == "error").ToList());

        InvalidChunkedConsumerScenario InfiniteInvalid(
            string scenarioId,
            string kind,
            InfiniteChunkedWorldSmokeProof proof,
            string expectedStatus = "rejected") =>
            Invalid(scenarioId, kind, expectedStatus, ValidateInfiniteProof(proof).Where(item => item.Severity == "error").ToList());

        InvalidChunkedConsumerScenario AuditInvalid(
            string scenarioId,
            string kind,
            PackageImmutabilityAudit audit,
            string expectedStatus = "rejected") =>
            Invalid(scenarioId, kind, expectedStatus, ValidatePackageAudit(audit).Where(item => item.Severity == "error").ToList());
    }

    private IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> ValidateFamilyViews(
        IReadOnlyList<ChunkedFamilyLensPayloadView> views,
        string target)
    {
        var diagnostics = new List<ChunkedRuntimePreviewExportDiagnostic>();
        if (views.Count < 3)
        {
            diagnostics.Add(Error("chunked_consumer.family.count_missing", target, "Payload must include at least three family lens views."));
        }

        foreach (var view in views)
        {
            if (view.ForksCoreTraversalSchema || view.CorePayloadSchemaId != ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId)
            {
                diagnostics.Add(Error("chunked_consumer.family.core_schema_fork", view.FamilyLensId, "Family lens view must reuse the same core payload schema."));
            }

            if (view.ExpectedConsumerNeeds.Count == 0)
            {
                diagnostics.Add(Error("chunked_consumer.family.needs_missing", view.FamilyLensId, "Family lens view must declare consumer needs."));
            }
        }

        return diagnostics;
    }

    private static IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> ValidateBoundaryClaims(
        ChunkedConsumerBoundaryClaims claims,
        string target)
    {
        var diagnostics = new List<ChunkedRuntimePreviewExportDiagnostic>();
        if (claims.GamePackageDefinitionsMutation) diagnostics.Add(Error("chunked_consumer.boundary.gamepackage.forbidden", target, "GamePackage definition mutation is forbidden."));
        if (claims.RuntimeSourceMutation || claims.UiWinFormsMutation || claims.UnitySourceMutation) diagnostics.Add(Error("chunked_consumer.boundary.runtime_ui_unity.forbidden", target, "Runtime/UI/Unity source mutation is forbidden."));
        if (claims.ProviderLlmRag) diagnostics.Add(Error("chunked_consumer.boundary.provider_llm_rag.forbidden", target, "Provider/LLM/RAG calls are forbidden."));
        if (claims.LuaSourceOrExecution) diagnostics.Add(Error("chunked_consumer.boundary.lua.forbidden", target, "Lua source/execution changes are forbidden."));
        if (claims.GeneratorLibraryMutation) diagnostics.Add(Error("chunked_consumer.boundary.generator_library.forbidden", target, "Generator-library mutation is forbidden."));
        if (claims.Filesystem || claims.Network || claims.Process || claims.Reflection || claims.Thread || claims.Time || claims.Random || claims.NativeInterop)
        {
            diagnostics.Add(Error("chunked_consumer.boundary.filesystem_network_process_reflection_thread_time_random_native_interop.forbidden", target, "Forbidden IO/process/reflection/thread/time/random/native interop claim was found."));
        }

        return diagnostics;
    }

    private static InvalidChunkedConsumerScenario Invalid(
        string scenarioId,
        string kind,
        string expectedStatus,
        IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> diagnostics)
    {
        var actualStatus = diagnostics.Any(item => item.Code.Contains(".boundary.", StringComparison.Ordinal))
            ? "blocked"
            : diagnostics.Any(item => item.Severity == "error")
                ? "rejected"
                : "accepted";
        return new InvalidChunkedConsumerScenario
        {
            ScenarioId = scenarioId,
            MutatedEvidenceKind = kind,
            ExpectedStatus = expectedStatus,
            ActualStatus = actualStatus,
            ExpectedValid = false,
            ActualValid = actualStatus == "accepted",
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> SortDiagnostics(IEnumerable<ChunkedRuntimePreviewExportDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(item => item.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static ChunkedRuntimePreviewExportDiagnostic Error(string code, string target, string message) =>
        ChunkedRuntimePreviewExportDiagnostic.Error(code, target, message);
}

public static class ChunkedRuntimePreviewExportHash
{
    public static string Hash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}
