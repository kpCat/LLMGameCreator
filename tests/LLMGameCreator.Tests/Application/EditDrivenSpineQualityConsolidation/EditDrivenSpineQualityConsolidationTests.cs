using System.Runtime.ExceptionServices;
using System.Text;
using LLMGameCreator.Application.Design.EditDrivenSpineQualityConsolidation;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenSpineQualityConsolidation;

public sealed class EditDrivenSpineQualityConsolidationTests
{
    private const string Goal078PackageReadProofPath =
        ".llmgc/procedural/goal-078-edit-driven-review-package-playable-session/package-read-proof.json";
    private const string Goal078NegativeProofPath =
        ".llmgc/procedural/goal-078-edit-driven-review-package-playable-session/tamper-negative-proof.json";

    [Fact]
    public async Task ServiceBuildsGreenConsolidationFromCurrentGoal074To078Artifacts()
    {
        var service = new EditDrivenSpineQualityConsolidationEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.SourceArtifactManifest.Goal078AcceptedByUserHandoff);
        Assert.True(result.SourceArtifactManifest.Goal078ArtifactGreenAcceptedFalse);
        Assert.True(result.SourceArtifactManifest.Goal072PreservedAsHistoricalBlocked);
        Assert.True(result.AcceptanceReadinessDashboard.PackageReadProofPassed);
        Assert.True(result.AcceptanceReadinessDashboard.ReplayProofPassed);
        Assert.True(result.NegativeProofIndex.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);
        Assert.True(result.SourceHealthScan.Passed);
        Assert.Equal(0, result.SourceHealthScan.ZeroLfSourceFileCount);
        Assert.Equal(0, result.SourceHealthScan.CrOnlySourceFileCount);
        Assert.Equal(0, result.SourceHealthScan.RawPhysicalOneLineSourceFileCount);
        Assert.True(result.SourceHealthScan.RawPhysicalMaxLineLength <= 500);
        Assert.True(result.SourceHealthScan.LogicalMaxLineLength <= 500);
        Assert.True(result.ArtifactHygieneScan.Passed);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(0, result.QualityGateScan.ZeroLfSourceFileCount);
        Assert.Equal(0, result.QualityGateScan.CrOnlySourceFileCount);
        Assert.Equal(0, result.QualityGateScan.RawPhysicalOneLineSourceFileCount);
        Assert.Equal(5, result.SpineChainManifest.ChainItemCount);
        Assert.Equal(0, result.QualityDebtClassification.P0Count);
        Assert.Equal(0, result.QualityDebtClassification.P1Count);

        foreach (var fileName in EditDrivenSpineQualityConsolidationEvidenceService.RequiredArtifactNames())
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public void SourceHealthScanRejectsCrOnlyNoLfSourceBytes()
    {
        var root = Path.Combine(Path.GetTempPath(), "llmgc-goal079a-cr-only-" + Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(
                root,
                "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                    + "CampaignAuthoringReviewWorkspacePageControl.cs",
                "public sealed class CampaignAuthoringReviewWorkspacePageControl\n{\n}\n");
            WriteBytes(
                root,
                "src/LLMGameCreator.Application/Design/EditDrivenSpineQualityConsolidation/CrOnly.cs",
                Encoding.UTF8.GetBytes("public sealed class CrOnly\r{\r    public int Value => 1;\r}\r"));

            var scan = new EditDrivenSpineQualityConsolidationQualityGateScanner().ScanSourceHealth(
                root,
                expectedAlphaRuntimeBootstrapHash: "");

            Assert.False(scan.Passed);
            Assert.Equal(1, scan.ZeroLfSourceFileCount);
            Assert.Equal(1, scan.CrOnlySourceFileCount);
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal079.source.zero_lf_with_cr");
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal079.source.cr_only_line_endings");
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public void SourceHealthScanRejectsZeroLfOnePhysicalLineSourceBytes()
    {
        var root = Path.Combine(Path.GetTempPath(), "llmgc-goal079a-zero-lf-" + Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(
                root,
                "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                    + "CampaignAuthoringReviewWorkspacePageControl.cs",
                "public sealed class CampaignAuthoringReviewWorkspacePageControl\n{\n}\n");
            var compactSource = "public sealed class ZeroLf { public string Value => \""
                + new string('x', 1_600)
                + "\"; }";
            WriteBytes(
                root,
                "src/LLMGameCreator.Application/Design/EditDrivenSpineQualityConsolidation/ZeroLf.cs",
                Encoding.UTF8.GetBytes(compactSource));

            var scan = new EditDrivenSpineQualityConsolidationQualityGateScanner().ScanSourceHealth(
                root,
                expectedAlphaRuntimeBootstrapHash: "");

            Assert.False(scan.Passed);
            Assert.Equal(1, scan.RawPhysicalOneLineSourceFileCount);
            Assert.True(scan.RawPhysicalMaxLineLength > 500);
            Assert.True(scan.MinifiedSourceFileCount > 0);
            Assert.Contains(
                scan.Diagnostics,
                diagnostic => diagnostic.Code == "goal079.source.raw_physical_one_line_source");
            Assert.Contains(
                scan.Diagnostics,
                diagnostic => diagnostic.Code == "goal079.source.raw_physical_line_over_500");
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public void ServiceBlocksWhenRequiredGoal078ProofArtifactIsMissing()
    {
        var options = new EditDrivenSpineQualityConsolidationBuildOptions
        {
            ArtifactTextOverridesByRelativePath = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [Goal078PackageReadProofPath] = null
            }
        };

        var result = new EditDrivenSpineQualityConsolidationEvidenceService().Build(ProjectRoot(), options);

        Assert.Equal("BLOCKED", result.Report.ImplementationStatus);
        Assert.Contains(result.Report.Diagnostics, diagnostic =>
            diagnostic.Code == "goal079.source.required_artifact_missing"
            && diagnostic.Target == Goal078PackageReadProofPath);
    }

    [Fact]
    public void ServiceBlocksWhenGoal078NegativeProofIsReportOnlyOrFake()
    {
        var root = ProjectRoot();
        var proof = File.ReadAllText(Path.Combine(root, Goal078NegativeProofPath));
        var tampered = ReplaceFirst(proof, "\"actualStatus\": \"rejected\"", "\"actualStatus\": \"accepted\"");
        var options = new EditDrivenSpineQualityConsolidationBuildOptions
        {
            ArtifactTextOverridesByRelativePath = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [Goal078NegativeProofPath] = tampered
            }
        };

        var result = new EditDrivenSpineQualityConsolidationEvidenceService().Build(root, options);

        Assert.Equal("BLOCKED", result.Report.ImplementationStatus);
        Assert.False(result.NegativeProofIndex.Passed);
        Assert.Contains(result.NegativeProofIndex.Diagnostics, diagnostic =>
            diagnostic.Code == "goal079.negative.scenario_not_real_rejection");
    }

    [Fact]
    public void NegativeProofIndexIncludesRequiredGoal078RejectionFamilies()
    {
        var result = new EditDrivenSpineQualityConsolidationEvidenceService().Build(ProjectRoot());
        var scenarioIds = result.NegativeProofIndex.Scenarios.Select(item => item.ScenarioId).ToHashSet();

        Assert.True(result.NegativeProofIndex.Passed);
        Assert.Contains("missing_target_file", scenarioIds);
        Assert.Contains("tampered_target_payload", scenarioIds);
        Assert.Contains("replay_order_mismatch", scenarioIds);
        Assert.Contains("illegal_action_target", scenarioIds);
        Assert.Contains("fake_success_without_target_payload_read", scenarioIds);
        Assert.All(result.NegativeProofIndex.Scenarios, scenario =>
        {
            Assert.Equal("rejected", scenario.ActualStatus);
            Assert.True(scenario.DiagnosticCount > 0);
        });
    }

    [Fact]
    public void SourceHealthScanCatchesMinifiedOverlongAndGodFormInputs()
    {
        var root = Path.Combine(Path.GetTempPath(), "llmgc-goal079-source-" + Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(
                root,
                "src/LLMGameCreator.Application/Design/EditDrivenSpineQualityConsolidation/Minified.cs",
                "public sealed class Minified { public string Value => \"" + new string('x', 520) + "\"; }");
            WriteFile(
                root,
                "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                    + "CampaignAuthoringReviewWorkspacePageControl.cs",
                string.Join(Environment.NewLine, Enumerable.Range(0, 276).Select(index => "public void M" + index + "() { }")));

            var scan = new EditDrivenSpineQualityConsolidationQualityGateScanner().ScanSourceHealth(
                root,
                expectedAlphaRuntimeBootstrapHash: "");

            Assert.False(scan.Passed);
            Assert.True(scan.LinesOver500Count > 0);
            Assert.True(scan.MinifiedSourceFileCount > 0);
            Assert.False(scan.ParentWorkspaceWithinLimit);
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal079.source.line_over_500");
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal079.source.parent_workspace_bloated");
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public void WorkspaceActivationBindsGoal079DashboardThroughParentPath()
    {
        RunSta(() =>
        {
            using var page = new CampaignAuthoringReviewWorkspacePageControl();

            page.OnActivated();

            var control = RequiredPrivateField<CampaignEditDrivenSpineQualityControl>(page, "_spineQualityControl");
            var result = RequiredPrivateField<EditDrivenSpineQualityConsolidationBuildResult>(control, "_result");
            Assert.Equal("GREEN", result.Report.ImplementationStatus);
            Assert.True(result.WorkspaceBindingInventory.Passed);
            Assert.True(result.NegativeProofIndex.Passed);
            Assert.Equal(5, result.SpineChainManifest.ChainItemCount);
        });
    }

    [Fact]
    public void QualityGateScannerRejectsDashboardDesignerTabWithoutParentBind()
    {
        var root = Path.Combine(Path.GetTempPath(), "llmgc-goal079-binding-" + Guid.NewGuid().ToString("N"));
        try
        {
            WriteDesignerOnlyBindingFixture(root);

            var inventory = new EditDrivenSpineQualityConsolidationQualityGateScanner()
                .BuildWorkspaceBindingInventory(root);

            Assert.False(inventory.Passed);
            Assert.True(inventory.ParentPageDashboardTabDeclared);
            Assert.False(inventory.ParentPageDashboardEvidenceServiceLoaded);
            Assert.False(inventory.ParentPageDashboardControlBound);
            Assert.Contains(inventory.Diagnostics, diagnostic =>
                diagnostic.Code == "goal079.winforms.surface_service_missing"
                && diagnostic.Target == "goal079_spine_quality_dashboard");
            Assert.Contains(inventory.Diagnostics, diagnostic =>
                diagnostic.Code == "goal079.winforms.surface_bind_missing"
                && diagnostic.Target == "goal079_spine_quality_dashboard");
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    private static void WriteDesignerOnlyBindingFixture(string root)
    {
        foreach (var control in new[]
                 {
                     "CampaignEditValidateApplyLoopControl",
                     "CampaignPlayableRefreshControl",
                     "CampaignReviewPackageControl",
                     "CampaignReviewPackagePlaySessionControl",
                     "CampaignEditDrivenSpineQualityControl"
                 })
        {
            WriteFile(
                root,
                "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/" + control + ".cs",
                "public sealed class " + control + " : UserControl { }");
        }

        WriteFile(
            root,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignAuthoringReviewWorkspacePageControl.Designer.cs",
            """
            private object _editLoopTabPage;
            private object _playableRefreshTabPage;
            private object _reviewPackageTabPage;
            private object _playSessionTabPage;
            private object _spineQualityTabPage;
            private CampaignEditValidateApplyLoopControl _editLoopControl;
            private CampaignPlayableRefreshControl _playableRefreshControl;
            private CampaignReviewPackageControl _reviewPackageControl;
            private CampaignReviewPackagePlaySessionControl _playSessionControl;
            private CampaignEditDrivenSpineQualityControl _spineQualityControl;
            """);
        WriteFile(
            root,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignAuthoringReviewWorkspacePageControl.cs",
            """
            SchemaDrivenCampaignEditEvidenceService _editService;
            EditDrivenPlayablePreviewRefreshEvidenceService _playableRefreshService;
            EditDrivenPlayableReviewPackageMaterializationEvidenceService _reviewPackageService;
            EditDrivenReviewPackagePlayableSessionEvidenceService _playSessionService;
            public void OnActivated()
            {
                var editResult = _editService.Build(root);
                var refreshResult = _playableRefreshService.Build(root);
                var reviewPackageResult = _reviewPackageService.Build(root);
                var playSessionResult = _playSessionService.Build(root);
                _editLoopControl.Bind(editResult);
                _playableRefreshControl.Bind(refreshResult);
                _reviewPackageControl.Bind(reviewPackageResult);
                _playSessionControl.Bind(playSessionResult);
            }
            """);
    }

    private static void RunSta(Action action)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                caught = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (caught is not null)
        {
            ExceptionDispatchInfo.Capture(caught).Throw();
        }
    }

    private static string ReplaceFirst(string text, string oldValue, string newValue)
    {
        var index = text.IndexOf(oldValue, StringComparison.Ordinal);
        return index < 0
            ? text
            : text[..index] + newValue + text[(index + oldValue.Length)..];
    }

    private static T RequiredPrivateField<T>(object owner, string fieldName)
    {
        var field = owner.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(field);
        var value = field!.GetValue(owner);
        Assert.NotNull(value);
        return Assert.IsType<T>(value);
    }

    private static void WriteFile(string root, string relativePath, string contents)
    {
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
    }

    private static void WriteBytes(string root, string relativePath, byte[] contents)
    {
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, contents);
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
