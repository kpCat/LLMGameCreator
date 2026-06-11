using System.ComponentModel;

namespace LLMGameCreator.WinForms;

public sealed partial class MainForm
{
    private IContainer? components;
    private ListBox _navigation = null!;
    private Panel _workspace = null!;
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _statusLabel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new Container();
        _navigation = new ListBox();
        _workspace = new Panel();
        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel();
        _statusStrip.SuspendLayout();
        SuspendLayout();

        _navigation.Dock = DockStyle.Left;
        _navigation.Width = 220;
        _navigation.DisplayMember = "Title";

        _workspace.Dock = DockStyle.Fill;
        _workspace.BackColor = SystemColors.Control;

        _statusStrip.Dock = DockStyle.Bottom;
        _statusStrip.Items.AddRange(new ToolStripItem[]
        {
            _statusLabel
        });

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 800);
        Controls.Add(_workspace);
        Controls.Add(_navigation);
        Controls.Add(_statusStrip);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "LLMGameCreator";

        _statusStrip.ResumeLayout(false);
        _statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
