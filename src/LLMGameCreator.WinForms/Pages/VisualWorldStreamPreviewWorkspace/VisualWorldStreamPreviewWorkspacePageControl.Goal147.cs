using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private FeatureModuleAuthoringWorkbenchController? _goal147Controller;
    private TabPage? _goal147AuthoringTab;
    private TextBox? _goal147LibraryStatus;
    private TextBox? _goal147CoreModules;
    private CheckedListBox? _goal147OptionalModules;
    private ListBox? _goal147SavedCompositions;
    private TextBox? _goal147CompositionId;
    private TextBox? _goal147DisplayName;
    private TextBox? _goal147Description;
    private FlowLayoutPanel? _goal147Parameters;
    private TextBox? _goal147Diagnostics;
    private TextBox? _goal147Results;
    private readonly List<Button> _goal147Buttons = [];
    private bool _goal147Binding;

    private void ConfigureGoal147AuthoringSurface(TabControl innerTabs)
    {
        _goal147AuthoringTab = new TabPage { Text = "Authoring & Saved Compositions" };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 5, Padding = new Padding(8) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _goal147LibraryStatus = Goal132ReadOnlyTextBox(true);
        _goal147CoreModules = Goal132ReadOnlyTextBox(true);
        _goal147OptionalModules = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true };
        _goal147SavedCompositions = new ListBox { Dock = DockStyle.Fill };
        _goal147Parameters = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        _goal147Diagnostics = Goal132ReadOnlyTextBox(true);
        _goal147Results = Goal132ReadOnlyTextBox(true);
        _goal147CompositionId = new TextBox { Dock = DockStyle.Top, Text = FeatureModuleAuthoringVocabulary.DefaultCompositionId };
        _goal147DisplayName = new TextBox { Dock = DockStyle.Top, Text = "Goal147 Custom Composition" };
        _goal147Description = new TextBox { Dock = DockStyle.Fill, Multiline = true, Text = "Persistent typed FeatureModule composition." };

        layout.Controls.Add(_goal147LibraryStatus, 0, 0);
        layout.SetColumnSpan(_goal147LibraryStatus, 3);
        layout.Controls.Add(_goal147CoreModules, 0, 1);
        layout.Controls.Add(_goal147OptionalModules, 1, 1);
        layout.Controls.Add(_goal147SavedCompositions, 2, 1);
        var identity = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        identity.Controls.Add(_goal147CompositionId, 0, 0);
        identity.Controls.Add(_goal147DisplayName, 0, 1);
        identity.Controls.Add(_goal147Description, 0, 2);
        layout.Controls.Add(identity, 0, 2);
        layout.Controls.Add(_goal147Parameters, 1, 2);
        layout.Controls.Add(_goal147Diagnostics, 2, 2);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
        foreach (var caption in new[]
                 {
                     "New Composition", "Open", "Save", "Save As / Clone", "Delete", "Validate",
                     "Materialize & Qualify", "Refresh Library", "Save, Materialize & Qualify"
                 })
        {
            var button = Goal132Button(caption);
            _goal147Buttons.Add(button);
            buttons.Controls.Add(button);
        }
        layout.Controls.Add(buttons, 0, 3);
        layout.SetColumnSpan(buttons, 3);
        layout.Controls.Add(_goal147Results, 0, 4);
        layout.SetColumnSpan(_goal147Results, 3);
        _goal147AuthoringTab.Controls.Add(layout);
        innerTabs.TabPages.Add(_goal147AuthoringTab);
    }

    private void WireGoal147AuthoringEvents()
    {
        if (_goal147Buttons.Count != 9) return;
        _goal147Buttons[0].Click += async (_, _) => await Goal147RunAsync(() =>
        {
            var controller = Goal147Controller();
            controller.RefreshLibrary();
            var id = _goal147CompositionId?.Text.Trim();
            controller.NewComposition(string.IsNullOrWhiteSpace(id) ? FeatureModuleAuthoringVocabulary.DefaultCompositionId : id,
                _goal147DisplayName?.Text.Trim() ?? "FeatureModule Composition", _goal147Description?.Text ?? string.Empty);
            BindGoal147All();
            return "new composition created";
        });
        _goal147Buttons[1].Click += async (_, _) => await Goal147RunAsync(() =>
        {
            if (_goal147SavedCompositions?.SelectedItem is not string id) throw new InvalidOperationException("Select a saved composition.");
            Goal147Controller().Open(id);
            BindGoal147All();
            return "opened=" + id;
        });
        _goal147Buttons[2].Click += async (_, _) => await Goal147RunAsync(() =>
        {
            Goal147CaptureDocument();
            var saved = Goal147Controller().Save();
            BindGoal147All();
            return "saved revision=" + saved.Revision;
        });
        _goal147Buttons[3].Click += async (_, _) => await Goal147RunAsync(() =>
        {
            Goal147CaptureDocument();
            var controller = Goal147Controller();
            var requested = _goal147CompositionId?.Text.Trim() ?? string.Empty;
            var cloneId = requested == controller.Document?.CompositionId ? requested + "-copy" : requested;
            var clone = controller.SaveAsClone(cloneId, (_goal147DisplayName?.Text.Trim() ?? "Composition") + " Clone");
            BindGoal147All();
            return "cloned=" + clone.CompositionId;
        });
        _goal147Buttons[4].Click += async (_, _) => await Goal147RunAsync(() =>
        {
            var id = Goal147Controller().Document?.CompositionId ?? string.Empty;
            Goal147Controller().Delete();
            BindGoal147All();
            return "deleted=" + id;
        });
        _goal147Buttons[5].Click += async (_, _) => await Goal147RunAsync(() =>
        {
            Goal147CaptureDocument();
            var validation = Goal147Controller().Validate();
            BindGoal147Status();
            return "validationPassed=" + validation.Passed + Environment.NewLine + string.Join(Environment.NewLine, validation.Diagnostics);
        });
        _goal147Buttons[6].Click += async (_, _) => await Goal147RunAsync(() =>
        {
            Goal147CaptureDocument();
            var result = Goal147Controller().MaterializeAndQualify();
            BindGoal147All();
            return "materialized=" + result.PackageSha256 + "; final=" + result.FinalStateHash;
        });
        _goal147Buttons[7].Click += async (_, _) => await Goal147RunAsync(() =>
        {
            Goal147Controller().RefreshLibrary();
            BindGoal147All();
            return "library refreshed";
        });
        _goal147Buttons[8].Click += async (_, _) => await Goal147RunAsync(() =>
        {
            Goal147CaptureDocument();
            var result = Goal147Controller().SaveMaterializeAndQualify();
            BindGoal147All();
            return "GREEN package=" + result.PackageSha256 + "; final=" + result.FinalStateHash;
        });
        if (_goal147OptionalModules is not null)
            _goal147OptionalModules.ItemCheck += (_, _) => BeginInvoke(new Action(() =>
            {
                if (_goal147Binding || _goal147OptionalModules is null) return;
                Goal147Controller().SetSelectedModules(_goal147OptionalModules.CheckedItems.Cast<string>().ToList());
                BuildGoal147ParameterEditors();
                BindGoal147Status();
            }));
    }

    private FeatureModuleAuthoringWorkbenchController Goal147Controller()
    {
        if (_goal147Controller is not null) return _goal147Controller;
        var root = FindProjectRoot() ?? throw new InvalidOperationException("Repository root was not found.");
        _goal147Controller = new FeatureModuleAuthoringWorkbenchController(
            root, SelectedRuntimeVariantInteractiveSessionService.CreateDefault());
        return _goal147Controller;
    }

    private void BindGoal147All()
    {
        var controller = Goal147Controller();
        var library = controller.Library ?? controller.RefreshLibrary();
        _goal147Binding = true;
        controller.BeginProgrammaticBinding();
        try
        {
            if (_goal147LibraryStatus is not null)
                _goal147LibraryStatus.Text = "library=GREEN; fingerprint=" + library.CatalogFingerprint
                                                  + "; required=" + library.Index.RequiredCoreModuleCount
                                                  + "; optional=" + library.Index.OptionalModuleCount
                                                  + "; parameters=" + library.Index.ParameterDefinitionCount;
            if (_goal147CoreModules is not null)
                _goal147CoreModules.Text = string.Join(Environment.NewLine,
                    library.Catalog.Modules.Where(module => module.Required).Select(module => "[locked] " + module.ModuleId));
            if (_goal147OptionalModules is not null)
            {
                _goal147OptionalModules.Items.Clear();
                foreach (var module in library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
                             .OrderBy(module => module.ModuleId, StringComparer.Ordinal))
                    _goal147OptionalModules.Items.Add(module.ModuleId,
                        controller.Document?.SelectedModuleIds.Contains(module.ModuleId, StringComparer.Ordinal) ?? true);
            }
            if (_goal147SavedCompositions is not null)
            {
                _goal147SavedCompositions.Items.Clear();
                foreach (var item in controller.List().Compositions.Where(item => !item.Corrupt))
                    _goal147SavedCompositions.Items.Add(item.CompositionId);
            }
            if (controller.Document is { } document)
            {
                if (_goal147CompositionId is not null) _goal147CompositionId.Text = document.CompositionId;
                if (_goal147DisplayName is not null) _goal147DisplayName.Text = document.DisplayName;
                if (_goal147Description is not null) _goal147Description.Text = document.Description;
            }
            BuildGoal147ParameterEditors();
        }
        finally
        {
            controller.EndProgrammaticBinding();
            _goal147Binding = false;
        }
        BindGoal147Status();
    }

    private void BuildGoal147ParameterEditors()
    {
        if (_goal147Parameters is null || Goal147Controller().Document is null) return;
        _goal147Parameters.Controls.Clear();
        foreach (var definition in Goal147Controller().ActiveParameterDefinitions())
        {
            var row = new FlowLayoutPanel { Width = 360, Height = 30, WrapContents = false };
            row.Controls.Add(new Label { Text = definition.Title, Width = 190, AutoEllipsis = true });
            var current = Goal147Value(definition);
            Control editor;
            if (definition.AuthoringControl == FeatureModuleAuthoringControls.NumericUpDown)
            {
                var numeric = new NumericUpDown
                {
                    Width = 130,
                    Minimum = definition.Minimum ?? -100000,
                    Maximum = definition.Maximum ?? 100000,
                    Increment = definition.Step ?? 1,
                    DecimalPlaces = definition.ValueType == FeatureModuleParameterValueTypes.Integer ? 0 : 4,
                    Value = current.GetDecimal()
                };
                numeric.ValueChanged += (_, _) => Goal147ParameterChanged(definition,
                    definition.ValueType == FeatureModuleParameterValueTypes.Integer
                        ? JsonSerializer.SerializeToElement(decimal.ToInt64(numeric.Value))
                        : JsonSerializer.SerializeToElement(numeric.Value));
                editor = numeric;
            }
            else if (definition.AuthoringControl == FeatureModuleAuthoringControls.CheckBox)
            {
                var check = new CheckBox { Width = 130, Checked = current.GetBoolean() };
                check.CheckedChanged += (_, _) => Goal147ParameterChanged(definition, JsonSerializer.SerializeToElement(check.Checked));
                editor = check;
            }
            else
            {
                var combo = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
                combo.Items.AddRange(definition.AllowedValues.Cast<object>().ToArray());
                combo.SelectedItem = current.GetString();
                combo.SelectionChangeCommitted += (_, _) => Goal147ParameterChanged(definition,
                    JsonSerializer.SerializeToElement(combo.SelectedItem?.ToString() ?? string.Empty));
                editor = combo;
            }
            row.Controls.Add(editor);
            _goal147Parameters.Controls.Add(row);
        }
    }

    private JsonElement Goal147Value(FeatureModuleParameterDefinition definition)
    {
        var supplied = Goal147Controller().Document!.ParameterValues.FirstOrDefault(item =>
            item.ModuleId == definition.ModuleId && item.ParameterId == definition.ParameterId);
        return supplied?.Value ?? definition.DefaultValue;
    }

    private void Goal147ParameterChanged(FeatureModuleParameterDefinition definition, JsonElement value)
    {
        if (_goal147Binding) return;
        Goal147Controller().SetParameterValue(definition.ModuleId, definition.ParameterId, value);
        BindGoal147Status();
    }

    private void Goal147CaptureDocument()
    {
        var controller = Goal147Controller();
        if (controller.Document is null) throw new InvalidOperationException("Create or open a composition first.");
        controller.SetIdentity(_goal147CompositionId?.Text.Trim() ?? string.Empty,
            _goal147DisplayName?.Text.Trim() ?? string.Empty, _goal147Description?.Text ?? string.Empty);
        if (_goal147OptionalModules is not null)
            controller.SetSelectedModules(_goal147OptionalModules.CheckedItems.Cast<string>().ToList());
    }

    private void BindGoal147Status()
    {
        if (_goal147Diagnostics is null || _goal147Results is null) return;
        var controller = Goal147Controller();
        if (controller.Document is null)
        {
            _goal147Diagnostics.Text = "Create or open a saved composition.";
            _goal147Results.Text = "Runtime remains gameplay truth; Unity is read-only.";
            return;
        }
        var validation = controller.Validate();
        var stale = controller.Staleness();
        var coverage = controller.CoveragePlan();
        var certification = controller.LastCertificationLedger;
        _goal147Diagnostics.Text = "dirty=" + controller.Dirty + "; stale=" + stale.Stale
                                       + "; valid=" + validation.Passed + Environment.NewLine
                                       + string.Join(Environment.NewLine, validation.Diagnostics.Concat(stale.Diagnostics));
        _goal147Results.Text = string.Join(Environment.NewLine,
        [
            "compositionId=" + controller.Document.CompositionId + "; revision=" + controller.Document.Revision,
            "lastMaterializedPackageSha256=" + controller.Document.LastMaterializedPackageSha256,
            "lastQualifiedFinalStateHash=" + controller.Document.LastQualifiedFinalStateHash,
            "lastQualificationStatus=" + controller.Document.LastQualificationStatus,
            "certificationLedger=" + (certification is null
                ? "planned=" + controller.Library!.Index.OptionalModuleCount
                : certification.CertifiedModuleCount + "/" + certification.PlannedModuleCount + "; reused=" + certification.ReusedCount),
            "interactionCoverage=" + coverage.CoverageMode + "; rows=" + coverage.GeneratedCompositionCount,
            "runtimeAuthority=true; unityGameplayTruth=false"
        ]);
    }

    private async Task Goal147RunAsync(Func<string> operation)
    {
        Goal147SetRunning(true);
        try
        {
            await Task.Yield();
            var message = operation();
            if (_goal147Diagnostics is not null) _goal147Diagnostics.Text = Goal144Tail(message);
        }
        catch (Exception exception)
        {
            if (_goal147Diagnostics is not null) _goal147Diagnostics.Text = Goal144Tail("failed: " + exception.Message);
        }
        finally { Goal147SetRunning(false); }
    }

    private void Goal147SetRunning(bool running)
    {
        foreach (var button in _goal147Buttons) button.Enabled = !running;
        if (_goal147OptionalModules is not null) _goal147OptionalModules.Enabled = !running;
        if (_goal147SavedCompositions is not null) _goal147SavedCompositions.Enabled = !running;
        if (_goal147Parameters is not null) _goal147Parameters.Enabled = !running;
    }
}
