using System.Windows.Controls;

namespace MainApp.Menu;

public static class MenuItem
{
    public static MenuBuilder<TControl> For<TControl>() where TControl : UserControl, new()
    {
        return new MenuBuilder<TControl>();
    }
}