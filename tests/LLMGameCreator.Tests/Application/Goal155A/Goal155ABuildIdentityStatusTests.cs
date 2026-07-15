using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Tests.Application.Goal155;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal155A;

public sealed class Goal155ABuildIdentityStatusTests
{
    [Fact]
    public void Behavioral_record_composition_hash_difference_is_last_success()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-composition-differs");
        fixture.Write();
        fixture.RemovePayload();
        fixture.RewriteRecord(root => root["compositionPackageSha256"] = new string('d', 64));

        AssertLastSuccess(fixture.Read());
    }

    [Fact]
    public void Behavioral_record_final_hash_difference_is_last_success()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-final-differs");
        fixture.Write();
        fixture.RemovePayload();
        fixture.RewriteRecord(root =>
        {
            root["finalStateHash"] = new string('d', 64);
            root["standaloneFinalStateHash"] = new string('d', 64);
        });

        AssertLastSuccess(fixture.Read());
    }

    [Theory]
    [InlineData("activated")]
    [InlineData("composition")]
    [InlineData("final")]
    public void Behavioral_missing_current_build_identity_hash_is_unknown(string missing)
    {
        using var fixture = Goal155RcFixture.Create("goal155a-missing-" + missing);
        fixture.Write();
        var document = missing switch
        {
            "activated" => fixture.Document with { LastActivatedProjectPackageSha256 = string.Empty },
            "composition" => fixture.Document with { LastCompositionPackageSha256 = string.Empty },
            _ => fixture.Document with { LastQualifiedFinalStateHash = string.Empty }
        };

        var read = fixture.Read(document);

        Assert.NotNull(read.Record);
        Assert.Equal("UNKNOWN", read.ConfigurationStatus);
        Assert.Contains("rc.read.current_build_identity_missing", read.Diagnostics);
    }

    [Fact]
    public void Behavioral_failed_current_authoring_fingerprint_is_unknown()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-fingerprint-failure");
        fixture.Write();
        var invalid = fixture.Document with { SelectedModuleIds = ["missing.module"] };

        var read = fixture.Read(invalid);

        Assert.NotNull(read.Record);
        Assert.Equal("UNKNOWN", read.ConfigurationStatus);
    }

    [Fact]
    public void Behavioral_saved_authoring_change_is_last_success()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-authoring-change");
        fixture.Write();
        var changed = fixture.Document with { SelectedModuleIds = ["feature.profile.alchemy_focus"] };

        Assert.Equal("LAST_SUCCESS", fixture.Read(changed).ConfigurationStatus);
    }

    [Fact]
    public void Behavioral_returning_authoring_alone_cannot_restore_current_when_build_identity_differs()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-return-authoring");
        fixture.Write();
        fixture.RemovePayload();
        fixture.RewriteRecord(root => root["compositionPackageSha256"] = new string('d', 64));
        var changed = fixture.Document with { SelectedModuleIds = ["feature.profile.alchemy_focus"] };

        Assert.Equal("LAST_SUCCESS", fixture.Read(changed).ConfigurationStatus);
        AssertLastSuccess(fixture.Read());
    }

    [Theory]
    [InlineData("title")]
    [InlineData("version")]
    public void Behavioral_identity_metadata_difference_is_last_success(string changed)
    {
        using var fixture = Goal155RcFixture.Create("goal155a-identity-" + changed);
        fixture.Write();
        var identity = changed == "title"
            ? fixture.Identity with { Title = "Renamed" }
            : fixture.Identity with { Version = "2.0.0" };

        var read = fixture.Read(identity: identity);

        Assert.NotNull(read.Record);
        Assert.Equal("LAST_SUCCESS", read.ConfigurationStatus);
        Assert.Contains("rc.read.project_identity_metadata_differs", read.Diagnostics);
    }

    [Fact]
    public void Behavioral_history_independent_record_is_current_when_all_truth_correlates()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-history-independent");
        fixture.Write();

        var read = fixture.Read();

        Assert.Equal("CURRENT", read.ConfigurationStatus);
        Assert.False(Directory.Exists(Path.Combine(fixture.Project, ".llmgc", "build-history")));
    }

    private static void AssertLastSuccess(GameProjectReleaseCandidateReadResult read)
    {
        Assert.NotNull(read.Record);
        Assert.Equal("LAST_SUCCESS", read.ConfigurationStatus);
        Assert.Contains("rc.read.record_build_identity_differs_from_current", read.Diagnostics);
    }
}
