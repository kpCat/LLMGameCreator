using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.ProjectStandaloneBuild;

public sealed class Goal155ReleaseCandidatePayloadTests
{
    [Fact]
    public void Behavioral_complete_summary_supplies_every_accepted_fact_and_rc_ready_fact()
    {
        var build = LLMGameCreator.Tests.Application.Goal155.Goal155AcceptedMechanicsProjectionTests.Complete();
        var service = new GameProjectAcceptedMechanicsSummaryService();
        build = build with { AcceptedMechanics = service.Project(build) };
        var facts = service.StandaloneHumanFacts(build, includeReleaseCandidateReady: true);
        Assert.All(build.AcceptedMechanics.HumanFacts, expected =>
            Assert.Contains(facts, actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.Contains(facts, fact => fact.Label == "Release Candidate" && fact.Value == "готов");
        Assert.Contains(facts, fact => fact.Label == "Прямой урон" && fact.Value == "2");
        Assert.Contains(facts, fact => fact.Label == "Начальная мана" && fact.Value == "12");
    }

    [Fact]
    public void Behavioral_incomplete_summary_never_adds_rc_ready_fact()
    {
        var source = LLMGameCreator.Tests.Application.Goal155.Goal155AcceptedMechanicsProjectionTests.Complete()
            with { AbilitySummary = string.Empty };
        var service = new GameProjectAcceptedMechanicsSummaryService();
        var build = source with { AcceptedMechanics = service.Project(source) };
        Assert.False(build.AcceptedMechanics.Passed);
        Assert.DoesNotContain(service.StandaloneHumanFacts(build, includeReleaseCandidateReady: true),
            fact => fact.Label == "Release Candidate");
    }
}
