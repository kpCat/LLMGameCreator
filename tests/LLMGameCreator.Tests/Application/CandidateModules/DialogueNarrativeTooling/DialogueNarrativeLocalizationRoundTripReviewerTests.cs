using LLMGameCreator.Application.Design.CandidateModules.DialogueNarrativeTooling;
using Xunit;

namespace LLMGameCreator.Tests.Application.CandidateModules.DialogueNarrativeTooling;

public sealed class DialogueNarrativeLocalizationRoundTripReviewerTests
{
    [Fact]
    public void LocalizationRoundTripValidTranslatedCsvPassesWithoutPublicSchemaChanges()
    {
        var source = ValidSourceExport();
        var translated = TranslateCsv(source.StringTableCsv, "fr", id => "fr:" + id);

        var review = Review(source, translated, new DialogueNarrativeLocalizationRoundTripReviewOptions { TargetLanguage = "fr" });

        Assert.False(review.HasErrors, JoinDiagnostics(review));
        Assert.Equal(source.Summary.StringTableRowCount, review.ReadyCount);
        Assert.Equal(0, review.NeedsUpdateCount);
        Assert.Equal(0, review.MissingCount);
        Assert.False(review.RequiresPublicGamePackageSchemaChanges);
        Assert.Equal(DialogueNarrativeLocalizationRoundTripReviewer.CandidateId, review.CandidateId);
        Assert.Equal(DialogueNarrativeLocalizationRoundTripReviewer.ContractId, review.ContractId);
    }

    [Fact]
    public void LocalizationRoundTripDuplicateTranslatedIdProducesError()
    {
        var source = ValidSourceExport();
        var translated = WithExtraRow(TranslateCsv(source.StringTableCsv, "fr", id => "fr:" + id), row =>
        {
            row[0] = "fr";
            row[2] = "duplicate";
        });

        var review = Review(source, translated);

        AssertError(review, "dialogue_narrative.localization_roundtrip.duplicate_id");
    }

    [Fact]
    public void LocalizationRoundTripUnknownTranslatedIdIsErrorByDefault()
    {
        var source = ValidSourceExport();
        var translated = WithExtraRow(TranslateCsv(source.StringTableCsv, "fr", id => "fr:" + id), row =>
        {
            row[0] = "fr";
            row[1] = "line/unknown";
            row[2] = "unknown";
        });

        var review = Review(source, translated);

        AssertError(review, "dialogue_narrative.localization_roundtrip.unknown_id");
        Assert.Equal(1, review.UnknownCount);
    }

    [Fact]
    public void LocalizationRoundTripMissingExpectedIdWarnsByDefaultAndErrorsWhenConfigured()
    {
        var source = ValidSourceExport();
        var translated = RemoveRow(TranslateCsv(source.StringTableCsv, "fr", id => "fr:" + id), "line/ask_signal");

        var relaxed = Review(source, translated);
        var strict = Review(
            source,
            translated,
            new DialogueNarrativeLocalizationRoundTripReviewOptions { TreatMissingTranslationsAsErrors = true });

        AssertWarning(relaxed, "dialogue_narrative.localization_roundtrip.missing_id");
        Assert.False(relaxed.HasErrors, JoinDiagnostics(relaxed));
        Assert.Equal(1, relaxed.MissingCount);
        AssertError(strict, "dialogue_narrative.localization_roundtrip.missing_id");
    }

    [Fact]
    public void LocalizationRoundTripProtectedColumnChangeProducesErrorByDefault()
    {
        var source = ValidSourceExport();
        var translated = ChangeRow(TranslateCsv(source.StringTableCsv, "fr", id => "fr:" + id), "line/ask_signal", row => row[3] = "other/file");

        var review = Review(source, translated);

        AssertError(review, "dialogue_narrative.localization_roundtrip.protected_column_changed");
    }

    [Fact]
    public void LocalizationRoundTripLockMismatchWarnsByDefaultAndErrorsWhenConfigured()
    {
        var source = ValidSourceExport();
        var translated = ChangeRow(TranslateCsv(source.StringTableCsv, "fr", id => "fr:" + id), "line/ask_signal", row => row[6] = "stale-lock");

        var relaxed = Review(source, translated);
        var strict = Review(
            source,
            translated,
            new DialogueNarrativeLocalizationRoundTripReviewOptions { TreatLockMismatchAsErrors = true });

        AssertWarning(relaxed, "dialogue_narrative.localization_roundtrip.lock_mismatch");
        Assert.Equal(1, relaxed.NeedsUpdateCount);
        Assert.False(relaxed.HasErrors, JoinDiagnostics(relaxed));
        AssertError(strict, "dialogue_narrative.localization_roundtrip.lock_mismatch");
    }

    [Fact]
    public void LocalizationRoundTripEmptyTranslatedTextProducesMissingTranslationWarning()
    {
        var source = ValidSourceExport();
        var translated = ChangeRow(TranslateCsv(source.StringTableCsv, "fr", id => "fr:" + id), "line/ask_signal", row => row[2] = string.Empty);

        var review = Review(source, translated);

        AssertWarning(review, "dialogue_narrative.localization_roundtrip.empty_text");
        Assert.Equal(1, review.MissingCount);
    }

    [Fact]
    public void LocalizationRoundTripCsvParserHandlesCommaQuoteAndNewlineInTranslatedText()
    {
        var source = new DialogueNarrativeAuthoringProjectionTextExport
        {
            StringTableCsv =
                "language,id,text,file,node,lineNumber,lock,comment\r\n" +
                "en,line/start,\"Hello, \"\"guide\"\"\nKeep moving.\",dialogue/frontier_intro.json,node/start,1,lock/start,\r\n",
            Summary = new DialogueNarrativeAuthoringProjectionTextExportSummary { StringTableRowCount = 1 }
        };
        var translated =
            "language,id,text,file,node,lineNumber,lock,comment\r\n" +
            "fr,line/start,\"Bonjour, \"\"guide\"\"\nContinue.\",dialogue/frontier_intro.json,node/start,1,lock/start,\r\n";

        var review = Review(source, translated, new DialogueNarrativeLocalizationRoundTripReviewOptions { TargetLanguage = "fr" });

        Assert.False(review.HasErrors, JoinDiagnostics(review));
        Assert.Contains(review.Rows, row =>
            row.LineId == "line/start" &&
            row.TranslatedText == "Bonjour, \"guide\"\nContinue." &&
            row.Status == DialogueNarrativeLocalizationRoundTripStatus.Ready);
    }

    [Fact]
    public void LocalizationRoundTripMalformedCsvAndHeaderMismatchReturnDiagnostics()
    {
        var source = ValidSourceExport();

        var malformed = Review(source, "language,id,text\r\nfr,line/start,\"unterminated");

        Assert.True(malformed.HasErrors);
        Assert.Contains(malformed.Diagnostics, diagnostic =>
            diagnostic.Code == "dialogue_narrative.localization_roundtrip.csv.malformed");
        Assert.Contains(malformed.Diagnostics, diagnostic =>
            diagnostic.Code == "dialogue_narrative.localization_roundtrip.csv.missing_header");
    }

    [Fact]
    public void LocalizationRoundTripReviewOrderingIsDeterministicAcrossEquivalentInputs()
    {
        var source = ValidSourceExport();
        var translated = WithExtraRow(TranslateCsv(source.StringTableCsv, "fr", id => "fr:" + id), row =>
        {
            row[0] = "fr";
            row[1] = "line/zzz_unknown";
            row[2] = "unknown";
        });

        var first = Review(source, translated);
        var second = Review(source, translated);

        Assert.Equal(first.Rows.Select(row => $"{row.Status}:{row.LineId}"), second.Rows.Select(row => $"{row.Status}:{row.LineId}"));
        Assert.Equal(first.Diagnostics.Select(diagnostic => $"{diagnostic.Severity}:{diagnostic.Code}:{diagnostic.TargetId}"), second.Diagnostics.Select(diagnostic => $"{diagnostic.Severity}:{diagnostic.Code}:{diagnostic.TargetId}"));
    }

    private static DialogueNarrativeLocalizationRoundTripReview Review(
        DialogueNarrativeAuthoringProjectionTextExport source,
        string translated,
        DialogueNarrativeLocalizationRoundTripReviewOptions? options = null) =>
        new DialogueNarrativeLocalizationRoundTripReviewer().Review(source, translated, options);

    private static DialogueNarrativeAuthoringProjectionTextExport ValidSourceExport() =>
        new()
        {
            StringTableCsv =
                "language,id,text,file,node,lineNumber,lock,comment\r\n" +
                "en,line/start,The trail is quiet tonight.,dialogue/frontier_intro.json,node/start,1,lock/start,\r\n" +
                "en,line/ask_signal,Ask about the signal fire.,dialogue/frontier_intro.json,node/start,2,lock/ask_signal,\r\n" +
                "en,line/signal,Then we keep moving.,dialogue/frontier_intro.json,node/signal,1,lock/signal,\r\n" +
                "en,line/finish,Close.,dialogue/frontier_intro.json,node/signal,2,lock/finish,\r\n",
            Summary = new DialogueNarrativeAuthoringProjectionTextExportSummary { StringTableRowCount = 4 }
        };

    private static string TranslateCsv(string csv, string language, Func<string, string> textFactory) =>
        UpdateRows(csv, row =>
        {
            row[0] = language;
            row[2] = textFactory(row[1]);
        });

    private static string ChangeRow(string csv, string id, Action<string[]> change) =>
        UpdateRows(csv, row =>
        {
            if (row[1] == id)
            {
                change(row);
            }
        });

    private static string RemoveRow(string csv, string id)
    {
        var lines = csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
        lines.RemoveAll(line => line.Contains("," + id + ",", StringComparison.Ordinal));
        return string.Join("\r\n", lines) + "\r\n";
    }

    private static string WithExtraRow(string csv, Action<string[]> change)
    {
        var lines = csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
        var row = lines[1].Split(',');
        change(row);
        lines.Add(string.Join(",", row.Select(Escape)));
        return string.Join("\r\n", lines) + "\r\n";
    }

    private static string UpdateRows(string csv, Action<string[]> change)
    {
        var lines = csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
        for (var index = 1; index < lines.Count; index++)
        {
            var row = lines[index].Split(',');
            change(row);
            lines[index] = string.Join(",", row.Select(Escape));
        }

        return string.Join("\r\n", lines) + "\r\n";
    }

    private static string Escape(string value) =>
        value.Contains(',', StringComparison.Ordinal) ||
        value.Contains('"', StringComparison.Ordinal) ||
        value.Contains('\r', StringComparison.Ordinal) ||
        value.Contains('\n', StringComparison.Ordinal)
            ? "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\""
            : value;

    private static void AssertWarning(DialogueNarrativeLocalizationRoundTripReview review, string code)
    {
        Assert.Contains(review.Diagnostics, diagnostic =>
            diagnostic.Code == code &&
            diagnostic.Severity == DialogueNarrativeLocalizationRoundTripSeverity.Warning);
    }

    private static void AssertError(DialogueNarrativeLocalizationRoundTripReview review, string code)
    {
        Assert.Contains(review.Diagnostics, diagnostic =>
            diagnostic.Code == code &&
            diagnostic.Severity == DialogueNarrativeLocalizationRoundTripSeverity.Error);
    }

    private static string JoinDiagnostics(DialogueNarrativeLocalizationRoundTripReview review) =>
        string.Join(Environment.NewLine, review.Diagnostics.Select(diagnostic => $"{diagnostic.Severity}:{diagnostic.Code}:{diagnostic.TargetId}:{diagnostic.Message}"));
}
