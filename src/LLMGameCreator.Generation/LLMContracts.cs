namespace LLMGameCreator.Generation;

public sealed class LlmProfile
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int ContextWindowTokens { get; set; } = 32768;
    public string Role { get; set; } = "general";
}

public sealed class LlmMessage
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}

public sealed class LlmCompletionRequest
{
    public string ProfileId { get; set; } = string.Empty;
    public List<LlmMessage> Messages { get; set; } = new List<LlmMessage>();
    public int MaxOutputTokens { get; set; } = 4096;
}

public sealed class LlmCompletionResponse
{
    public string Content { get; set; } = string.Empty;
    public string RawResponse { get; set; } = string.Empty;
}

public interface ILLMClient
{
    Task<LlmCompletionResponse> CompleteAsync(LlmCompletionRequest request, CancellationToken cancellationToken);
}

public sealed class GenerationSession
{
    public string Id { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public List<LlmMessage> Messages { get; set; } = new List<LlmMessage>();
    public List<GenerationJob> Jobs { get; set; } = new List<GenerationJob>();
}

public sealed class GenerationJob
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string InputSummary { get; set; } = string.Empty;
    public string OutputDraftPath { get; set; } = string.Empty;
}

public sealed class ContextPack
{
    public string Id { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public List<string> IncludedEntityIds { get; set; } = new List<string>();
    public string Content { get; set; } = string.Empty;
}
