using Core.Localization;
using Core.Properties;
using HandyControl.Tools;
using System.Globalization;
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

		// 加载上次保存的语言设置，默认为 zh-CN
		string savedCulture = Settings.Default.SelectedCulture ?? "zh-CN";
		// 1) 初始化默认 Provider（指向 Core 里生成的 Lang 类型）
		LocalizationProvider.Default.LangType = typeof(Lang); // Lang 为 resx 生成的类

		// 2) 设置默认语言（中文）
		LocalizationProvider.Default.SetCulture(new CultureInfo(savedCulture));
    
		await ApplicationInitializer.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex = null;
        base.OnExit(e);
    }

	public static void SwitchLanguage(string cultureCode)
	{
        ConfigHelper.Instance.SetLang(cultureCode);
		LocalizationProvider.Default.SetCulture(new CultureInfo(cultureCode));
	}
}