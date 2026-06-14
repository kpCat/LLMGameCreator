namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanExampleTemplateService
{
    private readonly GeneratorPlanExampleTemplateCatalog _catalog;

    public GeneratorPlanExampleTemplateService(GeneratorPlanExampleTemplateCatalog catalog)
    {
        _catalog = catalog;
    }

    public IReadOnlyList<GeneratorPlanExampleTemplateSummary> ListTemplates()
    {
        return _catalog.ListTemplates();
    }

    public GeneratorPlanExampleTemplate? GetTemplate(string id)
    {
        return _catalog.GetTemplate(id);
    }

    public async Task<GeneratorPlanExampleTemplateMaterializeResult> MaterializeAsync(
        GeneratorPlanExampleTemplateMaterializeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var template = _catalog.GetTemplate(request.TemplateId);
        if (template == null)
        {
            return Failure(request.TemplateId, string.Empty, "Template was not found.");
        }

        if (string.IsNullOrWhiteSpace(request.TargetDirectory))
        {
            return Failure(template.Summary.Id, string.Empty, "Target directory is required.");
        }

        var targetDirectory = Path.GetFullPath(request.TargetDirectory.Trim());
        var fileName = Path.GetFileName(template.FileName);
        if (!string.Equals(fileName, template.FileName, StringComparison.Ordinal) || string.IsNullOrWhiteSpace(fileName))
        {
            return Failure(template.Summary.Id, targetDirectory, "Template file name is invalid.");
        }

        Directory.CreateDirectory(targetDirectory);

        var filePath = Path.GetFullPath(Path.Combine(targetDirectory, fileName));
        if (!IsPathInsideDirectory(filePath, targetDirectory))
        {
            return Failure(template.Summary.Id, filePath, "Template path is outside the target directory.");
        }

        if (File.Exists(filePath) && !request.Overwrite)
        {
            return Failure(template.Summary.Id, filePath, "Template file already exists.");
        }

        await File.WriteAllTextAsync(filePath, template.Json, cancellationToken).ConfigureAwait(false);

        return new GeneratorPlanExampleTemplateMaterializeResult
        {
            Ok = true,
            TemplateId = template.Summary.Id,
            FilePath = filePath,
            Message = "Template file created."
        };
    }

    private static bool IsPathInsideDirectory(string filePath, string directoryPath)
    {
        var directory = Path.GetFullPath(directoryPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return Path.GetFullPath(filePath).StartsWith(directory, StringComparison.OrdinalIgnoreCase);
    }

    private static GeneratorPlanExampleTemplateMaterializeResult Failure(string templateId, string filePath, string message)
    {
        return new GeneratorPlanExampleTemplateMaterializeResult
        {
            Ok = false,
            TemplateId = templateId,
            FilePath = filePath,
            Message = message
        };
    }
}
