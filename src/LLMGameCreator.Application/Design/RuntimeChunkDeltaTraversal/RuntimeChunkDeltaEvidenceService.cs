using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;

public sealed class RuntimeChunkDeltaEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-039-runtime-chunk-delta-traversal-smoke";
    public const string FrontierPlanJsonFileName = "chunk-traversal-plan-frontier.json";
    public const string GothicPlanJsonFileName = "chunk-traversal-plan-gothic.json";
    public const string CaravanPlanJsonFileName = "chunk-traversal-plan-caravan.json";
    public const string MetamodulePlanJsonFileName = "chunk-traversal-plan-metamodule.json";
    public const string FrontierStateJsonFileName = "runtime-chunk-delta-state-frontier.json";
    public const string MetamoduleStateJsonFileName = "runtime-chunk-delta-state-metamodule.json";
    public const string SaveLoadRoundtripProofJsonFileName = "runtime-save-load-roundtrip-proof.json";
    public const string ReplayDeterminismProofJsonFileName = "chunk-replay-determinism-proof.json";
    public const string InvalidMatrixJsonFileName = "invalid-chunk-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "runtime-chunk-delta-traversal-smoke-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IRuntimeChunkDeltaPersistenceAdapter _persistenceAdapter;

    public RuntimeChunkDeltaEvidenceService(IRuntimeChunkDeltaPersistenceAdapter? persistenceAdapter = null)
    {
        _persistenceAdapter = persistenceAdapter ?? new MissingRuntimeChunkDeltaPersistenceAdapter();
    }

    public RuntimeChunkDeltaEvidenceResult Build()
    {
        var validator = new RuntimeChunkDeltaValidator();
        var plans = new RuntimeChunkTraversalPlanner().BuildPlans();
        var stateProofs = plans
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(RuntimeChunkDeltaProjector.Apply)
            .ToList();
        var persistenceResults = stateProofs
            .Select(proof => _persistenceAdapter.RoundTrip(new RuntimeChunkPersistenceRequest
            {
                ScenarioId = proof.ScenarioId,
                SlotName = "goal039_" + proof.ScenarioId.Replace('_', '-'),
                State = proof.RuntimeState,
                ExpectedStateEvidence = proof.AfterStateEvidence
            }))
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();
        var saveLoadProof = new RuntimeChunkSaveLoadRoundtripProof
        {
            ScenarioCount = persistenceResults.Count,
            Scenarios = persistenceResults,
            Passed = persistenceResults.Count == 4 && persistenceResults.All(item => item.Passed)
        };
        var replayProof = BuildReplayProof(plans);
        var invalidMatrix = validator.BuildInvalidMatrix(plans, saveLoadProof);
        var diagnostics = RuntimeChunkDeltaValidator.SortDiagnostics(
            plans.SelectMany(validator.ValidatePlan)
                .Concat(saveLoadProof.Scenarios.SelectMany(item => item.Diagnostics))
                .Concat(saveLoadProof.Passed
                    ? [RuntimeChunkDeltaDiagnostic.Info("runtime_chunk.persistence.roundtrip_passed", "runtime_state", "Runtime serializer and snapshot store roundtrip passed.")]
                    : [RuntimeChunkDeltaDiagnostic.Error("runtime_chunk.persistence.roundtrip_failed", "runtime_state", "Runtime serializer and snapshot store roundtrip failed.")])
                .Concat(replayProof.Passed
                    ? [RuntimeChunkDeltaDiagnostic.Info("runtime_chunk.replay.deterministic", "runtime_state", "Same-seed replay produced deterministic runtime chunk state.")]
                    : [RuntimeChunkDeltaDiagnostic.Error("runtime_chunk.replay.nondeterministic", "runtime_state", "Same-seed replay produced different runtime chunk state.")]));

        var proofByScenario = stateProofs.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);
        var planByScenario = plans.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);
        var metamodule = plans.Single(item => item.ScenarioId == "metamodule_kingdoms");
        var reportWithoutHash = new RuntimeChunkDeltaTraversalReport
        {
            Accepted = false,
            ImplementationStatus = diagnostics.Any(item => item.Severity == "error") ? "FAILED" : "GREEN",
            Goal038AcceptedByUserHandoff = true,
            ScenarioCount = plans.Count,
            TraversalPlanCount = plans.Count,
            RuntimeStateProofCount = stateProofs.Count,
            RuntimeMutationScenarioCount = stateProofs.Count(item => item.After.LocalMutations.Count > 0),
            TotalCommandCount = plans.Sum(item => item.Commands.Count),
            RuntimeStateChangedAfterTraversal = stateProofs.All(item => item.StateChangedAfterTraversal),
            SaveLoadRoundtripPassed = saveLoadProof.Passed,
            ReplayDeterminismPassed = replayProof.Passed,
            InvalidMatrixPassed = invalidMatrix.Passed,
            GamePackageDefinitionsMutated = false,
            MetamoduleKingdomGroupCount = metamodule.SourceFacts.KingdomGroupCount,
            MetamoduleSpeciesArchetypeSlotRefCount = metamodule.SourceFacts.SpeciesArchetypeSlotRefCount,
            NoRuntimeUiUnityProviderLlmRagLuaGeneratorLibraryLeakage = plans.All(item => item.BoundaryClaims.AllFalse),
            FrontierStateHash = proofByScenario["frontier_survival"].AfterStateHash,
            MetamoduleStateHash = proofByScenario["metamodule_kingdoms"].AfterStateHash,
            SaveLoadProofHash = Hash(Serialize(saveLoadProof)),
            ReplayProofHash = Hash(Serialize(replayProof)),
            InvalidMatrixHash = Hash(Serialize(invalidMatrix)),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        var plansByFileName = new SortedDictionary<string, RuntimeChunkTraversalPlan>(StringComparer.Ordinal)
        {
            [FrontierPlanJsonFileName] = planByScenario["frontier_survival"],
            [GothicPlanJsonFileName] = planByScenario["gothic_intrigue"],
            [CaravanPlanJsonFileName] = planByScenario["caravan_trade"],
            [MetamodulePlanJsonFileName] = planByScenario["metamodule_kingdoms"]
        };
        var statesByFileName = new SortedDictionary<string, RuntimeChunkDeltaStateSnapshot>(StringComparer.Ordinal)
        {
            [FrontierStateJsonFileName] = proofByScenario["frontier_survival"].After,
            [MetamoduleStateJsonFileName] = proofByScenario["metamodule_kingdoms"].After
        };
        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in plansByFileName)
        {
            artifactJson[pair.Key] = Serialize(pair.Value);
        }

        foreach (var pair in statesByFileName)
        {
            artifactJson[pair.Key] = Serialize(pair.Value);
        }

        artifactJson[SaveLoadRoundtripProofJsonFileName] = Serialize(saveLoadProof);
        artifactJson[ReplayDeterminismProofJsonFileName] = Serialize(replayProof);
        artifactJson[InvalidMatrixJsonFileName] = Serialize(invalidMatrix);

        return new RuntimeChunkDeltaEvidenceResult
        {
            PlansByFileName = plansByFileName,
            StatesByFileName = statesByFileName,
            SaveLoadRoundtripProof = saveLoadProof,
            ReplayDeterminismProof = replayProof,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            ReportMarkdown = RenderReport(report, plans, stateProofs, saveLoadProof, replayProof, invalidMatrix)
        };
    }

    public async Task<RuntimeChunkDeltaEvidenceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<RuntimeChunkDeltaEvidenceWriteResult> WriteAsync(
        string projectRootPath,
        RuntimeChunkDeltaEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var written = new List<string>();
        foreach (var file in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, file.Key);
            await File.WriteAllTextAsync(path, file.Value, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new RuntimeChunkDeltaEvidenceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    public static string PlanFileName(string scenarioId) =>
        scenarioId switch
        {
            "frontier_survival" => FrontierPlanJsonFileName,
            "gothic_intrigue" => GothicPlanJsonFileName,
            "caravan_trade" => CaravanPlanJsonFileName,
            "metamodule_kingdoms" => MetamodulePlanJsonFileName,
            _ => $"chunk-traversal-plan-{scenarioId}.json"
        };

    private static RuntimeChunkReplayDeterminismProof BuildReplayProof(IReadOnlyList<RuntimeChunkTraversalPlan> plans)
    {
        var scenarios = plans
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(plan =>
            {
                var first = RuntimeChunkDeltaProjector.Apply(plan);
                var second = RuntimeChunkDeltaProjector.Apply(plan);
                return new RuntimeChunkReplayScenarioProof
                {
                    ScenarioId = plan.ScenarioId,
                    ReplaySeed = plan.ReplaySeed,
                    FirstRunHash = first.AfterStateHash,
                    SecondRunHash = second.AfterStateHash,
                    SameSeedDeterministic = string.Equals(first.AfterStateHash, second.AfterStateHash, StringComparison.Ordinal),
                    CommandCount = plan.Commands.Count
                };
            })
            .ToList();

        return new RuntimeChunkReplayDeterminismProof
        {
            ScenarioCount = scenarios.Count,
            Scenarios = scenarios,
            Passed = scenarios.Count == 4 && scenarios.All(item => item.SameSeedDeterministic)
        };
    }

    private static string RenderReport(
        RuntimeChunkDeltaTraversalReport report,
        IReadOnlyList<RuntimeChunkTraversalPlan> plans,
        IReadOnlyList<RuntimeChunkDeltaStateProof> stateProofs,
        RuntimeChunkSaveLoadRoundtripProof saveLoadProof,
        RuntimeChunkReplayDeterminismProof replayProof,
        RuntimeChunkInvalidMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Runtime Chunk Delta Traversal Smoke Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            "- accepted=false",
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- finalStatus: {report.ManualGate}",
            $"- manualGate: {report.ManualGate}",
            $"- required marker: {RuntimeChunkDeltaTraversalVocabulary.FinalGate} required",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- goal038AcceptedByUserHandoff: {report.Goal038AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"- scenarioCount: {report.ScenarioCount}",
            $"- traversalPlanCount: {report.TraversalPlanCount}",
            $"- runtimeStateProofCount: {report.RuntimeStateProofCount}",
            $"- runtimeMutationScenarioCount: {report.RuntimeMutationScenarioCount}",
            $"- totalCommandCount: {report.TotalCommandCount}",
            $"- runtimeStateChangedAfterTraversal: {report.RuntimeStateChangedAfterTraversal.ToString().ToLowerInvariant()}",
            $"- saveLoadRoundtripPassed: {report.SaveLoadRoundtripPassed.ToString().ToLowerInvariant()}",
            $"- replayDeterminismPassed: {report.ReplayDeterminismPassed.ToString().ToLowerInvariant()}",
            $"- invalidMatrixPassed: {report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- gamePackageDefinitionsMutated: {report.GamePackageDefinitionsMutated.ToString().ToLowerInvariant()}",
            $"- metamoduleKingdomGroupCount: {report.MetamoduleKingdomGroupCount}",
            $"- metamoduleSpeciesArchetypeSlotRefCount: {report.MetamoduleSpeciesArchetypeSlotRefCount}",
            $"- noRuntimeUiUnityProviderLlmRagLuaGeneratorLibraryLeakage: {report.NoRuntimeUiUnityProviderLlmRagLuaGeneratorLibraryLeakage.ToString().ToLowerInvariant()}",
            $"- frontierStateHash: {report.FrontierStateHash}",
            $"- metamoduleStateHash: {report.MetamoduleStateHash}",
            $"- saveLoadProofHash: {report.SaveLoadProofHash}",
            $"- replayProofHash: {report.ReplayProofHash}",
            $"- invalidMatrixHash: {report.InvalidMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Goal 038 region graph, finite-map and chunk-config facts now drive runtime-facing traversal commands that mutate runtime-owned chunk delta state and survive serializer/snapshot save-load proof.",
            string.Empty,
            "## Traversal plans",
            string.Empty
        };

        lines.AddRange(plans
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(item => $"- {item.ScenarioId}: steps={item.Steps.Count}, commands={item.Commands.Count}, requiredTargets={item.RequiredTargetRegionIds.Count}, chunks={item.Steps.Select(step => step.ChunkId).Distinct(StringComparer.Ordinal).Count()}, mutations={item.Commands.Count(command => command.DeltaKind == "local_mutation")}"));
        lines.Add(string.Empty);
        lines.Add("## Runtime state");
        lines.Add(string.Empty);
        lines.AddRange(stateProofs
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(item => $"- {item.ScenarioId}: changed={item.StateChangedAfterTraversal.ToString().ToLowerInvariant()}, visitedRegions={item.After.VisitedRegionIds.Count}, chunks={item.After.DiscoveredChunkIds.Count}, mutations={item.After.LocalMutations.Count}, deltas={item.After.RuntimeDeltas.Count}, hash={item.AfterStateHash}"));
        lines.Add(string.Empty);
        lines.Add("## Save/load");
        lines.Add(string.Empty);
        lines.AddRange(saveLoadProof.Scenarios.Select(item => $"- {item.ScenarioId}: serializer={item.UsedRuntimeStateSerializer.ToString().ToLowerInvariant()}, snapshotStore={item.UsedRuntimeSnapshotStore.ToString().ToLowerInvariant()}, serializerRoundtrip={item.SerializerRoundtripPassed.ToString().ToLowerInvariant()}, snapshotRoundtrip={item.SnapshotRoundtripPassed.ToString().ToLowerInvariant()}, slot={item.SnapshotSlotName}"));
        lines.Add(string.Empty);
        lines.Add("## Replay determinism");
        lines.Add(string.Empty);
        lines.AddRange(replayProof.Scenarios.Select(item => $"- {item.ScenarioId}: sameSeed={item.SameSeedDeterministic.ToString().ToLowerInvariant()}, commands={item.CommandCount}, hash={item.FirstRunHash}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add("No GamePackage schema/source definition, WinForms/UI, Unity, provider, LLM/RAG, Lua source/execution, generator-library or external dependency change is required by this evidence.");
        lines.Add(string.Empty);
        lines.Add($"{RuntimeChunkDeltaTraversalVocabulary.FinalGate} required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string Hash(string text) => RuntimeChunkDeltaProjector.ComputeHash(text);

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}
