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
        private Label _encounterLabel;
        private ComboBox _encounterComboBox;
        private Button _startEncounterButton;
        private Label _abilityLabel;
        private ComboBox _abilityComboBox;
        private Label _sourceParticipantLabel;
        private TextBox _sourceParticipantTextBox;
        private Label _targetParticipantLabel;
        private TextBox _targetParticipantTextBox;
        private Button _useAbilityButton;
        private Button _basicAttackButton;
        private Button _endTurnButton;
        private Button _runAiButton;
        private Button _resolveEncounterButton;
        private Button _fleeEncounterButton;
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
        private Label _questLabel;
        private ComboBox _questComboBox;
        private Label _objectiveLabel;
        private TextBox _objectiveTextBox;
        private Button _startQuestButton;
        private Button _advanceObjectiveButton;
        private Button _completeQuestButton;
        private Button _failQuestButton;
        private Button _refreshObjectivesButton;
        private Label _dialogueLabel;
        private ComboBox _dialogueComboBox;
        private Label _choiceLabel;
        private TextBox _choiceTextBox;
        private Button _openDialogueButton;
        private Button _chooseDialogueButton;
        private Button _closeDialogueButton;
        private Label _factionLabel;
        private ComboBox _factionComboBox;
        private Label _reputationLabel;
        private NumericUpDown _reputationNumericUpDown;
        private Button _changeReputationButton;
        private Button _setReputationButton;
        private Label _snapshotSlotLabel;
        private TextBox _snapshotSlotTextBox;
        private Button _saveSnapshotButton;
        private Button _loadSnapshotButton;
        private Button _listSnapshotsButton;
        private Button _migrateSnapshotButton;
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
            this._encounterLabel = new Label();
            this._encounterComboBox = new ComboBox();
            this._startEncounterButton = new Button();
            this._abilityLabel = new Label();
            this._abilityComboBox = new ComboBox();
            this._sourceParticipantLabel = new Label();
            this._sourceParticipantTextBox = new TextBox();
            this._targetParticipantLabel = new Label();
            this._targetParticipantTextBox = new TextBox();
            this._useAbilityButton = new Button();
            this._basicAttackButton = new Button();
            this._endTurnButton = new Button();
            this._runAiButton = new Button();
            this._resolveEncounterButton = new Button();
            this._fleeEncounterButton = new Button();
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
            this._questLabel = new Label();
            this._questComboBox = new ComboBox();
            this._objectiveLabel = new Label();
            this._objectiveTextBox = new TextBox();
            this._startQuestButton = new Button();
            this._advanceObjectiveButton = new Button();
            this._completeQuestButton = new Button();
            this._failQuestButton = new Button();
            this._refreshObjectivesButton = new Button();
            this._dialogueLabel = new Label();
            this._dialogueComboBox = new ComboBox();
            this._choiceLabel = new Label();
            this._choiceTextBox = new TextBox();
            this._openDialogueButton = new Button();
            this._chooseDialogueButton = new Button();
            this._closeDialogueButton = new Button();
            this._factionLabel = new Label();
            this._factionComboBox = new ComboBox();
            this._reputationLabel = new Label();
            this._reputationNumericUpDown = new NumericUpDown();
            this._changeReputationButton = new Button();
            this._setReputationButton = new Button();
            this._snapshotSlotLabel = new Label();
            this._snapshotSlotTextBox = new TextBox();
            this._saveSnapshotButton = new Button();
            this._loadSnapshotButton = new Button();
            this._listSnapshotsButton = new Button();
            this._migrateSnapshotButton = new Button();
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
            ((ISupportInitialize)this._reputationNumericUpDown).BeginInit();
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
            this._topToolbarPanel.Controls.Add(this._encounterLabel);
            this._topToolbarPanel.Controls.Add(this._encounterComboBox);
            this._topToolbarPanel.Controls.Add(this._startEncounterButton);
            this._topToolbarPanel.Controls.Add(this._abilityLabel);
            this._topToolbarPanel.Controls.Add(this._abilityComboBox);
            this._topToolbarPanel.Controls.Add(this._sourceParticipantLabel);
            this._topToolbarPanel.Controls.Add(this._sourceParticipantTextBox);
            this._topToolbarPanel.Controls.Add(this._targetParticipantLabel);
            this._topToolbarPanel.Controls.Add(this._targetParticipantTextBox);
            this._topToolbarPanel.Controls.Add(this._useAbilityButton);
            this._topToolbarPanel.Controls.Add(this._basicAttackButton);
            this._topToolbarPanel.Controls.Add(this._endTurnButton);
            this._topToolbarPanel.Controls.Add(this._runAiButton);
            this._topToolbarPanel.Controls.Add(this._resolveEncounterButton);
            this._topToolbarPanel.Controls.Add(this._fleeEncounterButton);
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
            this._topToolbarPanel.Controls.Add(this._questLabel);
            this._topToolbarPanel.Controls.Add(this._questComboBox);
            this._topToolbarPanel.Controls.Add(this._objectiveLabel);
            this._topToolbarPanel.Controls.Add(this._objectiveTextBox);
            this._topToolbarPanel.Controls.Add(this._startQuestButton);
            this._topToolbarPanel.Controls.Add(this._advanceObjectiveButton);
            this._topToolbarPanel.Controls.Add(this._completeQuestButton);
            this._topToolbarPanel.Controls.Add(this._failQuestButton);
            this._topToolbarPanel.Controls.Add(this._refreshObjectivesButton);
            this._topToolbarPanel.Controls.Add(this._dialogueLabel);
            this._topToolbarPanel.Controls.Add(this._dialogueComboBox);
            this._topToolbarPanel.Controls.Add(this._choiceLabel);
            this._topToolbarPanel.Controls.Add(this._choiceTextBox);
            this._topToolbarPanel.Controls.Add(this._openDialogueButton);
            this._topToolbarPanel.Controls.Add(this._chooseDialogueButton);
            this._topToolbarPanel.Controls.Add(this._closeDialogueButton);
            this._topToolbarPanel.Controls.Add(this._factionLabel);
            this._topToolbarPanel.Controls.Add(this._factionComboBox);
            this._topToolbarPanel.Controls.Add(this._reputationLabel);
            this._topToolbarPanel.Controls.Add(this._reputationNumericUpDown);
            this._topToolbarPanel.Controls.Add(this._changeReputationButton);
            this._topToolbarPanel.Controls.Add(this._setReputationButton);
            this._topToolbarPanel.Controls.Add(this._snapshotSlotLabel);
            this._topToolbarPanel.Controls.Add(this._snapshotSlotTextBox);
            this._topToolbarPanel.Controls.Add(this._saveSnapshotButton);
            this._topToolbarPanel.Controls.Add(this._loadSnapshotButton);
            this._topToolbarPanel.Controls.Add(this._listSnapshotsButton);
            this._topToolbarPanel.Controls.Add(this._migrateSnapshotButton);
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
            // _encounterLabel
            // 
            this._encounterLabel.AutoSize = true;
            this._encounterLabel.Location = new Point(3, 496);
            this._encounterLabel.Margin = new Padding(3, 12, 3, 0);
            this._encounterLabel.Name = "_encounterLabel";
            this._encounterLabel.Size = new Size(62, 15);
            this._encounterLabel.TabIndex = 45;
            this._encounterLabel.Text = "Encounter";
            // 
            // _encounterComboBox
            // 
            this._encounterComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._encounterComboBox.FormattingEnabled = true;
            this._encounterComboBox.Location = new Point(3, 514);
            this._encounterComboBox.Name = "_encounterComboBox";
            this._encounterComboBox.Size = new Size(290, 23);
            this._encounterComboBox.TabIndex = 46;
            // 
            // _startEncounterButton
            // 
            this._startEncounterButton.Location = new Point(3, 543);
            this._startEncounterButton.Name = "_startEncounterButton";
            this._startEncounterButton.Size = new Size(220, 30);
            this._startEncounterButton.TabIndex = 47;
            this._startEncounterButton.Text = "Start Encounter";
            this._startEncounterButton.UseVisualStyleBackColor = true;
            // 
            // _abilityLabel
            // 
            this._abilityLabel.AutoSize = true;
            this._abilityLabel.Location = new Point(3, 588);
            this._abilityLabel.Margin = new Padding(3, 12, 3, 0);
            this._abilityLabel.Name = "_abilityLabel";
            this._abilityLabel.Size = new Size(39, 15);
            this._abilityLabel.TabIndex = 48;
            this._abilityLabel.Text = "Ability";
            // 
            // _abilityComboBox
            // 
            this._abilityComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._abilityComboBox.FormattingEnabled = true;
            this._abilityComboBox.Location = new Point(3, 606);
            this._abilityComboBox.Name = "_abilityComboBox";
            this._abilityComboBox.Size = new Size(290, 23);
            this._abilityComboBox.TabIndex = 49;
            // 
            // _sourceParticipantLabel
            // 
            this._sourceParticipantLabel.AutoSize = true;
            this._sourceParticipantLabel.Location = new Point(3, 632);
            this._sourceParticipantLabel.Name = "_sourceParticipantLabel";
            this._sourceParticipantLabel.Size = new Size(103, 15);
            this._sourceParticipantLabel.TabIndex = 50;
            this._sourceParticipantLabel.Text = "Source Participant";
            // 
            // _sourceParticipantTextBox
            // 
            this._sourceParticipantTextBox.Location = new Point(3, 650);
            this._sourceParticipantTextBox.Name = "_sourceParticipantTextBox";
            this._sourceParticipantTextBox.Size = new Size(220, 23);
            this._sourceParticipantTextBox.TabIndex = 51;
            this._sourceParticipantTextBox.Text = "player";
            // 
            // _targetParticipantLabel
            // 
            this._targetParticipantLabel.AutoSize = true;
            this._targetParticipantLabel.Location = new Point(3, 676);
            this._targetParticipantLabel.Name = "_targetParticipantLabel";
            this._targetParticipantLabel.Size = new Size(99, 15);
            this._targetParticipantLabel.TabIndex = 52;
            this._targetParticipantLabel.Text = "Target Participant";
            // 
            // _targetParticipantTextBox
            // 
            this._targetParticipantTextBox.Location = new Point(3, 694);
            this._targetParticipantTextBox.Name = "_targetParticipantTextBox";
            this._targetParticipantTextBox.Size = new Size(220, 23);
            this._targetParticipantTextBox.TabIndex = 53;
            // 
            // _useAbilityButton
            // 
            this._useAbilityButton.Location = new Point(3, 723);
            this._useAbilityButton.Name = "_useAbilityButton";
            this._useAbilityButton.Size = new Size(220, 30);
            this._useAbilityButton.TabIndex = 54;
            this._useAbilityButton.Text = "Use Ability";
            this._useAbilityButton.UseVisualStyleBackColor = true;
            // 
            // _basicAttackButton
            // 
            this._basicAttackButton.Location = new Point(3, 759);
            this._basicAttackButton.Name = "_basicAttackButton";
            this._basicAttackButton.Size = new Size(220, 30);
            this._basicAttackButton.TabIndex = 55;
            this._basicAttackButton.Text = "Basic Attack";
            this._basicAttackButton.UseVisualStyleBackColor = true;
            // 
            // _endTurnButton
            // 
            this._endTurnButton.Location = new Point(3, 795);
            this._endTurnButton.Name = "_endTurnButton";
            this._endTurnButton.Size = new Size(220, 30);
            this._endTurnButton.TabIndex = 56;
            this._endTurnButton.Text = "End Turn";
            this._endTurnButton.UseVisualStyleBackColor = true;
            // 
            // _runAiButton
            // 
            this._runAiButton.Location = new Point(3, 831);
            this._runAiButton.Name = "_runAiButton";
            this._runAiButton.Size = new Size(220, 30);
            this._runAiButton.TabIndex = 57;
            this._runAiButton.Text = "Run AI";
            this._runAiButton.UseVisualStyleBackColor = true;
            // 
            // _resolveEncounterButton
            // 
            this._resolveEncounterButton.Location = new Point(3, 867);
            this._resolveEncounterButton.Name = "_resolveEncounterButton";
            this._resolveEncounterButton.Size = new Size(220, 30);
            this._resolveEncounterButton.TabIndex = 58;
            this._resolveEncounterButton.Text = "Resolve Encounter";
            this._resolveEncounterButton.UseVisualStyleBackColor = true;
            // 
            // _fleeEncounterButton
            // 
            this._fleeEncounterButton.Location = new Point(3, 903);
            this._fleeEncounterButton.Name = "_fleeEncounterButton";
            this._fleeEncounterButton.Size = new Size(220, 30);
            this._fleeEncounterButton.TabIndex = 59;
            this._fleeEncounterButton.Text = "Flee Encounter";
            this._fleeEncounterButton.UseVisualStyleBackColor = true;
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
            // _questLabel
            // 
            this._questLabel.AutoSize = true;
            this._questLabel.Location = new Point(3, 1012);
            this._questLabel.Margin = new Padding(3, 12, 3, 0);
            this._questLabel.Name = "_questLabel";
            this._questLabel.Size = new Size(38, 15);
            this._questLabel.TabIndex = 45;
            this._questLabel.Text = "Quest";
            // 
            // _questComboBox
            // 
            this._questComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._questComboBox.FormattingEnabled = true;
            this._questComboBox.Location = new Point(3, 1030);
            this._questComboBox.Name = "_questComboBox";
            this._questComboBox.Size = new Size(290, 23);
            this._questComboBox.TabIndex = 46;
            // 
            // _objectiveLabel
            // 
            this._objectiveLabel.AutoSize = true;
            this._objectiveLabel.Location = new Point(3, 1056);
            this._objectiveLabel.Name = "_objectiveLabel";
            this._objectiveLabel.Size = new Size(57, 15);
            this._objectiveLabel.TabIndex = 47;
            this._objectiveLabel.Text = "Objective";
            // 
            // _objectiveTextBox
            // 
            this._objectiveTextBox.Location = new Point(3, 1074);
            this._objectiveTextBox.Name = "_objectiveTextBox";
            this._objectiveTextBox.Size = new Size(290, 23);
            this._objectiveTextBox.TabIndex = 48;
            // 
            // _startQuestButton
            // 
            this._startQuestButton.Location = new Point(3, 1103);
            this._startQuestButton.Name = "_startQuestButton";
            this._startQuestButton.Size = new Size(220, 30);
            this._startQuestButton.TabIndex = 49;
            this._startQuestButton.Text = "Start Quest";
            this._startQuestButton.UseVisualStyleBackColor = true;
            // 
            // _advanceObjectiveButton
            // 
            this._advanceObjectiveButton.Location = new Point(3, 1139);
            this._advanceObjectiveButton.Name = "_advanceObjectiveButton";
            this._advanceObjectiveButton.Size = new Size(220, 30);
            this._advanceObjectiveButton.TabIndex = 50;
            this._advanceObjectiveButton.Text = "Advance Objective";
            this._advanceObjectiveButton.UseVisualStyleBackColor = true;
            // 
            // _completeQuestButton
            // 
            this._completeQuestButton.Location = new Point(3, 1175);
            this._completeQuestButton.Name = "_completeQuestButton";
            this._completeQuestButton.Size = new Size(220, 30);
            this._completeQuestButton.TabIndex = 51;
            this._completeQuestButton.Text = "Complete Quest";
            this._completeQuestButton.UseVisualStyleBackColor = true;
            // 
            // _failQuestButton
            // 
            this._failQuestButton.Location = new Point(3, 1211);
            this._failQuestButton.Name = "_failQuestButton";
            this._failQuestButton.Size = new Size(220, 30);
            this._failQuestButton.TabIndex = 52;
            this._failQuestButton.Text = "Fail Quest";
            this._failQuestButton.UseVisualStyleBackColor = true;
            // 
            // _refreshObjectivesButton
            // 
            this._refreshObjectivesButton.Location = new Point(3, 1247);
            this._refreshObjectivesButton.Name = "_refreshObjectivesButton";
            this._refreshObjectivesButton.Size = new Size(220, 30);
            this._refreshObjectivesButton.TabIndex = 53;
            this._refreshObjectivesButton.Text = "Refresh Objectives";
            this._refreshObjectivesButton.UseVisualStyleBackColor = true;
            // 
            // _dialogueLabel
            // 
            this._dialogueLabel.AutoSize = true;
            this._dialogueLabel.Location = new Point(3, 1292);
            this._dialogueLabel.Margin = new Padding(3, 12, 3, 0);
            this._dialogueLabel.Name = "_dialogueLabel";
            this._dialogueLabel.Size = new Size(55, 15);
            this._dialogueLabel.TabIndex = 54;
            this._dialogueLabel.Text = "Dialogue";
            // 
            // _dialogueComboBox
            // 
            this._dialogueComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._dialogueComboBox.FormattingEnabled = true;
            this._dialogueComboBox.Location = new Point(3, 1310);
            this._dialogueComboBox.Name = "_dialogueComboBox";
            this._dialogueComboBox.Size = new Size(290, 23);
            this._dialogueComboBox.TabIndex = 55;
            // 
            // _choiceLabel
            // 
            this._choiceLabel.AutoSize = true;
            this._choiceLabel.Location = new Point(3, 1336);
            this._choiceLabel.Name = "_choiceLabel";
            this._choiceLabel.Size = new Size(44, 15);
            this._choiceLabel.TabIndex = 56;
            this._choiceLabel.Text = "Choice";
            // 
            // _choiceTextBox
            // 
            this._choiceTextBox.Location = new Point(3, 1354);
            this._choiceTextBox.Name = "_choiceTextBox";
            this._choiceTextBox.Size = new Size(290, 23);
            this._choiceTextBox.TabIndex = 57;
            // 
            // _openDialogueButton
            // 
            this._openDialogueButton.Location = new Point(3, 1383);
            this._openDialogueButton.Name = "_openDialogueButton";
            this._openDialogueButton.Size = new Size(220, 30);
            this._openDialogueButton.TabIndex = 58;
            this._openDialogueButton.Text = "Open Dialogue";
            this._openDialogueButton.UseVisualStyleBackColor = true;
            // 
            // _chooseDialogueButton
            // 
            this._chooseDialogueButton.Location = new Point(3, 1419);
            this._chooseDialogueButton.Name = "_chooseDialogueButton";
            this._chooseDialogueButton.Size = new Size(220, 30);
            this._chooseDialogueButton.TabIndex = 59;
            this._chooseDialogueButton.Text = "Choose Choice";
            this._chooseDialogueButton.UseVisualStyleBackColor = true;
            // 
            // _closeDialogueButton
            // 
            this._closeDialogueButton.Location = new Point(3, 1455);
            this._closeDialogueButton.Name = "_closeDialogueButton";
            this._closeDialogueButton.Size = new Size(220, 30);
            this._closeDialogueButton.TabIndex = 60;
            this._closeDialogueButton.Text = "Close Dialogue";
            this._closeDialogueButton.UseVisualStyleBackColor = true;
            // 
            // _factionLabel
            // 
            this._factionLabel.AutoSize = true;
            this._factionLabel.Location = new Point(3, 1500);
            this._factionLabel.Margin = new Padding(3, 12, 3, 0);
            this._factionLabel.Name = "_factionLabel";
            this._factionLabel.Size = new Size(46, 15);
            this._factionLabel.TabIndex = 61;
            this._factionLabel.Text = "Faction";
            // 
            // _factionComboBox
            // 
            this._factionComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._factionComboBox.FormattingEnabled = true;
            this._factionComboBox.Location = new Point(3, 1518);
            this._factionComboBox.Name = "_factionComboBox";
            this._factionComboBox.Size = new Size(290, 23);
            this._factionComboBox.TabIndex = 62;
            // 
            // _reputationLabel
            // 
            this._reputationLabel.AutoSize = true;
            this._reputationLabel.Location = new Point(3, 1544);
            this._reputationLabel.Name = "_reputationLabel";
            this._reputationLabel.Size = new Size(67, 15);
            this._reputationLabel.TabIndex = 63;
            this._reputationLabel.Text = "Reputation";
            // 
            // _reputationNumericUpDown
            // 
            this._reputationNumericUpDown.DecimalPlaces = 2;
            this._reputationNumericUpDown.Location = new Point(3, 1562);
            this._reputationNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this._reputationNumericUpDown.Minimum = new decimal(new int[] { 100000, 0, 0, int.MinValue });
            this._reputationNumericUpDown.Name = "_reputationNumericUpDown";
            this._reputationNumericUpDown.Size = new Size(120, 23);
            this._reputationNumericUpDown.TabIndex = 64;
            // 
            // _changeReputationButton
            // 
            this._changeReputationButton.Location = new Point(3, 1591);
            this._changeReputationButton.Name = "_changeReputationButton";
            this._changeReputationButton.Size = new Size(220, 30);
            this._changeReputationButton.TabIndex = 65;
            this._changeReputationButton.Text = "Change Reputation";
            this._changeReputationButton.UseVisualStyleBackColor = true;
            // 
            // _setReputationButton
            // 
            this._setReputationButton.Location = new Point(3, 1627);
            this._setReputationButton.Name = "_setReputationButton";
            this._setReputationButton.Size = new Size(220, 30);
            this._setReputationButton.TabIndex = 66;
            this._setReputationButton.Text = "Set Reputation";
            this._setReputationButton.UseVisualStyleBackColor = true;
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
            // _migrateSnapshotButton
            //
            this._migrateSnapshotButton.Location = new Point(3, 1167);
            this._migrateSnapshotButton.Name = "_migrateSnapshotButton";
            this._migrateSnapshotButton.Size = new Size(220, 30);
            this._migrateSnapshotButton.TabIndex = 67;
            this._migrateSnapshotButton.Text = "Перенести сохранение";
            this._migrateSnapshotButton.UseVisualStyleBackColor = true;
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
            ((ISupportInitialize)this._reputationNumericUpDown).EndInit();
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
