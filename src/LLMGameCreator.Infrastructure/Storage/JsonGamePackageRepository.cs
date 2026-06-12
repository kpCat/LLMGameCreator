using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Infrastructure.Storage;

public sealed class JsonGamePackageRepository : IGamePackageRepository
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static JsonGamePackageRepository()
    {
        Options.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<GamePackageDefinition> LoadAsync(string projectFolder, CancellationToken cancellationToken)
    {
        var path = Path.Combine(projectFolder, "package.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Файл package.json не найден.", path);
        }

        await using var stream = File.OpenRead(path);
        var package = await JsonSerializer.DeserializeAsync<GamePackageDefinition>(stream, Options, cancellationToken).ConfigureAwait(false);
        return package ?? throw new InvalidOperationException("Не удалось прочитать package.json.");
    }

    public async Task SaveAsync(string projectFolder, GamePackageDefinition package, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(projectFolder);
        var path = Path.Combine(projectFolder, "package.json");
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, package, Options, cancellationToken).ConfigureAwait(false);
    }
}
