using Core.Models;

namespace Module.Business.Repair;

/// <summary>
/// 返修业务模块
/// </summary>
public class RepairBusinessModule: ModuleBase
{
    public override string ModuleId => "Module.Business.RepairBusinessModule";
    public override string ModuleName => "返修业务逻辑";
    public override string Version => "1.0.0";

    public override List<Type> GetDatabaseTableTypes()
    {
        return new()
        {
            typeof(RepairRecord)
        };
    }
}