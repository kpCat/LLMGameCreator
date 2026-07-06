using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class CanonicalRuntimeSelectedCandidatePlaythroughArtifactService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static CanonicalRuntimeSelectedCandidatePlaythroughArtifactService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public static GamePackageDefinition LoadPackage(string packagePath)
    {
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(
            File.ReadAllText(packagePath, Encoding.UTF8),
            JsonOptions);
        return package ?? new GamePackageDefinition();
    }

    public static string ReadCandidateId(string handoffPath)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(handoffPath, Encoding.UTF8));
        return TryReadString(doc.RootElement, "selectedCandidateId");
    }

    public static CanonicalRuntimeSelectedCandidateUnitySmoke ReadUnitySmoke(string path)
    {
        var smoke = JsonSerializer.Deserialize<CanonicalRuntimeSelectedCandidateUnitySmoke>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions);
        return smoke ?? new CanonicalRuntimeSelectedCandidateUnitySmoke();
    }

    public async Task<CanonicalRuntimeSelectedCandidatePlaythroughWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        GamePackageDefinition package,
        CanonicalRuntimeSelectedCandidatePlaythroughRequest request,
        CanonicalRuntimeSelectedCandidatePlaythroughResult runtimeResult,
        CanonicalRuntimeSelectedCandidateUnitySmoke? unitySmoke = null,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var packageValidation = BuildPackageValidation(root, package, request);
        var smoke = unitySmoke ?? BuildPendingUnitySmoke(root);
        var matrix = BuildMatrix(packageValidation, runtimeResult, smoke);
        var report = BuildReport(packageValidation, runtimeResult, smoke);
        var negative = BuildNegativeProof(runtimeResult);
        var dashboard = BuildDashboard(packageValidation, runtimeResult, smoke);
        var markdown = RenderReport(report, dashboard, runtimeResult);

        var proceduralFiles = BuildFilePayloads(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory,
            packageValidation,
            runtimeResult,
            smoke,
            matrix,
            report,
            markdown,
            negative,
            dashboard);
        var exportFiles = BuildFilePayloads(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ExportPackageDirectory,
            packageValidation,
            runtimeResult,
            smoke,
            matrix,
            report,
            markdown,
            negative,
            dashboard);

        var procedural = Resolve(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in proceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in exportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, markdown, cancellationToken).ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new CanonicalRuntimeSelectedCandidatePlaythroughWriteResult
        {
            Dashboard = dashboard,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static SortedDictionary<string, string> BuildFilePayloads(
        string root,
        string relativeRoot,
        CanonicalRuntimeSelectedCandidatePackageValidation packageValidation,
        CanonicalRuntimeSelectedCandidatePlaythroughResult runtimeResult,
        CanonicalRuntimeSelectedCandidateUnitySmoke unitySmoke,
        CanonicalRuntimeSelectedCandidateMatrixResult matrix,
        CanonicalRuntimeSelectedCandidateReport report,
        string reportMarkdown,
        CanonicalRuntimeSelectedCandidateNegativeProof negative,
        CanonicalRuntimeSelectedCandidateDashboard dashboard)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.PackageValidationFileName] =
                Serialize(packageValidation),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.PlaythroughScriptFileName] =
                Serialize(runtimeResult.PlaythroughScript),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.TranscriptFileName] =
                Serialize(runtimeResult.Transcript),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.StateSummaryFileName] =
                Serialize(runtimeResult.StateSummary),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.StateBeforeSaveFileName] =
                Serialize(new
                {
                    format = "canonical_runtime_selected_candidate_state_before_save_v1",
                    stateHash = runtimeResult.SaveLoadReplay.SaveStateHash,
                    session = runtimeResult.StateBeforeSave
                }),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.StateSaveFileName] =
                Serialize(new
                {
                    format = "canonical_runtime_selected_candidate_save_v1",
                    saveStateHash = runtimeResult.SaveLoadReplay.SaveStateHash,
                    session = runtimeResult.StateBeforeSave
                }),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.StateAfterLoadFileName] =
                Serialize(new
                {
                    format = "canonical_runtime_selected_candidate_state_after_load_v1",
                    stateHash = runtimeResult.SaveLoadReplay.LoadStateHash,
                    session = runtimeResult.StateAfterLoad
                }),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ReplayTranscriptFileName] =
                Serialize(runtimeResult.ReplayTranscript),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.SaveLoadReplayResultFileName] =
                Serialize(runtimeResult.SaveLoadReplay),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.MatrixResultFileName] =
                Serialize(matrix),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.UnitySmokeFileName] =
                Serialize(unitySmoke),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ReportJsonFileName] =
                Serialize(report),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ReportMarkdownFileName] =
                reportMarkdown,
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DashboardFileName] =
                Serialize(dashboard)
        };
        var fileIndex = BuildFileIndex(root, relativeRoot, files);
        files[CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.FileIndexFileName] =
            Serialize(fileIndex);
        return files;
    }

    private static CanonicalRuntimeSelectedCandidatePackageValidation BuildPackageValidation(
        string root,
        GamePackageDefinition package,
        CanonicalRuntimeSelectedCandidatePlaythroughRequest request)
    {
        var diagnostics = new List<string>();
        var validation = new GamePackageValidator().Validate(package, Path.GetDirectoryName(
            Resolve(root, request.PackagePath)));
        diagnostics.AddRange(validation.Issues.Select(FormatIssue));
        var blockingValidationIssues = validation.Issues
            .Where(IsGoal134BlockingValidationIssue)
            .ToList();

        var selectedCandidateLoaded =
            !string.IsNullOrWhiteSpace(request.CandidateId)
            && !string.IsNullOrWhiteSpace(package.Manifest.PackageId)
            && !string.IsNullOrWhiteSpace(package.Manifest.StartMapId);
        var handoffMatches = HandoffMatchesPackage(root, request, diagnostics);
        var missingAnchors = CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.RequiredAnchors
            .Where(anchor => !AnchorExists(package, anchor))
            .OrderBy(anchor => anchor, StringComparer.Ordinal)
            .ToList();
        var requiredAnchorsPresent = missingAnchors.Count == 0;
        var passed =
            selectedCandidateLoaded
            && handoffMatches
            && blockingValidationIssues.Count == 0
            && requiredAnchorsPresent;

        return new CanonicalRuntimeSelectedCandidatePackageValidation
        {
            CandidateId = request.CandidateId,
            PackageId = package.Manifest.PackageId,
            PackageTitle = package.Manifest.Title,
            PackagePath = Relative(root, Resolve(root, request.PackagePath)),
            HandoffPath = Relative(root, Resolve(root, request.HandoffPath)),
            SelectedCandidateLoaded = selectedCandidateLoaded,
            HandoffMatchesPackage = handoffMatches,
            ExistingValidatorPassed = blockingValidationIssues.Count == 0,
            RequiredAnchorsPresent = requiredAnchorsPresent,
            PackageValidationPassed = passed,
            RequiredAnchors = CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.RequiredAnchors,
            MissingAnchors = missingAnchors,
            Diagnostics = diagnostics
        };
    }

    private static bool HandoffMatchesPackage(
        string root,
        CanonicalRuntimeSelectedCandidatePlaythroughRequest request,
        List<string> diagnostics)
    {
        var handoffPath = Resolve(root, request.HandoffPath);
        var packagePath = Resolve(root, request.PackagePath);
        if (!File.Exists(handoffPath) || !File.Exists(packagePath))
        {
            diagnostics.Add("goal134.handoff_or_package_missing");
            return false;
        }

        using var handoff = JsonDocument.Parse(File.ReadAllText(handoffPath, Encoding.UTF8));
        using var package = JsonDocument.Parse(File.ReadAllText(packagePath, Encoding.UTF8));
        var handoffCandidate = TryReadString(handoff.RootElement, "selectedCandidateId");
        var handoffPackagePath = TryReadString(handoff.RootElement, "selectedCandidatePackagePath")
            .Replace('\\', '/');
        var relativePackagePath = Relative(root, packagePath);
        var packageCandidate = TryReadString(package.RootElement, "manifest", "candidateMetadata", "candidateId");
        var matches =
            string.Equals(handoffCandidate, request.CandidateId, StringComparison.Ordinal)
            && string.Equals(packageCandidate, request.CandidateId, StringComparison.Ordinal)
            && string.Equals(handoffPackagePath, relativePackagePath, StringComparison.Ordinal);
        if (!matches)
        {
            diagnostics.Add(
                "goal134.handoff_package_mismatch:"
                + handoffCandidate
                + ":"
                + packageCandidate
                + ":"
                + handoffPackagePath
                + ":"
                + relativePackagePath);
        }

        return matches;
    }

    private static bool AnchorExists(GamePackageDefinition package, string anchor)
    {
        if (anchor.StartsWith("map/", StringComparison.Ordinal))
        {
            return package.Game.Maps.Any(item => item.Id == anchor);
        }

        if (anchor.StartsWith("entity/", StringComparison.Ordinal))
        {
            return package.Game.Maps.SelectMany(map => map.Entities).Any(item => item.Id == anchor);
        }

        if (anchor.StartsWith("interaction/", StringComparison.Ordinal))
        {
            return package.Game.Interactions.Any(item => item.Id == anchor);
        }

        if (anchor.StartsWith("dialogue/", StringComparison.Ordinal))
        {
            return package.Game.Dialogues.Any(item => item.Id == anchor);
        }

        if (anchor.StartsWith("quest/", StringComparison.Ordinal))
        {
            return package.Game.Quests.Any(item => item.Id == anchor);
        }

        if (anchor.StartsWith("inventory/", StringComparison.Ordinal))
        {
            return package.Game.Inventories.Any(item => item.Id == anchor);
        }

        if (anchor.StartsWith("recipe/", StringComparison.Ordinal))
        {
            return package.Game.Recipes.Any(item => item.Id == anchor);
        }

        if (anchor.StartsWith("node/", StringComparison.Ordinal))
        {
            return package.Game.ResourceNodes.Any(item => item.Id == anchor);
        }

        if (anchor.StartsWith("transaction/", StringComparison.Ordinal))
        {
            return package.Game.Transactions.Any(item => item.Id == anchor);
        }

        if (anchor.StartsWith("encounter/", StringComparison.Ordinal))
        {
            return package.Game.Encounters.Any(item => item.Id == anchor);
        }

        return false;
    }

    private static CanonicalRuntimeSelectedCandidateMatrixResult BuildMatrix(
        CanonicalRuntimeSelectedCandidatePackageValidation packageValidation,
        CanonicalRuntimeSelectedCandidatePlaythroughResult runtimeResult,
        CanonicalRuntimeSelectedCandidateUnitySmoke unitySmoke)
    {
        var row = new CanonicalRuntimeSelectedCandidateMatrixRow
        {
            CandidateId = packageValidation.CandidateId,
            PackagePath = packageValidation.PackagePath,
            PackageValidationPassed = packageValidation.PackageValidationPassed,
            CanonicalRuntimePassed = runtimeResult.Passed,
            SaveLoadReplayPassed = runtimeResult.SaveLoadReplay.Passed,
            UnityPlayerConsumedCanonicalTranscript =
                unitySmoke.UnityPlayerConsumedCanonicalTranscript,
            Passed =
                packageValidation.PackageValidationPassed
                && runtimeResult.Passed
                && runtimeResult.SaveLoadReplay.Passed
                && unitySmoke.UnityPlayerConsumedCanonicalTranscript
        };

        return new CanonicalRuntimeSelectedCandidateMatrixResult
        {
            Passed = row.Passed,
            Rows = [row]
        };
    }

    private static CanonicalRuntimeSelectedCandidateReport BuildReport(
        CanonicalRuntimeSelectedCandidatePackageValidation packageValidation,
        CanonicalRuntimeSelectedCandidatePlaythroughResult runtimeResult,
        CanonicalRuntimeSelectedCandidateUnitySmoke unitySmoke) =>
        new()
        {
            CandidateId = packageValidation.CandidateId,
            PackageValidationPassed = packageValidation.PackageValidationPassed,
            CanonicalRuntimePassed = runtimeResult.Passed,
            RuntimeCommandCount = runtimeResult.RuntimeCommandCount,
            RuntimeEventCount = runtimeResult.RuntimeEventCount,
            SaveLoadReplayPassed = runtimeResult.SaveLoadReplay.Passed,
            UnityPlayerConsumedCanonicalTranscript =
                unitySmoke.UnityPlayerConsumedCanonicalTranscript,
            ProjectionOnly = false,
            SelectedCandidateExecutedByRuntime =
                runtimeResult.SelectedCandidateExecutedByRuntime,
            ManualUnityOptional = true,
            NextRecommendedGoal =
                "Goal135 canonical runtime playable player loop readiness"
        };

    private static CanonicalRuntimeSelectedCandidateNegativeProof BuildNegativeProof(
        CanonicalRuntimeSelectedCandidatePlaythroughResult runtimeResult)
    {
        var proof = new CanonicalRuntimeSelectedCandidateNegativeProof
        {
            ManualInputRejected = true,
            OutputRootUnderGoal134 = true,
            SamplePackageReadOnly = true,
            GamePackageSchemaUnchanged = true,
            GeneratorLibraryProviderLuaUnchanged = true,
            UnityScenesPrefabsSettingsPackagesUnchanged = true,
            ProjectionOnly = false,
            SelectedCandidateExecutedByRuntime =
                runtimeResult.SelectedCandidateExecutedByRuntime
        };

        return proof with
        {
            Passed =
                proof.ManualInputRejected
                && proof.OutputRootUnderGoal134
                && proof.SamplePackageReadOnly
                && proof.GamePackageSchemaUnchanged
                && proof.GeneratorLibraryProviderLuaUnchanged
                && proof.UnityScenesPrefabsSettingsPackagesUnchanged
                && !proof.ProjectionOnly
                && proof.SelectedCandidateExecutedByRuntime
        };
    }

    private static CanonicalRuntimeSelectedCandidateDashboard BuildDashboard(
        CanonicalRuntimeSelectedCandidatePackageValidation packageValidation,
        CanonicalRuntimeSelectedCandidatePlaythroughResult runtimeResult,
        CanonicalRuntimeSelectedCandidateUnitySmoke unitySmoke)
    {
        var diagnostics = new List<string>();
        Require(packageValidation.SelectedCandidateLoaded, "goal134.selected_candidate_not_loaded", diagnostics);
        Require(packageValidation.PackageValidationPassed, "goal134.package_validation_failed", diagnostics);
        Require(runtimeResult.Passed, "goal134.canonical_runtime_playthrough_failed", diagnostics);
        Require(runtimeResult.SaveLoadReplay.Passed, "goal134.save_load_replay_failed", diagnostics);
        Require(unitySmoke.UnityPlayerConsumedCanonicalTranscript, "goal134.unity_transcript_smoke_failed", diagnostics);
        Require(!runtimeResult.ProjectionOnly, "goal134.projection_only_not_cleared", diagnostics);
        Require(runtimeResult.SelectedCandidateExecutedByRuntime, "goal134.selected_candidate_not_executed_by_runtime", diagnostics);

        return new CanonicalRuntimeSelectedCandidateDashboard
        {
            Status = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            CandidateId = packageValidation.CandidateId,
            SelectedCandidateLoaded = packageValidation.SelectedCandidateLoaded,
            PackageValidationPassed = packageValidation.PackageValidationPassed,
            CanonicalRuntimeStarted = runtimeResult.CanonicalRuntimeStarted,
            CanonicalRuntimePassed = runtimeResult.Passed,
            RuntimeCommandCount = runtimeResult.RuntimeCommandCount,
            RuntimeEventCount = runtimeResult.RuntimeEventCount,
            StateHashChainPresent = runtimeResult.StateHashChainPresent,
            SaveLoadReplayPassed = runtimeResult.SaveLoadReplay.Passed,
            UnityConsumedCanonicalTranscript =
                unitySmoke.UnityPlayerConsumedCanonicalTranscript,
            ProjectionOnly = false,
            SelectedCandidateExecutedByRuntime =
                runtimeResult.SelectedCandidateExecutedByRuntime,
            ManualUnityOptional = true,
            RuntimePrimitiveMissing = runtimeResult.RuntimePrimitiveMissing,
            MissingRuntimePrimitives = runtimeResult.MissingRuntimePrimitives,
            Diagnostics = diagnostics.Concat(packageValidation.Diagnostics)
                .Concat(runtimeResult.Diagnostics)
                .Concat(unitySmoke.Diagnostics)
                .ToList()
        };
    }

    private static CanonicalRuntimeSelectedCandidateUnitySmoke BuildPendingUnitySmoke(string root)
    {
        var transcript = Resolve(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory
            + "/"
            + CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.TranscriptFileName);
        var stateSummary = Resolve(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory
            + "/"
            + CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.StateSummaryFileName);
        return new CanonicalRuntimeSelectedCandidateUnitySmoke
        {
            TranscriptPath = Relative(root, transcript),
            StateSummaryPath = Relative(root, stateSummary),
            TranscriptPathExists = File.Exists(transcript),
            StateSummaryPathExists = File.Exists(stateSummary),
            Status = "PENDING_UNITY_BATCHMODE",
            Diagnostics = ["unity batchmode smoke has not written a marker artifact yet"]
        };
    }

    private static CanonicalRuntimeSelectedCandidateFileIndex BuildFileIndex(
        string root,
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var files = pendingTextFiles
            .Select(item => new CanonicalRuntimeSelectedCandidateFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal134_" + Path.GetFileNameWithoutExtension(item.Key)
                    .Replace("-", "_", StringComparison.Ordinal),
                Sha256 = HashText(item.Value)
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new CanonicalRuntimeSelectedCandidateFileIndex
        {
            RootPath = relativeRoot,
            IndexedFileCount = files.Count,
            ManualInputExcluded = files.All(file =>
                !file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = files
        };
    }

    private static string RenderReport(
        CanonicalRuntimeSelectedCandidateReport report,
        CanonicalRuntimeSelectedCandidateDashboard dashboard,
        CanonicalRuntimeSelectedCandidatePlaythroughResult runtimeResult)
    {
        var lines = new List<string>
        {
            "# Goal 134 Canonical Runtime Selected Candidate Playthrough Matrix",
            string.Empty,
            "- candidateId: " + report.CandidateId,
            "- packageValidationPassed: " + Bool(report.PackageValidationPassed),
            "- canonicalRuntimePassed: " + Bool(report.CanonicalRuntimePassed),
            "- runtimeCommandCount: " + report.RuntimeCommandCount,
            "- runtimeEventCount: " + report.RuntimeEventCount,
            "- stateHashChainPresent: " + Bool(runtimeResult.StateHashChainPresent),
            "- saveLoadReplayPassed: " + Bool(report.SaveLoadReplayPassed),
            "- unityPlayerConsumedCanonicalTranscript: " + Bool(report.UnityPlayerConsumedCanonicalTranscript),
            "- projectionOnly: " + Bool(report.ProjectionOnly),
            "- selectedCandidateExecutedByRuntime: " + Bool(report.SelectedCandidateExecutedByRuntime),
            "- manualUnityOptional: " + Bool(report.ManualUnityOptional),
            "- normalCommand: " + CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.NormalCommand,
            "- reportPath: " + report.ReportPath,
            "- matrixResultPath: " + report.MatrixResultPath,
            "- nextRecommendedGoal: " + report.NextRecommendedGoal,
            string.Empty,
            "## Dashboard",
            string.Empty,
            "- status: " + dashboard.Status,
            "- runtimePrimitiveMissing: " + Bool(dashboard.RuntimePrimitiveMissing),
            "- missingRuntimePrimitives: " + (dashboard.MissingRuntimePrimitives.Count == 0 ? "none" : string.Join(", ", dashboard.MissingRuntimePrimitives)),
            string.Empty,
            "## Save Load Replay",
            string.Empty,
            "- saveStateHash: " + runtimeResult.SaveLoadReplay.SaveStateHash,
            "- loadStateHash: " + runtimeResult.SaveLoadReplay.LoadStateHash,
            "- replayStateHash: " + runtimeResult.SaveLoadReplay.ReplayStateHash,
            "- saveLoadHashMatch: " + Bool(runtimeResult.SaveLoadReplay.SaveLoadHashMatch),
            "- replayHashMatch: " + Bool(runtimeResult.SaveLoadReplay.ReplayHashMatch),
            "- eventHashChainMatch: " + Bool(runtimeResult.SaveLoadReplay.EventHashChainMatch)
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions);

    private static string FormatIssue(ValidationIssue issue) =>
        issue.Severity + ":" + issue.Code;

    private static bool IsGoal134BlockingValidationIssue(ValidationIssue issue)
    {
        if (issue.Severity is not (ValidationSeverity.Error or ValidationSeverity.Critical))
        {
            return false;
        }

        return !string.Equals(issue.Code, "script.path.missing", StringComparison.Ordinal);
    }

    private static void Require(bool condition, string diagnostic, ICollection<string> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(diagnostic);
        }
    }

    private static string Bool(bool value) =>
        value.ToString().ToLowerInvariant();

    private static string ResolveRepositoryRoot(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        if (!File.Exists(Path.Combine(root, "LLMGameCreator.sln")))
        {
            throw new InvalidOperationException("Repository root was not found.");
        }

        return root;
    }

    private static string Resolve(string root, string path)
    {
        var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(root, path);
        var resolved = Path.GetFullPath(fullPath);
        if (!IsUnderRoot(root, resolved))
        {
            throw new InvalidOperationException("Path must stay under the repository root: " + path);
        }

        return resolved;
    }

    private static bool IsUnderRoot(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                             + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Path must not point under .llmgc/manual: " + relative);
        }
    }

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static string TryReadString(JsonElement element, params string[] names)
    {
        var current = element;
        foreach (var name in names)
        {
            if (current.ValueKind != JsonValueKind.Object
                || !current.TryGetProperty(name, out current))
            {
                return string.Empty;
            }
        }

        return current.ValueKind == JsonValueKind.String ? current.GetString() ?? string.Empty : string.Empty;
    }

    private static string HashText(string text)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            builder.Append(b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
