namespace LLMGameCreator.WinForms.Pages;

public sealed partial class AssetsPageControl
{
    private ListView _listView = null!;
    private ColumnHeader _idColumnHeader = null!;
    private ColumnHeader _typeColumnHeader = null!;
    private ColumnHeader _pathColumnHeader = null!;
    private ColumnHeader _contractColumnHeader = null!;

    private void InitializeComponent()
    {
        _listView = new ListView();
        _idColumnHeader = new ColumnHeader();
        _typeColumnHeader = new ColumnHeader();
        _pathColumnHeader = new ColumnHeader();
        _contractColumnHeader = new ColumnHeader();
        SuspendLayout();

        _listView.Columns.AddRange(new[]
        {
            _idColumnHeader,
            _typeColumnHeader,
            _pathColumnHeader,
            _contractColumnHeader
        });
        _listView.Dock = DockStyle.Fill;
        _listView.FullRowSelect = true;
        _listView.View = System.Windows.Forms.View.Details;

        _idColumnHeader.Text = "Id";
        _idColumnHeader.Width = 280;

        _typeColumnHeader.Text = "Type";
        _typeColumnHeader.Width = 160;

        _pathColumnHeader.Text = "Path";
        _pathColumnHeader.Width = 380;

        _contractColumnHeader.Text = "Contract";
        _contractColumnHeader.Width = 180;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(_listView);
        Name = "AssetsPageControl";
        Size = new Size(800, 450);

        ResumeLayout(false);
    }
}
