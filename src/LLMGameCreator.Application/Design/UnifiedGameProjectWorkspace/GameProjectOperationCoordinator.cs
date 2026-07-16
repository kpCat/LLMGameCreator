using System.Text;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public static class GameProjectOperationKinds
{
    public const string AuthoringSave = "authoring_save";
    public const string Build = "build";
    public const string Standalone = "standalone";
    public const string RegenerationPreview = "regeneration_preview";
    public const string RegenerationApply = "regeneration_apply";
    public const string WorldHistoryRollbackPreview = "world_history_rollback_preview";
    public const string WorldHistoryRollbackApply = "world_history_rollback_apply";
    public const string Recovery = "recovery";
}

public interface IGameProjectOperationCoordinator
{
    string ActiveOperationKind { get; }
    bool IsBusy { get; }
    GameProjectOperationLease TryAcquire(string projectFolder, string operationKind);
    GameProjectOperationLease TryAcquireChild(
        GameProjectOperationLease owner,
        string projectFolder,
        string operationKind);
    bool IsCurrent(GameProjectOperationLease lease, string projectFolder);
}

public sealed class GameProjectOperationCoordinator : IGameProjectOperationCoordinator
{
    public const string MutationLockRelativePath = ".llmgc/operations/project-mutation.lock";

    private readonly object _gate = new();
    private GameProjectOperationLease? _active;

    public string ActiveOperationKind
    {
        get { lock (_gate) return _active is { Acquired: true, IsDisposed: false } active
            ? active.OperationKind : string.Empty; }
    }

    public bool IsBusy => ActiveOperationKind.Length > 0;

    public GameProjectOperationLease TryAcquire(string projectFolder, string operationKind)
    {
        var normalizedProject = NormalizeProject(projectFolder);
        RequireKind(operationKind);
        lock (_gate)
        {
            if (_active is { Acquired: true, IsDisposed: false } active)
                return GameProjectOperationLease.Rejected(operationKind, normalizedProject,
                    "project_operation.busy:" + active.OperationKind);

            FileStream? projectLock = null;
            try
            {
                projectLock = AcquireProjectLock(normalizedProject, operationKind);
                var operationId = Guid.NewGuid().ToString("N");
                var lease = new GameProjectOperationLease(
                    this,
                    operationId,
                    operationKind,
                    normalizedProject,
                    operationId,
                    projectLock,
                    isChild: false,
                    Release);
                _active = lease;
                return lease;
            }
            catch (IOException)
            {
                projectLock?.Dispose();
                return GameProjectOperationLease.Rejected(operationKind, normalizedProject,
                    "project_operation.busy:external");
            }
            catch (UnauthorizedAccessException)
            {
                projectLock?.Dispose();
                return GameProjectOperationLease.Rejected(operationKind, normalizedProject,
                    "project_operation.lock_unavailable");
            }
        }
    }

    public GameProjectOperationLease TryAcquireChild(
        GameProjectOperationLease owner,
        string projectFolder,
        string operationKind)
    {
        ArgumentNullException.ThrowIfNull(owner);
        var normalizedProject = NormalizeProject(projectFolder);
        RequireKind(operationKind);
        lock (_gate)
        {
            if (!OwnsCurrent(owner))
                return GameProjectOperationLease.Rejected(operationKind, normalizedProject,
                    "project_operation.lease_invalid");

            FileStream? childLock = null;
            try
            {
                if (!PathEquals(normalizedProject, owner.NormalizedProjectFolder))
                    childLock = AcquireProjectLock(normalizedProject, operationKind);
                return new GameProjectOperationLease(
                    this,
                    Guid.NewGuid().ToString("N"),
                    operationKind,
                    normalizedProject,
                    owner.OwnerOperationId,
                    childLock,
                    isChild: true,
                    _ => { });
            }
            catch (IOException)
            {
                childLock?.Dispose();
                return GameProjectOperationLease.Rejected(operationKind, normalizedProject,
                    "project_operation.busy:external");
            }
            catch (UnauthorizedAccessException)
            {
                childLock?.Dispose();
                return GameProjectOperationLease.Rejected(operationKind, normalizedProject,
                    "project_operation.lock_unavailable");
            }
        }
    }

    public bool IsCurrent(GameProjectOperationLease lease, string projectFolder)
    {
        ArgumentNullException.ThrowIfNull(lease);
        var normalizedProject = NormalizeProject(projectFolder);
        lock (_gate)
            return lease.Acquired
                   && !lease.IsDisposed
                   && ReferenceEquals(lease.Coordinator, this)
                   && PathEquals(lease.NormalizedProjectFolder, normalizedProject)
                   && OwnsCurrent(lease);
    }

    private bool OwnsCurrent(GameProjectOperationLease lease) =>
        _active is { Acquired: true, IsDisposed: false } active
        && ReferenceEquals(active.Coordinator, this)
        && string.Equals(active.OwnerOperationId, lease.OwnerOperationId, StringComparison.Ordinal);

    private void Release(GameProjectOperationLease lease)
    {
        lock (_gate)
        {
            if (ReferenceEquals(_active, lease)) _active = null;
        }
    }

    private static FileStream AcquireProjectLock(string projectFolder, string operationKind)
    {
        var lockPath = Confined(projectFolder, MutationLockRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(lockPath)!);
        var stream = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None,
            4096, FileOptions.WriteThrough);
        try
        {
            var text = operationKind + Environment.NewLine;
            var bytes = new UTF8Encoding(false).GetBytes(text);
            stream.SetLength(0);
            stream.Write(bytes);
            stream.Flush(flushToDisk: true);
            stream.Position = 0;
            return stream;
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    private static string NormalizeProject(string projectFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFolder);
        return Path.GetFullPath(projectFolder).TrimEnd(
            Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string Confined(string root, string relative)
    {
        var fullRoot = NormalizeProject(root);
        var path = Path.GetFullPath(Path.Combine(fullRoot,
            relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!PathEquals(path, fullRoot)
            && !path.StartsWith(fullRoot + Path.DirectorySeparatorChar, PathComparison))
            throw new InvalidOperationException("project_operation.path_escape");
        return path;
    }

    private static bool PathEquals(string left, string right) =>
        string.Equals(Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            PathComparison);

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private static void RequireKind(string operationKind)
    {
        if (string.IsNullOrWhiteSpace(operationKind))
            throw new ArgumentException("operation kind is required", nameof(operationKind));
    }
}

public sealed class GameProjectOperationLease : IDisposable
{
    private FileStream? _projectLock;
    private Action<GameProjectOperationLease>? _release;
    private int _disposed;

    internal GameProjectOperationLease(
        GameProjectOperationCoordinator coordinator,
        string operationId,
        string operationKind,
        string normalizedProjectFolder,
        string ownerOperationId,
        FileStream? projectLock,
        bool isChild,
        Action<GameProjectOperationLease> release)
    {
        Coordinator = coordinator;
        OperationId = operationId;
        OperationKind = operationKind;
        NormalizedProjectFolder = normalizedProjectFolder;
        OwnerOperationId = ownerOperationId;
        _projectLock = projectLock;
        IsChild = isChild;
        _release = release;
        Acquired = true;
    }

    private GameProjectOperationLease(
        string operationKind,
        string normalizedProjectFolder,
        string diagnostic)
    {
        OperationKind = operationKind;
        NormalizedProjectFolder = normalizedProjectFolder;
        Diagnostic = diagnostic;
    }

    public string OperationId { get; } = string.Empty;
    public string OperationKind { get; } = string.Empty;
    public string NormalizedProjectFolder { get; } = string.Empty;
    public bool Acquired { get; }
    public string Diagnostic { get; } = string.Empty;
    public bool IsDisposed => Volatile.Read(ref _disposed) != 0;
    public bool HoldsProjectMutationLock => Acquired && !IsDisposed && (_projectLock is not null || IsChild);
    internal string OwnerOperationId { get; } = string.Empty;
    internal bool IsChild { get; }
    internal GameProjectOperationCoordinator? Coordinator { get; }

    internal static GameProjectOperationLease Rejected(
        string operationKind,
        string normalizedProjectFolder,
        string diagnostic) => new(operationKind, normalizedProjectFolder, diagnostic);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        var lockPath = _projectLock?.Name;
        try
        {
            _projectLock?.Dispose();
            if (!string.IsNullOrWhiteSpace(lockPath) && File.Exists(lockPath))
            {
                try { File.Delete(lockPath); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
        finally
        {
            _projectLock = null;
            var release = Interlocked.Exchange(ref _release, null);
            release?.Invoke(this);
        }
    }
}
