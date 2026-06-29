using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.SemanticArtifactContracts;
using LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;

namespace LLMGameCreator.Application.Design.LuaModuleManifestRegistry;

public static partial class LuaModuleManifestRegistryValidator
{
    private static readonly string[] FinalProseNeedles =
    [
        "final prose",
        "final dialogue",
        "dialogue line:",
        "quest text:",
        "\"hello"
    ];

    private static readonly string[] LuaExecutionNeedles =
    [
        "execute_lua",
        "eval_lua",
        "run lua",
        "execute lua",
        "function("
    ];

    private static readonly string[] ProviderNeedles =
    [
        "call provider",
        "call llm",
        "query rag",
        "provider_llm_rag"
    ];

    private static readonly string[] RuntimeUiUnityGamePackageNeedles =
    [
        "runtime_direct_mutation",
        "ui_winforms",
        "unity_direct_call",
        "gamepackage_schema_mutation",
        "mutate runtime",
        "show form",
        "call unity",
        "change schema"
    ];

    public static IReadOnlyList<LuaModuleManifestDiagnostic> ValidateFamilies(IReadOnlyList<LuaModuleFamilyDefinition> families)
    {
        var diagnostics = new List<LuaModuleManifestDiagnostic>();
        foreach (var duplicate in families.GroupBy(item => item.FamilyId, StringComparer.Ordinal).Where(item => item.Count() > 1).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.family_id.duplicate_conflict", duplicate.Key, "Lua module family ids must be unique."));
        }

        foreach (var family in families.OrderBy(item => item.FamilyId, StringComparer.Ordinal))
        {
            if (!StableIdPattern().IsMatch(family.FamilyId))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.family_id.invalid", family.FamilyId, "Family id must be stable."));
            }

            if (family.RequiredSemanticScopes.Count == 0)
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.family.semantic_scope.required_missing", family.FamilyId, "Family must declare required semantic scopes."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<LuaModuleManifestDiagnostic> ValidateHostApiSurface(IReadOnlyList<LuaHostApiGroup> groups)
    {
        var diagnostics = new List<LuaModuleManifestDiagnostic>();
        foreach (var duplicate in groups.GroupBy(item => item.GroupId, StringComparer.Ordinal).Where(item => item.Count() > 1).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.host_api.duplicate", duplicate.Key, "Host API group ids must be unique."));
        }

        foreach (var group in groups.OrderBy(item => item.GroupId, StringComparer.Ordinal))
        {
            if (!StableIdPattern().IsMatch(group.GroupId))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.host_api.invalid", group.GroupId, "Host API group id must be stable."));
            }

            if (!LuaModuleManifestVocabulary.LifecycleStatuses.Contains(group.Status))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.host_api.status.unknown", group.GroupId, "Host API status is unknown."));
            }

            if (!LuaModuleManifestVocabulary.SideEffectClasses.Contains(group.SideEffectClass))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.host_api.side_effect.unknown", group.GroupId, "Host API side-effect class is unknown."));
            }

            if (group.Status is "blocked" && group.AllowedOperationKinds.Count > 0)
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.host_api.blocked_has_allowed_ops", group.GroupId, "Blocked host API groups must not declare allowed operations."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<LuaModuleManifestDiagnostic> ValidateManifests(
        IReadOnlyList<LuaModuleFamilyDefinition> families,
        IReadOnlyList<LuaHostApiGroup> hostApiGroups,
        IReadOnlyList<LuaModuleManifest> manifests)
    {
        var diagnostics = new List<LuaModuleManifestDiagnostic>();
        var familyById = families.GroupBy(item => item.FamilyId, StringComparer.Ordinal).ToDictionary(item => item.Key, item => item.First(), StringComparer.Ordinal);
        var hostApiById = hostApiGroups.GroupBy(item => item.GroupId, StringComparer.Ordinal).ToDictionary(item => item.Key, item => item.First(), StringComparer.Ordinal);
        var deniedHostApiIds = hostApiGroups.Where(item => item.Status is "blocked" or "future_required").Select(item => item.GroupId).ToHashSet(StringComparer.Ordinal);
        var contractIds = SemanticArtifactContractRegistry.BuildDefaultContracts().Select(item => item.ContractId).ToHashSet(StringComparer.Ordinal);
        var intentFamilies = SemanticAuthoringIntentVocabulary.IntentFamilies;
        var moduleIds = manifests.Select(item => item.ModuleId).ToList();

        if (!manifests.Select(item => item.DeterministicOrderingKey).SequenceEqual(manifests.Select(item => item.DeterministicOrderingKey).Order(StringComparer.Ordinal)))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.order.nondeterministic", "manifests", "Manifests must be written in deterministic ordering-key order."));
        }

        foreach (var duplicate in manifests.GroupBy(item => item.ModuleId, StringComparer.Ordinal).Where(item => item.Count() > 1).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.module_id.duplicate", duplicate.Key, "Lua module ids must be unique."));
        }

        foreach (var manifest in manifests.OrderBy(item => item.ModuleId, StringComparer.Ordinal))
        {
            ValidateManifest(manifest, familyById, hostApiById, deniedHostApiIds, contractIds, intentFamilies, moduleIds, diagnostics);
        }

        diagnostics.AddRange(ValidateDependencyCycles(manifests));
        return SortDiagnostics(diagnostics);
    }

    public static LuaModuleManifestInvalidMatrix BuildInvalidMatrix()
    {
        var families = LuaModuleManifestRegistryCatalog.BuildFamilies();
        var policy = LuaModuleManifestRegistryCatalog.BuildHostApiSurfacePolicy();
        var manifests = LuaModuleManifestRegistryCatalog.BuildDefaultManifests();
        var first = manifests.First(item => item.LifecycleStatus == "ready");
        var future = manifests.First(item => item.LifecycleStatus == "future_required");
        var second = manifests.First(item => item.ModuleId != first.ModuleId && item.LifecycleStatus == "ready");
        var firstStandalone = first with { Dependencies = [] };
        var futureStandalone = future with { Dependencies = [] };
        var cases = new List<LuaModuleManifestInvalidScenario>
        {
            Invalid("duplicate_module_id", "duplicate module id", ValidateManifests(families, policy.Groups, [firstStandalone, firstStandalone])),
            Invalid("invalid_module_id", "invalid module id", ValidateManifests(families, policy.Groups, [firstStandalone with { ModuleId = "Invalid Module Id!" }])),
            Invalid("duplicate_family_id_conflict", "duplicate family id conflict", ValidateFamilies([families[0], families[0]])),
            Invalid("unknown_dependency", "unknown dependency", ValidateManifests(families, policy.Groups, [firstStandalone with { Dependencies = ["lua-module/missing/dependency"] }])),
            Invalid("dependency_cycle", "dependency cycle", ValidateManifests(families, policy.Groups, [firstStandalone with { Dependencies = [second.ModuleId] }, second with { Dependencies = [first.ModuleId] }])),
            Invalid("unknown_host_api_group", "unknown host API group", ValidateManifests(families, policy.Groups, [firstStandalone with { AllowedHostApiGroups = ["unknown.host"] }])),
            Invalid("denied_host_api_group_allowed", "denied host API group used as allowed", ValidateManifests(families, policy.Groups, [firstStandalone with { AllowedHostApiGroups = ["filesystem"] }])),
            Invalid("missing_required_semantic_scope", "missing required semantic scope", ValidateManifests(families, policy.Groups, [firstStandalone with { SemanticScopes = [] }])),
            Invalid("unknown_artifact_contract_reference", "unknown artifact contract reference", ValidateManifests(families, policy.Groups, [firstStandalone with { ArtifactContractIds = ["fake_contract_v1"] }])),
            Invalid("unknown_intent_family_reference", "unknown intent family reference", ValidateManifests(families, policy.Groups, [firstStandalone with { IntentFamilies = ["fake_intent_family"] }])),
            Invalid("fake_profile_scenario", "fake profile/scenario", ValidateManifests(families, policy.Groups, [firstStandalone with { ScenarioCompatibility = ["fake_scenario"], ProfileCompatibility = ["fake_profile"] }])),
            Invalid("provenance_mismatch", "provenance mismatch", ValidateManifests(families, policy.Groups, [firstStandalone with { SourceKind = "llm_candidate", ProvenanceId = "accepted/not-quarantined" }])),
            Invalid("quarantined_candidate_marked_ready", "draft/quarantined candidate marked ready without review", ValidateManifests(families, policy.Groups, [firstStandalone with { SourceKind = "llm_candidate", ProvenanceId = "quarantine/goal034/candidate", LifecycleStatus = "ready", PromotionStatus = "quarantined", SelectableAsReady = true }])),
            Invalid("over_budget_module", "over-budget module", ValidateManifests(families, policy.Groups, [firstStandalone with { ResourceBudget = first.ResourceBudget with { MaxOutputRecords = 4096 } }])),
            Invalid("future_required_treated_ready", "future-required module treated as ready", ValidateManifests(families, policy.Groups, [futureStandalone with { SelectableAsReady = true }])),
            Invalid("side_effect_class_mismatch", "side-effect class mismatch", ValidateManifests(families, policy.Groups, [firstStandalone with { SideEffectClass = "read_only", AllowedHostApiGroups = ["quest.plan"] }])),
            Invalid("final_prose_content", "final prose content", ValidateManifests(families, policy.Groups, [firstStandalone with { ContainsFinalProse = true, Summary = "final prose dialogue line: hello" }])),
            Invalid("lua_source_execution_claim", "Lua source/execution claim", ValidateManifests(families, policy.Groups, [firstStandalone with { DeclaresLuaSource = true, ClaimsLuaExecution = true, Summary = "execute_lua function()" }])),
            Invalid("provider_llm_rag_leak", "provider/LLM/RAG leak", ValidateManifests(families, policy.Groups, [firstStandalone with { DeclaresProviderLlmRagAccess = true, Summary = "call LLM provider and query RAG" }])),
            Invalid("runtime_ui_unity_gamepackage_leak", "Runtime/UI/Unity/GamePackage schema leak", ValidateManifests(families, policy.Groups, [firstStandalone with { DeclaresRuntimeUiUnityOrGamePackageMutation = true, AllowedHostApiGroups = ["runtime_direct_mutation"] }])),
            Invalid("nondeterministic_ordering_mutation", "nondeterministic ordering mutation", ValidateManifests(families, policy.Groups, manifests.Take(3).Reverse().ToList()))
        };

        return new LuaModuleManifestInvalidMatrix
        {
            ScenarioCount = cases.Count,
            MatchedExpectationCount = cases.Count(item => item.ExpectedValid == item.ActualValid),
            RejectedCount = cases.Count(item => !item.ActualValid),
            Passed = cases.All(item => item.ExpectedValid == item.ActualValid),
            Scenarios = cases.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static LuaModuleManifestDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    public static IReadOnlyList<LuaModuleManifestDiagnostic> SortDiagnostics(IEnumerable<LuaModuleManifestDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static void ValidateManifest(
        LuaModuleManifest manifest,
        IReadOnlyDictionary<string, LuaModuleFamilyDefinition> familyById,
        IReadOnlyDictionary<string, LuaHostApiGroup> hostApiById,
        IReadOnlySet<string> deniedHostApiIds,
        IReadOnlySet<string> knownContractIds,
        IReadOnlySet<string> knownIntentFamilies,
        IReadOnlyList<string> moduleIds,
        ICollection<LuaModuleManifestDiagnostic> diagnostics)
    {
        if (!StableIdPattern().IsMatch(manifest.ModuleId))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.module_id.invalid", manifest.ModuleId, "Module id must be stable."));
        }

        if (!LuaModuleManifestVocabulary.LifecycleStatuses.Contains(manifest.LifecycleStatus))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.lifecycle.unknown", manifest.ModuleId, "Lifecycle status is unknown."));
        }

        if (!LuaModuleManifestVocabulary.TargetDialects.Contains(manifest.TargetDialect))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.target_dialect.unknown", manifest.ModuleId, "Target dialect is unknown."));
        }

        if (!LuaModuleManifestVocabulary.SourceKinds.Contains(manifest.SourceKind))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.provenance.source_kind_unknown", manifest.ModuleId, "Source kind is unknown."));
        }

        if (!LuaModuleManifestVocabulary.PromotionStatuses.Contains(manifest.PromotionStatus))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.promotion_status.unknown", manifest.ModuleId, "Promotion status is unknown."));
        }

        if (!LuaModuleManifestVocabulary.SideEffectClasses.Contains(manifest.SideEffectClass))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.side_effect.unknown", manifest.ModuleId, "Side-effect class is unknown."));
        }

        if (!familyById.TryGetValue(manifest.FamilyId, out var family))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.family.unknown", manifest.ModuleId, "Manifest references an unknown family."));
        }
        else
        {
            foreach (var scope in family.RequiredSemanticScopes.Order(StringComparer.Ordinal))
            {
                if (!manifest.SemanticScopes.Contains(scope, StringComparer.Ordinal))
                {
                    diagnostics.Add(Diagnostic("error", "lua_manifest.semantic_scope.required_missing", $"{manifest.ModuleId}:{scope}", "Manifest dropped a family-required semantic scope."));
                }
            }
        }

        foreach (var scenario in manifest.ScenarioCompatibility.Order(StringComparer.Ordinal))
        {
            if (!LuaModuleManifestVocabulary.Scenarios.Contains(scenario))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.scenario.fake", $"{manifest.ModuleId}:{scenario}", "Manifest declares a fake scenario."));
            }
        }

        foreach (var profile in manifest.ProfileCompatibility.Order(StringComparer.Ordinal))
        {
            if (!LuaModuleManifestVocabulary.Scenarios.Contains(profile))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.profile.fake", $"{manifest.ModuleId}:{profile}", "Manifest declares a fake profile."));
            }
        }

        foreach (var contractId in manifest.ArtifactContractIds.Order(StringComparer.Ordinal))
        {
            if (!knownContractIds.Contains(contractId))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.artifact_contract.unknown", $"{manifest.ModuleId}:{contractId}", "Manifest references an unknown artifact contract."));
            }
        }

        foreach (var intentFamily in manifest.IntentFamilies.Order(StringComparer.Ordinal))
        {
            if (!knownIntentFamilies.Contains(intentFamily))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.intent_family.unknown", $"{manifest.ModuleId}:{intentFamily}", "Manifest references an unknown intent family."));
            }
        }

        foreach (var dependency in manifest.Dependencies.Order(StringComparer.Ordinal))
        {
            if (!moduleIds.Contains(dependency, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.dependency.unknown", $"{manifest.ModuleId}->{dependency}", "Dependency must reference a known module id."));
            }
        }

        foreach (var groupId in manifest.AllowedHostApiGroups.Order(StringComparer.Ordinal))
        {
            if (!hostApiById.TryGetValue(groupId, out var group))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.host_api.unknown", $"{manifest.ModuleId}:{groupId}", "Allowed host API group is unknown."));
                continue;
            }

            if (deniedHostApiIds.Contains(groupId))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.host_api.denied_allowed", $"{manifest.ModuleId}:{groupId}", "Denied host API group cannot be listed as allowed."));
            }

            if (!SideEffectCompatible(manifest.SideEffectClass, group.SideEffectClass))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.side_effect.mismatch", $"{manifest.ModuleId}:{groupId}", "Manifest side-effect class is not compatible with allowed host API group."));
            }
        }

        if (manifest.SourceKind is "llm_candidate" or "imported_candidate" or "goal_034_quarantined_candidate"
            && !manifest.ProvenanceId.StartsWith("quarantine/", StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.provenance.mismatch", manifest.ModuleId, "Candidate provenance must be quarantined."));
        }

        if (manifest.SourceKind is "llm_candidate" or "imported_candidate" or "goal_034_quarantined_candidate"
            && (manifest.LifecycleStatus is "ready" or "optional")
            && manifest.PromotionStatus != "reviewed")
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.candidate.ready_without_review", manifest.ModuleId, "Draft/quarantined candidates cannot become ready without deterministic review."));
        }

        if (manifest.LifecycleStatus == "future_required" && manifest.SelectableAsReady)
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.future_required.treated_ready", manifest.ModuleId, "Future-required modules must not be selectable as ready."));
        }

        if (manifest.ResourceBudget.MaxInputRecords > 1024
            || manifest.ResourceBudget.MaxOutputRecords > 256
            || manifest.ResourceBudget.MaxDependencyDepth > 8
            || manifest.ResourceBudget.MaxEstimatedMilliseconds > 1000
            || manifest.ResourceBudget.MaxMemoryKb > 4096)
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.resource_budget.over_limit", manifest.ModuleId, "Manifest resource budget exceeds Goal 035 metadata-only limits."));
        }

        var text = $"{manifest.DisplayName} {manifest.Summary} {manifest.ProvenanceDetails}";
        if (manifest.ContainsFinalProse || ContainsAny(text, FinalProseNeedles))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.final_prose.forbidden", manifest.ModuleId, "Manifest must not contain final prose content."));
        }

        if (manifest.DeclaresLuaSource || manifest.ClaimsLuaExecution || ContainsAny(text, LuaExecutionNeedles))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.lua_source_or_execution.forbidden", manifest.ModuleId, "Goal 035 manifests must not declare Lua source or execution."));
        }

        if (manifest.DeclaresProviderLlmRagAccess || ContainsAny(text, ProviderNeedles))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.provider_llm_rag.leakage", manifest.ModuleId, "Manifest leaks provider/LLM/RAG access."));
        }

        if (manifest.DeclaresRuntimeUiUnityOrGamePackageMutation
            || manifest.AllowedHostApiGroups.Intersect(["runtime_direct_mutation", "ui_winforms", "unity_direct_call", "gamepackage_schema_mutation"], StringComparer.Ordinal).Any()
            || ContainsAny(text, RuntimeUiUnityGamePackageNeedles))
        {
            diagnostics.Add(Diagnostic("error", "lua_manifest.runtime_ui_unity_gamepackage.leakage", manifest.ModuleId, "Manifest leaks Runtime/UI/Unity/GamePackage mutation authority."));
        }
    }

    private static IReadOnlyList<LuaModuleManifestDiagnostic> ValidateDependencyCycles(IReadOnlyList<LuaModuleManifest> manifests)
    {
        var diagnostics = new List<LuaModuleManifestDiagnostic>();
        var byId = manifests.GroupBy(item => item.ModuleId, StringComparer.Ordinal).ToDictionary(item => item.Key, item => item.First(), StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        foreach (var moduleId in byId.Keys.Order(StringComparer.Ordinal))
        {
            Visit(moduleId, []);
        }

        return diagnostics;

        void Visit(string moduleId, IReadOnlyList<string> path)
        {
            if (visited.Contains(moduleId) || !byId.TryGetValue(moduleId, out var manifest))
            {
                return;
            }

            if (!visiting.Add(moduleId))
            {
                diagnostics.Add(Diagnostic("error", "lua_manifest.dependency.cycle", moduleId, $"Dependency cycle detected: {string.Join("->", path.Concat([moduleId]))}."));
                return;
            }

            foreach (var dependency in manifest.Dependencies.Order(StringComparer.Ordinal))
            {
                Visit(dependency, path.Concat([moduleId]).ToList());
            }

            visiting.Remove(moduleId);
            visited.Add(moduleId);
        }
    }

    private static bool SideEffectCompatible(string manifestSideEffect, string apiSideEffect) =>
        manifestSideEffect switch
        {
            "metadata_only" => apiSideEffect is "read_only" or "planning_only" or "metadata_only",
            "planning_only" => apiSideEffect is "read_only" or "planning_only",
            "read_only" => apiSideEffect is "none" or "read_only",
            "none" => apiSideEffect is "none" or "read_only",
            _ => false
        };

    private static bool ContainsAny(string value, IReadOnlyList<string> needles)
    {
        var text = value.ToLowerInvariant();
        return needles.Any(text.Contains);
    }

    private static LuaModuleManifestInvalidScenario Invalid(string id, string kind, IReadOnlyList<LuaModuleManifestDiagnostic> diagnostics)
    {
        var sorted = SortDiagnostics(diagnostics);
        return new LuaModuleManifestInvalidScenario
        {
            ScenarioId = id,
            MutatedEvidenceKind = kind,
            ExpectedValid = false,
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
