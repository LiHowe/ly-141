using MainApp.Menu;
using MainApp.Views;
using Core.Properties;

namespace MainApp;

public class MenuDefinition
{
    public static IMenuConfig[] Menus =>
    [
        // MenuItem
        //     .For<DashbaordView>()
        //     .WithText("看板")
        //     .WithImmediateInit()
        //     .WithImagePath("Resources/Images/history.png")
        //     .Build(),

        MenuItem
            .For<MonitorView>()
            .WithTextKey(nameof(Lang.MonitorMenu))
            .WithImmediateInit()
            .WithFirstShow()
            .WithImagePath("Resources/Images/history.png")
            .WithInitializer(async view =>
            {
              
                
            })
            .Build(),

        MenuItem
            .For<HistoryView>()
            .WithText("历史追溯")
            .WithImmediateInit()
            .WithImagePath("Resources/Images/history.png")
            .Build()
    ];
}