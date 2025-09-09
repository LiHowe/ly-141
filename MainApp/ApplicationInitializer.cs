using System.Reflection;
using Logger;
using MainApp.Initializers;
using MainApp.Windows;

namespace MainApp;

public static class ApplicationInitializer
{
    public static async Task Initialize()
    {
        var welcomeWindow = new WelcomeWindow(false); 
        var definitionTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IInitializer).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var type in definitionTypes)
        {
            var initializeMethod = type.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);
            var nameProp = type.GetProperty("Name", BindingFlags.Public | BindingFlags.Static);
            var name = nameProp?.GetValue(null)?.ToString() ?? "Unknown";
            welcomeWindow.RegisterLoadingTask(name, async () =>
            {
                try
                {
                    initializeMethod?.Invoke(null, null);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error($"ִ�г�ʼ������{name}ʱ����", ex);
                    return false;
                }
            });
        }

        welcomeWindow.Show();
        await welcomeWindow.StartLoadingAsync();
    }
}