using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CreateGameDialog : Form
{
    private string _lastTitleDefault = string.Empty;
    private string _lastPackageIdDefault = string.Empty;
    private readonly GenerationPresetOptionsService _presetOptionsService;
    private bool _seedEdited;
    private bool _updatingSeed;

    public CreateGameDialog()
        : this(new GenerationPresetOptionsService())
    {
    }

    public CreateGameDialog(GenerationPresetOptionsService presetOptionsService)
    {
        _presetOptionsService = presetOptionsService ?? throw new ArgumentNullException(nameof(presetOptionsService));
        InitializeComponent();
        _versionTextBox.Text = "0.1.0";
        BindGenerationChoices();
        WireEvents();
    }

    public IReadOnlyList<string> AvailableGenerationModes => _generationModeComboBox.Items
        .Cast<Choice>().Select(item => item.Value).ToList();

    public IReadOnlyList<string> AvailableGenerationPresets => _generationPresetComboBox.Items
        .Cast<Choice>().Select(item => item.Value).ToList();

    public IReadOnlyList<string> AvailableMechanicsProfiles => _mechanicsProfileComboBox.Items
        .Cast<Choice>().Select(item => item.Value).ToList();

    public CreateGameProjectRequest CreateRequest(string gamesRootPath)
    {
        var folderName = _folderNameTextBox.Text.Trim();
        var title = _titleTextBox.Text.Trim();
        var packageId = _packageIdTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(packageId) && !string.IsNullOrWhiteSpace(folderName))
        {
            packageId = "game/" + NormalizePackageIdPart(folderName);
        }

        return new CreateGameProjectRequest
        {
            GamesRootPath = gamesRootPath,
            FolderName = folderName,
            Title = string.IsNullOrWhiteSpace(title) ? folderName : title,
            PackageId = packageId,
            Version = _versionTextBox.Text.Trim(),
            CreationKind = SelectedValue(_creationKindComboBox),
            GenerationSeed = _seedTextBox.Text.Trim(),
            GenerationMode = SelectedValue(_generationModeComboBox),
            GenerationPresetId = SelectedValue(_generationPresetComboBox),
            MechanicsProfileId = SelectedValue(_mechanicsProfileComboBox)
        };
    }

    private void WireEvents()
    {
        _folderNameTextBox.TextChanged += (_, _) => UpdateDefaults();
        _seedTextBox.TextChanged += (_, _) =>
        {
            if (!_updatingSeed) _seedEdited = true;
        };
        _creationKindComboBox.SelectedIndexChanged += (_, _) => UpdateGenerationControls();
        _createButton.Click += (_, _) => Confirm();
        _cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
    }

    private void UpdateDefaults()
    {
        var folderName = _folderNameTextBox.Text.Trim();
        var titleDefault = folderName;
        var packageIdDefault = string.IsNullOrWhiteSpace(folderName)
            ? string.Empty
            : "game/" + NormalizePackageIdPart(folderName);

        if (string.IsNullOrWhiteSpace(_titleTextBox.Text) || _titleTextBox.Text == _lastTitleDefault)
        {
            _titleTextBox.Text = titleDefault;
        }

        if (string.IsNullOrWhiteSpace(_packageIdTextBox.Text) || _packageIdTextBox.Text == _lastPackageIdDefault)
        {
            _packageIdTextBox.Text = packageIdDefault;
        }

        _lastTitleDefault = titleDefault;
        _lastPackageIdDefault = packageIdDefault;
        if (!_seedEdited)
        {
            _updatingSeed = true;
            try { _seedTextBox.Text = NormalizePackageIdPart(folderName); }
            finally { _updatingSeed = false; }
        }
    }

    private void Confirm()
    {
        var folderName = _folderNameTextBox.Text.Trim();
        var title = _titleTextBox.Text.Trim();
        var version = _versionTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(folderName))
        {
            ShowValidation("Укажи имя папки проекта.");
            return;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            ShowValidation("Укажи название игры.");
            return;
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            ShowValidation("Укажи версию игры.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_packageIdTextBox.Text))
        {
            ShowValidation("Укажи идентификатор пакета.");
            return;
        }
        if (SelectedValue(_creationKindComboBox) == GameProjectCreationKinds.SeededGenerated)
        {
            if (string.IsNullOrWhiteSpace(_seedTextBox.Text))
            {
                ShowValidation("Укажи seed для генерации.");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedValue(_generationModeComboBox)))
            {
                ShowValidation("Выбери режим генерации.");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedValue(_generationPresetComboBox)))
            {
                ShowValidation("Выбери пресет генерации.");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedValue(_mechanicsProfileComboBox)))
            {
                ShowValidation("Выбери профиль механик.");
                return;
            }
        }

        DialogResult = DialogResult.OK;
    }

    private void BindGenerationChoices()
    {
        _creationKindComboBox.Items.AddRange(new object[]
        {
            new Choice(GameProjectCreationKinds.SeededGenerated, "Сгенерированная игра"),
            new Choice(GameProjectCreationKinds.Template, "Пустой шаблон")
        });
        foreach (var mode in ProceduralGameGenerationModes.Supported.OrderBy(value => value, StringComparer.Ordinal))
            _generationModeComboBox.Items.Add(new Choice(mode, ModeTitle(mode)));
        foreach (var preset in _presetOptionsService.GetPresets().OrderBy(item => item.PresetId, StringComparer.Ordinal))
            _generationPresetComboBox.Items.Add(new Choice(preset.PresetId, preset.Title));
        foreach (var profile in GeneratedProjectMechanicsProfiles.Supported.OrderBy(value => value, StringComparer.Ordinal))
            _mechanicsProfileComboBox.Items.Add(new Choice(profile, profile == GeneratedProjectMechanicsProfiles.AllSelectableDefaults
                ? "Все доступные механики" : "Только обязательные"));
        Select(_creationKindComboBox, GameProjectCreationKinds.SeededGenerated);
        Select(_generationModeComboBox, GenerationPresetOptionsService.DefaultMode);
        Select(_generationPresetComboBox, GenerationPresetOptionsService.DefaultPresetId);
        Select(_mechanicsProfileComboBox, GeneratedProjectMechanicsProfiles.AllSelectableDefaults);
        UpdateGenerationControls();
    }

    private void UpdateGenerationControls()
    {
        var enabled = SelectedValue(_creationKindComboBox) == GameProjectCreationKinds.SeededGenerated;
        _seedTextBox.Enabled = enabled;
        _generationModeComboBox.Enabled = enabled;
        _generationPresetComboBox.Enabled = enabled;
        _mechanicsProfileComboBox.Enabled = enabled;
    }

    private void ShowValidation(string message) =>
        MessageBox.Show(this, message, "Новая игра", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private static string ModeTitle(string mode) => mode switch
    {
        ProceduralGameGenerationModes.AuthoredSmallWorld => "Авторский компактный мир",
        ProceduralGameGenerationModes.SemiProceduralRegions => "Полупроцедурные регионы",
        ProceduralGameGenerationModes.FullySeededWorld => "Полностью генерируемый мир",
        _ => mode
    };

    private static void Select(ComboBox comboBox, string value)
    {
        var match = comboBox.Items.Cast<Choice>().Select((item, index) => (item, index))
            .Single(item => item.item.Value == value);
        comboBox.SelectedIndex = match.index;
    }

    private static string SelectedValue(ComboBox comboBox) => (comboBox.SelectedItem as Choice)?.Value ?? string.Empty;

    private static string NormalizePackageIdPart(string value)
    {
        var chars = value.Trim().Select(ch =>
        {
            if (char.IsLetterOrDigit(ch))
            {
                return char.ToLowerInvariant(ch);
            }

            return ch == '_' || ch == '-' ? ch : '-';
        });

        return string.Concat(chars).Trim('-');
    }

    private sealed record Choice(string Value, string Title)
    {
        public override string ToString() => Title;
    }
}
