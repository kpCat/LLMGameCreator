using System.Diagnostics;
using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal150FPowerShellParserGateTests
{
    [Fact]
    public void Goal150F_production_scripts_parse_with_the_PowerShell_AST_parser()
    {
        var root = FindRoot();
        var scripts = new[]
        {
            ".devflow/scripts/run-complete-test-suite.ps1",
            ".devflow/scripts/check-artifact-scope.ps1",
            ".devflow/scripts/run-goal150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix.ps1",
            ".devflow/scripts/run-goal150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix.ps1"
        };
        var literalPaths = string.Join(",", scripts.Select(path => "'" + Path.Combine(root, path).Replace("'", "''") + "'"));
        var command = "$ErrorActionPreference='Stop'; $failed=@(); foreach($path in @(" + literalPaths + ")) { $tokens=$null; $errors=$null; [System.Management.Automation.Language.Parser]::ParseFile($path,[ref]$tokens,[ref]$errors)|Out-Null; $failed += @($errors | ForEach-Object { \"$($path):$($_.Extent.StartLineNumber):$($_.Extent.StartColumnNumber) $($_.Message)\" }) }; if($failed.Count -gt 0) { $failed | ForEach-Object { Write-Error $_ }; exit 1 }";
        var result = RunPowerShell(command, root);
        Assert.True(result.ExitCode == 0, result.Output);
    }

    private static (int ExitCode, string Output) RunPowerShell(string command, string root)
    {
        using var process = Process.Start(new ProcessStartInfo("powershell", "-NoProfile -NonInteractive -Command " + Quote(command))
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        }) ?? throw new InvalidOperationException("PowerShell could not be started.");
        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output);
    }

    private static string Quote(string value) => "\"" + value.Replace("\"", "\\\"") + "\"";

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
