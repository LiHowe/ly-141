using System.Windows.Media;

namespace MainApp.Menu;

public interface IMenuConfig
{
    string Text { get; }
    string TextKey { get; }
	ImageSource Image { get; }
    string ImagePath { get; }
    bool UseCache { get; }
    int Sort { get; }
    Type ViewType { get; }
    Delegate Initializer { get; }
    bool ImmediateInit { get; }
    List<Type> ViewModels { get; }

    bool ShowOnLoaded { get; }
}