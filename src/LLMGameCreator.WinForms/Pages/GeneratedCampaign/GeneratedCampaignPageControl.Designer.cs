namespace LLMGameCreator.WinForms.Pages;

partial class GeneratedCampaignPageControl
{
    private System.ComponentModel.IContainer components;
    private TableLayoutPanel _rootLayout = null!;
    private FlowLayoutPanel _toolbar = null!;
    private Label _status = null!;
    private Button _newGame = null!;
    private Button _continue = null!;
    private Button _save = null!;
    private TextBox _slot = null!;
    private CheckBox _technicalToggle = null!;
    private TableLayoutPanel _workspace = null!;
    private TableLayoutPanel _mapLayout = null!;
    private Label _mapTitle = null!;
    private Panel _mapViewport = null!;
    private GeneratedCampaignMapControl _map = null!;
    private TableLayoutPanel _contextLayout = null!;
    private Label _contextTitle = null!;
    private Label _contextDescription = null!;
    private FlowLayoutPanel _actions = null!;
    private TabControl _hud = null!;
    private TabPage _characterTab = null!;
    private TabPage _questsTab = null!;
    private TabPage _inventoryTab = null!;
    private TabPage _consequencesTab = null!;
    private TabPage _eventsTab = null!;
    private TabPage _decisionsTab = null!;
    private TextBox _technical = null!;
    private ToolTip _actionToolTip = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _rootLayout = new TableLayoutPanel();
        _toolbar = new FlowLayoutPanel();
        _status = new Label();
        _newGame = new Button();
        _continue = new Button();
        _save = new Button();
        _slot = new TextBox();
        _technicalToggle = new CheckBox();
        _workspace = new TableLayoutPanel();
        _mapLayout = new TableLayoutPanel();
        _mapTitle = new Label();
        _mapViewport = new Panel();
        _map = new GeneratedCampaignMapControl();
        _contextLayout = new TableLayoutPanel();
        _contextTitle = new Label();
        _contextDescription = new Label();
        _actions = new FlowLayoutPanel();
        _hud = new TabControl();
        _characterTab = new TabPage();
        _questsTab = new TabPage();
        _inventoryTab = new TabPage();
        _consequencesTab = new TabPage();
        _eventsTab = new TabPage();
        _decisionsTab = new TabPage();
        _technical = new TextBox();
        _actionToolTip = new ToolTip(components);
        _rootLayout.SuspendLayout();
        _toolbar.SuspendLayout();
        _workspace.SuspendLayout();
        _mapLayout.SuspendLayout();
        _mapViewport.SuspendLayout();
        _contextLayout.SuspendLayout();
        _hud.SuspendLayout();
        SuspendLayout();
        //
        // _rootLayout
        //
        _rootLayout.ColumnCount = 1;
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.Controls.Add(_toolbar, 0, 0);
        _rootLayout.Controls.Add(_workspace, 0, 1);
        _rootLayout.Controls.Add(_technical, 0, 2);
        _rootLayout.Dock = DockStyle.Fill;
        _rootLayout.RowCount = 3;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
        //
        // _toolbar
        //
        _toolbar.Controls.Add(_status);
        _toolbar.Controls.Add(_newGame);
        _toolbar.Controls.Add(_continue);
        _toolbar.Controls.Add(_save);
        _toolbar.Controls.Add(_slot);
        _toolbar.Controls.Add(_technicalToggle);
        _toolbar.Dock = DockStyle.Fill;
        _toolbar.Padding = new Padding(6, 7, 6, 3);
        _toolbar.WrapContents = false;
        //
        // _status
        //
        _status.AutoEllipsis = true;
        _status.Margin = new Padding(3, 7, 12, 3);
        _status.Size = new Size(260, 24);
        _status.Text = "Кампания";
        //
        // _newGame
        //
        _newGame.AutoSize = true;
        _newGame.Text = "Новая игра";
        _newGame.UseVisualStyleBackColor = true;
        //
        // _continue
        //
        _continue.AutoSize = true;
        _continue.Text = "Продолжить";
        _continue.UseVisualStyleBackColor = true;
        //
        // _save
        //
        _save.AutoSize = true;
        _save.Text = "Сохранить";
        _save.UseVisualStyleBackColor = true;
        //
        // _slot
        //
        _slot.AccessibleName = "Имя сохранения";
        _slot.Margin = new Padding(6, 4, 3, 3);
        _slot.Size = new Size(120, 23);
        _slot.Text = "campaign";
        //
        // _technicalToggle
        //
        _technicalToggle.AutoSize = true;
        _technicalToggle.Margin = new Padding(12, 7, 3, 3);
        _technicalToggle.Text = "Технические сведения";
        _technicalToggle.UseVisualStyleBackColor = true;
        //
        // _workspace
        //
        _workspace.ColumnCount = 3;
        _workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37F));
        _workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31F));
        _workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
        _workspace.Controls.Add(_mapLayout, 0, 0);
        _workspace.Controls.Add(_contextLayout, 1, 0);
        _workspace.Controls.Add(_hud, 2, 0);
        _workspace.Dock = DockStyle.Fill;
        _workspace.Padding = new Padding(6);
        _workspace.RowCount = 1;
        _workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // _mapLayout
        //
        _mapLayout.ColumnCount = 1;
        _mapLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _mapLayout.Controls.Add(_mapTitle, 0, 0);
        _mapLayout.Controls.Add(_mapViewport, 0, 1);
        _mapLayout.Dock = DockStyle.Fill;
        _mapLayout.RowCount = 2;
        _mapLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _mapLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // _mapTitle
        //
        _mapTitle.AutoSize = true;
        _mapTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _mapTitle.Margin = new Padding(4, 4, 4, 8);
        _mapTitle.Text = "Карта";
        //
        // _mapViewport
        //
        _mapViewport.AutoScroll = true;
        _mapViewport.BorderStyle = BorderStyle.FixedSingle;
        _mapViewport.Controls.Add(_map);
        _mapViewport.Dock = DockStyle.Fill;
        _mapViewport.Margin = new Padding(3, 0, 8, 3);
        //
        // _map
        //
        _map.Location = new Point(0, 0);
        _map.MinimumSize = new Size(240, 240);
        _map.Size = new Size(320, 320);
        //
        // _contextLayout
        //
        _contextLayout.ColumnCount = 1;
        _contextLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _contextLayout.Controls.Add(_contextTitle, 0, 0);
        _contextLayout.Controls.Add(_contextDescription, 0, 1);
        _contextLayout.Controls.Add(_actions, 0, 2);
        _contextLayout.Dock = DockStyle.Fill;
        _contextLayout.RowCount = 3;
        _contextLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _contextLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _contextLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // _contextTitle
        //
        _contextTitle.AutoSize = true;
        _contextTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _contextTitle.Margin = new Padding(4, 4, 4, 8);
        _contextTitle.Text = "Действия";
        //
        // _contextDescription
        //
        _contextDescription.AutoSize = true;
        _contextDescription.MaximumSize = new Size(340, 0);
        _contextDescription.Margin = new Padding(4, 0, 8, 10);
        _contextDescription.Text = "Начните новую игру или продолжите сохранённую кампанию.";
        //
        // _actions
        //
        _actions.AutoScroll = true;
        _actions.Dock = DockStyle.Fill;
        _actions.FlowDirection = FlowDirection.TopDown;
        _actions.Margin = new Padding(3, 0, 8, 3);
        _actions.WrapContents = false;
        //
        // _hud
        //
        _hud.Controls.Add(_characterTab);
        _hud.Controls.Add(_questsTab);
        _hud.Controls.Add(_inventoryTab);
        _hud.Controls.Add(_consequencesTab);
        _hud.Controls.Add(_eventsTab);
        _hud.Controls.Add(_decisionsTab);
        _hud.Dock = DockStyle.Fill;
        _hud.Margin = new Padding(3, 0, 3, 3);
        //
        // tabs
        //
        _characterTab.AutoScroll = true;
        _characterTab.Text = "Персонаж";
        _characterTab.UseVisualStyleBackColor = true;
        _questsTab.AutoScroll = true;
        _questsTab.Text = "Задания";
        _questsTab.UseVisualStyleBackColor = true;
        _inventoryTab.AutoScroll = true;
        _inventoryTab.Text = "Инвентарь";
        _inventoryTab.UseVisualStyleBackColor = true;
        _consequencesTab.AutoScroll = true;
        _consequencesTab.Text = "Последствия";
        _consequencesTab.UseVisualStyleBackColor = true;
        _eventsTab.AutoScroll = true;
        _eventsTab.Text = "События";
        _eventsTab.UseVisualStyleBackColor = true;
        _decisionsTab.AutoScroll = true;
        _decisionsTab.Text = "Решения";
        _decisionsTab.UseVisualStyleBackColor = true;
        //
        // _technical
        //
        _technical.Dock = DockStyle.Fill;
        _technical.Multiline = true;
        _technical.ReadOnly = true;
        _technical.ScrollBars = ScrollBars.Vertical;
        _technical.Visible = false;
        //
        // GeneratedCampaignPageControl
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(_rootLayout);
        MinimumSize = new Size(900, 600);
        Name = "GeneratedCampaignPageControl";
        Size = new Size(1180, 720);
        _rootLayout.ResumeLayout(false);
        _rootLayout.PerformLayout();
        _toolbar.ResumeLayout(false);
        _toolbar.PerformLayout();
        _workspace.ResumeLayout(false);
        _mapLayout.ResumeLayout(false);
        _mapLayout.PerformLayout();
        _mapViewport.ResumeLayout(false);
        _contextLayout.ResumeLayout(false);
        _contextLayout.PerformLayout();
        _hud.ResumeLayout(false);
        ResumeLayout(false);
    }
}
