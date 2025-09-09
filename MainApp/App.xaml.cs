using System.Windows;

namespace MainApp;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string MutexName = "WinM_SCADA_WPF";
    private Mutex _mutex;

    protected override async void OnStartup(StartupEventArgs e)
    {
        bool createdNew;
        _mutex = new Mutex(true, MutexName, out createdNew);

        if (!createdNew)
        {
            // 已有实例运行，直接退出
            Shutdown();
            return;
        }

        base.OnStartup(e);
        await ApplicationInitializer.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex = null;
        base.OnExit(e);
    }
}