using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Validation;

internal sealed class AssetCatalogValidator : IGamePackageValidationRule
{
    private const string Category = "Asset";

    public void Validate(ValidationContext context, ValidationReport report)
    {
        ValidationIssueBuilder.CheckDuplicates(report, context.Package.AssetCatalog.Assets.Select(x => x.Id), "asset", Category);

        foreach (var asset in context.Package.AssetCatalog.Assets)
        {
            ValidateAsset(context, report, asset);
        }
    }

    private static void ValidateAsset(ValidationContext context, ValidationReport report, AssetDefinition asset)
    {
        if (string.IsNullOrWhiteSpace(asset.Id))
        {
            ValidationIssueBuilder.Add(report, "asset.id.empty", ValidationSeverity.Error, "Asset id is empty.", null, Category);
        }

        if (string.IsNullOrWhiteSpace(asset.Type))
        {
            ValidationIssueBuilder.Add(report, "asset.type.empty", ValidationSeverity.Error, "Asset type is empty.", asset.Id, Category);
        }

        ValidateAssetPath(context, report, asset);

        if (!string.IsNullOrWhiteSpace(asset.FallbackAssetId) && !context.AssetIds.Contains(asset.FallbackAssetId))
        {
            ValidationIssueBuilder.Add(report, "asset.fallback.missing", ValidationSeverity.Error, "FallbackAssetId references a missing asset.", asset.Id, Category);
        }

        if (!string.IsNullOrWhiteSpace(asset.ContractId) && !context.AssetContractIds.Contains(asset.ContractId))
        {
            ValidationIssueBuilder.Add(report, "asset.contract.missing", ValidationSeverity.Error, "ContractId references a missing asset contract.", asset.Id, Category);
        }
    }

    private static void ValidateAssetPath(ValidationContext context, ValidationReport report, AssetDefinition asset)
    {
        if (string.IsNullOrWhiteSpace(asset.Path))
        {
            return;
        }

        if (Path.IsPathRooted(asset.Path))
        {
            ValidationIssueBuilder.Add(report, "asset.path.rooted", ValidationSeverity.Error, "Asset path must be relative.", asset.Id, Category, asset.Path);
            return;
        }

        if (ValidationPathGuard.ContainsPathTraversal(asset.Path))
        {
            ValidationIssueBuilder.Add(report, "asset.path.traversal", ValidationSeverity.Error, "Asset path must not contain path traversal.", asset.Id, Category, asset.Path);
            return;
        }

        if (string.IsNullOrWhiteSpace(context.ProjectFolder))
        {
            return;
        }

        if (!ValidationPathGuard.TryResolveInsideProject(context.ProjectFolder, asset.Path, out var resolvedPath))
        {
            ValidationIssueBuilder.Add(report, "asset.path.outside_project", ValidationSeverity.Error, "Asset path normalizes outside projectFolder.", asset.Id, Category, asset.Path);
            return;
        }

        if (!File.Exists(resolvedPath))
        {
            ValidationIssueBuilder.Add(report, "asset.path.missing", ValidationSeverity.Warning, "Asset path points to a missing file.", asset.Id, Category, asset.Path);
        }
    }
}
