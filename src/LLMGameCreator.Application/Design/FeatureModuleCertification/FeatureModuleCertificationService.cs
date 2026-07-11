using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.FeatureModuleCertification;

public sealed class FeatureModuleCertificationService
{
    private readonly FeatureModuleCompositionService _compositionService;
    private readonly FeatureModuleParameterValidator _parameters = new();
    private readonly FeatureModuleCertificationPlanner _planner = new();
    private readonly FeatureModuleCertificationCache _cache;
    private readonly IFeatureModuleAuthoringClock _clock;

    public FeatureModuleCertificationService(
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        FeatureModuleCertificationCache cache,
        IFeatureModuleAuthoringClock? clock = null)
    {
        _compositionService = new FeatureModuleCompositionService(runtime ?? throw new ArgumentNullException(nameof(runtime)));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _clock = clock ?? new SystemFeatureModuleAuthoringClock();
    }

    public FeatureModuleCertificationLedger Certify(
        string repositoryRoot,
        FeatureModuleLibrarySnapshot library,
        string basePackageSha256,
        string executionRoot,
        string runtimeQualifierContractVersion = FeatureModuleCertificationVocabulary.RuntimeQualifierContractVersion)
    {
        var actionSignature = string.Join("|", ProductLineRuntimeQualifier.CanonicalActionPlan);
        var plan = _planner.Plan(library, basePackageSha256, runtimeQualifierContractVersion, actionSignature);
        var entries = new List<FeatureModuleCertificationEntry>();
        var executed = 0;
        var reused = 0;
        var invalidated = 0;
        var corrupt = false;
        foreach (var item in plan.Modules)
        {
            var state = _cache.TryRead(item, out var cached);
            if (state == FeatureModuleCertificationCacheReadState.Reused)
            {
                entries.Add(cached!);
                reused++;
                continue;
            }
            if (state is FeatureModuleCertificationCacheReadState.Invalidated or FeatureModuleCertificationCacheReadState.Corrupt)
                invalidated++;
            if (state == FeatureModuleCertificationCacheReadState.Corrupt) corrupt = true;
            var output = Path.Combine(Path.GetFullPath(executionRoot), FeatureModuleLibraryFingerprintService.Hash(item.ModuleId));
            var qualification = _compositionService.ComposeAndQualify(
                repositoryRoot, library.Catalog, item.CertificationSelectedModuleIds, output,
                FeatureModuleCompositionIdentity.CompositionId(library.Catalog, item.CertificationSelectedModuleIds));
            var parameterValidation = _parameters.Validate(library.Catalog, item.CertificationSelectedModuleIds, []);
            var result = qualification.Result;
            var target = library.Catalog.Modules.Single(module => module.ModuleId == item.ModuleId);
            var targetObservations = result.SemanticEffects.Observations
                .Where(observation => observation.ModuleId == item.ModuleId)
                .ToList();
            var targetRuntimeEffectsPassed = target.RuntimeEffectContracts.Count > 0
                                             && targetObservations.Count == target.RuntimeEffectContracts.Count
                                             && targetObservations.All(observation => observation.Passed);
            var entry = new FeatureModuleCertificationEntry
            {
                ModuleId = item.ModuleId,
                CertificationSelectedModuleIds = item.CertificationSelectedModuleIds,
                OptionalDependencyClosureIds = item.OptionalDependencyClosureIds,
                DependencyClosureFingerprint = item.DependencyClosureFingerprint,
                ModuleFingerprint = item.ModuleFingerprint,
                DependencyFingerprint = item.DependencyFingerprint,
                BasePackageSha256 = item.BasePackageSha256,
                RuntimeQualifierContractVersion = item.RuntimeQualifierContractVersion,
                ActionPlanSignature = item.ActionPlanSignature,
                ParameterDefaultsFingerprint = item.ParameterDefaultsFingerprint,
                Status = result.Passed && parameterValidation.Passed && targetRuntimeEffectsPassed ? "GREEN" : "FAILED",
                StructuralValidationPassed = library.Validation.Passed,
                DefaultParameterValidationPassed = parameterValidation.Passed,
                MaterializationPassed = result.MutationAuditPassed,
                PackageValidationPassed = result.PackageValidationPassed,
                RuntimeQualificationPassed = result.Passed,
                RuntimeEffectsPassed = targetRuntimeEffectsPassed,
                ClosureRuntimeEffectsPassed = result.SemanticEffects.Passed,
                TargetRuntimeEffectsPassed = targetRuntimeEffectsPassed,
                CheckpointReloadPassed = result.CheckpointReloadPassed,
                FullReplayEquivalent = result.FullReplayEquivalent,
                ActionBindingPassed = result.ActionBindingsPassed,
                CertifiedAtUtc = _clock.UtcNow.ToUniversalTime(),
                Diagnostics = result.Diagnostics
            };
            _cache.Write(item, entry);
            entries.Add(entry);
            executed++;
        }
        var certified = entries.Count(entry => entry.Status == "GREEN");
        return new FeatureModuleCertificationLedger
        {
            Status = certified == plan.ModuleCount ? "GREEN" : "FAILED",
            PlannedModuleCount = plan.ModuleCount,
            CertifiedModuleCount = certified,
            ExecutedCount = executed,
            ReusedCount = reused,
            InvalidatedCount = invalidated,
            CorruptCacheRejected = corrupt,
            Entries = entries.OrderBy(entry => entry.ModuleId, StringComparer.Ordinal).ToList()
        };
    }
}
