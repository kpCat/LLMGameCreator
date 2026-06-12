using System.Text.Json;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Scripting;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratorLibraryPrototypeLuaTabControl : UserControl
{
    private IPrototypeLuaExecutor? _executor;
    private IPrototypeLuaPatchArtifactService? _artifactService;
    private IGamePackagePatchService? _patchService;
    private GeneratedArtifactRecord? _createdArtifact;

    public GeneratorLibraryPrototypeLuaTabControl()
    {
        InitializeComponent();
        WireEvents();
        _sourceTextBox.Text = """
        data:extend({
          {
            type = "tile",
            id = "tile/grass",
            name = "Grass",
            walkable = true,
            movement_cost = 1.0,
            asset_id = "asset/tile/grass"
          }
        })
        """;
    }

    public Func<Task>? PatchArtifactCreated { get; set; }

    public void Configure(
        IPrototypeLuaExecutor executor,
        IPrototypeLuaPatchArtifactService artifactService,
        IGamePackagePatchService patchService)
    {
        _executor = executor;
        _artifactService = artifactService;
        _patchService = patchService;
    }

    private void WireEvents()
    {
        _runButton.Click += async (_, _) => await RunPrototypeLuaAsync();
        _createArtifactButton.Click += async (_, _) => await CreatePatchArtifactAsync(false);
        _dryRunCreatedButton.Click += async (_, _) => await DryRunCreatedPatchAsync();
    }

    private async Task RunPrototypeLuaAsync()
    {
        if (_executor == null)
        {
            SetStatus("Runtime services are not available.");
            return;
        }

        try
        {
            var result = await _executor.ExecuteAsync(BuildExecutionRequest(), CancellationToken.None).ConfigureAwait(true);
            _diagnosticsTextBox.Text = FormatDiagnostics(result.Diagnostics);
            _declarationsTextBox.Text = FormatDeclarations(result.Declarations);
            SetStatus(result.Success ? $"Captured {result.Declarations.Count} Prototype Lua declarations." : "Prototype Lua diagnostics contain errors.");
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private async Task CreatePatchArtifactAsync(bool dryRun)
    {
        if (_artifactService == null)
        {
            SetStatus("Runtime services are not available.");
            return;
        }

        try
        {
            var result = await _artifactService.CreatePatchArtifactFromPrototypeLuaAsync(new PrototypeLuaPatchArtifactRequest
            {
                ScriptId = NormalizeScriptId(),
                Title = "Prototype Lua",
                Source = _sourceTextBox.Text,
                DryRun = dryRun
            }, CancellationToken.None).ConfigureAwait(true);

            _createdArtifact = result.PatchArtifact;
            _artifactIdTextBox.Text = result.PatchArtifact?.Id ?? string.Empty;
            _diagnosticsTextBox.Text = FormatValidationResults(result.ValidationResults);
            _declarationsTextBox.Text = result.PatchArtifact?.Json ?? string.Empty;
            SetStatus(result.Message);
            if (result.Saved && PatchArtifactCreated != null)
            {
                await PatchArtifactCreated().ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private async Task DryRunCreatedPatchAsync()
    {
        if (_patchService == null || _createdArtifact == null)
        {
            SetStatus("Create a Prototype Lua patch artifact first.");
            return;
        }

        try
        {
            var result = await _patchService.DryRunPatchArtifactAsync(_createdArtifact.Id, CancellationToken.None).ConfigureAwait(true);
            _diagnosticsTextBox.Text = FormatDryRunResult(result);
            SetStatus(result.Message);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private PrototypeLuaExecutionRequest BuildExecutionRequest()
    {
        return new PrototypeLuaExecutionRequest
        {
            ScriptId = NormalizeScriptId(),
            Source = _sourceTextBox.Text
        };
    }

    private string NormalizeScriptId()
    {
        return string.IsNullOrWhiteSpace(_scriptIdTextBox.Text) ? "inline" : _scriptIdTextBox.Text.Trim();
    }

    private void SetStatus(string status)
    {
        _statusLabel.Text = status;
    }

    private static string FormatDiagnostics(IReadOnlyList<PrototypeLuaDiagnostic> diagnostics)
    {
        return diagnostics.Count == 0
            ? "No diagnostics."
            : string.Join(Environment.NewLine, diagnostics.Select(item => $"{item.Severity} {item.Code}: {item.Message} ({item.Target})"));
    }

    private static string FormatDeclarations(IReadOnlyList<PrototypeLuaDeclaration> declarations)
    {
        return declarations.Count == 0
            ? "No declarations captured."
            : JsonSerializer.Serialize(declarations.Select(item => new
            {
                item.SourceIndex,
                item.Type,
                item.Id,
                Json = item.Json
            }), new JsonSerializerOptions { WriteIndented = true });
    }

    private static string FormatValidationResults(IReadOnlyList<GeneratedArtifactValidationResultRecord> results)
    {
        return results.Count == 0
            ? "No validation results."
            : string.Join(Environment.NewLine, results.Select(item => $"{item.Severity} {item.Code}: {item.Message} ({item.Target})"));
    }

    private static string FormatDryRunResult(GamePackagePatchDryRunResult result)
    {
        var lines = new List<string>
        {
            result.Message,
            $"Can apply: {result.CanApply}",
            string.Empty,
            "Diff:"
        };
        lines.AddRange(result.DiffLines.Select(line => $"{line.ChangeKind} {line.Operation} {line.Target}: {line.Message}"));
        lines.Add(string.Empty);
        lines.Add("Validation:");
        lines.AddRange(result.ValidationIssues.Select(issue => $"{issue.Severity} {issue.Code}: {issue.Message} ({issue.TargetId})"));
        return string.Join(Environment.NewLine, lines);
    }
}

