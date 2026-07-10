using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal141RuntimeBackedPlayerCommandRoundtripProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = RuntimeBackedPlayerCommandRoundtripVocabulary.ProceduralOutputDirectory;
        var goalId = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId;
        if (!Goal141DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal141.runtime_backed_player_command_roundtrip.dashboard",
                RuntimeBackedPlayerCommandRoundtripVocabulary.DashboardFileName,
                "unityConsumesRoundtripResult", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal141.runtime_backed_player_command_roundtrip.request",
                RuntimeBackedPlayerCommandRoundtripVocabulary.RequestFileName,
                "controlRequestBridgePresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal141.runtime_backed_player_command_roundtrip.result",
                RuntimeBackedPlayerCommandRoundtripVocabulary.ResultFileName,
                "runtimeBackedPlayerCommandRoundtripPassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal141.runtime_backed_player_command_roundtrip.session",
                RuntimeBackedPlayerCommandRoundtripVocabulary.SessionFileName,
                "controlRequestBridgePresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal141.runtime_backed_player_command_roundtrip.model",
                RuntimeBackedPlayerCommandRoundtripVocabulary.ModelFileName,
                "unityConsumesRoundtripResult", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal141.runtime_backed_player_command_roundtrip.unity_smoke",
                RuntimeBackedPlayerCommandRoundtripVocabulary.UnitySmokeFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal141.runtime_backed_player_command_roundtrip.negative_proof",
                RuntimeBackedPlayerCommandRoundtripVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal141.runtime_backed_player_command_roundtrip.goal140_acceptance",
                RuntimeBackedPlayerCommandRoundtripVocabulary.Goal140AcceptanceFileName,
                "acceptedByHuman", ledger, diagnostics)
        ];
    }

    private static bool Goal141DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + RuntimeBackedPlayerCommandRoundtripVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal138String(dashboard.RootElement, "status") == "GREEN";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
