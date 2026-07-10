using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunSelectedRuntimeVariantPlayerAdapterHandoffScriptTests
{
    [Fact]
    public void ScriptHasRequiredParametersSafetyTransactionAndUnitySmoke()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow/scripts/run-selected-runtime-variant-playeradapter-handoff.ps1"));

        foreach (var parameter in new[]
                 {
                     "$SelectedHandoffPath",
                     "$SelectedPackagePath",
                     "$SelectedOutcomePath",
                     "$SelectedRoundtripResultPath",
                     "$OutputRoot",
                     "$UnityPath",
                     "$DryRun",
                     "$ApplyCleanup"
                 })
        {
            Assert.Contains(parameter, script, StringComparison.Ordinal);
        }

        Assert.Contains("Goal143 refuses .llmgc/manual", script, StringComparison.Ordinal);
        Assert.Contains("OutputRoot must stay under the Goal143 output root", script);
        Assert.Contains("selected package SHA-256", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Copy-Goal143Directory", script);
        Assert.Contains("Restore-Goal143Directory", script);
        Assert.Contains("Invoke-Goal143CoreProof", script);
        Assert.Contains("Invoke-Goal143UnitySmoke", script);
        Assert.Contains("GOAL143_SELECTED_RUNTIME_VARIANT_PLAYERADAPTER_PASS", script);
        Assert.Contains("GOAL143_SELECTED_RUNTIME_VARIANT_PLAYERADAPTER_FAIL", script);
        Assert.Contains("LLMGC_GOAL143_REQUIRE_UNITY_SMOKE", script);
    }

    [Fact]
    public void CmdInvokesPowerShellScript()
    {
        var root = ProjectRoot();
        var command = File.ReadAllText(Path.Combine(
            root,
            ".devflow/scripts/run-selected-runtime-variant-playeradapter-handoff.cmd"));

        Assert.Contains("run-selected-runtime-variant-playeradapter-handoff.ps1", command);
        Assert.Contains("%*", command);
        Assert.Contains("%ERRORLEVEL%", command);
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
