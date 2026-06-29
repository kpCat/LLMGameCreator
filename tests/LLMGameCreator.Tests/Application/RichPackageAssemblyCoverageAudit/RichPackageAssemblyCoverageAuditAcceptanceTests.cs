using System.Text.Json;
using LLMGameCreator.Application.Design.RichPackageAssemblyCoverageAudit;
using Xunit;

namespace LLMGameCreator.Tests.Application.RichPackageAssemblyCoverageAudit;

public sealed class RichPackageAssemblyCoverageAuditAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicCoverageAuditArtifacts()
    {
        using var temp = new TempDirectory();
        CopyGoal023Artifacts(FindRepoRoot(), temp.Path);
        var service = new RichPackageAssemblyCoverageAuditAcceptanceService();

        var first = await service.BuildAsync(temp.Path);
        var second = await service.BuildAsync(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(RichPackageAssemblyCoverageAuditAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(RichPackageAssemblyCoverageAuditAcceptanceService.FinalGate, first.Report.ManualGate);
        Assert.Equal(RichPackageAssemblyCoverageAuditAcceptanceService.PreviousAcceptedGate, first.Report.PreviousAcceptedGate);
        Assert.Equal(["S192", "S193", "S194", "S195", "S196", "S197", "S198"], first.Report.CompletedSlices);
        Assert.True(first.Report.Goal023EvidenceVerified);
        Assert.Equal(8, first.Report.CoverageDomainCount);
        Assert.Equal(first.Report.CoverageMatrixHash, second.Report.CoverageMatrixHash);
        Assert.Equal(first.Report.GapReportHash, second.Report.GapReportHash);
        Assert.Equal(first.Report.NextSlicePlanHash, second.Report.NextSlicePlanHash);
        Assert.Equal(first.Report.InvalidMatrixHash, second.Report.InvalidMatrixHash);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(File.Exists(write.CoverageMatrixJsonPath));
        Assert.True(File.Exists(write.GapReportJsonPath));
        Assert.True(File.Exists(write.NextSlicePlanJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
    }

    [Fact]
    public async Task CoverageDomainsArePresentAndEvidenceBacked()
    {
        var repoRoot = FindRepoRoot();
        var result = await new RichPackageAssemblyCoverageAuditAcceptanceService().BuildAsync(repoRoot);

        Assert.True(result.Report.ContractProofPassed, string.Join(Environment.NewLine, result.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.Equal(8, result.CoverageMatrix.Domains.Count);
        Assert.Contains(result.CoverageMatrix.Domains, domain => domain.DomainId == "world" && domain.CandidatePackageSchemaAreas.Contains("GamePackageDefinition.Game.Maps"));
        Assert.Contains(result.CoverageMatrix.Domains, domain => domain.DomainId == "entities" && domain.CandidatePackageSchemaAreas.Contains("MapDefinition.Entities"));
        Assert.Contains(result.CoverageMatrix.Domains, domain => domain.DomainId == "quests" && domain.ValidatorIds.Contains("NarrativeDefinitionValidator"));
        Assert.Contains(result.CoverageMatrix.Domains, domain => domain.DomainId == "items_inventory_economy" && domain.ValidatorIds.Contains("EconomyDefinitionValidator"));
        Assert.All(result.CoverageMatrix.Domains, domain =>
        {
            Assert.NotEmpty(domain.RelatedProfileIds);
            Assert.NotEmpty(domain.RelatedGoal023PipelineInputIds);
            Assert.NotEmpty(domain.Evidence);
            Assert.NotEqual("package_supported", domain.SupportStatus);
        });
    }

    [Fact]
    public async Task FutureRequiredAndBlockedGapsArePreserved()
    {
        var repoRoot = FindRepoRoot();
        var result = await new RichPackageAssemblyCoverageAuditAcceptanceService().BuildAsync(repoRoot);

        Assert.True(result.Report.FutureRequiredAndBlockedGapsPreserved);
        Assert.Contains(result.GapReport.Gaps, gap => gap.Status == "future_required");
        Assert.Contains(result.GapReport.Gaps, gap => gap.Status == "blocked_gap");
        Assert.Contains(result.CoverageMatrix.Domains, domain => domain.Evidence.Any(item => item.EvidenceClass == "future_required"));
        Assert.Contains(result.CoverageMatrix.Domains, domain => domain.Evidence.Any(item => item.EvidenceClass == "blocked_gap"));
        Assert.DoesNotContain(result.CoverageMatrix.Domains, domain =>
            domain.SupportStatus == "package_supported"
            && domain.Evidence.Any(item => item.EvidenceClass is "future_required" or "blocked_gap"));
    }

    [Fact]
    public async Task NextSlicePlanIsRecommendationOnly()
    {
        var repoRoot = FindRepoRoot();
        var result = await new RichPackageAssemblyCoverageAuditAcceptanceService().BuildAsync(repoRoot);

        Assert.False(result.NextSlicePlan.StartsGoal025OrS199);
        Assert.Equal("package_assembly_expansion_1_world_and_entities", result.NextSlicePlan.RecommendedFirstCandidateId);
        Assert.All(result.NextSlicePlan.Candidates, candidate => Assert.False(candidate.StartsGoal025OrS199));
        Assert.Contains(result.NextSlicePlan.Candidates, candidate => candidate.Title == "Package Assembly Expansion 1 - World And Entities" && candidate.Recommended);
    }

    [Fact]
    public async Task InvalidFakeLeakMatrixRejectsRequiredScenarios()
    {
        var repoRoot = FindRepoRoot();
        var result = await new RichPackageAssemblyCoverageAuditAcceptanceService().BuildAsync(repoRoot);

        Assert.True(result.InvalidMatrix.Passed);
        Assert.True(result.InvalidMatrix.ScenarioCount >= 16);
        Assert.Equal(result.InvalidMatrix.ScenarioCount, result.InvalidMatrix.RejectedCount);
        Assert.All(result.InvalidMatrix.Scenarios, scenario => Assert.False(scenario.ActualValid));
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "missing_accepted_goal023_report");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "docs_only_gamepackage_mention_treated_as_support");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "future_required_capability_marked_package_supported");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "goal025_or_s199_started_marker");
    }

    [Fact]
    public async Task RejectsMissingGateMissingInputsAndMutationClaims()
    {
        var repoRoot = FindRepoRoot();
        var service = new RichPackageAssemblyCoverageAuditAcceptanceService();

        var stale = await service.BuildAsync(repoRoot, new RichPackageAssemblyCoverageAuditOptions { PreviousAcceptedGate = "capability_bundle_pipeline_inputs_verification required" });
        var copied = await service.BuildAsync(repoRoot, new RichPackageAssemblyCoverageAuditOptions { CopiedCoverageReportWithoutGoal023GeneratorInputs = true });
        var mutation = await service.BuildAsync(repoRoot, new RichPackageAssemblyCoverageAuditOptions
        {
            DocsOnlyPackageMentionTreatedAsSupport = true,
            PackageAssemblyExecutedClaim = true,
            Goal025OrS199StartedMarker = true
        });

        Assert.False(stale.Report.ContractProofPassed);
        Assert.Contains(stale.Report.Diagnostics, item => item.Code == "rich_package_audit.previous_gate.missing");
        Assert.False(copied.Report.ContractProofPassed);
        Assert.Contains(copied.Report.Diagnostics, item => item.Code == "rich_package_audit.goal023_generator_inputs.missing");
        Assert.False(mutation.Report.ContractProofPassed);
        Assert.Contains(mutation.Report.Diagnostics, item => item.Code == "rich_package_audit.docs_only_support_claim");
        Assert.Contains(mutation.Report.Diagnostics, item => item.Code == "rich_package_audit.claims.package_assembly_executed");
        Assert.Contains(mutation.Report.Diagnostics, item => item.Code == "rich_package_audit.next_goal.started");
    }

    [Fact]
    public async Task WrittenReportRoundTripsManualGate()
    {
        using var temp = new TempDirectory();
        CopyGoal023Artifacts(FindRepoRoot(), temp.Path);
        var service = new RichPackageAssemblyCoverageAuditAcceptanceService();
        var result = await service.BuildAsync(temp.Path);
        var write = await service.WriteAsync(temp.Path, result);

        var report = JsonSerializer.Deserialize<RichPackageAssemblyCoverageAuditReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(RichPackageAssemblyCoverageAuditAcceptanceService.FinalGate, report.FinalStatus);
        Assert.Equal(RichPackageAssemblyCoverageAuditAcceptanceService.FinalGate, report.ManualGate);
        Assert.True(report.Goal023EvidenceVerified);
        Assert.True(report.InvalidMatrix.Passed);
    }

    [Fact]
    public void CurrentStateKeepsGoal024RecordAfterLaterGoalHandoff()
    {
        var repoRoot = FindRepoRoot();
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(repoRoot, "docs", "CURRENT_GENERATOR_STATE.json")));
        var markdown = File.ReadAllText(Path.Combine(repoRoot, "docs", "CURRENT_GENERATOR_STATE.md"));
        var contextIndex = File.ReadAllText(Path.Combine(repoRoot, "docs", "CONTEXT_INDEX.md"));
        var root = state.RootElement;
        var currentGate = root.GetProperty("gate_status").GetString();
        var currentSliceId = root.GetProperty("last_completed_product_slice_id").GetString();
        var goal024 = root.GetProperty("goal_024_rich_package_assembly_coverage_audit");

        Assert.Equal("goal_024_rich_package_assembly_coverage_audit", goal024.GetProperty("slice_id").GetString());
        Assert.Equal("passed_by_user_prompt_before_modular_contract_policy_adoption", goal024.GetProperty("status").GetString());
        Assert.Contains(
            "capability_bundle_pipeline_inputs_verification passed",
            goal024.GetProperty("summary").GetString());
        Assert.False(string.IsNullOrWhiteSpace(currentGate));
        Assert.False(string.IsNullOrWhiteSpace(currentSliceId));
        Assert.Equal(
            currentSliceId,
            root.GetProperty("last_completed_product_slice").GetProperty("slice_id").GetString());
        Assert.Contains(currentGate, markdown);
        Assert.Contains(currentGate, contextIndex);
    }

    private static void CopyGoal023Artifacts(string sourceRepoRoot, string targetRoot)
    {
        var source = Path.Combine(sourceRepoRoot, ".llmgc", "procedural", "capability-bundle-pipeline-inputs");
        var target = Path.Combine(targetRoot, ".llmgc", "procedural", "capability-bundle-pipeline-inputs");
        Directory.CreateDirectory(target);
        foreach (var file in Directory.EnumerateFiles(source))
        {
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)), overwrite: true);
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
