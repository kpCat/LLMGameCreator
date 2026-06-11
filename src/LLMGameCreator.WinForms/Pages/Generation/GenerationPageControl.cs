using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Generation;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GenerationPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly IFirstPlayableSliceGenerator? _generator;
    private readonly IGamePackageValidator? _validator;
    private readonly IAppSettingsRepository? _settingsRepository;
    private readonly ValidationReportFormatter _validationFormatter = new ValidationReportFormatter();
    private FirstPlayableSliceDraft? _latestDraft;
    private bool _isBusy;

    public GenerationPageControl()
    {
        InitializeComponent();
        SetNoRuntimeState("Design-time preview. Runtime services are not available in Visual Studio Designer.");
    }

    public GenerationPageControl(
        ICurrentGamePackageService currentGamePackageService,
        IFirstPlayableSliceGenerator generator,
        IGamePackageValidator validator,
        IAppSettingsRepository settingsRepository)
    {
        _currentGamePackageService = currentGamePackageService;
        _generator = generator;
        _validator = validator;
        _settingsRepository = settingsRepository;
        InitializeComponent();
        WireEvents();
        _currentGamePackageService.CurrentChanged += CurrentGamePackageService_CurrentChanged;
        RefreshCurrentPackageState();
    }

    public string Id => "generation";
    public string Title => "Генерация";
    public int SortOrder => 30;
    Control IEditorPage.View => this;
    public async void OnActivated()
    {
        RefreshCurrentPackageState();
        await RefreshProfileAsync();
    }

    private void WireEvents()
    {
        _testLmStudioButton.Click += async (_, _) => await TestLmStudioAsync();
        _generateButton.Click += async (_, _) => await GenerateAsync();
        _applyButton.Click += (_, _) => ApplyDraft();
        _saveButton.Click += async (_, _) => await SavePackageAsync();
        _validateButton.Click += (_, _) => ValidatePackage();
    }

    private void CurrentGamePackageService_CurrentChanged(object? sender, EventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(RefreshCurrentPackageState));
            return;
        }

        RefreshCurrentPackageState();
    }

    private async Task RefreshProfileAsync()
    {
        if (_settingsRepository == null)
        {
            return;
        }

        try
        {
            var settings = await _settingsRepository.LoadAsync(CancellationToken.None).ConfigureAwait(true);
            var profile = settings.LlmProfiles.FirstOrDefault(item => string.Equals(item.Id, settings.DefaultLlmProfileId, StringComparison.Ordinal))
                ?? settings.LlmProfiles.FirstOrDefault();
            _profileValueLabel.Text = profile == null
                ? "LLM profile не найден"
                : $"{profile.Title} | {profile.Endpoint.TrimEnd('/')}/chat/completions | model: {profile.Model}";
        }
        catch (Exception ex)
        {
            _profileValueLabel.Text = $"Не удалось загрузить LLM profile: {ex.Message}";
        }
    }

    private void RefreshCurrentPackageState()
    {
        var package = _currentGamePackageService?.CurrentPackage;
        if (package == null)
        {
            _currentPackageLabel.Text = "Проект игры не открыт.";
            _applyButton.Enabled = false;
            _saveButton.Enabled = false;
            _validateButton.Enabled = false;
            return;
        }

        _currentPackageLabel.Text = $"Текущий package: {package.Manifest.Title} ({package.Manifest.PackageId})";
        _saveButton.Enabled = !_isBusy;
        _validateButton.Enabled = !_isBusy;
        _applyButton.Enabled = !_isBusy && _latestDraft != null;
    }

    private async Task TestLmStudioAsync()
    {
        if (_generator == null)
        {
            SetNoRuntimeState("Generation service недоступен.");
            return;
        }

        await RunBusyAsync("Проверяю LM Studio...", async () =>
        {
            var result = await _generator.TestConnectionAsync(CancellationToken.None).ConfigureAwait(true);
            _resultTextBox.Text = $"{result.Message}\r\nEndpoint: {result.Endpoint}\r\nModel: {result.Model}\r\n\r\n{result.RawContent}";
        });
    }

    private async Task GenerateAsync()
    {
        if (_generator == null)
        {
            SetNoRuntimeState("Generation service недоступен.");
            return;
        }

        if (_currentGamePackageService?.CurrentPackage == null)
        {
            _resultTextBox.Text = "Открой или создай проект перед генерацией.";
            return;
        }

        await RunBusyAsync("Генерирую playable slice через LM Studio...", async () =>
        {
            _latestDraft = null;
            var result = await _generator.GenerateAsync(ReadInterview(), CancellationToken.None).ConfigureAwait(true);
            _rawJsonTextBox.Text = string.IsNullOrWhiteSpace(result.RawJson) ? result.RawContent : result.RawJson;
            _resultTextBox.Text = BuildGenerationLog(result);
            if (result.Success)
            {
                _latestDraft = result.Draft;
            }

            RefreshCurrentPackageState();
        });
    }

    private void ApplyDraft()
    {
        if (_generator == null)
        {
            SetNoRuntimeState("Generation service недоступен.");
            return;
        }

        if (_latestDraft == null)
        {
            _resultTextBox.Text = "Сначала успешно сгенерируй draft.";
            return;
        }

        try
        {
            var result = _generator.ApplyDraft(_latestDraft);
            _resultTextBox.Text = BuildApplyLog(result);
            RefreshCurrentPackageState();
        }
        catch (Exception ex)
        {
            _resultTextBox.Text = $"Не удалось применить draft:\r\n{ex.Message}";
        }
    }

    private async Task SavePackageAsync()
    {
        if (_currentGamePackageService == null)
        {
            return;
        }

        await RunBusyAsync("Сохраняю package...", async () =>
        {
            await _currentGamePackageService.SaveAsync(CancellationToken.None).ConfigureAwait(true);
            _resultTextBox.Text = "Package сохранён.";
        });
    }

    private void ValidatePackage()
    {
        var package = _currentGamePackageService?.CurrentPackage;
        if (package == null || _validator == null)
        {
            _resultTextBox.Text = "Проект игры не открыт или валидатор недоступен.";
            return;
        }

        var report = _validator.Validate(package, _currentGamePackageService?.CurrentFolder);
        _resultTextBox.Text = BuildValidationLog(report);
    }

    private async Task RunBusyAsync(string status, Func<Task> action)
    {
        try
        {
            SetBusy(true, status);
            await action().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _resultTextBox.Text = ex.Message;
        }
        finally
        {
            SetBusy(false, string.Empty);
            RefreshCurrentPackageState();
        }
    }

    private void SetBusy(bool busy, string status)
    {
        _isBusy = busy;
        _statusLabel.Text = status;
        _testLmStudioButton.Enabled = !busy;
        _generateButton.Enabled = !busy;
        _applyButton.Enabled = !busy && _latestDraft != null;
        _saveButton.Enabled = !busy && _currentGamePackageService?.CurrentPackage != null;
        _validateButton.Enabled = !busy && _currentGamePackageService?.CurrentPackage != null;
    }

    private void SetNoRuntimeState(string message)
    {
        _currentPackageLabel.Text = message;
        _resultTextBox.Text = message;
        _testLmStudioButton.Enabled = false;
        _generateButton.Enabled = false;
        _applyButton.Enabled = false;
        _saveButton.Enabled = false;
        _validateButton.Enabled = false;
    }

    private GenerationInterviewModel ReadInterview()
    {
        return new GenerationInterviewModel
        {
            GameIdea = _ideaTextBox.Text,
            Genre = _genreComboBox.Text,
            Tone = _toneComboBox.Text,
            CameraView = _cameraComboBox.Text,
            Setting = _settingComboBox.Text,
            FirstLocation = _firstLocationTextBox.Text,
            FirstConflict = _conflictComboBox.Text,
            PlayerRole = _playerRoleTextBox.Text,
            RequiredNpc = _requiredNpcTextBox.Text,
            MapWidth = (int)_mapWidthNumeric.Value,
            MapHeight = (int)_mapHeightNumeric.Value,
            GenerationMode = "first_playable_slice"
        };
    }

    private string BuildGenerationLog(GenerationResult result)
    {
        return string.Join("\r\n", new[]
        {
            result.Success ? "Draft готов к применению." : "Draft не готов к применению.",
            result.Message,
            $"Profile: {result.ProfileTitle}",
            $"Endpoint: {result.Endpoint}",
            $"Model: {result.Model}",
            string.Empty,
            _validationFormatter.Format(result.DraftValidationReport)
        });
    }

    private string BuildApplyLog(FirstPlayableSliceApplyResult result)
    {
        return string.Join("\r\n", new[]
        {
            result.Success ? "Draft применён." : "Draft не применён.",
            result.Message,
            string.Empty,
            _validationFormatter.Format(result.ValidationReport)
        });
    }

    private string BuildValidationLog(ValidationReport report)
    {
        var errors = report.Issues.Count(issue => issue.Severity == ValidationSeverity.Error || issue.Severity == ValidationSeverity.Critical);
        var warnings = report.Issues.Count(issue => issue.Severity == ValidationSeverity.Warning);
        return string.Join("\r\n", new[]
        {
            report.IsValid ? $"Package valid, {warnings} warnings." : $"Package invalid, {errors} errors, {warnings} warnings.",
            string.Empty,
            _validationFormatter.Format(report)
        });
    }
}
