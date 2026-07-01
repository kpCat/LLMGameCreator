using System.Text;

namespace LLMGameCreator.Application.Design.UnityAlphaInteractiveCampaignPlayer;

public sealed class UnityAlphaInteractiveCampaignEvidenceService
{
    public const string SourceManifestJsonFileName = "interactive-campaign-source-manifest.json";
    public const string MatrixJsonFileName = "interactive-campaign-row-matrix.json";
    public const string SelectorJsonFileName = "interactive-campaign-family-seed-selector.json";
    public const string CommandPlanJsonFileName = "interactive-campaign-command-plan.json";
    public const string InputScriptJsonFileName = "interactive-campaign-input-script.json";
    public const string StateTransitionLedgerJsonFileName = "interactive-campaign-state-transition-ledger.json";
    public const string SaveLoadReplayProofJsonFileName = "interactive-campaign-save-load-replay-proof.json";
    public const string HudContractJsonFileName = "interactive-campaign-hud-contract.json";
    public const string PlayerProofSummaryJsonFileName = "interactive-campaign-player-proof-summary.json";
    public const string InvalidDiagnosticsMatrixJsonFileName = "interactive-campaign-invalid-diagnostics-matrix.json";
    public const string PreviewExportPayloadJsonFileName = "interactive-campaign-preview-export-payload.json";
    public const string ArtifactScopeReportJsonFileName = "interactive-campaign-artifact-scope-report.json";
    public const string ReportMarkdownFileName = "unity-alpha-interactive-campaign-player-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public InteractiveCampaignBuildResult Build(string projectRootPath, InteractiveCampaignUnityProof? unityProof = null)
    {
        var source = new UnityAlphaInteractiveCampaignSourceLoader().Load(projectRootPath);
        var builder = new UnityAlphaInteractiveCampaignBuilder();
        var sourceManifest = builder.BuildSourceManifest(source);
        var matrix = builder.BuildMatrix(source);
        var selector = builder.BuildSelector(matrix);
        var inputScript = builder.BuildInputActionScript(matrix);
        var ledger = builder.BuildStateTransitionLedger(matrix);
        var replay = builder.BuildSaveLoadReplayProof(matrix);
        var hud = builder.BuildHudContract(matrix);
        var commandPlan = builder.BuildUnityCommandPlan(matrix);
        var proof = unityProof ?? UnityAlphaInteractiveCampaignUnityProofRunner.NotRequested(commandPlan);
        var invalid = builder.BuildInvalidMatrix();
        var preview = builder.BuildPreviewExportPayload(matrix);
        var diagnostics = BuildDiagnostics(sourceManifest, matrix, selector, inputScript, ledger, replay, hud, commandPlan, proof.PlayerProof, invalid, preview);
        var reportWithoutHash = BuildReport(sourceManifest, matrix, selector, inputScript, ledger, replay, hud, commandPlan, proof.PlayerProof, invalid, preview, diagnostics);
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new InteractiveCampaignBuildResult
        {
            SourceManifest = sourceManifest,
            Matrix = matrix,
            Selector = selector,
            InputActionScript = inputScript,
            StateTransitionLedger = ledger,
            SaveLoadReplayProof = replay,
            HudContract = hud,
            UnityCommandPlan = commandPlan,
            UnityProofSummary = proof.PlayerProof,
            InvalidMatrix = invalid,
            PreviewExportPayload = preview,
            Report = report,
            StagingFiles = builder.BuildStagingFiles(source, commandPlan),
            ReportMarkdown = RenderReport(report, sourceManifest, matrix, selector, inputScript, ledger, replay, hud, proof.PlayerProof, invalid)
        };
    }

    public async Task<InteractiveCampaignWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        UnityAlphaInteractiveCampaignOptions options,
        CancellationToken cancellationToken = default)
    {
        var initial = Build(projectRootPath);
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutput: true, cancellationToken).ConfigureAwait(false);
        if (!options.ExecuteUnityProof)
        {
            return initialWrite;
        }

        var proof = new UnityAlphaInteractiveCampaignUnityProofRunner().Run(
            projectRootPath,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityCommandPlan,
            options);
        var final = Build(projectRootPath, proof);
        return await WriteAsync(projectRootPath, final, resetOutput: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<InteractiveCampaignWriteResult> WriteAsync(
        string projectRootPath,
        InteractiveCampaignBuildResult result,
        bool resetOutput = true,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, UnityAlphaInteractiveCampaignVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        if (resetOutput)
        {
            ResetDirectory(outputDirectory);
        }
        else
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var written = new List<string>();
        await WriteText(outputDirectory, SourceManifestJsonFileName, Serialize(result.SourceManifest), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, MatrixJsonFileName, Serialize(result.Matrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SelectorJsonFileName, Serialize(result.Selector), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, CommandPlanJsonFileName, Serialize(result.UnityCommandPlan), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, InputScriptJsonFileName, Serialize(result.InputActionScript), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, StateTransitionLedgerJsonFileName, Serialize(result.StateTransitionLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SaveLoadReplayProofJsonFileName, Serialize(result.SaveLoadReplayProof), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, HudContractJsonFileName, Serialize(result.HudContract), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, PlayerProofSummaryJsonFileName, Serialize(result.UnityProofSummary), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, InvalidDiagnosticsMatrixJsonFileName, Serialize(result.InvalidMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, PreviewExportPayloadJsonFileName, Serialize(result.PreviewExportPayload), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ArtifactScopeReportJsonFileName, RenderArtifactScopeReportJson(), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ReportMarkdownFileName, result.ReportMarkdown, written, cancellationToken).ConfigureAwait(false);

        foreach (var file in result.StagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, UnityAlphaInteractiveCampaignVocabulary.StagingRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await WriteBytes(path, file.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        return new InteractiveCampaignWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, UnityAlphaInteractiveCampaignVocabulary.StagingRoot),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            WrittenFiles = written.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static IReadOnlyList<InteractiveCampaignDiagnostic> BuildDiagnostics(
        InteractiveCampaignSourceManifest sourceManifest,
        InteractiveCampaignMatrix matrix,
        FamilySeedSelectorModel selector,
        InputActionScript inputScript,
        StateTransitionLedger ledger,
        InteractiveCampaignSaveLoadReplayProof replay,
        InteractiveCampaignHudContract hud,
        InteractiveCampaignUnityCommandPlan commandPlan,
        InteractiveCampaignUnityProofSummary unityProof,
        InteractiveCampaignInvalidDiagnosticsMatrix invalid,
        InteractiveCampaignPreviewExportPayload preview)
    {
        var validator = new UnityAlphaInteractiveCampaignValidator();
        return UnityAlphaInteractiveCampaignValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateMatrixAndSelector(matrix, selector))
                .Concat(validator.ValidateActionsAndTransitions(inputScript, ledger))
                .Concat(validator.ValidateReplay(replay))
                .Concat(validator.ValidateHudAndPreview(hud, preview))
                .Concat(validator.ValidateUnityCommandPlan(commandPlan))
                .Concat(validator.ValidateUnityProof(commandPlan, unityProof))
                .Concat(validator.ValidateInvalidMatrix(invalid)));
    }

    private static InteractiveCampaignReport BuildReport(
        InteractiveCampaignSourceManifest sourceManifest,
        InteractiveCampaignMatrix matrix,
        FamilySeedSelectorModel selector,
        InputActionScript inputScript,
        StateTransitionLedger ledger,
        InteractiveCampaignSaveLoadReplayProof replay,
        InteractiveCampaignHudContract hud,
        InteractiveCampaignUnityCommandPlan commandPlan,
        InteractiveCampaignUnityProofSummary unityProof,
        InteractiveCampaignInvalidDiagnosticsMatrix invalid,
        InteractiveCampaignPreviewExportPayload preview,
        IReadOnlyList<InteractiveCampaignDiagnostic> diagnostics)
    {
        var noErrors = diagnostics.All(item => item.Severity != "error");
        var sourceConsumed = sourceManifest.Goal070TimelineEvidenceConsumed && sourceManifest.Goal070UnityProofConsumed;
        var green = noErrors
            && sourceManifest.Goal070AcceptedByUserHandoff
            && sourceConsumed
            && matrix.Passed
            && selector.Passed
            && inputScript.Passed
            && ledger.Passed
            && replay.Passed
            && hud.Passed
            && commandPlan.Passed
            && unityProof.Passed
            && invalid.Passed
            && preview.Passed;
        var failed = diagnostics.Any(item => item.Severity == "error" && !item.Code.StartsWith("goal071.unity.", StringComparison.Ordinal));

        return new InteractiveCampaignReport
        {
            ImplementationStatus = green ? "GREEN" : failed ? "FAILED" : "BLOCKED",
            Accepted = false,
            Goal070AcceptedByUserHandoff = sourceManifest.Goal070AcceptedByUserHandoff,
            SourceFactsConsumed = sourceConsumed,
            RowMatrixPassed = matrix.Passed,
            SelectorPassed = selector.Passed,
            InputActionScriptPassed = inputScript.Passed,
            StateTransitionLedgerPassed = ledger.Passed,
            SaveLoadReplayPassed = replay.Passed,
            HudContractPassed = hud.Passed,
            UnityCommandPlanPassed = commandPlan.Passed,
            UnityProofPassed = unityProof.Passed,
            UnityExitCode = unityProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerExitCode,
            AllInteractiveMarkersMatched = unityProof.Passed && unityProof.MissingMarkers.Count == 0,
            PreviewExportPayloadPassed = preview.Passed,
            InvalidMatrixPassed = invalid.Passed,
            RowCount = matrix.RowCount,
            StateChangingRowCount = matrix.StateChangingRowCount,
            ActionCount = inputScript.ActionCount,
            TransitionCount = ledger.TransitionCount,
            SaveLoadPassedRowCount = replay.SaveLoadPassedRowCount,
            ReplayPassedRowCount = replay.ReplayPassedRowCount,
            FamilyCount = matrix.FamilyCount,
            SeedCount = matrix.SeedCount,
            SourceManifestHash = Hash(Serialize(sourceManifest)),
            MatrixHash = Hash(Serialize(matrix)),
            SelectorHash = Hash(Serialize(selector)),
            InputActionScriptHash = Hash(Serialize(inputScript)),
            StateTransitionLedgerHash = Hash(Serialize(ledger)),
            SaveLoadReplayProofHash = Hash(Serialize(replay)),
            HudContractHash = Hash(Serialize(hud)),
            UnityCommandPlanHash = Hash(Serialize(commandPlan)),
            UnityProofSummaryHash = Hash(Serialize(unityProof)),
            PreviewExportPayloadHash = Hash(Serialize(preview)),
            InvalidMatrixHash = Hash(Serialize(invalid)),
            Diagnostics = diagnostics
        };
    }

    private static string RenderReport(
        InteractiveCampaignReport report,
        InteractiveCampaignSourceManifest sourceManifest,
        InteractiveCampaignMatrix matrix,
        FamilySeedSelectorModel selector,
        InputActionScript inputScript,
        StateTransitionLedger ledger,
        InteractiveCampaignSaveLoadReplayProof replay,
        InteractiveCampaignHudContract hud,
        InteractiveCampaignUnityProofSummary unityProof,
        InteractiveCampaignInvalidDiagnosticsMatrix invalid)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Goal 071 Unity Alpha Interactive Campaign Player Report");
        builder.AppendLine();
        builder.AppendLine("unity_alpha_interactive_campaign_player_verification required");
        builder.AppendLine("accepted=false");
        builder.AppendLine("implementationStatus=" + report.ImplementationStatus);
        builder.AppendLine("rowCount=" + report.RowCount);
        builder.AppendLine("stateChangingRowCount=" + report.StateChangingRowCount);
        builder.AppendLine("familyCount=" + report.FamilyCount);
        builder.AppendLine("seedCount=" + report.SeedCount);
        builder.AppendLine("actionCount=" + report.ActionCount);
        builder.AppendLine("transitionCount=" + report.TransitionCount);
        builder.AppendLine("sourceFactsConsumed=" + report.SourceFactsConsumed);
        builder.AppendLine("goal070AcceptedByUserHandoff=" + report.Goal070AcceptedByUserHandoff);
        builder.AppendLine("rowMatrixPassed=" + report.RowMatrixPassed);
        builder.AppendLine("selectorPassed=" + report.SelectorPassed);
        builder.AppendLine("inputActionScriptPassed=" + report.InputActionScriptPassed);
        builder.AppendLine("stateTransitionLedgerPassed=" + report.StateTransitionLedgerPassed);
        builder.AppendLine("saveLoadReplayPassed=" + report.SaveLoadReplayPassed);
        builder.AppendLine("saveLoadPassedRowCount=" + report.SaveLoadPassedRowCount);
        builder.AppendLine("replayPassedRowCount=" + report.ReplayPassedRowCount);
        builder.AppendLine("hudContractPassed=" + report.HudContractPassed);
        builder.AppendLine("unityCommandPlanPassed=" + report.UnityCommandPlanPassed);
        builder.AppendLine("unityProofPassed=" + report.UnityProofPassed);
        builder.AppendLine("unityExitCode=" + (report.UnityExitCode?.ToString() ?? "null"));
        builder.AppendLine("playerExitCode=" + (report.PlayerExitCode?.ToString() ?? "null"));
        builder.AppendLine("provenRowCount=" + unityProof.ProvenRowCount);
        builder.AppendLine("allInteractiveMarkersMatched=" + report.AllInteractiveMarkersMatched);
        builder.AppendLine("previewExportPayloadPassed=" + report.PreviewExportPayloadPassed);
        builder.AppendLine("invalidMatrixPassed=" + report.InvalidMatrixPassed);
        builder.AppendLine("reportHash=" + report.DeterministicHash);
        builder.AppendLine();
        builder.AppendLine("## Source Gates");
        foreach (var gate in sourceManifest.PreflightGates)
        {
            builder.AppendLine("- " + gate.GateId + " " + gate.Status + " " + gate.ProvenanceKind);
        }

        builder.AppendLine();
        builder.AppendLine("## Selector");
        foreach (var family in selector.Families)
        {
            builder.AppendLine("- " + family.FamilyId + " seeds=" + string.Join(",", family.SeedIds) + " rows=" + family.RowIds.Count);
        }

        builder.AppendLine();
        builder.AppendLine("## Interactive Rows");
        foreach (var row in matrix.Rows.OrderBy(item => item.RowId, StringComparer.Ordinal))
        {
            builder.AppendLine("- " + row.RowId
                + " family=" + row.FamilyId
                + " seed=" + row.SeedId
                + " actions=" + row.Actions.Count
                + " selectedInput=" + row.SelectedInputId
                + " selectedStep=" + row.SelectedStepId
                + " stateChanged=" + row.StateChanging
                + " hud=" + row.HudRenderable
                + " replay=" + row.SaveLoadReplayPassed);
        }

        builder.AppendLine();
        builder.AppendLine("## Action And Transition Proof");
        builder.AppendLine("- actionCount=" + inputScript.ActionCount + " transitionCount=" + ledger.TransitionCount);
        builder.AppendLine("- hudRows=" + hud.Rows.Count + " saveLoadRows=" + replay.RowCount);
        builder.AppendLine();
        builder.AppendLine("## Invalid Matrix");
        foreach (var scenario in invalid.Scenarios)
        {
            builder.AppendLine("- " + scenario.ScenarioId + " " + scenario.ActualStatus);
        }

        builder.AppendLine();
        builder.AppendLine("## Diagnostics");
        foreach (var diagnostic in report.Diagnostics.Take(80))
        {
            builder.AppendLine("- [" + diagnostic.Severity + "] " + diagnostic.Code + " " + diagnostic.Target + " - " + diagnostic.Message);
        }

        return builder.ToString();
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            scenario = UnityAlphaInteractiveCampaignVocabulary.ProductSmokeRoute,
            status = "produced",
            allowedArtifactRoot = UnityAlphaInteractiveCampaignVocabulary.RelativeOutputDirectory,
            gate = UnityAlphaInteractiveCampaignVocabulary.FinalGate,
            accepted = false
        });

    private static async Task WriteText(string directory, string fileName, string content, List<string> written, CancellationToken cancellationToken)
    {
        var path = Path.Combine(directory, fileName);
        EnsureContained(directory, path);
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await File.WriteAllTextAsync(path, content, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
                written.Add(path);
                return;
            }
            catch (IOException) when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException) when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async Task WriteBytes(string path, byte[] bytes, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (IOException) when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException) when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static void ResetDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        Directory.CreateDirectory(directory);
    }

    private static void EnsureContained(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes expected root: " + normalizedPath);
        }
    }

    private static string Serialize<T>(T value) =>
        UnityAlphaInteractiveCampaignHash.Serialize(value);

    private static string Hash(string text) =>
        UnityAlphaInteractiveCampaignHash.Sha256(text);
}
