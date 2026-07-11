using LLMGameCreator.Runtime.Abstractions;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.ProductLineInteractiveSessionMatrix;

public sealed class ProductLineInteractiveSessionSelectionController
{
    private readonly ISelectedRuntimeVariantInteractiveSessionService _runtime;
    private readonly Goal142CandidateDiscovery _discovery;
    private Goal142CandidateDiscoveryResult? _matrix;
    private Goal142DiscoveredCandidate? _selected;
    private SelectedRuntimeVariantInteractiveSessionStartRequest? _start;

    public ProductLineInteractiveSessionSelectionController(
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        Goal142CandidateDiscovery? discovery = null)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _discovery = discovery ?? new Goal142CandidateDiscovery();
    }

    public IReadOnlyList<ProductLineInteractiveSessionCandidate> Candidates =>
        _matrix?.Candidates.Select(candidate => candidate.Candidate).ToList() ?? [];

    public string SelectedCandidateId => _selected?.Candidate.CandidateId ?? string.Empty;
    public RuntimeInteractiveSession? Session { get; private set; }
    public SelectedRuntimeVariantInteractiveCheckpoint? Checkpoint { get; private set; }
    public SelectedRuntimeVariantInteractiveActionResult? LastActionResult { get; private set; }
    public SelectedRuntimeVariantInteractiveReplayResult? LastReplayResult { get; private set; }

    public IReadOnlyList<ProductLineInteractiveSessionCandidate> LoadCandidateMatrix(string repositoryRootPath)
    {
        _matrix = _discovery.Discover(repositoryRootPath, ProductLineInteractiveSessionMatrixVocabulary.Goal142Root);
        SelectCandidate(_matrix.DefaultSelectedCandidateId);
        return Candidates;
    }

    public void SelectCandidate(string candidateId)
    {
        if (_matrix is null) throw new InvalidOperationException("Load the Goal145 candidate matrix first.");
        var next = ProductLineInteractiveSessionMatrixService.ResolveSelectable(_matrix.Candidates, candidateId);
        if (_selected?.Candidate.CandidateId == next.Candidate.CandidateId) return;
        _selected = next;
        Session = null;
        Checkpoint = null;
        LastActionResult = null;
        LastReplayResult = null;
        _start = null;
    }

    public RuntimeInteractiveSession StartSelected()
    {
        if (_selected is null) throw new InvalidOperationException("Select a passing Goal145 candidate first.");
        _start = new SelectedRuntimeVariantInteractiveSessionStartRequest
        {
            SessionId = "goal145-winforms-" + _selected.Candidate.CandidateId,
            CandidateId = _selected.Candidate.CandidateId,
            VariantKind = _selected.Candidate.VariantKind,
            PackagePath = _selected.Candidate.PackagePath,
            PackageSha256 = _selected.Candidate.PackageSha256
        };
        Session = _runtime.StartSession(_selected.Package, _start);
        Checkpoint = null;
        LastActionResult = null;
        LastReplayResult = null;
        return Session;
    }

    public SelectedRuntimeVariantInteractiveActionResult ExecuteSelectedAction(string actionId)
    {
        EnsureStarted();
        LastActionResult = _runtime.ExecuteAction(_selected!.Package, Session!, new()
        {
            ActionRequestId = Session!.SessionId + "-action-" + Session.CurrentActionIndex.ToString("000"),
            SessionId = Session.SessionId,
            ActionIndex = Session.CurrentActionIndex,
            ActionId = actionId
        });
        return LastActionResult;
    }

    public SelectedRuntimeVariantInteractiveCheckpoint SaveCheckpoint()
    {
        EnsureStarted();
        Checkpoint = _runtime.SaveCheckpoint(Session!, "goal145-winforms-checkpoint", DateTime.UtcNow.ToString("O"));
        return Checkpoint;
    }

    public SelectedRuntimeVariantInteractiveReplayResult ReloadCheckpoint()
    {
        EnsureStarted();
        if (Checkpoint is null) throw new InvalidOperationException("Save a selected-candidate checkpoint first.");
        LastReplayResult = _runtime.ReloadCheckpoint(_selected!.Package, _start!, Checkpoint);
        if (LastReplayResult.Passed) Session = LastReplayResult.Session;
        return LastReplayResult;
    }

    public SelectedRuntimeVariantInteractiveReplayResult ReplayVerify()
    {
        EnsureStarted();
        var final = _runtime.SaveCheckpoint(Session!, "goal145-winforms-full-replay", DateTime.UtcNow.ToString("O"));
        LastReplayResult = _runtime.ReloadCheckpoint(_selected!.Package, _start!, final);
        return LastReplayResult;
    }

    private void EnsureStarted()
    {
        if (_selected is null || _start is null || Session is null)
        {
            throw new InvalidOperationException("Start the selected Goal145 Runtime session first.");
        }
    }
}
