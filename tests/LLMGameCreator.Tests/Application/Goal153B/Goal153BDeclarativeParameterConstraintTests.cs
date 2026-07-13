using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal153B;

public sealed class Goal153BDeclarativeParameterConstraintTests
{
    [Fact]
    public void Reusable_numeric_constraints_cover_all_operators_and_same_module_values()
    {
        foreach (var row in new[]
        {
            (Operator: "<", PassLeft: 2, PassRight: 3, FailLeft: 3, FailRight: 2),
            (Operator: "<=", PassLeft: 2, PassRight: 2, FailLeft: 3, FailRight: 2),
            (Operator: "==", PassLeft: 2, PassRight: 2, FailLeft: 3, FailRight: 2),
            (Operator: "!=", PassLeft: 2, PassRight: 3, FailLeft: 2, FailRight: 2),
            (Operator: ">=", PassLeft: 3, PassRight: 2, FailLeft: 2, FailRight: 3),
            (Operator: ">", PassLeft: 3, PassRight: 2, FailLeft: 2, FailRight: 3)
        })
        {
            var module = Module("feature.synthetic.constraint", [], row.Operator);
            var catalog = new FeatureModuleCatalogDocument { Modules = [module] };
            var pass = new FeatureModuleParameterBindingService().Bind(catalog, [module.ModuleId],
                [Value(module.ModuleId, "left", row.PassLeft), Value(module.ModuleId, "right", row.PassRight)]);
            Assert.True(pass.Passed, row.Operator + ": " + string.Join("; ", pass.Diagnostics));
            var fail = new FeatureModuleParameterBindingService().Bind(catalog, [module.ModuleId],
                [Value(module.ModuleId, "left", row.FailLeft), Value(module.ModuleId, "right", row.FailRight)]);
            Assert.False(fail.Passed);
            Assert.Contains(fail.Diagnostics, item => item.StartsWith("constraint.failed:", StringComparison.Ordinal)
                                                    && item.Contains("left=", StringComparison.Ordinal)
                                                    && item.Contains("right=", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Cross_module_constraints_require_dependency_and_selected_references()
    {
        var baseModule = Module("feature.synthetic.base", [], "<=");
        var dependent = Module("feature.synthetic.dependent", [baseModule.ModuleId], "<=", "${parameter:feature.synthetic.base.left}");
        var catalog = new FeatureModuleCatalogDocument { Modules = [baseModule, dependent] };
        var diagnostics = new List<string>();
        var byId = catalog.Modules.ToDictionary(module => module.ModuleId, StringComparer.Ordinal);
        Assert.True(FeatureModuleParameterConstraintEvaluator.ValidateDefinitions(dependent, byId, diagnostics));
        Assert.Empty(diagnostics);

        var allowed = new FeatureModuleParameterBindingService().Bind(catalog, [baseModule.ModuleId, dependent.ModuleId],
            [Value(baseModule.ModuleId, "left", 2), Value(baseModule.ModuleId, "right", 3),
             Value(dependent.ModuleId, "left", 2), Value(dependent.ModuleId, "right", 2)]);
        Assert.True(allowed.Passed, string.Join("; ", allowed.Diagnostics));

        var unselected = new FeatureModuleParameterBindingService().Bind(catalog, [dependent.ModuleId],
            [Value(dependent.ModuleId, "left", 2), Value(dependent.ModuleId, "right", 2)]);
        Assert.False(unselected.Passed);
        Assert.Contains(unselected.Diagnostics, item => item.Contains("unselected parameter constraint reference", StringComparison.Ordinal));

        var withoutDependency = dependent with { Dependencies = [] };
        diagnostics.Clear();
        Assert.False(FeatureModuleParameterConstraintEvaluator.ValidateDefinitions(withoutDependency, byId, diagnostics));
        Assert.Contains(diagnostics, item => item.Contains("cross-module parameter constraint dependency rejected", StringComparison.Ordinal));
    }

    [Fact]
    public void Invalid_constraint_references_and_fingerprint_changes_are_causal()
    {
        var module = Module("feature.synthetic.invalid", [], "<=", "${parameter:missing}");
        var byId = new Dictionary<string, FeatureModuleDefinition>(StringComparer.Ordinal) { [module.ModuleId] = module };
        var diagnostics = new List<string>();
        Assert.False(FeatureModuleParameterConstraintEvaluator.ValidateDefinitions(module, byId, diagnostics));
        Assert.Contains(diagnostics, item => item.Contains("unknown parameter constraint reference rejected", StringComparison.Ordinal));

        var invalidArithmetic = Module("feature.synthetic.invalid_arithmetic", [], "<=", "${parameter:left} / 0");
        byId = new Dictionary<string, FeatureModuleDefinition>(StringComparer.Ordinal) { [invalidArithmetic.ModuleId] = invalidArithmetic };
        diagnostics.Clear();
        Assert.False(FeatureModuleParameterConstraintEvaluator.ValidateDefinitions(invalidArithmetic, byId, diagnostics));
        Assert.Contains(diagnostics, item => item.Contains("division by zero rejected", StringComparison.Ordinal));

        var source = Module("feature.synthetic.fingerprint", [], "<=");
        var changed = source with { ParameterConstraints = [source.ParameterConstraints.Single() with { Message = "Changed diagnostic." }] };
        var unrelated = Module("feature.synthetic.unrelated", [], "<=");
        var fingerprints = new FeatureModuleLibraryFingerprintService();
        Assert.NotEqual(fingerprints.ModuleFingerprint(source), fingerprints.ModuleFingerprint(changed));
        Assert.Equal(fingerprints.ModuleFingerprint(unrelated), fingerprints.ModuleFingerprint(unrelated));
    }

    private static FeatureModuleDefinition Module(string id, IReadOnlyList<string> dependencies, string op, string? left = null) => new()
    {
        ModuleId = id,
        Title = "Synthetic",
        Category = "test",
        ModuleKind = "test",
        Selectable = true,
        Dependencies = dependencies,
        ParameterDefinitions = [Parameter(id, "left"), Parameter(id, "right")],
        ParameterConstraints = [new FeatureModuleParameterConstraint
        {
            ConstraintId = "compare",
            Kind = FeatureModuleParameterConstraintKinds.NumericCompare,
            LeftExpression = left ?? "${parameter:left}",
            Operator = op,
            RightExpression = "${parameter:right}",
            DiagnosticCode = "constraint.failed",
            Message = "Synthetic comparison failed."
        }]
    };

    private static FeatureModuleParameterDefinition Parameter(string moduleId, string parameterId) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Title = parameterId,
        ValueType = FeatureModuleParameterValueTypes.Integer,
        Required = true,
        DefaultValue = JsonSerializer.SerializeToElement(2),
        Minimum = 0,
        Maximum = 10,
        AuthoringControl = FeatureModuleAuthoringControls.NumericUpDown
    };

    private static FeatureModuleParameterValue Value(string moduleId, string parameterId, int value) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Value = JsonSerializer.SerializeToElement(value)
    };
}
