using System.Text;

namespace LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;

public sealed class FullGeneratorWithoutMediaDryRunEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-047-full-generator-without-media-dry-run";
    public const string SourceManifestJsonFileName = "dry-run-source-manifest.json";
    public const string ReviewPromotionLedgerJsonFileName = "review-promotion-ledger.json";
    public const string RepairDiagnosticsMatrixJsonFileName = "repair-diagnostics-matrix.json";
    public const string MapPanelFamilyDryRunJsonFileName = "family-map-panel-rpg-dry-run.json";
    public const string SurvivalFamilyDryRunJsonFileName = "family-survival-sandbox-dry-run.json";
    public const string GridDungeonFamilyDryRunJsonFileName = "family-first-person-grid-dungeon-dry-run.json";
    public const string RuntimePreviewValidationMatrixJsonFileName = "runtime-preview-validation-matrix.json";
    public const string ExportProfileSelectionMatrixJsonFileName = "export-profile-selection-matrix.json";
    public const string PackageCompatibilitySummaryJsonFileName = "package-compatibility-or-materialization-summary.json";
    public const string OneClickDryRunSummaryJsonFileName = "one-click-dry-run-summary.json";
    public const string InvalidFakeLeakMatrixJsonFileName = "invalid-fake-leak-matrix.json";
    public const string ReportMarkdownFileName = "full-generator-without-media-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly FullGeneratorWithoutMediaDryRunSourceLoader _sourceLoader;

    public FullGeneratorWithoutMediaDryRunEvidenceService(FullGeneratorWithoutMediaDryRunSourceLoader? sourceLoader = null)
    {
        _sourceLoader = sourceLoader ?? new FullGeneratorWithoutMediaDryRunSourceLoader();
    }

    public FullGeneratorWithoutMediaDryRunEvidenceResult Build(string projectRootPath)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var builder = new FullGeneratorDryRunBuilder();
        var validator = new FullGeneratorWithoutMediaDryRunValidator();

        var manifest = builder.BuildManifest(source);
        var ledger = new FullGeneratorReviewPromotionWorkflow().BuildLedger(source, manifest);
        var repairMatrix = new FullGeneratorRepairDiagnosticsPlanner().BuildMatrix();
        var packageSummary = builder.BuildPackageCompatibilitySummary(manifest);
        var familyDryRuns = builder.BuildFamilyDryRuns(source, manifest, ledger, packageSummary);
        var runtimePreview = builder.BuildRuntimePreviewValidationMatrix(source, familyDryRuns);
        var exportProfiles = builder.BuildExportProfileSelectionMatrix(source, familyDryRuns);
        var invalidMatrix = validator.BuildInvalidMatrix(
            manifest,
            ledger,
            repairMatrix,
            familyDryRuns,
            runtimePreview,
            exportProfiles,
            packageSummary);

        var diagnostics = FullGeneratorWithoutMediaDryRunValidator.SortDiagnostics(
            validator.ValidateManifest(manifest)
                .Concat(ledger.Diagnostics)
                .Concat(validator.ValidateReviewPromotionLedger(ledger))
                .Concat(validator.ValidateRepairMatrix(repairMatrix))
                .Concat(validator.ValidateFamilyDryRuns(familyDryRuns))
                .Concat(validator.ValidateRuntimePreviewMatrix(runtimePreview))
                .Concat(validator.ValidateExportProfileMatrix(exportProfiles))
                .Concat(validator.ValidatePackageSummary(packageSummary)));

        var allRequiredProofPassed = diagnostics.All(item => item.Severity != "error" && item.Severity != "critical")
            && !manifest.Accepted
            && manifest.AcceptedPreflightGates.Any(item =>
                item.GateId == "multi_family_generated_template_vertical_slice_verification"
                && item.Status == "passed"
                && item.ProvenanceKind == "user_handoff")
            && ledger.Passed
            && repairMatrix.Passed
            && familyDryRuns.Count == 3
            && familyDryRuns.All(item => item.StateChangingLoopProof && item.BoundaryClaims.AllFalse)
            && runtimePreview.Passed
            && exportProfiles.Passed
            && packageSummary.CompatibilityProofPassed
            && invalidMatrix.Passed;
        var blocked = diagnostics.Any(item =>
            item.Code.Contains(".boundary.", StringComparison.Ordinal)
            || item.Code.Contains(".source.", StringComparison.Ordinal));

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceManifestJsonFileName] = Serialize(manifest),
            [ReviewPromotionLedgerJsonFileName] = Serialize(ledger),
            [RepairDiagnosticsMatrixJsonFileName] = Serialize(repairMatrix),
            [RuntimePreviewValidationMatrixJsonFileName] = Serialize(runtimePreview),
            [ExportProfileSelectionMatrixJsonFileName] = Serialize(exportProfiles),
            [PackageCompatibilitySummaryJsonFileName] = Serialize(packageSummary),
            [InvalidFakeLeakMatrixJsonFileName] = Serialize(invalidMatrix)
        };

        foreach (var record in familyDryRuns.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal))
        {
            artifactJson[FamilyDryRunFileName(record.FamilyId)] = Serialize(record);
        }

        var evidenceFiles = artifactJson.Keys
            .Append(OneClickDryRunSummaryJsonFileName)
            .Append(ReportMarkdownFileName)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

        var oneClickWithoutHash = new FullGeneratorOneClickDryRunSummary
        {
            Accepted = false,
            Status = allRequiredProofPassed ? "GREEN" : blocked ? "BLOCKED" : "FAILED",
            FamilyCount = familyDryRuns.Count,
            EvidenceFileCount = evidenceFiles.Count,
            ReviewPromotionPassed = ledger.Passed,
            RepairDiagnosticsPassed = repairMatrix.Passed,
            RuntimePreviewValidationPassed = runtimePreview.Passed,
            ExportProfileSelectionPassed = exportProfiles.Passed,
            PackageProofPassed = packageSummary.CompatibilityProofPassed || packageSummary.MaterializedValidatorCleanPackages,
            InvalidMatrixPassed = invalidMatrix.Passed,
            MediaGenerated = false,
            ProviderCalled = false,
            UnityExecuted = false,
            RuntimeSourceChanged = false,
            EvidenceFiles = evidenceFiles
        };
        var oneClick = oneClickWithoutHash with
        {
            DeterministicHash = Hash(Serialize(oneClickWithoutHash))
        };
        artifactJson[OneClickDryRunSummaryJsonFileName] = Serialize(oneClick);

        var reportWithoutHash = new FullGeneratorWithoutMediaReport
        {
            ImplementationStatus = allRequiredProofPassed ? "GREEN" : blocked ? "BLOCKED" : "FAILED",
            Accepted = false,
            FamilyCount = familyDryRuns.Count,
            Goal043AcceptedByUserHandoff = true,
            ReviewPromotionPassed = ledger.Passed,
            RepairDiagnosticsPassed = repairMatrix.Passed,
            RuntimePreviewValidationPassed = runtimePreview.Passed,
            ExportProfileSelectionPassed = exportProfiles.Passed,
            PackageProofPassed = packageSummary.CompatibilityProofPassed || packageSummary.MaterializedValidatorCleanPackages,
            InvalidMatrixPassed = invalidMatrix.Passed,
            MediaGenerated = false,
            ProviderCalled = false,
            UnityExecuted = false,
            RuntimeSourceChanged = false,
            SourceManifestHash = Hash(artifactJson[SourceManifestJsonFileName]),
            ReviewLedgerHash = Hash(artifactJson[ReviewPromotionLedgerJsonFileName]),
            RepairMatrixHash = Hash(artifactJson[RepairDiagnosticsMatrixJsonFileName]),
            RuntimePreviewMatrixHash = Hash(artifactJson[RuntimePreviewValidationMatrixJsonFileName]),
            ExportProfileMatrixHash = Hash(artifactJson[ExportProfileSelectionMatrixJsonFileName]),
            PackageProofHash = Hash(artifactJson[PackageCompatibilitySummaryJsonFileName]),
            OneClickSummaryHash = Hash(artifactJson[OneClickDryRunSummaryJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidFakeLeakMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new FullGeneratorWithoutMediaDryRunEvidenceResult
        {
            SourceManifest = manifest,
            ReviewPromotionLedger = ledger,
            RepairDiagnosticsMatrix = repairMatrix,
            FamilyDryRuns = familyDryRuns,
            RuntimePreviewValidationMatrix = runtimePreview,
            ExportProfileSelectionMatrix = exportProfiles,
            PackageCompatibilitySummary = packageSummary,
            OneClickSummary = oneClick,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            ReportMarkdown = RenderReport(
                report,
                manifest,
                ledger,
                repairMatrix,
                familyDryRuns,
                runtimePreview,
                exportProfiles,
                packageSummary,
                oneClick,
                invalidMatrix)
        };
    }

    public async Task<FullGeneratorWithoutMediaDryRunWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<FullGeneratorWithoutMediaDryRunWriteResult> WriteAsync(
        string projectRootPath,
        FullGeneratorWithoutMediaDryRunEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var written = new List<string>();
        foreach (var pair in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, pair.Key);
            await File.WriteAllTextAsync(path, pair.Value, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new FullGeneratorWithoutMediaDryRunWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    public static string FamilyDryRunFileName(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => MapPanelFamilyDryRunJsonFileName,
            "survival_sandbox" => SurvivalFamilyDryRunJsonFileName,
            "first_person_grid_dungeon" => GridDungeonFamilyDryRunJsonFileName,
            _ => $"family-{familyId}-dry-run.json"
        };

    private static string RenderReport(
        FullGeneratorWithoutMediaReport report,
        FullGeneratorDryRunManifest manifest,
        FullGeneratorReviewPromotionLedger ledger,
        FullGeneratorRepairDiagnosticsMatrix repairMatrix,
        IReadOnlyList<FullGeneratorFamilyDryRunRecord> familyDryRuns,
        FullGeneratorRuntimePreviewValidationMatrix runtimePreview,
        FullGeneratorExportProfileSelectionMatrix exportProfiles,
        FullGeneratorPackageCompatibilitySummary packageSummary,
        FullGeneratorOneClickDryRunSummary oneClick,
        FullGeneratorInvalidMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Full Generator Without Media Dry Run Report",
            string.Empty,
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            $"manualGate={report.ManualGate}",
            $"familyCount={report.FamilyCount}",
            $"goal043AcceptedByUserHandoff={report.Goal043AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"reviewPromotionPassed={report.ReviewPromotionPassed.ToString().ToLowerInvariant()}",
            $"repairDiagnosticsPassed={report.RepairDiagnosticsPassed.ToString().ToLowerInvariant()}",
            $"runtimePreviewValidationPassed={report.RuntimePreviewValidationPassed.ToString().ToLowerInvariant()}",
            $"exportProfileSelectionPassed={report.ExportProfileSelectionPassed.ToString().ToLowerInvariant()}",
            $"packageProofPassed={report.PackageProofPassed.ToString().ToLowerInvariant()}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- goal043AcceptedGate: {report.Goal043AcceptedGate}",
            $"- finalGate: {report.ManualGate} required",
            $"- mediaPolicy: {FullGeneratorWithoutMediaDryRunVocabulary.MediaPolicy}",
            $"- providerCalled: {report.ProviderCalled.ToString().ToLowerInvariant()}",
            $"- mediaGenerated: {report.MediaGenerated.ToString().ToLowerInvariant()}",
            $"- unityExecuted: {report.UnityExecuted.ToString().ToLowerInvariant()}",
            $"- runtimeSourceChanged: {report.RuntimeSourceChanged.ToString().ToLowerInvariant()}",
            $"- sourceManifestHash: {report.SourceManifestHash}",
            $"- reviewLedgerHash: {report.ReviewLedgerHash}",
            $"- repairMatrixHash: {report.RepairMatrixHash}",
            $"- runtimePreviewMatrixHash: {report.RuntimePreviewMatrixHash}",
            $"- exportProfileMatrixHash: {report.ExportProfileMatrixHash}",
            $"- packageProofHash: {report.PackageProofHash}",
            $"- oneClickSummaryHash: {report.OneClickSummaryHash}",
            $"- invalidMatrixHash: {report.InvalidMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## Preflight gates",
            string.Empty
        };
        lines.AddRange(manifest.AcceptedPreflightGates.Select(item => $"- {item.GateId}: status={item.Status}, provenance={item.ProvenanceKind}, evidence={item.EvidenceRef}"));
        lines.Add(string.Empty);
        lines.Add("## Source manifest");
        lines.Add(string.Empty);
        lines.Add($"- sourceArtifactRefs: {manifest.SourceArtifactRefs.Count}");
        lines.Add($"- selectedFamilies: {string.Join(",", manifest.SelectedFamilyIds)}");
        lines.Add($"- selectedTemplateLoopRefs: {manifest.SelectedTemplateLoopRefs.Count}");
        lines.Add($"- selectedDraftLuaExpansionRefs: {manifest.SelectedDraftLuaExpansionRefs.Count}");
        lines.Add($"- selectedWorldChunkRuntimeRefs: {manifest.SelectedWorldChunkRuntimeRefs.Count}");
        lines.Add(string.Empty);
        lines.Add("## Review and promotion");
        lines.Add(string.Empty);
        lines.Add($"- passed: {ledger.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- transitionCount: {ledger.TransitionCount}");
        lines.AddRange(ledger.Transitions.Select(item => $"- {item.TransitionId}: {item.BeforeState}->{item.AfterState}, decision={item.PromotionDecision}, provenance={item.ProvenanceKind}"));
        lines.Add(string.Empty);
        lines.Add("## Repair diagnostics");
        lines.Add(string.Empty);
        lines.Add($"- passed: {repairMatrix.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- diagnosticCount: {repairMatrix.DiagnosticCount}");
        lines.Add($"- manualRequiredCount: {repairMatrix.ManualRequiredCount}");
        lines.AddRange(repairMatrix.Rows.Select(item => $"- {item.DiagnosticId}: decision={item.Decision}, action={item.RepairActionKind}"));
        lines.Add(string.Empty);
        lines.Add("## Family dry-runs");
        lines.Add(string.Empty);
        lines.AddRange(familyDryRuns.Select(item => $"- {item.FamilyId}: scenario={item.ScenarioId}, profile={item.ProfileId}, systems={item.GeneratedSystemCoverage.Count}, stateChangingLoop={item.StateChangingLoopProof.ToString().ToLowerInvariant()}, replayHashPassed={item.ReplayHashProof.Passed.ToString().ToLowerInvariant()}, payload={item.RuntimePreviewPayloadSummary.PayloadRelativePath}, exportProfile={item.ExportCandidatePayloadSummary.ExportProfileId}"));
        lines.Add(string.Empty);
        lines.Add("## Runtime preview validation");
        lines.Add(string.Empty);
        lines.Add($"- passed: {runtimePreview.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(runtimePreview.Rows.Select(item => $"- {item.FamilyId}: stableRefs={item.StableRelativeRefs.ToString().ToLowerInvariant()}, sourceHashesMatch={item.SourceHashesMatch.ToString().ToLowerInvariant()}, commandStateTransitionsConsistent={item.CommandStateTransitionsConsistent.ToString().ToLowerInvariant()}, passed={item.Passed.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Export profile selection");
        lines.Add(string.Empty);
        lines.Add($"- passed: {exportProfiles.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(exportProfiles.Rows.Select(item => $"- {item.FamilyId}: profile={item.ExportProfileId}, presentation={item.PresentationMode}, withoutMedia={item.WithoutMedia.ToString().ToLowerInvariant()}, passed={item.Passed.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Package proof");
        lines.Add(string.Empty);
        lines.Add($"- proofMode: {packageSummary.ProofMode}");
        lines.Add($"- packageMaterializationAttempted: {packageSummary.PackageMaterializationAttempted.ToString().ToLowerInvariant()}");
        lines.Add($"- materializedValidatorCleanPackages: {packageSummary.MaterializedValidatorCleanPackages.ToString().ToLowerInvariant()}");
        lines.Add($"- compatibilityProofPassed: {packageSummary.CompatibilityProofPassed.ToString().ToLowerInvariant()}");
        lines.Add($"- directMaterializationSafetyDecision: {packageSummary.DirectMaterializationSafetyDecision}");
        lines.AddRange(packageSummary.Rows.Select(item => $"- {item.FamilyId}/{item.SystemId}: status={item.CompatibilityStatus}, target={item.ExistingPackageTarget}, directMaterializationSafeNow={item.DirectMaterializationSafeNow.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## One-click dry-run proof");
        lines.Add(string.Empty);
        lines.Add($"- status: {oneClick.Status}");
        lines.Add($"- evidenceFileCount: {oneClick.EvidenceFileCount}");
        lines.Add($"- deterministicHash: {oneClick.DeterministicHash}");
        lines.AddRange(oneClick.EvidenceFiles.Select(item => $"- evidenceFile: {item}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.Add($"- passed: {invalidMatrix.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- scenarioCount: {invalidMatrix.ScenarioCount}");
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add("No public GamePackage schema, Runtime, Runtime.Abstractions, WinForms UI, Unity, provider/LLM/RAG/media path, generator-library, sample/template, solution/project file, external dependency or arbitrary Lua execution change is required by this Goal 047 evidence.");
        lines.Add(string.Empty);
        lines.Add($"{FullGeneratorWithoutMediaDryRunVocabulary.FinalGate} required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => FullGeneratorWithoutMediaDryRunHash.Serialize(value);

    private static string Hash(string text) => FullGeneratorWithoutMediaDryRunHash.Hash(text);

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
