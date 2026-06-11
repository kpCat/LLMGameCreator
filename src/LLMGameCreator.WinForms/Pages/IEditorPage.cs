namespace LLMGameCreator.WinForms.Pages;

public interface IEditorPage
{
    string Id { get; }
    string Title { get; }
    int SortOrder { get; }
    Control View { get; }
    void OnActivated();
}

public interface IEditorPageRegistry
{
    IReadOnlyList<IEditorPage> Pages { get; }
}

public sealed class EditorPageRegistry : IEditorPageRegistry
{
    public EditorPageRegistry(IEnumerable<IEditorPage> pages)
    {
        Pages = pages.OrderBy(p => p.SortOrder).ToList();
    }

    public IReadOnlyList<IEditorPage> Pages { get; }
}
