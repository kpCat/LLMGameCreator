using System.Diagnostics;

namespace LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;

public sealed class FullMediaBoundUnityProofRunner
{
    public FullMediaBoundCampaignUnityProof Run(
        string projectRootPath,
        string outputDirectoryPath,
        string stagingDirectoryPath,
        FullMediaBoundUnityCampaignCommandPlan commandPlan,
        FullMediaBoundGeneratorCampaignOptions options)
    {
        var outputDirectory = Path.GetFullPath(outputDirectoryPath);
        var stagingDirectory = Path.GetFullPath(stagingDirectoryPath);
        var repositoryRoot = string.IsNullOrWhiteSpace(options.RepositoryRootPath)
            ? Path.GetFullPath(projectRootPath)
            : Path.GetFullPath(options.RepositoryRootPath);

        if (!options.ExecuteUnityProof)
        {
            return Blocked(
                "goal058.unity.not_requested",
                "Unity execution was not requested for this validation pass.",
                Summary(
                    [Warning("goal058.unity.not_requested", "unity_cli", "Unity build/player proof was not requested.")],
                    unityEditorExecuted: false,
                    playerExecuted: false));
        }

        var diagnostics = new List<FullMediaBoundCampaignDiagnostic>();
        var unityProject = Path.Combine(repositoryRoot, "unity", "LLMGameCreatorAlpha");
        var entrypoint = Path.Combine(unityProject, "Assets", "Editor", "AlphaBuildEntrypoint.cs");
        if (!Directory.Exists(unityProject) || !File.Exists(entrypoint))
        {
            return Blocked(
                "goal058.unity.project_missing",
                "Repository-local Unity Alpha project or build entrypoint was not found.",
                Summary([Error("goal058.unity.project_missing", "unity/LLMGameCreatorAlpha", "Unity Alpha project and build entrypoint are required.")], false, false));
        }

        var unityExecutable = FindUnityExecutable();
        if (string.IsNullOrWhiteSpace(unityExecutable))
        {
            return Blocked(
                "goal058.unity.unity_not_found",
                "Unity Editor executable was not found in PATH or standard Unity Hub locations.",
                Summary([Error("goal058.unity.unity_not_found", "unity_cli", "Unity Editor executable was not found.")], false, false));
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
        diagnostics.Add(Info("goal058.unity.editor_executed", "logs/unity-build.log", "Unity Editor was invoked through the existing Alpha build entrypoint."));
        diagnostics.Add(unityResult.ExitCode == 0
            ? Info("goal058.unity.editor_exit_success", "exit_code:0", "Unity Editor build process exited successfully.")
            : Error("goal058.unity.editor_exit_failure", "exit_code:" + unityResult.ExitCode, "Unity Editor build process did not exit successfully."));

        if (unityResult.ExitCode != 0)
        {
            Cleanup(workRoot, options);
            return Blocked(
                "goal058.unity.editor_exit_failure",
                "Unity Editor build process failed; see logs/unity-build.log.",
                Summary(diagnostics, unityEditorExecuted: true, playerExecuted: false, unityResult.ExitCode, null, unityBuildLog, launchLog, playLoopLog, commandPlan));
        }

        var executable = Path.Combine(buildRoot, "LLMGameCreatorAlpha.exe");
        if (!File.Exists(executable))
        {
            diagnostics.Add(Error("goal058.unity.executable_missing", "build/b/LLMGameCreatorAlpha.exe", "Unity build did not produce the Alpha executable."));
            Cleanup(workRoot, options);
            return Blocked(
                "goal058.unity.executable_missing",
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
        diagnostics.Add(Info("goal058.unity.player_executed", "logs/alpha-player-play-loop.log", "The produced Alpha player was launched in campaign diagnostic play-loop mode."));
        diagnostics.Add(playerResult.ExitCode == 0
            ? Info("goal058.unity.player_exit_success", "exit_code:0", "Alpha player process exited successfully.")
            : Error("goal058.unity.player_exit_failure", "exit_code:" + playerResult.ExitCode, "Alpha player process did not exit successfully."));

        var summary = Summary(diagnostics, unityEditorExecuted: true, playerExecuted: true, unityResult.ExitCode, playerResult.ExitCode, unityBuildLog, launchLog, playLoopLog, commandPlan);
        var passed = playerResult.ExitCode == 0 && summary.MissingMarkers.Count == 0;
        Cleanup(workRoot, options);

        return new FullMediaBoundCampaignUnityProof
        {
            Passed = passed,
            UnityEditorOrPlayerExecuted = true,
            BlockerCode = passed ? string.Empty : "goal058.unity.marker_or_player_failure",
            BlockerMessage = passed ? string.Empty : "Unity player did not produce all required Goal 058 markers or exited with failure.",
            PlayerProof = summary with { Passed = passed },
            Diagnostics = SortDiagnostics(diagnostics.Concat(summary.Diagnostics))
        };
    }

    private static FullMediaBoundCampaignUnityProof Blocked(
        string blockerCode,
        string blockerMessage,
        FullMediaBoundCampaignPlayerProof summary) =>
        new()
        {
            Passed = false,
            UnityEditorOrPlayerExecuted = summary.UnityEditorExecuted || summary.PlayerExecuted,
            BlockerCode = blockerCode,
            BlockerMessage = blockerMessage,
            PlayerProof = summary,
            Diagnostics = SortDiagnostics(summary.Diagnostics.Concat([Warning(blockerCode, "unity-proof", blockerMessage)]))
        };

    private static FullMediaBoundCampaignPlayerProof Summary(
        IEnumerable<FullMediaBoundCampaignDiagnostic> diagnostics,
        bool unityEditorExecuted,
        bool playerExecuted,
        int? unityExitCode = null,
        int? playerExitCode = null,
        string unityBuildLog = "",
        string launchLog = "",
        string playLoopLog = "",
        FullMediaBoundUnityCampaignCommandPlan? commandPlan = null)
    {
        var required = commandPlan?.ExpectedPlayerMarkers ?? [];
        var lines = ReadLines(launchLog).Concat(ReadLines(playLoopLog)).ToList();
        var matched = required.Where(marker => lines.Contains(marker, StringComparer.Ordinal)).Order(StringComparer.Ordinal).ToList();
        var missing = required.Where(marker => !matched.Contains(marker, StringComparer.Ordinal)).Order(StringComparer.Ordinal).ToList();
        var summaryDiagnostics = diagnostics.ToList();
        foreach (var marker in missing)
        {
            summaryDiagnostics.Add(playerExecuted
                ? Error("goal058.unity.marker_missing", marker, "Unity player logs did not contain the required Goal 058 marker.")
                : Warning("goal058.unity.marker_not_checked", marker, "Unity player was not executed, so the Goal 058 marker could not be checked."));
        }

        return new FullMediaBoundCampaignPlayerProof
        {
            Passed = missing.Count == 0 && playerExitCode == 0,
            UnityEditorExecuted = unityEditorExecuted,
            PlayerExecuted = playerExecuted,
            UnityExitCode = unityExitCode,
            PlayerExitCode = playerExitCode,
            UnityBuildLogRelativePath = RelativeGoalPath(unityBuildLog),
            LaunchLogRelativePath = RelativeGoalPath(launchLog),
            PlayLoopLogRelativePath = RelativeGoalPath(playLoopLog),
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
            var relative = Path.GetRelativePath(sourceRoot, directory).Replace('\\', '/');
            if (IsSkippedUnityDirectory(relative))
            {
                continue;
            }

            Directory.CreateDirectory(Path.Combine(targetRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        }

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, file).Replace('\\', '/');
            if (IsSkippedUnityDirectory(relative))
            {
                continue;
            }

            var destination = Path.Combine(targetRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
    }

    private static bool IsSkippedUnityDirectory(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/');
        return normalized.StartsWith("Library/", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("Temp/", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("Obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("Logs/", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("UserSettings/", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("Builds/", StringComparison.OrdinalIgnoreCase);
    }

    private static void Cleanup(string workRoot, FullMediaBoundGeneratorCampaignOptions options)
    {
        if (options.CleanupUnityWorkProject)
        {
            SafeDeleteDirectory(workRoot);
        }
    }

    private static void ResetDirectory(string path)
    {
        if (!TryResetDirectory(path, maxAttempts: 120, out var exception))
        {
            throw new IOException($"Directory could not be reset: {path}", exception);
        }
    }

    private static void SafeDeleteDirectory(string path)
    {
        if (!TryResetDirectory(path, maxAttempts: 120, out _))
        {
            return;
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static bool TryResetDirectory(string path, int maxAttempts, out Exception? lastException)
    {
        lastException = null;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }

                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                lastException = exception;
                if (attempt < maxAttempts)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        return false;
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
        var marker = "/" + FullMediaBoundGeneratorCampaignVocabulary.RelativeOutputDirectory.TrimStart('/').Replace('\\', '/') + "/";
        var index = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return index < 0
            ? string.Empty
            : FullMediaBoundGeneratorCampaignVocabulary.RelativeOutputDirectory + "/" + normalized[(index + marker.Length)..];
    }

    private static IReadOnlyList<FullMediaBoundCampaignDiagnostic> SortDiagnostics(IEnumerable<FullMediaBoundCampaignDiagnostic> diagnostics) =>
        FullMediaBoundGeneratorCampaignBuilder.SortDiagnostics(diagnostics);

    private static FullMediaBoundCampaignDiagnostic Error(string code, string target, string message) =>
        FullMediaBoundCampaignDiagnostic.Error(code, target, message);

    private static FullMediaBoundCampaignDiagnostic Warning(string code, string target, string message) =>
        FullMediaBoundCampaignDiagnostic.Warning(code, target, message);

    private static FullMediaBoundCampaignDiagnostic Info(string code, string target, string message) =>
        FullMediaBoundCampaignDiagnostic.Info(code, target, message);

    private sealed record ProcessResult(int ExitCode);
}
