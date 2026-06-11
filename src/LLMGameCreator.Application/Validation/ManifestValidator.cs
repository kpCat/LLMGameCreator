using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Validation;

internal sealed class ManifestValidator : IGamePackageValidationRule
{
    private const string Category = "Manifest";

    public void Validate(ValidationContext context, ValidationReport report)
    {
        var package = context.Package;

        if (string.IsNullOrWhiteSpace(package.Manifest.PackageId))
        {
            ValidationIssueBuilder.Add(report, "manifest.package_id.empty", ValidationSeverity.Error, "PackageId не заполнен.", null, Category);
        }

        if (string.IsNullOrWhiteSpace(package.Manifest.StartMapId))
        {
            ValidationIssueBuilder.Add(report, "manifest.start_map.empty", ValidationSeverity.Error, "StartMapId не заполнен.", null, Category);
        }
        else if (!package.Game.Maps.Any(m => m.Id == package.Manifest.StartMapId))
        {
            ValidationIssueBuilder.Add(report, "manifest.start_map.missing", ValidationSeverity.Error, "StartMapId ссылается на несуществующую карту.", package.Manifest.StartMapId, Category);
        }
    }
}
