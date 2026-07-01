using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignEditValidateApplyLoopTests
{
    [Fact]
    public void SourceManifestAndCatalogLoadGoal074Rows()
    {
        var result = Build();

        Assert.True(result.SourceManifest.Goal074AcceptedByUserHandoff);
        Assert.True(result.SourceManifest.Goal072RemainsHistoricalBlocked);
        Assert.True(result.SourceManifest.Goal031And032RemainProducedForReview);
        Assert.Equal(9, result.SourceManifest.RowCount);
        Assert.Equal(3, result.SourceManifest.FamilyCount);
        Assert.Equal(3, result.SourceManifest.SeedCount);
        Assert.True(result.FieldCatalog.Passed);
        Assert.NotEmpty(result.FieldCatalog.Fields);
        Assert.Equal(6, result.FieldCatalog.FieldCount);
    }

    [Fact]
    public void ManualAndAutoCandidatesCoverEveryFamilyAndEditDomain()
    {
        var result = Build();

        foreach (var familyId in SchemaDrivenCampaignEditVocabulary.FamilyIds)
        {
            var familyCandidates = result.ChangeSetCatalog.Candidates
                .Where(candidate => candidate.FamilyId == familyId)
                .ToList();
            Assert.Contains(familyCandidates, candidate => candidate.CandidateKind == "manual");
            Assert.Contains(familyCandidates, candidate => candidate.CandidateKind == "auto_suggestion");
        }

        var domains = result.ChangeSetCatalog.Candidates
            .Select(candidate => candidate.FieldDomain)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(domain => domain, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            new[]
            {
                "combat_magic_status",
                "gameplay_consequence",
                "living_world_faction",
                "narrative_event_intent",
                "settlement_production",
                "weather_crisis_pressure"
            },
            domains);
    }

    [Fact]
    public void ApplyAndRollbackChangeHashesForEveryRow()
    {
        var result = Build();

        Assert.True(result.ValidationMatrix.Passed);
        Assert.True(result.ApplyRollbackLedger.Passed);
        Assert.Equal(9, result.ApplyRollbackLedger.RowCount);
        Assert.Equal(18, result.ApplyRollbackLedger.AppliedChangeCount);
        Assert.All(result.ApplyRollbackLedger.Rows, row =>
        {
            Assert.True(row.StateChanged);
            Assert.NotEqual(row.BeforeHash, row.AfterHash);
            Assert.Equal(row.BeforeHash, row.RollbackHash);
            Assert.True(row.RollbackRestored);
            Assert.True(row.SaveLoadReplayPassed);
            Assert.Equal(2, row.AppliedChanges.Count);
        });
    }

    [Fact]
    public void PreviewExportRefreshPayloadReferencesChangedRows()
    {
        var result = Build();

        Assert.True(result.DiffMatrix.Passed);
        Assert.True(result.PreviewExportRefreshPayload.Passed);
        Assert.Equal(9, result.PreviewExportRefreshPayload.ChangedRowCount);
        Assert.Equal(
            result.DiffMatrix.Rows.Select(row => row.RowId).OrderBy(rowId => rowId, StringComparer.Ordinal),
            result.PreviewExportRefreshPayload.ChangedRowIds.OrderBy(rowId => rowId, StringComparer.Ordinal));
        Assert.Contains("gameplay_consequence", result.PreviewExportRefreshPayload.ChangedDomains);
        Assert.Contains("weather_crisis_pressure", result.PreviewExportRefreshPayload.ChangedDomains);
    }

    private static SchemaDrivenCampaignEditBuildResult Build() =>
        new SchemaDrivenCampaignEditEvidenceService().Build(ProjectRoot());

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
