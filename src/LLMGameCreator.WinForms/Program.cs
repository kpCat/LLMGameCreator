namespace LLMGameCreator.WinForms;

internal static class Program
{
    [global::System.STAThreadAttribute]
    private static void Main()
    {
        global::System.Windows.Forms.Application.EnableVisualStyles();
        global::System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

        using var compositionRoot = new CompositionRoot();
        global::System.Windows.Forms.Application.Run(compositionRoot.ResolveMainForm());
    }
}
