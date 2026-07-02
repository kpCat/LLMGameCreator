using System.Text;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.EditDrivenSpineQualityConsolidation;

public sealed class EditDrivenSpineQualityConsolidationQualityGateScanner
{
    private const int ParentWorkspaceLineLimit = 275;
    private const int LargeSourceFileByteThreshold = 1_500;

    private static readonly Regex TimestampLikePattern = new(
        @"\b20\d{2}-\d{2}-\d{2}[T ][0-2]\d:[0-5]\d:[0-5]\d",
        RegexOptions.Compiled);

    private static readonly IReadOnlyList<string> ScanDirectories =
    [
        "src/LLMGameCreator.Application/Design/SchemaDrivenCampaignAuthoringReviewWorkspace",
        "src/LLMGameCreator.Application/Design/SchemaDrivenCampaignEditValidateApplyLoop",
        "src/LLMGameCreator.Application/Design/EditDrivenPlayablePreviewRefresh",
        "src/LLMGameCreator.Application/Design/EditDrivenPlayableReviewPackageMaterialization",
        "src/LLMGameCreator.Application/Design/EditDrivenReviewPackagePlayableSession",
        "src/LLMGameCreator.Application/Design/EditDrivenSpineQualityConsolidation",
        "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace",
        "tests/LLMGameCreator.Tests/Application/SchemaDrivenCampaignEditValidateApplyLoop",
        "tests/LLMGameCreator.Tests/Application/EditDrivenPlayablePreviewRefresh",
        "tests/LLMGameCreator.Tests/Application/EditDrivenPlayableReviewPackageMaterialization",
        "tests/LLMGameCreator.Tests/Application/EditDrivenReviewPackagePlayableSession",
        "tests/LLMGameCreator.Tests/Application/EditDrivenSpineQualityConsolidation"
    ];

    private static readonly IReadOnlyList<string> ScanFiles =
    [
        "src/LLMGameCreator.WinForms/CompositionRoot.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs"
    ];

    public EditDrivenSpineQualityConsolidationWorkspaceBindingInventory BuildWorkspaceBindingInventory(
        string projectRoot)
    {
        var pageDesignerRelative =
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
            + "CampaignAuthoringReviewWorkspacePageControl.Designer.cs";
        var pageCodeRelative =
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
            + "CampaignAuthoringReviewWorkspacePageControl.cs";
        var pageDesigner = ReadOptional(projectRoot, pageDesignerRelative);
        var pageCode = ReadOptional(projectRoot, pageCodeRelative);
        var compactPageCode = Compact(pageCode);
        var surfaces = BuildSurfaceSpecs()
            .Select(spec => BuildSurface(projectRoot, pageDesigner, pageCode, compactPageCode, spec))
            .ToList();
        var dashboard = surfaces.Single(item => item.SurfaceId == "goal079_spine_quality_dashboard");
        var diagnostics = new List<EditDrivenSpineQualityConsolidationDiagnostic>();

        foreach (var surface in surfaces)
        {
            if (!surface.TabDeclared)
            {
                diagnostics.Add(Error(
                    "goal079.winforms.surface_tab_missing",
                    surface.SurfaceId,
                    "The parent workspace must declare every edit-driven child surface as a tab."));
            }

            if (!surface.ServiceBuiltDuringActivation)
            {
                diagnostics.Add(Error(
                    "goal079.winforms.surface_service_missing",
                    surface.SurfaceId,
                    "The parent workspace OnActivated path must build this child surface evidence."));
            }

            if (!surface.BoundByParent)
            {
                diagnostics.Add(Error(
                    "goal079.winforms.surface_bind_missing",
                    surface.SurfaceId,
                    "The parent workspace must bind this child surface through the parent path."));
            }

            if (!surface.SeparateUserControl)
            {
                diagnostics.Add(Error(
                    "goal079.winforms.surface_not_user_control",
                    surface.RelativePath,
                    "Every edit-driven child surface must remain a separate UserControl."));
            }
        }

        return new EditDrivenSpineQualityConsolidationWorkspaceBindingInventory
        {
            Passed = diagnostics.Count == 0,
            ParentPageDashboardTabDeclared = dashboard.TabDeclared,
            ParentPageDashboardEvidenceServiceLoaded = dashboard.ServiceBuiltDuringActivation,
            ParentPageDashboardControlBound = dashboard.BoundByParent,
            ParentPageActivationBindsGoal079Data =
                dashboard.TabDeclared && dashboard.ServiceBuiltDuringActivation && dashboard.BoundByParent,
            AllFiveChildSurfacesBound = surfaces.All(item =>
                item.TabDeclared && item.ServiceBuiltDuringActivation && item.BoundByParent),
            AllChildSurfacesSeparateUserControls = surfaces.All(item => item.SeparateUserControl),
            Surfaces = surfaces,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public EditDrivenSpineQualityConsolidationSourceHealthScan ScanSourceHealth(
        string projectRoot,
        string expectedAlphaRuntimeBootstrapHash)
    {
        var files = EnumerateFiles(projectRoot)
            .OrderBy(path => Relative(projectRoot, path), StringComparer.Ordinal)
            .Select(path => ScanFile(projectRoot, path))
            .ToList();
        var diagnostics = new List<EditDrivenSpineQualityConsolidationDiagnostic>();
        var linesOver500 = files.Sum(file => file.LinesOver500Count);
        var zeroLfWithCr = files.Count(file => file.ZeroLfWithCr);
        var crOnlyLineEndings = files.Count(file => file.ContainsCrOnlyLineEndings);
        var rawPhysicalLinesOver500 = files.Sum(file => file.RawPhysicalLinesOver500Count);
        var rawPhysicalOneLine = files.Count(file => file.RawPhysicalOneLineSourceCandidate);
        var filesOver1000 = files.Count(file =>
            file.FileOver1000Lines
            && file.RelativePath != "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
        var minified = files.Count(file => file.MinifiedSourceCandidate);
        var logicalMaxLineLength = files.Count == 0 ? 0 : files.Max(file => file.LogicalMaxLineLength);
        var rawPhysicalMaxLineLength = files.Count == 0 ? 0 : files.Max(file => file.RawPhysicalMaxLineLength);
        var parent = files.FirstOrDefault(file => file.RelativePath ==
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
            + "CampaignAuthoringReviewWorkspacePageControl.cs");
        var alphaPath = Resolve(projectRoot, "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
        var alpha = files.FirstOrDefault(file =>
            file.RelativePath == "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
        var alphaHash = File.Exists(alphaPath)
            ? EditDrivenSpineQualityConsolidationHash.Sha256(File.ReadAllBytes(alphaPath))
            : string.Empty;
        var alphaUnchanged = string.IsNullOrWhiteSpace(expectedAlphaRuntimeBootstrapHash)
            || string.Equals(alphaHash, expectedAlphaRuntimeBootstrapHash, StringComparison.OrdinalIgnoreCase);
        var parentWithinLimit = parent is not null && parent.LineCount <= ParentWorkspaceLineLimit;

        if (linesOver500 > 0)
        {
            diagnostics.Add(Error(
                "goal079.source.line_over_500",
                "sourceHealth.linesOver500Count",
                "Scanned C# files must not contain lines over 500 characters."));
        }

        if (zeroLfWithCr > 0)
        {
            diagnostics.Add(Error(
                "goal079.source.zero_lf_with_cr",
                "sourceHealth.zeroLfSourceFileCount",
                "Scanned C# files must not contain CR-only/no-LF source bytes."));
        }

        if (crOnlyLineEndings > 0)
        {
            diagnostics.Add(Error(
                "goal079.source.cr_only_line_endings",
                "sourceHealth.crOnlySourceFileCount",
                "Scanned C# files must not contain CR-only line endings."));
        }

        if (rawPhysicalLinesOver500 > 0)
        {
            diagnostics.Add(Error(
                "goal079.source.raw_physical_line_over_500",
                "sourceHealth.rawPhysicalMaxLineLength",
                "Scanned C# files must not contain raw LF-physical lines over 500 bytes."));
        }

        if (rawPhysicalOneLine > 0)
        {
            diagnostics.Add(Error(
                "goal079.source.raw_physical_one_line_source",
                "sourceHealth.rawPhysicalOneLineSourceFileCount",
                "Large C# files must not collapse to one physical line when split only by LF."));
        }

        if (filesOver1000 > 0)
        {
            diagnostics.Add(Error(
                "goal079.source.file_over_1000_lines",
                "sourceHealth.filesOver1000LinesCount",
                "Scanned non-Unity C# files must stay below 1000 lines."));
        }

        if (minified > 0)
        {
            diagnostics.Add(Error(
                "goal079.source.minified_source",
                "sourceHealth.minifiedSourceFileCount",
                "Goal 079 must not leave minified or one-line source candidates."));
        }

        if (!parentWithinLimit)
        {
            diagnostics.Add(Error(
                "goal079.source.parent_workspace_bloated",
                "CampaignAuthoringReviewWorkspacePageControl.cs",
                "Parent workspace must remain under the Goal 079 line limit."));
        }

        if (!alphaUnchanged)
        {
            diagnostics.Add(Error(
                "goal079.source.alpha_runtime_bootstrap_changed",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs",
                "Goal 079 records AlphaRuntimeBootstrap.cs as debt and must not edit it."));
        }

        return new EditDrivenSpineQualityConsolidationSourceHealthScan
        {
            Passed = diagnostics.Count == 0,
            ScannedFileCount = files.Count,
            MaxLineLength = logicalMaxLineLength,
            LogicalMaxLineLength = logicalMaxLineLength,
            LinesOver500Count = linesOver500,
            ZeroLfSourceFileCount = zeroLfWithCr,
            CrOnlySourceFileCount = crOnlyLineEndings,
            RawPhysicalMaxLineLength = rawPhysicalMaxLineLength,
            RawPhysicalOneLineSourceFileCount = rawPhysicalOneLine,
            RawPhysicalLinesOver500Count = rawPhysicalLinesOver500,
            FilesOver1000LinesCount = filesOver1000,
            MinifiedSourceFileCount = minified,
            ParentWorkspaceLineCount = parent?.LineCount ?? 0,
            ParentWorkspaceWithinLimit = parentWithinLimit,
            AlphaRuntimeBootstrapLineCount = alpha?.LineCount ?? 0,
            AlphaRuntimeBootstrapHash = alphaHash,
            AlphaRuntimeBootstrapUnchanged = alphaUnchanged,
            Files = files,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public EditDrivenSpineQualityConsolidationArtifactHygieneScan ScanArtifactHygiene(
        IReadOnlyDictionary<string, string> evidencePayloads)
    {
        var diagnostics = new List<EditDrivenSpineQualityConsolidationDiagnostic>();
        var containsAbsolutePath = false;
        var containsTimestamp = false;
        var containsHeavyLogs = false;
        var containsScratchTamper = false;

        foreach (var pair in evidencePayloads.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            if (pair.Value.Contains(@"C:\", StringComparison.OrdinalIgnoreCase)
                || pair.Value.Contains("/Users/", StringComparison.OrdinalIgnoreCase)
                || pair.Value.Contains(@"D:\", StringComparison.OrdinalIgnoreCase))
            {
                containsAbsolutePath = true;
                diagnostics.Add(Error(
                    "goal079.hygiene.absolute_local_path",
                    pair.Key,
                    "Goal 079 evidence must not contain absolute local paths."));
            }

            if (TimestampLikePattern.IsMatch(pair.Value))
            {
                containsTimestamp = true;
                diagnostics.Add(Error(
                    "goal079.hygiene.timestamp_like_value",
                    pair.Key,
                    "Goal 079 evidence must not contain volatile timestamp-like values."));
            }

            if (pair.Key.Contains("/logs/", StringComparison.OrdinalIgnoreCase)
                || pair.Key.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            {
                containsHeavyLogs = true;
                diagnostics.Add(Error(
                    "goal079.hygiene.heavy_log",
                    pair.Key,
                    "Goal 079 evidence must stay compact and must not contain heavy logs."));
            }

            if (pair.Key.Contains("scratch", StringComparison.OrdinalIgnoreCase)
                || pair.Key.Contains("tamper-copy", StringComparison.OrdinalIgnoreCase))
            {
                containsScratchTamper = true;
                diagnostics.Add(Error(
                    "goal079.hygiene.scratch_tamper_file",
                    pair.Key,
                    "Temporary tamper inputs must not be left in Goal 079 tracked evidence."));
            }
        }

        return new EditDrivenSpineQualityConsolidationArtifactHygieneScan
        {
            Passed = diagnostics.Count == 0,
            ArtifactCount = evidencePayloads.Count,
            ContainsAbsoluteLocalPaths = containsAbsolutePath,
            ContainsTimestampLikeValues = containsTimestamp,
            ContainsHeavyLogs = containsHeavyLogs,
            ContainsScratchTamperFiles = containsScratchTamper,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public EditDrivenSpineQualityConsolidationDebtClassification ClassifyDebt(
        EditDrivenSpineQualityConsolidationSourceArtifactManifest source,
        EditDrivenSpineQualityConsolidationWorkspaceBindingInventory binding,
        EditDrivenSpineQualityConsolidationNegativeProofIndex negative,
        EditDrivenSpineQualityConsolidationSourceHealthScan sourceHealth,
        EditDrivenSpineQualityConsolidationArtifactHygieneScan hygiene)
    {
        var debts = new List<EditDrivenSpineQualityConsolidationDebtItem>();
        AddBlockingDebts(debts, source, binding, negative, sourceHealth, hygiene);
        AddLongServiceDebts(debts, sourceHealth);
        debts.Add(new EditDrivenSpineQualityConsolidationDebtItem
        {
            FindingId = "GQ-P2-REPEATED-EDIT-DRIVEN-HELPERS",
            Severity = "P2",
            Area = "seam-patterns",
            Evidence = "Goal 074-079 seams intentionally duplicate narrow hash/read/quality helpers.",
            Disposition = "Classified for a future extraction goal; not repaired in Goal 079."
        });
        debts.Add(new EditDrivenSpineQualityConsolidationDebtItem
        {
            FindingId = "GQ-P3-ADAPTIVE-DOCS-CONTEXT-INDEXING",
            Severity = "P3",
            Area = "docs-context",
            Evidence = "Commit c8343e8 docs adaptive quality remains tracked as P3 context debt.",
            Disposition = "Preserved as non-blocking docs-context debt."
        });
        debts.Add(new EditDrivenSpineQualityConsolidationDebtItem
        {
            FindingId = "GQ-P3-DASHBOARD-DENSITY",
            Severity = "P3",
            Area = "winforms-dashboard",
            Evidence = "Goal 079 dashboard is functional and compact, with limited presentation polish.",
            Disposition = "Cosmetic dashboard polish is deferred."
        });

        return new EditDrivenSpineQualityConsolidationDebtClassification
        {
            P0Count = debts.Count(item => item.Severity == "P0"),
            P1Count = debts.Count(item => item.Severity == "P1"),
            P2Count = debts.Count(item => item.Severity == "P2"),
            P3Count = debts.Count(item => item.Severity == "P3"),
            Debts = debts.OrderBy(item => item.Severity, StringComparer.Ordinal)
                .ThenBy(item => item.FindingId, StringComparer.Ordinal)
                .ToList()
        };
    }

    public EditDrivenSpineQualityConsolidationQualityGateScan BuildQualityGateScan(
        EditDrivenSpineQualityConsolidationSourceArtifactManifest source,
        EditDrivenSpineQualityConsolidationWorkspaceBindingInventory binding,
        EditDrivenSpineQualityConsolidationNegativeProofIndex negative,
        EditDrivenSpineQualityConsolidationSourceHealthScan sourceHealth,
        EditDrivenSpineQualityConsolidationArtifactHygieneScan hygiene,
        EditDrivenSpineQualityConsolidationDebtClassification debt)
    {
        var diagnostics = SortDiagnostics(
            source.Diagnostics
                .Concat(binding.Diagnostics)
                .Concat(negative.Diagnostics)
                .Concat(sourceHealth.Diagnostics)
                .Concat(hygiene.Diagnostics));
        var requiredArtifactsPresent = source.SourceArtifacts.All(item => item.Exists);
        var passed = requiredArtifactsPresent
            && source.Goal078AcceptedByUserHandoff
            && source.Goal078ArtifactGreenAcceptedFalse
            && source.Goal072PreservedAsHistoricalBlocked
            && binding.Passed
            && negative.Passed
            && sourceHealth.Passed
            && hygiene.Passed
            && debt.P0Count == 0
            && debt.P1Count == 0
            && diagnostics.All(item => item.Severity != "error");

        return new EditDrivenSpineQualityConsolidationQualityGateScan
        {
            Passed = passed,
            RequiredArtifactsPresent = requiredArtifactsPresent,
            Goal078HandoffRecordedBeforeGoal079 = source.Goal078AcceptedByUserHandoff,
            WorkspaceBindingPassed = binding.Passed,
            NegativeProofPassed = negative.Passed,
            SourceHealthPassed = sourceHealth.Passed,
            ArtifactHygienePassed = hygiene.Passed,
            P0Count = debt.P0Count,
            P1Count = debt.P1Count,
            P2Count = debt.P2Count,
            P3Count = debt.P3Count,
            ZeroLfSourceFileCount = sourceHealth.ZeroLfSourceFileCount,
            CrOnlySourceFileCount = sourceHealth.CrOnlySourceFileCount,
            RawPhysicalMaxLineLength = sourceHealth.RawPhysicalMaxLineLength,
            RawPhysicalOneLineSourceFileCount = sourceHealth.RawPhysicalOneLineSourceFileCount,
            LogicalMaxLineLength = sourceHealth.LogicalMaxLineLength,
            MinifiedSourceFileCount = sourceHealth.MinifiedSourceFileCount,
            FilesOver1000LinesCount = sourceHealth.FilesOver1000LinesCount,
            Diagnostics = diagnostics
        };
    }

    public static IReadOnlyList<EditDrivenSpineQualityConsolidationDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenSpineQualityConsolidationDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static void AddBlockingDebts(
        ICollection<EditDrivenSpineQualityConsolidationDebtItem> debts,
        EditDrivenSpineQualityConsolidationSourceArtifactManifest source,
        EditDrivenSpineQualityConsolidationWorkspaceBindingInventory binding,
        EditDrivenSpineQualityConsolidationNegativeProofIndex negative,
        EditDrivenSpineQualityConsolidationSourceHealthScan sourceHealth,
        EditDrivenSpineQualityConsolidationArtifactHygieneScan hygiene)
    {
        foreach (var diagnostic in source.Diagnostics
                     .Concat(binding.Diagnostics)
                     .Concat(negative.Diagnostics)
                     .Concat(sourceHealth.Diagnostics)
                     .Where(item => item.Severity == "error"))
        {
            debts.Add(new EditDrivenSpineQualityConsolidationDebtItem
            {
                FindingId = "GQ-P0-" + diagnostic.Code.ToUpperInvariant().Replace('.', '-'),
                Severity = "P0",
                Area = diagnostic.Target,
                Evidence = diagnostic.Message,
                Disposition = "Blocks GREEN until repaired inside Goal 079 scope."
            });
        }

        foreach (var diagnostic in hygiene.Diagnostics.Where(item => item.Severity == "error"))
        {
            debts.Add(new EditDrivenSpineQualityConsolidationDebtItem
            {
                FindingId = "GQ-P1-" + diagnostic.Code.ToUpperInvariant().Replace('.', '-'),
                Severity = "P1",
                Area = diagnostic.Target,
                Evidence = diagnostic.Message,
                Disposition = "Blocks GREEN because tracked evidence hygiene is unsafe."
            });
        }
    }

    private static void AddLongServiceDebts(
        ICollection<EditDrivenSpineQualityConsolidationDebtItem> debts,
        EditDrivenSpineQualityConsolidationSourceHealthScan sourceHealth)
    {
        foreach (var file in sourceHealth.Files
                     .Where(item => item.LineCount >= 500 && !item.FileOver1000Lines)
                     .OrderByDescending(item => item.LineCount)
                     .Take(6))
        {
            debts.Add(new EditDrivenSpineQualityConsolidationDebtItem
            {
                FindingId = "GQ-P2-LONG-BELOW-LIMIT-" + file.LineCount,
                Severity = "P2",
                Area = "source-size",
                Evidence = file.RelativePath + " lines=" + file.LineCount,
                Disposition = "Below Goal 079 hard limit; classify for future extraction."
            });
        }

        var alpha = sourceHealth.Files.FirstOrDefault(item =>
            item.RelativePath == "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
        if (alpha is not null)
        {
            debts.Add(new EditDrivenSpineQualityConsolidationDebtItem
            {
                FindingId = "GQ-P2-ALPHA-RUNTIME-BOOTSTRAP-LARGE-READ-ONLY",
                Severity = "P2",
                Area = "unity-alpha-bootstrap",
                Evidence = alpha.RelativePath + " lines=" + alpha.LineCount
                    + " hash=" + sourceHealth.AlphaRuntimeBootstrapHash,
                Disposition = "Recorded as read-only debt; forbidden to repair in Goal 079."
            });
        }
    }

    private static IReadOnlyList<SurfaceSpec> BuildSurfaceSpecs() =>
    [
        new(
            "goal075_edit_loop",
            "CampaignEditValidateApplyLoopControl",
            "CampaignEditValidateApplyLoopControl.cs",
            "_editLoopTabPage",
            "_editLoopControl",
            "SchemaDrivenCampaignEditEvidenceService",
            "_editService.Build(root)",
            "_editLoopControl.Bind(editResult)"),
        new(
            "goal076_playable_refresh",
            "CampaignPlayableRefreshControl",
            "CampaignPlayableRefreshControl.cs",
            "_playableRefreshTabPage",
            "_playableRefreshControl",
            "EditDrivenPlayablePreviewRefreshEvidenceService",
            "_playableRefreshService.Build(root)",
            "_playableRefreshControl.Bind(refreshResult)"),
        new(
            "goal077_review_package",
            "CampaignReviewPackageControl",
            "CampaignReviewPackageControl.cs",
            "_reviewPackageTabPage",
            "_reviewPackageControl",
            "EditDrivenPlayableReviewPackageMaterializationEvidenceService",
            "_reviewPackageService.Build(root)",
            "_reviewPackageControl.Bind(reviewPackageResult)"),
        new(
            "goal078_play_session",
            "CampaignReviewPackagePlaySessionControl",
            "CampaignReviewPackagePlaySessionControl.cs",
            "_playSessionTabPage",
            "_playSessionControl",
            "EditDrivenReviewPackagePlayableSessionEvidenceService",
            "_playSessionService.Build(root)",
            "_playSessionControl.Bind(playSessionResult)"),
        new(
            "goal079_spine_quality_dashboard",
            "CampaignEditDrivenSpineQualityControl",
            "CampaignEditDrivenSpineQualityControl.cs",
            "_spineQualityTabPage",
            "_spineQualityControl",
            "EditDrivenSpineQualityConsolidationEvidenceService",
            "_spineQualityService.Build(root)",
            "_spineQualityControl.Bind(spineQualityResult)")
    ];

    private static EditDrivenSpineQualityConsolidationWorkspaceSurface BuildSurface(
        string projectRoot,
        string pageDesigner,
        string pageCode,
        string compactPageCode,
        SurfaceSpec spec)
    {
        var relativePath = "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/" + spec.FileName;
        var controlText = ReadOptional(projectRoot, relativePath);
        return new EditDrivenSpineQualityConsolidationWorkspaceSurface
        {
            SurfaceId = spec.SurfaceId,
            ControlName = spec.ControlName,
            RelativePath = relativePath,
            TabDeclared = pageDesigner.Contains(spec.TabField, StringComparison.Ordinal)
                && pageDesigner.Contains(spec.ControlField, StringComparison.Ordinal)
                && pageDesigner.Contains(spec.ControlName, StringComparison.Ordinal),
            ServiceBuiltDuringActivation = pageCode.Contains(spec.ServiceName, StringComparison.Ordinal)
                && compactPageCode.Contains(spec.BuildCall, StringComparison.Ordinal),
            BoundByParent = compactPageCode.Contains(spec.BindCall, StringComparison.Ordinal),
            SeparateUserControl = controlText.Contains(": UserControl", StringComparison.Ordinal)
                || controlText.Contains(":UserControl", StringComparison.Ordinal)
        };
    }

    private static IEnumerable<string> EnumerateFiles(string projectRoot)
    {
        foreach (var directory in ScanDirectories)
        {
            var full = Resolve(projectRoot, directory);
            if (!Directory.Exists(full))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(full, "*.cs", SearchOption.AllDirectories))
            {
                yield return file;
            }
        }

        foreach (var file in ScanFiles)
        {
            var full = Resolve(projectRoot, file);
            if (File.Exists(full))
            {
                yield return full;
            }
        }

        var productSmokeRoot = Resolve(projectRoot, "tests/LLMGameCreator.Tests/ProductSmoke");
        if (Directory.Exists(productSmokeRoot))
        {
            foreach (var file in Directory.EnumerateFiles(
                         productSmokeRoot,
                         "*EditDriven*.cs",
                         SearchOption.TopDirectoryOnly))
            {
                yield return file;
            }
        }
    }

    private static EditDrivenSpineQualityConsolidationSourceFileScan ScanFile(string projectRoot, string path)
    {
        var bytes = File.ReadAllBytes(path);
        var raw = AnalyzeRawPhysicalLines(bytes);
        var text = Encoding.UTF8.GetString(bytes);
        var lines = Regex.Split(text, "\r\n|\n|\r");
        var lengths = lines.Select(line => line.Length).ToList();
        var maxLineLength = lengths.Count == 0 ? 0 : lengths.Max();
        var lineCount = lines.Length;
        var rawPhysicalOneLine = bytes.Length >= LargeSourceFileByteThreshold && raw.RawPhysicalLineCount <= 3;
        return new EditDrivenSpineQualityConsolidationSourceFileScan
        {
            RelativePath = Relative(projectRoot, path),
            LineCount = lineCount,
            ByteCount = bytes.Length,
            MaxLineLength = maxLineLength,
            LogicalLineCount = lineCount,
            LogicalMaxLineLength = maxLineLength,
            LfByteCount = raw.LfByteCount,
            CrByteCount = raw.CrByteCount,
            RawPhysicalLineCount = raw.RawPhysicalLineCount,
            RawPhysicalMaxLineLength = raw.RawPhysicalMaxLineLength,
            LinesOver500Count = lengths.Count(length => length > 500),
            RawPhysicalLinesOver500Count = raw.RawPhysicalLinesOver500Count,
            ZeroLfWithCr = raw.LfByteCount == 0 && raw.CrByteCount > 0,
            ContainsCrOnlyLineEndings = raw.ContainsCrOnlyLineEndings,
            RawPhysicalOneLineSourceCandidate = rawPhysicalOneLine,
            FileOver1000Lines = lineCount > 1000,
            MinifiedSourceCandidate = lineCount <= 1
                || maxLineLength > 500
                || raw.RawPhysicalMaxLineLength > 500
                || rawPhysicalOneLine
        };
    }

    private static RawSourceLineMetrics AnalyzeRawPhysicalLines(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return new RawSourceLineMetrics(0, 0, 0, 0, 0, false);
        }

        var lfCount = 0;
        var crCount = 0;
        var currentLength = 0;
        var maxLength = 0;
        var over500Count = 0;
        var containsCrOnlyLineEndings = false;

        for (var index = 0; index < bytes.Length; index++)
        {
            var value = bytes[index];
            if (value == '\n')
            {
                lfCount++;
                if (currentLength > maxLength)
                {
                    maxLength = currentLength;
                }

                if (currentLength > 500)
                {
                    over500Count++;
                }

                currentLength = 0;
                continue;
            }

            currentLength++;
            if (value != '\r')
            {
                continue;
            }

            crCount++;
            if (index + 1 >= bytes.Length || bytes[index + 1] != '\n')
            {
                containsCrOnlyLineEndings = true;
            }
        }

        if (currentLength > maxLength)
        {
            maxLength = currentLength;
        }

        if (currentLength > 500)
        {
            over500Count++;
        }

        return new RawSourceLineMetrics(
            lfCount,
            crCount,
            lfCount + 1,
            maxLength,
            over500Count,
            containsCrOnlyLineEndings);
    }

    private static string ReadOptional(string projectRoot, string relativePath)
    {
        var path = Resolve(projectRoot, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Resolve(string projectRoot, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, path);
        return path;
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string Compact(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (!char.IsWhiteSpace(ch))
            {
                builder.Append(ch);
            }
        }

        return builder.ToString();
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

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            _ => 2
        };

    private static EditDrivenSpineQualityConsolidationDiagnostic Error(
        string code,
        string target,
        string message) =>
        EditDrivenSpineQualityConsolidationDiagnostic.Error(code, target, message);

    private sealed record SurfaceSpec(
        string SurfaceId,
        string ControlName,
        string FileName,
        string TabField,
        string ControlField,
        string ServiceName,
        string BuildCall,
        string BindCall);

    private sealed record RawSourceLineMetrics(
        int LfByteCount,
        int CrByteCount,
        int RawPhysicalLineCount,
        int RawPhysicalMaxLineLength,
        int RawPhysicalLinesOver500Count,
        bool ContainsCrOnlyLineEndings);
}
