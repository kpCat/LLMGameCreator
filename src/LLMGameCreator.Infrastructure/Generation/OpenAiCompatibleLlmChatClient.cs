using System.Net.Http.Json;
using System.Text.Json;
using LLMGameCreator.Application.Generation;
using LLMGameCreator.Application.Settings;

namespace LLMGameCreator.Infrastructure.Generation;

public sealed class OpenAiCompatibleLlmChatClient : ILlmChatClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public OpenAiCompatibleLlmChatClient()
        : this(new HttpClient { Timeout = Timeout.InfiniteTimeSpan })
    {
    }

    public OpenAiCompatibleLlmChatClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LlmChatResponse> CompleteAsync(LlmEndpointSettings profile, LlmChatRequest request, CancellationToken cancellationToken)
    {
        var endpoint = BuildChatCompletionsEndpoint(profile.Endpoint);
        var body = new
        {
            model = profile.Model,
            messages = new[]
            {
                new { role = "system", content = request.SystemPrompt },
                new { role = "user", content = request.UserPrompt }
            },
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            stream = false
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(endpoint, body, Options, cancellationToken).ConfigureAwait(false);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"LM Studio вернул HTTP {(int)response.StatusCode}: {responseText}");
            }

            var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(responseText, Options);
            var content = completion?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("LM Studio response не содержит choices[0].message.content.");
            }

            return new LlmChatResponse
            {
                Content = content,
                Endpoint = endpoint,
                Model = profile.Model
            };
        }
        catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("Операция отменена пользователем.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new InvalidOperationException("LM Studio request был отменён или соединение оборвалось.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Не удалось подключиться к LM Studio endpoint '{endpoint}': {ex.Message}", ex);
        }
    }

    private static string BuildChatCompletionsEndpoint(string endpoint)
    {
        var trimmed = endpoint.TrimEnd('/');
        if (trimmed.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        return trimmed + "/chat/completions";
    }

    private sealed class ChatCompletionResponse
    {
        public List<ChatCompletionChoice> Choices { get; set; } = new List<ChatCompletionChoice>();
    }

    private sealed class ChatCompletionChoice
    {
        public ChatCompletionMessage? Message { get; set; }
    }

    private sealed class ChatCompletionMessage
    {
        public string Content { get; set; } = string.Empty;
    }
}
