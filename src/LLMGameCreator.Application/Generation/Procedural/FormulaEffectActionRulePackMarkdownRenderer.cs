namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class FormulaEffectActionRulePackMarkdownRenderer
{
    public string RenderRulePack(FormulaEffectActionRulePack rulePack)
    {
        ArgumentNullException.ThrowIfNull(rulePack);

        var lines = new List<string>
        {
            "# Formula/Effect/Action Rule Pack v1",
            string.Empty,
            "This deterministic rule pack does not call an LLM, provider, Lua, Unity, media generator, or runtime execution.",
            string.Empty,
            "## Metadata",
            string.Empty,
            $"- Rule pack id: `{rulePack.Metadata.RulePackId}`",
            $"- Schema version: `{rulePack.Metadata.SchemaVersion}`",
            $"- Source plan id: `{rulePack.Metadata.SourcePlanId}`",
            $"- Source plan hash: `{rulePack.Metadata.SourcePlanHash}`",
            $"- Deterministic hash: `{rulePack.Metadata.DeterministicHash}`",
            $"- Stable summary: `{rulePack.Metadata.StableSummary}`",
            string.Empty,
            "## Counts",
            string.Empty,
            $"- Formulas: `{rulePack.Formulas.Count}`",
            $"- Requirements: `{rulePack.Requirements.Count}`",
            $"- Effects: `{rulePack.Effects.Count}`",
            $"- Actions: `{rulePack.Actions.Count}`",
            $"- Event rules: `{rulePack.EventRules.Count}`",
            $"- Diagnostics: `{rulePack.Diagnostics.Count}`",
            string.Empty,
            "## Formulas",
            string.Empty
        };

        lines.AddRange(rulePack.Formulas.Select(formula =>
            $"- `{formula.FormulaId}` result=`{formula.ResultType}` variables=`{string.Join(",", formula.DeclaredVariables)}` expression=`{formula.Expression}`"));

        lines.AddRange([string.Empty, "## Requirements", string.Empty]);
        lines.AddRange(rulePack.Requirements.Select(requirement =>
            $"- `{requirement.RequirementId}` type=`{requirement.RequirementType}` formula=`{requirement.FormulaId}` refs=`{FormatRefs(requirement.SourceRefs)}`"));

        lines.AddRange([string.Empty, "## Effects", string.Empty]);
        lines.AddRange(rulePack.Effects.Select(effect =>
            $"- `{effect.EffectId}` type=`{effect.EffectType}` target=`{effect.TargetRef}` formula=`{effect.FormulaId}` refs=`{FormatRefs(effect.SourceRefs)}`"));

        lines.AddRange([string.Empty, "## Actions", string.Empty]);
        lines.AddRange(rulePack.Actions.Select(action =>
            $"- `{action.ActionId}` type=`{action.ActionType}` requirements=`{string.Join(",", action.RequirementIds)}` effects=`{string.Join(",", action.EffectIds)}` refs=`{FormatRefs(action.SourceRefs)}`"));

        lines.AddRange([string.Empty, "## Event Rules", string.Empty]);
        lines.AddRange(rulePack.EventRules.Select(eventRule =>
            $"- `{eventRule.EventRuleId}` trigger=`{eventRule.TriggerId}` requirements=`{string.Join(",", eventRule.RequirementIds)}` actions=`{string.Join(",", eventRule.ActionIds)}` refs=`{FormatRefs(eventRule.SourceRefs)}`"));

        lines.AddRange([string.Empty, "## Diagnostics", string.Empty]);
        lines.AddRange(rulePack.Diagnostics.Count == 0
            ? ["- None"]
            : rulePack.Diagnostics.Select(diagnostic =>
                $"- `{diagnostic.Severity}` `{diagnostic.Code}` target=`{diagnostic.Target}`: {diagnostic.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    public string RenderValidationReport(FormulaEffectActionValidationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var lines = new List<string>
        {
            "# Formula/Effect/Action Validation Report v1",
            string.Empty,
            "## Summary",
            string.Empty,
            $"- Rule pack id: `{report.RulePackId}`",
            $"- Rule pack hash: `{report.RulePackHash}`",
            $"- Diagnostic count: `{report.DiagnosticCount}`",
            $"- Has errors: `{report.HasErrors.ToString().ToLowerInvariant()}`",
            string.Empty,
            "## Diagnostics",
            string.Empty
        };

        lines.AddRange(report.Diagnostics.Count == 0
            ? ["- None"]
            : report.Diagnostics.Select(diagnostic =>
                $"- `{diagnostic.Severity}` `{diagnostic.Code}` target=`{diagnostic.Target}`: {diagnostic.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    private static string FormatRefs(IReadOnlyList<GeneratedPlanReference> refs) =>
        refs.Count == 0
            ? "none"
            : string.Join(",", refs.Select(item => item.Kind + ":" + item.Id));
}
