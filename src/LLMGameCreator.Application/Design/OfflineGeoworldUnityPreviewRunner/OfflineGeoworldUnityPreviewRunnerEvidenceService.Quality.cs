using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;

public sealed partial class OfflineGeoworldUnityPreviewRunnerEvidenceService
{
    private static OfflineGeoworldPreviewWorkspaceBindingInventory BuildWorkspaceBindingInventory(
        string root)
    {
        var diagnostics = new List<OfflineGeoworldUnityPreviewDiagnostic>();
        var workspaceDirectory = Resolve(
            root,
            "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace");
        var pagePath = Resolve(
            root,
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.cs");
        var workspaceText = Directory.Exists(workspaceDirectory)
            ? string.Join(Environment.NewLine, Directory.EnumerateFiles(workspaceDirectory, "*.cs")
                .Select(File.ReadAllText))
            : string.Empty;
        var pageText = File.Exists(pagePath) ? File.ReadAllText(pagePath, Encoding.UTF8) : string.Empty;
        var group = workspaceText.Contains("offline_geoworld_unity_preview", StringComparison.Ordinal);
        var relative = workspaceText.Contains(
            OfflineGeoworldUnityPreviewRunnerVocabulary.RelativeOutputDirectory,
            StringComparison.Ordinal);
        var winForms = pageText.Contains("offlineGeoworldUnityPreviewCommandCount", StringComparison.Ordinal)
                       && pageText.Contains("offlineGeoworldUnityPreviewKindCoverage", StringComparison.Ordinal);
        var commandCount = pageText.Contains(
            "offlineGeoworldUnityPreviewCommandCount",
            StringComparison.Ordinal);
        var kindCoverage = pageText.Contains(
            "offlineGeoworldUnityPreviewKindCoverage",
            StringComparison.Ordinal);
        var travelSteps = pageText.Contains(
            "offlineGeoworldUnityPreviewTravelWindowStepCount",
            StringComparison.Ordinal);
        var scriptsReady = pageText.Contains(
            "offlineGeoworldUnityPreviewUnityScriptsReady",
            StringComparison.Ordinal);
        var simulated = pageText.Contains(
            "offlineGeoworldUnityPreviewSimulatedCommandProofPassed",
            StringComparison.Ordinal);
        var alpha = pageText.Contains(
            "offlineGeoworldUnityPreviewAlphaRuntimeBootstrapUnchanged",
            StringComparison.Ordinal);
        var negative = pageText.Contains(
            "offlineGeoworldUnityPreviewNegativeProofPassed",
            StringComparison.Ordinal);

        AddIfFalse(group, "goal101.workspace.group_missing", "workspace", diagnostics);
        AddIfFalse(relative, "goal101.workspace.relative_path_missing", "workspace", diagnostics);
        AddIfFalse(winForms, "goal101.workspace.winforms_fields_missing", "page", diagnostics);
        AddIfFalse(commandCount, "goal101.workspace.command_count_missing", "page", diagnostics);
        AddIfFalse(kindCoverage, "goal101.workspace.kind_coverage_missing", "page", diagnostics);
        AddIfFalse(travelSteps, "goal101.workspace.travel_missing", "page", diagnostics);
        AddIfFalse(scriptsReady, "goal101.workspace.scripts_missing", "page", diagnostics);
        AddIfFalse(simulated, "goal101.workspace.simulated_missing", "page", diagnostics);
        AddIfFalse(alpha, "goal101.workspace.alpha_missing", "page", diagnostics);
        AddIfFalse(negative, "goal101.workspace.negative_missing", "page", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldPreviewWorkspaceBindingInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            WorkspaceCatalogIncludesUnityPreviewGroup = group,
            WorkspaceReadsGoal101EvidenceByRelativePath = relative,
            WinFormsPageDisplaysUnityPreviewFields = winForms,
            ShowsPreviewCommandCount = commandCount,
            ShowsCommandKindCoverage = kindCoverage,
            ShowsTravelWindowSteps = travelSteps,
            ShowsUnityScriptsReady = scriptsReady,
            ShowsSimulatedCommandProof = simulated,
            ShowsAlphaRuntimeBootstrapUnchangedStatus = alpha,
            ShowsNegativeProofStatus = negative,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldPreviewSourceLineage BuildSourceLineage(
        string root,
        Goal101SourceContext context)
    {
        var records = SourceLineageInputs()
            .Select(item => SourceLineageRecord(root, item.Path, item.Purpose))
            .ToList();
        var diagnostics = new List<OfflineGeoworldUnityPreviewDiagnostic>();
        foreach (var missing in records.Where(item => !item.Exists))
        {
            diagnostics.Add(OfflineGeoworldUnityPreviewDiagnostic.Error(
                "goal101.lineage.missing",
                missing.RelativePath,
                "Required source lineage artifact is missing."));
        }

        AddIfFalse(context.Goal100AcceptedFalse, "goal101.lineage.goal100_accepted", Goal100Root, diagnostics);
        AddIfFalse(context.Goal100CountsProven, "goal101.lineage.goal100_counts", Goal100Root, diagnostics);
        AddIfFalse(
            context.Goal100SimulatedReadProofPassed,
            "goal101.lineage.goal100_read",
            Goal100Root,
            diagnostics);
        AddIfFalse(
            context.Goal100AlphaRuntimeBootstrapUnchanged,
            "goal101.lineage.goal100_alpha",
            Goal100Root,
            diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldPreviewSourceLineage
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal100AcceptedFalsePreserved = context.Goal100AcceptedFalse,
            Goal100PayloadConsumed = context.Goal100CountsProven && context.Records.Count == 18,
            Goal100SimulatedReadProofPassed = context.Goal100SimulatedReadProofPassed,
            Goal100AlphaRuntimeBootstrapUnchanged = context.Goal100AlphaRuntimeBootstrapUnchanged,
            Records = records,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldPreviewQualityGateScan BuildQualityGate(
        string root,
        Goal101SourceContext context,
        Goal101Payload payload,
        OfflineGeoworldPreviewStreamingAssetsLedger ledger,
        OfflineGeoworldPreviewUnityScriptInventory scripts,
        OfflineGeoworldPreviewSimulatedCommandProof proof,
        OfflineGeoworldPreviewNegativeProof negative,
        OfflineGeoworldPreviewWorkspaceBindingInventory binding,
        OfflineGeoworldPreviewSourceLineage lineage)
    {
        var diagnostics = new List<OfflineGeoworldUnityPreviewDiagnostic>();
        diagnostics.AddRange(context.Diagnostics);
        diagnostics.AddRange(ledger.Diagnostics);
        diagnostics.AddRange(scripts.Diagnostics);
        diagnostics.AddRange(proof.Diagnostics);
        diagnostics.AddRange(binding.Diagnostics);
        diagnostics.AddRange(lineage.Diagnostics);
        var sourceFiles = CandidateSourceFiles(root)
            .Where(File.Exists)
            .Select(path => ScanSourceFile(root, path))
            .ToList();
        foreach (var file in sourceFiles.Where(item => item.Lines > 700))
        {
            diagnostics.Add(OfflineGeoworldUnityPreviewDiagnostic.Error(
                "goal101.source.file_over_700",
                file.RelativePath,
                "New or touched Goal101 C# files must remain below 700 logical lines."));
        }

        foreach (var file in sourceFiles.Where(item => item.Lines > 1000))
        {
            diagnostics.Add(OfflineGeoworldUnityPreviewDiagnostic.Error(
                "goal101.source.file_over_1000",
                file.RelativePath,
                "Changed C# files must remain below 1000 logical lines."));
        }

        var alphaPath = Resolve(root, OfflineGeoworldUnityPreviewRunnerVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldUnityPreviewRunnerVocabulary.AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldUnityPreviewRunnerVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        var commandsBuilt = payload.CommandCatalog.CommandCount == 18;
        var allKinds = payload.CommandCatalog.CommandCountByKind.Keys
            .OrderBy(value => value, StringComparer.Ordinal)
            .SequenceEqual(OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredCommandKinds
                .OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal);
        var travelBuilt = payload.TravelWindowScript.StepCount >= 4
                          && payload.TravelWindowScript.CommandCoverageCount == payload.CommandCatalog.CommandCount;
        var noNetwork = proof.NoProviderOrNetworkMarkers && scripts.HasNoProviderLlmNetworkMarkers;
        var noRaw = proof.NoRawGeodata
                    && payload.CommandCatalog.Commands.All(item => !item.RawGeodataIncluded);
        var noAbsolute = proof.NoAbsolutePaths;
        var noBinary = proof.NoBinaryOrRasterMedia;

        AddIfFalse(context.Goal100CountsProven, "goal101.quality.goal100", Goal100Root, diagnostics);
        AddIfFalse(commandsBuilt, "goal101.quality.commands", "commands", diagnostics);
        AddIfFalse(allKinds, "goal101.quality.command_kinds", "commands", diagnostics);
        AddIfFalse(travelBuilt, "goal101.quality.travel", "travel", diagnostics);
        AddIfFalse(ledger.Passed, "goal101.quality.streamingassets", "StreamingAssets", diagnostics);
        AddIfFalse(scripts.Passed, "goal101.quality.scripts", "Unity scripts", diagnostics);
        AddIfFalse(proof.Passed, "goal101.quality.proof", "simulated command proof", diagnostics);
        AddIfFalse(negative.Passed, "goal101.quality.negative", "negative proof", diagnostics);
        AddIfFalse(binding.Passed, "goal101.quality.workspace", "workspace", diagnostics);
        AddIfFalse(lineage.Passed, "goal101.quality.lineage", "source lineage", diagnostics);
        AddIfFalse(alphaUnchanged, "goal101.quality.alpha_bootstrap",
            OfflineGeoworldUnityPreviewRunnerVocabulary.AlphaRuntimeBootstrapPath, diagnostics);
        AddIfFalse(noNetwork, "goal101.quality.network_provider", "payload/scripts", diagnostics);
        AddIfFalse(noRaw, "goal101.quality.raw_geodata", "payload", diagnostics);
        AddIfFalse(noAbsolute, "goal101.quality.absolute_paths", "payload", diagnostics);
        AddIfFalse(noBinary, "goal101.quality.binary_media", "payload", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldPreviewQualityGateScan
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal100Consumed = context.Goal100CountsProven,
            PreviewCommandsBuilt = commandsBuilt,
            AllCommandKindsMapped = allKinds,
            TravelWindowDemoBuilt = travelBuilt,
            UnityPayloadCreated = ledger.Passed && ledger.PayloadFileCount == 5,
            UnityScriptsReady = scripts.Passed,
            SimulatedCommandProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            SourceLineagePassed = lineage.Passed,
            AlphaRuntimeBootstrapUnchanged = alphaUnchanged,
            AlphaRuntimeBootstrapAfterHash = alphaHash,
            AlphaRuntimeBootstrapAfterLineCount = alphaLineCount,
            NoNetworkOrProviderImplementation = noNetwork,
            NoRawGeodataDump = noRaw,
            NoAbsolutePaths = noAbsolute,
            NoBinaryOrRasterMedia = noBinary,
            CommandCount = payload.Manifest.CommandCount,
            CommandKindCount = payload.Manifest.CommandKindCount,
            TravelWindowStepCount = payload.Manifest.TravelWindowStepCount,
            UnityPayloadFileCount = ledger.PayloadFileCount,
            ScannedCSharpFileCount = sourceFiles.Count,
            MaxLogicalLineCount = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(item => item.Lines),
            FilesOver700LogicalLinesCount = sourceFiles.Count(item => item.Lines > 700),
            FilesOver1000LogicalLinesCount = sourceFiles.Count(item => item.Lines > 1000),
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/OfflineGeoworldUnityPreviewRunner/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityPreviewRunner/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityPreviewRunnerProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs",
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101/",
                ".llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/",
                "docs/agent-tasks/goal-101-offline-geoworld-unity-preview-runner/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = ordered
        };
    }

    private static IReadOnlyDictionary<string, string> BuildEvidencePayloads(
        Goal101Payload payload,
        OfflineGeoworldPreviewStreamingAssetsLedger ledger,
        OfflineGeoworldPreviewUnityScriptInventory scripts,
        OfflineGeoworldPreviewSimulatedCommandProof proof,
        OfflineGeoworldPreviewNegativeProof negative,
        OfflineGeoworldPreviewWorkspaceBindingInventory binding,
        OfflineGeoworldPreviewSourceLineage lineage,
        OfflineGeoworldPreviewQualityGateScan quality) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldUnityPreviewRunnerVocabulary.CommandCatalogFileName] =
                Serialize(payload.CommandCatalog),
            [OfflineGeoworldUnityPreviewRunnerVocabulary.StyleLegendFileName] =
                Serialize(payload.StyleLegend),
            [OfflineGeoworldUnityPreviewRunnerVocabulary.TravelWindowScriptFileName] =
                Serialize(payload.TravelWindowScript),
            [OfflineGeoworldUnityPreviewRunnerVocabulary.StreamingAssetsLedgerFileName] =
                Serialize(ledger),
            [OfflineGeoworldUnityPreviewRunnerVocabulary.UnityScriptInventoryFileName] =
                Serialize(scripts),
            [OfflineGeoworldUnityPreviewRunnerVocabulary.SimulatedCommandProofFileName] =
                Serialize(proof),
            [OfflineGeoworldUnityPreviewRunnerVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [OfflineGeoworldUnityPreviewRunnerVocabulary.WorkspaceBindingInventoryFileName] =
                Serialize(binding),
            [OfflineGeoworldUnityPreviewRunnerVocabulary.SourceLineageFileName] =
                Serialize(lineage),
            [OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName] =
                Serialize(quality)
        };

    private static IReadOnlyList<(string Path, string Purpose)> SourceLineageInputs() =>
    [
        ("docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md", "geoworld pattern study"),
        ("docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md", "streaming policy"),
        ("docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md", "source adapter policy"),
        (Goal100Root + "/offline-geoworld-visual-cache-unity-handoff-report.md", "Goal100 report"),
        (Goal100Root + "/offline-geoworld-visual-cache-catalog.json", "Goal100 visual cache"),
        (Goal100Root + "/offline-geoworld-feature-chunk-ledger.json", "Goal100 feature ledger"),
        (Goal100Root + "/offline-geoworld-unity-handoff-manifest.json", "Goal100 manifest"),
        (Goal100Root + "/offline-geoworld-unity-simulated-read-proof.json", "Goal100 read proof"),
        (Goal100Root + "/offline-geoworld-negative-proof.json", "Goal100 negative proof")
    ];
}
