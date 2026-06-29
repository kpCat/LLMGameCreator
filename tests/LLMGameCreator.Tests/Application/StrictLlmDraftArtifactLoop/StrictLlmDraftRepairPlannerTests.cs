using LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.StrictLlmDraftArtifactLoop;

public sealed class StrictLlmDraftRepairPlannerTests
{
    [Fact]
    public void RepairPlannerCreatesBoundedDataRecordsForFixableDiagnostics()
    {
        var requestSets = StrictLlmDraftArtifactLoopCatalog.BuildDefaultRequestSets();
        var requests = requestSets.SelectMany(item => item.Requests).OrderBy(item => item.RequestId, StringComparer.Ordinal).ToList();
        var candidate = StrictLlmDraftArtifactLoopCatalog.BuildProgrammaticFixtureCandidates(requestSets).First();
        var missingRequired = candidate with { PayloadFields = candidate.PayloadFields.Skip(1).ToList() };
        var diagnostics = StrictLlmDraftArtifactLoopValidator.ValidateCandidates(requests, [missingRequired]);

        var repairs = new StrictLlmDraftRepairPlanner().PlanRepairRequests(requests, [missingRequired], diagnostics);

        var repair = Assert.Single(repairs);
        Assert.Equal("planned", repair.Status);
        Assert.Contains("strict_draft.required_field.missing", repair.BlockingDiagnosticCodes);
        Assert.NotEmpty(repair.AllowedFieldsToFix);
        Assert.Contains("candidate_id", repair.ImmutableFields);
        Assert.Equal(candidate.ProvenanceId, repair.PreservedProvenanceId);
        Assert.DoesNotContain("provider", repair.BoundedHumanHint, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("llm", repair.BoundedHumanHint, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(StrictLlmDraftArtifactLoopValidator.ValidateRepairRequests([missingRequired], repairs), item => item.Severity == "error");
    }

    [Fact]
    public void RepairPlannerBlocksBoundaryLeakageAndFinalProse()
    {
        var requestSets = StrictLlmDraftArtifactLoopCatalog.BuildDefaultRequestSets();
        var requests = requestSets.SelectMany(item => item.Requests).OrderBy(item => item.RequestId, StringComparer.Ordinal).ToList();
        var candidate = StrictLlmDraftArtifactLoopCatalog.BuildProgrammaticFixtureCandidates(requestSets).First() with
        {
            PayloadFields = [new StrictLlmDraftPayloadField { Name = "dialogue_line", ValueKind = "text", Value = "final dialogue line", FinalProse = true }]
        };
        var diagnostics = StrictLlmDraftArtifactLoopValidator.ValidateCandidates(requests, [candidate]);

        var repair = Assert.Single(new StrictLlmDraftRepairPlanner().PlanRepairRequests(requests, [candidate], diagnostics));

        Assert.Equal("blocked", repair.Status);
        Assert.Empty(repair.AllowedFieldsToFix);
        Assert.Contains("strict_draft.final_prose.forbidden", repair.BlockingDiagnosticCodes);
    }
}
