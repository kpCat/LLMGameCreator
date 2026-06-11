namespace LLMGameCreator.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        global::System.Windows.Forms.ApplicationConfiguration.Initialize();
        using var compositionRoot = new CompositionRoot();
        global::System.Windows.Forms.Application.Run(compositionRoot.ResolveMainForm());
    }
}
