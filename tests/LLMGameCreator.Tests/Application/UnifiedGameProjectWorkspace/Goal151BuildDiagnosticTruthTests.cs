using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;
using RuntimeSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace;

public sealed class Goal151BuildDiagnosticTruthTests
{
    [Fact]
    public void Semantic_mismatch_reports_exact_stage_and_causal_effect_values()
    {
        var root = Goal150AParameterizedRuntimeContractSynchronizationTests.FindRoot();
        var temp = Goal150AParameterizedRuntimeContractSynchronizationTests.Temp("goal151-semantic-mismatch");
        try
        {
            var library = Goal150AParameterizedRuntimeContractSynchronizationTests.Load(root);
            const string moduleId = "feature.profile.alchemy_focus";
            var original = library.Catalog.Modules.Single(module => module.ModuleId == moduleId);
            var changed = original with
            {
                RuntimeEffectContracts = original.RuntimeEffectContracts.Select((effect, index) =>
                    index == 0 ? effect with
                    {
                        ComparisonKind = FeatureModuleRuntimeEffectComparisonKinds.Equal,
                        ExpectedValue = "999"
                    } : effect).ToList()
            };
            var modules = library.Catalog.Modules.Select(module => module.ModuleId == moduleId ? changed : module).ToList();
            var fingerprints = new FeatureModuleLibraryFingerprintService();
            var moduleFingerprints = modules.ToDictionary(module => module.ModuleId,
                fingerprints.ModuleFingerprint, StringComparer.Ordinal);
            var changedLibrary = library with
            {
                Catalog = library.Catalog with { Modules = modules },
                ModuleFingerprints = moduleFingerprints,
                CatalogFingerprint = fingerprints.CatalogFingerprint(moduleFingerprints)
            };
            var document = new FeatureModuleCompositionPersistenceService(Path.Combine(temp, "authoring"))
                .CreateNew("goal151-semantic-mismatch", "Diagnostic fixture", "Synthetic mismatch", changedLibrary) with
            {
                SelectedModuleIds = [moduleId]
            };
            var result = new FeatureModuleParameterizedCompositionService(
                    SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
                .MaterializeAndQualify(root, changedLibrary, document, Path.Combine(temp, "output"), true);

            Assert.False(result.Passed);
            Assert.Equal("runtime.semantic_effect", result.FailureStage);
            Assert.Contains(result.Diagnostics, line => line == "runtime.semantic_effect.failed");
            Assert.Contains(result.Diagnostics, line => line.Contains("moduleId=" + moduleId, StringComparison.Ordinal)
                                                        && line.Contains("expectedValue=999", StringComparison.Ordinal)
                                                        && line.Contains("actualValue=", StringComparison.Ordinal));
        }
        finally
        {
            Goal150AParameterizedRuntimeContractSynchronizationTests.Delete(temp);
        }
    }

    [Fact]
    public void Checkpoint_replay_failure_is_nonempty_and_stage_aware()
    {
        var root = Goal150AParameterizedRuntimeContractSynchronizationTests.FindRoot();
        var temp = Goal150AParameterizedRuntimeContractSynchronizationTests.Temp("goal151-checkpoint-failure");
        try
        {
            var library = Goal150AParameterizedRuntimeContractSynchronizationTests.Load(root);
            var document = new FeatureModuleCompositionPersistenceService(Path.Combine(temp, "authoring"))
                .CreateNew("goal151-checkpoint-failure", "Diagnostic fixture", "Synthetic replay failure", library);
            var result = new FeatureModuleParameterizedCompositionService(new FailingReplayRuntime())
                .MaterializeAndQualify(root, library, document, Path.Combine(temp, "output"), true);

            Assert.False(result.Passed);
            Assert.Equal("runtime.checkpoint_replay", result.FailureStage);
            Assert.Contains("runtime.checkpoint_replay.failed", result.Diagnostics);
            Assert.Contains(result.Diagnostics, line => line.Contains("injected checkpoint mismatch", StringComparison.Ordinal));
        }
        finally
        {
            Goal150AParameterizedRuntimeContractSynchronizationTests.Delete(temp);
        }
    }

    private sealed class FailingReplayRuntime : ISelectedRuntimeVariantInteractiveSessionService
    {
        private readonly ISelectedRuntimeVariantInteractiveSessionService _inner =
            SelectedRuntimeVariantInteractiveSessionService.CreateDefault();

        public RuntimeSession StartSession(GamePackageDefinition package,
            SelectedRuntimeVariantInteractiveSessionStartRequest request) => _inner.StartSession(package, request);

        public SelectedRuntimeVariantInteractiveActionResult ExecuteAction(GamePackageDefinition package,
            RuntimeSession session, SelectedRuntimeVariantInteractiveActionRequest request) =>
            _inner.ExecuteAction(package, session, request);

        public SelectedRuntimeVariantInteractiveCheckpoint SaveCheckpoint(
            RuntimeSession session, string checkpointId, string createdAtUtc) =>
            _inner.SaveCheckpoint(session, checkpointId, createdAtUtc);

        public SelectedRuntimeVariantInteractiveReplayResult ReloadCheckpoint(GamePackageDefinition package,
            SelectedRuntimeVariantInteractiveSessionStartRequest request,
            SelectedRuntimeVariantInteractiveCheckpoint checkpoint)
        {
            var actual = _inner.ReloadCheckpoint(package, request, checkpoint);
            return new SelectedRuntimeVariantInteractiveReplayResult
            {
                Passed = false,
                PackageHashValidated = actual.PackageHashValidated,
                CandidateValidated = actual.CandidateValidated,
                JournalCorrelationPassed = actual.JournalCorrelationPassed,
                StateHashContinuityPassed = false,
                ExpectedStateHashMatched = false,
                ExpectedStateHash = actual.ExpectedStateHash,
                ActualStateHash = actual.ActualStateHash,
                ReplayedActionCount = actual.ReplayedActionCount,
                Session = actual.Session,
                Diagnostics = ["injected checkpoint mismatch"]
            };
        }
    }
}
