using System.IO;
using System.Windows.Controls;
using Core;
using Core.Models;

namespace Module.Business.SG141;
/// <summary>
/// 新松M3项目逻辑
/// </summary>
public class SG141Module: ModuleBase
{
    public override string ModuleId => "Module.Business.SG141Module";
    public override string ModuleName => "业务设置";
    
    public override string Version => "1.0.0";
    
    public override string Description => "LY SG141 Monitor Line Logic";

    public static string SettingFilePath = Path.Combine(Constants.ConfigRootPath, "SG141Settings.conf");
    
    public override List<Type> GetDatabaseTableTypes()
    {
        
        return new();
    }

    public override Task<bool> InitializeAsync()
    {
        return base.InitializeAsync();
    }

	public override bool HasSettingsPage => true;

	public override UserControl? GetSettingsPage()
	{
        return new SG141SettingsView();
	}

	public override Type? GetSettingsPageType()
	{
		return typeof(SG141SettingsView);
	}
}