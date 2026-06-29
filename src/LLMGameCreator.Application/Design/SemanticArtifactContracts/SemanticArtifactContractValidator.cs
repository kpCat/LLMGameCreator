using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.SemanticArtifactContracts;

public static partial class SemanticArtifactContractValidator
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.Ordinal)
    {
        "ready",
        "optional",
        "blocked",
        "future_required",
        "deprecated"
    };

    private static readonly string[] LeakageNeedles =
    [
        "runtime_mutation",
        "runtime behavior change",
        "provider_call",
        "call provider",
        "llm_call",
        "call llm",
        "rag_call",
        "lua_execution",
        "execute lua",
        "winforms_ui",
        "ui mutation",
        "gamepackage_schema_change",
        "mutate gamepackage schema"
    ];

    public static IReadOnlyList<SemanticArtifactDiagnostic> ValidateContracts(IReadOnlyList<SemanticArtifactContractDescriptor> contracts)
    {
        var diagnostics = new List<SemanticArtifactDiagnostic>();
        var byId = contracts
            .Where(contract => !string.IsNullOrWhiteSpace(contract.ContractId))
            .GroupBy(contract => contract.ContractId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        foreach (var contract in contracts)
        {
            ValidateContractShape(contract, diagnostics);
        }

        foreach (var duplicate in byId.Where(item => item.Value.Count > 1))
        {
            diagnostics.Add(Diagnostic("error", "semantic_registry.contract_id.duplicate", duplicate.Key, "Contract ids must be unique."));
        }

        foreach (var contract in contracts.OrderBy(item => item.ContractId, StringComparer.Ordinal))
        {
            foreach (var dependency in contract.Dependencies.Order(StringComparer.Ordinal))
            {
                if (!byId.ContainsKey(dependency))
                {
                    diagnostics.Add(Diagnostic("error", "semantic_registry.dependency.unknown", $"{contract.ContractId}->{dependency}", "Contract dependency must reference a known contract id."));
                }
            }
        }

        diagnostics.AddRange(DetectCycles(contracts));
        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<string> ResolveDependencyOrder(IReadOnlyList<SemanticArtifactContractDescriptor> contracts)
    {
        var byId = contracts
            .GroupBy(contract => contract.ContractId, StringComparer.Ordinal)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var order = new List<string>();

        foreach (var id in byId.Keys.Order(StringComparer.Ordinal))
        {
            Visit(id);
        }

        return order;

        void Visit(string id)
        {
            if (!visited.Add(id))
            {
                return;
            }

            foreach (var dependency in byId[id].Dependencies.Order(StringComparer.Ordinal))
            {
                if (byId.ContainsKey(dependency))
                {
                    Visit(dependency);
                }
            }

            order.Add(id);
        }
    }

    public static SemanticArtifactDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    public static IReadOnlyList<SemanticArtifactDiagnostic> SortDiagnostics(IEnumerable<SemanticArtifactDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static void ValidateContractShape(SemanticArtifactContractDescriptor contract, ICollection<SemanticArtifactDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(contract.ContractId) || !ContractIdPattern().IsMatch(contract.ContractId))
        {
            diagnostics.Add(Diagnostic("error", "semantic_registry.contract_id.invalid", contract.ContractId, "Contract id must be a stable lowercase id."));
        }

        if (!Version.TryParse(contract.Version, out _))
        {
            diagnostics.Add(Diagnostic("error", "semantic_registry.version.invalid", contract.ContractId, "Contract version must be a valid version string."));
        }

        if (string.IsNullOrWhiteSpace(contract.ArtifactKind) || contract.ProducedArtifactTypes.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "semantic_registry.produced_artifact.missing", contract.ContractId, "Contract must declare artifact kind and at least one produced artifact type."));
        }

        var semanticDependent = contract.CapabilityTags.Contains("semantic_dependent", StringComparer.Ordinal)
            || contract.ConsumedArtifactTypes.Any(item => item.Contains("semantic", StringComparison.Ordinal));
        if (semanticDependent && contract.RequiredSemanticScopes.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "semantic_registry.semantic_scope.missing", contract.ContractId, "Semantic-dependent contracts must declare required semantic scopes."));
        }

        if (!ValidStatuses.Contains(contract.LifecycleStatus))
        {
            diagnostics.Add(Diagnostic("error", "semantic_registry.lifecycle.unknown", contract.ContractId, "Lifecycle status is not part of the Goal 030 vocabulary."));
        }

        var allTags = contract.CapabilityTags.Concat(contract.CompatibilityTags).ToArray();
        if (allTags.Contains("ready_now", StringComparer.Ordinal) && allTags.Contains("blocked_gap", StringComparer.Ordinal)
            || allTags.Contains("bcl_only", StringComparer.Ordinal) && allTags.Contains("provider_required", StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_registry.tags.incompatible", contract.ContractId, "Contract contains incompatible tag declarations."));
        }

        if (contract.LifecycleStatus == "ready"
            && (allTags.Any(IsFutureRequiredMarker) || contract.Notes.Contains("future required", StringComparison.OrdinalIgnoreCase) || contract.Notes.Contains("future-required", StringComparison.OrdinalIgnoreCase)))
        {
            diagnostics.Add(Diagnostic("error", "semantic_registry.lifecycle.future_required_marked_ready", contract.ContractId, "Future-required contracts must not be treated as ready."));
        }

        var leakageText = string.Join(" ", allTags.Append(contract.Notes)).ToLowerInvariant();
        if (LeakageNeedles.Any(leakageText.Contains))
        {
            diagnostics.Add(Diagnostic("error", "semantic_registry.boundary.leakage", contract.ContractId, "Goal 030 registry entries must not imply runtime, provider, LLM, RAG, Lua, UI or GamePackage schema mutation."));
        }
    }

    private static IEnumerable<SemanticArtifactDiagnostic> DetectCycles(IReadOnlyList<SemanticArtifactContractDescriptor> contracts)
    {
        var diagnostics = new List<SemanticArtifactDiagnostic>();
        var byId = contracts
            .GroupBy(contract => contract.ContractId, StringComparer.Ordinal)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        foreach (var id in byId.Keys.Order(StringComparer.Ordinal))
        {
            Visit(id, []);
        }

        return diagnostics;

        void Visit(string id, IReadOnlyList<string> path)
        {
            if (visited.Contains(id))
            {
                return;
            }

            if (!visiting.Add(id))
            {
                var cycleStart = path.ToList().IndexOf(id);
                var cycle = cycleStart >= 0 ? path.Skip(cycleStart).Append(id) : path.Append(id);
                diagnostics.Add(Diagnostic("error", "semantic_registry.dependency.cycle", string.Join("->", cycle), "Contract dependencies must be acyclic."));
                return;
            }

            foreach (var dependency in byId[id].Dependencies.Order(StringComparer.Ordinal))
            {
                if (byId.ContainsKey(dependency))
                {
                    Visit(dependency, path.Append(id).ToList());
                }
            }

            visiting.Remove(id);
            visited.Add(id);
        }
    }

    private static bool IsFutureRequiredMarker(string tag) =>
        tag.Contains("future_required", StringComparison.OrdinalIgnoreCase)
        || tag.Contains("future-required", StringComparison.OrdinalIgnoreCase);

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    [GeneratedRegex("^[a-z0-9][a-z0-9_./-]*[a-z0-9]$")]
    private static partial Regex ContractIdPattern();
}
