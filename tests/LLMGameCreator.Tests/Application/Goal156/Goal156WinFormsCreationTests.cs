using System.Reflection;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal156;

[Collection(Goal156Collection.Name)]
public sealed class Goal156WinFormsCreationTests
{
    [Fact]
    public void Behavioral_new_game_dialog_choices_are_derived_from_current_services_and_vocabularies()
    {
        using var dialog = new CreateGameDialog(new GenerationPresetOptionsService());

        Assert.Equal(ProceduralGameGenerationModes.Supported.OrderBy(value => value, StringComparer.Ordinal),
            dialog.AvailableGenerationModes);
        Assert.Equal(new GenerationPresetOptionsService().GetPresets().Select(item => item.PresetId)
                .OrderBy(value => value, StringComparer.Ordinal),
            dialog.AvailableGenerationPresets);
        Assert.Equal(GeneratedProjectMechanicsProfiles.Supported.OrderBy(value => value, StringComparer.Ordinal),
            dialog.AvailableMechanicsProfiles);
    }

    [Fact]
    public void Behavioral_new_game_dialog_defaults_to_seeded_generated_and_tracks_folder_seed()
    {
        using var dialog = new CreateGameDialog();
        Field<TextBox>(dialog, "_folderNameTextBox").Text = "My Generated World";

        var request = dialog.CreateRequest("C:\\Games");

        Assert.Equal(GameProjectCreationKinds.SeededGenerated, request.CreationKind);
        Assert.Equal("my-generated-world", request.GenerationSeed);
        Assert.Equal(GenerationPresetOptionsService.DefaultMode, request.GenerationMode);
        Assert.Equal(GenerationPresetOptionsService.DefaultPresetId, request.GenerationPresetId);
        Assert.Equal(GeneratedProjectMechanicsProfiles.AllSelectableDefaults, request.MechanicsProfileId);
    }

    [Fact]
    public void Behavioral_manually_edited_seed_is_not_overwritten_by_later_folder_changes()
    {
        using var dialog = new CreateGameDialog();
        var folder = Field<TextBox>(dialog, "_folderNameTextBox");
        var seed = Field<TextBox>(dialog, "_seedTextBox");
        folder.Text = "first-world";
        seed.Text = "operator-seed";
        folder.Text = "second-world";

        Assert.Equal("operator-seed", dialog.CreateRequest("C:\\Games").GenerationSeed);
    }

    [Fact]
    public void Behavioral_legacy_template_choice_disables_generation_controls_and_keeps_request_shape()
    {
        using var dialog = new CreateGameDialog();
        Field<TextBox>(dialog, "_folderNameTextBox").Text = "legacy-world";
        var kind = Field<ComboBox>(dialog, "_creationKindComboBox");
        kind.SelectedIndex = 1;

        var request = dialog.CreateRequest("C:\\Games");

        Assert.Equal(GameProjectCreationKinds.Template, request.CreationKind);
        Assert.False(Field<TextBox>(dialog, "_seedTextBox").Enabled);
        Assert.False(Field<ComboBox>(dialog, "_generationModeComboBox").Enabled);
        Assert.False(Field<ComboBox>(dialog, "_generationPresetComboBox").Enabled);
        Assert.False(Field<ComboBox>(dialog, "_mechanicsProfileComboBox").Enabled);
    }

    [Fact]
    public void Behavioral_generated_world_card_is_concise_human_readable_and_excludes_technical_hashes()
    {
        var source = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path);
        var summary = new GameProjectGeneratedWorldSummaryService().ProjectSource(source)!;

        var card = GameProjectGeneratedWorldSummaryService.FormatCard(summary);

        Assert.StartsWith("Сгенерированный мир", card, StringComparison.Ordinal);
        Assert.Contains(summary.Seed, card, StringComparison.Ordinal);
        Assert.Contains(summary.RegionCount.ToString(), card, StringComparison.Ordinal);
        Assert.DoesNotContain(summary.PlanSha256, card, StringComparison.Ordinal);
        Assert.DoesNotContain(summary.GeneratedBasePackageSha256, card, StringComparison.Ordinal);
    }

    private static T Field<T>(CreateGameDialog dialog, string name) where T : class =>
        Assert.IsType<T>(typeof(CreateGameDialog).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(dialog));
}
