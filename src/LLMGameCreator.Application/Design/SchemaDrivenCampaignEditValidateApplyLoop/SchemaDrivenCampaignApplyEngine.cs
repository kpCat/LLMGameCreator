namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignApplyEngine
{
    public ApplyRollbackLedger Apply(
        CampaignEditSourceBundle source,
        EditableSchemaFieldCatalog fieldCatalog,
        ChangeSetCatalog changeSetCatalog,
        ValidationDiagnosticsMatrix validationMatrix)
    {
        var validCandidates = changeSetCatalog.Candidates
            .Where(candidate => validationMatrix.Records.Any(record => record.CandidateId == candidate.CandidateId && record.Valid))
            .GroupBy(candidate => candidate.RowId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        var rows = new List<RowApplyRollbackRecord>();
        foreach (var row in source.Rows)
        {
            var values = SchemaDrivenCampaignEditCatalog.BuildInitialValues(row, fieldCatalog);
            var beforeHash = SchemaDrivenCampaignEditCatalog.HashRow(row, values);
            var working = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in values)
            {
                working[pair.Key] = pair.Value;
            }
            var appliedChanges = new List<RowAppliedChange>();
            if (validCandidates.TryGetValue(row.RowId, out var candidates))
            {
                foreach (var candidate in candidates.OrderBy(item => item.CandidateId, StringComparer.Ordinal))
                {
                    var beforeValue = working[candidate.FieldId];
                    working[candidate.FieldId] = candidate.ProposedValue;
                    appliedChanges.Add(new RowAppliedChange
                    {
                        CandidateId = candidate.CandidateId,
                        CandidateKind = candidate.CandidateKind,
                        FieldId = candidate.FieldId,
                        BeforeValue = beforeValue,
                        AfterValue = candidate.ProposedValue
                    });
                }
            }

            var afterHash = SchemaDrivenCampaignEditCatalog.HashRow(row, working);
            var rollbackHash = SchemaDrivenCampaignEditCatalog.HashRow(row, values);
            rows.Add(new RowApplyRollbackRecord
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                BeforeHash = beforeHash,
                AfterHash = afterHash,
                RollbackHash = rollbackHash,
                StateChanged = beforeHash != afterHash,
                RollbackRestored = beforeHash == rollbackHash,
                SaveLoadReplayPassed = row.SaveLoadReplayPassed && beforeHash != afterHash,
                AppliedChanges = appliedChanges
            });
        }

        return new ApplyRollbackLedger
        {
            Passed = rows.Count == 9
                && rows.All(row => row.StateChanged && row.RollbackRestored && row.SaveLoadReplayPassed),
            RowCount = rows.Count,
            AppliedChangeCount = rows.Sum(row => row.AppliedChanges.Count),
            RollbackCount = rows.Count(row => row.RollbackRestored),
            SaveLoadReplayPassed = rows.All(row => row.SaveLoadReplayPassed),
            Rows = rows
                .OrderBy(row => SchemaDrivenCampaignEditVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
                .ThenBy(row => SchemaDrivenCampaignEditVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
                .ToList()
        };
    }

    public RowBeforeAfterDiffMatrix BuildDiffMatrix(ApplyRollbackLedger ledger)
    {
        var rows = ledger.Rows
            .Select(row => new RowBeforeAfterDiff
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                BeforeHash = row.BeforeHash,
                AfterHash = row.AfterHash,
                ChangedFields = row.AppliedChanges
                    .OrderBy(change => change.FieldId, StringComparer.Ordinal)
                    .Select(change => new FieldDiff
                    {
                        FieldId = change.FieldId,
                        BeforeValue = change.BeforeValue,
                        AfterValue = change.AfterValue
                    })
                    .ToList()
            })
            .ToList();

        return new RowBeforeAfterDiffMatrix
        {
            Passed = rows.Count == 9 && rows.All(row => row.BeforeHash != row.AfterHash && row.ChangedFields.Count > 0),
            RowCount = rows.Count,
            StateChangingRowCount = rows.Count(row => row.BeforeHash != row.AfterHash),
            Rows = rows
        };
    }
}
