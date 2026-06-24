#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class RuntimePreviewPageControl
    {
        private IContainer components;
        private SplitContainer _rootSplitContainer;
        private Panel _leftPanel;
        private FlowLayoutPanel _toolbarPanel;
        private Button _generatePreviewButton;
        private Button _startButton;
        private RuntimeMapCanvas _canvas;
        private TabControl _rightTabControl;
        private TabPage _logTabPage;
        private TabPage _generatedContentTabPage;
        private TextBox _logTextBox;
        private TabControl _generatedContentInnerTabControl;
        private TabPage _generatedBrowserTabPage;
        private TabPage _generatedSummaryTabPage;
        private TabPage _questJournalTabPage;
        private TableLayoutPanel _generatedBrowserLayout;
        private ComboBox _generatedCategoryComboBox;
        private SplitContainer _generatedBrowserSplitContainer;
        private ListBox _generatedEntriesListBox;
        private TextBox _generatedDetailsTextBox;
        private FlowLayoutPanel _generatedActionsPanel;
        private Button _appendGeneratedSelectionButton;
        private Button _previewDialogueButton;
        private Button _startQuestPreviewButton;
        private Button _markNextQuestStepButton;
        private TextBox _generatedContentTextBox;
        private TextBox _questJournalTextBox;

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
            this.components = new Container();
            this._rootSplitContainer = new SplitContainer();
            this._leftPanel = new Panel();
            this._canvas = new RuntimeMapCanvas();
            this._toolbarPanel = new FlowLayoutPanel();
            this._generatePreviewButton = new Button();
            this._startButton = new Button();
            this._rightTabControl = new TabControl();
            this._logTabPage = new TabPage();
            this._generatedContentTabPage = new TabPage();
            this._logTextBox = new TextBox();
            this._generatedContentInnerTabControl = new TabControl();
            this._generatedBrowserTabPage = new TabPage();
            this._generatedSummaryTabPage = new TabPage();
            this._questJournalTabPage = new TabPage();
            this._generatedBrowserLayout = new TableLayoutPanel();
            this._generatedCategoryComboBox = new ComboBox();
            this._generatedBrowserSplitContainer = new SplitContainer();
            this._generatedEntriesListBox = new ListBox();
            this._generatedDetailsTextBox = new TextBox();
            this._generatedActionsPanel = new FlowLayoutPanel();
            this._appendGeneratedSelectionButton = new Button();
            this._previewDialogueButton = new Button();
            this._startQuestPreviewButton = new Button();
            this._markNextQuestStepButton = new Button();
            this._generatedContentTextBox = new TextBox();
            this._questJournalTextBox = new TextBox();
            ((ISupportInitialize)this._rootSplitContainer).BeginInit();
            this._rootSplitContainer.Panel1.SuspendLayout();
            this._rootSplitContainer.Panel2.SuspendLayout();
            this._rootSplitContainer.SuspendLayout();
            this._leftPanel.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            this._rightTabControl.SuspendLayout();
            this._logTabPage.SuspendLayout();
            this._generatedContentTabPage.SuspendLayout();
            this._generatedContentInnerTabControl.SuspendLayout();
            this._generatedBrowserTabPage.SuspendLayout();
            this._generatedSummaryTabPage.SuspendLayout();
            this._questJournalTabPage.SuspendLayout();
            this._generatedBrowserLayout.SuspendLayout();
            ((ISupportInitialize)this._generatedBrowserSplitContainer).BeginInit();
            this._generatedBrowserSplitContainer.Panel1.SuspendLayout();
            this._generatedBrowserSplitContainer.Panel2.SuspendLayout();
            this._generatedBrowserSplitContainer.SuspendLayout();
            this._generatedActionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootSplitContainer
            // 
            this._rootSplitContainer.Dock = DockStyle.Fill;
            this._rootSplitContainer.Location = new Point(0, 0);
            this._rootSplitContainer.Name = "_rootSplitContainer";
            this._rootSplitContainer.Panel1.Controls.Add(this._leftPanel);
            this._rootSplitContainer.Panel2.Controls.Add(this._rightTabControl);
            this._rootSplitContainer.Size = new Size(1100, 600);
            this._rootSplitContainer.TabIndex = 0;
            // 
            // _leftPanel
            // 
            this._leftPanel.Controls.Add(this._canvas);
            this._leftPanel.Controls.Add(this._toolbarPanel);
            this._leftPanel.Dock = DockStyle.Fill;
            this._leftPanel.Location = new Point(0, 0);
            this._leftPanel.Name = "_leftPanel";
            this._leftPanel.Padding = new Padding(12);
            this._leftPanel.Size = new Size(820, 600);
            this._leftPanel.TabIndex = 0;
            // 
            // _canvas
            // 
            this._canvas.BackColor = Color.Black;
            this._canvas.Dock = DockStyle.Fill;
            this._canvas.Location = new Point(12, 54);
            this._canvas.Name = "_canvas";
            this._canvas.Size = new Size(796, 534);
            this._canvas.TabIndex = 1;
            this._canvas.TabStop = true;
            // 
            // _toolbarPanel
            // 
            this._toolbarPanel.Controls.Add(this._generatePreviewButton);
            this._toolbarPanel.Controls.Add(this._startButton);
            this._toolbarPanel.Dock = DockStyle.Top;
            this._toolbarPanel.Location = new Point(12, 12);
            this._toolbarPanel.Name = "_toolbarPanel";
            this._toolbarPanel.Size = new Size(796, 42);
            this._toolbarPanel.TabIndex = 0;
            // 
            // _generatePreviewButton
            // 
            this._generatePreviewButton.Location = new Point(3, 3);
            this._generatePreviewButton.Name = "_generatePreviewButton";
            this._generatePreviewButton.Size = new Size(132, 30);
            this._generatePreviewButton.TabIndex = 0;
            this._generatePreviewButton.Text = "Generate Preview";
            this._generatePreviewButton.UseVisualStyleBackColor = true;
            // 
            // _startButton
            // 
            this._startButton.Location = new Point(141, 3);
            this._startButton.Name = "_startButton";
            this._startButton.Size = new Size(100, 30);
            this._startButton.TabIndex = 1;
            this._startButton.Text = "Старт";
            this._startButton.UseVisualStyleBackColor = true;
            // 
            // _rightTabControl
            // 
            this._rightTabControl.Controls.Add(this._logTabPage);
            this._rightTabControl.Controls.Add(this._generatedContentTabPage);
            this._rightTabControl.Dock = DockStyle.Fill;
            this._rightTabControl.Location = new Point(0, 0);
            this._rightTabControl.Name = "_rightTabControl";
            this._rightTabControl.SelectedIndex = 0;
            this._rightTabControl.Size = new Size(550, 600);
            this._rightTabControl.TabIndex = 0;
            // 
            // _logTabPage
            // 
            this._logTabPage.Controls.Add(this._logTextBox);
            this._logTabPage.Location = new Point(4, 24);
            this._logTabPage.Name = "_logTabPage";
            this._logTabPage.Padding = new Padding(3);
            this._logTabPage.Size = new Size(542, 572);
            this._logTabPage.TabIndex = 0;
            this._logTabPage.Text = "Log";
            this._logTabPage.UseVisualStyleBackColor = true;
            // 
            // _generatedContentTabPage
            // 
            this._generatedContentTabPage.Controls.Add(this._generatedContentInnerTabControl);
            this._generatedContentTabPage.Location = new Point(4, 24);
            this._generatedContentTabPage.Name = "_generatedContentTabPage";
            this._generatedContentTabPage.Padding = new Padding(3);
            this._generatedContentTabPage.Size = new Size(542, 572);
            this._generatedContentTabPage.TabIndex = 1;
            this._generatedContentTabPage.Text = "Generated Content";
            this._generatedContentTabPage.UseVisualStyleBackColor = true;
            // 
            // _logTextBox
            // 
            this._logTextBox.Dock = DockStyle.Fill;
            this._logTextBox.Location = new Point(3, 3);
            this._logTextBox.Multiline = true;
            this._logTextBox.Name = "_logTextBox";
            this._logTextBox.ReadOnly = true;
            this._logTextBox.ScrollBars = ScrollBars.Vertical;
            this._logTextBox.Size = new Size(536, 566);
            this._logTextBox.TabIndex = 0;
            // 
            // _generatedContentInnerTabControl
            // 
            this._generatedContentInnerTabControl.Controls.Add(this._generatedBrowserTabPage);
            this._generatedContentInnerTabControl.Controls.Add(this._generatedSummaryTabPage);
            this._generatedContentInnerTabControl.Controls.Add(this._questJournalTabPage);
            this._generatedContentInnerTabControl.Dock = DockStyle.Fill;
            this._generatedContentInnerTabControl.Location = new Point(3, 3);
            this._generatedContentInnerTabControl.Name = "_generatedContentInnerTabControl";
            this._generatedContentInnerTabControl.SelectedIndex = 0;
            this._generatedContentInnerTabControl.Size = new Size(536, 566);
            this._generatedContentInnerTabControl.TabIndex = 0;
            // 
            // _generatedBrowserTabPage
            // 
            this._generatedBrowserTabPage.Controls.Add(this._generatedBrowserLayout);
            this._generatedBrowserTabPage.Location = new Point(4, 24);
            this._generatedBrowserTabPage.Name = "_generatedBrowserTabPage";
            this._generatedBrowserTabPage.Padding = new Padding(3);
            this._generatedBrowserTabPage.Size = new Size(528, 538);
            this._generatedBrowserTabPage.TabIndex = 0;
            this._generatedBrowserTabPage.Text = "Browser";
            this._generatedBrowserTabPage.UseVisualStyleBackColor = true;
            // 
            // _generatedSummaryTabPage
            // 
            this._generatedSummaryTabPage.Controls.Add(this._generatedContentTextBox);
            this._generatedSummaryTabPage.Location = new Point(4, 24);
            this._generatedSummaryTabPage.Name = "_generatedSummaryTabPage";
            this._generatedSummaryTabPage.Padding = new Padding(3);
            this._generatedSummaryTabPage.Size = new Size(528, 538);
            this._generatedSummaryTabPage.TabIndex = 1;
            this._generatedSummaryTabPage.Text = "Summary";
            this._generatedSummaryTabPage.UseVisualStyleBackColor = true;
            // 
            // _questJournalTabPage
            // 
            this._questJournalTabPage.Controls.Add(this._questJournalTextBox);
            this._questJournalTabPage.Location = new Point(4, 24);
            this._questJournalTabPage.Name = "_questJournalTabPage";
            this._questJournalTabPage.Padding = new Padding(3);
            this._questJournalTabPage.Size = new Size(528, 538);
            this._questJournalTabPage.TabIndex = 2;
            this._questJournalTabPage.Text = "Quest Journal";
            this._questJournalTabPage.UseVisualStyleBackColor = true;
            // 
            // _generatedBrowserLayout
            // 
            this._generatedBrowserLayout.ColumnCount = 1;
            this._generatedBrowserLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._generatedBrowserLayout.Controls.Add(this._generatedCategoryComboBox, 0, 0);
            this._generatedBrowserLayout.Controls.Add(this._generatedBrowserSplitContainer, 0, 1);
            this._generatedBrowserLayout.Controls.Add(this._generatedActionsPanel, 0, 2);
            this._generatedBrowserLayout.Dock = DockStyle.Fill;
            this._generatedBrowserLayout.Location = new Point(3, 3);
            this._generatedBrowserLayout.Name = "_generatedBrowserLayout";
            this._generatedBrowserLayout.RowCount = 3;
            this._generatedBrowserLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._generatedBrowserLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._generatedBrowserLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            this._generatedBrowserLayout.Size = new Size(522, 532);
            this._generatedBrowserLayout.TabIndex = 0;
            // 
            // _generatedCategoryComboBox
            // 
            this._generatedCategoryComboBox.Dock = DockStyle.Fill;
            this._generatedCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._generatedCategoryComboBox.FormattingEnabled = true;
            this._generatedCategoryComboBox.Location = new Point(3, 3);
            this._generatedCategoryComboBox.Name = "_generatedCategoryComboBox";
            this._generatedCategoryComboBox.Size = new Size(516, 23);
            this._generatedCategoryComboBox.TabIndex = 0;
            // 
            // _generatedBrowserSplitContainer
            // 
            this._generatedBrowserSplitContainer.Dock = DockStyle.Fill;
            this._generatedBrowserSplitContainer.Location = new Point(3, 37);
            this._generatedBrowserSplitContainer.Name = "_generatedBrowserSplitContainer";
            this._generatedBrowserSplitContainer.Panel1.Controls.Add(this._generatedEntriesListBox);
            this._generatedBrowserSplitContainer.Panel2.Controls.Add(this._generatedDetailsTextBox);
            this._generatedBrowserSplitContainer.Size = new Size(516, 452);
            this._generatedBrowserSplitContainer.SplitterDistance = 172;
            this._generatedBrowserSplitContainer.TabIndex = 1;
            // 
            // _generatedEntriesListBox
            // 
            this._generatedEntriesListBox.Dock = DockStyle.Fill;
            this._generatedEntriesListBox.FormattingEnabled = true;
            this._generatedEntriesListBox.ItemHeight = 15;
            this._generatedEntriesListBox.Location = new Point(0, 0);
            this._generatedEntriesListBox.Name = "_generatedEntriesListBox";
            this._generatedEntriesListBox.Size = new Size(172, 452);
            this._generatedEntriesListBox.TabIndex = 0;
            // 
            // _generatedDetailsTextBox
            // 
            this._generatedDetailsTextBox.Dock = DockStyle.Fill;
            this._generatedDetailsTextBox.Location = new Point(0, 0);
            this._generatedDetailsTextBox.Multiline = true;
            this._generatedDetailsTextBox.Name = "_generatedDetailsTextBox";
            this._generatedDetailsTextBox.ReadOnly = true;
            this._generatedDetailsTextBox.ScrollBars = ScrollBars.Vertical;
            this._generatedDetailsTextBox.Size = new Size(340, 452);
            this._generatedDetailsTextBox.TabIndex = 0;
            // 
            // _generatedActionsPanel
            // 
            this._generatedActionsPanel.Controls.Add(this._appendGeneratedSelectionButton);
            this._generatedActionsPanel.Controls.Add(this._previewDialogueButton);
            this._generatedActionsPanel.Controls.Add(this._startQuestPreviewButton);
            this._generatedActionsPanel.Controls.Add(this._markNextQuestStepButton);
            this._generatedActionsPanel.Dock = DockStyle.Fill;
            this._generatedActionsPanel.Location = new Point(3, 495);
            this._generatedActionsPanel.Name = "_generatedActionsPanel";
            this._generatedActionsPanel.Size = new Size(516, 34);
            this._generatedActionsPanel.TabIndex = 2;
            // 
            // _appendGeneratedSelectionButton
            // 
            this._appendGeneratedSelectionButton.Enabled = false;
            this._appendGeneratedSelectionButton.Location = new Point(3, 3);
            this._appendGeneratedSelectionButton.Name = "_appendGeneratedSelectionButton";
            this._appendGeneratedSelectionButton.Size = new Size(126, 28);
            this._appendGeneratedSelectionButton.TabIndex = 0;
            this._appendGeneratedSelectionButton.Text = "Append selected to log";
            this._appendGeneratedSelectionButton.UseVisualStyleBackColor = true;
            // 
            // _previewDialogueButton
            // 
            this._previewDialogueButton.Enabled = false;
            this._previewDialogueButton.Location = new Point(135, 3);
            this._previewDialogueButton.Name = "_previewDialogueButton";
            this._previewDialogueButton.Size = new Size(110, 28);
            this._previewDialogueButton.TabIndex = 1;
            this._previewDialogueButton.Text = "Preview dialogue";
            this._previewDialogueButton.UseVisualStyleBackColor = true;
            // 
            // _startQuestPreviewButton
            // 
            this._startQuestPreviewButton.Enabled = false;
            this._startQuestPreviewButton.Location = new Point(251, 3);
            this._startQuestPreviewButton.Name = "_startQuestPreviewButton";
            this._startQuestPreviewButton.Size = new Size(110, 28);
            this._startQuestPreviewButton.TabIndex = 2;
            this._startQuestPreviewButton.Text = "Start quest preview";
            this._startQuestPreviewButton.UseVisualStyleBackColor = true;
            // 
            // _markNextQuestStepButton
            // 
            this._markNextQuestStepButton.Enabled = false;
            this._markNextQuestStepButton.Location = new Point(367, 3);
            this._markNextQuestStepButton.Name = "_markNextQuestStepButton";
            this._markNextQuestStepButton.Size = new Size(120, 28);
            this._markNextQuestStepButton.TabIndex = 3;
            this._markNextQuestStepButton.Text = "Mark next quest step";
            this._markNextQuestStepButton.UseVisualStyleBackColor = true;
            // 
            // _generatedContentTextBox
            // 
            this._generatedContentTextBox.Dock = DockStyle.Fill;
            this._generatedContentTextBox.Location = new Point(3, 3);
            this._generatedContentTextBox.Multiline = true;
            this._generatedContentTextBox.Name = "_generatedContentTextBox";
            this._generatedContentTextBox.ReadOnly = true;
            this._generatedContentTextBox.ScrollBars = ScrollBars.Vertical;
            this._generatedContentTextBox.Size = new Size(522, 532);
            this._generatedContentTextBox.TabIndex = 0;
            // 
            // _questJournalTextBox
            // 
            this._questJournalTextBox.Dock = DockStyle.Fill;
            this._questJournalTextBox.Location = new Point(3, 3);
            this._questJournalTextBox.Multiline = true;
            this._questJournalTextBox.Name = "_questJournalTextBox";
            this._questJournalTextBox.ReadOnly = true;
            this._questJournalTextBox.ScrollBars = ScrollBars.Vertical;
            this._questJournalTextBox.Size = new Size(522, 532);
            this._questJournalTextBox.TabIndex = 0;
            // 
            // RuntimePreviewPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootSplitContainer);
            this.Name = "RuntimePreviewPageControl";
            this.Size = new Size(1100, 600);
            this._rootSplitContainer.Panel1.ResumeLayout(false);
            this._rootSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)this._rootSplitContainer).EndInit();
            this._rootSplitContainer.ResumeLayout(false);
            this._leftPanel.ResumeLayout(false);
            this._toolbarPanel.ResumeLayout(false);
            this._rightTabControl.ResumeLayout(false);
            this._logTabPage.ResumeLayout(false);
            this._logTabPage.PerformLayout();
            this._generatedContentTabPage.ResumeLayout(false);
            this._generatedContentInnerTabControl.ResumeLayout(false);
            this._generatedBrowserTabPage.ResumeLayout(false);
            this._generatedSummaryTabPage.ResumeLayout(false);
            this._generatedSummaryTabPage.PerformLayout();
            this._questJournalTabPage.ResumeLayout(false);
            this._questJournalTabPage.PerformLayout();
            this._generatedBrowserLayout.ResumeLayout(false);
            this._generatedBrowserLayout.PerformLayout();
            this._generatedBrowserSplitContainer.Panel1.ResumeLayout(false);
            this._generatedBrowserSplitContainer.Panel2.ResumeLayout(false);
            this._generatedBrowserSplitContainer.Panel2.PerformLayout();
            ((ISupportInitialize)this._generatedBrowserSplitContainer).EndInit();
            this._generatedBrowserSplitContainer.ResumeLayout(false);
            this._generatedActionsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
