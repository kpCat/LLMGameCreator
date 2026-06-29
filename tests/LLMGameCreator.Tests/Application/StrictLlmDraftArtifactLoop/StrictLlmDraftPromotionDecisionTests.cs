using LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.StrictLlmDraftArtifactLoop;

public sealed class StrictLlmDraftPromotionDecisionTests
{
    [Fact]
    public void ValidQuarantinedCandidatesPromoteOnlyThroughPromotionEngine()
    {
        var requestSets = StrictLlmDraftArtifactLoopCatalog.BuildDefaultRequestSets();
        var requests = requestSets.SelectMany(item => item.Requests).OrderBy(item => item.RequestId, StringComparer.Ordinal).ToList();
        var candidate = StrictLlmDraftArtifactLoopCatalog.BuildProgrammaticFixtureCandidates(requestSets).First();

        var decision = Assert.Single(new StrictLlmDraftPromotionDecisionEngine().Decide(requests, [candidate]));

        Assert.True(decision.Promoted);
        Assert.Equal("promoted", decision.Status);
        Assert.StartsWith("draft-artifact/", decision.TargetDraftArtifactId, StringComparison.Ordinal);
        Assert.Equal(candidate.ProvenanceId, decision.PreservedProvenanceId);
    }

    [Fact]
    public void InvalidCandidatesBecomeRepairRequiredOrRejected()
    {
        var requestSets = StrictLlmDraftArtifactLoopCatalog.BuildDefaultRequestSets();
        var requests = requestSets.SelectMany(item => item.Requests).OrderBy(item => item.RequestId, StringComparer.Ordinal).ToList();
        var valid = StrictLlmDraftArtifactLoopCatalog.BuildProgrammaticFixtureCandidates(requestSets).Take(2).ToList();
        var fixable = valid[0] with { CandidateId = "candidate/fixable/missing-required", PayloadFields = valid[0].PayloadFields.Skip(1).ToList() };
        var selfPromoted = valid[1] with { CandidateId = "candidate/rejected/self-promoted", Status = "promoted" };

        var decisions = new StrictLlmDraftPromotionDecisionEngine().Decide(requests, [fixable, selfPromoted]);

        Assert.Contains(decisions, item => item.CandidateId == fixable.CandidateId && item.Status == "repair_required" && !item.Promoted);
        Assert.Contains(decisions, item => item.CandidateId == selfPromoted.CandidateId && item.Status == "rejected" && item.Diagnostics.Any(diagnostic => diagnostic.Code == "strict_draft.candidate.self_promoted"));
    }
}
