using System.Text;

namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignEditEvidenceService
{
    public const string SourceManifestFileName = "edit-workspace-source-manifest.json";
    public const string FieldCatalogFileName = "editable-schema-field-catalog.json";
    public const string ChangeSetCatalogFileName = "change-set-catalog.json";
    public const string ValidationDiagnosticsMatrixFileName = "validation-diagnostics-matrix.json";
    public const string ApplyRollbackLedgerFileName = "apply-rollback-ledger.json";
    public const string DiffMatrixFileName = "row-before-after-diff-matrix.json";
    public const string PreviewExportRefreshPayloadFileName = "preview-export-refresh-payload.json";
    public const string WinFormsBindingInventoryFileName = "winforms-binding-inventory.json";
    public const string QualityGateScanFileName = "quality-gate-scan.json";
    public const string InvalidEditDiagnosticsMatrixFileName = "invalid-edit-diagnostics-matrix.json";
    public const string ArtifactScopeReportFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "schema-driven-campaign-edit-validate-apply-loop-report.md";

    private static readonly UTF8Encoding Utf8NoBom = new(false);
    private readonly SchemaDrivenCampaignEditValidateApplySourceLoader _loader = new();
    private readonly SchemaDrivenCampaignEditCatalog _catalog = new();
    private readonly SchemaDrivenCampaignEditValidator _validator = new();
    private readonly SchemaDrivenCampaignApplyEngine _applyEngine = new();
    private readonly SchemaDrivenCampaignRollbackPlanner _rollbackPlanner = new();
    private readonly SchemaDrivenCampaignEditQualityGateScanner _qualityScanner = new();

    public SchemaDrivenCampaignEditBuildResult Build(string projectRoot)
    {
        var root = Path.GetFullPath(projectRoot);
        var source = _loader.Load(root);
        var sourceManifest = _loader.BuildSourceManifest(source);
        var fieldCatalog = _catalog.BuildFieldCatalog();
        var changeSets = _catalog.BuildChangeSetCatalog(source, fieldCatalog);
        var validationMatrix = _validator.ValidateCandidates(source, fieldCatalog, changeSets);
        var applyRollbackLedger = _applyEngine.Apply(source, fieldCatalog, changeSets, validationMatrix);
        var diffMatrix = _applyEngine.BuildDiffMatrix(applyRollbackLedger);
        var previewPayload = _rollbackPlanner.BuildPreviewExportRefreshPayload(diffMatrix);
        var winFormsInventory = _qualityScanner.BuildWinFormsBindingInventory(root);
        var qualityScan = _qualityScanner.Scan(root);
        var invalidMatrix = _validator.BuildInvalidDiagnosticsMatrix(source, fieldCatalog);
        var report = BuildReport(
            sourceManifest,
            fieldCatalog,
            changeSets,
            validationMatrix,
            applyRollbackLedger,
            diffMatrix,
            previewPayload,
            winFormsInventory,
            qualityScan,
            invalidMatrix);

        return new SchemaDrivenCampaignEditBuildResult
        {
            SourceManifest = sourceManifest,
            FieldCatalog = fieldCatalog,
            ChangeSetCatalog = changeSets,
            ValidationMatrix = validationMatrix,
            ApplyRollbackLedger = applyRollbackLedger,
            DiffMatrix = diffMatrix,
            PreviewExportRefreshPayload = previewPayload,
            WinFormsBindingInventory = winFormsInventory,
            QualityGateScan = qualityScan,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ReportMarkdown = BuildMarkdownReport(report)
        };
    }

    public async Task<SchemaDrivenCampaignEditWriteResult> BuildAndWriteAsync(
        string projectRoot,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRoot);
        return await WriteAsync(projectRoot, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SchemaDrivenCampaignEditWriteResult> WriteAsync(
        string projectRoot,
        SchemaDrivenCampaignEditBuildResult result,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(projectRoot);
        var outputDirectory = Path.Combine(
            root,
            SchemaDrivenCampaignEditVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar));
        EnsureContained(root, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var writtenFiles = new List<string>();
        await WriteJson(outputDirectory, SourceManifestFileName, result.SourceManifest, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, FieldCatalogFileName, result.FieldCatalog, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, ChangeSetCatalogFileName, result.ChangeSetCatalog, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, ValidationDiagnosticsMatrixFileName, result.ValidationMatrix, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, ApplyRollbackLedgerFileName, result.ApplyRollbackLedger, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, DiffMatrixFileName, result.DiffMatrix, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(
                outputDirectory,
                PreviewExportRefreshPayloadFileName,
                result.PreviewExportRefreshPayload,
                writtenFiles,
                cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(
                outputDirectory,
                WinFormsBindingInventoryFileName,
                result.WinFormsBindingInventory,
                writtenFiles,
                cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, QualityGateScanFileName, result.QualityGateScan, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(
                outputDirectory,
                InvalidEditDiagnosticsMatrixFileName,
                result.InvalidMatrix,
                writtenFiles,
                cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, ArtifactScopeReportFileName, BuildArtifactScopeReport(result), writtenFiles, cancellationToken)
            .ConfigureAwait(false);

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8NoBom, cancellationToken).ConfigureAwait(false);
        writtenFiles.Add(ReportMarkdownFileName);

        return new SchemaDrivenCampaignEditWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = writtenFiles.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private SchemaDrivenCampaignEditReport BuildReport(
        CampaignEditSourceManifest sourceManifest,
        EditableSchemaFieldCatalog fieldCatalog,
        ChangeSetCatalog changeSetCatalog,
        ValidationDiagnosticsMatrix validationMatrix,
        ApplyRollbackLedger applyRollbackLedger,
        RowBeforeAfterDiffMatrix diffMatrix,
        PreviewExportRefreshPayload previewPayload,
        WinFormsEditBindingInventory winFormsInventory,
        CampaignEditQualityGateScan qualityScan,
        InvalidEditDiagnosticsMatrix invalidMatrix)
    {
        var diagnostics = new List<CampaignEditDiagnostic>();
        diagnostics.AddRange(_validator.ValidateSourceManifest(sourceManifest));
        diagnostics.AddRange(sourceManifest.Diagnostics);
        diagnostics.AddRange(winFormsInventory.Diagnostics);
        diagnostics.AddRange(qualityScan.Diagnostics);

        var validationPassed = diagnostics.All(item => item.Severity != "error")
            && fieldCatalog.Passed
            && changeSetCatalog.Passed
            && validationMatrix.Passed
            && applyRollbackLedger.Passed
            && diffMatrix.Passed
            && previewPayload.Passed
            && winFormsInventory.Passed
            && qualityScan.Passed
            && invalidMatrix.Passed;
        var status = validationPassed ? "GREEN" : "BLOCKED";
        var deterministicHash = SchemaDrivenCampaignEditHash.Sha256(string.Join(
            "|",
            Hash(sourceManifest),
            Hash(fieldCatalog),
            Hash(changeSetCatalog),
            Hash(validationMatrix),
            Hash(applyRollbackLedger),
            Hash(diffMatrix),
            Hash(previewPayload),
            Hash(winFormsInventory),
            Hash(qualityScan),
            Hash(invalidMatrix)));

        return new SchemaDrivenCampaignEditReport
        {
            ImplementationStatus = status,
            Accepted = false,
            Goal074AcceptedByUserHandoff = sourceManifest.Goal074AcceptedByUserHandoff,
            Goal072PreservedAsBlocked = sourceManifest.Goal072RemainsHistoricalBlocked,
            Goal031And032RemainProducedForReview = sourceManifest.Goal031And032RemainProducedForReview,
            RowCount = sourceManifest.RowCount,
            FamilyCount = sourceManifest.FamilyCount,
            SeedCount = sourceManifest.SeedCount,
            EditableFieldCount = fieldCatalog.FieldCount,
            CandidateCount = changeSetCatalog.CandidateCount,
            AppliedChangeCount = applyRollbackLedger.AppliedChangeCount,
            RollbackCount = applyRollbackLedger.RollbackCount,
            InvalidScenarioCount = invalidMatrix.ScenarioCount,
            ValidationPassed = validationMatrix.Passed,
            ApplyRollbackPassed = applyRollbackLedger.Passed,
            DiffMatrixPassed = diffMatrix.Passed,
            PreviewExportRefreshPassed = previewPayload.Passed,
            WinFormsBindingPassed = winFormsInventory.Passed,
            QualityGatePassed = qualityScan.Passed,
            InvalidMatrixPassed = invalidMatrix.Passed,
            DeterministicHash = deterministicHash,
            Diagnostics = SchemaDrivenCampaignEditValidateApplySourceLoader.SortDiagnostics(diagnostics)
        };
    }

    private static string BuildMarkdownReport(SchemaDrivenCampaignEditReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Goal 075 Schema-Driven Campaign Edit/Validate/Apply Loop");
        builder.AppendLine();
        builder.AppendLine("- gate: " + SchemaDrivenCampaignEditVocabulary.FinalGate + " required");
        builder.AppendLine("- accepted: false");
        builder.AppendLine("- implementationStatus: " + report.ImplementationStatus);
        builder.AppendLine("- goal074Handoff: " + report.Goal074AcceptedByUserHandoff);
        builder.AppendLine("- goal072PreservedAsBlocked: " + report.Goal072PreservedAsBlocked);
        builder.AppendLine("- goal031And032ProducedForReview: " + report.Goal031And032RemainProducedForReview);
        builder.AppendLine("- rowCount: " + report.RowCount);
        builder.AppendLine("- familyCount: " + report.FamilyCount);
        builder.AppendLine("- seedCount: " + report.SeedCount);
        builder.AppendLine("- editableFieldCount: " + report.EditableFieldCount);
        builder.AppendLine("- candidateCount: " + report.CandidateCount);
        builder.AppendLine("- appliedChangeCount: " + report.AppliedChangeCount);
        builder.AppendLine("- rollbackCount: " + report.RollbackCount);
        builder.AppendLine("- invalidScenarioCount: " + report.InvalidScenarioCount);
        builder.AppendLine("- deterministicHash: " + report.DeterministicHash);
        builder.AppendLine();
        builder.AppendLine("## Proof");
        builder.AppendLine("- validationPassed: " + report.ValidationPassed);
        builder.AppendLine("- applyRollbackPassed: " + report.ApplyRollbackPassed);
        builder.AppendLine("- diffMatrixPassed: " + report.DiffMatrixPassed);
        builder.AppendLine("- previewExportRefreshPassed: " + report.PreviewExportRefreshPassed);
        builder.AppendLine("- winFormsBindingPassed: " + report.WinFormsBindingPassed);
        builder.AppendLine("- qualityGatePassed: " + report.QualityGatePassed);
        builder.AppendLine("- invalidMatrixPassed: " + report.InvalidMatrixPassed);
        builder.AppendLine();
        builder.AppendLine("## Diagnostics");
        if (report.Diagnostics.Count == 0)
        {
            builder.AppendLine("- none");
        }
        else
        {
            foreach (var diagnostic in report.Diagnostics)
            {
                builder.AppendLine("- " + diagnostic.Severity + ": " + diagnostic.Code + " [" + diagnostic.Target + "]");
            }
        }

        return builder.ToString();
    }

    private static object BuildArtifactScopeReport(SchemaDrivenCampaignEditBuildResult result) =>
        new
        {
            schemaVersion = "goal075_artifact_scope_report_v1",
            scenario = SchemaDrivenCampaignEditVocabulary.ProductSmokeRoute,
            gate = SchemaDrivenCampaignEditVocabulary.FinalGate,
            accepted = false,
            implementationStatus = result.Report.ImplementationStatus,
            allowedOutputDirectory = SchemaDrivenCampaignEditVocabulary.RelativeOutputDirectory,
            artifactFileCount = 12,
            generatedFiles = RequiredArtifactNames(),
            rowCount = result.Report.RowCount,
            editableFieldCount = result.Report.EditableFieldCount,
            appliedChangeCount = result.Report.AppliedChangeCount,
            rollbackCount = result.Report.RollbackCount,
            invalidScenarioCount = result.Report.InvalidScenarioCount,
            qualityGatePassed = result.QualityGateScan.Passed,
            winFormsBindingPassed = result.WinFormsBindingInventory.Passed
        };

    public static IReadOnlyList<string> RequiredArtifactNames() =>
    [
        SourceManifestFileName,
        FieldCatalogFileName,
        ChangeSetCatalogFileName,
        ValidationDiagnosticsMatrixFileName,
        ApplyRollbackLedgerFileName,
        DiffMatrixFileName,
        PreviewExportRefreshPayloadFileName,
        WinFormsBindingInventoryFileName,
        QualityGateScanFileName,
        InvalidEditDiagnosticsMatrixFileName,
        ArtifactScopeReportFileName,
        ReportMarkdownFileName
    ];

    private static async Task WriteJson<T>(
        string outputDirectory,
        string fileName,
        T value,
        ICollection<string> writtenFiles,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(outputDirectory, fileName);
        var json = SchemaDrivenCampaignEditHash.Serialize(value);
        await File.WriteAllTextAsync(path, json + Environment.NewLine, Utf8NoBom, cancellationToken).ConfigureAwait(false);
        writtenFiles.Add(fileName);
    }

    private static string Hash<T>(T value) =>
        SchemaDrivenCampaignEditHash.Sha256(SchemaDrivenCampaignEditHash.Serialize(value));

    private static void EnsureContained(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + normalizedPath);
        }
    }
}
