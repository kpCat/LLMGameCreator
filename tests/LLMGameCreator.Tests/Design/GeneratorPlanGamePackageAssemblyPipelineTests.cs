using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanGamePackageAssemblyPipelineTests
{
    [Fact]
    public void ReaderParsesApprovedArtifactSetWithJsonContentAndRejectsInvalidJson()
    {
        var reader = new GeneratorPlanApprovedArtifactSetReader();
        var parsed = reader.ReadJson(ApprovedSetJson(
            ArtifactJson("artifact/profile", "game_profile_v1", """{"game":{"title":"Pipe Game","genre":"adventure"}}""")));

        Assert.Equal("snapshot/test", parsed.SnapshotId);
        Assert.Equal("batch/test", parsed.SourceProductionBatchId);
        var artifact = Assert.Single(parsed.ApprovedArtifacts);
        Assert.Equal("artifact/profile", artifact.ArtifactId);
        Assert.Equal("game_profile_v1", artifact.ArtifactKind);
        Assert.Equal("game_profile_v1", artifact.ExpectedArtifactContract);
        Assert.Contains("\"Pipe Game\"", artifact.ContentJson);
        Assert.Throws<ArgumentException>(() => reader.ReadJson("{ invalid"));
    }

    [Fact]
    public void AssemblerCreatesBaselineValidPackageAndMapsKnownArtifacts()
    {
        var artifactSet = ApprovedSet(
            Artifact("artifact/profile", "game_profile_v1", """{"game":{"title":"Assembly Quest","genre":"cozy_test"}}"""),
            Artifact("artifact/scene", "scene_pack_v1", """{"scenes":[{"id":"scene/start","title":"Landing Field"},{"id":"scene/cave","title":"Quiet Cave"}]}"""),
            Artifact("artifact/entities", "entity_pack_v1", """{"entities":[{"id":"npc/guide","kind":"guide","title":"Guide"}]}"""),
            Artifact("artifact/quests", "quest_pack_v1", """{"quests":[{"id":"quest/hello","title":"Say Hello","objectives":["talk_to_guide"]}]}"""),
            Artifact("artifact/mechanics", "mechanics_pack_v1", """{"mechanics":[{"id":"mechanic/look","title":"Look Around"}]}"""));

        var assembled = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet);
        var report = new GamePackageValidator().Validate(assembled.Package);

        Assert.Equal("Assembly Quest", assembled.Package.Manifest.Title);
        Assert.Equal("map/start", assembled.Package.Manifest.StartMapId);
        Assert.Contains(assembled.Package.Game.TilePrototypes, tile => tile.Id == "tile/grass");
        Assert.Contains(assembled.Package.Game.Maps, map => map.Id == "map/start" && map.Name == "Landing Field");
        Assert.Contains(assembled.Package.Game.Maps, map => map.Id.StartsWith("map/draft/", StringComparison.Ordinal));
        Assert.Contains(assembled.Package.Game.EntityPrototypes, entity => entity.Id == "entity/player");
        Assert.Contains(assembled.Package.Game.EntityPrototypes, entity => entity.Name == "Guide");
        Assert.Contains(assembled.Package.Game.Maps.Single(map => map.Id == "map/start").Entities, entity => entity.PrototypeId == "entity/player");
        Assert.Contains(assembled.Package.Game.Quests, quest => quest.Title == "Say Hello");
        Assert.Contains(assembled.Package.Game.Abilities, ability => ability.Name == "Look Around");
        Assert.True(report.IsValid, string.Join(Environment.NewLine, report.Issues.Select(issue => issue.ToString())));
    }

    [Fact]
    public void AssemblerWarnsForSemanticUnknownAndInvalidArtifactJson()
    {
        var artifactSet = ApprovedSet(
            Artifact("artifact/semantic", "semantic_pack_v1", """{"semantic_groups":[]}"""),
            Artifact("artifact/unknown", "unknown_pack_v1", "{}"),
            Artifact("artifact/broken", "entity_pack_v1", "{ invalid"));

        var assembled = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet);

        Assert.Contains(assembled.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanGamePackageAssemblyDiagnosticCodes.UnmappedArtifactKind && diagnostic.ArtifactKind == "semantic_pack_v1");
        Assert.Contains(assembled.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanGamePackageAssemblyDiagnosticCodes.UnmappedArtifactKind && diagnostic.ArtifactKind == "unknown_pack_v1");
        Assert.Contains(assembled.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanGamePackageAssemblyDiagnosticCodes.ApprovedArtifactInvalidJson && diagnostic.ArtifactId == "artifact/broken");
    }

    [Fact]
    public async Task ServiceAssemblesValidatesSerializesExportsAndCanSkipMarkdown()
    {
        using var temp = new TempDirectory();
        var exportFolder = Path.Combine(temp.Path, "exported");
        var service = new GeneratorPlanGamePackageAssemblyService(
            new GeneratorPlanGamePackageAssembler(),
            new GamePackageValidator(),
            new GeneratorPlanGamePackageAssemblyValidator(),
            new GeneratorPlanGamePackageAssemblyMarkdownRenderer(),
            new JsonGamePackageRepository());

        var result = await service.AssembleFromApprovedArtifactSetAsync(
            ApprovedSet(Artifact("artifact/profile", "game_profile_v1", """{"game":{"title":"Export Game","genre":"test"}}""")),
            new GeneratorPlanGamePackageAssemblyRequest
            {
                ExportPackageJson = true,
                ExportFolderPath = exportFolder
            },
            CancellationToken.None);
        var withoutMarkdown = await service.AssembleFromApprovedArtifactSetAsync(
            ApprovedSet(Artifact("artifact/profile", "game_profile_v1", """{"game":{"title":"No Markdown"}}""")),
            new GeneratorPlanGamePackageAssemblyRequest { RenderMarkdown = false },
            CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.Equal(GeneratorPlanGamePackageAssemblyStatus.ValidPackage, result.Status);
        Assert.Contains("\"title\": \"Export Game\"", result.PackageJson);
        Assert.NotEmpty(result.MarkdownReport);
        Assert.True(File.Exists(Path.Combine(exportFolder, "package.json")));
        Assert.Empty(withoutMarkdown.MarkdownReport);
    }

    [Fact]
    public void ValidatorAndPolicyReportInputPackageAndExportIssues()
    {
        var validator = new GeneratorPlanGamePackageAssemblyValidator();
        var report = new GamePackageValidator().Validate(new LLMGameCreator.GamePackage.GamePackageDefinition());
        var diagnostics = validator.Validate(
            new GeneratorPlanApprovedArtifactSet(),
            new GeneratorPlanGamePackageAssemblyRequest { ExportPackageJson = true, SerializePackageJson = true },
            string.Empty,
            report,
            Array.Empty<GeneratorPlanGamePackageAssemblyDiagnostic>());
        var validationResults = GeneratorPlanGamePackageAssemblyPolicy.ToValidationResults("artifact/test",
        [
            new GeneratorPlanGamePackageAssemblyDiagnostic { Severity = GeneratorPlanPreviewDiagnosticSeverity.Info, Code = "info", Message = "Info" },
            new GeneratorPlanGamePackageAssemblyDiagnostic { Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning, Code = "warning", Message = "Warning" }
        ]);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == GeneratorPlanGamePackageAssemblyDiagnosticCodes.NoApprovedArtifacts);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == GeneratorPlanGamePackageAssemblyDiagnosticCodes.ExportPathMissing);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == GeneratorPlanGamePackageAssemblyDiagnosticCodes.PackageSerializationError);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == GeneratorPlanGamePackageAssemblyDiagnosticCodes.PackageValidationError);
        Assert.DoesNotContain(validationResults, result => result.Severity == GeneratorPlanPreviewDiagnosticSeverity.Info);
        Assert.Single(validationResults);
    }

    [Fact]
    public void MarkdownRendererRendersSummaryEscapesCellsAndTruncatesPackageJson()
    {
        var result = new GeneratorPlanGamePackageAssemblyResult
        {
            Status = GeneratorPlanGamePackageAssemblyStatus.ValidPackage,
            Package = new LLMGameCreator.GamePackage.GamePackageDefinition
            {
                Manifest =
                {
                    PackageId = "game/test",
                    Title = "Pipe | Game"
                }
            },
            PackageJson = "{\"value\":\"" + new string('x', 4000) + "\"}",
            Summary = new GeneratorPlanGamePackageAssemblySummary { ApprovedArtifactCount = 1, MappedArtifactCount = 1 },
            Mappings =
            [
                new GeneratorPlanGamePackageAssemblyMapping
                {
                    ArtifactId = "artifact|profile",
                    ArtifactKind = "game_profile_v1",
                    ExpectedArtifactContract = "game_profile_v1",
                    Result = "mapped",
                    Target = "manifest"
                }
            ],
            Diagnostics =
            [
                new GeneratorPlanGamePackageAssemblyDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "code|pipe",
                    Target = "target|pipe",
                    Message = "Line 1\nLine 2"
                }
            ]
        };

        var markdown = new GeneratorPlanGamePackageAssemblyMarkdownRenderer().Render(result);

        Assert.Contains("# GamePackage Assembly", markdown);
        Assert.Contains("artifact\\|profile", markdown);
        Assert.Contains("Line 1<br>Line 2", markdown);
        Assert.Contains("\n...", markdown);
    }

    [Fact]
    public async Task ArtifactServiceSavesAssemblyPackageDraftMarkdownValidationAndIsIdempotent()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var assembly = await AssembleAsync();
        var service = new GeneratorPlanGamePackageAssemblyArtifactService(database);

        var first = await service.SaveAsync(assembly, new GeneratorPlanGamePackageAssemblyArtifactSaveRequest(), CancellationToken.None);
        var second = await service.SaveAsync(assembly, new GeneratorPlanGamePackageAssemblyArtifactSaveRequest(), CancellationToken.None);
        var artifacts = await database.ListGeneratedArtifactsAsync(CancellationToken.None);

        Assert.Equal(first.AssemblyArtifact.Id, second.AssemblyArtifact.Id);
        Assert.Equal(3, artifacts.Count);
        Assert.NotNull(first.MarkdownArtifact);
        Assert.Contains("\"manifest\"", first.PackageDraftArtifact.Json);
        Assert.Equal(first.ValidationResults.Count, (await database.ListValidationResultsByArtifactAsync(first.AssemblyArtifact.Id, CancellationToken.None)).Count);
    }

    [Fact]
    public async Task ArtifactServiceSupportsCustomIdsSkipsMarkdownAndReaderReturnsLatestArtifacts()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var reader = new GeneratorPlanGamePackageAssemblyArtifactReader(database);
        var missing = await reader.ReadLatestAsync(CancellationToken.None);
        var assembly = await new GeneratorPlanGamePackageAssemblyService().AssembleFromApprovedArtifactSetAsync(
            ApprovedSet(Artifact("artifact/profile", "game_profile_v1", """{"game":{"title":"Stored Game"}}""")),
            new GeneratorPlanGamePackageAssemblyRequest { RenderMarkdown = false },
            CancellationToken.None);
        var service = new GeneratorPlanGamePackageAssemblyArtifactService(database);

        var custom = await service.SaveAsync(assembly, new GeneratorPlanGamePackageAssemblyArtifactSaveRequest
        {
            AssemblyArtifactId = GeneratorPlanGamePackageAssemblyArtifactIds.AssemblyArtifactId,
            PackageDraftArtifactId = GeneratorPlanGamePackageAssemblyArtifactIds.PackageDraftArtifactId,
            MarkdownArtifactId = "artifact/custom/markdown",
            GeneratedBy = "test"
        }, CancellationToken.None);
        var loaded = await reader.ReadLatestAsync(CancellationToken.None);

        Assert.False(missing.Exists);
        Assert.Null(custom.MarkdownArtifact);
        Assert.True(loaded.Exists);
        Assert.NotNull(loaded.AssemblyArtifact);
        Assert.NotNull(loaded.PackageDraftArtifact);
        Assert.Null(loaded.MarkdownArtifact);
        Assert.Empty(loaded.ValidationResults);
    }

    [Fact]
    public async Task EndToEndProductionApprovalAssemblyPersistenceAndLatestReader()
    {
        using var temp = new TempDirectory();
        var examplePath = WriteExample(temp.Path);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var approvalArtifactService = new GeneratorPlanDraftArtifactApprovalArtifactService(
            new GeneratorPlanDraftArtifactApprovalService(),
            database);
        var approval = await approvalArtifactService.CaptureAsync(new GeneratorPlanDraftArtifactApprovalArtifactRequest
        {
            PreviewRequest = new GeneratorPlanPreviewRequest { SourcePath = examplePath },
            ApprovalRequest = new GeneratorPlanDraftArtifactApprovalRequest
            {
                AutoApproveValidArtifacts = true,
                RenderMarkdown = false
            }
        }, CancellationToken.None);
        var assemblyService = new GeneratorPlanGamePackageAssemblyService(
            new GeneratorPlanGamePackageAssembler(),
            new GamePackageValidator(),
            new GeneratorPlanGamePackageAssemblyValidator(),
            new GeneratorPlanGamePackageAssemblyMarkdownRenderer(),
            new JsonGamePackageRepository(),
            new GeneratorPlanDraftArtifactApprovalArtifactReader(database));

        var assembly = await assemblyService.AssembleFromLatestApprovedArtifactSetAsync(new GeneratorPlanGamePackageAssemblyRequest
        {
            ExportPackageJson = true,
            ExportFolderPath = Path.Combine(temp.Path, "package")
        }, CancellationToken.None);
        await new GeneratorPlanGamePackageAssemblyArtifactService(database).SaveAsync(assembly, new GeneratorPlanGamePackageAssemblyArtifactSaveRequest(), CancellationToken.None);
        var loaded = await new GeneratorPlanGamePackageAssemblyArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.NotNull(approval.ApprovedArtifactSetArtifact);
        Assert.True(assembly.Ok, string.Join(Environment.NewLine, assembly.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.Contains(assembly.Package.Game.Maps, map => map.Id == "map/start");
        Assert.Contains(assembly.Package.Game.EntityPrototypes, entity => entity.Id == "entity/player");
        Assert.True(File.Exists(Path.Combine(temp.Path, "package", "package.json")));
        Assert.True(loaded.Exists);
        Assert.NotNull(loaded.AssemblyArtifact);
        Assert.NotNull(loaded.PackageDraftArtifact);
        Assert.NotNull(loaded.MarkdownArtifact);
    }

    private static async Task<GeneratorPlanGamePackageAssemblyResult> AssembleAsync()
    {
        return await new GeneratorPlanGamePackageAssemblyService().AssembleFromApprovedArtifactSetAsync(
            ApprovedSet(Artifact("artifact/profile", "game_profile_v1", """{"game":{"title":"Persisted Game"}}""")),
            new GeneratorPlanGamePackageAssemblyRequest(),
            CancellationToken.None);
    }

    private static GeneratorPlanApprovedArtifactSet ApprovedSet(params GeneratorPlanApprovedArtifact[] artifacts)
    {
        return new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "0.1",
            SnapshotId = "snapshot/test",
            SourceProductionBatchId = "batch/test",
            ApprovedArtifacts = artifacts
        };
    }

    private static GeneratorPlanApprovedArtifact Artifact(string id, string kind, string contentJson)
    {
        return new GeneratorPlanApprovedArtifact
        {
            ArtifactId = id,
            ArtifactKind = kind,
            ExpectedArtifactContract = kind,
            ContentJson = contentJson
        };
    }

    private static string ApprovedSetJson(params string[] artifacts)
    {
        return $$"""
        {
          "schema_version": "0.1",
          "snapshot_id": "snapshot/test",
          "source_production_batch_id": "batch/test",
          "approved_artifacts": [
            {{string.Join(",", artifacts)}}
          ]
        }
        """;
    }

    private static string ArtifactJson(string id, string kind, string contentJson)
    {
        using var document = JsonDocument.Parse(contentJson);
        return $$"""
        {
          "artifact_id": "{{id}}",
          "artifact_kind": "{{kind}}",
          "expected_artifact_contract": "{{kind}}",
          "content_json": {{document.RootElement.GetRawText()}}
        }
        """;
    }

    private static string WriteExample(string root)
    {
        var path = Path.Combine(root, "assembly.example.json");
        File.WriteAllText(path, """
        {
          "schema_version": "0.1",
          "example_id": "example/assembly/v1",
          "title": "Assembly Example",
          "purpose": "Test package assembly.",
          "source_profile": {
            "id": "profile/assembly/v1"
          },
          "selected_feature_bundles": [
            "feature_bundle/assembly/v1"
          ],
          "target_artifacts": [
            "game_profile_v1",
            "scene_pack_v1",
            "entity_pack_v1",
            "quest_pack_v1"
          ],
          "steps": [
            {
              "id": "step/profile",
              "order": 1,
              "title": "Profile",
              "producer_role": "role/designer_llm/v1",
              "context_pack_template": "context_template/design_discussion/v1",
              "expected_artifact_contract": "game_profile_v1",
              "inputs": ["game_profile_v1"],
              "validation_gates": ["validation.level_0_json_shape"],
              "on_success": "stage_profile",
              "on_failure": "request_profile_clarification"
            },
            {
              "id": "step/scene",
              "order": 2,
              "title": "Scene",
              "producer_role": "role/designer_llm/v1",
              "context_pack_template": "context_template/design_discussion/v1",
              "expected_artifact_contract": "scene_pack_v1",
              "inputs": ["scene_pack_v1"],
              "validation_gates": ["validation.level_0_json_shape"],
              "on_success": "stage_scene",
              "on_failure": "request_scene_clarification"
            },
            {
              "id": "step/entity",
              "order": 3,
              "title": "Entity",
              "producer_role": "role/designer_llm/v1",
              "context_pack_template": "context_template/design_discussion/v1",
              "expected_artifact_contract": "entity_pack_v1",
              "inputs": ["entity_pack_v1"],
              "validation_gates": ["validation.level_0_json_shape"],
              "on_success": "stage_entity",
              "on_failure": "request_entity_clarification"
            },
            {
              "id": "step/quest",
              "order": 4,
              "title": "Quest",
              "producer_role": "role/designer_llm/v1",
              "context_pack_template": "context_template/design_discussion/v1",
              "expected_artifact_contract": "quest_pack_v1",
              "inputs": ["quest_pack_v1"],
              "validation_gates": ["validation.level_0_json_shape"],
              "on_success": "stage_quest",
              "on_failure": "request_quest_clarification"
            }
          ]
        }
        """);
        return path;
    }

    private static async Task<SqliteDesignDatabase> CreateInitializedDatabaseAsync(string root)
    {
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(root, ".llmgc", "design.db"), CancellationToken.None);
        return database;
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
