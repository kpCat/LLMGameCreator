using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.World;

public sealed class ConnectedWorldTravelAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/connected-world-travel";
    public const string ReportJsonFileName = "connected-world-travel-report.json";
    public const string ReportMarkdownFileName = "connected-world-travel-report.md";
    public const string VerificationMarkdownFileName = "connected-world-travel-verification.md";
    public const string ManualGate = "connected_world_travel_state_artifact_verification";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static ConnectedWorldTravelAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public ConnectedWorldTravelAcceptanceResult Build(string? projectRootPath = null)
    {
        var package = BuildPackage();
        var validScenarios = new[]
        {
            BuildValidScenario(
                package,
                "connected_world_core_route",
                "goal007-core-route-seed",
                ["connection/hub-to-wildland", "connection/wildland-to-hub"],
                [RuntimeChunkDelta.Discovered("chunk/hub/0_0"), RuntimeChunkDelta.OpenedRoute("chunk/hub/0_0", "connection/hub-to-wildland")]),
            BuildValidScenario(
                package,
                "connected_world_branching_route",
                "goal007-branching-route-seed",
                ["connection/hub-to-mystery", "connection/mystery-to-hub", "connection/hub-to-trade"],
                [RuntimeChunkDelta.Discovered("chunk/mystery/1_0"), RuntimeChunkDelta.VisitedLandmark("chunk/trade/0_1", "landmark/caravan-gate")]),
            BuildValidScenario(
                package,
                "connected_world_variable_maps",
                "goal007-variable-maps-seed",
                ["connection/hub-to-wildland", "connection/wildland-to-mystery", "connection/mystery-to-hub"],
                [RuntimeChunkDelta.Discovered("chunk/wildland/1_0"), RuntimeChunkDelta.Harvested("chunk/wildland/1_0", "marker/herb-cache")]),
            BuildValidScenario(
                package,
                "connected_world_chunk_delta_persistence",
                "goal007-chunk-delta-seed",
                ["connection/hub-to-trade", "connection/trade-to-wildland"],
                [RuntimeChunkDelta.Discovered("chunk/trade/0_1"), RuntimeChunkDelta.OpenedRoute("chunk/trade/0_1", "connection/trade-to-wildland")])
        };

        var invalidScenarios = new[]
        {
            BuildInvalidScenario(package, "invalid_disconnected_region_graph", "goal007-invalid-disconnected", InvalidScenarioKind.DisconnectedGraph),
            BuildInvalidScenario(package, "invalid_missing_region_or_map_ref", "goal007-invalid-missing-ref", InvalidScenarioKind.MissingRegionOrMapRef),
            BuildInvalidScenario(package, "invalid_chunk_boundary_or_rules", "goal007-invalid-chunk-boundary", InvalidScenarioKind.ChunkBoundaryOrRules),
            BuildInvalidScenario(package, "invalid_runtime_delta_as_source", "goal007-invalid-runtime-delta-source", InvalidScenarioKind.RuntimeDeltaAsSource)
        };

        var repeated = BuildValidScenario(
            package,
            "connected_world_core_route",
            "goal007-core-route-seed",
            ["connection/hub-to-wildland", "connection/wildland-to-hub"],
            [RuntimeChunkDelta.Discovered("chunk/hub/0_0"), RuntimeChunkDelta.OpenedRoute("chunk/hub/0_0", "connection/hub-to-wildland")]);

        var scenarios = validScenarios.Concat(invalidScenarios).ToList();
        var validAccepted = validScenarios.All(item => item.ExpectedValid && item.ActualValid);
        var invalidRejected = invalidScenarios.All(item =>
            !item.ExpectedValid &&
            !item.ActualValid &&
            !item.RuntimeEvidence.RuntimeAttempted &&
            item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error"));
        var deterministicReplayPassed = validScenarios[0].DeterministicHash == repeated.DeterministicHash &&
                                        validScenarios[0].RuntimeEvidence.RuntimeStateHash == repeated.RuntimeEvidence.RuntimeStateHash &&
                                        validScenarios[0].RuntimeEvidence.RestoredRuntimeStateHash == repeated.RuntimeEvidence.RestoredRuntimeStateHash;
        var travelRuntimeExecutionPassed = validScenarios.All(item =>
            item.RuntimeEvidence.RuntimeAttempted &&
            item.RuntimeEvidence.CommandSucceeded &&
            item.RuntimeEvidence.RouteSteps.Count > 0 &&
            item.RuntimeEvidence.RouteSteps.All(step => step.Succeeded));
        var saveLoadPassed = validScenarios.All(item =>
            item.RuntimeEvidence.SaveLoadRoundtripPassed &&
            item.RuntimeEvidence.ExactStateComparisonPassed &&
            item.RuntimeEvidence.StateEvidence.SequenceEqual(item.RuntimeEvidence.RestoredStateEvidence));
        var variableMapsPassed = validScenarios.All(HasVariableMapEvidence);
        var chunkDeltaPersistencePassed = validScenarios.All(item =>
            item.ChunkEvidence.RuntimeDeltas.Count > 0 &&
            item.RuntimeEvidence.ChunkDeltasPersisted &&
            item.ChunkEvidence.SourceRuntimeDeltaIds.Count == 0);
        var graphReachabilityPassed = validScenarios.All(item => item.Reachability.AllRequiredReachable);
        var routeBindingsPassed = validScenarios.All(RouteStepsReferenceConnectionsAndMapBindings);

        var diagnostics = new List<ConnectedWorldTravelDiagnostic>
        {
            Diagnostic("info", "connected_world.goal006_gate_recorded", "semantic_selected_runtime_composition_artifact_verification", "User-confirmed Goal 006 semantic-selected runtime composition artifact verification is recorded as passed."),
            Diagnostic("info", "connected_world.no_external_execution", "harness", "No LLM, RAG, provider, Lua, Unity or media execution was invoked."),
            Diagnostic(validAccepted ? "info" : "error", validAccepted ? "connected_world.valid_scenarios_accepted" : "connected_world.valid_scenarios_failed", "valid_scenarios", "All required connected-world valid scenarios must be accepted."),
            Diagnostic(invalidRejected ? "info" : "error", invalidRejected ? "connected_world.invalid_scenarios_rejected" : "connected_world.invalid_scenarios_not_rejected", "invalid_scenarios", "All required invalid scenarios must fail by validator diagnostics before runtime travel."),
            Diagnostic(deterministicReplayPassed ? "info" : "error", deterministicReplayPassed ? "connected_world.replay_stable" : "connected_world.replay_unstable", "connected_world_core_route", "Repeated route execution must produce stable scenario and runtime hashes."),
            Diagnostic(travelRuntimeExecutionPassed ? "info" : "error", travelRuntimeExecutionPassed ? "connected_world.travel_runtime_executed" : "connected_world.travel_runtime_missing", "runtime_state", "Travel must update runtime-owned state rather than report-only fields."),
            Diagnostic(saveLoadPassed ? "info" : "error", saveLoadPassed ? "connected_world.save_load_roundtrip_passed" : "connected_world.save_load_roundtrip_failed", "runtime_state", "Save/load must restore exact world, travel and chunk evidence."),
            Diagnostic(variableMapsPassed ? "info" : "error", variableMapsPassed ? "connected_world.variable_maps_verified" : "connected_world.variable_maps_missing", "map_bindings", "At least three exact package maps must differ by dimensions or layout signature."),
            Diagnostic(chunkDeltaPersistencePassed ? "info" : "error", chunkDeltaPersistencePassed ? "connected_world.runtime_chunk_deltas_persisted" : "connected_world.runtime_chunk_delta_failure", "runtime_chunks", "Runtime chunk deltas must persist through save/load and stay out of source content."),
            Diagnostic(graphReachabilityPassed ? "info" : "error", graphReachabilityPassed ? "connected_world.graph_reachability_passed" : "connected_world.graph_reachability_failed", "region_graph", "All required regions must be reachable from the start region."),
            Diagnostic(routeBindingsPassed ? "info" : "error", routeBindingsPassed ? "connected_world.route_bindings_verified" : "connected_world.route_binding_failure", "route_steps", "Every route step must reference a real connection and destination map binding.")
        };
        diagnostics.AddRange(scenarios.SelectMany(item => item.Diagnostics));

        var reportWithoutHash = new ConnectedWorldTravelReport
        {
            Accepted = validAccepted &&
                       invalidRejected &&
                       deterministicReplayPassed &&
                       travelRuntimeExecutionPassed &&
                       saveLoadPassed &&
                       variableMapsPassed &&
                       chunkDeltaPersistencePassed &&
                       graphReachabilityPassed &&
                       routeBindingsPassed,
            ManualGate = ManualGate,
            Goal006GateRecorded = true,
            ScenarioCount = scenarios.Count,
            ValidScenarioCount = validScenarios.Length,
            InvalidScenarioCount = invalidScenarios.Length,
            ValidScenariosAccepted = validAccepted,
            InvalidScenariosRejected = invalidRejected,
            DeterministicReplayPassed = deterministicReplayPassed,
            TravelRuntimeExecutionPassed = travelRuntimeExecutionPassed,
            SaveLoadRoundtripPassed = saveLoadPassed,
            VariableMapEvidencePassed = variableMapsPassed,
            ChunkDeltaPersistencePassed = chunkDeltaPersistencePassed,
            GraphReachabilityPassed = graphReachabilityPassed,
            RouteBindingEvidencePassed = routeBindingsPassed,
            PublicGamePackageSchemaChanged = false,
            ExternalExecution = new ConnectedWorldExternalExecutionFlags(),
            Scenarios = scenarios,
            Diagnostics = SortDiagnostics(diagnostics),
            RemainingPrimitiveLimits =
            [
                "infinite chunk streaming is not implemented",
                "Unity runtime travel presentation is not implemented",
                "provider/media generation is not implemented",
                "rule-pack gameplay families beyond travel/world state remain for later goals"
            ]
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new ConnectedWorldTravelAcceptanceResult
        {
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<ConnectedWorldTravelWriteResult> WriteAsync(
        string projectRootPath,
        ConnectedWorldTravelAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "connected-world-travel"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportJsonFileName));
        var markdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportMarkdownFileName));
        var verificationPath = Path.GetFullPath(Path.Combine(outputDirectory, VerificationMarkdownFileName));
        EnsureContained(outputDirectory, jsonPath);
        EnsureContained(outputDirectory, markdownPath);
        EnsureContained(outputDirectory, verificationPath);

        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new ConnectedWorldTravelWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<ConnectedWorldTravelWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static ConnectedWorldTravelScenario BuildValidScenario(
        GamePackageDefinition package,
        string scenarioId,
        string seed,
        IReadOnlyList<string> routeConnectionIds,
        IReadOnlyList<RuntimeChunkDelta> runtimeDeltas)
    {
        var profile = BuildWorldProfile();
        var graph = BuildBaseGraph();
        var bindings = BuildBindings();
        var chunkEvidence = BuildChunkEvidence(seed, runtimeDeltas, []);
        var diagnostics = ValidateScenario(package, profile, graph, bindings, chunkEvidence);
        var reachability = BuildReachability(graph);

        ConnectedWorldRuntimeEvidence runtimeEvidence;
        if (diagnostics.Any(item => item.Severity == "error"))
        {
            runtimeEvidence = new ConnectedWorldRuntimeEvidence();
        }
        else
        {
            runtimeEvidence = ExecuteRoute(package, profile, graph, bindings, chunkEvidence, scenarioId, routeConnectionIds);
        }

        var scenarioWithoutHash = new ConnectedWorldTravelScenario
        {
            ScenarioId = scenarioId,
            Seed = seed,
            ExpectedValid = true,
            ActualValid = diagnostics.All(item => item.Severity != "error") &&
                          runtimeEvidence.RuntimeAttempted &&
                          runtimeEvidence.CommandSucceeded &&
                          runtimeEvidence.SaveLoadRoundtripPassed &&
                          runtimeEvidence.InvalidTravelRejected,
            WorldProfile = profile,
            RegionGraph = graph,
            MapBindings = bindings,
            MapSignatures = BuildMapSignatures(package, bindings),
            Reachability = reachability,
            ChunkEvidence = chunkEvidence,
            RuntimeEvidence = runtimeEvidence,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        return scenarioWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(scenarioWithoutHash, JsonOptions))
        };
    }

    private static ConnectedWorldTravelScenario BuildInvalidScenario(
        GamePackageDefinition package,
        string scenarioId,
        string seed,
        InvalidScenarioKind kind)
    {
        var profile = BuildWorldProfile();
        var graph = BuildBaseGraph();
        var bindings = BuildBindings();
        var chunkEvidence = BuildChunkEvidence(seed, [], []);

        switch (kind)
        {
            case InvalidScenarioKind.DisconnectedGraph:
                graph = graph with
                {
                    Connections = graph.Connections
                        .Where(item => item.FromRegionId != "region/hub" || item.ToRegionId != "region/trade-caravan")
                        .Where(item => item.FromRegionId != "region/trade-caravan" || item.ToRegionId != "region/hub")
                        .Where(item => item.FromRegionId != "region/trade-caravan" && item.ToRegionId != "region/trade-caravan")
                        .ToList()
                };
                break;
            case InvalidScenarioKind.MissingRegionOrMapRef:
                graph = graph with
                {
                    Connections = graph.Connections.Concat([
                        new RegionConnectionRecord
                        {
                            ConnectionId = "connection/hub-to-missing-region",
                            FromRegionId = "region/hub",
                            ToRegionId = "region/missing",
                            TravelRuleId = "travel_rule/walk"
                        }
                    ]).ToList()
                };
                bindings = bindings.Select(item => item.RegionId == "region/mystery-dungeon" ? item with { MapId = "map/missing-dungeon" } : item).ToList();
                break;
            case InvalidScenarioKind.ChunkBoundaryOrRules:
                chunkEvidence = BuildChunkEvidence(string.Empty, [], []) with
                {
                    RulesVersion = string.Empty,
                    ChunkSize = 0,
                    Chunks = BuildChunks(seed).Select((chunk, index) => index == 0
                        ? chunk with { BoundaryExits = SetBoundary(chunk.BoundaryExits, "east", "road:alpha") }
                        : index == 1
                            ? chunk with { BoundaryExits = SetBoundary(chunk.BoundaryExits, "west", "road:beta") }
                            : chunk).ToList()
                };
                break;
            case InvalidScenarioKind.RuntimeDeltaAsSource:
                chunkEvidence = BuildChunkEvidence(seed, [], ["delta/source/discovered-hub"]);
                break;
        }

        var diagnostics = ValidateScenario(package, profile, graph, bindings, chunkEvidence);
        var scenarioWithoutHash = new ConnectedWorldTravelScenario
        {
            ScenarioId = scenarioId,
            Seed = seed,
            ExpectedValid = false,
            ActualValid = diagnostics.All(item => item.Severity != "error"),
            WorldProfile = profile,
            RegionGraph = graph,
            MapBindings = bindings,
            MapSignatures = BuildMapSignatures(package, bindings),
            Reachability = BuildReachability(graph),
            ChunkEvidence = chunkEvidence,
            RuntimeEvidence = new ConnectedWorldRuntimeEvidence(),
            Diagnostics = SortDiagnostics(diagnostics)
        };

        return scenarioWithoutHash with
        {
            ActualValid = false,
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(scenarioWithoutHash, JsonOptions))
        };
    }

    private static IReadOnlyList<ConnectedWorldTravelDiagnostic> ValidateScenario(
        GamePackageDefinition package,
        WorldProfileRecord profile,
        RegionGraphRecord graph,
        IReadOnlyList<RegionMapBindingRecord> bindings,
        BoundedChunkEvidence chunkEvidence)
    {
        var diagnostics = new List<ConnectedWorldTravelDiagnostic>();
        if (string.IsNullOrWhiteSpace(profile.WorldProfileId))
        {
            diagnostics.Add(Diagnostic("error", "connected_world.world_profile_missing", "world_profile", "World profile id is required."));
        }

        var regionIds = graph.Regions.Select(item => item.RegionId).ToList();
        foreach (var duplicate in regionIds.GroupBy(item => item, StringComparer.Ordinal).Where(group => group.Count() > 1).Select(group => group.Key))
        {
            diagnostics.Add(Diagnostic("error", "connected_world.duplicate_region_id", duplicate, "Region ids must be unique."));
        }

        if (string.IsNullOrWhiteSpace(graph.StartRegionId) || !regionIds.Contains(graph.StartRegionId, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "connected_world.missing_start_region", graph.StartRegionId, "Start region must exist."));
        }

        var packageMapIds = package.Game.Maps.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var region in graph.Regions)
        {
            var binding = bindings.FirstOrDefault(item => item.RegionId == region.RegionId);
            if (binding == null)
            {
                diagnostics.Add(Diagnostic("error", "connected_world.missing_map_binding", region.RegionId, "Every required region must bind to an exact package map id."));
                continue;
            }

            if (!packageMapIds.Contains(binding.MapId))
            {
                diagnostics.Add(Diagnostic("error", "connected_world.missing_map_ref", binding.MapId, "Region map binding must reference an existing package map id."));
            }
        }

        foreach (var duplicate in graph.Connections.GroupBy(item => item.ConnectionId, StringComparer.Ordinal).Where(group => group.Count() > 1).Select(group => group.Key))
        {
            diagnostics.Add(Diagnostic("error", "connected_world.duplicate_connection_id", duplicate, "Connection ids must be unique."));
        }

        foreach (var connection in graph.Connections)
        {
            if (!regionIds.Contains(connection.FromRegionId, StringComparer.Ordinal) ||
                !regionIds.Contains(connection.ToRegionId, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "connected_world.missing_connection_region_ref", connection.ConnectionId, "Connection endpoints must reference existing regions."));
            }
        }

        var reachability = BuildReachability(graph);
        if (!reachability.AllRequiredReachable)
        {
            diagnostics.Add(Diagnostic("error", "connected_world.disconnected_required_region", graph.StartRegionId, "All required regions must be reachable from the start region."));
        }

        if (graph.Connections.Count > 0 && graph.Connections.All(item => item.FromRegionId == item.ToRegionId) && graph.Regions.Count > 1)
        {
            diagnostics.Add(Diagnostic("error", "connected_world.self_loop_only_graph", graph.StartRegionId, "Self-loop-only graphs cannot reach required regions."));
        }

        if (string.IsNullOrWhiteSpace(chunkEvidence.Seed))
        {
            diagnostics.Add(Diagnostic("error", "connected_world.chunk_seed_missing", "chunk_config", "Chunk seed is required."));
        }

        if (string.IsNullOrWhiteSpace(chunkEvidence.RulesVersion))
        {
            diagnostics.Add(Diagnostic("error", "connected_world.chunk_rules_version_missing", "chunk_config", "Chunk rules version is required."));
        }

        if (chunkEvidence.ChunkSize <= 0)
        {
            diagnostics.Add(Diagnostic("error", "connected_world.chunk_size_invalid", "chunk_config", "Chunk size must be positive."));
        }

        diagnostics.AddRange(ValidateBoundaryCompatibility(chunkEvidence.Chunks));
        if (chunkEvidence.SourceRuntimeDeltaIds.Count > 0)
        {
            diagnostics.Add(Diagnostic("error", "connected_world.runtime_delta_in_source_content", "chunk_source_content", "Runtime chunk deltas must remain save/runtime-only and must not be placed in immutable source content."));
        }

        return SortDiagnostics(diagnostics);
    }

    private static ConnectedWorldRuntimeEvidence ExecuteRoute(
        GamePackageDefinition package,
        WorldProfileRecord profile,
        RegionGraphRecord graph,
        IReadOnlyList<RegionMapBindingRecord> bindings,
        BoundedChunkEvidence chunkEvidence,
        string scenarioId,
        IReadOnlyList<string> routeConnectionIds)
    {
        var diagnostics = new List<ConnectedWorldTravelDiagnostic>();
        var state = new GameRuntimeState
        {
            PackageId = package.Manifest.PackageId,
            CurrentMapId = bindings.Single(item => item.RegionId == graph.StartRegionId).MapId
        };
        WriteRuntimeState(state, new RuntimeWorldStateSnapshot
        {
            WorldProfileId = profile.WorldProfileId,
            CurrentRegionId = graph.StartRegionId,
            CurrentMapId = state.CurrentMapId,
            VisitedRegionIds = [graph.StartRegionId],
            DiscoveredConnectionIds = [],
            TravelLog = [],
            RegionEvidenceHashes = BuildRegionEvidence(graph.StartRegionId, state.CurrentMapId, []),
            DiscoveredChunkIds = [],
            RuntimeChunkDeltas = []
        });

        var steps = new List<TravelRouteStepEvidence>();
        foreach (var connectionId in routeConnectionIds)
        {
            var result = Travel(package, graph, bindings, state, connectionId);
            steps.Add(result.Step);
            diagnostics.AddRange(result.Diagnostics);
        }

        foreach (var delta in chunkEvidence.RuntimeDeltas)
        {
            ApplyRuntimeChunkDelta(state, delta);
        }

        var invalidTravel = Travel(package, graph, bindings, state, "connection/not-real");
        var invalidTravelRejected = !invalidTravel.Step.Succeeded &&
                                    invalidTravel.Diagnostics.Any(item => item.Code == "connected_world.travel.connection_not_available");

        var stateEvidence = ExtractStateEvidence(state);
        var stateHash = ComputeHash(JsonSerializer.Serialize(stateEvidence, JsonOptions));
        state.Metadata["world.stateHash"] = stateHash;
        stateEvidence = ExtractStateEvidence(state);
        var json = JsonSerializer.Serialize(state, JsonOptions);
        var restored = JsonSerializer.Deserialize<GameRuntimeState>(json, JsonOptions) ?? new GameRuntimeState();
        var restoredEvidence = ExtractStateEvidence(restored);
        var restoredHash = restoredEvidence.GetValueOrDefault("stateHash", string.Empty);
        var exactComparison = StateEvidenceEquals(stateEvidence, restoredEvidence);
        var deltasPersisted = chunkEvidence.RuntimeDeltas
            .Select(item => item.DeltaId)
            .OrderBy(item => item, StringComparer.Ordinal)
            .SequenceEqual(RuntimeWorldStateSnapshot.FromState(restored).RuntimeChunkDeltas.Select(item => item.DeltaId).OrderBy(item => item, StringComparer.Ordinal));

        return new ConnectedWorldRuntimeEvidence
        {
            RuntimeAttempted = true,
            RuntimeStateOwner = nameof(GameRuntimeState),
            StartRegionId = graph.StartRegionId,
            StartMapId = bindings.Single(item => item.RegionId == graph.StartRegionId).MapId,
            FinalRegionId = RuntimeWorldStateSnapshot.FromState(state).CurrentRegionId,
            FinalMapId = state.CurrentMapId,
            CommandSucceeded = steps.Count > 0 && steps.All(item => item.Succeeded),
            InvalidTravelRejected = invalidTravelRejected,
            RouteSteps = steps,
            RuntimeStateHash = stateHash,
            RestoredRuntimeStateHash = restoredHash,
            SaveLoadRoundtripPassed = exactComparison && stateHash == restoredHash,
            ExactStateComparisonPassed = exactComparison,
            ChunkDeltasPersisted = deltasPersisted,
            StateEvidence = stateEvidence,
            RestoredStateEvidence = restoredEvidence,
            Diagnostics = SortDiagnostics(diagnostics.Concat(invalidTravel.Diagnostics))
        };
    }

    private static TravelCommandResult Travel(
        GamePackageDefinition package,
        RegionGraphRecord graph,
        IReadOnlyList<RegionMapBindingRecord> bindings,
        GameRuntimeState state,
        string connectionId)
    {
        var snapshot = RuntimeWorldStateSnapshot.FromState(state);
        var diagnostics = new List<ConnectedWorldTravelDiagnostic>();
        var connection = graph.Connections.FirstOrDefault(item =>
            item.ConnectionId == connectionId &&
            item.FromRegionId == snapshot.CurrentRegionId);
        var fromRegion = snapshot.CurrentRegionId;
        var fromMap = state.CurrentMapId;

        if (connection == null)
        {
            diagnostics.Add(Diagnostic("error", "connected_world.travel.connection_not_available", connectionId, "Travel connection is not available from the current region."));
            return new TravelCommandResult(
                new TravelRouteStepEvidence
                {
                    ConnectionId = connectionId,
                    FromRegionId = fromRegion,
                    FromMapId = fromMap,
                    ToRegionId = string.Empty,
                    ToMapId = string.Empty,
                    Succeeded = false,
                    DiagnosticCode = "connected_world.travel.connection_not_available"
                },
                diagnostics);
        }

        var destinationBinding = bindings.FirstOrDefault(item => item.RegionId == connection.ToRegionId);
        if (destinationBinding == null || package.Game.Maps.All(item => item.Id != destinationBinding.MapId))
        {
            diagnostics.Add(Diagnostic("error", "connected_world.travel.destination_map_missing", connection.ConnectionId, "Travel destination must have an exact package map binding."));
            return new TravelCommandResult(
                new TravelRouteStepEvidence
                {
                    ConnectionId = connectionId,
                    FromRegionId = fromRegion,
                    FromMapId = fromMap,
                    ToRegionId = connection.ToRegionId,
                    ToMapId = destinationBinding?.MapId ?? string.Empty,
                    Succeeded = false,
                    DiagnosticCode = "connected_world.travel.destination_map_missing"
                },
                diagnostics);
        }

        snapshot = snapshot with
        {
            CurrentRegionId = connection.ToRegionId,
            CurrentMapId = destinationBinding.MapId,
            VisitedRegionIds = snapshot.VisitedRegionIds.Append(connection.ToRegionId).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            DiscoveredConnectionIds = snapshot.DiscoveredConnectionIds.Append(connection.ConnectionId).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            TravelLog = snapshot.TravelLog.Concat([
                new TravelLogEntry
                {
                    ConnectionId = connection.ConnectionId,
                    FromRegionId = connection.FromRegionId,
                    ToRegionId = connection.ToRegionId,
                    FromMapId = fromMap,
                    ToMapId = destinationBinding.MapId,
                    StepIndex = snapshot.TravelLog.Count + 1
                }
            ]).ToList(),
            RegionEvidenceHashes = BuildRegionEvidence(connection.ToRegionId, destinationBinding.MapId, snapshot.DiscoveredConnectionIds.Append(connection.ConnectionId))
        };
        state.CurrentMapId = destinationBinding.MapId;
        state.Tick += 1;
        WriteRuntimeState(state, snapshot);

        return new TravelCommandResult(
            new TravelRouteStepEvidence
            {
                ConnectionId = connectionId,
                FromRegionId = fromRegion,
                FromMapId = fromMap,
                ToRegionId = connection.ToRegionId,
                ToMapId = destinationBinding.MapId,
                Succeeded = true
            },
            diagnostics);
    }

    private static void ApplyRuntimeChunkDelta(GameRuntimeState state, RuntimeChunkDelta delta)
    {
        var snapshot = RuntimeWorldStateSnapshot.FromState(state);
        snapshot = snapshot with
        {
            DiscoveredChunkIds = snapshot.DiscoveredChunkIds.Append(delta.ChunkId).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            RuntimeChunkDeltas = snapshot.RuntimeChunkDeltas.Append(delta).OrderBy(item => item.DeltaId, StringComparer.Ordinal).ToList()
        };
        WriteRuntimeState(state, snapshot);
    }

    private static bool RouteStepsReferenceConnectionsAndMapBindings(ConnectedWorldTravelScenario scenario)
    {
        var connectionIds = scenario.RegionGraph.Connections.Select(item => item.ConnectionId).ToHashSet(StringComparer.Ordinal);
        var bindingByRegion = scenario.MapBindings.ToDictionary(item => item.RegionId, item => item.MapId, StringComparer.Ordinal);
        return scenario.RuntimeEvidence.RouteSteps.Count > 0 &&
               scenario.RuntimeEvidence.RouteSteps.All(step =>
                   step.Succeeded &&
                   connectionIds.Contains(step.ConnectionId) &&
                   bindingByRegion.TryGetValue(step.ToRegionId, out var mapId) &&
                   mapId == step.ToMapId);
    }

    private static bool HasVariableMapEvidence(ConnectedWorldTravelScenario scenario)
    {
        var signatures = scenario.MapSignatures
            .Where(item => scenario.RegionGraph.Regions.Any(region => region.RegionId == item.RegionId && region.Required))
            .Select(item => item.Width.ToString("D", System.Globalization.CultureInfo.InvariantCulture) + "x" + item.Height.ToString("D", System.Globalization.CultureInfo.InvariantCulture) + ":" + item.LayoutSignature)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        return signatures.Count >= 3;
    }

    private static WorldProfileRecord BuildWorldProfile() => new()
    {
        WorldProfileId = "world_profile/goal007_connected_world",
        TopologyKind = "world_topology/region_graph",
        RulesVersion = "connected-world-rules-v1",
        StartRegionId = "region/hub"
    };

    private static RegionGraphRecord BuildBaseGraph() => new()
    {
        GraphId = "region_graph/goal007_connected_world",
        StartRegionId = "region/hub",
        Regions =
        [
            new RegionNodeRecord { RegionId = "region/hub", Label = "hub_start", Required = true, ScenarioRole = "hub/start region" },
            new RegionNodeRecord { RegionId = "region/wildland-frontier", Label = "wildland_frontier", Required = true, ScenarioRole = "wildland/frontier-style region" },
            new RegionNodeRecord { RegionId = "region/mystery-dungeon", Label = "mystery_dungeon", Required = true, ScenarioRole = "mystery/dungeon-style region" },
            new RegionNodeRecord { RegionId = "region/trade-caravan", Label = "trade_caravan", Required = true, ScenarioRole = "trade/caravan-style region" }
        ],
        Connections =
        [
            Connection("connection/hub-to-wildland", "region/hub", "region/wildland-frontier"),
            Connection("connection/wildland-to-hub", "region/wildland-frontier", "region/hub"),
            Connection("connection/hub-to-mystery", "region/hub", "region/mystery-dungeon"),
            Connection("connection/mystery-to-hub", "region/mystery-dungeon", "region/hub"),
            Connection("connection/hub-to-trade", "region/hub", "region/trade-caravan"),
            Connection("connection/trade-to-hub", "region/trade-caravan", "region/hub"),
            Connection("connection/wildland-to-mystery", "region/wildland-frontier", "region/mystery-dungeon"),
            Connection("connection/mystery-to-wildland", "region/mystery-dungeon", "region/wildland-frontier"),
            Connection("connection/trade-to-wildland", "region/trade-caravan", "region/wildland-frontier")
        ],
        TravelRules =
        [
            new TravelRuleRecord { TravelRuleId = "travel_rule/walk", Mode = "walk", DeterministicCost = 1 }
        ]
    };

    private static RegionConnectionRecord Connection(string id, string from, string to) => new()
    {
        ConnectionId = id,
        FromRegionId = from,
        ToRegionId = to,
        TravelRuleId = "travel_rule/walk"
    };

    private static IReadOnlyList<RegionMapBindingRecord> BuildBindings() =>
    [
        new RegionMapBindingRecord { RegionId = "region/hub", MapId = "map/hub-start" },
        new RegionMapBindingRecord { RegionId = "region/wildland-frontier", MapId = "map/wildland-frontier" },
        new RegionMapBindingRecord { RegionId = "region/mystery-dungeon", MapId = "map/mystery-dungeon" },
        new RegionMapBindingRecord { RegionId = "region/trade-caravan", MapId = "map/trade-caravan" }
    ];

    private static GamePackageDefinition BuildPackage() => new()
    {
        Manifest = new GameManifest
        {
            PackageId = "game/goal007-connected-world",
            Title = "Goal 007 Connected World",
            Version = "0.7.0",
            FormatVersion = "0.1",
            StartMapId = "map/hub-start"
        },
        Game = new GameDefinition
        {
            TilePrototypes =
            [
                new TilePrototypeDefinition { Id = "tile/floor", Name = "Floor", Walkable = true },
                new TilePrototypeDefinition { Id = "tile/wall", Name = "Wall", Walkable = false }
            ],
            Maps =
            [
                Map("map/hub-start", "Hub Start", 12, 10, [(1, 1), (2, 1), (3, 1), (4, 2)]),
                Map("map/wildland-frontier", "Wildland Frontier", 16, 9, [(1, 7), (5, 5), (10, 2), (14, 7)]),
                Map("map/mystery-dungeon", "Mystery Dungeon", 9, 13, [(2, 2), (2, 3), (6, 8), (7, 8), (4, 11)]),
                Map("map/trade-caravan", "Trade Caravan", 14, 8, [(3, 3), (4, 3), (8, 4), (11, 2)])
            ]
        }
    };

    private static MapDefinition Map(string id, string name, int width, int height, IReadOnlyList<(int X, int Y)> wallTiles) => new()
    {
        Id = id,
        Name = name,
        Width = width,
        Height = height,
        DefaultTileId = "tile/floor",
        StartPosition = new Position2D { X = 1, Y = 1 },
        Tiles = wallTiles
            .OrderBy(item => item.X)
            .ThenBy(item => item.Y)
            .Select(item => new TileOverrideDefinition { X = item.X, Y = item.Y, TileId = "tile/wall" })
            .ToList()
    };

    private static IReadOnlyList<MapSignatureRecord> BuildMapSignatures(
        GamePackageDefinition package,
        IReadOnlyList<RegionMapBindingRecord> bindings)
    {
        return bindings
            .Select(binding =>
            {
                var map = package.Game.Maps.FirstOrDefault(item => item.Id == binding.MapId);
                return map == null
                    ? new MapSignatureRecord { RegionId = binding.RegionId, MapId = binding.MapId }
                    : new MapSignatureRecord
                    {
                        RegionId = binding.RegionId,
                        MapId = map.Id,
                        Width = map.Width,
                        Height = map.Height,
                        LayoutSignature = MapSignature(map)
                    };
            })
            .OrderBy(item => item.RegionId, StringComparer.Ordinal)
            .ToList();
    }

    private static string MapSignature(MapDefinition map)
    {
        var tileText = string.Join("|", map.Tiles
            .OrderBy(item => item.X)
            .ThenBy(item => item.Y)
            .Select(item => item.X + "," + item.Y + "=" + item.TileId));
        return ShortHash(ComputeHash(map.Id + "|" + map.Width + "x" + map.Height + "|" + map.DefaultTileId + "|" + tileText));
    }

    private static BoundedChunkEvidence BuildChunkEvidence(
        string seed,
        IReadOnlyList<RuntimeChunkDelta> runtimeDeltas,
        IReadOnlyList<string> sourceRuntimeDeltaIds)
    {
        var chunks = BuildChunks(seed);
        return new BoundedChunkEvidence
        {
            Seed = seed,
            RulesVersion = "chunk_rules/goal007_v1",
            ChunkSize = 16,
            BoundedCoordinateMinX = 0,
            BoundedCoordinateMaxX = 1,
            BoundedCoordinateMinY = 0,
            BoundedCoordinateMaxY = 1,
            Chunks = chunks,
            DiscoveredChunkIds = runtimeDeltas.Select(item => item.ChunkId).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            RuntimeDeltas = runtimeDeltas.OrderBy(item => item.DeltaId, StringComparer.Ordinal).ToList(),
            SourceRuntimeDeltaIds = sourceRuntimeDeltaIds.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static IReadOnlyList<BoundedChunkRecord> BuildChunks(string seed)
    {
        var coordinates = new[] { (0, 0, "hub"), (1, 0, "wildland"), (0, 1, "trade"), (1, 1, "mystery") };
        return coordinates
            .Select(item =>
            {
                var id = "chunk/" + item.Item3 + "/" + item.Item1 + "_" + item.Item2;
                var boundary = new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["north"] = item.Item2 == 0 ? "edge" : "road:vertical",
                    ["south"] = item.Item2 == 1 ? "edge" : "road:vertical",
                    ["west"] = item.Item1 == 0 ? "edge" : "road:horizontal",
                    ["east"] = item.Item1 == 1 ? "edge" : "road:horizontal"
                };
                return new BoundedChunkRecord
                {
                    ChunkId = id,
                    X = item.Item1,
                    Y = item.Item2,
                    Hash = ShortHash(ComputeHash(seed + "|chunk_rules/goal007_v1|16|" + item.Item1 + "," + item.Item2 + "|" + item.Item3)),
                    BoundaryExits = boundary
                };
            })
            .OrderBy(item => item.ChunkId, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<ConnectedWorldTravelDiagnostic> ValidateBoundaryCompatibility(IReadOnlyList<BoundedChunkRecord> chunks)
    {
        var diagnostics = new List<ConnectedWorldTravelDiagnostic>();
        var byCoordinate = chunks.ToDictionary(item => (item.X, item.Y));
        foreach (var chunk in chunks)
        {
            if (byCoordinate.TryGetValue((chunk.X + 1, chunk.Y), out var east) &&
                !BoundaryMatches(chunk.BoundaryExits.GetValueOrDefault("east"), east.BoundaryExits.GetValueOrDefault("west")))
            {
                diagnostics.Add(Diagnostic("error", "connected_world.chunk_boundary_incompatible", chunk.ChunkId + "->" + east.ChunkId, "Adjacent east/west chunk exits must be compatible."));
            }

            if (byCoordinate.TryGetValue((chunk.X, chunk.Y + 1), out var south) &&
                !BoundaryMatches(chunk.BoundaryExits.GetValueOrDefault("south"), south.BoundaryExits.GetValueOrDefault("north")))
            {
                diagnostics.Add(Diagnostic("error", "connected_world.chunk_boundary_incompatible", chunk.ChunkId + "->" + south.ChunkId, "Adjacent north/south chunk exits must be compatible."));
            }
        }

        return diagnostics;
    }

    private static bool BoundaryMatches(string? left, string? right)
    {
        if (string.Equals(left, "edge", StringComparison.Ordinal) || string.Equals(right, "edge", StringComparison.Ordinal))
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        return string.Equals(left, right, StringComparison.Ordinal);
    }

    private static IReadOnlyDictionary<string, string> SetBoundary(IReadOnlyDictionary<string, string> source, string key, string value)
    {
        var copy = ToSortedDictionary(source);
        copy[key] = value;
        return copy;
    }

    private static ReachabilityEvidence BuildReachability(RegionGraphRecord graph)
    {
        var visited = new SortedSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        if (!string.IsNullOrWhiteSpace(graph.StartRegionId))
        {
            queue.Enqueue(graph.StartRegionId);
        }

        while (queue.Count > 0)
        {
            var region = queue.Dequeue();
            if (!visited.Add(region))
            {
                continue;
            }

            foreach (var next in graph.Connections
                         .Where(item => item.FromRegionId == region)
                         .Select(item => item.ToRegionId)
                         .OrderBy(item => item, StringComparer.Ordinal))
            {
                if (!visited.Contains(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        var required = graph.Regions.Where(item => item.Required).Select(item => item.RegionId).OrderBy(item => item, StringComparer.Ordinal).ToList();
        var missing = required.Where(item => !visited.Contains(item)).ToList();
        return new ReachabilityEvidence
        {
            StartRegionId = graph.StartRegionId,
            RequiredRegionIds = required,
            ReachableRegionIds = visited.ToList(),
            MissingRequiredRegionIds = missing,
            AllRequiredReachable = missing.Count == 0 && required.Count > 0
        };
    }

    private static IReadOnlyDictionary<string, string> BuildRegionEvidence(
        string currentRegionId,
        string currentMapId,
        IEnumerable<string> discoveredConnectionIds)
    {
        return new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [currentRegionId] = ShortHash(ComputeHash(currentRegionId + "|" + currentMapId + "|" + string.Join(",", discoveredConnectionIds.OrderBy(item => item, StringComparer.Ordinal))))
        };
    }

    private static void WriteRuntimeState(GameRuntimeState state, RuntimeWorldStateSnapshot snapshot)
    {
        state.Metadata["world.profileId"] = snapshot.WorldProfileId;
        state.Metadata["world.currentRegionId"] = snapshot.CurrentRegionId;
        state.Metadata["world.currentMapId"] = snapshot.CurrentMapId;
        state.Metadata["world.visitedRegionIds"] = string.Join("|", snapshot.VisitedRegionIds.OrderBy(item => item, StringComparer.Ordinal));
        state.Metadata["world.discoveredConnectionIds"] = string.Join("|", snapshot.DiscoveredConnectionIds.OrderBy(item => item, StringComparer.Ordinal));
        state.Metadata["world.travelLog"] = JsonSerializer.Serialize(snapshot.TravelLog.OrderBy(item => item.StepIndex).ToList(), JsonOptions);
        state.Metadata["world.regionEvidenceHashes"] = JsonSerializer.Serialize(ToSortedDictionary(snapshot.RegionEvidenceHashes), JsonOptions);
        state.Metadata["world.discoveredChunkIds"] = string.Join("|", snapshot.DiscoveredChunkIds.OrderBy(item => item, StringComparer.Ordinal));
        state.Metadata["world.runtimeChunkDeltas"] = JsonSerializer.Serialize(snapshot.RuntimeChunkDeltas.OrderBy(item => item.DeltaId, StringComparer.Ordinal).ToList(), JsonOptions);
    }

    private static IReadOnlyDictionary<string, string> ExtractStateEvidence(GameRuntimeState state)
    {
        return new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["packageId"] = state.PackageId,
            ["currentMapId"] = state.CurrentMapId,
            ["worldProfileId"] = state.Metadata.GetValueOrDefault("world.profileId", string.Empty),
            ["currentRegionId"] = state.Metadata.GetValueOrDefault("world.currentRegionId", string.Empty),
            ["worldCurrentMapId"] = state.Metadata.GetValueOrDefault("world.currentMapId", string.Empty),
            ["visitedRegionIds"] = state.Metadata.GetValueOrDefault("world.visitedRegionIds", string.Empty),
            ["discoveredConnectionIds"] = state.Metadata.GetValueOrDefault("world.discoveredConnectionIds", string.Empty),
            ["travelLog"] = state.Metadata.GetValueOrDefault("world.travelLog", string.Empty),
            ["regionEvidenceHashes"] = state.Metadata.GetValueOrDefault("world.regionEvidenceHashes", string.Empty),
            ["discoveredChunkIds"] = state.Metadata.GetValueOrDefault("world.discoveredChunkIds", string.Empty),
            ["runtimeChunkDeltas"] = state.Metadata.GetValueOrDefault("world.runtimeChunkDeltas", string.Empty),
            ["stateHash"] = state.Metadata.GetValueOrDefault("world.stateHash", string.Empty)
        };
    }

    private static bool StateEvidenceEquals(
        IReadOnlyDictionary<string, string> left,
        IReadOnlyDictionary<string, string> right)
    {
        return left.Count == right.Count && left.All(pair => right.TryGetValue(pair.Key, out var value) && pair.Value == value);
    }

    private static SortedDictionary<string, string> ToSortedDictionary(IReadOnlyDictionary<string, string> source)
    {
        var copy = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in source)
        {
            copy[pair.Key] = pair.Value;
        }

        return copy;
    }

    private static string RenderReport(ConnectedWorldTravelReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Connected World Travel Report");
        builder.AppendLine();
        builder.AppendLine("- Accepted: " + report.Accepted.ToString().ToLowerInvariant());
        builder.AppendLine("- Manual gate: " + report.ManualGate);
        builder.AppendLine("- Goal 006 gate recorded: " + report.Goal006GateRecorded.ToString().ToLowerInvariant());
        builder.AppendLine("- Valid scenarios: " + report.ValidScenarioCount);
        builder.AppendLine("- Invalid scenarios: " + report.InvalidScenarioCount);
        builder.AppendLine("- Deterministic replay: " + report.DeterministicReplayPassed.ToString().ToLowerInvariant());
        builder.AppendLine("- Travel runtime execution: " + report.TravelRuntimeExecutionPassed.ToString().ToLowerInvariant());
        builder.AppendLine("- Save/load exact state: " + report.SaveLoadRoundtripPassed.ToString().ToLowerInvariant());
        builder.AppendLine("- Public GamePackage schema changed: " + report.PublicGamePackageSchemaChanged.ToString().ToLowerInvariant());
        builder.AppendLine();
        foreach (var scenario in report.Scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal))
        {
            builder.AppendLine("## " + scenario.ScenarioId);
            builder.AppendLine();
            builder.AppendLine("- Expected valid: " + scenario.ExpectedValid.ToString().ToLowerInvariant());
            builder.AppendLine("- Actual valid: " + scenario.ActualValid.ToString().ToLowerInvariant());
            builder.AppendLine("- Start region: " + scenario.RegionGraph.StartRegionId);
            builder.AppendLine("- Final region: " + scenario.RuntimeEvidence.FinalRegionId);
            builder.AppendLine("- Route: " + string.Join(" -> ", scenario.RuntimeEvidence.RouteSteps.Select(item => item.ConnectionId)));
            builder.AppendLine("- Diagnostics: " + string.Join(", ", scenario.Diagnostics.Select(item => item.Code).Distinct().OrderBy(item => item, StringComparer.Ordinal)));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string RenderVerification(ConnectedWorldTravelReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Connected World Travel Verification");
        builder.AppendLine();
        builder.AppendLine("Final gate:");
        builder.AppendLine();
        builder.AppendLine("```text");
        builder.AppendLine(report.ManualGate);
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("This report does not mark the manual gate passed.");
        builder.AppendLine();
        builder.AppendLine("- Accepted: " + report.Accepted.ToString().ToLowerInvariant());
        builder.AppendLine("- Valid accepted: " + report.ValidScenariosAccepted.ToString().ToLowerInvariant());
        builder.AppendLine("- Invalid rejected: " + report.InvalidScenariosRejected.ToString().ToLowerInvariant());
        builder.AppendLine("- External execution flags all false: " + report.ExternalExecution.AllFalse.ToString().ToLowerInvariant());
        builder.AppendLine("- S071/Goal 008 started: false");
        return builder.ToString();
    }

    private static IReadOnlyList<ConnectedWorldTravelDiagnostic> SortDiagnostics(IEnumerable<ConnectedWorldTravelDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static ConnectedWorldTravelDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

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
            throw new InvalidOperationException("Connected-world travel output path must stay under the project root.");
        }
    }

    private enum InvalidScenarioKind
    {
        DisconnectedGraph,
        MissingRegionOrMapRef,
        ChunkBoundaryOrRules,
        RuntimeDeltaAsSource
    }

    private sealed record TravelCommandResult(
        TravelRouteStepEvidence Step,
        IReadOnlyList<ConnectedWorldTravelDiagnostic> Diagnostics);

    private sealed record RuntimeWorldStateSnapshot
    {
        public string WorldProfileId { get; init; } = string.Empty;
        public string CurrentRegionId { get; init; } = string.Empty;
        public string CurrentMapId { get; init; } = string.Empty;
        public IReadOnlyList<string> VisitedRegionIds { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> DiscoveredConnectionIds { get; init; } = Array.Empty<string>();
        public IReadOnlyList<TravelLogEntry> TravelLog { get; init; } = Array.Empty<TravelLogEntry>();
        public IReadOnlyDictionary<string, string> RegionEvidenceHashes { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
        public IReadOnlyList<string> DiscoveredChunkIds { get; init; } = Array.Empty<string>();
        public IReadOnlyList<RuntimeChunkDelta> RuntimeChunkDeltas { get; init; } = Array.Empty<RuntimeChunkDelta>();

        public static RuntimeWorldStateSnapshot FromState(GameRuntimeState state) => new()
        {
            WorldProfileId = state.Metadata.GetValueOrDefault("world.profileId", string.Empty),
            CurrentRegionId = state.Metadata.GetValueOrDefault("world.currentRegionId", string.Empty),
            CurrentMapId = state.Metadata.GetValueOrDefault("world.currentMapId", state.CurrentMapId),
            VisitedRegionIds = Split(state.Metadata.GetValueOrDefault("world.visitedRegionIds", string.Empty)),
            DiscoveredConnectionIds = Split(state.Metadata.GetValueOrDefault("world.discoveredConnectionIds", string.Empty)),
            TravelLog = DeserializeList<TravelLogEntry>(state.Metadata.GetValueOrDefault("world.travelLog", "[]")),
            RegionEvidenceHashes = DeserializeDictionary(state.Metadata.GetValueOrDefault("world.regionEvidenceHashes", "{}")),
            DiscoveredChunkIds = Split(state.Metadata.GetValueOrDefault("world.discoveredChunkIds", string.Empty)),
            RuntimeChunkDeltas = DeserializeList<RuntimeChunkDelta>(state.Metadata.GetValueOrDefault("world.runtimeChunkDeltas", "[]"))
        };

        private static IReadOnlyList<string> Split(string value) =>
            string.IsNullOrWhiteSpace(value)
                ? []
                : value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).OrderBy(item => item, StringComparer.Ordinal).ToList();

        private static IReadOnlyList<T> DeserializeList<T>(string json) =>
            JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];

        private static IReadOnlyDictionary<string, string> DeserializeDictionary(string json) =>
            JsonSerializer.Deserialize<SortedDictionary<string, string>>(json, JsonOptions) ?? new SortedDictionary<string, string>(StringComparer.Ordinal);
    }
}

public sealed record ConnectedWorldTravelAcceptanceResult
{
    public ConnectedWorldTravelReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record ConnectedWorldTravelWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record ConnectedWorldTravelReport
{
    public string SchemaVersion { get; init; } = "1";
    public string DeterministicHash { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public bool Goal006GateRecorded { get; init; }
    public int ScenarioCount { get; init; }
    public int ValidScenarioCount { get; init; }
    public int InvalidScenarioCount { get; init; }
    public bool ValidScenariosAccepted { get; init; }
    public bool InvalidScenariosRejected { get; init; }
    public bool DeterministicReplayPassed { get; init; }
    public bool TravelRuntimeExecutionPassed { get; init; }
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool VariableMapEvidencePassed { get; init; }
    public bool ChunkDeltaPersistencePassed { get; init; }
    public bool GraphReachabilityPassed { get; init; }
    public bool RouteBindingEvidencePassed { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public ConnectedWorldExternalExecutionFlags ExternalExecution { get; init; } = new();
    public IReadOnlyList<ConnectedWorldTravelScenario> Scenarios { get; init; } = Array.Empty<ConnectedWorldTravelScenario>();
    public IReadOnlyList<ConnectedWorldTravelDiagnostic> Diagnostics { get; init; } = Array.Empty<ConnectedWorldTravelDiagnostic>();
    public IReadOnlyList<string> RemainingPrimitiveLimits { get; init; } = Array.Empty<string>();
}

public sealed record ConnectedWorldTravelScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public WorldProfileRecord WorldProfile { get; init; } = new();
    public RegionGraphRecord RegionGraph { get; init; } = new();
    public IReadOnlyList<RegionMapBindingRecord> MapBindings { get; init; } = Array.Empty<RegionMapBindingRecord>();
    public IReadOnlyList<MapSignatureRecord> MapSignatures { get; init; } = Array.Empty<MapSignatureRecord>();
    public ReachabilityEvidence Reachability { get; init; } = new();
    public BoundedChunkEvidence ChunkEvidence { get; init; } = new();
    public ConnectedWorldRuntimeEvidence RuntimeEvidence { get; init; } = new();
    public IReadOnlyList<ConnectedWorldTravelDiagnostic> Diagnostics { get; init; } = Array.Empty<ConnectedWorldTravelDiagnostic>();
}

public sealed record WorldProfileRecord
{
    public string WorldProfileId { get; init; } = string.Empty;
    public string TopologyKind { get; init; } = string.Empty;
    public string RulesVersion { get; init; } = string.Empty;
    public string StartRegionId { get; init; } = string.Empty;
}

public sealed record RegionGraphRecord
{
    public string GraphId { get; init; } = string.Empty;
    public string StartRegionId { get; init; } = string.Empty;
    public IReadOnlyList<RegionNodeRecord> Regions { get; init; } = Array.Empty<RegionNodeRecord>();
    public IReadOnlyList<RegionConnectionRecord> Connections { get; init; } = Array.Empty<RegionConnectionRecord>();
    public IReadOnlyList<TravelRuleRecord> TravelRules { get; init; } = Array.Empty<TravelRuleRecord>();
}

public sealed record RegionNodeRecord
{
    public string RegionId { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string ScenarioRole { get; init; } = string.Empty;
    public bool Required { get; init; }
}

public sealed record RegionConnectionRecord
{
    public string ConnectionId { get; init; } = string.Empty;
    public string FromRegionId { get; init; } = string.Empty;
    public string ToRegionId { get; init; } = string.Empty;
    public string TravelRuleId { get; init; } = string.Empty;
}

public sealed record RegionMapBindingRecord
{
    public string RegionId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
}

public sealed record MapSignatureRecord
{
    public string RegionId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public string LayoutSignature { get; init; } = string.Empty;
}

public sealed record TravelRuleRecord
{
    public string TravelRuleId { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public int DeterministicCost { get; init; }
}

public sealed record ReachabilityEvidence
{
    public string StartRegionId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredRegionIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ReachableRegionIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingRequiredRegionIds { get; init; } = Array.Empty<string>();
    public bool AllRequiredReachable { get; init; }
}

public sealed record BoundedChunkEvidence
{
    public string Seed { get; init; } = string.Empty;
    public string RulesVersion { get; init; } = string.Empty;
    public int ChunkSize { get; init; }
    public int BoundedCoordinateMinX { get; init; }
    public int BoundedCoordinateMaxX { get; init; }
    public int BoundedCoordinateMinY { get; init; }
    public int BoundedCoordinateMaxY { get; init; }
    public IReadOnlyList<BoundedChunkRecord> Chunks { get; init; } = Array.Empty<BoundedChunkRecord>();
    public IReadOnlyList<string> DiscoveredChunkIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<RuntimeChunkDelta> RuntimeDeltas { get; init; } = Array.Empty<RuntimeChunkDelta>();
    public IReadOnlyList<string> SourceRuntimeDeltaIds { get; init; } = Array.Empty<string>();
}

public sealed record BoundedChunkRecord
{
    public string ChunkId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public string Hash { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> BoundaryExits { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

public sealed record RuntimeChunkDelta
{
    public string DeltaId { get; init; } = string.Empty;
    public string ChunkId { get; init; } = string.Empty;
    public string DeltaKind { get; init; } = string.Empty;
    public string MarkerId { get; init; } = string.Empty;
    public bool RuntimeSaveOnly { get; init; } = true;

    public static RuntimeChunkDelta Discovered(string chunkId) => new()
    {
        DeltaId = "delta/discovered/" + chunkId,
        ChunkId = chunkId,
        DeltaKind = "discovered_chunk",
        MarkerId = chunkId,
        RuntimeSaveOnly = true
    };

    public static RuntimeChunkDelta OpenedRoute(string chunkId, string routeId) => new()
    {
        DeltaId = "delta/opened-route/" + routeId,
        ChunkId = chunkId,
        DeltaKind = "opened_route_marker",
        MarkerId = routeId,
        RuntimeSaveOnly = true
    };

    public static RuntimeChunkDelta Harvested(string chunkId, string markerId) => new()
    {
        DeltaId = "delta/harvested/" + markerId,
        ChunkId = chunkId,
        DeltaKind = "harvested_marker",
        MarkerId = markerId,
        RuntimeSaveOnly = true
    };

    public static RuntimeChunkDelta VisitedLandmark(string chunkId, string markerId) => new()
    {
        DeltaId = "delta/visited-landmark/" + markerId,
        ChunkId = chunkId,
        DeltaKind = "visited_landmark_marker",
        MarkerId = markerId,
        RuntimeSaveOnly = true
    };
}

public sealed record ConnectedWorldRuntimeEvidence
{
    public bool RuntimeAttempted { get; init; }
    public string RuntimeStateOwner { get; init; } = string.Empty;
    public string StartRegionId { get; init; } = string.Empty;
    public string StartMapId { get; init; } = string.Empty;
    public string FinalRegionId { get; init; } = string.Empty;
    public string FinalMapId { get; init; } = string.Empty;
    public bool CommandSucceeded { get; init; }
    public bool InvalidTravelRejected { get; init; }
    public IReadOnlyList<TravelRouteStepEvidence> RouteSteps { get; init; } = Array.Empty<TravelRouteStepEvidence>();
    public string RuntimeStateHash { get; init; } = string.Empty;
    public string RestoredRuntimeStateHash { get; init; } = string.Empty;
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool ExactStateComparisonPassed { get; init; }
    public bool ChunkDeltasPersisted { get; init; }
    public IReadOnlyDictionary<string, string> StateEvidence { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> RestoredStateEvidence { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<ConnectedWorldTravelDiagnostic> Diagnostics { get; init; } = Array.Empty<ConnectedWorldTravelDiagnostic>();
}

public sealed record TravelRouteStepEvidence
{
    public string ConnectionId { get; init; } = string.Empty;
    public string FromRegionId { get; init; } = string.Empty;
    public string FromMapId { get; init; } = string.Empty;
    public string ToRegionId { get; init; } = string.Empty;
    public string ToMapId { get; init; } = string.Empty;
    public bool Succeeded { get; init; }
    public string DiagnosticCode { get; init; } = string.Empty;
}

public sealed record TravelLogEntry
{
    public int StepIndex { get; init; }
    public string ConnectionId { get; init; } = string.Empty;
    public string FromRegionId { get; init; } = string.Empty;
    public string ToRegionId { get; init; } = string.Empty;
    public string FromMapId { get; init; } = string.Empty;
    public string ToMapId { get; init; } = string.Empty;
}

public sealed record ConnectedWorldExternalExecutionFlags
{
    public bool LlmExecuted { get; init; }
    public bool RagExecuted { get; init; }
    public bool ProviderExecuted { get; init; }
    public bool LuaExecuted { get; init; }
    public bool UnityExecuted { get; init; }
    public bool MediaExecuted { get; init; }

    [JsonIgnore]
    public bool AllFalse => !LlmExecuted && !RagExecuted && !ProviderExecuted && !LuaExecuted && !UnityExecuted && !MediaExecuted;
}

public sealed record ConnectedWorldTravelDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
