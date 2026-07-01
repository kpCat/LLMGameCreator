using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.GeneratorSpineQualityConsolidation;

public sealed partial class GeneratorSpineQualityScanner
{
    private static readonly Regex DeclarationTokenRegex = CreateDeclarationTokenRegex();
    private static readonly Regex WindowsAbsolutePathRegex = CreateWindowsAbsolutePathRegex();
    private static readonly Regex UnixAbsolutePathRegex = CreateUnixAbsolutePathRegex();
    private static readonly Regex TimestampLikeRegex = CreateTimestampLikeRegex();
    private static readonly Regex MethodStartRegex = CreateMethodStartRegex();

    public GeneratorSpineQualityScanResult ScanProject(string projectRootPath) =>
        Scan(new GeneratorSpineQualityScanOptions { ProjectRootPath = projectRootPath });

    public GeneratorSpineQualityScanResult Scan(GeneratorSpineQualityScanOptions options)
    {
        var projectRoot = NormalizeRoot(options.ProjectRootPath);
        var sourceFiles = ScanSourceFiles(projectRoot, options.SourceRoots);
        var artifactFiles = EnumerateExistingFiles(projectRoot, options.ArtifactRoots, ["*.json", "*.md", "*.txt"]).ToList();
        var productSmokeFiles = EnumerateExistingFiles(projectRoot, options.ProductSmokeRoots, ["*.cs"]).ToList();

        return new GeneratorSpineQualityScanResult
        {
            SourceFiles = sourceFiles,
            LargeMethods = ScanLargeMethods(projectRoot, sourceFiles.Select(item => item.RelativePath)),
            RepeatedSeamRoles = ScanRepeatedSeamRoles(projectRoot),
            UnityAlphaBootstrap = ScanUnityAlphaBootstrap(projectRoot),
            ProductSmokeRecords = productSmokeFiles
                .Select(path => AnalyzeProductSmokeFile(projectRoot, path))
                .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
                .ToList(),
            AbsolutePathLikeArtifacts = ScanArtifactText(projectRoot, artifactFiles, detectAbsolutePaths: true),
            TimestampLikeArtifacts = ScanArtifactText(projectRoot, artifactFiles, detectAbsolutePaths: false),
            CurrentStateConsistency = ScanCurrentStateConsistency(projectRoot),
            Goal071ProofIndicators = ScanGoal071ProofIndicators(projectRoot),
            ArtifactFileCount = artifactFiles.Count
        };
    }

    public IReadOnlyList<SourceFileQualityRecord> ScanSourceFiles(string projectRootPath, IReadOnlyList<string> relativeRoots)
    {
        var projectRoot = NormalizeRoot(projectRootPath);
        return EnumerateExistingFiles(projectRoot, relativeRoots, ["*.cs"])
            .Select(path => AnalyzeSourceFile(projectRoot, path))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    public SourceFileQualityRecord AnalyzeSourceText(string relativePath, string text)
    {
        var lines = SplitLines(text);
        var maxLineLength = lines.Count == 0 ? 0 : lines.Max(line => line.Length);
        var declarationCount = DeclarationTokenRegex.Matches(text).Count;
        var semicolonCount = text.Count(ch => ch == ';');
        var isMinified = lines.Count <= 3
            && maxLineLength > 240
            && (declarationCount >= 8 || semicolonCount >= 12);

        return new SourceFileQualityRecord
        {
            RelativePath = Normalize(relativePath),
            LineCount = lines.Count,
            MaxLineLength = maxLineLength,
            DeclarationTokenCount = declarationCount,
            SemicolonCount = semicolonCount,
            IsOneLineOrMinifiedCandidate = isMinified,
            IsLargeFileCandidate = lines.Count >= 700,
            HasExtremeLineLength = maxLineLength >= 500
        };
    }

    public ProductSmokeQualityRecord AnalyzeProductSmokeText(string relativePath, string text)
    {
        var assertCount = Regex.Matches(text, @"\bAssert\.").Count;
        var strongSignals = new[]
        {
            "RowCount",
            "StateChangingRowCount",
            "ActionCount",
            "TransitionCount",
            "Hash",
            "MissingMarkers",
            "MatchedMarkers",
            "Delta",
            "Staging",
            "CommandPlan",
            "ExitCode",
            "File.Exists",
            "Directory.EnumerateFiles"
        }
            .Where(signal => text.Contains(signal, StringComparison.Ordinal))
            .OrderBy(signal => signal, StringComparer.Ordinal)
            .ToList();

        var reportOnly = text.Contains("ImplementationStatus", StringComparison.Ordinal)
            && text.Contains("Passed", StringComparison.Ordinal)
            && strongSignals.Count < 3;
        var shallow = assertCount <= 3 || reportOnly;

        return new ProductSmokeQualityRecord
        {
            RelativePath = Normalize(relativePath),
            AssertCount = assertCount,
            StrongAssertionSignalCount = strongSignals.Count,
            ReportOnlyShallowCandidate = shallow,
            StrongSignals = strongSignals
        };
    }

    public IReadOnlyList<ArtifactVolatilityRecord> DetectAbsolutePathLikeStrings(string relativePath, string text) =>
        ScanTextForVolatileMatches(relativePath, text, detectAbsolutePaths: true);

    public IReadOnlyList<ArtifactVolatilityRecord> DetectTimestampLikeValues(string relativePath, string text) =>
        ScanTextForVolatileMatches(relativePath, text, detectAbsolutePaths: false);

    private static SourceFileQualityRecord AnalyzeSourceFile(string projectRoot, string path)
    {
        var relative = Relative(projectRoot, path);
        var text = File.ReadAllText(path, Encoding.UTF8);
        return new GeneratorSpineQualityScanner().AnalyzeSourceText(relative, text);
    }

    private static ProductSmokeQualityRecord AnalyzeProductSmokeFile(string projectRoot, string path)
    {
        var relative = Relative(projectRoot, path);
        var text = File.ReadAllText(path, Encoding.UTF8);
        return new GeneratorSpineQualityScanner().AnalyzeProductSmokeText(relative, text);
    }

    private static IReadOnlyList<MethodSizeRecord> ScanLargeMethods(string projectRoot, IEnumerable<string> relativePaths)
    {
        var results = new List<MethodSizeRecord>();
        foreach (var relativePath in relativePaths.OrderBy(path => path, StringComparer.Ordinal))
        {
            var path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                continue;
            }

            var lines = File.ReadAllLines(path, Encoding.UTF8);
            for (var index = 0; index < lines.Length; index++)
            {
                var match = MethodStartRegex.Match(lines[index]);
                if (!match.Success)
                {
                    continue;
                }

                var lineCount = CountBlockLines(lines, index);
                if (lineCount >= 120)
                {
                    results.Add(new MethodSizeRecord
                    {
                        RelativePath = Normalize(relativePath),
                        MethodName = match.Groups["name"].Value,
                        StartLine = index + 1,
                        LineCount = lineCount
                    });
                }
            }
        }

        return results
            .OrderByDescending(item => item.LineCount)
            .ThenBy(item => item.RelativePath, StringComparer.Ordinal)
            .ThenBy(item => item.StartLine)
            .ToList();
    }

    private static int CountBlockLines(IReadOnlyList<string> lines, int startIndex)
    {
        var depth = 0;
        var opened = false;
        for (var index = startIndex; index < lines.Count; index++)
        {
            foreach (var ch in StripLineStringLiterals(lines[index]))
            {
                if (ch == '{')
                {
                    depth++;
                    opened = true;
                }
                else if (ch == '}')
                {
                    depth--;
                    if (opened && depth <= 0)
                    {
                        return index - startIndex + 1;
                    }
                }
            }
        }

        return 0;
    }

    private static string StripLineStringLiterals(string line)
    {
        var builder = new StringBuilder(line.Length);
        var inString = false;
        var escaped = false;
        foreach (var ch in line)
        {
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (ch == '\\')
                {
                    escaped = true;
                }
                else if (ch == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (ch == '"')
            {
                inString = true;
                continue;
            }

            builder.Append(ch);
        }

        return builder.ToString();
    }

    private static IReadOnlyList<SeamRoleFolderRecord> ScanRepeatedSeamRoles(string projectRoot)
    {
        var designRoot = Path.Combine(projectRoot, "src", "LLMGameCreator.Application", "Design");
        if (!Directory.Exists(designRoot))
        {
            return [];
        }

        var records = new List<SeamRoleFolderRecord>();
        foreach (var directory in Directory.EnumerateDirectories(designRoot).OrderBy(path => path, StringComparer.Ordinal))
        {
            var files = Directory.EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly)
                .Select(path => Relative(projectRoot, path))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
            foreach (var role in GeneratorSpineQualityVocabulary.SeamRoleNames)
            {
                var matches = files
                    .Where(path => Path.GetFileNameWithoutExtension(path).EndsWith(role, StringComparison.Ordinal))
                    .ToList();
                if (matches.Count > 0)
                {
                    records.Add(new SeamRoleFolderRecord
                    {
                        FolderRelativePath = Relative(projectRoot, directory),
                        RoleName = role,
                        FileCount = matches.Count,
                        Files = matches
                    });
                }
            }
        }

        return records
            .OrderBy(item => item.FolderRelativePath, StringComparer.Ordinal)
            .ThenBy(item => item.RoleName, StringComparer.Ordinal)
            .ToList();
    }

    private static UnityAlphaBootstrapRiskRecord ScanUnityAlphaBootstrap(string projectRoot)
    {
        var relative = "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
        var path = Path.Combine(projectRoot, relative.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            return new UnityAlphaBootstrapRiskRecord();
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        var lines = SplitLines(text);
        var routeCandidates = new[]
        {
            "media_bound",
            "family_loop",
            "campaign",
            "matrix",
            "package",
            "review_package",
            "spatial_detail",
            "gameplay_consequence",
            "living_world",
            "interlocked_gameplay",
            "settlement",
            "narrative",
            "combat_magic",
            "world_event",
            "campaign_timeline",
            "interactive_campaign"
        }
            .Where(route => text.Contains(route, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(route => route, StringComparer.Ordinal)
            .ToList();

        return new UnityAlphaBootstrapRiskRecord
        {
            RelativePath = relative,
            LineCount = lines.Count,
            MarkerRouteCount = routeCandidates.Count,
            MarkerRoutes = routeCandidates,
            PrivateNestedTypeCount = Regex.Matches(text, @"private\s+sealed\s+class\s+").Count,
            MonolithicGrowthRisk = lines.Count >= 2500 || routeCandidates.Count >= 12
        };
    }

    private static IReadOnlyList<ArtifactVolatilityRecord> ScanArtifactText(string projectRoot, IReadOnlyList<string> artifactFiles, bool detectAbsolutePaths)
    {
        var results = new List<ArtifactVolatilityRecord>();
        foreach (var path in artifactFiles.OrderBy(path => path, StringComparer.Ordinal))
        {
            var relative = Relative(projectRoot, path);
            var text = File.ReadAllText(path, Encoding.UTF8);
            results.AddRange(ScanTextForVolatileMatches(relative, text, detectAbsolutePaths));
        }

        return results
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ThenBy(item => item.LineNumber)
            .ThenBy(item => item.MatchKind, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<ArtifactVolatilityRecord> ScanTextForVolatileMatches(string relativePath, string text, bool detectAbsolutePaths)
    {
        var records = new List<ArtifactVolatilityRecord>();
        var lines = SplitLines(text);
        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            if (detectAbsolutePaths)
            {
                if (WindowsAbsolutePathRegex.IsMatch(line))
                {
                    records.Add(VolatileRecord(relativePath, index + 1, "windows_absolute_path_like"));
                }

                if (UnixAbsolutePathRegex.IsMatch(line))
                {
                    records.Add(VolatileRecord(relativePath, index + 1, "unix_absolute_path_like"));
                }
            }
            else if (TimestampLikeRegex.IsMatch(line))
            {
                records.Add(VolatileRecord(relativePath, index + 1, "timestamp_like_value"));
            }
        }

        return records;
    }

    private static ArtifactVolatilityRecord VolatileRecord(string relativePath, int lineNumber, string kind) =>
        new()
        {
            RelativePath = Normalize(relativePath),
            LineNumber = lineNumber,
            MatchKind = kind
        };

    private static CurrentStateConsistencyRecord ScanCurrentStateConsistency(string projectRoot)
    {
        var diagnostics = new List<string>();
        var jsonParses = false;
        var gateStatusMatches = false;
        var activeManualGateMentions = false;
        var stateJsonPath = Path.Combine(projectRoot, "docs", "CURRENT_GENERATOR_STATE.json");
        if (File.Exists(stateJsonPath))
        {
            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(stateJsonPath, Encoding.UTF8));
                jsonParses = true;
                var root = document.RootElement;
                gateStatusMatches = Text(root, "gate_status") == GeneratorSpineQualityVocabulary.FinalGate;
                activeManualGateMentions = Text(root, "active_manual_gate").Contains(GeneratorSpineQualityVocabulary.FinalGate, StringComparison.Ordinal)
                    && Text(root, "active_manual_gate").Contains("accepted=false", StringComparison.Ordinal);
            }
            catch (JsonException exception)
            {
                diagnostics.Add("CURRENT_GENERATOR_STATE.json parse failure: " + exception.GetType().Name);
            }
        }
        else
        {
            diagnostics.Add("CURRENT_GENERATOR_STATE.json missing");
        }

        var stateMarkdown = ReadOptional(projectRoot, "docs/CURRENT_GENERATOR_STATE.md");
        var contextIndex = ReadOptional(projectRoot, "docs/CONTEXT_INDEX.md");
        var goalQueue = ReadOptional(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var markdownGoal071 = stateMarkdown.Contains("unity_alpha_interactive_campaign_player_verification passed before Goal 072", StringComparison.Ordinal);
        var markdownGoal072 = stateMarkdown.Contains(GeneratorSpineQualityVocabulary.FinalGate + " required", StringComparison.Ordinal)
            && stateMarkdown.Contains("accepted=false", StringComparison.Ordinal);
        var contextGoal072 = contextIndex.Contains(GeneratorSpineQualityVocabulary.FinalGate, StringComparison.Ordinal);
        var queueGoal072 = goalQueue.Contains(GeneratorSpineQualityVocabulary.FinalGate + " required", StringComparison.Ordinal);

        AddMissing(diagnostics, gateStatusMatches, "gate_status does not match Goal 072");
        AddMissing(diagnostics, activeManualGateMentions, "active_manual_gate does not mention Goal 072 required accepted=false");
        AddMissing(diagnostics, markdownGoal071, "CURRENT_GENERATOR_STATE.md does not record Goal 071 handoff before Goal 072");
        AddMissing(diagnostics, markdownGoal072, "CURRENT_GENERATOR_STATE.md does not record Goal 072 required accepted=false");
        AddMissing(diagnostics, contextGoal072, "CONTEXT_INDEX.md does not route Goal 072");
        AddMissing(diagnostics, queueGoal072, "FULL_GENERATOR_GOAL_QUEUE.md does not route Goal 072");

        return new CurrentStateConsistencyRecord
        {
            JsonParses = jsonParses,
            GateStatusMatchesGoal072 = gateStatusMatches,
            ActiveManualGateMentionsGoal072Required = activeManualGateMentions,
            MarkdownMentionsGoal071Handoff = markdownGoal071,
            MarkdownMentionsGoal072Required = markdownGoal072,
            ContextIndexMentionsGoal072Required = contextGoal072,
            GoalQueueMentionsGoal072Required = queueGoal072,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static Goal071ProofQualityRecord ScanGoal071ProofIndicators(string projectRoot)
    {
        const string root = ".llmgc/procedural/goal-071-unity-alpha-interactive-campaign-player";
        var report = Exists(projectRoot, root + "/unity-alpha-interactive-campaign-player-report.md");
        var commandPlan = ReadJson(projectRoot, root + "/interactive-campaign-command-plan.json");
        var playerProof = ReadJson(projectRoot, root + "/interactive-campaign-player-proof-summary.json");
        var transitionLedger = ReadJson(projectRoot, root + "/interactive-campaign-state-transition-ledger.json");
        var inputScript = ReadJson(projectRoot, root + "/interactive-campaign-input-script.json");
        var commandPlanExists = commandPlan.ValueKind != JsonValueKind.Undefined;
        var playerProofExists = playerProof.ValueKind != JsonValueKind.Undefined;
        var transitionLedgerExists = transitionLedger.ValueKind != JsonValueKind.Undefined;
        var inputScriptExists = inputScript.ValueKind != JsonValueKind.Undefined;
        var staged = Exists(projectRoot, root + "/staging/interactive-campaign/unity-interactive-campaign-command-plan.json");
        var rows = commandPlanExists && commandPlan.TryGetProperty("rows", out var rowArray) && rowArray.ValueKind == JsonValueKind.Array
            ? rowArray.GetArrayLength()
            : 0;
        var expectedMarkers = commandPlanExists && commandPlan.TryGetProperty("expectedPlayerMarkers", out var markerArray) && markerArray.ValueKind == JsonValueKind.Array
            ? markerArray.GetArrayLength()
            : 0;
        var matchedMarkers = playerProofExists && playerProof.TryGetProperty("matchedMarkers", out var matchedArray) && matchedArray.ValueKind == JsonValueKind.Array
            ? matchedArray.GetArrayLength()
            : 0;
        var missingMarkers = playerProofExists && playerProof.TryGetProperty("missingMarkers", out var missingArray) && missingArray.ValueKind == JsonValueKind.Array
            ? missingArray.GetArrayLength()
            : 0;

        var record = new Goal071ProofQualityRecord
        {
            ReportExists = report,
            CommandPlanExists = commandPlanExists,
            StagedCommandPlanExists = staged,
            PlayerProofExists = playerProofExists,
            TransitionLedgerExists = transitionLedgerExists,
            InputScriptExists = inputScriptExists,
            CommandPlanPassed = Bool(commandPlan, "passed"),
            CommandPlanAcceptedFalse = commandPlanExists && !Bool(commandPlan, "accepted"),
            CommandPlanRowCount = rows,
            ExpectedMarkerCount = expectedMarkers,
            PlayerProofPassed = Bool(playerProof, "passed"),
            PlayerExecuted = Bool(playerProof, "playerExecuted"),
            ProvenRowCount = Int(playerProof, "provenRowCount"),
            MissingMarkerCount = missingMarkers,
            MatchedMarkerCount = matchedMarkers,
            TransitionCount = Int(transitionLedger, "transitionCount"),
            ActionCount = Int(inputScript, "actionCount")
        };

        return record with
        {
            ProofQualityPassed = record.ReportExists
                && record.CommandPlanExists
                && record.StagedCommandPlanExists
                && record.PlayerProofExists
                && record.TransitionLedgerExists
                && record.InputScriptExists
                && record.CommandPlanPassed
                && record.CommandPlanAcceptedFalse
                && record.CommandPlanRowCount == 9
                && record.ExpectedMarkerCount > 0
                && record.PlayerProofPassed
                && record.PlayerExecuted
                && record.ProvenRowCount == 9
                && record.MissingMarkerCount == 0
                && record.MatchedMarkerCount >= record.ExpectedMarkerCount
                && record.TransitionCount >= 63
                && record.ActionCount >= 63
        };
    }

    private static IEnumerable<string> EnumerateExistingFiles(string projectRoot, IReadOnlyList<string> relativeRoots, IReadOnlyList<string> patterns)
    {
        foreach (var relativeRoot in relativeRoots)
        {
            var root = Path.Combine(projectRoot, relativeRoot.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (var pattern in patterns)
            {
                foreach (var file in Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories))
                {
                    yield return file;
                }
            }
        }
    }

    private static JsonElement ReadJson(string projectRoot, string relativePath)
    {
        var path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            return default;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static string ReadOptional(string projectRoot, string relativePath)
    {
        var path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static bool Exists(string projectRoot, string relativePath) =>
        File.Exists(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Text(JsonElement element, string propertyName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return string.Empty;
        }

        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static int Int(JsonElement element, string propertyName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return 0;
        }

        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
            ? value
            : 0;
    }

    private static bool Bool(JsonElement element, string propertyName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return false;
        }

        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.True;
    }

    private static void AddMissing(List<string> diagnostics, bool condition, string message)
    {
        if (!condition)
        {
            diagnostics.Add(message);
        }
    }

    private static IReadOnlyList<string> SplitLines(string text)
    {
        if (text.Length == 0)
        {
            return [];
        }

        return text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');
    }

    private static string NormalizeRoot(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        return Path.GetFullPath(projectRootPath);
    }

    private static string Relative(string projectRoot, string path) =>
        Normalize(Path.GetRelativePath(projectRoot, path));

    private static string Normalize(string path) =>
        path.Replace('\\', '/');

    [GeneratedRegex(@"\b(class|record|struct|enum|interface|public|private|internal|protected|void|static|sealed|partial)\b")]
    private static partial Regex CreateDeclarationTokenRegex();

    [GeneratedRegex(@"[A-Za-z]:[\\/][^""'\s,}\]]+")]
    private static partial Regex CreateWindowsAbsolutePathRegex();

    [GeneratedRegex(@"(?<![A-Za-z0-9_])/(?:home|Users|mnt|tmp|var|workspace|root)/[^""'\s,}\]]+")]
    private static partial Regex CreateUnixAbsolutePathRegex();

    [GeneratedRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}")]
    private static partial Regex CreateTimestampLikeRegex();

    [GeneratedRegex(@"\b(?:public|private|internal|protected)\s+(?:static\s+)?(?:async\s+)?(?:[\w<>\[\],?]+\s+)+(?<name>[A-Z_a-z]\w*)\s*\(")]
    private static partial Regex CreateMethodStartRegex();
}
