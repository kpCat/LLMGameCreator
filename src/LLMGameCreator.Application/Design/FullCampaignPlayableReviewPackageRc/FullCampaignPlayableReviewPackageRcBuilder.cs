using System.Text;

namespace LLMGameCreator.Application.Design.FullCampaignPlayableReviewPackageRc;

public sealed class FullCampaignPlayableReviewPackageRcBuilder
{
    private const string Goal060Root = FullCampaignPlayableReviewPackageRcVocabulary.Goal060RelativeOutputDirectory;

    public FullCampaignPlayableSourceManifest BuildSourceManifest(FullCampaignPlayableSourceBundle source)
    {
        var diagnostics = new List<FullCampaignPlayableReviewPackageRcDiagnostic>(source.Diagnostics)
        {
            Info("goal061.preflight.goal060_handoff_recorded", "full_campaign_gamepackage_materialization_matrix_verification", "Goal 060 is recorded as accepted by user handoff before Goal 061."),
            Info("goal061.source.loaded", "Goal060", "Goal 061 source facts were loaded from repository-local Goal 060 package materialization evidence.")
        };

        return new FullCampaignPlayableSourceManifest
        {
            Accepted = false,
            Goal060AcceptedByUserHandoff = source.Goal060AcceptedByUserHandoff,
            Goal060ReportWasGreenProducedForReview = source.Goal060ReportWasGreenProducedForReview,
            Goal060UnityProofPassed = source.Goal060UnityProofPassed,
            Goal059MatrixConsumed = source.Goal059MatrixConsumed,
            Goal058CampaignProofConsumed = source.Goal058CampaignProofConsumed,
            MediaProofChainConsumed = source.MediaProofChainConsumed,
            PackageRowCount = source.PackageRows.Count,
            MediaBindingCount = source.MediaBindings.Count,
            FamilyIds = source.PackageRows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).OrderBy(FullCampaignPlayableReviewPackageRcSourceLoader.FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            SeedIds = source.PackageRows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(FullCampaignPlayableReviewPackageRcSourceLoader.SeedOrderingKey, StringComparer.Ordinal).ToList(),
            PreflightGates =
            [
                new FullCampaignPlayableGateRecord
                {
                    GateId = "full_campaign_gamepackage_materialization_matrix_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 061 task preflight handoff"
                },
                new FullCampaignPlayableGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 031 preserved policy"
                },
                new FullCampaignPlayableGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 032 preserved policy"
                },
                new FullCampaignPlayableGateRecord
                {
                    GateId = FullCampaignPlayableReviewPackageRcVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "programmatic",
                    EvidenceRef = "Goal 061 produced for review"
                }
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public FullCampaignPlayablePackageRowSelectionMatrix BuildPackageRowSelectionMatrix(FullCampaignPlayableSourceBundle source)
    {
        var rows = source.PackageRows
            .OrderBy(item => FullCampaignPlayableReviewPackageRcSourceLoader.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => FullCampaignPlayableReviewPackageRcSourceLoader.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(item => new FullCampaignPlayableReviewPackageRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                PackageId = item.PackageId,
                PackageRelativePath = item.StagedUnityRelativePath,
                PackageHash = item.PackageHash,
                PackageHashVerified = item.PackageHashVerified,
                PackageMediaBindingsVerified = FamilyMediaBindingsVerified(source.MediaBindings, item.FamilyId),
                RuntimeLoopPassed = item.RuntimePassed,
                SaveLoadReplayVerified = item.SaveLoadRoundtripPassed && item.PackageHashVerified,
                ScenarioSummaryRelativePath = "review-package/scenario-summaries/" + item.RowId + ".md",
                CommandPlanSteps = CommandPlanSteps(item)
            })
            .ToList();

        return new FullCampaignPlayablePackageRowSelectionMatrix
        {
            Passed = rows.Count == 9
                && rows.All(item => item.PackageHashVerified)
                && rows.All(item => item.PackageMediaBindingsVerified)
                && rows.All(item => item.RuntimeLoopPassed)
                && rows.All(item => item.SaveLoadReplayVerified)
                && rows.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() == 9,
            RowCount = rows.Count,
            Rows = rows
        };
    }

    public FullCampaignPlayablePackageMediaBindingAudit BuildPackageMediaBindingAudit(FullCampaignPlayableSourceBundle source)
    {
        var rows = source.PackageRows
            .OrderBy(item => FullCampaignPlayableReviewPackageRcSourceLoader.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => FullCampaignPlayableReviewPackageRcSourceLoader.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(row =>
            {
                var bindings = source.MediaBindings
                    .Where(binding => binding.FamilyId == row.FamilyId)
                    .OrderBy(binding => binding.BindingId, StringComparer.Ordinal)
                    .ToList();
                return new FullCampaignPlayablePackageMediaBindingAuditRow
                {
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    PackageId = row.PackageId,
                    PackageHash = row.PackageHash,
                    PackageMediaBindingsVerified = bindings.Count == 5
                        && bindings.All(item => item.Exists && item.HashMatches)
                        && bindings.All(item => !string.IsNullOrWhiteSpace(item.ReviewTrace)),
                    BindingCount = bindings.Count,
                    BindingIds = bindings.Select(item => item.BindingId).ToList()
                };
            })
            .ToList();

        return new FullCampaignPlayablePackageMediaBindingAudit
        {
            Passed = rows.Count == 9 && rows.All(item => item.PackageMediaBindingsVerified),
            RowCount = rows.Count,
            Rows = rows
        };
    }

    public FullCampaignPlayableSaveLoadReplayPackageRowAudit BuildSaveLoadReplayAudit(FullCampaignPlayableSourceBundle source)
    {
        var rows = source.PackageRows
            .OrderBy(item => FullCampaignPlayableReviewPackageRcSourceLoader.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => FullCampaignPlayableReviewPackageRcSourceLoader.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(row => new FullCampaignPlayableSaveLoadReplayPackageRowAuditRow
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                PackageId = row.PackageId,
                PackageHash = row.PackageHash,
                SaveLoadRoundtripPassed = row.SaveLoadRoundtripPassed,
                ReplayDeterminismPassed = row.PackageHashVerified && string.Equals(row.PackageHash, row.PackageFileHash, StringComparison.Ordinal),
                PreviewExportPayloadConsistent = !string.IsNullOrWhiteSpace(row.PreviewPayloadRef)
                    && !string.IsNullOrWhiteSpace(row.ExportPayloadRef)
                    && row.PackageHashVerified,
                RuntimeCommandIds = row.RuntimeCommandIds
            })
            .ToList();

        return new FullCampaignPlayableSaveLoadReplayPackageRowAudit
        {
            Passed = rows.Count == 9
                && rows.All(item => item.SaveLoadRoundtripPassed)
                && rows.All(item => item.ReplayDeterminismPassed)
                && rows.All(item => item.PreviewExportPayloadConsistent)
                && rows.All(item => !string.IsNullOrWhiteSpace(item.PackageHash)),
            RowCount = rows.Count,
            Rows = rows
        };
    }

    public FullCampaignPlayableUnityCommandPlan BuildUnityCommandPlan(
        FullCampaignPlayablePackageRowSelectionMatrix selectionMatrix,
        FullCampaignPlayablePackageMediaBindingAudit mediaAudit,
        FullCampaignPlayableSaveLoadReplayPackageRowAudit saveLoadReplayAudit)
    {
        var mediaByRow = mediaAudit.Rows.ToDictionary(item => item.RowId, item => item, StringComparer.Ordinal);
        var saveByRow = saveLoadReplayAudit.Rows.ToDictionary(item => item.RowId, item => item, StringComparer.Ordinal);
        var commandRows = selectionMatrix.Rows
            .OrderBy(item => FullCampaignPlayableReviewPackageRcSourceLoader.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => FullCampaignPlayableReviewPackageRcSourceLoader.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(item =>
            {
                mediaByRow.TryGetValue(item.RowId, out var media);
                saveByRow.TryGetValue(item.RowId, out var save);
                var row = new FullCampaignPlayableUnityCommandRow
                {
                    RowId = item.RowId,
                    FamilyId = item.FamilyId,
                    SeedId = item.SeedId,
                    PackageId = item.PackageId,
                    PackageRelativePath = item.PackageRelativePath.Replace("review-package/", "review-package-rc/", StringComparison.Ordinal),
                    PackageHash = item.PackageHash,
                    PackageHashVerified = item.PackageHashVerified,
                    PackageMediaBindingsVerified = media?.PackageMediaBindingsVerified == true,
                    SaveLoadReplayVerified = save?.ReplayDeterminismPassed == true && save.SaveLoadRoundtripPassed,
                    OrderedStepIds = CommandPlanSteps(item.RowId)
                };
                return row with { ExpectedPlayerMarkers = ExpectedUnityRowMarkers(row) };
            })
            .ToList();

        var expected = new List<string>
        {
            "review_package_rc_loaded=true",
            "review_package_rc_id=" + FullCampaignPlayableReviewPackageRcVocabulary.ReviewPackageRcId,
            "review_package_rc_proof=goal061",
            "full_campaign_playable_review_package_rc_verification=required"
        };
        expected.AddRange(commandRows.SelectMany(item => item.ExpectedPlayerMarkers));

        return new FullCampaignPlayableUnityCommandPlan
        {
            Passed = commandRows.Count == 9
                && commandRows.All(item => item.PackageHashVerified)
                && commandRows.All(item => item.PackageMediaBindingsVerified)
                && commandRows.All(item => item.SaveLoadReplayVerified),
            Accepted = false,
            Rows = commandRows,
            ExpectedPlayerMarkers = expected.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
        };
    }

    public IReadOnlyList<FullCampaignPlayableFilePayload> BuildReviewPackageFiles(
        FullCampaignPlayableSourceBundle source,
        FullCampaignPlayablePackageRowSelectionMatrix selectionMatrix,
        FullCampaignPlayableUnityCommandPlan unityCommandPlan,
        FullCampaignPlayablePackageMediaBindingAudit mediaAudit,
        FullCampaignPlayableSaveLoadReplayPackageRowAudit saveLoadReplayAudit)
    {
        var files = new List<FullCampaignPlayableFilePayload>
        {
            TextFile("review-package/README.md", RenderReadme(selectionMatrix)),
            TextFile("review-package/RUN_MANUAL.ps1", RenderManualScript()),
            TextFile("review-package/RUN_AUTOMATED_SMOKE.ps1", RenderAutomatedScript()),
            TextFile("review-package/package-selection-matrix.json", Serialize(selectionMatrix)),
            TextFile("review-package/package-inventory.json", Serialize(new
            {
                schemaVersion = "goal061_review_package_inventory_v1",
                goalId = FullCampaignPlayableReviewPackageRcVocabulary.GoalId,
                packageCount = source.PackageRows.Count,
                packages = source.PackageRows.Select(item => new
                {
                    item.RowId,
                    item.FamilyId,
                    item.SeedId,
                    item.PackageId,
                    item.ReviewPackageRelativePath,
                    item.PackageHash,
                    item.PackageHashVerified
                }).ToArray()
            })),
            TextFile("review-package/media/StreamingAssets-payload-manifest.json", Serialize(new
            {
                schemaVersion = "goal061_review_package_streamingassets_media_payload_v1",
                goalId = FullCampaignPlayableReviewPackageRcVocabulary.GoalId,
                source = "Goal055 streaming-assets-media-manifest.json plus Goal054 physical media hashes",
                bindingCount = source.MediaBindings.Count,
                bindings = source.MediaBindings.Select(item => new
                {
                    item.BindingId,
                    item.FamilyId,
                    item.SlotId,
                    item.MediaKind,
                    item.StreamingAssetsRelativePath,
                    item.SourceSha256,
                    item.ActualSha256,
                    item.SizeBytes,
                    item.ReviewTrace,
                    item.HashMatches
                }).ToArray()
            })),
            TextFile("review-package/family-seed-command-plan.json", Serialize(new
            {
                schemaVersion = "goal061_family_seed_command_plan_v1",
                goalId = FullCampaignPlayableReviewPackageRcVocabulary.GoalId,
                rows = unityCommandPlan.Rows.Select(item => new
                {
                    item.RowId,
                    item.FamilyId,
                    item.SeedId,
                    item.PackageId,
                    item.PackageHash,
                    item.OrderedStepIds
                }).ToArray()
            })),
            TextFile("review-package/manual-checklist.md", RenderManualChecklist(selectionMatrix, mediaAudit, saveLoadReplayAudit)),
            TextFile("review-package/hashes-and-provenance.json", Serialize(new
            {
                schemaVersion = "goal061_hashes_and_provenance_v1",
                goalId = FullCampaignPlayableReviewPackageRcVocabulary.GoalId,
                sources = source.SourceArtifactRefs.Select(item => new
                {
                    item.SourceGoal,
                    item.ArtifactFamily,
                    item.ArtifactRelativePath,
                    item.ArtifactHash,
                    item.Exists,
                    item.HashMatches
                }).ToArray()
            })),
            TextFile("review-package/StreamingAssets/LLMGameCreatorAlpha/review-package-rc/unity-player-command-plan.json", Serialize(unityCommandPlan))
        };

        files.AddRange(selectionMatrix.Rows.Select(row => TextFile(row.ScenarioSummaryRelativePath, RenderScenarioSummary(row, source))));
        files.AddRange(source.PackageRows.Select(row => TextFile(row.ReviewPackageRelativePath, row.PackageJson)));

        return files
            .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    public FullCampaignPlayableReviewPackageFileInventory BuildFileInventory(IReadOnlyList<FullCampaignPlayableFilePayload> reviewPackageFiles)
    {
        var entries = reviewPackageFiles
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(item => new FullCampaignPlayableReviewPackageFileEntry
            {
                RelativePath = item.RelativePath,
                Sha256 = HashBytes(item.Bytes),
                SizeBytes = item.Bytes.Length,
                ArtifactKind = ArtifactKind(item.RelativePath)
            })
            .ToList();

        return new FullCampaignPlayableReviewPackageFileInventory
        {
            Passed = entries.Count > 0
                && entries.Any(item => item.RelativePath == "review-package/README.md")
                && entries.Any(item => item.RelativePath == "review-package/RUN_MANUAL.ps1")
                && entries.Any(item => item.RelativePath == "review-package/RUN_AUTOMATED_SMOKE.ps1")
                && entries.Count(item => IsReviewPackagePhysicalPackage(item.RelativePath)) == 9
                && entries.All(item => FullCampaignPlayableReviewPackageRcSourceLoader.IsSafeRelativePath(item.RelativePath)),
            FileCount = entries.Count,
            Files = entries
        };
    }

    public FullCampaignPlayableReviewPackageRcManifest BuildReviewPackageManifest(
        FullCampaignPlayableSourceManifest sourceManifest,
        FullCampaignPlayablePackageRowSelectionMatrix selectionMatrix,
        FullCampaignPlayableReviewPackageFileInventory fileInventory,
        FullCampaignPlayablePackageMediaBindingAudit mediaAudit,
        FullCampaignPlayableSaveLoadReplayPackageRowAudit saveLoadReplayAudit,
        FullCampaignPlayableSmokeScriptManifest scriptManifest)
    {
        var files = fileInventory.Files.Select(item => item.RelativePath).Order(StringComparer.Ordinal).ToList();
        return new FullCampaignPlayableReviewPackageRcManifest
        {
            Passed = sourceManifest.Goal060AcceptedByUserHandoff
                && sourceManifest.Goal060ReportWasGreenProducedForReview
                && sourceManifest.Goal060UnityProofPassed
                && sourceManifest.Goal059MatrixConsumed
                && sourceManifest.Goal058CampaignProofConsumed
                && sourceManifest.MediaProofChainConsumed
                && selectionMatrix.Passed
                && fileInventory.Passed
                && mediaAudit.Passed
                && saveLoadReplayAudit.Passed
                && scriptManifest.Passed,
            Accepted = false,
            PackageRowCount = selectionMatrix.RowCount,
            PhysicalPackageCount = fileInventory.Files.Count(item => IsReviewPackagePhysicalPackage(item.RelativePath)),
            ScriptCount = scriptManifest.Scripts.Count,
            ScenarioSummaryCount = fileInventory.Files.Count(item => item.RelativePath.StartsWith("review-package/scenario-summaries/", StringComparison.Ordinal)),
            SourceChainConsumed = sourceManifest.SourceArtifactRefs.All(item => item.Exists && item.HashMatches && item.Diagnostics.Count == 0),
            PackageHashesVerified = selectionMatrix.Rows.All(item => item.PackageHashVerified),
            MediaBindingsVerified = mediaAudit.Passed,
            SaveLoadReplayTiedToPackageRows = saveLoadReplayAudit.Rows.Count == 9
                && saveLoadReplayAudit.Rows.All(item => !string.IsNullOrWhiteSpace(item.RowId) && !string.IsNullOrWhiteSpace(item.PackageHash))
                && saveLoadReplayAudit.Passed,
            Rows = selectionMatrix.Rows,
            ReviewPackageFiles = files
        };
    }

    public FullCampaignPlayableSmokeScriptManifest BuildScriptManifest(IReadOnlyList<FullCampaignPlayableFilePayload> reviewPackageFiles)
    {
        var scripts = reviewPackageFiles
            .Where(item => item.RelativePath.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(item => new FullCampaignPlayableReviewPackageFileEntry
            {
                RelativePath = item.RelativePath,
                Sha256 = HashBytes(item.Bytes),
                SizeBytes = item.Bytes.Length,
                ArtifactKind = "script"
            })
            .ToList();

        return new FullCampaignPlayableSmokeScriptManifest
        {
            Passed = scripts.Count == 2
                && scripts.All(item => item.RelativePath.StartsWith("review-package/", StringComparison.Ordinal))
                && scripts.All(item => !item.RelativePath.Contains("..", StringComparison.Ordinal)),
            Scripts = scripts
        };
    }

    public IReadOnlyList<FullCampaignPlayableFilePayload> BuildStagingFiles(
        FullCampaignPlayableSourceBundle source,
        FullCampaignPlayableUnityCommandPlan commandPlan)
    {
        var files = source.StagingFiles.ToList();
        files.Add(TextFile(FullCampaignPlayableReviewPackageRcVocabulary.UnityReviewPackageCommandPlanStagingRelativePath, Serialize(commandPlan)));

        foreach (var row in source.PackageRows)
        {
            files.Add(TextFile(row.StagedUnityRelativePath, row.PackageJson));
            files.Add(TextFile("package-materialization/" + row.SourcePackageRelativePath[(Goal060Root.Length + 1)..], row.PackageJson));
        }

        files.Add(TextFile("package-materialization/unity-package-consumption-command-plan.json", Serialize(new
        {
            schemaVersion = "goal061_staged_goal060_package_consumption_plan_v1",
            goalId = FullCampaignPlayableReviewPackageRcVocabulary.GoalId,
            rows = source.PackageRows.Select(item => new
            {
                item.RowId,
                item.FamilyId,
                item.SeedId,
                item.PackageId,
                packageRelativePath = "package-materialization/" + item.SourcePackageRelativePath[(Goal060Root.Length + 1)..],
                item.PackageHash,
                packageValidationPassed = item.ValidationPassed,
                runtimeLoopCompleted = item.RuntimePassed
            }).ToArray()
        })));

        return files
            .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    public InvalidFullCampaignPlayableReviewPackageMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidFullCampaignPlayableReviewPackageScenario>
        {
            Invalid("missing_goal060_inventory", "Remove Goal 060 materialized-package-inventory.json before loading.", "blocked", Error("goal061.source.goal060_inventory_missing", "Goal060", "Goal 060 inventory is required.")),
            Invalid("stale_package_hash", "Change one physical Goal 060 package after inventory hashes were recorded.", "rejected", Error("goal061.source.package_hash_mismatch", "packages", "Package hash must match inventory.")),
            Invalid("missing_package_file", "Delete a package file referenced by Goal 060 inventory.", "rejected", Error("goal061.source.package_file_missing", "packages", "Package file must exist.")),
            Invalid("malformed_package_json", "Replace a package file with malformed JSON.", "rejected", Error("goal061.source.package_json_malformed", "packages", "Package JSON must parse.")),
            Invalid("fake_family_seed_package_row", "Inject a fake family, seed or package row not present in Goal 060/059.", "rejected", Error("goal061.source.fake_family_seed_package_row", "package-row", "Row must be one of the accepted matrix rows.")),
            Invalid("duplicate_row_id", "Duplicate a row id in the review package selection matrix.", "rejected", Error("goal061.source.duplicate_row_id", "rowId", "Row ids must be unique.")),
            Invalid("unsafe_relative_path_traversal", "Point a package or script outside the review package root with ../ traversal.", "rejected", Error("goal061.path.unsafe_relative_path", "../", "Paths must be traversal-free repo-relative paths.")),
            Invalid("missing_media_binding", "Remove a family media binding required by the package row.", "rejected", Error("goal061.media.binding_missing", "media", "Every package row needs its family media bindings.")),
            Invalid("stale_media_hash", "Change a Goal 054 media file after Goal 055 recorded its hash.", "rejected", Error("goal061.source.media_hash_mismatch", "media", "Media hash must match source proof.")),
            Invalid("fake_unity_proof_marker", "Add a marker to the report without running the Unity/player route.", "rejected", Error("goal061.unity.fake_marker", "unity-proof", "Player markers must come from the Unity Alpha logs.")),
            Invalid("provider_llm_rag_media_generation_claim", "Claim provider/LLM/RAG/media generation in a review package script.", "rejected", Error("goal061.leak.provider_llm_rag_media_generation_claim", "review-package", "Provider, LLM, RAG and media generation are forbidden.")),
            Invalid("runtime_gamepackage_schema_broad_mutation_claim", "Claim Runtime or public GamePackage schema mutation as proof.", "rejected", Error("goal061.leak.runtime_gamepackage_schema_broad_mutation_claim", "scope", "Runtime and GamePackage schema broad mutation are forbidden.")),
            Invalid("unity_broad_mutation_claim", "Mutate Unity outside the narrow bootstrap marker route.", "rejected", Error("goal061.leak.unity_broad_mutation_claim", "unity", "Unity changes are limited to AlphaRuntimeBootstrap markers.")),
            Invalid("nondeterministic_row_order", "Emit package rows in filesystem enumeration order instead of family/seed order.", "rejected", Error("goal061.matrix.nondeterministic_row_order", "rows", "Rows must be deterministically family/seed ordered.")),
            Invalid("missing_review_trace", "Drop package/media provenance from the RC manifest.", "rejected", Error("goal061.review_trace.missing", "provenance", "Every row and binding must keep review trace.")),
            Invalid("script_path_escaping_review_package_root", "Reference an executable path outside the review package root.", "rejected", Error("goal061.script.path_escape", "RUN_*.ps1", "Scripts must stay review-package relative."))
        };

        return new InvalidFullCampaignPlayableReviewPackageMatrix
        {
            Passed = scenarios.Count == FullCampaignPlayableReviewPackageRcVocabulary.RequiredInvalidScenarioIds.Count
                && FullCampaignPlayableReviewPackageRcVocabulary.RequiredInvalidScenarioIds.All(required => scenarios.Any(item => item.ScenarioId == required))
                && scenarios.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<string> ExpectedUnityRowMarkers(FullCampaignPlayableUnityCommandRow row)
    {
        var markers = new List<string>
        {
            "package_row_selected=" + row.RowId,
            "package_id=" + row.PackageId,
            "family_id=" + row.FamilyId,
            "seed_id=" + row.SeedId,
            "package_hash_verified=true",
            "package_media_bindings_verified=true",
            "package_loop_started=true",
            "package_loop_completed=true",
            "save_load_replay_verified=true"
        };
        markers.AddRange(row.OrderedStepIds.Select(step => "package_loop_step=" + step));
        return markers.Order(StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<FullCampaignPlayableReviewPackageRcDiagnostic> SortDiagnostics(IEnumerable<FullCampaignPlayableReviewPackageRcDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    public static IReadOnlyList<string> CommandPlanSteps(FullCampaignPlayablePackageRowSource row) =>
        CommandPlanSteps(row.RowId)
            .Concat(row.RuntimeCommandTypes.Select(commandType => row.RowId + ":runtime:" + commandType))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<string> CommandPlanSteps(FullCampaignPlayableReviewPackageRow row) =>
        row.CommandPlanSteps.Count == 0 ? CommandPlanSteps(row.RowId) : row.CommandPlanSteps;

    private static IReadOnlyList<string> CommandPlanSteps(string rowId) =>
    [
        rowId + ":load_package",
        rowId + ":verify_hash",
        rowId + ":verify_media_bindings",
        rowId + ":start_loop",
        rowId + ":save_load_replay",
        rowId + ":complete_loop"
    ];

    private static bool FamilyMediaBindingsVerified(IReadOnlyList<FullCampaignPlayableMediaBindingSource> mediaBindings, string familyId)
    {
        var familyBindings = mediaBindings.Where(item => item.FamilyId == familyId).ToList();
        return familyBindings.Count == 5
            && familyBindings.All(item => item.Exists && item.HashMatches)
            && familyBindings.All(item => !string.IsNullOrWhiteSpace(item.ReviewTrace));
    }

    private static string RenderReadme(FullCampaignPlayablePackageRowSelectionMatrix selectionMatrix)
    {
        var lines = new List<string>
        {
            "# Goal 061 Review Package RC",
            string.Empty,
            "Gate: full_campaign_playable_review_package_rc_verification required",
            "Accepted: false",
            string.Empty,
            "This package stages the Goal 060 materialized package matrix for manual and automated review.",
            string.Empty,
            "Rows:"
        };
        lines.AddRange(selectionMatrix.Rows.Select(item => "- " + item.RowId + " | " + item.PackageId + " | " + item.PackageHash));
        lines.Add(string.Empty);
        lines.Add("Use RUN_AUTOMATED_SMOKE.ps1 for deterministic script validation and Unity/player proof routing.");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderManualScript() =>
        string.Join(Environment.NewLine, new[]
        {
            "param()",
            "$ErrorActionPreference = 'Stop'",
            "$Root = Split-Path -Parent $MyInvocation.MyCommand.Path",
            "$Manifest = Join-Path $Root 'package-selection-matrix.json'",
            "if (-not (Test-Path -LiteralPath $Manifest)) { throw 'package-selection-matrix.json is missing.' }",
            "$Rows = (Get-Content -Raw -LiteralPath $Manifest | ConvertFrom-Json).rows",
            "Write-Host ('Goal061 manual review rows: ' + $Rows.Count)",
            "foreach ($Row in $Rows) {",
            "  $PackagePath = Join-Path (Split-Path -Parent $Root) $Row.packageRelativePath",
            "  if (-not (Test-Path -LiteralPath $PackagePath)) { throw ('Missing package: ' + $Row.rowId) }",
            "  Write-Host ($Row.rowId + ' ' + $Row.packageId)",
            "}",
            "Write-Host 'full_campaign_playable_review_package_rc_verification required'"
        }) + Environment.NewLine;

    private static string RenderAutomatedScript() =>
        string.Join(Environment.NewLine, new[]
        {
            "param()",
            "$ErrorActionPreference = 'Stop'",
            "$Root = Split-Path -Parent $MyInvocation.MyCommand.Path",
            "$Plan = Join-Path $Root 'StreamingAssets\\LLMGameCreatorAlpha\\review-package-rc\\unity-player-command-plan.json'",
            "if (-not (Test-Path -LiteralPath $Plan)) { throw 'Unity command plan is missing.' }",
            "$CommandPlan = Get-Content -Raw -LiteralPath $Plan | ConvertFrom-Json",
            "if ($CommandPlan.rows.Count -ne 9) { throw 'Expected 9 package rows.' }",
            "foreach ($Row in $CommandPlan.rows) {",
            "  $PackagePath = Join-Path (Split-Path -Parent $Root) ('review-package\\' + ($Row.packageRelativePath -replace '^review-package-rc/', ''))",
            "  if (-not (Test-Path -LiteralPath $PackagePath)) { throw ('Missing package for ' + $Row.rowId) }",
            "  if (-not $Row.packageHashVerified) { throw ('Package hash was not verified for ' + $Row.rowId) }",
            "}",
            "Write-Host 'review_package_rc_loaded=true'",
            "Write-Host ('review_package_rc_id=' + $CommandPlan.reviewPackageRcId)",
            "Write-Host 'review_package_rc_proof=goal061'"
        }) + Environment.NewLine;

    private static string RenderManualChecklist(
        FullCampaignPlayablePackageRowSelectionMatrix selectionMatrix,
        FullCampaignPlayablePackageMediaBindingAudit mediaAudit,
        FullCampaignPlayableSaveLoadReplayPackageRowAudit saveLoadReplayAudit)
    {
        var lines = new List<string>
        {
            "# Goal 061 Manual Review Checklist",
            string.Empty,
            "- Gate remains full_campaign_playable_review_package_rc_verification required.",
            "- Confirm all nine package rows are listed.",
            "- Confirm RUN_MANUAL.ps1 and RUN_AUTOMATED_SMOKE.ps1 stay under the review package root.",
            "- Confirm package hashes, media bindings, save/load and replay audits are row-bound.",
            string.Empty,
            "## Rows"
        };
        foreach (var row in selectionMatrix.Rows)
        {
            var media = mediaAudit.Rows.First(item => item.RowId == row.RowId);
            var save = saveLoadReplayAudit.Rows.First(item => item.RowId == row.RowId);
            lines.Add("- " + row.RowId + ": packageHashVerified=" + row.PackageHashVerified.ToString().ToLowerInvariant()
                + ", mediaBindings=" + media.PackageMediaBindingsVerified.ToString().ToLowerInvariant()
                + ", saveLoadReplay=" + (save.SaveLoadRoundtripPassed && save.ReplayDeterminismPassed).ToString().ToLowerInvariant());
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderScenarioSummary(FullCampaignPlayableReviewPackageRow row, FullCampaignPlayableSourceBundle source)
    {
        var mediaCount = source.MediaBindings.Count(item => item.FamilyId == row.FamilyId);
        return string.Join(Environment.NewLine, new[]
        {
            "# Scenario Summary: " + row.RowId,
            string.Empty,
            "- familyId: " + row.FamilyId,
            "- seedId: " + row.SeedId,
            "- packageId: " + row.PackageId,
            "- packageHash: " + row.PackageHash,
            "- packageHashVerified: " + row.PackageHashVerified.ToString().ToLowerInvariant(),
            "- packageMediaBindingCount: " + mediaCount,
            "- packageMediaBindingsVerified: " + row.PackageMediaBindingsVerified.ToString().ToLowerInvariant(),
            "- runtimeLoopPassed: " + row.RuntimeLoopPassed.ToString().ToLowerInvariant(),
            "- saveLoadReplayVerified: " + row.SaveLoadReplayVerified.ToString().ToLowerInvariant(),
            "- manualGate: full_campaign_playable_review_package_rc_verification required"
        }) + Environment.NewLine;
    }

    private static FullCampaignPlayableFilePayload TextFile(string relativePath, string text) =>
        new()
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Bytes = Encoding.UTF8.GetBytes(text.TrimEnd('\r', '\n') + Environment.NewLine)
        };

    private static string ArtifactKind(string relativePath)
    {
        if (relativePath.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
        {
            return "script";
        }

        if (IsReviewPackagePhysicalPackage(relativePath))
        {
            return "package";
        }

        if (relativePath.Contains("scenario-summaries/", StringComparison.Ordinal))
        {
            return "scenario_summary";
        }

        if (relativePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            return "markdown";
        }

        return "manifest";
    }

    private static bool IsReviewPackagePhysicalPackage(string relativePath) =>
        relativePath.StartsWith("review-package/p/", StringComparison.Ordinal)
        && relativePath.EndsWith(".json", StringComparison.Ordinal);

    private static InvalidFullCampaignPlayableReviewPackageScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params FullCampaignPlayableReviewPackageRcDiagnostic[] diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            CausalMutation = mutation,
            ExpectedStatus = expectedStatus,
            ActualStatus = expectedStatus,
            ExpectedValid = false,
            ActualValid = false,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "critical" => 0,
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static string Serialize<T>(T value) => FullCampaignPlayableReviewPackageRcHash.Serialize(value);

    private static string HashBytes(byte[] bytes) => FullCampaignPlayableReviewPackageRcHash.HashBytes(bytes);

    private static FullCampaignPlayableReviewPackageRcDiagnostic Error(string code, string target, string message) =>
        FullCampaignPlayableReviewPackageRcDiagnostic.Error(code, target, message);

    private static FullCampaignPlayableReviewPackageRcDiagnostic Info(string code, string target, string message) =>
        FullCampaignPlayableReviewPackageRcDiagnostic.Info(code, target, message);
}
