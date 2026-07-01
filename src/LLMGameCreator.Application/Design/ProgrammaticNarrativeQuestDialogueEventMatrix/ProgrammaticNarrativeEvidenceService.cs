using System.Text;

namespace LLMGameCreator.Application.Design.ProgrammaticNarrativeQuestDialogueEventMatrix;

public sealed class ProgrammaticNarrativeEvidenceService
{
    public const string SourceManifestJsonFileName = "narrative-source-manifest.json";
    public const string RowMatrixJsonFileName = "narrative-row-matrix.json";
    public const string TemplateCatalogJsonFileName = "narrative-template-catalog.json";
    public const string QuestStageLedgerJsonFileName = "quest-stage-ledger.json";
    public const string DialogueOptionLedgerJsonFileName = "dialogue-option-ledger.json";
    public const string EventConsequenceLedgerJsonFileName = "event-trigger-consequence-ledger.json";
    public const string LocalizationKeyTableJsonFileName = "localization-key-table.json";
    public const string MemoryRumorLedgerJsonFileName = "memory-rumor-propagation-ledger.json";
    public const string SaveLoadReplayProofJsonFileName = "narrative-save-load-replay-proof.json";
    public const string PreviewExportPayloadJsonFileName = "narrative-preview-export-payload.json";
    public const string UnityCommandPlanJsonFileName = "narrative-unity-command-plan.json";
    public const string UnityProofSummaryJsonFileName = "narrative-unity-player-proof-summary.json";
    public const string InvalidDiagnosticsMatrixJsonFileName = "narrative-invalid-diagnostics-matrix.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "programmatic-narrative-quest-dialogue-event-matrix-report.md";
    public const string RowsDirectoryName = "rows";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public ProgrammaticNarrativeBuildResult Build(string projectRootPath, ProgrammaticNarrativeUnityProof? unityProof = null)
    {
        var source = new ProgrammaticNarrativeSourceLoader().Load(projectRootPath);
        var builder = new ProgrammaticNarrativeMatrixBuilder();
        var sourceManifest = builder.BuildSourceManifest(source);
        var catalog = builder.BuildTemplateCatalog();
        var rows = builder.BuildRows(source);
        var matrix = builder.BuildRowMatrix(rows);
        var questStage = builder.BuildQuestStageLedger(rows);
        var dialogueOption = builder.BuildDialogueOptionLedger(rows);
        var eventConsequence = builder.BuildEventConsequenceLedger(rows);
        var localization = builder.BuildLocalizationKeyTable(rows);
        var memoryRumor = builder.BuildMemoryRumorLedger(rows);
        var replay = builder.BuildSaveLoadReplayProof(rows);
        var meaningfulVariancePassed = builder.MeaningfulVariancePassed(rows);
        var unityCommandPlan = builder.BuildUnityCommandPlan(rows);
        var proof = unityProof ?? ProgrammaticNarrativeUnityProofRunner.NotRequested(unityCommandPlan);
        var preview = builder.BuildPreviewExportPayload(rows);
        var invalid = builder.BuildInvalidMatrix();
        var diagnostics = BuildDiagnostics(sourceManifest, catalog, matrix, questStage, dialogueOption, eventConsequence, localization, memoryRumor, replay, meaningfulVariancePassed, unityCommandPlan, proof.PlayerProof, preview, invalid);
        var reportWithoutHash = BuildReport(sourceManifest, catalog, matrix, questStage, dialogueOption, eventConsequence, localization, memoryRumor, replay, meaningfulVariancePassed, unityCommandPlan, proof.PlayerProof, preview, invalid, diagnostics);
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new ProgrammaticNarrativeBuildResult
        {
            SourceManifest = sourceManifest,
            TemplateCatalog = catalog,
            RowMatrix = matrix,
            QuestStageLedger = questStage,
            DialogueOptionLedger = dialogueOption,
            EventConsequenceLedger = eventConsequence,
            LocalizationKeyTable = localization,
            MemoryRumorLedger = memoryRumor,
            SaveLoadReplayProof = replay,
            UnityCommandPlan = unityCommandPlan,
            UnityProofSummary = proof.PlayerProof,
            PreviewExportPayload = preview,
            InvalidMatrix = invalid,
            Report = report,
            Rows = rows,
            StagingFiles = builder.BuildStagingFiles(source, unityCommandPlan),
            ReportMarkdown = RenderReport(report, sourceManifest, matrix, questStage, dialogueOption, eventConsequence, localization, memoryRumor, replay, proof.PlayerProof, invalid)
        };
    }

    public async Task<ProgrammaticNarrativeWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        ProgrammaticNarrativeOptions options,
        CancellationToken cancellationToken = default)
    {
        var initial = Build(projectRootPath);
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutput: true, cancellationToken).ConfigureAwait(false);
        if (!options.ExecuteUnityProof)
        {
            return initialWrite;
        }

        var proof = new ProgrammaticNarrativeUnityProofRunner().Run(
            projectRootPath,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityCommandPlan,
            options);
        var final = Build(projectRootPath, proof);
        return await WriteAsync(projectRootPath, final, resetOutput: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ProgrammaticNarrativeWriteResult> WriteAsync(
        string projectRootPath,
        ProgrammaticNarrativeBuildResult result,
        bool resetOutput = true,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ProgrammaticNarrativeVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
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
        await WriteText(outputDirectory, RowMatrixJsonFileName, Serialize(result.RowMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, TemplateCatalogJsonFileName, Serialize(result.TemplateCatalog), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, QuestStageLedgerJsonFileName, Serialize(result.QuestStageLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, DialogueOptionLedgerJsonFileName, Serialize(result.DialogueOptionLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, EventConsequenceLedgerJsonFileName, Serialize(result.EventConsequenceLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, LocalizationKeyTableJsonFileName, Serialize(result.LocalizationKeyTable), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, MemoryRumorLedgerJsonFileName, Serialize(result.MemoryRumorLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SaveLoadReplayProofJsonFileName, Serialize(result.SaveLoadReplayProof), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, PreviewExportPayloadJsonFileName, Serialize(result.PreviewExportPayload), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityCommandPlanJsonFileName, Serialize(result.UnityCommandPlan), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityProofSummaryJsonFileName, Serialize(result.UnityProofSummary), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, InvalidDiagnosticsMatrixJsonFileName, Serialize(result.InvalidMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ArtifactScopeReportJsonFileName, RenderArtifactScopeReportJson(), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ReportMarkdownFileName, result.ReportMarkdown, written, cancellationToken).ConfigureAwait(false);

        var rowsDirectory = Path.Combine(outputDirectory, RowsDirectoryName);
        Directory.CreateDirectory(rowsDirectory);
        foreach (var row in result.Rows.OrderBy(item => item.RowId, StringComparer.Ordinal))
        {
            await WriteText(rowsDirectory, RowFileName(row), Serialize(row), written, cancellationToken).ConfigureAwait(false);
        }

        foreach (var file in result.StagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, ProgrammaticNarrativeVocabulary.StagingRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await WriteBytes(path, file.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        return new ProgrammaticNarrativeWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, ProgrammaticNarrativeVocabulary.StagingRoot),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            WrittenFiles = written.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    public static string RowFileName(ProgrammaticNarrativeRow row) =>
        row.FamilyId.Replace('_', '-') + "-" + row.SeedId.Replace('_', '-') + "-narrative-row.json";

    private static IReadOnlyList<ProgrammaticNarrativeDiagnostic> BuildDiagnostics(
        ProgrammaticNarrativeSourceManifest sourceManifest,
        ProgrammaticNarrativeTemplateCatalog catalog,
        ProgrammaticNarrativeRowMatrix matrix,
        NarrativeLedger questStage,
        NarrativeLedger dialogueOption,
        NarrativeLedger eventConsequence,
        LocalizationKeyTable localization,
        NarrativeLedger memoryRumor,
        ProgrammaticNarrativeSaveLoadReplayProof replay,
        bool meaningfulVariancePassed,
        ProgrammaticNarrativeUnityCommandPlan unityCommandPlan,
        ProgrammaticNarrativeUnityProofSummary unityProof,
        ProgrammaticNarrativePreviewExportPayload preview,
        InvalidProgrammaticNarrativeDiagnosticsMatrix invalid)
    {
        var validator = new ProgrammaticNarrativeValidator();
        return ProgrammaticNarrativeValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateRows(catalog, matrix, preview, meaningfulVariancePassed))
                .Concat(validator.ValidateLedgers(questStage, dialogueOption, eventConsequence, localization, memoryRumor))
                .Concat(validator.ValidateReplay(replay))
                .Concat(validator.ValidateUnityCommandPlan(unityCommandPlan))
                .Concat(validator.ValidateUnityProof(unityCommandPlan, unityProof))
                .Concat(validator.ValidateInvalidMatrix(invalid)));
    }

    private static ProgrammaticNarrativeReport BuildReport(
        ProgrammaticNarrativeSourceManifest sourceManifest,
        ProgrammaticNarrativeTemplateCatalog catalog,
        ProgrammaticNarrativeRowMatrix matrix,
        NarrativeLedger questStage,
        NarrativeLedger dialogueOption,
        NarrativeLedger eventConsequence,
        LocalizationKeyTable localization,
        NarrativeLedger memoryRumor,
        ProgrammaticNarrativeSaveLoadReplayProof replay,
        bool meaningfulVariancePassed,
        ProgrammaticNarrativeUnityCommandPlan unityCommandPlan,
        ProgrammaticNarrativeUnityProofSummary unityProof,
        ProgrammaticNarrativePreviewExportPayload preview,
        InvalidProgrammaticNarrativeDiagnosticsMatrix invalid,
        IReadOnlyList<ProgrammaticNarrativeDiagnostic> diagnostics)
    {
        var noErrors = diagnostics.All(item => item.Severity != "error");
        var sourceConsumed = sourceManifest.Goal060PackageRowsConsumed
            && sourceManifest.Goal061ReviewPackageRcConsumed
            && sourceManifest.Goal062SpatialRowsConsumed
            && sourceManifest.Goal063GameplayRowsConsumed
            && sourceManifest.Goal064LivingWorldRowsConsumed
            && sourceManifest.Goal065InterlockedRowsConsumed
            && sourceManifest.Goal066SettlementRowsConsumed;
        var noProseLeak = matrix.Rows.All(item => item.NoFinalProse)
            && diagnostics.All(item => item.Code != "goal067.prose.final_leakage");
        var green = noErrors
            && sourceManifest.Goal066AcceptedByUserHandoff
            && sourceConsumed
            && catalog.Passed
            && matrix.Passed
            && questStage.Passed
            && dialogueOption.Passed
            && eventConsequence.Passed
            && localization.Passed
            && memoryRumor.Passed
            && replay.Passed
            && meaningfulVariancePassed
            && unityCommandPlan.Passed
            && unityProof.Passed
            && preview.Passed
            && invalid.Passed
            && noProseLeak;
        var failed = diagnostics.Any(item => item.Severity == "error" && !item.Code.StartsWith("goal067.unity.", StringComparison.Ordinal));

        return new ProgrammaticNarrativeReport
        {
            ImplementationStatus = green ? "GREEN" : failed ? "FAILED" : "BLOCKED",
            Accepted = false,
            Goal066AcceptedByUserHandoff = sourceManifest.Goal066AcceptedByUserHandoff,
            SourceFactsConsumed = sourceConsumed,
            TemplateCatalogPassed = catalog.Passed,
            RowMatrixPassed = matrix.Passed,
            QuestStageLedgerPassed = questStage.Passed,
            DialogueOptionLedgerPassed = dialogueOption.Passed,
            EventConsequenceLedgerPassed = eventConsequence.Passed,
            LocalizationKeyTablePassed = localization.Passed,
            MemoryRumorLedgerPassed = memoryRumor.Passed,
            SaveLoadReplayPassed = replay.Passed,
            MeaningfulVariancePassed = meaningfulVariancePassed,
            UnityCommandPlanPassed = unityCommandPlan.Passed,
            UnityProofPassed = unityProof.Passed,
            UnityExitCode = unityProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerExitCode,
            AllNarrativeMarkersMatched = unityProof.Passed && unityProof.MissingMarkers.Count == 0,
            PreviewExportPayloadPassed = preview.Passed,
            InvalidMatrixPassed = invalid.Passed,
            NoFinalProseLeakage = noProseLeak,
            RowCount = matrix.RowCount,
            StateChangingRowCount = matrix.StateChangingRowCount,
            FamilyCount = matrix.FamilyCount,
            SeedCount = matrix.SeedCount,
            SourceManifestHash = Hash(Serialize(sourceManifest)),
            TemplateCatalogHash = Hash(Serialize(catalog)),
            RowMatrixHash = Hash(Serialize(matrix)),
            QuestStageLedgerHash = Hash(Serialize(questStage)),
            DialogueOptionLedgerHash = Hash(Serialize(dialogueOption)),
            EventConsequenceLedgerHash = Hash(Serialize(eventConsequence)),
            LocalizationKeyTableHash = Hash(Serialize(localization)),
            MemoryRumorLedgerHash = Hash(Serialize(memoryRumor)),
            SaveLoadReplayProofHash = Hash(Serialize(replay)),
            UnityCommandPlanHash = Hash(Serialize(unityCommandPlan)),
            UnityProofSummaryHash = Hash(Serialize(unityProof)),
            PreviewExportPayloadHash = Hash(Serialize(preview)),
            InvalidMatrixHash = Hash(Serialize(invalid)),
            Diagnostics = diagnostics
        };
    }

    private static string RenderReport(
        ProgrammaticNarrativeReport report,
        ProgrammaticNarrativeSourceManifest sourceManifest,
        ProgrammaticNarrativeRowMatrix matrix,
        NarrativeLedger questStage,
        NarrativeLedger dialogueOption,
        NarrativeLedger eventConsequence,
        LocalizationKeyTable localization,
        NarrativeLedger memoryRumor,
        ProgrammaticNarrativeSaveLoadReplayProof replay,
        ProgrammaticNarrativeUnityProofSummary unityProof,
        InvalidProgrammaticNarrativeDiagnosticsMatrix invalid)
    {
        var lines = new List<string>
        {
            "# Programmatic Narrative Quest Dialogue Event Matrix Report",
            string.Empty,
            "programmatic_narrative_quest_dialogue_event_matrix_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            $"manualGate={ProgrammaticNarrativeVocabulary.FinalGate}",
            $"goal066AcceptedByUserHandoff={report.Goal066AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"rowCount={report.RowCount}",
            $"familyCount={report.FamilyCount}",
            $"seedCount={report.SeedCount}",
            $"stateChangingRowCount={report.StateChangingRowCount}",
            $"questStageLedgerPassed={report.QuestStageLedgerPassed.ToString().ToLowerInvariant()}",
            $"dialogueOptionLedgerPassed={report.DialogueOptionLedgerPassed.ToString().ToLowerInvariant()}",
            $"eventConsequenceLedgerPassed={report.EventConsequenceLedgerPassed.ToString().ToLowerInvariant()}",
            $"localizationKeyTablePassed={report.LocalizationKeyTablePassed.ToString().ToLowerInvariant()}",
            $"memoryRumorLedgerPassed={report.MemoryRumorLedgerPassed.ToString().ToLowerInvariant()}",
            $"saveLoadReplayPassed={report.SaveLoadReplayPassed.ToString().ToLowerInvariant()}",
            $"meaningfulVariancePassed={report.MeaningfulVariancePassed.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"allNarrativeMarkersMatched={report.AllNarrativeMarkersMatched.ToString().ToLowerInvariant()}",
            $"noFinalProseLeakage={report.NoFinalProseLeakage.ToString().ToLowerInvariant()}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"templateCatalogHash={report.TemplateCatalogHash}",
            $"rowMatrixHash={report.RowMatrixHash}",
            $"questStageLedgerHash={report.QuestStageLedgerHash}",
            $"dialogueOptionLedgerHash={report.DialogueOptionLedgerHash}",
            $"eventConsequenceLedgerHash={report.EventConsequenceLedgerHash}",
            $"localizationKeyTableHash={report.LocalizationKeyTableHash}",
            $"memoryRumorLedgerHash={report.MemoryRumorLedgerHash}",
            $"saveLoadReplayProofHash={report.SaveLoadReplayProofHash}",
            $"unityCommandPlanHash={report.UnityCommandPlanHash}",
            $"unityProofSummaryHash={report.UnityProofSummaryHash}",
            $"previewExportPayloadHash={report.PreviewExportPayloadHash}",
            $"invalidMatrixHash={report.InvalidMatrixHash}",
            $"reportHash={report.DeterministicHash}",
            string.Empty,
            "## Source Chain",
            string.Empty
        };

        lines.AddRange(sourceManifest.PreflightGates.Select(item => $"- {item.GateId}: status={item.Status}, provenance={item.ProvenanceKind}, evidence={item.EvidenceRef}"));
        lines.AddRange(sourceManifest.SourceArtifactRefs.Select(item => $"- {item.ArtifactFamily}: artifact={item.ArtifactRelativePath}, exists={item.Exists.ToString().ToLowerInvariant()}, hashMatches={item.HashMatches.ToString().ToLowerInvariant()}, hash={item.ArtifactHash}"));
        lines.Add(string.Empty);
        lines.Add("## Row Matrix");
        lines.Add(string.Empty);
        foreach (var row in matrix.Rows)
        {
            lines.Add($"- {row.RowId}: family={row.FamilyId}, seed={row.SeedId}, questArc={row.QuestArcId}, dialogueGraph={row.DialogueGraphId}, eventChain={row.EventChainId}, before={row.BeforeState.StateHash}, after={row.AfterState.StateHash}, rowHash={row.RowHash}");
            lines.Add($"  - sourceRefs: package={row.SourcePackageRowRef}, spatial={row.SourceSpatialDetailRowRef}, livingWorld={row.SourceLivingWorldRowRef}, interlocked={row.SourceInterlockedGameplayRowRef}, settlement={row.SourceSettlementRowRef}");
            lines.Add($"  - questStages={row.QuestStageGraph.Count}, dialogueOptions={row.DialogueOptionGraph.Count}, events={row.EventTriggerConsequenceChain.Count}, localizationKeys={row.LocalizationKeyTable.Count}, memoryRumors={row.MemoryRumorPropagation.Count}, stateDeltas={row.StateDeltas.Count}");
            lines.Add($"  - firstLineKey={row.LocalizationKeyTable.FirstOrDefault()?.LineKey ?? string.Empty}, template={row.LocalizationKeyTable.FirstOrDefault()?.TemplateId ?? string.Empty}, settlement={row.SettlementId}, building={row.BuildingId}");
        }

        lines.Add(string.Empty);
        lines.Add("## Ledgers");
        lines.Add(string.Empty);
        lines.Add($"- questStage: passed={questStage.Passed.ToString().ToLowerInvariant()}, entries={questStage.EntryCount}");
        lines.Add($"- dialogueOption: passed={dialogueOption.Passed.ToString().ToLowerInvariant()}, entries={dialogueOption.EntryCount}");
        lines.Add($"- eventConsequence: passed={eventConsequence.Passed.ToString().ToLowerInvariant()}, entries={eventConsequence.EntryCount}");
        lines.Add($"- localizationKeyTable: passed={localization.Passed.ToString().ToLowerInvariant()}, entries={localization.EntryCount}");
        lines.Add($"- memoryRumor: passed={memoryRumor.Passed.ToString().ToLowerInvariant()}, entries={memoryRumor.EntryCount}");
        lines.Add(string.Empty);
        lines.Add("## Save/load/replay");
        lines.Add(string.Empty);
        lines.AddRange(replay.Rows.Select(item => $"- {item.RowId}: changed={item.BeforeAfterStateChanged.ToString().ToLowerInvariant()}, saveLoad={item.SaveLoadRoundtripPassed.ToString().ToLowerInvariant()}, replay={item.ReplayDeterminismPassed.ToString().ToLowerInvariant()}, hash={item.FirstReplayHash}"));
        lines.Add(string.Empty);
        lines.Add("## Unity Proof");
        lines.Add(string.Empty);
        lines.Add($"- passed: {unityProof.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- unityExitCode: {TextOrNone(unityProof.UnityExitCode?.ToString())}");
        lines.Add($"- playerExitCode: {TextOrNone(unityProof.PlayerExitCode?.ToString())}");
        lines.Add($"- provenRowCount: {unityProof.ProvenRowCount}");
        lines.Add($"- missingMarkers: {unityProof.MissingMarkers.Count}");
        lines.AddRange(unityProof.MatchedMarkers.Select(marker => $"- matchedMarker: {marker}"));
        lines.AddRange(unityProof.MissingMarkers.Select(marker => $"- missingMarker: {marker}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak Matrix");
        lines.Add(string.Empty);
        lines.Add($"- passed: {invalid.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- scenarioCount: {invalid.ScenarioCount}");
        lines.AddRange(invalid.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).OrderBy(code => code, StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add("No public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution/project file change, new dependency, provider/LLM/RAG call, final dialogue prose generation, Yarn/ink runtime integration, media generation, or arbitrary Lua execution is part of this Goal 067 proof. Unity changes are limited to deterministic narrative marker loading in AlphaRuntimeBootstrap.");
        lines.Add(string.Empty);
        lines.Add("programmatic_narrative_quest_dialogue_event_matrix_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            schemaVersion = "goal067_artifact_scope_report_v1",
            scenario = ProgrammaticNarrativeVocabulary.ProductSmokeRoute,
            gate = ProgrammaticNarrativeVocabulary.FinalGate + " required",
            allowedArtifactRoot = ProgrammaticNarrativeVocabulary.RelativeOutputDirectory + "/",
            allowedCodeRoot = "src/LLMGameCreator.Application/Design/ProgrammaticNarrativeQuestDialogueEventMatrix/",
            allowedTestsRoot = "tests/LLMGameCreator.Tests/Application/ProgrammaticNarrativeQuestDialogueEventMatrix/",
            allowedProductSmoke = "tests/LLMGameCreator.Tests/ProductSmoke/ProgrammaticNarrativeQuestDialogueEventMatrixProductSmokeTests.cs",
            narrowUnityAllowance = "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs",
            forbiddenChanges = new[]
            {
                "public GamePackage schema/model definitions",
                "Runtime/Runtime.Abstractions",
                "WinForms UI",
                "Infrastructure provider/LLM/RAG paths",
                "generator-library",
                "solution/project files",
                "external dependencies",
                "Yarn/ink runtime dependency",
                "final dialogue prose generation",
                "arbitrary Lua execution"
            }
        });

    private static async Task WriteText(
        string directory,
        string fileName,
        string text,
        List<string> written,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(directory, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await WriteTextWithRetry(path, text.TrimEnd('\r', '\n') + Environment.NewLine, cancellationToken).ConfigureAwait(false);
        written.Add(path);
    }

    private static async Task WriteTextWithRetry(string path, string text, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 120; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (File.Exists(path) && string.Equals(await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false), text, StringComparison.Ordinal))
                {
                    return;
                }

                await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                if (attempt == 120)
                {
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async Task WriteBytes(string path, byte[] bytes, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 120; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (File.Exists(path) && File.ReadAllBytes(path).SequenceEqual(bytes))
                {
                    return;
                }

                await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                if (attempt == 120)
                {
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static string TextOrNone(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "(none)" : value;

    private static string Serialize<T>(T value) => ProgrammaticNarrativeHash.Serialize(value);

    private static string Hash(string text) => ProgrammaticNarrativeHash.Hash(text);

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static void ResetDirectory(string path)
    {
        if (!TryResetDirectory(path, maxAttempts: 120, out var exception))
        {
            throw new IOException($"Directory could not be reset: {path}", exception);
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
}
