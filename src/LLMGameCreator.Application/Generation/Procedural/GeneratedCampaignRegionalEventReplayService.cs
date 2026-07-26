namespace LLMGameCreator.Application.Generation.Procedural;

public static class GeneratedCampaignRegionalEventReplayService
{
    public static GeneratedCampaignRegionalEventReplaySignature CreateSignature(
        string regionalEventId,
        GeneratedCampaignRegionalEventReplayRouteKind routeKind,
        int replayIndex,
        IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame> frames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionalEventId);
        ArgumentNullException.ThrowIfNull(frames);

        var ordered = frames.OrderBy(item => item.SequenceIndex).ToList();
        var nestedCombat = ordered.Where(item => item.NestedCombat)
            .ToList();
        var ownershipPassed = ordered.Count > 0
                              && ordered.Select(item => item.SequenceIndex)
                                  .SequenceEqual(Enumerable.Range(0,
                                      ordered.Count))
                              && ordered.All(item =>
                                  item.RegionalEventId == regionalEventId
                                  && item.RouteKind == routeKind
                                  && item.ReplayIndex == replayIndex)
                              && ordered.Zip(ordered.Skip(1),
                                      (left, right) =>
                                          !left.NestedCombat
                                          && !right.NestedCombat
                                          || left.AfterStateHash
                                          == right.BeforeStateHash)
                                  .All(item => item)
                              && nestedCombat.Select(item =>
                                      item.NestedCombatSequenceIndex)
                                  .SequenceEqual(Enumerable.Range(0,
                                      nestedCombat.Count));
        var signature = new GeneratedCampaignRegionalEventReplaySignature
        {
            RegionalEventId = regionalEventId,
            RouteKind = routeKind,
            ReplayIndex = replayIndex,
            FrameCount = ordered.Count,
            CommandSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(ordered.Select(item =>
                    new { item.CommandType, item.CommandSha256 }).ToList()),
            MapEventSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(ordered.Select(item =>
                    item.MapEventSha256).ToList()),
            GameplayEventSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(ordered.Select(item =>
                    item.GameplayEventSha256).ToList()),
            StatusTransitionSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(ordered.Select(item =>
                    new { item.StatusBefore, item.StatusAfter }).ToList()),
            StateHashChainSha256 =
                GeneratedCampaignChoiceCanonical.Hash(ordered.Select(item =>
                    new { item.BeforeStateHash, item.AfterStateHash }).ToList()),
            AvailableChoiceSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(ordered.Select(item =>
                    item.AvailableChoiceIdsSha256).ToList()),
            ReputationSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(ordered.Select(item =>
                    new
                    {
                        item.ObservedReputation,
                        item.ObservedReputationDelta
                    }).ToList()),
            ResolutionFlagSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(ordered.Select(item =>
                    item.ObservedResolutionFlag).ToList()),
            RelationshipFlagSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(ordered.Select(item =>
                    item.RelationshipFlagsSha256).ToList()),
            QuestStateSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(ordered.Select(item =>
                    item.QuestStatesSha256).ToList()),
            EncounterStateSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(ordered.Select(item =>
                    item.EncounterStateSha256).ToList()),
            NestedCombatFrameCount = nestedCombat.Count,
            NestedCombatCommandSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(nestedCombat.Select(
                    item => new
                    {
                        item.NestedCombatSequenceIndex,
                        item.CommandType,
                        item.NestedCombatCommandIdentity,
                        item.CommandSha256
                    }).ToList()),
            NestedCombatEventSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(nestedCombat.Select(
                    item => new
                    {
                        item.NestedCombatMapEventSequenceSha256,
                        item.NestedCombatGameplayEventSequenceSha256,
                        item.EventSha256
                    }).ToList()),
            NestedCombatDescriptorSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(nestedCombat.Select(
                    item => new
                    {
                        item.QualifiedDescriptorFingerprint,
                        item.AbilityDefinitionSha256
                    }).ToList()),
            NestedCombatEffectSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(nestedCombat.Select(
                    item => new
                    {
                        item.ObservedEffectClass,
                        item.ObservedEffectFingerprint,
                        item.CombatProgressObserved,
                        item.CombatOutcome
                    }).ToList()),
            NestedCombatEncounterStateChainSha256 =
                GeneratedCampaignChoiceCanonical.Hash(nestedCombat.Select(
                    item => new
                    {
                        item.EncounterStateBeforeSha256,
                        item.EncounterStateAfterSha256
                    }).ToList()),
            NestedCombatTurnSequenceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(nestedCombat.Select(
                    item => new
                    {
                        item.TurnBefore,
                        item.TurnAfter,
                        item.RoundBefore,
                        item.RoundAfter
                    }).ToList()),
            NestedCombatTraceSha256 =
                GeneratedCampaignChoiceCanonical.Hash(nestedCombat.Select(
                    item => new
                    {
                        item.SequenceIndex,
                        item.NestedCombatSequenceIndex,
                        item.NestedCombatCommandIdentity,
                        item.QualifiedDescriptorFingerprint,
                        item.AbilityDefinitionSha256,
                        item.ObservedEffectClass,
                        item.ObservedEffectFingerprint,
                        item.NestedCombatMapEventSequenceSha256,
                        item.NestedCombatGameplayEventSequenceSha256,
                        item.EncounterStateBeforeSha256,
                        item.EncounterStateAfterSha256,
                        item.TurnBefore,
                        item.TurnAfter,
                        item.RoundBefore,
                        item.RoundAfter,
                        item.CombatProgressObserved,
                        item.CombatOutcome
                    }).ToList()),
            FinalStateHash = ordered.LastOrDefault()?.AfterStateHash
                             ?? string.Empty,
            Passed = ownershipPassed && ordered.All(item => item.Passed)
        };
        return signature with
        {
            SignatureSha256 = ComparableSha256(signature)
        };
    }

    public static GeneratedCampaignRegionalEventReplayComparison
        ValidateSignature(
            GeneratedCampaignRegionalEventReplaySignature signature,
            IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame> frames)
    {
        ArgumentNullException.ThrowIfNull(signature);
        ArgumentNullException.ThrowIfNull(frames);
        var recomputed = CreateSignature(signature.RegionalEventId,
            signature.RouteKind, signature.ReplayIndex, frames);
        var diagnostics = Dimensions(signature, recomputed,
            requireReplayPair: false);
        return new GeneratedCampaignRegionalEventReplayComparison
        {
            Passed = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
    }

    public static GeneratedCampaignRegionalEventReplayComparison Compare(
        GeneratedCampaignRegionalEventReplaySignature first,
        GeneratedCampaignRegionalEventReplaySignature second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        var diagnostics = Dimensions(first, second,
            requireReplayPair: true);
        return new GeneratedCampaignRegionalEventReplayComparison
        {
            Passed = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
    }

    private static List<string> Dimensions(
        GeneratedCampaignRegionalEventReplaySignature first,
        GeneratedCampaignRegionalEventReplaySignature second,
        bool requireReplayPair)
    {
        var diagnostics = new List<string>();
        if (first.RegionalEventId != second.RegionalEventId
            || first.RouteKind != second.RouteKind
            || requireReplayPair
            && (first.ReplayIndex != 1 || second.ReplayIndex != 2)
            || !requireReplayPair
            && first.ReplayIndex != second.ReplayIndex)
            diagnostics.Add(
                "generated_regional_event.replay_mismatch.route_identity");
        Compare(first.FrameCount, second.FrameCount, "frame_count",
            diagnostics);
        Compare(first.CommandSequenceSha256,
            second.CommandSequenceSha256, "command_sequence",
            diagnostics);
        Compare(first.MapEventSequenceSha256,
            second.MapEventSequenceSha256, "map_event_sequence",
            diagnostics);
        Compare(first.GameplayEventSequenceSha256,
            second.GameplayEventSequenceSha256,
            "gameplay_event_sequence", diagnostics);
        Compare(first.StatusTransitionSequenceSha256,
            second.StatusTransitionSequenceSha256,
            "status_transitions", diagnostics);
        Compare(first.StateHashChainSha256,
            second.StateHashChainSha256, "state_hash_chain",
            diagnostics);
        Compare(first.AvailableChoiceSequenceSha256,
            second.AvailableChoiceSequenceSha256,
            "available_choices", diagnostics);
        Compare(first.ReputationSequenceSha256,
            second.ReputationSequenceSha256, "reputation_sequence",
            diagnostics);
        Compare(first.ResolutionFlagSequenceSha256,
            second.ResolutionFlagSequenceSha256,
            "resolution_flags", diagnostics);
        Compare(first.RelationshipFlagSequenceSha256,
            second.RelationshipFlagSequenceSha256,
            "relationship_flags", diagnostics);
        Compare(first.QuestStateSequenceSha256,
            second.QuestStateSequenceSha256, "quest_states",
            diagnostics);
        Compare(first.EncounterStateSequenceSha256,
            second.EncounterStateSequenceSha256, "encounter_state",
            diagnostics);
        Compare(first.NestedCombatFrameCount,
            second.NestedCombatFrameCount, "nested_combat_frame_count",
            diagnostics);
        Compare(first.NestedCombatCommandSequenceSha256,
            second.NestedCombatCommandSequenceSha256,
            "nested_combat_command_sequence", diagnostics);
        Compare(first.NestedCombatEventSequenceSha256,
            second.NestedCombatEventSequenceSha256,
            "nested_combat_event_sequence", diagnostics);
        Compare(first.NestedCombatDescriptorSequenceSha256,
            second.NestedCombatDescriptorSequenceSha256,
            "nested_combat_descriptor_sequence", diagnostics);
        Compare(first.NestedCombatEffectSequenceSha256,
            second.NestedCombatEffectSequenceSha256,
            "nested_combat_effect_sequence", diagnostics);
        Compare(first.NestedCombatEncounterStateChainSha256,
            second.NestedCombatEncounterStateChainSha256,
            "nested_combat_encounter_state_chain", diagnostics);
        Compare(first.NestedCombatTurnSequenceSha256,
            second.NestedCombatTurnSequenceSha256,
            "nested_combat_turn_sequence", diagnostics);
        Compare(first.NestedCombatTraceSha256,
            second.NestedCombatTraceSha256,
            "nested_combat_trace", diagnostics);
        Compare(first.FinalStateHash, second.FinalStateHash,
            "final_state", diagnostics);
        Compare(first.SignatureSha256, second.SignatureSha256,
            "signature", diagnostics);
        if (!first.Passed || !second.Passed)
            diagnostics.Add(
                "generated_regional_event.replay_mismatch.frame_pass");
        return diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static void Compare<T>(
        T first,
        T second,
        string dimension,
        ICollection<string> diagnostics)
    {
        if (!EqualityComparer<T>.Default.Equals(first, second))
            diagnostics.Add(
                "generated_regional_event.replay_mismatch." + dimension);
    }

    private static string ComparableSha256(
        GeneratedCampaignRegionalEventReplaySignature signature) =>
        GeneratedCampaignChoiceCanonical.Hash(new
        {
            signature.RegionalEventId,
            signature.RouteKind,
            signature.FrameCount,
            signature.CommandSequenceSha256,
            signature.MapEventSequenceSha256,
            signature.GameplayEventSequenceSha256,
            signature.StatusTransitionSequenceSha256,
            signature.StateHashChainSha256,
            signature.AvailableChoiceSequenceSha256,
            signature.ReputationSequenceSha256,
            signature.ResolutionFlagSequenceSha256,
            signature.RelationshipFlagSequenceSha256,
            signature.QuestStateSequenceSha256,
            signature.EncounterStateSequenceSha256,
            signature.NestedCombatFrameCount,
            signature.NestedCombatCommandSequenceSha256,
            signature.NestedCombatEventSequenceSha256,
            signature.NestedCombatDescriptorSequenceSha256,
            signature.NestedCombatEffectSequenceSha256,
            signature.NestedCombatEncounterStateChainSha256,
            signature.NestedCombatTurnSequenceSha256,
            signature.NestedCombatTraceSha256,
            signature.FinalStateHash,
            signature.Passed
        });
}

public static class GeneratedCampaignRegionalEventInventoryService
{
    public static GeneratedCampaignRegionalEventInventoryRow Create(
        GeneratedCampaignRegionalEventBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        var row = new GeneratedCampaignRegionalEventInventoryRow
        {
            RegionalEventId = binding.RegionalEventId,
            EventKind = binding.EventKind,
            RelationshipId = binding.RelationshipId,
            RelationshipBranch = binding.RelationshipBranch,
            ActorSeedId = binding.ActorSeedId,
            ActorEntityId = binding.ActorEntityId,
            FactionId = binding.FactionId,
            RegionId = binding.RegionId,
            MapId = binding.MapId,
            EntityPrototypeId = binding.EntityPrototypeId,
            MapEntityId = binding.MapEntityId,
            InteractionId = binding.InteractionId,
            DialogueId = binding.DialogueId,
            ResolutionFlagId = binding.ResolutionFlagId,
            SourceQuestId = binding.SourceQuestId,
            ChallengeEncounterId = binding.ChallengeEncounterId,
            ChallengeEncounterSourceId =
                binding.ChallengeEncounterSourceId,
            TargetRegionDerivation = binding.TargetRegionDerivation,
            TargetRegionFingerprint = binding.TargetRegionFingerprint,
            X = binding.Placement.X,
            Y = binding.Placement.Y,
            PrerequisiteFingerprint =
                binding.Prerequisite.Fingerprint,
            RewardDerivationFingerprint =
                binding.SourceQuestRewardFingerprint,
            ResolutionChoiceId = binding.ResolutionChoiceId,
            DialogueDefinitionSha256 =
                binding.DialogueDefinitionSha256,
            InteractionDefinitionSha256 =
                binding.InteractionDefinitionSha256,
            EntityPrototypeDefinitionSha256 =
                binding.EntityPrototypeDefinitionSha256,
            MapEntityDefinitionSha256 =
                binding.MapEntityDefinitionSha256,
            SourceQuestDefinitionSha256 =
                binding.SourceQuestDefinitionSha256,
            ChallengeEncounterDefinitionSha256 =
                binding.ChallengeEncounterDefinitionSha256,
            PositionSha256 = binding.PositionSha256,
            InteractableReferencesSha256 =
                binding.InteractableReferencesSha256,
            ResolutionRequirementsSha256 =
                binding.ResolutionRequirementsSha256,
            ResolutionEffectsSha256 =
                binding.ResolutionEffectsSha256,
            EventMetadataSha256 = binding.EventMetadataSha256
        };
        return row with
        {
            EventSemanticFingerprint = SemanticFingerprint(row)
        };
    }

    public static string SemanticFingerprint(
        GeneratedCampaignRegionalEventInventoryRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return GeneratedCampaignChoiceCanonical.Hash(
            row with { EventSemanticFingerprint = string.Empty });
    }
}
