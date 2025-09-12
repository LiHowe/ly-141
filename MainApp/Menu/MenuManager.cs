using Core.Localization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MainApp.Menu;

public class MenuManager : IMenuManager
{
    private readonly List<(Button button, string text, IMenuConfig config)> _menuItems;
    private Panel _menuPanel;
    private Button _prevButton;
    private Window _targetWindow;
    private ViewsManager _viewsManager;

    private Brush _buttonActiveBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#001741"));
    private Brush _buttonDefaultBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#243B76"));

    public MenuManager(Window window, Panel menuPanel, ViewsManager viewManager)
    {
        _menuItems = new List<(Button button, string text, IMenuConfig config)>();
        _targetWindow = window;
        _menuPanel = menuPanel;
        _viewsManager = viewManager;
    }

    public MenuManager()
    {
        _menuItems = new List<(Button button, string text, IMenuConfig config)>();
    }

    public Button AddMenu(IMenuConfig config)
    {
        var imgSource = config.Image == null
            ? new BitmapImage(new Uri(config.ImagePath, UriKind.RelativeOrAbsolute))
            : config.Image;

        var textBlock = new TextBlock
        {
            Text = config.Text ?? new LangExtension(config.TextKey).ProvideValue(null)?.ToString(),
            VerticalAlignment = VerticalAlignment.Center
        };
        try
        {
            if (config.TextKey != null && !string.IsNullOrWhiteSpace(config.TextKey))
            {
                textBlock.SetBinding(TextBlock.TextProperty,
                    new Binding($"[{config.TextKey}]")
                    {
                        Source = LocalizationProvider.Default,
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });
            }
        }
        catch (Exception ex)
        {
            // ignore
        }

		var button = new Button
        {
            Name = config.Text,
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Image
                    {
                        Source = imgSource, // 你需要确保 config.Image 为 ImageSource
                        Width = 16,
                        Height = 16,
                        Margin = new Thickness(0, 0, 5, 0)
                    },
					textBlock
				}
            },
            MinWidth = 120,
            Height = 30,
            Padding = new Thickness(10, 0, 10, 0),
            Margin = new Thickness(0, 0, 10, 0),
            Background = _buttonDefaultBrush,
            BorderThickness = new (0),
            Foreground = Brushes.White,
            Cursor = Cursors.Hand,
            FontSize = 12
        };
        // button.Style = (Style)_menuPanel.FindResource("ModuleMenuButtonStyle");

        _menuItems.Add((button, config.Text, config));

        if (config.ImmediateInit) PreActiveMenu(button, config);

        return button;
    }

    public void RemoveMenu(string text)
    {
        var menuItem = _menuItems.FirstOrDefault(item => item.text == text);
        if (menuItem != default)
        {
            _menuItems.Remove(menuItem);
            _menuPanel.Children.Remove(menuItem.button);
        }
    }

    public void Apply(Window mainWindow, Panel menuPanel, ViewsManager viewManager)
    {
        _targetWindow = mainWindow;
        _menuPanel = menuPanel;
        _viewsManager = viewManager;

        menuPanel.Children.Clear();

        var sortedMenuItems = _menuItems.OrderBy(item => item.config.Sort).ToList();

        foreach (var (button, text, config) in sortedMenuItems)
        {
            menuPanel.Children.Add(button);
            button.Click -= ButtonClickHandler;
            button.Click += ButtonClickHandler;

            void ButtonClickHandler(object sender, RoutedEventArgs args)
            {
                ActivateMenu(button, config);
            }
        }

        mainWindow.Loaded += (s, e) =>
        {
            var firstShowItem = _menuItems.FirstOrDefault(x => x.config.ShowOnLoaded);
            if (firstShowItem != default)
                ShowMenuView(firstShowItem.text);
            else
                // 默认显示第一个
                ShowMenuView(sortedMenuItems.First().text);
        };
    }

    public List<Button> AddMenus(List<IMenuConfig> configs)
    {
        var buttons = new List<Button>();
        foreach (var config in configs)
        {
            var button = AddMenu(config);
            buttons.Add(button);
        }

        return buttons;
    }

    public void Refresh()
    {
        Apply(_targetWindow, _menuPanel, _viewsManager);
    }

    public void ShowMenuView(string text)
    {
        var menuItem = _menuItems.FirstOrDefault(item => item.text == text);
        if (menuItem != default) ActivateMenu(menuItem.button, menuItem.config);
    }

    private void PreActiveMenu(Button button, IMenuConfig config)
    {
        if (config.ViewType == null || _viewsManager == null) return;

        var methodInfo = typeof(ViewsManager)
            .GetMethods()
            .FirstOrDefault(m => m.Name == nameof(ViewsManager.InitView) && m.IsGenericMethod);

        if (methodInfo == null) return;

        var genericMethod = methodInfo.MakeGenericMethod(config.ViewType);
        genericMethod.Invoke(_viewsManager, new object[] { null, config.UseCache });
    }

    private void ActivateMenu(Button button, IMenuConfig config)
    {
        if (config.ViewType != null && _viewsManager != null)
        {
            var methodInfo = typeof(ViewsManager)
                .GetMethods()
                .FirstOrDefault(m => m.Name == nameof(ViewsManager.ChangeView) && m.IsGenericMethod);

            if (methodInfo != null)
            {
                var genericMethod = methodInfo.MakeGenericMethod(config.ViewType);

                Action<object> typedInitializer = null;
                if (config.Initializer != null)
                    typedInitializer = view =>
                    {
                        try
                        {
                            if (config.Initializer.Method.GetParameters()[0].ParameterType
                                .IsAssignableFrom(config.ViewType)) config.Initializer.DynamicInvoke(view);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Initialization error: {ex.Message}");
                        }
                    };

                genericMethod.Invoke(_viewsManager, new object[] { config.Text, config.UseCache, typedInitializer });
                UpdateButtonAppearance(button);
            }
            else
            {
                throw new InvalidOperationException($"Cannot find ChangeView method for type {config.ViewType.Name}");
            }
        }
    }

    private void UpdateButtonAppearance(Button btn)
    {
        if (_prevButton != null)
            _prevButton.Background =  _buttonDefaultBrush;

        _prevButton = btn;
        btn.Background = _buttonActiveBrush;
    }

    //private int ScaleSize(int size)
    //{
    //	return (int)(size * (PresentationSource.FromVisual(_targetWindow)?.CompositionTarget?.TransformToDevice.M11 ?? 1.0));
    //}
}