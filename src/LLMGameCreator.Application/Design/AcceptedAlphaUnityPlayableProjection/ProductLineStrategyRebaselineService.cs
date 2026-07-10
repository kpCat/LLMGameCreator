using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class ProductLineStrategyRebaselineService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly IReadOnlyList<string> RequiredPolicyStatements =
    [
        "Narrow alpha must be an expansion-safe kernel, not a hardcoded demo.",
        "Projection-only goals are not enough for product readiness.",
        "Canonical runtime playthrough is required for the next product milestone."
    ];

    private static readonly IReadOnlyList<string> ForbiddenPrefixes =
    [
        ".llmgc/manual/",
        "samples/minimal-map-game/",
        "src/LLMGameCreator.Runtime/",
        "src/LLMGameCreator.Runtime.Abstractions/",
        "src/LLMGameCreator.GamePackage/",
        "src/LLMGameCreator.Generation/",
        "src/LLMGameCreator.AssetPipeline/",
        "src/LLMGameCreator.Scripting/",
        "generator-library/",
        "unity/LLMGameCreatorAlpha/",
        "provider/",
        "LLM/",
        "RAG/"
    ];

    public ProductLineStrategyRebaselineDashboard BuildStatus(string repositoryRootPath) =>
        Build(repositoryRootPath).Dashboard;

    public ProductLineStrategyRebaselineBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var docScan = BuildDocScan(root);
        var plannedWrites = PlannedWritePaths();
        var negative = BuildNegativeProof(plannedWrites);
        var dashboard = BuildDashboard(docScan, negative);
        var report = RenderReport(dashboard, docScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [ProductLineStrategyRebaselineVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [ProductLineStrategyRebaselineVocabulary.DocScanFileName] =
                Serialize(docScan),
            [ProductLineStrategyRebaselineVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [ProductLineStrategyRebaselineVocabulary.ReportFileName] = report
        };
        var proceduralIndex = BuildFileIndex(
            ProductLineStrategyRebaselineVocabulary.ProceduralOutputDirectory,
            proceduralFiles);
        proceduralFiles[ProductLineStrategyRebaselineVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in proceduralFiles.Where(item =>
                     item.Key != ProductLineStrategyRebaselineVocabulary.FileIndexFileName))
        {
            exportFiles[item.Key] = item.Value;
        }

        var exportIndex = BuildFileIndex(
            ProductLineStrategyRebaselineVocabulary.ExportPackageDirectory,
            exportFiles);
        exportFiles[ProductLineStrategyRebaselineVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new ProductLineStrategyRebaselineBuildResult
        {
            Dashboard = dashboard,
            DocScan = docScan,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    public Task<ProductLineStrategyRebaselineWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default) =>
        WriteAsync(repositoryRootPath, Build(repositoryRootPath), cancellationToken);

    private static ProductLineStrategyRebaselineDashboard BuildDashboard(
        ProductLineStrategyRebaselineDocScan docScan,
        ProductLineStrategyRebaselineNegativeProof negative)
    {
        var diagnostics = new List<string>();
        Require(docScan.Passed, "goal133a.doc_scan_failed", diagnostics);
        Require(negative.Passed, "goal133a.negative_proof_failed", diagnostics);

        return new ProductLineStrategyRebaselineDashboard
        {
            ImplementationStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            ProductLineCombiner = docScan.ReadmeProductIdentityPresent,
            NotPromptToGame = docScan.ReadmeProductIdentityPresent,
            LlmOptionalAuthoringOnly = docScan.ReadmeLlmRuntimeBoundaryPresent,
            NewDocsPresent = docScan.ProductLineStrategyDocPresent
                             && docScan.NarrowAlphaPolicyDocPresent
                             && docScan.AutomatedValidationTiersDocPresent,
            AgentsRoutingUpdated = docScan.AgentsRoutingUpdated,
            ContextIndexRoutingUpdated = docScan.ContextIndexRoutingUpdated,
            CurrentStateUpdated = docScan.CurrentStateUpdated,
            QueueUpdated = docScan.QueueUpdated,
            RuntimeUnchanged = negative.RuntimeUnchanged,
            UnityUnchanged = negative.UnityUnchanged,
            SchemaUnchanged = negative.SchemaUnchanged,
            SamplePackageUnchanged = negative.SamplePackageUnchanged,
            ManualInputUnchanged = negative.ManualInputUnchanged,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static ProductLineStrategyRebaselineDocScan BuildDocScan(string root)
    {
        var missing = new List<string>();
        var readme = Read(root, "README.md");
        var agents = Read(root, "AGENTS.md");
        var context = Read(root, "docs/CONTEXT_INDEX.md");
        var stateMarkdown = Read(root, "docs/CURRENT_GENERATOR_STATE.md");
        var stateJson = Read(root, "docs/CURRENT_GENERATOR_STATE.json");
        var queue = Read(root, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var milestone = Read(root, "docs/MILESTONE_GATES.md");
        var risk = Read(root, "docs/RELEASE_RISK_REGISTER.md");
        var debt = Read(root, "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md");
        var oldGoal = Read(root, "docs/agent-tasks/goal-133a-product-line-strategy-rebaseline/GOAL.md");
        var goal131 = Read(
            root,
            ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-recipe-pipeline-result.json");
        var goal132 = Read(
            root,
            ".llmgc/procedural/goal-132-winforms-candidate-pipeline-operator-panel/candidate-pipeline-operator-dashboard.json");
        var policy = Read(root, ".devflow/artifact-scope/artifact-scope-policy.json");

        var strategyTexts = ProductLineStrategyRebaselineVocabulary.StrategyDocs
            .ToDictionary(path => path, path => Read(root, path), StringComparer.Ordinal);
        var combinedStrategy = string.Join(
            Environment.NewLine,
            strategyTexts.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => item.Value));

        var docScan = new ProductLineStrategyRebaselineDocScan
        {
            ReadmeProductIdentityPresent =
                Contains(readme, "LLMGameCreator is a data-driven game product-line combiner, not prompt-to-game."),
            ReadmeLlmRuntimeBoundaryPresent =
                Contains(readme, "LLM is optional authoring assistance only and is not runtime authority."),
            ReadmeCanonicalTruthPresent =
                Contains(readme, "GamePackage and canonical runtime state are the source of truth."),
            ProductLineStrategyDocPresent =
                Exists(root, "docs/PRODUCT_LINE_CORE_STRATEGY.md"),
            NarrowAlphaPolicyDocPresent =
                Exists(root, "docs/NARROW_ALPHA_EXPANSION_POLICY.md"),
            AutomatedValidationTiersDocPresent =
                Exists(root, "docs/AUTOMATED_VALIDATION_TIERS.md"),
            RequiredSeamsPresent =
                AllContain(combinedStrategy, ProductLineStrategyRebaselineVocabulary.RequiredSeams),
            RequiredPolicyStatementsPresent =
                AllContain(combinedStrategy, RequiredPolicyStatements),
            AgentsRoutingUpdated =
                Contains(agents, "docs/PRODUCT_LINE_CORE_STRATEGY.md")
                && Contains(agents, "docs/NARROW_ALPHA_EXPANSION_POLICY.md")
                && Contains(agents, "docs/AUTOMATED_VALIDATION_TIERS.md")
                && Contains(agents, "LLMGameCreator is a data-driven game product-line combiner, not prompt-to-game.")
                && Contains(agents, "LLM is optional local authoring assistance only.")
                && Contains(
                    agents,
                    "Next broad product work must preserve FeatureModule / RuntimePrimitive / SemanticPack / VisualPartPack / WorldSourceAdapter / PlayerAdapter seams."),
            ContextIndexRoutingUpdated =
                Contains(context, "Product-Line Strategy Routing")
                && Contains(context, "broad generation work")
                && Contains(context, "candidate pipeline work")
                && Contains(context, "WinForms operator pipeline work")
                && Contains(context, "Runtime/player pivot work")
                && Contains(context, "Codex task shaping")
                && Contains(context, "roadmap/rebaseline decisions"),
            CurrentStateUpdated = CurrentStateJsonUpdated(stateJson)
                                  && CurrentStateMarkdownUpdated(stateMarkdown),
            QueueUpdated =
                Contains(queue, ProductLineStrategyRebaselineVocabulary.Gate)
                && ContainsAnyGoal134State(queue),
            MilestoneGateUpdated =
                Contains(milestone, ProductLineStrategyRebaselineVocabulary.Gate)
                && Contains(milestone, ProductLineStrategyRebaselineVocabulary.NextGoal),
            RiskRegisterUpdated =
                Contains(risk, "Projection-only candidate/operator evidence is explicitly not enough for product readiness."),
            TechnicalDebtUpdated =
                Contains(debt, ProductLineStrategyRebaselineVocabulary.NextGoal),
            OldGoal133Rerouted =
                Contains(oldGoal, "Goal133A canonical-runtime-pivot routing supersedes")
                && Contains(oldGoal, "candidate package -> package validation -> canonical runtime playthrough"),
            Goal131EvidencePresent =
                Contains(goal131, "\"recipePipelineStatus\"")
                && Contains(goal131, "\"GREEN\"")
                && Contains(goal131, "\"projectionOnly\""),
            Goal132EvidencePresent =
                Contains(goal132, "\"operatorStatus\"")
                && Contains(goal132, "\"GREEN_READY\"")
                && Contains(goal132, "\"projectionOnly\""),
            ArtifactScopeScenarioPresent =
                Contains(policy, ProductLineStrategyRebaselineVocabulary.ScenarioId)
        };

        CollectMissing(docScan, missing);
        return docScan with
        {
            Passed = missing.Count == 0,
            MissingMarkers = missing.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static ProductLineStrategyRebaselineNegativeProof BuildNegativeProof(
        IReadOnlyList<string> plannedWrites)
    {
        var normalizedWrites = plannedWrites
            .Select(NormalizeRelativePath)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
        var normalizedForbidden = ForbiddenPrefixes
            .Select(NormalizeRelativePath)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
        var violations = normalizedWrites
            .Where(path => normalizedForbidden.Any(prefix =>
                path.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        var runtimeUnchanged = NoViolation(violations, "src/LLMGameCreator.Runtime/");
        var runtimeAbstractionsUnchanged =
            NoViolation(violations, "src/LLMGameCreator.Runtime.Abstractions/");
        var unityUnchanged = NoViolation(violations, "unity/LLMGameCreatorAlpha/");
        var schemaUnchanged = NoViolation(violations, "src/LLMGameCreator.GamePackage/");
        var generationUnchanged = NoViolation(violations, "src/LLMGameCreator.Generation/");
        var assetPipelineUnchanged = NoViolation(violations, "src/LLMGameCreator.AssetPipeline/");
        var scriptingUnchanged = NoViolation(violations, "src/LLMGameCreator.Scripting/");
        var samplePackageUnchanged = NoViolation(violations, "samples/minimal-map-game/");
        var manualInputUnchanged = NoViolation(violations, ".llmgc/manual/");
        var providerMediaLuaGeneratorUnchanged =
            NoViolation(violations, "provider/")
            && NoViolation(violations, "LLM/")
            && NoViolation(violations, "RAG/")
            && NoViolation(violations, "generator-library/");

        return new ProductLineStrategyRebaselineNegativeProof
        {
            RuntimeUnchanged = runtimeUnchanged,
            RuntimeAbstractionsUnchanged = runtimeAbstractionsUnchanged,
            UnityUnchanged = unityUnchanged,
            SchemaUnchanged = schemaUnchanged,
            GamePackageProjectUnchanged = schemaUnchanged,
            GenerationUnchanged = generationUnchanged,
            AssetPipelineUnchanged = assetPipelineUnchanged,
            ScriptingUnchanged = scriptingUnchanged,
            SamplePackageUnchanged = samplePackageUnchanged,
            ManualInputUnchanged = manualInputUnchanged,
            ProviderMediaLuaGeneratorLibraryUnchanged = providerMediaLuaGeneratorUnchanged,
            PlannedWriteCount = normalizedWrites.Count,
            ForbiddenPrefixes = normalizedForbidden,
            PlannedWrites = normalizedWrites,
            Violations = violations,
            Passed = violations.Count == 0
                     && runtimeUnchanged
                     && runtimeAbstractionsUnchanged
                     && unityUnchanged
                     && schemaUnchanged
                     && generationUnchanged
                     && assetPipelineUnchanged
                     && scriptingUnchanged
                     && samplePackageUnchanged
                     && manualInputUnchanged
                     && providerMediaLuaGeneratorUnchanged
        };
    }

    private static IReadOnlyList<string> PlannedWritePaths() =>
    [
        ProductLineStrategyRebaselineVocabulary.DocumentationPath,
        ProductLineStrategyRebaselineVocabulary.DashboardRelativePath,
        ProductLineStrategyRebaselineVocabulary.DocScanRelativePath,
        ProductLineStrategyRebaselineVocabulary.NegativeProofRelativePath,
        ProductLineStrategyRebaselineVocabulary.ReportRelativePath,
        ProductLineStrategyRebaselineVocabulary.FileIndexRelativePath,
        ProductLineStrategyRebaselineVocabulary.ExportPackageDirectory + "/"
        + ProductLineStrategyRebaselineVocabulary.DashboardFileName,
        ProductLineStrategyRebaselineVocabulary.ExportPackageDirectory + "/"
        + ProductLineStrategyRebaselineVocabulary.DocScanFileName,
        ProductLineStrategyRebaselineVocabulary.ExportPackageDirectory + "/"
        + ProductLineStrategyRebaselineVocabulary.NegativeProofFileName,
        ProductLineStrategyRebaselineVocabulary.ExportPackageDirectory + "/"
        + ProductLineStrategyRebaselineVocabulary.ReportFileName,
        ProductLineStrategyRebaselineVocabulary.ExportPackageDirectory + "/"
        + ProductLineStrategyRebaselineVocabulary.FileIndexFileName
    ];

    private static async Task<ProductLineStrategyRebaselineWriteResult> WriteAsync(
        string repositoryRootPath,
        ProductLineStrategyRebaselineBuildResult result,
        CancellationToken cancellationToken)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var procedural = Resolve(
            root,
            ProductLineStrategyRebaselineVocabulary.ProceduralOutputDirectory);
        var export = Resolve(root, ProductLineStrategyRebaselineVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, ProductLineStrategyRebaselineVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in result.ProceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.ExportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new ProductLineStrategyRebaselineWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static ProductLineStrategyRebaselineFileIndex BuildFileIndex(
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var entries = pendingTextFiles.Select(item =>
            new ProductLineStrategyRebaselineFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal133a_product_line_strategy_rebaseline_"
                       + Path.GetFileNameWithoutExtension(item.Key),
                Sha256 = HashText(item.Value)
            }).ToList();
        var ordered = entries
            .OrderBy(entry => entry.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new ProductLineStrategyRebaselineFileIndex
        {
            RootPath = relativeRoot,
            IndexedFileCount = ordered.Count,
            ManualInputExcluded = ordered.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = ordered
        };
    }

    private static string RenderReport(
        ProductLineStrategyRebaselineDashboard dashboard,
        ProductLineStrategyRebaselineDocScan docScan,
        ProductLineStrategyRebaselineNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 133A Product-Line Strategy Rebaseline",
            string.Empty,
            "- implementationStatus: " + dashboard.ImplementationStatus,
            "- gate: " + dashboard.Gate,
            "- accepted: " + dashboard.Accepted.ToString().ToLowerInvariant(),
            "- productLineCombiner: " + dashboard.ProductLineCombiner.ToString().ToLowerInvariant(),
            "- notPromptToGame: " + dashboard.NotPromptToGame.ToString().ToLowerInvariant(),
            "- llmOptionalAuthoringOnly: "
            + dashboard.LlmOptionalAuthoringOnly.ToString().ToLowerInvariant(),
            "- nextGoal: " + dashboard.NextGoal,
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant(),
            "- projectionOnlyStopCondition: "
            + dashboard.ProjectionOnlyStopCondition.ToString().ToLowerInvariant(),
            string.Empty,
            "## Doc Scan",
            string.Empty,
            "- newDocsPresent: " + dashboard.NewDocsPresent.ToString().ToLowerInvariant(),
            "- agentsRoutingUpdated: "
            + dashboard.AgentsRoutingUpdated.ToString().ToLowerInvariant(),
            "- contextIndexRoutingUpdated: "
            + dashboard.ContextIndexRoutingUpdated.ToString().ToLowerInvariant(),
            "- currentStateUpdated: " + dashboard.CurrentStateUpdated.ToString().ToLowerInvariant(),
            "- queueUpdated: " + dashboard.QueueUpdated.ToString().ToLowerInvariant(),
            "- requiredSeamsPresent: " + docScan.RequiredSeamsPresent.ToString().ToLowerInvariant(),
            "- requiredPolicyStatementsPresent: "
            + docScan.RequiredPolicyStatementsPresent.ToString().ToLowerInvariant(),
            "- oldGoal133Rerouted: " + docScan.OldGoal133Rerouted.ToString().ToLowerInvariant(),
            string.Empty,
            "## Negative Proof",
            string.Empty,
            "- runtimeUnchanged: " + dashboard.RuntimeUnchanged.ToString().ToLowerInvariant(),
            "- unityUnchanged: " + dashboard.UnityUnchanged.ToString().ToLowerInvariant(),
            "- schemaUnchanged: " + dashboard.SchemaUnchanged.ToString().ToLowerInvariant(),
            "- samplePackageUnchanged: "
            + dashboard.SamplePackageUnchanged.ToString().ToLowerInvariant(),
            "- manualInputUnchanged: "
            + dashboard.ManualInputUnchanged.ToString().ToLowerInvariant(),
            "- plannedWriteCount: " + negative.PlannedWriteCount
        };

        if (dashboard.Diagnostics.Count > 0 || docScan.MissingMarkers.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Diagnostics");
            lines.AddRange(dashboard.Diagnostics.Select(item => "- " + item));
            lines.AddRange(docScan.MissingMarkers.Select(item => "- " + item));
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderDocumentation(ProductLineStrategyRebaselineDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Product-Line Strategy Rebaseline And Canonical Runtime Pivot",
            string.Empty,
            "Goal133A records the product-line strategy rebaseline and keeps the gate open for review.",
            string.Empty,
            "## Gate",
            string.Empty,
            "- gate: " + dashboard.Gate,
            "- implementationStatus: " + dashboard.ImplementationStatus,
            "- accepted: " + dashboard.Accepted.ToString().ToLowerInvariant(),
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant(),
            "- projectionOnlyStopCondition: "
            + dashboard.ProjectionOnlyStopCondition.ToString().ToLowerInvariant(),
            "- nextProductGoal: " + dashboard.NextGoal,
            string.Empty,
            "## Next Product Path",
            string.Empty,
            "Goal134 must start: candidate package -> package validation -> canonical runtime playthrough -> save/load/replay proof -> Unity/player consumes canonical transcript/state summary -> one-click report.",
            string.Empty,
            "## Scope Guard",
            string.Empty,
            "- Runtime, Runtime.Abstractions and GamePackage schema are unchanged.",
            "- Unity/player files are unchanged.",
            "- samples/minimal-map-game and .llmgc/manual remain unchanged.",
            "- Lua/provider/media/generator-library work remains out of scope."
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static void CollectMissing(
        ProductLineStrategyRebaselineDocScan docScan,
        List<string> missing)
    {
        AddMissing(!docScan.ReadmeProductIdentityPresent, "readme.product_identity", missing);
        AddMissing(!docScan.ReadmeLlmRuntimeBoundaryPresent, "readme.llm_boundary", missing);
        AddMissing(!docScan.ReadmeCanonicalTruthPresent, "readme.canonical_truth", missing);
        AddMissing(!docScan.ProductLineStrategyDocPresent, "docs.product_line_strategy", missing);
        AddMissing(!docScan.NarrowAlphaPolicyDocPresent, "docs.narrow_alpha_policy", missing);
        AddMissing(!docScan.AutomatedValidationTiersDocPresent, "docs.validation_tiers", missing);
        AddMissing(!docScan.RequiredSeamsPresent, "docs.required_seams", missing);
        AddMissing(!docScan.RequiredPolicyStatementsPresent, "docs.required_policy_statements", missing);
        AddMissing(!docScan.AgentsRoutingUpdated, "agents.routing", missing);
        AddMissing(!docScan.ContextIndexRoutingUpdated, "context_index.routing", missing);
        AddMissing(!docScan.CurrentStateUpdated, "current_state.routing", missing);
        AddMissing(!docScan.QueueUpdated, "queue.routing", missing);
        AddMissing(!docScan.MilestoneGateUpdated, "milestone_gate.note", missing);
        AddMissing(!docScan.RiskRegisterUpdated, "risk_register.note", missing);
        AddMissing(!docScan.TechnicalDebtUpdated, "technical_debt.note", missing);
        AddMissing(!docScan.OldGoal133Rerouted, "old_goal133.reroute", missing);
        AddMissing(!docScan.Goal131EvidencePresent, "goal131.evidence", missing);
        AddMissing(!docScan.Goal132EvidencePresent, "goal132.evidence", missing);
        AddMissing(!docScan.ArtifactScopeScenarioPresent, "artifact_scope.scenario", missing);
    }

    private static bool CurrentStateJsonUpdated(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var goal133AState =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "manualUnityOptional")
                   && BoolValue(root, "projectionOnlyStopCondition")
                   && StringValue(root, "nextProductGoal")
                   == ProductLineStrategyRebaselineVocabulary.NextGoal
                   && StringValue(root, "recommended_next_decision")
                       .Contains(ProductLineStrategyRebaselineVocabulary.NextGoal,
                           StringComparison.Ordinal)
                   && StringValue(root, "current_user_action")
                       .Contains("canonical runtime path", StringComparison.Ordinal);
            var postGoal134State =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Goal134Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "manualUnityOptional")
                   && BoolValue(root, "canonicalRuntimeCoverage")
                   && BoolValue(root, "saveLoadReplayCoverage")
                   && BoolValue(root, "selectedCandidateExecutedByRuntime")
                   && BoolValue(root, "unityConsumesCanonicalTranscript")
                   && StringValue(root, "nextProductGoal")
                   == ProductLineStrategyRebaselineVocabulary.PostGoal134NextGoal;
            var postGoal135State =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Goal135Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "manualUnityOptional")
                   && BoolValue(root, "projectionOnly") == false
                   && BoolValue(root, "canonicalRuntimeSource")
                   && BoolValue(root, "playerAdapterCoverage")
                   && BoolValue(root, "unityGameplayTruth") == false
                   && BoolValue(root, "noUnclassifiedErrorDiagnostics");
            var postGoal136State =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Goal136Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "manualUnityOptional")
                   && BoolValue(root, "projectionOnly") == false
                   && BoolValue(root, "canonicalRuntimeSource")
                   && BoolValue(root, "playerCommandLoopCoverage")
                   && BoolValue(root, "playerAdapterCoverage")
                   && BoolValue(root, "unityConsumesRuntimeSnapshots")
                   && BoolValue(root, "unityGameplayTruth") == false
                   && BoolValue(root, "noUnclassifiedErrorDiagnostics");
            var postGoal137State =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Goal137Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "manualUnityOptional")
                   && BoolValue(root, "projectionOnly") == false
                   && BoolValue(root, "canonicalRuntimeSource")
                   && BoolValue(root, "runtimeSnapshotSource")
                   && BoolValue(root, "unityConsumesRuntimeSnapshots")
                   && BoolValue(root, "unityPlayerLoopPlaybackPassed")
                   && BoolValue(root, "unityGameplayTruth") == false
                   && BoolValue(root, "noUnclassifiedErrorDiagnostics");
            var postGoal138State =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Goal138Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "goal137Accepted")
                   && BoolValue(root, "goal138Accepted") == false
                   && BoolValue(root, "manualUnityOptional")
                   && BoolValue(root, "projectionOnly") == false
                   && BoolValue(root, "runtimeAuthority")
                   && BoolValue(root, "runtimeBackedUnityStepper")
                   && BoolValue(root, "stepperBatchSmokePassed")
                   && BoolValue(root, "unityGameplayTruth") == false;
            var postGoal139State =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Goal139Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "goal138Accepted")
                   && BoolValue(root, "goal139Accepted") == false
                   && BoolValue(root, "manualUnityOptional")
                   && BoolValue(root, "projectionOnly") == false
                   && BoolValue(root, "runtimeAuthority")
                   && BoolValue(root, "runtimeBackedUnityInteractiveControls")
                   && BoolValue(root, "interactiveControlsWindowPresent")
                   && BoolValue(root, "unityInteractiveControlsSmokePassed")
                   && BoolValue(root, "unityGameplayTruth") == false;
            var postGoal140State =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Goal140Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "goal139Accepted")
                   && BoolValue(root, "goal140Accepted") == false
                   && BoolValue(root, "projectionOnly") == false
                   && BoolValue(root, "runtimeAuthority")
                   && BoolValue(root, "runtimeBackedUnityControlsUxPolish")
                    && BoolValue(root, "knownUnityEditorNoiseClassified")
                    && BoolValue(root, "unityControlsUxSmokePassed")
                    && BoolValue(root, "unityGameplayTruth") == false;
            var postGoal141State =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Goal141Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "goal140Accepted")
                   && BoolValue(root, "goal141Accepted") == false
                   && BoolValue(root, "projectionOnly") == false
                   && BoolValue(root, "runtimeAuthority")
                   && BoolValue(root, "runtimeBackedPlayerCommandRoundtrip")
                   && BoolValue(root, "controlRequestBridgePresent")
                   && BoolValue(root, "unityConsumesRoundtripResult")
                   && BoolValue(root, "unityGameplayTruth") == false;
            var postGoal142State =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Goal142Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "goal141Accepted") == false
                   && BoolValue(root, "projectionOnly") == false
                   && BoolValue(root, "runtimeAuthority")
                   && BoolValue(root, "runtimeSignificantProductLineVariantMatrix")
                   && BoolValue(root, "runtimeBackedPlayerCommandRoundtrip")
                   && BoolValue(root, "sourceTemplateUnmodified")
                   && BoolValue(root, "unityGameplayTruth") == false;
            var postGoal143State =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Goal143Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "goal142Accepted")
                   && BoolValue(root, "goal143Accepted") == false
                   && BoolValue(root, "selectedRuntimeVariantPlayerAdapterHandoff")
                   && BoolValue(root, "selectedPackageSha256MatchesHandoff")
                   && BoolValue(root, "selectedFinalStateHashMatches")
                   && BoolValue(root, "selectedVariantEffectVisible")
                   && BoolValue(root, "noBalancedBaselineFallback")
                   && BoolValue(root, "unityConsumesSelectedRuntimeVariantPlayerAdapter")
                   && BoolValue(root, "runtimeAuthority")
                   && BoolValue(root, "projectionOnly") == false
                   && BoolValue(root, "unityGameplayTruth") == false;
            var postGoal144State =
                StringValue(root, "gate_status")
                   == ProductLineStrategyRebaselineVocabulary.Goal144Gate
                   && BoolValue(root, "accepted") == false
                   && BoolValue(root, "goal143Accepted")
                   && BoolValue(root, "goal144Accepted") == false
                   && BoolValue(root, "selectedRuntimeVariantInteractiveSession")
                   && BoolValue(root, "checkpointReloadByReplayPassed")
                   && BoolValue(root, "fullReplayEquivalent")
                   && BoolValue(root, "finalStateHashMatchesGoal142")
                   && BoolValue(root, "selectedVariantEffectVisible")
                   && BoolValue(root, "noBalancedBaselineFallback")
                   && BoolValue(root, "runtimeAuthority")
                   && BoolValue(root, "projectionOnly") == false
                   && BoolValue(root, "unityGameplayTruth") == false;
            return goal133AState
                   || postGoal134State
                   || postGoal135State
                   || postGoal136State
                   || postGoal137State
                   || postGoal138State
                   || postGoal139State
                   || postGoal140State
                   || postGoal141State
                   || postGoal142State
                   || postGoal143State
                   || postGoal144State;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool CurrentStateMarkdownUpdated(string markdown) =>
        Contains(markdown, ProductLineStrategyRebaselineVocabulary.Gate)
        && Contains(markdown, "projectionOnlyStopCondition=true")
        && Contains(markdown, ProductLineStrategyRebaselineVocabulary.NextGoal)
        || Contains(markdown, ProductLineStrategyRebaselineVocabulary.Goal134Gate)
        && Contains(markdown, "projectionOnly=false")
        && Contains(markdown, ProductLineStrategyRebaselineVocabulary.PostGoal134NextGoal)
        || Contains(markdown, ProductLineStrategyRebaselineVocabulary.Goal135Gate)
        && Contains(markdown, "canonicalRuntimeSource=true")
        && Contains(markdown, "playerAdapterCoverage=true")
        || Contains(markdown, ProductLineStrategyRebaselineVocabulary.Goal136Gate)
        && Contains(markdown, "playerCommandLoopCoverage=true")
        && Contains(markdown, "unityConsumesRuntimeSnapshots=true")
        || Contains(markdown, ProductLineStrategyRebaselineVocabulary.Goal137Gate)
        && Contains(markdown, "runtimeSnapshotSource=true")
        && Contains(markdown, "unityPlayerLoopPlaybackPassed=true")
        || Contains(markdown, ProductLineStrategyRebaselineVocabulary.Goal138Gate)
        && Contains(markdown, "goal137Accepted=true")
        && Contains(markdown, "goal138Accepted=false")
        && Contains(markdown, "runtimeBackedUnityStepper=true")
        || Contains(markdown, ProductLineStrategyRebaselineVocabulary.Goal139Gate)
        && Contains(markdown, "goal138Accepted=true")
        && Contains(markdown, "goal139Accepted=false")
        && Contains(markdown, "runtimeBackedUnityInteractiveControls=true")
        || Contains(markdown, ProductLineStrategyRebaselineVocabulary.Goal140Gate)
        && Contains(markdown, "goal139Accepted=true")
        && Contains(markdown, "goal140Accepted=false")
        && Contains(markdown, "runtimeBackedUnityControlsUxPolish=true")
        || Contains(markdown, ProductLineStrategyRebaselineVocabulary.Goal141Gate)
        && Contains(markdown, "goal140Accepted=true")
        && Contains(markdown, "goal141Accepted=false")
        && Contains(markdown, "runtimeBackedPlayerCommandRoundtrip=true")
        || Contains(markdown, ProductLineStrategyRebaselineVocabulary.Goal142Gate)
        && Contains(markdown, "goal141Accepted=false")
        && Contains(markdown, "runtimeBackedPlayerCommandRoundtrip=true")
        && Contains(markdown, "sourceTemplateUnmodified=true")
        || Contains(markdown, ProductLineStrategyRebaselineVocabulary.Goal143Gate)
        && Contains(markdown, "goal142Accepted=true")
        && Contains(markdown, "selectedRuntimeVariantPlayerAdapterHandoff=true")
        && Contains(markdown, "selectedPackageSha256MatchesHandoff=true")
        && Contains(markdown, "selectedFinalStateHashMatches=true")
        || Contains(markdown, ProductLineStrategyRebaselineVocabulary.Goal144Gate)
        && Contains(markdown, "goal143Accepted=true")
        && Contains(markdown, "selectedRuntimeVariantInteractiveSession=true")
        && Contains(markdown, "checkpointReloadByReplayPassed=true")
        && Contains(markdown, "fullReplayEquivalent=true");

    private static bool ContainsAnyGoal134State(string text) =>
        Contains(text, ProductLineStrategyRebaselineVocabulary.NextGoal)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.Goal134Gate)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.PostGoal134NextGoal)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.Goal135Gate)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.PostGoal135NextGoal)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.Goal136Gate)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.PostGoal136NextGoal)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.Goal137Gate)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.Goal138Gate)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.Goal139Gate)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.Goal140Gate)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.Goal141Gate)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.Goal142Gate)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.Goal143Gate)
        || Contains(text, ProductLineStrategyRebaselineVocabulary.Goal144Gate);

    private static bool Contains(string text, string value) =>
        text.Contains(value, StringComparison.Ordinal);

    private static bool AllContain(string text, IEnumerable<string> markers) =>
        markers.All(marker => text.Contains(marker, StringComparison.Ordinal));

    private static string Read(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static bool Exists(string root, string relativePath) =>
        File.Exists(Resolve(root, relativePath));

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static string ResolveRepositoryRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Repository root path is required.", nameof(path));
        }

        return Path.GetFullPath(path);
    }

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)
                                  ?? throw new InvalidOperationException("Missing directory."));
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal133A must not write the manual input path.");
        }
    }

    private static void Require(bool condition, string code, List<string> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(code);
        }
    }

    private static void AddMissing(bool condition, string code, List<string> missing)
    {
        if (condition)
        {
            missing.Add(code);
        }
    }

    private static string StringValue(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool BoolValue(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.True;

    private static string NormalizeRelativePath(string path) =>
        path.Replace('\\', '/').TrimStart('/');

    private static bool NoViolation(IEnumerable<string> violations, string prefix)
    {
        var normalized = NormalizeRelativePath(prefix);
        return violations.All(path => !path.StartsWith(normalized, StringComparison.Ordinal));
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
