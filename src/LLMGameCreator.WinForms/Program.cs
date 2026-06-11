namespace LLMGameCreator.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var compositionRoot = new CompositionRoot();
        Application.Run(compositionRoot.ResolveMainForm());
    }
}
