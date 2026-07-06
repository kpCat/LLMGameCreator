namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class GamePackageCandidatePipelineOperatorVocabulary
{
    public const string GoalId = "goal_132_winforms_candidate_pipeline_operator_panel";
    public const string ScenarioId = "goal-132-winforms-candidate-pipeline-operator-panel";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-132-winforms-candidate-pipeline-operator-panel";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-132-winforms-candidate-pipeline-operator-panel";
    public const string DocumentationPath =
        "docs/manual-acceptance/winforms-candidate-pipeline-operator-panel.md";

    public const string NormalCommand =
        ".devflow\\scripts\\run-gamepackage-candidate-recipe-pipeline.cmd";
    public const string DryRunCommand =
        "powershell -NoProfile -ExecutionPolicy Bypass -File .devflow\\scripts\\run-gamepackage-candidate-recipe-pipeline.ps1 -DryRun";
    public const string FullRunCommand =
        "powershell -NoProfile -ExecutionPolicy Bypass -File .devflow\\scripts\\run-gamepackage-candidate-recipe-pipeline.ps1 -ApplyCleanup";
    public const string PipelineScriptPath =
        ".devflow/scripts/run-gamepackage-candidate-recipe-pipeline.ps1";
    public const string PipelineCmdPath =
        ".devflow/scripts/run-gamepackage-candidate-recipe-pipeline.cmd";

    public const string DashboardFileName = "candidate-pipeline-operator-dashboard.json";
    public const string ResultFileName = "candidate-pipeline-operator-result.json";
    public const string ScriptScanFileName = "candidate-pipeline-operator-script-scan.json";
    public const string WinFormsScanFileName = "candidate-pipeline-operator-winforms-scan.json";
    public const string NegativeProofFileName = "candidate-pipeline-operator-negative-proof.json";
    public const string ReportFileName = "candidate-pipeline-operator-report.md";
    public const string FileIndexFileName = "candidate-pipeline-operator-file-index.json";

    public const string DashboardRelativePath = ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string ResultRelativePath = ProceduralOutputDirectory + "/" + ResultFileName;
    public const string ScriptScanRelativePath = ProceduralOutputDirectory + "/" + ScriptScanFileName;
    public const string WinFormsScanRelativePath = ProceduralOutputDirectory + "/" + WinFormsScanFileName;
    public const string NegativeProofRelativePath =
        ProceduralOutputDirectory + "/" + NegativeProofFileName;
    public const string ReportRelativePath = ProceduralOutputDirectory + "/" + ReportFileName;
    public const string FileIndexRelativePath = ProceduralOutputDirectory + "/" + FileIndexFileName;

    public const string Goal131ResultPath =
        GamePackageCandidateRecipePipelineVocabulary.PipelineResultRelativePath;
    public const string Goal131ScoringResultPath =
        GamePackageCandidateRecipePipelineVocabulary.ScoringResultRelativePath;
    public const string Goal131SelectedHandoffPath =
        GamePackageCandidateRecipePipelineVocabulary.SelectedCandidateHandoffRelativePath;
}

public sealed record GamePackageCandidatePipelineOperatorBuildResult
{
    public GamePackageCandidatePipelineOperatorDashboard Dashboard { get; init; } = new();
    public GamePackageCandidatePipelineOperatorRunResult OperatorResult { get; init; } = new();
    public GamePackageCandidatePipelineOperatorScriptScan ScriptScan { get; init; } = new();
    public GamePackageCandidatePipelineOperatorWinFormsScan WinFormsScan { get; init; } = new();
    public GamePackageCandidatePipelineOperatorNegativeProof NegativeProof { get; init; } = new();
    public GamePackageCandidatePipelineOperatorFileIndex ProceduralFileIndex { get; init; } = new();
    public GamePackageCandidatePipelineOperatorFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record GamePackageCandidatePipelineOperatorWriteResult
{
    public GamePackageCandidatePipelineOperatorBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record GamePackageCandidatePipelineOperatorRunResultInput
{
    public string RunMode { get; init; } = "status_refresh";
    public string Command { get; init; } = GamePackageCandidatePipelineOperatorVocabulary.NormalCommand;
    public int ExitCode { get; init; } = -1;
    public long DurationMilliseconds { get; init; }
    public string OutputTail { get; init; } = string.Empty;
    public string ErrorTail { get; init; } = string.Empty;
}

public sealed record GamePackageCandidatePipelineOperatorDashboard
{
    public string GoalId { get; init; } = GamePackageCandidatePipelineOperatorVocabulary.GoalId;
    public string OperatorStatus { get; init; } = "BLOCKED";
    public string NormalCommand { get; init; } =
        GamePackageCandidatePipelineOperatorVocabulary.NormalCommand;
    public string DryRunCommand { get; init; } =
        GamePackageCandidatePipelineOperatorVocabulary.DryRunCommand;
    public string FullRunCommand { get; init; } =
        GamePackageCandidatePipelineOperatorVocabulary.FullRunCommand;
    public bool WinFormsPanelPresent { get; init; }
    public bool RefreshButtonPresent { get; init; }
    public bool CopyCommandButtonPresent { get; init; }
    public bool DryRunButtonPresent { get; init; }
    public bool RunButtonPresent { get; init; }
    public bool AsyncRunPresent { get; init; }
    public string ResultPath { get; init; } =
        GamePackageCandidatePipelineOperatorVocabulary.Goal131ResultPath;
    public string SelectedCandidateHandoffPath { get; init; } =
        GamePackageCandidatePipelineOperatorVocabulary.Goal131SelectedHandoffPath;
    public string SelectedCandidateId { get; init; } = string.Empty;
    public int SelectedCandidateScore { get; init; }
    public int CandidateCount { get; init; }
    public int PassedCandidates { get; init; }
    public int FailedCandidates { get; init; }
    public bool MatrixPassed { get; init; }
    public int LastOperatorExitCode { get; init; } = -1;
    public long LastOperatorDurationMilliseconds { get; init; }
    public string LastOperatorRunMode { get; init; } = string.Empty;
    public string OutputTail { get; init; } = string.Empty;
    public bool ManualUnityOptional { get; init; } = true;
    public bool ProjectionOnly { get; init; } = true;
    public bool SamplePackageReadOnly { get; init; }
    public bool OperatorResultPresent { get; init; }
    public string EvidencePath { get; init; } =
        GamePackageCandidatePipelineOperatorVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        GamePackageCandidatePipelineOperatorVocabulary.ExportPackageDirectory;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GamePackageCandidatePipelineOperatorRunResult
{
    public string GoalId { get; init; } = GamePackageCandidatePipelineOperatorVocabulary.GoalId;
    public bool OperatorResultCaptured { get; init; }
    public string RunMode { get; init; } = "status_refresh";
    public string Command { get; init; } = GamePackageCandidatePipelineOperatorVocabulary.NormalCommand;
    public int ExitCode { get; init; } = -1;
    public long DurationMilliseconds { get; init; }
    public string OutputTail { get; init; } = string.Empty;
    public string ErrorTail { get; init; } = string.Empty;
    public string ResultPath { get; init; } =
        GamePackageCandidatePipelineOperatorVocabulary.Goal131ResultPath;
    public string SelectedCandidateId { get; init; } = string.Empty;
    public int SelectedCandidateScore { get; init; }
    public int CandidateCount { get; init; }
    public int PassedCandidates { get; init; }
    public int FailedCandidates { get; init; }
    public bool MatrixPassed { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool ProjectionOnly { get; init; } = true;
    public bool SamplePackageReadOnly { get; init; }
}

public sealed record GamePackageCandidatePipelineOperatorScriptScan
{
    public string GoalId { get; init; } = GamePackageCandidatePipelineOperatorVocabulary.GoalId;
    public bool PipelineScriptExists { get; init; }
    public bool PipelineCmdExists { get; init; }
    public bool SupportsDryRun { get; init; }
    public bool SupportsApplyCleanup { get; init; }
    public bool NormalCommandUsesCmdWrapper { get; init; }
    public bool DryRunCommandUsesScriptDryRun { get; init; }
    public bool FullRunCommandUsesApplyCleanup { get; init; }
    public bool RejectsManualInputRoot { get; init; }
    public bool NoLlmProviderNetwork { get; init; }
    public bool NoBroadGitClean { get; init; }
    public bool Passed { get; init; }
}

public sealed record GamePackageCandidatePipelineOperatorWinFormsScan
{
    public string GoalId { get; init; } = GamePackageCandidatePipelineOperatorVocabulary.GoalId;
    public bool PanelPartialExists { get; init; }
    public bool WinFormsPanelPresent { get; init; }
    public bool RefreshButtonPresent { get; init; }
    public bool CopyCommandButtonPresent { get; init; }
    public bool DryRunButtonPresent { get; init; }
    public bool RunButtonPresent { get; init; }
    public bool AsyncRunPresent { get; init; }
    public bool MarshalUiUpdatesPresent { get; init; }
    public bool UsesApplicationOperatorService { get; init; }
    public bool ShowsOutputTail { get; init; }
    public bool NoDesignerChangeRequired { get; init; } = true;
    public bool Passed { get; init; }
}

public sealed record GamePackageCandidatePipelineOperatorNegativeProof
{
    public string GoalId { get; init; } = GamePackageCandidatePipelineOperatorVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool ProjectionOnly { get; init; } = true;
    public bool SamplePackageReadOnly { get; init; }
    public bool DoesNotWriteSamplePackage { get; init; } = true;
    public bool RuntimeSchemaProviderLuaGeneratorLibraryUnchanged { get; init; } = true;
    public bool UnityAssetsProjectSettingsPackagesUnchanged { get; init; } = true;
    public bool ExistingDevflowRunnerScriptsReadOnly { get; init; } = true;
    public bool NoManualInputArtifacts { get; init; } = true;
}

public sealed record GamePackageCandidatePipelineOperatorFileIndex
{
    public string GoalId { get; init; } = GamePackageCandidatePipelineOperatorVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<GamePackageCandidatePipelineOperatorFileIndexEntry> Files { get; init; } = [];
}

public sealed record GamePackageCandidatePipelineOperatorFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
