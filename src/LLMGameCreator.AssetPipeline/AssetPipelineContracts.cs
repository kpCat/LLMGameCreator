namespace LLMGameCreator.AssetPipeline;

public sealed class AssetWorkflowProfile
{
    public string Id { get; set; } = string.Empty;
    public string Provider { get; set; } = "manual";
    public string AssetType { get; set; } = string.Empty;
    public string ContractId { get; set; } = string.Empty;
    public string WorkflowFile { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
}

public sealed class AssetGenerationJob
{
    public string Id { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string WorkflowProfileId { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string NegativePrompt { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public List<string> OutputFiles { get; set; } = new List<string>();
}

public sealed class AssetGenerationResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<string> OutputFiles { get; set; } = new List<string>();
}

public interface IAssetGenerationProvider
{
    string Id { get; }
    Task<AssetGenerationResult> GenerateAsync(AssetGenerationJob job, CancellationToken cancellationToken);
}

public sealed class NullAssetGenerationProvider : IAssetGenerationProvider
{
    public string Id => "null";

    public Task<AssetGenerationResult> GenerateAsync(AssetGenerationJob job, CancellationToken cancellationToken)
    {
        return Task.FromResult(new AssetGenerationResult
        {
            Success = false,
            Error = "Asset generation provider не подключён. В v0.1 это ожидаемая заглушка."
        });
    }
}
