using System.Text;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;

public static class OfflineGeoworldUnityEditorSourceFormatGuardScanner
{
    public const int MaxAllowedPhysicalLineLength = 500;
    public const int PreferredMaxLogicalLineCount = 700;
    public const int MaxAllowedLogicalLineCount = 1000;

    private static readonly IReadOnlyList<string> UnityScriptRelativePaths =
    [
        OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs"
    ];

    private static readonly IReadOnlyList<string> Goal102ApplicationDirectories =
    [
        "src/LLMGameCreator.Application/Design/OfflineGeoworldUnityEditorPreviewTool",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace"
    ];

    private static readonly IReadOnlyList<string> UnitySceneProjectMutationMarkers =
    [
        "EditorSceneManager.SaveScene",
        "EditorSceneManager.MarkSceneDirty",
        "PrefabUtility",
        "EditorBuildSettings",
        "ProjectSettings/",
        "Packages/manifest.json",
        ".unity",
        ".prefab"
    ];

    public static OfflineGeoworldUnityEditorSourceFormatScan ScanGoal102RelevantSources(
        string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var relativePaths = EnumerateGoal102RelevantSourcePaths(root).ToList();
        var files = relativePaths
            .Select(path => AnalyzeSourceFile(root, path))
            .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToList();

        var diagnostics = new List<OfflineGeoworldUnityEditorPreviewDiagnostic>();
        foreach (var missing in files.Where(file => !file.Exists))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102a.source.missing",
                missing.RelativePath,
                "Required Goal102 source-format target is missing."));
        }

        foreach (var file in files.Where(file => file.ZeroLfSource))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102a.source.zero_lf",
                file.RelativePath,
                "C# source must contain LF physical line separators."));
        }

        foreach (var file in files.Where(file => file.CrOnlySource || file.ContainsCrOnlyLineEndings))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102a.source.cr_only",
                file.RelativePath,
                "C# source must not use CR-only physical line separators."));
        }

        foreach (var file in files.Where(file => file.OnePhysicalLineMultiStatementSource))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102a.source.one_physical_line",
                file.RelativePath,
                "C# source must not collapse multiple statements onto one raw physical line."));
        }

        foreach (var file in files.Where(file => file.RawPhysicalLinesOver500Count > 0))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102a.source.physical_line_over_500",
                file.RelativePath,
                "C# source must not contain raw physical lines over 500 bytes."));
        }

        foreach (var file in files.Where(file => file.MinifiedSourceCandidate))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102a.source.minified_markers",
                file.RelativePath,
                "C# source has minified source markers such as too many braces, semicolons, or using tokens on one line."));
        }

        foreach (var file in files.Where(file => file.FileOver700LogicalLines))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102a.source.file_over_700_lines",
                file.RelativePath,
                "Goal102 C# source should stay at or below 700 logical lines."));
        }

        foreach (var file in files.Where(file => file.FileOver1000LogicalLines))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102a.source.file_over_1000_lines",
                file.RelativePath,
                "Goal102 C# source must stay at or below 1000 logical lines."));
        }

        var maxPhysical = files.Count == 0 ? 0 : files.Max(file => file.RawPhysicalMaxLineLength);
        var maxLogical = files.Count == 0 ? 0 : files.Max(file => file.LogicalLineCount);
        var orderedDiagnostics = diagnostics
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

        return new OfflineGeoworldUnityEditorSourceFormatScan
        {
            Passed = orderedDiagnostics.Count == 0,
            ScannedCSharpFileCount = files.Count(file => file.Exists),
            ByteScannedFileCount = files.Count(file => file.BytesRead),
            EditorWindowScriptScanned = files.Any(file => string.Equals(
                file.RelativePath,
                OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
                StringComparison.Ordinal)),
            UnityPreviewRunnerScriptScanned = files.Any(file => string.Equals(
                file.RelativePath,
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs",
                StringComparison.Ordinal)),
            UnityPrimitiveFactoryScriptScanned = files.Any(file => string.Equals(
                file.RelativePath,
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs",
                StringComparison.Ordinal)),
            UnityTravelWindowScriptScanned = files.Any(file => string.Equals(
                file.RelativePath,
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs",
                StringComparison.Ordinal)),
            ApplicationNamespaceScanned = files.Any(file => file.RelativePath.StartsWith(
                "src/LLMGameCreator.Application/Design/OfflineGeoworldUnityEditorPreviewTool/",
                StringComparison.Ordinal)),
            VisualWorldStreamPreviewWorkspaceScanned = files.Any(file => file.RelativePath.StartsWith(
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                StringComparison.Ordinal)),
            MaxLogicalLineCount = maxLogical,
            MaxPhysicalLineLength = maxPhysical,
            RawPhysicalLinesOver500Count = files.Sum(file => file.RawPhysicalLinesOver500Count),
            ZeroLfSourceFileCount = files.Count(file => file.ZeroLfSource),
            CrOnlySourceFileCount = files.Count(file => file.CrOnlySource),
            RawPhysicalOneLineSourceFileCount = files.Count(file => file.OnePhysicalLineMultiStatementSource),
            MinifiedSourceFileCount = files.Count(file => file.MinifiedSourceCandidate),
            FilesOver700LogicalLinesCount = files.Count(file => file.FileOver700LogicalLines),
            FilesOver1000LogicalLinesCount = files.Count(file => file.FileOver1000LogicalLines),
            Files = files,
            Diagnostics = orderedDiagnostics
        };
    }

    public static OfflineGeoworldUnityEditorSourceFormatFileScan AnalyzeSourceFile(
        string repositoryRootPath,
        string relativePath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var path = Resolve(root, relativePath);
        if (!File.Exists(path))
        {
            return new OfflineGeoworldUnityEditorSourceFormatFileScan
            {
                RelativePath = relativePath.Replace('\\', '/'),
                Exists = false,
                Passed = false
            };
        }

        return AnalyzeSourceBytes(relativePath, File.ReadAllBytes(path));
    }

    public static OfflineGeoworldUnityEditorSourceFormatFileScan AnalyzeSourceBytes(
        string relativePath,
        byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        var text = Encoding.UTF8.GetString(bytes);
        var raw = AnalyzeRawPhysicalLines(bytes);
        var logicalLines = Regex.Split(text, "\r\n|\n|\r");
        var logicalMaxLineLength = logicalLines.Length == 0 ? 0 : logicalLines.Max(line => line.Length);
        var physicalLines = text.Split('\n');
        var lineMarkers = physicalLines
            .Select(line => AnalyzePhysicalLineMarkers(line))
            .ToList();
        var looksLikeCSharp = LooksLikeCSharpSource(relativePath, text);
        var zeroLf = looksLikeCSharp && raw.LfByteCount == 0;
        var crOnly = looksLikeCSharp && raw.LfByteCount == 0 && raw.CrByteCount > 0;
        var oneLineMultiStatement = looksLikeCSharp
                                    && raw.RawPhysicalLineCount <= 1
                                    && lineMarkers.Any(marker => marker.SemicolonCount >= 2
                                                                 || marker.BraceCount >= 4
                                                                 || marker.UsingTokenCount >= 2);
        var markerMinified = looksLikeCSharp
                             && lineMarkers.Any(marker => marker.SemicolonCount >= 8
                                                          || marker.BraceCount >= 10
                                                          || marker.UsingTokenCount >= 3);
        var tooFewLinesForSize = looksLikeCSharp
                                 && bytes.Length >= 1_500
                                 && raw.RawPhysicalLineCount <= 3;
        var minified = zeroLf
                       || crOnly
                       || raw.ContainsCrOnlyLineEndings
                       || oneLineMultiStatement
                       || markerMinified
                       || tooFewLinesForSize
                       || raw.RawPhysicalLinesOver500Count > 0
                       || logicalMaxLineLength > MaxAllowedPhysicalLineLength;
        var fileOver700 = logicalLines.Length > PreferredMaxLogicalLineCount;
        var fileOver1000 = logicalLines.Length > MaxAllowedLogicalLineCount;
        var passed = looksLikeCSharp
                     && !zeroLf
                     && !crOnly
                     && !raw.ContainsCrOnlyLineEndings
                     && !oneLineMultiStatement
                     && !minified
                     && !fileOver700
                     && !fileOver1000;

        return new OfflineGeoworldUnityEditorSourceFormatFileScan
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Exists = true,
            BytesRead = true,
            ByteCount = bytes.Length,
            Sha256 = OfflineGeoworldUnityEditorPreviewHash.Sha256Bytes(bytes),
            LogicalLineCount = logicalLines.Length,
            LogicalMaxLineLength = logicalMaxLineLength,
            LfByteCount = raw.LfByteCount,
            CrByteCount = raw.CrByteCount,
            RawPhysicalLineCount = raw.RawPhysicalLineCount,
            RawPhysicalMaxLineLength = raw.RawPhysicalMaxLineLength,
            RawPhysicalLinesOver500Count = raw.RawPhysicalLinesOver500Count,
            MaxSemicolonsOnPhysicalLine = lineMarkers.Count == 0 ? 0 : lineMarkers.Max(marker => marker.SemicolonCount),
            MaxBracesOnPhysicalLine = lineMarkers.Count == 0 ? 0 : lineMarkers.Max(marker => marker.BraceCount),
            MaxUsingTokensOnPhysicalLine = lineMarkers.Count == 0 ? 0 : lineMarkers.Max(marker => marker.UsingTokenCount),
            LooksLikeCSharp = looksLikeCSharp,
            ZeroLfSource = zeroLf,
            CrOnlySource = crOnly,
            ContainsCrOnlyLineEndings = raw.ContainsCrOnlyLineEndings,
            OnePhysicalLineMultiStatementSource = oneLineMultiStatement,
            MinifiedSourceCandidate = minified,
            FileOver700LogicalLines = fileOver700,
            FileOver1000LogicalLines = fileOver1000,
            Passed = passed
        };
    }

    public static bool RejectsSuspiciousRawSourceBytes(byte[] bytes)
    {
        var scan = AnalyzeSourceBytes("synthetic/Suspicious.cs", bytes);
        return scan.ZeroLfSource
               || scan.CrOnlySource
               || scan.ContainsCrOnlyLineEndings
               || scan.OnePhysicalLineMultiStatementSource
               || scan.RawPhysicalLinesOver500Count > 0
               || scan.MinifiedSourceCandidate;
    }

    public static bool RejectsOverLargeLogicalLineCounts(byte[] bytes)
    {
        var scan = AnalyzeSourceBytes("synthetic/Oversized.cs", bytes);
        return scan.FileOver700LogicalLines || scan.FileOver1000LogicalLines;
    }

    public static bool RejectsFakePassWithoutReadingBytes(bool bytesWereRead) =>
        !bytesWereRead;

    public static bool RejectsUnitySceneProjectSettingChangeMarker(string value) =>
        UnitySceneProjectMutationMarkers.Any(marker =>
            value.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<string> EnumerateGoal102RelevantSourcePaths(string root)
    {
        var paths = new SortedSet<string>(UnityScriptRelativePaths, StringComparer.Ordinal);
        foreach (var directory in Goal102ApplicationDirectories)
        {
            var fullDirectory = Resolve(root, directory);
            if (!Directory.Exists(fullDirectory))
            {
                continue;
            }

            foreach (var path in Directory.EnumerateFiles(fullDirectory, "*.cs", SearchOption.TopDirectoryOnly))
            {
                paths.Add(Relative(root, path));
            }
        }

        return paths;
    }

    private static RawSourceLineMetrics AnalyzeRawPhysicalLines(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return new RawSourceLineMetrics(0, 0, 0, 0, 0, false);
        }

        var lfCount = 0;
        var crCount = 0;
        var currentLength = 0;
        var maxLength = 0;
        var over500Count = 0;
        var containsCrOnlyLineEndings = false;

        for (var index = 0; index < bytes.Length; index++)
        {
            var value = bytes[index];
            if (value == '\n')
            {
                lfCount++;
                if (currentLength > maxLength)
                {
                    maxLength = currentLength;
                }

                if (currentLength > MaxAllowedPhysicalLineLength)
                {
                    over500Count++;
                }

                currentLength = 0;
                continue;
            }

            currentLength++;
            if (value != '\r')
            {
                continue;
            }

            crCount++;
            if (index + 1 >= bytes.Length || bytes[index + 1] != '\n')
            {
                containsCrOnlyLineEndings = true;
            }
        }

        if (currentLength > maxLength)
        {
            maxLength = currentLength;
        }

        if (currentLength > MaxAllowedPhysicalLineLength)
        {
            over500Count++;
        }

        return new RawSourceLineMetrics(
            lfCount,
            crCount,
            lfCount + 1,
            maxLength,
            over500Count,
            containsCrOnlyLineEndings);
    }

    private static PhysicalLineMarkers AnalyzePhysicalLineMarkers(string line) =>
        new(
            Count(line, ';'),
            Count(line, '{') + Count(line, '}'),
            Regex.Matches(line, @"\busing\b").Count);

    private static bool LooksLikeCSharpSource(string relativePath, string text) =>
        relativePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
        && (text.Contains("class ", StringComparison.Ordinal)
            || text.Contains("namespace ", StringComparison.Ordinal)
            || text.Contains("using ", StringComparison.Ordinal)
            || text.Contains("public ", StringComparison.Ordinal)
            || text.Contains("internal ", StringComparison.Ordinal)
            || text.Contains("private ", StringComparison.Ordinal));

    private static int Count(string text, char value) =>
        text.Count(ch => ch == value);

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(
            Path.GetFullPath(root),
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var normalizedRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root.");
        }

        return path;
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private sealed record RawSourceLineMetrics(
        int LfByteCount,
        int CrByteCount,
        int RawPhysicalLineCount,
        int RawPhysicalMaxLineLength,
        int RawPhysicalLinesOver500Count,
        bool ContainsCrOnlyLineEndings);

    private sealed record PhysicalLineMarkers(
        int SemicolonCount,
        int BraceCount,
        int UsingTokenCount);
}

public sealed record OfflineGeoworldUnityEditorSourceFormatScan
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.ScanSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int ByteScannedFileCount { get; init; }
    public bool EditorWindowScriptScanned { get; init; }
    public bool UnityPreviewRunnerScriptScanned { get; init; }
    public bool UnityPrimitiveFactoryScriptScanned { get; init; }
    public bool UnityTravelWindowScriptScanned { get; init; }
    public bool ApplicationNamespaceScanned { get; init; }
    public bool VisualWorldStreamPreviewWorkspaceScanned { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int MaxPhysicalLineLength { get; init; }
    public int RawPhysicalLinesOver500Count { get; init; }
    public int ZeroLfSourceFileCount { get; init; }
    public int CrOnlySourceFileCount { get; init; }
    public int RawPhysicalOneLineSourceFileCount { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public int FilesOver700LogicalLinesCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public IReadOnlyList<OfflineGeoworldUnityEditorSourceFormatFileScan> Files { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldUnityEditorSourceFormatFileScan
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool BytesRead { get; init; }
    public int ByteCount { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public int LogicalLineCount { get; init; }
    public int LogicalMaxLineLength { get; init; }
    public int LfByteCount { get; init; }
    public int CrByteCount { get; init; }
    public int RawPhysicalLineCount { get; init; }
    public int RawPhysicalMaxLineLength { get; init; }
    public int RawPhysicalLinesOver500Count { get; init; }
    public int MaxSemicolonsOnPhysicalLine { get; init; }
    public int MaxBracesOnPhysicalLine { get; init; }
    public int MaxUsingTokensOnPhysicalLine { get; init; }
    public bool LooksLikeCSharp { get; init; }
    public bool ZeroLfSource { get; init; }
    public bool CrOnlySource { get; init; }
    public bool ContainsCrOnlyLineEndings { get; init; }
    public bool OnePhysicalLineMultiStatementSource { get; init; }
    public bool MinifiedSourceCandidate { get; init; }
    public bool FileOver700LogicalLines { get; init; }
    public bool FileOver1000LogicalLines { get; init; }
    public bool Passed { get; init; }
}
