using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169D;

[Collection(LLMGameCreator.Tests.Application.Goal160
    .Goal160Collection.Name)]
public sealed class Goal169DEvidenceCaptureTests
{
    [Fact]
    public void Behavioral_capture_qualified_core_only_portable_truth()
    {
        var state = Goal169DTestKit.State;
        var path = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL169D_CAPTURE_PATH");

        Assert.True(state.Qualified.Build.Passed);
        Assert.True(state.QualifiedCorrelation.Passed);
        Assert.True(state.PortableCorrelation.Passed);
        Assert.Equal(state.RetainedBefore.CurrentPointerSha256,
            state.RetainedAfter.CurrentPointerSha256);

        if (string.IsNullOrWhiteSpace(path))
            return;

        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        var payload = new
        {
            status = "GREEN",
            raw = new
            {
                state.Raw.Status,
                state.Raw.BuildInvocationCount,
                state.Raw.PackageSha256,
                state.Raw.SourceSha256,
                state.Raw.AuthoringSha256,
                state.Raw.GenerationSha256,
                packageValid = true,
                sourcePresent = state.Raw.Source.Present,
                sourcePassed = state.Raw.Source.Passed,
                selectedOptionalModuleCount =
                    state.Raw.Authoring.Document.SelectedModuleIds.Count,
                configuredParameterCount =
                    state.Raw.Authoring.Document.ParameterValues.Count,
                historyCount = Goal169DTestKit.BuildHistoryFiles(
                    state.Raw.Project.Path).Count,
                generatedWorldStatus =
                    state.Raw.Snapshot.GeneratedWorld?.Status,
                regionalEventStatus =
                    state.Raw.Snapshot
                        .GeneratedCampaignRegionalEvents?.Status,
                releaseCandidateConfigurationStatus =
                    state.Raw.Snapshot
                        .ReleaseCandidateConfigurationStatus,
                releaseCandidateRecordConfigurationStatus =
                    state.Raw.Snapshot
                        .ReleaseCandidateRecordConfigurationStatus
            },
            qualified = new
            {
                buildInvocationCount =
                    state.QualifiedBuildInvocationCount,
                buildPassed = state.Qualified.Build.Passed,
                buildStatus = state.Qualified.Build.Status,
                historyCount = state.QualifiedHistoryFiles.Count,
                historySchema =
                    state.QualifiedHistory.SchemaVersion,
                historySha256 = Goal169DTestKit.FileSha(
                    state.Qualified.Build.BuildHistoryPath),
                packageSha256 = state.QualifiedPackageSha256,
                finalStateHash =
                    state.Qualified.Build.FinalStateHash,
                packageValid = true,
                generatedWorldStatus =
                    state.Qualified.Snapshot.GeneratedWorld?.Status,
                availableBranchCount =
                    state.AvailableBranchCount,
                relationshipStatus =
                    state.Relationships.Status,
                relationshipCount =
                    state.Relationships.RelationshipCount,
                relationshipBranchMatrixSha256 =
                    state.Relationships
                        .RelationshipBranchMatrixSha256,
                regionalEventStatus = state.Events.Status,
                regionalEventCount = state.Events.EventCount,
                qualifiedRegionalEventCount =
                    state.Events.QualifiedEventCount,
                regionalEventInventorySha256 =
                    state.Events.RegionalEventInventorySha256,
                regionalEventFinalStateHash =
                    state.Events.FinalStateHash,
                strictEmptyPolicy =
                    state.Events.EmptyOverlayPolicy,
                packageCorrelationPassed =
                    state.QualifiedCorrelation.Passed,
                acceptedMechanicsPassed =
                    state.Qualified.Build.AcceptedMechanics?.Passed,
                acceptedMechanicsMissingFactCount =
                    state.Qualified.Build.AcceptedMechanics
                        ?.MissingFactKinds.Count,
                releaseCandidateConfigurationStatus =
                    state.Qualified.Snapshot
                        .ReleaseCandidateConfigurationStatus,
                releaseCandidateRecordConfigurationStatus =
                    state.Qualified.Snapshot
                        .ReleaseCandidateRecordConfigurationStatus,
                sourceSha256 = state.QualifiedSourceSha256,
                authoringSha256 =
                    state.QualifiedAuthoringSha256,
                generationSha256 =
                    state.QualifiedGenerationSha256
            },
            portable = new
            {
                sourceProjectPath =
                    state.Qualified.Project.Path,
                copyProjectPath = state.Portable.Path,
                buildsPresent = Directory.Exists(Path.Combine(
                    state.Portable.Path, "Builds")),
                packageSha256 =
                    state.PortableAfterOpen.PackageSha256,
                selectedHistorySha256 =
                    state.PortableAfterOpen.SelectedHistorySha256,
                authoringSha256 =
                    state.PortableAfterOpen.AuthoringSha256,
                generationSha256 =
                    state.PortableAfterOpen.GenerationSha256,
                generatedWorldStatus =
                    state.PortableSnapshot.GeneratedWorld?.Status,
                relationshipStatus =
                    state.PortableSnapshot
                        .GeneratedCampaignRelationships?.Status,
                regionalEventStatus =
                    state.PortableSnapshot
                        .GeneratedCampaignRegionalEvents?.Status,
                regionalEventInventorySha256 =
                    state.PortableSnapshot
                        .GeneratedCampaignRegionalEvents
                        ?.RegionalEventInventorySha256,
                regionalEventFinalStateHash =
                    state.PortableSnapshot
                        .GeneratedCampaignRegionalEvents
                        ?.FinalStateHash,
                packageCorrelationPassed =
                    state.PortableCorrelation.Passed,
                operationalPointerResolved =
                    state.PortablePointer.Passed,
                operationalPointerDiagnostic =
                    state.PortablePointer.Diagnostic,
                releaseCandidateConfigurationStatus =
                    state.PortableSnapshot
                        .ReleaseCandidateConfigurationStatus,
                releaseCandidateRecordConfigurationStatus =
                    state.PortableSnapshot
                        .ReleaseCandidateRecordConfigurationStatus,
                reopenPreservedPackage =
                    state.PortableBeforeOpen.PackageSha256 ==
                    state.PortableAfterOpen.PackageSha256,
                reopenPreservedHistory =
                    state.PortableBeforeOpen.SelectedHistorySha256 ==
                    state.PortableAfterOpen.SelectedHistorySha256,
                reopenPreservedAuthoring =
                    state.PortableBeforeOpen.AuthoringSha256 ==
                    state.PortableAfterOpen.AuthoringSha256,
                reopenPreservedGeneration =
                    state.PortableBeforeOpen.GenerationSha256 ==
                    state.PortableAfterOpen.GenerationSha256
            },
            retainedGoal169C = new
            {
                pointerPath =
                    state.RetainedAfter.CurrentPointerPath,
                pointerSha256 =
                    state.RetainedAfter.CurrentPointerSha256,
                runRoot = state.RetainedAfter.RunRoot,
                runTreeSha256 =
                    state.RetainedAfter.RunTreeSha256,
                runStatusPath =
                    state.RetainedAfter.RunStatusPath,
                runStatusSha256 =
                    state.RetainedAfter.RunStatusSha256,
                payloadRoot =
                    state.RetainedAfter.PayloadRoot,
                payloadTreeSha256 =
                    state.RetainedAfter.PayloadTreeSha256,
                standaloneHistoryPath =
                    state.RetainedAfter.StandaloneHistoryPath,
                standaloneHistorySha256 =
                    state.RetainedAfter
                        .StandaloneHistorySha256,
                selectedHistoryPath =
                    state.RetainedAfter.SelectedHistoryPath,
                selectedHistorySha256 =
                    state.RetainedAfter.SelectedHistorySha256,
                releaseCandidatePath =
                    state.RetainedAfter.ReleaseCandidatePath,
                releaseCandidateSha256 =
                    state.RetainedAfter.ReleaseCandidateSha256,
                packagePath =
                    state.RetainedAfter.PackagePath,
                packageSha256 =
                    state.RetainedAfter.PackageSha256,
                finalStateHash =
                    state.RetainedAfter.ExpectedFinalStateHash,
                beforeAfterByteIdentical =
                    state.RetainedBefore.CurrentPointerSha256 ==
                    state.RetainedAfter.CurrentPointerSha256
                    && state.RetainedBefore.RunTreeSha256 ==
                    state.RetainedAfter.RunTreeSha256
                    && state.RetainedBefore.PayloadTreeSha256 ==
                    state.RetainedAfter.PayloadTreeSha256
                    && state.RetainedBefore
                        .StandaloneHistorySha256 ==
                    state.RetainedAfter
                        .StandaloneHistorySha256
                    && state.RetainedBefore
                        .SelectedHistorySha256 ==
                    state.RetainedAfter.SelectedHistorySha256
                    && state.RetainedBefore
                        .ReleaseCandidateSha256 ==
                    state.RetainedAfter.ReleaseCandidateSha256
                    && state.RetainedBefore.PackageSha256 ==
                    state.RetainedAfter.PackageSha256
            },
            invocationCounts = new
            {
                realPlayerSmoke =
                    state.RealPlayerSmokeInvocationCount,
                unityEditorStarts =
                    state.UnityEditorProcessStartCount,
                unityHostBuilds = state.UnityHostBuildCount,
                cachedHostMutations =
                    state.CachedHostMutationCount
            },
            hostBeforeSha256 = state.HostBeforeSha256,
            hostAfterSha256 = state.HostAfterSha256
        };
        File.WriteAllText(
            path,
            JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder =
                        JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }) + Environment.NewLine,
            new UTF8Encoding(false));
    }
}
