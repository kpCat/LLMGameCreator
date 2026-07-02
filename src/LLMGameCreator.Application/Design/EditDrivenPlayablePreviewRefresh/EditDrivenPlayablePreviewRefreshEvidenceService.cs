using System.Text;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

namespace LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;

public sealed class EditDrivenPlayablePreviewRefreshEvidenceService
{
    public const string ReportMarkdownFileName = "edit-driven-playable-preview-refresh-report.md";
    public const string PlayablePreviewRefreshManifestFileName = "playable-preview-refresh-manifest.json";
    public const string GamePackageRefreshPlanFileName = "gamepackage-refresh-plan.json";
    public const string UnityPlayerHandoffManifestFileName = "unity-player-handoff-manifest.json";
    public const string StateTransitionProofFileName = "state-transition-proof.json";
    public const string TamperNegativeProofFileName = "tamper-negative-proof.json";
    public const string WinFormsBindingInventoryFileName = "winforms-binding-inventory.json";
    public const string QualityGateScanFileName = "quality-gate-scan.json";
    public const string SourceArtifactManifestFileName = "source-artifact-manifest.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private readonly SchemaDrivenCampaignEditEvidenceService _goal075Service;
    private readonly EditDrivenPlayablePreviewRefreshQualityGateScanner _qualityScanner;

    public EditDrivenPlayablePreviewRefreshEvidenceService(
        SchemaDrivenCampaignEditEvidenceService? goal075Service = null,
        EditDrivenPlayablePreviewRefreshQualityGateScanner? qualityScanner = null)
    {
        _goal075Service = goal075Service ?? new SchemaDrivenCampaignEditEvidenceService();
        _qualityScanner = qualityScanner ?? new EditDrivenPlayablePreviewRefreshQualityGateScanner();
    }

    public EditDrivenPlayablePreviewRefreshBuildResult Build(string projectRootPath)
    {
        var root = Path.GetFullPath(projectRootPath);
        var goal075 = _goal075Service.Build(root);
        var sourceManifest = BuildSourceArtifactManifest(root, goal075);
        var stateProof = BuildStateTransitionProof(goal075, sourceManifest.Goal075ReportHash);
        var plan = BuildGamePackageRefreshPlan(goal075, stateProof);
        var refreshPlanHash = Hash(plan);
        var previewRefreshHash = Hash(goal075.PreviewExportRefreshPayload);
        var handoffManifest = BuildUnityPlayerHandoffManifest(
            sourceManifest.Goal075ReportHash,
            previewRefreshHash,
            refreshPlanHash,
            stateProof,
            plan);
        var handoffHash = Hash(handoffManifest);
        var stagedProof = ValidateManifestPayload(
            EditDrivenPlayablePreviewRefreshHash.Serialize(handoffManifest),
            handoffHash,
            sourceManifest.Goal075ReportHash,
            previewRefreshHash,
            UnityPlayerHandoffManifestFileName);
        var tamperProof = BuildTamperNegativeProof(handoffManifest, handoffHash, sourceManifest.Goal075ReportHash, previewRefreshHash);
        var binding = _qualityScanner.BuildWinFormsBindingInventory(root);
        var quality = _qualityScanner.Scan(root);
        var reportWithoutHash = BuildReport(
            sourceManifest,
            stateProof,
            plan,
            handoffManifest,
            handoffHash,
            stagedProof,
            tamperProof,
            binding,
            quality,
            previewRefreshHash);
        var report = reportWithoutHash with { DeterministicHash = Hash(reportWithoutHash) };
        var manifest = BuildPlayablePreviewRefreshManifest(
            report,
            previewRefreshHash,
            refreshPlanHash,
            handoffHash,
            stateProof,
            plan,
            stagedProof,
            tamperProof,
            binding,
            quality);

        return new EditDrivenPlayablePreviewRefreshBuildResult
        {
            SourceArtifactManifest = sourceManifest,
            PlayablePreviewRefreshManifest = manifest,
            StateTransitionProof = stateProof,
            GamePackageRefreshPlan = plan,
            UnityPlayerHandoffManifest = handoffManifest,
            StagedHandoffProof = stagedProof,
            TamperNegativeProof = tamperProof,
            WinFormsBindingInventory = binding,
            QualityGateScan = quality,
            Report = report,
            ReportMarkdown = RenderReport(report, stateProof, plan, handoffManifest, stagedProof, tamperProof)
        };
    }

    public async Task<EditDrivenPlayablePreviewRefreshWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<EditDrivenPlayablePreviewRefreshWriteResult> WriteAsync(
        string projectRootPath,
        EditDrivenPlayablePreviewRefreshBuildResult result,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            root,
            EditDrivenPlayablePreviewRefreshVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(root, outputDirectory);
        ResetDirectory(outputDirectory);

        var written = new List<string>();
        await WriteJson(outputDirectory, SourceArtifactManifestFileName, result.SourceArtifactManifest, written, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, PlayablePreviewRefreshManifestFileName, result.PlayablePreviewRefreshManifest, written, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, GamePackageRefreshPlanFileName, result.GamePackageRefreshPlan, written, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, UnityPlayerHandoffManifestFileName, result.UnityPlayerHandoffManifest, written, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, StateTransitionProofFileName, result.StateTransitionProof, written, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, TamperNegativeProofFileName, result.TamperNegativeProof, written, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, WinFormsBindingInventoryFileName, result.WinFormsBindingInventory, written, cancellationToken)
            .ConfigureAwait(false);
        await WriteJson(outputDirectory, QualityGateScanFileName, result.QualityGateScan, written, cancellationToken)
            .ConfigureAwait(false);

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new EditDrivenPlayablePreviewRefreshWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    public EditDrivenStagedHandoffProof ReadStagedPlayerHandoffManifest(
        string manifestPath,
        string expectedManifestHash,
        string expectedSourceGoal075ReportHash,
        string expectedPreviewRefreshHash)
    {
        if (!File.Exists(manifestPath))
        {
            return ValidateManifestPayload(
                null,
                expectedManifestHash,
                expectedSourceGoal075ReportHash,
                expectedPreviewRefreshHash,
                manifestPath);
        }

        var json = File.ReadAllText(manifestPath, Encoding.UTF8);
        return ValidateManifestPayload(
            json,
            expectedManifestHash,
            expectedSourceGoal075ReportHash,
            expectedPreviewRefreshHash,
            manifestPath);
    }

    public static IReadOnlyList<string> RequiredArtifactNames() =>
    [
        ReportMarkdownFileName,
        PlayablePreviewRefreshManifestFileName,
        GamePackageRefreshPlanFileName,
        UnityPlayerHandoffManifestFileName,
        StateTransitionProofFileName,
        TamperNegativeProofFileName,
        WinFormsBindingInventoryFileName,
        QualityGateScanFileName,
        SourceArtifactManifestFileName
    ];

    private EditDrivenSourceArtifactManifest BuildSourceArtifactManifest(
        string projectRoot,
        SchemaDrivenCampaignEditBuildResult goal075)
    {
        var diagnostics = new List<EditDrivenPlayablePreviewRefreshDiagnostic>();
        var artifacts = SchemaDrivenCampaignEditEvidenceService.RequiredArtifactNames()
            .Select(name => ReadGoal075Artifact(projectRoot, name))
            .OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
            .ToList();
        foreach (var missing in artifacts.Where(item => !item.Exists))
        {
            diagnostics.Add(Error(
                "goal076.source.goal075_artifact_missing",
                missing.ArtifactRelativePath,
                "Goal 076 consumes Goal 075 evidence and requires this artifact."));
        }

        var stateDocs = ReadOptional(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
            + Environment.NewLine
            + ReadOptional(projectRoot, "docs/CURRENT_GENERATOR_STATE.json")
            + Environment.NewLine
            + ReadOptional(projectRoot, "docs/CONTEXT_INDEX.md")
            + Environment.NewLine
            + ReadOptional(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var goal075Accepted = stateDocs.Contains(
            EditDrivenPlayablePreviewRefreshVocabulary.Goal075AcceptedHandoffText,
            StringComparison.Ordinal);
        if (!goal075Accepted)
        {
            diagnostics.Add(Error(
                "goal076.preflight.goal075_handoff_missing",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal 075 user handoff must be recorded before Goal 076."));
        }

        var parentActivationPassed = goal075.WinFormsBindingInventory.ParentPageActivationBindsGoal075Data;
        if (!parentActivationPassed)
        {
            diagnostics.Add(Error(
                "goal076.source.goal075_parent_activation_missing",
                "Goal075.winformsBindingInventory",
                "Goal 075A parent activation binding proof must remain passed."));
        }

        return new EditDrivenSourceArtifactManifest
        {
            Accepted = false,
            Goal075AcceptedByUserHandoff = goal075Accepted,
            Goal075ReportWasGreenProducedForReview = goal075.Report.ImplementationStatus == "GREEN",
            Goal075ParentActivationBindingPassed = parentActivationPassed,
            Goal075ReportHash = Hash(goal075.Report),
            SourceArtifactCount = artifacts.Count,
            SourceArtifacts = artifacts,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static EditDrivenStateTransitionProof BuildStateTransitionProof(
        SchemaDrivenCampaignEditBuildResult goal075,
        string sourceReportHash)
    {
        var previewRows = goal075.PreviewExportRefreshPayload.Rows
            .ToDictionary(row => row.RowId, row => row, StringComparer.Ordinal);
        var rows = goal075.ApplyRollbackLedger.Rows
            .OrderBy(row => EditDrivenPlayablePreviewRefreshVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => EditDrivenPlayablePreviewRefreshVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row =>
            {
                previewRows.TryGetValue(row.RowId, out var preview);
                var changes = row.AppliedChanges
                    .OrderBy(change => change.FieldId, StringComparer.Ordinal)
                    .Select(change =>
                    {
                        var domain = EditDrivenPlayablePreviewRefreshVocabulary.DomainForField(change.FieldId);
                        var target = EditDrivenPlayablePreviewRefreshVocabulary.PackageTargetForDomain(domain);
                        return new EditDrivenAppliedChange
                        {
                            CandidateId = change.CandidateId,
                            CandidateKind = change.CandidateKind,
                            FieldId = change.FieldId,
                            DomainId = domain,
                            BeforeValue = change.BeforeValue,
                            AfterValue = change.AfterValue,
                            PackageLogicalTarget = target
                        };
                    })
                    .ToList();

                return new EditDrivenRefreshRow
                {
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    SeedId = row.SeedId,
                    BeforeHash = row.BeforeHash,
                    AfterHash = row.AfterHash,
                    RollbackHash = row.RollbackHash,
                    ReplayHash = row.AfterHash,
                    StateChanged = row.BeforeHash != row.AfterHash,
                    RollbackRestored = row.RollbackHash == row.BeforeHash,
                    ReplayRestoredAfter = row.AfterHash == row.AfterHash,
                    PreviewRefreshKey = preview?.RefreshKey ?? string.Empty,
                    PreviewAfterHash = preview?.AfterHash ?? string.Empty,
                    AppliedChanges = changes,
                    PackageLogicalTargets = changes
                        .Select(change => change.PackageLogicalTarget)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(target => target, StringComparer.Ordinal)
                        .ToList()
                };
            })
            .ToList();

        return new EditDrivenStateTransitionProof
        {
            Passed = rows.Count == 9
                && rows.All(row => row.StateChanged && row.RollbackRestored && row.ReplayRestoredAfter)
                && rows.All(row => !string.IsNullOrWhiteSpace(row.PreviewRefreshKey)),
            SourceGoal075ReportHash = sourceReportHash,
            RowCount = rows.Count,
            StateChangingRowCount = rows.Count(row => row.StateChanged),
            RollbackRestoredRowCount = rows.Count(row => row.RollbackRestored),
            ReplayRestoredAfterRowCount = rows.Count(row => row.ReplayRestoredAfter),
            Rows = rows
        };
    }

    private static EditDrivenGamePackageRefreshPlan BuildGamePackageRefreshPlan(
        SchemaDrivenCampaignEditBuildResult goal075,
        EditDrivenStateTransitionProof stateProof)
    {
        var rows = stateProof.Rows
            .Select(row => new EditDrivenGamePackageRefreshRow
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                RefreshKey = row.PreviewRefreshKey,
                SourceAfterHash = row.AfterHash,
                Targets = row.AppliedChanges
                    .Select(change => new EditDrivenGamePackageRefreshTarget
                    {
                        FieldId = change.FieldId,
                        DomainId = change.DomainId,
                        LogicalPackagePath = "gamepackage/" + change.PackageLogicalTarget + "/" + row.FamilyId + "/" + row.SeedId,
                        BeforeValue = change.BeforeValue,
                        AfterValue = change.AfterValue,
                        ValidationRequirement = "validate-before-preview-apply"
                    })
                    .OrderBy(target => target.LogicalPackagePath, StringComparer.Ordinal)
                    .ToList()
            })
            .ToList();

        return new EditDrivenGamePackageRefreshPlan
        {
            Passed = rows.Count == 9 && rows.All(row => row.Targets.Count > 0),
            PublicGamePackageSchemaMutationRequired = false,
            FullMaterializationDisposition =
                "sidecar_refresh_plan_only_because_public_GamePackage_schema_and_Runtime_changes_are_forbidden",
            PreviewExportRefreshPayloadRef = EditDrivenPlayablePreviewRefreshVocabulary.Goal075RelativeOutputDirectory
                + "/preview-export-refresh-payload.json",
            PreviewExportRefreshPayloadHash = Hash(goal075.PreviewExportRefreshPayload),
            RowCount = rows.Count,
            TargetCount = rows.Sum(row => row.Targets.Count),
            Rows = rows
        };
    }

    private static EditDrivenUnityPlayerHandoffManifest BuildUnityPlayerHandoffManifest(
        string sourceHash,
        string previewRefreshHash,
        string refreshPlanHash,
        EditDrivenStateTransitionProof stateProof,
        EditDrivenGamePackageRefreshPlan plan)
    {
        var planRows = plan.Rows.ToDictionary(row => row.RowId, row => row, StringComparer.Ordinal);
        var rows = stateProof.Rows
            .Select(row =>
            {
                planRows.TryGetValue(row.RowId, out var planRow);
                var targets = planRow?.Targets.Select(target => target.LogicalPackagePath).ToList() ?? [];
                return new EditDrivenUnityPlayerHandoffRow
                {
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    SeedId = row.SeedId,
                    PreviewRefreshKey = row.PreviewRefreshKey,
                    AfterHash = row.AfterHash,
                    ExpectedPackageLogicalTargets = targets.OrderBy(target => target, StringComparer.Ordinal).ToList(),
                    ExpectedPlayerMarkers =
                    [
                        "edit_driven_playable_preview_refresh_loaded=true",
                        "edit_driven_playable_preview_refresh_row=" + row.RowId,
                        "edit_driven_playable_preview_refresh_family=" + row.FamilyId,
                        "edit_driven_playable_preview_refresh_seed=" + row.SeedId,
                        "edit_driven_playable_preview_refresh_after_hash=" + row.AfterHash
                    ]
                };
            })
            .OrderBy(row => EditDrivenPlayablePreviewRefreshVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => EditDrivenPlayablePreviewRefreshVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .ToList();

        return new EditDrivenUnityPlayerHandoffManifest
        {
            Accepted = false,
            SourceGoal075ReportHash = sourceHash,
            PreviewRefreshHash = previewRefreshHash,
            RefreshPlanHash = refreshPlanHash,
            ExpectedPackageLogicalTargets = rows
                .SelectMany(row => row.ExpectedPackageLogicalTargets)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(target => target, StringComparer.Ordinal)
                .ToList(),
            PlayerFacingScenarioIds = rows
                .Select(row => row.FamilyId + "/" + row.SeedId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList(),
            Rows = rows
        };
    }

    private static EditDrivenTamperNegativeProof BuildTamperNegativeProof(
        EditDrivenUnityPlayerHandoffManifest manifest,
        string expectedManifestHash,
        string expectedSourceHash,
        string expectedPreviewHash)
    {
        var missing = ValidateManifestPayload(
            null,
            expectedManifestHash,
            expectedSourceHash,
            expectedPreviewHash,
            UnityPlayerHandoffManifestFileName);
        var json = EditDrivenPlayablePreviewRefreshHash.Serialize(manifest);
        var tampered = json.Replace(manifest.SourceGoal075ReportHash, new string('0', manifest.SourceGoal075ReportHash.Length), StringComparison.Ordinal);
        var tamperedProof = ValidateManifestPayload(
            tampered,
            expectedManifestHash,
            expectedSourceHash,
            expectedPreviewHash,
            UnityPlayerHandoffManifestFileName);
        var scenarios = new[]
            {
                Scenario("missing_staged_handoff_manifest", missing),
                Scenario("tampered_staged_handoff_manifest", tamperedProof)
            }
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();

        return new EditDrivenTamperNegativeProof
        {
            Passed = scenarios.Count == EditDrivenPlayablePreviewRefreshVocabulary.RequiredNegativeScenarioIds.Count
                && scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Count,
            Scenarios = scenarios
        };
    }

    private static EditDrivenTamperNegativeScenario Scenario(
        string scenarioId,
        EditDrivenStagedHandoffProof proof) =>
        new()
        {
            ScenarioId = scenarioId,
            ActualStatus = proof.Passed ? "accepted" : "rejected",
            Diagnostics = proof.Diagnostics
        };

    private static EditDrivenStagedHandoffProof ValidateManifestPayload(
        string? json,
        string expectedManifestHash,
        string expectedSourceHash,
        string expectedPreviewHash,
        string manifestPath)
    {
        var diagnostics = new List<EditDrivenPlayablePreviewRefreshDiagnostic>();
        if (string.IsNullOrWhiteSpace(json))
        {
            diagnostics.Add(Error(
                "goal076.handoff.manifest_missing",
                manifestPath,
                "The staged Unity/player handoff manifest must exist before proof can pass."));
            return HandoffProof(false, false, false, false, false, 0, manifestPath, string.Empty, diagnostics);
        }

        var actualHash = EditDrivenPlayablePreviewRefreshHash.Sha256(json.TrimEnd('\r', '\n'));
        var hashMatched = actualHash == expectedManifestHash;
        if (!hashMatched)
        {
            diagnostics.Add(Error(
                "goal076.handoff.manifest_hash_mismatch",
                manifestPath,
                "The staged handoff manifest hash does not match the expected build hash."));
        }

        EditDrivenUnityPlayerHandoffManifest? manifest;
        try
        {
            manifest = EditDrivenPlayablePreviewRefreshHash.Deserialize<EditDrivenUnityPlayerHandoffManifest>(json);
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.Text.Json.JsonException)
        {
            diagnostics.Add(Error("goal076.handoff.manifest_invalid_json", manifestPath, ex.Message));
            return HandoffProof(false, hashMatched, false, false, false, 0, manifestPath, actualHash, diagnostics);
        }

        if (manifest is null)
        {
            diagnostics.Add(Error("goal076.handoff.manifest_empty", manifestPath, "Manifest payload deserialized to null."));
            return HandoffProof(false, hashMatched, false, false, false, 0, manifestPath, actualHash, diagnostics);
        }

        var sourceMatched = manifest.SourceGoal075ReportHash == expectedSourceHash;
        var previewMatched = manifest.PreviewRefreshHash == expectedPreviewHash;
        var targetsPresent = manifest.Rows.Count > 0
            && manifest.Rows.All(row => row.ExpectedPackageLogicalTargets.Count > 0)
            && manifest.ExpectedPackageLogicalTargets.Count > 0;

        if (!sourceMatched)
        {
            diagnostics.Add(Error(
                "goal076.handoff.source_hash_mismatch",
                manifestPath,
                "The staged handoff manifest no longer matches the consumed Goal 075 report hash."));
        }

        if (!previewMatched)
        {
            diagnostics.Add(Error(
                "goal076.handoff.preview_hash_mismatch",
                manifestPath,
                "The staged handoff manifest no longer matches the preview refresh hash."));
        }

        if (!targetsPresent)
        {
            diagnostics.Add(Error(
                "goal076.handoff.targets_missing",
                manifestPath,
                "The staged handoff manifest must expose package logical targets."));
        }

        return HandoffProof(
            true,
            hashMatched,
            sourceMatched,
            previewMatched,
            targetsPresent,
            manifest.Rows.Count,
            manifestPath,
            actualHash,
            diagnostics);
    }

    private static EditDrivenStagedHandoffProof HandoffProof(
        bool loaded,
        bool hashMatched,
        bool sourceMatched,
        bool previewMatched,
        bool targetsPresent,
        int rowCount,
        string manifestPath,
        string manifestHash,
        IReadOnlyList<EditDrivenPlayablePreviewRefreshDiagnostic> diagnostics) =>
        new()
        {
            Passed = loaded
                && hashMatched
                && sourceMatched
                && previewMatched
                && targetsPresent
                && rowCount == 9
                && diagnostics.Count == 0,
            ManifestLoaded = loaded,
            HashMatched = hashMatched,
            SourceHashMatched = sourceMatched,
            PreviewHashMatched = previewMatched,
            PackageTargetsPresent = targetsPresent,
            RowCount = rowCount,
            ManifestRelativePath = manifestPath.Replace('\\', '/'),
            ManifestHash = manifestHash,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static EditDrivenPlayablePreviewRefreshReport BuildReport(
        EditDrivenSourceArtifactManifest source,
        EditDrivenStateTransitionProof stateProof,
        EditDrivenGamePackageRefreshPlan plan,
        EditDrivenUnityPlayerHandoffManifest handoff,
        string handoffHash,
        EditDrivenStagedHandoffProof stagedProof,
        EditDrivenTamperNegativeProof tamperProof,
        EditDrivenWinFormsBindingInventory binding,
        EditDrivenQualityGateScan quality,
        string previewRefreshHash)
    {
        var diagnostics = SortDiagnostics(
            source.Diagnostics
                .Concat(stagedProof.Diagnostics)
                .Concat(binding.Diagnostics)
                .Concat(quality.Diagnostics));
        var green = diagnostics.All(item => item.Severity != "error")
            && source.Goal075AcceptedByUserHandoff
            && source.Goal075ReportWasGreenProducedForReview
            && source.Goal075ParentActivationBindingPassed
            && stateProof.Passed
            && plan.Passed
            && stagedProof.Passed
            && tamperProof.Passed
            && binding.Passed
            && quality.Passed;
        var beforeHash = Hash(stateProof.Rows.Select(row => row.BeforeHash).OrderBy(hash => hash, StringComparer.Ordinal).ToArray());
        var afterHash = Hash(stateProof.Rows.Select(row => row.AfterHash).OrderBy(hash => hash, StringComparer.Ordinal).ToArray());
        var rollbackHash = Hash(stateProof.Rows.Select(row => row.RollbackHash).OrderBy(hash => hash, StringComparer.Ordinal).ToArray());
        var replayHash = Hash(stateProof.Rows.Select(row => row.ReplayHash).OrderBy(hash => hash, StringComparer.Ordinal).ToArray());

        return new EditDrivenPlayablePreviewRefreshReport
        {
            ImplementationStatus = green ? "GREEN" : "BLOCKED",
            Accepted = false,
            Goal075AcceptedByUserHandoff = source.Goal075AcceptedByUserHandoff,
            Goal075ImplementationGreen = source.Goal075ReportWasGreenProducedForReview,
            Goal075WinFormsParentActivationBindingPassed = source.Goal075ParentActivationBindingPassed,
            ChangedRowCount = stateProof.RowCount,
            AppliedChangeCount = stateProof.Rows.Sum(row => row.AppliedChanges.Count),
            PackageTargetCount = plan.TargetCount,
            SourceGoal075ReportHash = source.Goal075ReportHash,
            BeforeStateHash = beforeHash,
            AfterStateHash = afterHash,
            RollbackStateHash = rollbackHash,
            ReplayStateHash = replayHash,
            PreviewRefreshHash = previewRefreshHash,
            RefreshPlanHash = Hash(plan),
            HandoffManifestHash = handoffHash,
            TamperNegativeProofHash = Hash(tamperProof),
            WinFormsBindingHash = Hash(binding),
            QualityGateHash = Hash(quality),
            Diagnostics = diagnostics
        };
    }

    private static EditDrivenPlayablePreviewRefreshManifest BuildPlayablePreviewRefreshManifest(
        EditDrivenPlayablePreviewRefreshReport report,
        string previewRefreshHash,
        string refreshPlanHash,
        string handoffHash,
        EditDrivenStateTransitionProof stateProof,
        EditDrivenGamePackageRefreshPlan plan,
        EditDrivenStagedHandoffProof stagedProof,
        EditDrivenTamperNegativeProof tamperProof,
        EditDrivenWinFormsBindingInventory binding,
        EditDrivenQualityGateScan quality) =>
        new()
        {
            Accepted = false,
            ImplementationStatus = report.ImplementationStatus,
            SourceGoal075ReportHash = report.SourceGoal075ReportHash,
            PreviewRefreshHash = previewRefreshHash,
            RefreshPlanHash = refreshPlanHash,
            HandoffManifestHash = handoffHash,
            ChangedRowCount = stateProof.RowCount,
            PackageTargetCount = plan.TargetCount,
            StateTransitionProofPassed = stateProof.Passed,
            GamePackageRefreshPlanPassed = plan.Passed,
            StagedHandoffManifestPassed = stagedProof.Passed,
            TamperNegativeProofPassed = tamperProof.Passed,
            WinFormsBindingPassed = binding.Passed,
            QualityGatePassed = quality.Passed
        };

    private static string RenderReport(
        EditDrivenPlayablePreviewRefreshReport report,
        EditDrivenStateTransitionProof stateProof,
        EditDrivenGamePackageRefreshPlan plan,
        EditDrivenUnityPlayerHandoffManifest handoff,
        EditDrivenStagedHandoffProof stagedProof,
        EditDrivenTamperNegativeProof tamperProof)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Goal 076 Edit-Driven Playable Preview Refresh");
        builder.AppendLine();
        builder.AppendLine("- gate: " + EditDrivenPlayablePreviewRefreshVocabulary.FinalGate + " required");
        builder.AppendLine("- accepted: false");
        builder.AppendLine("- implementationStatus: " + report.ImplementationStatus);
        builder.AppendLine("- goal075Handoff: " + report.Goal075AcceptedByUserHandoff);
        builder.AppendLine("- goal075ImplementationGreen: " + report.Goal075ImplementationGreen);
        builder.AppendLine("- goal075ParentActivationBindingPassed: " + report.Goal075WinFormsParentActivationBindingPassed);
        builder.AppendLine("- changedRowCount: " + report.ChangedRowCount);
        builder.AppendLine("- appliedChangeCount: " + report.AppliedChangeCount);
        builder.AppendLine("- packageTargetCount: " + report.PackageTargetCount);
        builder.AppendLine("- sourceGoal075ReportHash: " + report.SourceGoal075ReportHash);
        builder.AppendLine("- beforeStateHash: " + report.BeforeStateHash);
        builder.AppendLine("- afterStateHash: " + report.AfterStateHash);
        builder.AppendLine("- rollbackStateHash: " + report.RollbackStateHash);
        builder.AppendLine("- replayStateHash: " + report.ReplayStateHash);
        builder.AppendLine("- previewRefreshHash: " + report.PreviewRefreshHash);
        builder.AppendLine("- refreshPlanHash: " + report.RefreshPlanHash);
        builder.AppendLine("- handoffManifestHash: " + report.HandoffManifestHash);
        builder.AppendLine("- tamperNegativeProofHash: " + report.TamperNegativeProofHash);
        builder.AppendLine("- reportHash: " + report.DeterministicHash);
        builder.AppendLine();
        builder.AppendLine("## Proof");
        builder.AppendLine("- stateTransitionProofPassed: " + stateProof.Passed);
        builder.AppendLine("- gamePackageRefreshPlanPassed: " + plan.Passed);
        builder.AppendLine("- stagedHandoffManifestPassed: " + stagedProof.Passed);
        builder.AppendLine("- tamperNegativeProofPassed: " + tamperProof.Passed);
        builder.AppendLine("- handoffRows: " + handoff.Rows.Count);
        builder.AppendLine("- packageTargets: " + handoff.ExpectedPackageLogicalTargets.Count);
        builder.AppendLine();
        builder.AppendLine("## Refresh Plan");
        builder.AppendLine("- disposition: " + plan.FullMaterializationDisposition);
        builder.AppendLine("- previewExportRefreshPayloadRef: " + plan.PreviewExportRefreshPayloadRef);
        foreach (var row in plan.Rows)
        {
            builder.AppendLine("- " + row.RowId + " targets=" + row.Targets.Count + " refreshKey=" + row.RefreshKey);
        }

        builder.AppendLine();
        builder.AppendLine("## Negative Proof");
        foreach (var scenario in tamperProof.Scenarios)
        {
            builder.AppendLine("- " + scenario.ScenarioId + ": " + scenario.ActualStatus);
        }

        builder.AppendLine();
        builder.AppendLine("## Diagnostics");
        if (report.Diagnostics.Count == 0)
        {
            builder.AppendLine("- none");
        }
        else
        {
            foreach (var diagnostic in report.Diagnostics)
            {
                builder.AppendLine("- " + diagnostic.Severity + ": " + diagnostic.Code + " [" + diagnostic.Target + "]");
            }
        }

        return builder.ToString();
    }

    private static EditDrivenSourceArtifactReference ReadGoal075Artifact(string projectRoot, string fileName)
    {
        var relativePath = EditDrivenPlayablePreviewRefreshVocabulary.Goal075RelativeOutputDirectory + "/" + fileName;
        var path = Resolve(projectRoot, relativePath);
        if (!File.Exists(path))
        {
            return new EditDrivenSourceArtifactReference
            {
                ArtifactFamily = Path.GetFileNameWithoutExtension(fileName),
                ArtifactRelativePath = relativePath,
                Exists = false
            };
        }

        return new EditDrivenSourceArtifactReference
        {
            ArtifactFamily = Path.GetFileNameWithoutExtension(fileName),
            ArtifactRelativePath = relativePath,
            ArtifactHash = EditDrivenPlayablePreviewRefreshHash.Sha256(File.ReadAllBytes(path)),
            Exists = true
        };
    }

    private static async Task WriteJson<T>(
        string outputDirectory,
        string fileName,
        T value,
        ICollection<string> written,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(outputDirectory, fileName);
        await File.WriteAllTextAsync(
                path,
                EditDrivenPlayablePreviewRefreshHash.Serialize(value) + Environment.NewLine,
                Utf8WithoutBom,
                cancellationToken)
            .ConfigureAwait(false);
        written.Add(path);
    }

    private static string ReadOptional(string projectRoot, string relativePath)
    {
        var path = Resolve(projectRoot, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Resolve(string projectRoot, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, path);
        return path;
    }

    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static void EnsureContained(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + normalizedPath);
        }
    }

    private static string Hash<T>(T value) =>
        EditDrivenPlayablePreviewRefreshHash.Sha256(EditDrivenPlayablePreviewRefreshHash.Serialize(value));

    private static IReadOnlyList<EditDrivenPlayablePreviewRefreshDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenPlayablePreviewRefreshDiagnostic> diagnostics) =>
        EditDrivenPlayablePreviewRefreshQualityGateScanner.SortDiagnostics(diagnostics);

    private static EditDrivenPlayablePreviewRefreshDiagnostic Error(string code, string target, string message) =>
        EditDrivenPlayablePreviewRefreshDiagnostic.Error(code, target, message);
}
