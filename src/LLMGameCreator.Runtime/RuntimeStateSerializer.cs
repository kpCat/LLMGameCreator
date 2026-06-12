using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class RuntimeStateSerializer : IRuntimeStateSerializer
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    static RuntimeStateSerializer()
    {
        Options.Converters.Add(new JsonStringEnumConverter());
    }

    public string Serialize(GameRuntimeState state)
    {
        return JsonSerializer.Serialize(state, Options);
    }

    public GameRuntimeState DeserializeGameRuntimeState(string json)
    {
        return JsonSerializer.Deserialize<GameRuntimeState>(json, Options) ?? new GameRuntimeState();
    }

    public string Serialize(UnifiedRuntimeSession session)
    {
        return JsonSerializer.Serialize(session, Options);
    }

    public UnifiedRuntimeSession DeserializeUnifiedSession(string json)
    {
        return JsonSerializer.Deserialize<UnifiedRuntimeSession>(json, Options) ?? new UnifiedRuntimeSession();
    }
}
