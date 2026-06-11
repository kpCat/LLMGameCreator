using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Validation;

public interface IGamePackageValidator
{
    ValidationReport Validate(GamePackageDefinition package);
    ValidationReport Validate(GamePackageDefinition package, string? projectFolder);
}

public sealed class GamePackageValidator : IGamePackageValidator
{
    private readonly IReadOnlyList<IGamePackageValidationRule> _rules;

    public GamePackageValidator()
    {
        _rules = new IGamePackageValidationRule[]
        {
            new ManifestValidator(),
            new GameDefinitionValidator(),
            new AssetCatalogValidator(),
            new ScriptCatalogValidator()
        };
    }

    public ValidationReport Validate(GamePackageDefinition package)
    {
        return Validate(package, null);
    }

    public ValidationReport Validate(GamePackageDefinition package, string? projectFolder)
    {
        var context = new ValidationContext(package, projectFolder);
        var report = new ValidationReport();

        foreach (var rule in _rules)
        {
            rule.Validate(context, report);
        }

        return report;
    }
}
