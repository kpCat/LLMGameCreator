using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed partial class FormulaEffectActionRulePackValidator
{
    private const int MaxFormulaLength = 160;

    private static readonly IReadOnlySet<string> AllowedRequirementTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        FormulaEffectActionRulePackConstants.RequirementOpenRoute,
        FormulaEffectActionRulePackConstants.RequirementFactionAccess
    };

    private static readonly IReadOnlySet<string> AllowedEffectTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "effect/set_flag",
        "effect/grant_item",
        "effect/adjust_reputation",
        "effect/advance_quest_event"
    };

    private static readonly IReadOnlySet<string> AllowedActionTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        FormulaEffectActionRulePackConstants.ActionResolveEncounter,
        "action/grant_quest_progress"
    };

    private static readonly IReadOnlySet<string> AllowedEventRuleTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "event_rule/on_enter_region",
        "event_rule/on_resolve_encounter",
        "event_rule/on_complete_quest_event"
    };

    public IReadOnlyList<FormulaEffectActionDiagnostic> Validate(
        FormulaEffectActionRulePack rulePack,
        ProceduralGeneratedGamePlan? sourcePlan = null)
    {
        ArgumentNullException.ThrowIfNull(rulePack);

        var diagnostics = new List<FormulaEffectActionDiagnostic>();
        var formulaIds = rulePack.Formulas.Select(item => item.FormulaId).ToHashSet(StringComparer.Ordinal);
        var requirementIds = rulePack.Requirements.Select(item => item.RequirementId).ToHashSet(StringComparer.Ordinal);
        var effectIds = rulePack.Effects.Select(item => item.EffectId).ToHashSet(StringComparer.Ordinal);
        var actionIds = rulePack.Actions.Select(item => item.ActionId).ToHashSet(StringComparer.Ordinal);

        CheckDuplicateIds(rulePack, diagnostics);

        foreach (var formula in rulePack.Formulas)
        {
            ValidateId(formula.FormulaId, "formula", diagnostics);
            ValidateFormula(formula, diagnostics);
        }

        foreach (var requirement in rulePack.Requirements)
        {
            ValidateId(requirement.RequirementId, "requirement", diagnostics);
            ValidateAllowedType(requirement.RequirementType, AllowedRequirementTypes, requirement.RequirementId, "requirement.type", diagnostics);
            ValidateKnownRef(requirement.FormulaId, formulaIds, requirement.RequirementId, "rule_pack.unknown_formula_ref", diagnostics);
            ValidateSourceRefs(requirement.SourceRefs, sourcePlan, requirement.RequirementId, diagnostics);
        }

        foreach (var effect in rulePack.Effects)
        {
            ValidateId(effect.EffectId, "effect", diagnostics);
            ValidateAllowedType(effect.EffectType, AllowedEffectTypes, effect.EffectId, "effect.type", diagnostics);
            if (!string.IsNullOrWhiteSpace(effect.FormulaId))
            {
                ValidateKnownRef(effect.FormulaId, formulaIds, effect.EffectId, "rule_pack.unknown_formula_ref", diagnostics);
            }

            ValidateSourceRefs(effect.SourceRefs, sourcePlan, effect.EffectId, diagnostics);
        }

        foreach (var action in rulePack.Actions)
        {
            ValidateId(action.ActionId, "action", diagnostics);
            ValidateAllowedType(action.ActionType, AllowedActionTypes, action.ActionId, "action.type", diagnostics);
            if (action.EffectIds.Count == 0)
            {
                Add(diagnostics, "error", "rule_pack.empty_action_effects", action.ActionId, "Action must reference at least one effect.");
            }

            foreach (var requirementId in action.RequirementIds)
            {
                ValidateKnownRef(requirementId, requirementIds, action.ActionId, "rule_pack.unknown_requirement_ref", diagnostics);
            }

            foreach (var effectId in action.EffectIds)
            {
                ValidateKnownRef(effectId, effectIds, action.ActionId, "rule_pack.unknown_effect_ref", diagnostics);
            }

            ValidateSourceRefs(action.SourceRefs, sourcePlan, action.ActionId, diagnostics);
        }

        foreach (var eventRule in rulePack.EventRules)
        {
            ValidateId(eventRule.EventRuleId, "eventRule", diagnostics);
            ValidateAllowedType(eventRule.EventRuleType, AllowedEventRuleTypes, eventRule.EventRuleId, "event_rule.type", diagnostics);
            if (string.IsNullOrWhiteSpace(eventRule.TriggerId))
            {
                Add(diagnostics, "error", "rule_pack.empty_event_trigger", eventRule.EventRuleId, "Event rule trigger id is required.");
            }
            else
            {
                ValidateId(eventRule.TriggerId, "eventTrigger", diagnostics);
            }

            foreach (var requirementId in eventRule.RequirementIds)
            {
                ValidateKnownRef(requirementId, requirementIds, eventRule.EventRuleId, "rule_pack.unknown_requirement_ref", diagnostics);
            }

            foreach (var actionId in eventRule.ActionIds)
            {
                ValidateKnownRef(actionId, actionIds, eventRule.EventRuleId, "rule_pack.unknown_action_ref", diagnostics);
            }

            ValidateSourceRefs(eventRule.SourceRefs, sourcePlan, eventRule.EventRuleId, diagnostics);
        }

        return SortDiagnostics(diagnostics);
    }

    private static void CheckDuplicateIds(FormulaEffectActionRulePack rulePack, ICollection<FormulaEffectActionDiagnostic> diagnostics)
    {
        var ids = rulePack.Formulas.Select(item => ("formula", item.FormulaId))
            .Concat(rulePack.Requirements.Select(item => ("requirement", item.RequirementId)))
            .Concat(rulePack.Effects.Select(item => ("effect", item.EffectId)))
            .Concat(rulePack.Actions.Select(item => ("action", item.ActionId)))
            .Concat(rulePack.EventRules.Select(item => ("eventRule", item.EventRuleId)))
            .Where(item => !string.IsNullOrWhiteSpace(item.Item2))
            .ToList();

        foreach (var group in ids.GroupBy(item => item.Item2, StringComparer.Ordinal).Where(group => group.Count() > 1))
        {
            Add(diagnostics, "error", "rule_pack.duplicate_id", group.Key, $"Duplicate rule id '{group.Key}' was found.");
        }
    }

    private static void ValidateFormula(FormulaDefinition formula, ICollection<FormulaEffectActionDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(formula.Expression))
        {
            Add(diagnostics, "error", "formula.expression.empty", formula.FormulaId, "Formula expression is required.");
            return;
        }

        if (formula.Expression.Length > MaxFormulaLength)
        {
            Add(diagnostics, "error", "formula.expression.too_long", formula.FormulaId, $"Formula expression must not exceed {MaxFormulaLength} characters.");
        }

        if (LooksUnsafeFormula(formula.Expression))
        {
            Add(diagnostics, "error", "formula.expression.unsafe", formula.FormulaId, "Formula expression contains unsafe or code-looking text.");
            return;
        }

        var variables = formula.DeclaredVariables.ToHashSet(StringComparer.Ordinal);
        foreach (var variable in variables)
        {
            ValidateVariableName(variable, formula.FormulaId, diagnostics);
        }

        foreach (var token in FormulaIdentifierRegex().Matches(formula.Expression).Select(match => match.Value).Distinct(StringComparer.Ordinal))
        {
            if (!variables.Contains(token))
            {
                Add(diagnostics, "error", "formula.expression.unknown_variable", formula.FormulaId, $"Formula expression references undeclared variable '{token}'.");
            }
        }
    }

    private static bool LooksUnsafeFormula(string expression)
    {
        if (expression.Any(character => !(char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) || character is '_' or '+' or '-' or '*' or '/' or '(' or ')' or '.')))
        {
            return true;
        }

        if (expression.Contains("../", StringComparison.Ordinal) ||
            expression.Contains("..\\", StringComparison.Ordinal) ||
            expression.Contains("//", StringComparison.Ordinal) ||
            expression.Contains("/*", StringComparison.Ordinal) ||
            expression.Contains("*/", StringComparison.Ordinal))
        {
            return true;
        }

        var codeLookingTokens = new[]
        {
            "using", "namespace", "class", "new", "return", "system", "reflection", "process", "file", "directory", "lua", "load"
        };

        return FormulaIdentifierRegex().Matches(expression)
            .Select(match => match.Value)
            .Any(token => codeLookingTokens.Contains(token, StringComparer.OrdinalIgnoreCase));
    }

    private static void ValidateVariableName(string variable, string formulaId, ICollection<FormulaEffectActionDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(variable) || !VariableNameRegex().IsMatch(variable))
        {
            Add(diagnostics, "error", "formula.variable.unsafe", formulaId, $"Formula variable '{variable}' is unsafe.");
        }
    }

    private static void ValidateKnownRef(
        string id,
        IReadOnlySet<string> knownIds,
        string target,
        string code,
        ICollection<FormulaEffectActionDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(id) || !knownIds.Contains(id))
        {
            Add(diagnostics, "error", code, target, $"Referenced id '{id}' was not found.");
        }
    }

    private static void ValidateAllowedType(
        string type,
        IReadOnlySet<string> allowedTypes,
        string target,
        string category,
        ICollection<FormulaEffectActionDiagnostic> diagnostics)
    {
        if (!allowedTypes.Contains(type))
        {
            Add(diagnostics, "error", "rule_pack.unsupported_" + category, target, $"Type '{type}' is not supported by this rule-pack foundation.");
        }
    }

    private static void ValidateSourceRefs(
        IReadOnlyList<GeneratedPlanReference> refs,
        ProceduralGeneratedGamePlan? sourcePlan,
        string target,
        ICollection<FormulaEffectActionDiagnostic> diagnostics)
    {
        if (sourcePlan is null)
        {
            return;
        }

        foreach (var reference in refs)
        {
            if (!SourcePlanContains(sourcePlan, reference.Kind, reference.Id))
            {
                Add(diagnostics, "warning", "rule_pack.missing_source_plan_ref", target, $"Source plan reference '{reference.Kind}:{reference.Id}' was not found.");
            }
        }
    }

    private static bool SourcePlanContains(ProceduralGeneratedGamePlan sourcePlan, string kind, string id) => kind switch
    {
        "region" => sourcePlan.World.Regions.Any(item => item.RegionId == id),
        "connection" => sourcePlan.World.Connections.Any(item => item.ConnectionId == id),
        "faction" => sourcePlan.Factions.Any(item => item.FactionId == id),
        "actor" => sourcePlan.ActorSeeds.Any(item => item.ActorSeedId == id),
        "item" => sourcePlan.ItemResourceSeeds.Any(item => item.ItemSeedId == id),
        "encounter" => sourcePlan.EncounterSeeds.Any(item => item.EncounterSeedId == id),
        "quest_event" => sourcePlan.QuestEventSeeds.Any(item => item.QuestEventSeedId == id),
        _ => false
    };

    private static void ValidateId(string id, string category, ICollection<FormulaEffectActionDiagnostic> diagnostics)
    {
        if (!IsSafeId(id))
        {
            Add(diagnostics, "error", "rule_pack.unsafe_id", id, $"{category} id is empty, too long or contains unsafe characters.");
        }
    }

    private static bool IsSafeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > 160 || id.Contains('\\') || id.Contains(':'))
        {
            return false;
        }

        if (id.StartsWith('/') || id.EndsWith('/'))
        {
            return false;
        }

        var segments = id.Split('/', StringSplitOptions.None);
        return segments.All(segment => !string.IsNullOrWhiteSpace(segment) && segment is not "." and not "..") &&
               id.All(character => character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-' or '/');
    }

    private static IReadOnlyList<FormulaEffectActionDiagnostic> SortDiagnostics(IEnumerable<FormulaEffectActionDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static void Add(
        ICollection<FormulaEffectActionDiagnostic> diagnostics,
        string severity,
        string code,
        string target,
        string message)
    {
        diagnostics.Add(new FormulaEffectActionDiagnostic
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        });
    }

    [GeneratedRegex("[A-Za-z_][A-Za-z0-9_]*", RegexOptions.CultureInvariant)]
    private static partial Regex FormulaIdentifierRegex();

    [GeneratedRegex("^[a-z][a-z0-9_]{0,63}$", RegexOptions.CultureInvariant)]
    private static partial Regex VariableNameRegex();
}
