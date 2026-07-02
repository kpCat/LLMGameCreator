namespace LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;

internal sealed class EditDrivenUnityAlphaStreamingAssetsHandoffReadValidator
{
    public EditDrivenUnityAlphaStreamingAssetsHandoffProbeReadProof ValidateMirroredPayload(
        string repositoryRootPath,
        Goal082SourceContext context)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var payloadRoot = Path.Combine(
            root,
            Normalize(EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.StreamingAssetsRelativeRoot));
        var payloadFiles = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var fileName in EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredUnityPayloadFileNames)
        {
            var path = Path.Combine(payloadRoot, Normalize(fileName));
            if (File.Exists(path))
            {
                payloadFiles[fileName] = File.ReadAllText(path);
            }
        }

        return ValidatePayloadFiles(root, context, payloadFiles, payloadReadAttempted: true);
    }

    public EditDrivenUnityAlphaStreamingAssetsHandoffProbeReadProof ValidatePayloadFiles(
        string repositoryRootPath,
        Goal082SourceContext context,
        IReadOnlyDictionary<string, string> payloadFiles,
        bool payloadReadAttempted)
    {
        var diagnostics = new List<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic>();
        if (!payloadReadAttempted)
        {
            diagnostics.Add(Error(
                "goal082.probe.payload_read_required",
                "StreamingAssets",
                "Unity handoff proof must read the mirrored StreamingAssets payload."));
        }

        var requiredPresent = EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredUnityPayloadFileNames
            .All(payloadFiles.ContainsKey);
        foreach (var missing in EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredUnityPayloadFileNames
                     .Where(fileName => !payloadFiles.ContainsKey(fileName)))
        {
            diagnostics.Add(Error(
                "goal082.probe.payload_file_missing",
                missing,
                "Required Unity StreamingAssets payload file was not read."));
        }

        var handoff = Read<EditDrivenUnityAlphaStreamingAssetsHandoffPayloadManifest>(
            payloadFiles,
            "handoff-manifest.json",
            diagnostics);
        var expected = Read<EditDrivenUnityAlphaStreamingAssetsHandoffExpectedHashes>(
            payloadFiles,
            "expected-hashes.json",
            diagnostics);
        var projected = Read<EditDrivenUnityAlphaStreamingAssetsHandoffProjectedPackageIndexPayload>(
            payloadFiles,
            "projected-package-index.json",
            diagnostics);
        var command = Read<EditDrivenUnityAlphaStreamingAssetsHandoffCommandIndexPayload>(
            payloadFiles,
            "playthrough-command-index.json",
            diagnostics);
        var transcript = Read<EditDrivenUnityAlphaStreamingAssetsHandoffTranscriptIndexPayload>(
            payloadFiles,
            "playthrough-transcript-index.json",
            diagnostics);

        var expectedHashesHash = payloadFiles.TryGetValue("expected-hashes.json", out var expectedJson)
            ? Hash(expectedJson)
            : string.Empty;
        var projectedHash = payloadFiles.TryGetValue("projected-package-index.json", out var projectedJson)
            ? Hash(projectedJson)
            : string.Empty;
        var commandHash = payloadFiles.TryGetValue("playthrough-command-index.json", out var commandJson)
            ? Hash(commandJson)
            : string.Empty;
        var transcriptHash = payloadFiles.TryGetValue("playthrough-transcript-index.json", out var transcriptJson)
            ? Hash(transcriptJson)
            : string.Empty;

        var hashesMatchExpected = expected is not null
                                  && handoff is not null
                                  && handoff.ExpectedHashesHash == expectedHashesHash
                                  && expected.ProjectedPackageIndexPayloadHash == projectedHash
                                  && expected.PlaythroughCommandIndexPayloadHash == commandHash
                                  && expected.PlaythroughTranscriptIndexPayloadHash == transcriptHash;
        var packageHashMatchesGoal080 = expected?.ProjectedPackageHash == context.ProjectedPackageHash
                                        && projected?.ProjectedPackageHash == context.ProjectedPackageHash
                                        && handoff?.ProjectedPackageHash == context.ProjectedPackageHash;
        var commandHashMatchesGoal081 = expected?.Goal081CommandScriptHash == context.CommandScriptHash
                                        && command?.CommandScriptHash == context.CommandScriptHash;
        var transcriptHashMatchesGoal081 = expected?.Goal081TranscriptHash == context.TranscriptHash
                                           && transcript?.TranscriptHash == context.TranscriptHash;
        var stateHashMatchesGoal081 = expected?.Goal081StateHashChainHash == context.StateHashChainHash
                                      && transcript?.StateHashChainHash == context.StateHashChainHash
                                      && expected?.FinalCoverageStateHash == context.FinalCoverageStateHash
                                      && transcript?.FinalCoverageStateHash == context.FinalCoverageStateHash
                                      && expected?.ReplayFinalStateHash == context.ReplayFinalStateHash
                                      && transcript?.ReplayFinalStateHash == context.ReplayFinalStateHash;
        var countsMatch = handoff?.RowCount == context.RowCount
                          && handoff.TargetCount == context.TargetCount
                          && handoff.Goal078ActionCount == context.Goal078ActionCount
                          && handoff.CommandCount == context.CommandCount
                          && expected?.RowCount == context.RowCount
                          && expected.TargetCount == context.TargetCount
                          && expected.Goal078ActionCount == context.Goal078ActionCount
                          && expected.CommandCount == context.CommandCount
                          && projected?.RowCount == context.RowCount
                          && projected.TargetCount == context.TargetCount
                          && projected.ActionCount == context.Goal078ActionCount
                          && command?.RowCount == context.RowCount
                          && command.TargetCount == context.TargetCount
                          && command.Goal078ActionCount == context.Goal078ActionCount
                          && command.CommandCount == context.CommandCount
                          && transcript?.CoveredRowCount == context.RowCount
                          && transcript.CoveredTargetCount == context.TargetCount
                          && transcript.CoveredGoal078ActionCount == context.Goal078ActionCount;
        var probeSource = SafeRead(
            repositoryRootPath,
            EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityProbeScriptPath);
        var probeReferencesRoot = probeSource.Contains("Application.streamingAssetsPath", StringComparison.Ordinal)
                                  && probeSource.Contains(
                                      EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityStreamingAssetsProbeRoot,
                                      StringComparison.Ordinal);
        var probeNoBootstrap = !probeSource.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal);

        AddIfFalse(diagnostics, hashesMatchExpected, "goal082.probe.payload_hash_mismatch", "expected-hashes.json", "Mirrored payload hashes do not match expected-hashes.json.");
        AddIfFalse(diagnostics, packageHashMatchesGoal080, "goal082.probe.package_hash_mismatch", "projected-package-index.json", "Projected package hash does not match Goal080 source evidence.");
        AddIfFalse(diagnostics, commandHashMatchesGoal081, "goal082.probe.command_hash_mismatch", "playthrough-command-index.json", "Command script hash does not match Goal081 source evidence.");
        AddIfFalse(diagnostics, transcriptHashMatchesGoal081, "goal082.probe.transcript_hash_mismatch", "playthrough-transcript-index.json", "Transcript hash does not match Goal081 source evidence.");
        AddIfFalse(diagnostics, stateHashMatchesGoal081, "goal082.probe.state_hash_mismatch", "playthrough-transcript-index.json", "State hash chain values do not match Goal081 source evidence.");
        AddIfFalse(diagnostics, countsMatch, "goal082.probe.count_mismatch", "handoff-manifest.json", "Row, target, action, or command counts do not match Goal080/Goal081 evidence.");
        AddIfFalse(diagnostics, probeReferencesRoot, "goal082.probe.source_streamingassets_missing", EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityProbeScriptPath, "Unity probe source must use Application.streamingAssetsPath and the Goal082 payload root.");
        AddIfFalse(diagnostics, probeNoBootstrap, "goal082.probe.source_bootstrap_dependency", EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityProbeScriptPath, "Unity probe source must not depend on AlphaRuntimeBootstrap.");

        return new EditDrivenUnityAlphaStreamingAssetsHandoffProbeReadProof
        {
            Passed = diagnostics.Count == 0
                     && payloadReadAttempted
                     && requiredPresent
                     && hashesMatchExpected
                     && packageHashMatchesGoal080
                     && commandHashMatchesGoal081
                     && transcriptHashMatchesGoal081
                     && stateHashMatchesGoal081
                     && countsMatch
                     && probeReferencesRoot
                     && probeNoBootstrap,
            PayloadReadAttempted = payloadReadAttempted,
            HandoffManifestRead = handoff is not null,
            ExpectedHashesRead = expected is not null,
            ProjectedPackageIndexRead = projected is not null,
            PlaythroughCommandIndexRead = command is not null,
            PlaythroughTranscriptIndexRead = transcript is not null,
            RequiredPayloadFilesPresent = requiredPresent,
            PayloadFileHashesMatchExpected = hashesMatchExpected,
            PackageHashMatchesGoal080 = packageHashMatchesGoal080,
            CommandHashMatchesGoal081 = commandHashMatchesGoal081,
            TranscriptHashMatchesGoal081 = transcriptHashMatchesGoal081,
            StateHashMatchesGoal081 = stateHashMatchesGoal081,
            CountsMatchExpected = countsMatch,
            UnityProbeSourceReferencesStreamingAssetsRoot = probeReferencesRoot,
            UnityProbeSourceDoesNotReferenceAlphaRuntimeBootstrap = probeNoBootstrap,
            PayloadFileCount = payloadFiles.Count,
            RowCount = handoff?.RowCount ?? 0,
            TargetCount = handoff?.TargetCount ?? 0,
            Goal078ActionCount = handoff?.Goal078ActionCount ?? 0,
            CommandCount = handoff?.CommandCount ?? 0,
            ProjectedPackageHash = projected?.ProjectedPackageHash ?? string.Empty,
            CommandScriptHash = command?.CommandScriptHash ?? string.Empty,
            TranscriptHash = transcript?.TranscriptHash ?? string.Empty,
            StateHashChainHash = transcript?.StateHashChainHash ?? string.Empty,
            FinalCoverageStateHash = transcript?.FinalCoverageStateHash ?? string.Empty,
            ReplayFinalStateHash = transcript?.ReplayFinalStateHash ?? string.Empty,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public EditDrivenUnityAlphaStreamingAssetsHandoffNegativeProof BuildNegativeProof(
        string repositoryRootPath,
        Goal082SourceContext context,
        IReadOnlyDictionary<string, string> payloadFiles)
    {
        var scenarios = new List<EditDrivenUnityAlphaStreamingAssetsHandoffNegativeScenario>
        {
            Scenario(
                "missing_handoff_manifest",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Without(payloadFiles, "handoff-manifest.json"),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "missing_expected_hashes",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Without(payloadFiles, "expected-hashes.json"),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "missing_command_index",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Without(payloadFiles, "playthrough-command-index.json"),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "tampered_projected_package_index",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Mutate(payloadFiles, "projected-package-index.json", context.ProjectedPackageHash, new string('0', 64)),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "tampered_expected_hashes",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Mutate(payloadFiles, "expected-hashes.json", context.CommandScriptHash, new string('1', 64)),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "fake_success_without_payload_read",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    payloadFiles,
                    payloadReadAttempted: false).Diagnostics)
        };

        var ordered = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList();
        return new EditDrivenUnityAlphaStreamingAssetsHandoffNegativeProof
        {
            Passed = ordered.Count == EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredNegativeScenarioIds.Count
                     && ordered.All(item => item.ActualStatus == "rejected")
                     && ordered.All(item => item.Diagnostics.Count > 0),
            ScenarioCount = ordered.Count,
            Scenarios = ordered
        };
    }

    private static EditDrivenUnityAlphaStreamingAssetsHandoffNegativeScenario Scenario(
        string scenarioId,
        IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> diagnostics)
    {
        var rejected = diagnostics.Any(diagnostic => diagnostic.Severity == "error");
        return new EditDrivenUnityAlphaStreamingAssetsHandoffNegativeScenario
        {
            ScenarioId = scenarioId,
            ActualStatus = rejected ? "rejected" : "accepted",
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static SortedDictionary<string, string> Without(
        IReadOnlyDictionary<string, string> payloadFiles,
        string fileName) =>
        new(payloadFiles
            .Where(item => item.Key != fileName)
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal), StringComparer.Ordinal);

    private static SortedDictionary<string, string> Mutate(
        IReadOnlyDictionary<string, string> payloadFiles,
        string fileName,
        string oldValue,
        string newValue) =>
        new(payloadFiles.ToDictionary(
            item => item.Key,
            item => item.Key == fileName
                ? item.Value.Replace(oldValue, newValue, StringComparison.Ordinal)
                : item.Value,
            StringComparer.Ordinal), StringComparer.Ordinal);

    private static T? Read<T>(
        IReadOnlyDictionary<string, string> payloadFiles,
        string fileName,
        ICollection<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> diagnostics)
    {
        if (!payloadFiles.TryGetValue(fileName, out var json))
        {
            return default;
        }

        try
        {
            return EditDrivenUnityAlphaStreamingAssetsHandoffJson.Deserialize<T>(json);
        }
        catch (Exception exception) when (exception is System.Text.Json.JsonException or NotSupportedException)
        {
            diagnostics.Add(Error(
                "goal082.probe.payload_json_invalid",
                fileName,
                exception.Message));
            return default;
        }
    }

    private static string SafeRead(string root, string relativePath)
    {
        var path = Path.Combine(root, Normalize(relativePath));
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static void AddIfFalse(
        ICollection<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> diagnostics,
        bool condition,
        string code,
        string target,
        string message)
    {
        if (!condition)
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static string Hash(string text) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffHash.Sha256Text(text);

    private static string Normalize(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);

    private static IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> diagnostics) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner.SortDiagnostics(diagnostics);

    private static EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic Error(
        string code,
        string target,
        string message) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic.Error(code, target, message);
}
