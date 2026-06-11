using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Validation;

internal sealed class ScriptCatalogValidator : IGamePackageValidationRule
{
    private const string Category = "Script";

    public void Validate(ValidationContext context, ValidationReport report)
    {
        ValidationIssueBuilder.CheckDuplicates(report, context.Package.ScriptCatalog.Scripts.Select(x => x.Id), "script", Category);

        var duplicateGeneratorIds = context.Package.ScriptCatalog.Generators
            .Select(generator => generator.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .GroupBy(id => id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet();

        foreach (var script in context.Package.ScriptCatalog.Scripts)
        {
            ValidateScriptDefinition(context, report, script);
        }

        foreach (var generator in context.Package.ScriptCatalog.Generators)
        {
            ValidateGeneratorDefinition(context, report, generator, duplicateGeneratorIds);
        }
    }

    private static void ValidateScriptDefinition(ValidationContext context, ValidationReport report, ScriptDefinition script)
    {
        if (string.IsNullOrWhiteSpace(script.Id))
        {
            ValidationIssueBuilder.Add(report, "script.id.empty", ValidationSeverity.Error, "Script id не заполнен.", null, Category);
        }

        var resolvedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(script.Path))
        {
            ValidationIssueBuilder.Add(report, "script.path.empty", ValidationSeverity.Error, "Script path не заполнен.", script.Id, Category);
        }
        else if (Path.IsPathRooted(script.Path))
        {
            ValidationIssueBuilder.Add(report, "script.path.rooted", ValidationSeverity.Error, "Script path должен быть относительным.", script.Id, Category, script.Path);
        }
        else if (ValidationPathGuard.ContainsPathTraversal(script.Path))
        {
            ValidationIssueBuilder.Add(report, "script.path.traversal", ValidationSeverity.Error, "Script path must not contain path traversal.", script.Id, Category, script.Path);
        }
        else if (!string.IsNullOrWhiteSpace(context.ProjectFolder) && !ValidationPathGuard.TryResolveInsideProject(context.ProjectFolder, script.Path, out resolvedPath))
        {
            ValidationIssueBuilder.Add(report, "script.path.outside_project", ValidationSeverity.Error, "Script path normalizes outside projectFolder.", script.Id, Category, script.Path);
        }
        else if (!string.IsNullOrWhiteSpace(context.ProjectFolder) && !File.Exists(resolvedPath))
        {
            ValidationIssueBuilder.Add(report, "script.path.missing", ValidationSeverity.Error, "Script path указывает на несуществующий файл.", script.Id, Category, script.Path);
        }

        if (!Enum.IsDefined(typeof(LuaScriptKind), script.Kind))
        {
            ValidationIssueBuilder.Add(report, "script.kind.invalid", ValidationSeverity.Error, "Script kind неизвестен.", script.Id, Category);
        }

        if (!ValidationIssueBuilder.HasAnyText(script.EntryPoints))
        {
            ValidationIssueBuilder.Add(report, "script.entry_points.empty", ValidationSeverity.Error, "Script entryPoints не заполнены.", script.Id, Category);
        }

        if (!ValidationIssueBuilder.HasAnyText(script.Capabilities))
        {
            ValidationIssueBuilder.Add(report, "script.capabilities.empty", ValidationSeverity.Error, "Script capabilities не заполнены.", script.Id, Category);
        }
    }

    private static void ValidateGeneratorDefinition(
        ValidationContext context,
        ValidationReport report,
        GeneratorDefinition generator,
        IReadOnlySet<string> duplicateGeneratorIds)
    {
        if (string.IsNullOrWhiteSpace(generator.Id))
        {
            ValidationIssueBuilder.Add(report, "generator.id.empty", ValidationSeverity.Error, "Generator id не заполнен.", null, Category);
        }

        if (!string.IsNullOrWhiteSpace(generator.Id) && duplicateGeneratorIds.Contains(generator.Id))
        {
            ValidationIssueBuilder.Add(report, "duplicate.generator", ValidationSeverity.Error, $"Duplicate generator id: {generator.Id}", generator.Id, Category);
        }

        if (string.IsNullOrWhiteSpace(generator.Kind))
        {
            ValidationIssueBuilder.Add(report, "generator.kind.empty", ValidationSeverity.Error, "Generator kind is empty.", generator.Id, Category);
        }

        if (string.IsNullOrWhiteSpace(generator.ScriptId))
        {
            ValidationIssueBuilder.Add(report, "generator.script_id.empty", ValidationSeverity.Error, "Generator scriptId не заполнен.", generator.Id, Category);
            return;
        }

        if (!context.ScriptsById.TryGetValue(generator.ScriptId, out var script))
        {
            ValidationIssueBuilder.Add(report, "generator.script_id.missing", ValidationSeverity.Error, "Generator scriptId ссылается на несуществующий script.", generator.Id, Category);
            return;
        }

        if (script.Kind != LuaScriptKind.Generator)
        {
            ValidationIssueBuilder.Add(report, "generator.script_kind.invalid", ValidationSeverity.Error, "Generator scriptId должен ссылаться на script с Kind == Generator.", generator.Id, Category);
        }

        if (string.IsNullOrWhiteSpace(generator.EntryPoint))
        {
            ValidationIssueBuilder.Add(report, "generator.entry_point.empty", ValidationSeverity.Error, "Generator entryPoint не заполнен.", generator.Id, Category);
        }
        else if (!script.EntryPoints.Any(entryPoint => string.Equals(entryPoint, generator.EntryPoint.Trim(), StringComparison.Ordinal)))
        {
            ValidationIssueBuilder.Add(report, "generator.entry_point.missing", ValidationSeverity.Error, "Generator entryPoint отсутствует в script.EntryPoints.", generator.Id, Category);
        }
    }
}
