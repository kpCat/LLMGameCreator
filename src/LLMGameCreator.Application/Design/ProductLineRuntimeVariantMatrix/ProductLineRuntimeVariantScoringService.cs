using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;

public sealed class ProductLineRuntimeVariantScoringService
{
    public ProductLineRuntimeVariantScore Score(
        ProductLineRuntimeVariantRecipe recipe,
        ProductLineRuntimeVariantMutationAudit mutationAudit,
        ProductLineRuntimeVariantPackageValidation packageValidation,
        ProductLineRuntimeVariantRuntimeOutcomeSummary runtimeOutcome)
    {
        var weights = recipe.SelectionWeights;
        var components = new List<ProductLineRuntimeVariantScoreComponent>
        {
            Component(
                "packageValidation",
                packageValidation.Passed,
                weights.PackageValidation,
                "candidate package exists, parses, validates, matches metadata, preserves anchors and stays under Goal142 root"),
            Component(
                "roundtripSemanticCorrectness",
                runtimeOutcome.RoundtripSemanticProofPassed,
                weights.RoundtripSemanticCorrectness,
                "corrected Goal141A request/response semantics pass over this candidate package"),
            Component(
                "requiredAnchorCoverage",
                packageValidation.RequiredAnchorsPresent,
                weights.RequiredAnchorCoverage,
                "canonical vertical-slice anchors remain present"),
            Component(
                "mutationAudit",
                mutationAudit.Passed,
                weights.MutationAudit,
                mutationAudit.OperationCount == 0
                    ? "baseline has no semantic mutations and serves only as comparison"
                    : "all mutation targets had expected old values and verified new values"),
            Component(
                "runtimeEffectObserved",
                runtimeOutcome.RuntimeEffectObserved,
                weights.RuntimeEffectObserved,
                "declared runtime effect is visible in runtime snapshots/state summaries"),
            Component(
                "runtimeStateDistinctness",
                runtimeOutcome.RuntimeStateDistinctFromBaseline,
                weights.RuntimeStateDistinctness,
                "final runtime state hash differs from baseline"),
            Component(
                "noBlockingDiagnostics",
                packageValidation.Diagnostics.Count == 0,
                weights.NoBlockingDiagnostics,
                "package validation emitted no blocking diagnostics"),
            Component(
                "profileSpecificObjective",
                ProfileObjectivePassed(recipe, runtimeOutcome),
                weights.ProfileSpecificObjective,
                "variant-specific craft, combat, harvest or transaction objective remained executable")
        };

        var eligible = packageValidation.Passed
                       && mutationAudit.Passed
                       && runtimeOutcome.RoundtripSemanticProofPassed
                       && (recipe.RecipeId == "balanced_baseline" || runtimeOutcome.RuntimeEffectObserved);
        return new ProductLineRuntimeVariantScore
        {
            CandidateId = recipe.CandidateId,
            RecipeId = recipe.RecipeId,
            VariantKind = recipe.VariantKind,
            Score = components.Sum(component => component.Score),
            Eligible = eligible,
            TieBreakPriority = weights.TieBreakPriority,
            ScoreBreakdown = components
        };
    }

    private static ProductLineRuntimeVariantScoreComponent Component(
        string name,
        bool passed,
        int maxScore,
        string explanation) =>
        new()
        {
            Name = name,
            Score = passed ? maxScore : 0,
            MaxScore = maxScore,
            Passed = passed,
            Explanation = explanation
        };

    private static bool ProfileObjectivePassed(
        ProductLineRuntimeVariantRecipe recipe,
        ProductLineRuntimeVariantRuntimeOutcomeSummary runtimeOutcome) =>
        recipe.RecipeId switch
        {
            "balanced_baseline" => runtimeOutcome.RoundtripSemanticProofPassed,
            "alchemy_focus" => runtimeOutcome.CraftRequestPassed
                               && runtimeOutcome.RuntimeEffectObserved,
            "combat_focus" => runtimeOutcome.CombatRequestPassed
                              && runtimeOutcome.RuntimeEffectObserved,
            "exploration_resource_focus" => runtimeOutcome.HarvestRequestPassed
                                            && runtimeOutcome.TransactionRequestPassed
                                            && runtimeOutcome.RuntimeEffectObserved,
            _ => false
        };
}
