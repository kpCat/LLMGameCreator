using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed record GameProjectSeedRegenerationCandidateSealResult
{
    public bool Passed { get; init; }
    public GameProjectSeedRegenerationCandidateSeal? Seal { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class GameProjectSeedRegenerationCandidateSealService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public GameProjectSeedRegenerationCandidateSeal Create(
        string candidateRoot,
        string candidateRootIdentity,
        string attemptId,
        string selectedHistoryFileName,
        GameProjectBuildResult build,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        GameProjectSeedRegenerationDiff diff,
        GameProjectAuthoringState authoring)
    {
        var seal = Calculate(candidateRoot, candidateRootIdentity, attemptId, selectedHistoryFileName,
            build, snapshot, diff, authoring);
        seal = seal with { SealSha256 = HashText(Serialize(seal with { SealSha256 = string.Empty })) };
        WriteAtomic(Confined(candidateRoot, GameProjectSeedRegenerationVocabulary.CandidateSealRelativePath),
            Serialize(seal));
        return seal;
    }

    public GameProjectSeedRegenerationCandidateSealResult Verify(
        string candidateRoot,
        GameProjectSeedRegenerationCandidateSeal expected,
        GameProjectBuildResult build,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        GameProjectSeedRegenerationDiff diff,
        GameProjectAuthoringState authoring)
    {
        ArgumentNullException.ThrowIfNull(expected);
        var diagnostics = new List<string>();
        try
        {
            var path = Confined(candidateRoot, GameProjectSeedRegenerationVocabulary.CandidateSealRelativePath);
            if (!File.Exists(path)) return Failed("regeneration.candidate_seal_mismatch");
            var persisted = JsonSerializer.Deserialize<GameProjectSeedRegenerationCandidateSeal>(
                File.ReadAllText(path, Encoding.UTF8), JsonOptions);
            if (persisted is null
                || persisted.SchemaVersion != GameProjectSeedRegenerationVocabulary.CandidateSealSchemaVersion
                || !string.Equals(persisted.SealSha256, expected.SealSha256, StringComparison.Ordinal)
                || !string.Equals(HashText(Serialize(persisted with { SealSha256 = string.Empty })),
                    persisted.SealSha256, StringComparison.Ordinal))
                return Failed("regeneration.candidate_seal_mismatch");

            var actual = Calculate(candidateRoot, persisted.CandidateRootIdentity, persisted.AttemptId,
                persisted.SelectedBuildHistoryFileName, build, snapshot, diff, authoring);
            Compare(persisted.SourceRecordSha256, actual.SourceRecordSha256,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.GenerationTreeSha256, actual.GenerationTreeSha256,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.PackageSha256, actual.PackageSha256,
                "regeneration.candidate_package_changed", diagnostics);
            Compare(persisted.CandidatePackageSha256, actual.CandidatePackageSha256,
                "regeneration.candidate_package_changed", diagnostics);
            Compare(persisted.AuthoringTreeSha256, actual.AuthoringTreeSha256,
                "regeneration.candidate_authoring_changed", diagnostics);
            Compare(persisted.QualifiedAuthoringFingerprint, actual.QualifiedAuthoringFingerprint,
                "regeneration.candidate_authoring_changed", diagnostics);
            Compare(persisted.SelectedModuleIdsSha256, actual.SelectedModuleIdsSha256,
                "regeneration.candidate_authoring_changed", diagnostics);
            Compare(persisted.ParameterValuesSha256, actual.ParameterValuesSha256,
                "regeneration.candidate_authoring_changed", diagnostics);
            Compare(persisted.IdentitySha256, actual.IdentitySha256,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.SelectedBuildHistoryFileName, actual.SelectedBuildHistoryFileName,
                "regeneration.candidate_history_changed", diagnostics);
            Compare(persisted.SelectedBuildHistorySha256, actual.SelectedBuildHistorySha256,
                "regeneration.candidate_history_changed", diagnostics);
            Compare(persisted.SupportTreeSha256, actual.SupportTreeSha256,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.CandidateCompositionSha256, actual.CandidateCompositionSha256,
                "regeneration.candidate_package_changed", diagnostics);
            Compare(persisted.CandidateFinalStateHash, actual.CandidateFinalStateHash,
                "regeneration.candidate_package_changed", diagnostics);
            Compare(persisted.CandidateSourceRequestSha256, actual.CandidateSourceRequestSha256,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.CandidatePlanSha256, actual.CandidatePlanSha256,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.CandidateOverlaySha256, actual.CandidateOverlaySha256,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.CandidateGeneratedBaseSha256, actual.CandidateGeneratedBaseSha256,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.GeneratedEncounterCombatSummarySha256,
                actual.GeneratedEncounterCombatSummarySha256,
                "regeneration.candidate_combat_changed", diagnostics);
            Compare(persisted.GeneratedEncounterCombatOverlaySha256,
                actual.GeneratedEncounterCombatOverlaySha256,
                "regeneration.candidate_combat_changed", diagnostics);
            Compare(persisted.GeneratedEncounterCombatContractId,
                actual.GeneratedEncounterCombatContractId,
                "regeneration.candidate_combat_changed", diagnostics);
            Compare(persisted.CandidateSnapshotStatus, actual.CandidateSnapshotStatus,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.MechanicsProfileId, actual.MechanicsProfileId,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.AcceptedMechanicsSummarySha256, actual.AcceptedMechanicsSummarySha256,
                "regeneration.candidate_history_changed", diagnostics);
            Compare(persisted.AcceptedMechanicsCompatibilitySha256,
                actual.AcceptedMechanicsCompatibilitySha256,
                "regeneration.candidate_history_changed", diagnostics);
            Compare(persisted.ExpectedCandidateRcRecordStatus, actual.ExpectedCandidateRcRecordStatus,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.ExpectedCandidateRcOverallStatus, actual.ExpectedCandidateRcOverallStatus,
                "regeneration.candidate_tampered", diagnostics);
            Compare(persisted.DiffSha256, actual.DiffSha256,
                "regeneration.candidate_seal_mismatch", diagnostics);
            Compare(expected.CandidateRootIdentity, persisted.CandidateRootIdentity,
                "regeneration.candidate_seal_mismatch", diagnostics);
            Compare(expected.AttemptId, persisted.AttemptId,
                "regeneration.candidate_seal_mismatch", diagnostics);
            return new GameProjectSeedRegenerationCandidateSealResult
            {
                Passed = diagnostics.Count == 0,
                Seal = persisted,
                Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).ToList()
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException
                                           or InvalidOperationException)
        {
            return Failed("regeneration.candidate_tampered:" + exception.Message);
        }
    }

    public string Serialize(GameProjectSeedRegenerationCandidateSeal seal) =>
        JsonSerializer.Serialize(seal, JsonOptions) + Environment.NewLine;

    public static string TreeSha256(string root, params string[] relativeRoots)
    {
        var fullRoot = Path.GetFullPath(root);
        var rows = new List<(string Path, string Sha)>();
        foreach (var relativeRoot in relativeRoots.Distinct(StringComparer.Ordinal))
        {
            var path = Confined(fullRoot, relativeRoot);
            if (File.Exists(path))
            {
                rows.Add((Relative(fullRoot, path), HashFile(path)));
                continue;
            }
            if (!Directory.Exists(path)) continue;
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                var relative = Relative(fullRoot, file);
                if (Excluded(relative)) continue;
                rows.Add((relative, HashFile(file)));
            }
        }
        var stable = new StringBuilder();
        foreach (var row in rows.OrderBy(row => row.Path, StringComparer.Ordinal))
            stable.Append(row.Path.Length).Append(':').Append(row.Path)
                .Append(row.Sha.Length).Append(':').Append(row.Sha).Append(';');
        return HashText(stable.ToString());
    }

    private static GameProjectSeedRegenerationCandidateSeal Calculate(
        string candidateRoot,
        string candidateRootIdentity,
        string attemptId,
        string selectedHistoryFileName,
        GameProjectBuildResult build,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        GameProjectSeedRegenerationDiff diff,
        GameProjectAuthoringState authoring)
    {
        var sourcePath = Confined(candidateRoot, SeededGeneratedProjectVocabulary.SourceRelativePath);
        var packagePath = Confined(candidateRoot, "package.json");
        var identityPath = new GameProjectIdentityStore().PathFor(candidateRoot);
        var historyPath = Confined(candidateRoot,
            UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot + "/" + selectedHistoryFileName);
        return new GameProjectSeedRegenerationCandidateSeal
        {
            AttemptId = attemptId,
            CandidateRootIdentity = candidateRootIdentity,
            SourceRecordSha256 = HashFile(sourcePath),
            GenerationTreeSha256 = TreeSha256(candidateRoot, SeededGeneratedProjectVocabulary.GenerationRelativeRoot),
            PackageSha256 = HashFile(packagePath),
            AuthoringTreeSha256 = TreeSha256(candidateRoot, UnifiedGameProjectWorkspaceVocabulary.AuthoringRelativeRoot),
            IdentitySha256 = HashFile(identityPath),
            SelectedBuildHistoryFileName = selectedHistoryFileName,
            SelectedBuildHistorySha256 = HashFile(historyPath),
            SupportTreeSha256 = TreeSha256(candidateRoot, "assets", "scripts",
                UnifiedGameProjectWorkspaceVocabulary.ReleaseCandidateRelativeRoot),
            QualifiedAuthoringFingerprint = build.QualifiedAuthoringFingerprint,
            SelectedModuleIdsSha256 = SelectedModuleIdsSha256(authoring),
            ParameterValuesSha256 = ParameterValuesSha256(authoring),
            CandidatePackageSha256 = build.PackageSha256,
            CandidateCompositionSha256 = build.CompositionPackageSha256,
            CandidateFinalStateHash = build.FinalStateHash,
            CandidateSourceRequestSha256 = diff.NewSourceRequestSha256,
            CandidatePlanSha256 = snapshot.GeneratedWorld?.PlanSha256 ?? string.Empty,
            CandidateOverlaySha256 = snapshot.GeneratedWorld?.OverlaySha256 ?? string.Empty,
            CandidateGeneratedBaseSha256 = snapshot.GeneratedWorld?.GeneratedBasePackageSha256 ?? string.Empty,
            GeneratedEncounterCombatSummarySha256 = CanonicalSha256(build.GeneratedEncounterCombat),
            GeneratedEncounterCombatOverlaySha256 = CanonicalSha256(build.GeneratedEncounterCombat?.Overlay),
            GeneratedEncounterCombatContractId = build.GeneratedEncounterCombat?.ContractId ?? string.Empty,
            CandidateSnapshotStatus = snapshot.GeneratedWorld?.Status ?? string.Empty,
            MechanicsProfileId = snapshot.GeneratedWorld?.MechanicsProfileId ?? string.Empty,
            AcceptedMechanicsSummarySha256 = CanonicalSha256(snapshot.AcceptedMechanics),
            AcceptedMechanicsCompatibilitySha256 = CanonicalSha256(snapshot.AcceptedMechanicsCompatibility),
            ExpectedCandidateRcRecordStatus = snapshot.ReleaseCandidateRecordConfigurationStatus,
            ExpectedCandidateRcOverallStatus = snapshot.ReleaseCandidateConfigurationStatus,
            DiffSha256 = HashText(JsonSerializer.Serialize(diff, JsonOptions))
        };
    }

    private static bool Excluded(string relative) =>
        relative.StartsWith(UnifiedGameProjectWorkspaceVocabulary.BuildStagingRelativeRoot + "/",
            StringComparison.OrdinalIgnoreCase)
        || relative.StartsWith("Builds/", StringComparison.OrdinalIgnoreCase)
        || relative.StartsWith(GameProjectSeedRegenerationVocabulary.TransactionsRelativeRoot + "/",
            StringComparison.OrdinalIgnoreCase)
        || relative.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)
        || relative.Contains(".tmp-", StringComparison.OrdinalIgnoreCase)
        || relative.Equals(GameProjectSeedRegenerationVocabulary.CandidateSealRelativePath,
            StringComparison.OrdinalIgnoreCase);

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string Confined(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.Equals(fullRoot, comparison) && !path.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("regeneration.path_escape");
        return path;
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string HashText(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    public static string CanonicalSha256<T>(T value) =>
        HashText(JsonSerializer.Serialize(value, JsonOptions));

    public static string SelectedModuleIdsSha256(GameProjectAuthoringState authoring) => HashText(
        JsonSerializer.Serialize(authoring.Document.SelectedModuleIds
            .OrderBy(value => value, StringComparer.Ordinal), JsonOptions));

    public static string ParameterValuesSha256(GameProjectAuthoringState authoring) => HashText(
        JsonSerializer.Serialize(authoring.Document.ParameterValues
            .OrderBy(value => value.ModuleId, StringComparer.Ordinal)
            .ThenBy(value => value.ParameterId, StringComparer.Ordinal), JsonOptions));

    private static void Compare(string expected, string actual, string diagnostic, ICollection<string> diagnostics)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal)) diagnostics.Add(diagnostic);
    }

    private static void WriteAtomic(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporary, text, Utf8WithoutBom);
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static GameProjectSeedRegenerationCandidateSealResult Failed(string diagnostic) => new()
    {
        Diagnostics = [diagnostic]
    };
}
