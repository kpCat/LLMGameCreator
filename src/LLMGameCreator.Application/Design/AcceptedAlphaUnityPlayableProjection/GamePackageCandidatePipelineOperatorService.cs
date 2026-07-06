using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class GamePackageCandidatePipelineOperatorService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public GamePackageCandidatePipelineOperatorDashboard BuildStatus(string repositoryRootPath) =>
        Build(repositoryRootPath).Dashboard;

    public GamePackageCandidatePipelineOperatorBuildResult Build(
        string repositoryRootPath,
        GamePackageCandidatePipelineOperatorRunResultInput? runInput = null)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var scriptScan = BuildScriptScan(root);
        var winFormsScan = BuildWinFormsScan(root);
        var status = ReadGoal131Status(root);
        var existingResult = ReadExistingOperatorResult(root);
        var operatorResult = BuildOperatorResult(status, runInput, existingResult);
        var negative = BuildNegativeProof(status);
        var dashboard = BuildDashboard(status, scriptScan, winFormsScan, operatorResult, negative);
        var report = RenderReport(dashboard, scriptScan, winFormsScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [GamePackageCandidatePipelineOperatorVocabulary.ResultFileName] =
                Serialize(operatorResult),
            [GamePackageCandidatePipelineOperatorVocabulary.ScriptScanFileName] =
                Serialize(scriptScan),
            [GamePackageCandidatePipelineOperatorVocabulary.WinFormsScanFileName] =
                Serialize(winFormsScan),
            [GamePackageCandidatePipelineOperatorVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [GamePackageCandidatePipelineOperatorVocabulary.ReportFileName] = report
        };
        var proceduralIndex = BuildFileIndex(
            root,
            GamePackageCandidatePipelineOperatorVocabulary.ProceduralOutputDirectory,
            proceduralFiles);
        proceduralFiles[GamePackageCandidatePipelineOperatorVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in proceduralFiles.Where(item =>
                     item.Key != GamePackageCandidatePipelineOperatorVocabulary.FileIndexFileName))
        {
            exportFiles[item.Key] = item.Value;
        }
        var exportIndex = BuildFileIndex(
            root,
            GamePackageCandidatePipelineOperatorVocabulary.ExportPackageDirectory,
            exportFiles);
        exportFiles[GamePackageCandidatePipelineOperatorVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new GamePackageCandidatePipelineOperatorBuildResult
        {
            Dashboard = dashboard,
            OperatorResult = operatorResult,
            ScriptScan = scriptScan,
            WinFormsScan = winFormsScan,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    public Task<GamePackageCandidatePipelineOperatorWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default) =>
        WriteAsync(repositoryRootPath, Build(repositoryRootPath), cancellationToken);

    public Task<GamePackageCandidatePipelineOperatorWriteResult> WriteOperatorResultAsync(
        string repositoryRootPath,
        GamePackageCandidatePipelineOperatorRunResultInput runInput,
        CancellationToken cancellationToken = default) =>
        WriteAsync(repositoryRootPath, Build(repositoryRootPath, runInput), cancellationToken);

    private static GamePackageCandidatePipelineOperatorDashboard BuildDashboard(
        Goal131Status status,
        GamePackageCandidatePipelineOperatorScriptScan scriptScan,
        GamePackageCandidatePipelineOperatorWinFormsScan winFormsScan,
        GamePackageCandidatePipelineOperatorRunResult operatorResult,
        GamePackageCandidatePipelineOperatorNegativeProof negative)
    {
        var diagnostics = new List<string>();
        Require(status.ResultExists, "goal132.goal131_result_missing", diagnostics);
        Require(status.SelectedHandoffExists, "goal132.selected_handoff_missing", diagnostics);
        Require(status.MatrixPassed, "goal132.matrix_not_green", diagnostics);
        Require(!string.IsNullOrWhiteSpace(status.SelectedCandidateId),
            "goal132.selected_candidate_missing", diagnostics);
        Require(scriptScan.Passed, "goal132.script_scan_failed", diagnostics);
        Require(winFormsScan.Passed, "goal132.winforms_scan_failed", diagnostics);
        Require(negative.Passed, "goal132.negative_proof_failed", diagnostics);
        Require(operatorResult.OperatorResultCaptured, "goal132.operator_result_missing", diagnostics);

        return new GamePackageCandidatePipelineOperatorDashboard
        {
            OperatorStatus = diagnostics.Count == 0 ? "GREEN_READY" : "BLOCKED",
            WinFormsPanelPresent = winFormsScan.WinFormsPanelPresent,
            RefreshButtonPresent = winFormsScan.RefreshButtonPresent,
            CopyCommandButtonPresent = winFormsScan.CopyCommandButtonPresent,
            DryRunButtonPresent = winFormsScan.DryRunButtonPresent,
            RunButtonPresent = winFormsScan.RunButtonPresent,
            AsyncRunPresent = winFormsScan.AsyncRunPresent,
            ResultPath = GamePackageCandidatePipelineOperatorVocabulary.Goal131ResultPath,
            SelectedCandidateHandoffPath =
                GamePackageCandidatePipelineOperatorVocabulary.Goal131SelectedHandoffPath,
            SelectedCandidateId = status.SelectedCandidateId,
            SelectedCandidateScore = status.SelectedCandidateScore,
            CandidateCount = status.CandidateCount,
            PassedCandidates = status.PassedCandidates,
            FailedCandidates = status.FailedCandidates,
            MatrixPassed = status.MatrixPassed,
            LastOperatorExitCode = operatorResult.ExitCode,
            LastOperatorDurationMilliseconds = operatorResult.DurationMilliseconds,
            LastOperatorRunMode = operatorResult.RunMode,
            OutputTail = operatorResult.OutputTail,
            ManualUnityOptional = status.ManualUnityOptional,
            ProjectionOnly = status.ProjectionOnly,
            SamplePackageReadOnly = status.SamplePackageReadOnly,
            OperatorResultPresent = operatorResult.OperatorResultCaptured,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static GamePackageCandidatePipelineOperatorRunResult BuildOperatorResult(
        Goal131Status status,
        GamePackageCandidatePipelineOperatorRunResultInput? runInput,
        GamePackageCandidatePipelineOperatorRunResult? existingResult)
    {
        if (runInput is null && existingResult is not null)
        {
            return existingResult with
            {
                ResultPath = GamePackageCandidatePipelineOperatorVocabulary.Goal131ResultPath,
                SelectedCandidateId = status.SelectedCandidateId,
                SelectedCandidateScore = status.SelectedCandidateScore,
                CandidateCount = status.CandidateCount,
                PassedCandidates = status.PassedCandidates,
                FailedCandidates = status.FailedCandidates,
                MatrixPassed = status.MatrixPassed,
                ManualUnityOptional = status.ManualUnityOptional,
                ProjectionOnly = status.ProjectionOnly,
                SamplePackageReadOnly = status.SamplePackageReadOnly
            };
        }

        var input = runInput ?? new GamePackageCandidatePipelineOperatorRunResultInput
        {
            RunMode = "status_refresh",
            Command = GamePackageCandidatePipelineOperatorVocabulary.NormalCommand,
            ExitCode = -1,
            DurationMilliseconds = 0,
            OutputTail = "No operator run has been captured yet.",
            ErrorTail = string.Empty
        };

        return new GamePackageCandidatePipelineOperatorRunResult
        {
            OperatorResultCaptured = true,
            RunMode = input.RunMode,
            Command = string.IsNullOrWhiteSpace(input.Command)
                ? GamePackageCandidatePipelineOperatorVocabulary.NormalCommand
                : input.Command,
            ExitCode = input.ExitCode,
            DurationMilliseconds = input.DurationMilliseconds,
            OutputTail = NormalizeTail(input.OutputTail),
            ErrorTail = NormalizeTail(input.ErrorTail),
            SelectedCandidateId = status.SelectedCandidateId,
            SelectedCandidateScore = status.SelectedCandidateScore,
            CandidateCount = status.CandidateCount,
            PassedCandidates = status.PassedCandidates,
            FailedCandidates = status.FailedCandidates,
            MatrixPassed = status.MatrixPassed,
            ManualUnityOptional = status.ManualUnityOptional,
            ProjectionOnly = status.ProjectionOnly,
            SamplePackageReadOnly = status.SamplePackageReadOnly
        };
    }

    private static GamePackageCandidatePipelineOperatorScriptScan BuildScriptScan(string root)
    {
        var scriptPath = Resolve(root, GamePackageCandidatePipelineOperatorVocabulary.PipelineScriptPath);
        var cmdPath = Resolve(root, GamePackageCandidatePipelineOperatorVocabulary.PipelineCmdPath);
        var scriptExists = File.Exists(scriptPath);
        var cmdExists = File.Exists(cmdPath);
        var script = scriptExists ? File.ReadAllText(scriptPath, Encoding.UTF8) : string.Empty;
        var cmd = cmdExists ? File.ReadAllText(cmdPath, Encoding.UTF8) : string.Empty;
        var scan = new GamePackageCandidatePipelineOperatorScriptScan
        {
            PipelineScriptExists = scriptExists,
            PipelineCmdExists = cmdExists,
            SupportsDryRun = script.Contains("[switch]$DryRun", StringComparison.Ordinal),
            SupportsApplyCleanup = script.Contains("[switch]$ApplyCleanup", StringComparison.Ordinal),
            NormalCommandUsesCmdWrapper =
                GamePackageCandidatePipelineOperatorVocabulary.NormalCommand
                == ".devflow\\scripts\\run-gamepackage-candidate-recipe-pipeline.cmd"
                && cmd.Contains("run-gamepackage-candidate-recipe-pipeline.ps1",
                    StringComparison.Ordinal),
            DryRunCommandUsesScriptDryRun =
                GamePackageCandidatePipelineOperatorVocabulary.DryRunCommand.Contains(
                    "-DryRun",
                    StringComparison.Ordinal)
                && GamePackageCandidatePipelineOperatorVocabulary.DryRunCommand.Contains(
                    "run-gamepackage-candidate-recipe-pipeline.ps1",
                    StringComparison.Ordinal),
            FullRunCommandUsesApplyCleanup =
                GamePackageCandidatePipelineOperatorVocabulary.FullRunCommand.Contains(
                    "-ApplyCleanup",
                    StringComparison.Ordinal),
            RejectsManualInputRoot = script.Contains(".llmgc/manual/", StringComparison.Ordinal),
            NoLlmProviderNetwork =
                !script.Contains("Invoke-WebRequest", StringComparison.OrdinalIgnoreCase)
                && !script.Contains("curl ", StringComparison.OrdinalIgnoreCase)
                && !script.Contains("ComfyUI", StringComparison.OrdinalIgnoreCase),
            NoBroadGitClean =
                !script.Contains("git clean", StringComparison.OrdinalIgnoreCase)
                && !cmd.Contains("git clean", StringComparison.OrdinalIgnoreCase)
        };

        return scan with
        {
            Passed = scan.PipelineScriptExists
                     && scan.PipelineCmdExists
                     && scan.SupportsDryRun
                     && scan.SupportsApplyCleanup
                     && scan.NormalCommandUsesCmdWrapper
                     && scan.DryRunCommandUsesScriptDryRun
                     && scan.FullRunCommandUsesApplyCleanup
                     && scan.RejectsManualInputRoot
                     && scan.NoLlmProviderNetwork
                     && scan.NoBroadGitClean
        };
    }

    private static GamePackageCandidatePipelineOperatorWinFormsScan BuildWinFormsScan(string root)
    {
        const string partialPath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal132.cs";
        var fullPath = Resolve(root, partialPath);
        var exists = File.Exists(fullPath);
        var text = exists ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        var scan = new GamePackageCandidatePipelineOperatorWinFormsScan
        {
            PanelPartialExists = exists,
            WinFormsPanelPresent =
                text.Contains("Goal132 Candidate Pipeline Operator", StringComparison.Ordinal),
            RefreshButtonPresent =
                text.Contains("Refresh Candidate Pipeline Status", StringComparison.Ordinal),
            CopyCommandButtonPresent =
                text.Contains("Copy Candidate Pipeline Command", StringComparison.Ordinal),
            DryRunButtonPresent =
                text.Contains("Dry Run Candidate Recipe Pipeline", StringComparison.Ordinal),
            RunButtonPresent =
                text.Contains("Run Candidate Recipe Pipeline", StringComparison.Ordinal),
            AsyncRunPresent =
                text.Contains("WaitForExitAsync", StringComparison.Ordinal)
                && text.Contains("async", StringComparison.Ordinal),
            MarshalUiUpdatesPresent =
                text.Contains("BeginInvoke", StringComparison.Ordinal)
                || text.Contains("InvokeRequired", StringComparison.Ordinal)
                || text.Contains("await", StringComparison.Ordinal),
            UsesApplicationOperatorService =
                text.Contains("GamePackageCandidatePipelineOperatorService", StringComparison.Ordinal),
            ShowsOutputTail = text.Contains("OutputTail", StringComparison.Ordinal)
        };

        return scan with
        {
            Passed = scan.PanelPartialExists
                     && scan.WinFormsPanelPresent
                     && scan.RefreshButtonPresent
                     && scan.CopyCommandButtonPresent
                     && scan.DryRunButtonPresent
                     && scan.RunButtonPresent
                     && scan.AsyncRunPresent
                     && scan.MarshalUiUpdatesPresent
                     && scan.UsesApplicationOperatorService
                     && scan.ShowsOutputTail
                     && scan.NoDesignerChangeRequired
        };
    }

    private static GamePackageCandidatePipelineOperatorNegativeProof BuildNegativeProof(
        Goal131Status status)
    {
        var proof = new GamePackageCandidatePipelineOperatorNegativeProof
        {
            ManualUnityOptional = status.ManualUnityOptional,
            ProjectionOnly = status.ProjectionOnly,
            SamplePackageReadOnly = status.SamplePackageReadOnly
        };

        return proof with
        {
            Passed = proof.ManualUnityOptional
                     && proof.ProjectionOnly
                     && proof.SamplePackageReadOnly
                     && proof.DoesNotWriteSamplePackage
                     && proof.RuntimeSchemaProviderLuaGeneratorLibraryUnchanged
                     && proof.UnityAssetsProjectSettingsPackagesUnchanged
                     && proof.ExistingDevflowRunnerScriptsReadOnly
                     && proof.NoManualInputArtifacts
        };
    }

    private static Goal131Status ReadGoal131Status(string root)
    {
        var resultPath = Resolve(root, GamePackageCandidatePipelineOperatorVocabulary.Goal131ResultPath);
        var handoffPath = Resolve(root, GamePackageCandidatePipelineOperatorVocabulary.Goal131SelectedHandoffPath);
        using var result = File.Exists(resultPath)
            ? JsonDocument.Parse(File.ReadAllText(resultPath, Encoding.UTF8))
            : null;
        using var handoff = File.Exists(handoffPath)
            ? JsonDocument.Parse(File.ReadAllText(handoffPath, Encoding.UTF8))
            : null;
        var resultElement = result?.RootElement;
        var handoffElement = handoff?.RootElement;
        var selectedId = StringValue(resultElement, "selectedCandidateId");
        if (string.IsNullOrWhiteSpace(selectedId))
        {
            selectedId = StringValue(handoffElement, "selectedCandidateId");
        }

        var selectedScore = IntValue(resultElement, "selectedCandidateScore");
        if (selectedScore == 0)
        {
            selectedScore = IntValue(handoffElement, "selectedCandidateScore");
        }

        return new Goal131Status(
            ResultExists: result is not null,
            SelectedHandoffExists: handoff is not null,
            CandidateCount: IntValue(resultElement, "candidateCount"),
            PassedCandidates: IntValue(resultElement, "passedCandidates"),
            FailedCandidates: IntValue(resultElement, "failedCandidates"),
            MatrixPassed: BoolValue(resultElement, "matrixPassed"),
            SelectedCandidateId: selectedId,
            SelectedCandidateScore: selectedScore,
            ManualUnityOptional:
                BoolValue(resultElement, "manualUnityOptional")
                || BoolValue(handoffElement, "manualUnityOptional"),
            ProjectionOnly:
                BoolValue(resultElement, "projectionOnly")
                || BoolValue(handoffElement, "projectionOnly"),
            SamplePackageReadOnly:
                BoolValue(resultElement, "samplePackageUnmodified")
                || BoolValue(handoffElement, "samplePackageUnmodified"));
    }

    private static GamePackageCandidatePipelineOperatorRunResult? ReadExistingOperatorResult(string root)
    {
        var path = Resolve(root, GamePackageCandidatePipelineOperatorVocabulary.ResultRelativePath);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<GamePackageCandidatePipelineOperatorRunResult>(
                File.ReadAllText(path, Encoding.UTF8),
                JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static async Task<GamePackageCandidatePipelineOperatorWriteResult> WriteAsync(
        string repositoryRootPath,
        GamePackageCandidatePipelineOperatorBuildResult result,
        CancellationToken cancellationToken)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var procedural = Resolve(root, GamePackageCandidatePipelineOperatorVocabulary.ProceduralOutputDirectory);
        var export = Resolve(root, GamePackageCandidatePipelineOperatorVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, GamePackageCandidatePipelineOperatorVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in result.ProceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.ExportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new GamePackageCandidatePipelineOperatorWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static GamePackageCandidatePipelineOperatorFileIndex BuildFileIndex(
        string root,
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var entries = pendingTextFiles.Select(item =>
            new GamePackageCandidatePipelineOperatorFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal132_candidate_pipeline_operator_"
                       + Path.GetFileNameWithoutExtension(item.Key),
                Sha256 = HashText(item.Value)
            }).ToList();
        var ordered = entries
            .OrderBy(entry => entry.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new GamePackageCandidatePipelineOperatorFileIndex
        {
            RootPath = relativeRoot,
            IndexedFileCount = ordered.Count,
            ManualInputExcluded = ordered.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = ordered
        };
    }

    private static string RenderReport(
        GamePackageCandidatePipelineOperatorDashboard dashboard,
        GamePackageCandidatePipelineOperatorScriptScan scriptScan,
        GamePackageCandidatePipelineOperatorWinFormsScan winFormsScan,
        GamePackageCandidatePipelineOperatorNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 132 WinForms Candidate Pipeline Operator Panel",
            string.Empty,
            "- operatorStatus: " + dashboard.OperatorStatus,
            "- normalCommand: " + dashboard.NormalCommand,
            "- dryRunCommand: " + dashboard.DryRunCommand,
            "- resultPath: " + dashboard.ResultPath,
            "- selectedCandidateId: " + dashboard.SelectedCandidateId,
            "- selectedCandidateScore: " + dashboard.SelectedCandidateScore,
            "- candidateCount: " + dashboard.CandidateCount,
            "- passedCandidates: " + dashboard.PassedCandidates,
            "- failedCandidates: " + dashboard.FailedCandidates,
            "- matrixPassed: " + dashboard.MatrixPassed.ToString().ToLowerInvariant(),
            "- lastOperatorExitCode: " + dashboard.LastOperatorExitCode,
            "- lastOperatorDurationMilliseconds: " + dashboard.LastOperatorDurationMilliseconds,
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant(),
            "- projectionOnly: " + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            "- samplePackageReadOnly: " + dashboard.SamplePackageReadOnly.ToString().ToLowerInvariant(),
            string.Empty,
            "## Scans",
            string.Empty,
            "- scriptScanPassed: " + scriptScan.Passed.ToString().ToLowerInvariant(),
            "- winFormsPanelPresent: " + winFormsScan.WinFormsPanelPresent.ToString().ToLowerInvariant(),
            "- refreshButtonPresent: " + winFormsScan.RefreshButtonPresent.ToString().ToLowerInvariant(),
            "- copyCommandButtonPresent: " + winFormsScan.CopyCommandButtonPresent.ToString().ToLowerInvariant(),
            "- dryRunButtonPresent: " + winFormsScan.DryRunButtonPresent.ToString().ToLowerInvariant(),
            "- runButtonPresent: " + winFormsScan.RunButtonPresent.ToString().ToLowerInvariant(),
            "- asyncRunPresent: " + winFormsScan.AsyncRunPresent.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + negative.Passed.ToString().ToLowerInvariant()
        };
        if (dashboard.Diagnostics.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Diagnostics");
            lines.AddRange(dashboard.Diagnostics.Select(item => "- " + item));
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderDocumentation(GamePackageCandidatePipelineOperatorDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# WinForms Candidate Pipeline Operator Panel",
            string.Empty,
            "Goal132 adds a WinForms operator panel for the existing Goal131 GamePackage candidate recipe pipeline. The panel surfaces status, command paths and selected-candidate proof, and can run dry-run or cleanup pipeline commands asynchronously.",
            string.Empty,
            "## Normal Command",
            string.Empty,
            "- `" + dashboard.NormalCommand + "`",
            string.Empty,
            "## Current Status",
            string.Empty,
            "- operatorStatus: " + dashboard.OperatorStatus,
            "- resultPath: " + dashboard.ResultPath,
            "- selectedCandidateId: " + dashboard.SelectedCandidateId,
            "- selectedCandidateScore: " + dashboard.SelectedCandidateScore,
            "- candidateCount: " + dashboard.CandidateCount,
            "- passedCandidates: " + dashboard.PassedCandidates,
            "- failedCandidates: " + dashboard.FailedCandidates,
            "- matrixPassed: " + dashboard.MatrixPassed.ToString().ToLowerInvariant(),
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant(),
            string.Empty,
            "## Scope Guard",
            string.Empty,
            "- Manual Unity inspection remains optional.",
            "- The sample package stays read-only.",
            "- Runtime, public schema, provider, Lua, generator-library, final art, Unity Assets, StreamingAssets, ProjectSettings, Packages and release packaging remain outside this goal."
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string NormalizeTail(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var lines = value.Replace("\r\n", "\n").Replace('\r', '\n')
            .Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .TakeLast(80);
        return string.Join(Environment.NewLine, lines);
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static string ResolveRepositoryRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Repository root path is required.", nameof(path));
        }

        return Path.GetFullPath(path);
    }

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)
                                  ?? throw new InvalidOperationException("Missing directory."));
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal132 must not write the manual input path.");
        }
    }

    private static void Require(bool condition, string code, List<string> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(code);
        }
    }

    private static string StringValue(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool BoolValue(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.True;

    private static int IntValue(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed record Goal131Status(
        bool ResultExists,
        bool SelectedHandoffExists,
        int CandidateCount,
        int PassedCandidates,
        int FailedCandidates,
        bool MatrixPassed,
        string SelectedCandidateId,
        int SelectedCandidateScore,
        bool ManualUnityOptional,
        bool ProjectionOnly,
        bool SamplePackageReadOnly);
}
