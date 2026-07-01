using System.Text.Json;
using LLMGameCreator.Application.Design.CombatMagicAbilityBossEncounterMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.CombatMagicAbilityBossEncounterMatrix;

public sealed class CombatMagicSourceLoadingTests
{
    [Fact]
    public void SourceLoaderConsumesGoal060ThroughGoal067EvidenceAndPreflightHandoff()
    {
        var source = new CombatMagicAbilityBossEncounterSourceLoader().Load(CombatMagicProjectRootLocator.ProjectRoot());

        Assert.True(source.Goal067AcceptedByUserHandoff);
        Assert.True(source.Goal060PackageRowsConsumed);
        Assert.True(source.Goal061ReviewPackageRcConsumed);
        Assert.True(source.Goal062SpatialRowsConsumed);
        Assert.True(source.Goal063GameplayRowsConsumed);
        Assert.True(source.Goal064LivingWorldRowsConsumed);
        Assert.True(source.Goal065InterlockedRowsConsumed);
        Assert.True(source.Goal066SettlementRowsConsumed);
        Assert.True(source.Goal067NarrativeRowsConsumed);
        Assert.True(source.Goal067UnityProofConsumed);
        Assert.Equal(9, source.Rows.Count);
        Assert.Equal(3, source.FamilyIds.Count);
        Assert.Equal(3, source.SeedIds.Count);
        Assert.All(source.Rows, row =>
        {
            Assert.StartsWith("Goal060:", row.SourcePackageRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal061:", row.SourceReviewPackageRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal062:", row.SourceSpatialDetailRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal063:", row.SourceGameplayConsequenceRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal064:", row.SourceLivingWorldRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal065:", row.SourceInterlockedGameplayRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal066:", row.SourceSettlementRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal067:", row.SourceNarrativeRowRef, StringComparison.Ordinal);
            Assert.True(row.Goal060PackageValid);
            Assert.True(row.Goal061ReviewPackageRcExists);
            Assert.True(row.Goal062SpatialRowValid);
            Assert.True(row.Goal063GameplayRowValid);
            Assert.True(row.Goal064LivingWorldRowValid);
            Assert.True(row.Goal065InterlockedRowValid);
            Assert.True(row.Goal066SettlementRowValid);
            Assert.True(row.Goal067NarrativeRowValid);
            Assert.True(row.Goal067SaveLoadReplayPassed);
            Assert.NotEmpty(row.InterlockedCombatProgressionLedgerEntryIds);
            Assert.NotEmpty(row.InterlockedStatusLedgerEntryIds);
            Assert.NotEmpty(row.SettlementLedgerEntryIds);
            Assert.NotEmpty(row.NarrativeDeltaIds);
        });
    }
}

public sealed class CombatMagicMatrixTests
{
    [Fact]
    public void BuildCreatesNineStateChangingCombatMagicRowsWithoutFinalProse()
    {
        var result = new CombatMagicAbilityBossEncounterEvidenceService().Build(CombatMagicProjectRootLocator.ProjectRoot());

        Assert.True(result.SourceManifest.Goal067AcceptedByUserHandoff);
        Assert.True(result.SourceManifest.Goal060PackageRowsConsumed);
        Assert.True(result.SourceManifest.Goal061ReviewPackageRcConsumed);
        Assert.True(result.SourceManifest.Goal062SpatialRowsConsumed);
        Assert.True(result.SourceManifest.Goal063GameplayRowsConsumed);
        Assert.True(result.SourceManifest.Goal064LivingWorldRowsConsumed);
        Assert.True(result.SourceManifest.Goal065InterlockedRowsConsumed);
        Assert.True(result.SourceManifest.Goal066SettlementRowsConsumed);
        Assert.True(result.SourceManifest.Goal067NarrativeRowsConsumed);
        Assert.True(result.AbilityTraitCatalog.Passed);
        Assert.True(result.StatusEffectCatalog.Passed);
        Assert.True(result.BossPhaseCatalog.Passed);
        Assert.True(result.RowMatrix.Passed);
        Assert.Equal(9, result.RowMatrix.RowCount);
        Assert.Equal(9, result.RowMatrix.StateChangingRowCount);
        Assert.Equal(3, result.RowMatrix.FamilyCount);
        Assert.Equal(3, result.RowMatrix.SeedCount);
        Assert.Equal(9, result.RowMatrix.DistinctRowHashCount);
        Assert.True(result.RowMatrix.BossEliteRowCount >= 3);
        Assert.True(result.RowMatrix.MagicStatusRowCount >= 3);
        Assert.True(result.RowMatrix.ResourceGearCraftingRowCount >= 3);
        Assert.All(result.Rows, row =>
        {
            Assert.True(row.StateChanging);
            Assert.True(row.NoFinalProse);
            Assert.NotEqual(row.BeforeState.StateHash, row.AfterState.StateHash);
            Assert.NotEmpty(row.ActiveAbilities);
            Assert.NotEmpty(row.PassiveTraits);
            Assert.NotEmpty(row.StatusEffects);
            Assert.NotEmpty(row.DamageEffectPackets);
            Assert.NotEmpty(row.CooldownCosts);
            Assert.NotEmpty(row.ResistanceWeaknesses);
            Assert.NotEmpty(row.BossPhases);
            Assert.True(row.RoundPhaseResults.Count >= 2);
            Assert.NotEmpty(row.CounterplayRecords);
            Assert.NotEmpty(row.LootProgressionRecords);
            Assert.NotEmpty(row.NonCombatConsequences);
            Assert.True(row.ChangedCategories.Count >= 3);
            Assert.True(row.StateDeltas.Count >= 5);
            Assert.NotEmpty(row.SourceNarrativeRowRef);
            Assert.DoesNotContain("lineText", CombatMagicAbilityBossEncounterHash.Serialize(row), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("finalDialogue", CombatMagicAbilityBossEncounterHash.Serialize(row), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("generatedLua", CombatMagicAbilityBossEncounterHash.Serialize(row), StringComparison.OrdinalIgnoreCase);
        });
    }
}

public sealed class CombatMagicLedgersReplayAndUnityPlanTests
{
    [Fact]
    public void LedgersReplayVarianceAndUnityCommandPlanPassForEveryFamilySeedRow()
    {
        var result = new CombatMagicAbilityBossEncounterEvidenceService().Build(CombatMagicProjectRootLocator.ProjectRoot());

        Assert.True(result.ProgressionLootLedger.Passed);
        Assert.True(result.CounterplayLedger.Passed);
        Assert.True(result.SaveLoadReplayProof.Passed);
        Assert.Equal(9, result.SaveLoadReplayProof.StateChangedRowCount);
        Assert.Equal(9, result.SaveLoadReplayProof.SaveLoadPassedRowCount);
        Assert.Equal(9, result.SaveLoadReplayProof.ReplayPassedRowCount);
        Assert.True(result.Report.MeaningfulVariancePassed);
        Assert.True(result.PreviewExportPayload.Passed);
        Assert.True(result.UnityCommandPlan.Passed);
        Assert.False(result.UnityCommandPlan.Accepted);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        foreach (var marker in CombatMagicAbilityBossEncounterValidator.RequiredUnityMarkers())
        {
            Assert.Contains(marker, result.UnityCommandPlan.ExpectedPlayerMarkers);
        }

        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.Contains("combat_magic_row_loaded=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("combat_magic_family=" + row.FamilyId, row.ExpectedPlayerMarkers);
            Assert.Contains("combat_magic_seed=" + row.SeedId, row.ExpectedPlayerMarkers);
            Assert.Contains("combat_magic_ability_resolved=" + row.AbilityUseId, row.ExpectedPlayerMarkers);
            Assert.Contains("combat_magic_status_delta=" + row.StatusApplicationId, row.ExpectedPlayerMarkers);
            Assert.Contains("combat_magic_progression_delta=" + row.ProgressionId, row.ExpectedPlayerMarkers);
            Assert.Contains("combat_magic_row_completed=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.All(row.RoundStepIds, step => Assert.Contains("combat_magic_round_step=" + step, row.ExpectedPlayerMarkers));
        });
    }
}

public sealed class CombatMagicInvalidMatrixTests
{
    [Fact]
    public void InvalidFakeLeakMatrixCoversRequiredCases()
    {
        var result = new CombatMagicAbilityBossEncounterEvidenceService().Build(CombatMagicProjectRootLocator.ProjectRoot());
        var matrix = result.InvalidMatrix;
        var ids = matrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        Assert.True(result.Report.NoFinalProseLeakage);
        foreach (var required in CombatMagicAbilityBossEncounterVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, ids);
        }

        Assert.All(matrix.Scenarios, scenario =>
        {
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.Diagnostics);
            Assert.All(scenario.Diagnostics, diagnostic => Assert.StartsWith("goal068.", diagnostic.Code, StringComparison.Ordinal));
        });
    }
}

public sealed class CombatMagicEvidenceWriteTests
{
    [Fact]
    public async Task WriteAsyncEmitsDeterministicArtifactsRowsAndStagingCommandPlan()
    {
        var service = new CombatMagicAbilityBossEncounterEvidenceService();
        var result = service.Build(CombatMagicProjectRootLocator.ProjectRoot());
        var second = service.Build(CombatMagicProjectRootLocator.ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal068Write", Guid.NewGuid().ToString("N"));

        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            Assert.Equal(result.Report.RowMatrixHash, second.Report.RowMatrixHash);
            Assert.Equal(result.Report.ProgressionLootLedgerHash, second.Report.ProgressionLootLedgerHash);
            Assert.Equal(result.Report.CounterplayLedgerHash, second.Report.CounterplayLedgerHash);
            Assert.Equal(result.Report.SaveLoadReplayProofHash, second.Report.SaveLoadReplayProofHash);
            Assert.Equal(result.Report.InvalidMatrixHash, second.Report.InvalidMatrixHash);
            Assert.DoesNotContain(result.Report.Diagnostics, item => item.Severity == "error" && !item.Code.StartsWith("goal068.unity.", StringComparison.Ordinal));

            foreach (var fileName in RequiredJsonFiles())
            {
                var path = Path.Combine(write.OutputDirectoryPath, fileName);
                Assert.True(File.Exists(path), "Missing artifact: " + fileName);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            foreach (var row in result.Rows)
            {
                var path = Path.Combine(write.OutputDirectoryPath, CombatMagicAbilityBossEncounterEvidenceService.RowsDirectoryName, CombatMagicAbilityBossEncounterEvidenceService.RowFileName(row));
                Assert.True(File.Exists(path), "Missing row artifact: " + path);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            Assert.True(File.Exists(write.ReportMarkdownPath));
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, CombatMagicAbilityBossEncounterVocabulary.StagingRoot, CombatMagicAbilityBossEncounterVocabulary.UnityCombatMagicCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private static IReadOnlyList<string> RequiredJsonFiles() =>
    [
        CombatMagicAbilityBossEncounterEvidenceService.SourceManifestJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.AbilityTraitCatalogJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.StatusEffectCatalogJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.BossEncounterPhaseCatalogJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.RowMatrixJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.SaveLoadReplayProofJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.ProgressionLootLedgerJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.CounterplayLedgerJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.PreviewExportPayloadJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.UnityCommandPlanJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.UnityProofSummaryJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.InvalidDiagnosticsMatrixJsonFileName,
        CombatMagicAbilityBossEncounterEvidenceService.ArtifactScopeReportJsonFileName
    ];
}

internal static class CombatMagicProjectRootLocator
{
    public static string ProjectRoot()
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
