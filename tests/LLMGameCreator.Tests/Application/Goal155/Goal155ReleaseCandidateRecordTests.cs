using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal155;

public sealed class Goal155ReleaseCandidateRecordTests
{
    [Fact]
    public void Behavioral_successful_correlated_payload_writes_atomic_current_record()
    {
        using var fixture = Goal155RcFixture.Create("write-current");
        var record = fixture.Write();
        var read = fixture.Read();
        Assert.Equal("GREEN", record.Status);
        Assert.Equal("CURRENT", read.ConfigurationStatus);
        Assert.NotNull(read.Record);
        Assert.True(File.Exists(fixture.Service.RecordPath(fixture.Project)));
        Assert.Empty(Directory.EnumerateFiles(Path.GetDirectoryName(fixture.Service.RecordPath(fixture.Project))!, "*.tmp-*"));
    }

    [Fact]
    public void Behavioral_saved_semantic_change_is_last_success_and_return_is_current_without_write()
    {
        using var fixture = Goal155RcFixture.Create("last-success-return");
        fixture.Write();
        var changed = fixture.Document with
        {
            SelectedModuleIds = ["feature.profile.alchemy_focus"]
        };
        Assert.Equal("LAST_SUCCESS", fixture.Read(changed).ConfigurationStatus);
        Assert.Equal("CURRENT", fixture.Read().ConfigurationStatus);
    }

    [Fact]
    public void Behavioral_failed_standalone_cannot_replace_last_success_record()
    {
        using var fixture = Goal155RcFixture.Create("failed-standalone");
        fixture.Write();
        var path = fixture.Service.RecordPath(fixture.Project);
        var before = File.ReadAllBytes(path);
        Assert.Throws<InvalidOperationException>(() => fixture.Service.Write(
            fixture.Project, fixture.Identity, fixture.Build, fixture.Standalone with { Status = "FAILED" }));
        Assert.Equal(before, File.ReadAllBytes(path));
    }

    [Fact]
    public void Behavioral_malformed_record_is_rejected_causally_and_left_untouched()
    {
        using var fixture = Goal155RcFixture.Create("malformed");
        var path = fixture.Service.RecordPath(fixture.Project);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{broken", new UTF8Encoding(false));
        var before = File.ReadAllBytes(path);
        var read = fixture.Read();
        Assert.Null(read.Record);
        Assert.Equal("ABSENT", read.ConfigurationStatus);
        Assert.Contains("rc.read.invalid_json", read.Diagnostics);
        Assert.Equal(before, File.ReadAllBytes(path));
    }

    [Theory]
    [InlineData("schema")]
    [InlineData("status")]
    [InlineData("standalone_hash")]
    [InlineData("host_rebuilt")]
    public void Behavioral_unsupported_or_mismatched_records_are_rejected_causally(string mutation)
    {
        using var fixture = Goal155RcFixture.Create("reject-" + mutation);
        fixture.Write();
        var path = fixture.Service.RecordPath(fixture.Project);
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        switch (mutation)
        {
            case "schema": root["schemaVersion"] = "unsupported"; break;
            case "status": root["status"] = "FAILED"; break;
            case "standalone_hash": root["standalonePackageSha256"] = new string('f', 64); break;
            case "host_rebuilt": root["hostRebuilt"] = true; break;
        }
        File.WriteAllText(path, root.ToJsonString(), new UTF8Encoding(false));
        var read = fixture.Read();
        Assert.Null(read.Record);
        Assert.Equal("ABSENT", read.ConfigurationStatus);
        Assert.NotEmpty(read.Diagnostics);
    }

    [Fact]
    public void Behavioral_actual_player_adapter_payload_hash_mismatch_is_rejected()
    {
        using var fixture = Goal155RcFixture.Create("payload-mismatch");
        fixture.Write();
        File.AppendAllText(fixture.PlayerAdapterModelPath, " ", new UTF8Encoding(false));
        var read = fixture.Read();
        Assert.Null(read.Record);
        Assert.Contains("rc.read.player_adapter_model_hash_mismatch", read.Diagnostics);
    }

    [Fact]
    public void Behavioral_complete_project_copy_restores_current_without_build_or_standalone()
    {
        using var fixture = Goal155RcFixture.Create("portable-source");
        fixture.Write();
        using var copy = Goal155RcFixture.CopyOf(fixture, "portable-copy");
        var read = copy.Read();
        Assert.Equal("CURRENT", read.ConfigurationStatus);
        Assert.Equal(fixture.Read().Record?.AcceptedMechanicsSummary.HumanFacts,
            read.Record?.AcceptedMechanicsSummary.HumanFacts);
        Assert.False(copy.BuildOrStandaloneExecuted);
    }

    [Fact]
    public void Behavioral_absent_record_reports_absent_without_creating_files()
    {
        using var fixture = Goal155RcFixture.Create("absent");
        var path = fixture.Service.RecordPath(fixture.Project);
        var read = fixture.Read();
        Assert.Equal("ABSENT", read.ConfigurationStatus);
        Assert.Null(read.Record);
        Assert.False(File.Exists(path));
    }
}

internal sealed class Goal155RcFixture : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private Goal155RcFixture(
        string root,
        string project,
        FeatureModuleLibrarySnapshot library,
        FeatureModuleCompositionDocument document,
        GameProjectIdentityDocument identity,
        GameProjectBuildResult build,
        ProjectStandaloneBuildResult standalone,
        string playerAdapterModelPath)
    {
        Root = root;
        Project = project;
        Library = library;
        Document = document;
        Identity = identity;
        Build = build;
        Standalone = standalone;
        PlayerAdapterModelPath = playerAdapterModelPath;
    }

    public string Root { get; }
    public string Project { get; }
    public FeatureModuleLibrarySnapshot Library { get; }
    public FeatureModuleCompositionDocument Document { get; }
    public GameProjectIdentityDocument Identity { get; }
    public GameProjectBuildResult Build { get; }
    public ProjectStandaloneBuildResult Standalone { get; }
    public string PlayerAdapterModelPath { get; }
    public GameProjectReleaseCandidateRecordService Service { get; } = new();
    public bool BuildOrStandaloneExecuted { get; private init; }

    public static Goal155RcFixture Create(string name)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Goal155Tests", Guid.NewGuid().ToString("N"));
        var project = Path.Combine(root, name);
        Directory.CreateDirectory(project);
        var repositoryRoot = Goal155HumanAcceptanceLedgerTests.Root();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(repositoryRoot, "catalogs", "feature-modules"));
        var document = new FeatureModuleCompositionDocument
        {
            CompositionId = "goal155-record-fixture",
            BaseCandidateId = "minimal-map-game-balanced-baseline"
        };
        var fingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(document, library);
        Assert.True(fingerprint.Passed, string.Join(";", fingerprint.Diagnostics));
        var packagePath = Path.Combine(project, "package.json");
        File.WriteAllText(packagePath, "{}" + Environment.NewLine, new UTF8Encoding(false));
        var packageSha = HashFile(packagePath);
        var compositionSha = new string('b', 64);
        var finalHash = new string('c', 64);
        document = document with
        {
            LastActivatedProjectPackageSha256 = packageSha,
            LastCompositionPackageSha256 = compositionSha,
            LastQualifiedFinalStateHash = finalHash,
            LastQualificationStatus = "GREEN"
        };
        var sourceBuild = Goal155AcceptedMechanicsProjectionTests.Complete() with
        {
            QualifiedAuthoringFingerprint = fingerprint.Sha256,
            PackageSha256 = packageSha,
            ActivatedProjectPackageSha256 = packageSha,
            CompositionPackageSha256 = compositionSha,
            FinalStateHash = finalHash
        };
        var summaryService = new GameProjectAcceptedMechanicsSummaryService();
        var build = sourceBuild with { AcceptedMechanics = summaryService.Project(sourceBuild) };
        var identity = new GameProjectIdentityDocument
        {
            PackageId = "game/goal155",
            Title = "Goal155 RC",
            Version = "1.0.0",
            FormatVersion = "1.0"
        };
        var slug = "game-goal155";
        var payloadRoot = Path.Combine(project, "Builds", "Windows", slug, slug + "_Data",
            "StreamingAssets", "LLMGameCreatorProject");
        Directory.CreateDirectory(payloadRoot);
        var facts = summaryService.StandaloneHumanFacts(build, includeReleaseCandidateReady: true);
        File.WriteAllText(Path.Combine(payloadRoot, "project-manifest.json"), JsonSerializer.Serialize(new
        {
            packageSha256 = packageSha,
            compositionPackageSha256 = compositionSha,
            finalStateHash = finalHash
        }, JsonOptions), new UTF8Encoding(false));
        var playerAdapterModelPath = Path.Combine(payloadRoot, "player-adapter-model.json");
        File.WriteAllText(playerAdapterModelPath, JsonSerializer.Serialize(new
        {
            schemaVersion = "llmgc_player_adapter_model_v2",
            humanReviewFacts = facts,
            finalStateHash = finalHash
        }, JsonOptions), new UTF8Encoding(false));
        var standalone = new ProjectStandaloneBuildResult
        {
            Status = "GREEN",
            PackageSha256 = packageSha,
            FinalStateHash = finalHash,
            HostCacheKey = "goal155-test-host",
            HostReused = true,
            HostRebuilt = false,
            LaunchSmokePassed = true,
            SelfCheckPassedCount = 5,
            SelfCheckTotalCount = 5,
            ProjectFolder = project,
            OutputFolder = Path.Combine(project, "Builds", "Windows", slug),
            ExecutablePath = Path.Combine(project, "Builds", "Windows", slug, slug + ".exe")
        };
        return new Goal155RcFixture(root, project, library, document, identity, build, standalone,
            playerAdapterModelPath);
    }

    public static Goal155RcFixture CopyOf(Goal155RcFixture source, string name)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Goal155Tests", Guid.NewGuid().ToString("N"));
        var project = Path.Combine(root, name);
        CopyDirectory(source.Project, project);
        var playerAdapterModelPath = source.PlayerAdapterModelPath.Replace(source.Project, project, StringComparison.Ordinal);
        return new Goal155RcFixture(root, project, source.Library, source.Document, source.Identity,
            source.Build, source.Standalone with { ProjectFolder = project }, playerAdapterModelPath);
    }

    public GameProjectReleaseCandidateRecord Write() => Service.Write(Project, Identity, Build, Standalone);
    public GameProjectReleaseCandidateReadResult Read(
        FeatureModuleCompositionDocument? document = null,
        GameProjectIdentityDocument? identity = null) => Service.Read(new GameProjectReleaseCandidateReadRequest
    {
        ProjectFolder = Project,
        Document = document ?? Document,
        Library = Library,
        Identity = identity ?? Identity
    });

    public void RewriteRecord(Action<JsonObject> change)
    {
        var path = Service.RecordPath(Project);
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        change(root);
        File.WriteAllText(path, root.ToJsonString(), new UTF8Encoding(false));
    }

    public void RemovePayload()
    {
        var builds = Path.Combine(Project, "Builds");
        if (Directory.Exists(builds)) Directory.Delete(builds, true);
    }

    public void Dispose()
    {
        if (Directory.Exists(Root)) Directory.Delete(Root, true);
    }

    private static void CopyDirectory(string source, string target)
    {
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, true);
        }
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
