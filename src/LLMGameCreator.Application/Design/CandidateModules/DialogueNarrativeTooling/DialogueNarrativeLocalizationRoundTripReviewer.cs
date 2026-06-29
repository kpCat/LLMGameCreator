namespace LLMGameCreator.Application.Design.CandidateModules.DialogueNarrativeTooling;

public sealed class DialogueNarrativeLocalizationRoundTripReviewer
{
    public const string CandidateId = "candidate_dialogue_narrative_tooling_v1";
    public const string ContractId = "dialogue_narrative_ir_contract_v1";

    private static readonly StringComparer IdComparer = StringComparer.Ordinal;
    private static readonly IReadOnlyList<string> RequiredHeaders =
    [
        "language",
        "id",
        "text",
        "file",
        "node",
        "lineNumber",
        "lock",
        "comment"
    ];

    public DialogueNarrativeLocalizationRoundTripReview Review(
        DialogueNarrativeAuthoringProjectionTextExport sourceExport,
        string translatedStringTableCsv,
        DialogueNarrativeLocalizationRoundTripReviewOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(sourceExport);

        var settings = options ?? new DialogueNarrativeLocalizationRoundTripReviewOptions();
        var diagnostics = new List<DialogueNarrativeLocalizationRoundTripDiagnostic>();
        var sourceParse = CsvStringTableParser.Parse(sourceExport.StringTableCsv, "source");
        var translatedParse = CsvStringTableParser.Parse(translatedStringTableCsv ?? string.Empty, "translated");
        diagnostics.AddRange(sourceParse.Diagnostics);
        diagnostics.AddRange(translatedParse.Diagnostics);

        if (sourceParse.HasErrors || translatedParse.HasErrors)
        {
            var parseReview = BuildReview([], diagnostics);
            return parseReview with
            {
                RequiresPublicGamePackageSchemaChanges = false
            };
        }

        var expectedRows = sourceParse.Rows
            .Select((row, index) => new RowWithOrder(row, index))
            .ToList();
        var translatedRows = translatedParse.Rows
            .Select((row, index) => new RowWithOrder(row, index))
            .ToList();
        var translatedById = translatedRows
            .Where(item => !string.IsNullOrWhiteSpace(item.Row.Id))
            .GroupBy(item => item.Row.Id, IdComparer)
            .ToDictionary(group => group.Key, group => group.ToList(), IdComparer);
        var expectedIds = expectedRows
            .Select(item => item.Row.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(IdComparer);

        foreach (var duplicateGroup in translatedById.Where(pair => pair.Value.Count > 1).OrderBy(pair => pair.Key, IdComparer))
        {
            diagnostics.Add(Diagnostic(
                DialogueNarrativeLocalizationRoundTripSeverity.Error,
                "dialogue_narrative.localization_roundtrip.duplicate_id",
                duplicateGroup.Key,
                "Translated CSV contains duplicate line ids."));
        }

        var rows = new List<DialogueNarrativeLocalizationRoundTripRow>();
        foreach (var expected in expectedRows)
        {
            var rowDiagnostics = new List<DialogueNarrativeLocalizationRoundTripDiagnostic>();
            translatedById.TryGetValue(expected.Row.Id, out var translatedMatches);
            var translated = translatedMatches is { Count: > 0 } ? translatedMatches[0].Row : null;
            var row = ReviewExpectedRow(expected.Row, translated, settings, rowDiagnostics);
            diagnostics.AddRange(rowDiagnostics);
            rows.Add(row);
        }

        var unknownRows = translatedRows
            .Where(item => !expectedIds.Contains(item.Row.Id))
            .OrderBy(item => item.Row.Id, IdComparer)
            .ThenBy(item => item.Order)
            .ToList();
        foreach (var unknown in unknownRows)
        {
            var severity = settings.TreatUnknownLineIdsAsErrors
                ? DialogueNarrativeLocalizationRoundTripSeverity.Error
                : DialogueNarrativeLocalizationRoundTripSeverity.Warning;
            var diagnostic = Diagnostic(
                severity,
                "dialogue_narrative.localization_roundtrip.unknown_id",
                unknown.Row.Id,
                "Translated CSV contains a line id that is not present in the current source export.");
            diagnostics.Add(diagnostic);
            rows.Add(new DialogueNarrativeLocalizationRoundTripRow
            {
                LineId = unknown.Row.Id,
                TranslatedLanguage = unknown.Row.Language,
                TranslatedText = unknown.Row.Text,
                File = unknown.Row.File,
                Node = unknown.Row.Node,
                LineNumber = unknown.Row.LineNumber,
                Lock = unknown.Row.Lock,
                Comment = unknown.Row.Comment,
                Status = DialogueNarrativeLocalizationRoundTripStatus.Unknown,
                Diagnostics = [diagnostic]
            });
        }

        return BuildReview(rows, diagnostics);
    }

    private static DialogueNarrativeLocalizationRoundTripRow ReviewExpectedRow(
        CsvStringTableRow source,
        CsvStringTableRow? translated,
        DialogueNarrativeLocalizationRoundTripReviewOptions options,
        ICollection<DialogueNarrativeLocalizationRoundTripDiagnostic> diagnostics)
    {
        if (translated == null)
        {
            var severity = options.TreatMissingTranslationsAsErrors
                ? DialogueNarrativeLocalizationRoundTripSeverity.Error
                : DialogueNarrativeLocalizationRoundTripSeverity.Warning;
            diagnostics.Add(Diagnostic(
                severity,
                "dialogue_narrative.localization_roundtrip.missing_id",
                source.Id,
                "Expected source line id is missing from translated CSV."));
            return BuildRow(source, null, DialogueNarrativeLocalizationRoundTripStatus.Missing, diagnostics);
        }

        if (!string.IsNullOrWhiteSpace(options.TargetLanguage) &&
            !IdComparer.Equals(translated.Language, options.TargetLanguage))
        {
            diagnostics.Add(Diagnostic(
                DialogueNarrativeLocalizationRoundTripSeverity.Error,
                "dialogue_narrative.localization_roundtrip.language_mismatch",
                source.Id,
                "Translated CSV row language does not match the requested target language."));
        }

        if (string.IsNullOrWhiteSpace(translated.Text))
        {
            diagnostics.Add(Diagnostic(
                options.TreatMissingTranslationsAsErrors
                    ? DialogueNarrativeLocalizationRoundTripSeverity.Error
                    : DialogueNarrativeLocalizationRoundTripSeverity.Warning,
                "dialogue_narrative.localization_roundtrip.empty_text",
                source.Id,
                "Translated CSV row text is empty."));
        }

        AddProtectedColumnDiagnostic(source.Id, "file", source.File, translated.File, options, diagnostics);
        AddProtectedColumnDiagnostic(source.Id, "node", source.Node, translated.Node, options, diagnostics);
        AddProtectedColumnDiagnostic(source.Id, "lineNumber", source.LineNumber, translated.LineNumber, options, diagnostics);
        AddProtectedColumnDiagnostic(source.Id, "comment", source.Comment, translated.Comment, options, diagnostics);

        if (!IdComparer.Equals(source.Lock, translated.Lock))
        {
            diagnostics.Add(Diagnostic(
                options.TreatLockMismatchAsErrors
                    ? DialogueNarrativeLocalizationRoundTripSeverity.Error
                    : DialogueNarrativeLocalizationRoundTripSeverity.Warning,
                "dialogue_narrative.localization_roundtrip.lock_mismatch",
                source.Id,
                "Translated CSV lock does not match the current source export; source text may have changed."));
        }

        var status = ResolveStatus(diagnostics);
        return BuildRow(source, translated, status, diagnostics);
    }

    private static void AddProtectedColumnDiagnostic(
        string lineId,
        string column,
        string expected,
        string actual,
        DialogueNarrativeLocalizationRoundTripReviewOptions options,
        ICollection<DialogueNarrativeLocalizationRoundTripDiagnostic> diagnostics)
    {
        if (IdComparer.Equals(expected, actual))
        {
            return;
        }

        diagnostics.Add(Diagnostic(
            options.TreatProtectedColumnChangesAsErrors
                ? DialogueNarrativeLocalizationRoundTripSeverity.Error
                : DialogueNarrativeLocalizationRoundTripSeverity.Warning,
            "dialogue_narrative.localization_roundtrip.protected_column_changed",
            lineId,
            $"Translated CSV changed protected column '{column}'."));
    }

    private static DialogueNarrativeLocalizationRoundTripStatus ResolveStatus(
        IEnumerable<DialogueNarrativeLocalizationRoundTripDiagnostic> diagnostics)
    {
        if (diagnostics.Any(diagnostic => diagnostic.Severity == DialogueNarrativeLocalizationRoundTripSeverity.Error))
        {
            return DialogueNarrativeLocalizationRoundTripStatus.Error;
        }

        if (diagnostics.Any(diagnostic => diagnostic.Code == "dialogue_narrative.localization_roundtrip.empty_text" ||
                                          diagnostic.Code == "dialogue_narrative.localization_roundtrip.missing_id"))
        {
            return DialogueNarrativeLocalizationRoundTripStatus.Missing;
        }

        if (diagnostics.Any(diagnostic => diagnostic.Code == "dialogue_narrative.localization_roundtrip.lock_mismatch"))
        {
            return DialogueNarrativeLocalizationRoundTripStatus.NeedsUpdate;
        }

        return DialogueNarrativeLocalizationRoundTripStatus.Ready;
    }

    private static DialogueNarrativeLocalizationRoundTripRow BuildRow(
        CsvStringTableRow source,
        CsvStringTableRow? translated,
        DialogueNarrativeLocalizationRoundTripStatus status,
        IEnumerable<DialogueNarrativeLocalizationRoundTripDiagnostic> diagnostics) =>
        new()
        {
            LineId = source.Id,
            SourceLanguage = source.Language,
            TranslatedLanguage = translated?.Language ?? string.Empty,
            SourceText = source.Text,
            TranslatedText = translated?.Text ?? string.Empty,
            File = source.File,
            Node = source.Node,
            LineNumber = source.LineNumber,
            Lock = source.Lock,
            Comment = source.Comment,
            Status = status,
            Diagnostics = diagnostics.OrderBy(diagnostic => diagnostic.Code, IdComparer).ToList()
        };

    private static DialogueNarrativeLocalizationRoundTripReview BuildReview(
        IReadOnlyList<DialogueNarrativeLocalizationRoundTripRow> rows,
        IReadOnlyList<DialogueNarrativeLocalizationRoundTripDiagnostic> diagnostics)
    {
        var sortedDiagnostics = diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.TargetId, item.Message))
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, IdComparer)
            .ThenBy(item => item.TargetId, IdComparer)
            .ThenBy(item => item.Message, IdComparer)
            .ToList();

        var summary = new DialogueNarrativeLocalizationRoundTripReviewSummary
        {
            RowCount = rows.Count,
            ReadyCount = rows.Count(row => row.Status == DialogueNarrativeLocalizationRoundTripStatus.Ready),
            NeedsUpdateCount = rows.Count(row => row.Status == DialogueNarrativeLocalizationRoundTripStatus.NeedsUpdate),
            MissingCount = rows.Count(row => row.Status == DialogueNarrativeLocalizationRoundTripStatus.Missing),
            UnknownCount = rows.Count(row => row.Status == DialogueNarrativeLocalizationRoundTripStatus.Unknown),
            ErrorCount = sortedDiagnostics.Count(diagnostic => diagnostic.Severity == DialogueNarrativeLocalizationRoundTripSeverity.Error),
            WarningCount = sortedDiagnostics.Count(diagnostic => diagnostic.Severity == DialogueNarrativeLocalizationRoundTripSeverity.Warning)
        };

        return new DialogueNarrativeLocalizationRoundTripReview
        {
            CandidateId = CandidateId,
            ContractId = ContractId,
            RequiresPublicGamePackageSchemaChanges = false,
            Rows = rows,
            Diagnostics = sortedDiagnostics,
            Summary = summary
        };
    }

    private static DialogueNarrativeLocalizationRoundTripDiagnostic Diagnostic(
        DialogueNarrativeLocalizationRoundTripSeverity severity,
        string code,
        string targetId,
        string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            TargetId = targetId,
            Message = message
        };

    private static int SeverityOrder(DialogueNarrativeLocalizationRoundTripSeverity severity) =>
        severity switch
        {
            DialogueNarrativeLocalizationRoundTripSeverity.Error => 0,
            DialogueNarrativeLocalizationRoundTripSeverity.Warning => 1,
            DialogueNarrativeLocalizationRoundTripSeverity.Info => 2,
            _ => 3
        };

    private sealed record RowWithOrder(CsvStringTableRow Row, int Order);

    private sealed class CsvStringTableParser
    {
        public static CsvStringTableParseResult Parse(string csv, string target)
        {
            var diagnostics = new List<DialogueNarrativeLocalizationRoundTripDiagnostic>();
            var table = ParseCells(csv, target, diagnostics);
            if (table.Count == 0)
            {
                diagnostics.Add(Diagnostic(
                    DialogueNarrativeLocalizationRoundTripSeverity.Error,
                    "dialogue_narrative.localization_roundtrip.csv.empty",
                    target,
                    "CSV text does not contain a header row."));
                return new CsvStringTableParseResult { Diagnostics = diagnostics };
            }

            var header = table[0];
            ValidateHeaders(header, target, diagnostics);
            var headerIndex = header
                .Select((value, index) => new { value, index })
                .GroupBy(item => item.value, IdComparer)
                .ToDictionary(group => group.Key, group => group.First().index, IdComparer);
            var rows = new List<CsvStringTableRow>();
            for (var index = 1; index < table.Count; index++)
            {
                var cells = table[index];
                if (cells.Count == 1 && string.IsNullOrEmpty(cells[0]))
                {
                    continue;
                }

                if (cells.Count != header.Count)
                {
                    diagnostics.Add(Diagnostic(
                        DialogueNarrativeLocalizationRoundTripSeverity.Error,
                        "dialogue_narrative.localization_roundtrip.csv.column_count",
                        $"{target}:{index + 1}",
                        "CSV row column count does not match the header."));
                    continue;
                }

                if (RequiredHeaders.Any(headerName => !headerIndex.ContainsKey(headerName)))
                {
                    continue;
                }

                rows.Add(new CsvStringTableRow
                {
                    Language = cells[headerIndex["language"]],
                    Id = cells[headerIndex["id"]],
                    Text = cells[headerIndex["text"]],
                    File = cells[headerIndex["file"]],
                    Node = cells[headerIndex["node"]],
                    LineNumber = cells[headerIndex["lineNumber"]],
                    Lock = cells[headerIndex["lock"]],
                    Comment = cells[headerIndex["comment"]]
                });
            }

            return new CsvStringTableParseResult
            {
                Rows = rows,
                Diagnostics = diagnostics
            };
        }

        private static IReadOnlyList<IReadOnlyList<string>> ParseCells(
            string csv,
            string target,
            ICollection<DialogueNarrativeLocalizationRoundTripDiagnostic> diagnostics)
        {
            var rows = new List<IReadOnlyList<string>>();
            var row = new List<string>();
            var cell = new System.Text.StringBuilder();
            var inQuotes = false;

            for (var index = 0; index < csv.Length; index++)
            {
                var current = csv[index];
                if (inQuotes)
                {
                    if (current == '"')
                    {
                        if (index + 1 < csv.Length && csv[index + 1] == '"')
                        {
                            cell.Append('"');
                            index++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        cell.Append(current);
                    }

                    continue;
                }

                switch (current)
                {
                    case '"':
                        if (cell.Length == 0)
                        {
                            inQuotes = true;
                        }
                        else
                        {
                            diagnostics.Add(Diagnostic(
                                DialogueNarrativeLocalizationRoundTripSeverity.Error,
                                "dialogue_narrative.localization_roundtrip.csv.malformed",
                                target,
                                "CSV quote appeared inside an unquoted field."));
                        }

                        break;
                    case ',':
                        row.Add(cell.ToString());
                        cell.Clear();
                        break;
                    case '\r':
                        row.Add(cell.ToString());
                        cell.Clear();
                        rows.Add(row);
                        row = [];
                        if (index + 1 < csv.Length && csv[index + 1] == '\n')
                        {
                            index++;
                        }

                        break;
                    case '\n':
                        row.Add(cell.ToString());
                        cell.Clear();
                        rows.Add(row);
                        row = [];
                        break;
                    default:
                        cell.Append(current);
                        break;
                }
            }

            if (inQuotes)
            {
                diagnostics.Add(Diagnostic(
                    DialogueNarrativeLocalizationRoundTripSeverity.Error,
                    "dialogue_narrative.localization_roundtrip.csv.malformed",
                    target,
                    "CSV ended inside a quoted field."));
            }

            if (cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                rows.Add(row);
            }

            return rows;
        }

        private static void ValidateHeaders(
            IReadOnlyList<string> header,
            string target,
            ICollection<DialogueNarrativeLocalizationRoundTripDiagnostic> diagnostics)
        {
            var duplicates = header
                .GroupBy(value => value, IdComparer)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .OrderBy(value => value, IdComparer)
                .ToList();
            foreach (var duplicate in duplicates)
            {
                diagnostics.Add(Diagnostic(
                    DialogueNarrativeLocalizationRoundTripSeverity.Error,
                    "dialogue_narrative.localization_roundtrip.csv.duplicate_header",
                    $"{target}:{duplicate}",
                    "CSV header contains a duplicate column."));
            }

            foreach (var required in RequiredHeaders.Where(required => !header.Contains(required, IdComparer)))
            {
                diagnostics.Add(Diagnostic(
                    DialogueNarrativeLocalizationRoundTripSeverity.Error,
                    "dialogue_narrative.localization_roundtrip.csv.missing_header",
                    $"{target}:{required}",
                    "CSV header is missing a required column."));
            }
        }
    }

    private sealed record CsvStringTableRow
    {
        public string Language { get; init; } = string.Empty;
        public string Id { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
        public string File { get; init; } = string.Empty;
        public string Node { get; init; } = string.Empty;
        public string LineNumber { get; init; } = string.Empty;
        public string Lock { get; init; } = string.Empty;
        public string Comment { get; init; } = string.Empty;
    }

    private sealed record CsvStringTableParseResult
    {
        public IReadOnlyList<CsvStringTableRow> Rows { get; init; } = [];
        public IReadOnlyList<DialogueNarrativeLocalizationRoundTripDiagnostic> Diagnostics { get; init; } = [];
        public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DialogueNarrativeLocalizationRoundTripSeverity.Error);
    }
}

public sealed record DialogueNarrativeLocalizationRoundTripReviewOptions
{
    public string TargetLanguage { get; init; } = string.Empty;
    public bool TreatMissingTranslationsAsErrors { get; init; }
    public bool TreatUnknownLineIdsAsErrors { get; init; } = true;
    public bool TreatProtectedColumnChangesAsErrors { get; init; } = true;
    public bool TreatLockMismatchAsErrors { get; init; }
}

public sealed record DialogueNarrativeAuthoringProjectionTextExport
{
    public string StringTableCsv { get; init; } = string.Empty;
    public DialogueNarrativeAuthoringProjectionTextExportSummary Summary { get; init; } = new();
}

public sealed record DialogueNarrativeAuthoringProjectionTextExportSummary
{
    public int StringTableRowCount { get; init; }
}

public sealed record DialogueNarrativeLocalizationRoundTripReview
{
    public string CandidateId { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
    public bool RequiresPublicGamePackageSchemaChanges { get; init; }
    public DialogueNarrativeLocalizationRoundTripReviewSummary Summary { get; init; } = new();
    public IReadOnlyList<DialogueNarrativeLocalizationRoundTripRow> Rows { get; init; } = [];
    public IReadOnlyList<DialogueNarrativeLocalizationRoundTripDiagnostic> Diagnostics { get; init; } = [];
    public bool HasErrors => Summary.ErrorCount > 0;
    public int WarningCount => Summary.WarningCount;
    public int ReadyCount => Summary.ReadyCount;
    public int NeedsUpdateCount => Summary.NeedsUpdateCount;
    public int MissingCount => Summary.MissingCount;
    public int UnknownCount => Summary.UnknownCount;
}

public sealed record DialogueNarrativeLocalizationRoundTripReviewSummary
{
    public int RowCount { get; init; }
    public int ReadyCount { get; init; }
    public int NeedsUpdateCount { get; init; }
    public int MissingCount { get; init; }
    public int UnknownCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
}

public sealed record DialogueNarrativeLocalizationRoundTripRow
{
    public string LineId { get; init; } = string.Empty;
    public string SourceLanguage { get; init; } = string.Empty;
    public string TranslatedLanguage { get; init; } = string.Empty;
    public string SourceText { get; init; } = string.Empty;
    public string TranslatedText { get; init; } = string.Empty;
    public string File { get; init; } = string.Empty;
    public string Node { get; init; } = string.Empty;
    public string LineNumber { get; init; } = string.Empty;
    public string Lock { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public DialogueNarrativeLocalizationRoundTripStatus Status { get; init; } = DialogueNarrativeLocalizationRoundTripStatus.Ready;
    public IReadOnlyList<DialogueNarrativeLocalizationRoundTripDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record DialogueNarrativeLocalizationRoundTripDiagnostic
{
    public DialogueNarrativeLocalizationRoundTripSeverity Severity { get; init; } = DialogueNarrativeLocalizationRoundTripSeverity.Info;
    public string Code { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public enum DialogueNarrativeLocalizationRoundTripSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2
}

public enum DialogueNarrativeLocalizationRoundTripStatus
{
    Ready = 0,
    Missing = 1,
    NeedsUpdate = 2,
    Unknown = 3,
    Error = 4
}
