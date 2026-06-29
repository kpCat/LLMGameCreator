using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;

namespace LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;

public sealed class RuntimeChunkDeltaValidator
{
    public IReadOnlyList<RuntimeChunkDeltaDiagnostic> ValidatePlan(RuntimeChunkTraversalPlan plan)
    {
        var diagnostics = new List<RuntimeChunkDeltaDiagnostic>();
        var graphs = WorldScaleRegionMapCatalog.BuildDefaultGraphs();
        var graph = graphs.SingleOrDefault(item => item.ScenarioId == plan.ScenarioId);
        var mapPacks = new FiniteMapPackBuilder().BuildMapPacksByFileName(graphs);
        var chunkConfig = new ChunkedWorldConfigPreludeBuilder().Build(graphs, mapPacks);

        if (!RuntimeChunkDeltaTraversalVocabulary.Scenarios.Contains(plan.ScenarioId) || graph == null)
        {
            diagnostics.Add(Error("runtime_chunk.goal038_scenario.fake", plan.ScenarioId, "Traversal plan must reference a known Goal038 scenario id."));
            return SortDiagnostics(diagnostics);
        }

        var mapPack = mapPacks[FiniteMapPackBuilder.FileName(plan.ScenarioId)];
        var scenarioChunkConfig = chunkConfig.Scenarios.Single(item => item.ScenarioId == plan.ScenarioId);
        var regionIds = graph.Regions.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);
        var edgeIds = graph.TravelEdges.Where(item => item.IsTraversableNow).Select(item => item.EdgeId).ToHashSet(StringComparer.Ordinal);
        var chunkIds = scenarioChunkConfig.FiniteMapProjection.CoveredChunkIds.ToHashSet(StringComparer.Ordinal);
        var bindingByRegion = mapPack.RegionBindings.ToDictionary(item => item.RegionId, StringComparer.Ordinal);

        if (!string.Equals(plan.WorldGraphId, graph.WorldGraphId, StringComparison.Ordinal)
            || !string.Equals(plan.FiniteMapId, mapPack.MapId, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("runtime_chunk.goal038_source.mismatch", plan.ScenarioId, "Plan must consume the matching Goal038 graph and finite map ids."));
        }

        if (!IsSorted(plan.Steps.Select(item => item.StepIndex.ToString("000000"))))
        {
            diagnostics.Add(Error("runtime_chunk.order.nondeterministic", plan.ScenarioId, "Traversal steps must be in stable order."));
        }

        foreach (var step in plan.Steps.OrderBy(item => item.StepIndex))
        {
            if (!regionIds.Contains(step.RegionId))
            {
                diagnostics.Add(Error("runtime_chunk.region.unknown", step.RegionId, "Traversal step references an unknown Goal038 region id."));
            }

            if (!chunkIds.Contains(step.ChunkId))
            {
                diagnostics.Add(Error("runtime_chunk.chunk.unknown", step.ChunkId, "Traversal step references an unknown Goal038 chunk id."));
            }

            if (!string.IsNullOrWhiteSpace(step.ArrivedByEdgeId) && !edgeIds.Contains(step.ArrivedByEdgeId))
            {
                diagnostics.Add(Error("runtime_chunk.route.edge_unreachable", step.ArrivedByEdgeId, "Traversal step uses an edge outside the current reachability plan."));
            }

            if (bindingByRegion.TryGetValue(step.RegionId, out var binding)
                && !string.Equals(step.Coordinate, binding.AnchorCell, StringComparison.Ordinal))
            {
                diagnostics.Add(Error("runtime_chunk.coordinate.out_of_bounds", binding.RegionId, "Chunk coordinate is outside finite/chunk config bounds."));
            }
        }

        ValidateCommands(plan.Commands, plan, diagnostics);
        AddBoundaryDiagnostics(plan.BoundaryClaims, plan.ScenarioId, diagnostics);
        return SortDiagnostics(diagnostics);
    }

    public RuntimeChunkInvalidMatrix BuildInvalidMatrix(
        IReadOnlyList<RuntimeChunkTraversalPlan> validPlans,
        RuntimeChunkSaveLoadRoundtripProof saveLoadProof)
    {
        var frontier = validPlans.Single(item => item.ScenarioId == "frontier_survival");
        var firstStep = frontier.Steps.First(item => !string.IsNullOrWhiteSpace(item.ArrivedByEdgeId));
        var firstCommand = frontier.Commands.First(item => item.DeltaKind == "local_mutation");
        var invalid = new List<RuntimeChunkInvalidScenario>
        {
            PlanInvalid("fake_goal038_scenario_id", "fake Goal038 scenario id", frontier with { ScenarioId = "fake_goal038_scenario" }),
            PlanInvalid("fake_region_id", "fake region id", frontier with { Steps = frontier.Steps.Select((item, index) => index == 0 ? item with { RegionId = "region/fake/missing" } : item).ToList() }),
            PlanInvalid("fake_chunk_id", "fake chunk id", frontier with { Steps = frontier.Steps.Select((item, index) => index == 0 ? item with { ChunkId = "chunk/fake/missing/primary" } : item).ToList() }),
            PlanInvalid("route_edge_not_in_reachability_plan", "route edge not in reachability plan", frontier with { Steps = frontier.Steps.Select(item => item.StepIndex == firstStep.StepIndex ? item with { ArrivedByEdgeId = "edge/frontier/not-in-plan" } : item).ToList() }),
            PlanInvalid("chunk_coordinate_outside_bounds", "chunk coordinate outside finite/chunk config bounds", frontier with { Steps = frontier.Steps.Select((item, index) => index == 0 ? item with { Coordinate = "x9999:y9999" } : item).ToList() }),
            CommandInvalid("duplicate_delta_id", "duplicate delta id", frontier with { Commands = frontier.Commands.Concat([frontier.Commands[0]]).ToList() }),
            CommandInvalid("conflicting_delta_mutation", "conflicting delta mutation", frontier with { Commands = frontier.Commands.Select(command => command.DeltaId == firstCommand.DeltaId ? command with { MutationValue = "conflicting_value" } : command).Concat([firstCommand]).ToList() }),
            CommandInvalid("replay_seed_mismatch", "replay seed mismatch", frontier with { Commands = frontier.Commands.Select((command, index) => index == 0 ? command with { ReplaySeed = "wrong-seed" } : command).ToList() }),
            BoundaryInvalid("mutation_tries_to_edit_gamepackage_definitions", "mutation tries to edit GamePackage/package definitions", frontier with { BoundaryClaims = new RuntimeChunkBoundaryClaims { GamePackageDefinitionsMutation = true } }, "blocked"),
            BoundaryInvalid("runtime_ui_unity_provider_llm_rag_lua_generator_library_leakage", "Runtime/UI/Unity/provider/LLM/RAG/Lua source/generator-library leakage", frontier with { BoundaryClaims = new RuntimeChunkBoundaryClaims { RuntimeSourceMutation = true, UiWinForms = true, Unity = true, ProviderLlmRag = true, LuaSourceOrExecution = true, GeneratorLibrary = true } }, "blocked"),
            BoundaryInvalid("filesystem_network_process_reflection_thread_time_random_native_interop_leakage", "filesystem/network/process/reflection/thread/time/random/native interop leakage", frontier with { BoundaryClaims = new RuntimeChunkBoundaryClaims { Filesystem = true, Network = true, Process = true, Reflection = true, Thread = true, Time = true, Random = true, NativeInterop = true } }, "blocked"),
            MissingSaveLoadInvalid(saveLoadProof),
            CommandInvalid("nondeterministic_ordering", "nondeterministic ordering", frontier with { Commands = frontier.Commands.AsEnumerable().Reverse().ToList() })
        };

        return new RuntimeChunkInvalidMatrix
        {
            ScenarioCount = invalid.Count,
            MatchedExpectationCount = invalid.Count(item => item.ExpectedStatus == item.ActualStatus),
            RejectedCount = invalid.Count(item => item.ActualStatus == "rejected"),
            BlockedCount = invalid.Count(item => item.ActualStatus == "blocked"),
            Passed = invalid.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            Scenarios = invalid.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private RuntimeChunkInvalidScenario PlanInvalid(string scenarioId, string kind, RuntimeChunkTraversalPlan plan)
    {
        var diagnostics = ValidatePlan(plan).Where(item => item.Severity == "error").ToList();
        return Invalid(scenarioId, kind, "rejected", diagnostics);
    }

    private RuntimeChunkInvalidScenario CommandInvalid(string scenarioId, string kind, RuntimeChunkTraversalPlan plan)
    {
        var diagnostics = new List<RuntimeChunkDeltaDiagnostic>();
        ValidateCommands(plan.Commands, plan, diagnostics);
        return Invalid(scenarioId, kind, "rejected", diagnostics.Where(item => item.Severity == "error").ToList());
    }

    private RuntimeChunkInvalidScenario BoundaryInvalid(string scenarioId, string kind, RuntimeChunkTraversalPlan plan, string expectedStatus)
    {
        var diagnostics = new List<RuntimeChunkDeltaDiagnostic>();
        AddBoundaryDiagnostics(plan.BoundaryClaims, plan.ScenarioId, diagnostics);
        return Invalid(scenarioId, kind, expectedStatus, diagnostics.Where(item => item.Severity == "error").ToList());
    }

    private static RuntimeChunkInvalidScenario MissingSaveLoadInvalid(RuntimeChunkSaveLoadRoundtripProof saveLoadProof)
    {
        var diagnostics = saveLoadProof.Passed
            ? new List<RuntimeChunkDeltaDiagnostic>
            {
                Error("runtime_chunk.persistence.missing", "save_load_roundtrip", "A missing save/load proof must reject traversal acceptance.")
            }
            : saveLoadProof.Scenarios.SelectMany(item => item.Diagnostics).DefaultIfEmpty(
                Error("runtime_chunk.persistence.missing", "save_load_roundtrip", "Save/load proof is missing or failed.")).ToList();

        return Invalid("missing_save_load_proof", "missing save/load proof", "rejected", diagnostics.ToList());
    }

    private static void ValidateCommands(
        IReadOnlyList<RuntimeChunkDeltaCommand> commands,
        RuntimeChunkTraversalPlan plan,
        List<RuntimeChunkDeltaDiagnostic> diagnostics)
    {
        foreach (var duplicate in commands.GroupBy(item => item.DeltaId, StringComparer.Ordinal).Where(group => group.Count() > 1).Select(group => group.Key).Order(StringComparer.Ordinal))
        {
            diagnostics.Add(Error("runtime_chunk.delta.duplicate", duplicate, "Runtime chunk delta ids must be unique."));
        }

        var order = commands.Select(item => item.Order).ToList();
        if (!order.SequenceEqual(order.Order()))
        {
            diagnostics.Add(Error("runtime_chunk.order.nondeterministic", plan.ScenarioId, "Runtime chunk delta commands must be ordered by deterministic sequence."));
        }

        foreach (var command in commands.OrderBy(item => item.Order))
        {
            if (!string.Equals(command.ScenarioId, plan.ScenarioId, StringComparison.Ordinal))
            {
                diagnostics.Add(Error("runtime_chunk.command.scenario_mismatch", command.DeltaId, "Runtime delta command scenario must match traversal plan."));
            }

            if (!string.Equals(command.ReplaySeed, plan.ReplaySeed, StringComparison.Ordinal))
            {
                diagnostics.Add(Error("runtime_chunk.replay.seed_mismatch", command.DeltaId, "Runtime delta replay seed must match the traversal plan seed."));
            }
        }

        var mutationGroups = commands
            .Where(item => item.DeltaKind == "local_mutation")
            .GroupBy(item => item.MutationKey, StringComparer.Ordinal);
        foreach (var group in mutationGroups)
        {
            if (group.Select(item => item.MutationValue).Distinct(StringComparer.Ordinal).Count() > 1)
            {
                diagnostics.Add(Error("runtime_chunk.delta.conflict", group.Key, "A runtime chunk mutation has conflicting values."));
            }
        }
    }

    private static void AddBoundaryDiagnostics(
        RuntimeChunkBoundaryClaims claims,
        string target,
        List<RuntimeChunkDeltaDiagnostic> diagnostics)
    {
        if (claims.RuntimeSourceMutation) diagnostics.Add(Error("runtime_chunk.boundary.runtime_source.forbidden", target, "Runtime source mutation is forbidden for this Goal039 proof."));
        if (claims.UiWinForms) diagnostics.Add(Error("runtime_chunk.boundary.ui.forbidden", target, "WinForms/UI changes are forbidden."));
        if (claims.Unity) diagnostics.Add(Error("runtime_chunk.boundary.unity.forbidden", target, "Unity changes are forbidden."));
        if (claims.GamePackageDefinitionsMutation) diagnostics.Add(Error("runtime_chunk.boundary.gamepackage.forbidden", target, "Runtime chunk deltas must not mutate GamePackage definitions."));
        if (claims.ProviderLlmRag) diagnostics.Add(Error("runtime_chunk.boundary.provider_llm_rag.forbidden", target, "Provider/LLM/RAG calls are forbidden."));
        if (claims.LuaSourceOrExecution) diagnostics.Add(Error("runtime_chunk.boundary.lua.forbidden", target, "Lua source/execution changes are forbidden."));
        if (claims.GeneratorLibrary) diagnostics.Add(Error("runtime_chunk.boundary.generator_library.forbidden", target, "Generator-library changes are forbidden."));
        if (claims.ExternalDependency) diagnostics.Add(Error("runtime_chunk.boundary.external_dependency.forbidden", target, "External dependencies are forbidden."));
        if (claims.Filesystem) diagnostics.Add(Error("runtime_chunk.boundary.filesystem.forbidden", target, "Filesystem leakage is forbidden outside the deterministic evidence writer and runtime snapshot adapter."));
        if (claims.Network) diagnostics.Add(Error("runtime_chunk.boundary.network.forbidden", target, "Network access is forbidden."));
        if (claims.Process) diagnostics.Add(Error("runtime_chunk.boundary.process.forbidden", target, "Process execution is forbidden."));
        if (claims.Reflection) diagnostics.Add(Error("runtime_chunk.boundary.reflection.forbidden", target, "Reflection leakage is forbidden."));
        if (claims.Thread) diagnostics.Add(Error("runtime_chunk.boundary.thread.forbidden", target, "Thread leakage is forbidden."));
        if (claims.Time) diagnostics.Add(Error("runtime_chunk.boundary.time.forbidden", target, "Time-dependent evidence is forbidden."));
        if (claims.Random) diagnostics.Add(Error("runtime_chunk.boundary.random.forbidden", target, "Nondeterministic random evidence is forbidden."));
        if (claims.NativeInterop) diagnostics.Add(Error("runtime_chunk.boundary.native_interop.forbidden", target, "Native interop leakage is forbidden."));
    }

    private static RuntimeChunkInvalidScenario Invalid(
        string scenarioId,
        string kind,
        string expectedStatus,
        IReadOnlyList<RuntimeChunkDeltaDiagnostic> diagnostics)
    {
        var actualStatus = diagnostics.Any(item => item.Code.Contains(".boundary.", StringComparison.Ordinal))
            ? "blocked"
            : diagnostics.Any(item => item.Severity == "error")
                ? "rejected"
                : "accepted";

        return new RuntimeChunkInvalidScenario
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

    private static bool CoordinateWithinBounds(string coordinate, WorldScaleFiniteMapPack mapPack)
    {
        var parsed = ParseCoordinate(coordinate);
        if (parsed == null)
        {
            return false;
        }

        var (first, second) = parsed.Value;
        return mapPack.CoordinateKind == "axial_hex"
            ? first >= mapPack.MinQ && first <= mapPack.MaxQ && second >= mapPack.MinR && second <= mapPack.MaxR
            : first >= 0 && first <= mapPack.Width && second >= 0 && second <= mapPack.Height;
    }

    private static (int First, int Second)? ParseCoordinate(string coordinate)
    {
        var separator = coordinate.IndexOf(':', StringComparison.Ordinal);
        if (separator <= 1 || separator >= coordinate.Length - 2)
        {
            return null;
        }

        var left = coordinate[..separator];
        var right = coordinate[(separator + 1)..];
        var firstText = left[0] is 'x' or 'q' ? left[1..] : left;
        var secondText = right[0] is 'y' or 'r' ? right[1..] : right;
        return int.TryParse(firstText, out var first) && int.TryParse(secondText, out var second)
            ? (first, second)
            : null;
    }

    private static bool IsSorted(IEnumerable<string> values)
    {
        var list = values.ToList();
        return list.SequenceEqual(list.Order(StringComparer.Ordinal), StringComparer.Ordinal);
    }

    private static RuntimeChunkDeltaDiagnostic Error(string code, string target, string message) =>
        RuntimeChunkDeltaDiagnostic.Error(code, target, message);

    public static IReadOnlyList<RuntimeChunkDeltaDiagnostic> SortDiagnostics(IEnumerable<RuntimeChunkDeltaDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity == "error" ? 0 : 1)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();
}
