using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class RuntimeBackedPlayerCommandRoundtripArtifactService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static RuntimeBackedPlayerCommandRoundtripArtifactService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public static RuntimeBackedPlayerCommandRoundtripUnitySmoke ReadUnitySmoke(string path) =>
        JsonSerializer.Deserialize<RuntimeBackedPlayerCommandRoundtripUnitySmoke>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new RuntimeBackedPlayerCommandRoundtripUnitySmoke();

    public async Task<RuntimeBackedPlayerCommandRoundtripWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        RuntimeBackedPlayerCommandRoundtripRequest request,
        RuntimeBackedPlayerCommandRoundtripResult runtimeResult,
        string outputRootRelativePath =
            RuntimeBackedPlayerCommandRoundtripVocabulary.ProceduralOutputDirectory,
        string exportRootRelativePath =
            RuntimeBackedPlayerCommandRoundtripVocabulary.ExportPackageDirectory,
        RuntimeBackedPlayerCommandRoundtripUnitySmoke? unitySmoke = null,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var inputs = BuildInputs(root, request);
        runtimeResult.Inputs = inputs;
        var goal140Model = ReadJson<RuntimeBackedUnityPlayerLoopControlsUxModel>(
            Resolve(root, inputs.ControlsUxModelPath));
        var goal140Dashboard = ReadJson<RuntimeBackedUnityPlayerLoopControlsUxDashboard>(
            Resolve(root, RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DashboardRelativePath));
        var goal140Acceptance = BuildGoal140Acceptance(goal140Model, goal140Dashboard);
        var smoke = unitySmoke ?? BuildPendingUnitySmoke(root, outputRootRelativePath);
        var model = BuildModel(runtimeResult);
        var report = BuildReport(goal140Acceptance, runtimeResult, smoke);
        var dashboard = BuildDashboard(goal140Acceptance, runtimeResult, smoke);
        var negative = BuildNegativeProof(goal140Acceptance, runtimeResult);
        var markdown = RenderReport(report, dashboard, runtimeResult, negative);
        var goal140Markdown = RenderGoal140Acceptance(goal140Acceptance);
        var goal141Markdown = RenderGoal141ManualAcceptance(report, dashboard);
        var requestArtifact = new RuntimeBackedPlayerCommandRoundtripRequestArtifact
        {
            Inputs = inputs,
            ControlRequestBridgePresent = runtimeResult.ControlRequestBridgePresent,
            RequiredControlIntents = RuntimeBackedPlayerCommandRoundtripVocabulary.RequiredControlIntents,
            RequiredRuntimeCommandCoverage =
                RuntimeBackedPlayerCommandRoundtripVocabulary.RequiredRuntimeCommandCoverage,
            Requests = runtimeResult.Requests
        };

        var proceduralFiles = BuildFilePayloads(
            outputRootRelativePath,
            goal140Acceptance,
            requestArtifact,
            runtimeResult,
            model,
            dashboard,
            negative,
            smoke,
            report,
            markdown);
        var exportFiles = BuildFilePayloads(
            exportRootRelativePath,
            goal140Acceptance,
            requestArtifact,
            runtimeResult,
            model,
            dashboard,
            negative,
            smoke,
            report,
            markdown);

        var procedural = Resolve(root, outputRootRelativePath);
        var export = Resolve(root, exportRootRelativePath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in proceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardGoal141Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in exportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardGoal141Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        var goal140DocsPath = Resolve(
            root,
            RuntimeBackedPlayerCommandRoundtripVocabulary.Goal140DocumentationPath);
        var docsPath = Resolve(root, RuntimeBackedPlayerCommandRoundtripVocabulary.DocumentationPath);
        GuardGoal141Write(root, goal140DocsPath);
        GuardGoal141Write(root, docsPath);
        await WriteTextAsync(goal140DocsPath, goal140Markdown, cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(docsPath, goal141Markdown, cancellationToken).ConfigureAwait(false);
        written.Add(Relative(root, goal140DocsPath));
        written.Add(Relative(root, docsPath));

        return new RuntimeBackedPlayerCommandRoundtripWriteResult
        {
            Dashboard = dashboard,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            Goal140DocumentationPath = goal140DocsPath,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static RuntimeBackedPlayerCommandRoundtripInput BuildInputs(
        string root,
        RuntimeBackedPlayerCommandRoundtripRequest request)
    {
        var package = ResolveInput(root, request.PackagePath, "PackagePath");
        var handoff = ResolveInput(root, request.HandoffPath, "HandoffPath");
        var model = ResolveInput(root, request.ControlsUxModelPath, "ControlsUxModelPath");
        var result = ResolveInput(root, request.ControlsUxResultPath, "ControlsUxResultPath");
        var script = ResolveInput(root, request.ControlsUxScriptPath, "ControlsUxScriptPath");
        var snapshots = ResolveInput(root, request.CommandLoopSnapshotsPath, "CommandLoopSnapshotsPath");
        var commandLoopResult = ResolveInput(root, request.CommandLoopResultPath, "CommandLoopResultPath");
        return new RuntimeBackedPlayerCommandRoundtripInput
        {
            CandidateId = request.CandidateId,
            PackagePath = Relative(root, package),
            HandoffPath = Relative(root, handoff),
            ControlsUxModelPath = Relative(root, model),
            ControlsUxResultPath = Relative(root, result),
            ControlsUxScriptPath = Relative(root, script),
            CommandLoopSnapshotsPath = Relative(root, snapshots),
            CommandLoopResultPath = Relative(root, commandLoopResult),
            PackagePathExists = File.Exists(package),
            HandoffPathExists = File.Exists(handoff),
            ControlsUxModelPathExists = File.Exists(model),
            ControlsUxResultPathExists = File.Exists(result),
            ControlsUxScriptPathExists = File.Exists(script),
            CommandLoopSnapshotsPathExists = File.Exists(snapshots),
            CommandLoopResultPathExists = File.Exists(commandLoopResult)
        };
    }

    private static RuntimeBackedPlayerCommandRoundtripGoal140AcceptanceRecord BuildGoal140Acceptance(
        RuntimeBackedUnityPlayerLoopControlsUxModel model,
        RuntimeBackedUnityPlayerLoopControlsUxDashboard dashboard) =>
        new()
        {
            SelectedCandidate = model.CandidateId,
            Frames = model.FrameCount,
            HumanReadableFrameNumbering = model.HumanReadableFrameNumbering,
            StepOnceSemanticsClear = model.StepOnceSemanticsClear,
            PlayAllToEndSemanticsClear = model.PlayAllToEndSemanticsClear,
            CopyFrameSummaryStatusPresent = model.CopyFrameSummaryStatusPresent,
            KnownUnityEditorNoiseClassified = dashboard.KnownUnityEditorNoiseClassified,
            BlockingUnityErrorCount = dashboard.BlockingUnityErrorCount,
            ProjectionOnly = false,
            RuntimeAuthority = true,
            UnityGameplayTruth = false
        };

    private static RuntimeBackedPlayerCommandRoundtripModel BuildModel(
        RuntimeBackedPlayerCommandRoundtripResult result)
    {
        var currentResponse = result.Responses.FirstOrDefault()
                              ?? new RuntimeBackedPlayerCommandRoundtripResponse();
        return new RuntimeBackedPlayerCommandRoundtripModel
        {
            CandidateId = result.CandidateId,
            RoundtripRequestCount = result.RoundtripRequestCount,
            RuntimeExecutedRequestCount = result.RuntimeExecutedRequestCount,
            RoundtripSnapshotCount = result.RoundtripSnapshotCount,
            CurrentRequest = result.Requests.FirstOrDefault()
                             ?? new RuntimeBackedPlayerCommandRoundtripControlRequest(),
            CurrentResponseSnapshot = currentResponse.Snapshot,
            Status = result.RuntimeBackedPlayerCommandRoundtripPassed ? "GREEN" : "BLOCKED",
            Requests = result.Requests,
            Responses = result.Responses,
            StateHashChainPresent = result.StateHashChainPresent,
            RuntimeAuthority = result.RuntimeAuthority,
            ProjectionOnly = result.ProjectionOnly,
            UnityGameplayTruth = result.UnityGameplayTruth,
            ControlRequestBridgePresent = result.ControlRequestBridgePresent,
            UnityConsumesRoundtripResult = result.UnityConsumesRoundtripResult
        };
    }

    private static RuntimeBackedPlayerCommandRoundtripUnitySmoke BuildPendingUnitySmoke(
        string root,
        string outputRootRelativePath)
    {
        var model = Resolve(
            root,
            outputRootRelativePath + "/" + RuntimeBackedPlayerCommandRoundtripVocabulary.ModelFileName);
        var result = Resolve(
            root,
            outputRootRelativePath + "/" + RuntimeBackedPlayerCommandRoundtripVocabulary.ResultFileName);
        return new RuntimeBackedPlayerCommandRoundtripUnitySmoke
        {
            UnityAvailable = true,
            ModelPathExists = File.Exists(model),
            ModelPath = Relative(root, model),
            ResultPath = Relative(root, result),
            Status = "PENDING_UNITY_BATCHMODE",
            Diagnostics = ["Unity command roundtrip smoke has not written a marker artifact yet"]
        };
    }

    private static RuntimeBackedPlayerCommandRoundtripReport BuildReport(
        RuntimeBackedPlayerCommandRoundtripGoal140AcceptanceRecord goal140,
        RuntimeBackedPlayerCommandRoundtripResult result,
        RuntimeBackedPlayerCommandRoundtripUnitySmoke smoke) =>
        new()
        {
            Status = BuildDiagnostics(goal140, result, smoke).Count == 0 ? "GREEN" : "BLOCKED",
            Accepted = false,
            Goal140Accepted = Goal140Accepted(goal140),
            CandidateId = result.CandidateId,
            RoundtripRequestCount = result.RoundtripRequestCount,
            RuntimeExecutedRequestCount = result.RuntimeExecutedRequestCount,
            RoundtripSnapshotCount = result.RoundtripSnapshotCount,
            ControlRequestBridgePresent = result.ControlRequestBridgePresent,
            StateHashChainPresent = result.StateHashChainPresent,
            RuntimeAuthority = result.RuntimeAuthority,
            ProjectionOnly = result.ProjectionOnly,
            UnityGameplayTruth = result.UnityGameplayTruth,
            UnityConsumesRoundtripResult = result.UnityConsumesRoundtripResult,
            UnitySmokePassed = smoke.Passed,
            ManualUnityOptional = true
        };

    private static RuntimeBackedPlayerCommandRoundtripDashboard BuildDashboard(
        RuntimeBackedPlayerCommandRoundtripGoal140AcceptanceRecord goal140,
        RuntimeBackedPlayerCommandRoundtripResult result,
        RuntimeBackedPlayerCommandRoundtripUnitySmoke smoke)
    {
        var diagnostics = BuildDiagnostics(goal140, result, smoke);
        return new RuntimeBackedPlayerCommandRoundtripDashboard
        {
            Status = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            Accepted = false,
            Goal140Accepted = Goal140Accepted(goal140),
            CandidateId = result.CandidateId,
            RoundtripRequestCount = result.RoundtripRequestCount,
            RuntimeExecutedRequestCount = result.RuntimeExecutedRequestCount,
            RoundtripSnapshotCount = result.RoundtripSnapshotCount,
            ControlRequestBridgePresent = result.ControlRequestBridgePresent,
            StateHashChainPresent = result.StateHashChainPresent,
            RuntimeAuthority = result.RuntimeAuthority,
            ProjectionOnly = result.ProjectionOnly,
            UnityGameplayTruth = result.UnityGameplayTruth,
            UnityConsumesRoundtripResult = result.UnityConsumesRoundtripResult,
            UnitySmokePassed = smoke.Passed,
            NoUnclassifiedErrorDiagnostics = result.NoUnclassifiedErrorDiagnostics,
            ManualUnityOptional = true,
            MissingControlIntents = result.MissingControlIntents,
            MissingRuntimeCommandCoverage = result.MissingRuntimeCommandCoverage,
            Diagnostics = diagnostics.Concat(result.Diagnostics).Concat(smoke.Diagnostics).ToList()
        };
    }

    private static RuntimeBackedPlayerCommandRoundtripNegativeProof BuildNegativeProof(
        RuntimeBackedPlayerCommandRoundtripGoal140AcceptanceRecord goal140,
        RuntimeBackedPlayerCommandRoundtripResult result)
    {
        var proof = new RuntimeBackedPlayerCommandRoundtripNegativeProof
        {
            ManualInputRejected = true,
            RawManualInputNotCommitted = goal140.RawManualInputNotCommitted,
            OutputRootUnderGoal141 = true,
            SamplePackageReadOnly = true,
            GamePackageSchemaUnchanged = true,
            GeneratorLibraryProviderLuaUnchanged = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged = true,
            RuntimeOwnsRoundtripExecution = result.RuntimeExecutedRequestCount >= 6,
            UnityConsumesResultOnly = result.UnityConsumesRoundtripResult,
            RuntimeAuthority = result.RuntimeAuthority,
            ProjectionOnly = result.ProjectionOnly,
            UnityGameplayTruth = result.UnityGameplayTruth
        };
        return proof with
        {
            Passed = proof.ManualInputRejected
                     && proof.RawManualInputNotCommitted
                     && proof.OutputRootUnderGoal141
                     && proof.SamplePackageReadOnly
                     && proof.GamePackageSchemaUnchanged
                     && proof.GeneratorLibraryProviderLuaUnchanged
                     && proof.UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged
                     && proof.RuntimeOwnsRoundtripExecution
                     && proof.UnityConsumesResultOnly
                     && proof.RuntimeAuthority
                     && !proof.ProjectionOnly
                     && !proof.UnityGameplayTruth
        };
    }

    private static IReadOnlyList<string> BuildDiagnostics(
        RuntimeBackedPlayerCommandRoundtripGoal140AcceptanceRecord goal140,
        RuntimeBackedPlayerCommandRoundtripResult result,
        RuntimeBackedPlayerCommandRoundtripUnitySmoke smoke)
    {
        var diagnostics = new List<string>();
        Require(Goal140Accepted(goal140), "goal141.goal140_acceptance_record_invalid", diagnostics);
        Require(result.RoundtripRequestCount >= 6, "goal141.roundtrip_request_count", diagnostics);
        Require(result.RuntimeExecutedRequestCount >= 6, "goal141.runtime_executed_request_count", diagnostics);
        Require(result.RoundtripSnapshotCount >= result.RuntimeExecutedRequestCount,
            "goal141.roundtrip_snapshot_count",
            diagnostics);
        Require(result.ControlRequestBridgePresent, "goal141.control_request_bridge", diagnostics);
        Require(result.StateHashChainPresent, "goal141.state_hash_chain", diagnostics);
        Require(result.RuntimeAuthority, "goal141.runtime_authority", diagnostics);
        Require(!result.ProjectionOnly, "goal141.projection_only", diagnostics);
        Require(!result.UnityGameplayTruth, "goal141.unity_gameplay_truth", diagnostics);
        Require(result.UnityConsumesRoundtripResult, "goal141.unity_consumes_roundtrip_result", diagnostics);
        Require(result.NoUnclassifiedErrorDiagnostics,
            "goal141.unclassified_runtime_diagnostics",
            diagnostics);
        Require(smoke.Passed && smoke.Status == "GREEN", "goal141.unity_smoke_not_green", diagnostics);
        Require(smoke.UnityConsumesRoundtripResult, "goal141.unity_smoke_did_not_consume_result", diagnostics);
        Require(!smoke.UnityGameplayTruth, "goal141.unity_smoke_gameplay_truth", diagnostics);
        foreach (var missing in result.MissingControlIntents)
        {
            diagnostics.Add("goal141.missing_control_intent:" + missing);
        }

        foreach (var missing in result.MissingRuntimeCommandCoverage)
        {
            diagnostics.Add("goal141.missing_runtime_command_coverage:" + missing);
        }

        return diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static SortedDictionary<string, string> BuildFilePayloads(
        string relativeRoot,
        RuntimeBackedPlayerCommandRoundtripGoal140AcceptanceRecord goal140Acceptance,
        RuntimeBackedPlayerCommandRoundtripRequestArtifact request,
        RuntimeBackedPlayerCommandRoundtripResult result,
        RuntimeBackedPlayerCommandRoundtripModel model,
        RuntimeBackedPlayerCommandRoundtripDashboard dashboard,
        RuntimeBackedPlayerCommandRoundtripNegativeProof negative,
        RuntimeBackedPlayerCommandRoundtripUnitySmoke smoke,
        RuntimeBackedPlayerCommandRoundtripReport report,
        string reportMarkdown)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [RuntimeBackedPlayerCommandRoundtripVocabulary.Goal140AcceptanceFileName] =
                Serialize(goal140Acceptance),
            [RuntimeBackedPlayerCommandRoundtripVocabulary.RequestFileName] =
                Serialize(request),
            [RuntimeBackedPlayerCommandRoundtripVocabulary.ResultFileName] =
                Serialize(result),
            [RuntimeBackedPlayerCommandRoundtripVocabulary.SessionFileName] =
                Serialize(result.Session),
            [RuntimeBackedPlayerCommandRoundtripVocabulary.SnapshotsFileName] =
                Serialize(result.Snapshots),
            [RuntimeBackedPlayerCommandRoundtripVocabulary.ModelFileName] =
                Serialize(model),
            [RuntimeBackedPlayerCommandRoundtripVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [RuntimeBackedPlayerCommandRoundtripVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [RuntimeBackedPlayerCommandRoundtripVocabulary.UnitySmokeFileName] =
                Serialize(smoke),
            [RuntimeBackedPlayerCommandRoundtripVocabulary.ReportJsonFileName] =
                Serialize(report),
            [RuntimeBackedPlayerCommandRoundtripVocabulary.ReportMarkdownFileName] =
                reportMarkdown
        };
        files[RuntimeBackedPlayerCommandRoundtripVocabulary.FileIndexFileName] =
            Serialize(BuildFileIndex(relativeRoot, files));
        return files;
    }

    private static RuntimeBackedPlayerCommandRoundtripFileIndex BuildFileIndex(
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var files = pendingTextFiles
            .Select(item => new RuntimeBackedPlayerCommandRoundtripFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal141_" + Path.GetFileNameWithoutExtension(item.Key)
                    .Replace("-", "_", StringComparison.Ordinal),
                Sha256 = HashText(item.Value)
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new RuntimeBackedPlayerCommandRoundtripFileIndex
        {
            RootPath = relativeRoot,
            IndexedFileCount = files.Count,
            ManualInputExcluded = files.All(file =>
                !file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = files
        };
    }

    private static string RenderReport(
        RuntimeBackedPlayerCommandRoundtripReport report,
        RuntimeBackedPlayerCommandRoundtripDashboard dashboard,
        RuntimeBackedPlayerCommandRoundtripResult result,
        RuntimeBackedPlayerCommandRoundtripNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 141 Runtime-backed Unity Player Command Roundtrip Bridge",
            string.Empty,
            "- status: " + dashboard.Status,
            "- accepted: false",
            "- goal140Accepted: " + Bool(report.Goal140Accepted),
            "- candidateId: " + report.CandidateId,
            "- roundtripRequestCount: " + report.RoundtripRequestCount,
            "- runtimeExecutedRequestCount: " + report.RuntimeExecutedRequestCount,
            "- roundtripSnapshotCount: " + report.RoundtripSnapshotCount,
            "- controlRequestBridgePresent: " + Bool(report.ControlRequestBridgePresent),
            "- stateHashChainPresent: " + Bool(report.StateHashChainPresent),
            "- runtimeAuthority: " + Bool(report.RuntimeAuthority),
            "- projectionOnly: " + Bool(report.ProjectionOnly),
            "- unityGameplayTruth: " + Bool(report.UnityGameplayTruth),
            "- unityConsumesRoundtripResult: " + Bool(report.UnityConsumesRoundtripResult),
            "- unitySmokePassed: " + Bool(report.UnitySmokePassed),
            "- manualUnityOptional: true",
            "- normalCommand: " + report.NormalCommand,
            "- reportPath: " + report.ReportPath,
            "- negativeProofPassed: " + Bool(negative.Passed),
            string.Empty,
            "## Requests",
            string.Empty
        };
        lines.AddRange(result.Responses.Select(response =>
            "- " + response.RequestIndex
            + " " + response.ControlIntent
            + " -> " + response.RuntimeCommandCoverage
            + "; status=" + response.Status
            + "; snapshotHash=" + response.Snapshot.StateHashAfter
            + "; runtimeExecuted=" + Bool(response.RuntimeExecuted)));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(dashboard.Diagnostics.Count == 0
            ? ["- none"]
            : dashboard.Diagnostics.Select(item => "- " + item));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderGoal140Acceptance(
        RuntimeBackedPlayerCommandRoundtripGoal140AcceptanceRecord acceptance)
    {
        var lines = new List<string>
        {
            "# Goal 140 Runtime-backed Unity Player Loop Controls UX Polish And Noise Guard",
            string.Empty,
            "accepted=true",
            "acceptedByHuman=true",
            "acceptedByCodex=false",
            "rawManualInputNotCommitted=true",
            "selectedCandidate=" + acceptance.SelectedCandidate,
            "frames=" + acceptance.Frames.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "humanReadableFrameNumbering=" + Bool(acceptance.HumanReadableFrameNumbering),
            "stepOnceSemanticsClear=" + Bool(acceptance.StepOnceSemanticsClear),
            "playAllToEndSemanticsClear=" + Bool(acceptance.PlayAllToEndSemanticsClear),
            "copyFrameSummaryStatusPresent=" + Bool(acceptance.CopyFrameSummaryStatusPresent),
            "knownUnityEditorNoiseClassified=" + Bool(acceptance.KnownUnityEditorNoiseClassified),
            "blockingUnityErrorCount=" + acceptance.BlockingUnityErrorCount,
            "projectionOnly=false",
            "runtimeAuthority=true",
            "unityGameplayTruth=false",
            string.Empty,
            "Source: Goal141 task handoff recorded owner acceptance of Goal140. Raw manual input remains outside committed artifacts."
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderGoal141ManualAcceptance(
        RuntimeBackedPlayerCommandRoundtripReport report,
        RuntimeBackedPlayerCommandRoundtripDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Goal 141 Runtime-backed Unity Player Command Roundtrip Bridge",
            string.Empty,
            "accepted=false",
            "acceptedByHuman=false",
            "acceptedByCodex=false",
            "goal140Accepted=" + Bool(report.Goal140Accepted),
            "candidateId=" + report.CandidateId,
            "roundtripRequestCount=" + report.RoundtripRequestCount,
            "runtimeExecutedRequestCount=" + report.RuntimeExecutedRequestCount,
            "roundtripSnapshotCount=" + report.RoundtripSnapshotCount,
            "controlRequestBridgePresent=" + Bool(report.ControlRequestBridgePresent),
            "stateHashChainPresent=" + Bool(report.StateHashChainPresent),
            "runtimeAuthority=" + Bool(report.RuntimeAuthority),
            "projectionOnly=false",
            "unityGameplayTruth=false",
            "unityConsumesRoundtripResult=" + Bool(report.UnityConsumesRoundtripResult),
            "manualUnityOptional=true",
            "normalCommand=" + report.NormalCommand,
            "reportPath=" + report.ReportPath,
            "status=" + dashboard.Status
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static bool Goal140Accepted(RuntimeBackedPlayerCommandRoundtripGoal140AcceptanceRecord goal140) =>
        goal140.Accepted
        && goal140.AcceptedByHuman
        && !goal140.AcceptedByCodex
        && goal140.RawManualInputNotCommitted
        && goal140.SelectedCandidate == "minimal-map-game-balanced-baseline"
        && goal140.Frames == 13
        && goal140.HumanReadableFrameNumbering
        && goal140.StepOnceSemanticsClear
        && goal140.PlayAllToEndSemanticsClear
        && goal140.CopyFrameSummaryStatusPresent
        && goal140.KnownUnityEditorNoiseClassified
        && goal140.BlockingUnityErrorCount == 0
        && !goal140.ProjectionOnly
        && goal140.RuntimeAuthority
        && !goal140.UnityGameplayTruth;

    private static T ReadJson<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
        ?? throw new InvalidOperationException("Could not deserialize " + path);

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
    }

    private static string ResolveRepositoryRoot(string path)
    {
        var root = Path.GetFullPath(path);
        if (!File.Exists(Path.Combine(root, "LLMGameCreator.sln")))
        {
            throw new InvalidOperationException("Repository root was not found: " + root);
        }

        return root;
    }

    private static string ResolveInput(string root, string path, string name)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException(name + " is required.");
        }

        var full = Resolve(root, path);
        GuardNotManualInput(root, full);
        return full;
    }

    private static string Resolve(string root, string path)
    {
        var full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
        if (!IsUnderRoot(root, full))
        {
            throw new InvalidOperationException("Path must stay under repository root: " + path);
        }

        return full;
    }

    private static void GuardGoal141Write(string root, string path)
    {
        GuardNotManualInput(root, path);
        var relative = Relative(root, path);
        if (relative.StartsWith(
                RuntimeBackedPlayerCommandRoundtripVocabulary.ProceduralOutputDirectory + "/",
                StringComparison.Ordinal)
            || relative.StartsWith(
                RuntimeBackedPlayerCommandRoundtripVocabulary.ExportPackageDirectory + "/",
                StringComparison.Ordinal)
            || relative == RuntimeBackedPlayerCommandRoundtripVocabulary.Goal140DocumentationPath
            || relative == RuntimeBackedPlayerCommandRoundtripVocabulary.DocumentationPath)
        {
            return;
        }

        throw new InvalidOperationException("Goal141 writer refused path: " + relative);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Manual input path is not allowed: " + relative);
        }
    }

    private static bool IsUnderRoot(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(path);
        return full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase);
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static void Require(bool condition, string diagnostic, ICollection<string> diagnostics)
    {
        if (!condition && !diagnostics.Contains(diagnostic))
        {
            diagnostics.Add(diagnostic);
        }
    }

    private static string Bool(bool value) => value.ToString().ToLowerInvariant();

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
