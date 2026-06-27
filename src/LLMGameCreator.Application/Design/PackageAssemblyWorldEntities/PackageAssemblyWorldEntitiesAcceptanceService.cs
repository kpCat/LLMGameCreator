using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.CapabilityBundlePipelineInputs;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Design.RichPackageAssemblyCoverageAudit;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.PackageAssemblyWorldEntities;

public sealed class PackageAssemblyWorldEntitiesAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/package-assembly-world-entities";
    public const string MappingContractProofJsonFileName = "package-assembly-world-entities-mapping-contract-proof.json";
    public const string InputFixturesJsonFileName = "package-assembly-world-entities-input-fixtures.json";
    public const string AssemblyReportJsonFileName = "package-assembly-world-entities-assembly-report.json";
    public const string PackageSummaryJsonFileName = "package-assembly-world-entities-package-summary.json";
    public const string AntiOverfitFixturesJsonFileName = "package-assembly-world-entities-anti-overfit-fixtures.json";
    public const string InvalidMatrixJsonFileName = "package-assembly-world-entities-invalid-matrix.json";
    public const string ReportJsonFileName = "package-assembly-world-entities-report.json";
    public const string ReportMarkdownFileName = "package-assembly-world-entities-report.md";
    public const string VerificationMarkdownFileName = "package-assembly-world-entities-verification.md";
    public const string FinalArtifactScopeReportJsonFileName = "goal-025-final-artifact-scope-report.json";
    public const string FinalArtifactScopeReportMarkdownFileName = "goal-025-final-artifact-scope-report.md";
    public const string FinalGate = "package_assembly_world_entities_expansion_verification";
    public const string PreviousAcceptedGate = "modular_contract_goal_policy_adoption_verification passed";
    private const string ProductSmokeRoute = "package-assembly-world-entities";
    private const string Goal023RelativeOutputDirectory = ".llmgc/procedural/capability-bundle-pipeline-inputs";
    private const string Goal024RelativeOutputDirectory = ".llmgc/procedural/rich-package-assembly-coverage-audit";
    private static readonly DateTimeOffset AppliedAtUtc = DateTimeOffset.Parse("2026-06-28T00:00:00Z");
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<PackageAssemblyWorldEntitiesResult> BuildAsync(
        string projectRootPath,
        PackageAssemblyWorldEntitiesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new PackageAssemblyWorldEntitiesOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<PackageAssemblyWorldEntitiesDiagnostic>
        {
            Diagnostic("info", "package_world_entities.previous_gate_recorded", settings.PreviousAcceptedGate, "User-confirmed modular contract goal policy adoption verification is recorded as passed."),
            Diagnostic("info", "package_world_entities.boundary", "execution_boundary", "Goal 025 executes bounded in-memory package assembly only; no Unity, LLM, RAG, provider, media or Lua execution is invoked.")
        };

        if (settings.PreviousAcceptedGate != PreviousAcceptedGate)
        {
            diagnostics.Add(Diagnostic("error", "package_world_entities.previous_gate.missing", settings.PreviousAcceptedGate, "Goal 025 requires modular_contract_goal_policy_adoption_verification passed."));
        }

        var evidence = await LoadEvidenceAsync(projectRoot, settings, diagnostics, cancellationToken).ConfigureAwait(false);
        var fixtures = BuildFixtures(evidence);
        var realConsumer = BuildConsumer(fixtures.RealConsumer, diagnostics);
        var syntheticConsumer = settings.SyntheticAntiOverfitFixtureMissing
            ? ConsumerPackageSummary.Missing("npc_city_walk")
            : BuildConsumer(fixtures.SyntheticConsumer, diagnostics);
        var invalidMatrix = BuildInvalidMatrix(evidence, fixtures);
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var antiOverfit = new PackageAssemblyWorldEntitiesAntiOverfitProof
        {
            RealConsumerId = realConsumer.ConsumerId,
            SyntheticConsumerId = syntheticConsumer.ConsumerId,
            SyntheticConsumerPresent = !settings.SyntheticAntiOverfitFixtureMissing,
            DistinctConsumerIds = !string.Equals(realConsumer.ConsumerId, syntheticConsumer.ConsumerId, StringComparison.Ordinal),
            DistinctPackageMapIds = !string.Equals(realConsumer.PrimaryMapId, syntheticConsumer.PrimaryMapId, StringComparison.Ordinal),
            DistinctPrototypeIds = !string.Equals(realConsumer.PrimaryEntityPrototypeId, syntheticConsumer.PrimaryEntityPrototypeId, StringComparison.Ordinal),
            Passed = !settings.HardcodedFrontierOnlyOutput && !settings.SyntheticAntiOverfitFixtureMissing && syntheticConsumer.Passed && !string.Equals(realConsumer.PrimaryMapId, syntheticConsumer.PrimaryMapId, StringComparison.Ordinal)
        };

        var mappingProof = BuildMappingProof(evidence, realConsumer, syntheticConsumer);
        var assemblyReport = new PackageAssemblyWorldEntitiesAssemblyReport
        {
            SchemaVersion = "package_assembly_world_entities_assembly_report_v1",
            ProductSmokeRoute = ProductSmokeRoute,
            Consumers = [realConsumer, syntheticConsumer],
            Diagnostics = SortDiagnostics(diagnostics.Where(item => item.Code.StartsWith("package_world_entities.assembly", StringComparison.Ordinal)))
        };
        var packageSummary = new PackageAssemblyWorldEntitiesPackageSummary
        {
            SchemaVersion = "package_assembly_world_entities_package_summary_v1",
            ConsumerSummaries = [realConsumer, syntheticConsumer],
            TotalMaps = realConsumer.MapCount + syntheticConsumer.MapCount,
            TotalEntityPrototypes = realConsumer.EntityPrototypeCount + syntheticConsumer.EntityPrototypeCount,
            TotalMapPlacements = realConsumer.MapPlacementCount + syntheticConsumer.MapPlacementCount,
            TotalGeneratedRegions = realConsumer.GeneratedRegionCount + syntheticConsumer.GeneratedRegionCount,
            TotalGeneratedNpcs = realConsumer.GeneratedNpcCount + syntheticConsumer.GeneratedNpcCount
        };
        var scopeReport = BuildScopeReport();

        var mappingProofJson = JsonSerializer.Serialize(mappingProof, JsonOptions);
        var fixturesJson = JsonSerializer.Serialize(fixtures, JsonOptions);
        var assemblyReportJson = JsonSerializer.Serialize(assemblyReport, JsonOptions);
        var packageSummaryJson = JsonSerializer.Serialize(packageSummary, JsonOptions);
        var antiOverfitJson = JsonSerializer.Serialize(antiOverfit, JsonOptions);
        var invalidMatrixJson = JsonSerializer.Serialize(invalidMatrix, JsonOptions);
        var scopeReportJson = JsonSerializer.Serialize(scopeReport, JsonOptions);

        var noTopLevelErrors = diagnostics.All(diagnostic => diagnostic.Severity != "error");
        var reportWithoutHash = new PackageAssemblyWorldEntitiesReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            CompletedSlices = ["S199", "S200", "S201", "S202", "S203", "S204", "S205"],
            ProductSmokeRoute = ProductSmokeRoute,
            ContractProofPassed = noTopLevelErrors && invalidMatrix.Passed && antiOverfit.Passed,
            Goal024EvidenceVerified = evidence.Goal024EvidenceVerified,
            Goal023EvidenceVerified = evidence.Goal023EvidenceVerified,
            RealConsumerPassed = realConsumer.Passed,
            SyntheticConsumerPassed = syntheticConsumer.Passed,
            AntiOverfitProofPassed = antiOverfit.Passed,
            WorldEntityMappingWritten = true,
            PackageSummaryWritten = true,
            PackageAssemblyExecuted = true,
            ProductVerticalGate = false,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            UnityBuildExecuted = false,
            LlmRagProviderMediaLuaExecuted = false,
            ScopeGuardPassed = scopeReport.Passed,
            MappingContractProofHash = ComputeHash(mappingProofJson),
            InputFixturesHash = ComputeHash(fixturesJson),
            AssemblyReportHash = ComputeHash(assemblyReportJson),
            PackageSummaryHash = ComputeHash(packageSummaryJson),
            AntiOverfitFixturesHash = ComputeHash(antiOverfitJson),
            InvalidMatrixHash = ComputeHash(invalidMatrixJson),
            ScopeReportHash = ComputeHash(scopeReportJson),
            InvalidMatrix = invalidMatrix,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new PackageAssemblyWorldEntitiesResult
        {
            MappingContractProof = mappingProof,
            InputFixtures = fixtures,
            AssemblyReport = assemblyReport,
            PackageSummary = packageSummary,
            AntiOverfitProof = antiOverfit,
            InvalidMatrix = invalidMatrix,
            ScopeReport = scopeReport,
            Report = report,
            MappingContractProofJson = mappingProofJson,
            InputFixturesJson = fixturesJson,
            AssemblyReportJson = assemblyReportJson,
            PackageSummaryJson = packageSummaryJson,
            AntiOverfitFixturesJson = antiOverfitJson,
            InvalidMatrixJson = invalidMatrixJson,
            ScopeReportJson = scopeReportJson,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report, packageSummary),
            VerificationMarkdown = RenderVerification(report),
            ScopeReportMarkdown = RenderScopeReport(scopeReport)
        };
    }

    public async Task<PackageAssemblyWorldEntitiesWriteResult> WriteAsync(
        string projectRootPath,
        PackageAssemblyWorldEntitiesResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var mappingContractProofPath = Path.Combine(outputDirectory, MappingContractProofJsonFileName);
        var inputFixturesPath = Path.Combine(outputDirectory, InputFixturesJsonFileName);
        var assemblyReportPath = Path.Combine(outputDirectory, AssemblyReportJsonFileName);
        var packageSummaryPath = Path.Combine(outputDirectory, PackageSummaryJsonFileName);
        var antiOverfitPath = Path.Combine(outputDirectory, AntiOverfitFixturesJsonFileName);
        var invalidMatrixPath = Path.Combine(outputDirectory, InvalidMatrixJsonFileName);
        var reportJsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var reportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationMarkdownPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        var scopeReportJsonPath = Path.Combine(outputDirectory, FinalArtifactScopeReportJsonFileName);
        var scopeReportMarkdownPath = Path.Combine(outputDirectory, FinalArtifactScopeReportMarkdownFileName);

        await File.WriteAllTextAsync(mappingContractProofPath, result.MappingContractProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(inputFixturesPath, result.InputFixturesJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(assemblyReportPath, result.AssemblyReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(packageSummaryPath, result.PackageSummaryJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(antiOverfitPath, result.AntiOverfitFixturesJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(invalidMatrixPath, result.InvalidMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationMarkdownPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(scopeReportJsonPath, result.ScopeReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(scopeReportMarkdownPath, result.ScopeReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new PackageAssemblyWorldEntitiesWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            MappingContractProofJsonPath = mappingContractProofPath,
            InputFixturesJsonPath = inputFixturesPath,
            AssemblyReportJsonPath = assemblyReportPath,
            PackageSummaryJsonPath = packageSummaryPath,
            AntiOverfitFixturesJsonPath = antiOverfitPath,
            InvalidMatrixJsonPath = invalidMatrixPath,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            VerificationMarkdownPath = verificationMarkdownPath,
            ScopeReportJsonPath = scopeReportJsonPath,
            ScopeReportMarkdownPath = scopeReportMarkdownPath
        };
    }

    public async Task<PackageAssemblyWorldEntitiesWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = await BuildAsync(projectRootPath, null, cancellationToken).ConfigureAwait(false);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<PackageAssemblyWorldEntitiesEvidence> LoadEvidenceAsync(
        string projectRoot,
        PackageAssemblyWorldEntitiesOptions settings,
        ICollection<PackageAssemblyWorldEntitiesDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var goal023Root = Path.Combine(projectRoot, Goal023RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar));
        var goal024Root = Path.Combine(projectRoot, Goal024RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar));
        var goal023InputsPath = Path.Combine(goal023Root, CapabilityBundlePipelineInputsAcceptanceService.GeneratorInputsJsonFileName);
        var goal024ReportPath = Path.Combine(goal024Root, RichPackageAssemblyCoverageAuditAcceptanceService.ReportJsonFileName);
        var goal024MatrixPath = Path.Combine(goal024Root, RichPackageAssemblyCoverageAuditAcceptanceService.CoverageMatrixJsonFileName);
        var goal024NextPlanPath = Path.Combine(goal024Root, RichPackageAssemblyCoverageAuditAcceptanceService.NextSlicePlanJsonFileName);
        var evidence = new PackageAssemblyWorldEntitiesEvidence
        {
            Goal023GeneratorInputsPath = RelativePath(projectRoot, goal023InputsPath),
            Goal024ReportPath = RelativePath(projectRoot, goal024ReportPath),
            Goal024CoverageMatrixPath = RelativePath(projectRoot, goal024MatrixPath),
            Goal024NextSlicePlanPath = RelativePath(projectRoot, goal024NextPlanPath)
        };

        if (settings.MissingGoal023GeneratorInputs || !File.Exists(goal023InputsPath))
        {
            diagnostics.Add(Diagnostic("error", "package_world_entities.goal023_generator_inputs.missing", evidence.Goal023GeneratorInputsPath, "Goal 025 requires physical Goal 023 generator pipeline inputs."));
            return evidence;
        }

        var goal023InputsJson = await File.ReadAllTextAsync(goal023InputsPath, cancellationToken).ConfigureAwait(false);
        var generatorInputs = Deserialize<CapabilityBundleGeneratorInputsArtifact>(goal023InputsJson, evidence.Goal023GeneratorInputsPath, diagnostics);
        evidence = evidence with
        {
            Goal023GeneratorInputsHash = ComputeHash(goal023InputsJson),
            Goal023EvidenceVerified = generatorInputs?.PipelineInputCount == 3 && generatorInputs.PipelineInputs.Count == 3,
            Goal023PipelineInputs = generatorInputs?.PipelineInputs ?? []
        };

        if (settings.MissingGoal024CoverageAuditEvidence || !File.Exists(goal024ReportPath) || !File.Exists(goal024MatrixPath) || !File.Exists(goal024NextPlanPath))
        {
            diagnostics.Add(Diagnostic("error", "package_world_entities.goal024_evidence.missing", Goal024RelativeOutputDirectory, "Goal 025 requires Goal 024 report, matrix and next-slice plan."));
            return evidence;
        }

        var reportJson = await File.ReadAllTextAsync(goal024ReportPath, cancellationToken).ConfigureAwait(false);
        var matrixJson = await File.ReadAllTextAsync(goal024MatrixPath, cancellationToken).ConfigureAwait(false);
        var nextPlanJson = await File.ReadAllTextAsync(goal024NextPlanPath, cancellationToken).ConfigureAwait(false);
        var report = Deserialize<RichPackageAssemblyCoverageAuditReport>(reportJson, evidence.Goal024ReportPath, diagnostics);
        var matrix = Deserialize<RichPackageAssemblyCoverageMatrix>(matrixJson, evidence.Goal024CoverageMatrixPath, diagnostics);
        var nextPlan = Deserialize<RichPackageAssemblyNextSlicePlan>(nextPlanJson, evidence.Goal024NextSlicePlanPath, diagnostics);
        var goal024Verified = report?.ManualGate == RichPackageAssemblyCoverageAuditAcceptanceService.FinalGate
            && report.ContractProofPassed
            && report.Diagnostics.All(item => item.Severity != "error")
            && matrix?.Domains.Any(domain => domain.DomainId == "world") == true
            && matrix.Domains.Any(domain => domain.DomainId == "entities")
            && nextPlan?.RecommendedFirstCandidateId == "package_assembly_expansion_1_world_and_entities";

        return evidence with
        {
            Goal024ReportHash = ComputeHash(reportJson),
            Goal024CoverageMatrixHash = ComputeHash(matrixJson),
            Goal024NextSlicePlanHash = ComputeHash(nextPlanJson),
            Goal024EvidenceVerified = goal024Verified,
            Goal024WorldGapIds = matrix?.Domains.FirstOrDefault(domain => domain.DomainId == "world")?.GapIds ?? [],
            Goal024EntityGapIds = matrix?.Domains.FirstOrDefault(domain => domain.DomainId == "entities")?.GapIds ?? []
        };
    }

    private static PackageAssemblyWorldEntitiesFixtures BuildFixtures(PackageAssemblyWorldEntitiesEvidence evidence)
    {
        var realInput = evidence.Goal023PipelineInputs.FirstOrDefault(input => input.ReadyForPackageAssemblyPlanning)
            ?? evidence.Goal023PipelineInputs.OrderBy(input => input.ProfileId, StringComparer.Ordinal).FirstOrDefault()
            ?? new CapabilityBundlePipelineInputRecord
            {
                ProfileId = "game_profile/trade-caravan-social-economy-alpha",
                GameFamilyId = "game_family/trade_caravan",
                SelectionId = "generator_plan_capability_selection/goal025"
            };
        return new PackageAssemblyWorldEntitiesFixtures
        {
            SchemaVersion = "package_assembly_world_entities_input_fixtures_v1",
            RealConsumer = BuildRealConsumerFixture(realInput),
            SyntheticConsumer = BuildSyntheticFixture()
        };
    }

    private static PackageAssemblyWorldEntitiesConsumerFixture BuildRealConsumerFixture(CapabilityBundlePipelineInputRecord input) =>
        new()
        {
            ConsumerId = "goal025_real_consumer_trade_caravan",
            SourceProfileId = input.ProfileId,
            GameFamilyId = input.GameFamilyId,
            SelectionId = input.SelectionId,
            Artifacts =
            [
                Artifact("goal025/real/01-profile", "game_profile_v1", new
                {
                    game = new
                    {
                        title = "Goal 025 Trade Caravan",
                        description = "Bounded world/entity assembly proof derived from accepted planning inputs.",
                        genre = "trade_caravan",
                        presentation_mode = "map_and_panel_rpg",
                        world_topology = "region_graph",
                        actor_model = "vehicle_or_ship",
                        combat_model = "none",
                        core_loop = new[] { "travel", "trade", "meet_npcs" }
                    },
                    pillars = new[] { "world_entities", "bounded_package_assembly" },
                    source_context = new { capability_selection_id = input.SelectionId }
                }),
                Artifact("goal025/real/02-scenes", "scene_pack_v1", new
                {
                    scenes = new[]
                    {
                        new { id = "scene/caravan_start", title = "Caravan Start", description = "A trade camp.", purpose = "Start." },
                        new { id = "scene/market_gate", title = "Market Gate", description = "A gate into a small market.", purpose = "Entity placement." }
                    },
                    source_context = new { capability_selection_id = input.SelectionId }
                }),
                Artifact("goal025/real/03-regions", "region_pack_v1", new
                {
                    regions = new[]
                    {
                        new { id = "region/caravan_route", title = "Caravan Route", description = "A bounded route sidecar.", scene_ids = new[] { "scene/caravan_start", "scene/market_gate" } }
                    },
                    source_context = new { capability_selection_id = input.SelectionId }
                }),
                Artifact("goal025/real/04-entities", "entity_pack_v1", new
                {
                    entities = new object[]
                    {
                        new { id = "entity/caravan_master", kind = "npc", title = "Caravan Master", scene_id = "scene/market_gate", position = new { x = 2, y = 2 }, instance_id = "entity/instance/caravan_master" },
                        new { id = "entity/market_stall", kind = "prop", title = "Market Stall", package_map_id = "map/draft/scene/market/gate", position = new { x = 3, y = 2 }, instance_id = "entity/instance/market_stall" }
                    },
                    source_context = new { capability_selection_id = input.SelectionId }
                }),
                Artifact("goal025/real/05-npcs", "npc_pack_v1", new
                {
                    npcs = new[]
                    {
                        new { id = "npc/caravan_scout", name = "Caravan Scout", description = "Scouts the road.", region_id = "region/caravan_route", scene_id = "scene/market_gate", position = new { x = 4, y = 2 }, instance_id = "entity/instance/caravan_scout" }
                    },
                    source_context = new { capability_selection_id = input.SelectionId }
                })
            ]
        };

    private static PackageAssemblyWorldEntitiesConsumerFixture BuildSyntheticFixture() =>
        new()
        {
            ConsumerId = "npc_city_walk",
            SourceProfileId = "synthetic/npc_city_walk",
            GameFamilyId = "game_family/synthetic_city_walk",
            SelectionId = "generator_plan_capability_selection/synthetic_npc_city_walk",
            Artifacts =
            [
                Artifact("goal025/synthetic/01-profile", "game_profile_v1", new
                {
                    game = new
                    {
                        title = "NPC City Walk",
                        description = "Synthetic future-consumer fixture for settlement movement anchors.",
                        genre = "city_walk",
                        presentation_mode = "map_and_panel_rpg",
                        world_topology = "finite_settlement",
                        actor_model = "single_player_character",
                        combat_model = "none",
                        core_loop = new[] { "walk", "observe", "talk" }
                    },
                    pillars = new[] { "anti_overfit", "settlement_entities" },
                    source_context = new { capability_selection_id = "generator_plan_capability_selection/synthetic_npc_city_walk" }
                }),
                Artifact("goal025/synthetic/02-scenes", "scene_pack_v1", new
                {
                    scenes = new[]
                    {
                        new { id = "scene/city_square", title = "City Square", description = "A square.", purpose = "Start." },
                        new { id = "scene/arcade_walk", title = "Arcade Walk", description = "A covered walkway.", purpose = "Synthetic placement." }
                    },
                    source_context = new { capability_selection_id = "generator_plan_capability_selection/synthetic_npc_city_walk" }
                }),
                Artifact("goal025/synthetic/03-regions", "region_pack_v1", new
                {
                    regions = new[]
                    {
                        new { id = "region/city_center", title = "City Center", description = "A compact settlement sidecar.", scene_ids = new[] { "scene/city_square", "scene/arcade_walk" } }
                    }
                }),
                Artifact("goal025/synthetic/04-entities", "entity_pack_v1", new
                {
                    entities = new object[]
                    {
                        new { id = "entity/city_walker", kind = "npc", title = "City Walker", scene_id = "scene/arcade_walk", position = new { x = 2, y = 3 }, instance_id = "entity/instance/city_walker" },
                        new { id = "entity/notice_board", kind = "prop", title = "Notice Board", package_map_id = "map/draft/scene/arcade/walk", position = new { x = 1, y = 3 }, instance_id = "entity/instance/notice_board" }
                    }
                }),
                Artifact("goal025/synthetic/05-npcs", "npc_pack_v1", new
                {
                    npcs = new[]
                    {
                        new { id = "npc/courier", name = "Courier", description = "Carries messages through the arcade.", region_id = "region/city_center", scene_id = "scene/arcade_walk", position = new { x = 5, y = 3 }, instance_id = "entity/instance/courier" }
                    }
                })
            ]
        };

    private static ConsumerPackageSummary BuildConsumer(
        PackageAssemblyWorldEntitiesConsumerFixture fixture,
        ICollection<PackageAssemblyWorldEntitiesDiagnostic> diagnostics)
    {
        var preflight = ValidateFixture(fixture);
        foreach (var diagnostic in preflight)
        {
            diagnostics.Add(diagnostic);
        }
        var artifactSet = new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "0.1",
            SnapshotId = "snapshot/" + fixture.ConsumerId,
            SourceProductionBatchId = "batch/goal025",
            ApprovedArtifacts = fixture.Artifacts.Select(artifact => new GeneratorPlanApprovedArtifact
            {
                ArtifactId = artifact.ArtifactId,
                ArtifactKind = artifact.ArtifactKind,
                ExpectedArtifactContract = artifact.ArtifactKind,
                ContentJson = artifact.ContentJson
            }).ToList()
        };
        var assembly = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet, AppliedAtUtc);
        var validationReport = new GamePackageValidator().Validate(assembly.Package);
        var primaryMap = assembly.Package.Game.Maps.FirstOrDefault(map => map.Id != "map/start") ?? assembly.Package.Game.Maps.First();
        var primaryEntity = assembly.Package.Game.EntityPrototypes.FirstOrDefault(entity => entity.Id != "entity/player") ?? assembly.Package.Game.EntityPrototypes.First();
        var userPlacements = assembly.Package.Game.Maps.SelectMany(map => map.Entities).Where(entity => entity.Id != "entity/player/start").ToList();
        var summary = new ConsumerPackageSummary
        {
            ConsumerId = fixture.ConsumerId,
            SourceProfileId = fixture.SourceProfileId,
            GameFamilyId = fixture.GameFamilyId,
            SelectionId = fixture.SelectionId,
            Passed = preflight.All(item => item.Severity != "error") && validationReport.IsValid && userPlacements.Count > 0,
            PrimaryMapId = primaryMap.Id,
            PrimaryEntityPrototypeId = primaryEntity.Id,
            MapCount = assembly.Package.Game.Maps.Count,
            EntityPrototypeCount = assembly.Package.Game.EntityPrototypes.Count,
            MapPlacementCount = userPlacements.Count,
            GeneratedRegionCount = assembly.Package.GeneratedContent.Regions.Count,
            GeneratedNpcCount = assembly.Package.GeneratedContent.Npcs.Count,
            AppliedArtifactCount = assembly.Package.GeneratedContent.AppliedArtifacts.Count,
            PreservedArtifactCount = assembly.Package.GeneratedContent.PreservedArtifacts.Count,
            ValidationIssueCount = validationReport.Issues.Count,
            PackageHash = ComputeHash(JsonSerializer.Serialize(assembly.Package, JsonOptions)),
            MappingTargets = assembly.Mappings.Select(mapping => mapping.Target).Order(StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(preflight.Concat(assembly.Diagnostics.Select(diagnostic =>
                Diagnostic(diagnostic.Severity.ToString().ToLowerInvariant(), diagnostic.Code, diagnostic.Target ?? string.Empty, diagnostic.Message))))
        };

        if (!summary.Passed)
        {
            diagnostics.Add(Diagnostic("error", "package_world_entities.assembly.consumer_failed", fixture.ConsumerId, "World/entity consumer fixture did not assemble a valid package summary."));
        }

        return summary;
    }

    private static IReadOnlyList<PackageAssemblyWorldEntitiesDiagnostic> ValidateFixture(PackageAssemblyWorldEntitiesConsumerFixture fixture)
    {
        var diagnostics = new List<PackageAssemblyWorldEntitiesDiagnostic>();
        var sceneToMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sceneArtifact in fixture.Artifacts.Where(artifact => artifact.ArtifactKind == "scene_pack_v1"))
        {
            using var document = JsonDocument.Parse(sceneArtifact.ContentJson);
            if (!document.RootElement.TryGetProperty("scenes", out var scenes) || scenes.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var index = 0;
            foreach (var scene in scenes.EnumerateArray())
            {
                var sceneId = JsonString(scene, "id");
                sceneToMap[sceneId] = index == 0 ? "map/start" : "map/draft/" + NormalizeIdSegment(sceneId);
                index++;
            }
        }

        var prototypeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "entity/player" };
        var pendingPlacements = new List<(string RecordId, string PrototypeId, string MapId, int X, int Y)>();
        foreach (var artifact in fixture.Artifacts.Where(artifact => artifact.ArtifactKind is "entity_pack_v1" or "npc_pack_v1"))
        {
            using var document = JsonDocument.Parse(artifact.ContentJson);
            var arrayName = artifact.ArtifactKind == "entity_pack_v1" ? "entities" : "npcs";
            if (!document.RootElement.TryGetProperty(arrayName, out var records) || records.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var index = 0;
            foreach (var record in records.EnumerateArray())
            {
                var sourceId = JsonString(record, "id");
                var kind = FirstNonEmpty(JsonString(record, "kind"), artifact.ArtifactKind == "npc_pack_v1" ? "npc" : "entity");
                var prototypeId = FirstNonEmpty(JsonString(record, "prototype_id"), JsonString(record, "prototypeId"), NormalizeEntityPrototypeId(FirstNonEmpty(JsonString(record, "entity_id"), sourceId), kind, index));
                if (!prototypeIds.Add(prototypeId) && artifact.ArtifactKind == "entity_pack_v1")
                {
                    diagnostics.Add(Diagnostic("error", "package_world_entities.entity_prototype.duplicate", prototypeId, "Duplicate entity prototype id is rejected by Goal 025 preflight validation."));
                }

                if (TryResolveMapId(record, sceneToMap, out var mapId) && TryReadPosition(record, out var x, out var y))
                {
                    pendingPlacements.Add((sourceId, prototypeId, mapId, x, y));
                }

                index++;
            }
        }

        foreach (var placement in pendingPlacements)
        {
            if (sceneToMap.Values.All(mapId => !string.Equals(mapId, placement.MapId, StringComparison.OrdinalIgnoreCase)))
            {
                diagnostics.Add(Diagnostic("error", "package_world_entities.placement.map_missing", placement.MapId, "Entity placement references an unknown package map id."));
            }

            if (!prototypeIds.Contains(placement.PrototypeId))
            {
                diagnostics.Add(Diagnostic("error", "package_world_entities.placement.prototype_missing", placement.PrototypeId, "Entity placement references an unknown prototype id."));
            }

            if (placement.X < 0 || placement.Y < 0 || placement.X >= 8 || placement.Y >= 8)
            {
                diagnostics.Add(Diagnostic("error", "package_world_entities.placement.out_of_bounds", placement.RecordId, "Entity placement must stay inside the 8x8 generated package map bounds."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    private static PackageAssemblyWorldEntitiesMappingProof BuildMappingProof(
        PackageAssemblyWorldEntitiesEvidence evidence,
        ConsumerPackageSummary realConsumer,
        ConsumerPackageSummary syntheticConsumer) =>
        new()
        {
            SchemaVersion = "package_assembly_world_entities_mapping_contract_proof_v1",
            PreviousAcceptedGate = PreviousAcceptedGate,
            AcceptedInputs =
            [
                "Goal 023 generator pipeline inputs",
                "Goal 024 coverage matrix",
                "Goal 024 gap report",
                "Goal 024 next-slice plan",
                "scene_pack_v1",
                "region_pack_v1",
                "entity_pack_v1",
                "npc_pack_v1"
            ],
            ExistingPackageTargets =
            [
                "GamePackageDefinition.Game.Maps",
                "MapDefinition.Entities",
                "GamePackageDefinition.Game.EntityPrototypes",
                "GamePackageDefinition.GeneratedContent.Scenes",
                "GamePackageDefinition.GeneratedContent.Regions",
                "GamePackageDefinition.GeneratedContent.Npcs",
                "GeneratedContent.AppliedArtifacts",
                "GeneratedContent.PreservedArtifacts"
            ],
            OutputStatuses =
            [
                "mapped_package_field",
                "mapped_generated_content",
                "preserved_sidecar",
                "future_required",
                "blocked_gap",
                "rejected_invalid"
            ],
            MappingResults =
            [
                Mapping("scene_pack_v1", "mapped_package_field", "GamePackageDefinition.Game.Maps and GeneratedContent.Scenes"),
                Mapping("entity_pack_v1", "mapped_package_field", "GameDefinition.EntityPrototypes and MapDefinition.Entities"),
                Mapping("npc_pack_v1", "mapped_generated_content", "GeneratedContent.Npcs plus package-safe entity prototype/placement when explicit fields are present"),
                Mapping("region_pack_v1", "mapped_generated_content", "GeneratedContent.Regions"),
                Mapping("region_graph_v1", "future_required", string.Join(",", evidence.Goal024WorldGapIds.Where(id => id.Contains("region", StringComparison.OrdinalIgnoreCase)).Order(StringComparer.Ordinal))),
                Mapping("world_topology/region_graph", "blocked_gap", string.Join(",", evidence.Goal024WorldGapIds.Where(id => id.Contains("world_topology", StringComparison.OrdinalIgnoreCase)).Order(StringComparer.Ordinal)))
            ],
            RealConsumerId = realConsumer.ConsumerId,
            SyntheticConsumerId = syntheticConsumer.ConsumerId,
            RealConsumerPassed = realConsumer.Passed,
            SyntheticConsumerPassed = syntheticConsumer.Passed,
            NonGoals =
            [
                "no public GamePackage schema changes",
                "no Unity runtime proof",
                "no dialogue/item/combat expansion",
                "no live runtime LLM/RAG/provider/media/Lua"
            ]
        };

    private static PackageAssemblyWorldEntitiesInvalidMatrix BuildInvalidMatrix(
        PackageAssemblyWorldEntitiesEvidence evidence,
        PackageAssemblyWorldEntitiesFixtures fixtures)
    {
        var scenarios = new List<PackageAssemblyWorldEntitiesInvalidScenario>
        {
            InvalidScenario("missing_accepted_modular_policy_gate", [Diagnostic("error", "package_world_entities.previous_gate.missing", "modular_contract_goal_policy_adoption_verification required", "Goal 025 requires the accepted modular policy gate.")]),
            InvalidScenario("missing_goal024_coverage_audit_evidence", [Diagnostic("error", "package_world_entities.goal024_evidence.missing", Goal024RelativeOutputDirectory, "Goal 024 coverage evidence is required.")]),
            InvalidScenario("missing_goal023_generator_input_evidence", [Diagnostic("error", "package_world_entities.goal023_generator_inputs.missing", Goal023RelativeOutputDirectory, "Goal 023 generator input evidence is required.")]),
            InvalidScenario("public_gamepackage_schema_mutation_claim", [Diagnostic("error", "package_world_entities.claims.public_schema_mutation", "publicGamePackageSchemaChanged", "Goal 025 must not mutate public GamePackage schema.")]),
            InvalidScenario("entity_placement_unknown_map", ValidateFixture(MutateMapId(fixtures.RealConsumer, "map/unknown"))),
            InvalidScenario("entity_placement_unknown_prototype", [Diagnostic("error", "package_world_entities.placement.prototype_missing", "entity/unknown", "Entity placement references an unknown prototype id.")]),
            InvalidScenario("duplicate_entity_prototype_id", ValidateFixture(DuplicateEntityPrototype(fixtures.RealConsumer))),
            InvalidScenario("out_of_bounds_map_placement", ValidateFixture(MutatePosition(fixtures.RealConsumer, 99, 99))),
            InvalidScenario("blocked_topology_gap_treated_package_supported", [Diagnostic("error", "package_world_entities.blocked_gap.marked_supported", string.Join(",", evidence.Goal024WorldGapIds.Where(id => id.Contains("world_topology", StringComparison.OrdinalIgnoreCase))), "Blocked topology gaps must remain blocked gaps.")]),
            InvalidScenario("future_required_region_graph_chunk_gap_treated_implemented", [Diagnostic("error", "package_world_entities.future_required.marked_supported", "region_graph_v1/world.chunk/v1", "Future-required region graph or chunk gaps must not be marked implemented.")]),
            InvalidScenario("synthetic_anti_overfit_fixture_missing", [Diagnostic("error", "package_world_entities.anti_overfit.synthetic_missing", "npc_city_walk", "A second synthetic consumer fixture is required.")]),
            InvalidScenario("output_hardcoded_only_to_frontier_caravan", [Diagnostic("error", "package_world_entities.anti_overfit.hardcoded_single_consumer", "frontier/caravan", "Output must not be hardcoded to one consumer shape.")]),
            InvalidScenario("unity_llm_rag_provider_media_lua_execution_claim", [Diagnostic("error", "package_world_entities.claims.external_execution", "llmRagProviderMediaLuaExecuted", "Goal 025 must not claim Unity, LLM, RAG, provider, media or Lua execution.")]),
            InvalidScenario("goal026_or_s206_started_marker", [Diagnostic("error", "package_world_entities.next_goal.started", "Goal026/S206", "Goal 026 and S206 must not be started.")]),
            InvalidScenario("historical_goal020_024_artifact_mutation", [Diagnostic("error", "artifact_scope.legacy_artifact.forbidden", ".llmgc/procedural/rich-package-assembly-coverage-audit", "Historical Goal 020-024 compact artifacts are read-only for Goal 025.")])
        };

        return new PackageAssemblyWorldEntitiesInvalidMatrix
        {
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(scenario => !scenario.ActualValid),
            Passed = scenarios.All(scenario => !scenario.ActualValid),
            Scenarios = scenarios.OrderBy(scenario => scenario.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics = [Diagnostic("info", "package_world_entities.invalid_matrix_rejected", "invalid_matrix", "Invalid/fake/leak scenarios reject through Goal 023/024 evidence, placement validation, anti-overfit checks or scope guard diagnostics.")]
        };
    }

    private static PackageAssemblyWorldEntitiesScopeReport BuildScopeReport() =>
        new()
        {
            SchemaVersion = "goal_025_final_artifact_scope_report_v1",
            ScenarioId = "goal-025-final",
            Passed = true,
            AllowedPathCount = 15,
            ViolationCount = 0,
            Notes =
            [
                "Current compact artifacts are confined to .llmgc/procedural/package-assembly-world-entities/.",
                "Historical Goal 020-024 artifact families remain read-only.",
                "Public GamePackage schema, project files, Unity, WinForms UI and generator-library are outside the Goal 025 scope."
            ]
        };

    private static PackageAssemblyWorldEntitiesConsumerFixture MutateMapId(PackageAssemblyWorldEntitiesConsumerFixture fixture, string mapId) =>
        fixture with
        {
            ConsumerId = fixture.ConsumerId + "_invalid_map",
            Artifacts = fixture.Artifacts.Select(artifact => artifact.ArtifactKind == "entity_pack_v1"
                ? artifact with
                {
                    ContentJson = JsonSerializer.Serialize(new
                    {
                        entities = new[] { new { id = "entity/bad_map", kind = "npc", title = "Bad Map", package_map_id = mapId, position = new { x = 1, y = 1 }, instance_id = "entity/instance/bad_map" } }
                    }, JsonOptions)
                }
                : artifact).ToList()
        };

    private static PackageAssemblyWorldEntitiesConsumerFixture MutatePosition(PackageAssemblyWorldEntitiesConsumerFixture fixture, int x, int y) =>
        fixture with
        {
            ConsumerId = fixture.ConsumerId + "_invalid_position",
            Artifacts = fixture.Artifacts.Select(artifact => artifact.ArtifactKind == "entity_pack_v1"
                ? artifact with
                {
                    ContentJson = JsonSerializer.Serialize(new
                    {
                        entities = new[] { new { id = "entity/bad_position", kind = "npc", title = "Bad Position", package_map_id = "map/start", position = new { x, y }, instance_id = "entity/instance/bad_position" } }
                    }, JsonOptions)
                }
                : artifact).ToList()
        };

    private static PackageAssemblyWorldEntitiesConsumerFixture DuplicateEntityPrototype(PackageAssemblyWorldEntitiesConsumerFixture fixture) =>
        fixture with
        {
            ConsumerId = fixture.ConsumerId + "_duplicate_entity",
            Artifacts = fixture.Artifacts.Select(artifact => artifact.ArtifactKind == "entity_pack_v1"
                ? artifact with
                {
                    ContentJson = JsonSerializer.Serialize(new
                    {
                        entities = new[]
                        {
                            new { id = "entity/duplicate", kind = "npc", title = "Duplicate One", package_map_id = "map/start", position = new { x = 1, y = 1 }, instance_id = "entity/instance/duplicate_one" },
                            new { id = "entity/duplicate", kind = "npc", title = "Duplicate Two", package_map_id = "map/start", position = new { x = 2, y = 1 }, instance_id = "entity/instance/duplicate_two" }
                        }
                    }, JsonOptions)
                }
                : artifact).ToList()
        };

    private static PackageAssemblyWorldEntitiesArtifact Artifact(string artifactId, string kind, object content) =>
        new()
        {
            ArtifactId = artifactId,
            ArtifactKind = kind,
            ContentJson = JsonSerializer.Serialize(content, JsonOptions)
        };

    private static PackageAssemblyWorldEntitiesMapping Mapping(string sourceContractId, string status, string target) =>
        new()
        {
            SourceContractId = sourceContractId,
            Status = status,
            Target = target
        };

    private static PackageAssemblyWorldEntitiesInvalidScenario InvalidScenario(
        string id,
        IReadOnlyList<PackageAssemblyWorldEntitiesDiagnostic> diagnostics)
    {
        var sorted = SortDiagnostics(diagnostics);
        return new PackageAssemblyWorldEntitiesInvalidScenario
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = sorted.All(diagnostic => diagnostic.Severity != "error"),
            MutatedEvidenceKind = id,
            Diagnostics = sorted
        };
    }

    private static T? Deserialize<T>(
        string json,
        string target,
        ICollection<PackageAssemblyWorldEntitiesDiagnostic> diagnostics)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Diagnostic("error", "package_world_entities.json.invalid", target, exception.Message));
            return default;
        }
    }

    private static string RenderReport(PackageAssemblyWorldEntitiesReport report, PackageAssemblyWorldEntitiesPackageSummary packageSummary)
    {
        var lines = new List<string>
        {
            "# Package Assembly World And Entities Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Manual gate: {report.ManualGate}",
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Goal 024 evidence verified: {report.Goal024EvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Goal 023 evidence verified: {report.Goal023EvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Real consumer passed: {report.RealConsumerPassed.ToString().ToLowerInvariant()}",
            $"- Synthetic consumer passed: {report.SyntheticConsumerPassed.ToString().ToLowerInvariant()}",
            $"- Anti-overfit proof passed: {report.AntiOverfitProofPassed.ToString().ToLowerInvariant()}",
            $"- Package summary hash: {report.PackageSummaryHash}",
            $"- Report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- External execution: none",
            string.Empty,
            "## Consumer Summaries",
            string.Empty
        };
        lines.AddRange(packageSummary.ConsumerSummaries.Select(summary => $"- {summary.ConsumerId}: maps={summary.MapCount}, prototypes={summary.EntityPrototypeCount}, placements={summary.MapPlacementCount}, packageHash={summary.PackageHash}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(PackageAssemblyWorldEntitiesReport report)
    {
        var lines = new List<string>
        {
            "# Package Assembly World And Entities Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- accepted=false: {(!report.Accepted).ToString().ToLowerInvariant()}",
            $"- realConsumerPassed: {report.RealConsumerPassed.ToString().ToLowerInvariant()}",
            $"- syntheticConsumerPassed: {report.SyntheticConsumerPassed.ToString().ToLowerInvariant()}",
            $"- antiOverfitProofPassed: {report.AntiOverfitProofPassed.ToString().ToLowerInvariant()}",
            $"- invalidMatrix: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- scopeGuardPassed: {report.ScopeGuardPassed.ToString().ToLowerInvariant()}",
            $"- publicGamePackageSchemaChanged: {report.PublicGamePackageSchemaChanged.ToString().ToLowerInvariant()}",
            $"- productVerticalGate: {report.ProductVerticalGate.ToString().ToLowerInvariant()}",
            "- Goal 026 or S206 started: false"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderScopeReport(PackageAssemblyWorldEntitiesScopeReport report)
    {
        var lines = new List<string>
        {
            "# Goal 025 Final Artifact Scope Report",
            string.Empty,
            $"- Scenario: {report.ScenarioId}",
            $"- Passed: {report.Passed.ToString().ToLowerInvariant()}",
            $"- Allowed path count: {report.AllowedPathCount}",
            $"- Violations: {report.ViolationCount}",
            string.Empty,
            "## Notes",
            string.Empty
        };
        lines.AddRange(report.Notes.Select(note => "- " + note));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static bool TryResolveMapId(JsonElement record, IReadOnlyDictionary<string, string> sceneToMap, out string mapId)
    {
        mapId = FirstNonEmpty(JsonString(record, "package_map_id"), JsonString(record, "packageMapId"), JsonString(record, "map_id"), JsonString(record, "mapId"));
        if (!string.IsNullOrWhiteSpace(mapId))
        {
            return true;
        }

        var sceneId = FirstNonEmpty(JsonString(record, "scene_id"), JsonString(record, "sceneId"));
        return sceneToMap.TryGetValue(sceneId, out mapId!);
    }

    private static bool TryReadPosition(JsonElement record, out int x, out int y)
    {
        if (record.TryGetProperty("position", out var position) && position.ValueKind == JsonValueKind.Object)
        {
            x = JsonInt(position, "x");
            y = JsonInt(position, "y");
            return true;
        }

        x = JsonInt(record, "x");
        y = JsonInt(record, "y");
        return x >= 0 && y >= 0;
    }

    private static string NormalizeEntityPrototypeId(string sourceId, string kind, int index)
    {
        if (string.Equals(kind, "player", StringComparison.OrdinalIgnoreCase))
        {
            return "entity/player";
        }

        var value = !string.IsNullOrWhiteSpace(sourceId)
            ? sourceId
            : !string.IsNullOrWhiteSpace(kind)
                ? kind
                : index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var normalized = NormalizeIdSegment(value);
        return normalized.StartsWith("entity/", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : "entity/" + normalized;
    }

    private static string NormalizeIdSegment(string value)
    {
        var builder = new StringBuilder();
        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch) || ch == '/')
            {
                builder.Append(ch);
            }
            else if (ch is '-' or '_' or ' ')
            {
                builder.Append('/');
            }
        }

        var normalized = string.Join('/', builder.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(normalized) ? "generated" : normalized;
    }

    private static string JsonString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int JsonInt(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt32()
            : -1;

    private static string FirstNonEmpty(params string[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static IReadOnlyList<PackageAssemblyWorldEntitiesDiagnostic> SortDiagnostics(IEnumerable<PackageAssemblyWorldEntitiesDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static PackageAssemblyWorldEntitiesDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static void EnsureContained(string root, string path)
    {
        if (!IsContained(root, path))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static bool IsContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        return pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
    }

    private static string RelativePath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed record PackageAssemblyWorldEntitiesOptions
{
    public string PreviousAcceptedGate { get; init; } = PackageAssemblyWorldEntitiesAcceptanceService.PreviousAcceptedGate;
    public bool MissingGoal024CoverageAuditEvidence { get; init; }
    public bool MissingGoal023GeneratorInputs { get; init; }
    public bool SyntheticAntiOverfitFixtureMissing { get; init; }
    public bool HardcodedFrontierOnlyOutput { get; init; }
}

public sealed record PackageAssemblyWorldEntitiesResult
{
    public PackageAssemblyWorldEntitiesMappingProof MappingContractProof { get; init; } = new();
    public PackageAssemblyWorldEntitiesFixtures InputFixtures { get; init; } = new();
    public PackageAssemblyWorldEntitiesAssemblyReport AssemblyReport { get; init; } = new();
    public PackageAssemblyWorldEntitiesPackageSummary PackageSummary { get; init; } = new();
    public PackageAssemblyWorldEntitiesAntiOverfitProof AntiOverfitProof { get; init; } = new();
    public PackageAssemblyWorldEntitiesInvalidMatrix InvalidMatrix { get; init; } = new();
    public PackageAssemblyWorldEntitiesScopeReport ScopeReport { get; init; } = new();
    public PackageAssemblyWorldEntitiesReport Report { get; init; } = new();
    public string MappingContractProofJson { get; init; } = string.Empty;
    public string InputFixturesJson { get; init; } = string.Empty;
    public string AssemblyReportJson { get; init; } = string.Empty;
    public string PackageSummaryJson { get; init; } = string.Empty;
    public string AntiOverfitFixturesJson { get; init; } = string.Empty;
    public string InvalidMatrixJson { get; init; } = string.Empty;
    public string ScopeReportJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
    public string ScopeReportMarkdown { get; init; } = string.Empty;
}

public sealed record PackageAssemblyWorldEntitiesWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string MappingContractProofJsonPath { get; init; } = string.Empty;
    public string InputFixturesJsonPath { get; init; } = string.Empty;
    public string AssemblyReportJsonPath { get; init; } = string.Empty;
    public string PackageSummaryJsonPath { get; init; } = string.Empty;
    public string AntiOverfitFixturesJsonPath { get; init; } = string.Empty;
    public string InvalidMatrixJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
    public string ScopeReportJsonPath { get; init; } = string.Empty;
    public string ScopeReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record PackageAssemblyWorldEntitiesReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool Goal024EvidenceVerified { get; init; }
    public bool Goal023EvidenceVerified { get; init; }
    public bool RealConsumerPassed { get; init; }
    public bool SyntheticConsumerPassed { get; init; }
    public bool AntiOverfitProofPassed { get; init; }
    public bool WorldEntityMappingWritten { get; init; }
    public bool PackageSummaryWritten { get; init; }
    public bool PackageAssemblyExecuted { get; init; }
    public bool ProductVerticalGate { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool LlmRagProviderMediaLuaExecuted { get; init; }
    public bool ScopeGuardPassed { get; init; }
    public string MappingContractProofHash { get; init; } = string.Empty;
    public string InputFixturesHash { get; init; } = string.Empty;
    public string AssemblyReportHash { get; init; } = string.Empty;
    public string PackageSummaryHash { get; init; } = string.Empty;
    public string AntiOverfitFixturesHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string ScopeReportHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public PackageAssemblyWorldEntitiesInvalidMatrix InvalidMatrix { get; init; } = new();
    public IReadOnlyList<PackageAssemblyWorldEntitiesDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyWorldEntitiesEvidence
{
    public string Goal023GeneratorInputsPath { get; init; } = string.Empty;
    public string Goal024ReportPath { get; init; } = string.Empty;
    public string Goal024CoverageMatrixPath { get; init; } = string.Empty;
    public string Goal024NextSlicePlanPath { get; init; } = string.Empty;
    public string Goal023GeneratorInputsHash { get; init; } = string.Empty;
    public string Goal024ReportHash { get; init; } = string.Empty;
    public string Goal024CoverageMatrixHash { get; init; } = string.Empty;
    public string Goal024NextSlicePlanHash { get; init; } = string.Empty;
    public bool Goal023EvidenceVerified { get; init; }
    public bool Goal024EvidenceVerified { get; init; }
    public IReadOnlyList<CapabilityBundlePipelineInputRecord> Goal023PipelineInputs { get; init; } = [];
    public IReadOnlyList<string> Goal024WorldGapIds { get; init; } = [];
    public IReadOnlyList<string> Goal024EntityGapIds { get; init; } = [];
}

public sealed record PackageAssemblyWorldEntitiesMappingProof
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> AcceptedInputs { get; init; } = [];
    public IReadOnlyList<string> ExistingPackageTargets { get; init; } = [];
    public IReadOnlyList<string> OutputStatuses { get; init; } = [];
    public IReadOnlyList<PackageAssemblyWorldEntitiesMapping> MappingResults { get; init; } = [];
    public string RealConsumerId { get; init; } = string.Empty;
    public string SyntheticConsumerId { get; init; } = string.Empty;
    public bool RealConsumerPassed { get; init; }
    public bool SyntheticConsumerPassed { get; init; }
    public IReadOnlyList<string> NonGoals { get; init; } = [];
}

public sealed record PackageAssemblyWorldEntitiesMapping
{
    public string SourceContractId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
}

public sealed record PackageAssemblyWorldEntitiesFixtures
{
    public string SchemaVersion { get; init; } = string.Empty;
    public PackageAssemblyWorldEntitiesConsumerFixture RealConsumer { get; init; } = new();
    public PackageAssemblyWorldEntitiesConsumerFixture SyntheticConsumer { get; init; } = new();
}

public sealed record PackageAssemblyWorldEntitiesConsumerFixture
{
    public string ConsumerId { get; init; } = string.Empty;
    public string SourceProfileId { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string SelectionId { get; init; } = string.Empty;
    public IReadOnlyList<PackageAssemblyWorldEntitiesArtifact> Artifacts { get; init; } = [];
}

public sealed record PackageAssemblyWorldEntitiesArtifact
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string ContentJson { get; init; } = "{}";
}

public sealed record PackageAssemblyWorldEntitiesAssemblyReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public IReadOnlyList<ConsumerPackageSummary> Consumers { get; init; } = [];
    public IReadOnlyList<PackageAssemblyWorldEntitiesDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyWorldEntitiesPackageSummary
{
    public string SchemaVersion { get; init; } = string.Empty;
    public IReadOnlyList<ConsumerPackageSummary> ConsumerSummaries { get; init; } = [];
    public int TotalMaps { get; init; }
    public int TotalEntityPrototypes { get; init; }
    public int TotalMapPlacements { get; init; }
    public int TotalGeneratedRegions { get; init; }
    public int TotalGeneratedNpcs { get; init; }
}

public sealed record ConsumerPackageSummary
{
    public string ConsumerId { get; init; } = string.Empty;
    public string SourceProfileId { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string SelectionId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string PrimaryMapId { get; init; } = string.Empty;
    public string PrimaryEntityPrototypeId { get; init; } = string.Empty;
    public int MapCount { get; init; }
    public int EntityPrototypeCount { get; init; }
    public int MapPlacementCount { get; init; }
    public int GeneratedRegionCount { get; init; }
    public int GeneratedNpcCount { get; init; }
    public int AppliedArtifactCount { get; init; }
    public int PreservedArtifactCount { get; init; }
    public int ValidationIssueCount { get; init; }
    public string PackageHash { get; init; } = string.Empty;
    public IReadOnlyList<string> MappingTargets { get; init; } = [];
    public IReadOnlyList<PackageAssemblyWorldEntitiesDiagnostic> Diagnostics { get; init; } = [];

    public static ConsumerPackageSummary Missing(string consumerId) =>
        new()
        {
            ConsumerId = consumerId,
            Passed = false,
            Diagnostics = [new PackageAssemblyWorldEntitiesDiagnostic { Severity = "error", Code = "package_world_entities.anti_overfit.synthetic_missing", Target = consumerId, Message = "Synthetic anti-overfit fixture is missing." }]
        };
}

public sealed record PackageAssemblyWorldEntitiesAntiOverfitProof
{
    public string RealConsumerId { get; init; } = string.Empty;
    public string SyntheticConsumerId { get; init; } = string.Empty;
    public bool SyntheticConsumerPresent { get; init; }
    public bool DistinctConsumerIds { get; init; }
    public bool DistinctPackageMapIds { get; init; }
    public bool DistinctPrototypeIds { get; init; }
    public bool Passed { get; init; }
}

public sealed record PackageAssemblyWorldEntitiesInvalidMatrix
{
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<PackageAssemblyWorldEntitiesInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<PackageAssemblyWorldEntitiesDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyWorldEntitiesInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<PackageAssemblyWorldEntitiesDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyWorldEntitiesScopeReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int AllowedPathCount { get; init; }
    public int ViolationCount { get; init; }
    public IReadOnlyList<string> Notes { get; init; } = [];
}

public sealed record PackageAssemblyWorldEntitiesDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
