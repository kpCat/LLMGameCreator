using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Settings;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class SettingsPageControl : UserControl, IEditorPage
{
    private readonly IAppSettingsRepository? _settingsRepository;
    private AppSettings? _settings;
    private bool _loadingSelection;

    public SettingsPageControl()
    {
        InitializeComponent();
        _statusLabel.Text = "Design-time preview. Runtime settings repository is not available in Visual Studio Designer.";
    }

    public SettingsPageControl(IAppSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
        InitializeComponent();
        WireEvents();
    }

    public string Id => "settings";
    public string Title => "Настройки";
    public int SortOrder => 90;
    Control IEditorPage.View => this;

    public async void OnActivated()
    {
        await LoadSettingsAsync();
    }

    private void WireEvents()
    {
        _reloadButton.Click += async (_, _) => await LoadSettingsAsync();
        _applyProfileButton.Click += (_, _) => ApplySelectedProfileChanges();
        _addProfileButton.Click += (_, _) => AddProfile();
        _removeProfileButton.Click += (_, _) => RemoveSelectedProfile();
        _saveButton.Click += async (_, _) => await SaveSettingsAsync();
        _profilesListView.SelectedIndexChanged += (_, _) => FillProfileEditorFromSelection();
    }

    private async Task LoadSettingsAsync()
    {
        if (_settingsRepository == null)
        {
            return;
        }

        try
        {
            _settings = await _settingsRepository.LoadAsync(CancellationToken.None).ConfigureAwait(true);
            FillControlsFromSettings();
            _statusLabel.Text = "Настройки загружены.";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Не удалось загрузить настройки: {ex.Message}";
        }
    }

    private void FillControlsFromSettings()
    {
        if (_settings == null)
        {
            return;
        }

        _gamesRootTextBox.Text = _settings.GamesRootPath;
        _logsPathTextBox.Text = _settings.LogsPath;
        _defaultAssetProviderTextBox.Text = _settings.DefaultAssetProviderId;
        RefreshProfilesList();
        RefreshDefaultProfileCombo();
    }

    private void RefreshProfilesList()
    {
        _profilesListView.Items.Clear();
        if (_settings == null)
        {
            return;
        }

        foreach (var profile in _settings.LlmProfiles)
        {
            var item = new ListViewItem(profile.Id);
            item.SubItems.Add(profile.Title);
            item.SubItems.Add(profile.Endpoint);
            item.SubItems.Add(profile.Model);
            item.SubItems.Add(profile.ContextWindowTokens.ToString());
            item.SubItems.Add(profile.Role);
            item.Tag = profile;
            _profilesListView.Items.Add(item);
        }

        if (_profilesListView.Items.Count > 0)
        {
            _profilesListView.Items[0].Selected = true;
        }
    }

    private void RefreshDefaultProfileCombo()
    {
        if (_settings == null)
        {
            return;
        }

        _defaultLlmProfileComboBox.Items.Clear();
        foreach (var profile in _settings.LlmProfiles)
        {
            _defaultLlmProfileComboBox.Items.Add(profile.Id);
        }

        _defaultLlmProfileComboBox.Text = _settings.LlmProfiles.Any(item => string.Equals(item.Id, _settings.DefaultLlmProfileId, StringComparison.Ordinal))
            ? _settings.DefaultLlmProfileId
            : _settings.LlmProfiles.FirstOrDefault()?.Id ?? string.Empty;
    }

    private void FillProfileEditorFromSelection()
    {
        if (_profilesListView.SelectedItems.Count == 0 || _profilesListView.SelectedItems[0].Tag is not LlmEndpointSettings profile)
        {
            return;
        }

        _loadingSelection = true;
        _profileIdTextBox.Text = profile.Id;
        _profileTitleTextBox.Text = profile.Title;
        _profileEndpointTextBox.Text = profile.Endpoint;
        _profileModelTextBox.Text = profile.Model;
        _profileContextNumeric.Value = Math.Clamp(profile.ContextWindowTokens, (int)_profileContextNumeric.Minimum, (int)_profileContextNumeric.Maximum);
        _profileRoleComboBox.Text = profile.Role;
        _loadingSelection = false;
    }

    private void ApplySelectedProfileChanges()
    {
        if (_loadingSelection || _settings == null || _profilesListView.SelectedItems.Count == 0)
        {
            return;
        }

        if (_profilesListView.SelectedItems[0].Tag is not LlmEndpointSettings profile)
        {
            return;
        }

        var error = ValidateProfileEditor();
        if (error != null)
        {
            MessageBox.Show(this, error, "Настройки профиля", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        profile.Id = _profileIdTextBox.Text.Trim();
        profile.Title = _profileTitleTextBox.Text.Trim();
        profile.Endpoint = _profileEndpointTextBox.Text.Trim();
        profile.Model = _profileModelTextBox.Text.Trim();
        profile.ContextWindowTokens = (int)_profileContextNumeric.Value;
        profile.Role = _profileRoleComboBox.Text.Trim();

        RefreshProfilesList();
        SelectProfile(profile.Id);
        RefreshDefaultProfileCombo();
        _statusLabel.Text = "Изменения профиля применены в памяти. Нажми Save settings для записи файла.";
    }

    private void AddProfile()
    {
        if (_settings == null)
        {
            return;
        }

        var id = CreateUniqueProfileId();
        var profile = new LlmEndpointSettings
        {
            Id = id,
            Title = "Новый LLM profile",
            Endpoint = "http://127.0.0.1:1234/v1",
            Model = "local-model",
            ContextWindowTokens = 32768,
            Role = "general"
        };

        _settings.LlmProfiles.Add(profile);
        RefreshProfilesList();
        SelectProfile(id);
        RefreshDefaultProfileCombo();
        _statusLabel.Text = "Профиль добавлен в память. Нажми Save settings для записи файла.";
    }

    private void RemoveSelectedProfile()
    {
        if (_settings == null || _profilesListView.SelectedItems.Count == 0)
        {
            return;
        }

        if (_settings.LlmProfiles.Count <= 1)
        {
            MessageBox.Show(this, "Нельзя удалить последний LLM profile.", "Настройки", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_profilesListView.SelectedItems[0].Tag is LlmEndpointSettings profile)
        {
            _settings.LlmProfiles.Remove(profile);
            if (string.Equals(_settings.DefaultLlmProfileId, profile.Id, StringComparison.Ordinal))
            {
                _settings.DefaultLlmProfileId = _settings.LlmProfiles[0].Id;
            }

            RefreshProfilesList();
            RefreshDefaultProfileCombo();
            _statusLabel.Text = "Профиль удалён из памяти. Нажми Save settings для записи файла.";
        }
    }

    private async Task SaveSettingsAsync()
    {
        if (_settingsRepository == null || _settings == null)
        {
            return;
        }

        ApplyRootControlsToSettings();
        var error = ValidateSettings();
        if (error != null)
        {
            MessageBox.Show(this, error, "Настройки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!_settings.LlmProfiles.Any(item => string.Equals(item.Id, _settings.DefaultLlmProfileId, StringComparison.Ordinal)))
        {
            _settings.DefaultLlmProfileId = _settings.LlmProfiles[0].Id;
        }

        try
        {
            await _settingsRepository.SaveAsync(_settings, CancellationToken.None).ConfigureAwait(true);
            RefreshDefaultProfileCombo();
            _statusLabel.Text = "Настройки сохранены.";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Не удалось сохранить настройки: {ex.Message}";
        }
    }

    private void ApplyRootControlsToSettings()
    {
        if (_settings == null)
        {
            return;
        }

        _settings.GamesRootPath = _gamesRootTextBox.Text.Trim();
        _settings.LogsPath = _logsPathTextBox.Text.Trim();
        _settings.DefaultLlmProfileId = _defaultLlmProfileComboBox.Text.Trim();
        _settings.DefaultAssetProviderId = _defaultAssetProviderTextBox.Text.Trim();
    }

    private string? ValidateSettings()
    {
        if (_settings == null)
        {
            return "Настройки не загружены.";
        }

        if (string.IsNullOrWhiteSpace(_settings.GamesRootPath))
        {
            return "GamesRootPath не должен быть пустым.";
        }

        if (string.IsNullOrWhiteSpace(_settings.LogsPath))
        {
            return "LogsPath не должен быть пустым.";
        }

        if (_settings.LlmProfiles.Count == 0)
        {
            return "Нужен хотя бы один LLM profile.";
        }

        foreach (var profile in _settings.LlmProfiles)
        {
            if (string.IsNullOrWhiteSpace(profile.Id))
            {
                return "Profile Id не должен быть пустым.";
            }

            if (string.IsNullOrWhiteSpace(profile.Endpoint))
            {
                return $"Profile '{profile.Id}' endpoint не должен быть пустым.";
            }

            if (string.IsNullOrWhiteSpace(profile.Model))
            {
                return $"Profile '{profile.Id}' model не должен быть пустым.";
            }

            if (profile.ContextWindowTokens <= 0)
            {
                return $"Profile '{profile.Id}' ContextWindowTokens должен быть > 0.";
            }
        }

        return null;
    }

    private string? ValidateProfileEditor()
    {
        if (string.IsNullOrWhiteSpace(_profileIdTextBox.Text))
        {
            return "Profile Id не должен быть пустым.";
        }

        if (string.IsNullOrWhiteSpace(_profileEndpointTextBox.Text))
        {
            return "Profile Endpoint не должен быть пустым.";
        }

        if (string.IsNullOrWhiteSpace(_profileModelTextBox.Text))
        {
            return "Profile Model не должен быть пустым.";
        }

        if (_profileContextNumeric.Value <= 0)
        {
            return "ContextWindowTokens должен быть > 0.";
        }

        return null;
    }

    private string CreateUniqueProfileId()
    {
        if (_settings == null)
        {
            return "local-new";
        }

        if (_settings.LlmProfiles.All(item => !string.Equals(item.Id, "local-new", StringComparison.Ordinal)))
        {
            return "local-new";
        }

        for (var index = 1; index < 1000; index++)
        {
            var id = $"profile-{index}";
            if (_settings.LlmProfiles.All(item => !string.Equals(item.Id, id, StringComparison.Ordinal)))
            {
                return id;
            }
        }

        return "profile-" + Guid.NewGuid().ToString("N")[..8];
    }

    private void SelectProfile(string id)
    {
        foreach (ListViewItem item in _profilesListView.Items)
        {
            item.Selected = item.Tag is LlmEndpointSettings profile && string.Equals(profile.Id, id, StringComparison.Ordinal);
        }
    }
}
