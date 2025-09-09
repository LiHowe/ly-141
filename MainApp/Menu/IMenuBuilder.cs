using System.Windows.Controls;
using System.Windows.Media;

namespace MainApp.Menu;

// 菜单配置构建器接口
public interface IMenuBuilder<TBuilder, TControl>
    where TBuilder : IMenuBuilder<TBuilder, TControl>
    where TControl : UserControl, new()
{
    TBuilder WithText(string text);
    TBuilder WithImage(ImageSource image);
    TBuilder WithImagePath(string imagePath);

    TBuilder WithCache(bool useCache = true);
    TBuilder WithInitializer(Action<TControl> initializer);
    TBuilder WithViewModels(params Type[] viewModels);
    TBuilder WithImmediateInit(bool immediateInit = true);
    TBuilder WithSort(int sort);
    IMenuConfig Build();
}