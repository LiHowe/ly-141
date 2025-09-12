using MainApp.Menu;
using MainApp.Views;
using Core.Properties;
using Module.Business.SG141;

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
            // .WithText("监控")
            .WithTextKey(nameof(Lang.MonitorMenu))
            .WithImmediateInit()
            .WithFirstShow()
            .WithImagePath("Resources/Images/history.png")
            // .WithInitializer(async view =>
            // {
            //     Console.WriteLine("Enter initializer");
            //     // view.Container.AdjustLayout(1,1);
            //     // SG141MainView v = new();
            //     // view.AddControl(v, 0,0);
            // })
            .Build(),

        MenuItem
            .For<HistoryView>()
            .WithTextKey(nameof(Lang.HistoryMenu))
			.WithImmediateInit()
            .WithImagePath("Resources/Images/history.png")
            .Build()
    ];
}