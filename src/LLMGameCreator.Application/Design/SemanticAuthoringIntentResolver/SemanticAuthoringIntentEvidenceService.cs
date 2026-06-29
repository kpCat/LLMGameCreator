using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.DynamicSemanticFeatures;

namespace LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;

public sealed class SemanticAuthoringIntentEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-033-semantic-authoring-intent-resolver";
    public const string WorkspaceSchemaSummaryJsonFileName = "authoring-workspace-schema-summary.json";
    public const string LoreSkeletonJsonFileName = "lore-intake-skeleton-metamodule-kingdoms.json";
    public const string ManualMatrixJsonFileName = "manual-vs-auto-authoring-matrix.json";
    public const string FrontierResolutionJsonFileName = "intent-resolution-frontier.json";
    public const string GothicResolutionJsonFileName = "intent-resolution-gothic.json";
    public const string CaravanResolutionJsonFileName = "intent-resolution-caravan.json";
    public const string MetamoduleKingdomsResolutionJsonFileName = "intent-resolution-metamodule-kingdoms.json";
    public const string InvalidMatrixJsonFileName = "invalid-authoring-intent-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "semantic-authoring-intent-resolver-report.md";
    public const string FinalGate = "semantic_authoring_intent_resolver_verification";
    public const string PreviousProducedGate = "dynamic_semantic_feature_system_verification required";
    public const string ProductSmokeRoute = "goal-033-semantic-authoring-intent-resolver";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public SemanticAuthoringIntentEvidenceResult Build()
    {
        var workspaces = SemanticAuthoringIntentCatalog.BuildDefaultWorkspaces();
        var workspaceDiagnostics = workspaces.SelectMany(SemanticAuthoringIntentValidator.ValidateWorkspace).ToList();
        var workspaceSummary = BuildWorkspaceSummary(workspaces, workspaceDiagnostics);
        var skeleton = SemanticAuthoringIntentCatalog.BuildMetamoduleKingdomsLoreSkeleton();
        var manualMatrix = SemanticAuthoringIntentCatalog.BuildManualVsAutoAuthoringMatrix();
        var manualDiagnostics = SemanticAuthoringIntentValidator.ValidateManualMatrix(manualMatrix);
        var resolver = new FeatureDrivenIntentResolver();
        var resolutions = DynamicSemanticFeatureCatalog.BuildDefaultScenarios()
            .Select(resolver.ResolveScenario)
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();
        var invalidMatrix = SemanticAuthoringIntentValidator.BuildInvalidMatrix();

        var workspaceJson = Serialize(workspaceSummary);
        var skeletonJson = Serialize(skeleton);
        var manualJson = Serialize(manualMatrix);
        var frontierJson = Serialize(Find(resolutions, "frontier_survival"));
        var gothicJson = Serialize(Find(resolutions, "gothic_intrigue"));
        var caravanJson = Serialize(Find(resolutions, "caravan_trade"));
        var metamoduleJson = Serialize(Find(resolutions, "metamodule_kingdoms"));
        var invalidJson = Serialize(invalidMatrix);
        var allDiagnostics = SemanticAuthoringIntentValidator.SortDiagnostics(
            workspaceDiagnostics
                .Concat(manualDiagnostics)
                .Concat(resolutions.SelectMany(item => item.Diagnostics))
                .Concat(invalidMatrix.Scenarios.SelectMany(item => item.Diagnostics.Where(diagnostic => diagnostic.Severity == "warning"))));
        var intentHashes = new[] { frontierJson, gothicJson, caravanJson, metamoduleJson }
            .Select(ComputeHash)
            .Order(StringComparer.Ordinal)
            .ToList();
        var reportWithoutHash = new SemanticAuthoringIntentResolverReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousProducedGate = PreviousProducedGate,
            ProductSmokeRoute = ProductSmokeRoute,
            WorkspaceImplemented = true,
            LoreSkeletonImplemented = true,
            ProvenanceMatrixImplemented = true,
            IntentResolverImplemented = true,
            EvidenceArtifactsWritten = true,
            WorkspaceFieldCount = workspaceSummary.FieldCount,
            IntentCount = resolutions.Sum(item => item.Intents.Count),
            MetamoduleSpeciesArchetypeSlotCount = skeleton.EvidenceSummary.SpeciesArchetypeSlotCount,
            InvalidMatrixPassed = invalidMatrix.Passed,
            PublicGamePackageSchemaChanged = false,
            UiChanged = false,
            RuntimeBehaviorChanged = false,
            UnityBuildExecuted = false,
            LlmRagProviderMediaLuaExecuted = false,
            FinalDialogueProseGenerated = false,
            FinalGamePackageMaterialized = false,
            WorkspaceSchemaSummaryHash = ComputeHash(workspaceJson),
            LoreSkeletonHash = ComputeHash(skeletonJson),
            ManualMatrixHash = ComputeHash(manualJson),
            IntentResolutionHashes = intentHashes,
            InvalidMatrixHash = ComputeHash(invalidJson),
            Diagnostics = allDiagnostics
        };
        var report = reportWithoutHash with
        {
            ContractProofPassed = workspaceDiagnostics.All(item => item.Severity != "error")
                && manualDiagnostics.All(item => item.Severity != "error")
                && resolutions.All(item => item.Diagnostics.All(diagnostic => diagnostic.Severity != "error"))
                && invalidMatrix.Passed
                && skeleton.EvidenceSummary.KingdomCount is 6 or 7
                && skeleton.EvidenceSummary.SpeciesArchetypeSlotCount >= 100
                && resolutions.SelectMany(item => item.Intents).Select(item => item.IntentFamily).Distinct(StringComparer.Ordinal).Count() >= 10,
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new SemanticAuthoringIntentEvidenceResult
        {
            WorkspaceSchemaSummary = workspaceSummary,
            MetamoduleLoreSkeleton = skeleton,
            ManualVsAutoAuthoringMatrix = manualMatrix,
            FrontierResolution = Find(resolutions, "frontier_survival"),
            GothicResolution = Find(resolutions, "gothic_intrigue"),
            CaravanResolution = Find(resolutions, "caravan_trade"),
            MetamoduleKingdomsResolution = Find(resolutions, "metamodule_kingdoms"),
            InvalidMatrix = invalidMatrix,
            Report = report,
            WorkspaceSchemaSummaryJson = workspaceJson,
            MetamoduleLoreSkeletonJson = skeletonJson,
            ManualVsAutoAuthoringMatrixJson = manualJson,
            FrontierResolutionJson = frontierJson,
            GothicResolutionJson = gothicJson,
            CaravanResolutionJson = caravanJson,
            MetamoduleKingdomsResolutionJson = metamoduleJson,
            InvalidMatrixJson = invalidJson,
            ReportMarkdown = RenderReport(report, resolutions, invalidMatrix)
        };
    }

    public async Task<SemanticAuthoringIntentEvidenceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SemanticAuthoringIntentEvidenceWriteResult> WriteAsync(
        string projectRootPath,
        SemanticAuthoringIntentEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new SemanticAuthoringIntentEvidenceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WorkspaceSchemaSummaryJsonPath = Path.Combine(outputDirectory, WorkspaceSchemaSummaryJsonFileName),
            MetamoduleLoreSkeletonJsonPath = Path.Combine(outputDirectory, LoreSkeletonJsonFileName),
            ManualVsAutoAuthoringMatrixJsonPath = Path.Combine(outputDirectory, ManualMatrixJsonFileName),
            FrontierResolutionJsonPath = Path.Combine(outputDirectory, FrontierResolutionJsonFileName),
            GothicResolutionJsonPath = Path.Combine(outputDirectory, GothicResolutionJsonFileName),
            CaravanResolutionJsonPath = Path.Combine(outputDirectory, CaravanResolutionJsonFileName),
            MetamoduleKingdomsResolutionJsonPath = Path.Combine(outputDirectory, MetamoduleKingdomsResolutionJsonFileName),
            InvalidMatrixJsonPath = Path.Combine(outputDirectory, InvalidMatrixJsonFileName),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName)
        };

        await File.WriteAllTextAsync(write.WorkspaceSchemaSummaryJsonPath, result.WorkspaceSchemaSummaryJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.MetamoduleLoreSkeletonJsonPath, result.MetamoduleLoreSkeletonJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ManualVsAutoAuthoringMatrixJsonPath, result.ManualVsAutoAuthoringMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.FrontierResolutionJsonPath, result.FrontierResolutionJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.GothicResolutionJsonPath, result.GothicResolutionJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CaravanResolutionJsonPath, result.CaravanResolutionJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.MetamoduleKingdomsResolutionJsonPath, result.MetamoduleKingdomsResolutionJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.InvalidMatrixJsonPath, result.InvalidMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        return write;
    }

    private static SemanticAuthoringWorkspaceSchemaSummary BuildWorkspaceSummary(
        IReadOnlyList<SemanticAuthoringWorkspace> workspaces,
        IReadOnlyList<SemanticAuthoringDiagnostic> diagnostics)
    {
        var fields = workspaces.SelectMany(workspace => workspace.DomainGroups).SelectMany(group => group.Sections).SelectMany(section => section.Fields).ToList();
        return new SemanticAuthoringWorkspaceSchemaSummary
        {
            WorkspaceCount = workspaces.Count,
            DomainGroupCount = workspaces.SelectMany(item => item.DomainGroups).Select(item => item.DomainId).Distinct(StringComparer.Ordinal).Count(),
            FieldCount = fields.Count,
            FieldsByDomain = fields
                .GroupBy(item => item.DomainId, StringComparer.Ordinal)
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .ToDictionary(item => item.Key, item => item.Count(), StringComparer.Ordinal),
            ProvenanceKinds = fields.Select(item => item.Provenance).Concat(SemanticAuthoringIntentVocabulary.ProvenanceKinds).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            ValueKinds = fields.Select(item => item.ValueKind).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            SampleWorkspaces = workspaces.OrderBy(item => item.WorkspaceId, StringComparer.Ordinal).ToList(),
            UpstreamSeams = SemanticAuthoringIntentCatalog.BuildUpstreamSeamSummary(),
            Diagnostics = SemanticAuthoringIntentValidator.SortDiagnostics(diagnostics)
        };
    }

    private static SemanticAuthoringIntentResolution Find(IReadOnlyList<SemanticAuthoringIntentResolution> resolutions, string scenarioId) =>
        resolutions.Single(item => item.ScenarioId == scenarioId);

    private static string RenderReport(
        SemanticAuthoringIntentResolverReport report,
        IReadOnlyList<SemanticAuthoringIntentResolution> resolutions,
        SemanticAuthoringIntentInvalidMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Semantic Authoring Intent Resolver Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- finalStatus: {report.FinalStatus}",
            $"- manualGate: {report.ManualGate}",
            $"- required marker: {FinalGate} required",
            $"- previousProducedGate: {report.PreviousProducedGate}",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- contractProofPassed: {report.ContractProofPassed.ToString().ToLowerInvariant()}",
            $"- workspaceFieldCount: {report.WorkspaceFieldCount}",
            $"- intentCount: {report.IntentCount}",
            $"- metamoduleSpeciesArchetypeSlotCount: {report.MetamoduleSpeciesArchetypeSlotCount}",
            $"- invalidMatrixPassed: {report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- workspaceSchemaSummaryHash: {report.WorkspaceSchemaSummaryHash}",
            $"- loreSkeletonHash: {report.LoreSkeletonHash}",
            $"- manualMatrixHash: {report.ManualMatrixHash}",
            $"- invalidMatrixHash: {report.InvalidMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Goal 033 adds a deterministic semantic authoring workspace, lore intake skeleton, provenance matrix and feature-driven content-intent resolver over the existing Goal 030-032 semantic stack.",
            string.Empty,
            "## Scenarios",
            string.Empty
        };
        lines.AddRange(resolutions.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).Select(item => $"- {item.ScenarioId}: intents={item.Intents.Count}, summary={item.StableSummary}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedValid={item.ExpectedValid.ToString().ToLowerInvariant()}, actualValid={item.ActualValid.ToString().ToLowerInvariant()}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add($"- publicGamePackageSchemaChanged: {report.PublicGamePackageSchemaChanged.ToString().ToLowerInvariant()}");
        lines.Add($"- uiChanged: {report.UiChanged.ToString().ToLowerInvariant()}");
        lines.Add($"- runtimeBehaviorChanged: {report.RuntimeBehaviorChanged.ToString().ToLowerInvariant()}");
        lines.Add($"- unityBuildExecuted: {report.UnityBuildExecuted.ToString().ToLowerInvariant()}");
        lines.Add($"- llmRagProviderMediaLuaExecuted: {report.LlmRagProviderMediaLuaExecuted.ToString().ToLowerInvariant()}");
        lines.Add($"- finalDialogueProseGenerated: {report.FinalDialogueProseGenerated.ToString().ToLowerInvariant()}");
        lines.Add($"- finalGamePackageMaterialized: {report.FinalGamePackageMaterialized.ToString().ToLowerInvariant()}");
        lines.Add(string.Empty);
        lines.Add("Final dialogue/prose/GamePackage/runtime/UI/Unity/provider/LLM/RAG/Lua/media generation was not performed.");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string ComputeHash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

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
