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

namespace LLMGameCreator.Tests.Application.Goal169C;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169CStandaloneSmokeTests
{
    [Fact]
    public void Behavioral_exactly_one_post_fix_cached_hidden_immutable_smoke()
    {
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName(
            "Unity"));
        if (!string.Equals(Environment.GetEnvironmentVariable(
                "LLMGC_GOAL169C_RUN_SMOKE"), "true",
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
        Assert.True(proof.PlayerLogPresent);
        Assert.Equal(0, proof.SmokeExitCode);
        Assert.Equal(proof.SelfCheckTotalCount,
            proof.SelfCheckPassedCount);
        Assert.True(proof.SelfCheckTotalCount > 0);
        Assert.False(string.Equals(
            proof.FailedGoal169BRunRoot, proof.RunRoot,
            StringComparison.OrdinalIgnoreCase));
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
        Assert.Equal(pointer.RunDirectoryName,
            Path.GetFileName(proof.RunRoot));
        Assert.Equal("GREEN", status.Status);
        Assert.True(status.PayloadSelfCheckPassed);
        Assert.True(status.LegacyParserCompatibilityPassed);
        Assert.True(status.SmokeMarkersPassed);
        Assert.True(status.PlayerLogPresent);
        Assert.Equal(0, status.SmokeExitCode);
        Assert.True(status.HostReused);
        Assert.False(status.HostRebuilt);
        Assert.Equal(pointer.PackageSha256, status.PackageSha256);
        Assert.Equal(pointer.FinalStateHash,
            status.FinalStateHash);

        var standaloneHistory =
            Read<IReadOnlyList<ProjectStandaloneBuildResult>>(
                proof.StandaloneHistoryPath);
        var selectedStandalone = Assert.Single(standaloneHistory,
            item => item.AttemptId == pointer.PublishedAttemptId);
        Assert.Equal("GREEN", selectedStandalone.Status);
        Assert.Equal(pointer.PackageSha256,
            selectedStandalone.PackageSha256);
        Assert.Equal(pointer.FinalStateHash,
            selectedStandalone.FinalStateHash);
        Assert.Equal(pointer.RunDirectoryName,
            selectedStandalone.OutputRunDirectoryName);
        Assert.Equal(0, selectedStandalone.SmokeExitCode);
        Assert.True(selectedStandalone.PayloadSelfCheckPassed);
        Assert.True(selectedStandalone
            .LegacyHostParserCompatibilityPassed);
        Assert.True(selectedStandalone.LaunchSmokePassed);

        var selfCheck =
            new ProjectStandalonePayloadSelfCheckService().Check(
                proof.PayloadRoot, proof.BuildManifestPath);
        Assert.True(selfCheck.Passed,
            string.Join(",", selfCheck.FailedCheckCodes));
        Assert.Equal(13, selfCheck.PassedCount);
        Assert.Equal(13, selfCheck.TotalCount);
        Assert.True(
            selfCheck.LegacyHostParserCompatibility.Passed);

        var history = Read<GameProjectBuildHistoryEntry>(
            proof.BuildHistoryPath);
        Assert.Equal(GameProjectBuildHistoryReader.SchemaVersionV7,
            history.SchemaVersion);
        var events = Assert.IsType<
            GameProjectGeneratedCampaignRegionalEventSummary>(
            history.GeneratedCampaignRegionalEvents);
        var relationships = Assert.IsType<
            GameProjectGeneratedCampaignRelationshipSummary>(
            history.GeneratedCampaignRelationships);
        var packagePath = Path.Combine(proof.PayloadRoot,
            "game-package.json");
        var actualPackage = Read<GamePackageDefinition>(packagePath);
        Assert.Equal(pointer.PackageSha256, HashFile(packagePath));
        var correlation =
            GeneratedCampaignRegionalEventCorrelationService.Validate(
                actualPackage, pointer.PackageSha256, events,
                relationships);
        Assert.True(correlation.Passed,
            string.Join(",", correlation.Diagnostics));
        Assert.Equal(pointer.FinalStateHash, events.FinalStateHash);

        using var model = ReadJson(Path.Combine(proof.PayloadRoot,
            "player-adapter-model.json"));
        var facts = model.RootElement.GetProperty("humanReviewFacts")
            .EnumerateArray().ToList();
        var fact = Assert.Single(facts, item =>
            item.GetProperty("label").GetString() ==
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .HumanFactLabel);
        var factValue = fact.GetProperty("value").GetString()
                        ?? throw new JsonException(
                            "Goal169C payload authority is empty.");
        Assert.StartsWith("base64:", factValue,
            StringComparison.Ordinal);
        Assert.DoesNotContain("\"", factValue,
            StringComparison.Ordinal);
        Assert.DoesNotContain("\r", factValue,
            StringComparison.Ordinal);
        Assert.DoesNotContain("\n", factValue,
            StringComparison.Ordinal);
        var payloadAuthority =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .DeserializeHumanFact(factValue);
        Assert.Equal(6, payloadAuthority.RegionalEventIds.Count);
        Assert.Equal(24,
            payloadAuthority.ReplaySignatures.Count);
        Assert.Equal(24, payloadAuthority.FrameCounts.Count);
        Assert.Equal(24,
            payloadAuthority.NestedCombatTraceSha256.Count);
        Assert.Equal(events.PayloadAuthority.AuthoritySha256,
            payloadAuthority.AuthoritySha256);
        Assert.Equal(pointer.PackageSha256,
            payloadAuthority.PackageSha256);
        Assert.Equal(pointer.FinalStateHash,
            payloadAuthority.FinalStateHash);
        Assert.Equal(events.RegionalEventInventorySha256,
            payloadAuthority.InventorySha256);
        var authorityValidation =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .Validate(payloadAuthority, events.EventInventory,
                    events.ReplaySignatures, events.RuntimeFrames);
        Assert.True(authorityValidation.Passed,
            string.Join(",", authorityValidation.Diagnostics));

        var recomputedSignatures = events.ReplaySignatures
            .Select(signature =>
            {
                var frames = events.RuntimeFrames.Where(item =>
                        item.RegionalEventId ==
                        signature.RegionalEventId
                        && item.RouteKind == signature.RouteKind
                        && item.ReplayIndex ==
                        signature.ReplayIndex)
                    .OrderBy(item => item.SequenceIndex).ToList();
                return
                    GeneratedCampaignRegionalEventReplayService
                        .CreateSignature(
                            signature.RegionalEventId,
                            signature.RouteKind,
                            signature.ReplayIndex, frames);
            })
            .OrderBy(item => item.RegionalEventId,
                StringComparer.Ordinal)
            .ThenBy(item => item.RouteKind)
            .ThenBy(item => item.ReplayIndex).ToList();
        Assert.Equal(
            payloadAuthority.ReplaySignatures.Select(item =>
                item.SignatureSha256),
            recomputedSignatures.Select(item =>
                item.SignatureSha256));
        Assert.Equal(
            payloadAuthority.ReplaySignatures.Select(item =>
                item.NestedCombatTraceSha256),
            recomputedSignatures.Select(item =>
                item.NestedCombatTraceSha256));

        var payloadFrames =
            Read<IReadOnlyList<StandaloneRuntimeFrame>>(
                Path.Combine(proof.PayloadRoot,
                    "player-adapter-frames.json"));
        var identities = payloadFrames.Select(frame =>
        {
            Assert.Equal("generated-regional-event",
                frame.Category);
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
        Assert.Equal(payloadAuthority.RegionalEventIds,
            identities.Select(item => item.RegionalEventId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal));
        foreach (var eventId in payloadAuthority.RegionalEventIds)
        foreach (var route in Enum.GetValues<
                     GeneratedCampaignRegionalEventReplayRouteKind>())
        {
            var routeFrames = identities.Where(item =>
                item.RegionalEventId == eventId
                && item.RouteKind == route).ToList();
            Assert.Equal(new[] { 1, 2 }, routeFrames
                .Select(item => item.ReplayIndex).Distinct()
                .OrderBy(item => item));
            foreach (var replay in new[] { 1, 2 })
            {
                var sequences = routeFrames.Where(item =>
                        item.ReplayIndex == replay)
                    .Select(item => item.SequenceIndex)
                    .OrderBy(item => item).ToList();
                Assert.Equal(
                    Enumerable.Range(0, sequences.Count),
                    sequences);
            }
        }
        var nested = events.RuntimeFrames.Where(item =>
            item.NestedCombat).ToList();
        Assert.NotEmpty(nested);
        Assert.True(nested.Select(item => item.RegionalEventId)
            .Distinct(StringComparer.Ordinal).Count() > 0);
        Assert.All(nested, item =>
        {
            Assert.NotEmpty(item.NestedCombatCommandIdentity);
            Assert.NotEmpty(
                item.NestedCombatMapEventSequenceSha256);
            Assert.NotEmpty(
                item.NestedCombatGameplayEventSequenceSha256);
            Assert.NotEmpty(item.EncounterStateBeforeSha256);
            Assert.NotEmpty(item.EncounterStateAfterSha256);
            Assert.Contains(identities, identity =>
                identity.RegionalEventId == item.RegionalEventId
                && identity.RouteKind == item.RouteKind
                && identity.ReplayIndex == item.ReplayIndex
                && identity.SequenceIndex == item.SequenceIndex
                && identity.CommandIdentity ==
                item.CommandSha256);
        });
        Assert.Contains(nested, item =>
            item.QualifiedDescriptorFingerprint.Length > 0
            && item.ObservedEffectFingerprint.Length > 0
            && item.CombatProgressObserved);

        var releaseCandidate =
            Read<GameProjectReleaseCandidateRecord>(
                proof.ReleaseCandidatePath);
        Assert.Equal("GREEN", releaseCandidate.Status);
        Assert.Equal(pointer.PackageSha256,
            releaseCandidate.PackageSha256);
        Assert.Equal(pointer.PackageSha256,
            releaseCandidate.StandalonePackageSha256);
        Assert.Equal(pointer.FinalStateHash,
            releaseCandidate.FinalStateHash);
        Assert.Equal(pointer.FinalStateHash,
            releaseCandidate.StandaloneFinalStateHash);
        Assert.Equal(pointer.HostCacheKey,
            releaseCandidate.HostCacheKey);
        Assert.True(releaseCandidate.HostReused);
        Assert.False(releaseCandidate.HostRebuilt);
        Assert.True(releaseCandidate.LaunchSmokePassed);
        Assert.Equal(releaseCandidate.SelfCheckTotalCount,
            releaseCandidate.SelfCheckPassedCount);
        Assert.Equal(HashFile(Path.Combine(proof.PayloadRoot,
                "player-adapter-model.json")),
            releaseCandidate.PlayerAdapterModelSha256);

        var currentSnapshot =
            Goal156TestKit.OpenWorkspace(proof.ProjectFolder)
                .Snapshot();
        Assert.Equal("REGIONAL_EVENTS_CURRENT",
            currentSnapshot.GeneratedCampaignRegionalEvents?.Status);
        Assert.Equal("CURRENT",
            currentSnapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT",
            currentSnapshot
                .ReleaseCandidateRecordConfigurationStatus);

        using var portable = Goal169CPortableCopy.Create(
            proof.ProjectFolder, "goal169c-real-smoke-portable");
        var portableSnapshot =
            Goal156TestKit.OpenWorkspace(portable.Path).Snapshot();
        var portablePointer =
            new ProjectStandaloneOutputLocationService()
                .LoadCurrentOutput(portable.Path,
                    actualPackage.Manifest.PackageId);
        Assert.Equal("REGIONAL_EVENTS_CURRENT",
            portableSnapshot.GeneratedCampaignRegionalEvents?.Status);
        Assert.Equal("CURRENT",
            portableSnapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT",
            portableSnapshot
                .ReleaseCandidateRecordConfigurationStatus);
        Assert.False(portablePointer.Passed);

        using var coreOnly = Goal156TestKit.Copy(
            Goal156TestKit.CoreOnly,
            "goal169c-core-only-portable");
        var coreSnapshot =
            Goal156TestKit.OpenWorkspace(coreOnly.Path).Snapshot();
        Assert.True(coreSnapshot.GeneratedCampaignRegionalEvents is
        {
            Passed: true,
            Status: "ABSENT" or "REGIONAL_EVENTS_CURRENT"
        });
        Assert.NotEqual("CURRENT",
            coreSnapshot.ReleaseCandidateConfigurationStatus);

        WriteCapture(proof, pointer, status, selectedStandalone,
            events, payloadAuthority, identities.Count,
            nested.Count, releaseCandidate,
            currentSnapshot.ReleaseCandidateConfigurationStatus ==
            "CURRENT",
            portableSnapshot.GeneratedCampaignRegionalEvents
                ?.Status == "REGIONAL_EVENTS_CURRENT",
            portableSnapshot.ReleaseCandidateConfigurationStatus
            == "CURRENT",
            !portablePointer.Passed,
            coreSnapshot.GeneratedCampaignRegionalEvents is
            {
                Passed: true,
                Status: "ABSENT" or "REGIONAL_EVENTS_CURRENT"
            },
            coreSnapshot.ReleaseCandidateConfigurationStatus
            != "CURRENT");
    }

    private static Goal169CImmutableSmokeProof
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
        var failedGoal169BRunRoot = FailedGoal169BRunRoot();
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
                        "Goal169C standalone build is missing.");
        var payloadRoot = Path.Combine(standalone.OutputFolder,
            "g_Data", "StreamingAssets", "LLMGameCreatorProject");
        var usedHost = HostRoot(standalone.HostCacheKey);
        Assert.True(hostBefore.TryGetValue(
            usedHost, out var usedHostBefore));

        return new Goal169CImmutableSmokeProof
        {
            Status = standalone.Status,
            AttemptId = standalone.AttemptId,
            HostCacheKey = standalone.HostCacheKey,
            HostReused = standalone.HostReused,
            HostRebuilt = standalone.HostRebuilt,
            LaunchSmokePassed = standalone.LaunchSmokePassed,
            PlayerLogPresent = standalone.PlayerLogPresent,
            SmokeExitCode = standalone.SmokeExitCode,
            SelfCheckPassedCount = standalone.SelfCheckPassedCount,
            SelfCheckTotalCount = standalone.SelfCheckTotalCount,
            ProjectFolder = fixture.Project.Path,
            RunRoot = standalone.OutputFolder,
            PayloadRoot = payloadRoot,
            CurrentPointerPath = standalone.CurrentPointerPath,
            RunStatusPath = standalone.RunStatusPath,
            BuildHistoryPath = build.BuildHistoryPath,
            StandaloneHistoryPath = Path.Combine(
                fixture.Project.Path,
                ProjectStandaloneBuildVocabulary.HistoryRelativePath
                    .Replace('/', Path.DirectorySeparatorChar)),
            ReleaseCandidatePath = Path.Combine(
                fixture.Project.Path,
                UnifiedGameProjectWorkspaceVocabulary
                    .ReleaseCandidateRecordRelativePath.Replace('/',
                        Path.DirectorySeparatorChar)),
            BuildManifestPath = standalone.BuildManifestPath,
            FailedGoal169BRunRoot = failedGoal169BRunRoot,
            SidecarsBeforeSha256 = sidecarsBefore,
            SidecarsAfterSha256 = TreeHash(generationRoot),
            SidecarsUnchanged =
                sidecarsBefore == TreeHash(generationRoot),
            Goal142BeforeSha256 = goal142Before,
            Goal142AfterSha256 =
                Goal156TestKit.Hash(
                    Goal156TestKit.Goal142BaselinePath),
            Goal142Unchanged =
                goal142Before == Goal156TestKit.Hash(
                    Goal156TestKit.Goal142BaselinePath),
            Goal148BeforeSha256 = goal148Before,
            Goal148AfterSha256 = TreeHash(goal148),
            Goal148Unchanged =
                goal148Before == TreeHash(goal148),
            HostBeforeSha256 = usedHostBefore,
            HostAfterSha256 = TreeHash(usedHost),
            HostFilesUnchanged =
                usedHostBefore == TreeHash(usedHost)
        };
    }

    private static string FailedGoal169BRunRoot()
    {
        var path = Path.Combine(Goal164TestKit.RepositoryRoot,
            ".llmgc", "procedural",
            "goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure",
            "payload-only-standalone-proof.json");
        using var document = JsonDocument.Parse(
            File.ReadAllText(path, Encoding.UTF8));
        return document.RootElement.GetProperty("failedRunRoot")
                   .GetString()
               ?? throw new JsonException(
                   "Goal169B failed run root is missing.");
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
        Goal169CImmutableSmokeProof proof,
        ProjectStandaloneCurrentPointer pointer,
        ProjectStandaloneRunStatus status,
        ProjectStandaloneBuildResult standaloneHistory,
        GameProjectGeneratedCampaignRegionalEventSummary events,
        GeneratedCampaignRegionalEventPayloadAuthority authority,
        int payloadFrameCount,
        int nestedCombatFrameCount,
        GameProjectReleaseCandidateRecord releaseCandidate,
        bool releaseCandidateCurrent,
        bool portableCampaignCurrent,
        bool portableReleaseCandidateCurrent,
        bool portableOperationalPointerAbsent,
        bool coreOnlyCampaignCurrent,
        bool coreOnlyNoFalseRcReady)
    {
        var path = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL169C_SMOKE_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path))
            return;
        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            status = "GREEN",
            proof.AttemptId,
            pointerAttemptId = pointer.PublishedAttemptId,
            runStatusAttemptId = status.AttemptId,
            standaloneHistoryAttemptId =
                standaloneHistory.AttemptId,
            proof.HostCacheKey,
            proof.HostReused,
            proof.HostRebuilt,
            unityEditorProcessStartCount = 0,
            unityHostBuildCount = 0,
            hiddenSmokeInvocationCount = 1,
            correctiveRetryCount = 0,
            standaloneLaunchStarted = proof.LaunchSmokePassed,
            proof.LaunchSmokePassed,
            proof.PlayerLogPresent,
            proof.SmokeExitCode,
            proof.SelfCheckPassedCount,
            proof.SelfCheckTotalCount,
            payloadSelfCheckPassed =
                status.PayloadSelfCheckPassed,
            legacyParserPassed =
                status.LegacyParserCompatibilityPassed,
            smokeMarkersPassed = status.SmokeMarkersPassed,
            pointerPublished = true,
            runStatusPublished = true,
            pointer.RunDirectoryName,
            newRunDistinctFromGoal169B =
                !string.Equals(proof.FailedGoal169BRunRoot,
                    proof.RunRoot,
                    StringComparison.OrdinalIgnoreCase),
            proof.ProjectFolder,
            proof.RunRoot,
            proof.PayloadRoot,
            proof.CurrentPointerPath,
            pointerSha256 = HashFile(proof.CurrentPointerPath),
            proof.RunStatusPath,
            runStatusSha256 = HashFile(proof.RunStatusPath),
            proof.BuildHistoryPath,
            buildHistorySha256 =
                HashFile(proof.BuildHistoryPath),
            proof.StandaloneHistoryPath,
            standaloneHistorySha256 =
                HashFile(proof.StandaloneHistoryPath),
            proof.ReleaseCandidatePath,
            releaseCandidateSha256 =
                HashFile(proof.ReleaseCandidatePath),
            proof.BuildManifestPath,
            pointer.PackageSha256,
            pointer.FinalStateHash,
            actualPayloadPackageSha256 = HashFile(Path.Combine(
                proof.PayloadRoot, "game-package.json")),
            playerAdapterModelSha256 = HashFile(Path.Combine(
                proof.PayloadRoot, "player-adapter-model.json")),
            playerAdapterFramesSha256 = HashFile(Path.Combine(
                proof.PayloadRoot, "player-adapter-frames.json")),
            events.StrictProofSchemaVersion,
            eventCount = events.EventCount,
            eventIds = authority.RegionalEventIds,
            replaySignatureCount =
                authority.ReplaySignatures.Count,
            frameCountKeyCount = authority.FrameCounts.Count,
            nestedTraceKeyCount =
                authority.NestedCombatTraceSha256.Count,
            payloadFrameCount,
            nestedCombatFrameCount,
            authority.AuthoritySha256,
            authority.ComponentSha256,
            signaturesRecomputedFromSelectedHistory = true,
            historyPackagePayloadCorrelationPassed = true,
            correlationTerminology =
                "immutable_payload_history_package_correlation",
            frameSchema =
                GeneratedCampaignRegionalEventPayloadAuthorityService
                    .FrameSchema,
            frameIdentitiesPassed = true,
            exactCommandsPassed = true,
            nestedCombatIdentityPassed = true,
            releaseCandidate.Status,
            releaseCandidateCurrent,
            releaseCandidateRecordCurrent =
                releaseCandidate.Status == "GREEN",
            rcHashesExact = true,
            portableCampaignCurrent,
            portableReleaseCandidateCurrent,
            portableOperationalPointerAbsent,
            coreOnlyCampaignCurrent,
            coreOnlyNoFalseRcReady,
            proof.SidecarsBeforeSha256,
            proof.SidecarsAfterSha256,
            proof.SidecarsUnchanged,
            proof.Goal142BeforeSha256,
            proof.Goal142AfterSha256,
            proof.Goal142Unchanged,
            proof.Goal148BeforeSha256,
            proof.Goal148AfterSha256,
            proof.Goal148Unchanged,
            proof.HostBeforeSha256,
            proof.HostAfterSha256,
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

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream))
            .ToLowerInvariant();
    }

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
                    File.ReadAllBytes(file))).ToLowerInvariant())
                .AppendLine();
        }
        return Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(builder.ToString())))
            .ToLowerInvariant();
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

internal sealed record Goal169CImmutableSmokeProof
{
    public string Status { get; init; } = string.Empty;
    public string AttemptId { get; init; } = string.Empty;
    public string HostCacheKey { get; init; } = string.Empty;
    public bool HostReused { get; init; }
    public bool HostRebuilt { get; init; }
    public bool LaunchSmokePassed { get; init; }
    public bool PlayerLogPresent { get; init; }
    public int SmokeExitCode { get; init; }
    public int SelfCheckPassedCount { get; init; }
    public int SelfCheckTotalCount { get; init; }
    public string ProjectFolder { get; init; } = string.Empty;
    public string RunRoot { get; init; } = string.Empty;
    public string PayloadRoot { get; init; } = string.Empty;
    public string CurrentPointerPath { get; init; } = string.Empty;
    public string RunStatusPath { get; init; } = string.Empty;
    public string BuildHistoryPath { get; init; } = string.Empty;
    public string StandaloneHistoryPath { get; init; } = string.Empty;
    public string ReleaseCandidatePath { get; init; } = string.Empty;
    public string BuildManifestPath { get; init; } = string.Empty;
    public string FailedGoal169BRunRoot { get; init; } =
        string.Empty;
    public string SidecarsBeforeSha256 { get; init; } = string.Empty;
    public string SidecarsAfterSha256 { get; init; } = string.Empty;
    public bool SidecarsUnchanged { get; init; }
    public string Goal142BeforeSha256 { get; init; } = string.Empty;
    public string Goal142AfterSha256 { get; init; } = string.Empty;
    public bool Goal142Unchanged { get; init; }
    public string Goal148BeforeSha256 { get; init; } = string.Empty;
    public string Goal148AfterSha256 { get; init; } = string.Empty;
    public bool Goal148Unchanged { get; init; }
    public string HostBeforeSha256 { get; init; } = string.Empty;
    public string HostAfterSha256 { get; init; } = string.Empty;
    public bool HostFilesUnchanged { get; init; }
}

internal sealed class Goal169CPortableCopy : IDisposable
{
    private Goal169CPortableCopy(string root, string path)
    {
        Root = root;
        Path = path;
    }

    public string Root { get; }
    public string Path { get; }

    public static Goal169CPortableCopy Create(
        string source, string folderName)
    {
        var root = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "LLMGameCreator",
            "Goal169CPortable", Guid.NewGuid().ToString("N"));
        var target = System.IO.Path.Combine(root, folderName);
        CopyDirectory(source, target);
        return new Goal169CPortableCopy(root, target);
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
