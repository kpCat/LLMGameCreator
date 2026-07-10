using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;

public sealed class ProductLineRuntimeVariantValidator
{
    private readonly IGamePackageValidator _validator;

    public ProductLineRuntimeVariantValidator(IGamePackageValidator? validator = null)
    {
        _validator = validator ?? new GamePackageValidator();
    }

    public ProductLineRuntimeVariantPackageValidation Validate(
        string repositoryRootPath,
        string outputRootPath,
        string packagePath,
        string projectFolderPath,
        GamePackageDefinition package,
        ProductLineRuntimeVariantRecipe recipe,
        bool validJson,
        bool sourceTemplateUnchanged)
    {
        var diagnostics = new List<string>();
        var validation = _validator.Validate(package, projectFolderPath);
        diagnostics.AddRange(validation.Issues
            .Where(issue => issue.Severity is Domain.Validation.ValidationSeverity.Error
                or Domain.Validation.ValidationSeverity.Critical)
            .Select(issue => issue.ToString()));

        var missingAnchors = MissingAnchors(package, recipe.RequiredAnchors);
        if (missingAnchors.Count > 0)
        {
            diagnostics.Add("missing required anchors: " + string.Join(", ", missingAnchors));
        }

        var metadataMatches = CandidateIdMatchesPackageMetadata(package, recipe.CandidateId);
        if (!metadataMatches)
        {
            diagnostics.Add("candidate metadata does not match " + recipe.CandidateId);
        }

        var underOutputRoot = IsUnderDirectory(packagePath, outputRootPath);
        if (!underOutputRoot)
        {
            diagnostics.Add("candidate package is outside Goal142 output root");
        }

        return new ProductLineRuntimeVariantPackageValidation
        {
            CandidateId = recipe.CandidateId,
            RecipeId = recipe.RecipeId,
            PackagePath = Relative(repositoryRootPath, packagePath),
            CandidateFileExists = File.Exists(packagePath),
            ValidJson = validJson,
            ExistingPackageValidatorPassed = validation.IsValid,
            HandoffCandidateIdMatchesPackageMetadata = metadataMatches,
            RequiredAnchorsPresent = missingAnchors.Count == 0,
            NoBrokenRequiredReferences = validation.IsValid,
            SourceTemplateUnchanged = sourceTemplateUnchanged,
            CandidatePackageUnderGoal142Root = underOutputRoot,
            Passed = File.Exists(packagePath)
                     && validJson
                     && validation.IsValid
                     && metadataMatches
                     && missingAnchors.Count == 0
                     && sourceTemplateUnchanged
                     && underOutputRoot,
            MissingAnchors = missingAnchors,
            Diagnostics = diagnostics
        };
    }

    private static bool CandidateIdMatchesPackageMetadata(
        GamePackageDefinition package,
        string candidateId)
    {
        try
        {
            using var document = JsonDocument.Parse(package.GeneratedContent.Profile.SourceContextJson);
            return document.RootElement.TryGetProperty("candidateId", out var value)
                   && string.Equals(value.GetString(), candidateId, StringComparison.Ordinal);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static List<string> MissingAnchors(
        GamePackageDefinition package,
        IReadOnlyList<string> requiredAnchors)
    {
        var present = new HashSet<string>(StringComparer.Ordinal);
        foreach (var map in package.Game.Maps)
        {
            present.Add(map.Id);
            foreach (var entity in map.Entities)
            {
                present.Add(entity.Id);
            }
        }

        foreach (var item in package.Game.Interactions)
        {
            present.Add(item.Id);
        }

        foreach (var item in package.Game.Dialogues)
        {
            present.Add(item.Id);
        }

        foreach (var item in package.Game.Quests)
        {
            present.Add(item.Id);
        }

        foreach (var item in package.Game.Inventories)
        {
            present.Add(item.Id);
        }

        foreach (var item in package.Game.Recipes)
        {
            present.Add(item.Id);
        }

        foreach (var item in package.Game.ResourceNodes)
        {
            present.Add(item.Id);
        }

        foreach (var item in package.Game.Transactions)
        {
            present.Add(item.Id);
        }

        foreach (var item in package.Game.Encounters)
        {
            present.Add(item.Id);
        }

        return requiredAnchors
            .Where(anchor => !present.Contains(anchor))
            .OrderBy(anchor => anchor, StringComparer.Ordinal)
            .ToList();
    }

    private static bool IsUnderDirectory(string path, string directory)
    {
        var fullPath = Path.GetFullPath(path);
        var fullDirectory = Path.GetFullPath(directory);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return fullPath.Equals(fullDirectory, comparison)
               || fullPath.StartsWith(
                   fullDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                   + Path.DirectorySeparatorChar,
                   comparison);
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');
}
