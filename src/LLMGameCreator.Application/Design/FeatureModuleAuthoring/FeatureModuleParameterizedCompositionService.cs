using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed record FeatureModuleParameterizedCompositionResult
{
    public string Status { get; init; } = "FAILED";
    public FeatureModuleCompositionDocument SourceDocument { get; init; } = new();
    public FeatureModuleCompositionDocument QualifiedDocument { get; init; } = new();
    public FeatureModuleCompositionStaleness Staleness { get; init; } = new();
    public FeatureModuleParameterizedCompositionPlan Plan { get; init; } = new();
    public FeatureModuleCompositionQualification Qualification { get; init; } = new();
    public string PackageJson { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public int EffectObservationCount { get; init; }
    public int PassedEffectObservationCount { get; init; }
    public int SelectedModuleCount { get; init; }
    public int SatisfiedSelectedModuleCount { get; init; }
    public bool AtomicParameterGroupsPassed { get; init; }
    public bool CheckpointReloadPassed { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public bool ActionBindingPassed { get; init; }
    public bool RuntimeEffectsPassed { get; init; }
    public bool Passed { get; init; }
    public string FailureStage { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class FeatureModuleParameterizedCompositionService
{
    private readonly FeatureModuleCompositionService _compositionService;
    private readonly FeatureModuleParameterizedCompositionPlanner _planner;
    private readonly FeatureModuleCompositionDocumentValidator _documentValidator;
    private readonly FeatureModuleCompositionStalenessService _staleness;

    public FeatureModuleParameterizedCompositionService(
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        FeatureModuleParameterizedCompositionPlanner? planner = null,
        FeatureModuleCompositionDocumentValidator? documentValidator = null,
        FeatureModuleCompositionStalenessService? staleness = null)
    {
        _compositionService = new FeatureModuleCompositionService(runtime ?? throw new ArgumentNullException(nameof(runtime)));
        _planner = planner ?? new FeatureModuleParameterizedCompositionPlanner();
        _documentValidator = documentValidator ?? new FeatureModuleCompositionDocumentValidator();
        _staleness = staleness ?? new FeatureModuleCompositionStalenessService();
    }

    public FeatureModuleParameterizedCompositionResult MaterializeAndQualify(
        string repositoryRoot,
        FeatureModuleLibrarySnapshot library,
        FeatureModuleCompositionDocument document,
        string outputRoot,
        bool useCapabilityDrivenRuntimePlaythrough = false)
    {
        var root = Path.GetFullPath(repositoryRoot);
        var validation = _documentValidator.Validate(document, library);
        if (!validation.Passed)
            throw new InvalidOperationException("saved composition is not materializable: " + string.Join("; ", validation.Diagnostics));
        var stale = _staleness.Evaluate(document, library);
        if (stale.Stale)
            throw new InvalidOperationException("stale or unresolved composition rejected: " + string.Join("; ", stale.Diagnostics));
        var (basePath, baseSha) = ResolveBaseline(root);
        var plan = _planner.Plan(
            library.Catalog,
            document.CompositionId,
            document.SelectedModuleIds,
            document.ParameterValues,
            Relative(root, basePath),
            baseSha);
        var effectiveCatalog = plan.ParameterBinding.EffectiveCatalog;
        FeatureModuleCompositionQualification qualification;
        try
        {
            qualification = _compositionService.ComposeAndQualify(
                root,
                effectiveCatalog,
                document.SelectedModuleIds,
                Path.GetFullPath(outputRoot),
                document.CompositionId,
                useCapabilityDrivenRuntimePlaythrough);
        }
        catch (InvalidOperationException exception) when (IsQualificationFailure(exception.Message))
        {
            var (stage, code) = ClassifyQualificationFailure(exception.Message);
            return new FeatureModuleParameterizedCompositionResult
            {
                Status = "FAILED",
                SourceDocument = document,
                QualifiedDocument = document with { LastQualificationStatus = "FAILED" },
                Staleness = stale,
                Plan = plan,
                SelectedModuleCount = document.SelectedModuleIds.Count,
                AtomicParameterGroupsPassed = plan.ParameterBinding.Passed,
                FailureStage = stage,
                Diagnostics = [code, code + ": " + exception.Message],
                Passed = false
            };
        }
        var semantic = qualification.Artifacts.SemanticEffects;
        var passed = qualification.Result.Passed
                     && semantic.SatisfiedSelectedModuleCount == document.SelectedModuleIds.Count
                     && plan.ParameterBinding.Passed;
        var failure = BuildFailureDiagnostics(plan, qualification, document.SelectedModuleIds.Count);
        if (!passed && failure.Diagnostics.Count == 0)
            failure = ("composition.qualification", ["composition.qualification.failed"]);
        qualification = qualification with
        {
            Result = qualification.Result with { Diagnostics = failure.Diagnostics }
        };
        var qualifiedDocument = document with
        {
            LastMaterializedPackageSha256 = qualification.Result.PackageSha256,
            LastQualifiedFinalStateHash = qualification.Result.FinalStateHash,
            LastQualificationStatus = passed ? "GREEN" : "FAILED"
        };
        return new FeatureModuleParameterizedCompositionResult
        {
            Status = passed ? "GREEN" : "FAILED",
            SourceDocument = document,
            QualifiedDocument = qualifiedDocument,
            Staleness = stale,
            Plan = plan,
            Qualification = qualification,
            PackageJson = qualification.Artifacts.PackageJson,
            PackageSha256 = qualification.Result.PackageSha256,
            FinalStateHash = qualification.Result.FinalStateHash,
            EffectObservationCount = semantic.EffectObservationCount,
            PassedEffectObservationCount = semantic.PassedEffectObservationCount,
            SelectedModuleCount = semantic.SelectedModuleCount,
            SatisfiedSelectedModuleCount = semantic.SatisfiedSelectedModuleCount,
            AtomicParameterGroupsPassed = plan.ParameterBinding.Passed,
            CheckpointReloadPassed = qualification.Result.CheckpointReloadPassed,
            FullReplayEquivalent = qualification.Result.FullReplayEquivalent,
            ActionBindingPassed = qualification.Result.ActionBindingsPassed,
            RuntimeEffectsPassed = semantic.Passed,
            Passed = passed,
            FailureStage = passed ? string.Empty : failure.Stage,
            Diagnostics = passed ? [] : failure.Diagnostics
        };
    }

    private static (string Stage, IReadOnlyList<string> Diagnostics) BuildFailureDiagnostics(
        FeatureModuleParameterizedCompositionPlan plan,
        FeatureModuleCompositionQualification qualification,
        int selectedModuleCount)
    {
        var result = qualification.Result;
        var artifacts = qualification.Artifacts;
        var diagnostics = new List<string>();
        string stage = string.Empty;
        void Failed(string failedStage, string code, IEnumerable<string>? details = null)
        {
            if (string.IsNullOrEmpty(stage)) stage = failedStage;
            diagnostics.Add(code);
            if (details is not null)
                diagnostics.AddRange(details.Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => code + ": " + value));
        }

        if (!plan.ParameterBinding.Passed)
            Failed("composition.parameter_binding", "composition.parameter_binding.failed", plan.ParameterBinding.Diagnostics);
        if (!result.PackageValidationPassed)
            Failed("composition.package_validation", "composition.package_validation.failed", artifacts.PackageValidation.Diagnostics);
        if (!result.MutationAuditPassed)
            Failed("composition.mutation_audit", "composition.mutation_audit.failed", artifacts.MutationAudit.Diagnostics);
        if (!artifacts.Plan.Validation.Passed)
            Failed("composition.validation", "composition.validation.failed", artifacts.Plan.Validation.Diagnostics);
        if (!result.OrderIndependencePassed)
            Failed("composition.order_independence", "composition.order_independence.failed");
        if (!result.InvalidActionStateUnchanged)
            Failed("runtime.invalid_action", "runtime.invalid_action_state_changed");
        if (!result.CheckpointReloadPassed)
            Failed("runtime.checkpoint_replay", "runtime.checkpoint_replay.failed", artifacts.CheckpointReplay.Diagnostics);
        if (!result.FullReplayEquivalent)
            Failed("runtime.full_replay", "runtime.full_replay.failed", artifacts.FinalReplay.Diagnostics);
        if (!result.ActionBindingsPassed)
            Failed("runtime.action_binding", "runtime.action_binding.failed");
        foreach (var observation in artifacts.SemanticEffects.Observations.Where(item => !item.Passed))
        {
            var detail = "moduleId=" + observation.ModuleId
                         + "; effectId=" + observation.EffectId
                         + "; metricKind=" + observation.MetricKind
                         + "; targetId=" + observation.TargetId
                         + "; expectedValue=" + observation.ExpectedValue
                         + "; actualValue=" + observation.ActualValue;
            Failed("runtime.semantic_effect", "runtime.semantic_effect.failed", new[] { detail }
                .Concat(observation.Diagnostics));
        }
        if (artifacts.SemanticEffects.SatisfiedSelectedModuleCount != selectedModuleCount)
            Failed("runtime.selected_module", "runtime.selected_module_unsatisfied",
            ["satisfied=" + artifacts.SemanticEffects.SatisfiedSelectedModuleCount + "; selected=" + selectedModuleCount]);
        if (!result.PackageDistinctFromGoal142Candidates)
            Failed("composition.distinctness", "composition.package_distinctness.failed");
        return (stage, diagnostics.Distinct(StringComparer.Ordinal).ToList());
    }

    private static bool IsQualificationFailure(string message) =>
        message.Contains("qualification", StringComparison.OrdinalIgnoreCase)
        || message.Contains("checkpoint replay", StringComparison.OrdinalIgnoreCase)
        || message.Contains("full replay", StringComparison.OrdinalIgnoreCase)
        || message.Contains("action binding", StringComparison.OrdinalIgnoreCase);

    private static (string Stage, string Code) ClassifyQualificationFailure(string message)
    {
        if (message.Contains("checkpoint replay", StringComparison.OrdinalIgnoreCase))
            return ("runtime.checkpoint_replay", "runtime.checkpoint_replay.failed");
        if (message.Contains("full replay", StringComparison.OrdinalIgnoreCase))
            return ("runtime.full_replay", "runtime.full_replay.failed");
        if (message.Contains("action binding", StringComparison.OrdinalIgnoreCase))
            return ("runtime.action_binding", "runtime.action_binding.failed");
        return ("composition.qualification", "composition.qualification.failed");
    }

    public static string HashText(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static (string Path, string Sha256) ResolveBaseline(string root)
    {
        var matrixPath = Path.Combine(root, FeatureModuleCompositionVocabulary.Goal142Root.Replace('/', Path.DirectorySeparatorChar),
            ProductLineRuntimeVariantMatrixVocabulary.MatrixResultFileName);
        var matrix = JsonSerializer.Deserialize<ProductLineRuntimeVariantMatrixResult>(File.ReadAllText(matrixPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                     ?? throw new InvalidOperationException("Goal142 matrix could not be read");
        var row = matrix.Candidates.Single(item => item.CandidateId == FeatureModuleCompositionVocabulary.BaselineCandidateId);
        var path = Path.GetFullPath(Path.Combine(root, row.PackagePath.Replace('/', Path.DirectorySeparatorChar)));
        using var stream = File.OpenRead(path);
        var sha = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        if (!string.Equals(sha, row.PackageSha256, StringComparison.Ordinal))
            throw new InvalidOperationException("Goal142 baseline package hash mismatch rejected");
        return (path, sha);
    }

    private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
}
