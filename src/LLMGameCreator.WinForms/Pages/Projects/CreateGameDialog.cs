using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CreateGameDialog : Form
{
    private string _lastTitleDefault = string.Empty;
    private string _lastPackageIdDefault = string.Empty;

    public CreateGameDialog()
    {
        InitializeComponent();
        _versionTextBox.Text = "0.1.0";
        WireEvents();
    }

    public CreateGameProjectRequest CreateRequest(string gamesRootPath)
    {
        var folderName = _folderNameTextBox.Text.Trim();
        var title = _titleTextBox.Text.Trim();
        var packageId = _packageIdTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(packageId) && !string.IsNullOrWhiteSpace(folderName))
        {
            packageId = "game/" + NormalizePackageIdPart(folderName);
        }

        return new CreateGameProjectRequest
        {
            GamesRootPath = gamesRootPath,
            FolderName = folderName,
            Title = string.IsNullOrWhiteSpace(title) ? folderName : title,
            PackageId = packageId,
            Version = _versionTextBox.Text.Trim()
        };
    }

    private void WireEvents()
    {
        _folderNameTextBox.TextChanged += (_, _) => UpdateDefaults();
        _createButton.Click += (_, _) => Confirm();
        _cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
    }

    private void UpdateDefaults()
    {
        var folderName = _folderNameTextBox.Text.Trim();
        var titleDefault = folderName;
        var packageIdDefault = string.IsNullOrWhiteSpace(folderName)
            ? string.Empty
            : "game/" + NormalizePackageIdPart(folderName);

        if (string.IsNullOrWhiteSpace(_titleTextBox.Text) || _titleTextBox.Text == _lastTitleDefault)
        {
            _titleTextBox.Text = titleDefault;
        }

        if (string.IsNullOrWhiteSpace(_packageIdTextBox.Text) || _packageIdTextBox.Text == _lastPackageIdDefault)
        {
            _packageIdTextBox.Text = packageIdDefault;
        }

        _lastTitleDefault = titleDefault;
        _lastPackageIdDefault = packageIdDefault;
    }

    private void Confirm()
    {
        var folderName = _folderNameTextBox.Text.Trim();
        var title = _titleTextBox.Text.Trim();
        var version = _versionTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(folderName))
        {
            MessageBox.Show(this, "Folder name is required.", "New game", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show(this, "Title is required.", "New game", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            MessageBox.Show(this, "Version is required.", "New game", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult = DialogResult.OK;
    }

    private static string NormalizePackageIdPart(string value)
    {
        var chars = value.Trim().Select(ch =>
        {
            if (char.IsLetterOrDigit(ch))
            {
                return char.ToLowerInvariant(ch);
            }

            return ch == '_' || ch == '-' ? ch : '-';
        });

        return string.Concat(chars).Trim('-');
    }
}
