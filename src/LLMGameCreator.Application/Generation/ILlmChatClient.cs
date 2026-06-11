using LLMGameCreator.Application.Settings;

namespace LLMGameCreator.Application.Generation;

public interface ILlmChatClient
{
    Task<LlmChatResponse> CompleteAsync(LlmEndpointSettings profile, LlmChatRequest request, CancellationToken cancellationToken);
}

public sealed class LlmChatRequest
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.2;
    public int MaxTokens { get; set; } = 6000;
}

public sealed class LlmChatResponse
{
    public string Content { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
