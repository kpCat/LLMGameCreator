using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.DynamicSemanticFeatures;

public sealed class DynamicSemanticFeatureEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-032-dynamic-semantic-feature-system";
    public const string FeatureCatalogSummaryJsonFileName = "feature-catalog-summary.json";
    public const string InfluenceRuleSummaryJsonFileName = "influence-rule-summary.json";
    public const string AuthoringSchemaMatrixJsonFileName = "dynamic-authoring-schema-matrix.json";
    public const string FrontierStateJsonFileName = "resolved-feature-state-frontier.json";
    public const string GothicStateJsonFileName = "resolved-feature-state-gothic.json";
    public const string CaravanStateJsonFileName = "resolved-feature-state-caravan.json";
    public const string MetamoduleKingdomsStateJsonFileName = "resolved-feature-state-metamodule-kingdoms.json";
    public const string InvalidMatrixJsonFileName = "invalid-feature-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "dynamic-semantic-feature-system-report.md";
    public const string FinalGate = "dynamic_semantic_feature_system_verification";
    public const string PreviousProducedGate = "semantic_pack_composition_blueprint_verification required";
    public const string ProductSmokeRoute = "goal-032-dynamic-semantic-feature-system";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public DynamicSemanticFeatureEvidenceResult Build()
    {
        var definitions = DynamicSemanticFeatureCatalog.BuildDefaultFeatureDefinitions();
        var rules = DynamicSemanticFeatureCatalog.BuildDefaultInfluenceRules();
        var scenarios = DynamicSemanticFeatureCatalog.BuildDefaultScenarios();
        var resolver = new DynamicSemanticFeatureResolver();
        var resolved = scenarios.Select(scenario => resolver.ResolveScenario(scenario, definitions)).ToList();
        var catalogDiagnostics = DynamicSemanticFeatureValidator.ValidateCatalog(definitions, rules);
        var catalogSummary = BuildCatalogSummary(definitions, catalogDiagnostics);
        var ruleSummary = BuildRuleSummary(rules, catalogDiagnostics);
        var authoringSchema = new DynamicSemanticAuthoringSchemaPlanner().Build(definitions, resolved);
        var invalidMatrix = BuildInvalidMatrix();

        var featureCatalogSummaryJson = Serialize(catalogSummary);
        var influenceRuleSummaryJson = Serialize(ruleSummary);
        var authoringSchemaJson = Serialize(authoringSchema);
        var frontierJson = Serialize(Find(resolved, "frontier_survival"));
        var gothicJson = Serialize(Find(resolved, "gothic_intrigue"));
        var caravanJson = Serialize(Find(resolved, "caravan_trade"));
        var metamoduleJson = Serialize(Find(resolved, "metamodule_kingdoms"));
        var invalidMatrixJson = Serialize(invalidMatrix);
        var stateHashes = new[] { frontierJson, gothicJson, caravanJson, metamoduleJson }.Select(ComputeHash).Order(StringComparer.Ordinal).ToList();
        var allDiagnostics = catalogDiagnostics.Concat(resolved.SelectMany(item => item.Diagnostics)).ToList();

        var reportWithoutHash = new DynamicSemanticFeatureSystemReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousProducedGate = PreviousProducedGate,
            ProductSmokeRoute = ProductSmokeRoute,
            FeatureModelImplemented = true,
            ApplicabilityImplemented = true,
            InheritanceKernelImplemented = true,
            InfluenceRulesImplemented = true,
            ResolverImplemented = true,
            AuthoringSchemaImplemented = true,
            EvidenceArtifactsWritten = true,
            ScenarioCount = resolved.Count,
            FeatureCount = definitions.Count,
            InfluenceRuleCount = rules.Count,
            CatalogValidated = catalogDiagnostics.All(item => item.Severity != "error"),
            ResolverDiagnosticsCleanForValidScenarios = resolved.All(item => item.Diagnostics.All(diagnostic => diagnostic.Severity != "error")),
            InvalidMatrixPassed = invalidMatrix.Passed,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            UnityBuildExecuted = false,
            LlmRagProviderMediaLuaExecuted = false,
            RuntimeBehaviorChanged = false,
            FeatureCatalogSummaryHash = ComputeHash(featureCatalogSummaryJson),
            InfluenceRuleSummaryHash = ComputeHash(influenceRuleSummaryJson),
            AuthoringSchemaMatrixHash = ComputeHash(authoringSchemaJson),
            ResolvedStateHashes = stateHashes,
            InvalidMatrixHash = ComputeHash(invalidMatrixJson),
            Diagnostics = DynamicSemanticFeatureValidator.SortDiagnostics(allDiagnostics),
            InvalidMatrix = invalidMatrix
        };
        var report = reportWithoutHash with
        {
            ContractProofPassed = reportWithoutHash.CatalogValidated
                && reportWithoutHash.ResolverDiagnosticsCleanForValidScenarios
                && reportWithoutHash.InvalidMatrixPassed
                && resolved.Count == 4
                && Find(resolved, "metamodule_kingdoms").StableSummary.Contains("diagnostics=0", StringComparison.Ordinal),
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new DynamicSemanticFeatureEvidenceResult
        {
            FeatureCatalogSummary = catalogSummary,
            InfluenceRuleSummary = ruleSummary,
            AuthoringSchemaMatrix = authoringSchema,
            FrontierState = Find(resolved, "frontier_survival"),
            GothicState = Find(resolved, "gothic_intrigue"),
            CaravanState = Find(resolved, "caravan_trade"),
            MetamoduleKingdomsState = Find(resolved, "metamodule_kingdoms"),
            InvalidMatrix = invalidMatrix,
            Report = report,
            FeatureCatalogSummaryJson = featureCatalogSummaryJson,
            InfluenceRuleSummaryJson = influenceRuleSummaryJson,
            AuthoringSchemaMatrixJson = authoringSchemaJson,
            FrontierStateJson = frontierJson,
            GothicStateJson = gothicJson,
            CaravanStateJson = caravanJson,
            MetamoduleKingdomsStateJson = metamoduleJson,
            InvalidMatrixJson = invalidMatrixJson,
            ReportMarkdown = RenderReport(report, resolved)
        };
    }

    public async Task<DynamicSemanticFeatureEvidenceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<DynamicSemanticFeatureEvidenceWriteResult> WriteAsync(
        string projectRootPath,
        DynamicSemanticFeatureEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new DynamicSemanticFeatureEvidenceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            FeatureCatalogSummaryJsonPath = Path.Combine(outputDirectory, FeatureCatalogSummaryJsonFileName),
            InfluenceRuleSummaryJsonPath = Path.Combine(outputDirectory, InfluenceRuleSummaryJsonFileName),
            AuthoringSchemaMatrixJsonPath = Path.Combine(outputDirectory, AuthoringSchemaMatrixJsonFileName),
            FrontierStateJsonPath = Path.Combine(outputDirectory, FrontierStateJsonFileName),
            GothicStateJsonPath = Path.Combine(outputDirectory, GothicStateJsonFileName),
            CaravanStateJsonPath = Path.Combine(outputDirectory, CaravanStateJsonFileName),
            MetamoduleKingdomsStateJsonPath = Path.Combine(outputDirectory, MetamoduleKingdomsStateJsonFileName),
            InvalidMatrixJsonPath = Path.Combine(outputDirectory, InvalidMatrixJsonFileName),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName)
        };

        await File.WriteAllTextAsync(write.FeatureCatalogSummaryJsonPath, result.FeatureCatalogSummaryJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.InfluenceRuleSummaryJsonPath, result.InfluenceRuleSummaryJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.AuthoringSchemaMatrixJsonPath, result.AuthoringSchemaMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.FrontierStateJsonPath, result.FrontierStateJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.GothicStateJsonPath, result.GothicStateJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CaravanStateJsonPath, result.CaravanStateJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.MetamoduleKingdomsStateJsonPath, result.MetamoduleKingdomsStateJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.InvalidMatrixJsonPath, result.InvalidMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        return write;
    }

    public static DynamicSemanticInvalidMatrix BuildInvalidMatrix()
    {
        var definitions = DynamicSemanticFeatureCatalog.BuildDefaultFeatureDefinitions();
        var rules = DynamicSemanticFeatureCatalog.BuildDefaultInfluenceRules();
        var frontier = DynamicSemanticFeatureCatalog.FrontierScenario();
        var resolver = new DynamicSemanticFeatureResolver();
        var scenarios = new List<DynamicSemanticInvalidScenario>
        {
            Invalid("duplicate_feature_id", "duplicate feature id", DynamicSemanticFeatureValidator.ValidateCatalog([.. definitions, definitions[0]], rules)),
            Invalid("invalid_empty_id", "invalid/empty id", DynamicSemanticFeatureValidator.ValidateCatalog(MutateDefinition(definitions, "world.theme", item => item with { FeatureId = "" }), rules)),
            ResolveInvalid("unknown_feature_reference", "unknown feature reference", resolver.Resolve(MutateRequest(frontier, definitions, rules, assignments: [.. frontier.Assignments, frontier.Assignments[0] with { FeatureId = "fake.feature" }]))),
            ResolveInvalid("unknown_target_scope", "unknown target scope", resolver.Resolve(MutateRequest(frontier, definitions, rules, targets: MutateTarget(frontier.Targets, "npc/trail_medic", item => item with { TargetScope = "fake_scope" })))),
            ResolveInvalid("invalid_value_shape", "invalid value shape", resolver.Resolve(MutateRequest(frontier, definitions, rules, assignments: MutateAssignment(frontier.Assignments, "npc/trail_medic", "npc.hunger", item => item with { Value = DynamicSemanticFeatureCatalog.Enum("not_number") })))),
            ResolveInvalid("illegal_assignment_scope", "illegal assignment for target scope", resolver.Resolve(MutateRequest(frontier, definitions, rules, assignments: [.. frontier.Assignments, frontier.Assignments[0] with { TargetId = "npc/trail_medic", TargetScope = "npc", FeatureId = "quest.motive" }]))),
            ResolveInvalid("required_feature_missing", "required feature missing", resolver.Resolve(MutateRequest(frontier, MutateDefinition(definitions, "quest.motive", item => item with { DefaultStrategy = "none", DefaultValue = null }), rules, assignments: frontier.Assignments.Where(item => item.FeatureId != "quest.motive").ToList()))),
            Valid("optional_feature_missing_is_traceable", "optional feature missing is not an error", resolver.Resolve(MutateRequest(frontier, definitions, rules, targets: [Target("world/frontier", "world", [], ["frontier"]), Target("npc/quiet_wanderer", "npc", ["world/frontier"], [])], assignments: [Assign("world/frontier", "world", "world.theme", DynamicSemanticFeatureCatalog.Enum("frontier"))], targetIds: ["npc/quiet_wanderer"]))),
            ResolveInvalid("feature_conflict", "feature conflict", resolver.Resolve(MutateRequest(frontier, MutateDefinition(definitions, "npc.trust", item => item with { Conflicts = ["npc.hunger"] }), rules))),
            ResolveInvalid("unknown_inheritance_source", "unknown inheritance source", resolver.Resolve(MutateRequest(frontier, definitions, rules, targets: MutateTarget(frontier.Targets, "npc/trail_medic", item => item with { ParentTargetIds = ["target/fake_parent"] })))),
            ResolveInvalid("circular_inheritance", "circular inheritance", resolver.Resolve(MutateRequest(frontier, definitions, rules, targets: MutateTarget(MutateTarget(frontier.Targets, "world/frontier", item => item with { ParentTargetIds = ["npc/trail_medic"] }), "npc/trail_medic", item => item with { ParentTargetIds = ["world/frontier"] })))),
            Invalid("unknown_influence_target", "unknown influence target", DynamicSemanticFeatureValidator.ValidateCatalog(definitions, [.. rules, rules[0] with { RuleId = "rule/fake_target", Effects = [new DynamicSemanticInfluenceEffect { EffectKind = "set_feature", FeatureId = "fake.feature", Value = DynamicSemanticFeatureCatalog.Enum("x") }] }])),
            Invalid("circular_influence", "circular influence", DynamicSemanticFeatureValidator.ValidateCatalog(definitions, [.. rules, CycleRule("rule/cycle_a", "npc.trust", "npc.hunger"), CycleRule("rule/cycle_b", "npc.hunger", "npc.trust")])),
            Invalid("self_feeding_influence", "self feeding influence", DynamicSemanticFeatureValidator.ValidateCatalog(definitions, [.. rules, CycleRule("rule/self_feed", "npc.hunger", "npc.hunger")])),
            ResolveInvalid("overconstrained_output", "overconstrained output", resolver.Resolve(MutateRequest(frontier, definitions, [.. rules, new DynamicSemanticInfluenceRule
            {
                RuleId = "rule/block_hunger",
                TargetScope = "npc",
                TargetFamily = "frontier",
                Conditions = [new DynamicSemanticConditionClause { Operator = "feature_exists", FeatureId = "npc.hunger" }],
                Effects = [new DynamicSemanticInfluenceEffect { EffectKind = "block_feature", FeatureId = "npc.hunger" }],
                Priority = 1,
                TieBreaker = "rule/block_hunger"
            }]))),
            ResolveInvalid("fake_selected_feature_id", "fake selected feature id", resolver.Resolve(MutateRequest(frontier, definitions, rules, targetIds: ["target/fake_selected"]))),
            Invalid("forbidden_leakage_terms", "forbidden Runtime/UI/Unity/provider/LLM/RAG/Lua/GamePackage schema leakage", DynamicSemanticFeatureValidator.ValidateCatalog(MutateDefinition(definitions, "world.theme", item => item with { Notes = "Call LLM, mutate GamePackage schema, execute Lua and update Unity Runtime UI." }), rules))
        };

        return new DynamicSemanticInvalidMatrix
        {
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedValid == item.ActualValid),
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            Passed = scenarios.All(item => item.ExpectedValid == item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static DynamicSemanticFeatureCatalogSummary BuildCatalogSummary(
        IReadOnlyList<DynamicSemanticFeatureDefinition> definitions,
        IReadOnlyList<DynamicSemanticDiagnostic> diagnostics) =>
        new()
        {
            FeatureCount = definitions.Count,
            FeatureIds = definitions.Select(item => item.FeatureId).Order(StringComparer.Ordinal).ToList(),
            ScopeCount = definitions.GroupBy(item => item.TargetScope, StringComparer.Ordinal).OrderBy(item => item.Key, StringComparer.Ordinal).ToDictionary(item => item.Key, item => item.Count(), StringComparer.Ordinal),
            ValueKinds = definitions.Select(item => item.ValueKind).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics
        };

    private static DynamicSemanticInfluenceRuleSummary BuildRuleSummary(
        IReadOnlyList<DynamicSemanticInfluenceRule> rules,
        IReadOnlyList<DynamicSemanticDiagnostic> diagnostics) =>
        new()
        {
            RuleCount = rules.Count,
            RuleIds = rules.Select(item => item.RuleId).Order(StringComparer.Ordinal).ToList(),
            ConditionOperators = rules.SelectMany(item => item.Conditions).Select(item => item.Operator).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            EffectKinds = rules.SelectMany(item => item.Effects).Select(item => item.EffectKind).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics.Where(item => item.Code.Contains("influence", StringComparison.Ordinal)).ToList()
        };

    private static DynamicSemanticResolvedScenarioState Find(IReadOnlyList<DynamicSemanticResolvedScenarioState> states, string scenarioId) =>
        states.Single(item => item.ScenarioId == scenarioId);

    private static DynamicSemanticInvalidScenario Invalid(string id, string kind, IReadOnlyList<DynamicSemanticDiagnostic> diagnostics)
    {
        var sorted = DynamicSemanticFeatureValidator.SortDiagnostics(diagnostics);
        return new DynamicSemanticInvalidScenario
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = sorted.All(item => item.Severity != "error"),
            MutatedEvidenceKind = kind,
            Diagnostics = sorted
        };
    }

    private static DynamicSemanticInvalidScenario ResolveInvalid(string id, string kind, DynamicSemanticResolvedScenarioState state) =>
        new()
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = state.Diagnostics.All(item => item.Severity != "error"),
            MutatedEvidenceKind = kind,
            Diagnostics = state.Diagnostics
        };

    private static DynamicSemanticInvalidScenario Valid(string id, string kind, DynamicSemanticResolvedScenarioState state) =>
        new()
        {
            ScenarioId = id,
            ExpectedValid = true,
            ActualValid = state.Diagnostics.All(item => item.Severity != "error"),
            MutatedEvidenceKind = kind,
            Diagnostics = state.Diagnostics
        };

    private static DynamicSemanticResolveRequest MutateRequest(
        DynamicSemanticScenario scenario,
        IReadOnlyList<DynamicSemanticFeatureDefinition> definitions,
        IReadOnlyList<DynamicSemanticInfluenceRule> rules,
        IReadOnlyList<DynamicSemanticTargetNode>? targets = null,
        IReadOnlyList<DynamicSemanticFeatureAssignment>? assignments = null,
        IReadOnlyList<string>? targetIds = null) =>
        new()
        {
            FeatureDefinitions = definitions,
            Assignments = assignments ?? scenario.Assignments,
            InfluenceRules = rules,
            Targets = targets ?? scenario.Targets,
            TargetIds = targetIds ?? scenario.ResolveTargetIds,
            ProfileId = scenario.ProfileId,
            Seed = scenario.Seed
        };

    private static IReadOnlyList<DynamicSemanticFeatureDefinition> MutateDefinition(
        IReadOnlyList<DynamicSemanticFeatureDefinition> definitions,
        string featureId,
        Func<DynamicSemanticFeatureDefinition, DynamicSemanticFeatureDefinition> mutate) =>
        definitions.Select(item => item.FeatureId == featureId ? mutate(item) : item).ToList();

    private static IReadOnlyList<DynamicSemanticTargetNode> MutateTarget(
        IReadOnlyList<DynamicSemanticTargetNode> targets,
        string targetId,
        Func<DynamicSemanticTargetNode, DynamicSemanticTargetNode> mutate) =>
        targets.Select(item => item.TargetId == targetId ? mutate(item) : item).ToList();

    private static IReadOnlyList<DynamicSemanticFeatureAssignment> MutateAssignment(
        IReadOnlyList<DynamicSemanticFeatureAssignment> assignments,
        string targetId,
        string featureId,
        Func<DynamicSemanticFeatureAssignment, DynamicSemanticFeatureAssignment> mutate) =>
        assignments.Select(item => item.TargetId == targetId && item.FeatureId == featureId ? mutate(item) : item).ToList();

    private static DynamicSemanticInfluenceRule CycleRule(string id, string conditionFeature, string effectFeature) =>
        new()
        {
            RuleId = id,
            TargetScope = "npc",
            Conditions = [new DynamicSemanticConditionClause { Operator = "feature_exists", FeatureId = conditionFeature }],
            Effects = [new DynamicSemanticInfluenceEffect { EffectKind = "adjust_number", FeatureId = effectFeature, NumberDelta = 1 }],
            Priority = 900,
            TieBreaker = id
        };

    private static DynamicSemanticTargetNode Target(string id, string scope, IReadOnlyList<string> parents, IReadOnlyList<string> tags) =>
        new()
        {
            TargetId = id,
            TargetScope = scope,
            ParentTargetIds = parents,
            Tags = tags,
            FamilyIds = tags
        };

    private static DynamicSemanticFeatureAssignment Assign(string targetId, string scope, string featureId, DynamicSemanticFeatureValue value) =>
        new()
        {
            TargetId = targetId,
            TargetScope = scope,
            FeatureId = featureId,
            Value = value,
            SourceLayer = scope,
            SourceId = targetId,
            Provenance = "invalid_matrix_fixture"
        };

    private static string RenderReport(DynamicSemanticFeatureSystemReport report, IReadOnlyList<DynamicSemanticResolvedScenarioState> states)
    {
        var lines = new List<string>
        {
            "# Dynamic Semantic Feature System Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- finalStatus: {report.FinalStatus}",
            $"- manualGate: {report.ManualGate}",
            $"- required marker: {FinalGate} required",
            $"- previousProducedGate: {report.PreviousProducedGate}",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- contractProofPassed: {report.ContractProofPassed.ToString().ToLowerInvariant()}",
            $"- featureCount: {report.FeatureCount}",
            $"- influenceRuleCount: {report.InfluenceRuleCount}",
            $"- scenarioCount: {report.ScenarioCount}",
            $"- invalidMatrixPassed: {report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- featureCatalogSummaryHash: {report.FeatureCatalogSummaryHash}",
            $"- influenceRuleSummaryHash: {report.InfluenceRuleSummaryHash}",
            $"- authoringSchemaMatrixHash: {report.AuthoringSchemaMatrixHash}",
            $"- invalidMatrixHash: {report.InvalidMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Semantic variability now has an Application-layer feature, inheritance, influence and authoring-schema kernel. LLM can remain a seed/lore drafting helper while deterministic C# resolves NPC, faction, quest, dialogue, species/archetype and kingdom pressure combinations.",
            string.Empty,
            "## Scenarios",
            string.Empty
        };
        lines.AddRange(states.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).Select(item => $"- {item.ScenarioId}: targets={item.TargetStates.Count}, summary={item.StableSummary}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.AddRange(report.InvalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedValid={item.ExpectedValid.ToString().ToLowerInvariant()}, actualValid={item.ActualValid.ToString().ToLowerInvariant()}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
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

public sealed record DynamicSemanticFeatureCatalogSummary
{
    public string SchemaVersion { get; init; } = "dynamic_semantic_feature_catalog_summary_v1";
    public int FeatureCount { get; init; }
    public IReadOnlyList<string> FeatureIds { get; init; } = [];
    public IReadOnlyDictionary<string, int> ScopeCount { get; init; } = new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<string> ValueKinds { get; init; } = [];
    public IReadOnlyList<DynamicSemanticDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record DynamicSemanticInfluenceRuleSummary
{
    public string SchemaVersion { get; init; } = "dynamic_semantic_influence_rule_summary_v1";
    public int RuleCount { get; init; }
    public IReadOnlyList<string> RuleIds { get; init; } = [];
    public IReadOnlyList<string> ConditionOperators { get; init; } = [];
    public IReadOnlyList<string> EffectKinds { get; init; } = [];
    public IReadOnlyList<DynamicSemanticDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record DynamicSemanticFeatureSystemReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousProducedGate { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool FeatureModelImplemented { get; init; }
    public bool ApplicabilityImplemented { get; init; }
    public bool InheritanceKernelImplemented { get; init; }
    public bool InfluenceRulesImplemented { get; init; }
    public bool ResolverImplemented { get; init; }
    public bool AuthoringSchemaImplemented { get; init; }
    public bool EvidenceArtifactsWritten { get; init; }
    public int ScenarioCount { get; init; }
    public int FeatureCount { get; init; }
    public int InfluenceRuleCount { get; init; }
    public bool CatalogValidated { get; init; }
    public bool ResolverDiagnosticsCleanForValidScenarios { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool LlmRagProviderMediaLuaExecuted { get; init; }
    public bool RuntimeBehaviorChanged { get; init; }
    public string FeatureCatalogSummaryHash { get; init; } = string.Empty;
    public string InfluenceRuleSummaryHash { get; init; } = string.Empty;
    public string AuthoringSchemaMatrixHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ResolvedStateHashes { get; init; } = [];
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public DynamicSemanticInvalidMatrix InvalidMatrix { get; init; } = new();
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<DynamicSemanticDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record DynamicSemanticInvalidMatrix
{
    public string SchemaVersion { get; init; } = "dynamic_semantic_invalid_matrix_v1";
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<DynamicSemanticInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record DynamicSemanticInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<DynamicSemanticDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record DynamicSemanticFeatureEvidenceResult
{
    public DynamicSemanticFeatureCatalogSummary FeatureCatalogSummary { get; init; } = new();
    public DynamicSemanticInfluenceRuleSummary InfluenceRuleSummary { get; init; } = new();
    public DynamicSemanticAuthoringSchemaMatrix AuthoringSchemaMatrix { get; init; } = new();
    public DynamicSemanticResolvedScenarioState FrontierState { get; init; } = new();
    public DynamicSemanticResolvedScenarioState GothicState { get; init; } = new();
    public DynamicSemanticResolvedScenarioState CaravanState { get; init; } = new();
    public DynamicSemanticResolvedScenarioState MetamoduleKingdomsState { get; init; } = new();
    public DynamicSemanticInvalidMatrix InvalidMatrix { get; init; } = new();
    public DynamicSemanticFeatureSystemReport Report { get; init; } = new();
    public string FeatureCatalogSummaryJson { get; init; } = string.Empty;
    public string InfluenceRuleSummaryJson { get; init; } = string.Empty;
    public string AuthoringSchemaMatrixJson { get; init; } = string.Empty;
    public string FrontierStateJson { get; init; } = string.Empty;
    public string GothicStateJson { get; init; } = string.Empty;
    public string CaravanStateJson { get; init; } = string.Empty;
    public string MetamoduleKingdomsStateJson { get; init; } = string.Empty;
    public string InvalidMatrixJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record DynamicSemanticFeatureEvidenceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string FeatureCatalogSummaryJsonPath { get; init; } = string.Empty;
    public string InfluenceRuleSummaryJsonPath { get; init; } = string.Empty;
    public string AuthoringSchemaMatrixJsonPath { get; init; } = string.Empty;
    public string FrontierStateJsonPath { get; init; } = string.Empty;
    public string GothicStateJsonPath { get; init; } = string.Empty;
    public string CaravanStateJsonPath { get; init; } = string.Empty;
    public string MetamoduleKingdomsStateJsonPath { get; init; } = string.Empty;
    public string InvalidMatrixJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
}
