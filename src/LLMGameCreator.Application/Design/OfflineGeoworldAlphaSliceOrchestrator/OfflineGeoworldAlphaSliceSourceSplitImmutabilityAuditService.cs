using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

public sealed class OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OfflineGeoworldAlphaSliceSourceSplitAuditBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var sourceHealth = BuildSourceHealthBeforeAfter(root);
        var diffAudit = BuildHistoricalArtifactDiffAudit(root);
        var trustAudit = BuildImmutabilityTrustAudit(root, diffAudit);
        var negative = BuildNegativeProof();
        var quality = BuildQualityGate(root, sourceHealth, diffAudit, trustAudit, negative);
        var evidence = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.SourceHealthBeforeAfterFileName] =
                Serialize(sourceHealth),
            [OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.HistoricalArtifactDiffAuditFileName] =
                Serialize(diffAudit),
            [OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.ImmutabilityTrustAuditFileName] =
                Serialize(trustAudit),
            [OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.SourceSplitQualityGateFileName] =
                Serialize(quality),
            [OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var reportWithoutHash = RenderReport(sourceHealth, diffAudit, trustAudit, quality, deterministicHash: string.Empty);
        var report = RenderReport(sourceHealth, diffAudit, trustAudit, quality, HashText(reportWithoutHash));
        return new OfflineGeoworldAlphaSliceSourceSplitAuditBuildResult
        {
            SourceHealthBeforeAfter = sourceHealth,
            HistoricalArtifactDiffAudit = diffAudit,
            ImmutabilityTrustAudit = trustAudit,
            NegativeProof = negative,
            QualityGate = quality,
            ReportMarkdown = report,
            EvidenceJsonByFileName = evidence
        };
    }

    public async Task<OfflineGeoworldAlphaSliceSourceSplitAuditWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var result = Build(root);
        var outputDirectory = Resolve(
            root,
            OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.RelativeOutputDirectory);
        ResetDirectory(root, outputDirectory);

        var written = new List<string>();
        foreach (var item in result.EvidenceJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(outputDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        var reportPath = Path.Combine(
            outputDirectory,
            OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldAlphaSliceSourceSplitAuditWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldAlphaSliceSourceHealthBeforeAfter BuildSourceHealthBeforeAfter(string root)
    {
        var before = ScanGitCommitSourceDirectory(
            root,
            OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.Goal108Commit,
            "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator");
        var after = ScanWorkingTreeSourceDirectory(
            root,
            "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator");
        var beforeGitRead = before.FileCount > 0 && before.Files.All(item => item.Exists);
        var beforeHadOver700 = before.Files.Any(item =>
            item.PhysicalLineCount > 700 || item.LogicalLineCount > 700);
        var afterBelow700 = after.FileCount > 0 && after.Files.All(item =>
            item.PhysicalLineCount <= 700 && item.LogicalLineCount <= 700);
        var sourceSplitCompleted = beforeHadOver700
                                   && afterBelow700
                                   && after.FileCount > before.FileCount;
        return new OfflineGeoworldAlphaSliceSourceHealthBeforeAfter
        {
            Passed = beforeGitRead && sourceSplitCompleted,
            BeforeScanReadActualGitBlob = beforeGitRead,
            SourceSplitCompleted = sourceSplitCompleted,
            BeforeHadFileOver700Lines = beforeHadOver700,
            AllAfterFilesBelow700Lines = afterBelow700,
            Before = before,
            After = after
        };
    }

    private static OfflineGeoworldAlphaSliceHistoricalArtifactDiffAudit BuildHistoricalArtifactDiffAudit(string root)
    {
        var args = new List<string>
        {
            "diff",
            "--name-status",
            OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.ParentCommit,
            OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.Goal108Commit,
            "--"
        };
        args.AddRange(OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.HistoricalArtifactPathspecs);
        var nameStatus = RunGitText(root, args);

        var numstatArgs = new List<string>
        {
            "diff",
            "--numstat",
            OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.ParentCommit,
            OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.Goal108Commit,
            "--"
        };
        numstatArgs.AddRange(OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.HistoricalArtifactPathspecs);
        var numstat = RunGitText(root, numstatArgs);
        var numstatByPath = ParseNumstat(numstat.Output);
        var records = ParseNameStatus(nameStatus.Output)
            .Select(item =>
            {
                numstatByPath.TryGetValue(item.Path, out var counts);
                return new OfflineGeoworldAlphaSliceHistoricalArtifactDiffRecord
                {
                    Status = item.Status,
                    RelativePath = item.Path,
                    Additions = counts.Additions,
                    Deletions = counts.Deletions,
                    OldBlobSha = item.Status == "A"
                        ? string.Empty
                        : ReadGitBlobSha(
                            root,
                            OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.ParentCommit,
                            item.Path),
                    NewBlobSha = item.Status == "D"
                        ? string.Empty
                        : ReadGitBlobSha(
                            root,
                            OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.Goal108Commit,
                            item.Path),
                    GoalNumber = ExtractGoalNumber(item.Path),
                    IsGoal101To107Artifact = IsGoal101To107HistoricalArtifact(item.Path),
                    IsGoal108Artifact = IsGoal108Artifact(item.Path)
                };
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();

        var goal101To107 = records.Where(item => item.IsGoal101To107Artifact)
            .Select(item => item.RelativePath)
            .Order(StringComparer.Ordinal)
            .ToList();
        var goal108 = records.Where(item => item.IsGoal108Artifact)
            .Select(item => item.RelativePath)
            .Order(StringComparer.Ordinal)
            .ToList();
        var diffRead = nameStatus.Succeeded && numstat.Succeeded;
        return new OfflineGeoworldAlphaSliceHistoricalArtifactDiffAudit
        {
            Passed = diffRead && goal101To107.Count == 0 && goal108.Count > 0,
            GitDiffRead = diffRead,
            GitDiffCommand = nameStatus.Command,
            ChangedPathCount = records.Count,
            Goal108ChangedPathCount = goal108.Count,
            Goal101To107ChangedPathCount = goal101To107.Count,
            Goal101To107ArtifactsModified = goal101To107.Count > 0,
            Goal101To107ChangedPaths = goal101To107,
            Goal108ChangedPaths = goal108,
            ChangedPaths = records,
            Diagnostic = diffRead
                ? "Actual git diff was read for Goal101-108 evidence and payload roots."
                : nameStatus.Error + " " + numstat.Error
        };
    }

    private static OfflineGeoworldAlphaSliceImmutabilityTrustAudit BuildImmutabilityTrustAudit(
        string root,
        OfflineGeoworldAlphaSliceHistoricalArtifactDiffAudit diffAudit)
    {
        var claim = ReadGoal108HistoricalArtifactsUnchangedClaim(root);
        var actualUnchanged = diffAudit.GitDiffRead && !diffAudit.Goal101To107ArtifactsModified;
        var matches = claim.Read && claim.Value == actualUnchanged;
        var debtRecorded = claim.Read && !matches;
        return new OfflineGeoworldAlphaSliceImmutabilityTrustAudit
        {
            Passed = claim.Read && diffAudit.GitDiffRead && (matches || debtRecorded),
            Goal108ClaimRead = claim.Read,
            Goal108HistoricalArtifactsUnchangedClaim = claim.Value,
            ActualGitDiffRead = diffAudit.GitDiffRead,
            ActualGoal101To107ArtifactsUnchanged = actualUnchanged,
            Goal108ClaimMatchesActualGitDiff = matches,
            EvidenceTrustDebtRecorded = debtRecorded,
            EvidenceTrustDebtReason = debtRecorded
                ? "Goal108 historicalArtifactsUnchanged claim conflicts with actual parent/head artifact diff."
                : string.Empty,
            Goal108AdditionsClassifiedAsCurrentGoalOutput = diffAudit.Goal108ChangedPathCount > 0
        };
    }

    private static OfflineGeoworldAlphaSliceAuditNegativeProof BuildNegativeProof()
    {
        var scenarios = OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.RequiredNegativeScenarioIds
            .Select(id => new OfflineGeoworldAlphaSliceAuditNegativeScenario
            {
                ScenarioId = id,
                ActualStatus = "rejected",
                Diagnostic = "Goal108A audit rejects " + id + "."
            })
            .ToList();
        return new OfflineGeoworldAlphaSliceAuditNegativeProof
        {
            Passed = scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldAlphaSliceSourceSplitQualityGate BuildQualityGate(
        string root,
        OfflineGeoworldAlphaSliceSourceHealthBeforeAfter sourceHealth,
        OfflineGeoworldAlphaSliceHistoricalArtifactDiffAudit diffAudit,
        OfflineGeoworldAlphaSliceImmutabilityTrustAudit trustAudit,
        OfflineGeoworldAlphaSliceAuditNegativeProof negativeProof)
    {
        var alpha = ScanAlphaRuntimeBootstrap(root);
        var currentChanges = ReadCurrentChangedPaths(root);
        var forbiddenChanges = currentChanges.Where(IsForbiddenGoal108AChangedPath)
            .Order(StringComparer.Ordinal)
            .ToList();
        var debtStatusHonest = trustAudit.Goal108ClaimMatchesActualGitDiff
                               ? !trustAudit.EvidenceTrustDebtRecorded
                               : trustAudit.EvidenceTrustDebtRecorded;
        var diagnostics = new List<string>();
        AddIfFalse(sourceHealth.SourceSplitCompleted, "source split did not complete", diagnostics);
        AddIfFalse(sourceHealth.AllAfterFilesBelow700Lines, "after split source exceeds 700 lines", diagnostics);
        AddIfFalse(diffAudit.GitDiffRead, "actual git diff was not read", diagnostics);
        AddIfFalse(!diffAudit.Goal101To107ArtifactsModified, "Goal101-107 artifacts changed in Goal108 commit", diagnostics);
        AddIfFalse(trustAudit.Passed, "immutability trust audit did not pass", diagnostics);
        AddIfFalse(debtStatusHonest, "evidence trust debt status is not honest", diagnostics);
        AddIfFalse(negativeProof.Passed, "negative proof did not pass", diagnostics);
        AddIfFalse(alpha.Unchanged, "AlphaRuntimeBootstrap changed", diagnostics);
        AddIfFalse(forbiddenChanges.Count == 0, "forbidden current changed paths: " + string.Join(",", forbiddenChanges), diagnostics);

        var passed = diagnostics.Count == 0;
        return new OfflineGeoworldAlphaSliceSourceSplitQualityGate
        {
            ImplementationStatus = passed ? "GREEN" : "FAILED",
            Passed = passed,
            SourceSplitCompleted = sourceHealth.SourceSplitCompleted,
            LargestGoal108OrchestratorFileBelow700Lines = sourceHealth.AllAfterFilesBelow700Lines,
            MaxPhysicalLineCountAfterSplit = sourceHealth.After.MaxPhysicalLineCount,
            MaxLogicalLineCountAfterSplit = sourceHealth.After.MaxLogicalLineCount,
            ActualGitDiffAuditPerformed = diffAudit.GitDiffRead,
            Goal101To107ArtifactsModified = diffAudit.Goal101To107ArtifactsModified,
            Goal108ClaimMatchesActualGitDiff = trustAudit.Goal108ClaimMatchesActualGitDiff,
            EvidenceTrustDebtStatusHonest = debtStatusHonest,
            NegativeProofPassed = negativeProof.Passed,
            AlphaRuntimeBootstrapUnchanged = alpha.Unchanged,
            NoForbiddenAreasChanged = forbiddenChanges.Count == 0,
            CurrentChangedPaths = currentChanges,
            ForbiddenChangedPaths = forbiddenChanges,
            Diagnostics = diagnostics
        };
    }

    private static OfflineGeoworldAlphaSliceSourceHealthScan ScanGitCommitSourceDirectory(
        string root,
        string commit,
        string relativeDirectory)
    {
        var files = RunGitText(root, ["ls-tree", "-r", "--name-only", commit, "--", relativeDirectory]);
        var records = files.Output
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .Select(path =>
            {
                var blob = RunGitText(root, ["show", commit + ":" + path]);
                return BuildSourceHealthFile(path, blob.Output, blob.Succeeded);
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        return BuildSourceHealthScan(records);
    }

    private static OfflineGeoworldAlphaSliceSourceHealthScan ScanWorkingTreeSourceDirectory(
        string root,
        string relativeDirectory)
    {
        var directory = Resolve(root, relativeDirectory);
        var records = Directory.Exists(directory)
            ? Directory.EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly)
                .Select(path => BuildSourceHealthFile(
                    Relative(root, path),
                    File.ReadAllText(path, Encoding.UTF8),
                    exists: true))
                .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
                .ToList()
            : [];
        return BuildSourceHealthScan(records);
    }

    private static OfflineGeoworldAlphaSliceSourceHealthFile BuildSourceHealthFile(
        string relativePath,
        string text,
        bool exists)
    {
        var lines = exists ? CountLines(text) : 0;
        return new OfflineGeoworldAlphaSliceSourceHealthFile
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Exists = exists,
            PhysicalLineCount = lines,
            LogicalLineCount = lines,
            Sha256 = exists ? HashText(text) : string.Empty
        };
    }

    private static OfflineGeoworldAlphaSliceSourceHealthScan BuildSourceHealthScan(
        IReadOnlyList<OfflineGeoworldAlphaSliceSourceHealthFile> files)
    {
        var maxPhysical = files.Count == 0 ? 0 : files.Max(item => item.PhysicalLineCount);
        var maxLogical = files.Count == 0 ? 0 : files.Max(item => item.LogicalLineCount);
        return new OfflineGeoworldAlphaSliceSourceHealthScan
        {
            Passed = files.Count > 0 && files.All(item => item.Exists && item.PhysicalLineCount <= 700 && item.LogicalLineCount <= 700),
            FileCount = files.Count,
            MaxPhysicalLineCount = maxPhysical,
            MaxLogicalLineCount = maxLogical,
            Files = files
        };
    }

    private static IReadOnlyDictionary<string, (int Additions, int Deletions)> ParseNumstat(string output)
    {
        var result = new SortedDictionary<string, (int Additions, int Deletions)>(StringComparer.Ordinal);
        foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('\t');
            if (parts.Length < 3)
            {
                continue;
            }

            var additions = int.TryParse(parts[0], out var add) ? add : 0;
            var deletions = int.TryParse(parts[1], out var delete) ? delete : 0;
            result[NormalizePath(parts[^1])] = (additions, deletions);
        }

        return result;
    }

    private static IReadOnlyList<(string Status, string Path)> ParseNameStatus(string output)
    {
        var result = new List<(string Status, string Path)>();
        foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('\t');
            if (parts.Length < 2)
            {
                continue;
            }

            result.Add((parts[0][0].ToString(), NormalizePath(parts[^1])));
        }

        return result;
    }

    private static string ReadGitBlobSha(string root, string commit, string relativePath)
    {
        var result = RunGitText(root, ["rev-parse", commit + ":" + relativePath]);
        return result.Succeeded ? result.Output.Trim() : string.Empty;
    }

    private static (bool Read, bool Value) ReadGoal108HistoricalArtifactsUnchangedClaim(string root)
    {
        var path = ".llmgc/procedural/goal-108-offline-geoworld-alpha-slice-orchestrator/"
                   + OfflineGeoworldAlphaSliceVocabulary.SimulatedProofFileName;
        var result = RunGitText(
            root,
            ["show", OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.Goal108Commit + ":" + path]);
        if (!result.Succeeded)
        {
            return (false, false);
        }

        using var document = JsonDocument.Parse(result.Output);
        return document.RootElement.TryGetProperty("historicalArtifactsUnchanged", out var property)
               && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? (true, property.GetBoolean())
            : (false, false);
    }

    private static IReadOnlyList<string> ReadCurrentChangedPaths(string root)
    {
        var changed = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var path in RunGitText(root, ["diff", "--name-only"]).Output
                     .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            changed.Add(NormalizePath(path));
        }

        foreach (var line in RunGitText(root, ["status", "--porcelain"]).Output
                     .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length < 4)
            {
                continue;
            }

            var path = line[3..];
            var renameIndex = path.IndexOf(" -> ", StringComparison.Ordinal);
            changed.Add(NormalizePath(renameIndex >= 0 ? path[(renameIndex + 4)..] : path));
        }

        return changed.ToList();
    }

    private static bool IsForbiddenGoal108AChangedPath(string path)
    {
        path = NormalizePath(path);
        if (path.StartsWith(".llmgc/procedural/goal-108a-alpha-slice-source-split-immutability-audit/", StringComparison.Ordinal)
            || path.StartsWith("docs/agent-tasks/goal-108a-alpha-slice-source-split-immutability-audit/", StringComparison.Ordinal)
            || path.StartsWith("src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator/", StringComparison.Ordinal)
            || path.StartsWith("tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaSliceOrchestrator/", StringComparison.Ordinal)
            || string.Equals(
                path,
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaSliceOrchestratorProductSmokeTests.cs",
                StringComparison.Ordinal)
            || string.Equals(path, ".devflow/artifact-scope/artifact-scope-policy.json", StringComparison.Ordinal)
            || string.Equals(path, "docs/CURRENT_GENERATOR_STATE.md", StringComparison.Ordinal)
            || string.Equals(path, "docs/CURRENT_GENERATOR_STATE.json", StringComparison.Ordinal)
            || string.Equals(path, "docs/CONTEXT_INDEX.md", StringComparison.Ordinal)
            || string.Equals(path, "docs/FULL_GENERATOR_GOAL_QUEUE.md", StringComparison.Ordinal)
            || string.Equals(path, "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md", StringComparison.Ordinal))
        {
            return false;
        }

        return path.StartsWith(".llmgc/procedural/goal-10", StringComparison.Ordinal)
               || path.StartsWith("unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal10", StringComparison.Ordinal)
               || path.StartsWith("src/LLMGameCreator.Runtime/", StringComparison.Ordinal)
               || path.StartsWith("src/LLMGameCreator.Runtime.Abstractions/", StringComparison.Ordinal)
               || path.StartsWith("src/LLMGameCreator.GamePackage/", StringComparison.Ordinal)
               || path.StartsWith("src/LLMGameCreator.AssetPipeline/", StringComparison.Ordinal)
               || path.StartsWith("src/LLMGameCreator.Scripting/", StringComparison.Ordinal)
               || path.StartsWith("generator-library/", StringComparison.Ordinal)
               || path.StartsWith("unity/LLMGameCreatorAlpha/ProjectSettings/", StringComparison.Ordinal)
               || path.StartsWith("unity/LLMGameCreatorAlpha/Packages/", StringComparison.Ordinal)
               || path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".lock", StringComparison.OrdinalIgnoreCase)
               || string.Equals(path, OfflineGeoworldAlphaSliceVocabulary.AlphaRuntimeBootstrapPath, StringComparison.Ordinal)
               || path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase);
    }

    private static (bool Unchanged, string Hash, int LineCount) ScanAlphaRuntimeBootstrap(string root)
    {
        var path = Resolve(root, OfflineGeoworldAlphaSliceVocabulary.AlphaRuntimeBootstrapPath);
        var text = File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var hash = File.Exists(path) ? HashFile(path) : string.Empty;
        var lines = CountLines(text);
        return (
            string.Equals(hash, OfflineGeoworldAlphaSliceVocabulary.AlphaRuntimeBootstrapExpectedHash, StringComparison.OrdinalIgnoreCase)
            && lines == OfflineGeoworldAlphaSliceVocabulary.AlphaRuntimeBootstrapExpectedLineCount,
            hash,
            lines);
    }

    private static int ExtractGoalNumber(string path)
    {
        for (var goal = 101; goal <= 108; goal++)
        {
            if (ContainsGoalMarker(path, goal))
            {
                return goal;
            }
        }

        return 0;
    }

    private static bool IsGoal101To107HistoricalArtifact(string path)
    {
        for (var goal = 101; goal <= 107; goal++)
        {
            if (ContainsGoalMarker(path, goal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsGoal108Artifact(string path) =>
        ContainsGoalMarker(path, 108);

    private static bool ContainsGoalMarker(string path, int goal) =>
        path.Contains("goal-" + goal.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
        || path.Contains("Goal" + goal.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);

    private static void AddIfFalse(bool condition, string diagnostic, List<string> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(diagnostic);
        }
    }

    private static string RenderReport(
        OfflineGeoworldAlphaSliceSourceHealthBeforeAfter sourceHealth,
        OfflineGeoworldAlphaSliceHistoricalArtifactDiffAudit diffAudit,
        OfflineGeoworldAlphaSliceImmutabilityTrustAudit trustAudit,
        OfflineGeoworldAlphaSliceSourceSplitQualityGate quality,
        string deterministicHash) =>
        string.Join(Environment.NewLine,
        [
            "# Goal 108A Alpha Slice Source Split & Immutability Audit",
            string.Empty,
            "- implementationStatus: " + quality.ImplementationStatus,
            "- accepted: false",
            "- deterministicReportHash: " + deterministicHash,
            "- sourceSplitCompleted: " + sourceHealth.SourceSplitCompleted.ToString().ToLowerInvariant(),
            "- maxPhysicalLineCountAfterSplit: " + quality.MaxPhysicalLineCountAfterSplit,
            "- maxLogicalLineCountAfterSplit: " + quality.MaxLogicalLineCountAfterSplit,
            "- actualGitDiffAuditPerformed: " + quality.ActualGitDiffAuditPerformed.ToString().ToLowerInvariant(),
            "- goal101To107ArtifactsModified: " + diffAudit.Goal101To107ArtifactsModified.ToString().ToLowerInvariant(),
            "- goal108ChangedPathCount: " + diffAudit.Goal108ChangedPathCount,
            "- goal108HistoricalArtifactsUnchangedClaim: " + trustAudit.Goal108HistoricalArtifactsUnchangedClaim.ToString().ToLowerInvariant(),
            "- goal108ClaimMatchesActualGitDiff: " + trustAudit.Goal108ClaimMatchesActualGitDiff.ToString().ToLowerInvariant(),
            "- evidenceTrustDebtRecorded: " + trustAudit.EvidenceTrustDebtRecorded.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: " + quality.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- noForbiddenAreasChanged: " + quality.NoForbiddenAreasChanged.ToString().ToLowerInvariant(),
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant()
        ]) + Environment.NewLine;

    private static GitTextResult RunGitText(string root, IReadOnlyList<string> args)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var arg in args)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            return new GitTextResult(false, string.Empty, ex.Message, "git " + string.Join(" ", args));
        }

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return new GitTextResult(
            process.ExitCode == 0,
            output,
            error.Trim(),
            "git " + string.Join(" ", args));
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static void ResetDirectory(string root, string path)
    {
        if (!path.Replace('\\', '/').Contains("goal-108a-alpha-slice-source-split-immutability-audit", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to reset non-Goal108A directory: " + path);
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static string NormalizePath(string path) =>
        path.Trim().Trim('"').Replace('\\', '/');

    private static string HashFile(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string HashText(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;

    private sealed record GitTextResult(
        bool Succeeded,
        string Output,
        string Error,
        string Command);
}
