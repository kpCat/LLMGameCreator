using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal159;

[Collection(Goal156Collection.Name)]
public sealed class Goal159CrashRecoveryTests
{
    [Fact]
    public void Behavioral_prepared_journal_recovery_restores_exact_before_hashes()
    {
        using var fixture = Interrupted("prepared");
        var before = fixture.AuthoritativeHashes();
        fixture.WriteProject("package.json", "interrupted-package");

        var result = new GameProjectSeedRegenerationTransaction().Recover(fixture.Project);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(before, fixture.AuthoritativeHashes());
        Assert.Equal("rolled_back", fixture.ReadJournal().State);
    }

    [Fact]
    public void Behavioral_applying_journal_recovery_restores_exact_before_hashes()
    {
        using var fixture = Interrupted("applying");
        var before = fixture.AuthoritativeHashes();
        fixture.WriteProject(".llmgc/generation/world.txt", "interrupted-generation");
        fixture.WriteProject(".llmgc/authoring/composition.json", "interrupted-authoring");

        var result = new GameProjectSeedRegenerationTransaction().Recover(fixture.Project);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(before, fixture.AuthoritativeHashes());
        Assert.Equal("rolled_back", fixture.ReadJournal().State);
    }

    [Fact]
    public void Behavioral_committed_journal_recovery_validates_candidate_and_remains_committed()
    {
        using var fixture = Goal159TransactionFixture.Create();
        var applied = fixture.Apply();
        var before = fixture.AuthoritativeHashes();

        var recovery = new GameProjectSeedRegenerationTransaction().Recover(fixture.Project);

        Assert.True(applied.Passed);
        Assert.True(recovery.Passed, string.Join(Environment.NewLine, recovery.Diagnostics));
        Assert.Equal(before, fixture.AuthoritativeHashes());
        Assert.Equal("committed", fixture.ReadJournal().State);
    }

    [Fact]
    public void Behavioral_incomplete_backup_yields_recovery_required_without_mutation()
    {
        using var fixture = Interrupted("applying");
        var backup = Directory.EnumerateFiles(Path.Combine(fixture.TransactionRoot, "backups"), "*",
            SearchOption.AllDirectories).First();
        File.Delete(backup);
        var before = fixture.AuthoritativeHashes();

        var result = new GameProjectSeedRegenerationTransaction().Recover(fixture.Project);

        Assert.False(result.Passed);
        Assert.Equal("recovery_required", result.JournalStatus);
        Assert.Contains("regeneration.recovery_required", result.Diagnostics);
        Assert.Equal(before, fixture.AuthoritativeHashes());
        Assert.Equal("applying", fixture.ReadJournal().State);
    }

    [Fact]
    public void Behavioral_nonterminal_journal_evidence_is_preserved_after_recovery_decision()
    {
        using var fixture = Interrupted("prepared");
        var journalPath = Path.Combine(fixture.TransactionRoot, "journal.json");

        var result = new GameProjectSeedRegenerationTransaction().Recover(fixture.Project);

        Assert.True(result.Passed);
        Assert.True(File.Exists(journalPath));
        Assert.Equal("rolled_back", fixture.ReadJournal().State);
        Assert.NotEmpty(fixture.ReadJournal().BeforeSha256);
    }

    private static Goal159TransactionFixture Interrupted(string state)
    {
        var fixture = Goal159TransactionFixture.Create();
        var failed = fixture.Apply(GameProjectSeedRegenerationFailurePoint.AfterPackageReplace);
        Assert.True(failed.RollbackApplied);
        var journalPath = Path.Combine(fixture.TransactionRoot, "journal.json");
        var root = JsonNode.Parse(File.ReadAllText(journalPath, Encoding.UTF8))!.AsObject();
        root["state"] = state;
        File.WriteAllText(journalPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true })
                                       + Environment.NewLine, new UTF8Encoding(false));
        return fixture;
    }
}
