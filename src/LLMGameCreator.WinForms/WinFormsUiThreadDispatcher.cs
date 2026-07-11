namespace LLMGameCreator.WinForms;

public static class WinFormsUiThreadDispatcher
{
    public static void Post(Control owner, Action operation)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(operation);

        if (!CanDispatch(owner))
        {
            return;
        }

        if (!owner.InvokeRequired)
        {
            operation();
            return;
        }

        BeginInvoke(owner, () =>
        {
            if (CanDispatch(owner))
            {
                operation();
            }
        });
    }

    public static void PostAsync(
        Control owner,
        Func<Task> operation,
        Action<Exception> onError)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(onError);

        if (!CanDispatch(owner))
        {
            return;
        }

        if (!owner.InvokeRequired)
        {
            ExecuteAsync(owner, operation, onError);
            return;
        }

        BeginInvoke(owner, () => ExecuteAsync(owner, operation, onError));
    }

    private static async void ExecuteAsync(
        Control owner,
        Func<Task> operation,
        Action<Exception> onError)
    {
        if (!CanDispatch(owner))
        {
            return;
        }

        try
        {
            await operation().ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            if (CanDispatch(owner))
            {
                onError(exception);
            }
        }
    }

    private static void BeginInvoke(Control owner, Action operation)
    {
        try
        {
            owner.BeginInvoke(operation);
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException) when (!CanDispatch(owner))
        {
        }
    }

    private static bool CanDispatch(Control owner) =>
        !owner.IsDisposed && !owner.Disposing && owner.IsHandleCreated;
}
