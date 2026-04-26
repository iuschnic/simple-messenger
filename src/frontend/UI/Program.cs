namespace UI;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(AppBootstrapper.BuildMainForm());
    }
}
