using System.Diagnostics;

namespace LLMGameCreator.Application.Design.UnityAlphaInteractiveCampaignPlayer;

public sealed class UnityAlphaInteractiveCampaignUnityProofRunner
{
    public static InteractiveCampaignUnityProof NotRequested(InteractiveCampaignUnityCommandPlan commandPlan) =>
        Blocked(
            "goal071.unity.not_requested",
            "Unity execution was not requested for this validation pass.",
            Summary(
                [InteractiveCampaignDiagnostic.Warning("goal071.unity.not_requested", "unity_cli", "Unity build/player proof was not requested.")],
                unityEditorExecuted: false,
                playerExecuted: false,
                commandPlan: commandPlan));

    public InteractiveCampaignUnityProof Run(
        string projectRootPath,
        string outputDirectoryPath,
        string stagingDirectoryPath,
        InteractiveCampaignUnityCommandPlan commandPlan,
        UnityAlphaInteractiveCampaignOptions options)
    {
        var outputDirectory = Path.GetFullPath(outputDirectoryPath);
        var stagingDirectory = Path.GetFullPath(stagingDirectoryPath);
        var repositoryRoot = string.IsNullOrWhiteSpace(options.RepositoryRootPath)
            ? Path.GetFullPath(projectRootPath)
            : Path.GetFullPath(options.RepositoryRootPath);

        if (!options.ExecuteUnityProof)
        {
            return NotRequested(commandPlan);
        }

        var diagnostics = new List<InteractiveCampaignDiagnostic>();
        var unityProject = Path.Combine(repositoryRoot, "unity", "LLMGameCreatorAlpha");
        var entrypoint = Path.Combine(unityProject, "Assets", "Editor", "AlphaBuildEntrypoint.cs");
        if (!Directory.Exists(unityProject) || !File.Exists(entrypoint))
        {
            return Blocked(
                "goal071.unity.project_missing",
                "Repository-local Unity Alpha project or build entrypoint was not found.",
                Summary([Error("goal071.unity.project_missing", "unity/LLMGameCreatorAlpha", "Unity Alpha project and build entrypoint are required.")], false, false, commandPlan: commandPlan));
        }

        var unityExecutable = FindUnityExecutable();
        if (string.IsNullOrWhiteSpace(unityExecutable))
        {
            return Blocked(
                "goal071.unity.unity_not_found",
                "Unity Editor executable was not found in PATH or standard Unity Hub locations.",
                Summary([Error("goal071.unity.unity_not_found", "unity_cli", "Unity Editor executable was not found.")], false, false, commandPlan: commandPlan));
        }

        var logsRoot = Path.Combine(outputDirectory, "logs");
        var buildRoot = Path.Combine(outputDirectory, "build", "b");
        var workRoot = Path.Combine(outputDirectory, "unity-work");
        var workProject = Path.Combine(workRoot, "LLMGameCreatorAlpha");
        var unityBuildLog = Path.Combine(logsRoot, "unity-build.log");
        var launchLog = Path.Combine(logsRoot, "alpha-player-launch.log");
        var playLoopLog = Path.Combine(logsRoot, "alpha-player-play-loop.log");
        Directory.CreateDirectory(logsRoot);
        ResetDirectory(buildRoot);
        ResetDirectory(workRoot);
        CopyUnityTemplate(unityProject, workProject);

        var unityArgs = new List<string>
        {
            "-batchmode",
            "-quit",
            "-projectPath",
            workProject,
            "-executeMethod",
            "LLMGameCreatorAlpha.Editor.AlphaBuildEntrypoint.BuildWindows64",
            "-logFile",
            unityBuildLog,
            "-alphaStagingPath",
            stagingDirectory,
            "-alphaBuildOutputPath",
            buildRoot
        };

        var unityResult = RunProcess(unityExecutable, unityArgs, outputDirectory, Math.Max(1, options.UnityBuildTimeoutSeconds));
        diagnostics.Add(Info("goal071.unity.editor_executed", "logs/unity-build.log", "Unity Editor was invoked through the existing Alpha build entrypoint."));
        diagnostics.Add(unityResult.ExitCode == 0
            ? Info("goal071.unity.editor_exit_success", "exit_code:0", "Unity Editor build process exited successfully.")
            : Error("goal071.unity.editor_exit_failure", "exit_code:" + unityResult.ExitCode, "Unity Editor build process did not exit successfully."));

        if (unityResult.ExitCode != 0)
        {
            Cleanup(workRoot, options);
            return Blocked(
                "goal071.unity.editor_exit_failure",
                "Unity Editor build process failed; see logs/unity-build.log.",
                Summary(diagnostics, unityEditorExecuted: true, playerExecuted: false, unityResult.ExitCode, null, unityBuildLog, launchLog, playLoopLog, commandPlan));
        }

        var executable = Path.Combine(buildRoot, "LLMGameCreatorAlpha.exe");
        if (!File.Exists(executable))
        {
            diagnostics.Add(Error("goal071.unity.executable_missing", "build/b/LLMGameCreatorAlpha.exe", "Unity build did not produce the Alpha executable."));
            Cleanup(workRoot, options);
            return Blocked(
                "goal071.unity.executable_missing",
                "Unity build completed but did not produce LLMGameCreatorAlpha.exe.",
                Summary(diagnostics, unityEditorExecuted: true, playerExecuted: false, unityResult.ExitCode, null, unityBuildLog, launchLog, playLoopLog, commandPlan));
        }

        var playerArgs = new List<string>
        {
            "-batchmode",
            "-nographics",
            "-alphaSmokeExit",
            "-alphaPlayLoopSmokeExit",
            "-alphaFamilyMode",
            "all",
            "-alphaLogPath",
            launchLog,
            "-alphaPlayLoopLogPath",
            playLoopLog
        };
        var playerResult = RunProcess(executable, playerArgs, buildRoot, Math.Max(1, options.PlayerLaunchTimeoutSeconds));
        diagnostics.Add(Info("goal071.unity.player_executed", "logs/alpha-player-play-loop.log", "The produced Alpha player was launched in interactive campaign marker mode."));
        diagnostics.Add(playerResult.ExitCode == 0
            ? Info("goal071.unity.player_exit_success", "exit_code:0", "Alpha player process exited successfully.")
            : Error("goal071.unity.player_exit_failure", "exit_code:" + playerResult.ExitCode, "Alpha player process did not exit successfully."));

        var summary = Summary(diagnostics, unityEditorExecuted: true, playerExecuted: true, unityResult.ExitCode, playerResult.ExitCode, unityBuildLog, launchLog, playLoopLog, commandPlan);
        var passed = playerResult.ExitCode == 0 && summary.MissingMarkers.Count == 0 && summary.ProvenRowCount == 9;
        Cleanup(workRoot, options);

        return new InteractiveCampaignUnityProof
        {
            Passed = passed,
            UnityEditorOrPlayerExecuted = true,
            BlockerCode = passed ? string.Empty : "goal071.unity.marker_or_player_failure",
            BlockerMessage = passed ? string.Empty : "Unity player did not produce all required Goal 071 interactive markers or exited with failure.",
            PlayerProof = summary with { Passed = passed },
            Diagnostics = SortDiagnostics(diagnostics.Concat(summary.Diagnostics))
        };
    }

    private static InteractiveCampaignUnityProof Blocked(string blockerCode, string blockerMessage, InteractiveCampaignUnityProofSummary summary) =>
        new()
        {
            Passed = false,
            UnityEditorOrPlayerExecuted = summary.UnityEditorExecuted || summary.PlayerExecuted,
            BlockerCode = blockerCode,
            BlockerMessage = blockerMessage,
            PlayerProof = summary,
            Diagnostics = SortDiagnostics(summary.Diagnostics.Concat([Warning(blockerCode, "unity-proof", blockerMessage)]))
        };

    private static InteractiveCampaignUnityProofSummary Summary(
        IEnumerable<InteractiveCampaignDiagnostic> diagnostics,
        bool unityEditorExecuted,
        bool playerExecuted,
        int? unityExitCode = null,
        int? playerExitCode = null,
        string unityBuildLog = "",
        string launchLog = "",
        string playLoopLog = "",
        InteractiveCampaignUnityCommandPlan? commandPlan = null)
    {
        var required = commandPlan?.ExpectedPlayerMarkers ?? [];
        var lines = ReadLines(launchLog).Concat(ReadLines(playLoopLog)).ToList();
        var matched = required.Where(marker => lines.Contains(marker, StringComparer.Ordinal)).OrderBy(marker => marker, StringComparer.Ordinal).ToList();
        var missing = required.Where(marker => !matched.Contains(marker, StringComparer.Ordinal)).OrderBy(marker => marker, StringComparer.Ordinal).ToList();
        var summaryDiagnostics = diagnostics.ToList();
        foreach (var marker in missing)
        {
            summaryDiagnostics.Add(playerExecuted
                ? Error("goal071.unity.marker_missing", marker, "Unity player logs did not contain the required Goal 071 marker.")
                : Warning("goal071.unity.marker_not_checked", marker, "Unity player was not executed, so the Goal 071 marker could not be checked."));
        }

        return new InteractiveCampaignUnityProofSummary
        {
            Passed = missing.Count == 0 && playerExitCode == 0,
            UnityEditorExecuted = unityEditorExecuted,
            PlayerExecuted = playerExecuted,
            UnityExitCode = unityExitCode,
            PlayerExitCode = playerExitCode,
            UnityBuildLogRelativePath = RelativeGoalPath(unityBuildLog),
            LaunchLogRelativePath = RelativeGoalPath(launchLog),
            PlayLoopLogRelativePath = RelativeGoalPath(playLoopLog),
            ProvenRowCount = commandPlan?.Rows.Count(row => lines.Contains("interactive_campaign_row_completed=" + row.RowId, StringComparer.Ordinal)) ?? 0,
            MatchedMarkers = matched,
            MissingMarkers = missing,
            Diagnostics = SortDiagnostics(summaryDiagnostics)
        };
    }

    private static string FindUnityExecutable()
    {
        var pathEntries = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var entry in pathEntries)
        {
            var candidate = Path.Combine(entry, "Unity.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        var hubRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Unity", "Hub", "Editor");
        if (Directory.Exists(hubRoot))
        {
            var candidate = Directory.EnumerateDirectories(hubRoot)
                .OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
                .Select(version => Path.Combine(version, "Editor", "Unity.exe"))
                .FirstOrDefault(File.Exists);
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }
        }

        var legacy = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Unity", "Editor", "Unity.exe");
        return File.Exists(legacy) ? legacy : string.Empty;
    }

    private static ProcessResult RunProcess(string fileName, IReadOnlyList<string> arguments, string workingDirectory, int timeoutSeconds)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
        if (!process.WaitForExit(TimeSpan.FromSeconds(timeoutSeconds)))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            return new ProcessResult(-1000);
        }

        return new ProcessResult(process.ExitCode);
    }

    private static void CopyUnityTemplate(string sourceRoot, string targetRoot)
    {
        foreach (var directory in Directory.EnumerateDirectories(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, directory);
            if (ShouldSkip(relative))
            {
                continue;
            }

            Directory.CreateDirectory(Path.Combine(targetRoot, relative));
        }

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, file);
            if (ShouldSkip(relative))
            {
                continue;
            }

            var target = Path.Combine(targetRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }

    private static bool ShouldSkip(string relativePath)
    {
        var parts = relativePath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Any(part => part is "Library" or "Temp" or "Logs" or "Obj" or "Build" or "Builds" or ".vs");
    }

    private static void ResetDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        Directory.CreateDirectory(directory);
    }

    private static void Cleanup(string workRoot, UnityAlphaInteractiveCampaignOptions options)
    {
        if (!options.CleanupUnityWorkProject || !Directory.Exists(workRoot))
        {
            return;
        }

        try
        {
            Directory.Delete(workRoot, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static IReadOnlyList<string> ReadLines(string path) =>
        string.IsNullOrWhiteSpace(path) || !File.Exists(path)
            ? []
            : File.ReadAllLines(path);

    private static string RelativeGoalPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var normalized = path.Replace('\\', '/');
        const string marker = "goal-071-unity-alpha-interactive-campaign-player/";
        var index = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return index < 0
            ? Path.GetFileName(path)
            : ".llmgc/procedural/" + normalized[index..];
    }

    private static IReadOnlyList<InteractiveCampaignDiagnostic> SortDiagnostics(IEnumerable<InteractiveCampaignDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            _ => 2
        };

    private static InteractiveCampaignDiagnostic Info(string code, string target, string message) =>
        InteractiveCampaignDiagnostic.Info(code, target, message);

    private static InteractiveCampaignDiagnostic Warning(string code, string target, string message) =>
        InteractiveCampaignDiagnostic.Warning(code, target, message);

    private static InteractiveCampaignDiagnostic Error(string code, string target, string message) =>
        InteractiveCampaignDiagnostic.Error(code, target, message);

    private sealed record ProcessResult(int ExitCode);
}
