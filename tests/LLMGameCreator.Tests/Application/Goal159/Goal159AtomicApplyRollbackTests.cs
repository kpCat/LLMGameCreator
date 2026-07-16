using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal159;

[Collection(Goal156Collection.Name)]
public sealed class Goal159AtomicApplyRollbackTests
{
    [Fact]
    public void Behavioral_successful_apply_replaces_source_package_authoring_and_identity()
    {
        using var fixture = Goal159TransactionFixture.Create();

        var result = fixture.Apply();

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal("candidate-generation", fixture.ReadProject(".llmgc/generation/world.txt"));
        Assert.Equal("candidate-authoring", fixture.ReadProject(".llmgc/authoring/composition.json"));
        Assert.Equal("candidate-package", fixture.ReadProject("package.json"));
        Assert.Equal("candidate-identity", fixture.ReadProject(".llmgc/project-identity.json"));
    }

    [Fact]
    public void Behavioral_successful_apply_adds_exactly_one_history_and_retains_old_history()
    {
        using var fixture = Goal159TransactionFixture.Create();

        var result = fixture.Apply();

        Assert.True(result.Passed);
        Assert.Equal("old-history", fixture.ReadProject(".llmgc/build-history/old.json"));
        Assert.Equal("new-history", fixture.ReadProject(".llmgc/build-history/new.json"));
        Assert.Equal(2, Directory.EnumerateFiles(Path.Combine(fixture.Project, ".llmgc", "build-history")).Count());
    }

    [Fact]
    public void Behavioral_old_release_candidate_bytes_are_retained()
    {
        using var fixture = Goal159TransactionFixture.Create();
        var before = File.ReadAllBytes(fixture.ProjectPath(
            UnifiedGameProjectWorkspaceVocabulary.ReleaseCandidateRecordRelativePath));

        var result = fixture.Apply();

        Assert.True(result.Passed);
        Assert.Equal(before, File.ReadAllBytes(fixture.ProjectPath(
            UnifiedGameProjectWorkspaceVocabulary.ReleaseCandidateRecordRelativePath)));
    }

    [Fact]
    public void Behavioral_apply_journal_reaches_committed_and_working_files_are_cleaned()
    {
        using var fixture = Goal159TransactionFixture.Create();

        var result = fixture.Apply();
        var journal = fixture.ReadJournal();

        Assert.Equal("committed", result.JournalStatus);
        Assert.Equal("committed", journal.State);
        Assert.False(Directory.Exists(Path.Combine(fixture.TransactionRoot, "backups")));
        Assert.False(Directory.Exists(Path.Combine(fixture.TransactionRoot, "staging")));
    }

    [Theory]
    [InlineData(GameProjectSeedRegenerationFailurePoint.AfterGenerationSwap)]
    [InlineData(GameProjectSeedRegenerationFailurePoint.AfterPackageReplace)]
    [InlineData(GameProjectSeedRegenerationFailurePoint.AfterAuthoringReplace)]
    [InlineData(GameProjectSeedRegenerationFailurePoint.AfterHistoryAdd)]
    [InlineData(GameProjectSeedRegenerationFailurePoint.BeforeFinalValidation)]
    public void Behavioral_failure_points_restore_exact_authoritative_hashes(
        GameProjectSeedRegenerationFailurePoint failurePoint)
    {
        using var fixture = Goal159TransactionFixture.Create();
        var before = fixture.AuthoritativeHashes();

        var result = fixture.Apply(failurePoint);

        Assert.False(result.Passed);
        Assert.True(result.RollbackApplied, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal("rolled_back", result.JournalStatus);
        Assert.Equal(before, fixture.AuthoritativeHashes());
        Assert.False(File.Exists(fixture.ProjectPath(".llmgc/build-history/new.json")));
    }

    [Fact]
    public void Behavioral_failure_after_history_removes_only_new_history()
    {
        using var fixture = Goal159TransactionFixture.Create();

        var result = fixture.Apply(GameProjectSeedRegenerationFailurePoint.AfterHistoryAdd);

        Assert.True(result.RollbackApplied);
        Assert.Equal("old-history", fixture.ReadProject(".llmgc/build-history/old.json"));
        Assert.False(File.Exists(fixture.ProjectPath(".llmgc/build-history/new.json")));
    }

    [Fact]
    public void Behavioral_rollback_journal_retains_before_hash_inventory()
    {
        using var fixture = Goal159TransactionFixture.Create();
        var before = fixture.AuthoritativeHashes();

        var result = fixture.Apply(GameProjectSeedRegenerationFailurePoint.AfterPackageReplace);
        var journal = fixture.ReadJournal();

        Assert.True(result.RollbackApplied);
        Assert.NotEmpty(journal.BeforeSha256);
        Assert.All(journal.BeforeSha256, pair =>
        {
            if (pair.Value.Length == 0) Assert.False(before.ContainsKey(pair.Key));
            else Assert.Equal(before[pair.Key], pair.Value);
        });
    }

    [Fact]
    public void Behavioral_success_result_lists_only_transactional_authoritative_roots()
    {
        using var fixture = Goal159TransactionFixture.Create();

        var result = fixture.Apply();

        Assert.Contains(SeededGeneratedProjectVocabulary.GenerationRelativeRoot,
            result.ChangedRelativePaths);
        Assert.Contains(UnifiedGameProjectWorkspaceVocabulary.AuthoringRelativeRoot,
            result.ChangedRelativePaths);
        Assert.Contains("package.json", result.ChangedRelativePaths);
        Assert.DoesNotContain(result.ChangedRelativePaths,
            path => path.StartsWith("Builds", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_transaction_writes_current_regeneration_record_atomically()
    {
        using var fixture = Goal159TransactionFixture.Create();

        var result = fixture.Apply();

        Assert.True(result.Passed);
        Assert.Equal(fixture.RecordJson, fixture.ReadProject(
            GameProjectSeedRegenerationVocabulary.LastSuccessfulRelativePath));
    }

    [Fact]
    public void Behavioral_transaction_rejects_history_collision_without_mutating_authority()
    {
        using var fixture = Goal159TransactionFixture.Create();
        File.WriteAllText(fixture.ProjectPath(".llmgc/build-history/new.json"), "owner", new UTF8Encoding(false));
        var before = fixture.AuthoritativeHashes();

        var result = fixture.Apply();

        Assert.False(result.Passed);
        Assert.Contains("regeneration.history_collision", result.Diagnostics);
        Assert.Equal(before, fixture.AuthoritativeHashes());
    }
}

internal sealed class Goal159TransactionFixture : IDisposable
{
    private Goal159TransactionFixture(string root, string project, string candidate)
    {
        Root = root;
        Project = project;
        Candidate = candidate;
    }

    public string Root { get; }
    public string Project { get; }
    public string Candidate { get; }
    public string AttemptId { get; } = "goal159tx";
    public string RecordJson { get; } = "{\"schemaVersion\":\"seed_regeneration_result_v1\",\"status\":\"GREEN\"}\n";
    public string TransactionRoot => ProjectPath(
        GameProjectSeedRegenerationVocabulary.TransactionsRelativeRoot + "/" + AttemptId);

    public static Goal159TransactionFixture Create()
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal159Transaction",
            Guid.NewGuid().ToString("N"));
        var project = Path.Combine(root, "project");
        var candidate = Path.Combine(root, "candidate");
        var fixture = new Goal159TransactionFixture(root, project, candidate);
        fixture.WriteProject(".llmgc/generation/world.txt", "old-generation");
        fixture.WriteProject(".llmgc/authoring/composition.json", "old-authoring");
        fixture.WriteProject(".llmgc/project-identity.json", "old-identity");
        fixture.WriteProject("package.json", "old-package");
        fixture.WriteProject(".llmgc/build-history/old.json", "old-history");
        fixture.WriteProject(UnifiedGameProjectWorkspaceVocabulary.ReleaseCandidateRecordRelativePath, "old-rc");
        fixture.WriteCandidate(".llmgc/generation/world.txt", "candidate-generation");
        fixture.WriteCandidate(".llmgc/authoring/composition.json", "candidate-authoring");
        fixture.WriteCandidate(".llmgc/project-identity.json", "candidate-identity");
        fixture.WriteCandidate("package.json", "candidate-package");
        fixture.WriteCandidate(".llmgc/build-history/new.json", "new-history");
        fixture.WriteCandidate(UnifiedGameProjectWorkspaceVocabulary.ReleaseCandidateRecordRelativePath, "old-rc");
        return fixture;
    }

    public GameProjectSeedRegenerationTransactionResult Apply(
        GameProjectSeedRegenerationFailurePoint failurePoint = GameProjectSeedRegenerationFailurePoint.None) =>
        new GameProjectSeedRegenerationTransaction().Apply(new GameProjectSeedRegenerationTransactionRequest
        {
            AttemptId = AttemptId,
            ProjectFolder = Project,
            CandidateFolder = Candidate,
            CandidateBuildHistoryFileName = "new.json",
            RegenerationRecordJson = RecordJson,
            FailurePoint = failurePoint
        });

    public SeedRegenerationTransactionJournal ReadJournal() => JsonSerializer.Deserialize<SeedRegenerationTransactionJournal>(
        File.ReadAllText(Path.Combine(TransactionRoot, "journal.json"), Encoding.UTF8),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    public SortedDictionary<string, string> AuthoritativeHashes()
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in Directory.EnumerateFiles(Project, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(Project, file).Replace('\\', '/');
            if (relative.StartsWith(GameProjectSeedRegenerationVocabulary.RegenerationRelativeRoot + "/",
                    StringComparison.Ordinal)) continue;
            result[relative] = Goal156TestKit.Hash(file);
        }
        return result;
    }

    public string ProjectPath(string relative) => Path.Combine(Project,
        relative.Replace('/', Path.DirectorySeparatorChar));

    public string ReadProject(string relative) => File.ReadAllText(ProjectPath(relative), Encoding.UTF8);

    public void WriteProject(string relative, string value) => Write(ProjectPath(relative), value);
    public void WriteCandidate(string relative, string value) => Write(Path.Combine(Candidate,
        relative.Replace('/', Path.DirectorySeparatorChar)), value);

    private static void Write(string path, string value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, value, new UTF8Encoding(false));
    }

    public void Dispose()
    {
        if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
    }
}
