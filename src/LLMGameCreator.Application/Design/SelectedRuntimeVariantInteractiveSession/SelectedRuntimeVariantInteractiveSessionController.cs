using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime.Abstractions;
using RuntimeSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.SelectedRuntimeVariantInteractiveSession;

public sealed class SelectedRuntimeVariantInteractiveSessionController
{
    private readonly ISelectedRuntimeVariantInteractiveSessionService _runtime;
    private readonly SelectedRuntimeVariantInteractiveSessionValidator _validator = new();
    private SelectedRuntimeVariantInteractiveSessionValidatedInput? _input;
    private SelectedRuntimeVariantInteractiveSessionStartRequest? _start;

    public SelectedRuntimeVariantInteractiveSessionController(
        ISelectedRuntimeVariantInteractiveSessionService runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    public RuntimeSession? Session { get; private set; }
    public SelectedRuntimeVariantInteractiveCheckpoint? Checkpoint { get; private set; }
    public SelectedRuntimeVariantInteractiveActionResult? LastActionResult { get; private set; }
    public SelectedRuntimeVariantInteractiveReplayResult? LastReplayResult { get; private set; }

    public RuntimeSession StartOrReset(string repositoryRootPath)
    {
        _input = _validator.Validate(
            repositoryRootPath,
            new SelectedRuntimeVariantInteractiveSessionRequest());
        _start = new SelectedRuntimeVariantInteractiveSessionStartRequest
        {
            SessionId = "goal144-winforms-live-session",
            CandidateId = _input.CandidateId,
            VariantKind = _input.VariantKind,
            PackagePath = _input.PackageRelativePath,
            PackageSha256 = _input.PackageSha256
        };
        Session = _runtime.StartSession(_input.Package, _start);
        Checkpoint = null;
        LastActionResult = null;
        LastReplayResult = null;
        return Session;
    }

    public SelectedRuntimeVariantInteractiveActionResult ExecuteSelected(string actionId)
    {
        EnsureStarted();
        LastActionResult = _runtime.ExecuteAction(_input!.Package, Session!, new()
        {
            ActionRequestId = "goal144-winforms-action-" + Session!.CurrentActionIndex.ToString("000"),
            SessionId = Session.SessionId,
            ActionIndex = Session.CurrentActionIndex,
            ActionId = actionId
        });
        return LastActionResult;
    }

    public SelectedRuntimeVariantInteractiveCheckpoint SaveCheckpoint()
    {
        EnsureStarted();
        Checkpoint = _runtime.SaveCheckpoint(
            Session!,
            "goal144-winforms-checkpoint",
            DateTime.UtcNow.ToString("O"));
        return Checkpoint;
    }

    public SelectedRuntimeVariantInteractiveReplayResult ReloadCheckpoint()
    {
        EnsureStarted();
        if (Checkpoint is null) throw new InvalidOperationException("Save a checkpoint first.");
        LastReplayResult = _runtime.ReloadCheckpoint(_input!.Package, _start!, Checkpoint);
        if (LastReplayResult.Passed) Session = LastReplayResult.Session;
        return LastReplayResult;
    }

    public SelectedRuntimeVariantInteractiveReplayResult ReplayVerify()
    {
        EnsureStarted();
        var final = _runtime.SaveCheckpoint(
            Session!,
            "goal144-winforms-replay",
            DateTime.UtcNow.ToString("O"));
        LastReplayResult = _runtime.ReloadCheckpoint(_input!.Package, _start!, final);
        return LastReplayResult;
    }

    private void EnsureStarted()
    {
        if (_input is null || _start is null || Session is null)
        {
            throw new InvalidOperationException("Start the Goal144 session first.");
        }
    }
}
