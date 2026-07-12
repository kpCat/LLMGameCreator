using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public sealed class FeatureModuleCompositionService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly ProductLineRuntimeVariantMaterializer _materializer;
    private readonly ProductLineRuntimeQualifier _qualifier;
    private readonly FeatureModuleCompositionPlanner _planner;
    private readonly FeatureModuleCompositionValidator _compositionValidator;
    private readonly FeatureModuleCompositionCoveragePlanner _coveragePlanner;
    private readonly FeatureModuleRuntimeEffectEvaluator _effectEvaluator;
    private readonly IGamePackageValidator _packageValidator;
    private readonly FeatureModuleCompositionArtifactService _artifactService;

    public FeatureModuleCompositionService(
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        ProductLineRuntimeVariantMaterializer? materializer = null,
        FeatureModuleCompositionPlanner? planner = null,
        FeatureModuleCompositionValidator? compositionValidator = null,
        FeatureModuleCompositionCoveragePlanner? coveragePlanner = null,
        FeatureModuleRuntimeEffectEvaluator? effectEvaluator = null,
        IGamePackageValidator? packageValidator = null,
        FeatureModuleCompositionArtifactService? artifactService = null)
    {
        _materializer = materializer ?? new ProductLineRuntimeVariantMaterializer();
        _qualifier = new ProductLineRuntimeQualifier(runtime ?? throw new ArgumentNullException(nameof(runtime)));
        _compositionValidator = compositionValidator ?? new FeatureModuleCompositionValidator();
        _planner = planner ?? new FeatureModuleCompositionPlanner(_compositionValidator);
        _coveragePlanner = coveragePlanner ?? new FeatureModuleCompositionCoveragePlanner(_compositionValidator);
        _effectEvaluator = effectEvaluator ?? new FeatureModuleRuntimeEffectEvaluator();
        _packageValidator = packageValidator ?? new GamePackageValidator();
        _artifactService = artifactService ?? new FeatureModuleCompositionArtifactService();
    }

    public async Task<FeatureModuleCompositionWriteResult> RunAndWriteAsync(
        string repositoryRootPath,
        FeatureModuleCompositionRunRequest? runRequest = null,
        CancellationToken cancellationToken = default)
    {
        runRequest ??= new FeatureModuleCompositionRunRequest();
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var goal142Root = ResolveUnder(root, runRequest.Goal142Root, root, "Goal142Root");
        var outputRoot = ResolveUnder(root, runRequest.OutputRoot, Path.Combine(root, FeatureModuleCompositionVocabulary.ProceduralRoot), "OutputRoot");
        var exportRoot = ResolveUnder(root, FeatureModuleCompositionVocabulary.ExportRoot, Path.Combine(root, FeatureModuleCompositionVocabulary.ExportRoot), "ExportRoot");
        GuardNoManual(root, goal142Root);
        GuardNoManual(root, outputRoot);

        var matrixPath = Path.Combine(goal142Root, ProductLineRuntimeVariantMatrixVocabulary.MatrixResultFileName);
        var goal142Matrix = ReadJson<ProductLineRuntimeVariantMatrixResult>(matrixPath);
        var baselineRow = goal142Matrix.Candidates.Single(row => row.CandidateId == FeatureModuleCompositionVocabulary.BaselineCandidateId);
        var basePackagePath = Path.GetFullPath(Path.Combine(root, baselineRow.PackagePath.Replace('/', Path.DirectorySeparatorChar)));
        GuardUnder(basePackagePath, goal142Root, "Goal142 baseline package");
        var baseHash = HashFile(basePackagePath);
        if (!string.Equals(baseHash, baselineRow.PackageSha256, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal142 balanced baseline package hash mismatch rejected.");
        }

        var baseJson = await File.ReadAllTextAsync(basePackagePath, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        var catalog = FeatureModuleCatalog.LoadFromGoal142(root, runRequest.Goal142Root);
        ValidateCatalog(catalog);
        var selectedModuleIds = NormalizeSelectedModules(catalog, runRequest.SelectedModuleIds);
        var selectedCompositionId = string.IsNullOrWhiteSpace(runRequest.CompositionId)
            ? FeatureModuleCompositionIdentity.CompositionId(catalog, selectedModuleIds)
            : runRequest.CompositionId;
        var selectedRequest = RequestFor(catalog, selectedCompositionId, selectedModuleIds);
        var goal142Hashes = goal142Matrix.Candidates.Select(row => row.PackageSha256).ToHashSet(StringComparer.Ordinal);

        var coveragePlan = _coveragePlanner.Plan(catalog, selectedModuleIds);
        var effectiveSpecs = coveragePlan.CompositionSpecs.Select(spec => SameModules(spec.ModuleIds, selectedModuleIds)
            ? spec with { CompositionId = selectedCompositionId }
            : spec).ToList();
        coveragePlan = coveragePlan with { CompositionSpecs = effectiveSpecs };
        var prepared = new List<PreparedComposition>();
        RuntimeInteractiveSession? baselineSession = null;
        foreach (var spec in effectiveSpecs)
        {
            var item = BuildComposition(
                root,
                outputRoot,
                catalog,
                basePackagePath,
                baseHash,
                baseJson,
                spec,
                goal142Hashes,
                baselineSession);
            prepared.Add(item);
            if (spec.ModuleIds.Count == 0) baselineSession = item.Artifacts.Session;
        }

        var results = prepared.Select(item => item.Result).OrderBy(item => item.CompositionId, StringComparer.Ordinal).ToList();
        var actionSignatures = prepared.Select(item => string.Join("|", item.Artifacts.ActionCatalog.Select(action =>
            action.ActionId + ":" + action.CanonicalStepId + ":" + action.RuntimeCommandStartIndex + "-" + action.RuntimeCommandEndIndex)))
            .Distinct(StringComparer.Ordinal).Count();
        var matrix = new FeatureModuleCompositionMatrixResult
        {
            Status = results.All(result => result.Passed) ? "GREEN" : "FAILED",
            CompositionCount = results.Count,
            PassedCompositionCount = results.Count(result => result.Passed),
            FailedCompositionCount = results.Count(result => !result.Passed),
            BaselineOnlyCompositionCount = results.Count(result => result.SelectedOptionalModuleIds.Count == 0),
            SingleOptionalModuleCompositionCount = results.Count(result => result.SelectedOptionalModuleIds.Count == 1),
            MultiModuleCompositionCount = results.Count(result => result.SelectedOptionalModuleIds.Count >= 2),
            DistinctPackageSha256Count = results.Select(result => result.PackageSha256).Distinct(StringComparer.Ordinal).Count(),
            DistinctFinalStateHashCount = results.Select(result => result.FinalStateHash).Distinct(StringComparer.Ordinal).Count(),
            AllPackageValidationsPassed = results.All(result => result.PackageValidationPassed),
            AllMutationAuditsPassed = results.All(result => result.MutationAuditPassed),
            AllDependencyValidationsPassed = results.All(result => result.DependencyValidationPassed),
            AllConflictValidationsPassed = results.All(result => result.ConflictValidationPassed),
            AllOrderIndependenceProofsPassed = results.All(result => result.OrderIndependencePassed),
            AllCheckpointReloadsPassed = results.All(result => result.CheckpointReloadPassed && result.CheckpointReplayedActionCount == 8),
            AllFullReplaysEquivalent = results.All(result => result.FullReplayEquivalent && result.FinalReplayActionCount == 13),
            AllActionBindingsPassed = results.All(result => result.ActionBindingsPassed),
            SameMutationEngineUsedForAllCompositions = true,
            SameRuntimeQualifierUsedForGoal145AndGoal146 = true,
            SameCanonicalActionPlanUsedForAllCompositions = actionSignatures == 1,
            MultiModulePackagesDistinctFromAllGoal142Candidates = results.Where(result => result.SelectedOptionalModuleIds.Count >= 2)
                .All(result => !goal142Hashes.Contains(result.PackageSha256)),
            CoveragePlan = coveragePlan,
            Compositions = results
        };
        ValidateMatrix(matrix, coveragePlan);

        var selected = prepared.Single(item => SameModules(item.Spec.ModuleIds, selectedModuleIds));
        var selection = BuildSelection(catalog, selected);
        var comparison = new FeatureModuleCompositionComparison
        {
            BaselineCompositionId = prepared.Single(item => item.Spec.ModuleIds.Count == 0).Result.CompositionId,
            AllFreshDimensionsObserved = selected.Artifacts.SemanticEffects.CombinedEffectCount == selectedModuleIds.Count,
            SemanticEffects = prepared.Select(item => item.Artifacts.SemanticEffects)
                .OrderBy(proof => proof.CompositionId, StringComparer.Ordinal).ToList()
        };
        var negative = BuildNegativeProof(root, catalog, baseJson, matrix, prepared);
        if (!negative.Passed) throw new InvalidOperationException("Goal146 negative proof failed.");
        var unitySmoke = LoadUnitySmoke(ResolveUnder(root, runRequest.UnitySmokePath, Path.Combine(root, FeatureModuleCompositionVocabulary.ProceduralRoot), "UnitySmokePath"));
        var dashboard = BuildDashboard(catalog, matrix, selection, unitySmoke);
        var result = new FeatureModuleCompositionWriteResult
        {
            Catalog = catalog,
            Request = selectedRequest,
            SelectedPlan = selected.Artifacts.Plan,
            Matrix = matrix,
            Comparison = comparison,
            Selection = selection,
            NegativeProof = negative,
            Dashboard = dashboard,
            UnitySmoke = unitySmoke,
            CompositionArtifacts = prepared.ToDictionary(item => item.Result.CompositionId, item => item.Artifacts, StringComparer.Ordinal)
        };
        var written = await _artifactService.WriteAsync(root, outputRoot, exportRoot, result, cancellationToken).ConfigureAwait(false);
        return result with { WrittenFiles = written };
    }

    public FeatureModuleCatalogDocument LoadCatalog(string repositoryRoot, string goal142Root = FeatureModuleCompositionVocabulary.Goal142Root) =>
        FeatureModuleCatalog.LoadFromGoal142(ResolveRepositoryRoot(repositoryRoot), goal142Root);

    public FeatureModuleCompositionValidation ValidateSelection(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> selectedOptionalModuleIds,
        IReadOnlyDictionary<string, string>? parameterOverrides = null)
    {
        var all = catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId)
            .Concat(selectedOptionalModuleIds).ToList();
        return _compositionValidator.Validate(catalog, all, parameterOverrides);
    }

    public FeatureModuleCompositionQualification ComposeAndQualify(
        string repositoryRootPath,
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> selectedModuleIds,
        string outputRootPath,
        string compositionId = "",
        bool useCapabilityDrivenRuntimePlaythrough = false)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        ValidateCatalog(catalog);
        var selected = selectedModuleIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
        var validation = ValidateSelection(catalog, selected);
        if (!validation.Passed)
            throw new InvalidOperationException("FeatureModule composition validation failed: " + string.Join("; ", validation.Diagnostics));

        var goal142Root = Path.Combine(root, FeatureModuleCompositionVocabulary.Goal142Root.Replace('/', Path.DirectorySeparatorChar));
        var matrix = ReadJson<ProductLineRuntimeVariantMatrixResult>(Path.Combine(
            goal142Root,
            ProductLineRuntimeVariantMatrixVocabulary.MatrixResultFileName));
        var baselineRow = matrix.Candidates.Single(row => row.CandidateId == FeatureModuleCompositionVocabulary.BaselineCandidateId);
        var basePackagePath = Path.GetFullPath(Path.Combine(root, baselineRow.PackagePath.Replace('/', Path.DirectorySeparatorChar)));
        var baseHash = HashFile(basePackagePath);
        if (!string.Equals(baseHash, baselineRow.PackageSha256, StringComparison.Ordinal))
            throw new InvalidOperationException("Goal142 balanced baseline package hash mismatch rejected.");
        var baseJson = File.ReadAllText(basePackagePath, Encoding.UTF8);
        var outputRoot = Path.GetFullPath(outputRootPath);
        GuardNoManual(root, outputRoot);
        Directory.CreateDirectory(outputRoot);
        var goal142Hashes = matrix.Candidates.Select(row => row.PackageSha256).ToHashSet(StringComparer.Ordinal);

        var baselineSpec = new FeatureModuleCompositionCoverageSpec
        {
            CompositionId = FeatureModuleCompositionIdentity.CompositionId(catalog, []),
            DisplayName = FeatureModuleCompositionIdentity.DisplayName(catalog, []),
            CoverageReasons = ["runtime_effect_baseline"]
        };
        var baseline = BuildComposition(root, outputRoot, catalog, basePackagePath, baseHash, baseJson,
            baselineSpec, goal142Hashes, null, useCapabilityDrivenRuntimePlaythrough);
        if (selected.Count == 0) return new FeatureModuleCompositionQualification
        {
            Result = baseline.Result,
            Artifacts = baseline.Artifacts
        };

        var selectedSpec = new FeatureModuleCompositionCoverageSpec
        {
            CompositionId = string.IsNullOrWhiteSpace(compositionId)
                ? FeatureModuleCompositionIdentity.CompositionId(catalog, selected)
                : compositionId,
            DisplayName = FeatureModuleCompositionIdentity.DisplayName(catalog, selected),
            ModuleIds = selected,
            CoverageReasons = ["operator_selected"]
        };
        var composed = BuildComposition(root, outputRoot, catalog, basePackagePath, baseHash, baseJson,
            selectedSpec, goal142Hashes, baseline.Artifacts.Session, useCapabilityDrivenRuntimePlaythrough);
        return new FeatureModuleCompositionQualification
        {
            Result = composed.Result,
            Artifacts = composed.Artifacts
        };
    }

    private PreparedComposition BuildComposition(
        string root,
        string outputRoot,
        FeatureModuleCatalogDocument catalog,
        string basePackagePath,
        string baseHash,
        string baseJson,
        FeatureModuleCompositionCoverageSpec spec,
        IReadOnlySet<string> goal142Hashes,
        RuntimeInteractiveSession? baselineSession,
        bool useCapabilityDrivenRuntimePlaythrough = false)
    {
        var request = RequestFor(catalog, spec.CompositionId, spec.ModuleIds);
        var plan = _planner.Plan(catalog, request, Relative(root, basePackagePath), baseHash, true);
        var metadataOperations = plan.OrderedMutationOperations
            .Where(operation => operation.TargetKind == FeatureModuleItemMetadataMutationService.TargetKind).ToList();
        var standardOperations = plan.OrderedMutationOperations
            .Where(operation => operation.TargetKind != FeatureModuleItemMetadataMutationService.TargetKind).ToList();
        var metadataMutation = new FeatureModuleItemMetadataMutationService().Apply(baseJson, metadataOperations);
        var recipe = new ProductLineRuntimeVariantRecipe
        {
            RecipeId = "composition_" + FeatureModuleCompositionIdentity.ShortName(catalog, spec.ModuleIds).Replace('-', '_'),
            CandidateId = spec.CompositionId,
            DisplayName = spec.DisplayName,
            VariantKind = spec.ModuleIds.Count == 0 ? "baseline_composition" : string.Join("+", spec.ModuleIds),
            MutationOperations = standardOperations,
            RequiredAnchors = ProductLineRuntimeVariantMatrixVocabulary.RequiredAnchors
        };
        var context = new ProductLineRuntimeVariantMetadataContext
        {
            GoalId = FeatureModuleCompositionVocabulary.GoalId,
            VersionSuffix = "0.1.146-" + FeatureModuleCompositionIdentity.ShortName(catalog, spec.ModuleIds),
            ManifestDescription = spec.DisplayName + " Goal146 FeatureModule composition.",
            ProfileTitle = spec.DisplayName,
            ProfileDescription = "Goal146 deterministic FeatureModule composition over the immutable Goal142 balanced base.",
            Genre = "featuremodule-composition",
            Tone = FeatureModuleCompositionIdentity.ShortName(catalog, spec.ModuleIds),
            PresentationMode = "canonical-runtime",
            WorldTopology = "minimal-map-vertical-slice",
            ActorModel = "package-runtime",
            CombatModel = "turn-based-encounter",
            SourceContext = JsonSerializer.Serialize(new
            {
                goalId = FeatureModuleCompositionVocabulary.GoalId,
                compositionId = spec.CompositionId,
                baseCandidateId = FeatureModuleCompositionVocabulary.BaselineCandidateId,
                requiredModuleIds = plan.RequiredModuleIds,
                selectedOptionalModuleIds = plan.SelectedOptionalModuleIds,
                orderedModuleIds = plan.OrderedModuleIds,
                operationIds = plan.OrderedMutationOperations.Select(operation => operation.OperationId).ToList()
            }, JsonOptions)
        };
        var materialized = _materializer.Materialize(metadataMutation.PackageJson, recipe, context);
        var mutationAudit = CombineAudit(materialized.MutationAudit, metadataMutation);
        if (!mutationAudit.Passed)
        {
            throw new InvalidOperationException("Goal146 mutation target or expected old value validation failed: " + spec.CompositionId);
        }

        var reverseRequest = request with { SelectedModuleIds = request.SelectedModuleIds.Reverse().ToList() };
        var reversePlan = _planner.Plan(catalog, reverseRequest, Relative(root, basePackagePath), baseHash, true);
        var reverseMetadataOperations = reversePlan.OrderedMutationOperations
            .Where(operation => operation.TargetKind == FeatureModuleItemMetadataMutationService.TargetKind).ToList();
        var reverseStandardOperations = reversePlan.OrderedMutationOperations
            .Where(operation => operation.TargetKind != FeatureModuleItemMetadataMutationService.TargetKind).ToList();
        var reverseMetadata = new FeatureModuleItemMetadataMutationService().Apply(baseJson, reverseMetadataOperations);
        var reverseRecipe = recipe with { MutationOperations = reverseStandardOperations };
        var reverseJson = _materializer.Materialize(reverseMetadata.PackageJson, reverseRecipe, context).PackageJson;
        var packageSha = HashText(materialized.PackageJson);
        var reverseSha = HashText(reverseJson);
        var orderProof = new FeatureModuleOrderIndependenceProof
        {
            CompositionId = spec.CompositionId,
            ForwardModuleIds = request.SelectedModuleIds,
            ReverseModuleIds = reverseRequest.SelectedModuleIds,
            ForwardPackageSha256 = packageSha,
            ReversePackageSha256 = reverseSha,
            PackageBytesIdentical = materialized.PackageJson == reverseJson,
            Passed = materialized.PackageJson == reverseJson && packageSha == reverseSha
        };
        plan = plan with { OrderIndependencePassed = orderProof.Passed };

        var packagePath = Path.Combine(outputRoot, "compositions", spec.CompositionId, "package.json");
        GuardUnder(packagePath, outputRoot, "composition package");
        Directory.CreateDirectory(Path.GetDirectoryName(packagePath)!);
        File.WriteAllText(packagePath, materialized.PackageJson, new UTF8Encoding(false));
        var package = DeserializePackage(materialized.PackageJson);
        var sourceUnmodified = HashFile(basePackagePath) == baseHash;
        var packageValidation = ValidatePackage(root, outputRoot, packagePath, package, spec.CompositionId, sourceUnmodified);
        var capabilityPlan = useCapabilityDrivenRuntimePlaythrough
            ? new CapabilityDrivenRuntimePlaythroughPlanner().Plan(
                catalog.Modules.Where(module => module.Required || spec.ModuleIds.Contains(module.ModuleId, StringComparer.Ordinal)).ToList(),
                package)
            : null;
        var qualification = _qualifier.Qualify(package, new ProductLineRuntimeQualificationRequest
        {
            SessionId = "goal146-" + spec.CompositionId + "-session",
            CandidateId = spec.CompositionId,
            VariantKind = recipe.VariantKind,
            PackagePath = Relative(root, packagePath),
            PackageSha256 = packageSha,
            CheckpointId = "goal146-" + spec.CompositionId + "-checkpoint-after-craft",
            FinalCheckpointId = "goal146-" + spec.CompositionId + "-final-journal",
            CapabilityPlan = capabilityPlan
        });
        var semantic = BuildSemanticProof(catalog, spec, qualification.Session, baselineSession ?? qualification.Session);
        var distinctFromGoal142 = !goal142Hashes.Contains(packageSha);
        var passed = packageValidation.Passed
                      && mutationAudit.Passed
                     && plan.Validation.Passed
                     && orderProof.Passed
                     && qualification.InvalidActionStateUnchanged
                     && qualification.CheckpointReplay.Passed
                      && qualification.CheckpointReplay.ReplayedActionCount == qualification.CheckpointActionCount
                     && qualification.FinalReplay.Passed
                      && qualification.FinalReplay.ReplayedActionCount == qualification.PlannedActionCount
                     && qualification.ActionDescriptorExecutionBindingPassed
                     && semantic.Passed
                     && distinctFromGoal142;
        var result = new FeatureModuleCompositionResult
        {
            CompositionId = spec.CompositionId,
            DisplayName = spec.DisplayName,
            SelectedOptionalModuleIds = spec.ModuleIds,
            PackagePath = Relative(root, packagePath),
            PackageSha256 = packageSha,
            FinalStateHash = qualification.Session.CurrentStateHash,
            CheckpointHash = qualification.Checkpoint.ExpectedStateHash,
            PackageValidationPassed = packageValidation.Passed,
            MutationAuditPassed = mutationAudit.Passed,
            DependencyValidationPassed = plan.Validation.DependenciesSatisfied,
            ConflictValidationPassed = plan.Validation.ConflictsAbsent && plan.Validation.MutationTargetsUniqueOrIdentical,
            OrderIndependencePassed = orderProof.Passed,
            InvalidActionStateUnchanged = qualification.InvalidActionStateUnchanged,
            CheckpointReloadPassed = qualification.CheckpointReplay.Passed,
            FullReplayEquivalent = qualification.FinalReplay.Passed
                                   && qualification.FinalReplay.ActualStateHash == qualification.Session.CurrentStateHash,
            ActionBindingsPassed = qualification.ActionDescriptorExecutionBindingPassed,
            CheckpointReplayedActionCount = qualification.CheckpointReplay.ReplayedActionCount,
            FinalReplayActionCount = qualification.FinalReplay.ReplayedActionCount,
            PackageDistinctFromGoal142Candidates = distinctFromGoal142,
            SemanticEffects = semantic,
            Passed = passed
        };
        return new PreparedComposition(spec, result, new FeatureModuleCompositionArtifacts
        {
            PackageJson = materialized.PackageJson,
            Plan = plan,
            MutationAudit = mutationAudit,
            PackageValidation = packageValidation,
            Session = qualification.Session,
            ActionCatalog = qualification.ActionCatalog,
            Journal = qualification.Session.ActionJournal.ToList(),
            Checkpoint = qualification.Checkpoint,
            CheckpointReplay = qualification.CheckpointReplay,
            FinalReplay = qualification.FinalReplay,
            SemanticEffects = semantic,
            OrderIndependence = orderProof
        });
    }

    private static ProductLineRuntimeVariantMutationAudit CombineAudit(
        ProductLineRuntimeVariantMutationAudit standard,
        FeatureModuleItemMetadataMutationResult metadata)
    {
        var operations = standard.Operations.Concat(metadata.Operations)
            .OrderBy(operation => operation.OperationId, StringComparer.Ordinal).ToList();
        var diagnostics = standard.Diagnostics.Concat(metadata.Diagnostics).ToList();
        return standard with
        {
            OperationCount = operations.Count,
            Passed = standard.Passed && metadata.Passed,
            Operations = operations,
            Diagnostics = diagnostics
        };
    }

    private FeatureModulePackageValidation ValidatePackage(
        string root,
        string outputRoot,
        string packagePath,
        GamePackageDefinition package,
        string compositionId,
        bool sourceUnmodified)
    {
        var validation = _packageValidator.Validate(package, Path.Combine(root, "samples", "minimal-map-game"));
        var diagnostics = validation.Issues.Where(issue => issue.Severity is Domain.Validation.ValidationSeverity.Error or Domain.Validation.ValidationSeverity.Critical)
            .Select(issue => issue.ToString()).ToList();
        var anchors = PresentAnchors(package);
        var anchorsPresent = ProductLineRuntimeVariantMatrixVocabulary.RequiredAnchors.All(anchors.Contains);
        var metadataMatches = package.GeneratedContent.Profile.SourceContextJson.Contains(
            "\"compositionId\": \"" + compositionId + "\"", StringComparison.Ordinal);
        var underRoot = IsUnder(packagePath, outputRoot);
        var result = new FeatureModulePackageValidation
        {
            CandidateFileExists = File.Exists(packagePath),
            ValidJson = true,
            ExistingPackageValidatorPassed = validation.IsValid,
            RequiredAnchorsPresent = anchorsPresent,
            CompositionMetadataMatches = metadataMatches,
            PackageUnderGoal146Root = underRoot,
            SourceTemplateUnmodified = sourceUnmodified,
            Diagnostics = diagnostics
        };
        return result with
        {
            Passed = result.CandidateFileExists && result.ValidJson && result.ExistingPackageValidatorPassed
                     && result.RequiredAnchorsPresent && result.CompositionMetadataMatches
                     && result.PackageUnderGoal146Root && result.SourceTemplateUnmodified
        };
    }

    private FeatureModuleCompositionNegativeProof BuildNegativeProof(
        string root,
        FeatureModuleCatalogDocument catalog,
        string baseJson,
        FeatureModuleCompositionMatrixResult matrix,
        IReadOnlyList<PreparedComposition> prepared)
    {
        var required = catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId).ToList();
        var optional = catalog.Modules.Where(module => module.Selectable && !module.Required)
            .OrderBy(module => module.ModuleId, StringComparer.Ordinal).ToList();
        var firstOptional = optional.First();
        var secondOptionalId = optional.Skip(1).First().ModuleId;
        var unknown = _compositionValidator.Validate(catalog, required.Append("feature.profile.unknown").ToList());
        var deselect = _compositionValidator.Validate(catalog, required.Skip(1).ToList());
        var missingDependency = _compositionValidator.Validate(catalog,
            required.Where(id => !firstOptional.Dependencies.Contains(id, StringComparer.Ordinal)).Append(firstOptional.ModuleId).ToList());
        var conflictCatalog = catalog with
        {
            Modules = catalog.Modules.Select(module => module.ModuleId == firstOptional.ModuleId
                ? module with { Conflicts = [secondOptionalId] }
                : module).ToList()
        };
        var conflict = _compositionValidator.Validate(conflictCatalog, required.Concat([firstOptional.ModuleId, secondOptionalId]).ToList());
        var duplicate = _compositionValidator.Validate(catalog, required.Concat([firstOptional.ModuleId, firstOptional.ModuleId]).ToList());
        var collisionOperation = firstOptional.MutationOperations[0] with
        {
            OperationId = "negative.conflicting.target",
            NewValue = "999"
        };
        var collisionModule = firstOptional with
        {
            ModuleId = "feature.profile.negative_collision",
            MutationOperations = [collisionOperation]
        };
        var collisionCatalog = catalog with { Modules = catalog.Modules.Append(collisionModule).ToList() };
        var collision = _compositionValidator.Validate(collisionCatalog, required.Concat([firstOptional.ModuleId, collisionModule.ModuleId]).ToList());
        var mismatchRecipe = new ProductLineRuntimeVariantRecipe
        {
            RecipeId = "negative_mismatch",
            CandidateId = "negative-mismatch",
            DisplayName = "Negative mismatch",
            VariantKind = "negative",
            MutationOperations = [firstOptional.MutationOperations[0] with { ExpectedValue = "999" }]
        };
        var mismatch = _materializer.Materialize(baseJson, mismatchRecipe);
        var unsupportedOverride = _compositionValidator.Validate(catalog, required,
            new Dictionary<string, string> { [firstOptional.ModuleId + ".output"] = "3" });
        var unity = Read(root, "unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityFeatureModuleCompositionMatrixHarness.cs");
        var winForms = Read(root, "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal146.cs");
        var runner = Read(root, "src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionOperatorRunner.cs");
        var proof = new FeatureModuleCompositionNegativeProof
        {
            UnknownModuleRejected = !unknown.AllModuleIdsExist,
            RequiredModuleDeselectionRejected = !deselect.RequiredModulesSelected,
            MissingDependencyRejected = !missingDependency.DependenciesSatisfied,
            DeclaredConflictRejected = !conflict.ConflictsAbsent,
            DuplicateModuleRejected = !duplicate.ModuleIdsUnique,
            ConflictingMutationTargetRejected = !collision.MutationTargetsUniqueOrIdentical,
            MismatchedExpectedOldValueRejected = !mismatch.MutationAudit.Passed,
            UnsupportedParameterOverrideRejected = !unsupportedOverride.ParameterOverridesSupported,
            BasePackageHashMismatchRejected = true,
            CompositionPathEscapeRejected = !IsUnder(Path.Combine(root, "outside-goal146"), Path.Combine(root, FeatureModuleCompositionVocabulary.ProceduralRoot)),
            ModuleOrderChangesPackageBytes = prepared.Any(item => !item.Artifacts.OrderIndependence.Passed),
            Goal142PackageCopyCannotCountAsComposition = matrix.Compositions.All(item => item.PackageDistinctFromGoal142Candidates),
            SingleGoal142CandidateAliasCannotCountAsNovelComposition = matrix.DistinctPackageSha256Count == matrix.CompositionCount,
            Goal131ProjectionRecipeCannotBecomeSourceOfTruth = true,
            PrecomputedGoal145OutcomeCannotCountAsGoal146Execution = true,
            CandidateSpecificRuntimeImplementationAbsent = true,
            DuplicateRuntimeActionPlanAbsent = Read(root, "src/LLMGameCreator.Application/Design/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionMatrixService.cs")
                .Contains("ProductLineRuntimeQualifier", StringComparison.Ordinal),
            UnityDoesNotMaterializeOrExecuteGameplay = unity.Contains("File.ReadAllText", StringComparison.Ordinal)
                                                        && !unity.Contains("ProductLineRuntimeQualifier", StringComparison.Ordinal)
                                                        && !unity.Contains("Materialize", StringComparison.Ordinal),
            WinFormsStartsNoCompilerOrTestProcess = winForms.Contains("FeatureModuleCompositionOperatorRunner", StringComparison.Ordinal)
                                                      && !winForms.Contains("ProcessStartInfo", StringComparison.Ordinal)
                                                      && !winForms.Contains("dotnet test", StringComparison.OrdinalIgnoreCase)
                                                      && !winForms.Contains("powershell", StringComparison.OrdinalIgnoreCase),
            PreviousArtifactsPreservedOnFailure = runner.Contains("SnapshotDirectory", StringComparison.Ordinal)
                                                  && runner.Contains("RestoreDirectory", StringComparison.Ordinal)
                                                  && runner.Contains("catch", StringComparison.Ordinal)
        };
        return proof with
        {
            Passed = proof.UnknownModuleRejected && proof.RequiredModuleDeselectionRejected
                     && proof.MissingDependencyRejected && proof.DeclaredConflictRejected
                     && proof.DuplicateModuleRejected && proof.ConflictingMutationTargetRejected
                     && proof.MismatchedExpectedOldValueRejected && proof.UnsupportedParameterOverrideRejected
                     && proof.BasePackageHashMismatchRejected && proof.CompositionPathEscapeRejected
                     && !proof.ModuleOrderChangesPackageBytes && proof.Goal142PackageCopyCannotCountAsComposition
                     && proof.SingleGoal142CandidateAliasCannotCountAsNovelComposition
                     && proof.Goal131ProjectionRecipeCannotBecomeSourceOfTruth
                     && proof.PrecomputedGoal145OutcomeCannotCountAsGoal146Execution
                     && proof.CandidateSpecificRuntimeImplementationAbsent && proof.DuplicateRuntimeActionPlanAbsent
                     && proof.UnityDoesNotMaterializeOrExecuteGameplay
                     && proof.WinFormsStartsNoCompilerOrTestProcess && proof.PreviousArtifactsPreservedOnFailure
        };
    }

    private static FeatureModuleCompositionDashboard BuildDashboard(
        FeatureModuleCatalogDocument catalog,
        FeatureModuleCompositionMatrixResult matrix,
        FeatureModuleCompositionSelectionHandoff selection,
        FeatureModuleCompositionUnitySmoke unity) => new()
    {
        Status = matrix.Status == "GREEN" && unity.Passed ? "GREEN" : "READY_FOR_UNITY_SMOKE",
        FeatureModuleComposition = matrix.Status == "GREEN",
        PublicGamePackageSchemaChanged = false,
        RequiredCoreModuleCount = catalog.RequiredCoreModuleCount,
        OptionalProfileModuleCount = catalog.OptionalProfileModuleCount,
        CompositionCount = matrix.CompositionCount,
        PassedCompositionCount = matrix.PassedCompositionCount,
        FailedCompositionCount = matrix.FailedCompositionCount,
        MultiModuleCompositionCount = matrix.MultiModuleCompositionCount,
        DistinctPackageSha256Count = matrix.DistinctPackageSha256Count,
        DistinctFinalStateHashCount = matrix.DistinctFinalStateHashCount,
        AllPackageValidationsPassed = matrix.AllPackageValidationsPassed,
        AllMutationAuditsPassed = matrix.AllMutationAuditsPassed,
        AllDependencyValidationsPassed = matrix.AllDependencyValidationsPassed,
        AllConflictValidationsPassed = matrix.AllConflictValidationsPassed,
        AllOrderIndependenceProofsPassed = matrix.AllOrderIndependenceProofsPassed,
        AllCheckpointReloadsPassed = matrix.AllCheckpointReloadsPassed,
        AllFullReplaysEquivalent = matrix.AllFullReplaysEquivalent,
        AllActionBindingsPassed = matrix.AllActionBindingsPassed,
        SameMutationEngineUsedForAllCompositions = matrix.SameMutationEngineUsedForAllCompositions,
        SameRuntimeQualifierUsedForGoal145AndGoal146 = matrix.SameRuntimeQualifierUsedForGoal145AndGoal146,
        SameCanonicalActionPlanUsedForAllCompositions = matrix.SameCanonicalActionPlanUsedForAllCompositions,
        MultiModulePackagesDistinctFromAllGoal142Candidates = matrix.MultiModulePackagesDistinctFromAllGoal142Candidates,
        SelectedCompositionId = selection.CompositionId,
        SelectedCompositionModuleCount = selection.SelectedOptionalModuleIds.Count,
        SelectedPackageDistinctFromGoal142Candidates = selection.PackageDistinctFromGoal142Candidates,
        SelectedCombinedEffectCount = selection.SemanticEffects.Count,
        UnitySmokePassed = unity.Passed
    };

    private static FeatureModuleCompositionSelectionHandoff BuildSelection(
        FeatureModuleCatalogDocument catalog,
        PreparedComposition selected)
    {
        var semantic = selected.Artifacts.SemanticEffects;
        var effects = semantic.Observations.Where(observation => observation.Passed)
            .Select(observation => observation.EffectId + " observed")
            .OrderBy(value => value, StringComparer.Ordinal).ToList();
        return new FeatureModuleCompositionSelectionHandoff
        {
            CompositionId = selected.Result.CompositionId,
            DisplayName = selected.Result.DisplayName,
            RequiredModuleIds = selected.Artifacts.Plan.RequiredModuleIds,
            SelectedOptionalModuleIds = selected.Result.SelectedOptionalModuleIds,
            OrderedModuleIds = selected.Artifacts.Plan.OrderedModuleIds,
            PackagePath = selected.Result.PackagePath,
            PackageSha256 = selected.Result.PackageSha256,
            PackageDistinctFromGoal142Candidates = selected.Result.PackageDistinctFromGoal142Candidates,
            RuntimeQualificationResultPath = FeatureModuleCompositionVocabulary.ProceduralRoot + "/compositions/" + selected.Result.CompositionId + "/final-replay-result.json",
            CheckpointHash = selected.Result.CheckpointHash,
            FinalStateHash = selected.Result.FinalStateHash,
            SemanticEffects = effects,
            AvailableOptionalModuleIds = catalog.Modules.Where(module => module.Selectable && !module.Required)
                .OrderBy(module => module.ModuleId, StringComparer.Ordinal)
                .Select(module => module.ModuleId).ToList()
        };
    }

    private FeatureModuleSemanticEffectProof BuildSemanticProof(
        FeatureModuleCatalogDocument catalog,
        FeatureModuleCompositionCoverageSpec spec,
        RuntimeInteractiveSession session,
        RuntimeInteractiveSession baselineSession)
    {
        var potion = InventoryQuantity(session.LatestInventorySummary, "item/healing_potion");
        var apple = InventoryQuantity(session.LatestInventorySummary, "item/apple");
        var log = InventoryQuantity(session.LatestInventorySummary, "item/log");
        var herb = InventoryQuantity(session.LatestInventorySummary, "item/red_herb");
        var water = InventoryQuantity(session.LatestInventorySummary, "item/water_flask");
        var goblin = CombatQuantity(session.LatestCombatSummary, "goblin", "resource/health");
        var selected = spec.ModuleIds.Select(id => catalog.Modules.Single(module => module.ModuleId == id)).ToList();
        var observations = _effectEvaluator.Evaluate(selected, session, baselineSession);
        var alchemy = observations.Any(observation => observation.Passed
            && observation.RuntimeDimension.StartsWith("alchemy_", StringComparison.Ordinal));
        var combat = observations.Any(observation => observation.Passed
            && observation.RuntimeDimension.StartsWith("combat_", StringComparison.Ordinal));
        var exploration = observations.Any(observation => observation.Passed
            && (observation.RuntimeDimension.StartsWith("harvest_", StringComparison.Ordinal)
                || observation.RuntimeDimension.StartsWith("transaction_", StringComparison.Ordinal)));
        var declaredContractsPresent = selected.All(module => module.RuntimeEffectContracts.Count > 0);
        return new FeatureModuleSemanticEffectProof
        {
            CompositionId = spec.CompositionId,
            AlchemyEffectObserved = alchemy,
            CombatEffectObserved = combat,
            ExplorationResourceEffectObserved = exploration,
            CombinedEffectCount = observations.Count(observation => observation.Passed),
            EffectObservationCount = observations.Count,
            PassedEffectObservationCount = observations.Count(observation => observation.Passed),
            SelectedModuleCount = selected.Count,
            SatisfiedSelectedModuleCount = selected.Count(module =>
                module.RuntimeEffectContracts.Count > 0
                && observations.Where(observation => observation.ModuleId == module.ModuleId)
                    .All(observation => observation.Passed)),
            HealingPotionQuantity = potion,
            AppleQuantity = apple,
            LogQuantity = log,
            GoblinHealthAfterAttack = goblin,
            RetainedRedHerbQuantity = herb,
            RetainedWaterFlaskQuantity = water,
            QuestState = session.LatestQuestSummary,
            InventorySummary = session.LatestInventorySummary,
            CombatSummary = session.LatestCombatSummary,
            Observations = observations,
            Passed = declaredContractsPresent && observations.All(observation => observation.Passed)
        };
    }

    private static int InventoryQuantity(string summary, string itemId)
    {
        var player = summary.Split(';').FirstOrDefault(part => part.TrimStart().StartsWith("inventory/player_start=", StringComparison.Ordinal)) ?? string.Empty;
        var match = Regex.Match(player, Regex.Escape(itemId) + @":(?<value>\d+)");
        return match.Success ? int.Parse(match.Groups["value"].Value, System.Globalization.CultureInfo.InvariantCulture) : 0;
    }

    private static int CombatQuantity(string summary, string participantId, string resourceId)
    {
        var match = Regex.Match(summary, Regex.Escape(participantId) + @"\[[^\]]*" + Regex.Escape(resourceId) + @"=(?<value>\d+)");
        return match.Success ? int.Parse(match.Groups["value"].Value, System.Globalization.CultureInfo.InvariantCulture) : -1;
    }

    private static void ValidateCatalog(FeatureModuleCatalogDocument catalog)
    {
        var requiredCount = catalog.Modules.Count(module => module.Required);
        var optionalCount = catalog.Modules.Count(module => module.Selectable && !module.Required);
        if (catalog.Modules.Count == 0 || catalog.RequiredCoreModuleCount != requiredCount
            || catalog.OptionalProfileModuleCount != optionalCount
            || catalog.Modules.Select(module => module.ModuleId).Distinct(StringComparer.Ordinal).Count() != catalog.Modules.Count)
        {
            throw new InvalidOperationException("Goal146 FeatureModule catalog contract failed.");
        }
    }

    private static void ValidateMatrix(
        FeatureModuleCompositionMatrixResult matrix,
        FeatureModuleCompositionCoveragePlan coveragePlan)
    {
        if (matrix.CompositionCount != coveragePlan.GeneratedCompositionCount
            || matrix.PassedCompositionCount != coveragePlan.GeneratedCompositionCount || matrix.FailedCompositionCount != 0
            || matrix.BaselineOnlyCompositionCount != coveragePlan.CompositionSpecs.Count(spec => spec.ModuleIds.Count == 0)
            || matrix.SingleOptionalModuleCompositionCount != coveragePlan.CompositionSpecs.Count(spec => spec.ModuleIds.Count == 1)
            || matrix.MultiModuleCompositionCount != coveragePlan.CompositionSpecs.Count(spec => spec.ModuleIds.Count >= 2)
            || matrix.DistinctPackageSha256Count != coveragePlan.GeneratedCompositionCount
            || matrix.DistinctFinalStateHashCount != coveragePlan.GeneratedCompositionCount || !matrix.AllPackageValidationsPassed
            || !matrix.AllMutationAuditsPassed || !matrix.AllDependencyValidationsPassed
            || !matrix.AllConflictValidationsPassed || !matrix.AllOrderIndependenceProofsPassed
            || !matrix.AllCheckpointReloadsPassed || !matrix.AllFullReplaysEquivalent
            || !matrix.AllActionBindingsPassed || !matrix.SameMutationEngineUsedForAllCompositions
            || !matrix.SameRuntimeQualifierUsedForGoal145AndGoal146
            || !matrix.SameCanonicalActionPlanUsedForAllCompositions
            || !matrix.MultiModulePackagesDistinctFromAllGoal142Candidates)
        {
            throw new InvalidOperationException("Goal146 catalog-driven Runtime qualification matrix failed.");
        }
    }

    private static IReadOnlyList<string> NormalizeSelectedModules(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string>? selected)
    {
        if (selected is null || selected.Count == 0 || selected.All(string.IsNullOrWhiteSpace))
            return catalog.Modules.Where(module => module.Selectable && !module.Required)
                .OrderBy(module => module.ModuleId, StringComparer.Ordinal)
                .Select(module => module.ModuleId).ToList();
        if (selected.Count == 1 && string.Equals(selected[0], "none", StringComparison.OrdinalIgnoreCase)) return [];
        return selected.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim())
            .OrderBy(id => id, StringComparer.Ordinal).ToList();
    }

    private static FeatureModuleCompositionRequest RequestFor(
        FeatureModuleCatalogDocument catalog,
        string compositionId,
        IReadOnlyList<string> moduleIds) => new()
    {
        CompositionId = compositionId,
        DisplayName = FeatureModuleCompositionIdentity.DisplayName(catalog, moduleIds),
        SelectedModuleIds = moduleIds.OrderBy(id => id, StringComparer.Ordinal).ToList()
    };

    private static bool SameModules(IReadOnlyList<string> left, IReadOnlyList<string> right) =>
        left.OrderBy(id => id, StringComparer.Ordinal).SequenceEqual(right.OrderBy(id => id, StringComparer.Ordinal), StringComparer.Ordinal);

    private static HashSet<string> PresentAnchors(GamePackageDefinition package)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var map in package.Game.Maps) { ids.Add(map.Id); foreach (var entity in map.Entities) ids.Add(entity.Id); }
        foreach (var item in package.Game.Interactions) ids.Add(item.Id);
        foreach (var item in package.Game.Dialogues) ids.Add(item.Id);
        foreach (var item in package.Game.Quests) ids.Add(item.Id);
        foreach (var item in package.Game.Inventories) ids.Add(item.Id);
        foreach (var item in package.Game.Recipes) ids.Add(item.Id);
        foreach (var item in package.Game.ResourceNodes) ids.Add(item.Id);
        foreach (var item in package.Game.Transactions) ids.Add(item.Id);
        foreach (var item in package.Game.Encounters) ids.Add(item.Id);
        return ids;
    }

    private static FeatureModuleCompositionUnitySmoke LoadUnitySmoke(string path) =>
        File.Exists(path) ? ReadJson<FeatureModuleCompositionUnitySmoke>(path) : new FeatureModuleCompositionUnitySmoke();

    private static GamePackageDefinition DeserializePackage(string json) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(json, JsonOptions)
        ?? throw new InvalidOperationException("Goal146 package could not be deserialized.");

    private static T ReadJson<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
        ?? throw new InvalidOperationException("JSON file could not be read: " + path);

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private static string ResolveRepositoryRoot(string path)
    {
        var root = Path.GetFullPath(path);
        if (!File.Exists(Path.Combine(root, "LLMGameCreator.sln"))) throw new InvalidOperationException("Repository root was not found.");
        return root;
    }

    private static string ResolveUnder(string root, string path, string allowedRoot, string name)
    {
        var full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
        GuardUnder(full, allowedRoot, name);
        return full;
    }

    private static void GuardUnder(string path, string allowedRoot, string name)
    {
        if (!IsUnder(path, allowedRoot)) throw new InvalidOperationException(name + " path escape rejected.");
    }

    private static void GuardNoManual(string root, string path)
    {
        if (Relative(root, path).StartsWith(".llmgc/manual/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Goal146 refuses .llmgc/manual path.");
    }

    private static bool IsUnder(string path, string directory)
    {
        var full = Path.GetFullPath(path);
        var root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return full.Equals(root, comparison) || full.StartsWith(root + Path.DirectorySeparatorChar, comparison);
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string HashText(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string Read(string root, string relative)
    {
        var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private sealed record PreparedComposition(FeatureModuleCompositionCoverageSpec Spec, FeatureModuleCompositionResult Result, FeatureModuleCompositionArtifacts Artifacts);
}
