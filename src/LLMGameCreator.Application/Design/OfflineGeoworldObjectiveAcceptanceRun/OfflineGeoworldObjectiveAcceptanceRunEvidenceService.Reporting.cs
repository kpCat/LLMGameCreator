using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;

namespace LLMGameCreator.Application.Design.OfflineGeoworldObjectiveAcceptanceRun;

public sealed partial class OfflineGeoworldObjectiveAcceptanceRunEvidenceService
{
    private static IReadOnlyDictionary<string, string> BuildEvidencePayloads(
        OfflineGeoworldObjectiveUnityScriptInventory scripts,
        OfflineGeoworldObjectiveEditorWindowInventory editor,
        OfflineGeoworldObjectiveReplayAcceptanceProof proof,
        OfflineGeoworldObjectiveNegativeProof negative,
        OfflineGeoworldObjectiveWorkspaceBindingInventory binding,
        OfflineGeoworldObjectiveSourceLineage lineage,
        OfflineGeoworldObjectiveAlphaQualityConsolidation consolidation,
        OfflineGeoworldObjectiveQualityGateScan quality) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityScriptInventoryFileName] =
                Serialize(scripts),
            [OfflineGeoworldObjectiveAcceptanceRunVocabulary.EditorWindowInventoryFileName] =
                Serialize(editor),
            [OfflineGeoworldObjectiveAcceptanceRunVocabulary.SimulatedAcceptanceProofFileName] =
                Serialize(proof),
            [OfflineGeoworldObjectiveAcceptanceRunVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [OfflineGeoworldObjectiveAcceptanceRunVocabulary.WorkspaceBindingInventoryFileName] =
                Serialize(binding),
            [OfflineGeoworldObjectiveAcceptanceRunVocabulary.SourceLineageFileName] =
                Serialize(lineage),
            [OfflineGeoworldObjectiveAcceptanceRunVocabulary.AlphaQualityConsolidationFileName] =
                Serialize(consolidation),
            [OfflineGeoworldObjectiveAcceptanceRunVocabulary.QualityGateScanFileName] =
                Serialize(quality)
        };

    private static OfflineGeoworldObjectiveReport BuildReport(
        Goal107Payload payload,
        OfflineGeoworldObjectiveUnityScriptInventory scripts,
        OfflineGeoworldObjectiveEditorWindowInventory editor,
        OfflineGeoworldObjectiveReplayAcceptanceProof proof,
        OfflineGeoworldObjectiveNegativeProof negative,
        OfflineGeoworldObjectiveWorkspaceBindingInventory binding,
        OfflineGeoworldObjectiveAlphaQualityConsolidation consolidation,
        OfflineGeoworldObjectiveQualityGateScan quality) =>
        new()
        {
            ObjectiveCount = payload.Manifest.ObjectiveCount,
            CompletedObjectiveCount = payload.CompletionState.CompletedObjectiveCount,
            FinalStatus = payload.CompletionState.FinalStatus,
            ReplayStepCount = payload.AcceptanceRun.ReplayStepCount,
            StateDeltaCount = payload.AcceptanceRun.StateDeltaCount,
            CheckpointStepIndex = payload.AcceptanceRun.CheckpointStepIndex,
            FinalStateHash = payload.AcceptanceRun.FinalStateHash,
            UnityScriptsReady = scripts.Passed,
            EditorWindowReady = editor.Passed,
            ReplayAcceptanceProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            AlphaQualityConsolidationPassed = consolidation.Passed,
            AlphaRuntimeBootstrapUnchanged = quality.AlphaRuntimeBootstrapUnchanged,
            QualityGatePassed = quality.Passed
        };

    private static IReadOnlyList<(string Path, string Purpose)> SourceLineageInputs() =>
    [
        (OfflineGeoworldObjectiveAcceptanceRunVocabulary.SourceGoal106Root
         + "/"
         + OfflineGeoworldSessionPersistenceReplayVocabulary.ManifestFileName,
            "Goal106 session replay manifest"),
        (OfflineGeoworldObjectiveAcceptanceRunVocabulary.SourceGoal106Root
         + "/"
         + OfflineGeoworldSessionPersistenceReplayVocabulary.InitialStateFileName,
            "Goal106 initial state"),
        (OfflineGeoworldObjectiveAcceptanceRunVocabulary.SourceGoal106Root
         + "/"
         + OfflineGeoworldSessionPersistenceReplayVocabulary.DeltaLogFileName,
            "Goal106 state delta log"),
        (OfflineGeoworldObjectiveAcceptanceRunVocabulary.SourceGoal106Root
         + "/"
         + OfflineGeoworldSessionPersistenceReplayVocabulary.ReplayScriptFileName,
            "Goal106 replay script"),
        (OfflineGeoworldObjectiveAcceptanceRunVocabulary.SourceGoal106Root
         + "/"
         + OfflineGeoworldSessionPersistenceReplayVocabulary.SimulatedReplayProofFileName,
            "Goal106 simulated save/load/replay proof"),
        (OfflineGeoworldObjectiveAcceptanceRunVocabulary.SourceGoal106Root
         + "/"
         + OfflineGeoworldSessionPersistenceReplayVocabulary.UnityScriptInventoryFileName,
            "Goal106 Unity script inventory"),
        (OfflineGeoworldObjectiveAcceptanceRunVocabulary.SourceGoal106Root
         + "/"
         + OfflineGeoworldSessionPersistenceReplayVocabulary.EditorWindowInventoryFileName,
            "Goal106 Unity editor helper inventory"),
        (OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySaveLoadControllerScriptPath,
            "Goal106 Unity save/load controller"),
        (OfflineGeoworldSessionPersistenceReplayVocabulary.UnityReplayControllerScriptPath,
            "Goal106 Unity replay controller"),
        (OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveAcceptanceControllerScriptPath,
            "Goal107 Unity objective acceptance controller")
    ];

    private static IEnumerable<string> CandidateSourceFiles(string root)
    {
        var paths = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/OfflineGeoworldObjectiveAcceptanceRun");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceService.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewEvidenceWriter.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewReportBuilder.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewReportRenderer.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWinFormsBindingScanner.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceModels.Goal107.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewOfflineGeoworldObjectiveAcceptanceInspector.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldPreviewGoal107Quality.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewProofStatusLoader.Goal107.cs");
        AddPath(paths, root, "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/OfflineGeoworldObjectiveAcceptanceRun");
        AddPath(paths, root, "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldObjectiveAcceptanceRunProductSmokeTests.cs");
        AddPath(paths, root, OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveStateScriptPath);
        AddPath(paths, root, OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveTrackerScriptPath);
        AddPath(paths, root, OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveAcceptanceControllerScriptPath);
        AddPath(paths, root, OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityEditorWindowScriptPath);
        return paths;
    }

    private static void AddPath(ISet<string> paths, string root, string relativePath) =>
        paths.Add(Resolve(root, relativePath));

    private static void AddDirectory(ISet<string> paths, string root, string relativePath)
    {
        var directory = Resolve(root, relativePath);
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly))
        {
            paths.Add(file);
        }
    }

    private static (string RelativePath, int Lines) ScanSourceFile(string root, string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        return (Relative(root, path), CountLines(text));
    }

    private static OfflineGeoworldObjectiveSourceLineageRecord SourceLineageRecord(
        string root,
        string relativePath,
        string purpose)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        return new OfflineGeoworldObjectiveSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? HashFile(path) : string.Empty,
            Purpose = purpose
        };
    }
}
