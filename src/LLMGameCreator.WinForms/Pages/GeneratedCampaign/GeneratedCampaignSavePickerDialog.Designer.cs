namespace LLMGameCreator.WinForms.Pages;

partial class GeneratedCampaignSavePickerDialog
{
    private System.ComponentModel.IContainer components;
    private TableLayoutPanel _layout = null!;
    private ListView _list = null!;
    private ColumnHeader _slotColumn = null!;
    private ColumnHeader _statusColumn = null!;
    private ColumnHeader _revisionsColumn = null!;
    private Label _details = null!;
    private FlowLayoutPanel _buttons = null!;
    private Button _continue = null!;
    private Button _migrate = null!;
    private Button _cancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _layout = new TableLayoutPanel();
        _list = new ListView();
        _slotColumn = new ColumnHeader();
        _statusColumn = new ColumnHeader();
        _revisionsColumn = new ColumnHeader();
        _details = new Label();
        _buttons = new FlowLayoutPanel();
        _continue = new Button();
        _migrate = new Button();
        _cancel = new Button();
        _layout.SuspendLayout();
        _buttons.SuspendLayout();
        SuspendLayout();
        //
        // _layout
        //
        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.Controls.Add(_list, 0, 0);
        _layout.Controls.Add(_details, 0, 1);
        _layout.Controls.Add(_buttons, 0, 2);
        _layout.Dock = DockStyle.Fill;
        _layout.Padding = new Padding(10);
        _layout.RowCount = 3;
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        //
        // _list
        //
        _list.Columns.AddRange(new[] { _slotColumn, _statusColumn, _revisionsColumn });
        _list.Dock = DockStyle.Fill;
        _list.FullRowSelect = true;
        _list.HideSelection = false;
        _list.MultiSelect = false;
        _list.View = View.Details;
        //
        // columns
        //
        _slotColumn.Text = "Сохранение";
        _slotColumn.Width = 160;
        _statusColumn.Text = "Состояние";
        _statusColumn.Width = 310;
        _revisionsColumn.Text = "Ревизии";
        _revisionsColumn.Width = 80;
        //
        // _details
        //
        _details.AutoEllipsis = true;
        _details.Dock = DockStyle.Fill;
        _details.Padding = new Padding(3, 8, 3, 3);
        _details.Text = "Выберите сохранение.";
        //
        // _buttons
        //
        _buttons.AutoSize = true;
        _buttons.Controls.Add(_continue);
        _buttons.Controls.Add(_migrate);
        _buttons.Controls.Add(_cancel);
        _buttons.Dock = DockStyle.Fill;
        _buttons.FlowDirection = FlowDirection.RightToLeft;
        //
        // _continue
        //
        _continue.AutoSize = true;
        _continue.Enabled = false;
        _continue.Text = "Продолжить";
        _continue.UseVisualStyleBackColor = true;
        //
        // _migrate
        //
        _migrate.AutoSize = true;
        _migrate.Enabled = false;
        _migrate.Text = "Перенести и продолжить";
        _migrate.UseVisualStyleBackColor = true;
        //
        // _cancel
        //
        _cancel.AutoSize = true;
        _cancel.DialogResult = DialogResult.Cancel;
        _cancel.Text = "Отмена";
        _cancel.UseVisualStyleBackColor = true;
        //
        // GeneratedCampaignSavePickerDialog
        //
        AcceptButton = _continue;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancel;
        ClientSize = new Size(650, 390);
        Controls.Add(_layout);
        MinimizeBox = false;
        MinimumSize = new Size(560, 340);
        Name = "GeneratedCampaignSavePickerDialog";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Сохранения кампании";
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        _buttons.ResumeLayout(false);
        _buttons.PerformLayout();
        ResumeLayout(false);
    }
}
