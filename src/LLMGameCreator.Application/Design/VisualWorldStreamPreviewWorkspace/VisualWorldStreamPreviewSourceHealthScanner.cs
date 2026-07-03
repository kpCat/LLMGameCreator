using System.Text;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public static class VisualWorldStreamPreviewSourceHealthScanner
{
    public const int MaxAllowedLogicalLineCount = 1000;
    public const int PreferredMaxLogicalLineCount = 700;
    public const int MaxAllowedPhysicalLineLength = 500;

    private const string Goal092ApplicationDirectory =
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace";
    private const string WorkspaceServiceRelativePath =
        Goal092ApplicationDirectory + "/VisualWorldStreamPreviewWorkspaceService.cs";

    public static VisualWorldStreamPreviewSourceHealthScan ScanGoal092Namespace(
        string repositoryRootPath) =>
        ScanDirectory(repositoryRootPath, Goal092ApplicationDirectory);

    public static VisualWorldStreamPreviewSourceHealthScan ScanDirectory(
        string repositoryRootPath,
        string relativeDirectory)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var directory = Resolve(root, relativeDirectory);
        var files = Directory.Exists(directory)
            ? Directory.EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly)
                .OrderBy(path => Relative(root, path), StringComparer.Ordinal)
                .Select(path => AnalyzeSourceBytes(Relative(root, path), File.ReadAllBytes(path)))
                .ToList()
            : [];

        var diagnostics = new List<VisualWorldPreviewDiagnostic>();
        foreach (var file in files.Where(file => file.FileOver1000LogicalLines))
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.source.file_over_1000_lines",
                file.RelativePath,
                "Goal092 namespace C# file exceeds 1000 logical lines."));
        }

        foreach (var file in files.Where(file => file.FileOver700LogicalLines))
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.source.file_over_700_lines",
                file.RelativePath,
                "Goal092 namespace C# file exceeds the preferred 700 logical line target."));
        }

        foreach (var file in files.Where(file => file.RawPhysicalLinesOver500Count > 0))
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.source.raw_physical_line_over_500",
                file.RelativePath,
                "Raw physical source line exceeds 500 bytes."));
        }

        foreach (var file in files.Where(file => file.ZeroLfSource || file.CrOnlySource || file.RawPhysicalOneLineSource))
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.source.raw_source_format_rejected",
                file.RelativePath,
                "Source file has zero-LF, CR-only, or raw one-physical-line shape."));
        }

        foreach (var file in files.Where(file => file.MinifiedSourceCandidate))
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.source.minified_source",
                file.RelativePath,
                "Source file has a minified or too-few-lines-for-size shape."));
        }

        var service = files.FirstOrDefault(file =>
            string.Equals(file.RelativePath, WorkspaceServiceRelativePath, StringComparison.Ordinal));
        var maxLogical = files.Count == 0 ? 0 : files.Max(file => file.LogicalLineCount);
        var maxPhysical = files.Count == 0 ? 0 : files.Max(file => file.RawPhysicalMaxLineLength);

        return new VisualWorldStreamPreviewSourceHealthScan
        {
            Passed = diagnostics.Count == 0,
            ScannedCSharpFileCount = files.Count,
            MaxLogicalLineCount = maxLogical,
            MaxPhysicalLineLength = maxPhysical,
            RawPhysicalLinesOver500Count = files.Sum(file => file.RawPhysicalLinesOver500Count),
            FilesOver1000LogicalLinesCount = files.Count(file => file.FileOver1000LogicalLines),
            FilesOver700LogicalLinesInGoal092NamespaceCount =
                files.Count(file => file.FileOver700LogicalLines),
            ZeroLfSourceCount = files.Count(file => file.ZeroLfSource),
            CrOnlySourceCount = files.Count(file => file.CrOnlySource),
            RawPhysicalOneLineSourceCount = files.Count(file => file.RawPhysicalOneLineSource),
            MinifiedSourceCount = files.Count(file => file.MinifiedSourceCandidate),
            WorkspaceServiceLogicalLineCount = service?.LogicalLineCount ?? 0,
            WorkspaceServiceMaxPhysicalLineLength = service?.RawPhysicalMaxLineLength ?? 0,
            Files = files,
            Diagnostics = diagnostics
                .OrderBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.Target, StringComparer.Ordinal)
                .ToList()
        };
    }

    public static VisualWorldStreamPreviewSourceFileHealth AnalyzeSourceBytes(
        string relativePath,
        byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes);
        var raw = AnalyzeRawPhysicalLines(bytes);
        var logicalLines = Regex.Split(text, "\r\n|\n|\r");
        var logicalLengths = logicalLines.Select(line => line.Length).ToList();
        var logicalMaxLineLength = logicalLengths.Count == 0 ? 0 : logicalLengths.Max();
        var looksLikeCSharp = LooksLikeCSharpSource(text);
        var zeroLf = looksLikeCSharp && raw.LfByteCount == 0;
        var crOnly = zeroLf && raw.CrByteCount > 0;
        var onePhysicalLine = looksLikeCSharp && raw.RawPhysicalLineCount <= 1;
        var tooFewLinesForSize = looksLikeCSharp
                                 && bytes.Length >= 1_500
                                 && (raw.RawPhysicalLineCount <= 3 || logicalLines.Length <= 3);
        var minified = logicalMaxLineLength > MaxAllowedPhysicalLineLength
                       || raw.RawPhysicalMaxLineLength > MaxAllowedPhysicalLineLength
                       || onePhysicalLine
                       || tooFewLinesForSize;

        return new VisualWorldStreamPreviewSourceFileHealth
        {
            RelativePath = relativePath.Replace('\\', '/'),
            ByteCount = bytes.Length,
            LogicalLineCount = logicalLines.Length,
            LogicalMaxLineLength = logicalMaxLineLength,
            LfByteCount = raw.LfByteCount,
            CrByteCount = raw.CrByteCount,
            RawPhysicalLineCount = raw.RawPhysicalLineCount,
            RawPhysicalMaxLineLength = raw.RawPhysicalMaxLineLength,
            RawPhysicalLinesOver500Count = raw.RawPhysicalLinesOver500Count,
            ZeroLfSource = zeroLf,
            CrOnlySource = crOnly,
            ContainsCrOnlyLineEndings = raw.ContainsCrOnlyLineEndings,
            RawPhysicalOneLineSource = onePhysicalLine,
            MinifiedSourceCandidate = minified,
            FileOver1000LogicalLines = logicalLines.Length > MaxAllowedLogicalLineCount,
            FileOver700LogicalLines = logicalLines.Length > PreferredMaxLogicalLineCount
        };
    }

    public static bool RejectsSuspiciousRawSourceBytes(byte[] bytes)
    {
        var scan = AnalyzeSourceBytes("synthetic.cs", bytes);
        return scan.ZeroLfSource
               || scan.CrOnlySource
               || scan.RawPhysicalOneLineSource
               || scan.RawPhysicalLinesOver500Count > 0
               || scan.MinifiedSourceCandidate;
    }

    public static bool RejectsOver1000LogicalLines(byte[] bytes) =>
        AnalyzeSourceBytes("synthetic.cs", bytes).FileOver1000LogicalLines;

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

    private static bool LooksLikeCSharpSource(string text) =>
        text.Contains("class ", StringComparison.Ordinal)
        || text.Contains("namespace ", StringComparison.Ordinal)
        || text.Contains("using ", StringComparison.Ordinal)
        || text.Contains("public ", StringComparison.Ordinal)
        || text.Contains("internal ", StringComparison.Ordinal)
        || text.Contains("private ", StringComparison.Ordinal);

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(
            root,
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
}
