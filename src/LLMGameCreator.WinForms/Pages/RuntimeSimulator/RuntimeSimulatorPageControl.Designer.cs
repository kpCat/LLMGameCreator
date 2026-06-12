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
        private Button _moveUpButton;
        private Button _moveDownButton;
        private Button _moveLeftButton;
        private Button _moveRightButton;
        private Button _interactButton;
        private Label _itemLabel;
        private ComboBox _itemComboBox;
        private Button _useItemButton;
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
        private Label _interactionLabel;
        private ComboBox _interactionComboBox;
        private Button _interactionButton;
        private Label _equipmentSlotLabel;
        private ComboBox _equipmentSlotComboBox;
        private Button _equipButton;
        private Button _unequipButton;
        private Label _containerLabel;
        private ComboBox _containerComboBox;
        private Label _containerItemLabel;
        private TextBox _containerItemTextBox;
        private Label _containerAmountLabel;
        private NumericUpDown _containerAmountNumericUpDown;
        private Button _openContainerButton;
        private Button _takeContainerButton;
        private Button _depositContainerButton;
        private Label _resourceNodeLabel;
        private ComboBox _resourceNodeComboBox;
        private Label _toolItemLabel;
        private TextBox _toolItemTextBox;
        private Button _harvestButton;
        private Label _snapshotSlotLabel;
        private TextBox _snapshotSlotTextBox;
        private Button _saveSnapshotButton;
        private Button _loadSnapshotButton;
        private Button _listSnapshotsButton;
        private Label _ticksLabel;
        private NumericUpDown _ticksNumericUpDown;
        private Button _tickButton;
        private Button _waitButton;
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
            this._moveUpButton = new Button();
            this._moveDownButton = new Button();
            this._moveLeftButton = new Button();
            this._moveRightButton = new Button();
            this._interactButton = new Button();
            this._itemLabel = new Label();
            this._itemComboBox = new ComboBox();
            this._useItemButton = new Button();
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
            this._interactionLabel = new Label();
            this._interactionComboBox = new ComboBox();
            this._interactionButton = new Button();
            this._equipmentSlotLabel = new Label();
            this._equipmentSlotComboBox = new ComboBox();
            this._equipButton = new Button();
            this._unequipButton = new Button();
            this._containerLabel = new Label();
            this._containerComboBox = new ComboBox();
            this._containerItemLabel = new Label();
            this._containerItemTextBox = new TextBox();
            this._containerAmountLabel = new Label();
            this._containerAmountNumericUpDown = new NumericUpDown();
            this._openContainerButton = new Button();
            this._takeContainerButton = new Button();
            this._depositContainerButton = new Button();
            this._resourceNodeLabel = new Label();
            this._resourceNodeComboBox = new ComboBox();
            this._toolItemLabel = new Label();
            this._toolItemTextBox = new TextBox();
            this._harvestButton = new Button();
            this._snapshotSlotLabel = new Label();
            this._snapshotSlotTextBox = new TextBox();
            this._saveSnapshotButton = new Button();
            this._loadSnapshotButton = new Button();
            this._listSnapshotsButton = new Button();
            this._ticksLabel = new Label();
            this._ticksNumericUpDown = new NumericUpDown();
            this._tickButton = new Button();
            this._waitButton = new Button();
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
            ((ISupportInitialize)this._containerAmountNumericUpDown).BeginInit();
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
            this._commandPanel.AutoScroll = true;
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
            this._topToolbarPanel.AutoSize = true;
            this._topToolbarPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this._topToolbarPanel.Controls.Add(this._initializeButton);
            this._topToolbarPanel.Controls.Add(this._refreshButton);
            this._topToolbarPanel.Controls.Add(this._moveUpButton);
            this._topToolbarPanel.Controls.Add(this._moveDownButton);
            this._topToolbarPanel.Controls.Add(this._moveLeftButton);
            this._topToolbarPanel.Controls.Add(this._moveRightButton);
            this._topToolbarPanel.Controls.Add(this._interactButton);
            this._topToolbarPanel.Controls.Add(this._itemLabel);
            this._topToolbarPanel.Controls.Add(this._itemComboBox);
            this._topToolbarPanel.Controls.Add(this._useItemButton);
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
            this._topToolbarPanel.Controls.Add(this._interactionLabel);
            this._topToolbarPanel.Controls.Add(this._interactionComboBox);
            this._topToolbarPanel.Controls.Add(this._interactionButton);
            this._topToolbarPanel.Controls.Add(this._equipmentSlotLabel);
            this._topToolbarPanel.Controls.Add(this._equipmentSlotComboBox);
            this._topToolbarPanel.Controls.Add(this._equipButton);
            this._topToolbarPanel.Controls.Add(this._unequipButton);
            this._topToolbarPanel.Controls.Add(this._containerLabel);
            this._topToolbarPanel.Controls.Add(this._containerComboBox);
            this._topToolbarPanel.Controls.Add(this._containerItemLabel);
            this._topToolbarPanel.Controls.Add(this._containerItemTextBox);
            this._topToolbarPanel.Controls.Add(this._containerAmountLabel);
            this._topToolbarPanel.Controls.Add(this._containerAmountNumericUpDown);
            this._topToolbarPanel.Controls.Add(this._openContainerButton);
            this._topToolbarPanel.Controls.Add(this._takeContainerButton);
            this._topToolbarPanel.Controls.Add(this._depositContainerButton);
            this._topToolbarPanel.Controls.Add(this._resourceNodeLabel);
            this._topToolbarPanel.Controls.Add(this._resourceNodeComboBox);
            this._topToolbarPanel.Controls.Add(this._toolItemLabel);
            this._topToolbarPanel.Controls.Add(this._toolItemTextBox);
            this._topToolbarPanel.Controls.Add(this._harvestButton);
            this._topToolbarPanel.Controls.Add(this._snapshotSlotLabel);
            this._topToolbarPanel.Controls.Add(this._snapshotSlotTextBox);
            this._topToolbarPanel.Controls.Add(this._saveSnapshotButton);
            this._topToolbarPanel.Controls.Add(this._loadSnapshotButton);
            this._topToolbarPanel.Controls.Add(this._listSnapshotsButton);
            this._topToolbarPanel.Controls.Add(this._ticksLabel);
            this._topToolbarPanel.Controls.Add(this._ticksNumericUpDown);
            this._topToolbarPanel.Controls.Add(this._tickButton);
            this._topToolbarPanel.Controls.Add(this._waitButton);
            this._topToolbarPanel.Dock = DockStyle.Top;
            this._topToolbarPanel.FlowDirection = FlowDirection.TopDown;
            this._topToolbarPanel.Location = new Point(12, 12);
            this._topToolbarPanel.Name = "_topToolbarPanel";
            this._topToolbarPanel.Size = new Size(296, 886);
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
            // _moveUpButton
            // 
            this._moveUpButton.Location = new Point(3, 75);
            this._moveUpButton.Name = "_moveUpButton";
            this._moveUpButton.Size = new Size(220, 30);
            this._moveUpButton.TabIndex = 2;
            this._moveUpButton.Text = "Move Up";
            this._moveUpButton.UseVisualStyleBackColor = true;
            // 
            // _moveDownButton
            // 
            this._moveDownButton.Location = new Point(3, 111);
            this._moveDownButton.Name = "_moveDownButton";
            this._moveDownButton.Size = new Size(220, 30);
            this._moveDownButton.TabIndex = 3;
            this._moveDownButton.Text = "Move Down";
            this._moveDownButton.UseVisualStyleBackColor = true;
            // 
            // _moveLeftButton
            // 
            this._moveLeftButton.Location = new Point(3, 147);
            this._moveLeftButton.Name = "_moveLeftButton";
            this._moveLeftButton.Size = new Size(220, 30);
            this._moveLeftButton.TabIndex = 4;
            this._moveLeftButton.Text = "Move Left";
            this._moveLeftButton.UseVisualStyleBackColor = true;
            // 
            // _moveRightButton
            // 
            this._moveRightButton.Location = new Point(3, 183);
            this._moveRightButton.Name = "_moveRightButton";
            this._moveRightButton.Size = new Size(220, 30);
            this._moveRightButton.TabIndex = 5;
            this._moveRightButton.Text = "Move Right";
            this._moveRightButton.UseVisualStyleBackColor = true;
            // 
            // _interactButton
            // 
            this._interactButton.Location = new Point(3, 219);
            this._interactButton.Name = "_interactButton";
            this._interactButton.Size = new Size(220, 30);
            this._interactButton.TabIndex = 6;
            this._interactButton.Text = "Interact";
            this._interactButton.UseVisualStyleBackColor = true;
            // 
            // _itemLabel
            // 
            this._itemLabel.AutoSize = true;
            this._itemLabel.Location = new Point(3, 264);
            this._itemLabel.Margin = new Padding(3, 12, 3, 0);
            this._itemLabel.Name = "_itemLabel";
            this._itemLabel.Size = new Size(31, 15);
            this._itemLabel.TabIndex = 7;
            this._itemLabel.Text = "Item";
            // 
            // _itemComboBox
            // 
            this._itemComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._itemComboBox.FormattingEnabled = true;
            this._itemComboBox.Location = new Point(3, 282);
            this._itemComboBox.Name = "_itemComboBox";
            this._itemComboBox.Size = new Size(290, 23);
            this._itemComboBox.TabIndex = 8;
            // 
            // _useItemButton
            // 
            this._useItemButton.Location = new Point(3, 311);
            this._useItemButton.Name = "_useItemButton";
            this._useItemButton.Size = new Size(220, 30);
            this._useItemButton.TabIndex = 9;
            this._useItemButton.Text = "Use Item";
            this._useItemButton.UseVisualStyleBackColor = true;
            // 
            // _recipeLabel
            // 
            this._recipeLabel.AutoSize = true;
            this._recipeLabel.Location = new Point(3, 356);
            this._recipeLabel.Margin = new Padding(3, 12, 3, 0);
            this._recipeLabel.Name = "_recipeLabel";
            this._recipeLabel.Size = new Size(42, 15);
            this._recipeLabel.TabIndex = 10;
            this._recipeLabel.Text = "Recipe";
            // 
            // _recipeComboBox
            // 
            this._recipeComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._recipeComboBox.FormattingEnabled = true;
            this._recipeComboBox.Location = new Point(3, 374);
            this._recipeComboBox.Name = "_recipeComboBox";
            this._recipeComboBox.Size = new Size(290, 23);
            this._recipeComboBox.TabIndex = 11;
            // 
            // _craftButton
            // 
            this._craftButton.Location = new Point(3, 403);
            this._craftButton.Name = "_craftButton";
            this._craftButton.Size = new Size(220, 30);
            this._craftButton.TabIndex = 12;
            this._craftButton.Text = "Craft Recipe";
            this._craftButton.UseVisualStyleBackColor = true;
            // 
            // _lootLabel
            // 
            this._lootLabel.AutoSize = true;
            this._lootLabel.Location = new Point(3, 448);
            this._lootLabel.Margin = new Padding(3, 12, 3, 0);
            this._lootLabel.Name = "_lootLabel";
            this._lootLabel.Size = new Size(62, 15);
            this._lootLabel.TabIndex = 13;
            this._lootLabel.Text = "Loot Table";
            // 
            // _lootComboBox
            // 
            this._lootComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._lootComboBox.FormattingEnabled = true;
            this._lootComboBox.Location = new Point(3, 466);
            this._lootComboBox.Name = "_lootComboBox";
            this._lootComboBox.Size = new Size(290, 23);
            this._lootComboBox.TabIndex = 14;
            // 
            // _seedLabel
            // 
            this._seedLabel.AutoSize = true;
            this._seedLabel.Location = new Point(3, 492);
            this._seedLabel.Name = "_seedLabel";
            this._seedLabel.Size = new Size(32, 15);
            this._seedLabel.TabIndex = 15;
            this._seedLabel.Text = "Seed";
            // 
            // _seedTextBox
            // 
            this._seedTextBox.Location = new Point(3, 510);
            this._seedTextBox.Name = "_seedTextBox";
            this._seedTextBox.Size = new Size(120, 23);
            this._seedTextBox.TabIndex = 16;
            this._seedTextBox.Text = "123";
            // 
            // _rollLootButton
            // 
            this._rollLootButton.Location = new Point(3, 539);
            this._rollLootButton.Name = "_rollLootButton";
            this._rollLootButton.Size = new Size(220, 30);
            this._rollLootButton.TabIndex = 17;
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
            // _interactionLabel
            // 
            this._interactionLabel.AutoSize = true;
            this._interactionLabel.Location = new Point(3, 404);
            this._interactionLabel.Margin = new Padding(3, 12, 3, 0);
            this._interactionLabel.Name = "_interactionLabel";
            this._interactionLabel.Size = new Size(65, 15);
            this._interactionLabel.TabIndex = 18;
            this._interactionLabel.Text = "Interaction";
            // 
            // _interactionComboBox
            // 
            this._interactionComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._interactionComboBox.FormattingEnabled = true;
            this._interactionComboBox.Location = new Point(3, 422);
            this._interactionComboBox.Name = "_interactionComboBox";
            this._interactionComboBox.Size = new Size(290, 23);
            this._interactionComboBox.TabIndex = 19;
            // 
            // _interactionButton
            // 
            this._interactionButton.Location = new Point(3, 451);
            this._interactionButton.Name = "_interactionButton";
            this._interactionButton.Size = new Size(220, 30);
            this._interactionButton.TabIndex = 20;
            this._interactionButton.Text = "Execute Interaction";
            this._interactionButton.UseVisualStyleBackColor = true;
            // 
            // _equipmentSlotLabel
            // 
            this._equipmentSlotLabel.AutoSize = true;
            this._equipmentSlotLabel.Location = new Point(3, 496);
            this._equipmentSlotLabel.Margin = new Padding(3, 12, 3, 0);
            this._equipmentSlotLabel.Name = "_equipmentSlotLabel";
            this._equipmentSlotLabel.Size = new Size(88, 15);
            this._equipmentSlotLabel.TabIndex = 22;
            this._equipmentSlotLabel.Text = "Equipment Slot";
            // 
            // _equipmentSlotComboBox
            // 
            this._equipmentSlotComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._equipmentSlotComboBox.FormattingEnabled = true;
            this._equipmentSlotComboBox.Location = new Point(3, 514);
            this._equipmentSlotComboBox.Name = "_equipmentSlotComboBox";
            this._equipmentSlotComboBox.Size = new Size(290, 23);
            this._equipmentSlotComboBox.TabIndex = 23;
            // 
            // _equipButton
            // 
            this._equipButton.Location = new Point(3, 543);
            this._equipButton.Name = "_equipButton";
            this._equipButton.Size = new Size(220, 30);
            this._equipButton.TabIndex = 24;
            this._equipButton.Text = "Equip Item";
            this._equipButton.UseVisualStyleBackColor = true;
            // 
            // _unequipButton
            // 
            this._unequipButton.Location = new Point(3, 579);
            this._unequipButton.Name = "_unequipButton";
            this._unequipButton.Size = new Size(220, 30);
            this._unequipButton.TabIndex = 25;
            this._unequipButton.Text = "Unequip Slot";
            this._unequipButton.UseVisualStyleBackColor = true;
            // 
            // _containerLabel
            // 
            this._containerLabel.AutoSize = true;
            this._containerLabel.Location = new Point(3, 624);
            this._containerLabel.Margin = new Padding(3, 12, 3, 0);
            this._containerLabel.Name = "_containerLabel";
            this._containerLabel.Size = new Size(60, 15);
            this._containerLabel.TabIndex = 26;
            this._containerLabel.Text = "Container";
            // 
            // _containerComboBox
            // 
            this._containerComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._containerComboBox.FormattingEnabled = true;
            this._containerComboBox.Location = new Point(3, 642);
            this._containerComboBox.Name = "_containerComboBox";
            this._containerComboBox.Size = new Size(290, 23);
            this._containerComboBox.TabIndex = 27;
            // 
            // _containerItemLabel
            // 
            this._containerItemLabel.AutoSize = true;
            this._containerItemLabel.Location = new Point(3, 668);
            this._containerItemLabel.Name = "_containerItemLabel";
            this._containerItemLabel.Size = new Size(75, 15);
            this._containerItemLabel.TabIndex = 28;
            this._containerItemLabel.Text = "Transfer Item";
            // 
            // _containerItemTextBox
            // 
            this._containerItemTextBox.Location = new Point(3, 686);
            this._containerItemTextBox.Name = "_containerItemTextBox";
            this._containerItemTextBox.Size = new Size(290, 23);
            this._containerItemTextBox.TabIndex = 29;
            // 
            // _containerAmountLabel
            // 
            this._containerAmountLabel.AutoSize = true;
            this._containerAmountLabel.Location = new Point(3, 712);
            this._containerAmountLabel.Name = "_containerAmountLabel";
            this._containerAmountLabel.Size = new Size(51, 15);
            this._containerAmountLabel.TabIndex = 30;
            this._containerAmountLabel.Text = "Amount";
            // 
            // _containerAmountNumericUpDown
            // 
            this._containerAmountNumericUpDown.DecimalPlaces = 0;
            this._containerAmountNumericUpDown.Location = new Point(3, 730);
            this._containerAmountNumericUpDown.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            this._containerAmountNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this._containerAmountNumericUpDown.Name = "_containerAmountNumericUpDown";
            this._containerAmountNumericUpDown.Size = new Size(90, 23);
            this._containerAmountNumericUpDown.TabIndex = 31;
            this._containerAmountNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // _openContainerButton
            // 
            this._openContainerButton.Location = new Point(3, 759);
            this._openContainerButton.Name = "_openContainerButton";
            this._openContainerButton.Size = new Size(220, 30);
            this._openContainerButton.TabIndex = 32;
            this._openContainerButton.Text = "Open Container";
            this._openContainerButton.UseVisualStyleBackColor = true;
            // 
            // _takeContainerButton
            // 
            this._takeContainerButton.Location = new Point(3, 795);
            this._takeContainerButton.Name = "_takeContainerButton";
            this._takeContainerButton.Size = new Size(220, 30);
            this._takeContainerButton.TabIndex = 33;
            this._takeContainerButton.Text = "Take From Container";
            this._takeContainerButton.UseVisualStyleBackColor = true;
            // 
            // _depositContainerButton
            // 
            this._depositContainerButton.Location = new Point(3, 831);
            this._depositContainerButton.Name = "_depositContainerButton";
            this._depositContainerButton.Size = new Size(220, 30);
            this._depositContainerButton.TabIndex = 34;
            this._depositContainerButton.Text = "Deposit To Container";
            this._depositContainerButton.UseVisualStyleBackColor = true;
            // 
            // _resourceNodeLabel
            // 
            this._resourceNodeLabel.AutoSize = true;
            this._resourceNodeLabel.Location = new Point(3, 876);
            this._resourceNodeLabel.Margin = new Padding(3, 12, 3, 0);
            this._resourceNodeLabel.Name = "_resourceNodeLabel";
            this._resourceNodeLabel.Size = new Size(86, 15);
            this._resourceNodeLabel.TabIndex = 35;
            this._resourceNodeLabel.Text = "Resource Node";
            // 
            // _resourceNodeComboBox
            // 
            this._resourceNodeComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._resourceNodeComboBox.FormattingEnabled = true;
            this._resourceNodeComboBox.Location = new Point(3, 894);
            this._resourceNodeComboBox.Name = "_resourceNodeComboBox";
            this._resourceNodeComboBox.Size = new Size(290, 23);
            this._resourceNodeComboBox.TabIndex = 36;
            // 
            // _toolItemLabel
            // 
            this._toolItemLabel.AutoSize = true;
            this._toolItemLabel.Location = new Point(3, 920);
            this._toolItemLabel.Name = "_toolItemLabel";
            this._toolItemLabel.Size = new Size(56, 15);
            this._toolItemLabel.TabIndex = 37;
            this._toolItemLabel.Text = "Tool Item";
            // 
            // _toolItemTextBox
            // 
            this._toolItemTextBox.Location = new Point(3, 938);
            this._toolItemTextBox.Name = "_toolItemTextBox";
            this._toolItemTextBox.Size = new Size(290, 23);
            this._toolItemTextBox.TabIndex = 38;
            // 
            // _harvestButton
            // 
            this._harvestButton.Location = new Point(3, 967);
            this._harvestButton.Name = "_harvestButton";
            this._harvestButton.Size = new Size(220, 30);
            this._harvestButton.TabIndex = 39;
            this._harvestButton.Text = "Harvest Resource Node";
            this._harvestButton.UseVisualStyleBackColor = true;
            // 
            // _snapshotSlotLabel
            // 
            this._snapshotSlotLabel.AutoSize = true;
            this._snapshotSlotLabel.Location = new Point(3, 1012);
            this._snapshotSlotLabel.Margin = new Padding(3, 12, 3, 0);
            this._snapshotSlotLabel.Name = "_snapshotSlotLabel";
            this._snapshotSlotLabel.Size = new Size(77, 15);
            this._snapshotSlotLabel.TabIndex = 40;
            this._snapshotSlotLabel.Text = "Snapshot Slot";
            // 
            // _snapshotSlotTextBox
            // 
            this._snapshotSlotTextBox.Location = new Point(3, 1030);
            this._snapshotSlotTextBox.Name = "_snapshotSlotTextBox";
            this._snapshotSlotTextBox.Size = new Size(220, 23);
            this._snapshotSlotTextBox.TabIndex = 41;
            this._snapshotSlotTextBox.Text = "slot1";
            // 
            // _saveSnapshotButton
            // 
            this._saveSnapshotButton.Location = new Point(3, 1059);
            this._saveSnapshotButton.Name = "_saveSnapshotButton";
            this._saveSnapshotButton.Size = new Size(220, 30);
            this._saveSnapshotButton.TabIndex = 42;
            this._saveSnapshotButton.Text = "Save Snapshot";
            this._saveSnapshotButton.UseVisualStyleBackColor = true;
            // 
            // _loadSnapshotButton
            // 
            this._loadSnapshotButton.Location = new Point(3, 1095);
            this._loadSnapshotButton.Name = "_loadSnapshotButton";
            this._loadSnapshotButton.Size = new Size(220, 30);
            this._loadSnapshotButton.TabIndex = 43;
            this._loadSnapshotButton.Text = "Load Snapshot";
            this._loadSnapshotButton.UseVisualStyleBackColor = true;
            // 
            // _listSnapshotsButton
            // 
            this._listSnapshotsButton.Location = new Point(3, 1131);
            this._listSnapshotsButton.Name = "_listSnapshotsButton";
            this._listSnapshotsButton.Size = new Size(220, 30);
            this._listSnapshotsButton.TabIndex = 44;
            this._listSnapshotsButton.Text = "List Snapshots";
            this._listSnapshotsButton.UseVisualStyleBackColor = true;
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
            // _waitButton
            // 
            this._waitButton.Location = new Point(3, 487);
            this._waitButton.Name = "_waitButton";
            this._waitButton.Size = new Size(220, 30);
            this._waitButton.TabIndex = 21;
            this._waitButton.Text = "Wait";
            this._waitButton.UseVisualStyleBackColor = true;
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
            ((ISupportInitialize)this._containerAmountNumericUpDown).EndInit();
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
