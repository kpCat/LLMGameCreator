using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;

namespace LLMGameCreator.WinForms.Pages;

public sealed class ValidationPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService _currentGamePackageService;
    private readonly IGamePackageValidator _validator;
    private readonly ListBox _issuesListBox = new ListBox();

    public ValidationPageControl(ICurrentGamePackageService currentGamePackageService, IGamePackageValidator validator)
    {
        _currentGamePackageService = currentGamePackageService;
        _validator = validator;
        BuildLayout();
    }

    public string Id => "validation";
    public string Title => "Валидация";
    public int SortOrder => 40;
    public Control View => this;
    public void OnActivated() { }

    private void BuildLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        var button = new Button { Text = "Проверить текущий GamePackage", Dock = DockStyle.Top, Height = 36 };
        _issuesListBox.Dock = DockStyle.Fill;
        button.Click += (_, _) => ValidateCurrent();
        panel.Controls.Add(_issuesListBox);
        panel.Controls.Add(button);
        Controls.Add(panel);
    }

    private void ValidateCurrent()
    {
        _issuesListBox.Items.Clear();
        var package = _currentGamePackageService.CurrentPackage;
        if (package == null)
        {
            _issuesListBox.Items.Add("Проект игры не открыт.");
            return;
        }

        var report = _validator.Validate(package);
        if (report.Issues.Count == 0)
        {
            _issuesListBox.Items.Add("Ошибок не найдено.");
            return;
        }

        foreach (var issue in report.Issues)
        {
            _issuesListBox.Items.Add(issue.ToString());
        }
    }
}
