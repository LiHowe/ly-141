using System.Windows.Media;

namespace MainApp.Menu;

// 菜单配置类
// 菜单配置类 - 实现基础接口
public class MenuConfig : IMenuConfig
{
    public string Text { get; internal set; }
    public ImageSource Image { get; internal set; }

    public string ImagePath { get; internal set; }
    public bool UseCache { get; internal set; } = true;
    public int Sort { get; internal set; } = 0;
    public Type ViewType { get; internal set; }
    public Delegate Initializer { get; internal set; }
    public bool ImmediateInit { get; internal set; } = false;

    /// <summary>
    ///     是否在主页加载完成后显示
    /// </summary>
    public bool ShowOnLoaded { get; internal set; } = false;

    public List<Type> ViewModels { get; internal set; } = new();
}