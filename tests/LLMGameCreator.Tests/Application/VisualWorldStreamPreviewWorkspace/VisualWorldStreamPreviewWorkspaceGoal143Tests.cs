using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal143Tests
{
    [Fact]
    public void Goal143SurfaceUsesInProcessOperatorAndShowsRequiredFields()
    {
        var root = ProjectRoot();
        var source = File.ReadAllText(Path.Combine(
            root,
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal143.cs"));

        Assert.Contains("SelectedRuntimeVariantPlayerAdapterOperatorRunner", source);
        Assert.Contains("Task.Run", source);
        Assert.Contains("Build Selected Variant PlayerAdapter", source);
        Assert.Contains("selectedCandidateId=", source);
        Assert.Contains("selectedVariantKind=", source);
        Assert.Contains("packageHashMatch=", source);
        Assert.Contains("finalStateHashMatch=", source);
        Assert.Contains("selectedVariantEffectVisible=", source);
        Assert.Contains("noBalancedBaselineFallback=", source);
        Assert.Contains("unitySmokePassed=", source);
        Assert.DoesNotContain("ProcessStartInfo", source, StringComparison.Ordinal);
        Assert.DoesNotContain("powershell", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dotnet test", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dotnet build", source, StringComparison.OrdinalIgnoreCase);
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
