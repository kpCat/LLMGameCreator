using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorSpineQualityConsolidation;
using Xunit;

namespace LLMGameCreator.Tests.Application.GeneratorSpineQualityConsolidation;

public sealed class GeneratorSpineQualityScannerTests
{
    [Fact]
    public void ScannerIdentifiesDeliberatelyMinifiedFileFixture()
    {
        var text = "public sealed class A{public void M1(){var a=1;}public void M2(){var b=2;}public void M3(){var c=3;}public void M4(){var d=4;}public void M5(){var e=5;}public void M6(){var f=6;}public void M7(){var g=7;}public void M8(){var h=8;}public void M9(){var i=9;}public void M10(){var j=10;}}";

        var record = new GeneratorSpineQualityScanner().AnalyzeSourceText("src/Minified.cs", text);

        Assert.True(record.IsOneLineOrMinifiedCandidate);
        Assert.True(record.MaxLineLength > 240);
    }

    [Fact]
    public void ScannerDoesNotFlagNormalFilesAsMinified()
    {
        var text = """
namespace Fixture;

public sealed class NormalFile
{
    public int Add(int left, int right)
    {
        return left + right;
    }
}
""";

        var record = new GeneratorSpineQualityScanner().AnalyzeSourceText("src/NormalFile.cs", text);

        Assert.False(record.IsOneLineOrMinifiedCandidate);
        Assert.True(record.LineCount > 3);
    }

    [Fact]
    public void AbsolutePathDetectionCatchesWindowsAndUnixStylePaths()
    {
        var text = """
{
  "windows": "C:\\Users\\endim\\LLMGameCreator\\artifact.json",
  "unix": "/home/runner/work/artifact.json"
}
""";

        var records = new GeneratorSpineQualityScanner().DetectAbsolutePathLikeStrings("artifact.json", text);

        Assert.Contains(records, item => item.MatchKind == "windows_absolute_path_like");
        Assert.Contains(records, item => item.MatchKind == "unix_absolute_path_like");
    }

    [Fact]
    public void ProofQualityHeuristicCatchesReportOnlyShallowSmokeFixture()
    {
        var text = """
using Xunit;

public sealed class ShallowSmoke
{
    [Fact]
    public void Smoke()
    {
        Assert.True(result.Report.Passed);
        Assert.Equal("GREEN", result.Report.ImplementationStatus);
    }
}
""";

        var record = new GeneratorSpineQualityScanner().AnalyzeProductSmokeText("tests/ProductSmoke/ShallowSmoke.cs", text);

        Assert.True(record.ReportOnlyShallowCandidate);
        Assert.True(record.StrongAssertionSignalCount < 3);
    }
}

public sealed class GeneratorSpineQualityRiskClassifierTests
{
    [Fact]
    public void LargeFileRiskClassificationIsDeterministic()
    {
        var scan = new GeneratorSpineQualityScanResult
        {
            SourceFiles =
            [
                new SourceFileQualityRecord
                {
                    RelativePath = "src/LLMGameCreator.Application/Design/Fixture/Large.cs",
                    LineCount = 750,
                    MaxLineLength = 120,
                    IsLargeFileCandidate = true
                }
            ],
            CurrentStateConsistency = ConsistentState(),
            Goal071ProofIndicators = PassingGoal071Proof()
        };

        var classifier = new GeneratorSpineQualityRiskClassifier();
        var first = classifier.Classify(scan);
        var second = classifier.Classify(scan);

        Assert.Equal(
            first.Select(item => item.FindingId).ToArray(),
            second.Select(item => item.FindingId).ToArray());
        Assert.Contains(first, item => item.FindingId == "GQ-P1-LARGE-SOURCE-FILES" && item.Severity == "P1");
    }

    private static CurrentStateConsistencyRecord ConsistentState() =>
        new()
        {
            JsonParses = true,
            GateStatusMatchesGoal072 = true,
            ActiveManualGateMentionsGoal072Required = true,
            MarkdownMentionsGoal071Handoff = true,
            MarkdownMentionsGoal072Required = true,
            ContextIndexMentionsGoal072Required = true,
            GoalQueueMentionsGoal072Required = true
        };

    private static Goal071ProofQualityRecord PassingGoal071Proof() =>
        new()
        {
            ReportExists = true,
            CommandPlanExists = true,
            StagedCommandPlanExists = true,
            PlayerProofExists = true,
            TransitionLedgerExists = true,
            InputScriptExists = true,
            CommandPlanPassed = true,
            CommandPlanAcceptedFalse = true,
            CommandPlanRowCount = 9,
            ExpectedMarkerCount = 10,
            PlayerProofPassed = true,
            PlayerExecuted = true,
            ProvenRowCount = 9,
            MissingMarkerCount = 0,
            MatchedMarkerCount = 10,
            TransitionCount = 63,
            ActionCount = 63,
            ProofQualityPassed = true
        };
}

public sealed class GeneratorSpineQualityRepositoryTests
{
    [Fact]
    public void Goal071ProofIndicatorsAreRecognized()
    {
        var scan = new GeneratorSpineQualityScanner().ScanProject(ProjectRoot());

        Assert.True(scan.Goal071ProofIndicators.ReportExists);
        Assert.True(scan.Goal071ProofIndicators.CommandPlanExists);
        Assert.True(scan.Goal071ProofIndicators.StagedCommandPlanExists);
        Assert.True(scan.Goal071ProofIndicators.PlayerProofExists);
        Assert.True(scan.Goal071ProofIndicators.CommandPlanPassed);
        Assert.True(scan.Goal071ProofIndicators.PlayerProofPassed);
        Assert.Equal(9, scan.Goal071ProofIndicators.CommandPlanRowCount);
        Assert.Equal(9, scan.Goal071ProofIndicators.ProvenRowCount);
        Assert.Equal(0, scan.Goal071ProofIndicators.MissingMarkerCount);
        Assert.True(scan.Goal071ProofIndicators.ProofQualityPassed);
    }

    [Fact]
    public async Task EvidenceWriterProducesRequiredFiles()
    {
        var service = new GeneratorSpineQualityEvidenceService();
        var result = service.Build(ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal072Write", Guid.NewGuid().ToString("N"));

        try
        {
            var write = await service.WriteAsync(tempRoot, result);

            foreach (var fileName in GeneratorSpineQualityVocabulary.RequiredEvidenceFiles)
            {
                var path = Path.Combine(write.OutputDirectoryPath, fileName);
                Assert.True(File.Exists(path), "Missing artifact: " + fileName);
                if (Path.GetExtension(path) == ".json")
                {
                    using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
                }
            }

            Assert.True(File.Exists(write.DebtRegisterMarkdownPath));
            Assert.Contains("generator_spine_quality_consolidation_verification required", await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, GeneratorSpineQualityEvidenceService.ReportMarkdownFileName)));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void QualityDashboardContainsSeverityCountsAndRecommendedNextActions()
    {
        var result = new GeneratorSpineQualityEvidenceService().Build(ProjectRoot());

        Assert.True(result.QualityDashboard.P0Count >= 0);
        Assert.True(result.QualityDashboard.P1Count >= 0);
        Assert.True(result.QualityDashboard.P2Count >= 0);
        Assert.NotEmpty(result.QualityDashboard.RecommendedNextActions);
        Assert.False(string.IsNullOrWhiteSpace(result.QualityDashboard.InventoryHash));
        Assert.False(string.IsNullOrWhiteSpace(result.QualityDashboard.DebtRegisterHash));
    }

    private static string ProjectRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
