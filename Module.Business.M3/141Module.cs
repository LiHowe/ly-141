using Core.Models;

namespace Module.Business.M3;
/// <summary>
/// 新松M3项目逻辑
/// </summary>
public class M3Module: ModuleBase
{
    public override string ModuleId => "Module.Business.M3Module";
    public override string ModuleName => "新松M3项目逻辑";
    
    public override string Version => "1.0.0";
    
    public override string Description => "新松M3项目业务逻辑";
    
    public override List<Type> GetDatabaseTableTypes()
    {
        
        return new();
    }

    public override Task<bool> InitializeAsync()
    {
        return base.InitializeAsync();
    }
}