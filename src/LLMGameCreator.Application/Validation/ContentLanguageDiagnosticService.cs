using System.Text.Json;
using System.Text.RegularExpressions;
using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.Application.Validation;

public sealed record ContentLanguageDiagnostic
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
}

public sealed partial class ContentLanguageDiagnosticService
{
    public const string ObviousEnglishProseWarning = "content_language.obvious_english_prose";

    private static readonly HashSet<string> PlayerFacingScalarNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "title", "name", "description", "purpose", "text", "summary", "objective"
    };

    private static readonly HashSet<string> PlayerFacingCollectionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "lines", "objectives", "steps"
    };

    public IReadOnlyList<ContentLanguageDiagnostic> Inspect(string json, string? contentLanguage)
    {
        var language = ContentLanguageCodes.Normalize(contentLanguage);
        if (language == ContentLanguageCodes.English || string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<ContentLanguageDiagnostic>();
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return EnumeratePlayerFacingText(document.RootElement, string.Empty)
                .Where(item => IsObviouslyEnglish(item.Value))
                .Select(item => new ContentLanguageDiagnostic
                {
                    Code = ObviousEnglishProseWarning,
                    Target = item.Path,
                    Message = $"Player-facing text at {item.Path} appears to be English while content language is '{language}'."
                })
                .ToList();
        }
        catch (JsonException)
        {
            return Array.Empty<ContentLanguageDiagnostic>();
        }
    }

    private static IEnumerable<(string Path, string Value)> EnumeratePlayerFacingText(JsonElement element, string path)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            yield break;
        }

        foreach (var property in element.EnumerateObject())
        {
            var propertyPath = string.IsNullOrWhiteSpace(path) ? property.Name : path + "." + property.Name;
            if (PlayerFacingScalarNames.Contains(property.Name) && property.Value.ValueKind == JsonValueKind.String)
            {
                yield return (propertyPath, property.Value.GetString() ?? string.Empty);
                continue;
            }

            if (PlayerFacingCollectionNames.Contains(property.Name))
            {
                foreach (var item in EnumerateCollectionText(property.Value, propertyPath))
                {
                    yield return item;
                }

                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var item in EnumeratePlayerFacingText(property.Value, propertyPath))
                {
                    yield return item;
                }
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                var index = 0;
                foreach (var child in property.Value.EnumerateArray())
                {
                    if (child.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var item in EnumeratePlayerFacingText(child, $"{propertyPath}[{index}]"))
                        {
                            yield return item;
                        }
                    }

                    index++;
                }
            }
        }
    }

    private static IEnumerable<(string Path, string Value)> EnumerateCollectionText(JsonElement element, string path)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        var index = 0;
        foreach (var child in element.EnumerateArray())
        {
            var childPath = $"{path}[{index}]";
            if (child.ValueKind == JsonValueKind.String)
            {
                yield return (childPath, child.GetString() ?? string.Empty);
            }
            else if (child.ValueKind == JsonValueKind.Object)
            {
                foreach (var item in EnumeratePlayerFacingText(child, childPath))
                {
                    yield return item;
                }
            }

            index++;
        }
    }

    private static bool IsObviouslyEnglish(string value)
    {
        var latinWords = LatinWordRegex().Matches(value).Count;
        var cyrillicWords = CyrillicWordRegex().Matches(value).Count;
        var wordCount = latinWords + cyrillicWords;
        return latinWords >= 3 && wordCount > 0 && (double)latinWords / wordCount >= 0.70;
    }

    [GeneratedRegex("[A-Za-z]{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex LatinWordRegex();

    [GeneratedRegex("[\\u0400-\\u04FF]{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex CyrillicWordRegex();
}
