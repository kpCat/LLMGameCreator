using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class ActivePackageQuestDialoguePreviewSmokeTests
{
    [Fact]
    public async Task ActivePackageQuestDialoguePreviewProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectFolder = ResolveProjectFolder(temp.Path);
        var assemblyFolder = Path.Combine(projectFolder, ".llmgc", "package-assembly");
        var repository = new JsonGamePackageRepository();
        var artifacts = ProductSmokeBaselineApprovedArtifacts.CreateExpandedApprovedArtifactSet();
        var assembly = await new GeneratorPlanGamePackageAssemblyService(
                new GeneratorPlanGamePackageAssembler(),
                new GamePackageValidator(),
                new GeneratorPlanGamePackageAssemblyValidator(),
                new GeneratorPlanGamePackageAssemblyMarkdownRenderer(),
                repository)
            .AssembleFromApprovedArtifactSetAsync(
                artifacts,
                new GeneratorPlanGamePackageAssemblyRequest
                {
                    AppliedAtUtc = ProductSmokeBaselineApprovedArtifacts.AppliedAtUtc,
                    ExportPackageJson = true,
                    ExportFolderPath = assemblyFolder
                },
                CancellationToken.None);

        Assert.True(assembly.Ok, string.Join(Environment.NewLine, assembly.Diagnostics.Select(item => item.Message)));
        var assembledPackagePath = Path.Combine(assemblyFolder, "package.json");
        Assert.True(File.Exists(assembledPackagePath));

        await repository.SaveAsync(projectFolder, assembly.Package, CancellationToken.None);
        var rootPackagePath = Path.Combine(projectFolder, "package.json");
        var rootBeforeActivation = await File.ReadAllTextAsync(rootPackagePath);
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(projectFolder, CancellationToken.None);
        current.ReplaceCurrent(new GamePackageDefinition());

        var activation = await new AssembledGamePackageActivationService(repository, new GamePackageValidator(), current)
            .ActivateLatestAsync(CancellationToken.None);

        Assert.True(activation.Ok, string.Join(Environment.NewLine, activation.Diagnostics));
        var activePackage = Assert.IsType<GamePackageDefinition>(current.CurrentPackage);
        Assert.NotEmpty(activePackage.GeneratedContent.Npcs);
        Assert.NotEmpty(activePackage.GeneratedContent.Dialogues);
        Assert.NotEmpty(activePackage.GeneratedContent.Quests);
        Assert.Equal(rootBeforeActivation, await File.ReadAllTextAsync(rootPackagePath));

        var runtime = new DefaultGameRuntime();
        var start = runtime.Start(activePackage);
        Assert.True(start.Success);
        var preview = new GeneratedPackageRuntimePreviewService().Build(activePackage, start.State);
        var catalog = new GeneratedContentInteractionPreviewService().Build(preview);
        var npc = Assert.Single(catalog.Categories.Single(category => category.Id == "npcs").Entries);
        Assert.Contains("dialogue/smoke-guide-intro", npc.ReferenceIds);
        Assert.Contains("dialogue/smoke-guide-intro", npc.DetailsText);

        var session = new GeneratedQuestDialoguePreviewService();
        session.StartSession(activePackage);
        var linkedDialogue = Assert.Single(session.FindDialoguesLinkedToNpc("npc/smoke-guide"));
        var dialogue = session.PreviewDialogue(linkedDialogue.SourceId);
        Assert.True(dialogue.Ok);
        Assert.Contains("Welcome to Smoke Harbor.", dialogue.Lines);

        var quest = Assert.Single(activePackage.GeneratedContent.Quests);
        Assert.True(session.StartQuest(quest.SourceId).Ok);
        var advanced = session.MarkNextStep(quest.SourceId);
        Assert.True(advanced.Ok);
        Assert.Equal(1, advanced.CompletedStepCount);
        var journal = session.BuildJournal();
        Assert.Equal(1, journal.ActiveCount);
        Assert.Equal("Check generated content", Assert.Single(journal.Entries).CurrentStep);

        var movement = runtime.Execute(activePackage, start.State, PlayerCommand.Move(Direction2D.Right));
        Assert.True(movement.Success);
        Assert.Equal(2, movement.State.PlayerPosition.X);
        Assert.All(artifacts.ApprovedArtifacts, artifact =>
        {
            Assert.DoesNotContain("provider", artifact.ContentJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("lm_studio", artifact.ContentJson, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
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
