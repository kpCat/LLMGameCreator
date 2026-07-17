using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

namespace LLMGameCreator.Application.Design.ProjectStandaloneBuild;

public sealed record ProjectStandalonePayloadEvidenceResult
{
    public bool Passed { get; init; }
    public string SourceKind { get; init; } = "absent";
    public string RunOutputFolder { get; init; } = string.Empty;
    public ProjectStandaloneCurrentPointer? Pointer { get; init; }
    public ProjectStandaloneRunStatus? RunStatus { get; init; }
    public string ProjectManifestSha256 { get; init; } = string.Empty;
    public string PlayerAdapterModelSha256 { get; init; } = string.Empty;
    public string PlayerAdapterFramesSha256 { get; init; } = string.Empty;
    public string GamePackageSha256 { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string ModelFinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<GameProjectSocialHumanFact> HumanFacts { get; init; } = [];
    public string HumanFactsSha256 { get; init; } = string.Empty;
    public ProjectStandalonePayloadSelfCheckResult PayloadSelfCheck { get; init; } = new();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class ProjectStandalonePayloadEvidenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly ProjectStandaloneOutputLocationService _outputLocations;
    private readonly ProjectStandalonePayloadSelfCheckService _selfCheck;

    public ProjectStandalonePayloadEvidenceService(
        ProjectStandaloneOutputLocationService? outputLocations = null,
        ProjectStandalonePayloadSelfCheckService? selfCheck = null)
    {
        _outputLocations = outputLocations ?? new ProjectStandaloneOutputLocationService();
        _selfCheck = selfCheck ?? new ProjectStandalonePayloadSelfCheckService();
    }

    public ProjectStandalonePayloadEvidenceResult InspectForWrite(
        string projectFolder,
        string packageId,
        ProjectStandaloneBuildResult standalone) =>
        Inspect(projectFolder, packageId, standalone, forWrite: true);

    public ProjectStandalonePayloadEvidenceResult InspectForRead(
        string projectFolder,
        string packageId) =>
        Inspect(projectFolder, packageId, null, forWrite: false);

    private ProjectStandalonePayloadEvidenceResult Inspect(
        string projectFolder,
        string packageId,
        ProjectStandaloneBuildResult? expectedStandalone,
        bool forWrite)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFolder);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        var location = _outputLocations.Resolve(projectFolder, packageId, "000000000000");
        var pointerExists = File.Exists(location.CurrentPointerPath);
        if (pointerExists)
        {
            var current = _outputLocations.LoadCurrentOutput(projectFolder, packageId);
            if (!current.Passed || current.Pointer is null)
                return Failure("immutable_current_pointer", MapCurrentPointerDiagnostic(current.Diagnostic));

            var run = current.RunOutputFolder;
            var pointer = current.Pointer;
            try
            {
                var status = ReadStatus(Path.Combine(run, "run-status.json"));
                if (!string.Equals(status.AttemptId, pointer.PublishedAttemptId, StringComparison.Ordinal))
                    return Failure("immutable_current_pointer", $"rc.payload.current_run_attempt_mismatch:{status.AttemptId}:{pointer.PublishedAttemptId}", pointer, run, status);
                if (!string.Equals(status.PackageSha256, pointer.PackageSha256, StringComparison.Ordinal)
                    || !string.Equals(status.FinalStateHash, pointer.FinalStateHash, StringComparison.Ordinal)
                    || !string.Equals(status.HostCacheKey, pointer.HostCacheKey, StringComparison.Ordinal))
                    return Failure("immutable_current_pointer", $"rc.payload.current_run_result_mismatch:{status.PackageSha256}:{status.FinalStateHash}:{status.HostCacheKey}", pointer, run, status);

                var evidence = InspectPayload(
                    Path.Combine(run, "g_Data", "StreamingAssets", "LLMGameCreatorProject"),
                    Path.Combine(run, "build-manifest.json"),
                    pointer,
                    status,
                    requireSelfCheck: true);
                if (!evidence.Passed)
                    return evidence with
                    {
                        SourceKind = "immutable_current_pointer",
                        Pointer = pointer,
                        RunOutputFolder = run
                    };
                if (forWrite)
                {
                    var correlation = CorrelateStandalone(
                        projectFolder, packageId, expectedStandalone, location, pointer, status, evidence);
                    if (correlation is not null)
                        return Failure("immutable_current_pointer", correlation, pointer, run, status);
                }
                return evidence with { SourceKind = "immutable_current_pointer", Pointer = pointer, RunOutputFolder = run };
            }
            catch (JsonException)
            {
                return Failure("immutable_current_pointer", "rc.payload.current_run_invalid_json", pointer, run);
            }
            catch (IOException)
            {
                return Failure("immutable_current_pointer", "rc.payload.current_run_missing", pointer, run);
            }
            catch (InvalidOperationException exception)
            {
                return Failure("immutable_current_pointer", "rc.payload.current_run_result_mismatch:" + exception.Message, pointer, run);
            }
        }

        var legacy = InspectLegacy(projectFolder, packageId, expectedStandalone, forWrite);
        if (legacy.SourceKind != "absent" || forWrite) return legacy;
        return legacy;
    }

    private ProjectStandalonePayloadEvidenceResult InspectLegacy(
        string projectFolder,
        string packageId,
        ProjectStandaloneBuildResult? expectedStandalone,
        bool forWrite)
    {
        var payloadRoot = LegacyPayloadRoot(projectFolder, packageId);
        var manifestPath = Path.Combine(payloadRoot, "project-manifest.json");
        var modelPath = Path.Combine(payloadRoot, "player-adapter-model.json");
        var present = File.Exists(manifestPath) || File.Exists(modelPath);
        if (!present)
            return new ProjectStandalonePayloadEvidenceResult { Passed = true, SourceKind = "absent" };
        if (forWrite && expectedStandalone is not null)
        {
            var expectedRoot = Path.GetFullPath(expectedStandalone.OutputFolder);
            if (!PathsEqual(expectedRoot, Path.GetFullPath(Path.Combine(projectFolder, "Builds", "Windows", SafeSlug(packageId)))))
                return Failure("legacy_project_local_output", "rc.write.standalone_pointer_mismatch");
        }
        try
        {
            var evidence = InspectPayload(payloadRoot, buildManifestPath: string.Empty, pointer: null, status: null, requireSelfCheck: false);
            return evidence with { SourceKind = "legacy_project_local_output", RunOutputFolder = Path.GetDirectoryName(payloadRoot) ?? string.Empty };
        }
        catch (JsonException)
        {
            return Failure("legacy_project_local_output", "rc.payload.current_run_invalid_json");
        }
        catch (IOException)
        {
            return Failure("legacy_project_local_output", "rc.payload.missing");
        }
        catch (InvalidOperationException exception)
        {
            return Failure("legacy_project_local_output", "rc.payload.invalid:" + exception.Message);
        }
    }

    private ProjectStandalonePayloadEvidenceResult InspectPayload(
        string payloadRoot,
        string buildManifestPath,
        ProjectStandaloneCurrentPointer? pointer,
        ProjectStandaloneRunStatus? status,
        bool requireSelfCheck)
    {
        var manifestPath = Path.Combine(payloadRoot, "project-manifest.json");
        var modelPath = Path.Combine(payloadRoot, "player-adapter-model.json");
        var framesPath = Path.Combine(payloadRoot, "player-adapter-frames.json");
        var packagePath = Path.Combine(payloadRoot, "game-package.json");
        if (!File.Exists(manifestPath) || !File.Exists(modelPath))
            return Failure(pointer is null ? "legacy_project_local_output" : "immutable_current_pointer", "rc.payload.missing", pointer, Path.GetDirectoryName(payloadRoot) ?? string.Empty, status);
        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath, Encoding.UTF8));
        using var model = JsonDocument.Parse(File.ReadAllText(modelPath, Encoding.UTF8));
        var manifestRoot = manifest.RootElement;
        var modelRoot = model.RootElement;
        if (pointer is not null
            && (String(manifestRoot, "schemaVersion") != "llmgc_project_standalone_v2"
                || String(modelRoot, "schemaVersion") != "llmgc_player_adapter_model_v2"))
            return Failure(pointer is null ? "legacy_project_local_output" : "immutable_current_pointer", "rc.payload.current_run_result_mismatch:schema", pointer, Path.GetDirectoryName(payloadRoot) ?? string.Empty, status);
        var facts = modelRoot.TryGetProperty("humanReviewFacts", out var factsElement)
            && factsElement.ValueKind == JsonValueKind.Array
            ? factsElement.EnumerateArray().Select(item => new GameProjectSocialHumanFact
            {
                Label = String(item, "label"), Value = String(item, "value")
            }).ToList()
            : [];
        var packageSha = String(manifestRoot, "packageSha256");
        var compositionSha = String(manifestRoot, "compositionPackageSha256");
        var finalHash = String(manifestRoot, "finalStateHash");
        var modelFinalHash = String(modelRoot, "finalStateHash");
        var packageShaActual = File.Exists(packagePath) ? HashFile(packagePath) : string.Empty;
        var selfCheck = new ProjectStandalonePayloadSelfCheckResult();
        var diagnostics = new List<string>();
        if (requireSelfCheck)
        {
            selfCheck = _selfCheck.Check(payloadRoot, buildManifestPath);
            if (!selfCheck.Passed) diagnostics.AddRange(selfCheck.FailedCheckCodes);
        }
        if (pointer is not null && (!string.Equals(packageSha, pointer.PackageSha256, StringComparison.Ordinal)
                                    || !string.Equals(compositionSha, pointer.CompositionPackageSha256, StringComparison.Ordinal)
                                    || !string.Equals(finalHash, pointer.FinalStateHash, StringComparison.Ordinal)
                                    || !string.Equals(modelFinalHash, pointer.FinalStateHash, StringComparison.Ordinal)))
            diagnostics.Add("rc.payload.current_run_hash_mismatch:manifest-or-model");
        if (pointer is not null && (string.IsNullOrWhiteSpace(packageShaActual)
                                    || !string.Equals(packageShaActual, packageSha, StringComparison.Ordinal)))
            diagnostics.Add("rc.payload.current_run_hash_mismatch:game-package");
        if (diagnostics.Count > 0)
            return new ProjectStandalonePayloadEvidenceResult
            {
                Passed = false, SourceKind = pointer is null ? "legacy_project_local_output" : "immutable_current_pointer",
                Pointer = pointer, RunStatus = status, RunOutputFolder = Path.GetDirectoryName(payloadRoot) ?? string.Empty,
                PackageSha256 = packageSha, CompositionPackageSha256 = compositionSha, FinalStateHash = finalHash,
                ModelFinalStateHash = modelFinalHash, HumanFacts = facts, HumanFactsSha256 = HashFacts(facts),
                PayloadSelfCheck = selfCheck, Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).ToList()
            };
        return new ProjectStandalonePayloadEvidenceResult
        {
            Passed = true, Pointer = pointer, RunStatus = status, RunOutputFolder = Path.GetDirectoryName(payloadRoot) ?? string.Empty,
            ProjectManifestSha256 = HashFile(manifestPath), PlayerAdapterModelSha256 = HashFile(modelPath),
            PlayerAdapterFramesSha256 = File.Exists(framesPath) ? HashFile(framesPath) : string.Empty,
            GamePackageSha256 = packageShaActual, PackageSha256 = packageSha, CompositionPackageSha256 = compositionSha,
            FinalStateHash = finalHash, ModelFinalStateHash = modelFinalHash, HumanFacts = facts,
            HumanFactsSha256 = HashFacts(facts), PayloadSelfCheck = selfCheck
        };
    }

    private static string? CorrelateStandalone(
        string projectFolder,
        string packageId,
        ProjectStandaloneBuildResult? standalone,
        ProjectStandaloneOutputLocation location,
        ProjectStandaloneCurrentPointer pointer,
        ProjectStandaloneRunStatus status,
        ProjectStandalonePayloadEvidenceResult evidence)
    {
        if (standalone is null) return "rc.write.payload_evidence_missing";
        if (!string.Equals(standalone.Status, "GREEN", StringComparison.Ordinal)
            || standalone.OutputLocationKind != ProjectStandaloneBuildVocabulary.ImmutableOutputLocationKind)
            return "rc.write.standalone_pointer_mismatch";
        var expectedRun = Path.GetFullPath(Path.Combine(location.RunsFolder, pointer.RunDirectoryName));
        if (!string.Equals(standalone.OutputProjectToken, pointer.ProjectToken, StringComparison.Ordinal)
            || !string.Equals(standalone.OutputRunDirectoryName, pointer.RunDirectoryName, StringComparison.Ordinal)
            || !string.Equals(standalone.AttemptId, pointer.PublishedAttemptId, StringComparison.Ordinal))
            return "rc.write.standalone_pointer_mismatch";
        if (!PathsEqual(standalone.OutputFolder, expectedRun)
            || !PathsEqual(standalone.ExecutablePath, Path.Combine(expectedRun, "g.exe"))
            || !PathsEqual(standalone.BuildManifestPath, Path.Combine(expectedRun, "build-manifest.json")))
            return "rc.write.standalone_pointer_mismatch";
        var pointerPath = location.CurrentPointerPath;
        var pointerSha = File.Exists(pointerPath) ? HashFile(pointerPath) : string.Empty;
        if (string.IsNullOrWhiteSpace(pointerSha) || !string.Equals(standalone.CurrentPointerSha256, pointerSha, StringComparison.Ordinal))
            return "rc.write.standalone_pointer_mismatch";
        if (!string.Equals(standalone.PackageSha256, pointer.PackageSha256, StringComparison.Ordinal)
            || !string.Equals(standalone.FinalStateHash, pointer.FinalStateHash, StringComparison.Ordinal)
            || !string.Equals(standalone.HostCacheKey, pointer.HostCacheKey, StringComparison.Ordinal)
            || !string.Equals(evidence.GamePackageSha256, pointer.PackageSha256, StringComparison.Ordinal))
            return "rc.write.actual_payload_hash_mismatch";
        if (standalone.SmokeExitCode != 0 || !standalone.LaunchSmokePassed || !standalone.PayloadSelfCheckPassed
            || !standalone.LegacyHostParserCompatibilityPassed || standalone.SelfCheckTotalCount <= 0
            || standalone.SelfCheckPassedCount != standalone.SelfCheckTotalCount
            || !status.SmokeMarkersPassed || !status.PlayerLogPresent)
            return "rc.write.standalone_pointer_mismatch";
        return null;
    }

    private static ProjectStandalonePayloadEvidenceResult Failure(
        string sourceKind,
        string diagnostic,
        ProjectStandaloneCurrentPointer? pointer = null,
        string run = "",
        ProjectStandaloneRunStatus? status = null) => new()
    {
        Passed = false, SourceKind = sourceKind, Pointer = pointer, RunOutputFolder = run, RunStatus = status,
        Diagnostics = [diagnostic]
    };

    private static ProjectStandaloneRunStatus ReadStatus(string path) =>
        JsonSerializer.Deserialize<ProjectStandaloneRunStatus>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
        ?? throw new JsonException("run-status.json is empty");

    private static string MapCurrentPointerDiagnostic(string diagnostic) =>
        diagnostic.Contains("missing", StringComparison.OrdinalIgnoreCase)
            ? "rc.payload.current_run_missing"
            : diagnostic.Contains("hash", StringComparison.OrdinalIgnoreCase)
                ? "rc.payload.current_run_hash_mismatch"
                : "rc.payload.current_pointer_invalid:" + (string.IsNullOrWhiteSpace(diagnostic) ? "unknown" : diagnostic);

    private static string LegacyPayloadRoot(string projectFolder, string packageId)
    {
        var slug = SafeSlug(packageId);
        var root = Path.GetFullPath(projectFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var payload = Path.GetFullPath(Path.Combine(root, "Builds", "Windows", slug, slug + "_Data", "StreamingAssets", "LLMGameCreatorProject"));
        var confinedRoot = root + Path.DirectorySeparatorChar;
        if (!payload.StartsWith(confinedRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidOperationException("standalone.output.path_escape");
        return payload;
    }

    private static bool PathsEqual(string left, string right) =>
        string.Equals(Path.GetFullPath(left), Path.GetFullPath(right),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private static string String(JsonElement element, string name) =>
        element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty : string.Empty;

    public static bool ContainsAll(
        IReadOnlyList<GameProjectSocialHumanFact> actual,
        IReadOnlyList<GameProjectSocialHumanFact> required) => required.All(expected =>
        actual.Any(item => string.Equals(item.Label, expected.Label, StringComparison.Ordinal)
                           && string.Equals(item.Value, expected.Value, StringComparison.Ordinal)));

    public static string HashFacts(IReadOnlyList<GameProjectSocialHumanFact> facts)
    {
        var text = new StringBuilder();
        foreach (var fact in facts)
            text.Append(fact.Label.Length).Append(':').Append(fact.Label)
                .Append(fact.Value.Length).Append(':').Append(fact.Value).Append(';');
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString()))).ToLowerInvariant();
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string SafeSlug(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value.ToLowerInvariant())
            builder.Append(char.IsLetterOrDigit(character) ? character : '-');
        return builder.ToString().Trim('-') is { Length: > 0 } slug ? slug : "game";
    }
}
