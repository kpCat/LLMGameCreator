using System.Text.Json;

namespace LLMGameCreator.Application.Projects;

public static class ContentLanguageCodes
{
    public const string Russian = "ru";
    public const string Ukrainian = "uk";
    public const string English = "en";

    public static IReadOnlyList<string> Supported { get; } = [Russian, Ukrainian, English];

    public static bool IsSupported(string? value)
    {
        return Supported.Contains(value?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase);
    }

    public static string Normalize(string? value, string fallback = Russian)
    {
        return Supported.FirstOrDefault(code => string.Equals(code, value?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? fallback;
    }
}

public sealed record ContentLanguagePolicy
{
    public const string AsciiKebabCaseTechnicalIdPolicy = "ascii_kebab_case";

    public string ContentLanguage { get; init; } = ContentLanguageCodes.Russian;
    public string FallbackContentLanguage { get; init; } = ContentLanguageCodes.English;
    public string TechnicalIdPolicy { get; init; } = AsciiKebabCaseTechnicalIdPolicy;

    public static ContentLanguagePolicy CreateDefault()
    {
        return new ContentLanguagePolicy();
    }

    public ContentLanguagePolicy Normalize()
    {
        return this with
        {
            ContentLanguage = ContentLanguageCodes.Normalize(ContentLanguage),
            FallbackContentLanguage = ContentLanguageCodes.Normalize(FallbackContentLanguage, ContentLanguageCodes.English),
            TechnicalIdPolicy = AsciiKebabCaseTechnicalIdPolicy
        };
    }
}

public sealed record ContentLanguagePolicyLoadResult
{
    public ContentLanguagePolicy Policy { get; init; } = ContentLanguagePolicy.CreateDefault();
    public bool IsProjectPersisted { get; init; }
    public string PolicyPath { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public sealed class ContentLanguagePolicyService
{
    public const string RelativePolicyPath = ".llmgc/settings/content-language-policy.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public async Task<ContentLanguagePolicyLoadResult> LoadAsync(
        string? projectFolder,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            return new ContentLanguagePolicyLoadResult
            {
                Policy = ContentLanguagePolicy.CreateDefault(),
                Status = "No current project folder. Russian content language is active in memory only."
            };
        }

        var policyPath = ResolvePolicyPath(projectFolder);
        if (!File.Exists(policyPath))
        {
            var savedDefault = await SaveAsync(projectFolder, ContentLanguagePolicy.CreateDefault(), cancellationToken)
                .ConfigureAwait(false);
            return savedDefault with { Status = "Default Russian content language policy created for the current project." };
        }

        await using var stream = File.OpenRead(policyPath);
        var policy = await JsonSerializer.DeserializeAsync<ContentLanguagePolicy>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return new ContentLanguagePolicyLoadResult
        {
            Policy = (policy ?? ContentLanguagePolicy.CreateDefault()).Normalize(),
            IsProjectPersisted = true,
            PolicyPath = policyPath,
            Status = "Project content language policy loaded."
        };
    }

    public async Task<ContentLanguagePolicyLoadResult> SaveAsync(
        string? projectFolder,
        ContentLanguagePolicy policy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        var normalized = policy.Normalize();
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            return new ContentLanguagePolicyLoadResult
            {
                Policy = normalized,
                Status = "No current project folder. Content language changed in memory only."
            };
        }

        var policyPath = ResolvePolicyPath(projectFolder);
        Directory.CreateDirectory(Path.GetDirectoryName(policyPath)!);
        await using var stream = File.Create(policyPath);
        await JsonSerializer.SerializeAsync(stream, normalized, JsonOptions, cancellationToken).ConfigureAwait(false);

        return new ContentLanguagePolicyLoadResult
        {
            Policy = normalized,
            IsProjectPersisted = true,
            PolicyPath = policyPath,
            Status = "Project content language policy saved."
        };
    }

    private static string ResolvePolicyPath(string projectFolder)
    {
        return Path.Combine(Path.GetFullPath(projectFolder), ".llmgc", "settings", "content-language-policy.json");
    }
}
