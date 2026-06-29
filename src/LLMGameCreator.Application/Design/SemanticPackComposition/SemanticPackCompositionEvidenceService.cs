using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.SemanticArtifactContracts;

namespace LLMGameCreator.Application.Design.SemanticPackComposition;

public sealed class SemanticPackCompositionEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-031-semantic-pack-composition-blueprint";
    public const string CatalogSummaryJsonFileName = "pack-catalog-summary.json";
    public const string CompositionMatrixJsonFileName = "composition-matrix.json";
    public const string FrontierPlanJsonFileName = "semantic-blueprint-plan-frontier.json";
    public const string GothicPlanJsonFileName = "semantic-blueprint-plan-gothic.json";
    public const string CaravanPlanJsonFileName = "semantic-blueprint-plan-caravan.json";
    public const string CrossArtifactLinkageReportJsonFileName = "cross-artifact-linkage-report.json";
    public const string ReportMarkdownFileName = "semantic-pack-composition-blueprint-report.md";
    public const string FinalGate = "semantic_pack_composition_blueprint_verification";
    public const string PreviousAcceptedGate = "semantic_artifact_contract_registry_verification passed";
    public const string ProductSmokeRoute = "goal-031-semantic-pack-composition-blueprint";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public SemanticPackCompositionEvidenceResult Build()
    {
        var packs = SemanticPackCompositionCatalog.BuildDefaultPacks();
        var contracts = SemanticArtifactContractRegistry.BuildDefaultContracts();
        var catalogDiagnostics = SemanticPackCompositionValidator.ValidateCatalog(packs, contracts);
        var planner = new SemanticPackCompositionPlanner(packs, contracts);
        var frontier = planner.BuildBlueprint(SemanticPackCompositionCatalog.FrontierRequest());
        var gothic = planner.BuildBlueprint(SemanticPackCompositionCatalog.GothicRequest());
        var caravan = planner.BuildBlueprint(SemanticPackCompositionCatalog.CaravanRequest());
        var plans = new[] { frontier, gothic, caravan };
        var catalogSummary = BuildCatalogSummary(packs, catalogDiagnostics);
        var matrix = BuildMatrix(plans);
        var linkageReport = BuildLinkageReport(plans);
        var invalidMatrix = BuildInvalidMatrix();

        var catalogJson = Serialize(catalogSummary);
        var matrixJson = Serialize(matrix);
        var frontierJson = Serialize(frontier);
        var gothicJson = Serialize(gothic);
        var caravanJson = Serialize(caravan);
        var linkageJson = Serialize(linkageReport);
        var planHashes = new[] { frontierJson, gothicJson, caravanJson }
            .Select(ComputeHash)
            .Order(StringComparer.Ordinal)
            .ToList();
        var allDiagnostics = catalogDiagnostics
            .Concat(plans.SelectMany(plan => plan.Diagnostics))
            .ToList();

        var reportWithoutHash = new SemanticPackCompositionBlueprintReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = PreviousAcceptedGate,
            ProductSmokeRoute = ProductSmokeRoute,
            BlueprintProofPassed = allDiagnostics.All(item => item.Severity != "error")
                && matrix.ComposerSharedByAllScenarios
                && matrix.Goal030PlannerUsedByAllScenarios
                && matrix.ScenariosAreMeaningfullyDifferent
                && plans.All(plan => plan.Sections.Count == 11)
                && plans.All(plan => plan.CrossArtifactLinks.Count >= 4)
                && invalidMatrix.Passed,
            PackCount = packs.Count,
            ScenarioCount = plans.Length,
            CatalogValidated = catalogDiagnostics.All(item => item.Severity != "error"),
            ComposerShared = matrix.ComposerSharedByAllScenarios,
            Goal030PlannerIntegrated = matrix.Goal030PlannerUsedByAllScenarios,
            CrossArtifactLinksWritten = linkageReport.LinkCount >= 12,
            InvalidMatrixPassed = invalidMatrix.Passed,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            UnityBuildExecuted = false,
            LlmRagProviderMediaLuaExecuted = false,
            RuntimeBehaviorChanged = false,
            CatalogSummaryHash = ComputeHash(catalogJson),
            CompositionMatrixHash = ComputeHash(matrixJson),
            CrossArtifactLinkageHash = ComputeHash(linkageJson),
            PlanHashes = planHashes,
            InvalidMatrix = invalidMatrix,
            Diagnostics = SemanticPackCompositionValidator.SortDiagnostics(allDiagnostics)
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new SemanticPackCompositionEvidenceResult
        {
            CatalogSummary = catalogSummary,
            CompositionMatrix = matrix,
            FrontierPlan = frontier,
            GothicPlan = gothic,
            CaravanPlan = caravan,
            CrossArtifactLinkageReport = linkageReport,
            Report = report,
            CatalogSummaryJson = catalogJson,
            CompositionMatrixJson = matrixJson,
            FrontierPlanJson = frontierJson,
            GothicPlanJson = gothicJson,
            CaravanPlanJson = caravanJson,
            CrossArtifactLinkageReportJson = linkageJson,
            ReportMarkdown = RenderReport(report, matrix)
        };
    }

    public async Task<SemanticPackCompositionEvidenceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SemanticPackCompositionEvidenceWriteResult> WriteAsync(
        string projectRootPath,
        SemanticPackCompositionEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new SemanticPackCompositionEvidenceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            CatalogSummaryJsonPath = Path.Combine(outputDirectory, CatalogSummaryJsonFileName),
            CompositionMatrixJsonPath = Path.Combine(outputDirectory, CompositionMatrixJsonFileName),
            FrontierPlanJsonPath = Path.Combine(outputDirectory, FrontierPlanJsonFileName),
            GothicPlanJsonPath = Path.Combine(outputDirectory, GothicPlanJsonFileName),
            CaravanPlanJsonPath = Path.Combine(outputDirectory, CaravanPlanJsonFileName),
            CrossArtifactLinkageReportJsonPath = Path.Combine(outputDirectory, CrossArtifactLinkageReportJsonFileName),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName)
        };

        await File.WriteAllTextAsync(write.CatalogSummaryJsonPath, result.CatalogSummaryJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CompositionMatrixJsonPath, result.CompositionMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.FrontierPlanJsonPath, result.FrontierPlanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.GothicPlanJsonPath, result.GothicPlanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CaravanPlanJsonPath, result.CaravanPlanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CrossArtifactLinkageReportJsonPath, result.CrossArtifactLinkageReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        return write;
    }

    public static SemanticPackCompositionInvalidMatrix BuildInvalidMatrix()
    {
        var packs = SemanticPackCompositionCatalog.BuildDefaultPacks();
        var contracts = SemanticArtifactContractRegistry.BuildDefaultContracts();
        var planner = new SemanticPackCompositionPlanner(packs, contracts);
        var scenarios = new List<SemanticPackCompositionInvalidScenario>
        {
            Invalid("duplicate_pack_id_mutation", "duplicate semantic pack id", SemanticPackCompositionValidator.ValidateCatalog([.. packs, packs[0]], contracts)),
            PlannerInvalid("unknown_profile_family_mutation", "unknown profile/family mutation", planner.BuildBlueprint(new SemanticPackCompositionRequest
            {
                ProfileId = "unknown_family",
                SelectedPackIds = ["semantic_pack/core_blueprint_spine"]
            })),
            Invalid("missing_semantic_scope_mutation", "missing semantic scope mutation", SemanticPackCompositionValidator.ValidateCatalog(Mutate(packs, "semantic_pack/core_blueprint_spine", pack => pack with { ProvidedSemanticScopes = [] }), contracts)),
            Invalid("duplicate_fact_id_mutation", "duplicate fact id mutation", SemanticPackCompositionValidator.ValidateCatalog(Mutate(packs, "semantic_pack/frontier_survival", pack => pack with { Facts = [.. pack.Facts, pack.Facts[0]] }), contracts)),
            Invalid("unknown_fact_relation_mutation", "unknown fact relation mutation", SemanticPackCompositionValidator.ValidateCatalog(Mutate(packs, "semantic_pack/frontier_survival", pack => pack with
            {
                RelationHints = [.. pack.RelationHints, new SemanticPackRelationHint
                {
                    RelationId = "frontier.link.fake_fact",
                    SourceFactId = "frontier.npc.trail_medic",
                    RelationKind = "implies",
                    TargetFactId = "fake.fact"
                }]
            }), contracts)),
            Invalid("fake_goal030_contract_mutation", "fake Goal 030 contract/artifact mutation", SemanticPackCompositionValidator.ValidateCatalog(Mutate(packs, "semantic_pack/frontier_survival", pack => pack with
            {
                ExpansionIntents = [.. pack.ExpansionIntents, new SemanticPackExpansionIntent
                {
                    IntentId = "frontier.intent.fake_contract",
                    SourceFactId = "frontier.npc.trail_medic",
                    TargetContractId = "fake_contract_v1",
                    TargetArtifactKind = "fake_artifact_kind",
                    Priority = 999
                }]
            }), contracts)),
            PlannerInvalid("incompatible_pack_selection_mutation", "incompatible pack selection mutation", new SemanticPackCompositionPlanner(Mutate(packs, "semantic_pack/frontier_survival", pack => pack with
            {
                Exclusions = ["semantic_pack/winter_hazards"]
            }), contracts).BuildBlueprint(SemanticPackCompositionCatalog.FrontierRequest())),
            PlannerInvalid("future_only_pack_selected_mutation", "future-only pack selected as ready", new SemanticPackCompositionPlanner(Mutate(packs, "semantic_pack/winter_hazards", pack => pack with
            {
                IsFutureOnly = true,
                SourceStatus = "candidate"
            }), contracts).BuildBlueprint(SemanticPackCompositionCatalog.FrontierRequest())),
            PlannerInvalid("fake_selected_pack_id_mutation", "fake selected pack id accepted by composer", planner.BuildBlueprint(new SemanticPackCompositionRequest
            {
                ProfileId = "frontier_survival",
                SelectedPackIds = ["semantic_pack/core_blueprint_spine", "semantic_pack/fake_pack"]
            })),
            Invalid("leakage_attempt_mutation", "Runtime/UI/Unity/provider/LLM/RAG/Lua/GamePackage-schema leakage attempt", SemanticPackCompositionValidator.ValidateCatalog(Mutate(packs, "semantic_pack/core_blueprint_spine", pack => pack with
            {
                ThemeTags = [.. pack.ThemeTags, "llm_runtime_provider_bridge"],
                SourceNotes = "Goal 031 should call LLM, execute Lua and mutate GamePackage schema."
            }), contracts))
        };

        return new SemanticPackCompositionInvalidMatrix
        {
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(scenario => !scenario.ActualValid),
            Passed = scenarios.All(scenario => !scenario.ActualValid),
            Scenarios = scenarios.OrderBy(scenario => scenario.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static SemanticPackCatalogSummary BuildCatalogSummary(
        IReadOnlyList<SemanticPackCompositionPack> packs,
        IReadOnlyList<SemanticArtifactDiagnostic> diagnostics) =>
        new()
        {
            PackCount = packs.Count,
            FactCount = packs.Sum(pack => pack.Facts.Count),
            PackIds = packs.Select(pack => pack.PackId).Order(StringComparer.Ordinal).ToList(),
            FactDomainCounts = packs
                .SelectMany(pack => pack.Facts)
                .GroupBy(fact => fact.Domain, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal),
            ProfileIds = packs
                .SelectMany(pack => pack.SupportedProfileIds)
                .Where(profile => profile != "*")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList(),
            Diagnostics = diagnostics
        };

    private static SemanticPackCompositionMatrix BuildMatrix(IReadOnlyList<SemanticBlueprintPlan> plans)
    {
        var rows = plans
            .OrderBy(plan => plan.ProfileId, StringComparer.Ordinal)
            .Select(plan => new SemanticPackCompositionMatrixRow
            {
                ProfileId = plan.ProfileId,
                SelectedPackIds = plan.SelectedPackIds,
                FactCount = plan.MergedSemanticFacts.Count,
                RelationCount = plan.RelationGraph.Count,
                LinkCount = plan.CrossArtifactLinks.Count,
                SectionCount = plan.Sections.Count,
                CoverageContractIds = plan.Goal030CoverageContractIds,
                StableSummary = plan.StableSummary
            })
            .ToList();

        return new SemanticPackCompositionMatrix
        {
            Rows = rows,
            ComposerSharedByAllScenarios = rows.Count == 3 && rows.All(row => row.FactCount > 0 && row.SectionCount == 11),
            Goal030PlannerUsedByAllScenarios = rows.All(row => row.CoverageContractIds.Contains("semantic_pack_v1", StringComparer.Ordinal)),
            ScenariosAreMeaningfullyDifferent = rows.Select(row => string.Join("|", row.SelectedPackIds)).Distinct(StringComparer.Ordinal).Count() == rows.Count
        };
    }

    private static SemanticPackLinkageReport BuildLinkageReport(IReadOnlyList<SemanticBlueprintPlan> plans)
    {
        var links = plans
            .SelectMany(plan => plan.CrossArtifactLinks.Select(link => link with
            {
                LinkId = $"{plan.ProfileId}:{link.LinkId}"
            }))
            .OrderBy(link => link.LinkId, StringComparer.Ordinal)
            .ToList();

        return new SemanticPackLinkageReport
        {
            ScenarioCount = plans.Count,
            LinkCount = links.Count,
            Links = links
        };
    }

    private static SemanticPackCompositionInvalidScenario Invalid(
        string id,
        string kind,
        IReadOnlyList<SemanticArtifactDiagnostic> diagnostics)
    {
        var sorted = SemanticPackCompositionValidator.SortDiagnostics(diagnostics);
        return new SemanticPackCompositionInvalidScenario
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = sorted.All(diagnostic => diagnostic.Severity != "error"),
            MutatedEvidenceKind = kind,
            Diagnostics = sorted
        };
    }

    private static SemanticPackCompositionInvalidScenario PlannerInvalid(string id, string kind, SemanticBlueprintPlan plan)
    {
        var sorted = SemanticPackCompositionValidator.SortDiagnostics(plan.Diagnostics);
        return new SemanticPackCompositionInvalidScenario
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = sorted.All(diagnostic => diagnostic.Severity != "error"),
            MutatedEvidenceKind = kind,
            Diagnostics = sorted
        };
    }

    private static IReadOnlyList<SemanticPackCompositionPack> Mutate(
        IReadOnlyList<SemanticPackCompositionPack> packs,
        string packId,
        Func<SemanticPackCompositionPack, SemanticPackCompositionPack> mutate) =>
        packs
            .Select(pack => pack.PackId == packId ? mutate(pack) : pack)
            .ToList();

    private static string RenderReport(SemanticPackCompositionBlueprintReport report, SemanticPackCompositionMatrix matrix)
    {
        var lines = new List<string>
        {
            "# Semantic Pack Composition Blueprint Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- finalStatus: {report.FinalStatus}",
            $"- manualGate: {report.ManualGate}",
            $"- required marker: {FinalGate} required",
            $"- previousAcceptedGate: {report.PreviousAcceptedGate}",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- blueprintProofPassed: {report.BlueprintProofPassed.ToString().ToLowerInvariant()}",
            $"- packCount: {report.PackCount}",
            $"- scenarioCount: {report.ScenarioCount}",
            $"- invalidMatrixPassed: {report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- catalogSummaryHash: {report.CatalogSummaryHash}",
            $"- compositionMatrixHash: {report.CompositionMatrixHash}",
            $"- crossArtifactLinkageHash: {report.CrossArtifactLinkageHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Selected semantic packs can now be composed into a deterministic cross-artifact generation blueprint that links world, biome, faction, NPC, quest, dialogue, economy, combat, settlement and event intent before GamePackage materialization.",
            string.Empty,
            "## Scenarios",
            string.Empty
        };
        lines.AddRange(matrix.Rows.Select(row => $"- {row.ProfileId}: packs={string.Join(",", row.SelectedPackIds)}, facts={row.FactCount}, relations={row.RelationCount}, links={row.LinkCount}, contracts={row.CoverageContractIds.Count}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.AddRange(report.InvalidMatrix.Scenarios.Select(scenario => $"- {scenario.ScenarioId}: rejected={(!scenario.ActualValid).ToString().ToLowerInvariant()}, codes={string.Join(",", scenario.Diagnostics.Select(item => item.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add($"- publicGamePackageSchemaChanged: {report.PublicGamePackageSchemaChanged.ToString().ToLowerInvariant()}");
        lines.Add($"- runtimeBehaviorChanged: {report.RuntimeBehaviorChanged.ToString().ToLowerInvariant()}");
        lines.Add($"- unityBuildExecuted: {report.UnityBuildExecuted.ToString().ToLowerInvariant()}");
        lines.Add($"- llmRagProviderMediaLuaExecuted: {report.LlmRagProviderMediaLuaExecuted.ToString().ToLowerInvariant()}");
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

public sealed record SemanticPackCompositionEvidenceResult
{
    public SemanticPackCatalogSummary CatalogSummary { get; init; } = new();
    public SemanticPackCompositionMatrix CompositionMatrix { get; init; } = new();
    public SemanticBlueprintPlan FrontierPlan { get; init; } = new();
    public SemanticBlueprintPlan GothicPlan { get; init; } = new();
    public SemanticBlueprintPlan CaravanPlan { get; init; } = new();
    public SemanticPackLinkageReport CrossArtifactLinkageReport { get; init; } = new();
    public SemanticPackCompositionBlueprintReport Report { get; init; } = new();
    public string CatalogSummaryJson { get; init; } = string.Empty;
    public string CompositionMatrixJson { get; init; } = string.Empty;
    public string FrontierPlanJson { get; init; } = string.Empty;
    public string GothicPlanJson { get; init; } = string.Empty;
    public string CaravanPlanJson { get; init; } = string.Empty;
    public string CrossArtifactLinkageReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record SemanticPackCompositionEvidenceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string CatalogSummaryJsonPath { get; init; } = string.Empty;
    public string CompositionMatrixJsonPath { get; init; } = string.Empty;
    public string FrontierPlanJsonPath { get; init; } = string.Empty;
    public string GothicPlanJsonPath { get; init; } = string.Empty;
    public string CaravanPlanJsonPath { get; init; } = string.Empty;
    public string CrossArtifactLinkageReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
}
