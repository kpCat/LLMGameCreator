namespace LLMGameCreator.WinForms.Pages;

public interface IEditorPageNavigationService { event EventHandler<string>? NavigationRequested; void Request(string pageId); }
public sealed class EditorPageNavigationService : IEditorPageNavigationService
{
    public event EventHandler<string>? NavigationRequested;
    public void Request(string pageId) { if (!string.IsNullOrWhiteSpace(pageId)) NavigationRequested?.Invoke(this, pageId); }
}
