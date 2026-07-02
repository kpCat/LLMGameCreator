using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.VisualAssetContractRatingMetadata;

public sealed class VisualAssetContractRatingMetadataEvidenceService
{
    public const string ReportMarkdownFileName = "visual-asset-contract-rating-metadata-report.md";
    public const string CatalogJsonFileName = "visual-asset-contract-catalog.json";
    public const string RatingPolicyMatrixJsonFileName = "visual-rating-policy-matrix.json";
    public const string ValidationMatrixJsonFileName = "visual-contract-validation-matrix.json";
    public const string NegativeProofJsonFileName = "visual-contract-negative-proof.json";
    public const string SourceDocumentLineageJsonFileName = "source-document-lineage.json";
    public const string QualityGateScanJsonFileName = "quality-gate-scan.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static VisualAssetContractRatingMetadataEvidenceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public VisualAssetContractRatingMetadataEvidenceResult Build(string? projectRootPath = null)
    {
        var contract = VisualAssetContractRatingMetadataFixtures.BuildDefaultContract();
        var catalog = new VisualAssetContractCatalog
        {
            Accepted = false,
            FixtureCount = contract.Slots.Count,
            FixtureIds = contract.Slots.Select(item => item.AssetSlot).Order(StringComparer.Ordinal).ToList(),
            Contract = contract
        };
        var ratingPolicyMatrix = VisualAssetContractRatingMetadataFixtures.BuildRatingPolicyMatrix();
        var validationMatrix = BuildValidationMatrix(contract);
        var negativeProof = BuildNegativeProof(contract);
        var sourceLineage = BuildSourceDocumentLineage(projectRootPath);
        var qualityGate = BuildQualityGateScan(sourceLineage, validationMatrix, negativeProof);

        var catalogJson = Serialize(catalog);
        var ratingPolicyJson = Serialize(ratingPolicyMatrix);
        var validationMatrixJson = Serialize(validationMatrix);
        var negativeProofJson = Serialize(negativeProof);
        var sourceLineageJson = Serialize(sourceLineage);
        var qualityGateJson = Serialize(qualityGate);
        var reportWithoutHash = new VisualAssetContractRatingMetadataReport
        {
            Accepted = false,
            ContractModelsImplemented = true,
            ValidatorImplemented = true,
            FixturesImplemented = true,
            ValidFixturesPassed = validationMatrix.Passed,
            NegativeProofPassed = negativeProof.Passed,
            Goal083LineagePassed = sourceLineage.Passed,
            FixtureCount = catalog.FixtureCount,
            NegativeScenarioCount = negativeProof.ScenarioCount,
            CatalogHash = ComputeHash(catalogJson),
            RatingPolicyHash = ComputeHash(ratingPolicyJson),
            ValidationMatrixHash = ComputeHash(validationMatrixJson),
            NegativeProofHash = ComputeHash(negativeProofJson),
            SourceLineageHash = ComputeHash(sourceLineageJson),
            QualityGateHash = ComputeHash(qualityGateJson)
        };
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash, catalog, validationMatrix, negativeProof, sourceLineage, qualityGate, deterministicReportHash: string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = ComputeHash(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, catalog, validationMatrix, negativeProof, sourceLineage, qualityGate, report.DeterministicReportHash);

        return new VisualAssetContractRatingMetadataEvidenceResult
        {
            Catalog = catalog,
            RatingPolicyMatrix = ratingPolicyMatrix,
            ValidationMatrix = validationMatrix,
            NegativeProof = negativeProof,
            SourceDocumentLineage = sourceLineage,
            QualityGateScan = qualityGate,
            Report = report,
            CatalogJson = catalogJson,
            RatingPolicyMatrixJson = ratingPolicyJson,
            ValidationMatrixJson = validationMatrixJson,
            NegativeProofJson = negativeProofJson,
            SourceDocumentLineageJson = sourceLineageJson,
            QualityGateScanJson = qualityGateJson,
            ReportMarkdown = reportMarkdown
        };
    }

    public async Task<VisualAssetContractRatingMetadataWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualAssetContractRatingMetadataWriteResult> WriteAsync(
        string projectRootPath,
        VisualAssetContractRatingMetadataEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, VisualAssetContractRatingMetadataVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new VisualAssetContractRatingMetadataWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            CatalogJsonPath = Path.Combine(outputDirectory, CatalogJsonFileName),
            RatingPolicyMatrixJsonPath = Path.Combine(outputDirectory, RatingPolicyMatrixJsonFileName),
            ValidationMatrixJsonPath = Path.Combine(outputDirectory, ValidationMatrixJsonFileName),
            NegativeProofJsonPath = Path.Combine(outputDirectory, NegativeProofJsonFileName),
            SourceDocumentLineageJsonPath = Path.Combine(outputDirectory, SourceDocumentLineageJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName)
        };

        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CatalogJsonPath, result.CatalogJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.RatingPolicyMatrixJsonPath, result.RatingPolicyMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ValidationMatrixJsonPath, result.ValidationMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.NegativeProofJsonPath, result.NegativeProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.SourceDocumentLineageJsonPath, result.SourceDocumentLineageJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.QualityGateScanJsonPath, result.QualityGateScanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        return write;
    }

    public static VisualContractValidationMatrix BuildValidationMatrix(VisualAssetContract contract)
    {
        var rows = contract.Slots
            .OrderBy(slot => slot.AssetSlot, StringComparer.Ordinal)
            .Select(slot =>
            {
                var singleSlotContract = contract with
                {
                    Slots = [slot],
                    CandidateRecords = contract.CandidateRecords.Where(candidate => candidate.AssetSlot == slot.AssetSlot).ToList()
                };
                var validation = VisualAssetContractRatingMetadataValidator.Validate(singleSlotContract);
                return new VisualContractValidationRow
                {
                    FixtureId = slot.AssetSlot,
                    AdultEnabled = slot.AdultEnabled,
                    Rating = slot.Rating,
                    ExportPolicy = slot.ExportPolicy,
                    SafeFallbackRequired = slot.SafeFallbackRequired,
                    HasApprovedAssetRef = slot.ApprovedAssetRef != null,
                    HasDeterministicSafeFallback = VisualAssetContractRatingMetadataValidator.HasDeterministicSafeFallback(slot),
                    Passed = validation.Passed,
                    Diagnostics = validation.Diagnostics
                };
            })
            .ToList();

        return new VisualContractValidationMatrix
        {
            Passed = rows.Count == VisualAssetContractRatingMetadataFixtures.RequiredFixtureIds.Count
                && VisualAssetContractRatingMetadataFixtures.RequiredFixtureIds.All(id => rows.Any(row => row.FixtureId == id && row.Passed))
                && rows.All(row => row.Passed),
            FixtureCount = rows.Count,
            Rows = rows
        };
    }

    public static VisualContractNegativeProof BuildNegativeProof(VisualAssetContract baseline)
    {
        var firstSafe = baseline.Slots.First(item => item.AssetSlot == "fantasy_overworld_tile_safe");
        var adultSlot = baseline.Slots.First(item => item.AssetSlot == "humanoid_paperdoll_adult_capable_metadata_only");
        var scenarios = new List<VisualContractNegativeScenario>
        {
            Invalid("empty_invalid_ids", "blank contract and slot ids", baseline with { ContractId = "", Slots = [firstSafe with { AssetSlot = "" }] }),
            Invalid("absolute_path_rejected", "absolute approved asset path", ReplaceSlot(baseline, firstSafe with { ApprovedAssetRef = firstSafe.ApprovedAssetRef! with { RelativePath = Path.DirectorySeparatorChar + Path.Combine("unsafe", "asset.png") } })),
            Invalid("prompt_text_as_source_of_truth", "prompt source of truth claim", baseline with { CandidateRecords = [baseline.CandidateRecords[0] with { PromptTextIsSourceOfTruth = true, SourceOfTruthKind = "provider_prompt_text" }] }),
            Invalid("public_export_without_safe_ref_or_fallback", "public export without approved safe ref or fallback", ReplaceSlot(baseline, firstSafe with { ApprovedAssetRef = null, SafeFallbackRef = null, SafeFallbackRequired = false })),
            Invalid("adult_enabled_missing_rating_policy", "adult-enabled slot missing explicit rating/export policy", ReplaceSlot(baseline, adultSlot with { Rating = VisualRating.Unspecified, ExportPolicy = VisualExportPolicy.Unspecified })),
            Invalid("adult_public_export_without_fallback", "adult-enabled public export without fallback", ReplaceSlot(baseline, adultSlot with { ExportPolicy = VisualExportPolicy.PublicSafe, SafeFallbackRef = null })),
            Invalid("provider_candidate_treated_as_approved", "quarantined provider candidate marked approved", baseline with { CandidateRecords = [baseline.CandidateRecords[0] with { ProviderState = VisualProviderState.CandidateQuarantine, ReviewStatus = VisualReviewStatus.ApprovedAdult }] }),
            Invalid("unreviewed_rejected_promotion", "promotion requested before approval", baseline with { CandidateRecords = [baseline.CandidateRecords[0] with { PromotionRequested = true, ReviewStatus = VisualReviewStatus.Rejected }] }),
            Invalid("approved_ref_missing_hash_path_provenance", "approved ref missing path/hash/provenance", ReplaceSlot(baseline, firstSafe with { ApprovedAssetRef = firstSafe.ApprovedAssetRef! with { RelativePath = "", Sha256 = "", ProvenanceRef = "" } })),
            Invalid("missing_fallback_when_required", "required fallback missing", ReplaceSlot(baseline, adultSlot with { SafeFallbackRef = null })),
            Invalid("rating_export_contradiction", "adult rating exported through public-safe policy", ReplaceSlot(baseline, adultSlot with { Rating = VisualRating.AdultNudeReference, ExportPolicy = VisualExportPolicy.PublicSafe })),
            Invalid("age_ambiguous_adult_metadata", "age-ambiguous adult eligibility", ReplaceSlot(baseline, adultSlot with { BodyPlanEligibility = VisualBodyPlanEligibility.AgeAmbiguous, BodyPlanEligibilityFacts = adultSlot.BodyPlanEligibilityFacts with { AgeKnownAdult = false, AgeAmbiguous = true } })),
            Invalid("non_sapient_adult_metadata", "non-sapient adult eligibility", ReplaceSlot(baseline, adultSlot with { BodyPlanEligibility = VisualBodyPlanEligibility.NonSapient, BodyPlanEligibilityFacts = adultSlot.BodyPlanEligibilityFacts with { Sapient = false, NonSapient = true } })),
            Invalid("non_eligible_body_plan_adult_metadata", "non-humanoid safe-only adult eligibility", ReplaceSlot(baseline, adultSlot with { BodyPlanEligibility = VisualBodyPlanEligibility.NonHumanoidSafeOnly, BodyPlanEligibilityFacts = adultSlot.BodyPlanEligibilityFacts with { HumanoidCompatible = false, FeralOrNonHumanoidSafeOnly = true } })),
            Invalid("duplicate_slot_ids", "duplicate asset slot ids", baseline with { Slots = [.. baseline.Slots, firstSafe] }),
            Invalid("strict_unknown_recipe_ref", "unknown recipe ref in strict mode", ReplaceSlot(baseline, firstSafe with { RecipeRef = firstSafe.RecipeRef! with { RecipeId = "recipe/unknown_visual_ref/v1" } })),
            Invalid("strict_unknown_part_pack_ref", "unknown part-pack ref in strict mode", ReplaceSlot(baseline, firstSafe with { PartPackRef = firstSafe.PartPackRef! with { PartPackId = "part_pack/unknown_visual_ref/v1" } }))
        };

        return new VisualContractNegativeProof
        {
            Passed = scenarios.Count >= 16 && scenarios.All(item => item.ExpectedValid == item.ActualValid && !item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedValid == item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static VisualAssetContract ReplaceSlot(VisualAssetContract contract, VisualAssetSlot replacement) =>
        contract with
        {
            Slots = contract.Slots.Select(slot => slot.AssetSlot == replacement.AssetSlot ? replacement : slot).ToList()
        };

    private static VisualContractNegativeScenario Invalid(string id, string mutation, VisualAssetContract contract)
    {
        var result = VisualAssetContractRatingMetadataValidator.Validate(contract);
        return new VisualContractNegativeScenario
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ExpectedValid = false,
            ActualValid = result.Passed,
            Diagnostics = result.Diagnostics
        };
    }

    private static VisualSourceDocumentLineage BuildSourceDocumentLineage(string? projectRootPath)
    {
        var records = SourceLineageInputs()
            .Select(item => BuildLineageRecord(projectRootPath, item.Path, item.Tags))
            .ToList();

        var reportText = ReadText(projectRootPath, ".llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-layer-context-integration-report.md");
        var qualityText = ReadText(projectRootPath, ".llmgc/procedural/goal-083-visual-adult-layer-context-integration/quality-gate-scan.json");
        var stateText = ReadText(projectRootPath, "docs/CURRENT_GENERATOR_STATE.md") + Environment.NewLine + ReadText(projectRootPath, "docs/CURRENT_GENERATOR_STATE.json");
        var queueText = ReadText(projectRootPath, "docs/FULL_GENERATOR_GOAL_QUEUE.md");

        var goal083Green = reportText.Contains("implementationStatus: GREEN", StringComparison.Ordinal)
            && qualityText.Contains("\"implementationStatus\": \"GREEN\"", StringComparison.Ordinal);
        var acceptedFalse = reportText.Contains("accepted: false", StringComparison.Ordinal)
            && qualityText.Contains("\"accepted\": false", StringComparison.Ordinal);
        var futureGateRouted = (reportText + queueText + stateText).Contains(VisualAssetContractRatingMetadataVocabulary.FinalGate, StringComparison.Ordinal);
        var sourceFormatInactive = (queueText + stateText).Contains("source_format_physical_line_repair_verification required", StringComparison.Ordinal)
            && (queueText + stateText).Contains("Goal 082A", StringComparison.Ordinal)
            && qualityText.Contains("\"sourceFormatEvidenceNotP0\": true", StringComparison.Ordinal);

        return new VisualSourceDocumentLineage
        {
            Passed = records.All(item => item.Exists)
                && goal083Green
                && acceptedFalse
                && futureGateRouted
                && sourceFormatInactive,
            Goal083ArtifactsGreen = goal083Green,
            Goal083AcceptedFalse = acceptedFalse,
            Goal083FutureGateRouted = futureGateRouted,
            Goal082aP0P1SourceFormatEvidenceInactive = sourceFormatInactive,
            Records = records
        };
    }

    private static VisualSourceDocumentLineageRecord BuildLineageRecord(
        string? projectRootPath,
        string relativePath,
        IReadOnlyList<string> tags)
    {
        var fullPath = ResolveOptionalPath(projectRootPath, relativePath);
        if (fullPath == null || !File.Exists(fullPath))
        {
            return new VisualSourceDocumentLineageRecord
            {
                Path = relativePath,
                Exists = false,
                PurposeTags = tags
            };
        }

        return new VisualSourceDocumentLineageRecord
        {
            Path = relativePath,
            Exists = true,
            Sha256 = ComputeHash(File.ReadAllText(fullPath, Encoding.UTF8)),
            PurposeTags = tags
        };
    }

    private static IReadOnlyList<(string Path, IReadOnlyList<string> Tags)> SourceLineageInputs() =>
    [
        ("docs/context/VISUAL_ADULT_LAYER_CONTEXT_INDEX.md", ["policy", "rating_boundary", "safe_fallback", "provider_quarantine"]),
        ("docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md", ["stage_1_contract", "future_gate_sequence"]),
        ("docs/context/ADULT_VISUAL_LAYER_DOCUMENTATION_MANIFEST.md", ["manifest", "read_order"]),
        ("docs/context/VISUAL_WORLD_GENERATION_CONTEXT_BRIEF.md", ["visual_world", "runtime_provider_firewall"]),
        ("docs/context/METAMODULE_CARRIER_VISUAL_NSFW_CONTEXT_BRIEF.md", ["adult_capable_visuals", "policy_boundary"]),
        ("docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md", ["part_pack", "safe_fallback"]),
        ("docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md", ["body_plan", "eligibility"]),
        ("docs/proposals/ADULT_VISUAL_LAYER_STRATEGY.md", ["rating", "export_policy", "review"]),
        ("docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md", ["rating_export_policy", "validation_diagnostics"]),
        (".llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-layer-context-integration-report.md", ["goal083_report"]),
        (".llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-doc-inventory.json", ["goal083_inventory"]),
        (".llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-policy-routing-matrix.json", ["goal083_policy_routing"]),
        (".llmgc/procedural/goal-083-visual-adult-layer-context-integration/quality-gate-scan.json", ["goal083_quality_gate"])
    ];

    private static VisualContractQualityGateScan BuildQualityGateScan(
        VisualSourceDocumentLineage sourceLineage,
        VisualContractValidationMatrix validationMatrix,
        VisualContractNegativeProof negativeProof)
    {
        var diagnostics = new List<VisualAssetContractDiagnostic>();
        if (!sourceLineage.Passed)
        {
            diagnostics.Add(VisualAssetContractDiagnostic.Error("visual_contract.lineage.failed", "goal083_lineage", "Goal 083 source lineage must be complete and GREEN/accepted=false."));
        }

        if (!validationMatrix.Passed)
        {
            diagnostics.Add(VisualAssetContractDiagnostic.Error("visual_contract.valid_fixtures.failed", "validation_matrix", "All required metadata-only fixtures must pass validation."));
        }

        if (!negativeProof.Passed)
        {
            diagnostics.Add(VisualAssetContractDiagnostic.Error("visual_contract.negative_proof.failed", "negative_proof", "Invalid adult/export/provider/path/reference cases must be rejected."));
        }

        return new VisualContractQualityGateScan
        {
            Goal083LineagePassed = sourceLineage.Passed,
            ValidFixturesPassed = validationMatrix.Passed,
            NegativeProofPassed = negativeProof.Passed,
            ArtifactScopeReady = true,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/VisualAssetContractRatingMetadata/",
                "tests/LLMGameCreator.Tests/Application/VisualAssetContractRatingMetadata/",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualAssetContractRatingMetadataProductSmokeTests.cs",
                ".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/",
                "docs/agent-tasks/goal-084-visual-asset-contract-rating-metadata/"
            ],
            Diagnostics = VisualAssetContractRatingMetadataValidator.SortDiagnostics(diagnostics)
        };
    }

    private static string RenderReport(
        VisualAssetContractRatingMetadataReport report,
        VisualAssetContractCatalog catalog,
        VisualContractValidationMatrix validationMatrix,
        VisualContractNegativeProof negativeProof,
        VisualSourceDocumentLineage sourceLineage,
        VisualContractQualityGateScan qualityGate,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 084 Visual Asset Contract Rating Metadata Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 084 adds a BCL-only Application-side visual asset contract and rating/export metadata validator. It produces metadata-only fixtures and compact validation evidence; it does not generate media, provider output, prompt dumps, Runtime behavior, Unity behavior or public GamePackage schema changes.",
            string.Empty,
            "## Contract Types",
            string.Empty,
            "- VisualAssetContract",
            "- VisualAssetSlot",
            "- VisualAssetRecipeRef",
            "- VisualPartPackRef",
            "- VisualApprovedAssetRef",
            "- VisualSafeFallbackRef",
            "- VisualCandidateRecord",
            "- VisualRating",
            "- VisualExportPolicy",
            "- VisualReviewStatus",
            "- VisualProviderState",
            "- VisualBodyPlanEligibility",
            "- VisualAssetContractValidationResult",
            string.Empty,
            "## Fixture Coverage",
            string.Empty
        };
        lines.AddRange(catalog.FixtureIds.Select(id => $"- {id}"));
        lines.AddRange(
        [
            string.Empty,
            "## Validation",
            string.Empty,
            $"- validFixturesPassed: {validationMatrix.Passed.ToString().ToLowerInvariant()}",
            $"- negativeProofPassed: {negativeProof.Passed.ToString().ToLowerInvariant()}",
            $"- negativeScenarioCount: {negativeProof.ScenarioCount}",
            $"- rejectedNegativeScenarioCount: {negativeProof.RejectedCount}",
            string.Empty,
            "## Goal083 Lineage",
            string.Empty,
            $"- goal083LineagePassed: {sourceLineage.Passed.ToString().ToLowerInvariant()}",
            $"- goal083ArtifactsGreen: {sourceLineage.Goal083ArtifactsGreen.ToString().ToLowerInvariant()}",
            $"- goal083AcceptedFalse: {sourceLineage.Goal083AcceptedFalse.ToString().ToLowerInvariant()}",
            $"- goal083FutureGateRouted: {sourceLineage.Goal083FutureGateRouted.ToString().ToLowerInvariant()}",
            $"- goal082aP0P1SourceFormatEvidenceInactive: {sourceLineage.Goal082aP0P1SourceFormatEvidenceInactive.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Boundaries",
            string.Empty,
            $"- noPublicGamePackageSchemaChanged: {qualityGate.NoPublicGamePackageSchemaChanged.ToString().ToLowerInvariant()}",
            $"- noRuntimeChanged: {qualityGate.NoRuntimeChanged.ToString().ToLowerInvariant()}",
            $"- noUnityChanged: {qualityGate.NoUnityChanged.ToString().ToLowerInvariant()}",
            $"- noProviderOrLlmOrRagOrMediaExecution: {qualityGate.NoProviderOrLlmOrRagOrMediaExecution.ToString().ToLowerInvariant()}",
            $"- noLuaOrGeneratorLibraryChanged: {qualityGate.NoLuaOrGeneratorLibraryChanged.ToString().ToLowerInvariant()}",
            $"- noProjectFilesChanged: {qualityGate.NoProjectFilesChanged.ToString().ToLowerInvariant()}",
            $"- noBinaryMediaAdded: {qualityGate.NoBinaryMediaAdded.ToString().ToLowerInvariant()}",
            $"- noGeneratedImageAssetsAdded: {qualityGate.NoGeneratedImageAssetsAdded.ToString().ToLowerInvariant()}",
            $"- noRealAdultFixturesAdded: {qualityGate.NoRealAdultFixturesAdded.ToString().ToLowerInvariant()}",
            $"- noExplicitPromptDumpAdded: {qualityGate.NoExplicitPromptDumpAdded.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- catalogHash: {report.CatalogHash}",
            $"- ratingPolicyHash: {report.RatingPolicyHash}",
            $"- validationMatrixHash: {report.ValidationMatrixHash}",
            $"- negativeProofHash: {report.NegativeProofHash}",
            $"- sourceLineageHash: {report.SourceLineageHash}",
            $"- qualityGateHash: {report.QualityGateHash}"
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string ComputeHash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static string ReadText(string? projectRootPath, string relativePath)
    {
        var fullPath = ResolveOptionalPath(projectRootPath, relativePath);
        return fullPath != null && File.Exists(fullPath)
            ? File.ReadAllText(fullPath, Encoding.UTF8)
            : string.Empty;
    }

    private static string? ResolveOptionalPath(string? projectRootPath, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            return null;
        }

        return Path.GetFullPath(Path.Combine(Path.GetFullPath(projectRootPath), relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}
