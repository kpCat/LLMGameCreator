using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Tests.Application.Goal155;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal155A;

public sealed class Goal155ACurrentPackageCorrelationTests
{
    [Fact]
    public void Behavioral_exact_record_document_package_and_identity_is_current()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-exact");
        fixture.Write();
        Assert.Equal("CURRENT", fixture.Read().ConfigurationStatus);
    }

    [Fact]
    public void Behavioral_package_byte_tamper_rejects_record_and_preserves_record_bytes()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-tamper");
        fixture.Write();
        var recordPath = fixture.Service.RecordPath(fixture.Project);
        var before = File.ReadAllBytes(recordPath);
        File.AppendAllText(Path.Combine(fixture.Project, "package.json"), "tamper");

        var read = fixture.Read();

        Assert.Null(read.Record);
        Assert.Equal("ABSENT", read.ConfigurationStatus);
        Assert.Contains("rc.read.current_package_hash_mismatch", read.Diagnostics);
        Assert.Equal(before, File.ReadAllBytes(recordPath));
    }

    [Fact]
    public void Behavioral_missing_current_package_rejects_record()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-missing-package");
        fixture.Write();
        File.Delete(Path.Combine(fixture.Project, "package.json"));

        var read = fixture.Read();

        Assert.Null(read.Record);
        Assert.Contains("rc.read.current_package_missing", read.Diagnostics);
    }

    [Fact]
    public void Behavioral_document_activated_hash_that_differs_from_actual_package_rejects_record()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-document-package-mismatch");
        fixture.Write();

        var read = fixture.Read(fixture.Document with { LastActivatedProjectPackageSha256 = new string('a', 64) });

        Assert.Null(read.Record);
        Assert.Contains("rc.read.current_package_hash_mismatch", read.Diagnostics);
    }

    [Fact]
    public void Behavioral_record_package_hash_that_differs_from_current_document_is_last_success()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-record-package-differs");
        fixture.Write();
        fixture.RemovePayload();
        fixture.RewriteRecord(root =>
        {
            root["packageSha256"] = new string('d', 64);
            root["standalonePackageSha256"] = new string('d', 64);
        });

        var read = fixture.Read();

        Assert.NotNull(read.Record);
        Assert.Equal("LAST_SUCCESS", read.ConfigurationStatus);
        Assert.Contains("rc.read.record_build_identity_differs_from_current", read.Diagnostics);
    }

    [Fact]
    public void Behavioral_package_id_mismatch_rejects_other_project_record()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-package-id");
        fixture.Write();

        var read = fixture.Read(identity: fixture.Identity with { PackageId = "game/another-project" });

        Assert.Null(read.Record);
        Assert.Contains("rc.read.project_package_id_mismatch", read.Diagnostics);
    }

    [Fact]
    public void Behavioral_complete_portable_copy_remains_current_without_execution()
    {
        using var fixture = Goal155RcFixture.Create("goal155a-portable-source");
        fixture.Write();
        using var copy = Goal155RcFixture.CopyOf(fixture, "goal155a-portable-copy");

        var read = copy.Read();

        Assert.Equal("CURRENT", read.ConfigurationStatus);
        Assert.False(copy.BuildOrStandaloneExecuted);
    }
}
