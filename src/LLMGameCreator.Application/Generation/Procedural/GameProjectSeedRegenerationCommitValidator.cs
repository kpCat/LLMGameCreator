using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GameProjectSeedRegenerationTruthReader : IGameProjectSeedRegenerationTruthReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly string _repositoryRoot;
    private readonly SeededGeneratedProjectSourceService _sourceService;

    public GameProjectSeedRegenerationTruthReader(
        string repositoryRoot,
        SeededGeneratedProjectSourceService sourceService)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
    }

    public GameProjectSeedRegenerationTruthTokens CaptureTruthTokens(
        string projectFolder,
        GameProjectOperationLease operationLease)
    {
        var project = Path.GetFullPath(projectFolder);
        if (operationLease.Coordinator is null
            || !operationLease.Coordinator.IsCurrent(operationLease, project))
            throw new InvalidOperationException("project_operation.lease_invalid");
        var source = _sourceService.Validate(project);
        if (source is not { Present: true, Passed: true, Source: not null })
            throw new InvalidOperationException(source.Present
                ? "regeneration.generated_source_invalid" : "regeneration.not_generated_project");
        var packagePath = Confined(project, "package.json");
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(
                          File.ReadAllText(packagePath, Encoding.UTF8), JsonOptions)
                      ?? throw new InvalidOperationException("regeneration.package_changed");
        var authoring = new GameProjectFeatureModuleAuthoringService(_repositoryRoot);
        var state = authoring.OpenProject(project, package, operationLease);
        var fingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(state.Document, state.Library);
        if (!fingerprint.Passed) throw new InvalidOperationException("regeneration.authoring_invalid");
        var packageHash = HashFile(packagePath);
        if (!string.IsNullOrWhiteSpace(state.Document.LastActivatedProjectPackageSha256)
            && !string.Equals(packageHash, state.Document.LastActivatedProjectPackageSha256, StringComparison.Ordinal))
            throw new InvalidOperationException("regeneration.package_changed");
        var rcPath = new GameProjectReleaseCandidateRecordService().RecordPath(project);
        return new GameProjectSeedRegenerationTruthTokens
        {
            SourceRecordSha256 = HashFile(Confined(project, SeededGeneratedProjectVocabulary.SourceRelativePath)),
            QualifiedAuthoringFingerprint = fingerprint.Sha256,
            AuthoringRevision = state.Document.Revision,
            ActivatedPackageSha256 = packageHash,
            CompositionPackageSha256 = state.Document.LastCompositionPackageSha256,
            FinalStateHash = state.Document.LastQualifiedFinalStateHash,
            ProjectIdentityFingerprint = GameProjectSeedRegenerationService.IdentityFingerprint(state.Identity),
            ReleaseCandidateRecordSha256 = File.Exists(rcPath) ? HashFile(rcPath) : null
        };
    }

    public string CaptureAuthoritativeInventorySha256(string projectFolder)
    {
        var project = Path.GetFullPath(projectFolder);
        var rows = Directory.EnumerateFiles(project, "*", SearchOption.AllDirectories)
            .Select(path => (Path: Path.GetRelativePath(project, path).Replace('\\', '/'), FullPath: path))
            .Where(row => !Excluded(row.Path))
            .OrderBy(row => row.Path, StringComparer.Ordinal)
            .Select(row => (row.Path, Sha: HashFile(row.FullPath)))
            .ToList();
        var stable = new StringBuilder();
        foreach (var row in rows)
            stable.Append(row.Path.Length).Append(':').Append(row.Path)
                .Append(row.Sha.Length).Append(':').Append(row.Sha).Append(';');
        return HashText(stable.ToString());
    }

    private static bool Excluded(string relative) =>
        relative.Equals(GameProjectOperationCoordinator.MutationLockRelativePath, StringComparison.OrdinalIgnoreCase)
        || relative.StartsWith(GameProjectSeedRegenerationVocabulary.TransactionsRelativeRoot + "/",
            StringComparison.OrdinalIgnoreCase)
        || relative.StartsWith(UnifiedGameProjectWorkspaceVocabulary.BuildStagingRelativeRoot + "/",
            StringComparison.OrdinalIgnoreCase)
        || relative.Equals("Builds", StringComparison.OrdinalIgnoreCase)
        || relative.StartsWith("Builds/", StringComparison.OrdinalIgnoreCase)
        || relative.Contains(".tmp-", StringComparison.OrdinalIgnoreCase)
        || relative.EndsWith(".lock", StringComparison.OrdinalIgnoreCase);

    private static string Confined(string root, string relative) =>
        GameProjectFeatureModuleAuthoringService.ConfinedPath(root, relative);

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string HashText(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}

public sealed class GameProjectSeedRegenerationCommitValidator : IGameProjectSeedRegenerationCommitValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        ,Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _repositoryRoot;
    private readonly SeededGeneratedProjectSourceService _sourceService;
    private readonly IGamePackageValidator _packageValidator;
    private readonly GameProjectSeedRegenerationRecordService _regenerationRecordService;

    public GameProjectSeedRegenerationCommitValidator(
        string repositoryRoot,
        SeededGeneratedProjectSourceService sourceService,
        IGamePackageValidator packageValidator,
        GameProjectSeedRegenerationRecordService regenerationRecordService)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _packageValidator = packageValidator ?? throw new ArgumentNullException(nameof(packageValidator));
        _regenerationRecordService = regenerationRecordService
                                     ?? throw new ArgumentNullException(nameof(regenerationRecordService));
    }

    public GameProjectSeedRegenerationCommitValidationResult Validate(
        GameProjectSeedRegenerationCommitValidationRequest request,
        GameProjectOperationLease operationLease)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(operationLease);
        var diagnostics = new List<string>();
        try
        {
            var project = Path.GetFullPath(request.ProjectFolder);
            if (operationLease.Coordinator is null
                || !operationLease.Coordinator.IsCurrent(operationLease, project))
                return Failed("project_operation.lease_invalid");
            var source = _sourceService.Validate(project);
            var generatedEncounterCount = 0;
            if (source is not { Present: true, Passed: true, Source: not null })
                diagnostics.Add("semantic.source_invalid");
            else
            {
                generatedEncounterCount = source.GeneratedMvpPackage?.GeneratedContent.Encounters.Count ?? 0;
                Match(source.Source.PlanSha256, request.CandidateSeal.CandidatePlanSha256,
                    "semantic.source_plan_mismatch", diagnostics);
                Match(source.Source.GeneratedOverlaySha256, request.CandidateSeal.CandidateOverlaySha256,
                    "semantic.source_overlay_mismatch", diagnostics);
                Match(source.Source.GeneratedBasePackageSha256,
                    request.CandidateSeal.CandidateGeneratedBaseSha256,
                    "semantic.source_base_mismatch", diagnostics);
                Match(GameProjectSeedRegenerationDiffService.RequestSha256(source.Source.GenerationRequest),
                    request.CandidateSeal.CandidateSourceRequestSha256,
                    "semantic.source_request_mismatch", diagnostics);
            }

            var packagePath = Confined(project, "package.json");
            var package = JsonSerializer.Deserialize<GamePackageDefinition>(
                              File.ReadAllText(packagePath, Encoding.UTF8), JsonOptions)
                          ?? throw new InvalidOperationException("semantic.package_invalid");
            var packageValidation = _packageValidator.Validate(package, project);
            if (!packageValidation.IsValid) diagnostics.Add("semantic.package_invalid");
            Match(HashFile(packagePath), request.CandidateSeal.CandidatePackageSha256,
                "semantic.package_hash_mismatch", diagnostics);
            var choiceBinding = source is { Present: true, Passed: true }
                ? new GeneratedCampaignChoiceBindingService().Bind(source, package)
                : new GeneratedCampaignChoiceBindingResult();
            if (!choiceBinding.Passed) diagnostics.Add("semantic.history_choice_binding_invalid");
            var branchableDialogueCount = choiceBinding.Bindings.Count(item => item.Branches.Count > 0);

            var authoring = new GameProjectFeatureModuleAuthoringService(_repositoryRoot);
            var state = authoring.OpenProject(project, package, operationLease);
            var fingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(state.Document, state.Library);
            if (!fingerprint.Passed) diagnostics.Add("semantic.authoring_invalid");
            Match(fingerprint.Sha256, request.CandidateSeal.QualifiedAuthoringFingerprint,
                "semantic.authoring_fingerprint_mismatch", diagnostics);
            Match(state.Document.LastActivatedProjectPackageSha256,
                request.CandidateSeal.CandidatePackageSha256,
                "semantic.authoring_package_mismatch", diagnostics);
            Match(state.Document.LastCompositionPackageSha256,
                request.CandidateSeal.CandidateCompositionSha256,
                "semantic.authoring_composition_mismatch", diagnostics);
            Match(state.Document.LastQualifiedFinalStateHash,
                request.CandidateSeal.CandidateFinalStateHash,
                "semantic.authoring_final_state_mismatch", diagnostics);
            Match(GameProjectSeedRegenerationCandidateSealService.SelectedModuleIdsSha256(state),
                request.CandidateSeal.SelectedModuleIdsSha256,
                "semantic.selected_modules_mismatch", diagnostics);
            Match(GameProjectSeedRegenerationCandidateSealService.ParameterValuesSha256(state),
                request.CandidateSeal.ParameterValuesSha256,
                "semantic.parameters_mismatch", diagnostics);
            Match(GameProjectSeedRegenerationService.IdentityFingerprint(state.Identity),
                request.ExpectedProjectIdentityFingerprint,
                "semantic.identity_mismatch", diagnostics);

            var historyPath = Confined(project,
                UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot + "/"
                + request.SelectedBuildHistoryFileName);
            var history = JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(
                              File.ReadAllText(historyPath, Encoding.UTF8), JsonOptions)
                          ?? throw new InvalidOperationException("semantic.history_invalid");
            Match(HashFile(historyPath), request.CandidateSeal.SelectedBuildHistorySha256,
                "semantic.history_hash_mismatch", diagnostics);
            if (history.Status != "GREEN" || history.AttemptStatus != "GREEN"
                || history.GeneratedWorld is not { Present: true, Passed: true }
                || history.GeneratedWorldActivation is not { Present: true, Passed: true }
                || history.GeneratedRegionTravel is not { Present: true, Passed: true, ReplayEquivalent: true,
                    StateRoundtripPassed: true }
                || history.AcceptedMechanics is not { Present: true }
                || history.AcceptedMechanicsCompatibility is not { Passed: true })
                diagnostics.Add("semantic.history_qualification_incomplete");
            if (generatedEncounterCount > 0)
            {
                var combat = history.GeneratedEncounterCombat;
                if (history.SchemaVersion != GameProjectBuildHistoryReader.SchemaVersionV5
                    || combat is not
                    {
                        Present: true,
                        Passed: true,
                        Status: "CAMPAIGN_CURRENT",
                        ExactPackageReferencePassed: true,
                        PackageShaUnchangedDuringRuntime: true,
                        ReplayPassed: true,
                        Overlay: { Passed: true }
                    }
                    || combat.GeneratedEncounterCount != generatedEncounterCount
                    || combat.QualifiedEncounterCount != generatedEncounterCount
                    || !RouteEligible(combat)
                    || !QualifiedActionCatalogEligible(combat)
                    || !string.Equals(combat.ExactPackageSha256, history.PackageSha256,
                        StringComparison.Ordinal))
                    diagnostics.Add("semantic.history_combat_qualification_incomplete");
            }
            else if (history.GeneratedEncounterCombat is { Present: true }
                     or { Status: "CAMPAIGN_CURRENT" })
                diagnostics.Add("semantic.history_combat_unexpected");
            if (branchableDialogueCount > 0)
            {
                var choices = history.GeneratedCampaignChoices;
                if (history.SchemaVersion != GameProjectBuildHistoryReader.SchemaVersionV5
                    || choices is not
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
                        ReplayPassed: true,
                        Overlay: { Passed: true }
                    }
                    || choices.BranchableDialogueCount != branchableDialogueCount
                    || choices.QualifiedDialogueCount != branchableDialogueCount
                    || choices.BranchFlagIds.Count != branchableDialogueCount
                    || choices.BranchFlagIds.Distinct(StringComparer.Ordinal).Count() != branchableDialogueCount
                    || choices.RuntimeFrames.Count != (choices.SupportBranchCount + choices.ChallengeBranchCount
                                                       + choices.RefuseBranchCount) * 2
                    || choices.RuntimeFrames.GroupBy(item => (item.DialogueId, item.BranchKind))
                        .Any(group => !group.Select(item => item.ReplayIndex).OrderBy(item => item)
                            .SequenceEqual([1, 2]))
                    || !string.Equals(choices.FinalPackageSha256, history.PackageSha256,
                        StringComparison.Ordinal)
                    || !string.Equals(choices.FinalStateHash, history.FinalStateHash,
                        StringComparison.Ordinal)
                    || !string.Equals(choices.BranchFlagInventorySha256,
                        GeneratedCampaignChoiceCanonical.Hash(choices.Overlay.FlagInventory),
                        StringComparison.Ordinal))
                    diagnostics.Add("semantic.history_choice_qualification_incomplete");
            }
            Match(history.GeneratedWorld?.MechanicsProfileId ?? string.Empty,
                request.CandidateSeal.MechanicsProfileId,
                "semantic.mechanics_profile_mismatch", diagnostics);
            Match(GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(history.AcceptedMechanics),
                request.CandidateSeal.AcceptedMechanicsSummarySha256,
                "semantic.accepted_mechanics_summary_mismatch", diagnostics);
            Match(GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(
                    history.AcceptedMechanicsCompatibility),
                request.CandidateSeal.AcceptedMechanicsCompatibilitySha256,
                "semantic.accepted_mechanics_compatibility_mismatch", diagnostics);
            Match(GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(
                    history.GeneratedEncounterCombat),
                request.CandidateSeal.GeneratedEncounterCombatSummarySha256,
                "semantic.generated_combat_summary_mismatch", diagnostics);
            Match(GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(
                    history.GeneratedEncounterCombat?.Overlay),
                request.CandidateSeal.GeneratedEncounterCombatOverlaySha256,
                "semantic.generated_combat_overlay_mismatch", diagnostics);
            Match(history.GeneratedEncounterCombat?.ContractId ?? string.Empty,
                request.CandidateSeal.GeneratedEncounterCombatContractId,
                "semantic.generated_combat_contract_mismatch", diagnostics);
            Match(GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(
                    history.GeneratedCampaignChoices),
                request.CandidateSeal.GeneratedCampaignChoiceSummarySha256,
                "semantic.generated_choice_summary_mismatch", diagnostics);
            Match(GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(
                    history.GeneratedCampaignChoices?.Overlay),
                request.CandidateSeal.GeneratedCampaignChoiceOverlaySha256,
                "semantic.generated_choice_overlay_mismatch", diagnostics);
            Match(GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(
                    history.GeneratedCampaignChoices?.Overlay?.FlagInventory),
                request.CandidateSeal.GeneratedCampaignChoiceFlagInventorySha256,
                "semantic.generated_choice_flag_inventory_mismatch", diagnostics);
            Match(history.PackageSha256, request.CandidateSeal.CandidatePackageSha256,
                "semantic.history_package_mismatch", diagnostics);
            Match(history.CompositionPackageSha256, request.CandidateSeal.CandidateCompositionSha256,
                "semantic.history_composition_mismatch", diagnostics);
            Match(history.FinalStateHash, request.CandidateSeal.CandidateFinalStateHash,
                "semantic.history_final_state_mismatch", diagnostics);

            if (string.Equals(request.OperationKind, "regeneration", StringComparison.Ordinal))
            {
                var record = _regenerationRecordService.Read(project, operationLease);
                if (!record.Passed) diagnostics.Add("semantic.regeneration_record_invalid");
            }
            var worldChangePath = Confined(project, GameProjectGeneratedWorldChangeVocabulary.RelativePath);
            if (!File.Exists(worldChangePath)) diagnostics.Add("semantic.world_change_record_missing");
            else if (!string.IsNullOrWhiteSpace(request.ExpectedWorldChangeRecordSha256)
                     && !string.Equals(HashFile(worldChangePath), request.ExpectedWorldChangeRecordSha256,
                         StringComparison.Ordinal))
                diagnostics.Add("semantic.world_change_record_mismatch");

            var rcPath = new GameProjectReleaseCandidateRecordService().RecordPath(project);
            var actualRcHash = File.Exists(rcPath) ? HashFile(rcPath) : null;
            if (!string.Equals(actualRcHash, request.PreviousReleaseCandidateRecordSha256, StringComparison.Ordinal))
                diagnostics.Add("semantic.release_candidate_bytes_changed");
            var rc = new GameProjectReleaseCandidateRecordService().Read(new GameProjectReleaseCandidateReadRequest
            {
                ProjectFolder = project,
                Document = state.Document,
                Library = state.Library,
                Identity = state.Identity
            });
            if (actualRcHash is null)
            {
                if (rc.ConfigurationStatus != "ABSENT") diagnostics.Add("semantic.release_candidate_status_invalid");
            }
            else if (rc.ConfigurationStatus != "LAST_SUCCESS")
                diagnostics.Add("semantic.release_candidate_status_invalid");
            Match(rc.ConfigurationStatus,
                request.CandidateSeal.ExpectedCandidateRcRecordStatus,
                "semantic.release_candidate_record_status_mismatch", diagnostics);
            var acceptedMechanicsCurrent = history.AcceptedMechanics is { Passed: true }
                                           && fingerprint.Passed
                                           && !string.IsNullOrWhiteSpace(fingerprint.Sha256)
                                           && string.Equals(history.AcceptedMechanics.QualifiedAuthoringFingerprint,
                                               fingerprint.Sha256, StringComparison.Ordinal);
            var overallStatus = GameProjectReleaseCandidateRecordService.ResolveOverallStatus(
                history.AcceptedMechanics,
                acceptedMechanicsCurrent,
                history.PackageSha256,
                history.CompositionPackageSha256,
                history.FinalStateHash,
                rc);
            Match(overallStatus,
                request.CandidateSeal.ExpectedCandidateRcOverallStatus,
                "semantic.release_candidate_overall_status_mismatch", diagnostics);

            return new GameProjectSeedRegenerationCommitValidationResult
            {
                Passed = diagnostics.Count == 0,
                Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).ToList()
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or JsonException or InvalidOperationException)
        {
            return Failed(exception.Message);
        }
    }

    private static string Confined(string root, string relative) =>
        GameProjectFeatureModuleAuthoringService.ConfinedPath(root, relative);

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void Match(string expected, string actual, string diagnostic, ICollection<string> diagnostics)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal)) diagnostics.Add(diagnostic);
    }

    private static bool RouteEligible(GameProjectGeneratedEncounterCombatSummary combat) => combat.RouteMode switch
    {
        GeneratedEncounterCombatRouteMode.BASIC_ATTACK_ONLY =>
            combat.PlayerRoutePassed && combat.BasicAttackRequired && !combat.PackageAbilityRequired
            && combat.BasicAttackPassed && combat.PackageAbilityPassed,
        GeneratedEncounterCombatRouteMode.PACKAGE_ABILITY_ONLY =>
            combat.PlayerRoutePassed && !combat.BasicAttackRequired && combat.PackageAbilityRequired
            && combat.BasicAttackPassed && combat.PackageAbilityPassed,
        GeneratedEncounterCombatRouteMode.BOTH =>
            combat.PlayerRoutePassed && combat.BasicAttackRequired && combat.PackageAbilityRequired
            && combat.BasicAttackPassed && combat.PackageAbilityPassed,
        GeneratedEncounterCombatRouteMode.NONE =>
            !combat.PlayerRoutePassed && !combat.BasicAttackRequired && !combat.PackageAbilityRequired
            && combat.BasicAttackPassed && combat.PackageAbilityPassed,
        _ => false
    };

    private static bool QualifiedActionCatalogEligible(GameProjectGeneratedEncounterCombatSummary combat)
    {
        var catalogDeclared = combat.QualifiedActionCount > 0
                              || !string.IsNullOrWhiteSpace(combat.QualifiedActionsSha256)
                              || combat.QualifiedActions.Count > 0;
        if (!catalogDeclared) return true;
        var actions = combat.QualifiedActions.OrderBy(item => item.ActionKind)
            .ThenBy(item => item.AbilityId, StringComparer.Ordinal)
            .ThenBy(item => item.AbilityDefinitionSha256, StringComparer.Ordinal)
            .ThenBy(item => item.ObservedEffect.Fingerprint, StringComparer.Ordinal).ToList();
        var basic = actions.Count(item => item.ActionKind
            == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK);
        var abilities = actions.Count(item => item.ActionKind
            == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY);
        var mode = (basic > 0, abilities > 0) switch
        {
            (true, true) => GeneratedEncounterCombatRouteMode.BOTH,
            (true, false) => GeneratedEncounterCombatRouteMode.BASIC_ATTACK_ONLY,
            (false, true) => GeneratedEncounterCombatRouteMode.PACKAGE_ABILITY_ONLY,
            _ => GeneratedEncounterCombatRouteMode.NONE
        };
        return actions.Count > 0
               && combat.QualifiedActionCount == actions.Count
               && combat.QualifiedBasicAttackCount == basic
               && combat.QualifiedPackageAbilityCount == abilities
               && string.Equals(combat.QualifiedActionsSha256, GeneratedEncounterCombatCanonical.Hash(actions),
                   StringComparison.Ordinal)
               && combat.RouteMode == mode
               && actions.All(item => item.RuntimeQualificationPassed
                   && (item.ActionKind == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK
                       ? string.IsNullOrWhiteSpace(item.AbilityId)
                       : !string.IsNullOrWhiteSpace(item.AbilityId)
                         && !string.IsNullOrWhiteSpace(item.AbilityDefinitionSha256)));
    }

    private static GameProjectSeedRegenerationCommitValidationResult Failed(string diagnostic) => new()
    {
        Diagnostics = [diagnostic]
    };
}
