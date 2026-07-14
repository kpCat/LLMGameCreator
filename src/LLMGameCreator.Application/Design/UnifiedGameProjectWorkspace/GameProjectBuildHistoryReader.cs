using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

/// <summary>Reads validated persisted last-success summaries without mutating project history.</summary>
public sealed class GameProjectBuildHistoryReader
{
    private const string SupportedSchemaVersion = "unified_game_project_build_history_v2";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GameProjectBuildHistoryReadResult ReadLatestMatchingSocialSuccess(
        string projectFolder,
        FeatureModuleCompositionDocument document)
        => ReadLatestMatchingSocialSuccess(projectFolder, document, null);

    public GameProjectBuildHistoryReadResult ReadLatestMatchingSocialSuccess(
        string projectFolder,
        FeatureModuleCompositionDocument document,
        FeatureModuleLibrarySnapshot? library)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFolder);
        ArgumentNullException.ThrowIfNull(document);
        var historyRoot = GameProjectFeatureModuleAuthoringService.ConfinedPath(
            projectFolder, UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot);
        if (!Directory.Exists(historyRoot)) return new GameProjectBuildHistoryReadResult();

        var diagnostics = new List<string>();
        var candidates = new List<(GameProjectBuildHistoryEntry Entry, string FileName)>();
        foreach (var path in Directory.EnumerateFiles(historyRoot, "*.json", SearchOption.TopDirectoryOnly))
        {
            GameProjectBuildHistoryEntry? historyEntry;
            try { historyEntry = JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(File.ReadAllText(path), JsonOptions); }
            catch (JsonException) { diagnostics.Add("social.history.invalid_json:" + Path.GetFileName(path)); continue; }
            catch (IOException) { diagnostics.Add("social.history.unreadable:" + Path.GetFileName(path)); continue; }

            if (historyEntry is null || !string.Equals(historyEntry.SchemaVersion, SupportedSchemaVersion, StringComparison.Ordinal))
            {
                diagnostics.Add("social.history.unsupported_schema:" + Path.GetFileName(path));
                continue;
            }
            if (IsMatchingGreenSocial(historyEntry, document)) candidates.Add((historyEntry, Path.GetFileName(path)));
        }

        var selected = candidates.OrderByDescending(candidate => candidate.Entry.CompletedAtUtc)
            .ThenByDescending(candidate => candidate.FileName, StringComparer.Ordinal).FirstOrDefault();
        if (selected.Entry is null)
        {
            if (HasPersistedSuccessIdentity(document)) diagnostics.Add("social.history.no_matching_green_social_success");
            return new GameProjectBuildHistoryReadResult { Diagnostics = diagnostics };
        }

        var entry = selected.Entry;
        var fingerprint = library is null ? new FeatureModuleAuthoringFingerprintResult()
            : new FeatureModuleAuthoringFingerprintService().Calculate(document, library);
        var status = ResolveConfigurationStatus(entry.QualifiedAuthoringFingerprint, fingerprint);
        return new GameProjectBuildHistoryReadResult
        {
            LastSuccessfulBuild = new GameProjectBuildResult
            {
                Status = entry.Status, Passed = true, Diagnostics = entry.Diagnostics,
                SelectedMechanicCount = entry.SelectedMechanicCount, ConfiguredParameterCount = entry.ConfiguredParameterCount,
                PackageSha256 = entry.PackageSha256, CompositionPackageSha256 = entry.CompositionPackageSha256,
                ActivatedProjectPackageSha256 = entry.ActivatedProjectPackageSha256, FinalStateHash = entry.FinalStateHash,
                CheckpointReloadPassed = entry.CheckpointReloadPassed, FullReplayEquivalent = entry.FullReplayEquivalent,
                ActionBindingPassed = entry.ActionBindingPassed, AttemptId = entry.AttemptId, AttemptStatus = entry.AttemptStatus,
                AttemptedSelectedModuleIds = entry.AttemptedSelectedModuleIds, AttemptedCapabilityCount = entry.AttemptedCapabilityCount,
                AttemptedPlannedActionCount = entry.AttemptedPlannedActionCount, AttemptedCheckpointActionCount = entry.AttemptedCheckpointActionCount,
                AttemptedFinalReplayActionCount = entry.AttemptedFinalReplayActionCount, Social = entry.Social,
                QualifiedAuthoringFingerprint = entry.QualifiedAuthoringFingerprint
            },
            Diagnostics = diagnostics.Concat(fingerprint.Diagnostics).ToList(),
            CurrentAuthoringFingerprint = fingerprint.Sha256,
            QualifiedAuthoringFingerprint = entry.QualifiedAuthoringFingerprint,
            SocialConfigurationStatus = status,
            MatchesCurrentConfiguration = status == "CURRENT"
        };
    }

    private static bool IsMatchingGreenSocial(GameProjectBuildHistoryEntry entry, FeatureModuleCompositionDocument document) =>
        string.Equals(entry.Status, "GREEN", StringComparison.Ordinal)
        && string.Equals(entry.AttemptStatus, "GREEN", StringComparison.Ordinal)
        && entry.Social is { Present: true, Passed: true, CheckpointReplayPassed: true, FullReplayEquivalent: true }
        && string.Equals(entry.PackageSha256, document.LastActivatedProjectPackageSha256, StringComparison.Ordinal)
        && string.Equals(entry.CompositionPackageSha256, document.LastCompositionPackageSha256, StringComparison.Ordinal)
        && string.Equals(entry.FinalStateHash, document.LastQualifiedFinalStateHash, StringComparison.Ordinal)
        && entry.CheckpointReloadPassed && entry.FullReplayEquivalent && entry.ActionBindingPassed;

    private static bool HasPersistedSuccessIdentity(FeatureModuleCompositionDocument document) =>
        !string.IsNullOrWhiteSpace(document.LastActivatedProjectPackageSha256)
        || !string.IsNullOrWhiteSpace(document.LastCompositionPackageSha256)
        || !string.IsNullOrWhiteSpace(document.LastQualifiedFinalStateHash);

    private static string ResolveConfigurationStatus(string qualified, FeatureModuleAuthoringFingerprintResult current)
    {
        if (string.IsNullOrWhiteSpace(qualified)) return "UNKNOWN";
        if (!current.Passed || string.IsNullOrWhiteSpace(current.Sha256)) return "UNKNOWN";
        return string.Equals(qualified, current.Sha256, StringComparison.Ordinal) ? "CURRENT" : "LAST_SUCCESS";
    }
}

public sealed record GameProjectBuildHistoryReadResult
{
    public GameProjectBuildResult? LastSuccessfulBuild { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public string CurrentAuthoringFingerprint { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public bool MatchesCurrentConfiguration { get; init; }
    public string SocialConfigurationStatus { get; init; } = "ABSENT";
}
