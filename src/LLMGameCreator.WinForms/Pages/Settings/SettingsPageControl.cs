using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Settings;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class SettingsPageControl : UserControl, IEditorPage
{
    private readonly IAppSettingsRepository? _settingsRepository;

    public SettingsPageControl()
    {
        InitializeComponent();
        _settingsTextBox.Text = "Design-time preview. Runtime settings repository is not available in Visual Studio Designer.";
    }

    public SettingsPageControl(IAppSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
        InitializeComponent();
    }

    public string Id => "settings";
    public string Title => "Настройки";
    public int SortOrder => 90;
    Control IEditorPage. View => this;

    public async void OnActivated()
    {
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        if (_settingsRepository == null)
        {
            return;
        }

        AppSettings settings = await _settingsRepository.LoadAsync(CancellationToken.None).ConfigureAwait(true);
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
