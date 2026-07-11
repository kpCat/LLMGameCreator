using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public sealed class FeatureModuleCompositionArtifactService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<IReadOnlyList<string>> WriteAsync(
        string repositoryRoot,
        string proceduralRoot,
        string exportRoot,
        FeatureModuleCompositionWriteResult result,
        CancellationToken cancellationToken)
    {
        foreach (var root in new[] { proceduralRoot, exportRoot })
        {
            Directory.CreateDirectory(root);
            await WriteJson(root, "goal145-human-acceptance-record.json", new Goal145HumanAcceptanceRecord(), cancellationToken);
            await WriteJson(root, "featuremodule-catalog.json", result.Catalog, cancellationToken);
            await WriteJson(root, "featuremodule-composition-request.json", result.Request, cancellationToken);
            await WriteJson(root, "featuremodule-composition-plan.json", result.SelectedPlan, cancellationToken);
            await WriteJson(root, "featuremodule-composition-matrix-result.json", result.Matrix, cancellationToken);
            await WriteJson(root, "featuremodule-composition-comparison.json", result.Comparison, cancellationToken);
            await WriteJson(root, "featuremodule-composition-dashboard.json", result.Dashboard, cancellationToken);
            await WriteJson(root, "featuremodule-composition-negative-proof.json", result.NegativeProof, cancellationToken);
            await WriteJson(root, "featuremodule-composition-selection-handoff.json", result.Selection, cancellationToken);
            await WriteJson(root, "unity-featuremodule-composition-matrix-smoke.json", result.UnitySmoke, cancellationToken);
            await WriteJson(root, "one-click-featuremodule-composition-report.json", new
            {
                schemaVersion = "one_click_featuremodule_composition_report_v1",
                goalId = FeatureModuleCompositionVocabulary.GoalId,
                result.Dashboard,
                result.Selection,
                result.Matrix,
                result.Comparison,
                result.NegativeProof
            }, cancellationToken);
            await WriteText(
                Path.Combine(root, "one-click-featuremodule-composition-report.md"),
                RenderReport(result),
                cancellationToken);

            foreach (var pair in result.CompositionArtifacts.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                var compositionRoot = Path.Combine(root, "compositions", pair.Key);
                var artifact = pair.Value;
                await WriteText(Path.Combine(compositionRoot, "package.json"), artifact.PackageJson, cancellationToken);
                await WriteJson(compositionRoot, "composition-plan.json", artifact.Plan, cancellationToken);
                await WriteJson(compositionRoot, "mutation-audit.json", artifact.MutationAudit, cancellationToken);
                await WriteJson(compositionRoot, "package-validation.json", artifact.PackageValidation, cancellationToken);
                await WriteJson(compositionRoot, "session-state.json", artifact.Session, cancellationToken);
                await WriteJson(compositionRoot, "action-catalog.json", artifact.ActionCatalog, cancellationToken);
                await WriteJson(compositionRoot, "journal.json", artifact.Journal, cancellationToken);
                await WriteJson(compositionRoot, "checkpoint.json", artifact.Checkpoint, cancellationToken);
                await WriteJson(compositionRoot, "checkpoint-replay-result.json", artifact.CheckpointReplay, cancellationToken);
                await WriteJson(compositionRoot, "final-replay-result.json", artifact.FinalReplay, cancellationToken);
                await WriteJson(compositionRoot, "semantic-effect-proof.json", artifact.SemanticEffects, cancellationToken);
                await WriteJson(compositionRoot, "order-independence-proof.json", artifact.OrderIndependence, cancellationToken);
            }

            await WriteFileIndex(repositoryRoot, root, cancellationToken);
        }

        return new[] { proceduralRoot, exportRoot }
            .SelectMany(root => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            .Select(path => Relative(repositoryRoot, path))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
    }

    private static async Task WriteFileIndex(string repositoryRoot, string root, CancellationToken cancellationToken)
    {
        var indexPath = Path.Combine(root, "featuremodule-composition-file-index.json");
        var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !path.Equals(indexPath, StringComparison.OrdinalIgnoreCase)
                           && !path.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => new
            {
                relativePath = Relative(repositoryRoot, path),
                sha256 = HashFile(path),
                required = true
            }).ToList();
        await WriteJson(root, "featuremodule-composition-file-index.json", new
        {
            schemaVersion = "featuremodule_composition_file_index_v1",
            goalId = FeatureModuleCompositionVocabulary.GoalId,
            rootPath = Relative(repositoryRoot, root),
            indexedFileCount = files.Count,
            files
        }, cancellationToken);
    }

    private static Task WriteJson(string root, string fileName, object value, CancellationToken cancellationToken) =>
        WriteText(Path.Combine(root, fileName), JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine, cancellationToken);

    private static async Task WriteText(string path, string text, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await File.WriteAllTextAsync(temporary, text, new UTF8Encoding(false), cancellationToken).ConfigureAwait(false);
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static string RenderReport(FeatureModuleCompositionWriteResult result) => string.Join('\n',
    [
        "# Goal146 FeatureModule Composition Matrix",
        string.Empty,
        "- status: " + result.Dashboard.Status,
        "- requiredCoreModuleCount: " + result.Dashboard.RequiredCoreModuleCount,
        "- optionalProfileModuleCount: " + result.Dashboard.OptionalProfileModuleCount,
        "- compositionCount: " + result.Matrix.CompositionCount,
        "- passedCompositionCount: " + result.Matrix.PassedCompositionCount,
        "- distinctPackageSha256Count: " + result.Matrix.DistinctPackageSha256Count,
        "- distinctFinalStateHashCount: " + result.Matrix.DistinctFinalStateHashCount,
        "- selectedCompositionId: " + result.Selection.CompositionId,
        "- selectedPackageSha256: " + result.Selection.PackageSha256,
        "- selectedFinalStateHash: " + result.Selection.FinalStateHash,
        "- selectedCombinedEffectCount: " + result.Dashboard.SelectedCombinedEffectCount,
        "- runtimeAuthority: true",
        "- unityGameplayTruth: false",
        "- accepted: false",
        "- manualReviewDeferred: true",
        string.Empty
    ]);

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');
}
