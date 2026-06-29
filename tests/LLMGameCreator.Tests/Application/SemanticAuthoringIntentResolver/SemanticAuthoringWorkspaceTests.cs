using LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticAuthoringIntentResolver;

public sealed class SemanticAuthoringWorkspaceTests
{
    [Fact]
    public void WorkspaceSchemaIsDeterministicAndCoversRequiredDomains()
    {
        var first = SemanticAuthoringIntentCatalog.BuildDefaultWorkspaces();
        var second = SemanticAuthoringIntentCatalog.BuildDefaultWorkspaces();

        Assert.Equal(first.Select(item => item.StableSummary), second.Select(item => item.StableSummary));
        Assert.All(first, workspace =>
        {
            foreach (var domain in SemanticAuthoringIntentVocabulary.DomainGroups)
            {
                Assert.Contains(workspace.DomainGroups, item => item.DomainId == domain);
            }

            Assert.DoesNotContain(SemanticAuthoringIntentValidator.ValidateWorkspace(workspace), item => item.Severity == "error");
        });
    }

    [Fact]
    public void WorkspacePreservesLegalAbsenceAndProvenanceKinds()
    {
        var frontier = Assert.Single(SemanticAuthoringIntentCatalog.BuildDefaultWorkspaces(), item => item.ScenarioId == "frontier_survival");
        var fields = frontier.DomainGroups.SelectMany(item => item.Sections).SelectMany(item => item.Fields).ToList();

        Assert.Contains(fields, item => item.DomainId == "faction" && item.CompletionStatus == "optional_absent");
        Assert.Contains(fields, item => item.Provenance == "inherited");
        Assert.Contains(fields, item => item.Provenance == "programmatic");
        Assert.Contains(fields, item => item.Provenance == "semantic_pack" || item.Provenance == "user");
        Assert.DoesNotContain(fields, item => item.Provenance is "llm_candidate" or "imported_candidate" && item.CompletionStatus == "complete");
    }
}
