using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.ProjectStandaloneBuild;

public sealed class ProjectStandalonePayloadSelfCheckService
{
    private const string FramePattern = "\\{[^{}]*\\\"index\\\"\\s*:\\s*(\\d+)[^{}]*\\\"title\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"[^{}]*\\\"category\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"[^{}]*\\\"stateHash\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"[^{}]*\\}";
    private const string FactPattern = "\\{\\s*\\\"label\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"\\s*,\\s*\\\"value\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"";

    public ProjectStandalonePayloadSelfCheckResult CheckOutput(string outputFolder, string executablePath)
    {
        var data = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(executablePath) + "_Data");
        return Check(
            Path.Combine(data, "StreamingAssets", "LLMGameCreatorProject"),
            Path.Combine(outputFolder, "build-manifest.json"));
    }

    public ProjectStandalonePayloadSelfCheckResult Check(string payloadRoot, string buildManifestPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(buildManifestPath);

        var manifestPath = Path.Combine(payloadRoot, "project-manifest.json");
        var modelPath = Path.Combine(payloadRoot, "player-adapter-model.json");
        var framesPath = Path.Combine(payloadRoot, "player-adapter-frames.json");
        var launchPath = Path.Combine(payloadRoot, "standalone-launch.json");
        var packagePath = Path.Combine(payloadRoot, "game-package.json");
        var required = new[] { manifestPath, modelPath, framesPath, launchPath, packagePath, buildManifestPath };
        var missing = required.Where(path => !File.Exists(path)).Select(Path.GetFileName).OrderBy(value => value, StringComparer.Ordinal).ToList();

        JsonDocument? manifest = null;
        JsonDocument? model = null;
        JsonDocument? frames = null;
        JsonDocument? launch = null;
        JsonDocument? package = null;
        JsonDocument? buildManifest = null;
        var parseErrors = new List<string>();
        if (missing.Count == 0)
        {
            manifest = Parse(manifestPath, parseErrors);
            model = Parse(modelPath, parseErrors);
            frames = Parse(framesPath, parseErrors);
            launch = Parse(launchPath, parseErrors);
            package = Parse(packagePath, parseErrors);
            buildManifest = Parse(buildManifestPath, parseErrors);
        }

        try
        {
            var schemasPassed = missing.Count == 0
                                && parseErrors.Count == 0
                                && String(manifest, "schemaVersion") == "llmgc_project_standalone_v2"
                                && String(model, "schemaVersion") == "llmgc_player_adapter_model_v2"
                                && String(launch, "schemaVersion") == "llmgc_standalone_launch_v2"
                                && String(buildManifest, "schemaVersion") == "llmgc_project_standalone_build_v1";
            var check01Code = missing.Count > 0
                ? "standalone.payload.file_missing"
                : parseErrors.Count > 0
                    ? "standalone.payload.json_invalid"
                    : "standalone.payload.unsupported_schema";
            if (schemasPassed) check01Code = "standalone.payload.files_and_schemas";

            var manifestRoot = Root(manifest);
            var modelRoot = Root(model);
            var framesRoot = Root(frames);
            var effectiveParameters = Array(manifestRoot, "effectiveParameters");
            var selectedModules = Array(manifestRoot, "selectedModuleIds");
            var frameItems = framesRoot is { ValueKind: JsonValueKind.Array }
                ? framesRoot.Value.EnumerateArray().ToList()
                : [];
            var humanFacts = Array(modelRoot, "humanReviewFacts");

            var identityPassed = NonBlank(manifestRoot, "projectPackageId")
                                 && NonBlank(manifestRoot, "projectTitle")
                                 && NonBlank(manifestRoot, "projectVersion");
            var hashesPassed = NonBlank(manifestRoot, "packageSha256")
                               && NonBlank(manifestRoot, "finalStateHash");
            var authorityPassed = Bool(manifestRoot, "runtimeAuthority")
                                  && !Bool(manifestRoot, "unityGameplayTruth")
                                  && !Bool(manifestRoot, "projectionOnly");
            var framesPresent = frameItems.Count > 0;
            var contiguous = framesPresent && frameItems.Select((frame, index) =>
                    Int(frame, "index") == index
                    && NonBlank(frame, "title")
                    && NonBlank(frame, "category")
                    && NonBlank(frame, "stateHash"))
                .All(value => value);
            var selectedCountPassed = Int(manifestRoot, "selectedOptionalMechanicCount") == selectedModules.Count;
            var activeCountPassed = Int(manifestRoot, "activeMechanicCount")
                                    == Int(manifestRoot, "requiredMechanicCount")
                                    + Int(manifestRoot, "selectedOptionalMechanicCount");
            var parameterCountPassed = Int(manifestRoot, "configuredParameterCount") == effectiveParameters.Count;
            var factsPassed = humanFacts.Count > 0 && humanFacts.All(fact =>
                NonBlank(fact, "label") && NonBlank(fact, "value"));
            var cursorPassed = framesPresent;
            var equipment = Decimal(modelRoot, "equipmentDamageBonus");
            var total = Decimal(modelRoot, "totalAdditionalDamage");
            var damagePassed = equipment >= 0 && total >= equipment;
            var packageHashPassed = package is not null
                                    && string.Equals(HashFile(packagePath), String(manifest, "packageSha256"),
                                        StringComparison.Ordinal);

            var checks = new List<ProjectStandalonePayloadCheckResult>
            {
                Result(1, check01Code, schemasPassed, missing.Count > 0
                    ? "Missing: " + string.Join(", ", missing)
                    : parseErrors.Count > 0 ? "Invalid JSON: " + string.Join(", ", parseErrors) : "Supported standalone payload schemas required."),
                Result(2, "standalone.payload.project_identity", identityPassed, "Project package ID, title and version must be nonempty."),
                Result(3, "standalone.payload.hash_identity", hashesPassed, "Package and final-state hashes must be nonempty."),
                Result(4, "standalone.payload.runtime_authority", authorityPassed, "Runtime authority flags do not match the standalone contract."),
                Result(5, "standalone.payload.frames_present", framesPresent, "Runtime frames must be nonempty."),
                Result(6, "standalone.payload.frames_contiguous", contiguous, "Frame indices and identity fields must be contiguous and nonempty."),
                Result(7, "standalone.payload.selected_optional_count_mismatch", selectedCountPassed, "Selected optional count must equal selected module ID count."),
                Result(8, "standalone.payload.active_count_mismatch", activeCountPassed, "Active count must equal required plus selected optional."),
                Result(9, "standalone.payload.parameter_count_mismatch", parameterCountPassed, "Configured parameter count must equal effective parameter entries."),
                Result(10, "standalone.payload.human_facts_invalid", factsPassed, "Human review facts must be nonempty and parseable."),
                Result(11, "standalone.payload.frame_cursor_nondeterministic", cursorPassed, "Frame cursor transitions require at least one frame."),
                Result(12, "standalone.payload.damage_invariant_failed", damagePassed, "Equipment and total damage violate the host invariant."),
                Result(13, "standalone.payload.package_hash_mismatch", packageHashPassed, "Actual game-package.json SHA must equal manifest package SHA.")
            };

            var legacy = LegacyCompatibility(
                missing.Count == 0 && parseErrors.Count == 0 ? File.ReadAllText(framesPath) : string.Empty,
                missing.Count == 0 && parseErrors.Count == 0 ? File.ReadAllText(modelPath) : string.Empty,
                frameItems,
                humanFacts);
            var failed = checks.Where(check => !check.Passed).Select(check => check.Code)
                .Concat(legacy.FailedCodes).Distinct(StringComparer.Ordinal).ToList();
            return new ProjectStandalonePayloadSelfCheckResult
            {
                Passed = failed.Count == 0,
                PassedCount = checks.Count(check => check.Passed),
                TotalCount = checks.Count,
                Checks = checks,
                LegacyHostParserCompatibility = legacy,
                FailedCheckCodes = failed
            };
        }
        finally
        {
            manifest?.Dispose();
            model?.Dispose();
            frames?.Dispose();
            launch?.Dispose();
            package?.Dispose();
            buildManifest?.Dispose();
        }
    }

    private static LegacyHostParserCompatibility LegacyCompatibility(
        string framesJson,
        string modelJson,
        IReadOnlyList<JsonElement> structuralFrames,
        IReadOnlyList<JsonElement> structuralFacts)
    {
        var legacyFrames = Regex.Matches(framesJson, FramePattern).Cast<Match>().ToList();
        var legacyFacts = Regex.Matches(modelJson, FactPattern).Cast<Match>().ToList();
        var frameMatch = legacyFrames.Count == structuralFrames.Count
                         && legacyFrames.Select((match, index) =>
                             int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
                             && parsed == Int(structuralFrames[index], "index")
                             && match.Groups[2].Value == String(structuralFrames[index], "title")
                             && match.Groups[3].Value == String(structuralFrames[index], "category")
                             && match.Groups[4].Value == String(structuralFrames[index], "stateHash"))
                             .All(value => value);
        var factsMatch = legacyFacts.Count == structuralFacts.Count
                         && legacyFacts.Select((match, index) =>
                             match.Groups[1].Value == String(structuralFacts[index], "label")
                             && match.Groups[2].Value == String(structuralFacts[index], "value"))
                             .All(value => value);
        var failed = new List<string>();
        if (!frameMatch) failed.Add("standalone.payload.frames_parse_mismatch");
        if (!factsMatch) failed.Add("standalone.payload.human_facts_parse_mismatch");
        return new LegacyHostParserCompatibility
        {
            Passed = failed.Count == 0,
            StructuralFrameCount = structuralFrames.Count,
            LegacyFrameCount = legacyFrames.Count,
            StructuralHumanFactCount = structuralFacts.Count,
            LegacyHumanFactCount = legacyFacts.Count,
            FailedCodes = failed
        };
    }

    private static ProjectStandalonePayloadCheckResult Result(int number, string code, bool passed, string diagnostic) =>
        new() { Number = number, Code = code, Passed = passed, Diagnostic = passed ? string.Empty : diagnostic };

    private static JsonDocument? Parse(string path, ICollection<string> errors)
    {
        try { return JsonDocument.Parse(File.ReadAllText(path)); }
        catch (JsonException) { errors.Add(Path.GetFileName(path)); return null; }
    }

    private static JsonElement? Root(JsonDocument? document) => document?.RootElement;
    private static IReadOnlyList<JsonElement> Array(JsonElement? element, string name) =>
        element is { ValueKind: JsonValueKind.Object } value
        && value.TryGetProperty(name, out var array)
        && array.ValueKind == JsonValueKind.Array
            ? array.EnumerateArray().ToList()
            : [];
    private static string String(JsonDocument? document, string name) => String(Root(document), name);
    private static string String(JsonElement? element, string name) =>
        element is { ValueKind: JsonValueKind.Object } value
        && value.TryGetProperty(name, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    private static bool NonBlank(JsonElement? element, string name) => !string.IsNullOrWhiteSpace(String(element, name));
    private static bool Bool(JsonElement? element, string name) =>
        element is { ValueKind: JsonValueKind.Object } value
        && value.TryGetProperty(name, out var property)
        && property.ValueKind is JsonValueKind.True;
    private static int Int(JsonElement? element, string name) =>
        element is { ValueKind: JsonValueKind.Object } value
        && value.TryGetProperty(name, out var property)
        && property.TryGetInt32(out var result) ? result : 0;
    private static decimal Decimal(JsonElement? element, string name) =>
        element is { ValueKind: JsonValueKind.Object } value
        && value.TryGetProperty(name, out var property)
        && property.TryGetDecimal(out var result) ? result : 0;
    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
