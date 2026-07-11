using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleAuthoringArtifactService
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
        FeatureModuleAuthoringProofResult result,
        CancellationToken cancellationToken)
    {
        foreach (var root in new[] { proceduralRoot, exportRoot })
        {
            Directory.CreateDirectory(root);
            await WriteJson(root, "featuremodule-library-index.json", result.Library.Index, cancellationToken);
            await WriteJson(root, "featuremodule-library-validation.json", result.Library.Validation, cancellationToken);
            await WriteJson(root, "featuremodule-parameter-schema.json", result.ParameterSchema, cancellationToken);
            await WriteJson(root, "featuremodule-default-hash-compatibility-proof.json", result.DefaultHashCompatibilityProof, cancellationToken);
            await WriteJson(root, "saved-composition-roundtrip-proof.json", result.SavedCompositionRoundtripProof, cancellationToken);
            await WriteJson(root, "parameterized-composition-materialization-proof.json", result.ParameterizedCompositionMaterializationProof, cancellationToken);
            await WriteJson(root, "module-certification-ledger.json", result.CertificationLedger, cancellationToken);
            await WriteJson(root, "module-certification-cache-proof.json", result.CertificationCacheProof, cancellationToken);
            await WriteJson(root, "bounded-interaction-coverage-proof.json", result.BoundedInteractionCoverageProof, cancellationToken);
            await WriteJson(root, "hundred-module-scalability-proof.json", result.HundredModuleScalabilityProof, cancellationToken);
            await WriteJson(root, "multi-effect-module-proof.json", result.MultiEffectModuleProof, cancellationToken);
            await WriteJson(root, "featuremodule-authoring-dashboard.json", result.Dashboard, cancellationToken);
            await WriteJson(root, "featuremodule-authoring-negative-proof.json", result.NegativeProof, cancellationToken);
            await WriteJson(root, "unity-saved-featuremodule-composition-smoke.json", result.UnitySmoke, cancellationToken);
            await WriteJson(root, "one-click-featuremodule-authoring-report.json", new
            {
                schemaVersion = "one_click_featuremodule_authoring_report_v1",
                goalId = FeatureModuleAuthoringVocabulary.GoalId,
                result.Dashboard,
                result.Library.Index,
                result.SelectedComposition,
                result.ParameterizedCompositionMaterializationProof,
                result.CertificationLedger,
                result.HundredModuleScalabilityProof,
                result.MultiEffectModuleProof,
                result.NegativeProof,
                result.UnitySmoke
            }, cancellationToken);
            await WriteText(Path.Combine(root, "one-click-featuremodule-authoring-report.md"), RenderReport(result), cancellationToken);

            var selectedRoot = Path.Combine(root, "selected-composition");
            var materialization = result.SelectedMaterialization;
            var artifacts = materialization.Qualification.Artifacts;
            await WriteJson(selectedRoot, "composition.json", result.SelectedComposition, cancellationToken);
            await WriteJson(selectedRoot, "effective-parameter-values.json", materialization.Plan.ParameterBinding.EffectiveParameterValues, cancellationToken);
            await WriteJson(selectedRoot, "effective-mutation-plan.json", materialization.Plan.ParameterBinding.EffectiveMutationOperations, cancellationToken);
            await WriteJson(selectedRoot, "parameter-audit.json", materialization.Plan.ParameterBinding, cancellationToken);
            await WriteText(Path.Combine(selectedRoot, "package.json"), materialization.PackageJson, cancellationToken);
            await WriteJson(selectedRoot, "package-validation.json", artifacts.PackageValidation, cancellationToken);
            await WriteJson(selectedRoot, "session-state.json", artifacts.Session, cancellationToken);
            await WriteJson(selectedRoot, "action-catalog.json", artifacts.ActionCatalog, cancellationToken);
            await WriteJson(selectedRoot, "journal.json", artifacts.Journal, cancellationToken);
            await WriteJson(selectedRoot, "checkpoint.json", artifacts.Checkpoint, cancellationToken);
            await WriteJson(selectedRoot, "checkpoint-replay-result.json", artifacts.CheckpointReplay, cancellationToken);
            await WriteJson(selectedRoot, "final-replay-result.json", artifacts.FinalReplay, cancellationToken);
            await WriteJson(selectedRoot, "runtime-effect-observations.json", new
            {
                materialization.EffectObservationCount,
                materialization.PassedEffectObservationCount,
                materialization.SelectedModuleCount,
                materialization.SatisfiedSelectedModuleCount,
                observations = artifacts.SemanticEffects.Observations
            }, cancellationToken);
            await WriteFileIndex(repositoryRoot, root, cancellationToken);
        }
        return new[] { proceduralRoot, exportRoot }.SelectMany(root => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            .Where(path => !path.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            .Select(path => Relative(repositoryRoot, path)).OrderBy(path => path, StringComparer.Ordinal).ToList();
    }

    private static async Task WriteFileIndex(string repositoryRoot, string root, CancellationToken cancellationToken)
    {
        var indexPath = Path.Combine(root, "featuremodule-authoring-file-index.json");
        var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !path.Equals(indexPath, StringComparison.OrdinalIgnoreCase)
                           && !path.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => new { relativePath = Relative(repositoryRoot, path), sha256 = HashFile(path), required = true })
            .ToList();
        await WriteJson(root, "featuremodule-authoring-file-index.json", new
        {
            schemaVersion = "featuremodule_authoring_file_index_v1",
            goalId = FeatureModuleAuthoringVocabulary.GoalId,
            rootPath = Relative(repositoryRoot, root),
            indexedFileCount = files.Count,
            files
        }, cancellationToken);
    }

    private static string RenderReport(FeatureModuleAuthoringProofResult result) => string.Join('\n',
    [
        "# Goal147 FeatureModule Authoring, Persistence and Certification",
        string.Empty,
        "- status: " + result.Dashboard.Status,
        "- catalogFingerprint: " + result.Library.CatalogFingerprint,
        "- requiredCoreModuleCount: " + result.Dashboard.RequiredCoreModuleCount,
        "- optionalModuleCount: " + result.Dashboard.OptionalModuleCount,
        "- parameterDefinitionCount: " + result.Dashboard.ParameterDefinitionCount,
        "- allOptionalModulesCertified: " + result.Dashboard.AllOptionalModulesCertified.ToString().ToLowerInvariant(),
        "- hundredModuleInteractionRowCount: " + result.Dashboard.HundredModuleInteractionRowCount,
        "- selectedCompositionId: " + result.SelectedComposition.CompositionId,
        "- customPackageSha256: " + result.SelectedMaterialization.PackageSha256,
        "- customFinalStateHash: " + result.SelectedMaterialization.FinalStateHash,
        "- runtimeAuthority: true",
        "- projectionOnly: false",
        "- unityGameplayTruth: false",
        "- goal146Accepted: false",
        "- goal147Accepted: false",
        "- accepted: false",
        string.Empty
    ]);

    private static Task WriteJson(string root, string name, object value, CancellationToken cancellationToken) =>
        WriteText(Path.Combine(root, name), JsonSerializer.Serialize(value, JsonOptions) + "\n", cancellationToken);

    private static async Task WriteText(string path, string value, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await File.WriteAllTextAsync(temporary, value, new UTF8Encoding(false), cancellationToken).ConfigureAwait(false);
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
}
