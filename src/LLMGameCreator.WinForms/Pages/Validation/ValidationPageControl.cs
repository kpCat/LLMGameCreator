using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class ValidationPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly IGamePackageValidator? _validator;

    public ValidationPageControl()
    {
        InitializeComponent();
    }

    public ValidationPageControl(ICurrentGamePackageService currentGamePackageService, IGamePackageValidator validator)
    {
        _currentGamePackageService = currentGamePackageService;
        _validator = validator;
        InitializeComponent();
        _validateButton.Click += (_, _) => ValidateCurrent();
    }

    public string Id => "validation";
    public string Title => "Валидация";
    public int SortOrder => 40;
    Control IEditorPage. View => this;
    public void OnActivated() { }

    private void ValidateCurrent()
    {
        _issuesListBox.Items.Clear();
        var package = _currentGamePackageService?.CurrentPackage;
        if (package == null)
        {
            _issuesListBox.Items.Add("Проект игры не открыт.");
            return;
        }

        if (_validator == null)
        {
            _issuesListBox.Items.Add("Валидатор недоступен.");
            return;
        }

        var projectFolder = _currentGamePackageService?.CurrentFolder;
        var report = _validator.Validate(package, projectFolder);
        if (report.Issues.Count == 0)
        {
            _issuesListBox.Items.Add("Ошибок не найдено.");
            return;
        }

        var formatter = new ValidationReportFormatter();
        foreach (var line in formatter.Format(report).Split(new[] { Environment.NewLine }, StringSplitOptions.None))
        {
            _issuesListBox.Items.Add(line);
        }
    }
}
