#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class RuntimeSimulatorPageControl
    {
        private IContainer components;
        private SplitContainer _rootSplitContainer;
        private SplitContainer _rightSplitContainer;
        private Panel _commandPanel;
        private FlowLayoutPanel _topToolbarPanel;
        private Button _initializeButton;
        private Button _refreshButton;
        private Label _recipeLabel;
        private ComboBox _recipeComboBox;
        private Button _craftButton;
        private Label _lootLabel;
        private ComboBox _lootComboBox;
        private Label _seedLabel;
        private TextBox _seedTextBox;
        private Button _rollLootButton;
        private Label _transactionLabel;
        private ComboBox _transactionComboBox;
        private Button _transactionButton;
        private Label _ticksLabel;
        private NumericUpDown _ticksNumericUpDown;
        private Button _tickButton;
        private TextBox _stateTextBox;
        private TextBox _eventsTextBox;

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
            this._commandPanel = new Panel();
            this._topToolbarPanel = new FlowLayoutPanel();
            this._initializeButton = new Button();
            this._refreshButton = new Button();
            this._recipeLabel = new Label();
            this._recipeComboBox = new ComboBox();
            this._craftButton = new Button();
            this._lootLabel = new Label();
            this._lootComboBox = new ComboBox();
            this._seedLabel = new Label();
            this._seedTextBox = new TextBox();
            this._rollLootButton = new Button();
            this._transactionLabel = new Label();
            this._transactionComboBox = new ComboBox();
            this._transactionButton = new Button();
            this._ticksLabel = new Label();
            this._ticksNumericUpDown = new NumericUpDown();
            this._tickButton = new Button();
            this._rightSplitContainer = new SplitContainer();
            this._stateTextBox = new TextBox();
            this._eventsTextBox = new TextBox();
            ((ISupportInitialize)this._rootSplitContainer).BeginInit();
            this._rootSplitContainer.Panel1.SuspendLayout();
            this._rootSplitContainer.Panel2.SuspendLayout();
            this._rootSplitContainer.SuspendLayout();
            this._commandPanel.SuspendLayout();
            this._topToolbarPanel.SuspendLayout();
            ((ISupportInitialize)this._ticksNumericUpDown).BeginInit();
            ((ISupportInitialize)this._rightSplitContainer).BeginInit();
            this._rightSplitContainer.Panel1.SuspendLayout();
            this._rightSplitContainer.Panel2.SuspendLayout();
            this._rightSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootSplitContainer
            // 
            this._rootSplitContainer.Dock = DockStyle.Fill;
            this._rootSplitContainer.Location = new Point(0, 0);
            this._rootSplitContainer.Name = "_rootSplitContainer";
            this._rootSplitContainer.Panel1.Controls.Add(this._commandPanel);
            this._rootSplitContainer.Panel2.Controls.Add(this._rightSplitContainer);
            this._rootSplitContainer.Size = new Size(1100, 620);
            this._rootSplitContainer.SplitterDistance = 330;
            this._rootSplitContainer.TabIndex = 0;
            // 
            // _commandPanel
            // 
            this._commandPanel.Controls.Add(this._topToolbarPanel);
            this._commandPanel.Dock = DockStyle.Fill;
            this._commandPanel.Location = new Point(0, 0);
            this._commandPanel.Name = "_commandPanel";
            this._commandPanel.Padding = new Padding(12);
            this._commandPanel.Size = new Size(330, 620);
            this._commandPanel.TabIndex = 0;
            // 
            // _topToolbarPanel
            // 
            this._topToolbarPanel.Controls.Add(this._initializeButton);
            this._topToolbarPanel.Controls.Add(this._refreshButton);
            this._topToolbarPanel.Controls.Add(this._recipeLabel);
            this._topToolbarPanel.Controls.Add(this._recipeComboBox);
            this._topToolbarPanel.Controls.Add(this._craftButton);
            this._topToolbarPanel.Controls.Add(this._lootLabel);
            this._topToolbarPanel.Controls.Add(this._lootComboBox);
            this._topToolbarPanel.Controls.Add(this._seedLabel);
            this._topToolbarPanel.Controls.Add(this._seedTextBox);
            this._topToolbarPanel.Controls.Add(this._rollLootButton);
            this._topToolbarPanel.Controls.Add(this._transactionLabel);
            this._topToolbarPanel.Controls.Add(this._transactionComboBox);
            this._topToolbarPanel.Controls.Add(this._transactionButton);
            this._topToolbarPanel.Controls.Add(this._ticksLabel);
            this._topToolbarPanel.Controls.Add(this._ticksNumericUpDown);
            this._topToolbarPanel.Controls.Add(this._tickButton);
            this._topToolbarPanel.Dock = DockStyle.Top;
            this._topToolbarPanel.FlowDirection = FlowDirection.TopDown;
            this._topToolbarPanel.Location = new Point(12, 12);
            this._topToolbarPanel.Name = "_topToolbarPanel";
            this._topToolbarPanel.Size = new Size(306, 520);
            this._topToolbarPanel.TabIndex = 0;
            this._topToolbarPanel.WrapContents = false;
            // 
            // _initializeButton
            // 
            this._initializeButton.Location = new Point(3, 3);
            this._initializeButton.Name = "_initializeButton";
            this._initializeButton.Size = new Size(220, 30);
            this._initializeButton.TabIndex = 0;
            this._initializeButton.Text = "Initialize Runtime State";
            this._initializeButton.UseVisualStyleBackColor = true;
            // 
            // _refreshButton
            // 
            this._refreshButton.Location = new Point(3, 39);
            this._refreshButton.Name = "_refreshButton";
            this._refreshButton.Size = new Size(220, 30);
            this._refreshButton.TabIndex = 1;
            this._refreshButton.Text = "Refresh Definitions";
            this._refreshButton.UseVisualStyleBackColor = true;
            // 
            // _recipeLabel
            // 
            this._recipeLabel.AutoSize = true;
            this._recipeLabel.Location = new Point(3, 84);
            this._recipeLabel.Margin = new Padding(3, 12, 3, 0);
            this._recipeLabel.Name = "_recipeLabel";
            this._recipeLabel.Size = new Size(42, 15);
            this._recipeLabel.TabIndex = 2;
            this._recipeLabel.Text = "Recipe";
            // 
            // _recipeComboBox
            // 
            this._recipeComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._recipeComboBox.FormattingEnabled = true;
            this._recipeComboBox.Location = new Point(3, 102);
            this._recipeComboBox.Name = "_recipeComboBox";
            this._recipeComboBox.Size = new Size(290, 23);
            this._recipeComboBox.TabIndex = 3;
            // 
            // _craftButton
            // 
            this._craftButton.Location = new Point(3, 131);
            this._craftButton.Name = "_craftButton";
            this._craftButton.Size = new Size(220, 30);
            this._craftButton.TabIndex = 4;
            this._craftButton.Text = "Craft Recipe";
            this._craftButton.UseVisualStyleBackColor = true;
            // 
            // _lootLabel
            // 
            this._lootLabel.AutoSize = true;
            this._lootLabel.Location = new Point(3, 176);
            this._lootLabel.Margin = new Padding(3, 12, 3, 0);
            this._lootLabel.Name = "_lootLabel";
            this._lootLabel.Size = new Size(62, 15);
            this._lootLabel.TabIndex = 5;
            this._lootLabel.Text = "Loot Table";
            // 
            // _lootComboBox
            // 
            this._lootComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._lootComboBox.FormattingEnabled = true;
            this._lootComboBox.Location = new Point(3, 194);
            this._lootComboBox.Name = "_lootComboBox";
            this._lootComboBox.Size = new Size(290, 23);
            this._lootComboBox.TabIndex = 6;
            // 
            // _seedLabel
            // 
            this._seedLabel.AutoSize = true;
            this._seedLabel.Location = new Point(3, 220);
            this._seedLabel.Name = "_seedLabel";
            this._seedLabel.Size = new Size(32, 15);
            this._seedLabel.TabIndex = 7;
            this._seedLabel.Text = "Seed";
            // 
            // _seedTextBox
            // 
            this._seedTextBox.Location = new Point(3, 238);
            this._seedTextBox.Name = "_seedTextBox";
            this._seedTextBox.Size = new Size(120, 23);
            this._seedTextBox.TabIndex = 8;
            this._seedTextBox.Text = "123";
            // 
            // _rollLootButton
            // 
            this._rollLootButton.Location = new Point(3, 267);
            this._rollLootButton.Name = "_rollLootButton";
            this._rollLootButton.Size = new Size(220, 30);
            this._rollLootButton.TabIndex = 9;
            this._rollLootButton.Text = "Roll Loot";
            this._rollLootButton.UseVisualStyleBackColor = true;
            // 
            // _transactionLabel
            // 
            this._transactionLabel.AutoSize = true;
            this._transactionLabel.Location = new Point(3, 312);
            this._transactionLabel.Margin = new Padding(3, 12, 3, 0);
            this._transactionLabel.Name = "_transactionLabel";
            this._transactionLabel.Size = new Size(68, 15);
            this._transactionLabel.TabIndex = 10;
            this._transactionLabel.Text = "Transaction";
            // 
            // _transactionComboBox
            // 
            this._transactionComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._transactionComboBox.FormattingEnabled = true;
            this._transactionComboBox.Location = new Point(3, 330);
            this._transactionComboBox.Name = "_transactionComboBox";
            this._transactionComboBox.Size = new Size(290, 23);
            this._transactionComboBox.TabIndex = 11;
            // 
            // _transactionButton
            // 
            this._transactionButton.Location = new Point(3, 359);
            this._transactionButton.Name = "_transactionButton";
            this._transactionButton.Size = new Size(220, 30);
            this._transactionButton.TabIndex = 12;
            this._transactionButton.Text = "Execute Transaction";
            this._transactionButton.UseVisualStyleBackColor = true;
            // 
            // _ticksLabel
            // 
            this._ticksLabel.AutoSize = true;
            this._ticksLabel.Location = new Point(3, 404);
            this._ticksLabel.Margin = new Padding(3, 12, 3, 0);
            this._ticksLabel.Name = "_ticksLabel";
            this._ticksLabel.Size = new Size(33, 15);
            this._ticksLabel.TabIndex = 13;
            this._ticksLabel.Text = "Ticks";
            // 
            // _ticksNumericUpDown
            // 
            this._ticksNumericUpDown.Location = new Point(3, 422);
            this._ticksNumericUpDown.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this._ticksNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this._ticksNumericUpDown.Name = "_ticksNumericUpDown";
            this._ticksNumericUpDown.Size = new Size(90, 23);
            this._ticksNumericUpDown.TabIndex = 14;
            this._ticksNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // _tickButton
            // 
            this._tickButton.Location = new Point(3, 451);
            this._tickButton.Name = "_tickButton";
            this._tickButton.Size = new Size(220, 30);
            this._tickButton.TabIndex = 15;
            this._tickButton.Text = "Tick Resource Nodes";
            this._tickButton.UseVisualStyleBackColor = true;
            // 
            // _rightSplitContainer
            // 
            this._rightSplitContainer.Dock = DockStyle.Fill;
            this._rightSplitContainer.Location = new Point(0, 0);
            this._rightSplitContainer.Name = "_rightSplitContainer";
            this._rightSplitContainer.Orientation = Orientation.Horizontal;
            this._rightSplitContainer.Panel1.Controls.Add(this._stateTextBox);
            this._rightSplitContainer.Panel2.Controls.Add(this._eventsTextBox);
            this._rightSplitContainer.Size = new Size(766, 620);
            this._rightSplitContainer.SplitterDistance = 400;
            this._rightSplitContainer.TabIndex = 0;
            // 
            // _stateTextBox
            // 
            this._stateTextBox.Dock = DockStyle.Fill;
            this._stateTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this._stateTextBox.Location = new Point(0, 0);
            this._stateTextBox.Multiline = true;
            this._stateTextBox.Name = "_stateTextBox";
            this._stateTextBox.ReadOnly = true;
            this._stateTextBox.ScrollBars = ScrollBars.Both;
            this._stateTextBox.Size = new Size(766, 400);
            this._stateTextBox.TabIndex = 0;
            this._stateTextBox.WordWrap = false;
            // 
            // _eventsTextBox
            // 
            this._eventsTextBox.Dock = DockStyle.Fill;
            this._eventsTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this._eventsTextBox.Location = new Point(0, 0);
            this._eventsTextBox.Multiline = true;
            this._eventsTextBox.Name = "_eventsTextBox";
            this._eventsTextBox.ReadOnly = true;
            this._eventsTextBox.ScrollBars = ScrollBars.Both;
            this._eventsTextBox.Size = new Size(766, 216);
            this._eventsTextBox.TabIndex = 0;
            this._eventsTextBox.WordWrap = false;
            // 
            // RuntimeSimulatorPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootSplitContainer);
            this.Name = "RuntimeSimulatorPageControl";
            this.Size = new Size(1100, 620);
            this._rootSplitContainer.Panel1.ResumeLayout(false);
            this._rootSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)this._rootSplitContainer).EndInit();
            this._rootSplitContainer.ResumeLayout(false);
            this._commandPanel.ResumeLayout(false);
            this._topToolbarPanel.ResumeLayout(false);
            this._topToolbarPanel.PerformLayout();
            ((ISupportInitialize)this._ticksNumericUpDown).EndInit();
            this._rightSplitContainer.Panel1.ResumeLayout(false);
            this._rightSplitContainer.Panel1.PerformLayout();
            this._rightSplitContainer.Panel2.ResumeLayout(false);
            this._rightSplitContainer.Panel2.PerformLayout();
            ((ISupportInitialize)this._rightSplitContainer).EndInit();
            this._rightSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
