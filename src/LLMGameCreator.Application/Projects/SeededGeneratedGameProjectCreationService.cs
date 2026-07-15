using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Projects;

public sealed class SeededGeneratedGameProjectCreationService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _repositoryRoot;
    private readonly IGamePackageRepository _repository;
    private readonly IGamePackageValidator _validator;
    private readonly NewGamePackageFactory _templateFactory;
    private readonly GenerationPresetOptionsService _presetOptions;
    private readonly VisibleGeneratedPlayablePreviewService _generation;
    private readonly GeneratedProjectOverlayService _overlay;

    public SeededGeneratedGameProjectCreationService(
        string repositoryRoot,
        IGamePackageRepository repository,
        IGamePackageValidator validator,
        NewGamePackageFactory templateFactory,
        GenerationPresetOptionsService? presetOptions = null,
        VisibleGeneratedPlayablePreviewService? generation = null,
        GeneratedProjectOverlayService? overlay = null,
        SeededGeneratedProjectSourceService? sourceService = null)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _templateFactory = templateFactory ?? throw new ArgumentNullException(nameof(templateFactory));
        _presetOptions = presetOptions ?? new GenerationPresetOptionsService();
        _generation = generation ?? new VisibleGeneratedPlayablePreviewService(
            generationOptionsService: _presetOptions);
        _overlay = overlay ?? new GeneratedProjectOverlayService(_validator);
        SourceService = sourceService ?? new SeededGeneratedProjectSourceService(_validator, _presetOptions, _overlay);
    }

    public SeededGeneratedProjectSourceService SourceService { get; }

    public async Task<string> CreateAsync(
        CreateGameProjectRequest request,
        string gamesRootPath,
        string targetFolder,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateGenerationRequest(request);
        var gamesRoot = Path.GetFullPath(gamesRootPath);
        var target = Path.GetFullPath(targetFolder);
        EnsureContained(gamesRoot, target, "generated_project.target_path_escape");
        if (Directory.Exists(target) || File.Exists(target))
            throw new InvalidOperationException("generated_project.target_exists");

        var temporary = Path.GetFullPath(Path.Combine(
            gamesRoot,
            "." + request.FolderName.Trim() + ".creating-" + Guid.NewGuid().ToString("N")));
        EnsureContained(gamesRoot, temporary, "generated_project.temporary_path_escape");
        if (Directory.Exists(temporary) || File.Exists(temporary))
            throw new InvalidOperationException("generated_project.temporary_exists");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(temporary);
            Directory.CreateDirectory(Path.Combine(temporary, "assets"));
            Directory.CreateDirectory(Path.Combine(temporary, "scripts"));
            Directory.CreateDirectory(Path.Combine(temporary, "saves"));

            var options = _presetOptions.Resolve(new GenerationPresetOptionsRequest
            {
                Seed = request.GenerationSeed.Trim(),
                Mode = request.GenerationMode.Trim(),
                PresetId = request.GenerationPresetId.Trim(),
                CompactStyleHintIds = request.CompactStyleHintIds,
                SelectedVariantIds = request.SelectedVariantIds
            });
            AssertResolvedRequest(request, options);
            var generated = _generation.Generate(new VisibleGeneratedPlayablePreviewRequest
            {
                Seed = options.Seed,
                Mode = options.Mode,
                PresetId = options.PresetId,
                CompactStyleHintIds = options.CompactStyleHintIds,
                SelectedVariantIds = options.SelectedVariantIds
            });
            if (generated.PlanResult.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")
                || generated.RulePackResult.ValidationReport.HasErrors
                || generated.PackageMvpResult.Report.HasErrors
                || generated.TinyLoopResult.Report.HasErrors)
                throw new InvalidOperationException("generated_project.generation_failed");

            var baseline = ResolveGoal142Baseline();
            var generatedMvpPackageJson = _overlay.NamespaceGeneratedPackage(generated.PackageMvpResult.PackageJson);
            var overlay = _overlay.Build(
                baseline.Json,
                baseline.Sha256,
                generatedMvpPackageJson,
                generated.PlanResult.Plan);
            if (!overlay.Passed)
                throw new InvalidOperationException("generated_project.overlay_failed:" + string.Join(";", overlay.Diagnostics));

            var generationRoot = Resolve(temporary, SeededGeneratedProjectVocabulary.GenerationRelativeRoot);
            Directory.CreateDirectory(generationRoot);
            var sidecars = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [SeededGeneratedProjectVocabulary.PlanJsonFileName] = generated.PlanResult.Json,
                [SeededGeneratedProjectVocabulary.PlanMarkdownFileName] = generated.PlanResult.Markdown,
                [SeededGeneratedProjectVocabulary.RulePackJsonFileName] = generated.RulePackResult.Json,
                [SeededGeneratedProjectVocabulary.TinyLoopStateJsonFileName] = generated.TinyLoopResult.StateJson,
                [SeededGeneratedProjectVocabulary.TinyLoopReportMarkdownFileName] = generated.TinyLoopResult.ReportMarkdown,
                [SeededGeneratedProjectVocabulary.GeneratedMvpPackageJsonFileName] = generatedMvpPackageJson,
                [SeededGeneratedProjectVocabulary.GeneratedOverlayJsonFileName] = overlay.OverlayJson,
                [SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName] = overlay.GeneratedBasePackageJson
            };
            foreach (var sidecar in sidecars)
                await WriteAsync(Resolve(generationRoot, sidecar.Key), sidecar.Value, cancellationToken).ConfigureAwait(false);

            var sidecarHashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var fileName in SeededGeneratedProjectVocabulary.RequiredSidecarFileNames)
                sidecarHashes[fileName] = SeededGeneratedProjectSourceService.HashFile(Resolve(generationRoot, fileName));
            var source = new SeededGeneratedProjectSourceRecord
            {
                Seed = options.Seed,
                Mode = options.Mode,
                PresetId = options.PresetId,
                StyleHintIds = options.CompactStyleHintIds,
                VariantIds = options.SelectedVariantIds,
                MechanicsProfileId = request.MechanicsProfileId.Trim(),
                PlanId = generated.PlanResult.Plan.PlanId,
                PlanSha256 = sidecarHashes[SeededGeneratedProjectVocabulary.PlanJsonFileName],
                RulePackId = generated.RulePackResult.RulePack.Metadata.RulePackId,
                RulePackSha256 = sidecarHashes[SeededGeneratedProjectVocabulary.RulePackJsonFileName],
                TinyLoopStateSha256 = sidecarHashes[SeededGeneratedProjectVocabulary.TinyLoopStateJsonFileName],
                GeneratedMvpPackageSha256 = sidecarHashes[SeededGeneratedProjectVocabulary.GeneratedMvpPackageJsonFileName],
                GeneratedOverlaySha256 = sidecarHashes[SeededGeneratedProjectVocabulary.GeneratedOverlayJsonFileName],
                GeneratedBasePackageSha256 = sidecarHashes[SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName],
                Goal142BaselinePackageSha256 = baseline.Sha256,
                GeneratedStartMapId = overlay.Document.GeneratedStartMapId,
                Counts = SeededGeneratedProjectSourceService.Counts(generated.PlanResult.Plan),
                TinyLoop = SeededGeneratedProjectSourceService.BuildTinyLoopFacts(
                    generated.PlanResult.Plan,
                    generated.RulePackResult.RulePack,
                    generated.TinyLoopResult),
                SidecarSha256 = sidecarHashes
            };
            var sourceJson = JsonSerializer.Serialize(source, JsonOptions);
            await WriteAsync(Resolve(generationRoot, SeededGeneratedProjectVocabulary.SourceJsonFileName), sourceJson, cancellationToken)
                .ConfigureAwait(false);

            var package = DeserializePackage(overlay.GeneratedBasePackageJson);
            var templateManifest = _templateFactory.Create(request).Manifest;
            package.Manifest.PackageId = request.PackageId.Trim();
            package.Manifest.Title = request.Title.Trim();
            package.Manifest.Version = request.Version.Trim();
            package.Manifest.FormatVersion = templateManifest.FormatVersion;
            package.Manifest.Description = "Seeded generated project created by LLMGameCreator.";
            await _repository.SaveAsync(temporary, package, cancellationToken).ConfigureAwait(false);
            MaterializeSupportFiles(temporary, package);

            var validation = _validator.Validate(package, temporary);
            if (!validation.IsValid)
                throw new InvalidOperationException("generated_project.package_invalid:"
                                                    + string.Join(";", validation.Issues.Select(issue => issue.Code)));
            InitializeAuthoring(temporary, package, request.MechanicsProfileId.Trim());
            var sourceValidation = SourceService.Validate(temporary);
            if (!sourceValidation.Present || !sourceValidation.Passed)
                throw new InvalidOperationException("generated_project.source_invalid:"
                                                    + string.Join(";", sourceValidation.Diagnostics));

            cancellationToken.ThrowIfCancellationRequested();
            if (Directory.Exists(target) || File.Exists(target))
                throw new InvalidOperationException("generated_project.target_exists");
            Directory.Move(temporary, target);
            return target;
        }
        catch
        {
            if (Directory.Exists(temporary)) Directory.Delete(temporary, recursive: true);
            throw;
        }
    }

    private void InitializeAuthoring(string projectFolder, GamePackageDefinition package, string mechanicsProfileId)
    {
        var authoring = new GameProjectFeatureModuleAuthoringService(_repositoryRoot);
        var state = authoring.OpenProject(projectFolder, package);
        var desired = mechanicsProfileId == GeneratedProjectMechanicsProfiles.AllSelectableDefaults
            ? state.Library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
                .Select(module => module.ModuleId).ToHashSet(StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
        foreach (var module in state.Library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
                     .OrderBy(module => module.ModuleId, StringComparer.Ordinal))
            authoring.SetModuleSelected(module.ModuleId, desired.Contains(module.ModuleId));
        var document = authoring.Save();
        if (!document.SelectedModuleIds.SequenceEqual(desired.OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal)
            || document.ParameterValues.Count != 0)
            throw new InvalidOperationException("generated_project.mechanics_profile_initialization_failed");
    }

    private void MaterializeSupportFiles(string projectFolder, GamePackageDefinition package)
    {
        var materializer = new GameProjectSupportFileMaterializer();
        var plan = materializer.CreatePlan(
            package,
            projectFolder,
            new NarrowAlphaTemplateSupportFileSource(Path.Combine(_repositoryRoot, "samples", "minimal-map-game")));
        if (!plan.IsValid)
            throw new InvalidOperationException("generated_project.support_files_invalid:" + string.Join(";", plan.Diagnostics));
        foreach (var entry in GameProjectSupportFileMaterializer.UniqueEntries(plan))
        {
            if (entry.ActivationAction == GameProjectSupportFileActivationAction.Reuse) continue;
            Directory.CreateDirectory(Path.GetDirectoryName(entry.TargetPath)!);
            File.Copy(entry.SourcePath, entry.TargetPath, overwrite: false);
            if (!string.Equals(GameProjectSupportFileMaterializer.HashFile(entry.TargetPath), entry.SourceSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("generated_project.support_file_hash_mismatch:" + entry.RelativePath);
        }
    }

    private (string Path, string Json, string Sha256) ResolveGoal142Baseline()
    {
        var matrixPath = Path.Combine(
            _repositoryRoot,
            FeatureModuleCompositionVocabulary.Goal142Root.Replace('/', Path.DirectorySeparatorChar),
            ProductLineRuntimeVariantMatrixVocabulary.MatrixResultFileName);
        using var matrix = JsonDocument.Parse(File.ReadAllText(matrixPath, Encoding.UTF8));
        var row = matrix.RootElement.GetProperty("candidates").EnumerateArray()
            .Single(candidate => candidate.GetProperty("candidateId").GetString()
                                 == FeatureModuleCompositionVocabulary.BaselineCandidateId);
        var relativePath = row.GetProperty("packagePath").GetString()
                           ?? throw new InvalidOperationException("generated_project.baseline_path_missing");
        var expectedHash = row.GetProperty("packageSha256").GetString()
                           ?? throw new InvalidOperationException("generated_project.baseline_hash_missing");
        var path = Resolve(_repositoryRoot, relativePath);
        var actualHash = SeededGeneratedProjectSourceService.HashFile(path);
        if (!string.Equals(actualHash, expectedHash, StringComparison.Ordinal))
            throw new InvalidOperationException("generated_project.baseline_hash_mismatch");
        return (path, File.ReadAllText(path, Encoding.UTF8), actualHash);
    }

    private void ValidateGenerationRequest(CreateGameProjectRequest request)
    {
        if (!string.Equals(request.CreationKind, GameProjectCreationKinds.SeededGenerated, StringComparison.Ordinal))
            throw new ArgumentException("generated_project.creation_kind_invalid", nameof(request));
        if (string.IsNullOrWhiteSpace(request.GenerationSeed))
            throw new ArgumentException("generated_project.seed_required", nameof(request));
        if (!ProceduralGameGenerationModes.Supported.Contains(request.GenerationMode?.Trim() ?? string.Empty))
            throw new ArgumentException("generated_project.mode_unsupported", nameof(request));
        if (!_presetOptions.GetPresets().Any(preset => preset.PresetId == request.GenerationPresetId?.Trim()))
            throw new ArgumentException("generated_project.preset_unknown", nameof(request));
        if (!GeneratedProjectMechanicsProfiles.Supported.Contains(request.MechanicsProfileId?.Trim() ?? string.Empty, StringComparer.Ordinal))
            throw new ArgumentException("generated_project.profile_unknown", nameof(request));
    }

    private static void AssertResolvedRequest(CreateGameProjectRequest request, GenerationPresetOptions options)
    {
        if (!string.Equals(options.Seed, request.GenerationSeed.Trim(), StringComparison.Ordinal)
            || !string.Equals(options.Mode, request.GenerationMode.Trim(), StringComparison.Ordinal)
            || !string.Equals(options.PresetId, request.GenerationPresetId.Trim(), StringComparison.Ordinal))
            throw new InvalidOperationException("generated_project.options_silently_replaced");
    }

    private static GamePackageDefinition DeserializePackage(string json) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(json, JsonOptions)
        ?? throw new InvalidOperationException("generated_project.package_deserialization_failed");

    private static Task WriteAsync(string path, string value, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return File.WriteAllTextAsync(path, value, Utf8WithoutBom, cancellationToken);
    }

    private static string Resolve(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(fullRoot, path, "generated_project.path_escape");
        return path;
    }

    private static void EnsureContained(string root, string path, string message)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullPath = Path.GetFullPath(path);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!fullPath.Equals(fullRoot, comparison)
            && !fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException(message);
    }
}
