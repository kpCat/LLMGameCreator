using System;
using System.IO;
using System.Linq;
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

        foreach (var relativePath in executableSpecs)
        {
            var fullPath = Path.Combine(RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                continue;
            }

            var text = File.ReadAllText(fullPath);

            Assert.Contains("# Proof tests", text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("## System gates", text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("## Stop conditions", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ExpectedFinalReportFieldsArePresentInExecutableSpecs()
    {
        var indexText = File.ReadAllText(Path.Combine(DocsAgentTasksDir, "000_INDEX.md"));
        var executableSpecs = ExtractExecutableTaskSpecPathsFromIndex(indexText);

        foreach (var relativePath in executableSpecs)
        {
            var fullPath = Path.Combine(RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                continue;
            }

            var text = File.ReadAllText(fullPath);

            Assert.Contains("## Expected final report", text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("## Next task pointer", text, StringComparison.OrdinalIgnoreCase);
        }
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

    private static void ValidateExecutableSpec(string text, string fileName)
    {
        if (!text.Contains("# Proof tests", StringComparison.OrdinalIgnoreCase))
        {
            throw new Xunit.Sdk.XunitException($"Proof tests section missing in: {fileName}");
        }

        if (!text.Contains("## System gates", StringComparison.OrdinalIgnoreCase))
        {
            throw new Xunit.Sdk.XunitException($"System gates section missing in: {fileName}");
        }

        if (!text.Contains("## Stop conditions", StringComparison.OrdinalIgnoreCase))
        {
            throw new Xunit.Sdk.XunitException($"Stop conditions section missing in: {fileName}");
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

            if (line.StartsWith("| Task | Status | Spec |", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("| Task| Status| Spec|", StringComparison.OrdinalIgnoreCase))
            {
                inExecutableTable = true;
                continue;
            }

            if (inExecutableTable && line.StartsWith("### ", StringComparison.OrdinalIgnoreCase))
            {
                inExecutableTable = false;
                continue;
            }

            if (!inExecutableTable || !line.StartsWith("|", StringComparison.OrdinalIgnoreCase) || line.EndsWith("|", StringComparison.OrdinalIgnoreCase))
            {
                var clean = line.Trim('|').Trim();
                if (!clean.Contains("|", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                continue;
            }

            var segments = line.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 3)
            {
                continue;
            }

            var candidate = segments[2].Trim();
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var normalized = candidate.Trim('`').Trim('"').Trim('\'');

            if (!normalized.StartsWith("docs/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = "docs/agent-tasks/" + normalized;
            }

            if (normalized.IndexOf("M4_1/", StringComparison.OrdinalIgnoreCase) < 0
                && normalized.IndexOf("docs/agent-tasks/M4_1/", StringComparison.OrdinalIgnoreCase) < 0)
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
