using System.Windows.Controls;
using System.Windows.Media;

namespace MainApp.Menu;

// 菜单构建器 - 针对 WPF 的 UserControl 视图
public class MenuBuilder<TControl> : IMenuBuilder<MenuBuilder<TControl>, TControl>
    where TControl : UserControl, new()
{
    private readonly MenuConfig _config = new();

    public MenuBuilder()
    {
        _config.ViewType = typeof(TControl);
    }

    public MenuBuilder<TControl> WithText(string text)
    {
        _config.Text = text;
        return this;
    }

    public MenuBuilder<TControl> WithImage(ImageSource image)
    {
        _config.Image = image;
        return this;
    }

    public MenuBuilder<TControl> WithImagePath(string imagePath)
    {
        _config.ImagePath = imagePath;
        return this;
    }

    public MenuBuilder<TControl> WithCache(bool useCache = true)
    {
        _config.UseCache = useCache;
        return this;
    }

    public MenuBuilder<TControl> WithInitializer(Action<TControl> initializer)
    {
        _config.Initializer = initializer;
        return this;
    }

    public MenuBuilder<TControl> WithViewModels(params Type[] viewModels)
    {
        _config.ViewModels = new List<Type>(viewModels);
        return this;
    }

	public MenuBuilder<TControl> WithTextKey(string key)
	{
		_config.TextKey = key;
		return this;
	}

	public MenuBuilder<TControl> WithImmediateInit(bool immediateInit = true)
    {
        _config.ImmediateInit = immediateInit;
        return this;
    }

    public MenuBuilder<TControl> WithSort(int sort)
    {
        _config.Sort = sort;
        return this;
    }

    public IMenuConfig Build()
    {
        return _config;
    }

    public MenuBuilder<TControl> WithFirstShow(bool showOnLoaded = true)
    {
        _config.ShowOnLoaded = showOnLoaded;
        return this;
    }
}