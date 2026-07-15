using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal157;

[Collection(Goal156Collection.Name)]
public sealed class Goal157WorkspaceUiTests
{
    [Fact]
    public void Behavioral_generated_build_repeat_is_deterministic_across_both_lanes()
    {
        var fixture = Goal157BuildState.Value;

        Assert.True(fixture.First.Passed && fixture.Repeat.Passed);
        Assert.Equal(fixture.First.CompositionPackageSha256, fixture.Repeat.CompositionPackageSha256);
        Assert.Equal(fixture.First.PackageSha256, fixture.Repeat.PackageSha256);
        Assert.Equal(fixture.First.FinalStateHash, fixture.Repeat.FinalStateHash);
        Assert.Equal(fixture.First.AcceptedMechanicsCompatibility?.CompatibilityCompositionPackageSha256,
            fixture.Repeat.AcceptedMechanicsCompatibility?.CompatibilityCompositionPackageSha256);
        Assert.Equal(fixture.First.AcceptedMechanicsCompatibility?.CompatibilityFinalStateHash,
            fixture.Repeat.AcceptedMechanicsCompatibility?.CompatibilityFinalStateHash);
    }

    [Fact]
    public void Behavioral_fresh_reopen_restores_complete_travel_current_without_execution()
    {
        var snapshot = Goal157BuildState.Value.Reopen;

        Assert.Equal("TRAVEL_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.GeneratedWorldActivation?.Passed);
        Assert.True(snapshot.GeneratedWorldActivation?.ReplayEquivalent);
        Assert.True(snapshot.GeneratedWorldActivation?.StateRoundtripPassed);
        Assert.True(snapshot.AcceptedMechanicsCompatibility?.Passed);
    }

    [Fact]
    public void Behavioral_old_goal156_history_without_activation_cannot_claim_build_current()
    {
        using var copy = Goal156TestKit.Copy(Goal157BuildState.Value.Project, "old-history");
        foreach (var path in Goal157TestKit.HistoryFiles(copy.Path))
        {
            var root = JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8))!.AsObject();
            root.Remove("generatedWorldActivation");
            root.Remove("acceptedMechanicsCompatibility");
            File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
                new UTF8Encoding(false));
        }

        var snapshot = Goal157TestKit.OpenTravelWorkspace(copy.Path).Snapshot();

        Assert.NotEqual("BUILD_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.Null(snapshot.GeneratedWorldActivation);
    }

    [Fact]
    public void Behavioral_saved_mechanic_change_yields_last_success_for_generated_activation()
    {
        using var copy = Goal156TestKit.Copy(Goal157BuildState.Value.Project, "authoring-last-success");
        var controller = Goal157TestKit.OpenTravelWorkspace(copy.Path);
        var selected = Goal157TestKit.RemovableSelectedModule(copy.Path);

        controller.SetModuleSelected(selected, false);
        controller.SaveAuthoring();
        var snapshot = controller.Snapshot();

        Assert.Equal("LAST_SUCCESS", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.GeneratedWorldActivation?.Passed);
    }

    [Fact]
    public void Behavioral_source_provenance_failure_preserves_previous_green_package_and_history()
    {
        using var copy = Goal156TestKit.Copy(Goal157BuildState.Value.Project, "provenance-rollback");
        var packagePath = Path.Combine(copy.Path, "package.json");
        var packageBefore = File.ReadAllBytes(packagePath);
        var historyBefore = Goal157TestKit.HistoryFileHashes(copy.Path);
        Goal157TestKit.EditSource(copy.Path, root => root["seed"] = "goal157-provenance-build-failure");

        var failed = Goal157TestKit.OpenTravelWorkspace(copy.Path).BuildAndQualify();

        Assert.False(failed.Passed);
        Assert.Equal("generated_source.validation", failed.FailureStage);
        Assert.True(failed.RollbackApplied);
        Assert.Equal(packageBefore, File.ReadAllBytes(packagePath));
        Goal157TestKit.AssertExistingHistoryUnchanged(copy.Path, historyBefore);
    }

    [Fact]
    public void Behavioral_activation_failure_rolls_back_package_and_preserves_last_green_history()
    {
        using var copy = Goal156TestKit.Copy(Goal157BuildState.Value.Project, "activation-rollback");
        var packagePath = Path.Combine(copy.Path, "package.json");
        var packageBefore = File.ReadAllBytes(packagePath);
        var historyBefore = Goal157TestKit.HistoryFileHashes(copy.Path);
        var controller = Goal157TestKit.OpenTravelWorkspace(copy.Path, runtime: new FaultInjectingRuntime(failMove: true));

        var failed = controller.BuildAndQualify();

        Assert.False(failed.Passed);
        Assert.Equal("generated_activation.runtime", failed.FailureStage);
        Assert.True(failed.RollbackApplied);
        Assert.Equal(packageBefore, File.ReadAllBytes(packagePath));
        Goal157TestKit.AssertExistingHistoryUnchanged(copy.Path, historyBefore);
        Assert.Equal("TRAVEL_CURRENT", Goal157TestKit.OpenTravelWorkspace(copy.Path).Snapshot().GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_generated_card_contains_game_start_move_and_interaction_rows()
    {
        var snapshot = Goal157BuildState.Value.Reopen;

        var card = GameProjectGeneratedWorldSummaryService.FormatCard(
            snapshot.GeneratedWorld!, snapshot.GeneratedWorldActivation);

        Assert.Contains("Игровой старт", card);
        Assert.Contains("Движение", card);
        Assert.Contains("Взаимодействие", card);
        Assert.Contains("Сгенерированное содержимое", card);
        Assert.Contains("Повтор", card);
        Assert.Contains("Сохранение состояния", card);
    }

    [Fact]
    public void Behavioral_generated_card_contains_no_ids_hashes_or_paths()
    {
        var snapshot = Goal157BuildState.Value.Reopen;
        var card = GameProjectGeneratedWorldSummaryService.FormatCard(
            snapshot.GeneratedWorld!, snapshot.GeneratedWorldActivation);

        Assert.DoesNotMatch(new Regex("[0-9a-f]{64}", RegexOptions.IgnoreCase), card);
        Assert.DoesNotContain(".llmgc", card, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("generated/", card, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:\\", card, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(snapshot.GeneratedWorldActivation!.GeneratedStartMapId, card, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_technical_details_expose_source_compatibility_and_player_lanes()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var page = new ProjectsPageControl();
            Goal157TestKit.Bind(page, Goal157BuildState.Value.Reopen);
            var text = Goal157TestKit.Field<TextBox>(page, "_technicalDetailsTextBox").Text;

            Assert.Contains("Source request SHA-256", text);
            Assert.Contains("Compatibility package SHA-256", text);
            Assert.Contains("Compatibility final Runtime state hash", text);
            Assert.Contains("Player composition package SHA-256", text);
            Assert.Contains("Player final Runtime state hash", text);
            Assert.Contains("Generated start map ID", text);
            Assert.Contains("Activation start/move/interact: True/True/True", text);
        });
    }

    [Fact]
    public void Contract_green_history_persists_typed_activation_and_compatibility_sections()
    {
        var history = Goal157BuildState.Value.Repeat.BuildHistoryPath;
        using var document = JsonDocument.Parse(File.ReadAllText(history, Encoding.UTF8));

        var activation = document.RootElement.GetProperty("generatedWorldActivation");
        var compatibility = document.RootElement.GetProperty("acceptedMechanicsCompatibility");
        Assert.True(activation.GetProperty("passed").GetBoolean());
        Assert.True(compatibility.GetProperty("passed").GetBoolean());
        Assert.Equal(3, activation.GetProperty("runtimeFrames").GetArrayLength());
    }
}

internal static partial class Goal157TestKit
{
    public static IReadOnlyList<string> HistoryFiles(string project) => Directory.EnumerateFiles(
            Path.Combine(project, UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot), "*.json")
        .OrderBy(path => path, StringComparer.Ordinal).ToList();

    public static Dictionary<string, string> HistoryFileHashes(string project) => HistoryFiles(project)
        .ToDictionary(path => Path.GetRelativePath(project, path), Goal156TestKit.Hash, StringComparer.Ordinal);

    public static void AssertExistingHistoryUnchanged(string project, IReadOnlyDictionary<string, string> before)
    {
        foreach (var pair in before)
            Assert.Equal(pair.Value, Goal156TestKit.Hash(Path.Combine(project, pair.Key)));
    }

    public static string RemovableSelectedModule(string project)
    {
        var authoring = Goal156TestKit.Authoring(project);
        var selectedIds = authoring.Document.SelectedModuleIds.ToHashSet(StringComparer.Ordinal);
        return authoring.Library.Catalog.Modules
            .Where(module => selectedIds.Contains(module.ModuleId))
            .First(module => !authoring.Library.Catalog.Modules.Any(other =>
                selectedIds.Contains(other.ModuleId)
                && other.Dependencies.Contains(module.ModuleId, StringComparer.Ordinal))).ModuleId;
    }

    public static void Bind(ProjectsPageControl page, UnifiedGameProjectWorkspaceSnapshot snapshot) =>
        page.GetType().GetMethod("BindWorkspace", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, [snapshot]);

    public static T Field<T>(object target, string name) where T : class =>
        (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target)!;

    public static void RunSta(Action action)
    {
        Exception? captured = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { captured = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (captured is not null) throw new Xunit.Sdk.XunitException(captured.ToString());
    }
}
