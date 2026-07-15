using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal158;

[Collection(Goal156Collection.Name)]
public sealed class Goal158HistoryUiRollbackTests
{
    [Fact]
    public void Behavioral_fresh_reopen_restores_travel_current_without_execution()
    {
        var snapshot = Goal157BuildState.Value.Reopen;

        Assert.Equal("TRAVEL_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.GeneratedWorldActivation?.Passed);
        Assert.True(snapshot.GeneratedRegionTravel?.Passed);
        Assert.True(snapshot.GeneratedWorldTravelOverlay?.ControlledDeltaPassed);
        Assert.Equal(snapshot.FinalStateHash, snapshot.GeneratedRegionTravel?.FinalStateHash);
    }

    [Fact]
    public void Behavioral_old_goal157_history_restores_start_current_never_travel_current()
    {
        using var copy = Goal156TestKit.Copy(Goal157BuildState.Value.Project, "goal157-history-v2");
        foreach (var path in Goal157TestKit.HistoryFiles(copy.Path))
        {
            var root = JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8))!.AsObject();
            root["schemaVersion"] = "unified_game_project_build_history_v2";
            root.Remove("generatedWorldTravelOverlay");
            root.Remove("generatedRegionTravel");
            File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
                new UTF8Encoding(false));
        }

        var snapshot = Goal157TestKit.OpenTravelWorkspace(copy.Path).Snapshot();

        Assert.Equal("START_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.GeneratedWorldActivation?.Passed);
        Assert.Null(snapshot.GeneratedRegionTravel);
        Assert.NotEqual("TRAVEL_CURRENT", snapshot.GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_authoring_change_yields_last_success_with_prior_travel_truth()
    {
        using var copy = Goal156TestKit.Copy(Goal157BuildState.Value.Project, "travel-last-success");
        var controller = Goal157TestKit.OpenTravelWorkspace(copy.Path);
        controller.SetModuleSelected(Goal157TestKit.RemovableSelectedModule(copy.Path), false);
        controller.SaveAuthoring();

        var snapshot = controller.Snapshot();

        Assert.Equal("LAST_SUCCESS", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.GeneratedRegionTravel?.Passed);
        Assert.True(snapshot.GeneratedWorldActivation?.Passed);
    }

    [Fact]
    public void Behavioral_travel_runtime_failure_rolls_back_package_history_source_and_rc()
    {
        using var copy = Goal156TestKit.Copy(Goal157BuildState.Value.Project, "travel-runtime-rollback");
        var packagePath = Path.Combine(copy.Path, "package.json");
        var packageBefore = File.ReadAllBytes(packagePath);
        var historyBefore = Goal157TestKit.HistoryFileHashes(copy.Path);
        var generationRoot = Path.Combine(copy.Path, ".llmgc", "generation");
        var sourceBefore = Directory.EnumerateFiles(generationRoot, "*", SearchOption.TopDirectoryOnly)
            .ToDictionary(path => Path.GetFileName(path), Goal158TestKit.FileHash, StringComparer.Ordinal);
        var rcPath = Path.Combine(copy.Path, UnifiedGameProjectWorkspaceVocabulary.ReleaseCandidateRecordRelativePath);
        var rcBefore = File.Exists(rcPath) ? File.ReadAllBytes(rcPath) : null;
        var controller = Goal157TestKit.OpenTravelWorkspace(
            copy.Path,
            runtime: new Goal158FaultRuntime(suppressMapChanged: true));

        var failed = controller.BuildAndQualify();

        Assert.False(failed.Passed);
        Assert.Equal("generated_travel.runtime", failed.FailureStage);
        Assert.True(failed.RollbackApplied);
        Assert.Equal(packageBefore, File.ReadAllBytes(packagePath));
        Goal157TestKit.AssertExistingHistoryUnchanged(copy.Path, historyBefore);
        Assert.All(sourceBefore, pair => Assert.Equal(pair.Value,
            Goal158TestKit.FileHash(Path.Combine(generationRoot, pair.Key))));
        if (rcBefore is not null) Assert.Equal(rcBefore, File.ReadAllBytes(rcPath));
        var staging = Path.Combine(copy.Path, UnifiedGameProjectWorkspaceVocabulary.BuildStagingRelativeRoot);
        Assert.True(!Directory.Exists(staging) || !Directory.EnumerateFileSystemEntries(staging).Any());
    }

    [Fact]
    public void Behavioral_generated_card_contains_origin_travel_visited_and_destination_rows()
    {
        var snapshot = Goal157BuildState.Value.Reopen;

        var card = GameProjectGeneratedWorldSummaryService.FormatCard(
            snapshot.GeneratedWorld!, snapshot.GeneratedWorldActivation, snapshot.GeneratedRegionTravel);

        Assert.Contains("Начальный регион", card);
        Assert.Contains("Взаимодействие в начальном регионе", card);
        Assert.Contains("Переход между регионами", card);
        Assert.Contains("Посещено регионов", card);
        Assert.Contains("Регион назначения", card);
        Assert.Contains("Взаимодействие после перехода", card);
        Assert.Contains("Сгенерированный маршрут проверен", card);
    }

    [Fact]
    public void Behavioral_generated_card_has_no_ids_hashes_or_paths()
    {
        var snapshot = Goal157BuildState.Value.Reopen;
        var card = GameProjectGeneratedWorldSummaryService.FormatCard(
            snapshot.GeneratedWorld!, snapshot.GeneratedWorldActivation, snapshot.GeneratedRegionTravel);

        Assert.DoesNotMatch(new Regex("[0-9a-f]{64}", RegexOptions.IgnoreCase), card);
        Assert.DoesNotContain(".llmgc", card, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("generated/", card, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:\\", card, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(snapshot.GeneratedRegionTravel!.ConnectionIds[0], card, StringComparison.Ordinal);
        Assert.DoesNotContain(snapshot.GeneratedRegionTravel.OriginMapId, card, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_technical_details_contain_overlay_counts_route_ids_and_final_hash()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var page = new ProjectsPageControl();
            var snapshot = Goal157BuildState.Value.Reopen;
            Goal157TestKit.Bind(page, snapshot);
            var text = Goal157TestKit.Field<TextBox>(page, "_technicalDetailsTextBox").Text;

            Assert.Contains("Travel overlay SHA-256", text);
            Assert.Contains("Travel connection/gate/transition counts", text);
            Assert.Contains("Travel origin region/map IDs", text);
            Assert.Contains("Travel destination region/map IDs", text);
            Assert.Contains("Travel route connection IDs", text);
            Assert.Contains("Travel final Runtime state hash", text);
            Assert.Contains(snapshot.GeneratedRegionTravel!.FinalStateHash, text);
        });
    }
}
