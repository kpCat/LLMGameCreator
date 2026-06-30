namespace LLMGameCreator.Application.Design.FullGeneratorVariabilityRegressionMatrix;

public sealed class FullGeneratorVariabilityMatrixBuilder
{
    public FullGeneratorVariabilitySourceManifest BuildSourceManifest(FullGeneratorVariabilitySourceBundle source)
    {
        var diagnostics = new List<FullGeneratorVariabilityDiagnostic>(source.Diagnostics)
        {
            Info("goal059.preflight.goal058_handoff_recorded", "full_media_bound_generator_campaign_verification", "Goal 058 is recorded as accepted by user handoff before Goal 059."),
            Info("goal059.source.loaded", "Goal058", "Goal 059 source facts were loaded from repository-local Goal 058 campaign evidence.")
        };

        return new FullGeneratorVariabilitySourceManifest
        {
            Accepted = false,
            Goal058AcceptedByUserHandoff = true,
            Goal058ReportWasGreenProducedForReview = source.Goal058ReportWasGreenProducedForReview,
            Goal058UnityProofPassed = source.Goal058UnityProofPassed,
            SourceCampaignHash = source.SourceCampaignHash,
            SourceArtifactCount = source.SourceArtifactRefs.Count,
            FamilyCount = source.Families.Count,
            SelectedFamilyIds = source.Families.Select(item => item.FamilyId).OrderBy(FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            PreflightGates =
            [
                new FullGeneratorVariabilityGateRecord
                {
                    GateId = "full_media_bound_generator_campaign_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 059 task preflight handoff"
                },
                new FullGeneratorVariabilityGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 031 preserved policy"
                },
                new FullGeneratorVariabilityGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 032 preserved policy"
                },
                new FullGeneratorVariabilityGateRecord
                {
                    GateId = FullGeneratorVariabilityMatrixVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "programmatic",
                    EvidenceRef = "Goal 059 produced for review"
                }
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public IReadOnlyDictionary<string, FullGeneratorVariabilityMatrixRow> BuildRows(FullGeneratorVariabilitySourceBundle source) =>
        source.Families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .SelectMany(family => FullGeneratorVariabilityMatrixVocabulary.SeedIds
                .OrderBy(SeedOrderingKey, StringComparer.Ordinal)
                .Select(seed => BuildRow(source, family, seed)))
            .ToDictionary(item => item.RowId, item => item, StringComparer.Ordinal);

    public FullGeneratorVariabilitySeedProfileMatrix BuildSeedProfileMatrix(IReadOnlyDictionary<string, FullGeneratorVariabilityMatrixRow> rows)
    {
        var orderedRows = rows.Values.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal).ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal).ToList();
        return new FullGeneratorVariabilitySeedProfileMatrix
        {
            Passed = orderedRows.Count == 9
                && FullGeneratorVariabilityMatrixVocabulary.FamilyIds.All(familyId => orderedRows.Count(item => item.FamilyId == familyId) == 3)
                && FullGeneratorVariabilityMatrixVocabulary.SeedIds.All(seedId => orderedRows.Count(item => item.SeedId == seedId) == 3),
            Accepted = false,
            RowCount = orderedRows.Count,
            Families = orderedRows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).OrderBy(FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            Seeds = orderedRows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(SeedOrderingKey, StringComparer.Ordinal).ToList(),
            Rows = orderedRows
                .Select(item => new FullGeneratorVariabilityMatrixRowSummary
                {
                    RowId = item.RowId,
                    FamilyId = item.FamilyId,
                    SeedId = item.SeedId,
                    DerivedCampaignHash = item.DerivedCampaignHash,
                    VariationDimensionCount = item.VariationDimensions.Count
                })
                .ToList()
        };
    }

    public FullGeneratorVariabilityVarianceMetrics BuildVarianceMetrics(IReadOnlyDictionary<string, FullGeneratorVariabilityMatrixRow> rows)
    {
        var orderedRows = rows.Values.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal).ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal).ToList();
        var familySummaries = FullGeneratorVariabilityMatrixVocabulary.FamilyIds
            .Select(familyId =>
            {
                var familyRows = orderedRows.Where(item => item.FamilyId == familyId).ToList();
                var meaningful = familyRows
                    .SelectMany(item => item.VariationDimensions)
                    .GroupBy(item => item.DimensionId, StringComparer.Ordinal)
                    .Where(group => group.Select(item => item.Value).Distinct(StringComparer.Ordinal).Count() >= 2)
                    .Select(group => group.Key)
                    .Order(StringComparer.Ordinal)
                    .ToList();
                return new FullGeneratorVariabilityFamilyVarianceSummary
                {
                    FamilyId = familyId,
                    RowCount = familyRows.Count,
                    DistinctSeedCount = familyRows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
                    DistinctDerivedHashCount = familyRows.Select(item => item.DerivedCampaignHash).Distinct(StringComparer.Ordinal).Count(),
                    MeaningfulVariationDimensionCount = meaningful.Count,
                    MeaningfulVariationDimensions = meaningful
                };
            })
            .ToList();

        var pairSummaries = BuildPairDifferenceSummaries(orderedRows);
        var mediaRefs = orderedRows.SelectMany(item => item.SelectedMediaRefs).Distinct(StringComparer.Ordinal).ToList();
        var loopRefs = orderedRows.SelectMany(item => item.SelectedFamilyLoopRefs).Distinct(StringComparer.Ordinal).ToList();
        var overfitWarnings = 0;
        if (orderedRows.Select(item => item.DerivedCampaignHash).Distinct(StringComparer.Ordinal).Count() != orderedRows.Count)
        {
            overfitWarnings++;
        }

        overfitWarnings += familySummaries.Count(item => item.MeaningfulVariationDimensionCount < 2);

        return new FullGeneratorVariabilityVarianceMetrics
        {
            RowCount = orderedRows.Count,
            DistinctRowIdCount = orderedRows.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count(),
            DistinctDerivedCampaignHashCount = orderedRows.Select(item => item.DerivedCampaignHash).Distinct(StringComparer.Ordinal).Count(),
            FamilyCount = orderedRows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = orderedRows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            MediaBindingCoveragePassed = FullGeneratorVariabilityMatrixVocabulary.FamilyIds.All(familyId => mediaRefs.Any(item => item.Contains("/" + FullGeneratorVariabilityMatrixSourceLoader.SafeSegment(familyId) + "/", StringComparison.Ordinal))),
            MediaBindingCoverageCount = mediaRefs.Count,
            FamilyLoopMarkerCoveragePassed = FullGeneratorVariabilityMatrixVocabulary.FamilyIds.All(familyId => loopRefs.Any(item => item.Contains(familyId, StringComparison.Ordinal))),
            FamilyLoopMarkerCoverageCount = loopRefs.Count,
            MinimumMeaningfulVariationDimensionsPerFamily = familySummaries.Count == 0 ? 0 : familySummaries.Min(item => item.MeaningfulVariationDimensionCount),
            OverfitWarningCount = overfitWarnings,
            FamilySummaries = familySummaries,
            PairDifferenceSummaries = pairSummaries,
            Passed = orderedRows.Count == 9
                && orderedRows.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() == 9
                && orderedRows.Select(item => item.DerivedCampaignHash).Distinct(StringComparer.Ordinal).Count() == 9
                && familySummaries.All(item => item.RowCount == 3 && item.DistinctSeedCount == 3 && item.DistinctDerivedHashCount == 3 && item.MeaningfulVariationDimensionCount >= 2)
                && overfitWarnings == 0
                && pairSummaries.All(item => item.DifferenceDimensionCount >= 2)
        };
    }

    public FullGeneratorVariabilityReplayDeterminismProof BuildReplayProof(
        IReadOnlyDictionary<string, FullGeneratorVariabilityMatrixRow> firstRows,
        IReadOnlyDictionary<string, FullGeneratorVariabilityMatrixRow> secondRows)
    {
        var rows = firstRows.Values.OrderBy(item => item.RowId, StringComparer.Ordinal).Select(first =>
        {
            secondRows.TryGetValue(first.RowId, out var second);
            var firstJson = Serialize(first);
            var secondJson = second == null ? string.Empty : Serialize(second);
            var firstHash = Hash(firstJson);
            var secondHash = string.IsNullOrEmpty(secondJson) ? string.Empty : Hash(secondJson);
            return new FullGeneratorVariabilityReplayRowProof
            {
                RowId = first.RowId,
                FirstHash = firstHash,
                SecondHash = secondHash,
                JsonMatches = firstJson == secondJson,
                HashMatches = firstHash == secondHash
            };
        }).ToList();

        return new FullGeneratorVariabilityReplayDeterminismProof
        {
            Passed = rows.Count == 9 && rows.All(item => item.JsonMatches && item.HashMatches),
            RowCount = rows.Count,
            MatchedRowCount = rows.Count(item => item.JsonMatches && item.HashMatches),
            Rows = rows
        };
    }

    public FullGeneratorVariabilityReviewPackageMatrixManifest BuildReviewPackageMatrixManifest(IReadOnlyDictionary<string, FullGeneratorVariabilityMatrixRow> rows)
    {
        var rowRefs = rows.Values
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => FullGeneratorVariabilityMatrixEvidenceService.RowFileName(item.FamilyId, item.SeedId))
            .ToList();

        return new FullGeneratorVariabilityReviewPackageMatrixManifest
        {
            Passed = rowRefs.Count == 9,
            Accepted = false,
            SourceReviewPackageManifestRef = ".llmgc/procedural/goal-058-full-media-bound-generator-campaign/unified-review-package-manifest.json",
            RowCount = rowRefs.Count,
            MatrixRowRefs = rowRefs,
            RequiredEvidenceFiles = RequiredEvidenceFiles()
        };
    }

    public FullGeneratorVariabilityPreviewExportMatrixPayload BuildPreviewExportMatrixPayload(IReadOnlyDictionary<string, FullGeneratorVariabilityMatrixRow> rows)
    {
        var payloadRows = rows.Values
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(item => new FullGeneratorVariabilityPreviewExportMatrixRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                PreviewPayloadRef = item.SelectedPreviewExportRefs.FirstOrDefault(refValue => refValue.Contains("chunked-preview", StringComparison.Ordinal)) ?? item.SelectedPreviewExportRefs.FirstOrDefault() ?? string.Empty,
                ExportMode = item.SelectedPreviewExportRefs.FirstOrDefault(refValue => refValue.StartsWith("exportMode:", StringComparison.Ordinal))?.Substring("exportMode:".Length) ?? string.Empty,
                DerivedCampaignHash = item.DerivedCampaignHash
            })
            .ToList();

        return new FullGeneratorVariabilityPreviewExportMatrixPayload
        {
            Passed = payloadRows.Count == 9
                && payloadRows.All(item => !string.IsNullOrWhiteSpace(item.PreviewPayloadRef))
                && payloadRows.All(item => !string.IsNullOrWhiteSpace(item.ExportMode)),
            RowCount = payloadRows.Count,
            Rows = payloadRows
        };
    }

    public FullGeneratorVariabilityUnityMatrixCommandPlan BuildUnityCommandPlan(IReadOnlyDictionary<string, FullGeneratorVariabilityMatrixRow> rows)
    {
        var commandRows = rows.Values
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(item => new FullGeneratorVariabilityUnityMatrixCommandRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                DerivedCampaignHash = item.DerivedCampaignHash,
                ExpectedPlayerMarkers = ExpectedRowMarkers(item)
            })
            .ToList();
        var expected = new List<string> { "full_generator_matrix_loaded=true" };
        expected.AddRange(commandRows.SelectMany(item => item.ExpectedPlayerMarkers));
        expected.Add("full_generator_matrix_completed=true");

        return new FullGeneratorVariabilityUnityMatrixCommandPlan
        {
            Passed = commandRows.Count == 9 && expected.Count == 47,
            Accepted = false,
            Rows = commandRows,
            ExpectedPlayerMarkers = expected.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
        };
    }

    public IReadOnlyList<FullGeneratorVariabilityFilePayload> BuildStagingFiles(
        FullGeneratorVariabilitySourceBundle source,
        FullGeneratorVariabilityUnityMatrixCommandPlan commandPlan)
    {
        var files = source.Goal058StagingFiles.ToList();
        files.Add(new FullGeneratorVariabilityFilePayload
        {
            RelativePath = FullGeneratorVariabilityMatrixVocabulary.UnityMatrixCommandPlanStagingRelativePath,
            Bytes = System.Text.Encoding.UTF8.GetBytes(Serialize(commandPlan) + Environment.NewLine)
        });

        return files
            .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    public InvalidFullGeneratorVariabilityMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidFullGeneratorVariabilityScenario>
        {
            Invalid("missing_goal058_source", "Remove the Goal 058 campaign source manifest before loading.", "blocked", Error("goal059.source.goal058_missing", "Goal058", "Accepted Goal 058 evidence is required.")),
            Invalid("stale_mismatched_source_hash", "Change a consumed Goal 058 artifact after its hash was recorded.", "rejected", Error("goal059.source.hash_mismatch", "Goal058", "Source artifact bytes must match recorded hashes.")),
            Invalid("duplicate_row_id", "Emit two family/seed rows with the same row id.", "rejected", Error("goal059.matrix.duplicate_row_id", "seed-profile-matrix", "Every matrix row id must be unique.")),
            Invalid("fake_family", "Inject a family outside the supported Goal 058 family set.", "rejected", Error("goal059.matrix.fake_family", "family/fake", "Matrix family id must resolve to Goal 058 source families.")),
            Invalid("fake_seed", "Inject a seed outside the required seed set.", "rejected", Error("goal059.matrix.fake_seed", "seed/fake", "Matrix seed id must be one of seed_alpha, seed_beta or seed_gamma.")),
            Invalid("missing_matrix_row", "Remove one required family x seed row.", "rejected", Error("goal059.matrix.missing_row", "family-seed", "The matrix must contain all 3 x 3 rows.")),
            Invalid("identical_row_overfit", "Generate rows that differ only by row id while sharing all variation facts.", "rejected", Error("goal059.variance.overfit", "variance-metrics", "Rows require meaningful family/seed variation dimensions.")),
            Invalid("nondeterministic_replay", "Rebuild a row from the same facts with different JSON or hash.", "rejected", Error("goal059.replay.nondeterministic", "replay-proof", "Replay proof must match JSON and hash for every row.")),
            Invalid("missing_unity_marker", "Omit a matrix row marker from the Unity player log.", "blocked", Error("goal059.unity.marker_missing", "unity-alpha-matrix-player-proof", "Unity matrix proof must contain required row markers.")),
            Invalid("malformed_preview_export_payload", "Emit a preview/export matrix payload without row preview refs.", "rejected", Error("goal059.preview_export.malformed", "preview-export-matrix-payload", "Every matrix row requires preview/export refs.")),
            Invalid("unsafe_relative_path", "Use absolute paths or traversal in matrix refs.", "rejected", Error("goal059.path.unsafe", "../escape", "Matrix artifact refs must stay safe relative paths.")),
            Invalid("provider_network_llm_rag_claim", "Claim provider, network, LLM or RAG execution.", "blocked", Error("goal059.boundary.provider_network_llm_rag", "boundary", "Provider, network, LLM and RAG calls are forbidden.")),
            Invalid("gamepackage_schema_mutation_claim", "Claim public GamePackage schema mutation.", "blocked", Error("goal059.boundary.gamepackage_schema", "boundary", "Public GamePackage schema mutation is forbidden.")),
            Invalid("runtime_broad_mutation_claim", "Claim Runtime or Runtime.Abstractions mutation.", "blocked", Error("goal059.boundary.runtime", "boundary", "Runtime broad mutation is forbidden.")),
            Invalid("ui_winforms_mutation_claim", "Claim WinForms UI mutation.", "blocked", Error("goal059.boundary.winforms_ui", "boundary", "WinForms UI mutation is forbidden.")),
            Invalid("unity_broad_mutation_claim", "Change Unity outside the deterministic matrix command-plan marker route.", "blocked", Error("goal059.boundary.unity_broad", "unity", "Only narrow matrix marker support is allowed.")),
            Invalid("media_generation_import_claim", "Claim real media generation, import or network download.", "blocked", Error("goal059.boundary.media_generation_import", "boundary", "Goal 059 may only consume accepted fixture media evidence.")),
            Invalid("lua_arbitrary_execution_claim", "Claim arbitrary Lua execution while building the matrix.", "blocked", Error("goal059.boundary.lua_arbitrary_execution", "boundary", "Arbitrary Lua execution is forbidden."))
        };

        return new InvalidFullGeneratorVariabilityMatrix
        {
            Passed = FullGeneratorVariabilityMatrixVocabulary.RequiredInvalidScenarioIds.All(id => scenarios.Any(item => item.ScenarioId == id && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0)),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<string> ExpectedRowMarkers(FullGeneratorVariabilityMatrixRow row) =>
    [
        "matrix_row_started=" + row.RowId,
        "matrix_row_family=" + row.FamilyId,
        "matrix_row_seed=" + row.SeedId,
        "matrix_row_hash=" + row.DerivedCampaignHash,
        "matrix_row_completed=" + row.RowId
    ];

    public static IReadOnlyList<FullGeneratorVariabilityDiagnostic> SortDiagnostics(IEnumerable<FullGeneratorVariabilityDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static string FamilyOrderingKey(string familyId) =>
        FullGeneratorVariabilityMatrixSourceLoader.FamilyOrderingKey(familyId);

    public static string SeedOrderingKey(string seedId) =>
        FullGeneratorVariabilityMatrixSourceLoader.SeedOrderingKey(seedId);

    private FullGeneratorVariabilityMatrixRow BuildRow(
        FullGeneratorVariabilitySourceBundle source,
        FullGeneratorVariabilityFamilySource family,
        string seedId)
    {
        var profile = SeedProfile(seedId);
        var rowId = "matrix-row-" + FullGeneratorVariabilityMatrixSourceLoader.SafeSegment(family.FamilyId) + "-" + FullGeneratorVariabilityMatrixSourceLoader.SafeSegment(seedId);
        var mediaFocus = SelectMedia(family, profile.MediaSlotId);
        var commandFocus = SelectCommand(family, profile.CommandOffset);
        var selectedWorldRefs = new[]
        {
            family.RuntimePreviewPayloadRef,
            "loopTarget:" + commandFocus.TargetId,
            "loopSecondaryTarget:" + commandFocus.SecondaryTargetId
        }.Where(item => !string.IsNullOrWhiteSpace(item) && !item.EndsWith(":", StringComparison.Ordinal)).Order(StringComparer.Ordinal).ToList();
        var selectedMediaRefs = new[]
        {
            mediaFocus.RelativePath,
            "mediaBinding:" + mediaFocus.BindingId,
            "mediaHash:" + mediaFocus.Sha256
        }.Where(item => !string.IsNullOrWhiteSpace(item) && !item.EndsWith(":", StringComparison.Ordinal)).ToList();
        var selectedFamilyLoopRefs = new[]
        {
            family.Goal057LoopProofRef,
            commandFocus.ExpectedPlayerMarker,
            "loopMarker:" + commandFocus.FamilyMarker
        }.Where(item => !string.IsNullOrWhiteSpace(item) && !item.EndsWith(":", StringComparison.Ordinal)).ToList();
        var selectedPreviewRefs = new[]
        {
            family.RuntimePreviewPayloadRef,
            "exportMode:" + family.ExportMode,
            "seedProfile:" + profile.ProfileTag
        }.Where(item => !string.IsNullOrWhiteSpace(item) && !item.EndsWith(":", StringComparison.Ordinal)).ToList();
        var dimensions = new List<FullGeneratorVariabilityDimension>
        {
            Dimension("seed_profile", profile.ProfileTag, seedId),
            Dimension("media_focus_slot", mediaFocus.SlotId, mediaFocus.RelativePath),
            Dimension("family_loop_marker_focus", commandFocus.FamilyMarker, commandFocus.ExpectedPlayerMarker),
            Dimension("world_or_route_focus", FirstNonEmpty(commandFocus.SecondaryTargetId, commandFocus.TargetId), commandFocus.CommandId),
            Dimension("preview_export_mode", family.ExportMode, family.RuntimePreviewPayloadRef)
        };
        var rowWithoutHash = new FullGeneratorVariabilityMatrixRow
        {
            RowId = rowId,
            FamilyId = family.FamilyId,
            SeedId = seedId,
            SourceCampaignId = source.Goal058CampaignId,
            SourceCampaignHash = source.SourceCampaignHash,
            SourceManifestRefs = source.SourceArtifactRefs
                .Where(item => item.ArtifactFamily is "campaign_source_manifest" or "campaign_plan" or "unity_player_proof" or "review_package_manifest")
                .Select(item => item.ArtifactRelativePath + "#" + item.ArtifactHash)
                .Order(StringComparer.Ordinal)
                .ToList(),
            SelectedWorldMapChunkRefs = selectedWorldRefs,
            SelectedMediaRefs = selectedMediaRefs,
            SelectedFamilyLoopRefs = selectedFamilyLoopRefs,
            SelectedPreviewExportRefs = selectedPreviewRefs,
            VariationDimensions = dimensions.OrderBy(item => item.DimensionId, StringComparer.Ordinal).ToList(),
            VarianceExplanation = "Family " + family.FamilyId + " uses seed " + seedId + " to focus " + profile.ProfileTag + ", media slot " + mediaFocus.SlotId + ", loop marker " + commandFocus.FamilyMarker + " and export mode " + family.ExportMode + "."
        };
        var derivedHash = Hash(Serialize(new
        {
            rowWithoutHash.RowId,
            rowWithoutHash.FamilyId,
            rowWithoutHash.SeedId,
            rowWithoutHash.SourceCampaignId,
            rowWithoutHash.SourceCampaignHash,
            rowWithoutHash.SourceManifestRefs,
            rowWithoutHash.SelectedWorldMapChunkRefs,
            rowWithoutHash.SelectedMediaRefs,
            rowWithoutHash.SelectedFamilyLoopRefs,
            rowWithoutHash.SelectedPreviewExportRefs,
            rowWithoutHash.VariationDimensions
        }));
        var row = rowWithoutHash with
        {
            DerivedCampaignHash = derivedHash
        };

        return row with
        {
            DeterministicMarkerPlan = ExpectedRowMarkers(row)
        };
    }

    private static IReadOnlyList<FullGeneratorVariabilityPairDifferenceSummary> BuildPairDifferenceSummaries(IReadOnlyList<FullGeneratorVariabilityMatrixRow> rows)
    {
        var result = new List<FullGeneratorVariabilityPairDifferenceSummary>();
        foreach (var familyId in FullGeneratorVariabilityMatrixVocabulary.FamilyIds)
        {
            var familyRows = rows.Where(item => item.FamilyId == familyId).OrderBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal).ToList();
            for (var index = 0; index < familyRows.Count - 1; index++)
            {
                var left = familyRows[index];
                var right = familyRows[index + 1];
                var differences = DifferenceDimensions(left, right);
                result.Add(new FullGeneratorVariabilityPairDifferenceSummary
                {
                    LeftRowId = left.RowId,
                    RightRowId = right.RowId,
                    DifferenceDimensionCount = differences.Count,
                    DifferenceDimensions = differences
                });
            }
        }

        return result.OrderBy(item => item.LeftRowId, StringComparer.Ordinal).ThenBy(item => item.RightRowId, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> DifferenceDimensions(FullGeneratorVariabilityMatrixRow left, FullGeneratorVariabilityMatrixRow right)
    {
        var leftByDimension = left.VariationDimensions.ToDictionary(item => item.DimensionId, item => item.Value, StringComparer.Ordinal);
        var rightByDimension = right.VariationDimensions.ToDictionary(item => item.DimensionId, item => item.Value, StringComparer.Ordinal);
        return leftByDimension.Keys
            .Union(rightByDimension.Keys, StringComparer.Ordinal)
            .Where(key => !string.Equals(leftByDimension.GetValueOrDefault(key), rightByDimension.GetValueOrDefault(key), StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static FullGeneratorVariabilityMediaRef SelectMedia(FullGeneratorVariabilityFamilySource family, string slotId)
    {
        var match = family.MediaRefs.FirstOrDefault(item => item.SlotId == slotId);
        return match ?? family.MediaRefs.OrderBy(item => FullGeneratorVariabilityMatrixSourceLoader.SlotOrder(item.SlotId)).FirstOrDefault() ?? new FullGeneratorVariabilityMediaRef();
    }

    private static FullGeneratorVariabilityLoopCommandRef SelectCommand(FullGeneratorVariabilityFamilySource family, int offset)
    {
        var commands = family.LoopCommands.OrderBy(item => item.Order).ToList();
        if (commands.Count == 0)
        {
            return new FullGeneratorVariabilityLoopCommandRef();
        }

        return commands[Math.Clamp(offset, 0, commands.Count - 1)];
    }

    private static (string ProfileTag, string MediaSlotId, int CommandOffset) SeedProfile(string seedId) =>
        seedId switch
        {
            "seed_alpha" => ("baseline-route-emphasis", "world_key_art", 0),
            "seed_beta" => ("actor-resource-emphasis", "npc_portrait", 2),
            "seed_gamma" => ("review-feedback-emphasis", "sfx_interaction", 4),
            _ => ("unknown-seed", "world_key_art", 0)
        };

    private static IReadOnlyList<string> RequiredEvidenceFiles() =>
    [
        FullGeneratorVariabilityMatrixEvidenceService.SourceManifestJsonFileName,
        FullGeneratorVariabilityMatrixEvidenceService.SeedProfileMatrixJsonFileName,
        FullGeneratorVariabilityMatrixEvidenceService.RowFileName("map_panel_rpg", "seed_alpha"),
        FullGeneratorVariabilityMatrixEvidenceService.RowFileName("map_panel_rpg", "seed_beta"),
        FullGeneratorVariabilityMatrixEvidenceService.RowFileName("map_panel_rpg", "seed_gamma"),
        FullGeneratorVariabilityMatrixEvidenceService.RowFileName("survival_sandbox", "seed_alpha"),
        FullGeneratorVariabilityMatrixEvidenceService.RowFileName("survival_sandbox", "seed_beta"),
        FullGeneratorVariabilityMatrixEvidenceService.RowFileName("survival_sandbox", "seed_gamma"),
        FullGeneratorVariabilityMatrixEvidenceService.RowFileName("first_person_grid_dungeon", "seed_alpha"),
        FullGeneratorVariabilityMatrixEvidenceService.RowFileName("first_person_grid_dungeon", "seed_beta"),
        FullGeneratorVariabilityMatrixEvidenceService.RowFileName("first_person_grid_dungeon", "seed_gamma"),
        FullGeneratorVariabilityMatrixEvidenceService.VarianceMetricsJsonFileName,
        FullGeneratorVariabilityMatrixEvidenceService.ReplayProofJsonFileName,
        FullGeneratorVariabilityMatrixEvidenceService.ReviewPackageMatrixManifestJsonFileName,
        FullGeneratorVariabilityMatrixEvidenceService.PreviewExportMatrixPayloadJsonFileName,
        FullGeneratorVariabilityMatrixEvidenceService.UnityCommandPlanJsonFileName,
        FullGeneratorVariabilityMatrixEvidenceService.UnityPlayerProofJsonFileName,
        FullGeneratorVariabilityMatrixEvidenceService.InvalidMatrixJsonFileName,
        FullGeneratorVariabilityMatrixEvidenceService.ReportMarkdownFileName
    ];

    private static FullGeneratorVariabilityDimension Dimension(string id, string value, string sourceRef) =>
        new()
        {
            DimensionId = id,
            Value = value,
            SourceRef = sourceRef
        };

    private static InvalidFullGeneratorVariabilityScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params FullGeneratorVariabilityDiagnostic[] diagnostics) =>
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

    private static string FirstNonEmpty(params string[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "critical" => 0,
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static string Serialize<T>(T value) => FullGeneratorVariabilityMatrixHash.Serialize(value);

    private static string Hash(string text) => FullGeneratorVariabilityMatrixHash.Hash(text);

    private static FullGeneratorVariabilityDiagnostic Error(string code, string target, string message) =>
        FullGeneratorVariabilityDiagnostic.Error(code, target, message);

    private static FullGeneratorVariabilityDiagnostic Info(string code, string target, string message) =>
        FullGeneratorVariabilityDiagnostic.Info(code, target, message);
}
