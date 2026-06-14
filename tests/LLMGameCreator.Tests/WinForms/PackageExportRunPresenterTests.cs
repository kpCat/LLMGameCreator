using System.Text.Json;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.WinForms.Pages.PackageExport;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class PackageExportRunPresenterTests
{
    [Fact]
    public void PresenterBuildsViewStateFromSuccessfulRun()
    {
        var result = new GeneratorPlanPackageExportRunResult
        {
            Ok = true,
            Status = GeneratorPlanPackageExportRunStatus.Succeeded,
            SourceExamplePath = @"C:\examples\first.example.json",
            ExportFolderPath = @"C:\exports\first",
            PackageJsonPath = @"C:\exports\first\package.json",
            ApprovalArtifacts = new GeneratorPlanDraftArtifactApprovalArtifactResult
            {
                ApprovalResult = new GeneratorPlanDraftArtifactApprovalResult { Status = GeneratorPlanDraftArtifactStagingStatus.ReadyForPackage }
            },
            AssemblyResult = new GeneratorPlanGamePackageAssemblyResult { Status = GeneratorPlanGamePackageAssemblyStatus.ValidPackage },
            MarkdownReport = "# report"
        };

        var viewState = new PackageExportRunPresenter().FromRunResult(result);

        Assert.True(viewState.Exists);
        Assert.Equal(GeneratorPlanPackageExportRunStatus.Succeeded, viewState.Status);
        Assert.Equal(@"C:\exports\first\package.json", viewState.PackageJsonPath);
        Assert.Equal(GeneratorPlanDraftArtifactStagingStatus.ReadyForPackage, viewState.ApprovalStatus);
        Assert.Equal(GeneratorPlanGamePackageAssemblyStatus.ValidPackage, viewState.AssemblyStatus);
        Assert.Equal(0, viewState.ErrorCount);
        Assert.Equal(0, viewState.WarningCount);
        Assert.True(viewState.CanCopyPackagePath);
        Assert.True(viewState.CanCopyMarkdownReport);
    }

    [Fact]
    public void PresenterBuildsViewStateFromFailedRun()
    {
        var result = new GeneratorPlanPackageExportRunResult
        {
            Status = GeneratorPlanPackageExportRunStatus.Failed,
            Diagnostics =
            [
                new GeneratorPlanPackageExportRunDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Error,
                    Code = GeneratorPlanPackageExportRunDiagnosticCodes.MissingSourceExamplePath,
                    Target = "SourceExamplePath",
                    Message = "Source example path is required."
                },
                new GeneratorPlanPackageExportRunDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "warning",
                    Target = "target",
                    Message = "Warning."
                }
            ]
        };

        var viewState = new PackageExportRunPresenter().FromRunResult(result);

        Assert.Equal(GeneratorPlanPackageExportRunStatus.Failed, viewState.Status);
        Assert.Equal(1, viewState.ErrorCount);
        Assert.Equal(1, viewState.WarningCount);
        Assert.Contains(viewState.Diagnostics, row => row.Code == GeneratorPlanPackageExportRunDiagnosticCodes.MissingSourceExamplePath);
    }

    [Fact]
    public void PresenterMapsDiagnosticsRows()
    {
        var result = new GeneratorPlanPackageExportRunResult
        {
            Diagnostics =
            [
                new GeneratorPlanPackageExportRunDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "z.warning",
                    Target = "b",
                    Message = "Second."
                },
                new GeneratorPlanPackageExportRunDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Error,
                    Code = "a.error",
                    Target = "a",
                    Message = "First."
                }
            ]
        };

        var viewState = new PackageExportRunPresenter().FromRunResult(result);

        Assert.Collection(
            viewState.Diagnostics,
            row =>
            {
                Assert.Equal(GeneratorPlanPreviewDiagnosticSeverity.Error, row.Severity);
                Assert.Equal("a.error", row.Code);
                Assert.Equal("a", row.Target);
                Assert.Equal("First.", row.Message);
            },
            row =>
            {
                Assert.Equal(GeneratorPlanPreviewDiagnosticSeverity.Warning, row.Severity);
                Assert.Equal("z.warning", row.Code);
            });
    }

    [Fact]
    public void PresenterBuildsLatestRunViewStateWhenMissing()
    {
        var viewState = new PackageExportRunPresenter().FromLatestRun(new GeneratorPlanPackageExportRunArtifactReadResult());

        Assert.False(viewState.Exists);
        Assert.Equal("not_found", viewState.Status);
        Assert.Contains("No package export run found.", viewState.Summary);
    }

    [Fact]
    public void PresenterBuildsLatestRunViewStateFromSavedArtifact()
    {
        var runArtifact = new GeneratedArtifactRecord(
            GeneratorPlanPackageExportRunArtifactIds.RunArtifactId,
            GeneratorPlanPackageExportRunArtifactIds.RunArtifactKind,
            GeneratorPlanPackageExportRunArtifactIds.RunArtifactPath,
            JsonSerializer.Serialize(new
            {
                Status = GeneratorPlanPackageExportRunStatus.SucceededWithWarnings,
                SourceExamplePath = @"C:\examples\latest.example.json",
                ExportFolderPath = @"C:\exports\latest",
                PackageJsonPath = @"C:\exports\latest\package.json",
                ApprovalStatus = GeneratorPlanDraftArtifactStagingStatus.ReadyForPackage,
                AssemblyStatus = GeneratorPlanGamePackageAssemblyStatus.ValidPackage,
                Diagnostics = new[]
                {
                    new
                    {
                        Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                        Code = "generator_plan_package_export_run.assembly_diagnostic",
                        Target = "artifact/semantic",
                        Message = "Unmapped semantic artifact."
                    }
                }
            }),
            GeneratorPlanPackageExportRunArtifactIds.GeneratedBy,
            GeneratorPlanGamePackageAssemblyValidationState.Warnings,
            "{}");
        var markdownArtifact = new GeneratedArtifactRecord(
            GeneratorPlanPackageExportRunArtifactIds.MarkdownArtifactId,
            GeneratorPlanPackageExportRunArtifactIds.MarkdownArtifactKind,
            GeneratorPlanPackageExportRunArtifactIds.MarkdownArtifactPath,
            JsonSerializer.Serialize(new { Markdown = "# latest report" }),
            GeneratorPlanPackageExportRunArtifactIds.GeneratedBy,
            GeneratorPlanGamePackageAssemblyValidationState.Warnings,
            "{}");

        var viewState = new PackageExportRunPresenter().FromLatestRun(new GeneratorPlanPackageExportRunArtifactReadResult
        {
            Exists = true,
            RunArtifact = runArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults =
            [
                new GeneratedArtifactValidationResultRecord(
                    "validation/1",
                    runArtifact.Id,
                    GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    "validation.warning",
                    "Saved warning.",
                    "artifact/semantic",
                    "{}")
            ]
        });

        Assert.True(viewState.Exists);
        Assert.Equal(GeneratorPlanPackageExportRunStatus.SucceededWithWarnings, viewState.Status);
        Assert.Equal(@"C:\examples\latest.example.json", viewState.SourceExamplePath);
        Assert.Equal(@"C:\exports\latest", viewState.ExportFolderPath);
        Assert.Equal(@"C:\exports\latest\package.json", viewState.PackageJsonPath);
        Assert.Equal(GeneratorPlanDraftArtifactStagingStatus.ReadyForPackage, viewState.ApprovalStatus);
        Assert.Equal(GeneratorPlanGamePackageAssemblyStatus.ValidPackage, viewState.AssemblyStatus);
        Assert.Equal(1, viewState.WarningCount);
        Assert.Equal(1, viewState.ValidationResultCount);
        Assert.True(viewState.MarkdownExists);
        Assert.Equal("# latest report", viewState.MarkdownReport);
        Assert.Single(viewState.Diagnostics);
    }
}
