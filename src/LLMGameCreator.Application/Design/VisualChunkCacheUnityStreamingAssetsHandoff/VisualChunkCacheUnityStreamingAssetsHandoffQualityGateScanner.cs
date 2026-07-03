using System.Text;

namespace LLMGameCreator.Application.Design.VisualChunkCacheUnityStreamingAssetsHandoff;

internal sealed class VisualChunkCacheUnityStreamingAssetsHandoffQualityGateScanner
{
    private static readonly string[] ProviderLlmNetworkMarkers =
    [
        "LLMProvider",
        "ComfyUI",
        "Fooocus",
        "ProviderCallRequested",
        "HttpClient",
        "UnityWebRequest",
        "WebRequest",
        "TcpClient",
        "http://",
        "https://"
    ];

    public VisualChunkCacheUnityProbeSourceInventory BuildProbeSourceInventory(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var path = Resolve(root, VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityProbeScriptPath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var diagnostics = new List<VisualChunkCacheUnityHandoffDiagnostic>();
        var usesStreamingAssets = text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var usesRoot = text.Contains(
            VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var exposesInspector = text.Contains("public ProbeResult LastResult", StringComparison.Ordinal)
                               && text.Contains("[SerializeField]", StringComparison.Ordinal);
        var noBootstrap = !text.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal);
        var noMarkers = !ContainsProviderLlmNetworkMarker(text);

        AddIfFalse(diagnostics, exists, "goal095.probe.source_missing", VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityProbeScriptPath, "Unity probe source file must exist.");
        AddIfFalse(diagnostics, usesStreamingAssets, "goal095.probe.streamingassets_missing", VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityProbeScriptPath, "Unity probe must read from Application.streamingAssetsPath.");
        AddIfFalse(diagnostics, usesRoot, "goal095.probe.payload_root_missing", VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityProbeScriptPath, "Unity probe must target the Goal095 StreamingAssets root.");
        AddIfFalse(diagnostics, exposesInspector, "goal095.probe.inspector_fields_missing", VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityProbeScriptPath, "Unity probe must expose simple inspector result fields.");
        AddIfFalse(diagnostics, noBootstrap, "goal095.probe.bootstrap_dependency", VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityProbeScriptPath, "Unity probe must not depend on AlphaRuntimeBootstrap.");
        AddIfFalse(diagnostics, noMarkers, "goal095.probe.provider_network_marker", VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityProbeScriptPath, "Unity probe must not contain provider, LLM, or network call markers.");

        return new VisualChunkCacheUnityProbeSourceInventory
        {
            Passed = diagnostics.Count == 0,
            ProbeExists = exists,
            ProbeSha256 = exists ? VisualChunkCacheUnityStreamingAssetsHandoffHash.Sha256File(path) : string.Empty,
            ProbeLineCount = CountLines(text),
            UsesApplicationStreamingAssetsPath = usesStreamingAssets,
            UsesExpectedPayloadRoot = usesRoot,
            ExposesInspectorResultFields = exposesInspector,
            DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
            HasNoProviderLlmNetworkMarkers = noMarkers,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public VisualChunkCacheUnityQualityGateScan Scan(
        string repositoryRootPath,
        Goal095SourceContext context,
        VisualChunkCacheUnityFileLedger ledger,
        VisualChunkCacheUnitySimulatedReadProof readProof,
        VisualChunkCacheUnityNegativeProof negativeProof,
        VisualChunkCacheUnityProbeSourceInventory probeInventory,
        IReadOnlyDictionary<string, string> payloadFiles)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var diagnostics = new List<VisualChunkCacheUnityHandoffDiagnostic>();
        diagnostics.AddRange(context.Diagnostics);
        diagnostics.AddRange(context.SourceLineage.Diagnostics);
        diagnostics.AddRange(ledger.Diagnostics);
        diagnostics.AddRange(readProof.Diagnostics);
        diagnostics.AddRange(probeInventory.Diagnostics);

        var sourceFiles = CandidateFiles(root)
            .Where(File.Exists)
            .Select(path => ScanSourceFile(root, path))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        foreach (var tooLong in sourceFiles.Where(item => item.LogicalLineCount > 700))
        {
            diagnostics.Add(Error("goal095.quality.source_file_over_700", tooLong.RelativePath, "Goal095 C# files must stay below 700 logical lines."));
        }

        foreach (var overHardLimit in sourceFiles.Where(item => item.LogicalLineCount > 1000))
        {
            diagnostics.Add(Error("goal095.quality.source_file_over_1000", overHardLimit.RelativePath, "Goal095 C# files must stay below 1000 logical lines."));
        }

        var alphaPath = Resolve(root, VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapPath);
        var alphaBytes = File.Exists(alphaPath) ? File.ReadAllBytes(alphaPath) : [];
        var alphaHash = alphaBytes.Length == 0
            ? string.Empty
            : VisualChunkCacheUnityStreamingAssetsHandoffHash.Sha256Bytes(alphaBytes);
        var alphaLineCount = CountLines(alphaBytes);
        var alphaUnchanged = alphaLineCount == VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapExpectedLineCount
                             && string.Equals(
                                 alphaHash,
                                 VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase);
        AddIfFalse(diagnostics, alphaUnchanged, "goal095.quality.alpha_bootstrap_changed", VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapPath, "AlphaRuntimeBootstrap.cs must remain unchanged.");

        var noAbsolutePaths = !payloadFiles.Values.Any(ContainsAbsolutePath);
        var noRawDump = !payloadFiles.Values.Any(ContainsRawFullWorldDumpMarker);
        var noBinary = !payloadFiles.Keys.Any(IsBinaryOrRasterPath)
                       && !payloadFiles.Values.Any(value => BinaryOrRasterExtensions.Any(ext =>
                           value.Contains(ext, StringComparison.OrdinalIgnoreCase)));
        var noPrompt = !payloadFiles.Keys.Any(value => value.Contains("prompt", StringComparison.OrdinalIgnoreCase))
                       && !payloadFiles.Values.Any(value => value.Contains("prompt dump", StringComparison.OrdinalIgnoreCase));

        AddIfFalse(diagnostics, ledger.Passed, "goal095.quality.ledger_failed", StreamingAssetsLedgerTarget, "StreamingAssets ledger must pass.");
        AddIfFalse(diagnostics, readProof.Passed, "goal095.quality.read_proof_failed", SimulatedReadProofTarget, "Simulated Unity read proof must pass.");
        AddIfFalse(diagnostics, negativeProof.Passed, "goal095.quality.negative_proof_failed", NegativeProofTarget, "Negative proof must pass.");
        AddIfFalse(diagnostics, noAbsolutePaths, "goal095.quality.absolute_path", "payload", "Payload/evidence must not contain absolute local paths.");
        AddIfFalse(diagnostics, noRawDump, "goal095.quality.raw_full_world_dump", "payload", "Payload must not contain raw full-world dump markers.");
        AddIfFalse(diagnostics, noBinary, "goal095.quality.binary_raster_media", "payload", "Payload must not reference binary/raster media.");
        AddIfFalse(diagnostics, noPrompt, "goal095.quality.prompt_dump", "payload", "Payload must not contain prompt dumps.");

        var orderedDiagnostics = SortDiagnostics(diagnostics);
        return new VisualChunkCacheUnityQualityGateScan
        {
            Accepted = false,
            Passed = orderedDiagnostics.All(item => item.Severity != "error")
                     && context.SourceLineage.Passed
                     && ledger.Passed
                     && readProof.Passed
                     && negativeProof.Passed
                     && probeInventory.Passed
                     && alphaUnchanged,
            SourceLineagePassed = context.SourceLineage.Passed,
            StreamingAssetsMirrorPassed = ledger.Passed,
            SimulatedReadProofPassed = readProof.Passed,
            NegativeProofPassed = negativeProof.Passed,
            UnityProbeSourcePassed = probeInventory.Passed,
            AlphaRuntimeBootstrapUnchanged = alphaUnchanged,
            AlphaRuntimeBootstrapAfterHash = alphaHash,
            AlphaRuntimeBootstrapAfterLineCount = alphaLineCount,
            ScannedCSharpFileCount = sourceFiles.Count,
            MaxLogicalLineCount = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(item => item.LogicalLineCount),
            FilesOver700LogicalLinesCount = sourceFiles.Count(item => item.LogicalLineCount > 700),
            FilesOver1000LogicalLinesCount = sourceFiles.Count(item => item.LogicalLineCount > 1000),
            NoAbsolutePaths = noAbsolutePaths,
            NoRawFullWorldDump = noRawDump,
            NoBinaryOrRasterMedia = noBinary,
            NoPromptDumps = noPrompt,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/VisualChunkCacheUnityStreamingAssetsHandoff/",
                "tests/LLMGameCreator.Tests/Application/VisualChunkCacheUnityStreamingAssetsHandoff/",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualChunkCacheUnityStreamingAssetsHandoffProductSmokeTests.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/VisualChunkCacheHandoffProbe.cs",
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/VisualChunkCacheGoal095/",
                ".llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/",
                "docs/agent-tasks/goal-095-visual-chunk-cache-unity-streamingassets-handoff/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            SourceFiles = sourceFiles,
            Diagnostics = orderedDiagnostics
        };
    }

    public static IReadOnlyList<VisualChunkCacheUnityHandoffDiagnostic> SortDiagnostics(
        IEnumerable<VisualChunkCacheUnityHandoffDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(
                item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message,
                StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Severity == "error" ? 0 : 1)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static IEnumerable<string> CandidateFiles(string root)
    {
        var paths = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/VisualChunkCacheUnityStreamingAssetsHandoff");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/VisualChunkCacheUnityStreamingAssetsHandoff");
        paths.Add(Resolve(root, "tests/LLMGameCreator.Tests/ProductSmoke/VisualChunkCacheUnityStreamingAssetsHandoffProductSmokeTests.cs"));
        paths.Add(Resolve(root, VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityProbeScriptPath));
        return paths;
    }

    private static void AddDirectory(ISet<string> paths, string root, string relativePath)
    {
        var fullPath = Resolve(root, relativePath);
        if (!Directory.Exists(fullPath))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(fullPath, "*.cs", SearchOption.AllDirectories))
        {
            paths.Add(file);
        }
    }

    private static VisualChunkCacheUnitySourceFileScan ScanSourceFile(string root, string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        var lines = text.Split('\n');
        return new VisualChunkCacheUnitySourceFileScan
        {
            RelativePath = Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'),
            LogicalLineCount = CountLines(text),
            MaxLineLength = lines.Length == 0 ? 0 : lines.Max(line => line.TrimEnd('\r').Length)
        };
    }

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;

    private static int CountLines(byte[] bytes) =>
        bytes.Length == 0 ? 0 : Encoding.UTF8.GetString(bytes).Split('\n').Length;

    private static bool ContainsProviderLlmNetworkMarker(string value) =>
        ProviderLlmNetworkMarkers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static bool ContainsAbsolutePath(string value) =>
        value.Contains(@"C:\", StringComparison.OrdinalIgnoreCase)
        || value.Contains("C:/", StringComparison.OrdinalIgnoreCase)
        || value.Contains("/Users/", StringComparison.OrdinalIgnoreCase)
        || value.Contains(@"\\?\", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsRawFullWorldDumpMarker(string value) =>
        value.Contains("\"rawFullWorldDumpMarker\": true", StringComparison.OrdinalIgnoreCase)
        || value.Contains("\"containsRawFullWorldCellDump\": true", StringComparison.OrdinalIgnoreCase)
        || value.Contains("\"noRawFullWorldDump\": false", StringComparison.OrdinalIgnoreCase);

    private static bool IsBinaryOrRasterPath(string path) =>
        BinaryOrRasterExtensions.Contains(Path.GetExtension(path));

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var normalizedRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + path);
        }

        return path;
    }

    private static void AddIfFalse(
        ICollection<VisualChunkCacheUnityHandoffDiagnostic> diagnostics,
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

    private static VisualChunkCacheUnityHandoffDiagnostic Error(
        string code,
        string target,
        string message) =>
        VisualChunkCacheUnityHandoffDiagnostic.Error(code, target, message);

    private const string StreamingAssetsLedgerTarget =
        "visual-chunk-cache-unity-streamingassets-ledger.json";
    private const string SimulatedReadProofTarget =
        "visual-chunk-cache-unity-simulated-read-proof.json";
    private const string NegativeProofTarget =
        "visual-chunk-cache-unity-negative-proof.json";

    private static readonly HashSet<string> BinaryOrRasterExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".wav",
        ".ogg",
        ".mp3",
        ".mp4",
        ".asset",
        ".bytes"
    };
}
