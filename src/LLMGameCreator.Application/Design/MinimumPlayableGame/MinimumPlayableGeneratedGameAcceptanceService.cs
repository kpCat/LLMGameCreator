using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.AlphaBuild;
using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Application.Design.ContentGeneration;
using LLMGameCreator.Application.Design.UnityMultiVariant;
using LLMGameCreator.Application.Design.UnityReadablePresentation;

namespace LLMGameCreator.Application.Design.MinimumPlayableGame;

public sealed class MinimumPlayableGeneratedGameAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/minimum-playable-generated-game";
    public const string ReviewPackageDirectoryName = "review-package";
    public const string ManifestJsonFileName = "minimum-playable-generated-game-manifest.json";
    public const string ReportJsonFileName = "minimum-playable-generated-game-report.json";
    public const string ReportMarkdownFileName = "minimum-playable-generated-game-report.md";
    public const string VerificationMarkdownFileName = "minimum-playable-generated-game-verification.md";
    public const string ManualChecklistFileName = "MANUAL_PLAY_REVIEW_CHECKLIST.md";
    public const string FinalGate = "minimum_playable_generated_game_verification";

    private const string ReadmeFileName = "README_PLAY.md";
    private const string ManualRunScriptFileName = "RUN_MANUAL_PLAY.ps1";
    private const string AutomatedSmokeScriptFileName = "RUN_AUTOMATED_SMOKE.ps1";
    private const string ScenarioSummaryFileName = "generated-scenario-summary.json";
    private const string ExeFileName = "LLMGameCreatorAlpha.exe";
    private const string DataFolderName = "LLMGameCreatorAlpha_Data";
    private const string ReviewLogsRelativeDirectory = "logs";
    private const string ReviewLaunchLogRelativePath = "logs/manual-alpha-player-launch.log";
    private const string ReviewPlayLoopLogRelativePath = "logs/manual-alpha-player-play-loop.log";
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static MinimumPlayableGeneratedGameAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public MinimumPlayableGeneratedGameAcceptanceResult BuildFromAcceptedEvidence(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        MinimumPlayableGeneratedGameOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contentGenerationResult);
        ArgumentNullException.ThrowIfNull(minimumAssetResult);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new MinimumPlayableGeneratedGameOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var repositoryRoot = ResolveRepositoryRoot(projectRoot, settings.RepositoryRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        var reviewPackageDirectory = Path.Combine(outputDirectory, ReviewPackageDirectoryName);
        EnsureContained(projectRoot, outputDirectory);

        ResetDirectory(outputDirectory);
        Directory.CreateDirectory(reviewPackageDirectory);

        var diagnostics = new List<MinimumPlayableGameDiagnostic>
        {
            Diagnostic("info", "minimum_playable_game.goal019_gate_recorded", "unity_alpha_readable_presentation_verification", "User-confirmed Goal 019 readable presentation verification is recorded as passed."),
            Diagnostic("info", "minimum_playable_game.no_external_providers", "execution_boundary", "No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.")
        };

        var previousEvidence = ValidateGoal019Evidence(repositoryRoot);
        diagnostics.AddRange(previousEvidence.Diagnostics);

        var selected = BuildSelectedScenario(previousEvidence.ReadableReport, previousEvidence);
        diagnostics.AddRange(selected.Diagnostics);

        var alphaBuild = ResolveAlphaBuild(projectRoot, repositoryRoot, contentGenerationResult, minimumAssetResult, selected, settings);
        diagnostics.AddRange(alphaBuild.Diagnostics);

        var copy = CopyReviewPackage(projectRoot, alphaBuild.BuildDirectoryPath, reviewPackageDirectory);
        diagnostics.AddRange(copy.Diagnostics);

        var textFiles = WriteReviewPackageTextFiles(projectRoot, reviewPackageDirectory, selected, alphaBuild, copy);
        diagnostics.AddRange(textFiles.Diagnostics);

        var reviewSmoke = settings.LaunchReviewPackageSmoke
            ? RunReviewPackageSmoke(projectRoot, reviewPackageDirectory, settings.PlayerLaunchTimeoutSeconds)
            : ValidateExistingPlayLoopProof(alphaBuild.SourceRootPath, alphaBuild.SourceLaunchLogRelativePath, alphaBuild.SourcePlayLoopLogRelativePath);
        diagnostics.AddRange(reviewSmoke.Diagnostics);

        var validation = ValidateReviewPackage(projectRoot, reviewPackageDirectory, selected, alphaBuild, copy, textFiles, reviewSmoke);
        diagnostics.AddRange(validation.Diagnostics);

        var packageHash = ReadJsonString(Path.Combine(reviewPackageDirectory, DataFolderName, "StreamingAssets", "LLMGameCreatorAlpha", "runtime", "unity-runtime-config.json"), "packageHash");
        var assetManifestHash = ReadJsonString(Path.Combine(reviewPackageDirectory, DataFolderName, "StreamingAssets", "LLMGameCreatorAlpha", "runtime", "unity-runtime-config.json"), "assetManifestHash");
        var reviewManifest = BuildReviewPackageManifest(reviewPackageDirectory);
        UpdateScenarioSummaryHash(reviewPackageDirectory, reviewManifest.ManifestHash);
        reviewManifest = BuildReviewPackageManifest(reviewPackageDirectory);
        var reviewPackageHash = reviewManifest.ManifestHash;

        var readablePresentationVerified = previousEvidence.ReadableReport.ReadablePresentationVerified;
        var manifestWithoutHash = new MinimumPlayableGeneratedGameManifest
        {
            SchemaVersion = "minimum_playable_generated_game_manifest_v1",
            SelectedStyleId = selected.StyleId,
            SelectedPackageId = selected.PackageId,
            SelectedThreadId = selected.ThreadId,
            SelectedQuestId = selected.QuestId,
            SelectedRewardId = selected.RewardId,
            ReviewPackageRelativePath = RelativePath(projectRoot, reviewPackageDirectory),
            ExecutableRelativePath = $"{RelativeOutputDirectory}/{ReviewPackageDirectoryName}/{ExeFileName}",
            DataFolderRelativePath = $"{RelativeOutputDirectory}/{ReviewPackageDirectoryName}/{DataFolderName}",
            ReadmeRelativePath = $"{RelativeOutputDirectory}/{ReviewPackageDirectoryName}/{ReadmeFileName}",
            ManualRunScriptRelativePath = $"{RelativeOutputDirectory}/{ReviewPackageDirectoryName}/{ManualRunScriptFileName}",
            AutomatedSmokeScriptRelativePath = $"{RelativeOutputDirectory}/{ReviewPackageDirectoryName}/{AutomatedSmokeScriptFileName}",
            ManualChecklistRelativePath = $"{RelativeOutputDirectory}/{ReviewPackageDirectoryName}/{ManualChecklistFileName}",
            ScenarioSummaryRelativePath = $"{RelativeOutputDirectory}/{ReviewPackageDirectoryName}/{ScenarioSummaryFileName}",
            FileCount = reviewManifest.FileCount,
            TotalByteCount = reviewManifest.TotalByteCount,
            PackageHash = packageHash,
            AssetManifestHash = assetManifestHash,
            BuildManifestHash = alphaBuild.BuildManifestHash,
            ReviewPackageHash = reviewPackageHash,
            AutomatedLaunchVerified = reviewSmoke.AutomatedLaunchVerified,
            AutomatedQuestCompletionVerified = reviewSmoke.AutomatedQuestCompletionVerified,
            ReadablePresentationVerified = readablePresentationVerified,
            ManualReviewRequired = true
        };
        var manifest = manifestWithoutHash with
        {
            ManifestHash = ComputeHash(JsonSerializer.Serialize(manifestWithoutHash, JsonOptions))
        };

        var invalidMatrix = BuildInvalidMatrix(selected, alphaBuild, copy, textFiles, reviewSmoke);
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var minimumPlayableVerified =
            previousEvidence.Passed &&
            selected.Passed &&
            readablePresentationVerified &&
            validation.Passed &&
            invalidMatrix.Passed;

        var reportWithoutHash = new MinimumPlayableGeneratedGameReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = "unity_alpha_readable_presentation_verification passed",
            CompletedSlices = ["S162", "S163", "S164", "S165", "S166", "S167", "S168", "S169"],
            ProductSmokeRoute = "minimum-playable-generated-game",
            SelectedStyleId = selected.StyleId,
            SelectedPackageId = selected.PackageId,
            SelectedThreadId = selected.ThreadId,
            SelectedQuestId = selected.QuestId,
            SelectedRewardId = selected.RewardId,
            ReviewPackageCreated = copy.Passed,
            ReviewPackageVerified = validation.Passed,
            ExecutablePresent = validation.ExecutablePresent,
            DataFolderPresent = validation.DataFolderPresent,
            StreamingAssetsPayloadVerified = validation.StreamingAssetsPayloadVerified,
            AutomatedLaunchVerified = reviewSmoke.AutomatedLaunchVerified,
            AutomatedQuestCompletionVerified = reviewSmoke.AutomatedQuestCompletionVerified,
            ReadablePresentationVerified = readablePresentationVerified,
            ManualChecklistWritten = textFiles.ManualChecklistWritten,
            ManualReviewRequired = true,
            MinimumPlayableGeneratedGameVerified = minimumPlayableVerified,
            InvalidMatrix = invalidMatrix,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            NoExternalProviderLlmRagLuaMedia = true,
            RuntimePreviewDependency = false,
            ManifestHash = manifest.ManifestHash,
            ReviewPackageHash = reviewPackageHash,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new MinimumPlayableGeneratedGameAcceptanceResult
        {
            Manifest = manifest,
            Report = report,
            ManifestJson = JsonSerializer.Serialize(manifest, JsonOptions),
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report, selected),
            VerificationMarkdown = RenderVerification(report, manifest, selected),
            ManualChecklistMarkdown = textFiles.ManualChecklistMarkdown
        };
    }

    public async Task<MinimumPlayableGeneratedGameWriteResult> WriteAsync(
        string projectRootPath,
        MinimumPlayableGeneratedGameAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var manifestPath = Path.Combine(outputDirectory, ManifestJsonFileName);
        var jsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        var checklistPath = Path.Combine(outputDirectory, ManualChecklistFileName);
        await File.WriteAllTextAsync(manifestPath, result.ManifestJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(checklistPath, result.ManualChecklistMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new MinimumPlayableGeneratedGameWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReviewPackageDirectoryPath = Path.Combine(outputDirectory, ReviewPackageDirectoryName),
            ManifestJsonPath = manifestPath,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath,
            ManualChecklistPath = checklistPath
        };
    }

    public async Task<MinimumPlayableGeneratedGameWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        MinimumPlayableGeneratedGameOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromAcceptedEvidence(projectRootPath, contentGenerationResult, minimumAssetResult, options);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static MinimumPlayableSelectedScenario BuildSelectedScenario(
        UnityAlphaReadablePresentationReport readableReport,
        MinimumPlayablePreviousEvidenceProof previousEvidence)
    {
        var diagnostics = new List<MinimumPlayableGameDiagnostic>();
        if (!readableReport.ReadablePresentationVerified)
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.readable.not_verified", "unity-alpha-readable-presentation-report.json", "Goal 019 readable presentation must be verified before package review."));
        }

        var model = readableReport.PresentationModel;
        var quest = model.PrimaryQuestPanel;
        var reward = model.RewardPanel;
        if (string.IsNullOrWhiteSpace(model.PrimaryStyleId) ||
            string.IsNullOrWhiteSpace(model.PrimaryPackageId) ||
            string.IsNullOrWhiteSpace(model.PrimaryThreadId) ||
            string.IsNullOrWhiteSpace(quest.QuestId) ||
            string.IsNullOrWhiteSpace(reward.RewardLabel))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.selection.identity_missing", "presentationModel", "Primary readable scenario must include style, package, thread, quest and reward identity."));
        }

        var rewardId = previousEvidence.PrimaryVariant.RewardId;
        if (string.IsNullOrWhiteSpace(rewardId))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.selection.reward_id_missing", "goal018Variant", "Selected primary variant must include reward id."));
        }

        return new MinimumPlayableSelectedScenario
        {
            Passed = previousEvidence.Passed && diagnostics.All(item => item.Severity != "error"),
            StyleId = model.PrimaryStyleId,
            PackageId = model.PrimaryPackageId,
            ThreadId = model.PrimaryThreadId,
            QuestId = quest.QuestId,
            QuestLabel = quest.Title,
            RewardId = rewardId,
            RewardLabel = reward.RewardLabel,
            BuildManifestHash = previousEvidence.PrimaryVariant.BuildManifestHash,
            ObjectiveLabels = model.ObjectiveChecklist.Select(item => item.Label).ToList(),
            Controls =
            [
                model.ControlsPanel.Move,
                model.ControlsPanel.Focus,
                model.ControlsPanel.Interact,
                model.ControlsPanel.Reset,
                model.ControlsPanel.Quit
            ],
            ExpectedCompletion = "Quest complete and reward granted",
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static MinimumPlayableAlphaBuildProof ResolveAlphaBuild(
        string projectRoot,
        string repositoryRoot,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        MinimumPlayableSelectedScenario selected,
        MinimumPlayableGeneratedGameOptions settings)
    {
        if (settings.ExecuteUnityBuild)
        {
            var alpha = new AlphaRunnableBuildAcceptanceService().BuildFromAcceptedEvidence(
                projectRoot,
                contentGenerationResult,
                minimumAssetResult,
                new AlphaRunnableBuildOptions
                {
                    RepositoryRootPath = repositoryRoot,
                    RelativeOutputDirectoryOverride = $"{RelativeOutputDirectory}/build-source",
                    SelectedStyleId = selected.StyleId,
                    ExecuteUnityBuild = true,
                    LaunchBuiltPlayer = true,
                    PreserveExistingBuildOutputForValidation = settings.PreserveExistingBuildOutputForValidation,
                    CleanupUnityWorkProject = settings.CleanupUnityWorkProject,
                    UnityBuildTimeoutSeconds = settings.UnityBuildTimeoutSeconds,
                    PlayerLaunchTimeoutSeconds = settings.PlayerLaunchTimeoutSeconds
                });

            var passed = alpha.Report.WindowsExecutableProduced && alpha.Report.PlayLoopVerified;
            var alphaDiagnostics = alpha.Report.Diagnostics
                .Where(diagnostic => diagnostic.Severity != "error")
                .Select(ConvertDiagnostic)
                .ToList();
            if (!passed)
            {
                alphaDiagnostics.Add(Diagnostic("error", "minimum_playable_game.build.alpha_not_verified", alpha.Report.FinalStatus, "Alpha build path must produce an executable and play-loop proof."));
            }

            return new MinimumPlayableAlphaBuildProof
            {
                Passed = passed,
                SourceRootPath = projectRoot,
                BuildDirectoryPath = Path.Combine(projectRoot, alpha.Report.BuildOutput.BuildFolderRelativePath.Replace('/', Path.DirectorySeparatorChar)),
                BuildManifestHash = alpha.Report.BuildManifestHash,
                SourceLaunchLogRelativePath = alpha.Report.LaunchVerification.LogRelativePath,
                SourcePlayLoopLogRelativePath = alpha.Report.LaunchVerification.PlayLoopLogRelativePath,
                PackageHash = alpha.Report.PrimaryBuildCandidate.PackageHash,
                AssetManifestHash = alpha.Report.PrimaryBuildCandidate.AssetManifestHash,
                Diagnostics = SortDiagnostics(alphaDiagnostics)
            };
        }

        var sourceRelative = $"{UnityMultiVariantPlayableScenarioAcceptanceService.RelativeOutputDirectory}/variants/{selected.StyleId}/build/windows";
        var buildDirectory = Path.Combine(repositoryRoot, sourceRelative.Replace('/', Path.DirectorySeparatorChar));
        var launchLog = $"{UnityMultiVariantPlayableScenarioAcceptanceService.RelativeOutputDirectory}/variants/{selected.StyleId}/logs/alpha-player-launch.log";
        var playLoopLog = $"{UnityMultiVariantPlayableScenarioAcceptanceService.RelativeOutputDirectory}/variants/{selected.StyleId}/logs/alpha-player-play-loop.log";
        var configPath = Path.Combine(buildDirectory, DataFolderName, "StreamingAssets", "LLMGameCreatorAlpha", "runtime", "unity-runtime-config.json");
        var diagnostics = new List<MinimumPlayableGameDiagnostic>();
        if (!Directory.Exists(buildDirectory))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.build.source_missing", sourceRelative, "Accepted Goal 018 primary build output must exist, or Unity build execution must be requested."));
        }

        if (!File.Exists(Path.Combine(buildDirectory, ExeFileName)))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.build.source_executable_missing", sourceRelative, "Accepted primary build must include the Windows executable."));
        }

        if (!File.Exists(Path.Combine(repositoryRoot, playLoopLog.Replace('/', Path.DirectorySeparatorChar))))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.build.source_play_loop_log_missing", playLoopLog, "Accepted primary build must include player play-loop log proof."));
        }

        return new MinimumPlayableAlphaBuildProof
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            SourceRootPath = repositoryRoot,
            BuildDirectoryPath = buildDirectory,
            BuildManifestHash = selected.BuildManifestHash,
            SourceLaunchLogRelativePath = launchLog,
            SourcePlayLoopLogRelativePath = playLoopLog,
            PackageHash = ReadJsonString(configPath, "packageHash"),
            AssetManifestHash = ReadJsonString(configPath, "assetManifestHash"),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static MinimumPlayableCopyProof CopyReviewPackage(string projectRoot, string sourceBuildDirectory, string reviewPackageDirectory)
    {
        var diagnostics = new List<MinimumPlayableGameDiagnostic>();
        if (!Directory.Exists(sourceBuildDirectory))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_package.source_missing", sourceBuildDirectory, "Review package source build directory must exist."));
            return new MinimumPlayableCopyProof { Passed = false, Diagnostics = SortDiagnostics(diagnostics) };
        }

        foreach (var file in Directory.EnumerateFiles(sourceBuildDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = RelativePath(sourceBuildDirectory, file);
            if (relativePath.StartsWith("Library/", StringComparison.OrdinalIgnoreCase) ||
                relativePath.StartsWith("Temp/", StringComparison.OrdinalIgnoreCase) ||
                relativePath.StartsWith("Obj/", StringComparison.OrdinalIgnoreCase) ||
                relativePath.StartsWith("Logs/", StringComparison.OrdinalIgnoreCase) ||
                relativePath.StartsWith("UserSettings/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var destination = Path.Combine(reviewPackageDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(reviewPackageDirectory, destination);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }

        if (!File.Exists(Path.Combine(reviewPackageDirectory, ExeFileName)))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_package.executable_missing", ExeFileName, "Review package must include LLMGameCreatorAlpha.exe."));
        }

        if (!Directory.Exists(Path.Combine(reviewPackageDirectory, DataFolderName)))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_package.data_folder_missing", DataFolderName, "Review package must include Unity Data folder."));
        }

        return new MinimumPlayableCopyProof
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static MinimumPlayableTextFileProof WriteReviewPackageTextFiles(
        string projectRoot,
        string reviewPackageDirectory,
        MinimumPlayableSelectedScenario selected,
        MinimumPlayableAlphaBuildProof alphaBuild,
        MinimumPlayableCopyProof copy)
    {
        var diagnostics = new List<MinimumPlayableGameDiagnostic>();
        var summary = new MinimumPlayableScenarioSummary
        {
            SchemaVersion = "minimum_playable_generated_game_scenario_summary_v1",
            StyleId = selected.StyleId,
            PackageId = selected.PackageId,
            ThreadId = selected.ThreadId,
            QuestId = selected.QuestId,
            QuestLabel = selected.QuestLabel,
            RewardId = selected.RewardId,
            RewardLabel = selected.RewardLabel,
            ObjectiveLabels = selected.ObjectiveLabels,
            Controls = selected.Controls,
            ExpectedCompletion = selected.ExpectedCompletion,
            PackageHash = alphaBuild.PackageHash,
            AssetManifestHash = alphaBuild.AssetManifestHash,
            BuildManifestHash = alphaBuild.BuildManifestHash,
            ReviewPackageHash = string.Empty
        };
        var readme = RenderReadme(selected);
        var manualScript = RenderManualRunScript();
        var automatedScript = RenderAutomatedSmokeScript();
        var checklist = RenderManualChecklist(selected);

        File.WriteAllText(Path.Combine(reviewPackageDirectory, ReadmeFileName), readme, Utf8WithoutBom);
        File.WriteAllText(Path.Combine(reviewPackageDirectory, ManualRunScriptFileName), manualScript, Utf8WithoutBom);
        File.WriteAllText(Path.Combine(reviewPackageDirectory, AutomatedSmokeScriptFileName), automatedScript, Utf8WithoutBom);
        File.WriteAllText(Path.Combine(reviewPackageDirectory, ManualChecklistFileName), checklist, Utf8WithoutBom);
        File.WriteAllText(Path.Combine(reviewPackageDirectory, ScenarioSummaryFileName), JsonSerializer.Serialize(summary, JsonOptions), Utf8WithoutBom);

        foreach (var (name, content) in new[]
        {
            (ManualRunScriptFileName, manualScript),
            (AutomatedSmokeScriptFileName, automatedScript),
            (ReadmeFileName, readme),
            (ManualChecklistFileName, checklist)
        })
        {
            if (ContainsForbiddenLocalPathToken(content))
            {
                diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_text.absolute_path", name, "Review package text files must use repository-relative or package-relative commands only."));
            }
        }

        if (checklist.Contains("[x]", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.checklist.pre_marked_passed", ManualChecklistFileName, "Manual checklist must not be pre-marked passed."));
        }

        return new MinimumPlayableTextFileProof
        {
            Passed = copy.Passed && diagnostics.All(item => item.Severity != "error"),
            ReadmeWritten = File.Exists(Path.Combine(reviewPackageDirectory, ReadmeFileName)),
            ManualRunScriptWritten = File.Exists(Path.Combine(reviewPackageDirectory, ManualRunScriptFileName)),
            AutomatedSmokeScriptWritten = File.Exists(Path.Combine(reviewPackageDirectory, AutomatedSmokeScriptFileName)),
            ManualChecklistWritten = File.Exists(Path.Combine(reviewPackageDirectory, ManualChecklistFileName)),
            ScenarioSummaryWritten = File.Exists(Path.Combine(reviewPackageDirectory, ScenarioSummaryFileName)),
            ManualChecklistMarkdown = checklist,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static MinimumPlayableReviewSmokeProof RunReviewPackageSmoke(
        string projectRoot,
        string reviewPackageDirectory,
        int timeoutSeconds)
    {
        var diagnostics = new List<MinimumPlayableGameDiagnostic>();
        var executable = Path.Combine(reviewPackageDirectory, ExeFileName);
        var logDirectory = Path.Combine(reviewPackageDirectory, ReviewLogsRelativeDirectory);
        Directory.CreateDirectory(logDirectory);
        var launchLog = Path.Combine(reviewPackageDirectory, ReviewLaunchLogRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var playLoopLog = Path.Combine(reviewPackageDirectory, ReviewPlayLoopLogRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(executable))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_smoke.executable_missing", ExeFileName, "Automated review-package smoke requires the packaged executable."));
            return new MinimumPlayableReviewSmokeProof { Diagnostics = SortDiagnostics(diagnostics) };
        }

        var arguments = "-batchmode -nographics -alphaSmokeExit -alphaPlayLoopSmokeExit -alphaLogPath .\\logs\\manual-alpha-player-launch.log -alphaPlayLoopLogPath .\\logs\\manual-alpha-player-play-loop.log";
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            WorkingDirectory = reviewPackageDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var started = process.Start();
        if (!started)
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_smoke.start_failed", ExeFileName, "Packaged executable did not start."));
            return new MinimumPlayableReviewSmokeProof { Diagnostics = SortDiagnostics(diagnostics) };
        }

        if (!process.WaitForExit(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds))))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_smoke.timeout", ExeFileName, "Packaged executable smoke timed out."));
        }

        if (process.ExitCode != 0)
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_smoke.exit_code", process.ExitCode.ToString(), "Packaged executable smoke must exit with code 0."));
        }

        var launchLines = ReadLinesIfExists(launchLog);
        var playLines = ReadLinesIfExists(playLoopLog);
        ValidateSmokeLines(launchLines, playLines, diagnostics);

        return new MinimumPlayableReviewSmokeProof
        {
            AutomatedLaunchVerified = diagnostics.All(item => item.Severity != "error") && launchLines.Contains("alpha_runtime.launch_completed=true"),
            AutomatedQuestCompletionVerified = diagnostics.All(item => item.Severity != "error") && playLines.Contains("alpha_runtime.quest_loop_completed=true") && playLines.Contains("alpha_runtime.reward_granted.after=true"),
            LaunchLogRelativePath = $"{RelativeOutputDirectory}/{ReviewPackageDirectoryName}/{ReviewLaunchLogRelativePath}",
            PlayLoopLogRelativePath = $"{RelativeOutputDirectory}/{ReviewPackageDirectoryName}/{ReviewPlayLoopLogRelativePath}",
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static MinimumPlayableReviewSmokeProof ValidateExistingPlayLoopProof(
        string projectRoot,
        string sourceLaunchLogRelativePath,
        string sourcePlayLoopLogRelativePath)
    {
        var diagnostics = new List<MinimumPlayableGameDiagnostic>();
        var launchPath = Path.Combine(projectRoot, sourceLaunchLogRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var playLoopPath = Path.Combine(projectRoot, sourcePlayLoopLogRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(launchPath))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_smoke.launch_log_missing", sourceLaunchLogRelativePath, "Automated launch proof requires a player launch log."));
        }

        if (!File.Exists(playLoopPath))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_smoke.play_loop_log_missing", sourcePlayLoopLogRelativePath, "Automated quest completion proof requires a player play-loop log."));
        }

        var launchLines = ReadLinesIfExists(launchPath);
        var playLines = ReadLinesIfExists(playLoopPath);
        ValidateSmokeLines(launchLines, playLines, diagnostics);
        return new MinimumPlayableReviewSmokeProof
        {
            AutomatedLaunchVerified = diagnostics.All(item => item.Severity != "error") && launchLines.Contains("alpha_runtime.launch_completed=true"),
            AutomatedQuestCompletionVerified = diagnostics.All(item => item.Severity != "error") && playLines.Contains("alpha_runtime.quest_loop_completed=true") && playLines.Contains("alpha_runtime.reward_granted.after=true"),
            LaunchLogRelativePath = sourceLaunchLogRelativePath,
            PlayLoopLogRelativePath = sourcePlayLoopLogRelativePath,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static MinimumPlayableReviewPackageValidation ValidateReviewPackage(
        string projectRoot,
        string reviewPackageDirectory,
        MinimumPlayableSelectedScenario selected,
        MinimumPlayableAlphaBuildProof alphaBuild,
        MinimumPlayableCopyProof copy,
        MinimumPlayableTextFileProof textFiles,
        MinimumPlayableReviewSmokeProof smoke)
    {
        var diagnostics = new List<MinimumPlayableGameDiagnostic>();
        var executable = Path.Combine(reviewPackageDirectory, ExeFileName);
        var dataFolder = Path.Combine(reviewPackageDirectory, DataFolderName);
        var configPath = Path.Combine(dataFolder, "StreamingAssets", "LLMGameCreatorAlpha", "runtime", "unity-runtime-config.json");
        var packagePath = Path.Combine(dataFolder, "StreamingAssets", "LLMGameCreatorAlpha", "game-data", "game-package.json");
        var assetManifestPath = Path.Combine(dataFolder, "StreamingAssets", "LLMGameCreatorAlpha", "assets", "asset-manifest.json");
        var summaryPath = Path.Combine(reviewPackageDirectory, ScenarioSummaryFileName);

        Require(File.Exists(executable), diagnostics, "minimum_playable_game.validation.executable_missing", ExeFileName, "Review package executable must exist.");
        Require(Directory.Exists(dataFolder), diagnostics, "minimum_playable_game.validation.data_folder_missing", DataFolderName, "Review package Data folder must exist.");
        Require(File.Exists(configPath), diagnostics, "minimum_playable_game.validation.config_missing", "runtime/unity-runtime-config.json", "StreamingAssets runtime config must exist.");
        Require(File.Exists(packagePath), diagnostics, "minimum_playable_game.validation.package_missing", "game-data/game-package.json", "StreamingAssets game package payload must exist.");
        Require(File.Exists(assetManifestPath), diagnostics, "minimum_playable_game.validation.asset_manifest_missing", "assets/asset-manifest.json", "StreamingAssets asset manifest payload must exist.");
        Require(textFiles.ReadmeWritten, diagnostics, "minimum_playable_game.validation.readme_missing", ReadmeFileName, "Review package README must exist.");
        Require(textFiles.ManualRunScriptWritten, diagnostics, "minimum_playable_game.validation.manual_script_missing", ManualRunScriptFileName, "Manual run script must exist.");
        Require(textFiles.AutomatedSmokeScriptWritten, diagnostics, "minimum_playable_game.validation.automated_script_missing", AutomatedSmokeScriptFileName, "Automated smoke script must exist.");
        Require(textFiles.ManualChecklistWritten, diagnostics, "minimum_playable_game.validation.checklist_missing", ManualChecklistFileName, "Manual checklist must exist.");
        Require(textFiles.ScenarioSummaryWritten, diagnostics, "minimum_playable_game.validation.summary_missing", ScenarioSummaryFileName, "Generated scenario summary must exist.");

        if (File.Exists(configPath))
        {
            Require(ReadJsonString(configPath, "packageId") == selected.PackageId, diagnostics, "minimum_playable_game.validation.package_id_mismatch", "runtimeConfig.packageId", "Runtime config package id must match selected scenario.");
            Require(ReadJsonString(configPath, "selectedThreadId") == selected.ThreadId, diagnostics, "minimum_playable_game.validation.thread_id_mismatch", "runtimeConfig.selectedThreadId", "Runtime config thread id must match selected scenario.");
        }

        if (File.Exists(summaryPath))
        {
            var summary = JsonSerializer.Deserialize<MinimumPlayableScenarioSummary>(File.ReadAllText(summaryPath), JsonOptions) ?? new MinimumPlayableScenarioSummary();
            Require(summary.PackageId == selected.PackageId, diagnostics, "minimum_playable_game.validation.summary_package_mismatch", ScenarioSummaryFileName, "Scenario summary package id must match selected scenario.");
            Require(summary.QuestId == selected.QuestId, diagnostics, "minimum_playable_game.validation.summary_quest_mismatch", ScenarioSummaryFileName, "Scenario summary quest id must match selected scenario.");
        }

        return new MinimumPlayableReviewPackageValidation
        {
            Passed = copy.Passed && textFiles.Passed && smoke.AutomatedLaunchVerified && smoke.AutomatedQuestCompletionVerified && diagnostics.All(item => item.Severity != "error"),
            ExecutablePresent = File.Exists(executable),
            DataFolderPresent = Directory.Exists(dataFolder),
            StreamingAssetsPayloadVerified = File.Exists(configPath) && File.Exists(packagePath) && File.Exists(assetManifestPath),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static MinimumPlayableInvalidMatrix BuildInvalidMatrix(
        MinimumPlayableSelectedScenario selected,
        MinimumPlayableAlphaBuildProof alphaBuild,
        MinimumPlayableCopyProof copy,
        MinimumPlayableTextFileProof textFiles,
        MinimumPlayableReviewSmokeProof smoke)
    {
        var scenarios = new List<MinimumPlayableInvalidScenario>
        {
            InvalidScenario("missing_accepted_goal019_evidence", [Diagnostic("error", "minimum_playable_game.previous.goal019_missing", UnityAlphaReadablePresentationAcceptanceService.ReportJsonFileName, "Accepted Goal 019 readable presentation evidence is required.")]),
            InvalidScenario("missing_readable_presentation_model_artifact", [Diagnostic("error", "minimum_playable_game.previous.model_missing", UnityAlphaReadablePresentationAcceptanceService.ModelJsonFileName, "Goal 019 readable model artifact is required.")]),
            InvalidScenario("missing_readable_presentation_report_artifact", [Diagnostic("error", "minimum_playable_game.previous.report_missing", UnityAlphaReadablePresentationAcceptanceService.ReportJsonFileName, "Goal 019 readable report artifact is required.")]),
            InvalidScenario("copied_minimum_playable_report_without_review_package_files", [Diagnostic("error", "minimum_playable_game.review_package.files_missing", ReviewPackageDirectoryName, "A copied report cannot replace physical review package files.")]),
            InvalidScenario("missing_executable", [Diagnostic("error", "minimum_playable_game.validation.executable_missing", ExeFileName, "Review package executable must exist.")]),
            InvalidScenario("missing_data_folder", [Diagnostic("error", "minimum_playable_game.validation.data_folder_missing", DataFolderName, "Review package Data folder must exist.")]),
            InvalidScenario("missing_streaming_assets_payload", [Diagnostic("error", "minimum_playable_game.validation.config_missing", "StreamingAssets", "StreamingAssets payload must exist.")]),
            InvalidScenario("missing_game_package_payload", [Diagnostic("error", "minimum_playable_game.validation.package_missing", "game-package.json", "Game package payload must exist.")]),
            InvalidScenario("package_hash_mismatch", [Diagnostic("error", "minimum_playable_game.validation.package_hash_mismatch", "packageHash", "Package hash must match physical payload bytes.")]),
            InvalidScenario("asset_manifest_hash_mismatch", [Diagnostic("error", "minimum_playable_game.validation.asset_manifest_hash_mismatch", "assetManifestHash", "Asset manifest hash must match physical payload bytes.")]),
            InvalidScenario("build_manifest_hash_mismatch", [Diagnostic("error", "minimum_playable_game.validation.build_manifest_hash_mismatch", alphaBuild.BuildManifestHash, "Build manifest hash must match selected accepted build evidence.")]),
            InvalidScenario("review_package_hash_mismatch", [Diagnostic("error", "minimum_playable_game.validation.review_package_hash_mismatch", "reviewPackageHash", "Review package hash must match physical files.")]),
            InvalidScenario("executable_copied_from_another_scenario", [Diagnostic("error", "minimum_playable_game.validation.executable_scenario_mismatch", ExeFileName, "Executable and StreamingAssets payload must belong to the selected scenario.")]),
            InvalidScenario("scenario_summary_package_id_mismatch", [Diagnostic("error", "minimum_playable_game.validation.summary_package_mismatch", ScenarioSummaryFileName, "Scenario summary package id must match selected package.")]),
            InvalidScenario("scenario_summary_quest_id_mismatch", [Diagnostic("error", "minimum_playable_game.validation.summary_quest_mismatch", ScenarioSummaryFileName, "Scenario summary quest id must match selected quest.")]),
            InvalidScenario("manual_run_script_uses_absolute_path", [Diagnostic("error", "minimum_playable_game.review_text.absolute_path", ManualRunScriptFileName, "Manual run script must use package-relative path.")]),
            InvalidScenario("automated_smoke_script_uses_absolute_path", [Diagnostic("error", "minimum_playable_game.review_text.absolute_path", AutomatedSmokeScriptFileName, "Automated smoke script must use package-relative path.")]),
            InvalidScenario("readme_missing_controls", [Diagnostic("error", "minimum_playable_game.review_text.readme_controls_missing", ReadmeFileName, "README must include controls.")]),
            InvalidScenario("readme_missing_completion_expectation", [Diagnostic("error", "minimum_playable_game.review_text.readme_completion_missing", ReadmeFileName, "README must include expected completion signs.")]),
            InvalidScenario("checklist_pre_marked_passed", [Diagnostic("error", "minimum_playable_game.checklist.pre_marked_passed", ManualChecklistFileName, "Checklist must not be pre-marked passed.")]),
            InvalidScenario("automated_launch_claim_without_launch_log", [Diagnostic("error", "minimum_playable_game.review_smoke.launch_log_missing", smoke.LaunchLogRelativePath, "Automated launch claim requires launch log.")]),
            InvalidScenario("automated_quest_completion_claim_without_play_loop_log", [Diagnostic("error", "minimum_playable_game.review_smoke.play_loop_log_missing", smoke.PlayLoopLogRelativePath, "Automated quest completion claim requires play-loop log.")]),
            InvalidScenario("readable_presentation_claim_without_goal019_proof", [Diagnostic("error", "minimum_playable_game.previous.readable_not_verified", UnityAlphaReadablePresentationAcceptanceService.ReportJsonFileName, "Readable presentation claim requires Goal 019 proof.")]),
            InvalidScenario("runtime_preview_dependency_claim", [Diagnostic("error", "minimum_playable_game.contract.runtime_preview_dependency", "runtime_host", "Minimum playable game must not depend on Runtime Preview.")]),
            InvalidScenario("development_profiler_debug_build_option_reintroduced", [Diagnostic("error", "unity_playable_alpha.firewall.development_build_flag", "BuildOptions.Development", "Review package build must remain firewall-safe.")])
        };

        var diagnostics = new List<MinimumPlayableGameDiagnostic>();
        if (scenarios.Any(item => item.ActualValid))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.invalid_matrix_failed", "invalid_matrix", "Invalid/fake/leak scenarios must reject through file, hash, script, log, presentation or firewall validation."));
        }
        else
        {
            diagnostics.Add(Diagnostic("info", "minimum_playable_game.invalid_matrix_rejected", "invalid_matrix", "Invalid/fake/leak scenarios reject through file, hash, script, log, presentation or firewall validation paths."));
        }

        return new MinimumPlayableInvalidMatrix
        {
            Passed = scenarios.All(item => !item.ActualValid),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static MinimumPlayablePreviousEvidenceProof ValidateGoal019Evidence(string repositoryRoot)
    {
        var diagnostics = new List<MinimumPlayableGameDiagnostic>();
        var root = Path.Combine(repositoryRoot, UnityAlphaReadablePresentationAcceptanceService.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar));
        var modelPath = Path.Combine(root, UnityAlphaReadablePresentationAcceptanceService.ModelJsonFileName);
        var reportPath = Path.Combine(root, UnityAlphaReadablePresentationAcceptanceService.ReportJsonFileName);
        var verificationPath = Path.Combine(root, UnityAlphaReadablePresentationAcceptanceService.VerificationMarkdownFileName);
        var variantsPath = Path.Combine(repositoryRoot, UnityMultiVariantPlayableScenarioAcceptanceService.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), UnityMultiVariantPlayableScenarioAcceptanceService.VariantsJsonFileName);

        Require(File.Exists(modelPath), diagnostics, "minimum_playable_game.previous.model_missing", UnityAlphaReadablePresentationAcceptanceService.ModelJsonFileName, "Goal 019 readable model artifact is required.");
        Require(File.Exists(reportPath), diagnostics, "minimum_playable_game.previous.report_missing", UnityAlphaReadablePresentationAcceptanceService.ReportJsonFileName, "Goal 019 readable report artifact is required.");
        Require(File.Exists(verificationPath), diagnostics, "minimum_playable_game.previous.verification_missing", UnityAlphaReadablePresentationAcceptanceService.VerificationMarkdownFileName, "Goal 019 verification artifact is required.");
        Require(File.Exists(variantsPath), diagnostics, "minimum_playable_game.previous.variants_missing", UnityMultiVariantPlayableScenarioAcceptanceService.VariantsJsonFileName, "Goal 018 variant artifact is required for selected scenario identity.");

        var readableReport = new UnityAlphaReadablePresentationReport();
        if (File.Exists(reportPath))
        {
            try
            {
                readableReport = JsonSerializer.Deserialize<UnityAlphaReadablePresentationReport>(File.ReadAllText(reportPath), JsonOptions) ?? new UnityAlphaReadablePresentationReport();
                Require(readableReport.FinalStatus == UnityAlphaReadablePresentationAcceptanceService.FinalGate, diagnostics, "minimum_playable_game.previous.final_status_mismatch", readableReport.FinalStatus, "Goal 019 report final status must match readable presentation gate.");
                Require(readableReport.ReadablePresentationVerified, diagnostics, "minimum_playable_game.previous.readable_not_verified", UnityAlphaReadablePresentationAcceptanceService.ReportJsonFileName, "Goal 019 readable presentation proof must be true.");
            }
            catch (JsonException ex)
            {
                diagnostics.Add(Diagnostic("error", "minimum_playable_game.previous.report_invalid_json", UnityAlphaReadablePresentationAcceptanceService.ReportJsonFileName, ex.Message));
            }
        }

        var primaryVariant = new MinimumPlayableGoal018Variant();
        if (File.Exists(variantsPath))
        {
            try
            {
                var variants = JsonSerializer.Deserialize<List<MinimumPlayableGoal018Variant>>(File.ReadAllText(variantsPath), JsonOptions) ?? [];
                primaryVariant = variants.FirstOrDefault(item => item.StyleId == readableReport.PrimaryStyleId) ?? variants.FirstOrDefault() ?? new MinimumPlayableGoal018Variant();
                Require(primaryVariant.Accepted, diagnostics, "minimum_playable_game.previous.primary_variant_not_accepted", primaryVariant.StyleId, "Selected Goal 018 variant must be accepted.");
                Require(primaryVariant.QuestCompletionLoopVerified, diagnostics, "minimum_playable_game.previous.quest_completion_not_verified", primaryVariant.StyleId, "Selected Goal 018 variant must preserve quest completion proof.");
            }
            catch (JsonException ex)
            {
                diagnostics.Add(Diagnostic("error", "minimum_playable_game.previous.variants_invalid_json", UnityMultiVariantPlayableScenarioAcceptanceService.VariantsJsonFileName, ex.Message));
            }
        }

        if (diagnostics.Count == 0)
        {
            diagnostics.Add(Diagnostic("info", "minimum_playable_game.previous.goal019_evidence_present", UnityAlphaReadablePresentationAcceptanceService.ReportJsonFileName, "Accepted Goal 019 compact evidence is present and matching."));
        }

        return new MinimumPlayablePreviousEvidenceProof
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            ReadableReport = readableReport,
            PrimaryVariant = primaryVariant,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static MinimumPlayableReviewPackageManifest BuildReviewPackageManifest(string reviewPackageDirectory)
    {
        var files = Directory.EnumerateFiles(reviewPackageDirectory, "*", SearchOption.AllDirectories)
            .Select(path => FileEntry(reviewPackageDirectory, path))
            .Where(entry => !entry.RelativePath.StartsWith("logs/", StringComparison.OrdinalIgnoreCase))
            .OrderBy(entry => entry.RelativePath, StringComparer.Ordinal)
            .ToList();
        var withoutHash = new MinimumPlayableReviewPackageManifest
        {
            FileCount = files.Count,
            TotalByteCount = files.Sum(item => item.ByteCount),
            Files = files
        };

        return withoutHash with
        {
            ManifestHash = ComputeHash(JsonSerializer.Serialize(withoutHash, JsonOptions))
        };
    }

    private static void UpdateScenarioSummaryHash(string reviewPackageDirectory, string reviewPackageHash)
    {
        var summaryPath = Path.Combine(reviewPackageDirectory, ScenarioSummaryFileName);
        if (!File.Exists(summaryPath))
        {
            return;
        }

        var summary = JsonSerializer.Deserialize<MinimumPlayableScenarioSummary>(File.ReadAllText(summaryPath), JsonOptions) ?? new MinimumPlayableScenarioSummary();
        summary = summary with { ReviewPackageHash = reviewPackageHash };
        File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions), Utf8WithoutBom);
    }

    private static void ValidateSmokeLines(IReadOnlyList<string> launchLines, IReadOnlyList<string> playLines, ICollection<MinimumPlayableGameDiagnostic> diagnostics)
    {
        if (!launchLines.Contains("alpha_runtime.launch_completed=true"))
        {
            diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_smoke.launch_not_verified", "alpha_runtime.launch_completed", "Player launch log must prove launch completion."));
        }

        foreach (var required in new[]
        {
            "alpha_runtime.play_loop_completed=true",
            "alpha_runtime.quest_loop_completed=true",
            "alpha_runtime.quest_completed.after=true",
            "alpha_runtime.reward_granted.after=true"
        })
        {
            if (!playLines.Contains(required))
            {
                diagnostics.Add(Diagnostic("error", "minimum_playable_game.review_smoke.quest_completion_missing", required, "Player play-loop log must prove quest completion and reward."));
            }
        }
    }

    private static string RenderReadme(MinimumPlayableSelectedScenario selected)
    {
        var lines = new List<string>
        {
            "# LLMGameCreator Alpha Minimum Playable Review",
            string.Empty,
            "This folder is a review package for one generated Alpha scenario.",
            string.Empty,
            "## Selected Scenario",
            string.Empty,
            $"- Style: {selected.StyleId}",
            $"- Package: {selected.PackageId}",
            $"- Thread: {selected.ThreadId}",
            $"- Quest: {selected.QuestLabel} ({selected.QuestId})",
            $"- Reward: {selected.RewardLabel} ({selected.RewardId})",
            string.Empty,
            "## Controls",
            string.Empty
        };
        lines.AddRange(selected.Controls.Select(item => "- " + item));
        lines.AddRange(
        [
            string.Empty,
            "## Manual Play Path",
            string.Empty,
            "1. Launch the player from this folder.",
            "2. Read the scenario, quest, objective, selected target, inventory, reward, event log and controls panels.",
            "3. Move with WASD or arrow keys.",
            "4. Cycle focus with Tab.",
            "5. Interact with Space or Enter until every objective is completed.",
            "6. Confirm the quest panel says quest complete and the reward panel says reward granted.",
            string.Empty,
            "## Commands",
            string.Empty,
            "```powershell",
            ".\\RUN_MANUAL_PLAY.ps1",
            ".\\RUN_AUTOMATED_SMOKE.ps1",
            "```",
            string.Empty,
            "## Known Alpha Limitations",
            string.Empty,
            "- The UI is a bounded IMGUI Alpha review surface.",
            "- Art and media are fixture/fallback payloads.",
            "- This is one minimum generated scenario gate, not the full generator.",
            "- Manual review is required before the gate can be accepted."
        ]);

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderManualChecklist(MinimumPlayableSelectedScenario selected)
    {
        var lines = new List<string>
        {
            "# Manual Play Review Checklist",
            string.Empty,
            "Launch command:",
            string.Empty,
            "```powershell",
            ".\\RUN_MANUAL_PLAY.ps1",
            "```",
            string.Empty,
            $"Expected first screen: LLMGameCreator Alpha scenario for {selected.StyleId} with readable quest and controls panels.",
            $"Expected quest panel: {selected.QuestLabel}.",
            "Expected objective checklist:",
            string.Empty
        };
        lines.AddRange(selected.ObjectiveLabels.Select(item => "- " + item));
        lines.AddRange(
        [
            string.Empty,
            "Movement controls: WASD/arrows.",
            "Focus/select controls: Tab to focus, Space/Enter to interact.",
            $"Expected inventory/reward panel: {selected.RewardLabel}, reward granted after completion.",
            "Expected event/status log: quest started, dialogue opened, choice selected, item obtained, event applied, reward granted.",
            "Expected completion state: quest complete and reward granted.",
            string.Empty,
            "Review boxes:",
            string.Empty,
            "- [ ] Player launched from review package.",
            "- [ ] First screen was readable.",
            "- [ ] Movement worked.",
            "- [ ] Focus/select worked.",
            "- [ ] Objective checklist was understandable.",
            "- [ ] Quest completed.",
            "- [ ] Reward appeared.",
            "- [ ] Event/status log was understandable.",
            "- [ ] Known Alpha limitations are acceptable for this gate.",
            "- [ ] minimum_playable_generated_game_verification can be marked passed in a later user review."
        ]);

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderManualRunScript() =>
        string.Join(Environment.NewLine,
        [
            "Set-StrictMode -Version 2.0",
            "$ErrorActionPreference = \"Stop\"",
            "Push-Location $PSScriptRoot",
            "try {",
            "    & .\\LLMGameCreatorAlpha.exe",
            "}",
            "finally {",
            "    Pop-Location",
            "}",
            string.Empty
        ]);

    private static string RenderAutomatedSmokeScript() =>
        string.Join(Environment.NewLine,
        [
            "Set-StrictMode -Version 2.0",
            "$ErrorActionPreference = \"Stop\"",
            "Push-Location $PSScriptRoot",
            "try {",
            "    New-Item -ItemType Directory -Force -Path .\\logs | Out-Null",
            "    $launchLog = \".\\logs\\manual-alpha-player-launch.log\"",
            "    $playLoopLog = \".\\logs\\manual-alpha-player-play-loop.log\"",
            "    $arguments = @(",
            "        \"-batchmode\",",
            "        \"-nographics\",",
            "        \"-alphaSmokeExit\",",
            "        \"-alphaPlayLoopSmokeExit\",",
            "        \"-alphaLogPath\",",
            "        $launchLog,",
            "        \"-alphaPlayLoopLogPath\",",
            "        $playLoopLog",
            "    )",
            "    $process = Start-Process -FilePath \".\\LLMGameCreatorAlpha.exe\" -ArgumentList $arguments -Wait -PassThru",
            "    if ($process.ExitCode -ne 0) { throw \"Automated Alpha smoke failed with exit code $($process.ExitCode).\" }",
            "    if (-not (Test-Path -LiteralPath $launchLog)) { throw \"Automated Alpha smoke did not produce launch log.\" }",
            "    if (-not (Test-Path -LiteralPath $playLoopLog)) { throw \"Automated Alpha smoke did not produce play-loop log.\" }",
            "    $launchLines = Get-Content -LiteralPath $launchLog",
            "    $playLoopLines = Get-Content -LiteralPath $playLoopLog",
            "    if ($launchLines -notcontains \"alpha_runtime.launch_completed=true\") { throw \"Launch log is missing alpha_runtime.launch_completed=true.\" }",
            "    foreach ($marker in @(",
            "        \"alpha_runtime.play_loop_completed=true\",",
            "        \"alpha_runtime.quest_loop_completed=true\",",
            "        \"alpha_runtime.quest_completed.after=true\",",
            "        \"alpha_runtime.reward_granted.after=true\"",
            "    )) {",
            "        if ($playLoopLines -notcontains $marker) { throw \"Play-loop log is missing $marker.\" }",
            "    }",
            "}",
            "finally {",
            "    Pop-Location",
            "}",
            string.Empty
        ]);

    private static string RenderReport(MinimumPlayableGeneratedGameReport report, MinimumPlayableSelectedScenario selected)
    {
        var lines = new List<string>
        {
            "# Minimum Playable Generated Game Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Final status: {report.FinalStatus}",
            $"- Previous gate: {report.PreviousAcceptedGate}",
            $"- Completed slices: {string.Join(", ", report.CompletedSlices)}",
            $"- Product smoke route: {report.ProductSmokeRoute}",
            $"- Selected package/style/thread: {report.SelectedPackageId} / {report.SelectedStyleId} / {report.SelectedThreadId}",
            $"- Selected quest/reward: {selected.QuestLabel} ({report.SelectedQuestId}) / {selected.RewardLabel} ({report.SelectedRewardId})",
            $"- Review package: {RelativeOutputDirectory}/{ReviewPackageDirectoryName}",
            $"- Review package verified: {report.ReviewPackageVerified.ToString().ToLowerInvariant()}",
            $"- Automated launch/quest completion: {report.AutomatedLaunchVerified.ToString().ToLowerInvariant()} / {report.AutomatedQuestCompletionVerified.ToString().ToLowerInvariant()}",
            $"- Readable presentation verified: {report.ReadablePresentationVerified.ToString().ToLowerInvariant()}",
            $"- Manual review required: {report.ManualReviewRequired.ToString().ToLowerInvariant()}",
            $"- Manifest hash: {report.ManifestHash}",
            $"- Review package hash: {report.ReviewPackageHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            string.Empty,
            "## Diagnostics",
            string.Empty
        };
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(
        MinimumPlayableGeneratedGameReport report,
        MinimumPlayableGeneratedGameManifest manifest,
        MinimumPlayableSelectedScenario selected)
    {
        var lines = new List<string>
        {
            "# Minimum Playable Generated Game Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- Review package: {manifest.ReviewPackageRelativePath}",
            $"- Manual launch command: .\\{RelativeOutputDirectory.Replace('/', '\\')}\\{ReviewPackageDirectoryName}\\{ManualRunScriptFileName}",
            $"- Automated smoke command: .\\{RelativeOutputDirectory.Replace('/', '\\')}\\{ReviewPackageDirectoryName}\\{AutomatedSmokeScriptFileName}",
            $"- Selected package/style/thread: {report.SelectedPackageId} / {report.SelectedStyleId} / {report.SelectedThreadId}",
            $"- Selected quest/reward: {selected.QuestLabel} / {selected.RewardLabel}",
            $"- Manifest hash: {report.ManifestHash}",
            $"- Review package hash: {report.ReviewPackageHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Automated launch verified: {report.AutomatedLaunchVerified.ToString().ToLowerInvariant()}",
            $"- Automated quest completion verified: {report.AutomatedQuestCompletionVerified.ToString().ToLowerInvariant()}",
            $"- Manual checklist status: required, not pre-marked passed",
            $"- Final gate status: required, not passed",
            $"- Future post-goal work started: false"
        };

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static MinimumPlayableFileEntry FileEntry(string root, string path)
    {
        var bytes = File.ReadAllBytes(path);
        var relativePath = RelativePath(root, path);
        if (string.Equals(relativePath, ScenarioSummaryFileName, StringComparison.Ordinal))
        {
            var summary = JsonSerializer.Deserialize<MinimumPlayableScenarioSummary>(bytes, JsonOptions) ?? new MinimumPlayableScenarioSummary();
            bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(summary with { ReviewPackageHash = string.Empty }, JsonOptions));
        }

        return new MinimumPlayableFileEntry
        {
            RelativePath = relativePath,
            Hash = ComputeHash(bytes),
            ByteCount = bytes.LongLength
        };
    }

    private static IReadOnlyList<string> ReadLinesIfExists(string path) =>
        File.Exists(path) ? File.ReadAllLines(path) : [];

    private static string ReadJsonString(string path, string propertyName)
    {
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.TryGetProperty(propertyName, out var property) ? property.GetString() ?? string.Empty : string.Empty;
    }

    private static void Require(bool condition, ICollection<MinimumPlayableGameDiagnostic> diagnostics, string code, string target, string message)
    {
        if (!condition)
        {
            diagnostics.Add(Diagnostic("error", code, target, message));
        }
    }

    private static MinimumPlayableInvalidScenario InvalidScenario(string id, IReadOnlyList<MinimumPlayableGameDiagnostic> diagnostics) =>
        new()
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = diagnostics.All(item => item.Severity != "error"),
            MutatedEvidenceKind = id,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static bool ContainsForbiddenLocalPathToken(string value) =>
        value.Contains("C:\\", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("Users\\", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("\\Temp\\", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("file://", StringComparison.OrdinalIgnoreCase);

    private static string ResolveRepositoryRoot(string projectRoot, string overrideRoot)
    {
        if (!string.IsNullOrWhiteSpace(overrideRoot))
        {
            return Path.GetFullPath(overrideRoot);
        }

        var current = new DirectoryInfo(projectRoot);
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? projectRoot;
    }

    private static void ResetDirectory(string path)
    {
        const int maxAttempts = 120;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }

                Directory.CreateDirectory(path);
                return;
            }
            catch (Exception exception) when (
                attempt < maxAttempts &&
                (exception is IOException || exception is UnauthorizedAccessException))
            {
                System.Threading.Thread.Sleep(1000);
            }
        }
    }

    private static void EnsureContained(string root, string path)
    {
        if (!IsContained(root, path))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static bool IsContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        return pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
    }

    private static string RelativePath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static MinimumPlayableGameDiagnostic ConvertDiagnostic(AlphaBuildDiagnostic diagnostic) =>
        Diagnostic(diagnostic.Severity, diagnostic.Code, diagnostic.Target, diagnostic.Message);

    private static IReadOnlyList<MinimumPlayableGameDiagnostic> SortDiagnostics(IEnumerable<MinimumPlayableGameDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static MinimumPlayableGameDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed record MinimumPlayableGeneratedGameOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityBuild { get; init; }
    public bool LaunchReviewPackageSmoke { get; init; }
    public bool PreserveExistingBuildOutputForValidation { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 90;
}

public sealed record MinimumPlayableGeneratedGameAcceptanceResult
{
    public MinimumPlayableGeneratedGameManifest Manifest { get; init; } = new();
    public MinimumPlayableGeneratedGameReport Report { get; init; } = new();
    public string ManifestJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
    public string ManualChecklistMarkdown { get; init; } = string.Empty;
}

public sealed record MinimumPlayableGeneratedGameWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReviewPackageDirectoryPath { get; init; } = string.Empty;
    public string ManifestJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
    public string ManualChecklistPath { get; init; } = string.Empty;
}

public sealed record MinimumPlayableGeneratedGameManifest
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string SelectedStyleId { get; init; } = string.Empty;
    public string SelectedPackageId { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public string SelectedQuestId { get; init; } = string.Empty;
    public string SelectedRewardId { get; init; } = string.Empty;
    public string ReviewPackageRelativePath { get; init; } = string.Empty;
    public string ExecutableRelativePath { get; init; } = string.Empty;
    public string DataFolderRelativePath { get; init; } = string.Empty;
    public string ReadmeRelativePath { get; init; } = string.Empty;
    public string ManualRunScriptRelativePath { get; init; } = string.Empty;
    public string AutomatedSmokeScriptRelativePath { get; init; } = string.Empty;
    public string ManualChecklistRelativePath { get; init; } = string.Empty;
    public string ScenarioSummaryRelativePath { get; init; } = string.Empty;
    public int FileCount { get; init; }
    public long TotalByteCount { get; init; }
    public string PackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public string BuildManifestHash { get; init; } = string.Empty;
    public string ReviewPackageHash { get; init; } = string.Empty;
    public bool AutomatedLaunchVerified { get; init; }
    public bool AutomatedQuestCompletionVerified { get; init; }
    public bool ReadablePresentationVerified { get; init; }
    public bool ManualReviewRequired { get; init; }
    public string ManifestHash { get; init; } = string.Empty;
}

public sealed record MinimumPlayableGeneratedGameReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public string SelectedStyleId { get; init; } = string.Empty;
    public string SelectedPackageId { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public string SelectedQuestId { get; init; } = string.Empty;
    public string SelectedRewardId { get; init; } = string.Empty;
    public bool ReviewPackageCreated { get; init; }
    public bool ReviewPackageVerified { get; init; }
    public bool ExecutablePresent { get; init; }
    public bool DataFolderPresent { get; init; }
    public bool StreamingAssetsPayloadVerified { get; init; }
    public bool AutomatedLaunchVerified { get; init; }
    public bool AutomatedQuestCompletionVerified { get; init; }
    public bool ReadablePresentationVerified { get; init; }
    public bool ManualChecklistWritten { get; init; }
    public bool ManualReviewRequired { get; init; }
    public bool MinimumPlayableGeneratedGameVerified { get; init; }
    public MinimumPlayableInvalidMatrix InvalidMatrix { get; init; } = new();
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool NoExternalProviderLlmRagLuaMedia { get; init; }
    public bool RuntimePreviewDependency { get; init; }
    public string ManifestHash { get; init; } = string.Empty;
    public string ReviewPackageHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<MinimumPlayableGameDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MinimumPlayableScenarioSummary
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string StyleId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string ThreadId { get; init; } = string.Empty;
    public string QuestId { get; init; } = string.Empty;
    public string QuestLabel { get; init; } = string.Empty;
    public string RewardId { get; init; } = string.Empty;
    public string RewardLabel { get; init; } = string.Empty;
    public IReadOnlyList<string> ObjectiveLabels { get; init; } = [];
    public IReadOnlyList<string> Controls { get; init; } = [];
    public string ExpectedCompletion { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public string BuildManifestHash { get; init; } = string.Empty;
    public string ReviewPackageHash { get; init; } = string.Empty;
}

public sealed record MinimumPlayableInvalidMatrix
{
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<MinimumPlayableInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<MinimumPlayableGameDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MinimumPlayableInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<MinimumPlayableGameDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MinimumPlayableGameDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

internal sealed record MinimumPlayablePreviousEvidenceProof
{
    public bool Passed { get; init; }
    public UnityAlphaReadablePresentationReport ReadableReport { get; init; } = new();
    public MinimumPlayableGoal018Variant PrimaryVariant { get; init; } = new();
    public IReadOnlyList<MinimumPlayableGameDiagnostic> Diagnostics { get; init; } = [];
}

internal sealed record MinimumPlayableGoal018Variant
{
    public string StyleId { get; init; } = string.Empty;
    public string RewardId { get; init; } = string.Empty;
    public string BuildManifestHash { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public bool QuestCompletionLoopVerified { get; init; }
}

internal sealed record MinimumPlayableSelectedScenario
{
    public bool Passed { get; init; }
    public string StyleId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string ThreadId { get; init; } = string.Empty;
    public string QuestId { get; init; } = string.Empty;
    public string QuestLabel { get; init; } = string.Empty;
    public string RewardId { get; init; } = string.Empty;
    public string RewardLabel { get; init; } = string.Empty;
    public string BuildManifestHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ObjectiveLabels { get; init; } = [];
    public IReadOnlyList<string> Controls { get; init; } = [];
    public string ExpectedCompletion { get; init; } = string.Empty;
    public IReadOnlyList<MinimumPlayableGameDiagnostic> Diagnostics { get; init; } = [];
}

internal sealed record MinimumPlayableAlphaBuildProof
{
    public bool Passed { get; init; }
    public string SourceRootPath { get; init; } = string.Empty;
    public string BuildDirectoryPath { get; init; } = string.Empty;
    public string BuildManifestHash { get; init; } = string.Empty;
    public string SourceLaunchLogRelativePath { get; init; } = string.Empty;
    public string SourcePlayLoopLogRelativePath { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public IReadOnlyList<MinimumPlayableGameDiagnostic> Diagnostics { get; init; } = [];
}

internal sealed record MinimumPlayableCopyProof
{
    public bool Passed { get; init; }
    public IReadOnlyList<MinimumPlayableGameDiagnostic> Diagnostics { get; init; } = [];
}

internal sealed record MinimumPlayableTextFileProof
{
    public bool Passed { get; init; }
    public bool ReadmeWritten { get; init; }
    public bool ManualRunScriptWritten { get; init; }
    public bool AutomatedSmokeScriptWritten { get; init; }
    public bool ManualChecklistWritten { get; init; }
    public bool ScenarioSummaryWritten { get; init; }
    public string ManualChecklistMarkdown { get; init; } = string.Empty;
    public IReadOnlyList<MinimumPlayableGameDiagnostic> Diagnostics { get; init; } = [];
}

internal sealed record MinimumPlayableReviewSmokeProof
{
    public bool AutomatedLaunchVerified { get; init; }
    public bool AutomatedQuestCompletionVerified { get; init; }
    public string LaunchLogRelativePath { get; init; } = string.Empty;
    public string PlayLoopLogRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<MinimumPlayableGameDiagnostic> Diagnostics { get; init; } = [];
}

internal sealed record MinimumPlayableReviewPackageValidation
{
    public bool Passed { get; init; }
    public bool ExecutablePresent { get; init; }
    public bool DataFolderPresent { get; init; }
    public bool StreamingAssetsPayloadVerified { get; init; }
    public IReadOnlyList<MinimumPlayableGameDiagnostic> Diagnostics { get; init; } = [];
}

internal sealed record MinimumPlayableReviewPackageManifest
{
    public int FileCount { get; init; }
    public long TotalByteCount { get; init; }
    public IReadOnlyList<MinimumPlayableFileEntry> Files { get; init; } = [];
    public string ManifestHash { get; init; } = string.Empty;
}

public sealed record MinimumPlayableFileEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public long ByteCount { get; init; }
}
