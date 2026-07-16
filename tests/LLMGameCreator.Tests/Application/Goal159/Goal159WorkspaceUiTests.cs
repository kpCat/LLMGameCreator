using System.Text.RegularExpressions;
using System.Windows.Forms;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal159;

[Collection(Goal156Collection.Name)]
public sealed class Goal159WorkspaceUiTests
{
    [Fact]
    public async Task Behavioral_controller_exposes_regeneration_only_for_generated_project()
    {
        using var generated = Goal156TestKit.Copy(Goal157PortableState.Value.Project, "goal159-ui-generated");
        var generatedSnapshot = Goal159TestKit.CreateBundle(generated.Path).Controller.Snapshot();
        using var scope = Goal156TestKit.Scope("goal159-ui-template");
        var template = await scope.Service.CreateAsync(Goal156TestKit.TemplateRequest(scope.Root, "template"),
            CancellationToken.None);
        var templateSnapshot = Goal159TestKit.CreateBundle(template.FolderPath).Controller.Snapshot();

        Assert.True(generatedSnapshot.CanRegenerateGeneratedWorld);
        Assert.False(templateSnapshot.CanRegenerateGeneratedWorld);
    }

    [Fact]
    public void Behavioral_dialog_preloads_current_request()
    {
        Goal157TestKit.RunSta(() =>
        {
            var snapshot = Goal159SuccessState.Value.Result.AuthoritativeSnapshot!;
            var current = Assert.IsType<SeededGeneratedProjectGenerationRequest>(
                snapshot.GeneratedWorldGenerationRequest);
            using var dialog = new RegenerateGeneratedWorldDialog(
                current, snapshot.GeneratedWorldResolvedOptions!);

            Assert.Equal(current.Seed, dialog.GenerationRequest.Seed);
            Assert.Equal(current.Mode, dialog.GenerationRequest.Mode);
            Assert.Equal(current.PresetId, dialog.GenerationRequest.PresetId);
            Assert.Equal(current.CompactStyleHintIds,
                dialog.GenerationRequest.CompactStyleHintIds);
            Assert.Equal(current.SelectedVariantIds,
                dialog.GenerationRequest.SelectedVariantIds);
        });
    }

    [Fact]
    public void Behavioral_dialog_uses_current_seed_and_disables_semantic_no_op()
    {
        Goal157TestKit.RunSta(() =>
        {
            var snapshot = Goal159SuccessState.Value.Result.AuthoritativeSnapshot!;
            using var dialog = new RegenerateGeneratedWorldDialog(
                snapshot.GeneratedWorldGenerationRequest!, snapshot.GeneratedWorldResolvedOptions!);
            var apply = Goal157TestKit.Field<Button>(dialog, "_applyButton");

            Assert.Equal(snapshot.GeneratedWorldResolvedOptions!.Seed, dialog.GenerationRequest.Seed);
            Assert.True(dialog.IsSemanticNoOp);
            Assert.False(apply.Enabled);
        });
    }

    [Fact]
    public void Behavioral_dialog_mode_and_preset_choices_are_data_derived()
    {
        Goal157TestKit.RunSta(() =>
        {
            var snapshot = Goal159SuccessState.Value.Result.AuthoritativeSnapshot!;
            using var dialog = new RegenerateGeneratedWorldDialog(
                snapshot.GeneratedWorldGenerationRequest!, snapshot.GeneratedWorldResolvedOptions!);

            Assert.Equal(ProceduralGameGenerationModes.Supported.OrderBy(value => value, StringComparer.Ordinal),
                dialog.AvailableModes);
            Assert.Equal(new GenerationPresetOptionsService().GetPresets()
                    .Select(item => item.PresetId).OrderBy(value => value, StringComparer.Ordinal),
                dialog.AvailablePresets);
        });
    }

    [Fact]
    public void Behavioral_advanced_id_newline_normalization_is_deterministic()
    {
        Goal157TestKit.RunSta(() =>
        {
            var snapshot = Goal159SuccessState.Value.Result.AuthoritativeSnapshot!;
            using var dialog = new RegenerateGeneratedWorldDialog(
                snapshot.GeneratedWorldGenerationRequest!, snapshot.GeneratedWorldResolvedOptions!);
            Goal157TestKit.Field<TextBox>(dialog, "_styleOverridesTextBox").Text =
                " tone/mysterious\r\ntheme/trade\ntone/mysterious\r";

            Assert.Equal(["theme/trade", "tone/mysterious"],
                dialog.GenerationRequest.CompactStyleHintIds);
        });
    }

    [Fact]
    public void Behavioral_successful_apply_refreshes_same_project_and_record()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.Equal(fixture.Project.Path, fixture.Result.AuthoritativeSnapshot?.ProjectFolder);
        Assert.Equal(fixture.Record.AttemptId,
            fixture.Result.AuthoritativeSnapshot?.LastSuccessfulRegeneration?.AttemptId);
        Assert.Equal("TRAVEL_CURRENT", fixture.Result.AuthoritativeSnapshot?.GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_result_card_shows_old_to_new_counts_and_preserved_mechanics()
    {
        var card = ResultCard();

        Assert.Contains("Последняя перегенерация мира", card);
        Assert.Contains(" → ", card);
        Assert.Contains("Добавлено / удалено / изменено", card);
        Assert.Contains("Принятые механики    сохранены", card);
        Assert.Contains("Маршрут между регионами    проверен", card);
    }

    [Fact]
    public void Behavioral_result_card_shows_standalone_pending_without_ids_hashes_or_paths()
    {
        var card = ResultCard();

        Assert.Contains("Windows standalone    ожидает подтверждения", card);
        Assert.DoesNotMatch(new Regex("[0-9a-f]{64}", RegexOptions.IgnoreCase), card);
        Assert.DoesNotContain(".llmgc", card, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:\\", card, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Attempt ID", card, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_technical_details_show_regeneration_transaction_and_diff_hashes()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var page = new ProjectsPageControl();
            Goal157TestKit.Bind(page, Goal159SuccessState.Value.Result.AuthoritativeSnapshot!);
            var technical = Goal157TestKit.Field<TextBox>(page, "_technicalDetailsTextBox").Text;

            Assert.Contains("Seed regeneration", technical);
            Assert.Contains("Old/new source SHA-256", technical);
            Assert.Contains("Old/new plan SHA-256", technical);
            Assert.Contains("Old/new overlay SHA-256", technical);
            Assert.Contains("last-successful-regeneration.json", technical);
        });
    }

    [Fact]
    public void Behavioral_failed_attempt_does_not_replace_last_successful_result()
    {
        using var copy = Goal156TestKit.Copy(Goal159SuccessState.Value.Project, "goal159-failed-last-success");
        var bundle = Goal159TestKit.CreateBundle(copy.Path);
        var before = bundle.Controller.Snapshot().LastSuccessfulRegeneration;
        var request = bundle.Controller.CreateGeneratedWorldRegenerationRequest(
            Goal159TestKit.ChangedRequest(bundle.Controller.Snapshot(), "goal159-failed-attempt")) with
        {
            ExpectedSourceRecordSha256 = new string('0', 64)
        };

        var failed = bundle.Controller.PreviewGeneratedWorldRegeneration(request);
        var after = bundle.Controller.Snapshot().LastSuccessfulRegeneration;

        Assert.Equal("FAILED", failed.Status);
        Assert.Equal(before?.AttemptId, after?.AttemptId);
        Assert.Equal(before?.NewSourceRecordSha256, after?.NewSourceRecordSha256);
    }

    private static string ResultCard()
    {
        var result = string.Empty;
        Goal157TestKit.RunSta(() =>
        {
            using var page = new ProjectsPageControl();
            Goal157TestKit.Bind(page, Goal159SuccessState.Value.Result.AuthoritativeSnapshot!);
            result = Goal157TestKit.Field<Label>(page, "_generatedWorldCardLabel").Text;
        });
        return result;
    }
}
