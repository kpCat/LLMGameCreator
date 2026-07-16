using System.Text;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class SeededGeneratedProjectArtifactFactory
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly GenerationPresetOptionsService _presetOptions;
    private readonly VisibleGeneratedPlayablePreviewService _generation;
    private readonly GeneratedProjectOverlayService _overlay;
    private readonly IGeneratedProjectBaselineProvider _baselineProvider;

    public SeededGeneratedProjectArtifactFactory(
        IGeneratedProjectBaselineProvider baselineProvider,
        IGamePackageValidator? validator = null,
        GenerationPresetOptionsService? presetOptions = null,
        VisibleGeneratedPlayablePreviewService? generation = null,
        GeneratedProjectOverlayService? overlay = null)
    {
        _baselineProvider = baselineProvider ?? throw new ArgumentNullException(nameof(baselineProvider));
        _presetOptions = presetOptions ?? new GenerationPresetOptionsService();
        _generation = generation ?? new VisibleGeneratedPlayablePreviewService(generationOptionsService: _presetOptions);
        _overlay = overlay ?? new GeneratedProjectOverlayService(validator);
    }

    public SeededGeneratedProjectArtifactFactoryResult Create(SeededGeneratedProjectArtifactFactoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputDirectory);
        if (!GeneratedProjectMechanicsProfiles.Supported.Contains(request.MechanicsProfileId, StringComparer.Ordinal))
            throw new InvalidOperationException("generated_project.profile_unknown");

        var normalizedRequest = SeededGeneratedProjectSourceService.NormalizeRequest(request.GenerationRequest);
        var resolved = _presetOptions.Resolve(normalizedRequest);
        var generated = _generation.Generate(new VisibleGeneratedPlayablePreviewRequest
        {
            Seed = resolved.Seed,
            Mode = resolved.Mode,
            PresetId = resolved.PresetId,
            CompactStyleHintIds = resolved.CompactStyleHintIds,
            SelectedVariantIds = resolved.SelectedVariantIds
        });
        var diagnostics = generated.PlanResult.Diagnostics
            .Where(diagnostic => diagnostic.Severity == "error")
            .Select(diagnostic => diagnostic.Code)
            .Concat(generated.RulePackResult.ValidationReport.HasErrors ? ["generated_project.rule_pack_failed"] : [])
            .Concat(generated.PackageMvpResult.Report.HasErrors ? ["generated_project.mvp_failed"] : [])
            .Concat(generated.TinyLoopResult.Report.HasErrors ? ["generated_project.tiny_loop_failed"] : [])
            .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        if (diagnostics.Count > 0) return new SeededGeneratedProjectArtifactFactoryResult
        {
            ResolvedOptions = resolved,
            Generated = generated,
            Diagnostics = diagnostics
        };

        var baseline = _baselineProvider.Resolve();
        var namespacedMvp = _overlay.NamespaceGeneratedPackage(generated.PackageMvpResult.PackageJson);
        var overlay = _overlay.Build(
            baseline.PackageJson,
            baseline.PackageSha256,
            namespacedMvp,
            generated.PlanResult.Plan);
        if (!overlay.Passed) return new SeededGeneratedProjectArtifactFactoryResult
        {
            ResolvedOptions = resolved,
            Generated = generated,
            Overlay = overlay,
            Diagnostics = overlay.Diagnostics
        };

        var sidecars = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SeededGeneratedProjectVocabulary.PlanJsonFileName] = generated.PlanResult.Json,
            [SeededGeneratedProjectVocabulary.PlanMarkdownFileName] = generated.PlanResult.Markdown,
            [SeededGeneratedProjectVocabulary.RulePackJsonFileName] = generated.RulePackResult.Json,
            [SeededGeneratedProjectVocabulary.TinyLoopStateJsonFileName] = generated.TinyLoopResult.StateJson,
            [SeededGeneratedProjectVocabulary.TinyLoopReportMarkdownFileName] = generated.TinyLoopResult.ReportMarkdown,
            [SeededGeneratedProjectVocabulary.GeneratedMvpPackageJsonFileName] = namespacedMvp,
            [SeededGeneratedProjectVocabulary.GeneratedOverlayJsonFileName] = overlay.OverlayJson,
            [SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName] = overlay.GeneratedBasePackageJson
        };
        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var sidecar in sidecars) hashes[sidecar.Key] = SeededGeneratedProjectSourceService.HashText(sidecar.Value);
        var source = new SeededGeneratedProjectSourceRecord
        {
            SchemaVersion = SeededGeneratedProjectVocabulary.SourceV2SchemaVersion,
            Seed = resolved.Seed,
            Mode = resolved.Mode,
            PresetId = resolved.PresetId,
            StyleHintIds = resolved.CompactStyleHintIds,
            VariantIds = resolved.SelectedVariantIds,
            GenerationRequest = normalizedRequest,
            ResolvedGenerationOptions = resolved,
            RequestOrigin = SeededGeneratedProjectRequestOrigins.ExplicitV2Request,
            MechanicsProfileId = request.MechanicsProfileId,
            PlanId = generated.PlanResult.Plan.PlanId,
            PlanSha256 = hashes[SeededGeneratedProjectVocabulary.PlanJsonFileName],
            RulePackId = generated.RulePackResult.RulePack.Metadata.RulePackId,
            RulePackSha256 = hashes[SeededGeneratedProjectVocabulary.RulePackJsonFileName],
            TinyLoopStateSha256 = hashes[SeededGeneratedProjectVocabulary.TinyLoopStateJsonFileName],
            GeneratedMvpPackageSha256 = hashes[SeededGeneratedProjectVocabulary.GeneratedMvpPackageJsonFileName],
            GeneratedOverlaySha256 = hashes[SeededGeneratedProjectVocabulary.GeneratedOverlayJsonFileName],
            GeneratedBasePackageSha256 = hashes[SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName],
            Goal142BaselinePackageSha256 = baseline.PackageSha256,
            GeneratedStartMapId = overlay.Document.GeneratedStartMapId,
            Counts = SeededGeneratedProjectSourceService.Counts(generated.PlanResult.Plan),
            TinyLoop = SeededGeneratedProjectSourceService.BuildTinyLoopFacts(
                generated.PlanResult.Plan,
                generated.RulePackResult.RulePack,
                generated.TinyLoopResult),
            SidecarSha256 = hashes
        };
        var sourceJson = SeededGeneratedProjectSourceService.SerializeV2(source);
        var output = Path.GetFullPath(request.OutputDirectory);
        Directory.CreateDirectory(output);
        foreach (var sidecar in sidecars)
            File.WriteAllText(Confined(output, sidecar.Key), sidecar.Value, Utf8WithoutBom);
        File.WriteAllText(
            Confined(output, SeededGeneratedProjectVocabulary.SourceJsonFileName),
            sourceJson,
            Utf8WithoutBom);
        return new SeededGeneratedProjectArtifactFactoryResult
        {
            ResolvedOptions = resolved,
            Generated = generated,
            Overlay = overlay,
            Source = source,
            SourceJson = sourceJson,
            SidecarBytes = sidecars,
            SidecarSha256 = hashes,
            Passed = true
        };
    }

    private static string Confined(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("generated_project.path_escape");
        return path;
    }
}
