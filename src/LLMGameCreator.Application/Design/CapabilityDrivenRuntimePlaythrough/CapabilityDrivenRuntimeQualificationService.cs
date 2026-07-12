using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;

public sealed class CapabilityDrivenRuntimeQualificationService
{
    private readonly CapabilityDrivenRuntimePlaythroughPlanner _planner;
    private readonly ProductLineRuntimeQualifier _qualifier;

    public CapabilityDrivenRuntimeQualificationService(
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        CapabilityDrivenRuntimePlaythroughPlanner? planner = null)
    {
        _planner = planner ?? new CapabilityDrivenRuntimePlaythroughPlanner();
        _qualifier = new ProductLineRuntimeQualifier(runtime ?? throw new ArgumentNullException(nameof(runtime)));
    }

    public ProductLineRuntimeQualificationResult Qualify(
        IReadOnlyList<FeatureModuleDefinition> selectedModules,
        GamePackageDefinition package,
        ProductLineRuntimeQualificationRequest request)
    {
        var plan = _planner.Plan(selectedModules, package);
        return _qualifier.Qualify(package, request with { CapabilityPlan = plan });
    }
}
