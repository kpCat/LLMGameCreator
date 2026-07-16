using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class RegenerateGeneratedWorldDialog : Form
{
    private readonly GenerationPresetOptionsService _optionsService;
    private readonly SeededGeneratedProjectResolvedOptions _currentResolved;

    public RegenerateGeneratedWorldDialog()
        : this(new SeededGeneratedProjectGenerationRequest(),
            new GenerationPresetOptionsService().Resolve(new SeededGeneratedProjectGenerationRequest()),
            new GenerationPresetOptionsService())
    {
    }

    public RegenerateGeneratedWorldDialog(
        SeededGeneratedProjectGenerationRequest currentRequest,
        SeededGeneratedProjectResolvedOptions currentResolved,
        GenerationPresetOptionsService? optionsService = null)
    {
        ArgumentNullException.ThrowIfNull(currentRequest);
        _currentResolved = currentResolved ?? throw new ArgumentNullException(nameof(currentResolved));
        _optionsService = optionsService ?? new GenerationPresetOptionsService();
        InitializeComponent();
        BindChoices(currentRequest);
        WireEvents();
        RefreshValidation();
    }

    public SeededGeneratedProjectGenerationRequest GenerationRequest => new()
    {
        Seed = _seedTextBox.Text.Trim(),
        Mode = SelectedValue(_modeComboBox),
        PresetId = SelectedValue(_presetComboBox),
        CompactStyleHintIds = ParseOverrides(_styleOverridesTextBox.Text),
        SelectedVariantIds = ParseOverrides(_variantOverridesTextBox.Text)
    };

    public IReadOnlyList<string> AvailableModes => _modeComboBox.Items.Cast<Choice>()
        .Select(choice => choice.Value).ToList();

    public IReadOnlyList<string> AvailablePresets => _presetComboBox.Items.Cast<Choice>()
        .Select(choice => choice.Value).ToList();

    public bool IsSemanticNoOp => SemanticEquals(_optionsService.Resolve(GenerationRequest), _currentResolved);

    private void BindChoices(SeededGeneratedProjectGenerationRequest request)
    {
        foreach (var mode in ProceduralGameGenerationModes.Supported.OrderBy(value => value, StringComparer.Ordinal))
            _modeComboBox.Items.Add(new Choice(mode, ModeTitle(mode)));
        foreach (var preset in _optionsService.GetPresets().OrderBy(value => value.PresetId, StringComparer.Ordinal))
            _presetComboBox.Items.Add(new Choice(preset.PresetId, preset.Title));
        _seedTextBox.Text = request.Seed;
        Select(_modeComboBox, string.IsNullOrWhiteSpace(request.Mode) ? _currentResolved.Mode : request.Mode);
        Select(_presetComboBox, string.IsNullOrWhiteSpace(request.PresetId) ? _currentResolved.PresetId : request.PresetId);
        _styleOverridesTextBox.Text = JoinOverrides(request.CompactStyleHintIds);
        _variantOverridesTextBox.Text = JoinOverrides(request.SelectedVariantIds);
    }

    private void WireEvents()
    {
        _seedTextBox.TextChanged += (_, _) => RefreshValidation();
        _modeComboBox.SelectedIndexChanged += (_, _) => RefreshValidation();
        _presetComboBox.SelectedIndexChanged += (_, _) => RefreshValidation();
        _styleOverridesTextBox.TextChanged += (_, _) => RefreshValidation();
        _variantOverridesTextBox.TextChanged += (_, _) => RefreshValidation();
        _applyButton.Click += (_, _) => Confirm();
        _cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
    }

    private void RefreshValidation()
    {
        var error = ValidationMessage();
        _validationLabel.Text = error ?? (IsSemanticNoOp
            ? "Изменений нет: текущий мир уже соответствует этим параметрам."
            : "Будет собран отдельный кандидат; проект изменится только после успешной проверки.");
        _applyButton.Enabled = error is null && !IsSemanticNoOp;
    }

    private string? ValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(_seedTextBox.Text)) return "Укажите seed. Случайный seed не создаётся.";
        if (string.IsNullOrWhiteSpace(SelectedValue(_modeComboBox))) return "Выберите режим генерации.";
        if (string.IsNullOrWhiteSpace(SelectedValue(_presetComboBox))) return "Выберите пресет генерации.";
        return null;
    }

    private void Confirm()
    {
        var validation = ValidationMessage();
        if (validation is not null)
        {
            MessageBox.Show(this, validation, "Перегенерация мира", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }
        if (IsSemanticNoOp) return;
        DialogResult = DialogResult.OK;
    }

    private static IReadOnlyList<string> ParseOverrides(string value) => value
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Replace('\r', '\n')
        .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(item => !string.IsNullOrWhiteSpace(item))
        .Distinct(StringComparer.Ordinal)
        .OrderBy(item => item, StringComparer.Ordinal)
        .ToList();

    private static string JoinOverrides(IReadOnlyList<string> values) =>
        string.Join(Environment.NewLine, values.OrderBy(value => value, StringComparer.Ordinal));

    private static bool SemanticEquals(
        SeededGeneratedProjectResolvedOptions left,
        SeededGeneratedProjectResolvedOptions right) =>
        string.Equals(left.Seed, right.Seed, StringComparison.Ordinal)
        && string.Equals(left.Mode, right.Mode, StringComparison.Ordinal)
        && string.Equals(left.PresetId, right.PresetId, StringComparison.Ordinal)
        && left.CompactStyleHintIds.SequenceEqual(right.CompactStyleHintIds, StringComparer.Ordinal)
        && left.SelectedVariantIds.SequenceEqual(right.SelectedVariantIds, StringComparer.Ordinal);

    private static string SelectedValue(ComboBox comboBox) =>
        (comboBox.SelectedItem as Choice)?.Value ?? string.Empty;

    private static void Select(ComboBox comboBox, string value)
    {
        var match = comboBox.Items.Cast<Choice>().Select((choice, index) => (choice, index))
            .FirstOrDefault(item => string.Equals(item.choice.Value, value, StringComparison.Ordinal));
        comboBox.SelectedIndex = match.choice is null ? 0 : match.index;
    }

    private static string ModeTitle(string mode) => mode switch
    {
        ProceduralGameGenerationModes.AuthoredSmallWorld => "Авторский компактный мир",
        ProceduralGameGenerationModes.SemiProceduralRegions => "Полупроцедурные регионы",
        ProceduralGameGenerationModes.FullySeededWorld => "Полностью генерируемый мир",
        _ => mode
    };

    private sealed record Choice(string Value, string Title)
    {
        public override string ToString() => Title;
    }
}
