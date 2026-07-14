using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154D;

public sealed class Goal154DAllSelectedRealProjectTests
{
    internal static Goal154DPreparedProject PrepareExactOwnerCopy()
    {
        var source = SourcePath();
        var sourceHash = TreeHash(source);
        var copy = DisposableCopy(source, "prepared-owner-all-selected");
        try
        {
            var controller = Open(copy.Path);
            var attempted = LatestAttempt(copy.Path).SelectedModuleIds;
            foreach (var moduleId in attempted.Where(id => controller.Snapshot().Mechanics.All(item =>
                         item.ModuleId != id || !item.Selected)))
                controller.SetModuleSelected(moduleId, true);
            controller.SaveAuthoring();
            return new Goal154DPreparedProject(copy.Root, copy.Path, source, sourceHash);
        }
        catch
        {
            copy.Dispose();
            throw;
        }
    }

    [Fact]
    public void Behavioral_exact_owner_failed_attempt_build_repeat_reopen_is_green_and_source_immutable()
    {
        var source = SourcePath();
        Assert.True(Directory.Exists(source), "The real goal148-manual source project is required.");
        var sourceBefore = TreeHash(source);
        var sourceParameters = ExplicitParameters(ProjectDocument(source));
        using var copy = DisposableCopy(source, "owner-all-selected");
        var controller = Open(copy.Path);
        var failedAttempt = LatestAttempt(copy.Path);
        var attempted = failedAttempt.SelectedModuleIds;
        var allOptional = controller.Snapshot().Mechanics.Where(item => !item.Required)
            .Select(item => item.ModuleId).OrderBy(id => id, StringComparer.Ordinal).ToList();

        Assert.Equal("FAILED", failedAttempt.Status);
        Assert.Equal("composition.qualification", failedAttempt.FailureStage);
        Assert.Equal(allOptional, attempted);
        Assert.Equal(12, attempted.Count);
        Assert.Equal(10, failedAttempt.ConfiguredParameterCount);
        Assert.Contains(Goal154DFixture.AlchemyModuleId, attempted);
        Assert.Contains(Goal154DFixture.QuestModuleId, attempted);
        Assert.Contains(Goal154DFixture.DialogueModuleId, attempted);
        foreach (var moduleId in attempted.Where(id => controller.Snapshot().Mechanics.All(item =>
                     item.ModuleId != id || !item.Selected)))
            controller.SetModuleSelected(moduleId, true);
        controller.SaveAuthoring();

        var first = Open(copy.Path).BuildAndQualify();
        var second = Open(copy.Path).BuildAndQualify();
        var reopened = Open(copy.Path).Snapshot();

        Assert.True(first.Passed, string.Join("; ", first.Diagnostics));
        Assert.True(second.Passed, string.Join("; ", second.Diagnostics));
        Assert.Equal("GREEN", first.Status);
        Assert.Equal(22, first.SelectedMechanicCount);
        Assert.Equal(10, first.ConfiguredParameterCount);
        Assert.Equal(22, reopened.SelectedMechanicCount);
        Assert.All(attempted, id => Assert.Contains(reopened.Mechanics, item => item.ModuleId == id && item.Selected));
        Assert.Equal(first.PackageSha256, second.PackageSha256);
        Assert.Equal(first.CompositionPackageSha256, second.CompositionPackageSha256);
        Assert.Equal(first.FinalStateHash, second.FinalStateHash);
        Assert.True(first.CheckpointReloadPassed && first.FullReplayEquivalent);
        Assert.True(second.CheckpointReloadPassed && second.FullReplayEquivalent);
        Assert.Equal("CURRENT", reopened.SocialConfigurationStatus);
        Assert.Equal(0, first.Social?.ReputationBefore);
        Assert.Equal(10, first.Social?.ReputationAfter);
        Assert.Equal(10, first.Social?.GoldAfterQuest);
        Assert.Equal(17, first.Social?.GoldAfterClaim);
        Assert.Equal("claimed", first.Social?.SocialOutcome);
        Assert.Equal(sourceParameters, ExplicitParameters(ProjectDocument(copy.Path)));
        Assert.Equal(sourceBefore, TreeHash(source));
    }

    [Fact]
    public void Behavioral_every_current_selectable_optional_module_qualifies_without_disabling_profiles()
    {
        var root = Goal154DFixture.FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var optional = library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
            .Select(module => module.ModuleId).OrderBy(id => id, StringComparer.Ordinal).ToList();
        var document = new FeatureModuleCompositionDocument
        {
            CompositionId = "goal154d-all-current-optional",
            BaseCandidateId = "minimal-map-game-balanced-baseline",
            SelectedModuleIds = optional,
            CatalogFingerprint = library.CatalogFingerprint,
            ModuleFingerprints = optional.ToDictionary(id => id, id => library.ModuleFingerprints[id], StringComparer.Ordinal)
        };
        var output = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal154D", Guid.NewGuid().ToString("N"));
        try
        {
            var result = new FeatureModuleParameterizedCompositionService(
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault()).MaterializeAndQualify(
                root, library, document, output, useCapabilityDrivenRuntimePlaythrough: true);

            Assert.True(result.Passed, string.Join("; ", result.Diagnostics));
            Assert.Equal(optional.Count, result.SelectedModuleCount);
            Assert.All(optional, id => Assert.Contains(result.QualifiedDocument.SelectedModuleIds, item => item == id));
            Assert.Contains(optional, id => id == Goal154DFixture.AlchemyModuleId);
            var advance = result.Qualification.Artifacts.Session.ActionJournal.Single(item =>
                item.ActionId == Goal154DFixture.AdvanceActionId);
            Assert.Equal("SKIPPED", advance.Status);
            Assert.Contains(advance.Diagnostics, item => item == "questCompletionPath=already_completed");
            Assert.True(result.CheckpointReloadPassed);
            Assert.True(result.FullReplayEquivalent);
            Assert.Equal("completed", result.Qualification.Artifacts.Session.CanonicalSession.RuntimeSession.GameplayState.Quests
                .Single(item => item.QuestId == Goal154DFixture.QuestId).State);
        }
        finally
        {
            if (Directory.Exists(output)) Directory.Delete(output, true);
        }
    }

    private static UnifiedGameProjectWorkspaceController Open(string project)
    {
        var root = Goal154DFixture.FindRoot();
        var repository = new JsonGamePackageRepository();
        var current = new CurrentGamePackageService(repository);
        current.LoadAsync(project, CancellationToken.None).GetAwaiter().GetResult();
        var controller = new UnifiedGameProjectWorkspaceController(current,
            new GameProjectFeatureModuleAuthoringService(root),
            new GameProjectBuildAndQualificationService(root,
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), repository,
                new GamePackageValidator(), current));
        controller.OpenProject(project);
        return controller;
    }

    private static string SourcePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LLMGameCreator", "Games", "goal148-manual");

    private static string ProjectDocument(string project) => Directory.EnumerateFiles(
        Path.Combine(project, ".llmgc", "authoring"), "project-*.featurecomposition.json").Single();

    private static IReadOnlyList<string> ExplicitParameters(string path)
    {
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        return json.RootElement.GetProperty("parameterValues").EnumerateArray().Select(item =>
                item.GetProperty("moduleId").GetString() + "|" + item.GetProperty("parameterId").GetString()
                + "|" + item.GetProperty("value").GetRawText())
            .OrderBy(value => value, StringComparer.Ordinal).ToList();
    }

    private static FailedAttempt LatestAttempt(string project)
    {
        var path = Directory.EnumerateFiles(Path.Combine(project, ".llmgc", "build-history"), "*.json")
            .OrderBy(value => value, StringComparer.Ordinal).Last();
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        var root = json.RootElement;
        return new FailedAttempt(
            root.GetProperty("attemptStatus").GetString() ?? string.Empty,
            root.GetProperty("failureStage").GetString() ?? string.Empty,
            root.GetProperty("attemptedSelectedModuleIds").EnumerateArray()
                .Select(item => item.GetString() ?? string.Empty).OrderBy(id => id, StringComparer.Ordinal).ToList(),
            root.GetProperty("configuredParameterCount").GetInt32());
    }

    private static DisposableProject DisposableCopy(string source, string name)
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal154D", Guid.NewGuid().ToString("N"));
        var target = Path.Combine(root, name);
        Copy(source, target);
        return new DisposableProject(root, target);
    }

    private static void Copy(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, true);
        }
    }

    private static string TreeHash(string path)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                     .OrderBy(file => file, StringComparer.Ordinal))
        {
            hash.AppendData(Encoding.UTF8.GetBytes(Path.GetRelativePath(path, file).Replace('\\', '/') + "\n"));
            hash.AppendData(File.ReadAllBytes(file));
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private sealed record DisposableProject(string Root, string Path) : IDisposable
    {
        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, true);
        }
    }

    private sealed record FailedAttempt(
        string Status,
        string FailureStage,
        IReadOnlyList<string> SelectedModuleIds,
        int ConfiguredParameterCount);
}

internal sealed record Goal154DPreparedProject(
    string Root,
    string Path,
    string SourcePath,
    string SourceTreeHash) : IDisposable
{
    public void Dispose()
    {
        if (Directory.Exists(Root)) Directory.Delete(Root, true);
    }
}
