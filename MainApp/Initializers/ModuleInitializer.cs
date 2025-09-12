using Core.Services;
using Module.Business.SG141;

namespace MainApp.Initializers;

public class ModuleInitializer : IInitializer
{
    public static Task Initialize()
    {
        ModuleManager moduleManager = new();
        ServiceManager serviceManager = new();
        ServiceLocator.SetServiceManager(serviceManager);
        serviceManager.RegisterService<ModuleManager>(moduleManager);
        //moduleManager.RegisterModuleAsync(new WeldBusinessModule());
        //moduleManager.RegisterModuleAsync(new RepairBusinessModule());
        moduleManager.RegisterModuleAsync(new SG141Module());
        return moduleManager.InitializeAllModulesAsync();
    }
}