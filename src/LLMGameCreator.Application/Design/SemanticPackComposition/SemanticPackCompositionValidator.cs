using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.SemanticArtifactContracts;

namespace LLMGameCreator.Application.Design.SemanticPackComposition;

public static partial class SemanticPackCompositionValidator
{
    private static readonly string[] LeakageNeedles =
    [
        "runtime",
        "winforms",
        "winforms_ui",
        "ui mutation",
        "unity",
        "provider",
        "llm",
        "rag",
        "lua",
        "gamepackage schema",
        "gamepackage_schema",
        "schema mutation",
        "execute script",
        "call provider"
    ];

    public static IReadOnlyList<SemanticArtifactDiagnostic> ValidateCatalog(
        IReadOnlyList<SemanticPackCompositionPack> packs,
        IReadOnlyList<SemanticArtifactContractDescriptor>? contracts = null)
    {
        contracts ??= SemanticArtifactContractRegistry.BuildDefaultContracts();
        var diagnostics = new List<SemanticArtifactDiagnostic>();
        var contractIds = contracts.Select(contract => contract.ContractId).ToHashSet(StringComparer.Ordinal);
        var artifactKinds = contracts.Select(contract => contract.ArtifactKind).ToHashSet(StringComparer.Ordinal);
        var factsById = packs
            .SelectMany(pack => pack.Facts)
            .Where(fact => !string.IsNullOrWhiteSpace(fact.FactId))
            .GroupBy(fact => fact.FactId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var packIds = packs
            .Where(pack => !string.IsNullOrWhiteSpace(pack.PackId))
            .GroupBy(pack => pack.PackId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        foreach (var pack in packs.OrderBy(pack => pack.PackId, StringComparer.Ordinal))
        {
            ValidatePackShape(pack, diagnostics);
            ValidatePackReferences(pack, factsById, packIds.Keys.ToHashSet(StringComparer.Ordinal), contractIds, artifactKinds, diagnostics);
        }

        foreach (var duplicate in packIds.Where(item => item.Value.Count > 1).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_pack.catalog.pack_id.duplicate", duplicate.Key, "Semantic pack ids must be unique."));
        }

        foreach (var duplicate in factsById.Where(item => item.Value.Count > 1).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_pack.fact_id.duplicate", duplicate.Key, "Semantic fact ids must be unique across selected catalog seeds."));
        }

        diagnostics.AddRange(DetectImplicationCycles(packs, factsById.Keys.ToHashSet(StringComparer.Ordinal)));
        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<SemanticArtifactDiagnostic> ValidateRequest(
        SemanticPackCompositionRequest request,
        IReadOnlyList<SemanticPackCompositionPack> packs)
    {
        var diagnostics = new List<SemanticArtifactDiagnostic>();
        if (!SemanticPackCompositionCatalog.SupportedProfileIds.Contains(request.ProfileId))
        {
            diagnostics.Add(Diagnostic("error", "semantic_pack.request.profile.unknown", request.ProfileId, "Composition request references an unknown profile/family id."));
        }

        foreach (var duplicate in request.SelectedPackIds
                     .GroupBy(id => id, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_pack.request.pack_id.duplicate", duplicate.Key, "Selected pack ids must not repeat."));
        }

        var byId = packs.ToDictionary(pack => pack.PackId, StringComparer.Ordinal);
        foreach (var packId in request.SelectedPackIds.Order(StringComparer.Ordinal))
        {
            if (!byId.TryGetValue(packId, out var pack))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.request.pack_id.unknown", packId, "Selected semantic pack id is not in the seed catalog."));
                continue;
            }

            if (pack.IsFutureOnly)
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.request.pack.future_only", packId, "Future-only semantic pack cannot be selected as ready."));
            }

            if (!IsPackCompatible(pack, request.ProfileId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.request.profile.unsupported", packId, "Selected semantic pack does not support the requested profile/family id."));
            }
        }

        foreach (var selected in request.SelectedPackIds.Order(StringComparer.Ordinal))
        {
            if (!byId.TryGetValue(selected, out var selectedPack))
            {
                continue;
            }

            foreach (var excluded in selectedPack.Exclusions.Order(StringComparer.Ordinal))
            {
                if (request.SelectedPackIds.Contains(excluded, StringComparer.Ordinal))
                {
                    diagnostics.Add(Diagnostic("error", "semantic_pack.selection.exclusion.incompatible", $"{selected}->{excluded}", "Selected semantic packs declare an incompatible exclusion."));
                }
            }
        }

        return SortDiagnostics(diagnostics);
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

    internal static bool IsPackCompatible(SemanticPackCompositionPack pack, string profileId) =>
        pack.SupportedProfileIds.Contains(profileId, StringComparer.Ordinal)
        || pack.SupportedProfileIds.Contains("*", StringComparer.Ordinal);

    private static void ValidatePackShape(
        SemanticPackCompositionPack pack,
        ICollection<SemanticArtifactDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(pack.PackId) || !StableIdPattern().IsMatch(pack.PackId))
        {
            diagnostics.Add(Diagnostic("error", "semantic_pack.pack_id.invalid", pack.PackId, "Semantic pack id must be a stable lowercase id."));
        }

        if (pack.SupportedProfileIds.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "semantic_pack.profile.missing", pack.PackId, "Semantic pack must declare at least one supported profile/family id."));
        }

        foreach (var profileId in pack.SupportedProfileIds.Order(StringComparer.Ordinal))
        {
            if (profileId != "*" && !SemanticPackCompositionCatalog.SupportedProfileIds.Contains(profileId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.profile.unknown", $"{pack.PackId}:{profileId}", "Semantic pack references an unknown profile/family id."));
            }
        }

        if (pack.ProvidedSemanticScopes.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "semantic_pack.scope.missing", pack.PackId, "Semantic pack must declare provided semantic scopes."));
        }

        if (pack.IsFutureOnly && string.Equals(pack.SourceStatus, "ready", StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_pack.lifecycle.future_only_marked_ready", pack.PackId, "Future-only semantic pack must not be marked ready."));
        }

        var leakageText = string.Join(" ", pack.ThemeTags.Append(pack.SourceNotes)).ToLowerInvariant();
        if (LeakageNeedles.Any(leakageText.Contains))
        {
            diagnostics.Add(Diagnostic("error", "semantic_pack.boundary.leakage", pack.PackId, "Goal 031 semantic packs must not imply Runtime, UI, Unity, provider, LLM, RAG, Lua or GamePackage schema changes."));
        }

        foreach (var fact in pack.Facts.OrderBy(fact => fact.FactId, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(fact.FactId) || !StableIdPattern().IsMatch(fact.FactId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.fact_id.invalid", $"{pack.PackId}:{fact.FactId}", "Semantic fact id must be stable and lowercase."));
            }

            if (!SemanticPackCompositionCatalog.ValidFactDomains.Contains(fact.Domain))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.fact.domain.invalid", fact.FactId, "Semantic fact domain is not part of the Goal 031 domain vocabulary."));
            }

            var factLeakageText = string.Join(" ", fact.Tags.Append(fact.SourceNote)).ToLowerInvariant();
            if (LeakageNeedles.Any(factLeakageText.Contains))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.boundary.leakage", fact.FactId, "Semantic fact must not imply forbidden boundary changes."));
            }
        }
    }

    private static void ValidatePackReferences(
        SemanticPackCompositionPack pack,
        IReadOnlyDictionary<string, List<SemanticPackFact>> factsById,
        IReadOnlySet<string> packIds,
        IReadOnlySet<string> contractIds,
        IReadOnlySet<string> artifactKinds,
        ICollection<SemanticArtifactDiagnostic> diagnostics)
    {
        foreach (var excluded in pack.Exclusions.Order(StringComparer.Ordinal))
        {
            if (!packIds.Contains(excluded))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.exclusion.unknown", $"{pack.PackId}->{excluded}", "Semantic pack exclusion must reference a known pack id."));
            }
        }

        foreach (var relation in pack.RelationHints.OrderBy(relation => relation.RelationId, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(relation.RelationId) || !StableIdPattern().IsMatch(relation.RelationId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.relation_id.invalid", $"{pack.PackId}:{relation.RelationId}", "Relation id must be stable and lowercase."));
            }

            if (!factsById.ContainsKey(relation.SourceFactId) || !factsById.ContainsKey(relation.TargetFactId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.relation.fact.unknown", relation.RelationId, "Relation must reference known semantic fact ids."));
            }
        }

        foreach (var intent in pack.ExpansionIntents.OrderBy(intent => intent.IntentId, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(intent.IntentId) || !StableIdPattern().IsMatch(intent.IntentId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.expansion_intent_id.invalid", $"{pack.PackId}:{intent.IntentId}", "Expansion intent id must be stable and lowercase."));
            }

            if (!factsById.ContainsKey(intent.SourceFactId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.expansion_intent.fact.unknown", intent.IntentId, "Expansion intent must reference a known semantic fact id."));
            }

            if (!contractIds.Contains(intent.TargetContractId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.expansion_intent.contract.unknown", intent.IntentId, "Expansion intent must reference a known Goal 030 contract id."));
            }

            if (!artifactKinds.Contains(intent.TargetArtifactKind))
            {
                diagnostics.Add(Diagnostic("error", "semantic_pack.expansion_intent.artifact_kind.unknown", intent.IntentId, "Expansion intent must reference a known Goal 030 artifact kind."));
            }
        }
    }

    private static IEnumerable<SemanticArtifactDiagnostic> DetectImplicationCycles(
        IReadOnlyList<SemanticPackCompositionPack> packs,
        IReadOnlySet<string> factIds)
    {
        var diagnostics = new List<SemanticArtifactDiagnostic>();
        var edges = packs
            .SelectMany(pack => pack.RelationHints)
            .Where(relation => relation.Directed && relation.RelationKind == "implies")
            .Where(relation => factIds.Contains(relation.SourceFactId) && factIds.Contains(relation.TargetFactId))
            .GroupBy(relation => relation.SourceFactId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(relation => relation.TargetFactId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        foreach (var id in factIds.Order(StringComparer.Ordinal))
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
                diagnostics.Add(Diagnostic("error", "semantic_pack.relation.implies.cycle", string.Join("->", cycle), "Directed implication relations must be acyclic."));
                return;
            }

            if (edges.TryGetValue(id, out var targets))
            {
                foreach (var target in targets)
                {
                    Visit(target, path.Append(id).ToList());
                }
            }

            visiting.Remove(id);
            visited.Add(id);
        }
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    [GeneratedRegex("^[a-z0-9][a-z0-9_./-]*[a-z0-9]$")]
    private static partial Regex StableIdPattern();
}
