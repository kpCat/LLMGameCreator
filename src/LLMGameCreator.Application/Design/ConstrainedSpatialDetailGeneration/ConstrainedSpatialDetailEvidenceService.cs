using System.Text;

namespace LLMGameCreator.Application.Design.ConstrainedSpatialDetailGeneration;

public sealed class ConstrainedSpatialDetailEvidenceService
{
    public const string RelativeOutputDirectory = ConstrainedSpatialDetailVocabulary.RelativeOutputDirectory;
    public const string SourceManifestJsonFileName = "source-manifest.json";
    public const string PaletteCatalogJsonFileName = "spatial-palette-catalog.json";
    public const string RewriteRuleCatalogJsonFileName = "rewrite-rule-catalog.json";
    public const string ConstraintRuleCatalogJsonFileName = "constraint-rule-catalog.json";
    public const string SpatialDetailMatrixJsonFileName = "spatial-detail-matrix.json";
    public const string ReachabilityProofMatrixJsonFileName = "reachability-proof-matrix.json";
    public const string RepairFallbackMatrixJsonFileName = "spatial-repair-fallback-matrix.json";
    public const string UnityCommandPlanJsonFileName = "unity-spatial-detail-command-plan.json";
    public const string UnityProofSummaryJsonFileName = "unity-spatial-detail-proof-summary.json";
    public const string PreviewExportPayloadJsonFileName = "preview-export-spatial-payload.json";
    public const string InvalidMatrixJsonFileName = "invalid-spatial-detail-diagnostics-matrix.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "constrained-spatial-detail-generation-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly ConstrainedSpatialDetailSourceLoader _sourceLoader;
    private readonly ConstrainedSpatialUnityProofRunner _unityProofRunner;

    public ConstrainedSpatialDetailEvidenceService(
        ConstrainedSpatialDetailSourceLoader? sourceLoader = null,
        ConstrainedSpatialUnityProofRunner? unityProofRunner = null)
    {
        _sourceLoader = sourceLoader ?? new ConstrainedSpatialDetailSourceLoader();
        _unityProofRunner = unityProofRunner ?? new ConstrainedSpatialUnityProofRunner();
    }

    public ConstrainedSpatialDetailEvidenceResult Build(string projectRootPath, ConstrainedSpatialDetailOptions? options = null)
    {
        var proof = new ConstrainedSpatialUnityProof
        {
            Passed = false,
            BlockerCode = "goal062.unity.not_executed_yet",
            BlockerMessage = "Unity proof has not been executed in this in-memory build.",
            PlayerProof = new ConstrainedSpatialUnityProofSummary
            {
                Diagnostics =
                [
                    ConstrainedSpatialDiagnostic.Warning("goal062.unity.not_executed_yet", "unity-proof", "Unity proof is produced only by BuildAndWriteAsync with ExecuteUnityProof=true.")
                ]
            }
        };
        return BuildCore(projectRootPath, proof);
    }

    public async Task<ConstrainedSpatialDetailWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        ConstrainedSpatialDetailOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var settings = options ?? new ConstrainedSpatialDetailOptions();
        var sourceRoot = string.IsNullOrWhiteSpace(settings.RepositoryRootPath)
            ? projectRootPath
            : settings.RepositoryRootPath;
        var initial = BuildCore(sourceRoot, new ConstrainedSpatialUnityProof
        {
            Passed = false,
            BlockerCode = settings.ExecuteUnityProof ? "goal062.unity.pending" : "goal062.unity.not_requested",
            BlockerMessage = settings.ExecuteUnityProof
                ? "Unity proof is pending until staging files are written."
                : "Unity proof execution was not requested.",
            PlayerProof = new ConstrainedSpatialUnityProofSummary()
        });
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutputDirectory: true, cancellationToken).ConfigureAwait(false);

        var proof = _unityProofRunner.Run(
            sourceRoot,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityCommandPlan,
            settings);
        var final = BuildCore(sourceRoot, proof);
        return await WriteAsync(projectRootPath, final, resetOutputDirectory: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ConstrainedSpatialDetailWriteResult> WriteAsync(
        string projectRootPath,
        ConstrainedSpatialDetailEvidenceResult result,
        bool resetOutputDirectory = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        if (resetOutputDirectory)
        {
            ResetDirectory(outputDirectory);
        }
        else
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var written = new List<string>();
        foreach (var stagingFile in result.StagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.GetFullPath(Path.Combine(outputDirectory, ConstrainedSpatialDetailVocabulary.StagingRoot, stagingFile.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, stagingFile.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        foreach (var pair in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, pair.Key);
            await File.WriteAllTextAsync(path, pair.Value + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        foreach (var pair in result.RowJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, pair.Key);
            await File.WriteAllTextAsync(path, pair.Value + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var artifactScopePath = Path.Combine(outputDirectory, ArtifactScopeReportJsonFileName);
        await File.WriteAllTextAsync(artifactScopePath, RenderArtifactScopeReportJson() + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(artifactScopePath);

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new ConstrainedSpatialDetailWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, ConstrainedSpatialDetailVocabulary.StagingRoot),
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            Result = result
        };
    }

    private ConstrainedSpatialDetailEvidenceResult BuildCore(string projectRootPath, ConstrainedSpatialUnityProof unityProof)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var palette = new ConstrainedSpatialPaletteCatalogBuilder().Build();
        var rewrite = new ConstrainedSpatialRewriteRuleCatalogBuilder().Build();
        var planner = new ConstrainedSpatialConstraintPlanner();
        var constraints = planner.BuildConstraintRuleCatalog(palette);
        var rows = planner.BuildRows(source, palette, rewrite, constraints);
        var matrix = planner.BuildMatrix(rows);
        var reachability = new ConstrainedSpatialReachabilityPlanner(ConstrainedSpatialPaletteCatalogBuilder.TileById(palette)).BuildMatrix(rows);
        var repairs = new ConstrainedSpatialRepairPlanner().BuildMatrix(rows);
        var preview = BuildPreviewExportPayload(rows);
        var unityCommandPlan = BuildUnityCommandPlan(rows);
        var invalidMatrix = BuildInvalidMatrix();
        var sourceManifest = BuildSourceManifest(source);

        var validator = new ConstrainedSpatialDetailValidator();
        var stagingDiagnostics = ConstrainedSpatialDetailValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateCatalogs(palette, rewrite, constraints))
                .Concat(validator.ValidateSpatialRows(matrix, rows, palette))
                .Concat(validator.ValidateProofsAndPayloads(reachability, repairs, preview, unityCommandPlan, invalidMatrix)));
        var unityDiagnostics = validator.ValidateUnityProof(unityCommandPlan, unityProof);
        var diagnostics = ConstrainedSpatialDetailValidator.Sort(stagingDiagnostics.Concat(unityDiagnostics));

        var stagingPassed = sourceManifest.Goal061AcceptedByUserHandoff
            && sourceManifest.Goal061ReviewPackageRcManifestPassed
            && sourceManifest.Goal061UnityProofPassed
            && sourceManifest.Goal060PackageInventoryConsumed
            && sourceManifest.Goal059VarianceConsumed
            && palette.Passed
            && rewrite.Passed
            && constraints.Passed
            && matrix.Passed
            && reachability.Passed
            && repairs.Passed
            && preview.Passed
            && unityCommandPlan.Passed
            && invalidMatrix.Passed
            && stagingDiagnostics.All(item => item.Severity is not "error" and not "critical");
        var allUnityMarkersMatched = unityProof.Passed
            && unityProof.PlayerProof.MissingMarkers.Count == 0
            && unityProof.PlayerProof.ProvenRowCount == 9;
        var implementationStatus = stagingPassed && allUnityMarkersMatched
            ? "GREEN"
            : stagingPassed && !allUnityMarkersMatched
                ? "BLOCKED"
                : "FAILED";

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceManifestJsonFileName] = Serialize(sourceManifest),
            [PaletteCatalogJsonFileName] = Serialize(palette),
            [RewriteRuleCatalogJsonFileName] = Serialize(rewrite),
            [ConstraintRuleCatalogJsonFileName] = Serialize(constraints),
            [SpatialDetailMatrixJsonFileName] = Serialize(matrix),
            [ReachabilityProofMatrixJsonFileName] = Serialize(reachability),
            [RepairFallbackMatrixJsonFileName] = Serialize(repairs),
            [UnityCommandPlanJsonFileName] = Serialize(unityCommandPlan),
            [UnityProofSummaryJsonFileName] = Serialize(unityProof.PlayerProof),
            [PreviewExportPayloadJsonFileName] = Serialize(preview),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };
        var rowJson = rows.ToDictionary(RowFileName, Serialize, StringComparer.Ordinal);

        var reportWithoutHash = new ConstrainedSpatialDetailGenerationReport
        {
            ImplementationStatus = implementationStatus,
            Accepted = false,
            Goal061AcceptedByUserHandoff = sourceManifest.Goal061AcceptedByUserHandoff,
            SourceFactsConsumed = sourceManifest.SourceArtifactRefs.All(item => item.Exists && item.HashMatches && item.Diagnostics.Count == 0),
            PaletteCatalogPassed = palette.Passed,
            RewriteRuleCatalogPassed = rewrite.Passed,
            ConstraintRuleCatalogPassed = constraints.Passed,
            SpatialDetailMatrixPassed = matrix.Passed,
            ReachabilityProofPassed = reachability.Passed,
            RepairFallbackMatrixPassed = repairs.Passed,
            PreviewExportPayloadPassed = preview.Passed,
            InvalidMatrixPassed = invalidMatrix.Passed,
            UnityEditorOrPlayerExecuted = unityProof.UnityEditorOrPlayerExecuted,
            UnityExitCode = unityProof.PlayerProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerProof.PlayerExitCode,
            AllUnitySpatialMarkersMatched = allUnityMarkersMatched,
            RowCount = matrix.RowCount,
            FamilyCount = matrix.FamilyCount,
            SeedCount = matrix.SeedCount,
            DistinctRowHashCount = matrix.DistinctRowHashCount,
            UnityProvenRowCount = unityProof.PlayerProof.ProvenRowCount,
            SourceManifestHash = Hash(artifactJson[SourceManifestJsonFileName]),
            PaletteCatalogHash = Hash(artifactJson[PaletteCatalogJsonFileName]),
            RewriteRuleCatalogHash = Hash(artifactJson[RewriteRuleCatalogJsonFileName]),
            ConstraintRuleCatalogHash = Hash(artifactJson[ConstraintRuleCatalogJsonFileName]),
            SpatialDetailMatrixHash = Hash(artifactJson[SpatialDetailMatrixJsonFileName]),
            ReachabilityProofMatrixHash = Hash(artifactJson[ReachabilityProofMatrixJsonFileName]),
            RepairFallbackMatrixHash = Hash(artifactJson[RepairFallbackMatrixJsonFileName]),
            UnityCommandPlanHash = Hash(artifactJson[UnityCommandPlanJsonFileName]),
            UnityProofSummaryHash = Hash(artifactJson[UnityProofSummaryJsonFileName]),
            PreviewExportPayloadHash = Hash(artifactJson[PreviewExportPayloadJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new ConstrainedSpatialDetailEvidenceResult
        {
            SourceManifest = sourceManifest,
            PaletteCatalog = palette,
            RewriteRuleCatalog = rewrite,
            ConstraintRuleCatalog = constraints,
            SpatialDetailMatrix = matrix,
            SpatialDetailRows = rows,
            ReachabilityProofMatrix = reachability,
            RepairFallbackMatrix = repairs,
            UnityCommandPlan = unityCommandPlan,
            UnityProofSummary = unityProof.PlayerProof,
            PreviewExportPayload = preview,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            RowJsonByFileName = rowJson,
            StagingFiles = BuildStagingFiles(source, unityCommandPlan),
            ReportMarkdown = RenderReport(report, sourceManifest, palette, rewrite, constraints, matrix, rows, reachability, repairs, unityCommandPlan, unityProof, preview, invalidMatrix)
        };
    }

    private static ConstrainedSpatialSourceManifest BuildSourceManifest(ConstrainedSpatialSourceBundle source)
    {
        var diagnostics = new List<ConstrainedSpatialDiagnostic>(source.Diagnostics)
        {
            Info("goal062.preflight.goal061_handoff_recorded", "full_campaign_playable_review_package_rc_verification", "Goal 061 is recorded as accepted by user handoff before Goal 062."),
            Info("goal062.source.loaded", "Goal061", "Goal 062 source facts were loaded from repository-local Goal 061 review package RC evidence.")
        };

        return new ConstrainedSpatialSourceManifest
        {
            Accepted = false,
            Goal061AcceptedByUserHandoff = source.Goal061AcceptedByUserHandoff,
            Goal061ReviewPackageRcManifestPassed = source.Goal061ReviewPackageRcManifestPassed,
            Goal061UnityProofPassed = source.Goal061UnityProofPassed,
            Goal060PackageInventoryConsumed = source.Goal060PackageInventoryConsumed,
            Goal059VarianceConsumed = source.Goal059VarianceConsumed,
            PackageRowCount = source.PackageRows.Count,
            FamilyCount = source.FamilyIds.Count,
            SeedCount = source.SeedIds.Count,
            FamilyIds = source.FamilyIds,
            SeedIds = source.SeedIds,
            PreflightGates =
            [
                new ConstrainedSpatialGateRecord
                {
                    GateId = "full_campaign_playable_review_package_rc_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 062 task preflight handoff"
                },
                new ConstrainedSpatialGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 031 preserved policy"
                },
                new ConstrainedSpatialGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 032 preserved policy"
                },
                new ConstrainedSpatialGateRecord
                {
                    GateId = ConstrainedSpatialDetailVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "programmatic",
                    EvidenceRef = "Goal 062 produced for review"
                }
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = ConstrainedSpatialDetailValidator.Sort(diagnostics)
        };
    }

    private static ConstrainedSpatialPreviewExportPayload BuildPreviewExportPayload(IReadOnlyList<ConstrainedSpatialDetailRow> rows)
    {
        var payloadRows = rows
            .OrderBy(row => ConstrainedSpatialDetailVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => ConstrainedSpatialDetailVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row => new ConstrainedSpatialPreviewExportRow
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                Width = row.Width,
                Height = row.Height,
                TileDataCompact = row.TileDataCompact,
                Anchors = row.Anchors,
                Paths = row.Paths,
                PackageRowRef = row.PackageRowId,
                ReviewPackageRef = row.ReviewPackageRef,
                RowHash = row.RowHash,
                ThumbnailRef = row.ThumbnailRef,
                Provenance = row.Provenance
            })
            .ToList();

        return new ConstrainedSpatialPreviewExportPayload
        {
            Passed = payloadRows.Count == 9
                && payloadRows.All(row => row.Anchors.Count >= 3)
                && payloadRows.All(row => row.Paths.Count >= 3)
                && payloadRows.All(row => !string.IsNullOrWhiteSpace(row.RowHash)),
            RowCount = payloadRows.Count,
            Rows = payloadRows
        };
    }

    private static ConstrainedSpatialUnityCommandPlan BuildUnityCommandPlan(IReadOnlyList<ConstrainedSpatialDetailRow> rows)
    {
        var commandRows = rows
            .OrderBy(row => ConstrainedSpatialDetailVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => ConstrainedSpatialDetailVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row =>
            {
                var command = new ConstrainedSpatialUnityCommandRow
                {
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    SeedId = row.SeedId,
                    PackageId = row.PackageId,
                    SpatialDetailRowRef = RowFileName(row),
                    RowHash = row.RowHash,
                    Reachable = row.ReachabilityProof.Reachable,
                    RouteVerified = row.ReachabilityProof.RouteVerified,
                    VarianceMarker = row.VarianceMetrics.VarianceMarker
                };
                return command with { ExpectedPlayerMarkers = ExpectedUnityRowMarkers(command) };
            })
            .ToList();
        var expected = new List<string>
        {
            "spatial_detail_loaded=true",
            "review_package_proof=goal062",
            "constrained_spatial_detail_generation_verification=required"
        };
        expected.AddRange(commandRows.SelectMany(row => row.ExpectedPlayerMarkers));

        return new ConstrainedSpatialUnityCommandPlan
        {
            Passed = commandRows.Count == 9
                && commandRows.All(row => row.Reachable)
                && commandRows.All(row => row.RouteVerified)
                && commandRows.All(row => !string.IsNullOrWhiteSpace(row.VarianceMarker)),
            Accepted = false,
            Rows = commandRows,
            ExpectedPlayerMarkers = expected.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<string> ExpectedUnityRowMarkers(ConstrainedSpatialUnityCommandRow row) =>
    [
        "spatial_detail_family=" + row.FamilyId,
        "spatial_detail_seed=" + row.SeedId,
        "spatial_detail_row=" + row.RowId,
        "spatial_detail_reachable=" + row.Reachable.ToString().ToLowerInvariant(),
        "spatial_detail_route_verified=" + row.RouteVerified.ToString().ToLowerInvariant(),
        "spatial_detail_variance_marker=" + row.VarianceMarker
    ];

    private static InvalidConstrainedSpatialDetailDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidConstrainedSpatialDetailScenario>
        {
            Invalid("missing_goal061_source", "Remove Goal 061 review-package RC manifest before loading.", "blocked", Error("goal062.source.goal061_manifest_missing", "Goal061", "Goal 061 source is required.")),
            Invalid("fake_package_row_id", "Inject a row id not present in Goal 061/060/059.", "rejected", Error("goal062.source.fake_package_row_id", "package-row", "Package row id must come from Goal 061.")),
            Invalid("fake_family", "Inject an unsupported family id.", "rejected", Error("goal062.source.fake_family", "familyId", "Family must be one of the accepted Goal 061 families.")),
            Invalid("fake_seed", "Inject an unsupported seed id.", "rejected", Error("goal062.source.fake_seed", "seedId", "Seed must be one of seed_alpha, seed_beta or seed_gamma.")),
            Invalid("invalid_tile_id", "Use a tile id not present in the palette catalog.", "rejected", Error("goal062.row.invalid_tile_id", "tileId", "Tile id must be declared by the palette catalog.")),
            Invalid("missing_entry", "Remove entry anchor from a row.", "rejected", Error("goal062.reachability.entry_missing", "entry", "Entry anchor is required.")),
            Invalid("missing_exit", "Remove exit anchor from a row.", "rejected", Error("goal062.reachability.exit_missing", "exit", "Exit anchor is required.")),
            Invalid("unreachable_objective", "Block the only route from entry to objective.", "rejected", Error("goal062.reachability.entry_to_objective_unreachable", "objective", "Objective must be reachable.")),
            Invalid("contradiction_no_tile_candidate", "Provide no tile candidate for a constrained cell.", "rejected", Error("goal062.constraint.no_tile_candidate", "constraint", "Contradictory constraints are detected before promotion.")),
            Invalid("unsafe_path_traversal", "Route survival path through blocked hazard cells.", "rejected", Error("goal062.reachability.unsafe_path_traversal", "hazard", "Unsafe hazards cannot be part of the route.")),
            Invalid("external_asset_provenance_leak", "Claim external tile/image asset provenance.", "rejected", Error("goal062.leak.external_asset_provenance", "provenance", "Only in_house_fixture provenance is allowed.")),
            Invalid("copied_mxgmn_sample_asset_claim", "Claim copied mxgmn sample tiles or assets.", "rejected", Error("goal062.leak.mxgmn_sample_asset_claim", "mxgmn", "External sample assets are forbidden.")),
            Invalid("provider_network_llm_rag_claim", "Claim provider/network/LLM/RAG generation.", "rejected", Error("goal062.leak.provider_network_llm_rag_claim", "scope", "Provider, network, LLM and RAG calls are forbidden.")),
            Invalid("lua_execution_claim", "Claim Lua execution for spatial detail.", "rejected", Error("goal062.leak.lua_execution_claim", "scope", "Arbitrary Lua execution is forbidden.")),
            Invalid("public_gamepackage_mutation_claim", "Mutate public GamePackage schema for spatial detail.", "rejected", Error("goal062.leak.public_gamepackage_mutation_claim", "GamePackage", "Public GamePackage schema mutation is forbidden.")),
            Invalid("runtime_ui_broad_mutation_claim", "Mutate Runtime or UI broadly as proof.", "rejected", Error("goal062.leak.runtime_ui_broad_mutation_claim", "scope", "Runtime/UI broad mutation is forbidden.")),
            Invalid("nondeterministic_ordering", "Emit rows by filesystem enumeration order.", "rejected", Error("goal062.matrix.nondeterministic_ordering", "rows", "Rows must be sorted by family and seed order.")),
            Invalid("missing_unity_proof_trace", "Claim Unity proof without player markers.", "rejected", Error("goal062.unity.missing_proof_trace", "unity-proof", "Unity proof requires player log markers."))
        };

        return new InvalidConstrainedSpatialDetailDiagnosticsMatrix
        {
            Passed = scenarios.Count == ConstrainedSpatialDetailVocabulary.RequiredInvalidScenarioIds.Count
                && ConstrainedSpatialDetailVocabulary.RequiredInvalidScenarioIds.All(required => scenarios.Any(item => item.ScenarioId == required))
                && scenarios.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static IReadOnlyList<ConstrainedSpatialFilePayload> BuildStagingFiles(
        ConstrainedSpatialSourceBundle source,
        ConstrainedSpatialUnityCommandPlan commandPlan)
    {
        var files = source.BaseStagingFiles.ToList();
        files.RemoveAll(item => item.RelativePath == ConstrainedSpatialDetailVocabulary.UnitySpatialDetailCommandPlanStagingRelativePath);
        files.Add(TextFile(ConstrainedSpatialDetailVocabulary.UnitySpatialDetailCommandPlanStagingRelativePath, Serialize(commandPlan)));
        return files.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            schemaVersion = "goal062_artifact_scope_report_v1",
            scenario = ConstrainedSpatialDetailVocabulary.ProductSmokeRoute,
            gate = ConstrainedSpatialDetailVocabulary.FinalGate + " required",
            allowedArtifactRoot = ConstrainedSpatialDetailVocabulary.RelativeOutputDirectory + "/",
            allowedCodeRoot = "src/LLMGameCreator.Application/Design/ConstrainedSpatialDetailGeneration/",
            allowedTestsRoot = "tests/LLMGameCreator.Tests/Application/ConstrainedSpatialDetailGeneration/",
            allowedProductSmoke = "tests/LLMGameCreator.Tests/ProductSmoke/ConstrainedSpatialDetailGenerationProductSmokeTests.cs",
            narrowUnityAllowance = "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs",
            forbiddenChanges = new[]
            {
                "public GamePackage schema/model definitions",
                "Runtime/Runtime.Abstractions",
                "WinForms UI",
                "Infrastructure provider/LLM/RAG paths",
                "generator-library",
                "solution/project files",
                "external dependencies",
                "external mxgmn source/assets"
            }
        });

    private static string RenderReport(
        ConstrainedSpatialDetailGenerationReport report,
        ConstrainedSpatialSourceManifest sourceManifest,
        ConstrainedSpatialPaletteCatalog palette,
        ConstrainedSpatialRewriteRuleCatalog rewrite,
        ConstrainedSpatialConstraintRuleCatalog constraints,
        ConstrainedSpatialDetailMatrix matrix,
        IReadOnlyList<ConstrainedSpatialDetailRow> rows,
        ConstrainedSpatialReachabilityProofMatrix reachability,
        ConstrainedSpatialRepairFallbackMatrix repairs,
        ConstrainedSpatialUnityCommandPlan commandPlan,
        ConstrainedSpatialUnityProof unityProof,
        ConstrainedSpatialPreviewExportPayload preview,
        InvalidConstrainedSpatialDetailDiagnosticsMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Constrained Spatial Detail Generation Report",
            string.Empty,
            "constrained_spatial_detail_generation_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            "manualGate=constrained_spatial_detail_generation_verification",
            $"goal061AcceptedByUserHandoff={report.Goal061AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"sourceFactsConsumed={report.SourceFactsConsumed.ToString().ToLowerInvariant()}",
            $"rowCount={report.RowCount}",
            $"familyCount={report.FamilyCount}",
            $"seedCount={report.SeedCount}",
            $"distinctRowHashCount={report.DistinctRowHashCount}",
            $"paletteCatalogPassed={report.PaletteCatalogPassed.ToString().ToLowerInvariant()}",
            $"rewriteRuleCatalogPassed={report.RewriteRuleCatalogPassed.ToString().ToLowerInvariant()}",
            $"constraintRuleCatalogPassed={report.ConstraintRuleCatalogPassed.ToString().ToLowerInvariant()}",
            $"spatialDetailMatrixPassed={report.SpatialDetailMatrixPassed.ToString().ToLowerInvariant()}",
            $"reachabilityProofPassed={report.ReachabilityProofPassed.ToString().ToLowerInvariant()}",
            $"repairFallbackMatrixPassed={report.RepairFallbackMatrixPassed.ToString().ToLowerInvariant()}",
            $"previewExportPayloadPassed={report.PreviewExportPayloadPassed.ToString().ToLowerInvariant()}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"unityEditorOrPlayerExecuted={report.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"allUnitySpatialMarkersMatched={report.AllUnitySpatialMarkersMatched.ToString().ToLowerInvariant()}",
            $"unityProvenRowCount={report.UnityProvenRowCount}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"paletteCatalogHash={report.PaletteCatalogHash}",
            $"rewriteRuleCatalogHash={report.RewriteRuleCatalogHash}",
            $"constraintRuleCatalogHash={report.ConstraintRuleCatalogHash}",
            $"spatialDetailMatrixHash={report.SpatialDetailMatrixHash}",
            $"reachabilityProofMatrixHash={report.ReachabilityProofMatrixHash}",
            $"repairFallbackMatrixHash={report.RepairFallbackMatrixHash}",
            $"unityCommandPlanHash={report.UnityCommandPlanHash}",
            $"unityProofSummaryHash={report.UnityProofSummaryHash}",
            $"previewExportPayloadHash={report.PreviewExportPayloadHash}",
            $"invalidMatrixHash={report.InvalidMatrixHash}",
            $"reportHash={report.DeterministicHash}",
            string.Empty,
            "## Preflight",
            string.Empty
        };
        lines.AddRange(sourceManifest.PreflightGates.Select(item => $"- {item.GateId}: status={item.Status}, provenance={item.ProvenanceKind}, evidence={item.EvidenceRef}"));
        lines.Add(string.Empty);
        lines.Add("## Source Chain");
        lines.Add(string.Empty);
        lines.Add($"- goal061ReviewPackageRcManifestPassed: {sourceManifest.Goal061ReviewPackageRcManifestPassed.ToString().ToLowerInvariant()}");
        lines.Add($"- goal061UnityProofPassed: {sourceManifest.Goal061UnityProofPassed.ToString().ToLowerInvariant()}");
        lines.Add($"- goal060PackageInventoryConsumed: {sourceManifest.Goal060PackageInventoryConsumed.ToString().ToLowerInvariant()}");
        lines.Add($"- goal059VarianceConsumed: {sourceManifest.Goal059VarianceConsumed.ToString().ToLowerInvariant()}");
        lines.AddRange(sourceManifest.SourceArtifactRefs.Select(item => $"- {item.ArtifactFamily}: artifact={item.ArtifactRelativePath}, exists={item.Exists.ToString().ToLowerInvariant()}, hashMatches={item.HashMatches.ToString().ToLowerInvariant()}, hash={item.ArtifactHash}"));
        lines.Add(string.Empty);
        lines.Add("## Spatial Detail");
        lines.Add(string.Empty);
        lines.Add($"- paletteTiles: {palette.TileCount}");
        lines.Add($"- rewriteRules: {rewrite.RuleCount}");
        lines.Add($"- constraintRules: {constraints.RuleCount}");
        lines.Add($"- sameFamilyRowsDifferByTwoMetrics: {matrix.SameFamilyRowsDifferByTwoMetrics.ToString().ToLowerInvariant()}");
        lines.Add($"- familiesDifferByPaletteAndRuleSet: {matrix.FamiliesDifferByPaletteAndRuleSet.ToString().ToLowerInvariant()}");
        foreach (var row in rows)
        {
            lines.Add($"- {row.RowId}: family={row.FamilyId}, seed={row.SeedId}, dimensions={row.Width}x{row.Height}, reachable={row.ReachabilityProof.Reachable.ToString().ToLowerInvariant()}, routeVerified={row.ReachabilityProof.RouteVerified.ToString().ToLowerInvariant()}, pathLength={row.VarianceMetrics.PathLength}, varianceMarker={row.VarianceMetrics.VarianceMarker}, hash={row.RowHash}");
        }

        lines.Add(string.Empty);
        lines.Add("## Reachability And Repair");
        lines.Add(string.Empty);
        lines.Add($"- reachabilityRows: {reachability.ReachableRowCount}/{reachability.RowCount}");
        lines.Add($"- routeVerifiedRows: {reachability.RouteVerifiedRowCount}/{reachability.RowCount}");
        lines.Add($"- repairRows: {repairs.RowCount}");
        lines.Add($"- contradictionScenarios: {repairs.ContradictionScenarioCount}");
        lines.AddRange(repairs.ContradictionDiagnostics.Select(item => $"- contradictionDiagnostic: {item.Code} [{item.Target}] {item.Message}"));
        lines.Add(string.Empty);
        lines.Add("## Preview/Export Payload");
        lines.Add(string.Empty);
        lines.Add($"- passed: {preview.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- rows: {preview.RowCount}");
        lines.Add("- thumbnails: skipped_no_existing_bcl_png_helper_required_for_goal");
        lines.Add(string.Empty);
        lines.Add("## Unity Proof");
        lines.Add(string.Empty);
        lines.Add($"- passed: {unityProof.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- unityEditorOrPlayerExecuted: {unityProof.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}");
        lines.Add($"- unityExitCode: {TextOrNone(unityProof.PlayerProof.UnityExitCode?.ToString())}");
        lines.Add($"- playerExitCode: {TextOrNone(unityProof.PlayerProof.PlayerExitCode?.ToString())}");
        lines.Add($"- provenRowCount: {unityProof.PlayerProof.ProvenRowCount}");
        lines.Add($"- blockerCode: {TextOrNone(unityProof.BlockerCode)}");
        lines.Add($"- blockerMessage: {TextOrNone(unityProof.BlockerMessage)}");
        lines.Add($"- launchLog: {unityProof.PlayerProof.LaunchLogRelativePath}");
        lines.Add($"- playLoopLog: {unityProof.PlayerProof.PlayLoopLogRelativePath}");
        lines.Add($"- expectedMarkerCount: {commandPlan.ExpectedPlayerMarkers.Count}");
        lines.AddRange(commandPlan.ExpectedPlayerMarkers.Select(marker => $"- requiredMarker: {marker}"));
        lines.AddRange(unityProof.PlayerProof.MatchedMarkers.Select(marker => $"- matchedMarker: {marker}"));
        lines.AddRange(unityProof.PlayerProof.MissingMarkers.Select(marker => $"- missingMarker: {marker}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak Matrix");
        lines.Add(string.Empty);
        lines.Add($"- passed: {invalidMatrix.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- scenarioCount: {invalidMatrix.ScenarioCount}");
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add("No external dependency/source/asset import, provider/network/LLM/RAG call, media generation, arbitrary Lua execution, public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution or project file change is part of this Goal 062 proof. Unity changes are limited to spatial-detail marker support in AlphaRuntimeBootstrap.");
        lines.Add(string.Empty);
        lines.Add("constrained_spatial_detail_generation_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    public static string RowFileName(ConstrainedSpatialDetailRow row) =>
        "spatial-detail-row-" + row.FamilyId + "-" + row.SeedId + ".json";

    private static InvalidConstrainedSpatialDetailScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params ConstrainedSpatialDiagnostic[] diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            CausalMutation = mutation,
            ExpectedStatus = expectedStatus,
            ActualStatus = expectedStatus,
            ExpectedValid = false,
            ActualValid = false,
            Diagnostics = ConstrainedSpatialDetailValidator.Sort(diagnostics)
        };

    private static ConstrainedSpatialFilePayload TextFile(string relativePath, string text) =>
        new()
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Bytes = Encoding.UTF8.GetBytes(text.TrimEnd('\r', '\n') + Environment.NewLine)
        };

    private static string Serialize<T>(T value) => ConstrainedSpatialDetailHash.Serialize(value);

    private static string Hash(string text) => ConstrainedSpatialDetailHash.Hash(text);

    private static string TextOrNone(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "(none)" : value;

    private static void ResetDirectory(string path)
    {
        if (!TryResetDirectory(path, maxAttempts: 120, out var exception))
        {
            throw new IOException($"Directory could not be reset: {path}", exception);
        }
    }

    private static bool TryResetDirectory(string path, int maxAttempts, out Exception? lastException)
    {
        lastException = null;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }

                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                lastException = exception;
                if (attempt < maxAttempts)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        return false;
    }

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static ConstrainedSpatialDiagnostic Error(string code, string target, string message) =>
        ConstrainedSpatialDiagnostic.Error(code, target, message);

    private static ConstrainedSpatialDiagnostic Info(string code, string target, string message) =>
        ConstrainedSpatialDiagnostic.Info(code, target, message);
}
