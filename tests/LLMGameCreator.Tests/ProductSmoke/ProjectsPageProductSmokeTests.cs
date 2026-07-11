using System.Text.RegularExpressions;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class ProjectsPageProductSmokeTests
{
    [Fact]
    public void ProjectsPage_normal_workspace_has_friendly_sections_exact_primary_action_and_no_goal_number_labels()
    {
        var root = FindRepositoryRoot();
        var main = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages", "Projects", "ProjectsPageControl.cs"));
        var designer = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages", "Projects", "ProjectsPageControl.Designer.cs"));
        var source = main + Environment.NewLine + designer;
        Assert.Contains("Обзор", source, StringComparison.Ordinal);
        Assert.Contains("Механики", source, StringComparison.Ordinal);
        Assert.Contains("Настройки", source, StringComparison.Ordinal);
        Assert.Contains("Сборка и проверка", source, StringComparison.Ordinal);
        Assert.Contains("Технические детали", source, StringComparison.Ordinal);
        Assert.Contains("Собрать и проверить игру", source, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"\bGoal\d+\b", RegexOptions.CultureInvariant), source);
        Assert.DoesNotContain("Process.Start", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
