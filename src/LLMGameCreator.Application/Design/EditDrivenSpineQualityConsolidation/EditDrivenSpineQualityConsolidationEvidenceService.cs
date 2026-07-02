using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;

namespace LLMGameCreator.Application.Design.EditDrivenSpineQualityConsolidation;

public sealed class EditDrivenSpineQualityConsolidationEvidenceService
{
    public const string ReportMarkdownFileName = "edit-driven-spine-quality-consolidation-report.md";
    public const string SpineChainManifestFileName = "spine-chain-manifest.json";
    public const string AcceptanceReadinessDashboardFileName = "acceptance-readiness-dashboard.json";
    public const string NegativeProofIndexFileName = "negative-proof-index.json";
    public const string WorkspaceBindingInventoryFileName = "workspace-binding-inventory.json";
    public const string SourceHealthScanFileName = "source-health-scan.json";
    public const string QualityDebtClassificationFileName = "quality-debt-classification.json";
    public const string ArtifactHygieneScanFileName = "artifact-hygiene-scan.json";
    public const string QualityGateScanFileName = "quality-gate-scan.json";
    public const string SourceArtifactManifestFileName = "source-artifact-manifest.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly EditDrivenSpineQualityConsolidationQualityGateScanner _scanner;

    public EditDrivenSpineQualityConsolidationEvidenceService(
        EditDrivenSpineQualityConsolidationQualityGateScanner? scanner = null)
    {
        _scanner = scanner ?? new EditDrivenSpineQualityConsolidationQualityGateScanner();
    }

    public EditDrivenSpineQualityConsolidationBuildResult Build(
        string projectRootPath,
        EditDrivenSpineQualityConsolidationBuildOptions? options = null)
    {
        var root = Path.GetFullPath(projectRootPath);
        options ??= new EditDrivenSpineQualityConsolidationBuildOptions();
        var source = BuildSourceArtifactManifest(root, options);
        var chain = BuildSpineChainManifest(root, source, options);
        var packageReadProof = ReadArtifact<EditDrivenReviewPackagePlayableSessionPackageReadProof>(
            root,
            Goal078PackageReadProofPath,
            options);
        var replayProof = ReadArtifact<EditDrivenReviewPackagePlayableSessionReplayProof>(
            root,
            Goal078ReplayProofPath,
            options);
        var negative = BuildNegativeProofIndex(root, options);
        var binding = _scanner.BuildWorkspaceBindingInventory(root);
        var expectedAlphaHash = ExtractJsonString(ReadArtifactText(root, Goal078QualityGatePath, options, out _),
            "alphaRuntimeBootstrapHash");
        var sourceHealth = _scanner.ScanSourceHealth(root, expectedAlphaHash);
        var preArtifacts = BuildArtifactPayloads(
            source,
            chain,
            dashboard: null,
            negative,
            binding,
            sourceHealth,
            debt: null,
            hygiene: null,
            quality: null);
        var hygiene = _scanner.ScanArtifactHygiene(preArtifacts);
        var debt = _scanner.ClassifyDebt(source, binding, negative, sourceHealth, hygiene);
        var quality = _scanner.BuildQualityGateScan(source, binding, negative, sourceHealth, hygiene, debt);
        var dashboard = BuildReadinessDashboard(source, chain, packageReadProof, replayProof, negative, debt, quality);
        var reportWithoutHash = BuildReport(source, chain, dashboard, negative, binding, sourceHealth, debt, hygiene, quality);
        var report = reportWithoutHash with { DeterministicHash = Hash(reportWithoutHash) };
        var artifacts = BuildArtifactPayloads(
            source,
            chain,
            dashboard,
            negative,
            binding,
            sourceHealth,
            debt,
            hygiene,
            quality);
        var reportMarkdown = EditDrivenSpineQualityConsolidationReportRenderer.Render(
            report,
            chain,
            dashboard,
            debt,
            sourceHealth);

        return new EditDrivenSpineQualityConsolidationBuildResult
        {
            SourceArtifactManifest = source,
            SpineChainManifest = chain,
            AcceptanceReadinessDashboard = dashboard,
            NegativeProofIndex = negative,
            WorkspaceBindingInventory = binding,
            SourceHealthScan = sourceHealth,
            QualityDebtClassification = debt,
            ArtifactHygieneScan = hygiene,
            QualityGateScan = quality,
            Report = report,
            ReportMarkdown = reportMarkdown,
            ArtifactJsonByFileName = artifacts
        };
    }

    public async Task<EditDrivenSpineQualityConsolidationWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<EditDrivenSpineQualityConsolidationWriteResult> WriteAsync(
        string projectRootPath,
        EditDrivenSpineQualityConsolidationBuildResult result,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(projectRootPath);
        var outputDirectory = Resolve(root, EditDrivenSpineQualityConsolidationVocabulary.RelativeOutputDirectory);
        ResetDirectory(outputDirectory);
        var written = new List<string>();

        foreach (var artifact in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, artifact.Key);
            await File.WriteAllTextAsync(path, artifact.Value + Environment.NewLine, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(reportPath);

        return new EditDrivenSpineQualityConsolidationWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<string> RequiredArtifactNames() =>
    [
        ReportMarkdownFileName,
        SpineChainManifestFileName,
        AcceptanceReadinessDashboardFileName,
        NegativeProofIndexFileName,
        WorkspaceBindingInventoryFileName,
        SourceHealthScanFileName,
        QualityDebtClassificationFileName,
        ArtifactHygieneScanFileName,
        QualityGateScanFileName,
        SourceArtifactManifestFileName
    ];

    private static EditDrivenSpineQualityConsolidationSourceArtifactManifest BuildSourceArtifactManifest(
        string projectRoot,
        EditDrivenSpineQualityConsolidationBuildOptions options)
    {
        var artifacts = RequiredSourceArtifactPaths()
            .Select(item => ReadSourceArtifact(projectRoot, item.SourceGoal, item.RelativePath, options))
            .OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
            .ToList();
        var goals = BuildGoalEvidence(projectRoot, options);
        var docs = ReadStateDocs(projectRoot);
        var debtRegister = ReadOptional(projectRoot, "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md");
        var diagnostics = new List<EditDrivenSpineQualityConsolidationDiagnostic>();
        diagnostics.AddRange(artifacts
            .Where(item => !item.Exists)
            .Select(item => Error(
                "goal079.source.required_artifact_missing",
                item.ArtifactRelativePath,
                "Goal 079 must consume this real Goal 074-078 artifact.")));
        diagnostics.AddRange(goals.SelectMany(goal => goal.Diagnostics));

        var goal078 = goals.Single(item => item.GoalNumber == 78);
        var goal078GreenAcceptedFalse = goal078.ImplementationStatus == "GREEN" && goal078.Accepted == "false";
        if (!goal078GreenAcceptedFalse)
        {
            diagnostics.Add(Error(
                "goal079.preflight.goal078_not_green_accepted_false",
                goal078.ReportRelativePath,
                "Goal 078 artifact must remain GREEN and accepted=false."));
        }

        var goal078Handoff = docs.Contains(
            EditDrivenSpineQualityConsolidationVocabulary.Goal078AcceptedHandoffText,
            StringComparison.Ordinal);
        if (!goal078Handoff)
        {
            diagnostics.Add(Error(
                "goal079.preflight.goal078_handoff_missing",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal 078 must be recorded as accepted by user handoff before Goal 079."));
        }

        var goal072Blocked = docs.Contains("generator_spine_quality_consolidation_verification required", StringComparison.Ordinal)
            && docs.Contains(EditDrivenSpineQualityConsolidationVocabulary.Goal072BlockedText, StringComparison.Ordinal);
        if (!goal072Blocked)
        {
            diagnostics.Add(Error(
                "goal079.preflight.goal072_blocked_history_missing",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal 072 must remain historical BLOCKED produced-for-review evidence."));
        }

        var adaptiveDebt = debtRegister.Contains(
            EditDrivenSpineQualityConsolidationVocabulary.AdaptiveDocsDebtCommit,
            StringComparison.Ordinal)
            && debtRegister.Contains("P3", StringComparison.Ordinal);

        return new EditDrivenSpineQualityConsolidationSourceArtifactManifest
        {
            Goal078AcceptedByUserHandoff = goal078Handoff,
            Goal078ArtifactGreenAcceptedFalse = goal078GreenAcceptedFalse,
            Goal072PreservedAsHistoricalBlocked = goal072Blocked,
            AdaptiveDocsDebtStillP3 = adaptiveDebt,
            SourceArtifactCount = artifacts.Count,
            GoalEvidence = goals,
            SourceArtifacts = artifacts,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyList<EditDrivenSpineQualityConsolidationGoalEvidence> BuildGoalEvidence(
        string projectRoot,
        EditDrivenSpineQualityConsolidationBuildOptions options) =>
        GoalSpecs()
            .Select(spec =>
            {
                var report = ReadArtifactText(projectRoot, spec.ReportPath, options, out var reportExists);
                var quality = ReadArtifactText(projectRoot, spec.QualityPath, options, out var qualityExists);
                var fields = ParseReportFields(report);
                var diagnostics = new List<EditDrivenSpineQualityConsolidationDiagnostic>();
                if (!reportExists)
                {
                    diagnostics.Add(Error("goal079.source.report_missing", spec.ReportPath, "Required report is missing."));
                }

                if (!qualityExists)
                {
                    diagnostics.Add(Error(
                        "goal079.source.quality_gate_missing",
                        spec.QualityPath,
                        "Required quality-gate artifact is missing."));
                }

                return new EditDrivenSpineQualityConsolidationGoalEvidence
                {
                    GoalId = spec.GoalId,
                    GoalNumber = spec.GoalNumber,
                    ReportRelativePath = spec.ReportPath,
                    QualityGateRelativePath = spec.QualityPath,
                    ReportExists = reportExists,
                    QualityGateExists = qualityExists,
                    ReportHash = reportExists ? Hash(report) : string.Empty,
                    QualityGateHash = qualityExists ? Hash(quality) : string.Empty,
                    DeclaredReportHash = fields.DeclaredHash,
                    ImplementationStatus = fields.ImplementationStatus,
                    Accepted = fields.Accepted,
                    Gate = fields.Gate,
                    Diagnostics = SortDiagnostics(diagnostics)
                };
            })
            .ToList();

    private static EditDrivenSpineQualityConsolidationChainManifest BuildSpineChainManifest(
        string projectRoot,
        EditDrivenSpineQualityConsolidationSourceArtifactManifest source,
        EditDrivenSpineQualityConsolidationBuildOptions options)
    {
        var packageRead = ReadArtifactText(projectRoot, Goal078PackageReadProofPath, options, out var packageReadExists);
        var replay = ReadArtifactText(projectRoot, Goal078ReplayProofPath, options, out var replayExists);
        var negative = ReadArtifactText(projectRoot, Goal078NegativeProofPath, options, out var negativeExists);

        return new EditDrivenSpineQualityConsolidationChainManifest
        {
            ChainItemCount = source.GoalEvidence.Count,
            ChainItems = source.GoalEvidence
                .OrderBy(item => item.GoalNumber)
                .Select(item => new EditDrivenSpineQualityConsolidationChainItem
                {
                    GoalId = item.GoalId,
                    GoalNumber = item.GoalNumber,
                    ReportHash = item.ReportHash,
                    DeclaredReportHash = item.DeclaredReportHash,
                    QualityGateHash = item.QualityGateHash,
                    ImplementationStatus = item.ImplementationStatus,
                    Accepted = item.Accepted
                })
                .ToList(),
            Goal078PackageReadProofHash = packageReadExists ? Hash(packageRead) : string.Empty,
            Goal078ReplayProofHash = replayExists ? Hash(replay) : string.Empty,
            Goal078NegativeProofHash = negativeExists ? Hash(negative) : string.Empty
        };
    }

    private static EditDrivenSpineQualityConsolidationNegativeProofIndex BuildNegativeProofIndex(
        string projectRoot,
        EditDrivenSpineQualityConsolidationBuildOptions options)
    {
        var text = ReadArtifactText(projectRoot, Goal078NegativeProofPath, options, out var exists);
        var proof = EditDrivenSpineQualityConsolidationHash
            .Deserialize<EditDrivenReviewPackagePlayableSessionNegativeProof>(text);
        var diagnostics = new List<EditDrivenSpineQualityConsolidationDiagnostic>();
        if (!exists || proof is null)
        {
            diagnostics.Add(Error(
                "goal079.negative.goal078_negative_proof_missing",
                Goal078NegativeProofPath,
                "Goal 078 negative proof must exist as a real JSON artifact."));
            return new EditDrivenSpineQualityConsolidationNegativeProofIndex
            {
                Passed = false,
                Diagnostics = SortDiagnostics(diagnostics)
            };
        }

        var scenarios = proof.Scenarios
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(item => new EditDrivenSpineQualityConsolidationNegativeProofScenario
            {
                ScenarioId = item.ScenarioId,
                ExpectedStatus = item.ExpectedStatus,
                ActualStatus = item.ActualStatus,
                DiagnosticCount = item.Diagnostics.Count
            })
            .ToList();
        foreach (var scenarioId in EditDrivenSpineQualityConsolidationVocabulary.RequiredNegativeScenarioIds)
        {
            var scenario = proof.Scenarios.FirstOrDefault(item =>
                string.Equals(item.ScenarioId, scenarioId, StringComparison.Ordinal));
            if (scenario is null)
            {
                diagnostics.Add(Error(
                    "goal079.negative.required_scenario_missing",
                    scenarioId,
                    "Goal 078 negative proof must include this rejection scenario."));
                continue;
            }

            if (scenario.ActualStatus != "rejected" || scenario.Diagnostics.Count == 0)
            {
                diagnostics.Add(Error(
                    "goal079.negative.scenario_not_real_rejection",
                    scenarioId,
                    "Negative proof must include a concrete rejected diagnostic, not only a report flag."));
            }
        }

        return new EditDrivenSpineQualityConsolidationNegativeProofIndex
        {
            Passed = proof.Passed && diagnostics.Count == 0,
            ScenarioCount = scenarios.Count,
            Scenarios = scenarios,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static EditDrivenSpineQualityConsolidationReadinessDashboard BuildReadinessDashboard(
        EditDrivenSpineQualityConsolidationSourceArtifactManifest source,
        EditDrivenSpineQualityConsolidationChainManifest chain,
        EditDrivenReviewPackagePlayableSessionPackageReadProof? packageReadProof,
        EditDrivenReviewPackagePlayableSessionReplayProof? replayProof,
        EditDrivenSpineQualityConsolidationNegativeProofIndex negative,
        EditDrivenSpineQualityConsolidationDebtClassification debt,
        EditDrivenSpineQualityConsolidationQualityGateScan quality) =>
        new()
        {
            ImplementationStatus = quality.Passed ? "GREEN" : "BLOCKED",
            Accepted = false,
            Goal078AcceptedByUserHandoff = source.Goal078AcceptedByUserHandoff,
            Goal078ArtifactStillAcceptedFalse = source.Goal078ArtifactGreenAcceptedFalse,
            PackageReadProofPassed = packageReadProof?.Passed == true
                && packageReadProof.AllLedgerFilesExist
                && packageReadProof.AllLedgerFileHashesMatch,
            ReplayProofPassed = replayProof?.Passed == true,
            ReplayFinalHashMatchesOriginal = replayProof?.ReplayFinalHashMatchesOriginal == true,
            NegativeProofPassed = negative.Passed,
            ChainItemCount = chain.ChainItemCount,
            P0Count = debt.P0Count,
            P1Count = debt.P1Count,
            P2Count = debt.P2Count,
            P3Count = debt.P3Count
        };

    private static EditDrivenSpineQualityConsolidationReport BuildReport(
        EditDrivenSpineQualityConsolidationSourceArtifactManifest source,
        EditDrivenSpineQualityConsolidationChainManifest chain,
        EditDrivenSpineQualityConsolidationReadinessDashboard dashboard,
        EditDrivenSpineQualityConsolidationNegativeProofIndex negative,
        EditDrivenSpineQualityConsolidationWorkspaceBindingInventory binding,
        EditDrivenSpineQualityConsolidationSourceHealthScan sourceHealth,
        EditDrivenSpineQualityConsolidationDebtClassification debt,
        EditDrivenSpineQualityConsolidationArtifactHygieneScan hygiene,
        EditDrivenSpineQualityConsolidationQualityGateScan quality)
    {
        var diagnostics = SortDiagnostics(
            source.Diagnostics
                .Concat(negative.Diagnostics)
                .Concat(binding.Diagnostics)
                .Concat(sourceHealth.Diagnostics)
                .Concat(hygiene.Diagnostics)
                .Concat(quality.Diagnostics));
        return new EditDrivenSpineQualityConsolidationReport
        {
            ImplementationStatus = quality.Passed ? "GREEN" : "BLOCKED",
            Accepted = false,
            Goal078AcceptedByUserHandoff = source.Goal078AcceptedByUserHandoff,
            ChainItemCount = chain.ChainItemCount,
            P0Count = debt.P0Count,
            P1Count = debt.P1Count,
            P2Count = debt.P2Count,
            P3Count = debt.P3Count,
            BlockerCount = debt.P0Count + debt.P1Count,
            ParentWorkspaceLineCount = sourceHealth.ParentWorkspaceLineCount,
            MaxCSharpLineLength = sourceHealth.MaxLineLength,
            LogicalMaxLineLength = sourceHealth.LogicalMaxLineLength,
            ZeroLfSourceFileCount = sourceHealth.ZeroLfSourceFileCount,
            CrOnlySourceFileCount = sourceHealth.CrOnlySourceFileCount,
            RawPhysicalMaxLineLength = sourceHealth.RawPhysicalMaxLineLength,
            RawPhysicalOneLineSourceFileCount = sourceHealth.RawPhysicalOneLineSourceFileCount,
            MinifiedSourceFileCount = sourceHealth.MinifiedSourceFileCount,
            FilesOver1000LinesCount = sourceHealth.FilesOver1000LinesCount,
            AlphaRuntimeBootstrapLineCount = sourceHealth.AlphaRuntimeBootstrapLineCount,
            AlphaRuntimeBootstrapHash = sourceHealth.AlphaRuntimeBootstrapHash,
            SourceArtifactManifestHash = Hash(source),
            SpineChainManifestHash = Hash(chain),
            AcceptanceReadinessDashboardHash = Hash(dashboard),
            NegativeProofIndexHash = Hash(negative),
            WorkspaceBindingInventoryHash = Hash(binding),
            SourceHealthScanHash = Hash(sourceHealth),
            QualityDebtClassificationHash = Hash(debt),
            ArtifactHygieneScanHash = Hash(hygiene),
            QualityGateScanHash = Hash(quality),
            Diagnostics = diagnostics
        };
    }

    private static SortedDictionary<string, string> BuildArtifactPayloads(
        EditDrivenSpineQualityConsolidationSourceArtifactManifest source,
        EditDrivenSpineQualityConsolidationChainManifest chain,
        EditDrivenSpineQualityConsolidationReadinessDashboard? dashboard,
        EditDrivenSpineQualityConsolidationNegativeProofIndex negative,
        EditDrivenSpineQualityConsolidationWorkspaceBindingInventory binding,
        EditDrivenSpineQualityConsolidationSourceHealthScan sourceHealth,
        EditDrivenSpineQualityConsolidationDebtClassification? debt,
        EditDrivenSpineQualityConsolidationArtifactHygieneScan? hygiene,
        EditDrivenSpineQualityConsolidationQualityGateScan? quality)
    {
        var artifacts = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceArtifactManifestFileName] = Serialize(source),
            [SpineChainManifestFileName] = Serialize(chain),
            [NegativeProofIndexFileName] = Serialize(negative),
            [WorkspaceBindingInventoryFileName] = Serialize(binding),
            [SourceHealthScanFileName] = Serialize(sourceHealth)
        };
        if (dashboard is not null)
        {
            artifacts[AcceptanceReadinessDashboardFileName] = Serialize(dashboard);
        }

        if (debt is not null)
        {
            artifacts[QualityDebtClassificationFileName] = Serialize(debt);
        }

        if (hygiene is not null)
        {
            artifacts[ArtifactHygieneScanFileName] = Serialize(hygiene);
        }

        if (quality is not null)
        {
            artifacts[QualityGateScanFileName] = Serialize(quality);
        }

        return artifacts;
    }

    private static EditDrivenSpineQualityConsolidationReportFields ParseReportFields(string report)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in report.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            if (!line.StartsWith("- ", StringComparison.Ordinal))
            {
                continue;
            }

            var separator = line.IndexOf(':');
            if (separator < 0)
            {
                continue;
            }

            var key = line.Substring(2, separator - 2).Trim();
            var value = line[(separator + 1)..].Trim();
            values[key] = value.EndsWith(" required", StringComparison.Ordinal)
                ? value[..^" required".Length]
                : value;
        }

        return new EditDrivenSpineQualityConsolidationReportFields
        {
            ImplementationStatus = values.GetValueOrDefault("implementationStatus", string.Empty),
            Accepted = values.GetValueOrDefault("accepted", string.Empty),
            Gate = values.GetValueOrDefault("gate", string.Empty),
            DeclaredHash = values.GetValueOrDefault(
                "reportHash",
                values.GetValueOrDefault("deterministicHash", string.Empty))
        };
    }

    private static string ExtractJsonString(string json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(propertyName, out var property)
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static T? ReadArtifact<T>(
        string projectRoot,
        string relativePath,
        EditDrivenSpineQualityConsolidationBuildOptions options)
    {
        var text = ReadArtifactText(projectRoot, relativePath, options, out var exists);
        return exists ? EditDrivenSpineQualityConsolidationHash.Deserialize<T>(text) : default;
    }

    private static EditDrivenSpineQualityConsolidationSourceArtifactReference ReadSourceArtifact(
        string projectRoot,
        string sourceGoal,
        string relativePath,
        EditDrivenSpineQualityConsolidationBuildOptions options)
    {
        var text = ReadArtifactText(projectRoot, relativePath, options, out var exists);
        return new EditDrivenSpineQualityConsolidationSourceArtifactReference
        {
            SourceGoal = sourceGoal,
            ArtifactFamily = Path.GetFileNameWithoutExtension(relativePath),
            ArtifactRelativePath = relativePath,
            Exists = exists,
            ArtifactHash = exists ? Hash(text) : string.Empty
        };
    }

    private static string ReadArtifactText(
        string projectRoot,
        string relativePath,
        EditDrivenSpineQualityConsolidationBuildOptions options,
        out bool exists)
    {
        if (options.ArtifactTextOverridesByRelativePath.TryGetValue(relativePath, out var overrideText))
        {
            exists = overrideText is not null;
            return overrideText ?? string.Empty;
        }

        var path = Resolve(projectRoot, relativePath);
        exists = File.Exists(path);
        return exists ? File.ReadAllText(path, Encoding.UTF8).TrimEnd('\r', '\n') : string.Empty;
    }

    private static string ReadOptional(string projectRoot, string relativePath)
    {
        var path = Resolve(projectRoot, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string ReadStateDocs(string projectRoot) =>
        ReadOptional(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
        + Environment.NewLine
        + ReadOptional(projectRoot, "docs/CURRENT_GENERATOR_STATE.json")
        + Environment.NewLine
        + ReadOptional(projectRoot, "docs/CONTEXT_INDEX.md")
        + Environment.NewLine
        + ReadOptional(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");

    private static IReadOnlyList<GoalSpec> GoalSpecs() =>
    [
        new(
            74,
            "goal_074_schema_driven_campaign_authoring_review_workspace",
            ".llmgc/procedural/goal-074-schema-driven-campaign-authoring-review-workspace/"
                + "schema-driven-campaign-authoring-review-workspace-report.md",
            ".llmgc/procedural/goal-074-schema-driven-campaign-authoring-review-workspace/quality-gate-scan.json"),
        new(
            75,
            "goal_075_schema_driven_campaign_edit_validate_apply_loop",
            ".llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/"
                + "schema-driven-campaign-edit-validate-apply-loop-report.md",
            ".llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/quality-gate-scan.json"),
        new(
            76,
            "goal_076_edit_driven_playable_preview_refresh",
            ".llmgc/procedural/goal-076-edit-driven-playable-preview-refresh/"
                + "edit-driven-playable-preview-refresh-report.md",
            ".llmgc/procedural/goal-076-edit-driven-playable-preview-refresh/quality-gate-scan.json"),
        new(
            77,
            "goal_077_edit_driven_review_package_materialization",
            ".llmgc/procedural/goal-077-edit-driven-review-package-materialization/"
                + "edit-driven-review-package-materialization-report.md",
            ".llmgc/procedural/goal-077-edit-driven-review-package-materialization/quality-gate-scan.json"),
        new(
            78,
            "goal_078_edit_driven_review_package_playable_session",
            Goal078ReportPath,
            Goal078QualityGatePath)
    ];

    private static IReadOnlyList<SourceArtifactSpec> RequiredSourceArtifactPaths() =>
        GoalSpecs()
            .SelectMany(goal => new[]
            {
                new SourceArtifactSpec(goal.GoalId, goal.ReportPath),
                new SourceArtifactSpec(goal.GoalId, goal.QualityPath)
            })
            .Concat([
                new SourceArtifactSpec("goal_078_edit_driven_review_package_playable_session", Goal078PackageReadProofPath),
                new SourceArtifactSpec("goal_078_edit_driven_review_package_playable_session", Goal078ReplayProofPath),
                new SourceArtifactSpec("goal_078_edit_driven_review_package_playable_session", Goal078NegativeProofPath)
            ])
            .ToList();

    private static string Resolve(string projectRoot, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, path);
        return path;
    }

    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

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

    private static string Serialize<T>(T value) =>
        EditDrivenSpineQualityConsolidationHash.Serialize(value);

    private static string Hash<T>(T value) =>
        EditDrivenSpineQualityConsolidationHash.Sha256(Serialize(value));

    private static string Hash(string text) =>
        string.IsNullOrEmpty(text) ? string.Empty : EditDrivenSpineQualityConsolidationHash.Sha256(text);

    private static IReadOnlyList<EditDrivenSpineQualityConsolidationDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenSpineQualityConsolidationDiagnostic> diagnostics) =>
        EditDrivenSpineQualityConsolidationQualityGateScanner.SortDiagnostics(diagnostics);

    private static EditDrivenSpineQualityConsolidationDiagnostic Error(
        string code,
        string target,
        string message) =>
        EditDrivenSpineQualityConsolidationDiagnostic.Error(code, target, message);

    private const string Goal078ReportPath =
        ".llmgc/procedural/goal-078-edit-driven-review-package-playable-session/"
        + "edit-driven-review-package-playable-session-report.md";
    private const string Goal078QualityGatePath =
        ".llmgc/procedural/goal-078-edit-driven-review-package-playable-session/quality-gate-scan.json";
    private const string Goal078PackageReadProofPath =
        ".llmgc/procedural/goal-078-edit-driven-review-package-playable-session/package-read-proof.json";
    private const string Goal078ReplayProofPath =
        ".llmgc/procedural/goal-078-edit-driven-review-package-playable-session/playable-session-replay-proof.json";
    private const string Goal078NegativeProofPath =
        ".llmgc/procedural/goal-078-edit-driven-review-package-playable-session/tamper-negative-proof.json";

    private sealed record GoalSpec(int GoalNumber, string GoalId, string ReportPath, string QualityPath);

    private sealed record SourceArtifactSpec(string SourceGoal, string RelativePath);
}
