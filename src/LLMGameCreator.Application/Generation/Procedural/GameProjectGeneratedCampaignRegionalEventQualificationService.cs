using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class
    GameProjectGeneratedCampaignRegionalEventQualificationService
{
    private readonly GeneratedCampaignExactCombatRouteService _combatRoute;

    public GameProjectGeneratedCampaignRegionalEventQualificationService(
        GeneratedCampaignExactCombatRouteService? combatRoute = null)
    {
        _combatRoute = combatRoute ??
                       new GeneratedCampaignExactCombatRouteService();
    }

    public GameProjectGeneratedCampaignRegionalEventSummary Qualify(
        GamePackageDefinition finalPackage,
        GeneratedCampaignRegionalEventOverlayDocument overlay,
        GameProjectGeneratedCampaignRelationshipSummary relationships,
        GameProjectGeneratedEncounterCombatSummary? combatSummary,
        IUnifiedGameRuntimeService runtime)
    {
        ArgumentNullException.ThrowIfNull(finalPackage);
        ArgumentNullException.ThrowIfNull(overlay);
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(runtime);
        if (!overlay.Passed || !relationships.Passed)
            return Invalid(overlay,
                overlay.Diagnostics.Concat(relationships.Diagnostics)
                    .DefaultIfEmpty(
                        "generated_regional_event.overlay_invalid")
                    .ToList());
        var inputDiagnostics = ValidateInputCorrelation(finalPackage,
            overlay, relationships);
        if (inputDiagnostics.Count > 0)
            return Invalid(overlay, inputDiagnostics);
        if (overlay.EventCount == 0)
        {
            var packageSha256 = overlay.OutputPackageSha256;
            var emptyInventory =
                Array.Empty<GeneratedCampaignRegionalEventInventoryRow>();
            var emptyInventorySha256 =
                GeneratedCampaignChoiceCanonical.Hash(emptyInventory);
            var absentFinalStateHash =
                GeneratedCampaignRegionalEventCorrelationService
                    .EmptyFinalStateHash(packageSha256,
                        emptyInventorySha256);
            var absentPayloadAuthority =
                GeneratedCampaignRegionalEventPayloadAuthorityService
                    .Create(packageSha256, absentFinalStateHash,
                        emptyInventorySha256, emptyInventory, [], []);
            var absent = new
                GameProjectGeneratedCampaignRegionalEventSummary
            {
                StrictProofSchemaVersion =
                    GameProjectGeneratedCampaignRegionalEventSummary
                        .StrictProofSchema,
                Passed = true,
                Status = "ABSENT",
                IdentityPassed = overlay.IdentityPassed,
                PlacementPassed = overlay.PlacementPassed,
                OverlayControlledDeltaPassed =
                    overlay.ControlledDeltaPassed,
                RuntimeQualificationPassed = true,
                LockedStatePassed = true,
                AvailableStatePassed = true,
                ResolvedStatePassed = true,
                ExactlyOncePassed = true,
                ReplayPassed = true,
                ExactPackageSha256 = packageSha256,
                RegionalEventOverlaySha256 =
                    GeneratedCampaignChoiceCanonical.Hash(overlay),
                RegionalEventInventorySha256 =
                    emptyInventorySha256,
                RelationshipBranchMatrixSha256 =
                    relationships.RelationshipBranchMatrixSha256,
                FinalStateHash = absentFinalStateHash,
                EventInventory = emptyInventory,
                Overlay = overlay,
                PayloadAuthority = absentPayloadAuthority,
                HumanReviewFacts =
                [
                    new GeneratedCampaignRegionalEventHumanFact
                    {
                        Label =
                            GeneratedCampaignRegionalEventPayloadAuthorityService
                                .HumanFactLabel,
                        Value =
                            GeneratedCampaignRegionalEventPayloadAuthorityService
                                .SerializeHumanFact(absentPayloadAuthority)
                    }
                ]
            };
            var absentCorrelation =
                GeneratedCampaignRegionalEventCorrelationService.Validate(
                    finalPackage, packageSha256, absent,
                    relationships);
            return absentCorrelation.Passed
                ? absent
                : absent with
                {
                    Passed = false,
                    Status = "INVALID",
                    Diagnostics = absentCorrelation.Diagnostics
                };
        }

        var packageBefore = overlay.OutputPackageSha256;
        var packageObjectBefore =
            GeneratedCampaignChoiceCanonical.Hash(finalPackage);
        var diagnostics = new List<string>();
        var qualifications =
            new List<GeneratedCampaignRegionalEventQualification>();
        var frames = new List<GeneratedCampaignRegionalEventRuntimeFrame>();
        foreach (var binding in overlay.Bindings)
        {
            var relationship = relationships.Overlay?.Bindings
                .SingleOrDefault(item =>
                    item.RelationshipId == binding.RelationshipId);
            if (relationship is null)
            {
                diagnostics.Add(
                    "generated_regional_event.relationship_missing");
                continue;
            }
            var locked = ExecuteLocked(finalPackage, binding, runtime, 1);
            var lockedReplay = ExecuteLocked(finalPackage, binding,
                runtime, 2);
            frames.AddRange(locked.Frames);
            frames.AddRange(lockedReplay.Frames);
            diagnostics.AddRange(locked.Diagnostics);
            diagnostics.AddRange(lockedReplay.Diagnostics);
            var lockedComparison =
                GeneratedCampaignRegionalEventReplayService.Compare(
                    locked.Signature, lockedReplay.Signature);
            diagnostics.AddRange(lockedComparison.Diagnostics);

            var first = ExecuteResolved(finalPackage, binding,
                relationship, combatSummary, runtime, 1);
            frames.AddRange(first.Frames);
            diagnostics.AddRange(first.Diagnostics);
            var replay = ExecuteResolved(finalPackage, binding,
                relationship, combatSummary, runtime, 2);
            frames.AddRange(replay.Frames);
            diagnostics.AddRange(replay.Diagnostics);
            var resolutionComparison =
                GeneratedCampaignRegionalEventReplayService.Compare(
                    first.Signature, replay.Signature);
            diagnostics.AddRange(resolutionComparison.Diagnostics);
            var replayPassed = locked.Passed && lockedReplay.Passed
                                             && lockedComparison.Passed
                                             && first.Passed
                                             && replay.Passed
                                             && resolutionComparison.Passed;
            if (!replayPassed)
                diagnostics.Add(
                    "generated_regional_event.replay_not_equivalent");
            var passed = locked.Passed && lockedReplay.Passed
                                       && first.Passed && replay.Passed
                                       && replayPassed;
            var eventReplaySignatures =
                new[]
                {
                    locked.Signature,
                    lockedReplay.Signature,
                    first.Signature,
                    replay.Signature
                };
            qualifications.Add(new
                GeneratedCampaignRegionalEventQualification
                {
                    RegionalEventId = binding.RegionalEventId,
                    EventKind = binding.EventKind,
                    RelationshipId = binding.RelationshipId,
                    RelationshipBranch = binding.RelationshipBranch,
                    LockedStatePassed = locked.Passed
                                        && lockedReplay.Passed,
                    AvailableStatePassed = first.AvailablePassed,
                    ResolvedStatePassed = first.ResolvedPassed,
                    ExactlyOncePassed = first.ExactlyOncePassed,
                    ReplayPassed = replayPassed,
                    RuntimeStartCount = 4,
                    RuntimeCommandCount = locked.Frames.Count
                                          + lockedReplay.Frames.Count
                                          + first.Frames.Count
                                          + replay.Frames.Count,
                    FinalStateHash = first.FinalStateHash,
                    ReplaySignatures = eventReplaySignatures,
                    Diagnostics = locked.Diagnostics.Concat(
                            lockedReplay.Diagnostics)
                        .Concat(first.Diagnostics)
                        .Concat(replay.Diagnostics)
                        .Concat(lockedComparison.Diagnostics)
                        .Concat(resolutionComparison.Diagnostics)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal)
                        .ToList()
                });
            if (!passed)
                diagnostics.Add(
                    "generated_regional_event.runtime_route_failed");
        }

        if (packageObjectBefore !=
            GeneratedCampaignChoiceCanonical.Hash(finalPackage))
            diagnostics.Add(
                "generated_regional_event.existing_definition_changed");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        var lockedPassed = qualifications.All(item =>
            item.LockedStatePassed);
        var availablePassed = qualifications.All(item =>
            item.AvailableStatePassed);
        var resolvedPassed = qualifications.All(item =>
            item.ResolvedStatePassed);
        var oncePassed = qualifications.All(item =>
            item.ExactlyOncePassed);
        var replayAllPassed = qualifications.All(item =>
            item.ReplayPassed);
        var replaySignatures = qualifications
            .SelectMany(item => item.ReplaySignatures).ToList();
        var finalStateHash = GeneratedCampaignChoiceCanonical.Hash(
            qualifications.Select(item => new
            {
                item.RegionalEventId,
                item.FinalStateHash,
                ResolutionSignature = item.ReplaySignatures
                    .Single(signature =>
                        signature.RouteKind ==
                        GeneratedCampaignRegionalEventReplayRouteKind
                            .RESOLUTION
                        && signature.ReplayIndex == 1)
                    .SignatureSha256
            }).ToList());
        var payloadAuthority =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .Create(packageBefore, finalStateHash,
                    overlay.InventorySha256, overlay.Inventory,
                    replaySignatures, frames);
        var allPassed = diagnostics.Count == 0
                        && qualifications.Count == overlay.EventCount
                        && lockedPassed && availablePassed
                        && resolvedPassed && oncePassed
                        && replayAllPassed
                        && payloadAuthority.Passed;
        var summary = new GameProjectGeneratedCampaignRegionalEventSummary
        {
            StrictProofSchemaVersion =
                GameProjectGeneratedCampaignRegionalEventSummary
                    .StrictProofSchema,
            Present = true,
            Passed = allPassed,
            Status = allPassed
                ? "REGIONAL_EVENTS_CURRENT"
                : "INVALID",
            EventCount = overlay.EventCount,
            QualifiedEventCount = qualifications.Count(item =>
                item.LockedStatePassed
                && item.AvailableStatePassed
                && item.ResolvedStatePassed
                && item.ExactlyOncePassed
                && item.ReplayPassed),
            SupportGratitudeCount = overlay.SupportGratitudeCount,
            ChallengeAftermathCount = overlay.ChallengeAftermathCount,
            RefusalFalloutCount = overlay.RefusalFalloutCount,
            IdentityPassed = overlay.IdentityPassed,
            PlacementPassed = overlay.PlacementPassed,
            OverlayControlledDeltaPassed =
                overlay.ControlledDeltaPassed,
            RuntimeQualificationPassed = allPassed,
            LockedStatePassed = lockedPassed,
            AvailableStatePassed = availablePassed,
            ResolvedStatePassed = resolvedPassed,
            ExactlyOncePassed = oncePassed,
            ReplayPassed = replayAllPassed,
            ExactPackageSha256 = packageBefore,
            RegionalEventOverlaySha256 =
                GeneratedCampaignChoiceCanonical.Hash(overlay),
            RegionalEventInventorySha256 =
                overlay.InventorySha256,
            RelationshipBranchMatrixSha256 =
                relationships.RelationshipBranchMatrixSha256,
            FinalStateHash = finalStateHash,
            EventInventory = overlay.Inventory,
            EventQualifications = qualifications,
            RuntimeFrames = frames,
            ReplaySignatures = replaySignatures,
            HumanReviewFacts =
            [
                new GeneratedCampaignRegionalEventHumanFact
                {
                    Label = "События мира",
                    Value = overlay.EventCount.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)
                },
                new GeneratedCampaignRegionalEventHumanFact
                {
                    Label = "Благодарности",
                    Value = overlay.SupportGratitudeCount.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)
                },
                new GeneratedCampaignRegionalEventHumanFact
                {
                    Label = "Последствия вызовов и отказов",
                    Value = (overlay.ChallengeAftermathCount
                             + overlay.RefusalFalloutCount).ToString(
                        System.Globalization.CultureInfo.InvariantCulture)
                },
                new GeneratedCampaignRegionalEventHumanFact
                {
                    Label =
                        GeneratedCampaignRegionalEventPayloadAuthorityService
                            .HumanFactLabel,
                    Value =
                        GeneratedCampaignRegionalEventPayloadAuthorityService
                            .SerializeHumanFact(payloadAuthority)
                }
            ],
            TechnicalDetails =
                new SortedDictionary<string, string>(
                    StringComparer.Ordinal)
                {
                    ["regionalEventOverlayPackageSha256"] =
                        overlay.OutputPackageSha256,
                    ["regionalEventInventorySha256"] =
                        overlay.InventorySha256,
                    ["relationshipBranchMatrixSha256"] =
                        relationships.RelationshipBranchMatrixSha256
            },
            Overlay = overlay,
            PayloadAuthority = payloadAuthority,
            Diagnostics = diagnostics
        };
        var correlation =
            GeneratedCampaignRegionalEventCorrelationService.Validate(
                finalPackage, packageBefore, summary, relationships);
        return correlation.Passed
            ? summary
            : summary with
            {
                Passed = false,
                Status = "INVALID",
                RuntimeQualificationPassed = false,
                Diagnostics = summary.Diagnostics.Concat(
                        correlation.Diagnostics)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToList()
            };
    }

    private static IReadOnlyList<string> ValidateInputCorrelation(
        GamePackageDefinition finalPackage,
        GeneratedCampaignRegionalEventOverlayDocument overlay,
        GameProjectGeneratedCampaignRelationshipSummary relationships)
    {
        var diagnostics = new List<string>();
        var inventory = overlay.Bindings
            .OrderBy(item => item.RegionId, StringComparer.Ordinal)
            .ThenBy(item => item.MapId, StringComparer.Ordinal)
            .ThenBy(item => item.RelationshipId,
                StringComparer.Ordinal)
            .ThenBy(item => item.EventKind)
            .Select(GeneratedCampaignRegionalEventInventoryService.Create)
            .ToList();
        if (!PackageSha256Matches(finalPackage,
                overlay.OutputPackageSha256))
            diagnostics.Add(
                "generated_regional_event.overlay_package_hash_mismatch");
        if (GeneratedCampaignChoiceCanonical.Serialize(inventory) !=
            GeneratedCampaignChoiceCanonical.Serialize(
                overlay.Inventory)
            || overlay.InventorySha256 !=
            GeneratedCampaignChoiceCanonical.Hash(inventory))
            diagnostics.Add(
                "generated_regional_event.inventory_mismatch");
        if (overlay.EventCount != overlay.Bindings.Count
            || overlay.EventCount != overlay.Inventory.Count
            || overlay.SupportGratitudeCount != overlay.Inventory.Count(
                item => item.EventKind ==
                        GeneratedCampaignRegionalEventKind
                            .SUPPORT_GRATITUDE)
            || overlay.ChallengeAftermathCount !=
            overlay.Inventory.Count(item => item.EventKind ==
                GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH)
            || overlay.RefusalFalloutCount !=
            overlay.Inventory.Count(item => item.EventKind ==
                GeneratedCampaignRegionalEventKind.REFUSAL_FALLOUT))
            diagnostics.Add(
                "generated_regional_event.inventory_count_mismatch");
        foreach (var fact in relationships.BranchQualifications)
        {
            var kind = fact.Branch switch
            {
                GeneratedCampaignRelationshipBranch.SUPPORT =>
                    GeneratedCampaignRegionalEventKind
                        .SUPPORT_GRATITUDE,
                GeneratedCampaignRelationshipBranch.CHALLENGE =>
                    GeneratedCampaignRegionalEventKind
                        .CHALLENGE_AFTERMATH,
                _ => GeneratedCampaignRegionalEventKind.REFUSAL_FALLOUT
            };
            var count = overlay.Bindings.Count(item =>
                item.RelationshipId == fact.RelationshipId
                && item.RelationshipBranch == fact.Branch
                && item.EventKind == kind);
            if (count != (fact.Available ? 1 : 0))
                diagnostics.Add(
                    "generated_regional_event.branch_event_correlation");
        }
        return diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static LockedRoute ExecuteLocked(
        GamePackageDefinition package,
        GeneratedCampaignRegionalEventBinding binding,
        IUnifiedGameRuntimeService runtime,
        int replayIndex)
    {
        var started = runtime.Start(package);
        if (!started.Success)
            return LockedRoute.Failed(
                "generated_regional_event.runtime_start_failed");
        var contractBefore =
            GeneratedEncounterCombatCanonical.Clone(started.Session);
        var beforeSections = GameplayContractFingerprints(
            contractBefore);
        var frames =
            new List<GeneratedCampaignRegionalEventRuntimeFrame>();
        var opened = NavigateAndOpen(package, binding, runtime,
            started.Session, replayIndex,
            GeneratedCampaignRegionalEventReplayRouteKind.LOCKED_PROBE,
            GeneratedCampaignRegionalEventStatus.LOCKED, frames);
        var status = Status(binding, opened.Session);
        var available = AvailableChoiceIds(opened)
            .Contains(binding.DialogueId + "/resolve");
        var afterSections = GameplayContractFingerprints(
            opened.Session);
        var passed = opened.Success
                     && status ==
                     GeneratedCampaignRegionalEventStatus.LOCKED
                     && !available
                     && beforeSections.SequenceEqual(
                         afterSections, StringComparer.Ordinal);
        var signature =
            GeneratedCampaignRegionalEventReplayService.CreateSignature(
                binding.RegionalEventId,
                GeneratedCampaignRegionalEventReplayRouteKind.LOCKED_PROBE,
                replayIndex, frames);
        if (passed)
            return new LockedRoute(true, frames, signature, []);
        var diagnostic = !opened.Success
            ? "generated_regional_event.locked_route_failed"
            : status != GeneratedCampaignRegionalEventStatus.LOCKED
                ? "generated_regional_event.locked_prerequisite_invalid"
                : available
                    ? "generated_regional_event.locked_choice_available"
                    : "generated_regional_event.locked_mutation:"
                      + ChangedGameplaySections(beforeSections,
                          afterSections);
        return LockedRoute.Failed(diagnostic, frames, signature);
    }

    private ResolvedRoute ExecuteResolved(
        GamePackageDefinition package,
        GeneratedCampaignRegionalEventBinding binding,
        GeneratedCampaignRelationshipBinding relationship,
        GameProjectGeneratedEncounterCombatSummary? combatSummary,
        IUnifiedGameRuntimeService runtime,
        int replayIndex)
    {
        var started = runtime.Start(package);
        if (!started.Success)
            return ResolvedRoute.Failed(
                "generated_regional_event.runtime_start_failed");
        var prerequisite = SatisfyPrerequisite(package, binding,
            relationship, combatSummary, runtime, started.Session,
            replayIndex);
        if (!prerequisite.Passed)
            return ResolvedRoute.Failed(
                prerequisite.Diagnostics.First(), prerequisite.Frames);
        var frames = prerequisite.Frames.ToList();
        var opened = NavigateAndOpen(package, binding, runtime,
            prerequisite.Session, replayIndex,
            GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
            GeneratedCampaignRegionalEventStatus.AVAILABLE, frames);
        var resolveId = binding.DialogueId + "/resolve";
        var availablePassed = opened.Success
                              && Status(binding, opened.Session) ==
                              GeneratedCampaignRegionalEventStatus
                                  .AVAILABLE
                              && AvailableChoiceIds(opened)
                                  .Contains(resolveId);
        if (!availablePassed)
            return ResolvedRoute.Failed(
                "generated_regional_event.resolution_unavailable",
                frames);

        var reputationBefore = Reputation(opened.Session,
            binding.FactionId);
        var before = StableStateHash(opened.Session);
        var resolved = runtime.ExecuteGameplayCommand(package,
            opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(resolveId));
        AddFrame(frames,
            GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
            replayIndex, binding,
            GeneratedCampaignRegionalEventStatus.AVAILABLE,
            GeneratedCampaignRegionalEventStatus.RESOLVED,
            nameof(GameRuntimeCommandType.ChooseDialogueOption) + ":"
            + resolveId,
            before, resolved);
        var expectedDelta = binding.EventKind ==
                            GeneratedCampaignRegionalEventKind
                                .SUPPORT_GRATITUDE
            ? binding.ResolutionReputationDelta
            : 0;
        var resolvedPassed = resolved.Success
                             && Status(binding, resolved.Session) ==
                             GeneratedCampaignRegionalEventStatus.RESOLVED
                             && Flag(resolved.Session,
                                 binding.ResolutionFlagId) ==
                             "RESOLVED"
                             && Exact(Reputation(resolved.Session,
                                          binding.FactionId)
                                      - reputationBefore,
                                 expectedDelta);
        if (!resolvedPassed)
            return ResolvedRoute.Failed(
                "generated_regional_event.resolution_invalid",
                frames);

        var exactBeforeReplay = GameplayContractHash(resolved.Session);
        var interacted = runtime.ExecutePlayerCommand(package,
            resolved.Session, PlayerCommand.Interact());
        AddFrame(frames,
            GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
            replayIndex, binding,
            GeneratedCampaignRegionalEventStatus.RESOLVED,
            GeneratedCampaignRegionalEventStatus.RESOLVED,
            nameof(PlayerCommandType.Interact),
            StableStateHash(resolved.Session), interacted);
        var reopened = runtime.ExecuteGameplayCommand(package,
            interacted.Session,
            GameRuntimeCommand.OpenDialogue(binding.DialogueId));
        AddFrame(frames,
            GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
            replayIndex, binding,
            GeneratedCampaignRegionalEventStatus.RESOLVED,
            GeneratedCampaignRegionalEventStatus.RESOLVED,
            nameof(GameRuntimeCommandType.OpenDialogue) + ":"
            + binding.DialogueId,
            StableStateHash(interacted.Session), reopened);
        var availableIds = AvailableChoiceIds(reopened);
        var resolvedFollowUpId = binding.DialogueId + "/resolved";
        var exactlyOnce = reopened.Success
                          && !availableIds.Contains(resolveId)
                          && availableIds.Contains(resolvedFollowUpId);
        if (exactlyOnce)
        {
            var observed = runtime.ExecuteGameplayCommand(package,
                reopened.Session,
                GameRuntimeCommand.ChooseDialogueOption(
                    resolvedFollowUpId));
            AddFrame(frames,
                GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
                replayIndex, binding,
                GeneratedCampaignRegionalEventStatus.RESOLVED,
                GeneratedCampaignRegionalEventStatus.RESOLVED,
                nameof(GameRuntimeCommandType.ChooseDialogueOption) + ":"
                + resolvedFollowUpId,
                StableStateHash(reopened.Session), observed);
            exactlyOnce = observed.Success
                          && exactBeforeReplay ==
                          GameplayContractHash(observed.Session);
            reopened = observed;
        }
        if (!exactlyOnce)
            return ResolvedRoute.Failed(
                "generated_regional_event.duplicate_resolution",
                frames);
        var signature =
            GeneratedCampaignRegionalEventReplayService.CreateSignature(
                binding.RegionalEventId,
                GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
                replayIndex, frames);
        return new ResolvedRoute(
            true,
            true,
            true,
            true,
            StableStateHash(reopened.Session),
            frames,
            signature,
            []);
    }

    private PrerequisiteRoute SatisfyPrerequisite(
        GamePackageDefinition package,
        GeneratedCampaignRegionalEventBinding binding,
        GeneratedCampaignRelationshipBinding relationship,
        GameProjectGeneratedEncounterCombatSummary? combatSummary,
        IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession initial,
        int replayIndex)
    {
        var frames =
            new List<GeneratedCampaignRegionalEventRuntimeFrame>();
        var before = StableStateHash(initial);
        var opened = runtime.ExecuteGameplayCommand(package, initial,
            GameRuntimeCommand.OpenDialogue(
                relationship.DialogueId));
        AddFrame(frames,
            GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
            replayIndex, binding,
            GeneratedCampaignRegionalEventStatus.LOCKED,
            GeneratedCampaignRegionalEventStatus.LOCKED,
            "Prerequisite.OpenDialogue:"
            + relationship.DialogueId, before, opened);
        var initialChoice = FindRelationshipChoice(package,
            relationship.DialogueId, binding.RelationshipBranch,
            "initial");
        if (!opened.Success || initialChoice is null
                            || !AvailableChoiceIds(opened)
                                .Contains(initialChoice.Id))
            return PrerequisiteRoute.Failed(
                "generated_regional_event.relationship_route_failed",
                frames);
        before = StableStateHash(opened.Session);
        var chosen = runtime.ExecuteGameplayCommand(package,
            opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(initialChoice.Id));
        AddFrame(frames,
            GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
            replayIndex, binding,
            GeneratedCampaignRegionalEventStatus.LOCKED,
            Status(binding, chosen.Session),
            "Prerequisite.ChooseDialogueOption:"
            + initialChoice.Id, before, chosen);
        if (!chosen.Success)
            return PrerequisiteRoute.Failed(
                "generated_regional_event.relationship_route_failed",
                frames);

        if (binding.RelationshipBranch ==
            GeneratedCampaignRelationshipBranch.REFUSE)
            return PrerequisiteRoute.Success(chosen.Session, frames);

        if (binding.RelationshipBranch ==
            GeneratedCampaignRelationshipBranch.CHALLENGE)
        {
            if (combatSummary is null)
                return PrerequisiteRoute.Failed(
                    "generated_regional_event.combat_catalog_missing",
                    frames);
            before = StableStateHash(chosen.Session);
            var combat = _combatRoute.Execute(
                new GeneratedCampaignExactCombatRouteRequest
                {
                    FinalPackage = package,
                    EncounterId =
                        relationship.ChallengeEncounterId,
                    CombatSummary = combatSummary,
                    Runtime = runtime,
                    InitialSession = chosen.Session,
                    Goal =
                        GeneratedCampaignExactCombatRouteGoal.VICTORY
                });
            if (!combat.Passed || !combat.TracePassed)
                return PrerequisiteRoute.Failed(
                    combat.Diagnostics.FirstOrDefault()
                    ?? "generated_regional_event.challenge_victory_failed",
                    frames);
            AddCombatFrames(frames,
                GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
                replayIndex, binding, combat,
                GeneratedCampaignRegionalEventStatus.LOCKED,
                Status(binding, combat.Session));
            before = StableStateHash(combat.Session);
            var reopened = runtime.ExecuteGameplayCommand(package,
                combat.Session,
                GameRuntimeCommand.OpenDialogue(
                    relationship.DialogueId));
            AddFrame(frames,
                GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
                replayIndex, binding,
                Status(binding, combat.Session),
                Status(binding, reopened.Session),
                "Prerequisite.OpenDialogue:"
                + relationship.DialogueId, before, reopened);
            var followUp = FindRelationshipChoice(package,
                relationship.DialogueId,
                GeneratedCampaignRelationshipBranch.CHALLENGE,
                "followup");
            if (!reopened.Success || followUp is null
                                  || !AvailableChoiceIds(reopened)
                                      .Contains(followUp.Id))
                return PrerequisiteRoute.Failed(
                    "generated_regional_event.challenge_followup_missing",
                    frames);
            before = StableStateHash(reopened.Session);
            var confirmed = runtime.ExecuteGameplayCommand(package,
                reopened.Session,
                GameRuntimeCommand.ChooseDialogueOption(followUp.Id));
            AddFrame(frames,
                GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
                replayIndex, binding,
                Status(binding, reopened.Session),
                Status(binding, confirmed.Session),
                "Prerequisite.ChooseDialogueOption:"
                + followUp.Id, before,
                confirmed);
            return confirmed.Success
                   && Status(binding, confirmed.Session) ==
                   GeneratedCampaignRegionalEventStatus.AVAILABLE
                ? PrerequisiteRoute.Success(confirmed.Session, frames)
                : PrerequisiteRoute.Failed(
                    "generated_regional_event.challenge_victory_failed",
                    frames);
        }

        if (combatSummary is null
            && relationship.QuestArc.Any(item =>
                !string.IsNullOrWhiteSpace(
                    item.TargetEncounterId)))
            return PrerequisiteRoute.Failed(
                "generated_regional_event.combat_catalog_missing",
                frames);
        var session = chosen.Session;
        foreach (var step in relationship.QuestArc)
        {
            if (!string.IsNullOrWhiteSpace(step.TargetEncounterId))
            {
                before = StableStateHash(session);
                var combat = _combatRoute.Execute(
                    new GeneratedCampaignExactCombatRouteRequest
                    {
                        FinalPackage = package,
                        EncounterId = step.TargetEncounterId,
                        CombatSummary = combatSummary!,
                        Runtime = runtime,
                        InitialSession = session,
                        Goal =
                            GeneratedCampaignExactCombatRouteGoal.VICTORY
                    });
                if (!combat.Passed || !combat.TracePassed)
                    return PrerequisiteRoute.Failed(
                        combat.Diagnostics.FirstOrDefault()
                        ?? "generated_regional_event.support_combat_failed",
                        frames);
                AddCombatFrames(frames,
                    GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
                    replayIndex, binding, combat,
                    Status(binding, session),
                    Status(binding, combat.Session));
                session = combat.Session;
            }
            before = StableStateHash(session);
            var completed = runtime.ExecuteGameplayCommand(package,
                session,
                new GameRuntimeCommand
                {
                    Type = GameRuntimeCommandType.CompleteQuest,
                    Id = step.QuestId
                });
            if (!completed.Success)
                return PrerequisiteRoute.Failed(
                    "generated_regional_event.support_arc_failed",
                    frames);
            AddFrame(frames,
                GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
                replayIndex, binding,
                Status(binding, session),
                Status(binding, completed.Session),
                "Prerequisite.CompleteQuest:"
                + step.QuestId, before, completed);
            session = completed.Session;
            before = StableStateHash(session);
            var reopened = runtime.ExecuteGameplayCommand(package,
                session,
                GameRuntimeCommand.OpenDialogue(
                    relationship.DialogueId));
            AddFrame(frames,
                GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
                replayIndex, binding,
                Status(binding, session),
                Status(binding, reopened.Session),
                "Prerequisite.OpenDialogue:"
                + relationship.DialogueId, before, reopened);
            var followUp = step.Order + 1 <
                           relationship.QuestArc.Count
                ? package.Game.Dialogues.Single(item =>
                        item.Id == relationship.DialogueId)
                    .Nodes.SelectMany(item => item.Choices)
                    .SingleOrDefault(item =>
                        item.StartQuestId ==
                        relationship.QuestArc[step.Order + 1].QuestId)
                : package.Game.Dialogues.Single(item =>
                        item.Id == relationship.DialogueId)
                    .Nodes.SelectMany(item => item.Choices)
                    .SingleOrDefault(item =>
                        item.Metadata.GetValueOrDefault(
                            "generatedRelationshipPhase")
                        == "followup/completed");
            if (!reopened.Success || followUp is null
                                  || !AvailableChoiceIds(reopened)
                                      .Contains(followUp.Id))
                return PrerequisiteRoute.Failed(
                    "generated_regional_event.support_followup_missing",
                    frames);
            before = StableStateHash(reopened.Session);
            var observed = runtime.ExecuteGameplayCommand(package,
                reopened.Session,
                GameRuntimeCommand.ChooseDialogueOption(followUp.Id));
            AddFrame(frames,
                GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION,
                replayIndex, binding,
                Status(binding, reopened.Session),
                Status(binding, observed.Session),
                "Prerequisite.ChooseDialogueOption:"
                + followUp.Id, before,
                observed);
            if (!observed.Success)
                return PrerequisiteRoute.Failed(
                    "generated_regional_event.support_followup_failed",
                    frames);
            session = observed.Session;
        }
        return Status(binding, session) ==
               GeneratedCampaignRegionalEventStatus.AVAILABLE
            ? PrerequisiteRoute.Success(session, frames)
            : PrerequisiteRoute.Failed(
                "generated_regional_event.support_arc_failed",
                frames);
    }

    private static UnifiedRuntimeResult NavigateAndOpen(
        GamePackageDefinition package,
        GeneratedCampaignRegionalEventBinding binding,
        IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession initial,
        int replayIndex,
        GeneratedCampaignRegionalEventReplayRouteKind routeKind,
        GeneratedCampaignRegionalEventStatus expectedStatus,
        ICollection<GeneratedCampaignRegionalEventRuntimeFrame> frames)
    {
        var session = initial;
        var mapPath = MapPath(package, session.MapState.CurrentMapId,
            binding.MapId);
        if (mapPath is null)
            return FailedResult(session);
        foreach (var transition in mapPath)
        {
            var moved = MoveAdjacent(package, runtime, session,
                transition.EntityId);
            foreach (var result in moved.Results)
            {
                AddFrame(frames, routeKind, replayIndex, binding,
                    expectedStatus, expectedStatus,
                    result.CommandType, result.BeforeHash,
                    result.Result);
                session = result.Result.Session;
            }
            if (!moved.Passed)
                return FailedResult(session);
            var before = StableStateHash(session);
            var interacted = runtime.ExecutePlayerCommand(package,
                session, PlayerCommand.Interact());
            AddFrame(frames, routeKind, replayIndex, binding,
                expectedStatus, expectedStatus,
                nameof(PlayerCommandType.Interact) + ":"
                + transition.EntityId, before,
                interacted);
            if (!interacted.Success
                || interacted.Session.MapState.CurrentMapId !=
                transition.DestinationMapId)
                return FailedResult(interacted.Session);
            session = interacted.Session;
        }

        var eventMove = MoveAdjacent(package, runtime, session,
            binding.MapEntityId);
        foreach (var result in eventMove.Results)
        {
            AddFrame(frames, routeKind, replayIndex, binding,
                expectedStatus, expectedStatus,
                result.CommandType, result.BeforeHash,
                result.Result);
            session = result.Result.Session;
        }
        if (!eventMove.Passed)
            return FailedResult(session);
        var interactBefore = StableStateHash(session);
        var eventInteraction = runtime.ExecutePlayerCommand(package,
            session, PlayerCommand.Interact());
        AddFrame(frames, routeKind, replayIndex, binding,
            expectedStatus, expectedStatus,
            nameof(PlayerCommandType.Interact) + ":"
            + binding.MapEntityId, interactBefore,
            eventInteraction);
        if (!eventInteraction.Success
            || !eventInteraction.MapEvents.Any(item =>
                item.Type == RuntimeEventType.DialogueRequested
                && item.TargetId == binding.DialogueId))
            return FailedResult(eventInteraction.Session);
        var openBefore = StableStateHash(eventInteraction.Session);
        var opened = runtime.ExecuteGameplayCommand(package,
            eventInteraction.Session,
            GameRuntimeCommand.OpenDialogue(binding.DialogueId));
        AddFrame(frames, routeKind, replayIndex, binding,
            expectedStatus, expectedStatus,
            nameof(GameRuntimeCommandType.OpenDialogue) + ":"
            + binding.DialogueId, openBefore,
            opened);
        return opened;
    }

    private static MoveRoute MoveAdjacent(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession initial,
        string targetEntityId)
    {
        var map = package.Game.Maps.SingleOrDefault(item =>
            item.Id == initial.MapState.CurrentMapId);
        var target = map?.Entities.SingleOrDefault(item =>
            item.Id == targetEntityId);
        if (map is null || target is null)
            return new MoveRoute(false, []);
        var path = PathToInteractionCell(package, map,
            (initial.MapState.PlayerPosition.X,
                initial.MapState.PlayerPosition.Y),
            target);
        if (path is null)
            return new MoveRoute(false, []);
        var session = initial;
        var results = new List<MoveStep>();
        foreach (var direction in path)
        {
            var before = StableStateHash(session);
            var result = runtime.ExecutePlayerCommand(package,
                session, PlayerCommand.Move(direction));
            results.Add(new MoveStep(
                nameof(PlayerCommandType.Move) + "." + direction,
                before,
                result));
            if (!result.Success)
                return new MoveRoute(false, results);
            session = result.Session;
        }
        return new MoveRoute(true, results);
    }

    private static IReadOnlyList<Direction2D>? PathToInteractionCell(
        GamePackageDefinition package,
        MapDefinition map,
        (int X, int Y) start,
        EntityInstanceDefinition target)
    {
        var queue = new Queue<(int X, int Y)>();
        var previous = new Dictionary<(int X, int Y),
            ((int X, int Y) Cell, Direction2D Direction)>();
        var visited = new HashSet<(int X, int Y)> { start };
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (IsAdjacent(current,
                    (target.Position.X, target.Position.Y))
                && FirstInteractable(package, map, current)?.Id ==
                target.Id)
                return Reconstruct(start, current, previous);
            foreach (var step in Steps())
            {
                var next = (current.X + step.Dx,
                    current.Y + step.Dy);
                if (!visited.Add(next)
                    || !Walkable(package, map, next.Item1,
                        next.Item2)
                    || Blocking(package, map, next.Item1,
                        next.Item2))
                    continue;
                previous[next] = (current, step.Direction);
                queue.Enqueue(next);
            }
        }
        return null;
    }

    private static IReadOnlyList<Direction2D> Reconstruct(
        (int X, int Y) start,
        (int X, int Y) end,
        IReadOnlyDictionary<(int X, int Y),
            ((int X, int Y) Cell, Direction2D Direction)> previous)
    {
        var result = new List<Direction2D>();
        var current = end;
        while (current != start)
        {
            var step = previous[current];
            result.Add(step.Direction);
            current = step.Cell;
        }
        result.Reverse();
        return result;
    }

    private static EntityInstanceDefinition? FirstInteractable(
        GamePackageDefinition package,
        MapDefinition map,
        (int X, int Y) player) =>
        map.Entities.FirstOrDefault(entity =>
            IsAdjacent(player,
                (entity.Position.X, entity.Position.Y))
            && Components(package, entity).Any(component =>
                string.Equals(component.Type, "interactable",
                    StringComparison.OrdinalIgnoreCase)));

    private static IReadOnlyList<MapTransition>? MapPath(
        GamePackageDefinition package,
        string fromMapId,
        string toMapId)
    {
        if (fromMapId == toMapId)
            return [];
        var transitions = package.Game.Maps.SelectMany(map =>
                map.Entities.Select(entity => (map, entity)))
            .Select(pair =>
            {
                var component = Components(package, pair.entity)
                    .SingleOrDefault(item =>
                        string.Equals(item.Type, "interactable",
                            StringComparison.OrdinalIgnoreCase)
                        && item.Args.GetValueOrDefault(
                            MapTransitionInteractionContract
                                .TransitionKindKey) ==
                        MapTransitionInteractionContract
                            .TransitionKindMap);
                return component is null
                    ? null
                    : new MapTransition(pair.map.Id, pair.entity.Id,
                        component.Args.GetValueOrDefault(
                            MapTransitionInteractionContract
                                .DestinationMapIdKey)
                        ?? string.Empty);
            })
            .Where(item => item is not null
                           && !string.IsNullOrWhiteSpace(
                               item.DestinationMapId))
            .Select(item => item!)
            .OrderBy(item => item.SourceMapId, StringComparer.Ordinal)
            .ThenBy(item => item.DestinationMapId,
                StringComparer.Ordinal)
            .ThenBy(item => item.EntityId, StringComparer.Ordinal)
            .ToList();
        var queue = new Queue<string>();
        var previous = new Dictionary<string,
            (string MapId, MapTransition Transition)>(
            StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal)
            { fromMapId };
        queue.Enqueue(fromMapId);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var edge in transitions.Where(item =>
                         item.SourceMapId == current))
            {
                if (!visited.Add(edge.DestinationMapId))
                    continue;
                previous[edge.DestinationMapId] = (current, edge);
                if (edge.DestinationMapId == toMapId)
                {
                    var result = new List<MapTransition>();
                    var map = toMapId;
                    while (map != fromMapId)
                    {
                        var step = previous[map];
                        result.Add(step.Transition);
                        map = step.MapId;
                    }
                    result.Reverse();
                    return result;
                }
                queue.Enqueue(edge.DestinationMapId);
            }
        }
        return null;
    }

    private static IEnumerable<ComponentDefinition> Components(
        GamePackageDefinition package,
        EntityInstanceDefinition entity) =>
        entity.Components.Concat(package.Game.EntityPrototypes
            .SingleOrDefault(item => item.Id == entity.PrototypeId)
            ?.Components ?? []);

    private static bool Blocking(
        GamePackageDefinition package,
        MapDefinition map,
        int x,
        int y) => map.Entities.Any(entity =>
        entity.Position.X == x && entity.Position.Y == y
                               && Components(package, entity).Any(component =>
                                   string.Equals(component.Type,
                                       "collidable",
                                       StringComparison
                                           .OrdinalIgnoreCase)));

    private static bool Walkable(
        GamePackageDefinition package,
        MapDefinition map,
        int x,
        int y)
    {
        if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
            return false;
        var tileId = map.Tiles.LastOrDefault(item =>
            item.X == x && item.Y == y)?.TileId ?? map.DefaultTileId;
        return package.Game.TilePrototypes.SingleOrDefault(item =>
            item.Id == tileId)?.Walkable == true;
    }

    private static IEnumerable<(int Dx, int Dy,
        Direction2D Direction)> Steps()
    {
        yield return (0, -1, Direction2D.Up);
        yield return (-1, 0, Direction2D.Left);
        yield return (1, 0, Direction2D.Right);
        yield return (0, 1, Direction2D.Down);
    }

    private static bool IsAdjacent(
        (int X, int Y) left,
        (int X, int Y) right) =>
        Math.Abs(left.X - right.X)
        + Math.Abs(left.Y - right.Y) == 1;

    private static DialogueChoiceDefinition? FindRelationshipChoice(
        GamePackageDefinition package,
        string dialogueId,
        GeneratedCampaignRelationshipBranch branch,
        string phase) =>
        package.Game.Dialogues.Single(item => item.Id == dialogueId)
            .Nodes.SelectMany(item => item.Choices)
            .SingleOrDefault(item =>
                item.Metadata.GetValueOrDefault("generatedChoiceKind")
                == branch.ToString()
                && (phase == "initial"
                    ? item.Metadata.GetValueOrDefault(
                        "generatedChoicePhase") == "initial"
                    : (item.Metadata.GetValueOrDefault(
                           "generatedChoicePhase") ?? string.Empty)
                    .StartsWith("followup",
                        StringComparison.Ordinal)));

    public static GeneratedCampaignRegionalEventStatus Status(
        GeneratedCampaignRegionalEventBinding binding,
        UnifiedRuntimeSession session)
    {
        if (Flag(session, binding.ResolutionFlagId) == "RESOLVED")
            return GeneratedCampaignRegionalEventStatus.RESOLVED;
        if (Flag(session,
                binding.Prerequisite.DecisionFlagId) !=
            binding.Prerequisite.DecisionFlagValue)
            return GeneratedCampaignRegionalEventStatus.LOCKED;
        if (binding.Prerequisite.CompletedQuestIds.Any(id =>
                QuestState(session, id) != "completed"))
            return GeneratedCampaignRegionalEventStatus.LOCKED;
        if (!string.IsNullOrWhiteSpace(
                binding.Prerequisite.ChallengeVictoryFlagId)
            && (Flag(session,
                    binding.Prerequisite.ChallengeVictoryFlagId)
                != "VICTORY"
                || !EncounterVictory(session,
                    binding.Prerequisite.ChallengeEncounterId)))
            return GeneratedCampaignRegionalEventStatus.LOCKED;
        return GeneratedCampaignRegionalEventStatus.AVAILABLE;
    }

    private static bool EncounterVictory(
        UnifiedRuntimeSession session,
        string encounterId)
    {
        var encounter = session.GameplayState.ActiveEncounter;
        return encounter is { Active: false }
               && encounter.EncounterId == encounterId
               && encounter.Participants.Any(item =>
                   item.Alive && string.Equals(item.Team, "player",
                       StringComparison.OrdinalIgnoreCase))
               && !encounter.Participants.Any(item =>
                   item.Alive && !string.Equals(item.Team, "player",
                       StringComparison.OrdinalIgnoreCase));
    }

    private static HashSet<string> AvailableChoiceIds(
        UnifiedRuntimeResult result)
    {
        var value = result.GameplayEvents.LastOrDefault(item =>
                item.Type is GameRuntimeEventType.DialogueOpened
                    or GameRuntimeEventType.DialogueNodeChanged)
            ?.Args.GetValueOrDefault("choiceIds");
        return value?.Split(',',
                   StringSplitOptions.RemoveEmptyEntries
                   | StringSplitOptions.TrimEntries)
                   .ToHashSet(StringComparer.Ordinal)
               ?? [];
    }

    private static string Flag(UnifiedRuntimeSession session, string id) =>
        session.GameplayState.Flags.SingleOrDefault(item =>
            item.Id == id)?.Value ?? string.Empty;

    private static string QuestState(
        UnifiedRuntimeSession session,
        string id) =>
        session.GameplayState.Quests.SingleOrDefault(item =>
            item.QuestId == id)?.State
        ?? session.GameplayState.QuestStates.GetValueOrDefault(id)
        ?? "not_started";

    private static double Reputation(
        UnifiedRuntimeSession session,
        string id) =>
        session.GameplayState.Factions.SingleOrDefault(item =>
            item.FactionId == id)?.Reputation ?? 0;

    private static string GameplayContractHash(
        UnifiedRuntimeSession session) =>
        GeneratedCampaignChoiceCanonical.Hash(
            GameplayContractFingerprints(session));

    private static IReadOnlyList<string>
        GameplayContractFingerprints(
            UnifiedRuntimeSession session) =>
        [
            GeneratedCampaignChoiceCanonical.Hash(
                session.GameplayState.Flags),
            GeneratedCampaignChoiceCanonical.Hash(
                session.GameplayState.Factions),
            GeneratedCampaignChoiceCanonical.Hash(
                session.GameplayState.QuestStates),
            GeneratedCampaignChoiceCanonical.Hash(
                session.GameplayState.Quests),
            GeneratedCampaignChoiceCanonical.Hash(
                session.GameplayState.Inventories),
            GeneratedCampaignChoiceCanonical.Hash(
                session.GameplayState.ActiveEncounter)
        ];

    private static string ChangedGameplaySections(
        IReadOnlyList<string> before,
        IReadOnlyList<string> after)
    {
        string[] names =
        [
            "flags", "factions", "quest-states", "quests",
            "inventories", "encounter"
        ];
        return string.Join(",", names.Where((_, index) =>
            before[index] != after[index]));
    }

    private static string StableStateHash(
        UnifiedRuntimeSession session) =>
        GeneratedCampaignChoiceCanonical.Hash(new
        {
            session.MapState,
            session.GameplayState
        });

    private static void AddCombatFrames(
        ICollection<GeneratedCampaignRegionalEventRuntimeFrame> frames,
        GeneratedCampaignRegionalEventReplayRouteKind routeKind,
        int replayIndex,
        GeneratedCampaignRegionalEventBinding binding,
        GeneratedCampaignExactCombatRouteResult combat,
        GeneratedCampaignRegionalEventStatus beforeStatus,
        GeneratedCampaignRegionalEventStatus afterStatus)
    {
        var nestedSequence = frames.Count(item => item.NestedCombat);
        var previousReputation = frames.LastOrDefault()
            ?.ObservedReputation ?? 0;
        foreach (var step in combat.Trace.OrderBy(item =>
                     item.SequenceIndex))
        {
            var last = step.SequenceIndex == combat.Trace.Count - 1;
            var reputation = last
                ? Reputation(combat.Session, binding.FactionId)
                : previousReputation;
            frames.Add(new GeneratedCampaignRegionalEventRuntimeFrame
            {
                RouteKind = routeKind,
                ReplayIndex = replayIndex,
                SequenceIndex = frames.Count,
                RegionalEventId = binding.RegionalEventId,
                StatusBefore = beforeStatus,
                StatusAfter = last ? afterStatus : beforeStatus,
                CommandType = "Prerequisite.ExactCombat."
                              + step.CommandType,
                BeforeStateHash = step.BeforeStateHash,
                AfterStateHash = step.AfterStateHash,
                CommandSha256 = step.CommandIdentity,
                EventSha256 = GeneratedCampaignChoiceCanonical.Hash(
                    new
                    {
                        step.MapEventSequenceSha256,
                        step.GameplayEventSequenceSha256
                    }),
                MapEventSha256 = step.MapEventSequenceSha256,
                GameplayEventSha256 =
                    step.GameplayEventSequenceSha256,
                AvailableChoiceIdsSha256 =
                    GeneratedCampaignChoiceCanonical.Hash(
                        Array.Empty<string>()),
                ObservedReputation = reputation,
                ObservedReputationDelta =
                    reputation - previousReputation,
                ObservedResolutionFlag = string.Empty,
                RelationshipFlagsSha256 =
                    GeneratedCampaignChoiceCanonical.Hash(new
                    {
                        step.AfterStateHash,
                        Dimension = "relationship_flags"
                    }),
                QuestStatesSha256 =
                    GeneratedCampaignChoiceCanonical.Hash(new
                    {
                        step.AfterStateHash,
                        Dimension = "quest_states"
                    }),
                EncounterStateSha256 =
                    step.EncounterStateAfterSha256,
                NestedCombat = true,
                NestedCombatSequenceIndex = nestedSequence++,
                NestedCombatCommandIdentity =
                    step.CommandIdentity,
                QualifiedDescriptorFingerprint =
                    step.QualifiedDescriptorFingerprint,
                AbilityDefinitionSha256 =
                    step.AbilityDefinitionSha256,
                ObservedEffectClass = step.ObservedEffectClass,
                ObservedEffectFingerprint =
                    step.ObservedEffectFingerprint,
                NestedCombatMapEventSequenceSha256 =
                    step.MapEventSequenceSha256,
                NestedCombatGameplayEventSequenceSha256 =
                    step.GameplayEventSequenceSha256,
                EncounterStateBeforeSha256 =
                    step.EncounterStateBeforeSha256,
                EncounterStateAfterSha256 =
                    step.EncounterStateAfterSha256,
                TurnBefore = step.TurnBefore,
                TurnAfter = step.TurnAfter,
                RoundBefore = step.RoundBefore,
                RoundAfter = step.RoundAfter,
                CombatProgressObserved = step.ProgressObserved,
                CombatOutcome = step.Outcome,
                Passed = step.Passed
            });
            previousReputation = reputation;
        }
    }

    private static void AddFrame(
        ICollection<GeneratedCampaignRegionalEventRuntimeFrame> frames,
        GeneratedCampaignRegionalEventReplayRouteKind routeKind,
        int replayIndex,
        GeneratedCampaignRegionalEventBinding binding,
        GeneratedCampaignRegionalEventStatus beforeStatus,
        GeneratedCampaignRegionalEventStatus afterStatus,
        string command,
        string before,
        UnifiedRuntimeResult result)
    {
        var afterReputation = Reputation(result.Session,
            binding.FactionId);
        var previousReputation = frames
            .Where(item => item.RegionalEventId ==
                           binding.RegionalEventId
                           && item.RouteKind == routeKind
                           && item.ReplayIndex == replayIndex)
            .LastOrDefault()?.ObservedReputation;
        var beforeReputation = previousReputation is null
            ? afterReputation
            : previousReputation.Value;
        var mapEventSha256 =
            GeneratedCampaignChoiceCanonical.Hash(result.MapEvents);
        var gameplayEventSha256 =
            GeneratedCampaignChoiceCanonical.Hash(
                result.GameplayEvents);
        var choices = AvailableChoiceIds(result)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        frames.Add(new GeneratedCampaignRegionalEventRuntimeFrame
        {
            RouteKind = routeKind,
            ReplayIndex = replayIndex,
            SequenceIndex = frames.Count,
            RegionalEventId = binding.RegionalEventId,
            StatusBefore = beforeStatus,
            StatusAfter = Status(binding, result.Session),
            CommandType = command,
            BeforeStateHash = before,
            AfterStateHash = StableStateHash(result.Session),
            CommandSha256 =
                GeneratedCampaignChoiceCanonical.Hash(command),
            EventSha256 = GeneratedCampaignChoiceCanonical.Hash(new
            {
                result.MapEvents,
                result.GameplayEvents
            }),
            MapEventSha256 = mapEventSha256,
            GameplayEventSha256 = gameplayEventSha256,
            AvailableChoiceIdsSha256 =
                GeneratedCampaignChoiceCanonical.Hash(choices),
            ObservedReputation = afterReputation,
            ObservedReputationDelta =
                afterReputation - beforeReputation,
            ObservedResolutionFlag =
                Flag(result.Session, binding.ResolutionFlagId),
            RelationshipFlagsSha256 =
                GeneratedCampaignChoiceCanonical.Hash(new
                {
                    MapFlags = result.Session.MapState.Flags,
                    GameplayFlags =
                        result.Session.GameplayState.Flags
                }),
            QuestStatesSha256 =
                GeneratedCampaignChoiceCanonical.Hash(new
                {
                    result.Session.GameplayState.QuestStates,
                    result.Session.GameplayState.Quests
                }),
            EncounterStateSha256 =
                GeneratedCampaignChoiceCanonical.Hash(
                    result.Session.GameplayState.ActiveEncounter),
            Passed = result.Success
        });
    }

    private static UnifiedRuntimeResult FailedResult(
        UnifiedRuntimeSession session) => new()
    {
        Success = false,
        Session = session
    };

    private static string PackageSha256(GamePackageDefinition package) =>
        GeneratedEncounterCombatCanonical.HashText(
            GeneratedEncounterCombatCanonical.Serialize(package)
            + Environment.NewLine);

    private static bool PackageSha256Matches(
        GamePackageDefinition package,
        string expected) =>
        PackageSha256(package) == expected
        || GeneratedCampaignChoiceCanonical.HashText(
            GeneratedCampaignChoiceCanonical.Serialize(package)
            + Environment.NewLine) == expected;

    private static bool Exact(double left, double right) =>
        Math.Abs(left - right) < 0.0000001;

    private static GameProjectGeneratedCampaignRegionalEventSummary Invalid(
        GeneratedCampaignRegionalEventOverlayDocument overlay,
        IReadOnlyList<string> diagnostics) => new()
    {
        Present = overlay.EventCount > 0,
        Status = overlay.EventCount > 0 ? "INVALID" : "ABSENT",
        EventCount = overlay.EventCount,
        Overlay = overlay,
        Diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList()
    };

    private sealed record LockedRoute(
        bool Passed,
        IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame> Frames,
        GeneratedCampaignRegionalEventReplaySignature Signature,
        IReadOnlyList<string> Diagnostics)
    {
        public static LockedRoute Failed(
            string diagnostic,
            IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame>?
                frames = null,
            GeneratedCampaignRegionalEventReplaySignature?
                signature = null) =>
            new(false, frames ?? [], signature ?? new(), [diagnostic]);
    }

    private sealed record ResolvedRoute(
        bool Passed,
        bool AvailablePassed,
        bool ResolvedPassed,
        bool ExactlyOncePassed,
        string FinalStateHash,
        IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame> Frames,
        GeneratedCampaignRegionalEventReplaySignature Signature,
        IReadOnlyList<string> Diagnostics)
    {
        public static ResolvedRoute Failed(
            string diagnostic,
            IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame>?
                frames = null,
            GeneratedCampaignRegionalEventReplaySignature?
                signature = null) =>
            new(false, false, false, false, string.Empty,
                frames ?? [], signature ?? new(), [diagnostic]);
    }

    private sealed record PrerequisiteRoute(
        bool Passed,
        UnifiedRuntimeSession Session,
        IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame> Frames,
        IReadOnlyList<string> Diagnostics)
    {
        public static PrerequisiteRoute Success(
            UnifiedRuntimeSession session,
            IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame>
                frames) =>
            new(true, session, frames, []);

        public static PrerequisiteRoute Failed(
            string diagnostic,
            IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame>?
                frames = null) =>
            new(false, new UnifiedRuntimeSession(), frames ?? [],
                [diagnostic]);
    }

    private sealed record MoveStep(
        string CommandType,
        string BeforeHash,
        UnifiedRuntimeResult Result);

    private sealed record MoveRoute(
        bool Passed,
        IReadOnlyList<MoveStep> Results);

    private sealed record MapTransition(
        string SourceMapId,
        string EntityId,
        string DestinationMapId);
}
