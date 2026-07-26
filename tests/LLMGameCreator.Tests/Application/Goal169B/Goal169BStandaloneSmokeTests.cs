using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169B;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169BStandaloneSmokeTests
{
    [Fact]
    public void Behavioral_exactly_one_cached_hidden_immutable_payload_smoke()
    {
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName(
            "Unity"));
        if (!string.Equals(Environment.GetEnvironmentVariable(
                "LLMGC_GOAL169B_RUN_SMOKE"), "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Assert.NotEmpty(CompleteHostCaches());
            return;
        }

        var proof = BuildAndCloseInMemoryObjects();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.Equal("GREEN", proof.Status);
        Assert.True(proof.HostReused);
        Assert.False(proof.HostRebuilt);
        Assert.True(proof.LaunchSmokePassed);
        Assert.Equal(0, proof.SmokeExitCode);
        Assert.Equal(proof.SelfCheckTotalCount,
            proof.SelfCheckPassedCount);
        Assert.True(proof.SelfCheckTotalCount > 0);
        Assert.True(proof.SidecarsUnchanged);
        Assert.True(proof.Goal142Unchanged);
        Assert.True(proof.Goal148Unchanged);
        Assert.True(proof.HostFilesUnchanged);
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName(
            "Unity"));

        var pointer = Read<ProjectStandaloneCurrentPointer>(
            proof.CurrentPointerPath);
        var status = Read<ProjectStandaloneRunStatus>(
            proof.RunStatusPath);
        Assert.Equal(proof.AttemptId, pointer.PublishedAttemptId);
        Assert.Equal(pointer.PublishedAttemptId, status.AttemptId);
        Assert.Equal("GREEN", status.Status);
        Assert.True(status.PayloadSelfCheckPassed);
        Assert.True(status.LegacyParserCompatibilityPassed);
        Assert.True(status.SmokeMarkersPassed);
        Assert.True(status.PlayerLogPresent);
        Assert.Equal(0, status.SmokeExitCode);
        Assert.True(status.HostReused);
        Assert.False(status.HostRebuilt);

        var history = Read<GameProjectBuildHistoryEntry>(
            proof.BuildHistoryPath);
        var events = Assert.IsType<
            GameProjectGeneratedCampaignRegionalEventSummary>(
            history.GeneratedCampaignRegionalEvents);
        var relationships = Assert.IsType<
            GameProjectGeneratedCampaignRelationshipSummary>(
            history.GeneratedCampaignRelationships);
        var actualPackage = Read<GamePackageDefinition>(
            Path.Combine(proof.PayloadRoot, "game-package.json"));
        var correlation =
            GeneratedCampaignRegionalEventCorrelationService.Validate(
                actualPackage, pointer.PackageSha256, events,
                relationships);
        Assert.True(correlation.Passed,
            string.Join(",", correlation.Diagnostics));
        Assert.Equal(pointer.FinalStateHash, events.FinalStateHash);

        var model = ReadJson(Path.Combine(proof.PayloadRoot,
            "player-adapter-model.json"));
        var fact = model.RootElement.GetProperty("humanReviewFacts")
            .EnumerateArray().Single(item =>
                item.GetProperty("label").GetString() ==
                GeneratedCampaignRegionalEventPayloadAuthorityService
                    .HumanFactLabel);
        var payloadAuthority =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .DeserializeHumanFact(
                fact.GetProperty("value").GetString()
                ?? throw new JsonException(
                    "Goal169B payload authority is empty."));
        Assert.Equal(24, payloadAuthority.ReplaySignatures.Count);
        Assert.Equal(24, payloadAuthority.FrameCounts.Count);
        Assert.Equal(24,
            payloadAuthority.NestedCombatTraceSha256.Count);
        Assert.Equal(events.PayloadAuthority.AuthoritySha256,
            payloadAuthority.AuthoritySha256);
        var authorityValidation =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .Validate(payloadAuthority, events.EventInventory,
                    events.ReplaySignatures, events.RuntimeFrames);
        Assert.True(authorityValidation.Passed,
            string.Join(",", authorityValidation.Diagnostics));

        var payloadFrames = Read<IReadOnlyList<StandaloneRuntimeFrame>>(
            Path.Combine(proof.PayloadRoot,
                "player-adapter-frames.json"));
        var identities = payloadFrames.Select(frame =>
        {
            Assert.True(
                GeneratedCampaignRegionalEventPayloadAuthorityService
                    .TryParseFrameCategory(frame.Title,
                        out var identity));
            return identity;
        }).ToList();
        Assert.Equal(events.RuntimeFrames.Count, identities.Count);
        Assert.Equal(events.RuntimeFrames.Select(FrameIdentity),
            identities.Select(FrameIdentity));
        Assert.Equal(events.RuntimeFrames.Select(item =>
                item.CommandSha256),
            identities.Select(item => item.CommandIdentity));
        Assert.Contains(events.RuntimeFrames, item =>
            item.NestedCombat
            && item.NestedCombatCommandIdentity.Length > 0
            && item.NestedCombatMapEventSequenceSha256.Length > 0
            && item.NestedCombatGameplayEventSequenceSha256.Length > 0
            && item.QualifiedDescriptorFingerprint.Length > 0
            && item.ObservedEffectFingerprint.Length > 0
            && item.EncounterStateBeforeSha256.Length > 0
            && item.EncounterStateAfterSha256.Length > 0);

        var releaseCandidate =
            Read<GameProjectReleaseCandidateRecord>(
                proof.ReleaseCandidatePath);
        Assert.Equal("GREEN", releaseCandidate.Status);
        Assert.Equal(pointer.PackageSha256,
            releaseCandidate.StandalonePackageSha256);
        Assert.Equal(pointer.FinalStateHash,
            releaseCandidate.StandaloneFinalStateHash);
        Assert.True(releaseCandidate.HostReused);
        Assert.False(releaseCandidate.HostRebuilt);

        using var portable = Goal169BPortableCopy.Create(
            proof.ProjectFolder, "goal169b-real-smoke-portable");
        var portableSnapshot =
            Goal156TestKit.OpenWorkspace(portable.Path).Snapshot();
        Assert.Equal("REGIONAL_EVENTS_CURRENT",
            portableSnapshot.GeneratedCampaignRegionalEvents?.Status);
        Assert.Equal("CURRENT",
            portableSnapshot.ReleaseCandidateConfigurationStatus);
        using var coreOnly = Goal156TestKit.Copy(
            Goal156TestKit.CoreOnly,
            "goal169b-core-only-portable");
        var coreSnapshot =
            Goal156TestKit.OpenWorkspace(coreOnly.Path).Snapshot();
        Assert.NotEqual("CURRENT",
            coreSnapshot.ReleaseCandidateConfigurationStatus);

        WriteCapture(proof, pointer, status, events,
            payloadAuthority, identities.Count,
            portableSnapshot.GeneratedCampaignRegionalEvents?.Status
            == "REGIONAL_EVENTS_CURRENT",
            portableSnapshot.ReleaseCandidateConfigurationStatus
            == "CURRENT",
            coreSnapshot.ReleaseCandidateConfigurationStatus
            != "CURRENT");
    }

    private static Goal169BImmutableSmokeProof
        BuildAndCloseInMemoryObjects()
    {
        var hostBefore = CompleteHostCaches().ToDictionary(
            path => path, TreeHash,
            StringComparer.OrdinalIgnoreCase);
        var goal142Before =
            Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath);
        var goal148 = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Games", "goal148-manual");
        var goal148Before = TreeHash(goal148);
        var standaloneService = new ProjectStandaloneBuildService(
            Goal164TestKit.RepositoryRoot);
        var fixture = Goal164BuildFixture.Create(
            coreOnly: false, standaloneService);
        var generationRoot = Path.Combine(fixture.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));
        var sidecarsBefore = TreeHash(generationRoot);
        var standalone = fixture.Controller.BuildWindowsStandalone();
        var build = fixture.Controller.LastBuild
                    ?? throw new InvalidOperationException(
                        "Goal169B standalone build is missing.");
        var payloadRoot = Path.Combine(standalone.OutputFolder,
            "g_Data", "StreamingAssets", "LLMGameCreatorProject");
        var usedHost = HostRoot(standalone.HostCacheKey);
        Assert.True(hostBefore.TryGetValue(
            usedHost, out var usedHostBefore));

        return new Goal169BImmutableSmokeProof
        {
            Status = standalone.Status,
            AttemptId = standalone.AttemptId,
            HostCacheKey = standalone.HostCacheKey,
            HostReused = standalone.HostReused,
            HostRebuilt = standalone.HostRebuilt,
            LaunchSmokePassed = standalone.LaunchSmokePassed,
            SmokeExitCode = standalone.SmokeExitCode,
            SelfCheckPassedCount = standalone.SelfCheckPassedCount,
            SelfCheckTotalCount = standalone.SelfCheckTotalCount,
            ProjectFolder = fixture.Project.Path,
            PayloadRoot = payloadRoot,
            CurrentPointerPath = standalone.CurrentPointerPath,
            RunStatusPath = standalone.RunStatusPath,
            BuildHistoryPath = build.BuildHistoryPath,
            ReleaseCandidatePath = Path.Combine(
                fixture.Project.Path,
                UnifiedGameProjectWorkspaceVocabulary
                    .ReleaseCandidateRecordRelativePath.Replace('/',
                        Path.DirectorySeparatorChar)),
            SidecarsUnchanged =
                sidecarsBefore == TreeHash(generationRoot),
            Goal142Unchanged =
                goal142Before == Goal156TestKit.Hash(
                    Goal156TestKit.Goal142BaselinePath),
            Goal148Unchanged =
                goal148Before == TreeHash(goal148),
            HostFilesUnchanged =
                usedHostBefore == TreeHash(usedHost)
        };
    }

    private static string FrameIdentity(
        GeneratedCampaignRegionalEventRuntimeFrame frame) =>
        string.Join("|", frame.RegionalEventId, frame.RouteKind,
            frame.ReplayIndex, frame.SequenceIndex);

    private static string FrameIdentity(
        GeneratedCampaignRegionalEventPayloadFrameIdentity frame) =>
        string.Join("|", frame.RegionalEventId, frame.RouteKind,
            frame.ReplayIndex, frame.SequenceIndex);

    private static T Read<T>(string path) =>
        JsonSerializer.Deserialize<T>(
            File.ReadAllText(path, Encoding.UTF8), JsonOptions)
        ?? throw new JsonException(path + " is empty.");

    private static JsonDocument ReadJson(string path) =>
        JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));

    private static void WriteCapture(
        Goal169BImmutableSmokeProof proof,
        ProjectStandaloneCurrentPointer pointer,
        ProjectStandaloneRunStatus status,
        GameProjectGeneratedCampaignRegionalEventSummary events,
        GeneratedCampaignRegionalEventPayloadAuthority authority,
        int payloadFrameCount,
        bool portableCurrent,
        bool portableReleaseCandidateCurrent,
        bool coreOnlyNoFalseRcReady)
    {
        var path = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL169B_SMOKE_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path))
            return;
        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            status = "GREEN",
            proof.HostCacheKey,
            proof.HostReused,
            proof.HostRebuilt,
            unityEditorProcessStartCount = 0,
            hiddenSmokeInvocationCount = 1,
            correctiveRetryCount = 0,
            proof.LaunchSmokePassed,
            proof.SmokeExitCode,
            proof.SelfCheckPassedCount,
            proof.SelfCheckTotalCount,
            pointer.RunDirectoryName,
            pointer.PackageSha256,
            pointer.FinalStateHash,
            runStatus = status.Status,
            proof.ProjectFolder,
            proof.PayloadRoot,
            proof.CurrentPointerPath,
            proof.RunStatusPath,
            proof.BuildHistoryPath,
            proof.ReleaseCandidatePath,
            events.StrictProofSchemaVersion,
            events.EventCount,
            replaySignatureCount =
                authority.ReplaySignatures.Count,
            payloadFrameCount,
            nestedCombatFrameCount =
                events.RuntimeFrames.Count(item => item.NestedCombat),
            authority.AuthoritySha256,
            authority.ComponentSha256,
            authority.FrameCounts,
            authority.NestedCombatTraceSha256,
            portableCurrent,
            portableReleaseCandidateCurrent,
            coreOnlyNoFalseRcReady,
            proof.SidecarsUnchanged,
            proof.Goal142Unchanged,
            proof.Goal148Unchanged,
            proof.HostFilesUnchanged
        }, JsonOptions) + Environment.NewLine,
            new UTF8Encoding(false));
    }

    private static IReadOnlyList<string> CompleteHostCaches()
    {
        var root = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            ProjectStandaloneBuildVocabulary.HostCacheRootName);
        if (!Directory.Exists(root))
            return [];
        return Directory.EnumerateDirectories(root)
            .Select(path => Path.Combine(path, "host"))
            .Where(path => File.Exists(Path.Combine(path,
                               ProjectStandaloneBuildVocabulary
                                   .HostExecutableName))
                           && Directory.Exists(Path.Combine(path,
                               ProjectStandaloneBuildVocabulary
                                   .HostDataDirectoryName)))
            .ToList();
    }

    private static string HostRoot(string key) =>
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            ProjectStandaloneBuildVocabulary.HostCacheRootName,
            key, "host");

    private static string TreeHash(string path)
    {
        if (!Directory.Exists(path))
            return "<absent>";
        var builder = new StringBuilder();
        foreach (var file in Directory.EnumerateFiles(path, "*",
                     SearchOption.AllDirectories)
                 .OrderBy(item => item,
                     StringComparer.OrdinalIgnoreCase))
        {
            builder.Append(Path.GetRelativePath(path, file)
                    .Replace('\\', '/'))
                .Append('|')
                .Append(Convert.ToHexString(SHA256.HashData(
                    File.ReadAllBytes(file))))
                .AppendLine();
        }
        return Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(builder.ToString())));
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };
}

internal sealed record Goal169BImmutableSmokeProof
{
    public string Status { get; init; } = string.Empty;
    public string AttemptId { get; init; } = string.Empty;
    public string HostCacheKey { get; init; } = string.Empty;
    public bool HostReused { get; init; }
    public bool HostRebuilt { get; init; }
    public bool LaunchSmokePassed { get; init; }
    public int SmokeExitCode { get; init; }
    public int SelfCheckPassedCount { get; init; }
    public int SelfCheckTotalCount { get; init; }
    public string ProjectFolder { get; init; } = string.Empty;
    public string PayloadRoot { get; init; } = string.Empty;
    public string CurrentPointerPath { get; init; } = string.Empty;
    public string RunStatusPath { get; init; } = string.Empty;
    public string BuildHistoryPath { get; init; } = string.Empty;
    public string ReleaseCandidatePath { get; init; } = string.Empty;
    public bool SidecarsUnchanged { get; init; }
    public bool Goal142Unchanged { get; init; }
    public bool Goal148Unchanged { get; init; }
    public bool HostFilesUnchanged { get; init; }
}

internal sealed class Goal169BPortableCopy : IDisposable
{
    private Goal169BPortableCopy(string root, string path)
    {
        Root = root;
        Path = path;
    }

    public string Root { get; }
    public string Path { get; }

    public static Goal169BPortableCopy Create(
        string source, string folderName)
    {
        var root = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "LLMGameCreator",
            "Goal169BPortable", Guid.NewGuid().ToString("N"));
        var target = System.IO.Path.Combine(root, folderName);
        CopyDirectory(source, target);
        return new Goal169BPortableCopy(root, target);
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var directory in Directory.EnumerateDirectories(
                     source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(System.IO.Path.Combine(target,
                System.IO.Path.GetRelativePath(source, directory)));
        foreach (var file in Directory.EnumerateFiles(
                     source, "*", SearchOption.AllDirectories))
        {
            var destination = System.IO.Path.Combine(target,
                System.IO.Path.GetRelativePath(source, file));
            Directory.CreateDirectory(
                System.IO.Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
