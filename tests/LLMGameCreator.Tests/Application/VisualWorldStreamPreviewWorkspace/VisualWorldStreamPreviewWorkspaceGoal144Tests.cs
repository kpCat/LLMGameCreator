using Xunit;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal144Tests
{
    [Fact]
    public void RealWorkspaceKeepsHistoricalProofsGreen()
    {
        var result = new VisualWorldStreamPreviewWorkspaceService().Build(ProjectRoot());
        var failed = result.ProofStatus.Proofs.Where(proof => !proof.Passed).Select(proof =>
            proof.ProofId + ":" + proof.DiagnosticSummary).ToList();
        Assert.True(failed.Count == 0, string.Join(Environment.NewLine, failed));
    }

    [Fact]
    public void WinFormsGoal144SurfaceIsInteractiveInProcessOnly()
    {
        var source = File.ReadAllText(Path.Combine(
            ProjectRoot(),
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal144.cs"));
        Assert.Contains("Goal144 Live Session", source, StringComparison.Ordinal);
        Assert.Contains("Execute Selected Action", source, StringComparison.Ordinal);
        Assert.Contains("Save Checkpoint", source, StringComparison.Ordinal);
        Assert.Contains("Reload Checkpoint", source, StringComparison.Ordinal);
        Assert.Contains("Replay Verify", source, StringComparison.Ordinal);
        Assert.Contains("Run Selected Variant Session Drill", source, StringComparison.Ordinal);
        Assert.Contains("SelectedRuntimeVariantInteractiveSessionOperatorRunner", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProcessStartInfo", source, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet test", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("powershell", source, StringComparison.OrdinalIgnoreCase);
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
