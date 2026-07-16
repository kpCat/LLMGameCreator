using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161Q;

public sealed class Goal161QPayloadSelfCheckTests
{
    [Fact]
    public void Behavioral_valid_payload_passes_all_thirteen_checks_and_legacy_compatibility()
    {
        using var payload = Goal161QPayloadFixture.Create();
        var result = payload.Check();

        Assert.True(result.Passed, string.Join(", ", result.FailedCheckCodes));
        Assert.Equal(13, result.TotalCount);
        Assert.Equal(13, result.PassedCount);
        Assert.True(result.LegacyHostParserCompatibility.Passed);
    }

    [Fact]
    public void Behavioral_missing_payload_file_has_stable_named_failure()
    {
        using var payload = Goal161QPayloadFixture.Create();
        File.Delete(Path.Combine(payload.PayloadRoot, "standalone-launch.json"));

        var result = payload.Check();

        Assert.False(result.Passed);
        Assert.Contains("standalone.payload.file_missing", result.FailedCheckCodes);
    }

    [Fact]
    public void Behavioral_unsupported_schema_has_stable_named_failure()
    {
        using var payload = Goal161QPayloadFixture.Create();
        payload.EditManifest(manifest => manifest["schemaVersion"] = "llmgc_project_standalone_v1");

        var result = payload.Check();

        Assert.False(result.Passed);
        Assert.Contains("standalone.payload.unsupported_schema", result.FailedCheckCodes);
    }

    [Fact]
    public void Behavioral_package_hash_mismatch_is_named()
    {
        using var payload = Goal161QPayloadFixture.Create();
        File.AppendAllText(Path.Combine(payload.PayloadRoot, "game-package.json"), " ");

        var result = payload.Check();

        Assert.Contains("standalone.payload.package_hash_mismatch", result.FailedCheckCodes);
    }

    [Fact]
    public void Behavioral_noncontiguous_frame_index_is_named()
    {
        using var payload = Goal161QPayloadFixture.Create();
        payload.EditFrames(frames => frames[1]!["index"] = 3);

        var result = payload.Check();

        Assert.Contains("standalone.payload.frames_contiguous", result.FailedCheckCodes);
    }

    [Fact]
    public void Behavioral_blank_frame_identity_is_named()
    {
        using var payload = Goal161QPayloadFixture.Create();
        payload.EditFrames(frames => frames[0]!["title"] = "");

        var result = payload.Check();

        Assert.Contains("standalone.payload.frames_contiguous", result.FailedCheckCodes);
    }

    [Fact]
    public void Behavioral_selected_optional_count_mismatch_is_named()
    {
        using var payload = Goal161QPayloadFixture.Create();
        payload.EditManifest(manifest => manifest["selectedOptionalMechanicCount"] = 1);

        var result = payload.Check();

        Assert.Contains("standalone.payload.selected_optional_count_mismatch", result.FailedCheckCodes);
    }

    [Fact]
    public void Behavioral_active_count_mismatch_is_named()
    {
        using var payload = Goal161QPayloadFixture.Create();
        payload.EditManifest(manifest => manifest["activeMechanicCount"] = 99);

        var result = payload.Check();

        Assert.Contains("standalone.payload.active_count_mismatch", result.FailedCheckCodes);
    }

    [Fact]
    public void Behavioral_configured_parameter_count_mismatch_is_named()
    {
        using var payload = Goal161QPayloadFixture.Create();
        payload.EditManifest(manifest => manifest["configuredParameterCount"] = 2);

        var result = payload.Check();

        Assert.Contains("standalone.payload.parameter_count_mismatch", result.FailedCheckCodes);
    }

    [Fact]
    public void Behavioral_no_human_facts_is_named()
    {
        using var payload = Goal161QPayloadFixture.Create();
        payload.EditModel(model => model["humanReviewFacts"] = new JsonArray());

        var result = payload.Check();

        Assert.Contains("standalone.payload.human_facts_invalid", result.FailedCheckCodes);
    }

    [Fact]
    public void Behavioral_escaped_human_fact_exposes_exact_legacy_parser_mismatch()
    {
        using var payload = Goal161QPayloadFixture.Create();
        payload.EditModel(model =>
            model["humanReviewFacts"]![0]!["value"] = "quoted \"fact\"");

        var result = payload.Check();

        Assert.Contains("standalone.payload.human_facts_parse_mismatch", result.FailedCheckCodes);
        Assert.False(result.LegacyHostParserCompatibility.Passed);
    }

    [Fact]
    public void Behavioral_damage_invariant_failure_is_named()
    {
        using var payload = Goal161QPayloadFixture.Create();
        payload.EditModel(model => model["totalAdditionalDamage"] = 1);

        var result = payload.Check();

        Assert.Contains("standalone.payload.damage_invariant_failed", result.FailedCheckCodes);
    }
}

internal sealed class Goal161QPayloadFixture : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private Goal161QPayloadFixture(string root)
    {
        Root = root;
        PayloadRoot = Path.Combine(root, "game-test_Data", "StreamingAssets", "LLMGameCreatorProject");
        BuildManifestPath = Path.Combine(root, "build-manifest.json");
    }

    public string Root { get; }
    public string PayloadRoot { get; }
    public string BuildManifestPath { get; }

    public static Goal161QPayloadFixture Create()
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal161QTests",
            Guid.NewGuid().ToString("N"));
        var fixture = new Goal161QPayloadFixture(root);
        Directory.CreateDirectory(fixture.PayloadRoot);
        var packagePath = Path.Combine(fixture.PayloadRoot, "game-package.json");
        Write(packagePath, new { manifest = new { packageId = "game.test" } });
        var packageSha = Hash(packagePath);
        Write(Path.Combine(fixture.PayloadRoot, "project-manifest.json"), new
        {
            schemaVersion = "llmgc_project_standalone_v2",
            projectPackageId = "game.test",
            projectTitle = "Test game",
            projectVersion = "1.0.0",
            packageSha256 = packageSha,
            finalStateHash = new string('a', 64),
            selectedModuleIds = new[] { "feature.one", "feature.two" },
            effectiveParameters = new[]
            {
                new { moduleId = "feature.one", parameterId = "amount", value = 1 }
            },
            requiredMechanicCount = 1,
            selectedOptionalMechanicCount = 2,
            activeMechanicCount = 3,
            configuredParameterCount = 1,
            runtimeAuthority = true,
            unityGameplayTruth = false,
            projectionOnly = false
        });
        Write(Path.Combine(fixture.PayloadRoot, "player-adapter-model.json"), new
        {
            schemaVersion = "llmgc_player_adapter_model_v2",
            humanReviewFacts = new[]
            {
                new { label = "Игровое сохранение", value = "перенесено" },
                new { label = "Переход между регионами", value = "подтверждён" }
            },
            equipmentDamageBonus = 2,
            statDamageBonus = 2,
            totalAdditionalDamage = 4
        });
        Write(Path.Combine(fixture.PayloadRoot, "player-adapter-frames.json"), new[]
        {
            new { index = 0, title = "Старт", category = "generated_start", stateHash = new string('b', 64) },
            new { index = 1, title = "Переход", category = "generated_travel", stateHash = new string('c', 64) }
        });
        Write(Path.Combine(fixture.PayloadRoot, "standalone-launch.json"), new
        {
            schemaVersion = "llmgc_standalone_launch_v2",
            runtimeAuthority = true,
            unityGameplayTruth = false,
            projectionOnly = false
        });
        Write(fixture.BuildManifestPath, new
        {
            schemaVersion = "llmgc_project_standalone_build_v1",
            projectPackageId = "game.test",
            packageSha256 = packageSha
        });
        return fixture;
    }

    public ProjectStandalonePayloadSelfCheckResult Check() =>
        new ProjectStandalonePayloadSelfCheckService().Check(PayloadRoot, BuildManifestPath);

    public void EditManifest(Action<JsonObject> edit) =>
        Edit(Path.Combine(PayloadRoot, "project-manifest.json"), edit);

    public void EditModel(Action<JsonObject> edit) =>
        Edit(Path.Combine(PayloadRoot, "player-adapter-model.json"), edit);

    public void EditFrames(Action<JsonArray> edit)
    {
        var path = Path.Combine(PayloadRoot, "player-adapter-frames.json");
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsArray();
        edit(root);
        File.WriteAllText(path, root.ToJsonString(JsonOptions), new UTF8Encoding(false));
    }

    public void Dispose()
    {
        if (Directory.Exists(Root)) Directory.Delete(Root, true);
    }

    private static void Edit(string path, Action<JsonObject> edit)
    {
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        edit(root);
        File.WriteAllText(path, root.ToJsonString(JsonOptions), new UTF8Encoding(false));
    }

    private static void Write(string path, object value) =>
        File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions), new UTF8Encoding(false));

    internal static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
