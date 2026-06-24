using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Docs;

public sealed class CurrentGeneratorStateDocsTests
{
    [Fact]
    public void CurrentGeneratorStateFilesExist()
    {
        var root = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(root, "docs", "CURRENT_GENERATOR_STATE.md")));
        Assert.True(File.Exists(Path.Combine(root, "docs", "CURRENT_GENERATOR_STATE.json")));
    }

    [Fact]
    public void CurrentGeneratorStateJsonParses()
    {
        using var state = ReadCurrentStateJson();

        AssertRequiredString(state, "schema_version");
        AssertRequiredString(state, "state_id");
        AssertRequiredString(state, "updated_at_utc");
        AssertRequiredString(state, "current_phase");
        AssertRequiredString(state, "last_completed_milestone");
        AssertRequiredString(state, "last_completed_milestone_title");
        AssertRequiredString(state, "active_manual_gate");
        AssertRequiredString(state, "current_user_action");
        AssertRequiredString(state, "recommended_next_decision");
        AssertRequiredArray(state, "allowed_next_codex_task_types");
        AssertRequiredArray(state, "blocked_next_milestones_until_gate_passes");
        AssertRequiredArray(state, "required_reading_order_for_new_agents");
        AssertRequiredArray(state, "current_workflow");
        AssertRequiredString(state, "state_update_rule");
    }

    [Fact]
    public void CurrentGeneratorStateMarkdownReferencesJson()
    {
        var root = FindRepositoryRoot();
        var markdown = Read(root, "docs", "CURRENT_GENERATOR_STATE.md");

        Assert.Contains("docs/CURRENT_GENERATOR_STATE.json", markdown);
    }

    [Fact]
    public void ContextIndexLinksCurrentState()
    {
        var root = FindRepositoryRoot();
        var contextIndex = Read(root, "docs", "CONTEXT_INDEX.md");

        Assert.Contains("docs/CURRENT_GENERATOR_STATE.md", contextIndex);
        Assert.Contains("docs/CURRENT_GENERATOR_STATE.json", contextIndex);
    }

    [Fact]
    public void AgentsRequiresContextIndexAndCurrentState()
    {
        var root = FindRepositoryRoot();
        var agents = Read(root, "AGENTS.md");

        Assert.Contains("docs/CONTEXT_INDEX.md", agents);
        Assert.Contains("docs/CURRENT_GENERATOR_STATE.md", agents);
    }

    [Fact]
    public void ReadmeLinksCurrentState()
    {
        var root = FindRepositoryRoot();
        var readme = Read(root, "README.md");

        Assert.Contains("docs/CURRENT_GENERATOR_STATE.md", readme);
        Assert.Contains("Tiny Generated Runtime Loop", readme);
        Assert.Contains("Generated Package MVP", readme);
        Assert.Contains("Formula/Effect/Action Registry Foundation", readme);
        Assert.Contains("Visible Generated Playable Preview", readme);
        Assert.DoesNotContain("Next practical step:\r\nM4.1", readme, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RoadmapReferencesCurrentStateBeforeNextMilestoneChoice()
    {
        var root = FindRepositoryRoot();
        var roadmap = Read(root, "docs", "ROADMAP_TO_FULL_GENERATOR.md");

        Assert.Contains("docs/CURRENT_GENERATOR_STATE.md", roadmap);
        Assert.Contains("Before choosing the next milestone", roadmap);
    }

    [Fact]
    public void CurrentStateJsonMilestoneExistsInRoadmap()
    {
        var root = FindRepositoryRoot();
        using var state = ReadCurrentStateJson(root);
        var milestone = state.RootElement.GetProperty("last_completed_milestone").GetString();
        var roadmap = Read(root, "docs", "ROADMAP_TO_FULL_GENERATOR.md");

        Assert.False(string.IsNullOrWhiteSpace(milestone));
        Assert.Contains(milestone, roadmap);
    }

    [Fact]
    public void CurrentStateJsonKeepsLuaAndRuntimeExpansionLockedDuringStrategyReset()
    {
        using var state = ReadCurrentStateJson();
        var currentPhase = state.RootElement.GetProperty("current_phase").GetString();
        var recommendedNextWorkItem = state.RootElement.GetProperty("recommended_next_work_item").GetString();
        var blocked = state.RootElement.GetProperty("blocked_next_milestones_until_gate_passes")
            .EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty)
            .ToArray();

        Assert.Equal("strategy_reset_playable_procedural_generator", currentPhase);
        Assert.Equal("visible_generated_playable_preview", recommendedNextWorkItem);
        Assert.Contains(blocked, item => item.Contains("M5", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(blocked, item => item.Contains("Lua", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(blocked, item => item.Contains("M6", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(blocked, item => item.Contains("Runtime preview repair loop", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GeneratorPlanStrictEvaluationDocIsLinkedFromCurrentStateOrContextIndex()
    {
        var root = FindRepositoryRoot();
        var currentState = Read(root, "docs", "CURRENT_GENERATOR_STATE.md");
        var contextIndex = Read(root, "docs", "CONTEXT_INDEX.md");

        Assert.Contains(
            "docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md",
            currentState + Environment.NewLine + contextIndex);
    }

    private static JsonDocument ReadCurrentStateJson(string? root = null)
    {
        root ??= FindRepositoryRoot();
        var json = Read(root, "docs", "CURRENT_GENERATOR_STATE.json");

        return JsonDocument.Parse(json);
    }

    private static void AssertRequiredString(JsonDocument document, string propertyName)
    {
        Assert.True(document.RootElement.TryGetProperty(propertyName, out var property), $"Missing property: {propertyName}");
        Assert.Equal(JsonValueKind.String, property.ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(property.GetString()));
    }

    private static void AssertRequiredArray(JsonDocument document, string propertyName)
    {
        Assert.True(document.RootElement.TryGetProperty(propertyName, out var property), $"Missing property: {propertyName}");
        Assert.Equal(JsonValueKind.Array, property.ValueKind);
        Assert.NotEmpty(property.EnumerateArray());
    }

    private static string Read(string root, params string[] segments)
    {
        return File.ReadAllText(Path.Combine(new[] { root }.Concat(segments).ToArray()));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException("Repository root was not found.");
        }

        return directory.FullName;
    }
}
