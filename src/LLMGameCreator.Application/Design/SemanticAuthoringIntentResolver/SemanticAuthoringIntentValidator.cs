using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.DynamicSemanticFeatures;

namespace LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;

public static partial class SemanticAuthoringIntentValidator
{
    private static readonly string[] BoundaryLeakageNeedles =
    [
        "runtime",
        "winforms",
        "ui",
        "unity",
        "provider",
        "llm",
        "rag",
        "lua",
        "media",
        "gamepackage"
    ];

    private static readonly string[] FinalProseLeakageNeedles =
    [
        "final dialogue:",
        "dialogue line:",
        "\"hello",
        "quest text:",
        "final prose"
    ];

    public static IReadOnlyList<SemanticAuthoringDiagnostic> ValidateWorkspace(SemanticAuthoringWorkspace workspace)
    {
        var diagnostics = new List<SemanticAuthoringDiagnostic>();
        var definitions = DynamicSemanticFeatureCatalog.BuildDefaultFeatureDefinitions()
            .ToDictionary(item => item.FeatureId, StringComparer.Ordinal);
        var fields = workspace.DomainGroups.SelectMany(group => group.Sections).SelectMany(section => section.Fields).ToList();

        foreach (var duplicate in fields.GroupBy(item => item.FieldId, StringComparer.Ordinal).Where(item => item.Count() > 1).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_authoring.field_id.duplicate", duplicate.Key, "Workspace field ids must be unique."));
        }

        foreach (var group in workspace.DomainGroups.OrderBy(item => item.DomainId, StringComparer.Ordinal))
        {
            if (!SemanticAuthoringIntentVocabulary.DomainGroups.Contains(group.DomainId, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.domain.unknown", group.DomainId, "Workspace domain group is unknown."));
            }
        }

        foreach (var field in fields.OrderBy(item => item.FieldId, StringComparer.Ordinal))
        {
            ValidateField(field, definitions, diagnostics);
        }

        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<SemanticAuthoringDiagnostic> ValidateIntentResolution(SemanticAuthoringIntentResolution resolution)
    {
        var diagnostics = new List<SemanticAuthoringDiagnostic>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var orderedIds = resolution.Intents.Select(item => item.IntentId).ToList();
        if (!orderedIds.SequenceEqual(orderedIds.Order(StringComparer.Ordinal)))
        {
            diagnostics.Add(Diagnostic("error", "semantic_authoring.order.nondeterministic", resolution.ScenarioId, "Intent records must be written in stable id order."));
        }

        foreach (var intent in resolution.Intents.OrderBy(item => item.IntentId, StringComparer.Ordinal))
        {
            if (!ids.Add(intent.IntentId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.intent_id.duplicate", intent.IntentId, "Intent ids must be unique."));
            }

            if (string.IsNullOrWhiteSpace(intent.IntentId) || !StableIdPattern().IsMatch(intent.IntentId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.intent_id.invalid", intent.IntentId, "Intent id must be stable."));
            }

            if (!SemanticAuthoringIntentVocabulary.IntentFamilies.Contains(intent.IntentFamily))
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.intent_family.unknown", intent.IntentId, "Intent family is unknown."));
            }

            if (string.IsNullOrWhiteSpace(intent.TargetId) || intent.TargetId.StartsWith("fake/", StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.intent_target.unknown", intent.IntentId, "Intent target must be a known authoring target."));
            }

            if (!SemanticAuthoringIntentVocabulary.DomainGroups.Contains(intent.TargetDomain, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.domain.unknown", intent.IntentId, "Intent target domain is unknown."));
            }

            if (intent.SourceFeatureIds.Count == 0)
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.intent_trace.missing", intent.IntentId, "Intent must retain at least one source feature trace."));
            }

            if (ContainsFinalProseLeakage(intent.TemplateHint, intent.LocalizationKeyHint, intent.ResolvedFeatureValueSummary))
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.final_prose.leakage", intent.IntentId, "Intent planning must not contain final dialogue or prose."));
            }

            if (ContainsBoundaryLeakage(intent.TemplateHint, intent.LocalizationKeyHint, intent.ProvenanceSummary, intent.TraceSummary))
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.boundary.leakage", intent.IntentId, "Intent planning must not imply Runtime/UI/Unity/provider/LLM/RAG/Lua/media/GamePackage materialization."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<SemanticAuthoringDiagnostic> ValidateManualMatrix(ManualVsAutoAuthoringMatrix matrix)
    {
        var diagnostics = new List<SemanticAuthoringDiagnostic>();
        foreach (var row in matrix.Rows.OrderBy(item => item.CaseId, StringComparer.Ordinal))
        {
            if (!SemanticAuthoringIntentVocabulary.ProvenanceKinds.Contains(row.Provenance))
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.provenance.unknown", row.CaseId, "Unknown provenance classification."));
            }

            if (row.Provenance is "llm_candidate" or "imported_candidate" && row.AcceptedAutomatically)
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.candidate.not_accepted", row.CaseId, "Candidate values must require review and cannot be accepted automatically."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    public static SemanticAuthoringIntentInvalidMatrix BuildInvalidMatrix()
    {
        var validWorkspace = SemanticAuthoringIntentCatalog.BuildDefaultWorkspaces().Single(item => item.ScenarioId == "frontier_survival");
        var validResolution = new FeatureDrivenIntentResolver().ResolveScenario(DynamicSemanticFeatureCatalog.FrontierScenario());
        var cases = new List<SemanticAuthoringIntentInvalidScenario>
        {
            Invalid("duplicate_workspace_field_id", "duplicate workspace field id", ValidateWorkspace(MutateFirstField(validWorkspace, field => field, duplicateFirst: true))),
            Invalid("unknown_feature_reference", "unknown feature reference", ValidateWorkspace(MutateFirstField(validWorkspace, field => field with { FeatureId = "fake.feature" }))),
            Invalid("unknown_target_domain", "unknown target/domain", ValidateWorkspace(MutateFirstField(validWorkspace, field => field with { DomainId = "fake_domain" }))),
            Invalid("illegal_feature_domain_applicability", "illegal feature/domain applicability", ValidateWorkspace(MutateFirstField(validWorkspace, field => field with { DomainId = "combat", FeatureId = "npc.mood" }))),
            Invalid("required_manual_field_missing", "required manual field missing", ValidateWorkspace(MutateFirstField(validWorkspace, field => field with { RequirementStatus = "required", CompletionStatus = "missing_required", Provenance = "unset" }))),
            Valid("optional_absent_field_valid", "optional absent field valid and traceable", ValidateWorkspace(MutateFirstField(validWorkspace, field => field with { RequirementStatus = "optional", CompletionStatus = "optional_absent", Provenance = "unset" }))),
            Invalid("conflicting_provenance_for_same_field", "conflicting provenance for same field", ValidateWorkspace(MutateFirstField(validWorkspace, field => field with { Provenance = "user|llm_candidate" }))),
            Invalid("llm_candidate_treated_as_accepted", "LLM candidate accepted automatically", ValidateManualMatrix(new ManualVsAutoAuthoringMatrix { Rows = [new ManualVsAutoAuthoringMatrixRow { CaseId = "bad_llm_candidate", Provenance = "llm_candidate", ExpectedStatus = "accepted", AcceptedAutomatically = true }] })),
            Invalid("imported_candidate_treated_as_accepted", "imported candidate accepted automatically", ValidateManualMatrix(new ManualVsAutoAuthoringMatrix { Rows = [new ManualVsAutoAuthoringMatrixRow { CaseId = "bad_imported_candidate", Provenance = "imported_candidate", ExpectedStatus = "accepted", AcceptedAutomatically = true }] })),
            Invalid("final_dialogue_prose_leakage", "final dialogue/prose leakage", ValidateIntentResolution(MutateFirstIntent(validResolution, intent => intent with { TemplateHint = "final dialogue: hello traveler" }))),
            Invalid("final_gamepackage_materialization_leakage", "final GamePackage materialization leakage", ValidateIntentResolution(MutateFirstIntent(validResolution, intent => intent with { TraceSummary = "materialize GamePackage definition" }))),
            Invalid("runtime_ui_unity_provider_llm_rag_lua_media_boundary_leakage", "runtime/UI/Unity/provider/LLM/RAG/Lua/media boundary leakage", ValidateIntentResolution(MutateFirstIntent(validResolution, intent => intent with { ProvenanceSummary = "call LLM provider, run Lua, update Unity UI media" }))),
            Invalid("fake_intent_target_accepted", "fake intent target accepted", ValidateIntentResolution(MutateFirstIntent(validResolution, intent => intent with { TargetId = "fake/target" }))),
            Invalid("missing_source_feature_trace", "missing source feature trace", ValidateIntentResolution(MutateFirstIntent(validResolution, intent => intent with { SourceFeatureIds = [] }))),
            Invalid("nondeterministic_ordering_mutation", "nondeterministic ordering mutation", ValidateIntentResolution(validResolution with { Intents = validResolution.Intents.Reverse().ToList() }))
        };

        return new SemanticAuthoringIntentInvalidMatrix
        {
            ScenarioCount = cases.Count,
            MatchedExpectationCount = cases.Count(item => item.ExpectedValid == item.ActualValid),
            RejectedCount = cases.Count(item => !item.ActualValid),
            Passed = cases.All(item => item.ExpectedValid == item.ActualValid),
            Scenarios = cases.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static SemanticAuthoringDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    public static IReadOnlyList<SemanticAuthoringDiagnostic> SortDiagnostics(IEnumerable<SemanticAuthoringDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static void ValidateField(
        SemanticAuthoringField field,
        IReadOnlyDictionary<string, DynamicSemanticFeatureDefinition> definitions,
        ICollection<SemanticAuthoringDiagnostic> diagnostics)
    {
        if (!SemanticAuthoringIntentVocabulary.DomainGroups.Contains(field.DomainId, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_authoring.domain.unknown", field.FieldId, "Field domain is unknown."));
        }

        if (!SemanticAuthoringIntentVocabulary.ProvenanceKinds.Contains(field.Provenance))
        {
            diagnostics.Add(Diagnostic("error", "semantic_authoring.provenance.unknown", field.FieldId, "Field provenance is unknown or conflicting."));
        }

        if (!definitions.TryGetValue(field.FeatureId, out var definition))
        {
            if (!field.FeatureId.EndsWith(".optional_absence", StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "semantic_authoring.feature_ref.unknown", field.FieldId, "Field references an unknown Goal 032 feature."));
            }
        }
        else if (NormalizeDomain(definition.TargetScope) != field.DomainId
                 && !field.InheritanceHint.StartsWith("inherited_from:", StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_authoring.feature_domain.illegal", field.FieldId, "Feature target scope does not match the authoring domain group."));
        }

        if (field.RequirementStatus == "required" && field.CompletionStatus is "missing_required" or "optional_absent")
        {
            diagnostics.Add(Diagnostic("error", "semantic_authoring.required_field.missing", field.FieldId, "Required field is missing."));
        }

        if (field.Provenance is "llm_candidate" or "imported_candidate" && field.CompletionStatus == "complete")
        {
            diagnostics.Add(Diagnostic("error", "semantic_authoring.candidate.not_accepted", field.FieldId, "Candidate fields must remain review-required before acceptance."));
        }

        if (field.CompletionStatus == "blocked")
        {
            diagnostics.Add(Diagnostic("error", "semantic_authoring.field.blocked", field.FieldId, "Field is blocked or invalid."));
        }

        if (ContainsBoundaryLeakage(field.ControlHint, field.ApplicabilityHint, field.InheritanceHint, field.ResolvedValueSummary))
        {
            diagnostics.Add(Diagnostic("error", "semantic_authoring.boundary.leakage", field.FieldId, "Workspace fields must not imply forbidden boundary work."));
        }
    }

    private static string NormalizeDomain(string scope) =>
        scope switch
        {
            "resource" or "item" => "economy",
            "biome" => "region",
            "magic" or "relationship" => "world",
            _ => scope
        };

    private static bool ContainsBoundaryLeakage(params string[] values)
    {
        var text = string.Join(" ", values).ToLowerInvariant();
        return BoundaryLeakageNeedles.Any(text.Contains);
    }

    private static bool ContainsFinalProseLeakage(params string[] values)
    {
        var text = string.Join(" ", values).ToLowerInvariant();
        return FinalProseLeakageNeedles.Any(text.Contains);
    }

    private static SemanticAuthoringWorkspace MutateFirstField(
        SemanticAuthoringWorkspace workspace,
        Func<SemanticAuthoringField, SemanticAuthoringField> mutate,
        bool duplicateFirst = false)
    {
        var changed = false;
        var groups = workspace.DomainGroups.Select(group => group with
        {
            Sections = group.Sections.Select(section =>
            {
                if (changed || section.Fields.Count == 0)
                {
                    return section;
                }

                changed = true;
                var fields = section.Fields.Select((field, index) => index == 0 ? mutate(field) : field).ToList();
                if (duplicateFirst)
                {
                    fields.Add(fields[0]);
                }

                return section with { Fields = fields };
            }).ToList()
        }).ToList();

        return workspace with { DomainGroups = groups };
    }

    private static SemanticAuthoringIntentResolution MutateFirstIntent(
        SemanticAuthoringIntentResolution resolution,
        Func<SemanticContentIntentRecord, SemanticContentIntentRecord> mutate)
    {
        var intents = resolution.Intents.Select((intent, index) => index == 0 ? mutate(intent) : intent).ToList();
        return resolution with { Intents = intents };
    }

    private static SemanticAuthoringIntentInvalidScenario Invalid(string id, string kind, IReadOnlyList<SemanticAuthoringDiagnostic> diagnostics)
    {
        var sorted = SortDiagnostics(diagnostics);
        return new SemanticAuthoringIntentInvalidScenario
        {
            ScenarioId = id,
            MutatedEvidenceKind = kind,
            ExpectedValid = false,
            ActualValid = sorted.All(item => item.Severity != "error"),
            Diagnostics = sorted
        };
    }

    private static SemanticAuthoringIntentInvalidScenario Valid(string id, string kind, IReadOnlyList<SemanticAuthoringDiagnostic> diagnostics)
    {
        var sorted = SortDiagnostics(diagnostics);
        return new SemanticAuthoringIntentInvalidScenario
        {
            ScenarioId = id,
            MutatedEvidenceKind = kind,
            ExpectedValid = true,
            ActualValid = sorted.All(item => item.Severity != "error"),
            Diagnostics = sorted
        };
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    [GeneratedRegex("^[a-z0-9][a-z0-9_./:-]*[a-z0-9]$")]
    private static partial Regex StableIdPattern();
}
