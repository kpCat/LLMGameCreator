using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Settings;

namespace LLMGameCreator.Infrastructure.Storage;

public sealed class JsonAppSettingsRepository : IAppSettingsRepository
{
    private readonly string _settingsPath;

    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public JsonAppSettingsRepository(string settingsPath)
    {
        _settingsPath = settingsPath;
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_settingsPath))
        {
            var defaults = CreateDefaultSettings();
            await SaveAsync(defaults, cancellationToken).ConfigureAwait(false);
            return defaults;
        }

        await using var stream = File.OpenRead(_settingsPath);
        var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, Options, cancellationToken).ConfigureAwait(false);
        return settings ?? CreateDefaultSettings();
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_settingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, Options, cancellationToken).ConfigureAwait(false);
    }

    private static AppSettings CreateDefaultSettings()
    {
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator");
        return new AppSettings
        {
            GamesRootPath = Path.Combine(appData, "Games"),
            LogsPath = Path.Combine(appData, "Logs"),
            DefaultLlmProfileId = "local-main",
            DefaultAssetProviderId = "manual",
            LlmProfiles = new List<LlmEndpointSettings>
            {
                new LlmEndpointSettings
                {
                    Id = "local-main",
                    Title = "Основной локальный ПК",
                    Endpoint = "http://127.0.0.1:1234/v1",
                    Model = "local-model",
                    ContextWindowTokens = 32768,
                    Role = "general"
                },
                new LlmEndpointSettings
                {
                    Id = "wife-pc",
                    Title = "ПК жены / локальная сеть",
                    Endpoint = "http://192.168.1.184:1234/v1",
                    Model = "local-model",
                    ContextWindowTokens = 32768,
                    Role = "writer"
                }
            },
            ExternalTools = new List<ExternalToolSettings>
            {
                new ExternalToolSettings
                {
                    Id = "comfyui-local",
                    Type = "comfyui",
                    Endpoint = "http://127.0.0.1:8188",
                    MachineName = "local",
                    Enabled = false
                },
                new ExternalToolSettings
                {
                    Id = "comfyui-lan",
                    Type = "comfyui",
                    Endpoint = "http://192.168.1.184:8188",
                    MachineName = "lan-machine",
                    Enabled = false
                }
            }
        };
    }
}
