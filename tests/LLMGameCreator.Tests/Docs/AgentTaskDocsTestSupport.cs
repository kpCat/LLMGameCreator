using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Sdk;

namespace LLMGameCreator.Tests.Docs;

internal static class AgentTaskDocsTestSupport
{
    public static readonly string RepoRoot = FindRepoRoot();
    public static readonly string DocsAgentTasksDir = Path.Combine(RepoRoot, "docs", "agent-tasks");

    public static IReadOnlyList<string> ExtractExecutableTaskSpecPathsFromIndex(string indexText)
    {
        var results = new List<string>();
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
                break;
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

            results.Add(normalized);
        }

        return results;
    }

    public static IEnumerable<string> ExtractSpecPaths(string line)
    {
        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (part.StartsWith("M4_1/", StringComparison.OrdinalIgnoreCase)
                || part.StartsWith("docs/agent-tasks/M4_1/", StringComparison.OrdinalIgnoreCase))
            {
                yield return NormalizeSpecPath(part);
            }
        }
    }

    public static bool TryExtractSingleSpecPath(string line, out string specPath)
    {
        specPath = string.Empty;
        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (part.Contains("M4_1/", StringComparison.OrdinalIgnoreCase)
                || part.Contains("M5_", StringComparison.OrdinalIgnoreCase)
                || part.Contains("M6_", StringComparison.OrdinalIgnoreCase))
            {
                specPath = NormalizeSpecPath(part);
                return true;
            }
        }

        return false;
    }

    public static string NormalizeSpecPath(string candidate)
    {
        var normalized = candidate.Trim('`').Trim('"').Trim('\'').Trim();

        if (!normalized.StartsWith("docs/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = "docs/agent-tasks/" + normalized;
        }

        return normalized;
    }

    public static void ValidateExecutableSpec(string text, string fileName, params string[] requiredSubstrings)
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
                throw new XunitException($"{requiredSubstring} section missing in: {fileName}");
            }
        }
    }

    public static Process RunPowerShellScript(string scriptPath, string workingDirectory)
    {
        var info = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        return Process.Start(info)
            ?? throw new InvalidOperationException($"Failed to start {Path.GetFileName(scriptPath)}.");
    }

    public static ProcessResult RunCheckDevflowStateWithTemporaryNextTask(string nextTaskContent)
    {
        var nextTaskDir = Path.Combine(RepoRoot, ".devflow");
        var nextTaskPath = Path.Combine(nextTaskDir, "NEXT_TASK.md");
        var tempNextTask = Path.Combine(nextTaskDir, "NEXT_TASK.md.stop-mode-test");
        var tempBackup = Path.Combine(nextTaskDir, "NEXT_TASK.md.backup-stop-test");
        var scriptPath = Path.Combine(nextTaskDir, "scripts", "check-devflow-state.ps1");
        var tempCheckScript = Path.Combine(nextTaskDir, "scripts", "check-devflow-state.stop-mode-test.ps1");
        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        File.WriteAllText(tempNextTask, nextTaskContent, utf8NoBom);
        File.WriteAllText(tempCheckScript, File.ReadAllText(scriptPath), utf8NoBom);

        try
        {
            File.Copy(nextTaskPath, tempBackup, overwrite: true);
            File.Copy(tempNextTask, nextTaskPath, overwrite: true);

            try
            {
                using var process = RunPowerShellScript(tempCheckScript, RepoRoot);
                Assert.True(process.WaitForExit(120000), "check-devflow-state.ps1 did not exit in time for stop mode test.");

                return new ProcessResult(
                    process.ExitCode,
                    process.StandardOutput.ReadToEnd(),
                    process.StandardError.ReadToEnd());
            }
            finally
            {
                File.Copy(tempBackup, nextTaskPath, overwrite: true);
            }
        }
        finally
        {
            DeleteIfExists(tempBackup);
            DeleteIfExists(tempNextTask);
            DeleteIfExists(tempCheckScript);
        }
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

    private static bool IsSeparatorRow(IReadOnlyList<string> columns)
    {
        return columns.Count >= 3 && columns.Take(3).All(IsSeparatorCell);
    }

    private static bool IsSeparatorCell(string cell)
    {
        var trimmed = cell.Trim();
        return trimmed.Length > 0 && trimmed.All(ch => ch == '-' || ch == ':' || ch == ' ');
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}

internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
