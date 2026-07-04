namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

public sealed partial class OfflineGeoworldAlphaSliceOrchestratorEvidenceService
{
    private static IReadOnlyList<OfflineGeoworldAlphaSliceComponent> BuildComponentRecords(string root) =>
        ComponentDefinitions().Select(item => BuildComponentRecord(root, item)).ToList();

    private static OfflineGeoworldAlphaSliceComponent BuildComponentRecord(
        string root,
        ComponentDefinition definition)
    {
        var sourceHashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var sourcePaths = definition.RequiredArtifactFiles
            .Select(fileName => definition.ArtifactRoot + "/" + fileName)
            .ToList();
        if (definition.StreamingAssetsRoot.Length > 0
            && Directory.Exists(Resolve(root, definition.StreamingAssetsRoot)))
        {
            sourcePaths.AddRange(Directory
                .EnumerateFiles(
                    Resolve(root, definition.StreamingAssetsRoot),
                    "*",
                    SearchOption.TopDirectoryOnly)
                .Select(path => Relative(root, path)));
        }

        foreach (var relativePath in sourcePaths.OrderBy(item => item, StringComparer.Ordinal))
        {
            var path = Resolve(root, relativePath);
            if (File.Exists(path))
            {
                sourceHashes[relativePath] = HashFile(path);
            }
        }

        var primaryPath = Resolve(root, definition.ArtifactRoot + "/" + definition.PrimaryJsonFileName);
        using var primary = TryReadJson(primaryPath);
        var qualityPath = Resolve(root, definition.ArtifactRoot + "/" + definition.QualityJsonFileName);
        using var quality = TryReadJson(qualityPath);
        var requiredPresent = definition.RequiredArtifactFiles.All(file =>
            File.Exists(Resolve(root, definition.ArtifactRoot + "/" + file)));
        var streamingPayloadPaths = new List<string>();
        if (definition.StreamingAssetsRoot.Length > 0
            && Directory.Exists(Resolve(root, definition.StreamingAssetsRoot)))
        {
            streamingPayloadPaths.AddRange(Directory
                .EnumerateFiles(
                    Resolve(root, definition.StreamingAssetsRoot),
                    "*",
                    SearchOption.TopDirectoryOnly)
                .Select(path => Relative(root, path))
                .OrderBy(path => path, StringComparer.Ordinal));
        }

        var scriptsReady = definition.UnityScriptPaths.All(path => File.Exists(Resolve(root, path)));
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var accepted = primary is not null && TryGetBool(primary.RootElement, "accepted");
        var objectiveCount = primary is null ? 0 : ReadInt(primary.RootElement, "objectiveCount");
        var completedObjectiveCount = primary is null ? 0 : ReadInt(primary.RootElement, "completedObjectiveCount");
        var finalStatus = primary is null ? string.Empty : ReadString(primary.RootElement, "finalStatus");
        var finalStateHash = primary is null
            ? string.Empty
            : ReadString(primary.RootElement, "sourceGoal106FinalStateHash");
        var finalAcceptanceHash = primary is null
            ? string.Empty
            : ReadString(primary.RootElement, "objectiveAcceptanceHash");
        var aggregateHash = HashText(string.Join(
            "\n",
            sourceHashes.Select(item => item.Key + ":" + item.Value)));
        var ready = requiredPresent
                    && (definition.StreamingAssetsRoot.Length == 0 || streamingPayloadPaths.Count > 0)
                    && qualityPassed
                    && !accepted
                    && scriptsReady;
        return new OfflineGeoworldAlphaSliceComponent
        {
            ComponentId = definition.ComponentId,
            DisplayName = definition.DisplayName,
            SourceGoalId = definition.SourceGoalId,
            SourceArtifactRoot = definition.ArtifactRoot,
            StreamingAssetsRoot = definition.StreamingAssetsRoot,
            ManualGate = primary is null ? string.Empty : ReadString(primary.RootElement, "manualGate"),
            ImplementationStatus = primary is null ? string.Empty : ReadString(primary.RootElement, "implementationStatus"),
            Accepted = accepted,
            Ready = ready,
            RequiredArtifactFilesPresent = requiredPresent,
            QualityGatePassed = qualityPassed,
            UnityScriptsReady = scriptsReady,
            UnityPayloadPaths = streamingPayloadPaths,
            UnityScriptPaths = definition.UnityScriptPaths,
            SourceArtifactHashes = sourceHashes,
            AggregateHash = aggregateHash,
            ObjectiveCount = objectiveCount,
            CompletedObjectiveCount = completedObjectiveCount,
            FinalStatus = finalStatus,
            FinalStateHash = finalStateHash,
            FinalAcceptanceHash = finalAcceptanceHash,
            NotFinalWarnings =
            [
                "Alpha tooling only.",
                "accepted=false until manual gate review.",
                "No Runtime, provider, schema, scene, prefab, project settings, final art or real geodata promotion."
            ]
        };
    }

    private static IReadOnlyList<ComponentDefinition> ComponentDefinitions() =>
    [
        new(
            "preview",
            "Goal101 Preview Commands",
            "goal_101_offline_geoworld_unity_preview_runner",
            ".llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner",
            "offline-geoworld-preview-runner-manifest.json",
            "offline-geoworld-preview-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101",
            [
                "offline-geoworld-preview-runner-manifest.json",
                "offline-geoworld-preview-quality-gate-scan.json",
                "offline-geoworld-preview-unity-script-inventory.json",
                "offline-geoworld-preview-simulated-command-proof.json",
                "offline-geoworld-preview-negative-proof.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs"
            ]),
        new(
            "editor_preview",
            "Goal102 Unity Editor Preview",
            "goal_102_offline_geoworld_unity_editor_preview_tool",
            ".llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool",
            "offline-geoworld-unity-editor-quality-gate-scan.json",
            "offline-geoworld-unity-editor-quality-gate-scan.json",
            string.Empty,
            [
                "offline-geoworld-unity-editor-quality-gate-scan.json",
                "offline-geoworld-unity-editor-tool-inventory.json",
                "offline-geoworld-unity-editor-simulated-action-proof.json",
                "offline-geoworld-unity-editor-negative-proof.json"
            ],
            ["unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs"]),
        new(
            "play_mode_travel",
            "Goal103 Play Mode Travel",
            "goal_103_offline_geoworld_playmode_travel_preview",
            ".llmgc/procedural/goal-103-offline-geoworld-playmode-travel-preview",
            "offline-geoworld-playmode-travel-manifest.json",
            "offline-geoworld-playmode-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal103",
            [
                "offline-geoworld-playmode-travel-manifest.json",
                "offline-geoworld-playmode-quality-gate-scan.json",
                "offline-geoworld-playmode-simulated-execution-proof.json",
                "offline-geoworld-playmode-negative-proof.json",
                "offline-geoworld-playmode-unity-script-inventory.json",
                "offline-geoworld-playmode-editor-window-inventory.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelState.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeChunkVisibility.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPlayModeTravelWindow.cs"
            ]),
        new(
            "interactive_travel",
            "Goal104 Interactive Travel",
            "goal_104_offline_geoworld_interactive_travel_preview",
            ".llmgc/procedural/goal-104-offline-geoworld-interactive-travel-preview",
            "offline-geoworld-interactive-travel-manifest.json",
            "offline-geoworld-interactive-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal104",
            [
                "offline-geoworld-interactive-travel-manifest.json",
                "offline-geoworld-interactive-quality-gate-scan.json",
                "offline-geoworld-interactive-simulated-execution-proof.json",
                "offline-geoworld-interactive-negative-proof.json",
                "offline-geoworld-interactive-unity-script-inventory.json",
                "offline-geoworld-interactive-editor-window-inventory.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractiveTravelController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPlayerMotor.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldBoundaryPrefetchState.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractiveTravelWindow.cs"
            ]),
        new(
            "interactions",
            "Goal105 Interactions",
            "goal_105_offline_geoworld_interaction_playable_probe",
            ".llmgc/procedural/goal-105-offline-geoworld-interaction-playable-probe",
            "offline-geoworld-interaction-manifest.json",
            "offline-geoworld-interaction-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105",
            [
                "offline-geoworld-interaction-manifest.json",
                "offline-geoworld-interaction-quality-gate-scan.json",
                "offline-geoworld-interaction-simulated-session-proof.json",
                "offline-geoworld-interaction-negative-proof.json",
                "offline-geoworld-interaction-unity-script-inventory.json",
                "offline-geoworld-interaction-editor-window-inventory.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionTarget.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldStateDeltaLog.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractionProbeWindow.cs"
            ]),
        new(
            "session_replay",
            "Goal106 Session Replay",
            "goal_106_offline_geoworld_session_persistence_replay",
            ".llmgc/procedural/goal-106-offline-geoworld-session-persistence-replay",
            "offline-geoworld-session-manifest.json",
            "offline-geoworld-session-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal106",
            [
                "offline-geoworld-session-manifest.json",
                "offline-geoworld-session-quality-gate-scan.json",
                "offline-geoworld-session-simulated-save-load-replay-proof.json",
                "offline-geoworld-session-negative-proof.json",
                "offline-geoworld-session-unity-script-inventory.json",
                "offline-geoworld-session-editor-window-inventory.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSnapshot.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSaveLoadController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionReplayController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldSessionReplayWindow.cs"
            ]),
        new(
            "objective_acceptance",
            "Goal107 Objective Acceptance",
            "goal_107_offline_geoworld_objective_acceptance_run",
            ".llmgc/procedural/goal-107-offline-geoworld-objective-acceptance-run",
            "offline-geoworld-objective-manifest.json",
            "offline-geoworld-objective-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal107",
            [
                "offline-geoworld-objective-manifest.json",
                "offline-geoworld-objective-quality-gate-scan.json",
                "offline-geoworld-objective-simulated-acceptance-proof.json",
                "offline-geoworld-objective-negative-proof.json",
                "offline-geoworld-objective-unity-script-inventory.json",
                "offline-geoworld-objective-editor-window-inventory.json",
                "offline-geoworld-objective-alpha-quality-consolidation.json",
                "offline-geoworld-objective-completion-state.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveState.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveTracker.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveAcceptanceController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldObjectiveAcceptanceWindow.cs"
            ])
    ];

    private sealed record ComponentDefinition(
        string ComponentId,
        string DisplayName,
        string SourceGoalId,
        string ArtifactRoot,
        string PrimaryJsonFileName,
        string QualityJsonFileName,
        string StreamingAssetsRoot,
        IReadOnlyList<string> RequiredArtifactFiles,
        IReadOnlyList<string> UnityScriptPaths);
}
