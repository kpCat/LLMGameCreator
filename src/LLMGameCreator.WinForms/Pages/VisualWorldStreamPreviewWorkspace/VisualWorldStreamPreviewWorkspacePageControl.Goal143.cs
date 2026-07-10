using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.SelectedRuntimeVariantPlayerAdapter;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using LLMGameCreator.Runtime;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private readonly SelectedRuntimeVariantPlayerAdapterOperatorRunner _goal143OperatorRunner =
        new(new SelectedRuntimeVariantPlayerAdapterService(
            RuntimeBackedPlayerCommandRoundtripService.CreateDefault()));
    private readonly VisualWorldStreamPreviewSelectedRuntimeVariantPlayerAdapterInspector
        _goal143Inspector = new();
    private TabPage? _goal143PlayerAdapterTabPage;
    private TextBox? _goal143PlayerAdapterStatusTextBox;
    private TextBox? _goal143PlayerAdapterCommandTextBox;
    private TextBox? _goal143PlayerAdapterPathTextBox;
    private TextBox? _goal143PlayerAdapterOutputTextBox;
    private Button? _goal143BuildButton;

    private void ConfigureGoal143SelectedRuntimeVariantPlayerAdapterPanel()
    {
        _goal143PlayerAdapterTabPage = new TabPage
        {
            Name = "_goal143PlayerAdapterTabPage",
            Text = "Goal143 PlayerAdapter"
        };
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 6
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 250F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Goal143 Selected Runtime Variant PlayerAdapter"
        };
        _goal143PlayerAdapterStatusTextBox = Goal132ReadOnlyTextBox(multiline: true);
        _goal143PlayerAdapterCommandTextBox = Goal132ReadOnlyTextBox(multiline: false);
        _goal143PlayerAdapterPathTextBox = Goal132ReadOnlyTextBox(multiline: true);
        _goal143PlayerAdapterOutputTextBox = Goal132ReadOnlyTextBox(multiline: true);
        _goal143BuildButton = Goal132Button("Build Selected Variant PlayerAdapter");

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 4, 0, 0)
        };
        buttons.Controls.Add(_goal143BuildButton);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_goal143PlayerAdapterStatusTextBox, 0, 1);
        layout.Controls.Add(_goal143PlayerAdapterCommandTextBox, 0, 2);
        layout.Controls.Add(_goal143PlayerAdapterPathTextBox, 0, 3);
        layout.Controls.Add(buttons, 0, 4);
        layout.Controls.Add(_goal143PlayerAdapterOutputTextBox, 0, 5);
        _goal143PlayerAdapterTabPage.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal143PlayerAdapterTabPage);
    }

    private void WireGoal143SelectedRuntimeVariantPlayerAdapterEvents()
    {
        if (_goal143BuildButton is not null)
        {
            _goal143BuildButton.Click += async (_, _) =>
                await RunGoal143SelectedRuntimeVariantPlayerAdapterAsync();
        }
    }

    private void BindGoal143SelectedRuntimeVariantPlayerAdapter()
    {
        if (_goal143PlayerAdapterStatusTextBox is null
            || _goal143PlayerAdapterCommandTextBox is null
            || _goal143PlayerAdapterPathTextBox is null
            || _goal143PlayerAdapterOutputTextBox is null)
        {
            return;
        }

        var root = FindProjectRoot();
        if (root is null)
        {
            Goal143SetStatus("Repository root was not found.");
            return;
        }

        var dashboard = _goal143Inspector.Load(root);
        _goal143PlayerAdapterStatusTextBox.Text = string.Join(Environment.NewLine,
        [
            "status=" + dashboard.Status,
            "selectedCandidateId=" + dashboard.SelectedCandidateId,
            "selectedVariantKind=" + dashboard.SelectedVariantKind,
            "selectedScore=" + dashboard.SelectedScore,
            "packageHashMatch=" + dashboard.PackageHashMatch.ToString().ToLowerInvariant(),
            "finalStateHashMatch=" + dashboard.FinalStateHashMatch.ToString().ToLowerInvariant(),
            "frameCount=" + dashboard.FrameCount,
            "selectedVariantEffectVisible="
                + dashboard.SelectedVariantEffectVisible.ToString().ToLowerInvariant(),
            "noBalancedBaselineFallback="
                + dashboard.NoBalancedBaselineFallback.ToString().ToLowerInvariant(),
            "unitySmokePassed=" + dashboard.UnitySmokePassed.ToString().ToLowerInvariant(),
            "runtimeAuthority=" + dashboard.RuntimeAuthority.ToString().ToLowerInvariant(),
            "projectionOnly=" + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            "unityGameplayTruth=" + dashboard.UnityGameplayTruth.ToString().ToLowerInvariant(),
            "accepted=" + dashboard.Accepted.ToString().ToLowerInvariant()
        ]);
        _goal143PlayerAdapterCommandTextBox.Text = "normalCommand=" + dashboard.NormalCommand;
        _goal143PlayerAdapterPathTextBox.Text = "handoffPath=" + dashboard.HandoffPath;
        if (string.IsNullOrWhiteSpace(_goal143PlayerAdapterOutputTextBox.Text))
        {
            _goal143PlayerAdapterOutputTextBox.Text =
                "No Goal143 operator diagnostic summary captured yet.";
        }
    }

    private async Task RunGoal143SelectedRuntimeVariantPlayerAdapterAsync()
    {
        var root = FindProjectRoot();
        if (root is null)
        {
            Goal143SetStatus("Repository root was not found.");
            return;
        }

        Goal143SetRunning(true);
        try
        {
            Goal143SetStatus("running inProcess=true");
            var write = await Task.Run(() => _goal143OperatorRunner.RunAsync(root));
            Goal143SetOutput(string.Join(Environment.NewLine,
            [
                "operatorUsesInProcessService=true",
                "operatorStartsCompilerProcess=false",
                "operatorStartsDotnetTestProcess=false",
                "previousArtifactsPreservedOnFailure=true",
                "status=" + write.Dashboard.Status,
                "selectedCandidateId=" + write.Dashboard.SelectedCandidateId,
                "selectedVariantKind=" + write.Dashboard.SelectedVariantKind,
                "selectedScore=" + write.Dashboard.SelectedScore,
                "packageHashMatch=" + write.Dashboard.PackageHashMatch.ToString().ToLowerInvariant(),
                "finalStateHashMatch="
                    + write.Dashboard.FinalStateHashMatch.ToString().ToLowerInvariant(),
                "frameCount=" + write.Dashboard.FrameCount,
                "selectedVariantEffectVisible="
                    + write.Dashboard.SelectedVariantEffectVisible.ToString().ToLowerInvariant(),
                "noBalancedBaselineFallback="
                    + write.Dashboard.NoBalancedBaselineFallback.ToString().ToLowerInvariant(),
                "unitySmokePassed=" + write.Dashboard.UnitySmokePassed.ToString().ToLowerInvariant()
            ]));
            RefreshWorkspace();
            Goal143SetStatus("completed status=" + write.Dashboard.Status);
        }
        catch (Exception ex)
        {
            Goal143SetOutput("failed: " + Goal143DiagnosticTail(ex.Message));
            Goal143SetStatus("failed: " + ex.Message);
        }
        finally
        {
            Goal143SetRunning(false);
        }
    }

    private void Goal143SetRunning(bool running)
    {
        if (_goal143BuildButton is not null)
        {
            _goal143BuildButton.Enabled = !running;
        }
    }

    private void Goal143SetStatus(string text)
    {
        if (_goal143PlayerAdapterStatusTextBox is not null)
        {
            _goal143PlayerAdapterStatusTextBox.Text = text;
        }
    }

    private void Goal143SetOutput(string text)
    {
        if (_goal143PlayerAdapterOutputTextBox is not null)
        {
            _goal143PlayerAdapterOutputTextBox.Text = Goal143DiagnosticTail(text);
        }
    }

    private static string Goal143DiagnosticTail(string text) =>
        string.Join(
            Environment.NewLine,
            (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n')
                .Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .TakeLast(80));
}
