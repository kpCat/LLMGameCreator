using LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.StrictLlmDraftArtifactLoop;

public sealed class StrictLlmDraftArtifactLoopCatalogTests
{
    [Fact]
    public void DraftFamilyCatalogIsDeterministicAndForbidsDialogueFinalProse()
    {
        var first = StrictLlmDraftArtifactLoopCatalog.BuildDraftFamilies();
        var second = StrictLlmDraftArtifactLoopCatalog.BuildDraftFamilies();

        Assert.Equal(first.Select(item => item.FamilyId), second.Select(item => item.FamilyId));
        Assert.Equal(9, first.Count);
        Assert.DoesNotContain(StrictLlmDraftArtifactLoopValidator.ValidateFamilies(first), item => item.Severity == "error");
        Assert.Equal(first.Count, first.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count());

        var dialogue = first.Single(item => item.FamilyId == "dialogue_act_template_slot_draft");
        Assert.Contains("dialogue_line", dialogue.ForbiddenFields);
        Assert.Contains("final_dialogue_prose", dialogue.ForbiddenFields);
        Assert.Contains("template_slot_id", dialogue.RequiredFields);
        Assert.True(dialogue.NoFinalProse);
        Assert.True(dialogue.NoRuntimeAuthority);
    }
}
