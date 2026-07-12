using Xunit;

namespace LLMGameCreator.Tests.Application.ProjectStandaloneBuild;

public sealed class Goal152AStandaloneUxAndSelfCheckTests
{
    [Fact]
    public void Render_contract_has_one_clearing_camera_opaque_repaint_and_responsive_canvas()
    {
        var root = FindRoot();
        var entrypoint = File.ReadAllText(Path.Combine(root, "unity", "LLMGameCreatorAlpha", "Assets", "Editor", "ProjectStandaloneBuildEntrypoint.cs"));
        var bootstrap = File.ReadAllText(Path.Combine(root, "unity", "LLMGameCreatorAlpha", "Assets", "Scripts", "ProjectStandalonePlayerAdapterBootstrap.cs"));
        Assert.Contains("ProjectStandaloneBackgroundCamera", entrypoint, StringComparison.Ordinal);
        Assert.Contains("CameraClearFlags.SolidColor", entrypoint, StringComparison.Ordinal);
        Assert.Contains("camera.cullingMask = 0", entrypoint, StringComparison.Ordinal);
        Assert.Contains("EventType.Repaint", bootstrap, StringComparison.Ordinal);
        Assert.Contains("GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height)", bootstrap, StringComparison.Ordinal);
        Assert.Contains("GUI.color = originalColor", bootstrap, StringComparison.Ordinal);
        Assert.Contains("GUI.matrix = originalMatrix", bootstrap, StringComparison.Ordinal);
        Assert.Contains("ReferenceWidth = 1280f", bootstrap, StringComparison.Ordinal);
        Assert.Contains("Mathf.Min(Screen.width / ReferenceWidth", bootstrap, StringComparison.Ordinal);
        Assert.Contains("TextClipping.Clip", bootstrap, StringComparison.Ordinal);
    }

    [Fact]
    public void Self_check_contract_covers_integrity_navigation_authority_and_human_review()
    {
        var root = FindRoot();
        var bootstrap = File.ReadAllText(Path.Combine(root, "unity", "LLMGameCreatorAlpha", "Assets", "Scripts", "ProjectStandalonePlayerAdapterBootstrap.cs"));
        var service = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.Application", "Design", "ProjectStandaloneBuild", "ProjectStandaloneBuildService.cs"));
        foreach (var marker in new[] { "LLMGC_PROJECT_STANDALONE_LOAD_PASS", "LLMGC_PROJECT_STANDALONE_INTEGRITY_PASS", "LLMGC_PROJECT_STANDALONE_NAVIGATION_PASS", "LLMGC_PROJECT_STANDALONE_RUNTIME_AUTHORITY_PASS", "LLMGC_PROJECT_STANDALONE_SMOKE_PASS" }) Assert.Contains(marker, bootstrap, StringComparison.Ordinal);
        Assert.Contains("HashFile(Path.Combine(root, \"game-package.json\"))", bootstrap, StringComparison.Ordinal);
        Assert.Contains("CursorTransitionsAreDeterministic", bootstrap, StringComparison.Ordinal);
        Assert.Contains("configuredParameterCount == EffectiveParameterCount", bootstrap, StringComparison.Ordinal);
        Assert.Contains("humanReviewFacts", bootstrap, StringComparison.Ordinal);
        Assert.Contains("StandaloneSelfCheck.RequiredMarkers.All", service, StringComparison.Ordinal);
        Assert.Contains("-batchmode -nographics -llmgcStandaloneSmokeExit", service, StringComparison.Ordinal);
        Assert.Contains("ProcessStartInfo(result.ExecutablePath) { UseShellExecute = true }", service, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
