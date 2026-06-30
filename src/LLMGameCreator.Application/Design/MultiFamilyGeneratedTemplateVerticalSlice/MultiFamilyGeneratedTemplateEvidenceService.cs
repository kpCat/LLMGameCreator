using System.Text;

namespace LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class MultiFamilyGeneratedTemplateEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice";
    public const string CatalogJsonFileName = "family-template-catalog.json";
    public const string SharedLifecycleContractJsonFileName = "shared-lifecycle-contract.json";
    public const string MapPanelPlanJsonFileName = "family-loop-plan-map-panel-rpg.json";
    public const string SurvivalPlanJsonFileName = "family-loop-plan-survival-sandbox.json";
    public const string GridDungeonPlanJsonFileName = "family-loop-plan-first-person-grid-dungeon.json";
    public const string MapPanelProofJsonFileName = "family-simulatable-loop-proof-map-panel-rpg.json";
    public const string SurvivalProofJsonFileName = "family-simulatable-loop-proof-survival-sandbox.json";
    public const string GridDungeonProofJsonFileName = "family-simulatable-loop-proof-first-person-grid-dungeon.json";
    public const string RegressionMatrixJsonFileName = "multi-family-regression-matrix.json";
    public const string PreviewExportConsumptionMatrixJsonFileName = "preview-export-consumption-matrix.json";
    public const string InvalidMatrixJsonFileName = "invalid-family-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "multi-family-generated-template-vertical-slice-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly MultiFamilyGeneratedTemplateSourceLoader _sourceLoader;

    public MultiFamilyGeneratedTemplateEvidenceService(MultiFamilyGeneratedTemplateSourceLoader? sourceLoader = null)
    {
        _sourceLoader = sourceLoader ?? new MultiFamilyGeneratedTemplateSourceLoader();
    }

    public MultiFamilyGeneratedTemplateEvidenceResult Build(string projectRootPath)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var validator = new MultiFamilyGeneratedTemplateValidator();
        var catalog = new MultiFamilyGeneratedTemplateCatalog().Build(source);
        var builder = new MultiFamilyLifecycleBuilder();
        var plans = builder.BuildPlans(source, catalog);
        var loopRunner = new FamilySimulatableLoopRunner();
        var proofs = plans
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(loopRunner.Run)
            .ToList();
        var sharedContract = builder.BuildSharedContract(plans);
        var previewExport = builder.BuildPreviewExportConsumptionMatrix(source, plans);
        var regression = builder.BuildRegressionMatrix(plans, proofs, sharedContract, previewExport);
        var invalidMatrix = validator.BuildInvalidMatrix(catalog, plans, proofs, previewExport);

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [CatalogJsonFileName] = Serialize(catalog),
            [SharedLifecycleContractJsonFileName] = Serialize(sharedContract),
            [RegressionMatrixJsonFileName] = Serialize(regression),
            [PreviewExportConsumptionMatrixJsonFileName] = Serialize(previewExport),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };
        foreach (var plan in plans.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal))
        {
            artifactJson[PlanFileName(plan.FamilyId)] = Serialize(plan);
        }

        foreach (var proof in proofs.OrderBy(item => MultiFamilyGeneratedTemplateCatalog.OrderingKey(item.FamilyId), StringComparer.Ordinal))
        {
            artifactJson[LoopProofFileName(proof.FamilyId)] = Serialize(proof);
        }

        var diagnostics = MultiFamilyGeneratedTemplateValidator.SortDiagnostics(
            validator.ValidateCatalog(catalog)
                .Concat(plans.SelectMany(validator.ValidatePlan))
                .Concat(proofs.SelectMany(validator.ValidateProof))
                .Concat(validator.ValidateSharedContract(sharedContract))
                .Concat(validator.ValidatePreviewExportMatrix(previewExport)));

        var allRequiredProofPassed = diagnostics.All(item => item.Severity != "error")
            && catalog.FamilyCount == 3
            && plans.Count == 3
            && proofs.Count == 3
            && proofs.All(item => item.StateChanged && item.FamilySpecificMinimumsPassed && item.BlockedInvalidAction.Blocked)
            && sharedContract.Passed
            && previewExport.Passed
            && regression.Passed
            && invalidMatrix.Passed;
        var blocked = diagnostics.Any(item => item.Code.Contains(".boundary.", StringComparison.Ordinal)
            || item.Code.Contains(".architecture_fork.", StringComparison.Ordinal));
        var reportWithoutHash = new MultiFamilyGeneratedTemplateReport
        {
            Accepted = false,
            ImplementationStatus = allRequiredProofPassed ? "GREEN" : blocked ? "BLOCKED" : "FAILED",
            Goal040AcceptedByUserHandoff = true,
            FamilyCount = catalog.FamilyCount,
            SimulatableLoopProofCount = proofs.Count,
            SourceGoal037HybridExpansionConsumed = catalog.SourceGoal037HybridExpansionConsumed,
            SourceGoal038WorldMapConsumed = catalog.SourceGoal038WorldMapConsumed,
            SourceGoal039RuntimeTraversalConsumed = catalog.SourceGoal039RuntimeTraversalConsumed,
            SourceGoal040PreviewExportConsumed = catalog.SourceGoal040PreviewExportConsumed,
            SharedLifecycleContractPassed = sharedContract.Passed,
            InvalidMatrixPassed = invalidMatrix.Passed,
            PreviewExportConsumptionMatrixPassed = previewExport.Passed,
            MultiFamilyRegressionPassed = regression.Passed,
            CatalogHash = Hash(artifactJson[CatalogJsonFileName]),
            SharedLifecycleContractHash = Hash(artifactJson[SharedLifecycleContractJsonFileName]),
            RegressionMatrixHash = Hash(artifactJson[RegressionMatrixJsonFileName]),
            PreviewExportConsumptionMatrixHash = Hash(artifactJson[PreviewExportConsumptionMatrixJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new MultiFamilyGeneratedTemplateEvidenceResult
        {
            Catalog = catalog,
            SharedLifecycleContract = sharedContract,
            Plans = plans,
            LoopProofs = proofs,
            RegressionMatrix = regression,
            PreviewExportConsumptionMatrix = previewExport,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            ReportMarkdown = RenderReport(report, catalog, plans, proofs, sharedContract, regression, previewExport, invalidMatrix)
        };
    }

    public async Task<MultiFamilyGeneratedTemplateWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MultiFamilyGeneratedTemplateWriteResult> WriteAsync(
        string projectRootPath,
        MultiFamilyGeneratedTemplateEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var written = new List<string>();
        foreach (var file in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, file.Key);
            await File.WriteAllTextAsync(path, file.Value, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new MultiFamilyGeneratedTemplateWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    public static string PlanFileName(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => MapPanelPlanJsonFileName,
            "survival_sandbox" => SurvivalPlanJsonFileName,
            "first_person_grid_dungeon" => GridDungeonPlanJsonFileName,
            _ => $"family-loop-plan-{familyId}.json"
        };

    public static string LoopProofFileName(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => MapPanelProofJsonFileName,
            "survival_sandbox" => SurvivalProofJsonFileName,
            "first_person_grid_dungeon" => GridDungeonProofJsonFileName,
            _ => $"family-simulatable-loop-proof-{familyId}.json"
        };

    private static string RenderReport(
        MultiFamilyGeneratedTemplateReport report,
        FamilyTemplateCatalog catalog,
        IReadOnlyList<FamilyLifecyclePlan> plans,
        IReadOnlyList<FamilySimulatableLoopProof> proofs,
        SharedLifecycleContract sharedContract,
        MultiFamilyRegressionMatrix regression,
        PreviewExportConsumptionMatrix previewExport,
        InvalidFamilyDiagnosticsMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Multi-Family Generated Template Vertical Slice Report",
            string.Empty,
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            $"manualGate={report.ManualGate}",
            $"familyCount={report.FamilyCount}",
            $"simulatableLoopProofCount={report.SimulatableLoopProofCount}",
            $"sourceGoal040PreviewExportConsumed={report.SourceGoal040PreviewExportConsumed.ToString().ToLowerInvariant()}",
            $"sharedLifecycleContractPassed={report.SharedLifecycleContractPassed.ToString().ToLowerInvariant()}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- goal040AcceptedByUserHandoff: {report.Goal040AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"- goal040AcceptedGate: {report.Goal040AcceptedGate}",
            $"- sourceGoal037HybridExpansionConsumed: {report.SourceGoal037HybridExpansionConsumed.ToString().ToLowerInvariant()}",
            $"- sourceGoal038WorldMapConsumed: {report.SourceGoal038WorldMapConsumed.ToString().ToLowerInvariant()}",
            $"- sourceGoal039RuntimeTraversalConsumed: {report.SourceGoal039RuntimeTraversalConsumed.ToString().ToLowerInvariant()}",
            $"- previewExportConsumptionMatrixPassed: {report.PreviewExportConsumptionMatrixPassed.ToString().ToLowerInvariant()}",
            $"- multiFamilyRegressionPassed: {report.MultiFamilyRegressionPassed.ToString().ToLowerInvariant()}",
            $"- catalogHash: {report.CatalogHash}",
            $"- sharedLifecycleContractHash: {report.SharedLifecycleContractHash}",
            $"- regressionMatrixHash: {report.RegressionMatrixHash}",
            $"- previewExportConsumptionMatrixHash: {report.PreviewExportConsumptionMatrixHash}",
            $"- invalidMatrixHash: {report.InvalidMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Goal 040 preview/export payloads now feed three generated family lifecycle plans instead of stopping at family lens compatibility.",
            "Each family has an Application-owned simulatable before/after loop with ordered commands, events, changed markers, replay hash and a blocked invalid action.",
            "Goal 044, Goal 045 and Goal 046 intent is absorbed into this Goal 043 evidence because the three families share one lifecycle contract and differ only inside scoped family extensions.",
            string.Empty,
            "## Family catalog",
            string.Empty
        };
        lines.AddRange(catalog.Families.Select(item => $"- {item.FamilyId}: scenario={item.ScenarioId}, plan={item.LifecyclePlanFileName}, proof={item.LoopProofFileName}, payload={item.SourceGoal040PayloadFileName}, extension={item.FamilyExtensionSchemaId}"));
        lines.Add(string.Empty);
        lines.Add("## Shared lifecycle contract");
        lines.Add(string.Empty);
        lines.Add($"- passed: {sharedContract.Passed.ToString().ToLowerInvariant()}");
        lines.Add("- phases: " + string.Join(",", sharedContract.SharedPhaseIds));
        lines.AddRange(sharedContract.Families.Select(item => $"- {item.FamilyId}: onlyFamilyExtensionDiffers={item.OnlyFamilyExtensionDiffers.ToString().ToLowerInvariant()}, architectureForked={item.ArchitectureForked.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Family loop proofs");
        lines.Add(string.Empty);
        lines.AddRange(proofs
            .OrderBy(item => MultiFamilyGeneratedTemplateCatalog.OrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(item => $"- {item.FamilyId}: stateChanged={item.StateChanged.ToString().ToLowerInvariant()}, events={item.Events.Count}, changedMarkers={string.Join(",", item.ChangedMarkers)}, blockedInvalidAction={item.BlockedInvalidAction.Blocked.ToString().ToLowerInvariant()}, replayHash={item.ReplayDeterminismHash}"));
        lines.Add(string.Empty);
        lines.Add("## Preview/export consumption");
        lines.Add(string.Empty);
        lines.Add($"- passed: {previewExport.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(previewExport.Rows.Select(item => $"- {item.FamilyId}: payload={item.Goal040PayloadFileName}, lensFound={item.FamilyLensFound.ToString().ToLowerInvariant()}, transformed={item.TransformedIntoLifecyclePlan.ToString().ToLowerInvariant()}, copied={item.PayloadCopiedWithoutTransformation.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Multi-family regression");
        lines.Add(string.Empty);
        lines.Add($"- passed: {regression.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- noArchitectureForks: {regression.NoArchitectureForks.ToString().ToLowerInvariant()}");
        lines.AddRange(regression.Rows.Select(item => $"- {item.FamilyId}: sharedLifecycle={item.UsesSharedLifecycleContract.ToString().ToLowerInvariant()}, extensionOnly={item.UsesFamilyScopedExtensionOnly.ToString().ToLowerInvariant()}, loop={item.SimulatableLoopProofPassed.ToString().ToLowerInvariant()}, goal040={item.SourceGoal040PreviewExportConsumed.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.Add($"- passed: {invalidMatrix.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add("No public GamePackage schema, Runtime, Runtime.Abstractions, WinForms, Unity, Infrastructure, Scripting, Generation provider/LLM/RAG/media path, generator-library, sample/template, solution/project or Designer file change is required by this Goal 043 evidence.");
        lines.Add(string.Empty);
        lines.Add($"{MultiFamilyGeneratedTemplateVocabulary.FinalGate} required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => MultiFamilyGeneratedTemplateHash.Serialize(value);

    private static string Hash(string text) => MultiFamilyGeneratedTemplateHash.Hash(text);

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}
