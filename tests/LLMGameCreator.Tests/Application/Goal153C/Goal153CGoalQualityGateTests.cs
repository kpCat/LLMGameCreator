using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal153C;

public sealed class Goal153CGoalQualityGateTests
{
    [Fact]
    public void Activated_package_diff_gate_rejects_unclassified_and_proof_fixture_mutations()
    {
        var valid = Goal153CFixture.Create().Library.Catalog.Modules
            .Where(module => Goal153CFixture.GoalModuleIds.Contains(module.ModuleId)).ToList();
        foreach (var module in valid)
        {
            var diagnostics = new List<string>();
            Assert.True(FeatureModuleLibraryValidator.ValidateActivatedPackageDiffClaims(module, diagnostics),
                string.Join("; ", diagnostics));
        }

        var operation = new ProductLineRuntimeVariantMutationOperation
        {
            OperationId = "synthetic.proof",
            TargetKind = "encounter_participant_upsert",
            TargetId = "encounter/example|dummy",
            JsonPath = "game.encounters[id=encounter/example].participants[id=dummy]",
            ExpectedValue = "__MISSING_OR_EQUIVALENT__",
            NewValue = "{}",
            RuntimeDimension = "qualification_survivor"
        };
        var proofModule = Module(operation,
            "activated_package_diff:qualification_survivor:forbidden_qualification_proof_fixture");
        var proofDiagnostics = new List<string>();
        Assert.False(FeatureModuleLibraryValidator.ValidateActivatedPackageDiffClaims(proofModule, proofDiagnostics));
        Assert.Contains(proofDiagnostics, item => item.Contains("proof-only activated package mutation", StringComparison.Ordinal));

        var unclassified = Module(operation, null);
        var unclassifiedDiagnostics = new List<string>();
        Assert.False(FeatureModuleLibraryValidator.ValidateActivatedPackageDiffClaims(unclassified, unclassifiedDiagnostics));
        Assert.Contains(unclassifiedDiagnostics, item => item.Contains("unclassified activated package mutation", StringComparison.Ordinal));
    }

    [Fact]
    public void Constraint_definition_validation_rejects_invalid_same_and_dependent_defaults()
    {
        var same = ParameterModule("feature.synthetic.same", [], 3, 2, "${parameter:left}");
        var catalog = new Dictionary<string, FeatureModuleDefinition>(StringComparer.Ordinal) { [same.ModuleId] = same };
        var diagnostics = new List<string>();
        Assert.False(FeatureModuleParameterConstraintEvaluator.ValidateDefinitions(same, catalog, diagnostics));
        Assert.Contains(diagnostics, item => item.Contains("default parameter constraint rejected", StringComparison.Ordinal)
                                             && item.Contains("left=3", StringComparison.Ordinal)
                                             && item.Contains("right=2", StringComparison.Ordinal));

        var source = ParameterModule("feature.synthetic.source", [], 4, 4, "${parameter:left}", "==");
        var dependent = ParameterModule("feature.synthetic.dependent", [source.ModuleId], 5, 3,
            "${parameter:feature.synthetic.source.left}");
        catalog = new Dictionary<string, FeatureModuleDefinition>(StringComparer.Ordinal)
        {
            [source.ModuleId] = source,
            [dependent.ModuleId] = dependent
        };
        diagnostics.Clear();
        Assert.False(FeatureModuleParameterConstraintEvaluator.ValidateDefinitions(dependent, catalog, diagnostics));
        Assert.Contains(diagnostics, item => item.Contains("default parameter constraint rejected", StringComparison.Ordinal)
                                             && item.Contains("feature.synthetic.source.left=4", StringComparison.Ordinal)
                                             && item.Contains("feature.synthetic.dependent.right=3", StringComparison.Ordinal));
    }

    [Fact]
    public void Goal_quality_policy_requires_structured_product_proof_separation()
    {
        var policy = File.ReadAllText(Path.Combine(Goal153CFixture.FindRoot(), "docs", "GOAL_DESIGN_QUALITY_POLICY.md"));
        Assert.Contains("base package vs activated product package structured diff", policy, StringComparison.Ordinal);
        Assert.Contains("forbidden qualification/proof fixture count=0", policy, StringComparison.Ordinal);
        Assert.Contains("no unexplained global capacity or rule change", policy, StringComparison.Ordinal);
    }

    private static FeatureModuleDefinition Module(
        ProductLineRuntimeVariantMutationOperation operation,
        string? claim) => new()
    {
        ModuleId = "feature.synthetic.quality",
        Title = "Synthetic",
        Category = "test",
        ModuleKind = "test",
        RequiredValidationRules = claim is null
            ? ["activated_package_diff_classified"]
            : ["activated_package_diff_classified", claim],
        MutationOperations = [operation]
    };

    private static FeatureModuleDefinition ParameterModule(
        string id,
        IReadOnlyList<string> dependencies,
        int left,
        int right,
        string leftExpression,
        string op = "<=") => new()
    {
        ModuleId = id,
        Title = "Synthetic",
        Category = "test",
        ModuleKind = "test",
        Dependencies = dependencies,
        ParameterDefinitions = [Parameter(id, "left", left), Parameter(id, "right", right)],
        ParameterConstraints = [new FeatureModuleParameterConstraint
        {
            ConstraintId = "defaults",
            Kind = FeatureModuleParameterConstraintKinds.NumericCompare,
            LeftExpression = leftExpression,
            Operator = op,
            RightExpression = "${parameter:right}",
            DiagnosticCode = "synthetic.defaults",
            Message = "Synthetic defaults must satisfy the constraint."
        }]
    };

    private static FeatureModuleParameterDefinition Parameter(string moduleId, string id, int value) => new()
    {
        ModuleId = moduleId,
        ParameterId = id,
        Title = id,
        ValueType = FeatureModuleParameterValueTypes.Integer,
        Required = true,
        DefaultValue = JsonSerializer.SerializeToElement(value),
        Minimum = 0,
        Maximum = 10,
        AuthoringControl = FeatureModuleAuthoringControls.NumericUpDown
    };
}
