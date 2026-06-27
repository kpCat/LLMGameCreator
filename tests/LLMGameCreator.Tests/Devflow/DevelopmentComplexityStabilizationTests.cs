using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class DevelopmentComplexityStabilizationTests
{
    [Fact]
    public void ArtifactScopePolicyJsonParsesAndContainsRequiredClasses()
    {
        var repoRoot = DevelopmentComplexityStabilizationArtifacts.FindRepoRoot();
        var policyPath = Path.Combine(repoRoot, ".devflow", "artifact-scope", "artifact-scope-policy.json");

        using var document = JsonDocument.Parse(File.ReadAllText(policyPath));
        var root = document.RootElement;
        var classes = root.GetProperty("artifactMutabilityClasses")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();

        Assert.Equal("artifact_scope_policy_v1", root.GetProperty("schemaVersion").GetString());
        Assert.Contains("source_code_docs", classes);
        Assert.Contains("state_handoff_docs", classes);
        Assert.Contains("current_goal_compact_review_artifacts", classes);
        Assert.Contains("historical_compact_artifacts", classes);
        Assert.Contains("heavy_generated_build_runtime_outputs", classes);
        Assert.Contains("task_pack_docs", classes);
        Assert.Contains(root.GetProperty("allowedCurrentGoalArtifactRoots").EnumerateArray(), item =>
            item.GetString() == DevelopmentComplexityStabilizationArtifacts.RelativeOutputDirectory + "/");
    }

    [Fact]
    public void ArtifactScopeGuardAcceptsDeclaredCurrentGoalPaths()
    {
        var repoRoot = DevelopmentComplexityStabilizationArtifacts.FindRepoRoot();

        var run = DevelopmentComplexityStabilizationArtifacts.RunScopeGuard(
            repoRoot,
            "docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md",
            ".llmgc/procedural/development-complexity-stabilization/development-complexity-stabilization-report.json",
            "tests/LLMGameCreator.Tests/Devflow/DevelopmentComplexityStabilizationTests.cs");

        Assert.Equal(0, run.ExitCode);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.True(document.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Equal(0, document.RootElement.GetProperty("violationCount").GetInt32());
    }

    [Fact]
    public void ArtifactScopeGuardRejectsLegacyAndForbiddenMutations()
    {
        var repoRoot = DevelopmentComplexityStabilizationArtifacts.FindRepoRoot();
        var cases = new (string Path, string ExpectedCategory)[]
        {
            (".llmgc/procedural/minimum-playable-generated-game/minimum-playable-generated-game-report.json", "disallowed_legacy_artifact_mutation"),
            (".llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.json", "disallowed_legacy_artifact_mutation"),
            ("LLMGameCreator.sln", "disallowed_project_file_mutation"),
            ("src/LLMGameCreator.Application/LLMGameCreator.Application.csproj", "disallowed_project_file_mutation"),
            ("docs/GAME_PACKAGE_FORMAT.md", "disallowed_public_gamepackage_schema_mutation"),
            ("generator-library/catalog.json", "disallowed_generator_library_mutation"),
            ("unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs", "disallowed_unity_entrypoint_mutation")
        };

        foreach (var testCase in cases)
        {
            var run = DevelopmentComplexityStabilizationArtifacts.RunScopeGuard(repoRoot, testCase.Path);

            Assert.NotEqual(0, run.ExitCode);
            using var document = JsonDocument.Parse(run.StandardOutput);
            Assert.False(document.RootElement.GetProperty("accepted").GetBoolean());
            Assert.Contains(document.RootElement.GetProperty("changedPaths").EnumerateArray(), item =>
                item.GetProperty("category").GetString() == testCase.ExpectedCategory);
        }
    }

    [Fact]
    public void ArtifactScopeGuardRejectsTrackedHeavyOutputWhenFailOnTrackedIgnoredIsSet()
    {
        var repoRoot = DevelopmentComplexityStabilizationArtifacts.FindRepoRoot();

        var run = DevelopmentComplexityStabilizationArtifacts.RunScopeGuardWithFailOnTrackedIgnored(
            repoRoot,
            ".llmgc/procedural/minimum-playable-generated-game/review-package/LLMGameCreatorAlpha.exe");

        Assert.NotEqual(0, run.ExitCode);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.False(document.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Contains(document.RootElement.GetProperty("changedPaths").EnumerateArray(), item =>
            item.GetProperty("category").GetString() == "tracked_heavy_output_warning" &&
            item.GetProperty("severity").GetString() == "error");
    }

    [Fact]
    public void CheckAllScriptSetsAndRestoresProductSmokeArtifactEnvironmentVariables()
    {
        var repoRoot = DevelopmentComplexityStabilizationArtifacts.FindRepoRoot();
        var script = File.ReadAllText(Path.Combine(repoRoot, ".devflow", "scripts", "check-all.ps1"));

        Assert.Contains("$env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR = $CheckAllProductSmokeProjectDir", script, StringComparison.Ordinal);
        Assert.Contains("$env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR = $CheckAllProductSmokePackageOutputDir", script, StringComparison.Ordinal);
        Assert.Contains("$env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR = $PreviousProductSmokeProjectDir", script, StringComparison.Ordinal);
        Assert.Contains("$env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR = $PreviousProductSmokePackageOutputDir", script, StringComparison.Ordinal);
        Assert.Contains("Remove-Item Env:\\LLMGC_PRODUCT_SMOKE_PROJECT_DIR", script, StringComparison.Ordinal);
        Assert.Contains("Remove-Item Env:\\LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR", script, StringComparison.Ordinal);
        Assert.Contains("FullyQualifiedName!~ProductSmoke", script, StringComparison.Ordinal);
    }

    [Fact]
    public void WritesDeterministicStabilizationArtifactsWithRequiredManualGate()
    {
        using var temp = new TempDirectory();
        var repoRoot = DevelopmentComplexityStabilizationArtifacts.FindRepoRoot();

        var first = DevelopmentComplexityStabilizationArtifacts.WriteArtifacts(repoRoot, temp.Path);
        var firstReport = File.ReadAllText(first.ReportJsonPath);
        var second = DevelopmentComplexityStabilizationArtifacts.WriteArtifacts(repoRoot, temp.Path);
        var secondReport = File.ReadAllText(second.ReportJsonPath);

        Assert.Equal(firstReport, secondReport);
        using var document = JsonDocument.Parse(firstReport);
        var root = document.RootElement;
        Assert.False(root.GetProperty("accepted").GetBoolean());
        Assert.Equal(DevelopmentComplexityStabilizationArtifacts.FinalGate, root.GetProperty("finalStatus").GetString());
        Assert.Equal(DevelopmentComplexityStabilizationArtifacts.FinalGate, root.GetProperty("manualGate").GetString());
        Assert.Equal(DevelopmentComplexityStabilizationArtifacts.PreviousAcceptedGate, root.GetProperty("previousAcceptedGate").GetString());
        Assert.False(root.GetProperty("capabilitySelectionStarted").GetBoolean());
        Assert.True(root.GetProperty("scopeGuardImplemented").GetBoolean());
        Assert.True(root.GetProperty("checkAllArtifactIsolationImplemented").GetBoolean());
        Assert.True(File.Exists(first.InventoryJsonPath));
        Assert.True(File.Exists(second.InvalidMatrixJsonPath));
    }

    [Fact]
    public void StateDocsRecordGoal021AcceptedBeforeGoal022()
    {
        var repoRoot = DevelopmentComplexityStabilizationArtifacts.FindRepoRoot();
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(repoRoot, "docs", "CURRENT_GENERATOR_STATE.json")));
        var markdown = File.ReadAllText(Path.Combine(repoRoot, "docs", "CURRENT_GENERATOR_STATE.md"));
        var contextIndex = File.ReadAllText(Path.Combine(repoRoot, "docs", "CONTEXT_INDEX.md"));

        Assert.Equal("development_complexity_stabilization_verification", state.RootElement.GetProperty("gate_status").GetString());
        Assert.Equal("goal_022_development_complexity_stabilization", state.RootElement.GetProperty("last_completed_product_slice_id").GetString());
        Assert.Contains("generated_game_profile_contract_verification passed", markdown);
        Assert.Contains("development_complexity_stabilization_verification", markdown);
        Assert.Contains("development_complexity_stabilization_verification", contextIndex);
    }

    [Fact]
    public void QueueDocsPutCapabilitySelectionAfterStabilizationGoal()
    {
        var repoRoot = DevelopmentComplexityStabilizationArtifacts.FindRepoRoot();
        var queue = File.ReadAllText(Path.Combine(repoRoot, "docs", "FULL_GENERATOR_GOAL_QUEUE.md"));

        var stabilizationIndex = queue.IndexOf("### Goal 022: Development Complexity Stabilization", StringComparison.Ordinal);
        var capabilityIndex = queue.IndexOf("### Goal 023: Capability Bundle Selection To Pipeline Inputs", StringComparison.Ordinal);

        Assert.True(stabilizationIndex >= 0, "Goal 022 stabilization entry is missing.");
        Assert.True(capabilityIndex > stabilizationIndex, "Capability Bundle Selection must stay after stabilization.");
        Assert.Contains("development_complexity_stabilization_verification", queue);
        Assert.Contains(
            "Status:\n\nProduced for review. The gate remains `required`, not `passed`",
            queue.Replace("\r\n", "\n", StringComparison.Ordinal));
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
