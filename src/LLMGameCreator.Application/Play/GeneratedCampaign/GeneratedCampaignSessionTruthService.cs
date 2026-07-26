using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignSessionTruthService
{
    private readonly ICurrentGamePackageService _currentProject;
    private readonly GeneratedGameplaySaveValidator _validator;
    private readonly IGameProjectOperationCoordinator _operations;
    private readonly GameProjectBuildHistoryReader _history;

    public GeneratedCampaignSessionTruthService(
        ICurrentGamePackageService currentProject,
        GeneratedGameplaySaveValidator validator,
        IGameProjectOperationCoordinator operations,
        GameProjectBuildHistoryReader? history = null)
    {
        _currentProject = currentProject;
        _validator = validator;
        _operations = operations;
        _history = history ?? new GameProjectBuildHistoryReader();
    }

    public (GeneratedCampaignSessionStatus Status, GeneratedCampaignProjectTruth? Truth, IReadOnlyList<string> Diagnostics) Capture()
    {
        if (string.IsNullOrWhiteSpace(_currentProject.CurrentFolder)) return (GeneratedCampaignSessionStatus.NO_PROJECT, null, ["campaign.no_project"]);
        var folder = _currentProject.CurrentFolder;
        using var lease = _operations.TryAcquire(folder, GameProjectOperationKinds.GameplayLoad);
        if (!lease.Acquired) return (GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null, [lease.Diagnostic]);
        var captured = _validator.CaptureProjectTruth(folder, lease);
        if (!captured.Passed || captured.Truth is null)
        {
            var generated = captured.Diagnostics.Any(x => x == "generated_save.not_generated_project");
            return (generated ? GeneratedCampaignSessionStatus.PROJECT_NOT_GENERATED : GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null,
                captured.Diagnostics.Select(x => x.Replace("generated_save.", "campaign.", StringComparison.Ordinal)).ToList());
        }
        var truth = captured.Truth;
        var source = truth.StrictGeneratedSource.Source!;
        var build = ReadCurrentBuildHistory(folder);
        if (build.Result?.LastSuccessfulBuild is null)
            return (GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null, [build.Diagnostic]);
        var successfulBuild = build.Result.LastSuccessfulBuild;
        var finalStateHash = successfulBuild.FinalStateHash;
        if (string.IsNullOrWhiteSpace(finalStateHash)
            || !string.Equals(finalStateHash, build.DocumentFinalStateHash, StringComparison.Ordinal))
            return (GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null,
                ["campaign.current_final_state_mismatch"]);
        var generatedEncounterCount = truth.StrictGeneratedSource.GeneratedMvpPackage?
            .GeneratedContent.Encounters.Count ?? 0;
        if (!CombatCurrent(successfulBuild, generatedEncounterCount))
            return (GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null,
                ["campaign.generated_combat_not_current"]);
        var currentPackage = _currentProject.CurrentPackage;
        if (currentPackage is null)
            return (GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null,
                ["campaign.current_package_missing"]);
        var binding = new GeneratedCampaignChoiceBindingService().Bind(
            truth.StrictGeneratedSource, currentPackage);
        if (!binding.Passed)
            return (GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null,
                binding.Diagnostics.Select(item => "campaign." + item).ToList());
        var branchableCount = binding.Bindings.Count(item => item.Branches.Count > 0);
        if (branchableCount > 0 && !ChoicesCurrent(successfulBuild, branchableCount))
            return (GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null,
                ["campaign.generated_choices_not_current"]);
        var relationshipBinding = new GeneratedCampaignRelationshipBindingService().Bind(
            truth.StrictGeneratedSource, currentPackage, binding);
        if (!relationshipBinding.Passed)
            return (GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null,
                relationshipBinding.Diagnostics.Select(item => "campaign." + item).ToList());
        var relationshipCount = relationshipBinding.Bindings.Count(item => item.QuestArc.Count > 0);
        if (!RelationshipsCurrent(successfulBuild, relationshipCount))
            return (GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null,
                ["campaign.generated_relationships_not_current"]);
        if (string.IsNullOrWhiteSpace(truth.SelectedBuildHistorySha256))
            return (GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null,
                ["campaign.current_build_history_missing"]);
        return (GeneratedCampaignSessionStatus.READY, new GeneratedCampaignProjectTruth
        {
            ProjectFolder = truth.ProjectFolder,
            ProjectIdentityFingerprint = truth.IdentityFingerprint,
            WorldId = truth.WorldId,
            GenerationSeed = source.GenerationRequest.Seed,
            SourceRecordSha256 = Hash(Path.Combine(truth.ProjectFolder, SeededGeneratedProjectVocabulary.SourceRelativePath.Replace('/', Path.DirectorySeparatorChar))),
            SourceRequestSha256 = GameProjectSeedRegenerationDiffService.RequestSha256(source.GenerationRequest),
            PlanSha256 = source.PlanSha256,
            GeneratedBasePackageSha256 = source.GeneratedBasePackageSha256,
            PackageSha256 = truth.PackageSha256,
            CompositionPackageSha256 = truth.CompositionPackageSha256,
            FinalStateHash = finalStateHash,
            SelectedBuildHistorySha256 = truth.SelectedBuildHistorySha256,
            QualifiedAuthoringFingerprint = truth.QualifiedAuthoringFingerprint,
            SelectedBuildHistoryFileName = truth.SelectedBuildHistoryFileName,
            GeneratedStartMapId = truth.GeneratedStartMapId,
            RegionMapBindings = truth.GeneratedRegionMapBindings,
            RelationshipOverlay = successfulBuild.GeneratedCampaignRelationships?.Overlay
        }, []);
    }

    public static bool Same(GeneratedCampaignProjectTruth left, GeneratedCampaignProjectTruth right) =>
        left.ProjectIdentityFingerprint == right.ProjectIdentityFingerprint
        && left.WorldId == right.WorldId
        && left.SourceRecordSha256 == right.SourceRecordSha256
        && left.SourceRequestSha256 == right.SourceRequestSha256
        && left.PlanSha256 == right.PlanSha256
        && left.GeneratedBasePackageSha256 == right.GeneratedBasePackageSha256
        && left.PackageSha256 == right.PackageSha256
        && left.CompositionPackageSha256 == right.CompositionPackageSha256
        && left.FinalStateHash == right.FinalStateHash
        && left.SelectedBuildHistorySha256 == right.SelectedBuildHistorySha256
        && left.QualifiedAuthoringFingerprint == right.QualifiedAuthoringFingerprint
        && left.SelectedBuildHistoryFileName == right.SelectedBuildHistoryFileName
        && left.GeneratedStartMapId == right.GeneratedStartMapId
        && left.RegionMapBindings.OrderBy(item => item.Key, StringComparer.Ordinal)
            .SequenceEqual(right.RegionMapBindings.OrderBy(item => item.Key, StringComparer.Ordinal))
        && string.Equals(left.RelationshipOverlay?.InventorySha256,
            right.RelationshipOverlay?.InventorySha256, StringComparison.Ordinal);

    private static bool CombatCurrent(GameProjectBuildResult build, int generatedEncounterCount)
    {
        if (generatedEncounterCount == 0)
            return build.GeneratedEncounterCombat is null
                   or { Present: false, Status: "ABSENT" };
        return build.GeneratedEncounterCombat is
        {
            Present: true,
            Passed: true,
            Status: "CAMPAIGN_CURRENT",
            ExactPackageReferencePassed: true,
            PackageShaUnchangedDuringRuntime: true,
            ReplayPassed: true,
            Overlay: { Passed: true }
        } combat
        && combat.GeneratedEncounterCount == generatedEncounterCount
        && combat.QualifiedEncounterCount == generatedEncounterCount
        && string.Equals(combat.ExactPackageSha256, build.PackageSha256, StringComparison.Ordinal)
        && string.Equals(combat.Overlay.OutputPackageSha256, build.PackageSha256, StringComparison.Ordinal)
        && (build.GeneratedCampaignRelationships is { Passed: true }
            || build.GeneratedCampaignChoices is { Present: true, Passed: true, Status: "CHOICE_CURRENT" }
            || string.Equals(combat.FinalStateHash, build.FinalStateHash, StringComparison.Ordinal));
    }

    private static bool ChoicesCurrent(GameProjectBuildResult build, int branchableCount) =>
        build.GeneratedCampaignChoices is
        {
            Present: true,
            Passed: true,
            Status: "CHOICE_CURRENT",
            RuntimeQualificationPassed: true,
            ExclusiveBranchingPassed: true,
            FollowUpPassed: true,
            ChallengeFleeFollowUpPassed: true,
            ChallengeVictoryFollowUpPassed: true,
            AtomicRollbackPassed: true,
            ReplayPassed: true
        } choices
        && choices.BranchableDialogueCount == branchableCount
        && choices.QualifiedDialogueCount == branchableCount
        && choices.BranchFlagIds.Count == branchableCount
        && choices.BranchFlagIds.Distinct(StringComparer.Ordinal).Count() == branchableCount
        && !string.IsNullOrWhiteSpace(choices.BranchFlagInventorySha256)
        && string.Equals(choices.FinalPackageSha256, build.PackageSha256, StringComparison.Ordinal)
        && (build.GeneratedCampaignRelationships is { Passed: true }
            || string.Equals(choices.FinalStateHash, build.FinalStateHash, StringComparison.Ordinal));

    private static bool RelationshipsCurrent(GameProjectBuildResult build, int relationshipCount)
    {
        var relationships = build.GeneratedCampaignRelationships;
        if (relationshipCount == 0)
            return relationships is
            {
                Present: false,
                Passed: true,
                Status: "ABSENT",
                RelationshipCount: 0,
                ArcQuestCount: 0
            };
        return relationships is
        {
            Present: true,
            Passed: true,
            Status: "RELATIONSHIPS_CURRENT",
            AssignmentUnique: true,
            ArcOrderingDeterministic: true,
            OverlayControlledDeltaPassed: true,
            RuntimeQualificationPassed: true,
            ExclusiveBranchingPassed: true,
            ArcProgressionPassed: true,
            ExactCombatCatalogPassed: true,
            SupportPassed: true,
            SupportReplayEquivalent: true,
            ChallengeFleePassed: true,
            ChallengeVictoryPassed: true,
            ChallengeRecoveryPassed: true,
            RefusePassed: true,
            AtomicRollbackPassed: true,
            Overlay: { Passed: true }
        }
        && relationships.RelationshipCount == relationshipCount
        && relationships.QualifiedRelationshipCount == relationshipCount
        && relationships.ArcQuestCount == relationships.QualifiedArcQuestCount
        && string.Equals(relationships.ExactPackageSha256, build.PackageSha256,
            StringComparison.Ordinal)
        && string.Equals(relationships.FinalStateHash, build.FinalStateHash,
            StringComparison.Ordinal);
    }

    private static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }

    private (GameProjectBuildHistoryReadResult? Result, string DocumentFinalStateHash, string Diagnostic)
        ReadCurrentBuildHistory(string projectFolder)
    {
        try
        {
            var identityStore = new GameProjectIdentityStore();
            var identity = identityStore.Load(identityStore.PathFor(projectFolder));
            var compositionId = new GameProjectCompositionIdentityService().Create(identity.PackageId);
            var authoringRoot = GameProjectFeatureModuleAuthoringService.ConfinedPath(projectFolder,
                UnifiedGameProjectWorkspaceVocabulary.AuthoringRelativeRoot);
            var path = GameProjectFeatureModuleAuthoringService.ConfinedPath(authoringRoot,
                compositionId + FeatureModuleCompositionDocumentVocabulary.FileExtension);
            if (!File.Exists(path)) return (null, string.Empty, "campaign.current_build_history_missing");
            var document = JsonSerializer.Deserialize<FeatureModuleCompositionDocument>(
                File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (document is null) return (null, string.Empty, "campaign.current_build_history_missing");
            var result = _history.ReadLatestMatchingSocialSuccess(projectFolder, document);
            return result.LastSuccessfulBuild is null
                ? (result, document.LastQualifiedFinalStateHash, "campaign.current_build_history_missing")
                : (result, document.LastQualifiedFinalStateHash, string.Empty);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or JsonException or InvalidOperationException)
        {
            return (null, string.Empty, "campaign.current_build_history_missing");
        }
    }
}
