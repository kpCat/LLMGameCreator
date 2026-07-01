using System.Text;

namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

public sealed class SchemaDrivenCampaignWorkspaceEvidenceService
{
    public const string SourceManifestFileName = "workspace-source-manifest.json";
    public const string RowSelectorFileName = "campaign-row-selector.json";
    public const string DynamicAuthoringSchemaFileName = "dynamic-authoring-schema.json";
    public const string UiBindingContractFileName = "ui-binding-contract.json";
    public const string ValidationDashboardFileName = "workspace-validation-dashboard.json";
    public const string ReviewProvenanceLedgerFileName = "review-provenance-ledger.json";
    public const string AuthoringActionPlanFileName = "authoring-action-plan.json";
    public const string QualityGateScanFileName = "quality-gate-scan.json";
    public const string WinFormsControlInventoryFileName = "winforms-control-inventory.json";
    public const string InvalidDiagnosticsMatrixFileName = "invalid-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "schema-driven-campaign-authoring-review-workspace-report.md";
    public const string ArtifactScopeReportFileName = "artifact-scope-report.json";

    private static readonly UTF8Encoding Utf8NoBom = new(false);
    private readonly SchemaDrivenCampaignWorkspaceSourceLoader _loader = new();
    private readonly SchemaDrivenCampaignWorkspaceBuilder _builder = new();
    private readonly SchemaDrivenCampaignWorkspaceValidator _validator = new();
    private readonly SchemaDrivenCampaignWorkspaceQualityGateScanner _scanner = new();

    public CampaignWorkspaceBuildResult Build(string projectRoot)
    {
        var root = Path.GetFullPath(projectRoot);
        var source = _loader.Load(root);
        var sourceManifest = _builder.BuildSourceManifest(source);
        var rowSelector = _builder.BuildRowSelector(source);
        var dynamicSchema = _builder.BuildDynamicSchema(source);
        var uiBinding = _builder.BuildUiBindingContract(dynamicSchema);
        var provenance = _builder.BuildProvenanceLedger(source);
        var actionPlan = _builder.BuildActionPlan(dynamicSchema, provenance);
        var qualityScan = _scanner.Scan(root);
        var winFormsInventory = _builder.BuildWinFormsControlInventory(root);
        var invalidMatrix = _builder.BuildInvalidDiagnosticsMatrix();
        var validation = BuildValidationDashboard(
            sourceManifest,
            rowSelector,
            dynamicSchema,
            uiBinding,
            provenance,
            actionPlan,
            qualityScan,
            winFormsInventory,
            invalidMatrix);
        var report = BuildReport(
            sourceManifest,
            rowSelector,
            dynamicSchema,
            uiBinding,
            validation,
            provenance,
            actionPlan,
            qualityScan,
            winFormsInventory,
            invalidMatrix);

        return new CampaignWorkspaceBuildResult
        {
            SourceManifest = sourceManifest,
            RowSelector = rowSelector,
            DynamicSchema = dynamicSchema,
            UiBindingContract = uiBinding,
            ValidationDashboard = validation,
            ProvenanceLedger = provenance,
            ActionPlan = actionPlan,
            QualityGateScan = qualityScan,
            WinFormsControlInventory = winFormsInventory,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ReportMarkdown = BuildMarkdownReport(report)
        };
    }

    public async Task<CampaignWorkspaceWriteResult> BuildAndWriteAsync(
        string projectRoot,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRoot);
        return await WriteAsync(projectRoot, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CampaignWorkspaceWriteResult> WriteAsync(
        string projectRoot,
        CampaignWorkspaceBuildResult result,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(projectRoot);
        var outputDirectory = Path.Combine(
            root,
            SchemaDrivenCampaignWorkspaceVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar));
        EnsureContained(root, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var writtenFiles = new List<string>();
        await WriteJson(outputDirectory, SourceManifestFileName, result.SourceManifest, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, RowSelectorFileName, result.RowSelector, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, DynamicAuthoringSchemaFileName, result.DynamicSchema, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, UiBindingContractFileName, result.UiBindingContract, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, ValidationDashboardFileName, result.ValidationDashboard, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, ReviewProvenanceLedgerFileName, result.ProvenanceLedger, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, AuthoringActionPlanFileName, result.ActionPlan, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, QualityGateScanFileName, result.QualityGateScan, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, WinFormsControlInventoryFileName, result.WinFormsControlInventory, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, InvalidDiagnosticsMatrixFileName, result.InvalidMatrix, writtenFiles, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, ArtifactScopeReportFileName, BuildArtifactScopeReport(result), writtenFiles, cancellationToken)
            .ConfigureAwait(false);

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8NoBom, cancellationToken).ConfigureAwait(false);
        writtenFiles.Add(Normalize(root, reportPath));

        return new CampaignWorkspaceWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = writtenFiles.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private WorkspaceValidationDashboard BuildValidationDashboard(
        CampaignWorkspaceSourceManifest sourceManifest,
        CampaignRowSelector rowSelector,
        CampaignAuthoringSchema dynamicSchema,
        CampaignUiBindingContract uiBinding,
        ReviewProvenanceLedger provenance,
        AuthoringActionPlan actionPlan,
        QualityGateScan qualityScan,
        WinFormsControlInventory winFormsInventory,
        CampaignInvalidDiagnosticsMatrix invalidMatrix)
    {
        var sourceDiagnostics = _validator.ValidateSourceManifest(sourceManifest);
        var rowDiagnostics = _validator.ValidateRowSelector(rowSelector);
        var schemaDiagnostics = _validator.ValidateSchema(dynamicSchema);
        var uiDiagnostics = _validator.ValidateUiBinding(dynamicSchema, uiBinding);
        var provenanceDiagnostics = _validator.ValidateProvenance(provenance);
        var actionPlanDiagnostics = _validator.ValidateActionPlan(dynamicSchema, actionPlan);
        var qualityDiagnostics = _validator.ValidateQualityGate(qualityScan);
        var winFormsDiagnostics = _validator.ValidateWinFormsInventory(winFormsInventory);
        var invalidDiagnostics = _validator.ValidateInvalidMatrix(invalidMatrix);
        var all = new[]
            {
                sourceDiagnostics,
                rowDiagnostics,
                schemaDiagnostics,
                uiDiagnostics,
                provenanceDiagnostics,
                actionPlanDiagnostics,
                qualityDiagnostics,
                winFormsDiagnostics,
                invalidDiagnostics
            }
            .SelectMany(item => item)
            .ToList();

        return new WorkspaceValidationDashboard
        {
            Passed = all.All(item => item.Severity != "error"),
            SourceManifestPassed = sourceDiagnostics.All(item => item.Severity != "error"),
            RowSelectorPassed = rowDiagnostics.All(item => item.Severity != "error"),
            SchemaPassed = schemaDiagnostics.All(item => item.Severity != "error"),
            UiBindingPassed = uiDiagnostics.All(item => item.Severity != "error"),
            ProvenancePassed = provenanceDiagnostics.All(item => item.Severity != "error"),
            ActionPlanPassed = actionPlanDiagnostics.All(item => item.Severity != "error"),
            QualityGatePassed = qualityDiagnostics.All(item => item.Severity != "error"),
            InvalidMatrixPassed = invalidDiagnostics.All(item => item.Severity != "error")
                && winFormsDiagnostics.All(item => item.Severity != "error"),
            ErrorCount = all.Count(item => item.Severity == "error"),
            WarningCount = all.Count(item => item.Severity == "warning"),
            Diagnostics = SchemaDrivenCampaignWorkspaceSourceLoader.SortDiagnostics(all)
        };
    }

    private static CampaignWorkspaceReport BuildReport(
        CampaignWorkspaceSourceManifest sourceManifest,
        CampaignRowSelector rowSelector,
        CampaignAuthoringSchema dynamicSchema,
        CampaignUiBindingContract uiBinding,
        WorkspaceValidationDashboard validation,
        ReviewProvenanceLedger provenance,
        AuthoringActionPlan actionPlan,
        QualityGateScan qualityScan,
        WinFormsControlInventory winFormsInventory,
        CampaignInvalidDiagnosticsMatrix invalidMatrix)
    {
        var sourceHash = Hash(sourceManifest);
        var rowHash = Hash(rowSelector);
        var schemaHash = Hash(dynamicSchema);
        var uiHash = Hash(uiBinding);
        var dashboardHash = Hash(validation);
        var provenanceHash = Hash(provenance);
        var actionHash = Hash(actionPlan);
        var qualityHash = Hash(qualityScan);
        var winFormsHash = Hash(winFormsInventory);
        var invalidHash = Hash(invalidMatrix);
        var deterministicHash = SchemaDrivenCampaignWorkspaceHash.Sha256(string.Join(
            "|",
            sourceHash,
            rowHash,
            schemaHash,
            uiHash,
            dashboardHash,
            provenanceHash,
            actionHash,
            qualityHash,
            winFormsHash,
            invalidHash));
        var status = validation.Passed && winFormsInventory.Passed ? "GREEN" : "FAILED";

        return new CampaignWorkspaceReport
        {
            ImplementationStatus = status,
            Accepted = false,
            Goal073AcceptedByUserHandoff = sourceManifest.Goal073AcceptedByUserHandoff,
            Goal072PreservedAsBlocked = sourceManifest.Goal072RemainsHistoricalBlocked,
            Goal031And032RemainProducedForReview = sourceManifest.Goal031And032RemainProducedForReview,
            SourceManifestPassed = validation.SourceManifestPassed,
            RowSelectorPassed = validation.RowSelectorPassed,
            DynamicSchemaPassed = validation.SchemaPassed,
            UiBindingContractPassed = validation.UiBindingPassed,
            ProvenanceLedgerPassed = validation.ProvenancePassed,
            ActionPlanPassed = validation.ActionPlanPassed,
            ValidationDashboardPassed = validation.Passed,
            QualityGatePassed = validation.QualityGatePassed,
            WinFormsControlInventoryPassed = winFormsInventory.Passed,
            InvalidMatrixPassed = validation.InvalidMatrixPassed,
            RowCount = rowSelector.RowCount,
            FamilyCount = rowSelector.FamilyCount,
            SeedCount = rowSelector.SeedCount,
            SchemaGroupCount = dynamicSchema.Groups.Count,
            UiBindingGroupCount = uiBinding.GroupBindings.Count,
            ProvenanceEntryCount = provenance.Entries.Count,
            ActionPlanItemCount = actionPlan.Items.Count,
            SourceManifestHash = sourceHash,
            RowSelectorHash = rowHash,
            DynamicSchemaHash = schemaHash,
            UiBindingContractHash = uiHash,
            ValidationDashboardHash = dashboardHash,
            ProvenanceLedgerHash = provenanceHash,
            ActionPlanHash = actionHash,
            QualityGateScanHash = qualityHash,
            WinFormsControlInventoryHash = winFormsHash,
            InvalidMatrixHash = invalidHash,
            DeterministicHash = deterministicHash,
            Diagnostics = validation.Diagnostics
        };
    }

    private static string BuildMarkdownReport(CampaignWorkspaceReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Goal 074 Schema-Driven Campaign Authoring Review Workspace");
        builder.AppendLine();
        builder.AppendLine("- gate: " + SchemaDrivenCampaignWorkspaceVocabulary.FinalGate + " required");
        builder.AppendLine("- accepted: false");
        builder.AppendLine("- implementationStatus: " + report.ImplementationStatus);
        builder.AppendLine("- goal073Handoff: " + report.Goal073AcceptedByUserHandoff);
        builder.AppendLine("- goal072PreservedAsBlocked: " + report.Goal072PreservedAsBlocked);
        builder.AppendLine("- goal031And032ProducedForReview: " + report.Goal031And032RemainProducedForReview);
        builder.AppendLine("- rowCount: " + report.RowCount);
        builder.AppendLine("- familyCount: " + report.FamilyCount);
        builder.AppendLine("- seedCount: " + report.SeedCount);
        builder.AppendLine("- schemaGroupCount: " + report.SchemaGroupCount);
        builder.AppendLine("- uiBindingGroupCount: " + report.UiBindingGroupCount);
        builder.AppendLine("- provenanceEntryCount: " + report.ProvenanceEntryCount);
        builder.AppendLine("- actionPlanItemCount: " + report.ActionPlanItemCount);
        builder.AppendLine("- deterministicHash: " + report.DeterministicHash);
        builder.AppendLine();
        builder.AppendLine("## Artifact Hashes");
        builder.AppendLine("- workspace-source-manifest: " + report.SourceManifestHash);
        builder.AppendLine("- campaign-row-selector: " + report.RowSelectorHash);
        builder.AppendLine("- dynamic-authoring-schema: " + report.DynamicSchemaHash);
        builder.AppendLine("- ui-binding-contract: " + report.UiBindingContractHash);
        builder.AppendLine("- workspace-validation-dashboard: " + report.ValidationDashboardHash);
        builder.AppendLine("- review-provenance-ledger: " + report.ProvenanceLedgerHash);
        builder.AppendLine("- authoring-action-plan: " + report.ActionPlanHash);
        builder.AppendLine("- quality-gate-scan: " + report.QualityGateScanHash);
        builder.AppendLine("- winforms-control-inventory: " + report.WinFormsControlInventoryHash);
        builder.AppendLine("- invalid-diagnostics-matrix: " + report.InvalidMatrixHash);
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

    private static object BuildArtifactScopeReport(CampaignWorkspaceBuildResult result) =>
        new
        {
            schemaVersion = "goal074_artifact_scope_report_v1",
            scenario = SchemaDrivenCampaignWorkspaceVocabulary.ProductSmokeRoute,
            gate = SchemaDrivenCampaignWorkspaceVocabulary.FinalGate,
            accepted = false,
            implementationStatus = result.Report.ImplementationStatus,
            allowedOutputDirectory = SchemaDrivenCampaignWorkspaceVocabulary.RelativeOutputDirectory,
            artifactFileCount = 12,
            generatedFiles = new[]
            {
                SourceManifestFileName,
                RowSelectorFileName,
                DynamicAuthoringSchemaFileName,
                UiBindingContractFileName,
                ValidationDashboardFileName,
                ReviewProvenanceLedgerFileName,
                AuthoringActionPlanFileName,
                QualityGateScanFileName,
                WinFormsControlInventoryFileName,
                InvalidDiagnosticsMatrixFileName,
                ReportMarkdownFileName,
                ArtifactScopeReportFileName
            },
            qualityGatePassed = result.QualityGateScan.Passed,
            winFormsInventoryPassed = result.WinFormsControlInventory.Passed
        };

    private static async Task WriteJson<T>(
        string outputDirectory,
        string fileName,
        T value,
        ICollection<string> writtenFiles,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(outputDirectory, fileName);
        var json = SchemaDrivenCampaignWorkspaceHash.Serialize(value);
        await File.WriteAllTextAsync(path, json + Environment.NewLine, Utf8NoBom, cancellationToken).ConfigureAwait(false);
        writtenFiles.Add(fileName);
    }

    private static string Hash<T>(T value) =>
        SchemaDrivenCampaignWorkspaceHash.Sha256(SchemaDrivenCampaignWorkspaceHash.Serialize(value));

    private static string Normalize(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

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
