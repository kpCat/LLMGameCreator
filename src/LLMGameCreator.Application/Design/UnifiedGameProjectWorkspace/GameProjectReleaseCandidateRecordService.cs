using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Generation.Procedural;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public sealed record GameProjectReleaseCandidateRecord
{
    public string SchemaVersion { get; init; } = UnifiedGameProjectWorkspaceVocabulary.ReleaseCandidateSchemaVersion;
    public DateTimeOffset CompletedAtUtc { get; init; }
    public string Status { get; init; } = "GREEN";
    public string ProjectPackageId { get; init; } = string.Empty;
    public string ProjectTitle { get; init; } = string.Empty;
    public string ProjectVersion { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public GameProjectAcceptedMechanicsSummary AcceptedMechanicsSummary { get; init; } = new();
    public string HostCacheKey { get; init; } = string.Empty;
    public bool HostReused { get; init; }
    public bool HostRebuilt { get; init; }
    public bool LaunchSmokePassed { get; init; }
    public int SelfCheckPassedCount { get; init; }
    public int SelfCheckTotalCount { get; init; }
    public string StandalonePackageSha256 { get; init; } = string.Empty;
    public string StandaloneFinalStateHash { get; init; } = string.Empty;
    public string PlayerAdapterModelSha256 { get; init; } = string.Empty;
    public string HumanFactsSha256 { get; init; } = string.Empty;
}

public sealed record GameProjectReleaseCandidateReadResult
{
    public GameProjectReleaseCandidateRecord? Record { get; init; }
    public string ConfigurationStatus { get; init; } = "ABSENT";
    public string RecordPath { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GameProjectReleaseCandidateReadRequest
{
    public string ProjectFolder { get; init; } = string.Empty;
    public FeatureModuleCompositionDocument Document { get; init; } = new();
    public FeatureModuleLibrarySnapshot Library { get; init; } = new();
    public GameProjectIdentityDocument Identity { get; init; } = new();
}

public sealed class GameProjectReleaseCandidateRecordService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string RecordPath(string projectFolder) => Confined(
        projectFolder,
        UnifiedGameProjectWorkspaceVocabulary.ReleaseCandidateRecordRelativePath);

    public static string ResolveOverallStatus(
        GameProjectAcceptedMechanicsSummary? acceptedMechanics,
        bool acceptedMechanicsCurrent,
        string packageSha256,
        string compositionPackageSha256,
        string finalStateHash,
        GameProjectReleaseCandidateReadResult releaseCandidate)
    {
        if (acceptedMechanics is { Passed: true } && acceptedMechanicsCurrent)
        {
            var recordMatchesBuild = releaseCandidate.Record is not null
                                     && string.Equals(releaseCandidate.Record.PackageSha256,
                                         packageSha256, StringComparison.Ordinal)
                                     && string.Equals(releaseCandidate.Record.CompositionPackageSha256,
                                         compositionPackageSha256, StringComparison.Ordinal)
                                     && string.Equals(releaseCandidate.Record.FinalStateHash,
                                         finalStateHash, StringComparison.Ordinal);
            return releaseCandidate.ConfigurationStatus == "CURRENT" && recordMatchesBuild
                ? "CURRENT"
                : "BUILD_GREEN_STANDALONE_PENDING";
        }

        return releaseCandidate.Record is not null
            ? releaseCandidate.ConfigurationStatus
            : "ABSENT";
    }

    public GameProjectReleaseCandidateRecord Write(
        string projectFolder,
        GameProjectIdentityDocument identity,
        GameProjectBuildResult build,
        ProjectStandaloneBuildResult standalone)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(build);
        ArgumentNullException.ThrowIfNull(standalone);
        var summary = build.AcceptedMechanics;
        Require(build.Passed && string.Equals(build.Status, "GREEN", StringComparison.Ordinal),
            "rc.write.build_not_green");
        Require(summary is { Passed: true }, "rc.write.accepted_mechanics_incomplete");
        Require(string.Equals(standalone.Status, "GREEN", StringComparison.Ordinal),
            "rc.write.standalone_not_green");
        Require(string.Equals(standalone.PackageSha256, build.PackageSha256, StringComparison.Ordinal)
                && string.Equals(standalone.FinalStateHash, build.FinalStateHash, StringComparison.Ordinal),
            "rc.write.standalone_build_hash_mismatch");
        Require(standalone.HostReused && !standalone.HostRebuilt, "rc.write.host_not_reused");
        Require(standalone.LaunchSmokePassed, "rc.write.smoke_failed");
        Require(standalone.SelfCheckTotalCount > 0
                && standalone.SelfCheckPassedCount == standalone.SelfCheckTotalCount,
            "rc.write.self_check_failed");

        var packagePath = Confined(projectFolder, "package.json");
        Require(File.Exists(packagePath) && string.Equals(HashFile(packagePath), build.PackageSha256, StringComparison.Ordinal),
            "rc.write.activated_package_hash_mismatch");
        var payload = InspectPayload(projectFolder, identity.PackageId);
        Require(string.Equals(payload.PackageSha256, build.PackageSha256, StringComparison.Ordinal)
                && string.Equals(payload.CompositionPackageSha256, build.CompositionPackageSha256, StringComparison.Ordinal)
                && string.Equals(payload.FinalStateHash, build.FinalStateHash, StringComparison.Ordinal)
                && string.Equals(payload.ModelFinalStateHash, build.FinalStateHash, StringComparison.Ordinal),
            "rc.write.actual_payload_hash_mismatch");
        Require(ContainsAll(payload.HumanFacts, summary!.HumanFacts),
            "rc.write.actual_payload_missing_accepted_fact");
        Require(payload.HumanFacts.Any(fact => fact.Label == "Release Candidate" && fact.Value == "готов"),
            "rc.write.actual_payload_missing_ready_fact");

        var record = new GameProjectReleaseCandidateRecord
        {
            CompletedAtUtc = DateTimeOffset.UtcNow,
            ProjectPackageId = identity.PackageId,
            ProjectTitle = identity.Title,
            ProjectVersion = identity.Version,
            QualifiedAuthoringFingerprint = summary.QualifiedAuthoringFingerprint,
            PackageSha256 = build.PackageSha256,
            CompositionPackageSha256 = build.CompositionPackageSha256,
            FinalStateHash = build.FinalStateHash,
            AcceptedMechanicsSummary = summary,
            HostCacheKey = standalone.HostCacheKey,
            HostReused = standalone.HostReused,
            HostRebuilt = standalone.HostRebuilt,
            LaunchSmokePassed = standalone.LaunchSmokePassed,
            SelfCheckPassedCount = standalone.SelfCheckPassedCount,
            SelfCheckTotalCount = standalone.SelfCheckTotalCount,
            StandalonePackageSha256 = standalone.PackageSha256,
            StandaloneFinalStateHash = standalone.FinalStateHash,
            PlayerAdapterModelSha256 = payload.PlayerAdapterModelSha256,
            HumanFactsSha256 = HashFacts(payload.HumanFacts)
        };
        WriteAtomic(RecordPath(projectFolder), JsonSerializer.Serialize(record, JsonOptions) + Environment.NewLine);
        return record;
    }

    public GameProjectReleaseCandidateReadResult Read(GameProjectReleaseCandidateReadRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProjectFolder);
        ArgumentNullException.ThrowIfNull(request.Document);
        ArgumentNullException.ThrowIfNull(request.Library);
        ArgumentNullException.ThrowIfNull(request.Identity);
        var projectFolder = request.ProjectFolder;
        var document = request.Document;
        var library = request.Library;
        var identity = request.Identity;
        var path = RecordPath(projectFolder);
        if (!File.Exists(path)) return new GameProjectReleaseCandidateReadResult { RecordPath = path };

        GameProjectReleaseCandidateRecord? record;
        try
        {
            record = JsonSerializer.Deserialize<GameProjectReleaseCandidateRecord>(
                File.ReadAllText(path, Encoding.UTF8), JsonOptions);
        }
        catch (JsonException)
        {
            return Rejected(path, "rc.read.invalid_json");
        }
        catch (IOException)
        {
            return Rejected(path, "rc.read.unreadable");
        }
        if (record is null) return Rejected(path, "rc.read.empty");

        var invalid = ValidateRecord(record);
        if (invalid is not null) return Rejected(path, invalid);
        try
        {
            var payloadPaths = PayloadPaths(projectFolder, record.ProjectPackageId);
            var manifestExists = File.Exists(payloadPaths.ProjectManifestPath);
            var modelExists = File.Exists(payloadPaths.PlayerAdapterModelPath);
            if (manifestExists != modelExists) return Rejected(path, "rc.read.payload_incomplete");
            if (manifestExists)
            {
                var payload = InspectPayload(projectFolder, record.ProjectPackageId);
                if (!string.Equals(payload.PackageSha256, record.PackageSha256, StringComparison.Ordinal)
                    || !string.Equals(payload.CompositionPackageSha256, record.CompositionPackageSha256, StringComparison.Ordinal)
                    || !string.Equals(payload.FinalStateHash, record.FinalStateHash, StringComparison.Ordinal)
                    || !string.Equals(payload.ModelFinalStateHash, record.FinalStateHash, StringComparison.Ordinal))
                    return Rejected(path, "rc.read.payload_hash_mismatch");
                if (!string.Equals(payload.PlayerAdapterModelSha256, record.PlayerAdapterModelSha256, StringComparison.Ordinal))
                    return Rejected(path, "rc.read.player_adapter_model_hash_mismatch");
                if (!string.Equals(HashFacts(payload.HumanFacts), record.HumanFactsSha256, StringComparison.Ordinal))
                    return Rejected(path, "rc.read.human_facts_hash_mismatch");
                if (!ContainsAll(payload.HumanFacts, record.AcceptedMechanicsSummary.HumanFacts)
                    || !payload.HumanFacts.Any(fact => fact.Label == "Release Candidate" && fact.Value == "готов"))
                    return Rejected(path, "rc.read.payload_fact_mismatch");
            }
        }
        catch (JsonException)
        {
            return Rejected(path, "rc.read.payload_invalid_json");
        }
        catch (IOException)
        {
            return Rejected(path, "rc.read.payload_unreadable");
        }
        catch (Exception exception) when (exception is InvalidOperationException
                                          or KeyNotFoundException
                                          or FormatException)
        {
            return Rejected(path, "rc.read.payload_invalid_shape");
        }

        var packagePath = Confined(projectFolder, "package.json");
        if (!File.Exists(packagePath)) return Rejected(path, "rc.read.current_package_missing");
        string actualPackageSha;
        try
        {
            actualPackageSha = HashFile(packagePath);
        }
        catch (IOException)
        {
            return Rejected(path, "rc.read.current_package_unreadable");
        }

        if (string.IsNullOrWhiteSpace(document.LastActivatedProjectPackageSha256)
            || string.IsNullOrWhiteSpace(document.LastCompositionPackageSha256)
            || string.IsNullOrWhiteSpace(document.LastQualifiedFinalStateHash))
            return Result(record, "UNKNOWN", path, ["rc.read.current_build_identity_missing"]);
        if (!string.Equals(actualPackageSha, document.LastActivatedProjectPackageSha256, StringComparison.Ordinal))
            return Rejected(path, "rc.read.current_package_hash_mismatch");
        if (!string.Equals(record.ProjectPackageId, identity.PackageId, StringComparison.Ordinal))
            return Rejected(path, "rc.read.project_package_id_mismatch");
        if (!string.Equals(record.PackageSha256, document.LastActivatedProjectPackageSha256, StringComparison.Ordinal)
            || !string.Equals(record.CompositionPackageSha256, document.LastCompositionPackageSha256, StringComparison.Ordinal)
            || !string.Equals(record.FinalStateHash, document.LastQualifiedFinalStateHash, StringComparison.Ordinal))
            return Result(record, "LAST_SUCCESS", path, ["rc.read.record_build_identity_differs_from_current"]);
        if (!string.Equals(record.ProjectTitle, identity.Title, StringComparison.Ordinal)
            || !string.Equals(record.ProjectVersion, identity.Version, StringComparison.Ordinal))
            return Result(record, "LAST_SUCCESS", path, ["rc.read.project_identity_metadata_differs"]);

        var fingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(document, library);
        var status = !fingerprint.Passed || string.IsNullOrWhiteSpace(fingerprint.Sha256)
            ? "UNKNOWN"
            : string.Equals(record.QualifiedAuthoringFingerprint, fingerprint.Sha256, StringComparison.Ordinal)
                ? "CURRENT"
                : "LAST_SUCCESS";
        if (status == "CURRENT" && WorldChangeRequiresStandalone(projectFolder, path))
            status = "LAST_SUCCESS";
        return Result(record, status, path, fingerprint.Diagnostics);
    }

    private static bool WorldChangeRequiresStandalone(string projectFolder, string releaseCandidatePath)
    {
        var worldChangePath = Confined(projectFolder, GameProjectGeneratedWorldChangeVocabulary.RelativePath);
        if (!File.Exists(worldChangePath)) return false;
        try
        {
            var change = JsonSerializer.Deserialize<GameProjectGeneratedWorldChangeRecord>(
                File.ReadAllText(worldChangePath, Encoding.UTF8), JsonOptions);
            return change is
                {
                    SchemaVersion: GameProjectGeneratedWorldChangeVocabulary.SchemaVersion,
                    Status: "GREEN",
                    TransactionState: "committed"
                }
                && string.Equals(change.PreviousReleaseCandidateRecordSha256,
                    HashFile(releaseCandidatePath), StringComparison.Ordinal);
        }
        catch (Exception exception) when (exception is IOException or JsonException)
        {
            return false;
        }
    }

    private static string? ValidateRecord(GameProjectReleaseCandidateRecord record)
    {
        if (!string.Equals(record.SchemaVersion, UnifiedGameProjectWorkspaceVocabulary.ReleaseCandidateSchemaVersion,
                StringComparison.Ordinal)) return "rc.read.unsupported_schema";
        if (!string.Equals(record.Status, "GREEN", StringComparison.Ordinal)) return "rc.read.status_not_green";
        if (record.CompletedAtUtc == default) return "rc.read.missing_completion_time";
        if (string.IsNullOrWhiteSpace(record.ProjectPackageId)) return "rc.read.missing_project_identity";
        if (string.IsNullOrWhiteSpace(record.QualifiedAuthoringFingerprint)) return "rc.read.missing_fingerprint";
        if (record.AcceptedMechanicsSummary is not { Passed: true }) return "rc.read.accepted_summary_missing_or_failed";
        if (!string.Equals(record.AcceptedMechanicsSummary.QualifiedAuthoringFingerprint,
                record.QualifiedAuthoringFingerprint, StringComparison.Ordinal))
            return "rc.read.accepted_summary_fingerprint_mismatch";
        if (string.IsNullOrWhiteSpace(record.PackageSha256)
            || string.IsNullOrWhiteSpace(record.CompositionPackageSha256)
            || string.IsNullOrWhiteSpace(record.FinalStateHash)) return "rc.read.missing_build_hash";
        if (!string.Equals(record.PackageSha256, record.StandalonePackageSha256, StringComparison.Ordinal)
            || !string.Equals(record.FinalStateHash, record.StandaloneFinalStateHash, StringComparison.Ordinal))
            return "rc.read.standalone_hash_mismatch";
        if (!record.HostReused || record.HostRebuilt) return "rc.read.host_reuse_contract_failed";
        if (!record.LaunchSmokePassed || record.SelfCheckTotalCount <= 0
                                      || record.SelfCheckPassedCount != record.SelfCheckTotalCount)
            return "rc.read.smoke_or_self_check_failed";
        if (string.IsNullOrWhiteSpace(record.HostCacheKey)
            || string.IsNullOrWhiteSpace(record.PlayerAdapterModelSha256)
            || string.IsNullOrWhiteSpace(record.HumanFactsSha256)) return "rc.read.missing_payload_hash";
        return null;
    }

    private static PayloadInspection InspectPayload(string projectFolder, string packageId)
    {
        var paths = PayloadPaths(projectFolder, packageId);
        if (!File.Exists(paths.ProjectManifestPath) || !File.Exists(paths.PlayerAdapterModelPath))
            throw new InvalidOperationException("rc.payload.missing");
        using var manifest = JsonDocument.Parse(File.ReadAllText(paths.ProjectManifestPath, Encoding.UTF8));
        using var model = JsonDocument.Parse(File.ReadAllText(paths.PlayerAdapterModelPath, Encoding.UTF8));
        var facts = model.RootElement.GetProperty("humanReviewFacts").EnumerateArray()
            .Select(item => new GameProjectSocialHumanFact
            {
                Label = item.GetProperty("label").GetString() ?? string.Empty,
                Value = item.GetProperty("value").GetString() ?? string.Empty
            }).ToList();
        return new PayloadInspection(
            manifest.RootElement.GetProperty("packageSha256").GetString() ?? string.Empty,
            manifest.RootElement.GetProperty("compositionPackageSha256").GetString() ?? string.Empty,
            manifest.RootElement.GetProperty("finalStateHash").GetString() ?? string.Empty,
            model.RootElement.GetProperty("finalStateHash").GetString() ?? string.Empty,
            HashFile(paths.PlayerAdapterModelPath),
            facts);
    }

    private static PayloadPathSet PayloadPaths(string projectFolder, string packageId)
    {
        var slug = SafeSlug(string.IsNullOrWhiteSpace(packageId) ? Path.GetFileName(projectFolder) : packageId);
        var payloadRoot = Confined(projectFolder,
            Path.Combine("Builds", "Windows", slug, slug + "_Data", "StreamingAssets", "LLMGameCreatorProject"));
        return new PayloadPathSet(
            Path.Combine(payloadRoot, "project-manifest.json"),
            Path.Combine(payloadRoot, "player-adapter-model.json"));
    }

    private static bool ContainsAll(
        IReadOnlyList<GameProjectSocialHumanFact> actual,
        IReadOnlyList<GameProjectSocialHumanFact> required) => required.All(expected =>
        actual.Any(item => string.Equals(item.Label, expected.Label, StringComparison.Ordinal)
                           && string.Equals(item.Value, expected.Value, StringComparison.Ordinal)));

    private static string HashFacts(IReadOnlyList<GameProjectSocialHumanFact> facts)
    {
        var text = new StringBuilder();
        foreach (var fact in facts)
            text.Append(fact.Label.Length).Append(':').Append(fact.Label)
                .Append(fact.Value.Length).Append(':').Append(fact.Value).Append(';');
        return HashText(text.ToString());
    }

    private static void WriteAtomic(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            var bytes = new UTF8Encoding(false).GetBytes(text);
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                       4096, FileOptions.WriteThrough))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static GameProjectReleaseCandidateReadResult Rejected(string path, string diagnostic) => new()
    {
        RecordPath = path,
        Diagnostics = [diagnostic]
    };

    private static GameProjectReleaseCandidateReadResult Result(
        GameProjectReleaseCandidateRecord record,
        string status,
        string path,
        IReadOnlyList<string> diagnostics) => new()
    {
        Record = record,
        ConfigurationStatus = status,
        RecordPath = path,
        Diagnostics = diagnostics
    };

    private static void Require(bool condition, string diagnostic)
    {
        if (!condition) throw new InvalidOperationException(diagnostic);
    }

    private static string Confined(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.StartsWith(fullRoot, comparison)) throw new InvalidOperationException("Project path escape rejected.");
        return path;
    }

    private static string SafeSlug(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value.ToLowerInvariant())
            builder.Append(char.IsLetterOrDigit(character) ? character : '-');
        return builder.ToString().Trim('-') is { Length: > 0 } slug ? slug : "game";
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string HashText(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private sealed record PayloadPathSet(string ProjectManifestPath, string PlayerAdapterModelPath);
    private sealed record PayloadInspection(
        string PackageSha256,
        string CompositionPackageSha256,
        string FinalStateHash,
        string ModelFinalStateHash,
        string PlayerAdapterModelSha256,
        IReadOnlyList<GameProjectSocialHumanFact> HumanFacts);
}
