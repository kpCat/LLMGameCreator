using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.Semantics;

public sealed class SemanticGenerationContextPreviewService
{
    public const string PreviewJsonFileName = "semantic-generation-context-preview.json";
    public const string PreviewMarkdownFileName = "semantic-generation-context-preview.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly SemanticGenerationContextPreviewMarkdownRenderer _markdownRenderer;

    public SemanticGenerationContextPreviewService(
        SemanticGenerationContextPreviewMarkdownRenderer? markdownRenderer = null)
    {
        _markdownRenderer = markdownRenderer ?? new SemanticGenerationContextPreviewMarkdownRenderer();
    }

    public SemanticGenerationContextPreview Build(SemanticCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var sections = new List<SemanticGenerationContextSection>();
        AddSection(sections, catalog, "themes", "Themes", SemanticTermKinds.Theme);
        AddSection(sections, catalog, "tones", "Tones", SemanticTermKinds.Tone);
        AddSection(sections, catalog, "dialogue-intents", "Dialogue intents", SemanticTermKinds.DialogueIntent);
        AddSection(sections, catalog, "quest-motifs", "Quest motifs", SemanticTermKinds.QuestMotif);
        AddSection(sections, catalog, "asset-style-hints", "Asset style hints", SemanticTermKinds.AssetStyleHint);
        AddSection(sections, catalog, "audio-mood-hints", "Audio mood hints", SemanticTermKinds.AudioMoodHint);

        var relatedTerms = catalog.Relations
            .SelectMany(relation => new[] { relation.SourceTermId, relation.TargetTermId })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(termId => termId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(termId => termId, StringComparer.Ordinal)
            .Take(80)
            .ToList();
        if (relatedTerms.Count > 0)
        {
            sections.Add(new SemanticGenerationContextSection
            {
                SectionId = "important-relations",
                Title = "Important relation terms",
                TermIds = relatedTerms
            });
        }

        var conflicts = catalog.Terms
            .Where(term => term.Status == SemanticTermStatuses.Conflict)
            .Select(term => term.TermId)
            .OrderBy(termId => termId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(termId => termId, StringComparer.Ordinal)
            .Take(80)
            .ToList();
        if (conflicts.Count > 0)
        {
            sections.Add(new SemanticGenerationContextSection
            {
                SectionId = "unresolved-conflicts",
                Title = "Unresolved conflicts",
                TermIds = conflicts
            });
        }

        return new SemanticGenerationContextPreview
        {
            LlmPolicy = new SemanticGenerationLlmPolicy
            {
                LlmRequiredFor =
                [
                    "new creative game concept text",
                    "new quest/dialogue prose",
                    "new art/audio prompt phrasing when no deterministic template is sufficient",
                    "resolving ambiguous user intent when deterministic presets conflict"
                ],
                DeterministicSteps =
                [
                    "ID generation",
                    "path generation",
                    "slot planning",
                    "schema validation",
                    "compatibility validation",
                    "request planning",
                    "fulfillment scanning",
                    "archive review/history/comparison",
                    "manifest template generation",
                    "semantic catalog merge",
                    "known-term lookup",
                    "basic relation validation",
                    "fallback placeholder generation",
                    "report rendering"
                ],
                MaxRecommendedPromptTerms = 80
            },
            Sections = sections,
            CandidateTerms = catalog.Terms
                .Where(term => term.Status == SemanticTermStatuses.Candidate)
                .Select(term => term.TermId)
                .OrderBy(termId => termId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(termId => termId, StringComparer.Ordinal)
                .Take(80)
                .ToList(),
            Diagnostics = catalog.Diagnostics
        };
    }

    public async Task<SemanticGenerationContextPreviewWriteResult> WriteAsync(
        string projectRootPath,
        SemanticGenerationContextPreview preview,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preview);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "semantic"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.GetFullPath(Path.Combine(outputDirectory, PreviewJsonFileName));
        var markdownPath = Path.GetFullPath(Path.Combine(outputDirectory, PreviewMarkdownFileName));
        EnsureContained(outputDirectory, jsonPath);
        EnsureContained(outputDirectory, markdownPath);

        await File.WriteAllTextAsync(
            jsonPath,
            JsonSerializer.Serialize(preview, JsonOptions),
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            markdownPath,
            _markdownRenderer.Render(preview),
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);

        return new SemanticGenerationContextPreviewWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            PreviewJsonPath = jsonPath,
            PreviewMarkdownPath = markdownPath
        };
    }

    private static void AddSection(
        ICollection<SemanticGenerationContextSection> sections,
        SemanticCatalog catalog,
        string sectionId,
        string title,
        string kind)
    {
        var termIds = catalog.Terms
            .Where(term => term.Kind == kind && term.Status is SemanticTermStatuses.Known or SemanticTermStatuses.Candidate)
            .OrderBy(term => term.Status == SemanticTermStatuses.Known ? 0 : 1)
            .ThenBy(term => term.TermId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(term => term.TermId, StringComparer.Ordinal)
            .Select(term => term.TermId)
            .Take(80)
            .ToList();
        if (termIds.Count > 0)
        {
            sections.Add(new SemanticGenerationContextSection
            {
                SectionId = sectionId,
                Title = title,
                TermIds = termIds
            });
        }
    }

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Semantic preview output path must stay under the project root.");
        }
    }
}
