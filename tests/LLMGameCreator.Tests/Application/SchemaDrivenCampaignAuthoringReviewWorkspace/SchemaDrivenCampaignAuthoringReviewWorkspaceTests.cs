using System.Runtime.ExceptionServices;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.SchemaDrivenCampaignAuthoringReviewWorkspace;

public sealed class SchemaDrivenCampaignAuthoringReviewWorkspaceTests
{
    [Fact]
    public void SourceWorkspaceLoadsNineRowsAndRequiredSources()
    {
        var source = new SchemaDrivenCampaignWorkspaceSourceLoader().Load(ProjectRoot());

        Assert.True(source.Goal073AcceptedByUserHandoff);
        Assert.True(source.Goal072RemainsHistoricalBlocked);
        Assert.True(source.Goal031And032RemainProducedForReview);
        Assert.Equal(9, source.Rows.Count);
        Assert.Equal(SchemaDrivenCampaignWorkspaceVocabulary.FamilyIds, source.FamilyIds);
        Assert.Equal(SchemaDrivenCampaignWorkspaceVocabulary.SeedIds, source.SeedIds);
        Assert.All(source.SourceArtifactRefs, artifact =>
        {
            Assert.True(artifact.Exists, artifact.ArtifactRelativePath);
            Assert.False(Path.IsPathFullyQualified(artifact.ArtifactRelativePath));
        });
    }

    [Fact]
    public void DynamicSchemaHasRequiredPanelGroups()
    {
        var result = Build();
        var groupIds = result.DynamicSchema.Groups.Select(group => group.GroupId).ToArray();

        Assert.True(result.DynamicSchema.Passed);
        Assert.Equal(SchemaDrivenCampaignWorkspaceVocabulary.RequiredSchemaGroupIds.Count, groupIds.Length);
        Assert.All(SchemaDrivenCampaignWorkspaceVocabulary.RequiredSchemaGroupIds, required =>
            Assert.Contains(required, groupIds));
        Assert.Equal(13, result.DynamicSchema.Groups.Count);
        Assert.True(result.Report.Goal073AcceptedByUserHandoff);
        Assert.True(result.Report.Goal072PreservedAsBlocked);
    }

    [Fact]
    public void UiBindingContractReferencesValidSchemaGroupsAndFields()
    {
        var result = Build();
        var diagnostics = new SchemaDrivenCampaignWorkspaceValidator()
            .ValidateUiBinding(result.DynamicSchema, result.UiBindingContract);

        Assert.DoesNotContain(diagnostics, item => item.Severity == "error");
        Assert.True(result.UiBindingContract.Passed);
        Assert.Equal(result.DynamicSchema.Groups.Count, result.UiBindingContract.GroupBindings.Count);
        Assert.Contains("stateChanging", result.UiBindingContract.RowSelector.RequiredColumns);
    }

    [Fact]
    public void ProvenanceLedgerDistinguishesManualAutoQuarantinedAccepted()
    {
        var result = Build();

        Assert.True(result.ProvenanceLedger.Passed);
        Assert.Contains("manual", result.ProvenanceLedger.Categories);
        Assert.Contains("auto", result.ProvenanceLedger.Categories);
        Assert.Contains("quarantined", result.ProvenanceLedger.Categories);
        Assert.Contains("accepted", result.ProvenanceLedger.Categories);
        Assert.Contains(result.ProvenanceLedger.Entries, entry =>
            entry.SourceGoal == "Goal072" && entry.Category == "quarantined");
        Assert.All(
            result.ProvenanceLedger.Entries.Where(entry => entry.Category == "accepted"),
            entry => Assert.True(entry.HasReviewProvenance));
    }

    [Fact]
    public void AuthoringActionPlanIsDeterministic()
    {
        var first = Build();
        var second = Build();

        Assert.True(first.ActionPlan.Passed);
        Assert.Equal(first.ActionPlan.PlanHash, second.ActionPlan.PlanHash);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(
            first.ActionPlan.Items.Select(item => item.ActionId),
            second.ActionPlan.Items.Select(item => item.ActionId));
    }

    [Fact]
    public void InvalidMatrixMatchesExpectations()
    {
        var result = Build();

        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(
            SchemaDrivenCampaignWorkspaceVocabulary.RequiredInvalidScenarioIds.Count,
            result.InvalidMatrix.ScenarioCount);
        Assert.All(SchemaDrivenCampaignWorkspaceVocabulary.RequiredInvalidScenarioIds, scenarioId =>
            Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == scenarioId));
        Assert.All(result.InvalidMatrix.Scenarios, scenario =>
        {
            Assert.Equal("rejected", scenario.ActualStatus);
            Assert.Contains(scenario.Diagnostics, diagnostic => diagnostic.Code == "goal074.invalid." + scenario.ScenarioId);
        });
    }

    [Fact]
    public async Task EvidenceWriterCreatesAllRequiredFiles()
    {
        var service = new SchemaDrivenCampaignWorkspaceEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());

        Assert.True(Directory.Exists(write.OutputDirectoryPath));
        Assert.Equal("GREEN", write.Result.Report.ImplementationStatus);
        foreach (var fileName in RequiredArtifactNames())
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public void WinFormsUserControlsCanBeConstructedAndBound()
    {
        var result = Build();
        RunSta(() =>
        {
            using var page = new CampaignAuthoringReviewWorkspacePageControl();
            using var rows = new CampaignRowSelectorControl();
            using var schema = new CampaignSchemaGroupControl();
            using var diagnostics = new CampaignDiagnosticsControl();
            using var provenance = new CampaignProvenanceControl();
            using var actionPlan = new CampaignActionPlanControl();
            using var quality = new CampaignQualityGateControl();

            page.Bind(result);
            rows.Bind(result.RowSelector, result.UiBindingContract);
            schema.Bind(result.DynamicSchema, result.UiBindingContract);
            diagnostics.Bind(result.ValidationDashboard, result.Report);
            provenance.Bind(result.ProvenanceLedger);
            actionPlan.Bind(result.ActionPlan);
            quality.Bind(result.QualityGateScan, result.WinFormsControlInventory);
        });
    }

    private static CampaignWorkspaceBuildResult Build() =>
        new SchemaDrivenCampaignWorkspaceEvidenceService().Build(ProjectRoot());

    private static IReadOnlyList<string> RequiredArtifactNames() =>
    [
        SchemaDrivenCampaignWorkspaceEvidenceService.SourceManifestFileName,
        SchemaDrivenCampaignWorkspaceEvidenceService.RowSelectorFileName,
        SchemaDrivenCampaignWorkspaceEvidenceService.DynamicAuthoringSchemaFileName,
        SchemaDrivenCampaignWorkspaceEvidenceService.UiBindingContractFileName,
        SchemaDrivenCampaignWorkspaceEvidenceService.ValidationDashboardFileName,
        SchemaDrivenCampaignWorkspaceEvidenceService.ReviewProvenanceLedgerFileName,
        SchemaDrivenCampaignWorkspaceEvidenceService.AuthoringActionPlanFileName,
        SchemaDrivenCampaignWorkspaceEvidenceService.QualityGateScanFileName,
        SchemaDrivenCampaignWorkspaceEvidenceService.WinFormsControlInventoryFileName,
        SchemaDrivenCampaignWorkspaceEvidenceService.InvalidDiagnosticsMatrixFileName,
        SchemaDrivenCampaignWorkspaceEvidenceService.ReportMarkdownFileName,
        SchemaDrivenCampaignWorkspaceEvidenceService.ArtifactScopeReportFileName
    ];

    private static void RunSta(Action action)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                caught = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (caught is not null)
        {
            ExceptionDispatchInfo.Capture(caught).Throw();
        }
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
