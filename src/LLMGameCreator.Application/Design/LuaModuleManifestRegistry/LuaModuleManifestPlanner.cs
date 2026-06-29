namespace LLMGameCreator.Application.Design.LuaModuleManifestRegistry;

public sealed class LuaModuleManifestPlanner
{
    public LuaModuleSelectionPlan Plan(
        LuaModuleSelectionContext context,
        IReadOnlyList<LuaModuleManifest>? manifests = null,
        IReadOnlyList<LuaHostApiGroup>? hostApiGroups = null,
        IReadOnlyList<LuaModuleFamilyDefinition>? families = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        var allManifests = (manifests ?? LuaModuleManifestRegistryCatalog.BuildDefaultManifests())
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .ToList();
        var allHostApis = (hostApiGroups ?? LuaModuleManifestRegistryCatalog.BuildHostApiSurfacePolicy().Groups)
            .OrderBy(item => item.GroupId, StringComparer.Ordinal)
            .ToList();
        var allFamilies = (families ?? LuaModuleManifestRegistryCatalog.BuildFamilies())
            .OrderBy(item => item.FamilyId, StringComparer.Ordinal)
            .ToList();

        var diagnostics = new List<LuaModuleManifestDiagnostic>();
        if (!LuaModuleManifestVocabulary.Scenarios.Contains(context.ScenarioId))
        {
            diagnostics.Add(LuaModuleManifestRegistryValidator.Diagnostic("error", "lua_manifest.plan.scenario.fake", context.ScenarioId, "Planner context references a fake scenario."));
        }

        var validationDiagnostics = LuaModuleManifestRegistryValidator.ValidateFamilies(allFamilies)
            .Concat(LuaModuleManifestRegistryValidator.ValidateHostApiSurface(allHostApis))
            .Concat(LuaModuleManifestRegistryValidator.ValidateManifests(allFamilies, allHostApis, allManifests));

        diagnostics.AddRange(validationDiagnostics);

        var compatible = allManifests
            .Where(item => item.ScenarioCompatibility.Contains(context.ScenarioId, StringComparer.Ordinal)
                && item.ProfileCompatibility.Contains(context.ProfileId, StringComparer.Ordinal)
                && (context.RequestedFamilyIds.Count == 0 || context.RequestedFamilyIds.Contains(item.FamilyId, StringComparer.Ordinal))
                && (context.RequiredSemanticScopes.Count == 0 || item.SemanticScopes.Intersect(context.RequiredSemanticScopes, StringComparer.Ordinal).Any()))
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .ToList();

        var selected = compatible
            .Where(item => item.SelectableAsReady && (item.LifecycleStatus is "ready" or "optional"))
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .ToList();

        var blocked = compatible
            .Where(item => item.LifecycleStatus is "blocked" or "deprecated" or "draft" or "quarantined" or "review_required")
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .ToList();

        var futureRequired = compatible
            .Where(item => item.LifecycleStatus == "future_required")
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .ToList();

        var selectedIds = selected.Select(item => item.ModuleId).ToHashSet(StringComparer.Ordinal);
        var missingDependencies = selected
            .SelectMany(item => item.Dependencies.Select(dependency => new LuaModuleMissingDependency { ModuleId = item.ModuleId, MissingDependencyId = dependency }))
            .Where(item => !selectedIds.Contains(item.MissingDependencyId))
            .OrderBy(item => item.ModuleId, StringComparer.Ordinal)
            .ThenBy(item => item.MissingDependencyId, StringComparer.Ordinal)
            .ToList();

        diagnostics.AddRange(missingDependencies.Select(item =>
            LuaModuleManifestRegistryValidator.Diagnostic("error", "lua_manifest.plan.dependency.missing", $"{item.ModuleId}->{item.MissingDependencyId}", "Selected manifest dependency was not selected for this plan.")));

        if (selected.Count == 0)
        {
            diagnostics.Add(LuaModuleManifestRegistryValidator.Diagnostic("warning", "lua_manifest.plan.selection.empty", context.ScenarioId, "No manifests were selected for this scenario."));
        }

        var deniedDiagnostics = LuaModuleManifestRegistryValidator.SortDiagnostics(diagnostics.Where(item =>
            item.Code.Contains(".host_api.", StringComparison.Ordinal)
            || item.Code.Contains("provider_llm_rag", StringComparison.Ordinal)
            || item.Code.Contains("runtime_ui_unity_gamepackage", StringComparison.Ordinal)
            || item.Code.Contains("lua_source_or_execution", StringComparison.Ordinal)));

        var compatibilityDiagnostics = LuaModuleManifestRegistryValidator.SortDiagnostics(diagnostics.Where(item =>
            !deniedDiagnostics.Contains(item)
            && item.Severity is "error" or "warning"));

        var dependencyOrder = BuildDependencyOrder(selected);
        var summary = new LuaModuleSelectionSummary
        {
            ScenarioId = context.ScenarioId,
            ProfileId = context.ProfileId,
            SelectedCount = selected.Count,
            BlockedCount = blocked.Count,
            FutureRequiredCount = futureRequired.Count,
            MissingDependencyCount = missingDependencies.Count,
            DiagnosticCount = diagnostics.Count(item => item.Severity is "error" or "warning"),
            StableSummary = $"{context.ScenarioId}|selected={selected.Count}|blocked={blocked.Count}|future={futureRequired.Count}|missing={missingDependencies.Count}|order={dependencyOrder.Count}"
        };

        return new LuaModuleSelectionPlan
        {
            ScenarioId = context.ScenarioId,
            ProfileId = context.ProfileId,
            SelectedManifests = selected,
            DependencyOrder = dependencyOrder,
            BlockedManifests = blocked,
            FutureRequiredManifests = futureRequired,
            MissingDependencies = missingDependencies,
            DeniedApiDiagnostics = deniedDiagnostics,
            CompatibilityDiagnostics = compatibilityDiagnostics,
            Summary = summary
        };
    }

    public IReadOnlyList<LuaModuleSelectionPlan> PlanDefaultScenarios() =>
        LuaModuleManifestRegistryCatalog.BuildDefaultSelectionContexts()
            .Select(context => Plan(context))
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();

    public LuaModuleDependencyPlan BuildDependencyPlan(IReadOnlyList<LuaModuleSelectionPlan> plans)
    {
        var rows = plans
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(item => new LuaModuleDependencyPlanRow
            {
                ScenarioId = item.ScenarioId,
                DependencyOrder = item.DependencyOrder,
                MissingDependencies = item.MissingDependencies
            })
            .ToList();

        return new LuaModuleDependencyPlan
        {
            Rows = rows,
            ScenarioCount = rows.Count,
            DependencyOrdersStable = rows.All(item => item.DependencyOrder.SequenceEqual(item.DependencyOrder.Distinct(StringComparer.Ordinal)))
        };
    }

    private static IReadOnlyList<string> BuildDependencyOrder(IReadOnlyList<LuaModuleManifest> selected)
    {
        var byId = selected.ToDictionary(item => item.ModuleId, StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var order = new List<string>();

        foreach (var manifest in selected.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal))
        {
            Visit(manifest.ModuleId);
        }

        return order;

        void Visit(string moduleId)
        {
            if (!visited.Add(moduleId) || !byId.TryGetValue(moduleId, out var manifest))
            {
                return;
            }

            foreach (var dependency in manifest.Dependencies.Order(StringComparer.Ordinal))
            {
                Visit(dependency);
            }

            order.Add(moduleId);
        }
    }
}
