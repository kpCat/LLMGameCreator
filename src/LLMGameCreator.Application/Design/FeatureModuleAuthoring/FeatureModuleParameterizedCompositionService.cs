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
        var effectiveById = plan.ParameterBinding.EffectiveMutationOperations
            .ToDictionary(operation => operation.OperationId, StringComparer.Ordinal);
        var effectiveCatalog = library.Catalog with
        {
            Modules = library.Catalog.Modules.Select(module => document.SelectedModuleIds.Contains(module.ModuleId, StringComparer.Ordinal)
                ? module with
                {
                    MutationOperations = module.MutationOperations.Select(operation => effectiveById[operation.OperationId]).ToList()
                }
                : module).ToList()
        };
        var qualification = _compositionService.ComposeAndQualify(
            root,
            effectiveCatalog,
            document.SelectedModuleIds,
            Path.GetFullPath(outputRoot),
            document.CompositionId,
            useCapabilityDrivenRuntimePlaythrough);
        var semantic = qualification.Artifacts.SemanticEffects;
        var passed = qualification.Result.Passed
                     && semantic.SatisfiedSelectedModuleCount == document.SelectedModuleIds.Count
                     && plan.ParameterBinding.Passed;
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
            Passed = passed
        };
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
