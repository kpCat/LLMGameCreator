using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private readonly FeatureModuleCompositionWorkbenchController _goal146Controller = CreateGoal146Controller();
    private TabPage? _goal146Tab;
    private TextBox? _goal146CoreModules;
    private CheckedListBox? _goal146OptionalModules;
    private TextBox? _goal146ModuleDetails;
    private TextBox? _goal146CompositionId;
    private TextBox? _goal146DisplayName;
    private TextBox? _goal146Diagnostics;
    private TextBox? _goal146Results;
    private readonly List<Button> _goal146Buttons = [];
    private FeatureModuleCompositionWriteResult? _goal146LastResult;
    private bool _goal146BindingChecks;

    private static FeatureModuleCompositionWorkbenchController CreateGoal146Controller()
    {
        var service = new FeatureModuleCompositionService(SelectedRuntimeVariantInteractiveSessionService.CreateDefault());
        return new FeatureModuleCompositionWorkbenchController(service, new FeatureModuleCompositionOperatorRunner(service));
    }

    private void ConfigureGoal146ModuleComposerPanel()
    {
        _goal146Tab = new TabPage { Name = "_goal146Tab", Text = "Goal146 Module Composer" };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(8) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "Goal146 FeatureModule Composition Workbench",
            Font = new Font(Font, FontStyle.Bold)
        }, 0, 0);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 0)!, 2);

        _goal146CoreModules = Goal132ReadOnlyTextBox(true);
        _goal146OptionalModules = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true };
        _goal146ModuleDetails = Goal132ReadOnlyTextBox(true);
        _goal146Diagnostics = Goal132ReadOnlyTextBox(true);
        _goal146Results = Goal132ReadOnlyTextBox(true);
        _goal146CompositionId = new TextBox { Dock = DockStyle.Fill, Text = FeatureModuleCompositionVocabulary.DefaultCompositionId };
        _goal146DisplayName = new TextBox { Dock = DockStyle.Fill, Text = "Alchemy + Combat + Exploration Composition" };

        layout.Controls.Add(_goal146CoreModules, 0, 1);
        layout.Controls.Add(_goal146OptionalModules, 1, 1);
        layout.Controls.Add(_goal146ModuleDetails, 0, 2);
        layout.Controls.Add(_goal146Diagnostics, 1, 2);
        var identity = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        identity.Controls.Add(_goal146CompositionId, 0, 0);
        identity.Controls.Add(_goal146DisplayName, 0, 1);
        layout.Controls.Add(identity, 0, 3);
        layout.SetColumnSpan(identity, 2);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
        foreach (var caption in new[]
                 {
                     "Load FeatureModule Catalog", "Select All Optional", "Clear Optional",
                     "Validate Composition", "Materialize & Qualify Selected Composition", "Run Composition Matrix"
                 })
        {
            var button = Goal132Button(caption);
            _goal146Buttons.Add(button);
            buttons.Controls.Add(button);
        }
        layout.Controls.Add(buttons, 0, 4);
        layout.SetColumnSpan(buttons, 2);
        layout.Controls.Add(_goal146Results, 0, 5);
        layout.SetColumnSpan(_goal146Results, 2);
        _goal146Tab.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal146Tab);
    }

    private void WireGoal146ModuleComposerEvents()
    {
        if (_goal146Buttons.Count != 6) return;
        _goal146Buttons[0].Click += async (_, _) => await Goal146RunAsync(() =>
        {
            var root = FindProjectRoot() ?? throw new InvalidOperationException("Repository root was not found.");
            var catalog = _goal146Controller.LoadCatalog(root);
            BindGoal146Catalog(catalog);
            return Task.FromResult("required=" + catalog.RequiredCoreModuleCount + "; optional=" + catalog.OptionalProfileModuleCount);
        });
        _goal146Buttons[1].Click += (_, _) => Goal146SetAllOptional(true);
        _goal146Buttons[2].Click += (_, _) => Goal146SetAllOptional(false);
        _goal146Buttons[3].Click += async (_, _) => await Goal146RunAsync(() =>
        {
            Goal146CaptureCheckedModules();
            var validation = _goal146Controller.ValidateSelection();
            return Task.FromResult("validationPassed=" + validation.Passed + Environment.NewLine + string.Join(Environment.NewLine, validation.Diagnostics));
        });
        _goal146Buttons[4].Click += async (_, _) => await Goal146RunSelectedAsync();
        _goal146Buttons[5].Click += async (_, _) => await Goal146RunSelectedAsync();
        if (_goal146OptionalModules is not null)
            _goal146OptionalModules.SelectedIndexChanged += (_, _) => BindGoal146SelectedModuleDetails();
    }

    private async Task Goal146RunSelectedAsync()
    {
        await Goal146RunAsync(async () =>
        {
            var root = FindProjectRoot() ?? throw new InvalidOperationException("Repository root was not found.");
            if (_goal146Controller.Catalog is null) BindGoal146Catalog(_goal146Controller.LoadCatalog(root));
            Goal146CaptureCheckedModules();
            var compositionId = _goal146CompositionId?.Text.Trim() ?? string.Empty;
            _goal146LastResult = await Task.Run(() =>
                _goal146Controller.MaterializeAndQualifyAsync(root, compositionId));
            return "operatorUsesInProcessService=true; status=" + _goal146LastResult.Dashboard.Status;
        });
    }

    private void BindGoal146Catalog(FeatureModuleCatalogDocument catalog)
    {
        if (_goal146CoreModules is null || _goal146OptionalModules is null) return;
        _goal146CoreModules.Text = "Locked core modules" + Environment.NewLine + string.Join(Environment.NewLine,
            catalog.Modules.Where(module => module.Required).Select(module => "[locked] " + module.ModuleId));
        _goal146BindingChecks = true;
        try
        {
            _goal146OptionalModules.Items.Clear();
            foreach (var module in catalog.Modules.Where(module => module.Selectable).OrderBy(module => module.ModuleId, StringComparer.Ordinal))
                _goal146OptionalModules.Items.Add(module.ModuleId, true);
        }
        finally { _goal146BindingChecks = false; }
        BindGoal146SelectedModuleDetails();
    }

    private void Goal146SetAllOptional(bool value)
    {
        if (_goal146OptionalModules is null) return;
        _goal146BindingChecks = true;
        try
        {
            for (var index = 0; index < _goal146OptionalModules.Items.Count; index++)
                _goal146OptionalModules.SetItemChecked(index, value);
        }
        finally { _goal146BindingChecks = false; }
    }

    private void Goal146CaptureCheckedModules()
    {
        if (_goal146OptionalModules is null) return;
        _goal146Controller.SetSelectedOptionalModules(_goal146OptionalModules.CheckedItems.Cast<string>().ToList());
    }

    private void BindGoal146SelectedModuleDetails()
    {
        if (_goal146BindingChecks || _goal146ModuleDetails is null || _goal146OptionalModules?.SelectedItem is not string id
            || _goal146Controller.Catalog is null) return;
        var module = _goal146Controller.Catalog.Modules.Single(item => item.ModuleId == id);
        _goal146ModuleDetails.Text = string.Join(Environment.NewLine,
        [
            "moduleId=" + module.ModuleId,
            "title=" + module.Title,
            "dependencies=" + string.Join(",", module.Dependencies),
            "conflicts=" + string.Join(",", module.Conflicts),
            "runtimePrimitives=" + string.Join(",", module.RequiredRuntimePrimitives),
            "mutationLineage=" + string.Join(",", module.SourceLineage.OperationIds)
        ]);
    }

    private void BindGoal146ModuleComposer()
    {
        if (_goal146Results is null || _goal146Diagnostics is null) return;
        if (_goal146LastResult is null)
        {
            _goal146Results.Text = "Run Composition Matrix to produce the eight-composition Runtime qualification proof.";
            return;
        }
        var dashboard = _goal146LastResult.Dashboard;
        var selection = _goal146LastResult.Selection;
        _goal146Results.Text = string.Join(Environment.NewLine,
        [
            "status=" + dashboard.Status,
            "compositionCount=" + dashboard.CompositionCount,
            "passedCompositionCount=" + dashboard.PassedCompositionCount,
            "distinctPackageSha256Count=" + dashboard.DistinctPackageSha256Count,
            "distinctFinalStateHashCount=" + dashboard.DistinctFinalStateHashCount,
            "selectedCompositionId=" + selection.CompositionId,
            "packageSha256=" + selection.PackageSha256,
            "finalStateHash=" + selection.FinalStateHash,
            "semanticEffects=" + string.Join(",", selection.SemanticEffects),
            "runtimeAuthority=true; unityGameplayTruth=false"
        ]);
        _goal146Diagnostics.Text = "matrix=" + dashboard.PassedCompositionCount + "/" + dashboard.CompositionCount
                                   + "; replay=" + dashboard.AllFullReplaysEquivalent
                                   + "; orderIndependent=" + dashboard.AllOrderIndependenceProofsPassed;
    }

    private async Task Goal146RunAsync(Func<Task<string>> operation)
    {
        Goal146SetRunning(true);
        try
        {
            var message = await operation();
            if (_goal146Diagnostics is not null) _goal146Diagnostics.Text = Goal144Tail(message);
            BindGoal146ModuleComposer();
        }
        catch (Exception ex)
        {
            if (_goal146Diagnostics is not null) _goal146Diagnostics.Text = Goal144Tail("failed: " + ex.Message);
        }
        finally { Goal146SetRunning(false); }
    }

    private void Goal146SetRunning(bool running)
    {
        foreach (var button in _goal146Buttons) button.Enabled = !running;
        if (_goal146OptionalModules is not null) _goal146OptionalModules.Enabled = !running;
        if (_goal146CompositionId is not null) _goal146CompositionId.Enabled = !running;
        if (_goal146DisplayName is not null) _goal146DisplayName.Enabled = !running;
    }
}
