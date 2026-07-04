namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;

public sealed record OfflineGeoworldActualUnityEditorGitRevision
{
    public string Revision { get; init; } = "HEAD";
    public string Commit { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public bool ContainsGoal102ACommit { get; init; }
}

public sealed record OfflineGeoworldActualUnityEditorGitHead
{
    public string RelativePath { get; init; } = string.Empty;
    public string GitCommand { get; init; } = string.Empty;
    public bool BlobRead { get; init; }
    public int BlobByteCount { get; init; }
    public string Diagnostic { get; init; } = string.Empty;
    public OfflineGeoworldUnityEditorSourceFormatFileScan Scan { get; init; } = new();
}

public sealed record OfflineGeoworldActualUnityEditorChangedPathInventory
{
    public bool TargetFileChanged { get; init; }
    public bool NoForbiddenAreasChanged { get; init; }
    public IReadOnlyList<string> ChangedPaths { get; init; } = [];
    public IReadOnlyList<string> ForbiddenChangedPaths { get; init; } = [];
}

public sealed record OfflineGeoworldActualUnityEditorSourceBeforeAfter
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldActualUnityEditorSourceReformatVocabulary.BeforeAfterSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldActualUnityEditorSourceReformatVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public OfflineGeoworldActualUnityEditorGitRevision GitHead { get; init; } = new();
    public string BeforeSource { get; init; } = string.Empty;
    public string AfterSource { get; init; } = string.Empty;
    public bool ActualHeadBeforeBlobRead { get; init; }
    public bool ActualHeadBeforeMalformedDetected { get; init; }
    public bool WorkingTreeSourceReadable { get; init; }
    public bool TargetFileChanged { get; init; }
    public OfflineGeoworldUnityEditorSourceFormatFileScan ActualHeadBefore { get; init; } = new();
    public OfflineGeoworldUnityEditorSourceFormatFileScan WorkingTreeAfter { get; init; } = new();
    public OfflineGeoworldUnityEditorAlphaRuntimeBootstrapGuard AlphaRuntimeBootstrap { get; init; } = new();
    public OfflineGeoworldActualUnityEditorChangedPathInventory ChangedPaths { get; init; } = new();
}

public sealed record OfflineGeoworldActualUnityEditorSourceNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldActualUnityEditorSourceNegativeProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldActualUnityEditorSourceReformatVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldActualUnityEditorSourceReformatVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<OfflineGeoworldActualUnityEditorSourceNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldActualUnityEditorTrustAudit
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldActualUnityEditorSourceReformatVocabulary.TrustAuditSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldActualUnityEditorSourceReformatVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool Goal102ABeforeAfterExists { get; init; }
    public bool Goal102AQualityGateExists { get; init; }
    public bool Goal102AReportExists { get; init; }
    public bool Goal102AUsedSyntheticBeforeSample { get; init; }
    public bool Goal102AClaimedBeforeMalformedDetected { get; init; }
    public bool Goal102AClaimedQualityGreen { get; init; }
    public bool ActualHeadBlobRead { get; init; }
    public bool ActualHeadBeforeMalformedDetected { get; init; }
    public bool Goal102AEvidenceTrustDefectRecorded { get; init; }
    public bool Goal102AEvidenceConflictsWithActualHead { get; init; }
    public bool SupersededByGoal102B { get; init; }
    public string RootCause { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldActualUnityEditorSourceQualityGate
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldActualUnityEditorSourceReformatVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldActualUnityEditorSourceReformatVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldActualUnityEditorSourceReformatVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public string BlockedReason { get; init; } = string.Empty;
    public bool ActualHeadBeforeBlobRead { get; init; }
    public bool ActualHeadBeforeMalformedDetected { get; init; }
    public bool WorkingTreeSourceReadable { get; init; }
    public bool TargetFileChanged { get; init; }
    public bool Goal102AEvidenceTrustDefectRecorded { get; init; }
    public bool Goal102AEvidenceConflictsWithActualHead { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool NoForbiddenAreasChanged { get; init; }
    public int WorkingTreePhysicalLineCount { get; init; }
    public int WorkingTreeMaxPhysicalLineLength { get; init; }
    public int ChangedPathCount { get; init; }
    public IReadOnlyList<string> ForbiddenChangedPaths { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldActualUnityEditorSourceReformatReport
{
    public string GoalId { get; init; } =
        OfflineGeoworldActualUnityEditorSourceReformatVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldActualUnityEditorSourceReformatVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool QualityGatePassed { get; init; }
    public bool BeforeAfterPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool TrustAuditPassed { get; init; }
    public bool ActualHeadBeforeMalformedDetected { get; init; }
    public bool WorkingTreeSourceReadable { get; init; }
    public bool TargetFileChanged { get; init; }
    public bool Goal102AEvidenceTrustDefectRecorded { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string BlockedReason { get; init; } = string.Empty;
    public string BeforeAfterHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string TrustAuditHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldActualUnityEditorSourceReformatBuildResult
{
    public OfflineGeoworldActualUnityEditorSourceBeforeAfter BeforeAfter { get; init; } = new();
    public OfflineGeoworldActualUnityEditorSourceNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldActualUnityEditorTrustAudit TrustAudit { get; init; } = new();
    public OfflineGeoworldActualUnityEditorSourceQualityGate QualityGate { get; init; } = new();
    public OfflineGeoworldActualUnityEditorSourceReformatReport Report { get; init; } = new();
    public string BeforeAfterJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string TrustAuditJson { get; init; } = string.Empty;
    public string QualityGateJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldActualUnityEditorSourceReformatWriteResult
{
    public OfflineGeoworldActualUnityEditorSourceReformatBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
