namespace LLMGameCreator.Application.Settings;

public sealed class AppSettings
{
    public string GamesRootPath { get; set; } = "Games";
    public string LogsPath { get; set; } = "Logs";
    public string DefaultLlmProfileId { get; set; } = "local-main";
    public string DefaultAssetProviderId { get; set; } = "manual";
    public UiSettings Ui { get; set; } = new UiSettings();
    public GenerationSettings Generation { get; set; } = new GenerationSettings();
    public List<LlmEndpointSettings> LlmProfiles { get; set; } = new List<LlmEndpointSettings>();
    public List<ExternalToolSettings> ExternalTools { get; set; } = new List<ExternalToolSettings>();
}

public sealed class UiSettings
{
    public bool OpenLastProjectOnStart { get; set; } = true;
    public bool ConfirmDraftApply { get; set; } = true;
}

public sealed class GenerationSettings
{
    public int MaxParallelJobs { get; set; } = 1;
    public bool SaveEveryRequest { get; set; } = true;
    public bool RequireValidationBeforeApply { get; set; } = true;
    public int MaxContextTokens { get; set; } = 30000;
}

public sealed class LlmEndpointSettings
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int ContextWindowTokens { get; set; } = 32768;
    public string Role { get; set; } = "general";
}

public sealed class ExternalToolSettings
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}
