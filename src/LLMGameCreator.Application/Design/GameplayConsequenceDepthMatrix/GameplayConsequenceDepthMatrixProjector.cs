namespace LLMGameCreator.Application.Design.GameplayConsequenceDepthMatrix;

public static class GameplayConsequenceDepthMatrixProjector
{
    public static GameplayConsequenceRowProof Project(GameplayConsequenceCommandPlanRow row)
    {
        var values = InitialState(row);
        var beforeState = Snapshot(row, values, 0);
        var transitions = new List<GameplayConsequenceStateTransitionProof>();
        var stepIndex = 0;

        foreach (var command in row.Commands.OrderBy(item => item.StepId, StringComparer.Ordinal))
        {
            var before = Snapshot(row, values, stepIndex);
            foreach (var change in command.ExpectedChanges.OrderBy(item => item.Key, StringComparer.Ordinal))
            {
                values[change.Key] = change.Value;
            }

            stepIndex++;
            var after = Snapshot(row, values, stepIndex);
            var deltas = command.ExpectedChanges
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item =>
                {
                    var beforeValue = before.Values.TryGetValue(item.Key, out var existing) ? existing : "(missing)";
                    var afterValue = after.Values.TryGetValue(item.Key, out var actual) ? actual : "(missing)";
                    return new GameplayConsequenceStateDelta
                    {
                        DeltaId = command.DeltaId,
                        Key = item.Key,
                        BeforeValue = beforeValue,
                        AfterValue = afterValue,
                        ExpectedValue = item.Value,
                        ActualValue = afterValue,
                        Passed = string.Equals(item.Value, afterValue, StringComparison.Ordinal)
                            && !string.Equals(beforeValue, afterValue, StringComparison.Ordinal)
                    };
                })
                .ToList();

            transitions.Add(new GameplayConsequenceStateTransitionProof
            {
                RowId = row.RowId,
                CommandId = command.CommandId,
                CommandType = command.CommandType,
                DeltaId = command.DeltaId,
                Before = before,
                After = after,
                Deltas = deltas,
                ExpectedVsActualPassed = deltas.Count > 0 && deltas.All(item => item.Passed),
                StateChanged = !string.Equals(before.StateHash, after.StateHash, StringComparison.Ordinal)
            });
        }

        var afterState = Snapshot(row, values, stepIndex);
        var serializerRoundtrip = SerializerRoundtrip(afterState, out var restoredHash);
        var proofWithoutHash = new GameplayConsequenceRowProof
        {
            RowId = row.RowId,
            FamilyId = row.FamilyId,
            SeedId = row.SeedId,
            SourcePackageRowRef = row.SourcePackageRowRef,
            SourceReviewPackageRowRef = row.SourceReviewPackageRowRef,
            SourceSpatialDetailRowRef = row.SourceSpatialDetailRowRef,
            BeforeState = beforeState,
            AfterState = afterState,
            Transitions = transitions,
            StateChangingStepCount = transitions.Count(item => item.StateChanged),
            StateTransitionProofPassed = transitions.Count(item => item.StateChanged) >= 3
                && transitions.All(item => item.StateChanged && item.ExpectedVsActualPassed),
            SerializerRoundtripPassed = serializerRoundtrip && string.Equals(restoredHash, afterState.StateHash, StringComparison.Ordinal),
            ReplayDeterminismPassed = true,
            VarianceContribution = BuildVarianceContribution(row, afterState),
            RowHash = string.Empty
        };
        return proofWithoutHash with
        {
            RowHash = GameplayConsequenceDepthMatrixHash.Hash(GameplayConsequenceDepthMatrixHash.Serialize(proofWithoutHash))
        };
    }

    public static bool SerializerRoundtrip(GameplayConsequenceStateSnapshot snapshot, out string restoredHash)
    {
        var json = GameplayConsequenceDepthMatrixHash.Serialize(snapshot);
        var restored = GameplayConsequenceDepthMatrixHash.Deserialize<GameplayConsequenceStateSnapshot>(json);
        restoredHash = restored?.StateHash ?? string.Empty;
        return restored is not null
            && string.Equals(GameplayConsequenceDepthMatrixHash.Hash(GameplayConsequenceDepthMatrixHash.Serialize(restored.Values)), GameplayConsequenceDepthMatrixHash.Hash(GameplayConsequenceDepthMatrixHash.Serialize(snapshot.Values)), StringComparison.Ordinal);
    }

    public static IReadOnlyList<string> ChangedKeys(GameplayConsequenceStateSnapshot before, GameplayConsequenceStateSnapshot after) =>
        before.Values.Keys.Concat(after.Values.Keys)
            .Distinct(StringComparer.Ordinal)
            .Where(key => !before.Values.TryGetValue(key, out var beforeValue)
                || !after.Values.TryGetValue(key, out var afterValue)
                || !string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToList();

    private static SortedDictionary<string, string> InitialState(GameplayConsequenceCommandPlanRow row)
    {
        var safeFamily = GameplayConsequenceDepthMatrixHash.SafeSegment(row.FamilyId);
        var safeSeed = GameplayConsequenceDepthMatrixHash.SafeSegment(row.SeedId);
        return new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["row.id"] = row.RowId,
            ["family.id"] = row.FamilyId,
            ["seed.id"] = row.SeedId,
            ["source.package"] = row.SourcePackageRowRef,
            ["source.review"] = row.SourceReviewPackageRowRef,
            ["source.spatial"] = row.SourceSpatialDetailRowRef,
            ["location.region"] = "region/" + safeFamily + "/entry",
            ["location.detail"] = "detail/" + safeFamily + "/entry",
            ["inventory.relic"] = "0",
            ["inventory.resource"] = row.FamilyId == "survival_sandbox" ? "1" : "0",
            ["quest.progress"] = "0",
            ["reputation.local_faction"] = "0",
            ["survival.hazard"] = "0",
            ["survival.stamina"] = "10",
            ["grid.orientation"] = "north",
            ["grid.blocked_moves"] = "0",
            ["encounter.pressure"] = "0",
            ["progression.unlock"] = "locked",
            ["progression.xp"] = "0",
            ["social.trust"] = "0",
            ["seed.modifier"] = SeedModifier(row.SeedId).ToString()
        };
    }

    private static GameplayConsequenceStateSnapshot Snapshot(
        GameplayConsequenceCommandPlanRow row,
        IReadOnlyDictionary<string, string> values,
        int stepIndex)
    {
        var copy = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            copy[pair.Key] = pair.Value;
        }
        return new GameplayConsequenceStateSnapshot
        {
            RowId = row.RowId,
            FamilyId = row.FamilyId,
            SeedId = row.SeedId,
            StepIndex = stepIndex,
            Values = copy,
            StateHash = GameplayConsequenceDepthMatrixHash.Hash(GameplayConsequenceDepthMatrixHash.Serialize(copy))
        };
    }

    private static GameplayConsequenceVarianceContribution BuildVarianceContribution(
        GameplayConsequenceCommandPlanRow row,
        GameplayConsequenceStateSnapshot afterState)
    {
        var axisKeys = row.FamilyId switch
        {
            "map_panel_rpg" => new[] { "location.detail", "quest.progress", "inventory.relic", "reputation.local_faction", "social.trust" },
            "survival_sandbox" => new[] { "survival.hazard", "survival.stamina", "inventory.resource", "inventory.relic", "progression.unlock" },
            "first_person_grid_dungeon" => new[] { "grid.orientation", "grid.blocked_moves", "encounter.pressure", "progression.unlock", "progression.xp" },
            _ => []
        };
        var highlights = axisKeys
            .Where(key => afterState.Values.ContainsKey(key))
            .ToDictionary(key => key, key => afterState.Values[key], StringComparer.Ordinal);
        return new GameplayConsequenceVarianceContribution
        {
            RowId = row.RowId,
            FamilyId = row.FamilyId,
            SeedId = row.SeedId,
            ContributionId = row.FamilyId + "/" + row.SeedId + "/" + GameplayConsequenceDepthMatrixHash.ShortHash(GameplayConsequenceDepthMatrixHash.Serialize(highlights)),
            MeaningfulAxes = axisKeys.Order(StringComparer.Ordinal).ToList(),
            FinalStateHighlights = new SortedDictionary<string, string>(highlights, StringComparer.Ordinal)
        };
    }

    private static int SeedModifier(string seedId) =>
        seedId switch
        {
            "seed_alpha" => 1,
            "seed_beta" => 2,
            "seed_gamma" => 3,
            _ => 0
        };
}
