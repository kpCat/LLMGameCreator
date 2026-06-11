using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Settings;

namespace LLMGameCreator.WinForms.Pages;

public sealed class SettingsPageControl : UserControl, IEditorPage
{
    private readonly IAppSettingsRepository _settingsRepository;
    private readonly TextBox _settingsTextBox = new TextBox();

    public SettingsPageControl(IAppSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
        BuildLayout();
    }

    public string Id => "settings";
    public string Title => "Настройки";
    public int SortOrder => 90;
    public Control View => this;

    public async void OnActivated()
    {
        await LoadSettingsAsync();
    }

    private void BuildLayout()
    {
        _settingsTextBox.Dock = DockStyle.Fill;
        _settingsTextBox.Multiline = true;
        _settingsTextBox.ReadOnly = true;
        _settingsTextBox.ScrollBars = ScrollBars.Vertical;
        Controls.Add(_settingsTextBox);
    }

    private async Task LoadSettingsAsync()
    {
        AppSettings settings = await _settingsRepository.LoadAsync(CancellationToken.None);
        _settingsTextBox.Text =
            $"GamesRootPath: {settings.GamesRootPath}\r\n" +
            $"LogsPath: {settings.LogsPath}\r\n" +
            $"DefaultLlmProfileId: {settings.DefaultLlmProfileId}\r\n" +
            $"DefaultAssetProviderId: {settings.DefaultAssetProviderId}\r\n\r\n" +
            "LLM profiles:\r\n" +
            string.Join("\r\n", settings.LlmProfiles.Select(p => $"- {p.Id}: {p.Title} / {p.Endpoint} / ctx {p.ContextWindowTokens}")) +
            "\r\n\r\nExternal tools:\r\n" +
            string.Join("\r\n", settings.ExternalTools.Select(t => $"- {t.Id}: {t.Type} / {t.Endpoint} / enabled={t.Enabled}"));
    }
}
