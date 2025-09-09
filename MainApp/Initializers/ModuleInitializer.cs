using Core.Services;
using Module.Business;
using Module.Business.Repair;

namespace MainApp.Initializers;

public class ModuleInitializer : IInitializer
{
    public static Task Initialize()
    {
        ModuleManager moduleManager = new();
        ServiceManager serviceManager = new();
        ServiceLocator.SetServiceManager(serviceManager);
        moduleManager.RegisterModuleAsync(new WeldBusinessModule());
        moduleManager.RegisterModuleAsync(new RepairBusinessModule());
        return moduleManager.InitializeAllModulesAsync();
    }
}