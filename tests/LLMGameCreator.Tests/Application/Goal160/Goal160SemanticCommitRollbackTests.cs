using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal159;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal160;

[Collection(Goal160Collection.Name)]
public sealed class Goal160SemanticCommitRollbackTests
{
    [Fact]
    public void Behavioral_truth_tokens_are_rechecked_after_operation_lock()
    {
        using var fixture = Goal159TransactionFixture.Create();
        var truth = new StubTruthReader(Tokens(), "inventory");
        using var operation = Acquire(fixture.Project);
        var result = Apply(fixture, operation, truth, new StubCommitValidator(true));
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(1, truth.TokenReadCount);
        Assert.Equal(1, truth.InventoryReadCount);
    }

    [Theory]
    [InlineData("source", "regeneration.source_changed")]
    [InlineData("authoring", "regeneration.authoring_changed")]
    [InlineData("package", "regeneration.package_changed")]
    [InlineData("identity", "regeneration.identity_changed")]
    [InlineData("rc", "regeneration.release_candidate_changed")]
    public void Behavioral_locked_truth_mismatch_rejects_before_backups(
        string token,
        string diagnostic)
    {
        using var fixture = Goal159TransactionFixture.Create();
        var actual = token switch
        {
            "source" => Tokens() with { SourceRecordSha256 = "changed" },
            "authoring" => Tokens() with { AuthoringRevision = 8 },
            "package" => Tokens() with { FinalStateHash = "changed" },
            "identity" => Tokens() with { ProjectIdentityFingerprint = "changed" },
            _ => Tokens() with { ReleaseCandidateRecordSha256 = "changed" }
        };
        var before = fixture.AuthoritativeHashes();
        using var operation = Acquire(fixture.Project);
        var result = Apply(fixture, operation, new StubTruthReader(actual, "inventory"),
            new StubCommitValidator(true));
        operation.Dispose();
        Assert.Contains(diagnostic, result.Diagnostics);
        Assert.Equal(before, fixture.AuthoritativeHashes());
        Assert.False(Directory.Exists(fixture.TransactionRoot));
    }

    [Fact]
    public void Behavioral_authoritative_inventory_mismatch_rejects_before_mutation()
    {
        using var fixture = Goal159TransactionFixture.Create();
        var before = fixture.AuthoritativeHashes();
        using var operation = Acquire(fixture.Project);
        var result = Apply(fixture, operation, new StubTruthReader(Tokens(), "changed"),
            new StubCommitValidator(true));
        operation.Dispose();
        Assert.Contains("regeneration.authoritative_inventory_changed", result.Diagnostics);
        Assert.Equal(before, fixture.AuthoritativeHashes());
    }

    [Fact]
    public void Behavioral_disposed_operation_lease_is_rejected()
    {
        using var fixture = Goal159TransactionFixture.Create();
        var coordinator = new GameProjectOperationCoordinator();
        var operation = coordinator.TryAcquire(fixture.Project, GameProjectOperationKinds.RegenerationApply);
        operation.Dispose();
        var result = Apply(fixture, operation, new StubTruthReader(Tokens(), "inventory"),
            new StubCommitValidator(true));
        Assert.Contains("project_operation.lease_invalid", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_journal_enters_validating_before_semantic_injection()
    {
        using var fixture = Goal159TransactionFixture.Create();
        var result = fixture.Apply(GameProjectSeedRegenerationFailurePoint.DuringSemanticValidation);
        Assert.True(result.RollbackApplied);
        Assert.Contains("DuringSemanticValidation", result.Diagnostics.Single(), StringComparison.Ordinal);
        Assert.Equal("rolled_back", fixture.ReadJournal().State);
    }

    [Fact]
    public void Behavioral_semantic_validator_success_commits_and_cleans_backups()
    {
        using var fixture = Goal159TransactionFixture.Create();
        using var operation = Acquire(fixture.Project);
        var validator = new StubCommitValidator(true);
        var result = Apply(fixture, operation, new StubTruthReader(Tokens(), "inventory"), validator);
        Assert.True(result.Passed);
        Assert.Equal(1, validator.CallCount);
        Assert.Equal("committed", fixture.ReadJournal().State);
        Assert.False(Directory.Exists(Path.Combine(fixture.TransactionRoot, "backups")));
    }

    [Theory]
    [InlineData("semantic.source_invalid")]
    [InlineData("semantic.package_hash_mismatch")]
    [InlineData("semantic.history_hash_mismatch")]
    [InlineData("semantic.identity_mismatch")]
    [InlineData("semantic.release_candidate_bytes_changed")]
    public void Behavioral_semantic_failure_rolls_back_exact_before_hashes(string cause)
    {
        using var fixture = Goal159TransactionFixture.Create();
        var before = fixture.AuthoritativeHashes();
        using var operation = Acquire(fixture.Project);
        var result = Apply(fixture, operation, new StubTruthReader(Tokens(), "inventory"),
            new StubCommitValidator(false, cause));
        operation.Dispose();
        Assert.True(result.RollbackApplied);
        Assert.Contains("regeneration.commit_semantic_validation_failed:" + cause, result.Diagnostics);
        Assert.Equal(before, fixture.AuthoritativeHashes());
    }

    [Fact]
    public void Behavioral_validating_crash_state_recovers_exact_before_state()
    {
        using var fixture = Goal159TransactionFixture.Create();
        var before = fixture.AuthoritativeHashes();
        var failed = fixture.Apply(GameProjectSeedRegenerationFailurePoint.AfterPackageReplace);
        Assert.True(failed.RollbackApplied);
        var journalPath = Path.Combine(fixture.TransactionRoot, "journal.json");
        var journal = fixture.ReadJournal() with { State = "validating" };
        File.WriteAllText(journalPath, JsonSerializer.Serialize(journal,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }),
            new UTF8Encoding(false));
        File.WriteAllText(fixture.ProjectPath("package.json"), "crashed-candidate", new UTF8Encoding(false));
        var recovered = new GameProjectSeedRegenerationTransaction().Recover(fixture.Project);
        Assert.True(recovered.Passed, string.Join(Environment.NewLine, recovered.Diagnostics));
        Assert.Equal(before, fixture.AuthoritativeHashes());
        Assert.Equal("rolled_back", fixture.ReadJournal().State);
    }

    private static GameProjectOperationLease Acquire(string project)
    {
        var coordinator = new GameProjectOperationCoordinator();
        var lease = coordinator.TryAcquire(project, GameProjectOperationKinds.RegenerationApply);
        Assert.True(lease.Acquired, lease.Diagnostic);
        return lease;
    }

    private static GameProjectSeedRegenerationTransactionResult Apply(
        Goal159TransactionFixture fixture,
        GameProjectOperationLease operation,
        IGameProjectSeedRegenerationTruthReader truth,
        IGameProjectSeedRegenerationCommitValidator validator) =>
        new GameProjectSeedRegenerationTransaction().Apply(new GameProjectSeedRegenerationTransactionRequest
        {
            AttemptId = fixture.AttemptId,
            ProjectFolder = fixture.Project,
            CandidateFolder = fixture.Candidate,
            CandidateBuildHistoryFileName = "new.json",
            RegenerationRecordJson = fixture.RecordJson,
            ExpectedTruthTokens = Tokens(),
            ExpectedAuthoritativeInventorySha256 = "inventory",
            CandidateSealSha256 = new string('a', 64),
            OperationLease = operation,
            TruthReader = truth,
            CommitValidator = validator,
            CommitValidationRequest = new GameProjectSeedRegenerationCommitValidationRequest
            {
                ProjectFolder = fixture.Project
            }
        });

    private static GameProjectSeedRegenerationTruthTokens Tokens() => new()
    {
        SourceRecordSha256 = "source",
        QualifiedAuthoringFingerprint = "authoring",
        AuthoringRevision = 7,
        ActivatedPackageSha256 = "package",
        CompositionPackageSha256 = "composition",
        FinalStateHash = "state",
        ProjectIdentityFingerprint = "identity",
        ReleaseCandidateRecordSha256 = "rc"
    };
}

internal sealed class StubTruthReader : IGameProjectSeedRegenerationTruthReader
{
    private readonly GameProjectSeedRegenerationTruthTokens _tokens;
    private readonly string _inventory;
    public StubTruthReader(GameProjectSeedRegenerationTruthTokens tokens, string inventory)
    {
        _tokens = tokens;
        _inventory = inventory;
    }
    public int TokenReadCount { get; private set; }
    public int InventoryReadCount { get; private set; }
    public GameProjectSeedRegenerationTruthTokens CaptureTruthTokens(
        string projectFolder,
        GameProjectOperationLease operationLease)
    {
        TokenReadCount++;
        return _tokens;
    }
    public string CaptureAuthoritativeInventorySha256(string projectFolder)
    {
        InventoryReadCount++;
        return _inventory;
    }
}

internal sealed class StubCommitValidator : IGameProjectSeedRegenerationCommitValidator
{
    private readonly bool _passed;
    private readonly string _diagnostic;
    public StubCommitValidator(bool passed, string diagnostic = "semantic.failed")
    {
        _passed = passed;
        _diagnostic = diagnostic;
    }
    public int CallCount { get; private set; }
    public GameProjectSeedRegenerationCommitValidationResult Validate(
        GameProjectSeedRegenerationCommitValidationRequest request,
        GameProjectOperationLease operationLease)
    {
        CallCount++;
        return new GameProjectSeedRegenerationCommitValidationResult
        {
            Passed = _passed,
            Diagnostics = _passed ? [] : [_diagnostic]
        };
    }
}
