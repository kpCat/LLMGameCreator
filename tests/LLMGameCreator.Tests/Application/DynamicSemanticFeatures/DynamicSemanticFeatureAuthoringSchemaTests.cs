using LLMGameCreator.Application.Design.DynamicSemanticFeatures;
using Xunit;

namespace LLMGameCreator.Tests.Application.DynamicSemanticFeatures;

public sealed class DynamicSemanticFeatureAuthoringSchemaTests
{
    [Fact]
    public void AuthoringSchemaExposesGroupsOptionsApplicabilityAndSafeHints()
    {
        var service = new DynamicSemanticFeatureEvidenceService();
        var result = service.Build();
        var fields = result.AuthoringSchemaMatrix.Fields;

        Assert.NotEmpty(fields);
        Assert.Contains(fields, item => item.FeatureGroup == "npc" && item.FeatureId == "npc.mood" && item.OptionList.Contains("hungry"));
        Assert.Contains(fields, item => item.FeatureId == "species.module_capacity" && item.MinValue == 0 && item.MaxValue == 12);
        Assert.Contains(fields, item => item.InheritedValue != null && item.CanOverride);
        Assert.All(fields, field => Assert.Contains("application_layer_contract_only", field.SafeEditorHints));
        Assert.DoesNotContain(fields, field => field.SafeEditorHints.Any(hint => hint.Contains("llm", StringComparison.OrdinalIgnoreCase)));
    }
}
