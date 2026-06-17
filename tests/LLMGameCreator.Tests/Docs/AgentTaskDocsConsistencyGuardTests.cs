using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace LLMGameCreator.Tests.Docs;

public sealed class AgentTaskDocsConsistencyGuardTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string DocsAgentTasksDir = Path.Combine(RepoRoot, "docs", "agent-tasks");

    [Fact]
    public void RequiredFrameworkDocsExist()
    {
        Assert.True(File.Exists(Path.Combine(DocsAgentTasksDir, "000_INDEX.md")), "000_INDEX.md is missing.");
        Assert.True(File.Exists(Path.Combine(DocsAgentTasksDir, "001_TASK_PACK_LEDGER.md")), "001_TASK_PACK_LEDGER.md is missing.");
        Assert.True(File.Exists(Path.Combine(DocsAgentTasksDir, "002_NEXT_PACK_REQUEST.md")), "002_NEXT_PACK_REQUEST.md is missing.");
    }

    [Fact]
    public void IndexReferencesExistingTaskSpecFiles()
    {
        var indexPath = Path.Combine(DocsAgentTasksDir, "000_INDEX.md");
        var indexText = File.ReadAllText(indexPath);

        var referencedSpecs = indexText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .SelectMany(line => ExtractSpecPaths(line))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        Assert.NotEmpty(referencedSpecs);

        foreach (var relativePath in referencedSpecs)
        {
            var fullPath = Path.Combine(RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(fullPath), $"Task spec referenced in 000_INDEX.md is missing: {relativePath}");
        }
    }

    [Fact]
    public void AllExecutableMd1TaskSpecsContainRequiredSections()
    {
        var indexText = File.ReadAllText(Path.Combine(DocsAgentTasksDir, "000_INDEX.md"));
        var executableSpecs = ExtractExecutableTaskSpecPathsFromIndex(indexText);

        Assert.NotEmpty(executableSpecs);

        foreach (var relativePath in executableSpecs)
        {
            var fullPath = Path.Combine(RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(fullPath), $"Executable task spec is missing: {relativePath}");

            var text = File.ReadAllText(fullPath);
            ValidateExecutableSpec(text, relativePath);
        }
    }

    [Fact]
    public void ExpectedFinalReportFieldsArePresentInExecutableSpecs()
    {
        var indexText = File.ReadAllText(Path.Combine(DocsAgentTasksDir, "000_INDEX.md"));
        var executableSpecs = ExtractExecutableTaskSpecPathsFromIndex(indexText);

        Assert.NotEmpty(executableSpecs);

        foreach (var relativePath in executableSpecs)
        {
            var fullPath = Path.Combine(RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(fullPath), $"Executable task spec is missing: {relativePath}");

            var text = File.ReadAllText(fullPath);
            ValidateExecutableSpec(text, relativePath, "## Expected final report", "## Next task pointer");
        }
    }

    [Fact]
    public void ExtractExecutableTaskSpecPathsFromIndexExtractsTableRowEndingWithPipe()
    {
        var indexText = @"### M4.1

| Task | Status | Spec |
|---|---|---|
| M4_1_008 | Ready with approval | `M4_1/M4_1_008_AGENT_TASK_DOCS_CONSISTENCY_GUARD.md` |
| M4_1_018 | Support doc | `M4_1/018_EXEC_QUEUE.md` |
";

        var executableSpecs = ExtractExecutableTaskSpecPathsFromIndex(indexText).ToList();

        Assert.Contains("docs/agent-tasks/M4_1/M4_1_008_AGENT_TASK_DOCS_CONSISTENCY_GUARD.md", executableSpecs);
        Assert.DoesNotContain("docs/agent-tasks/M4_1/018_EXEC_QUEUE.md", executableSpecs);
    }

    [Fact]
    public void RequiredM41SupportDocsExist()
    {
        var expected = new[]
        {
            "docs/agent-tasks/M4_1/018_EXEC_QUEUE.md",
            "docs/agent-tasks/M4_1/019_KILO_PROMPTS.md",
            "docs/agent-tasks/M4_1/020_REVIEW_GATE.md"
        };

        foreach (var relativePath in expected)
        {
            var fullPath = Path.Combine(RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(fullPath), $"Required M4.1 support doc is missing: {relativePath}");
        }
    }

    [Fact]
    public void LockedM5AndM6SpecsRemainLockedWhileCurrentStateBlocksM5M6()
    {
        var currentStateText = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CURRENT_GENERATOR_STATE.md"));
        var isStateBlockingM5M6 = currentStateText.IndexOf("M5", StringComparison.OrdinalIgnoreCase) >= 0
                                  && currentStateText.IndexOf("lock", StringComparison.OrdinalIgnoreCase) >= 0;

        Assert.True(isStateBlockingM5M6, "CURRENT_GENERATOR_STATE.md must reference M5/M6 lock semantics for this guard to verify.");

        var indexText = File.ReadAllText(Path.Combine(DocsAgentTasksDir, "000_INDEX.md"));

        foreach (var line in indexText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!line.Contains("M5_", StringComparison.OrdinalIgnoreCase) 
                && !line.Contains("M6_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryExtractSingleSpecPath(line, out var specPath))
            {
                var fullPath = Path.Combine(RepoRoot, specPath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(fullPath))
                {
                    continue;
                }

                var specText = File.ReadAllText(fullPath);
                Assert.Contains("Locked", specText, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void NextPackRequestPointsToPackIdAndHasQualityBar()
    {
        var path = Path.Combine(DocsAgentTasksDir, "002_NEXT_PACK_REQUEST.md");
        Assert.True(File.Exists(path), "002_NEXT_PACK_REQUEST.md is missing.");

        var text = File.ReadAllText(path);

        Assert.Contains("agent-task-pack-", text.ToLowerInvariant());
        Assert.Contains("quality bar", text.ToLowerInvariant());
    }

    [Fact]
    public void MissingProofTestsSectionFailsWithNamedAssertion()
    {
        var badSpec = @"# TMP

## Header
Task ID: tmp

## Stop conditions
Stop if:
- x

";

        Assert.Throws<Xunit.Sdk.XunitException>(() =>
        {
            ValidateExecutableSpec(badSpec, "temp.md");
        });
    }

    [Fact]
    public void MissingSystemGatesSectionFailsWithNamedAssertion()
    {
        var badSpec = @"# TMP

## Proof tests
x

## Stop conditions
Stop if:
- x

";

        Assert.Throws<Xunit.Sdk.XunitException>(() =>
        {
            ValidateExecutableSpec(badSpec, "temp.md");
        });
    }

    [Fact]
    public void MissingStopConditionsSectionFailsWithNamedAssertion()
    {
        var badSpec = @"# TMP

## Proof tests
x

## System gates
x

";

        Assert.Throws<Xunit.Sdk.XunitException>(() =>
        {
            ValidateExecutableSpec(badSpec, "temp.md");
        });
    }

    [Fact]
    public void StopModeNextTaskRequiresStopReviewAndStopActionAndNoTaskSpecFile()
    {
        var stoppedContent = @"# NEXT_TASK

Mode: stop
Task source: stop
Task id: STOP_REVIEW
Reason: M4.1 deterministic hardening queue reached the human review gate.
User approval:
Expected stop after completion: yes
Stop action: Do not start future work. Review the completed task, check-all output, and whether M4.1 gate review should continue.
";

        var nextTaskDir = Path.Combine(RepoRoot, ".devflow");
        var tempNextTask = Path.Combine(nextTaskDir, "NEXT_TASK.md.stop-mode-test");
        File.WriteAllText(tempNextTask, stoppedContent, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        var tempCheckScript = Path.Combine(nextTaskDir, "scripts", "check-devflow-state.stop-mode-test.ps1");
        var originalScript = File.ReadAllText(Path.Combine(nextTaskDir, "scripts", "check-devflow-state.ps1"));
        File.WriteAllText(tempCheckScript, originalScript, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        try
        {
            var info = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-ExecutionPolicy Bypass -File \"{tempCheckScript}\"",
                WorkingDirectory = RepoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            var tempBackup = Path.Combine(nextTaskDir, "NEXT_TASK.md.backup-stop-test");
            File.Copy(Path.Combine(nextTaskDir, "NEXT_TASK.md"), tempBackup, overwrite: true);
            File.Copy(tempNextTask, Path.Combine(nextTaskDir, "NEXT_TASK.md"), overwrite: true);

            try
            {
                using var process = System.Diagnostics.Process.Start(info);
                Assert.True(process.WaitForExit(120000), "check-devflow-state.ps1 did not exit in time for stop mode test.");
                Assert.Equal(0, process.ExitCode);
            }
            finally
            {
                File.Copy(tempBackup, Path.Combine(nextTaskDir, "NEXT_TASK.md"), overwrite: true);
                File.Delete(tempBackup);
            }
        }
        finally
        {
            File.Delete(tempNextTask);
            File.Delete(tempCheckScript);
        }
    }

    [Fact]
    public void StopModeNextTaskWithoutStopActionFails()
    {
        var badStopContent = @"# NEXT_TASK

Mode: stop
Task source: stop
Task id: STOP_REVIEW
Reason: M4.1 deterministic hardening queue reached the human review gate.
User approval:
Expected stop after completion: yes
";

        var nextTaskDir = Path.Combine(RepoRoot, ".devflow");
        var tempNextTask = Path.Combine(nextTaskDir, "NEXT_TASK.md.stop-mode-test");
        var tempCheckScript = Path.Combine(nextTaskDir, "scripts", "check-devflow-state.stop-mode-test.ps1");
        File.WriteAllText(tempNextTask, badStopContent, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var originalScript = File.ReadAllText(Path.Combine(nextTaskDir, "scripts", "check-devflow-state.ps1"));
        File.WriteAllText(tempCheckScript, originalScript, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        try
        {
            var info = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-ExecutionPolicy Bypass -File \"{tempCheckScript}\"",
                WorkingDirectory = RepoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            var tempBackup = Path.Combine(nextTaskDir, "NEXT_TASK.md.backup-stop-test");
            File.Copy(Path.Combine(nextTaskDir, "NEXT_TASK.md"), tempBackup, overwrite: true);
            File.Copy(tempNextTask, Path.Combine(nextTaskDir, "NEXT_TASK.md"), overwrite: true);

            try
            {
                using var process = System.Diagnostics.Process.Start(info);
                Assert.True(process.WaitForExit(120000), "check-devflow-state.ps1 did not exit in time for bad stop mode test.");
                Assert.NotEqual(0, process.ExitCode);
            }
            finally
            {
                File.Copy(tempBackup, Path.Combine(nextTaskDir, "NEXT_TASK.md"), overwrite: true);
                File.Delete(tempBackup);
            }
        }
        finally
        {
            File.Delete(tempNextTask);
            File.Delete(tempCheckScript);
        }
    }

    private static void ValidateExecutableSpec(string text, string fileName, params string[] requiredSubstrings)
    {
        if (requiredSubstrings.Length == 0)
        {
            requiredSubstrings = new[]
            {
                "# Proof tests",
                "## System gates",
                "## Stop conditions"
            };
        }

        foreach (var requiredSubstring in requiredSubstrings)
        {
            if (!text.Contains(requiredSubstring, StringComparison.OrdinalIgnoreCase))
            {
                throw new Xunit.Sdk.XunitException($"{requiredSubstring} section missing in: {fileName}");
            }
        }
    }

    private static System.Collections.Generic.IEnumerable<string> ExtractExecutableTaskSpecPathsFromIndex(string indexText)
    {
        var inExecutableTable = false;
        var executableSupportFileHints = new[]
        {
            "018_EXEC_QUEUE.md",
            "019_KILO_PROMPTS.md",
            "020_REVIEW_GATE.md"
        };

        foreach (var rawLine in indexText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();

            if (IsExecutableTaskTableHeader(line))
            {
                inExecutableTable = true;
                continue;
            }

            if (!inExecutableTable)
            {
                continue;
            }

            if (line.StartsWith("### ", StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            if (!line.StartsWith("|", StringComparison.OrdinalIgnoreCase) || !line.EndsWith("|", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var columns = line.Trim().Trim('|').Split('|').Select(c => c.Trim()).ToList();
            if (columns.Count < 3 || IsSeparatorRow(columns))
            {
                continue;
            }

            var candidate = columns[2].Trim();
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var normalized = NormalizeSpecPath(candidate);

            if (!normalized.StartsWith("docs/agent-tasks/M4_1/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var fileName = Path.GetFileName(normalized.Replace('/', Path.DirectorySeparatorChar));
            if (executableSupportFileHints.Any(h => fileName.Equals(h, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            yield return normalized;
        }
    }

    private static bool IsExecutableTaskTableHeader(string line)
    {
        if (!line.StartsWith("|", StringComparison.OrdinalIgnoreCase) || !line.EndsWith("|", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var columns = line.Trim().Trim('|').Split('|').Select(c => c.Trim()).ToList();
        return columns.Count >= 3
               && columns[0].Equals("Task", StringComparison.OrdinalIgnoreCase)
               && columns[2].Equals("Spec", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSeparatorRow(System.Collections.Generic.IReadOnlyList<string> columns)
    {
        return columns.Count >= 3 && columns.Take(3).All(IsSeparatorCell);
    }

    private static bool IsSeparatorCell(string cell)
    {
        var trimmed = cell.Trim();
        return trimmed.Length > 0 && trimmed.All(ch => ch == '-' || ch == ':' || ch == ' ');
    }

    private static string NormalizeSpecPath(string candidate)
    {
        var normalized = candidate.Trim('`').Trim('"').Trim('\'').Trim();

        if (!normalized.StartsWith("docs/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = "docs/agent-tasks/" + normalized;
        }

        return normalized;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory ?? Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "LLMGameCreator.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("LLMGameCreator.sln not found.");
    }

    private static System.Collections.Generic.IEnumerable<string> ExtractSpecPaths(string line)
    {
        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (part.StartsWith("M4_1/", StringComparison.OrdinalIgnoreCase) 
                || part.StartsWith("docs/agent-tasks/M4_1/", StringComparison.OrdinalIgnoreCase))
            {
                var clean = part.Trim('`').Trim('"').Trim('\'');
                if (!clean.StartsWith("docs/", StringComparison.OrdinalIgnoreCase))
                {
                    clean = "docs/agent-tasks/" + clean;
                }

                yield return clean;
            }
        }
    }

    private static bool TryExtractSingleSpecPath(string line, out string specPath)
    {
        specPath = string.Empty;
        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (part.Contains("M4_1/", StringComparison.OrdinalIgnoreCase) 
                || part.Contains("M5_", StringComparison.OrdinalIgnoreCase)
                || part.Contains("M6_", StringComparison.OrdinalIgnoreCase))
            {
                specPath = part.Trim('`').Trim('"').Trim('\'');
                if (!specPath.StartsWith("docs/", StringComparison.OrdinalIgnoreCase))
                {
                    specPath = "docs/agent-tasks/" + specPath;
                }

                return true;
            }
        }

        return false;
    }
}
