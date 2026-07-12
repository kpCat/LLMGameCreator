using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class Goal152AProjectsPageReviewSurfaceTests
{
    [Fact]
    public void Green_standalone_review_card_is_concise_and_does_not_require_hash_comparison()
    {
        var root = FindRoot();
        var source = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages", "Projects", "ProjectsPageControl.cs"));
        Assert.Contains("Автоматическая проверка: ПРОЙДЕНА", source, StringComparison.Ordinal);
        Assert.Contains("Payload integrity: GREEN", source, StringComparison.Ordinal);
        Assert.Contains("Runtime authority: GREEN", source, StringComparison.Ordinal);
        Assert.Contains("Navigation self-check: GREEN", source, StringComparison.Ordinal);
        Assert.Contains("Нажмите Далее, Назад, В конец и Сбросить", source, StringComparison.Ordinal);
        Assert.Contains("без наложения", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Package SHA-256: \" + result.PackageSha256", source, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
