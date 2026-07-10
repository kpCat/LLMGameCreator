using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;

public sealed class ProductLineRuntimeVariantMatrixService :
    IProductLineRuntimeVariantMatrixWriter
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static ProductLineRuntimeVariantMatrixService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    private readonly ProductLineRuntimeVariantMaterializer _materializer;
    private readonly ProductLineRuntimeVariantValidator _validator;
    private readonly ProductLineRuntimeVariantScoringService _scoring;
    private readonly IRuntimeBackedPlayerCommandRoundtripService _roundtrip;

    public ProductLineRuntimeVariantMatrixService(
        IRuntimeBackedPlayerCommandRoundtripService roundtrip,
        ProductLineRuntimeVariantMaterializer? materializer = null,
        ProductLineRuntimeVariantValidator? validator = null,
        ProductLineRuntimeVariantScoringService? scoring = null)
    {
        _roundtrip = roundtrip;
        _materializer = materializer ?? new ProductLineRuntimeVariantMaterializer();
        _validator = validator ?? new ProductLineRuntimeVariantValidator();
        _scoring = scoring ?? new ProductLineRuntimeVariantScoringService();
    }

    public async Task<ProductLineRuntimeVariantMatrixWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        ProductLineRuntimeVariantMatrixRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        request ??= new ProductLineRuntimeVariantMatrixRequest();
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var outputRoot = Resolve(root, request.OutputRoot);
        var exportRoot = Resolve(root, ProductLineRuntimeVariantMatrixVocabulary.ExportPackageDirectory);
        var goal142Root = Resolve(root, ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory);
        GuardUnderRoot(root, outputRoot, nameof(request.OutputRoot));
        GuardUnderRoot(root, exportRoot, nameof(ProductLineRuntimeVariantMatrixVocabulary.ExportPackageDirectory));
        GuardGoal142WriteRoot(goal142Root, outputRoot, nameof(request.OutputRoot));

        var templatePath = ResolveInput(root, request.TemplatePackagePath, nameof(request.TemplatePackagePath));
        GuardNoManual(root, templatePath, nameof(request.TemplatePackagePath));
        var catalogPath = Resolve(root, request.VariantCatalogPath);
        GuardGoal142WriteRoot(goal142Root, catalogPath, nameof(request.VariantCatalogPath));

        Directory.CreateDirectory(outputRoot);
        Directory.CreateDirectory(exportRoot);

        var sourceHashBefore = HashFile(templatePath);
        var templateJson = await File.ReadAllTextAsync(templatePath, Encoding.UTF8, cancellationToken)
            .ConfigureAwait(false);
        var catalog = File.Exists(catalogPath)
            ? ReadJson<ProductLineRuntimeVariantCatalogDocument>(catalogPath)
            : ProductLineRuntimeVariantCatalog.CreateDefault();
        ValidateCatalog(catalog);
        await WriteJsonAsync(catalogPath, catalog, cancellationToken).ConfigureAwait(false);

        var prepared = new List<PreparedCandidate>();
        foreach (var recipe in catalog.Variants)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var candidateRoot = Path.Combine(outputRoot, "candidates", recipe.CandidateId);
            var packagePath = Path.Combine(candidateRoot, "package.json");
            GuardGoal142WriteRoot(outputRoot, packagePath, "candidate package");
            Directory.CreateDirectory(candidateRoot);

            var materialized = _materializer.Materialize(templateJson, recipe);
            await WriteTextAsync(packagePath, materialized.PackageJson, cancellationToken).ConfigureAwait(false);
            var package = DeserializePackage(materialized.PackageJson);
            var sourceTemplateUnchanged = string.Equals(sourceHashBefore, HashFile(templatePath), StringComparison.Ordinal);
            var packageValidation = _validator.Validate(
                root,
                outputRoot,
                packagePath,
                Path.GetDirectoryName(templatePath)!,
                package,
                recipe,
                validJson: true,
                sourceTemplateUnchanged);
            var handoff = BuildCandidateHandoff(root, packagePath, recipe);
            var handoffPath = Path.Combine(candidateRoot, "candidate-handoff.json");
            await WriteJsonAsync(handoffPath, handoff, cancellationToken).ConfigureAwait(false);

            var roundtripRequest = BuildRoundtripRequest(root, packagePath, handoffPath, recipe);
            var roundtripResult = _roundtrip.Execute(package, roundtripRequest);
            prepared.Add(new PreparedCandidate(
                recipe,
                package,
                materialized.PackageJson,
                packagePath,
                HashFile(packagePath),
                materialized.MutationAudit,
                packageValidation,
                handoffPath,
                roundtripRequest,
                roundtripResult));
        }

        var baseline = prepared.Single(item => item.Recipe.RecipeId == "balanced_baseline");
        var baselineFinal = FinalSnapshot(baseline.RoundtripResult);
        var baselineHash = FinalStateHash(baseline.RoundtripResult);
        var rows = new List<ProductLineRuntimeVariantMatrixRow>();
        foreach (var item in prepared)
        {
            var outcome = BuildOutcomeSummary(item, baselineFinal, baselineHash);
            var score = _scoring.Score(item.Recipe, item.MutationAudit, item.PackageValidation, outcome);
            var row = new ProductLineRuntimeVariantMatrixRow
            {
                CandidateId = item.Recipe.CandidateId,
                RecipeId = item.Recipe.RecipeId,
                VariantKind = item.Recipe.VariantKind,
                PackagePath = Relative(root, item.PackagePath),
                PackageSha256 = item.PackageSha256,
                MutationAudit = item.MutationAudit,
                PackageValidation = item.PackageValidation,
                RuntimeOutcomeSummary = outcome,
                CandidateScore = score,
                Passed = item.PackageValidation.Passed
                         && item.MutationAudit.Passed
                         && outcome.RoundtripSemanticProofPassed
                         && (item.Recipe.RecipeId == "balanced_baseline"
                             || (outcome.RuntimeEffectObserved && outcome.RuntimeStateDistinctFromBaseline))
            };
            rows.Add(row);

            await WriteCandidateArtifactsAsync(root, outputRoot, item, outcome, score, cancellationToken)
                .ConfigureAwait(false);
        }

        var selected = SelectCandidate(rows);
        var distinctness = BuildDistinctnessProof(rows, sourceHashBefore, HashFile(templatePath), selected);
        var result = BuildResult(rows, selected, distinctness);
        var dashboard = BuildDashboard(result);
        var selectedPrepared = prepared.Single(item => item.Recipe.CandidateId == selected.CandidateId);
        var selectedHandoff = BuildSelectedHandoff(root, selected, selectedPrepared);
        var written = new List<string>();

        await WriteAggregateArtifactsAsync(
                root,
                outputRoot,
                exportRoot,
                catalog,
                dashboard,
                result,
                distinctness,
                selectedHandoff,
                rows,
                selectedPrepared,
                cancellationToken)
            .ConfigureAwait(false);

        written.AddRange(Directory
            .EnumerateFiles(outputRoot, "*", SearchOption.AllDirectories)
            .Select(path => Relative(root, path)));
        written.AddRange(Directory
            .EnumerateFiles(exportRoot, "*", SearchOption.AllDirectories)
            .Select(path => Relative(root, path)));

        return new ProductLineRuntimeVariantMatrixWriteResult
        {
            Dashboard = dashboard,
            MatrixResult = result,
            DistinctnessProof = distinctness,
            SelectedHandoff = selectedHandoff,
            ProceduralOutputDirectoryPath = outputRoot,
            ExportPackageDirectoryPath = exportRoot,
            WrittenFiles = written.Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static void ValidateCatalog(ProductLineRuntimeVariantCatalogDocument catalog)
    {
        var expected = ProductLineRuntimeVariantMatrixVocabulary.CandidateIds
            .ToHashSet(StringComparer.Ordinal);
        var actual = catalog.Variants.Select(item => item.CandidateId)
            .ToHashSet(StringComparer.Ordinal);
        if (catalog.Variants.Count != 4 || !actual.SetEquals(expected))
        {
            throw new InvalidOperationException("Goal142 catalog must contain exactly four required candidates.");
        }

        var metadataOnly = catalog.Variants.Any(item =>
            item.VariantKind.Contains("metadata_only", StringComparison.OrdinalIgnoreCase)
            || item.RecipeId.Contains("metadata_only", StringComparison.OrdinalIgnoreCase));
        if (metadataOnly)
        {
            throw new InvalidOperationException("Goal142 catalog must not contain metadata_only variants.");
        }
    }

    private static object BuildCandidateHandoff(
        string root,
        string packagePath,
        ProductLineRuntimeVariantRecipe recipe) =>
        new
        {
            schemaVersion = "product_line_runtime_variant_candidate_handoff_v1",
            goalId = ProductLineRuntimeVariantMatrixVocabulary.GoalId,
            candidateId = recipe.CandidateId,
            recipeId = recipe.RecipeId,
            variantKind = recipe.VariantKind,
            packagePath = Relative(root, packagePath),
            runtimeSignificant = recipe.RuntimeSignificant,
            projectionOnly = false,
            runtimeAuthority = true,
            accepted = false
        };

    private static RuntimeBackedPlayerCommandRoundtripRequest BuildRoundtripRequest(
        string root,
        string packagePath,
        string handoffPath,
        ProductLineRuntimeVariantRecipe recipe) =>
        new()
        {
            CandidateId = recipe.CandidateId,
            PackagePath = Relative(root, packagePath),
            HandoffPath = Relative(root, handoffPath),
            ControlsUxModelPath =
                "goal142-runtime-variant-matrix/" + recipe.CandidateId + "/runtime-control-model.json",
            ControlsUxResultPath =
                "goal142-runtime-variant-matrix/" + recipe.CandidateId + "/runtime-control-result.json",
            ControlsUxScriptPath = ProductLineRuntimeVariantMatrixVocabulary.ScriptPath,
            CommandLoopSnapshotsPath =
                "goal142-runtime-variant-matrix/" + recipe.CandidateId + "/canonical-command-loop-snapshots.json",
            CommandLoopResultPath =
                "goal142-runtime-variant-matrix/" + recipe.CandidateId + "/canonical-command-loop-result.json"
        };

    private static ProductLineRuntimeVariantRuntimeOutcomeSummary BuildOutcomeSummary(
        PreparedCandidate item,
        RuntimeBackedPlayerCommandRoundtripSnapshot baselineFinal,
        string baselineHash)
    {
        var final = FinalSnapshot(item.RoundtripResult);
        var finalHash = FinalStateHash(item.RoundtripResult);
        var hashDiffers = !string.Equals(finalHash, baselineHash, StringComparison.Ordinal);
        var inventoryDiffers = !string.Equals(
            final.InventorySummary,
            baselineFinal.InventorySummary,
            StringComparison.Ordinal);
        var combatDiffers = !string.Equals(
            final.CombatSummary,
            baselineFinal.CombatSummary,
            StringComparison.Ordinal);
        var craftPassed = HasCanonicalStep(item.RoundtripResult, "craft_healing_potion");
        var harvestPassed = HasCanonicalStep(item.RoundtripResult, "harvest_apple_tree");
        var transactionPassed = HasCanonicalStep(item.RoundtripResult, "execute_transaction");
        var combatPassed = HasCanonicalStep(item.RoundtripResult, "combat_round");
        var roundtripPassed = CorrectedRoundtripPassed(item.RoundtripResult);
        var observed = new List<string>();
        var runtimeEffectObserved = item.Recipe.RecipeId switch
        {
            "balanced_baseline" => roundtripPassed,
            "alchemy_focus" => inventoryDiffers && hashDiffers && craftPassed,
            "combat_focus" => combatDiffers && hashDiffers && combatPassed,
            "exploration_resource_focus" => inventoryDiffers && hashDiffers && harvestPassed && transactionPassed,
            _ => false
        };

        if (item.Recipe.RecipeId == "balanced_baseline" && roundtripPassed)
        {
            observed.Add("baseline runtime comparison row captured");
        }

        if (inventoryDiffers)
        {
            observed.Add("final inventory summary differs from baseline");
        }

        if (combatDiffers)
        {
            observed.Add("final combat summary differs from baseline");
        }

        if (hashDiffers)
        {
            observed.Add("final runtime state hash differs from baseline");
        }

        return new ProductLineRuntimeVariantRuntimeOutcomeSummary
        {
            CandidateId = item.Recipe.CandidateId,
            RecipeId = item.Recipe.RecipeId,
            VariantKind = item.Recipe.VariantKind,
            FinalStateHash = finalHash,
            FinalInventorySummary = final.InventorySummary,
            FinalCombatSummary = final.CombatSummary,
            FinalQuestSummary = final.QuestSummary,
            BaselineFinalStateHash = baselineHash,
            RuntimeEffectObserved = runtimeEffectObserved,
            RuntimeStateDistinctFromBaseline = hashDiffers,
            CraftRequestPassed = craftPassed,
            HarvestRequestPassed = harvestPassed,
            TransactionRequestPassed = transactionPassed,
            CombatRequestPassed = combatPassed,
            RoundtripSemanticProofPassed = roundtripPassed,
            ObservedRuntimeEffects = observed
        };
    }

    private static bool CorrectedRoundtripPassed(RuntimeBackedPlayerCommandRoundtripResult result) =>
        result.TotalControlRequestCount == 6
        && result.RuntimeRoutedRequestCount == 4
        && result.PresentationOnlyRequestCount == 2
        && result.RuntimeExecutedRequestCount == 4
        && result.PresentationOnlyRuntimeExecutionCount == 0
        && result.RequestResponseCorrelationPassed
        && result.SequentialCursorContinuityPassed
        && result.StateHashContinuityPassed
        && result.CopySummaryStateUnchanged
        && result.LoadModelStateUnchanged
        && result.PlayAllExecutedRemainingCommands
        && result.NoControlIntentMappedToUnrelatedGameplayCommand
        && result.RuntimeAuthority
        && !result.ProjectionOnly
        && !result.UnityGameplayTruth
        && result.RuntimeBackedPlayerCommandRoundtripPassed;

    private static bool HasCanonicalStep(
        RuntimeBackedPlayerCommandRoundtripResult result,
        string stepId) =>
        result.Snapshots.Any(snapshot =>
            string.Equals(snapshot.CanonicalStepId, stepId, StringComparison.Ordinal)
            && snapshot.RuntimeExecuted
            && snapshot.CorrelationPassed);

    private static RuntimeBackedPlayerCommandRoundtripSnapshot FinalSnapshot(
        RuntimeBackedPlayerCommandRoundtripResult result) =>
        result.Snapshots.LastOrDefault()
        ?? throw new InvalidOperationException("Runtime roundtrip produced no snapshots.");

    private static string FinalStateHash(RuntimeBackedPlayerCommandRoundtripResult result) =>
        FinalSnapshot(result).StateHashAfter;

    private static ProductLineRuntimeVariantMatrixRow SelectCandidate(
        IReadOnlyList<ProductLineRuntimeVariantMatrixRow> rows) =>
        rows.Where(item => item.CandidateScore.Eligible)
            .OrderByDescending(item => item.CandidateScore.Score)
            .ThenByDescending(item => item.CandidateScore.TieBreakPriority)
            .ThenBy(item => item.RecipeId, StringComparer.Ordinal)
            .ThenBy(item => item.CandidateId, StringComparer.Ordinal)
            .FirstOrDefault()
        ?? throw new InvalidOperationException("No eligible Goal142 candidate was produced.");

    private static ProductLineRuntimeVariantDistinctnessProof BuildDistinctnessProof(
        IReadOnlyList<ProductLineRuntimeVariantMatrixRow> rows,
        string sourceHashBefore,
        string sourceHashAfter,
        ProductLineRuntimeVariantMatrixRow selected)
    {
        var byRecipe = rows.ToDictionary(item => item.RecipeId, StringComparer.Ordinal);
        var focusRows = rows.Where(item => item.RecipeId != "balanced_baseline").ToList();
        var noMetadataOnlySelected = selected.RecipeId != "balanced_baseline"
                                     && selected.MutationAudit.OperationCount > 0
                                     && selected.RuntimeOutcomeSummary.RuntimeEffectObserved;
        return new ProductLineRuntimeVariantDistinctnessProof
        {
            CandidateCount = rows.Count,
            PassedCandidateCount = rows.Count(item => item.Passed),
            FailedCandidateCount = rows.Count(item => !item.Passed),
            RuntimeSignificantCandidateCount = rows.Count(item => item.MutationAudit.RuntimeSignificant),
            AllPackageHashesDistinct = rows.Select(item => item.PackageSha256)
                .Distinct(StringComparer.Ordinal)
                .Count() == rows.Count,
            AllMutationAuditsPassed = rows.All(item => item.MutationAudit.Passed),
            AllRoundtripSemanticProofsPassed =
                rows.All(item => item.RuntimeOutcomeSummary.RoundtripSemanticProofPassed),
            BaselineFinalStateHash = byRecipe["balanced_baseline"].RuntimeOutcomeSummary.FinalStateHash,
            AlchemyFinalStateHash = byRecipe["alchemy_focus"].RuntimeOutcomeSummary.FinalStateHash,
            CombatFinalStateHash = byRecipe["combat_focus"].RuntimeOutcomeSummary.FinalStateHash,
            ExplorationFinalStateHash = byRecipe["exploration_resource_focus"].RuntimeOutcomeSummary.FinalStateHash,
            DistinctFinalStateHashCount = rows.Select(item => item.RuntimeOutcomeSummary.FinalStateHash)
                .Distinct(StringComparer.Ordinal)
                .Count(),
            AlchemyRuntimeEffectObserved =
                byRecipe["alchemy_focus"].RuntimeOutcomeSummary.RuntimeEffectObserved,
            CombatRuntimeEffectObserved =
                byRecipe["combat_focus"].RuntimeOutcomeSummary.RuntimeEffectObserved,
            ExplorationRuntimeEffectObserved =
                byRecipe["exploration_resource_focus"].RuntimeOutcomeSummary.RuntimeEffectObserved,
            NoMetadataOnlyVariantAccepted =
                noMetadataOnlySelected
                && focusRows.All(item => item.MutationAudit.OperationCount > 0),
            SourceTemplateUnmodified = string.Equals(sourceHashBefore, sourceHashAfter, StringComparison.Ordinal),
            Passed = rows.Count == 4
                     && rows.All(item => item.Passed)
                     && rows.Select(item => item.RuntimeOutcomeSummary.FinalStateHash)
                         .Distinct(StringComparer.Ordinal)
                         .Count() >= 3
                     && noMetadataOnlySelected
                     && string.Equals(sourceHashBefore, sourceHashAfter, StringComparison.Ordinal)
        };
    }

    private static ProductLineRuntimeVariantMatrixResult BuildResult(
        IReadOnlyList<ProductLineRuntimeVariantMatrixRow> rows,
        ProductLineRuntimeVariantMatrixRow selected,
        ProductLineRuntimeVariantDistinctnessProof distinctness)
    {
        var diagnostics = new List<string>();
        if (!distinctness.Passed)
        {
            diagnostics.Add("goal142.distinctness_or_effect_proof_failed");
        }

        if (selected.RecipeId == "balanced_baseline")
        {
            diagnostics.Add("goal142.selected_candidate_must_not_default_to_baseline");
        }

        return new ProductLineRuntimeVariantMatrixResult
        {
            MatrixStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            CandidateCount = rows.Count,
            PassedCandidateCount = rows.Count(item => item.Passed),
            FailedCandidateCount = rows.Count(item => !item.Passed),
            RuntimeSignificantCandidateCount = rows.Count(item => item.MutationAudit.RuntimeSignificant),
            DistinctFinalStateHashCount = distinctness.DistinctFinalStateHashCount,
            AllPackageHashesDistinct = distinctness.AllPackageHashesDistinct,
            AllMutationAuditsPassed = distinctness.AllMutationAuditsPassed,
            AllRoundtripSemanticProofsPassed = distinctness.AllRoundtripSemanticProofsPassed,
            SourceTemplateUnmodified = distinctness.SourceTemplateUnmodified,
            ProjectionOnly = false,
            Accepted = false,
            SelectedCandidateId = selected.CandidateId,
            SelectedVariantKind = selected.VariantKind,
            SelectedScore = selected.CandidateScore.Score,
            Candidates = rows,
            Diagnostics = diagnostics
        };
    }

    private static ProductLineRuntimeVariantMatrixDashboard BuildDashboard(
        ProductLineRuntimeVariantMatrixResult result) =>
        new()
        {
            MatrixStatus = result.MatrixStatus,
            CandidateCount = result.CandidateCount,
            PassedCandidateCount = result.PassedCandidateCount,
            FailedCandidateCount = result.FailedCandidateCount,
            RuntimeSignificantCandidateCount = result.RuntimeSignificantCandidateCount,
            DistinctFinalStateHashCount = result.DistinctFinalStateHashCount,
            SelectedCandidateId = result.SelectedCandidateId,
            SelectedVariantKind = result.SelectedVariantKind,
            SelectedScore = result.SelectedScore,
            SourceTemplateUnmodified = result.SourceTemplateUnmodified,
            Accepted = false
        };

    private static ProductLineRuntimeVariantSelectedHandoff BuildSelectedHandoff(
        string root,
        ProductLineRuntimeVariantMatrixRow selected,
        PreparedCandidate item) =>
        new()
        {
            CandidateId = selected.CandidateId,
            RecipeId = selected.RecipeId,
            VariantKind = selected.VariantKind,
            PackagePath = ProductLineRuntimeVariantMatrixVocabulary.SelectedHandoffRelativePath
                .Replace("selected-runtime-variant-handoff.json", "package.json", StringComparison.Ordinal),
            PackageSha256 = selected.PackageSha256,
            RoundtripResultPath = MatrixPath(selected.CandidateId, "roundtrip-result.json"),
            RuntimeOutcomeSummaryPath = MatrixPath(selected.CandidateId, "runtime-outcome-summary.json"),
            FinalStateHash = selected.RuntimeOutcomeSummary.FinalStateHash,
            Score = selected.CandidateScore.Score,
            ScoreBreakdown = selected.CandidateScore.ScoreBreakdown,
            SelectionReason = "Selected by deterministic order: "
                              + selected.CandidateScore.SelectionTieBreakOrder
                              + ". Source package: "
                              + Relative(root, item.PackagePath),
            RuntimeSignificant = true,
            ProjectionOnly = false,
            RuntimeAuthority = true,
            Accepted = false
        };

    private static async Task WriteCandidateArtifactsAsync(
        string root,
        string outputRoot,
        PreparedCandidate item,
        ProductLineRuntimeVariantRuntimeOutcomeSummary outcome,
        ProductLineRuntimeVariantScore score,
        CancellationToken cancellationToken)
    {
        var candidateRoot = Path.Combine(outputRoot, "candidates", item.Recipe.CandidateId);
        var matrixRoot = Path.Combine(outputRoot, "matrix", item.Recipe.CandidateId);
        Directory.CreateDirectory(candidateRoot);
        Directory.CreateDirectory(matrixRoot);
        await WriteJsonAsync(
            Path.Combine(candidateRoot, "mutation-audit.json"),
            item.MutationAudit,
            cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(
            Path.Combine(candidateRoot, "package-validation.json"),
            item.PackageValidation,
            cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(
            Path.Combine(matrixRoot, "roundtrip-request.json"),
            item.RoundtripRequest,
            cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(
            Path.Combine(matrixRoot, "roundtrip-result.json"),
            item.RoundtripResult,
            cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(
            Path.Combine(matrixRoot, "roundtrip-snapshots.json"),
            item.RoundtripResult.Snapshots,
            cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(
            Path.Combine(matrixRoot, "runtime-outcome-summary.json"),
            outcome,
            cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(
            Path.Combine(matrixRoot, "candidate-score.json"),
            score,
            cancellationToken).ConfigureAwait(false);

        _ = root;
    }

    private static async Task WriteAggregateArtifactsAsync(
        string root,
        string outputRoot,
        string exportRoot,
        ProductLineRuntimeVariantCatalogDocument catalog,
        ProductLineRuntimeVariantMatrixDashboard dashboard,
        ProductLineRuntimeVariantMatrixResult result,
        ProductLineRuntimeVariantDistinctnessProof distinctness,
        ProductLineRuntimeVariantSelectedHandoff selectedHandoff,
        IReadOnlyList<ProductLineRuntimeVariantMatrixRow> rows,
        PreparedCandidate selectedPrepared,
        CancellationToken cancellationToken)
    {
        var selectedOutputRoot = Path.Combine(outputRoot, "selected-runtime-variant");
        var selectedExportRoot = Path.Combine(exportRoot, "selected-runtime-variant");
        Directory.CreateDirectory(selectedOutputRoot);
        Directory.CreateDirectory(selectedExportRoot);

        var aggregate = new Dictionary<string, object>
        {
            [ProductLineRuntimeVariantMatrixVocabulary.CatalogFileName] = catalog,
            [ProductLineRuntimeVariantMatrixVocabulary.DashboardFileName] = dashboard,
            [ProductLineRuntimeVariantMatrixVocabulary.MatrixResultFileName] = result,
            [ProductLineRuntimeVariantMatrixVocabulary.MutationSummaryFileName] = new
            {
                schemaVersion = "product_line_runtime_variant_mutation_summary_v1",
                goalId = ProductLineRuntimeVariantMatrixVocabulary.GoalId,
                mutationAuditCount = rows.Count,
                audits = rows.Select(item => item.MutationAudit).ToList()
            },
            [ProductLineRuntimeVariantMatrixVocabulary.DistinctnessProofFileName] = distinctness,
            [ProductLineRuntimeVariantMatrixVocabulary.ScoreboardFileName] = new
            {
                schemaVersion = "product_line_runtime_variant_scoreboard_v1",
                goalId = ProductLineRuntimeVariantMatrixVocabulary.GoalId,
                tieBreakOrder = "score desc, tieBreakPriority desc, recipeId asc, candidateId asc",
                selectedCandidateId = result.SelectedCandidateId,
                scores = rows.Select(item => item.CandidateScore).ToList()
            },
            [ProductLineRuntimeVariantMatrixVocabulary.NegativeProofFileName] = new
            {
                schemaVersion = "product_line_runtime_variant_negative_proof_v1",
                goalId = ProductLineRuntimeVariantMatrixVocabulary.GoalId,
                noMetadataOnlyVariantAccepted = distinctness.NoMetadataOnlyVariantAccepted,
                sourceTemplateUnmodified = distinctness.SourceTemplateUnmodified,
                noPresentationOnlyRuntimeExecution = rows.All(item =>
                    item.RuntimeOutcomeSummary.RoundtripSemanticProofPassed),
                noGoal131FallbackCandidate = rows.All(item =>
                    item.PackagePath.Contains(ProductLineRuntimeVariantMatrixVocabulary.ScenarioId, StringComparison.Ordinal)),
                noManualInputArtifacts = true,
                projectionOnly = false,
                runtimeAuthority = true
            },
            [ProductLineRuntimeVariantMatrixVocabulary.OneClickReportJsonFileName] = new
            {
                schemaVersion = "one_click_product_line_runtime_variant_matrix_report_v1",
                goalId = ProductLineRuntimeVariantMatrixVocabulary.GoalId,
                dashboard,
                selectedHandoff,
                distinctness,
                candidates = rows
            }
        };

        foreach (var item in aggregate.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            await WriteJsonAsync(Path.Combine(outputRoot, item.Key), item.Value, cancellationToken)
                .ConfigureAwait(false);
            await WriteJsonAsync(Path.Combine(exportRoot, item.Key), item.Value, cancellationToken)
                .ConfigureAwait(false);
        }

        await WriteTextAsync(
            Path.Combine(outputRoot, ProductLineRuntimeVariantMatrixVocabulary.OneClickReportMarkdownFileName),
            RenderReport(result, distinctness, selectedHandoff),
            cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(
            Path.Combine(exportRoot, ProductLineRuntimeVariantMatrixVocabulary.OneClickReportMarkdownFileName),
            RenderReport(result, distinctness, selectedHandoff),
            cancellationToken).ConfigureAwait(false);

        await WriteJsonAsync(
            Path.Combine(selectedOutputRoot, "selected-runtime-variant-handoff.json"),
            selectedHandoff,
            cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(
            Path.Combine(selectedOutputRoot, "package.json"),
            selectedPrepared.PackageJson,
            cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(
            Path.Combine(selectedOutputRoot, "runtime-outcome-summary.json"),
            rows.Single(item => item.CandidateId == selectedHandoff.CandidateId).RuntimeOutcomeSummary,
            cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(
            Path.Combine(selectedOutputRoot, "selection-rationale.md"),
            RenderSelectionRationale(selectedHandoff),
            cancellationToken).ConfigureAwait(false);

        await WriteJsonAsync(
            Path.Combine(selectedExportRoot, "selected-runtime-variant-handoff.json"),
            selectedHandoff,
            cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(
            Path.Combine(selectedExportRoot, "package.json"),
            selectedPrepared.PackageJson,
            cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(
            Path.Combine(selectedExportRoot, "runtime-outcome-summary.json"),
            rows.Single(item => item.CandidateId == selectedHandoff.CandidateId).RuntimeOutcomeSummary,
            cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(
            Path.Combine(selectedExportRoot, "selection-rationale.md"),
            RenderSelectionRationale(selectedHandoff),
            cancellationToken).ConfigureAwait(false);

        await WriteFileIndexAsync(root, outputRoot, cancellationToken).ConfigureAwait(false);
        await WriteFileIndexAsync(root, exportRoot, cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteFileIndexAsync(
        string root,
        string artifactRoot,
        CancellationToken cancellationToken)
    {
        var entries = Directory
            .EnumerateFiles(artifactRoot, "*", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(
                ProductLineRuntimeVariantMatrixVocabulary.FileIndexFileName,
                StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => new ProductLineRuntimeVariantFileIndexEntry
            {
                RelativePath = Relative(root, path),
                Role = RoleFor(path),
                Required = true,
                Sha256 = HashFile(path)
            })
            .ToList();
        var index = new ProductLineRuntimeVariantFileIndex
        {
            RootPath = Relative(root, artifactRoot),
            IndexedFileCount = entries.Count,
            Files = entries
        };
        await WriteJsonAsync(
            Path.Combine(artifactRoot, ProductLineRuntimeVariantMatrixVocabulary.FileIndexFileName),
            index,
            cancellationToken).ConfigureAwait(false);
    }

    private static string RoleFor(string path) =>
        path.Contains(Path.DirectorySeparatorChar + "selected-runtime-variant" + Path.DirectorySeparatorChar,
            StringComparison.Ordinal)
            ? "selected_runtime_variant_handoff"
            : path.Contains(Path.DirectorySeparatorChar + "matrix" + Path.DirectorySeparatorChar,
                StringComparison.Ordinal)
                ? "candidate_runtime_matrix"
                : path.Contains(Path.DirectorySeparatorChar + "candidates" + Path.DirectorySeparatorChar,
                    StringComparison.Ordinal)
                    ? "candidate_package_materialization"
                    : "aggregate_matrix_evidence";

    private static string RenderReport(
        ProductLineRuntimeVariantMatrixResult result,
        ProductLineRuntimeVariantDistinctnessProof distinctness,
        ProductLineRuntimeVariantSelectedHandoff handoff)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Goal142 Runtime-Significant Product-Line Variant Matrix");
        builder.AppendLine();
        builder.AppendLine("- matrixStatus: " + result.MatrixStatus);
        builder.AppendLine("- candidateCount: " + result.CandidateCount);
        builder.AppendLine("- passedCandidateCount: " + result.PassedCandidateCount);
        builder.AppendLine("- runtimeSignificantCandidateCount: " + result.RuntimeSignificantCandidateCount);
        builder.AppendLine("- distinctFinalStateHashCount: " + distinctness.DistinctFinalStateHashCount);
        builder.AppendLine("- selectedCandidateId: " + handoff.CandidateId);
        builder.AppendLine("- selectedVariantKind: " + handoff.VariantKind);
        builder.AppendLine("- selectedScore: " + handoff.Score);
        builder.AppendLine("- sourceTemplateUnmodified: " + result.SourceTemplateUnmodified.ToString().ToLowerInvariant());
        builder.AppendLine("- runtimeAuthority: true");
        builder.AppendLine("- projectionOnly: false");
        builder.AppendLine("- accepted: false");
        return builder.ToString();
    }

    private static string RenderSelectionRationale(ProductLineRuntimeVariantSelectedHandoff handoff)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Selected Runtime Variant");
        builder.AppendLine();
        builder.AppendLine("- candidateId: " + handoff.CandidateId);
        builder.AppendLine("- recipeId: " + handoff.RecipeId);
        builder.AppendLine("- variantKind: " + handoff.VariantKind);
        builder.AppendLine("- score: " + handoff.Score);
        builder.AppendLine("- finalStateHash: " + handoff.FinalStateHash);
        builder.AppendLine("- runtimeSignificant: true");
        builder.AppendLine("- runtimeAuthority: true");
        builder.AppendLine("- projectionOnly: false");
        builder.AppendLine("- accepted: false");
        builder.AppendLine();
        builder.AppendLine(handoff.SelectionReason);
        return builder.ToString();
    }

    private static GamePackageDefinition DeserializePackage(string json) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(json, JsonOptions)
        ?? throw new InvalidOperationException("Candidate package JSON could not be deserialized.");

    private static T ReadJson<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
        ?? throw new InvalidOperationException("JSON file could not be deserialized: " + path);

    private static async Task WriteJsonAsync(
        string path,
        object value,
        CancellationToken cancellationToken)
    {
        var text = JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;
        await WriteTextAsync(path, text, cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
    }

    private static string ResolveRepositoryRoot(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        if (!File.Exists(Path.Combine(root, "LLMGameCreator.sln")))
        {
            throw new InvalidOperationException("Repository root was not found: " + repositoryRootPath);
        }

        return root;
    }

    private static string Resolve(string root, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("Path is required.");
        }

        return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
    }

    private static string ResolveInput(string root, string path, string name)
    {
        var resolved = Resolve(root, path);
        GuardUnderRoot(root, resolved, name);
        if (!File.Exists(resolved))
        {
            throw new FileNotFoundException(name + " was not found.", resolved);
        }

        return resolved;
    }

    private static void GuardUnderRoot(string root, string path, string name)
    {
        if (!IsUnderDirectory(path, root))
        {
            throw new InvalidOperationException(name + " must stay under repository root.");
        }
    }

    private static void GuardNoManual(string root, string path, string name)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal142 refuses .llmgc/manual path for " + name + ".");
        }
    }

    private static void GuardGoal142WriteRoot(string goal142Root, string path, string name)
    {
        if (!IsUnderDirectory(path, goal142Root))
        {
            throw new InvalidOperationException(name + " must stay under the Goal142 output root.");
        }
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

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(stream);
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            builder.Append(b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static string MatrixPath(string candidateId, string fileName) =>
        ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory
        + "/matrix/"
        + candidateId
        + "/"
        + fileName;

    private sealed record PreparedCandidate(
        ProductLineRuntimeVariantRecipe Recipe,
        GamePackageDefinition Package,
        string PackageJson,
        string PackagePath,
        string PackageSha256,
        ProductLineRuntimeVariantMutationAudit MutationAudit,
        ProductLineRuntimeVariantPackageValidation PackageValidation,
        string HandoffPath,
        RuntimeBackedPlayerCommandRoundtripRequest RoundtripRequest,
        RuntimeBackedPlayerCommandRoundtripResult RoundtripResult);
}
