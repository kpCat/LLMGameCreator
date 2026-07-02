using System.Text;

namespace LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;

internal sealed class EditDrivenUnityAlphaStreamingAssetsHandoffPayloadBuilder
{
    public SortedDictionary<string, string> BuildPayloadFiles(
        Goal082SourceContext context,
        string implementationStatus)
    {
        var projectedPackageIndex = new EditDrivenUnityAlphaStreamingAssetsHandoffProjectedPackageIndexPayload
        {
            ProjectedPackageHash = context.ProjectedPackageHash,
            ProjectedPackageByteCount = Encoding.UTF8.GetByteCount(context.Goal080PackageJson),
            ProjectedPackageIndexHash = context.SourceArtifactManifest.Goal080ProjectedPackageIndexHash,
            ValidationReportHash = context.SourceArtifactManifest.Goal080ValidationReportHash,
            RuntimePreviewBridgeProofHash = context.SourceArtifactManifest.Goal080RuntimePreviewBridgeProofHash,
            RowCount = context.RowCount,
            TargetCount = context.TargetCount,
            ActionCount = context.Goal078ActionCount
        };
        var commandIndex = new EditDrivenUnityAlphaStreamingAssetsHandoffCommandIndexPayload
        {
            CommandScriptHash = context.CommandScriptHash,
            RowCount = context.RowCount,
            TargetCount = context.TargetCount,
            Goal078ActionCount = context.Goal078ActionCount,
            CommandCount = context.CommandCount,
            CommandTypeCounts = context.CommandTypeCounts
        };
        var transcriptIndex = new EditDrivenUnityAlphaStreamingAssetsHandoffTranscriptIndexPayload
        {
            TranscriptHash = context.TranscriptHash,
            StateHashChainHash = context.StateHashChainHash,
            CoverageLedgerHash = context.CoverageLedgerHash,
            InitialStateHash = Field(context.Goal081ReportMarkdown, "initialPackageReadStateHash"),
            FinalStateHash = Field(context.Goal081TranscriptJson, "finalStateHash"),
            FinalCoverageStateHash = context.FinalCoverageStateHash,
            ReplayFinalStateHash = context.ReplayFinalStateHash,
            ReplayFinalHashMatchesOriginal = BoolField(context.Goal081TranscriptJson, "replayFinalHashMatchesOriginal"),
            CoveredRowCount = context.RowCount,
            CoveredTargetCount = context.TargetCount,
            CoveredGoal078ActionCount = context.Goal078ActionCount
        };

        var payload = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["projected-package-index.json"] = Serialize(projectedPackageIndex),
            ["playthrough-command-index.json"] = Serialize(commandIndex),
            ["playthrough-transcript-index.json"] = Serialize(transcriptIndex),
            ["README.md"] = BuildReadme(context)
        };

        var expectedHashes = new EditDrivenUnityAlphaStreamingAssetsHandoffExpectedHashes
        {
            ProjectedPackageHash = context.ProjectedPackageHash,
            Goal080ReportHash = context.SourceArtifactManifest.Goal080ReportHash,
            Goal080RuntimePreviewBridgeProofHash = context.SourceArtifactManifest.Goal080RuntimePreviewBridgeProofHash,
            Goal081PackageReadProofHash = context.SourceArtifactManifest.Goal081PackageReadProofHash,
            Goal081CommandScriptHash = context.CommandScriptHash,
            Goal081TranscriptHash = context.TranscriptHash,
            Goal081StateHashChainHash = context.StateHashChainHash,
            Goal081CoverageLedgerHash = context.CoverageLedgerHash,
            Goal081NegativeProofHash = context.SourceArtifactManifest.Goal081NegativeProofHash,
            ProjectedPackageIndexPayloadHash = Hash(payload["projected-package-index.json"]),
            PlaythroughCommandIndexPayloadHash = Hash(payload["playthrough-command-index.json"]),
            PlaythroughTranscriptIndexPayloadHash = Hash(payload["playthrough-transcript-index.json"]),
            RowCount = context.RowCount,
            TargetCount = context.TargetCount,
            Goal078ActionCount = context.Goal078ActionCount,
            CommandCount = context.CommandCount,
            FinalCoverageStateHash = context.FinalCoverageStateHash,
            ReplayFinalStateHash = context.ReplayFinalStateHash
        };
        payload["expected-hashes.json"] = Serialize(expectedHashes);

        var handoffManifest = new EditDrivenUnityAlphaStreamingAssetsHandoffPayloadManifest
        {
            ImplementationStatus = implementationStatus,
            PayloadFileCount = EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredUnityPayloadFileNames.Count,
            RowCount = context.RowCount,
            TargetCount = context.TargetCount,
            Goal078ActionCount = context.Goal078ActionCount,
            CommandCount = context.CommandCount,
            ProjectedPackageHash = context.ProjectedPackageHash,
            ExpectedHashesHash = Hash(payload["expected-hashes.json"])
        };
        payload["handoff-manifest.json"] = Serialize(handoffManifest);

        return new SortedDictionary<string, string>(
            payload.OrderBy(item => item.Key, StringComparer.Ordinal).ToDictionary(
                item => item.Key,
                item => item.Value,
                StringComparer.Ordinal),
            StringComparer.Ordinal);
    }

    public static EditDrivenUnityAlphaStreamingAssetsHandoffFileLedger BuildFileLedger(
        IReadOnlyDictionary<string, string> payloadFiles)
    {
        var diagnostics = new List<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic>();
        foreach (var required in EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredUnityPayloadFileNames)
        {
            if (!payloadFiles.ContainsKey(required))
            {
                diagnostics.Add(Error(
                    "goal082.payload.required_file_missing",
                    required,
                    "Required Unity StreamingAssets payload file is missing."));
            }
        }

        var files = payloadFiles
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new EditDrivenUnityAlphaStreamingAssetsHandoffFileEntry
            {
                RelativePath = item.Key,
                Role = Role(item.Key),
                Sha256 = Hash(item.Value),
                ByteCount = Encoding.UTF8.GetByteCount(item.Value)
            })
            .ToList();

        return new EditDrivenUnityAlphaStreamingAssetsHandoffFileLedger
        {
            Passed = diagnostics.Count == 0
                     && files.Count == EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredUnityPayloadFileNames.Count,
            FileCount = files.Count,
            Files = files,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static string BuildReadme(Goal082SourceContext context)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Goal 082 Unity Alpha StreamingAssets Handoff");
        builder.AppendLine();
        builder.AppendLine("This folder is a compact player-facing handoff for manual Unity Alpha inspection.");
        builder.AppendLine("It is derived from Goal 080 projected GamePackage evidence and Goal 081 runtime-preview playthrough evidence.");
        builder.AppendLine("Runtime and Unity probe code must read this payload from Application.streamingAssetsPath.");
        builder.AppendLine();
        builder.AppendLine("- gate: " + EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.FinalGate + " required");
        builder.AppendLine("- accepted: false");
        builder.AppendLine("- rowCount: " + context.RowCount);
        builder.AppendLine("- targetCount: " + context.TargetCount);
        builder.AppendLine("- goal078ActionCount: " + context.Goal078ActionCount);
        builder.AppendLine("- commandCount: " + context.CommandCount);
        builder.AppendLine("- projectedPackageHash: " + context.ProjectedPackageHash);
        return builder.ToString();
    }

    private static string Role(string fileName) =>
        fileName switch
        {
            "handoff-manifest.json" => "manifest",
            "projected-package-index.json" => "projected_package_index",
            "playthrough-command-index.json" => "command_index",
            "playthrough-transcript-index.json" => "transcript_index",
            "expected-hashes.json" => "expected_hashes",
            "README.md" => "manual_inspection_note",
            _ => "payload"
        };

    private static string Field(string text, string field)
    {
        foreach (var line in text.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            if (line.StartsWith("- " + field + ":", StringComparison.Ordinal))
            {
                return line[(field.Length + 3)..].Trim();
            }
        }

        var marker = "\"" + field + "\":";
        var index = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return string.Empty;
        }

        var after = text[(index + marker.Length)..].TrimStart();
        if (!after.StartsWith("\"", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        var end = after.IndexOf('"', 1);
        return end > 1 ? after[1..end] : string.Empty;
    }

    private static bool BoolField(string json, string field)
    {
        var marker = "\"" + field + "\":";
        var index = json.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return false;
        }

        var after = json[(index + marker.Length)..].TrimStart();
        return after.StartsWith("true", StringComparison.OrdinalIgnoreCase);
    }

    private static string Serialize<T>(T value) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffJson.Serialize(value);

    private static string Hash(string text) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffHash.Sha256Text(text);

    private static IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> diagnostics) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner.SortDiagnostics(diagnostics);

    private static EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic Error(
        string code,
        string target,
        string message) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic.Error(code, target, message);
}
