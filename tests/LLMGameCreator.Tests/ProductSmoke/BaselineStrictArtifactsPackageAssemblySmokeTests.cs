using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class BaselineStrictArtifactsPackageAssemblySmokeTests
{
    private static readonly string[] BaselineContracts =
    [
        "game_profile_v1",
        "scene_pack_v1",
        "quest_pack_v1",
        "mechanics_pack_v1"
    ];

    [Fact]
    public async Task BaselineStrictArtifactsPackageAssemblySmoke()
    {
        using var temp = new TempDirectory();
        var exportFolder = ResolveExportFolder(temp.Path);
        var service = new GeneratorPlanGamePackageAssemblyService(
            new GeneratorPlanGamePackageAssembler(),
            new GamePackageValidator(),
            new GeneratorPlanGamePackageAssemblyValidator(),
            new GeneratorPlanGamePackageAssemblyMarkdownRenderer(),
            new JsonGamePackageRepository());

        var result = await service.AssembleFromApprovedArtifactSetAsync(
            ProductSmokeBaselineApprovedArtifacts.CreateApprovedArtifactSet(),
            new GeneratorPlanGamePackageAssemblyRequest
            {
                AppliedAtUtc = ProductSmokeBaselineApprovedArtifacts.AppliedAtUtc,
                ExportPackageJson = true,
                ExportFolderPath = exportFolder
            },
            CancellationToken.None);

        var packageJsonPath = Path.Combine(exportFolder, "package.json");

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.NotNull(result.PackageValidationReport);
        Assert.DoesNotContain(result.PackageValidationReport.Issues, issue => issue.Severity is ValidationSeverity.Error or ValidationSeverity.Critical);
        Assert.True(File.Exists(packageJsonPath), $"Expected package output: {packageJsonPath}");

        using var packageJson = JsonDocument.Parse(await File.ReadAllTextAsync(packageJsonPath));
        var root = packageJson.RootElement;
        var manifest = root.GetProperty("manifest");
        var generatedContent = root.GetProperty("generatedContent");
        var profile = generatedContent.GetProperty("profile");
        var appliedArtifacts = generatedContent.GetProperty("appliedArtifacts").EnumerateArray().ToList();

        Assert.False(string.IsNullOrWhiteSpace(manifest.GetProperty("title").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(manifest.GetProperty("description").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(profile.GetProperty("title").GetString()));
        Assert.True(generatedContent.GetProperty("scenes").GetArrayLength() >= 1);
        Assert.True(generatedContent.GetProperty("quests").GetArrayLength() >= 1);
        Assert.True(generatedContent.GetProperty("mechanics").GetArrayLength() >= 1);
        Assert.True(appliedArtifacts.Count >= BaselineContracts.Length);

        foreach (var contractId in BaselineContracts)
        {
            var provenance = Assert.Single(appliedArtifacts, artifact =>
                string.Equals(artifact.GetProperty("contractId").GetString(), contractId, StringComparison.OrdinalIgnoreCase));

            Assert.False(string.IsNullOrWhiteSpace(provenance.GetProperty("artifactId").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(provenance.GetProperty("capabilitySelectionId").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(provenance.GetProperty("contentHash").GetString()));
            Assert.Equal("mapped", provenance.GetProperty("mappingResult").GetString());
        }

        Assert.All(result.Mappings, mapping => Assert.Equal(GeneratorPlanGamePackageAssemblyMappingResult.Mapped, mapping.Result));
    }

    [Fact]
    public void BaselineFixtureContainsAllStrictContractsWithoutProviderMetadata()
    {
        var artifactSet = ProductSmokeBaselineApprovedArtifacts.CreateApprovedArtifactSet();

        Assert.Equal(BaselineContracts.Length, artifactSet.ApprovedArtifacts.Count);
        foreach (var contractId in BaselineContracts)
        {
            var artifact = Assert.Single(artifactSet.ApprovedArtifacts, item =>
                string.Equals(item.ExpectedArtifactContract, contractId, StringComparison.OrdinalIgnoreCase));

            Assert.Equal(contractId, artifact.ArtifactKind);
            Assert.Contains("\"artifact_kind\"", artifact.ContentJson, StringComparison.Ordinal);
            Assert.DoesNotContain("provider", artifact.ContentJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("lm_studio", artifact.ContentJson, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string ResolveExportFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR");
        var exportFolder = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(tempPath, "package-output")
            : configured;

        Directory.CreateDirectory(exportFolder);
        return exportFolder;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}

internal static class ProductSmokeBaselineApprovedArtifacts
{
    public static readonly DateTimeOffset AppliedAtUtc = DateTimeOffset.Parse("2026-06-19T13:43:00Z");

    public static GeneratorPlanApprovedArtifactSet CreateApprovedArtifactSet()
    {
        return new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "0.1",
            SnapshotId = "snapshot/product-smoke/baseline-strict-package-assembly",
            SourceProductionBatchId = "batch/product-smoke/baseline-strict-package-assembly",
            ApprovedArtifacts =
            [
                Artifact("artifact/product-smoke/game-profile", "game_profile_v1", GameProfileJson()),
                Artifact("artifact/product-smoke/scene-pack", "scene_pack_v1", ScenePackJson()),
                Artifact("artifact/product-smoke/quest-pack", "quest_pack_v1", QuestPackJson()),
                Artifact("artifact/product-smoke/mechanics-pack", "mechanics_pack_v1", MechanicsPackJson())
            ]
        };
    }

    public static GeneratorPlanApprovedArtifactSet CreateExpandedApprovedArtifactSet()
    {
        var baseline = CreateApprovedArtifactSet();
        return baseline with
        {
            SnapshotId = "snapshot/product-smoke/expanded-contract-batch",
            SourceProductionBatchId = "batch/product-smoke/expanded-contract-batch",
            ApprovedArtifacts = baseline.ApprovedArtifacts.Concat(
            [
                Artifact("artifact/product-smoke/region-pack", "region_pack_v1", RegionPackJson()),
                Artifact("artifact/product-smoke/npc-pack", "npc_pack_v1", NpcPackJson()),
                Artifact("artifact/product-smoke/item-pack", "item_pack_v1", ItemPackJson()),
                Artifact("artifact/product-smoke/dialogue-pack", "dialogue_pack_v1", DialoguePackJson()),
                Artifact("artifact/product-smoke/encounter-pack", "encounter_pack_v1", EncounterPackJson())
            ]).ToList()
        };
    }

    private static GeneratorPlanApprovedArtifact Artifact(string id, string contractId, string contentJson)
    {
        return new GeneratorPlanApprovedArtifact
        {
            ArtifactId = id,
            ArtifactKind = contractId,
            ExpectedArtifactContract = contractId,
            ContentJson = contentJson
        };
    }

    private static string GameProfileJson()
    {
        return """
        {
          "schema_version": "0.1",
          "artifact_id": "artifact/product-smoke/game-profile",
          "artifact_kind": "game_profile_v1",
          "game": {
            "title": "Headless Smoke Baseline",
            "description": "A deterministic package assembly smoke baseline.",
            "genre": "adventure",
            "tone": "bright",
            "presentation_mode": "top_down_2d",
            "world_topology": "finite_map",
            "actor_model": "single_hero",
            "combat_model": "none",
            "core_loop": ["inspect", "help", "complete_quest"]
          },
          "pillars": ["Deterministic", "Inspectable", "Small"],
          "source_context": {
            "capability_selection_id": "generator_plan_capability_selection/product-smoke-baseline",
            "generated_at": "2026-06-19T13:43:00Z",
            "audit_id": "strict_llm_evaluation/product-smoke-fixture"
          }
        }
        """;
    }

    private static string ScenePackJson()
    {
        return """
        {
          "schema_version": "0.1",
          "artifact_id": "artifact/product-smoke/scene-pack",
          "artifact_kind": "scene_pack_v1",
          "scenes": [
            {
              "id": "scene/smoke-start",
              "title": "Smoke Start",
              "description": "A compact start scene for package assembly smoke.",
              "purpose": "Prove scene mapping and generatedContent.scenes."
            }
          ],
          "source_context": {
            "capability_selection_id": "generator_plan_capability_selection/product-smoke-baseline",
            "generated_at": "2026-06-19T13:43:00Z",
            "audit_id": "strict_llm_evaluation/product-smoke-fixture"
          }
        }
        """;
    }

    private static string QuestPackJson()
    {
        return """
        {
          "schema_version": "0.1",
          "artifact_id": "artifact/product-smoke/quest-pack",
          "artifact_kind": "quest_pack_v1",
          "quests": [
            {
              "id": "quest/smoke-intro",
              "title": "Run the Smoke",
              "description": "Verify the assembled package has an inspectable quest.",
              "steps": ["Inspect the package", "Check generated content", "Confirm provenance"],
              "objectives": ["package_json_exists"]
            }
          ],
          "source_context": {
            "capability_selection_id": "generator_plan_capability_selection/product-smoke-baseline",
            "generated_at": "2026-06-19T13:43:00Z",
            "audit_id": "strict_llm_evaluation/product-smoke-fixture"
          }
        }
        """;
    }

    private static string MechanicsPackJson()
    {
        return """
        {
          "schema_version": "0.1",
          "artifact_id": "artifact/product-smoke/mechanics-pack",
          "artifact_kind": "mechanics_pack_v1",
          "mechanics": [
            {
              "id": "mechanic/smoke-check",
              "name": "Smoke Check",
              "title": "Smoke Check",
              "description": "A deterministic mechanic used by the headless product smoke.",
              "tags": ["smoke", "baseline"]
            }
          ],
          "source_context": {
            "capability_selection_id": "generator_plan_capability_selection/product-smoke-baseline",
            "generated_at": "2026-06-19T13:43:00Z",
            "audit_id": "strict_llm_evaluation/product-smoke-fixture"
          }
        }
        """;
    }

    private static string RegionPackJson()
    {
        return """
        {"schema_version":"0.1","artifact_kind":"region_pack_v1","regions":[{"id":"region/smoke-harbor","title":"Smoke Harbor","description":"A compact region for expanded smoke.","scene_ids":["scene/smoke-start"]}],"source_context":{"capability_selection_id":"generator_plan_capability_selection/product-smoke-expanded","generated_at":"2026-06-19T13:43:00Z","audit_id":"strict_llm_evaluation/product-smoke-fixture"}}
        """;
    }

    private static string NpcPackJson()
    {
        return """
        {"schema_version":"0.1","artifact_kind":"npc_pack_v1","npcs":[{"id":"npc/smoke-guide","name":"Smoke Guide","description":"Guides the expanded smoke route.","region_id":"region/smoke-harbor","scene_id":"scene/smoke-start"}],"source_context":{"capability_selection_id":"generator_plan_capability_selection/product-smoke-expanded","generated_at":"2026-06-19T13:43:00Z","audit_id":"strict_llm_evaluation/product-smoke-fixture"}}
        """;
    }

    private static string ItemPackJson()
    {
        return """
        {"schema_version":"0.1","artifact_kind":"item_pack_v1","items":[{"id":"item/smoke-kit","name":"Smoke Kit","description":"A declarative smoke item without executable effects."}],"source_context":{"capability_selection_id":"generator_plan_capability_selection/product-smoke-expanded","generated_at":"2026-06-19T13:43:00Z","audit_id":"strict_llm_evaluation/product-smoke-fixture"}}
        """;
    }

    private static string DialoguePackJson()
    {
        return """
        {"schema_version":"0.1","artifact_kind":"dialogue_pack_v1","dialogues":[{"id":"dialogue/smoke-guide-intro","title":"Guide Introduction","description":"Introduces the expanded smoke route.","npc_id":"npc/smoke-guide","scene_id":"scene/smoke-start","lines":["Welcome to Smoke Harbor."]}],"source_context":{"capability_selection_id":"generator_plan_capability_selection/product-smoke-expanded","generated_at":"2026-06-19T13:43:00Z","audit_id":"strict_llm_evaluation/product-smoke-fixture"}}
        """;
    }

    private static string EncounterPackJson()
    {
        return """
        {"schema_version":"0.1","artifact_kind":"encounter_pack_v1","encounters":[{"id":"encounter/smoke-road","title":"Smoke Road","description":"A declarative encounter summary without execution.","region_id":"region/smoke-harbor","scene_id":"scene/smoke-start","npc_ids":["npc/smoke-guide"]}],"source_context":{"capability_selection_id":"generator_plan_capability_selection/product-smoke-expanded","generated_at":"2026-06-19T13:43:00Z","audit_id":"strict_llm_evaluation/product-smoke-fixture"}}
        """;
    }
}
