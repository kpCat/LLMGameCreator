using System.Text;

namespace LLMGameCreator.Application.Design.GeneratorSpineQualityConsolidation;

public sealed class GeneratorSpineQualityEvidenceService
{
    public const string QualityInventorySummaryJsonFileName = "quality-inventory-summary.json";
    public const string SourceFormatRiskReportJsonFileName = "source-format-risk-report.json";
    public const string LargeFileAndMethodRiskReportJsonFileName = "large-file-and-method-risk-report.json";
    public const string UnityAlphaBootstrapRiskReportJsonFileName = "unity-alpha-bootstrap-risk-report.json";
    public const string ProofQualityRiskReportJsonFileName = "proof-quality-risk-report.json";
    public const string ArtifactReproducibilityRiskReportJsonFileName = "artifact-reproducibility-risk-report.json";
    public const string SafeFixSummaryJsonFileName = "safe-fix-summary.json";
    public const string TechnicalDebtRegisterJsonFileName = "technical-debt-register.json";
    public const string QualityDashboardJsonFileName = "quality-dashboard.json";
    public const string ReportMarkdownFileName = "generator-spine-quality-consolidation-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public GeneratorSpineQualityBuildResult Build(string projectRootPath)
    {
        var scan = new GeneratorSpineQualityScanner().ScanProject(projectRootPath);
        var findings = new GeneratorSpineQualityRiskClassifier().Classify(scan);
        var inventory = BuildInventory(scan);
        var sourceFormat = BuildSourceFormatReport(scan);
        var largeFile = BuildLargeFileReport(scan);
        var proof = BuildProofQualityReport(scan);
        var artifact = new ArtifactReproducibilityRiskReport
        {
            AbsolutePathLikeStrings = scan.AbsolutePathLikeArtifacts,
            TimestampLikeValues = scan.TimestampLikeArtifacts
        };
        var safeFix = BuildSafeFixSummary();
        var debt = new TechnicalDebtRegister { Findings = findings };
        var dashboard = BuildDashboard(inventory, debt);

        return new GeneratorSpineQualityBuildResult
        {
            Inventory = inventory,
            SourceFormatRiskReport = sourceFormat,
            LargeFileAndMethodRiskReport = largeFile,
            UnityAlphaBootstrapRiskReport = scan.UnityAlphaBootstrap,
            ProofQualityRiskReport = proof,
            ArtifactReproducibilityRiskReport = artifact,
            SafeFixSummary = safeFix,
            TechnicalDebtRegister = debt,
            QualityDashboard = dashboard,
            ReportMarkdown = RenderReport(inventory, sourceFormat, largeFile, scan.UnityAlphaBootstrap, proof, artifact, safeFix, debt, dashboard),
            DebtRegisterMarkdown = RenderDebtRegisterMarkdown(debt)
        };
    }

    public async Task<GeneratorSpineQualityWriteResult> BuildAndWriteAsync(string projectRootPath, CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GeneratorSpineQualityWriteResult> WriteAsync(
        string projectRootPath,
        GeneratorSpineQualityBuildResult result,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, GeneratorSpineQualityVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        ResetDirectory(outputDirectory);

        var written = new List<string>();
        await WriteText(outputDirectory, QualityInventorySummaryJsonFileName, Serialize(result.Inventory), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SourceFormatRiskReportJsonFileName, Serialize(result.SourceFormatRiskReport), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, LargeFileAndMethodRiskReportJsonFileName, Serialize(result.LargeFileAndMethodRiskReport), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityAlphaBootstrapRiskReportJsonFileName, Serialize(result.UnityAlphaBootstrapRiskReport), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ProofQualityRiskReportJsonFileName, Serialize(result.ProofQualityRiskReport), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ArtifactReproducibilityRiskReportJsonFileName, Serialize(result.ArtifactReproducibilityRiskReport), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SafeFixSummaryJsonFileName, Serialize(result.SafeFixSummary), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, TechnicalDebtRegisterJsonFileName, Serialize(result.TechnicalDebtRegister), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, QualityDashboardJsonFileName, Serialize(result.QualityDashboard), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ReportMarkdownFileName, result.ReportMarkdown, written, cancellationToken).ConfigureAwait(false);

        var debtPath = Path.GetFullPath(Path.Combine(projectRoot, GeneratorSpineQualityVocabulary.TechnicalDebtMarkdownPath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, debtPath);
        Directory.CreateDirectory(Path.GetDirectoryName(debtPath)!);
        await File.WriteAllTextAsync(debtPath, result.DebtRegisterMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(debtPath);

        return new GeneratorSpineQualityWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            DebtRegisterMarkdownPath = debtPath,
            WrittenFiles = written.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static QualityInventorySummary BuildInventory(GeneratorSpineQualityScanResult scan) =>
        new()
        {
            Accepted = false,
            SourceFileCount = scan.SourceFiles.Count,
            ArtifactFileCount = scan.ArtifactFileCount,
            ProductSmokeFileCount = scan.ProductSmokeRecords.Count,
            MinifiedCandidateCount = scan.SourceFiles.Count(item => item.IsOneLineOrMinifiedCandidate),
            LargeFileCandidateCount = scan.SourceFiles.Count(item => item.IsLargeFileCandidate),
            LargeMethodCandidateCount = scan.LargeMethods.Count,
            AbsolutePathLikeArtifactCount = scan.AbsolutePathLikeArtifacts.Count,
            TimestampLikeArtifactCount = scan.TimestampLikeArtifacts.Count,
            ShallowProductSmokeCandidateCount = scan.ProductSmokeRecords.Count(item => item.ReportOnlyShallowCandidate),
            SeamRoleFolderCount = scan.RepeatedSeamRoles.Count,
            UnityBootstrapLineCount = scan.UnityAlphaBootstrap.LineCount,
            UnityBootstrapMarkerRouteCount = scan.UnityAlphaBootstrap.MarkerRouteCount
        };

    private static SourceFormatRiskReport BuildSourceFormatReport(GeneratorSpineQualityScanResult scan) =>
        new()
        {
            MinifiedCandidates = scan.SourceFiles
                .Where(item => item.IsOneLineOrMinifiedCandidate)
                .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
                .ToList(),
            ExtremeLineLengthCandidates = scan.SourceFiles
                .Where(item => item.HasExtremeLineLength)
                .OrderByDescending(item => item.MaxLineLength)
                .ThenBy(item => item.RelativePath, StringComparer.Ordinal)
                .ToList(),
            TopMaxLineLengthFiles = scan.SourceFiles
                .OrderByDescending(item => item.MaxLineLength)
                .ThenBy(item => item.RelativePath, StringComparer.Ordinal)
                .Take(25)
                .ToList()
        };

    private static LargeFileAndMethodRiskReport BuildLargeFileReport(GeneratorSpineQualityScanResult scan) =>
        new()
        {
            LargeFileCandidates = scan.SourceFiles
                .Where(item => item.IsLargeFileCandidate)
                .OrderByDescending(item => item.LineCount)
                .ThenBy(item => item.RelativePath, StringComparer.Ordinal)
                .ToList(),
            LargeMethodCandidates = scan.LargeMethods,
            RepeatedSeamRolesByFolder = scan.RepeatedSeamRoles
        };

    private static ProofQualityRiskReport BuildProofQualityReport(GeneratorSpineQualityScanResult scan) =>
        new()
        {
            ProductSmokeRecords = scan.ProductSmokeRecords,
            ShallowProductSmokeCandidates = scan.ProductSmokeRecords
                .Where(item => item.ReportOnlyShallowCandidate)
                .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
                .ToList(),
            Goal071ProofIndicators = scan.Goal071ProofIndicators
        };

    private static SafeFixSummary BuildSafeFixSummary() =>
        new()
        {
            FixedItems =
            [
                "Recorded Goal 071 user handoff acceptance before Goal 072 in the state docs quartet.",
                "Added deterministic BCL-only Goal 072 scanner/evidence seam.",
                "Added concrete technical debt register instead of broad source refactoring."
            ],
            DeferredItems =
            [
                "Broad shared SourceLoader/EvidenceService/Hash/Validator/UnityProofRunner extraction remains P2 future work.",
                "Unity Alpha bootstrap decomposition remains a dedicated P1 follow-up because broad Unity architecture changes are forbidden here."
            ],
            ForbiddenScopeNotTouched =
            [
                "src/LLMGameCreator.GamePackage/**",
                "src/LLMGameCreator.Runtime/**",
                "src/LLMGameCreator.Runtime.Abstractions/**",
                "src/LLMGameCreator.WinForms/**",
                "src/LLMGameCreator.Infrastructure/**",
                "src/LLMGameCreator.Scripting/**",
                "generator-library/**",
                "samples/**",
                "templates/**",
                "*.sln",
                "*.csproj"
            ]
        };

    private static QualityDashboard BuildDashboard(QualityInventorySummary inventory, TechnicalDebtRegister debt)
    {
        var p0 = debt.Findings.Count(item => item.Severity == "P0");
        var p1 = debt.Findings.Count(item => item.Severity == "P1");
        var p2 = debt.Findings.Count(item => item.Severity == "P2");
        var p3 = debt.Findings.Count(item => item.Severity == "P3");
        var status = p0 == 0 ? "GREEN" : "BLOCKED";
        var actions = new List<string>();
        if (p0 > 0)
        {
            actions.Add("Repair or explicitly block on P0 findings before accepting Goal 072.");
        }

        if (p1 > 0)
        {
            actions.Add("Schedule a bounded P1 follow-up for Unity Alpha bootstrap and largest source/test seams.");
        }

        if (p2 > 0)
        {
            actions.Add("Plan a future shared generator spine infrastructure extraction only after current proof routes stay green.");
        }

        if (actions.Count == 0)
        {
            actions.Add("Review Goal 072 evidence and keep the gate required until user acceptance.");
        }

        return new QualityDashboard
        {
            Status = status,
            P0Count = p0,
            P1Count = p1,
            P2Count = p2,
            P3Count = p3,
            RecommendedNextActions = actions,
            InventoryHash = Hash(Serialize(inventory)),
            DebtRegisterHash = Hash(Serialize(debt))
        };
    }

    private static string RenderReport(
        QualityInventorySummary inventory,
        SourceFormatRiskReport sourceFormat,
        LargeFileAndMethodRiskReport largeFile,
        UnityAlphaBootstrapRiskRecord unity,
        ProofQualityRiskReport proof,
        ArtifactReproducibilityRiskReport artifact,
        SafeFixSummary safeFix,
        TechnicalDebtRegister debt,
        QualityDashboard dashboard)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Goal 072 Generator Spine Quality Consolidation Report");
        builder.AppendLine();
        builder.AppendLine("generator_spine_quality_consolidation_verification required");
        builder.AppendLine("accepted=false");
        builder.AppendLine("implementationStatus=" + dashboard.Status);
        builder.AppendLine("sourceFileCount=" + inventory.SourceFileCount);
        builder.AppendLine("artifactFileCount=" + inventory.ArtifactFileCount);
        builder.AppendLine("productSmokeFileCount=" + inventory.ProductSmokeFileCount);
        builder.AppendLine("minifiedCandidateCount=" + inventory.MinifiedCandidateCount);
        builder.AppendLine("largeFileCandidateCount=" + inventory.LargeFileCandidateCount);
        builder.AppendLine("largeMethodCandidateCount=" + inventory.LargeMethodCandidateCount);
        builder.AppendLine("absolutePathLikeArtifactCount=" + inventory.AbsolutePathLikeArtifactCount);
        builder.AppendLine("timestampLikeArtifactCount=" + inventory.TimestampLikeArtifactCount);
        builder.AppendLine("shallowProductSmokeCandidateCount=" + inventory.ShallowProductSmokeCandidateCount);
        builder.AppendLine("unityBootstrapLineCount=" + inventory.UnityBootstrapLineCount);
        builder.AppendLine("unityBootstrapMarkerRouteCount=" + inventory.UnityBootstrapMarkerRouteCount);
        builder.AppendLine("p0Count=" + dashboard.P0Count);
        builder.AppendLine("p1Count=" + dashboard.P1Count);
        builder.AppendLine("p2Count=" + dashboard.P2Count);
        builder.AppendLine("p3Count=" + dashboard.P3Count);
        builder.AppendLine("inventoryHash=" + dashboard.InventoryHash);
        builder.AppendLine("debtRegisterHash=" + dashboard.DebtRegisterHash);
        builder.AppendLine();
        builder.AppendLine("## Goal 071 Proof Indicators");
        builder.AppendLine("- proofQualityPassed=" + proof.Goal071ProofIndicators.ProofQualityPassed);
        builder.AppendLine("- commandPlanRows=" + proof.Goal071ProofIndicators.CommandPlanRowCount);
        builder.AppendLine("- expectedMarkers=" + proof.Goal071ProofIndicators.ExpectedMarkerCount);
        builder.AppendLine("- matchedMarkers=" + proof.Goal071ProofIndicators.MatchedMarkerCount);
        builder.AppendLine("- missingMarkers=" + proof.Goal071ProofIndicators.MissingMarkerCount);
        builder.AppendLine("- actionCount=" + proof.Goal071ProofIndicators.ActionCount);
        builder.AppendLine("- transitionCount=" + proof.Goal071ProofIndicators.TransitionCount);
        builder.AppendLine();
        builder.AppendLine("## Unity Alpha Bootstrap Risk");
        builder.AppendLine("- " + unity.RelativePath + " lines=" + unity.LineCount + " markerRoutes=" + unity.MarkerRouteCount + " nestedTypes=" + unity.PrivateNestedTypeCount + " monolithicGrowthRisk=" + unity.MonolithicGrowthRisk);
        builder.AppendLine();
        builder.AppendLine("## Source Format");
        builder.AppendLine("- minifiedCandidates=" + sourceFormat.MinifiedCandidates.Count);
        builder.AppendLine("- extremeLineLengthCandidates=" + sourceFormat.ExtremeLineLengthCandidates.Count);
        builder.AppendLine();
        builder.AppendLine("## Large File And Method Risk");
        foreach (var file in largeFile.LargeFileCandidates.Take(20))
        {
            builder.AppendLine("- " + file.RelativePath + " lines=" + file.LineCount + " maxLineLength=" + file.MaxLineLength);
        }

        foreach (var method in largeFile.LargeMethodCandidates.Take(20))
        {
            builder.AppendLine("- " + method.RelativePath + "#" + method.MethodName + " startLine=" + method.StartLine + " lines=" + method.LineCount);
        }

        builder.AppendLine();
        builder.AppendLine("## Artifact Reproducibility");
        builder.AppendLine("- absolutePathLikeStrings=" + artifact.AbsolutePathLikeStrings.Count);
        builder.AppendLine("- timestampLikeValues=" + artifact.TimestampLikeValues.Count);
        builder.AppendLine();
        builder.AppendLine("## Safe Fixes");
        foreach (var item in safeFix.FixedItems)
        {
            builder.AppendLine("- fixed: " + item);
        }

        foreach (var item in safeFix.DeferredItems)
        {
            builder.AppendLine("- deferred: " + item);
        }

        builder.AppendLine();
        builder.AppendLine("## Findings");
        foreach (var finding in debt.Findings)
        {
            builder.AppendLine("- " + finding.FindingId + " " + finding.Severity + " area=" + finding.Area + " fixed=" + finding.FixedInGoal072);
            builder.AppendLine("  evidence: " + finding.Evidence);
            builder.AppendLine("  next: " + finding.RecommendedFutureGoal);
            if (!string.IsNullOrWhiteSpace(finding.WhyNotFixed))
            {
                builder.AppendLine("  whyNotFixed: " + finding.WhyNotFixed);
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Recommended Next Actions");
        foreach (var action in dashboard.RecommendedNextActions)
        {
            builder.AppendLine("- " + action);
        }

        return builder.ToString();
    }

    private static string RenderDebtRegisterMarkdown(TechnicalDebtRegister debt)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Generator Spine Quality Debt Register");
        builder.AppendLine();
        builder.AppendLine("Source: Goal 072 generator spine quality consolidation.");
        builder.AppendLine();
        builder.AppendLine("| Finding id | Severity | Area | Evidence | Recommended future goal | Fixed in Goal 072 | Why not fixed |");
        builder.AppendLine("|---|---|---|---|---|---|---|");
        foreach (var finding in debt.Findings)
        {
            builder.AppendLine("| "
                + Escape(finding.FindingId) + " | "
                + Escape(finding.Severity) + " | "
                + Escape(finding.Area) + " | "
                + Escape(finding.Evidence) + " | "
                + Escape(finding.RecommendedFutureGoal) + " | "
                + finding.FixedInGoal072.ToString().ToLowerInvariant() + " | "
                + Escape(finding.WhyNotFixed) + " |");
        }

        if (debt.Findings.Count == 0)
        {
            builder.AppendLine("| GQ-NONE | P3 | none | No actionable debt findings were emitted by the scanner. | Review Goal 072 evidence. | false | No fix needed. |");
        }

        return builder.ToString();
    }

    private static string Escape(string value) =>
        value.Replace("|", "\\|", StringComparison.Ordinal).Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);

    private static async Task WriteText(string directory, string fileName, string content, List<string> written, CancellationToken cancellationToken)
    {
        var path = Path.Combine(directory, fileName);
        EnsureContained(directory, path);
        await File.WriteAllTextAsync(path, content, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(path);
    }

    private static void ResetDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        Directory.CreateDirectory(directory);
    }

    private static void EnsureContained(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes expected root: " + normalizedPath);
        }
    }

    private static string Serialize<T>(T value) =>
        GeneratorSpineQualityHash.Serialize(value);

    private static string Hash(string text) =>
        GeneratorSpineQualityHash.Sha256(text);
}
