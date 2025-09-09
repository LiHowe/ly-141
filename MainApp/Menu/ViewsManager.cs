using System.Windows;
using System.Windows.Controls;
using Core.Services;
using Logger;


namespace MainApp.Menu;

public class ViewsManager : IViewsManager
{
    private readonly ContentControl _container;
    private readonly Dictionary<string, UserControl> _viewsDictionary = new();
    private UserControl? _currentView;
    private bool _disposed;

    public ViewsManager(ContentControl container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public T ChangeView<T>(string? instanceId = null, bool useCache = true, Action<T>? viewInitializer = null)
        where T : UserControl, new()
    {
        var viewType = typeof(T);
        var key = instanceId != null ? $"{viewType.FullName}_{instanceId}" : viewType.FullName ?? viewType.Name;

        if (useCache && _viewsDictionary.TryGetValue(key, out var cachedView))
        {
            if (cachedView == null)
            {
                cachedView = new T();
                viewInitializer?.Invoke((T)cachedView);
                _viewsDictionary[key] = cachedView;
            }

            ShowView(cachedView);
            return (T)cachedView;
        }

        var view = new T();
        viewInitializer?.Invoke(view);

        if (useCache)
            _viewsDictionary[key] = view;

        ShowView(view);
        return view;
    }

    public void InitView<T>(string? instanceId = null, bool useCache = true) where T : UserControl, new()
    {
        InitView(new T(), instanceId, useCache);
    }

    public void InitView(UserControl view, string? instanceId = null, bool useCache = true)
    {
        if (view == null)
        {
            Log.Error("ViewsManager 调用方法失败: InitView", new ArgumentNullException(nameof(view)));
            throw new ArgumentNullException(nameof(view));
        }

        if (useCache)
        {
            var key = instanceId != null
                ? $"{view.GetType().FullName}_{instanceId}"
                : view.GetType().FullName ?? view.GetType().Name;
            _viewsDictionary[key] = view;
            Log.Info($"ViewManager 初始化视图 {view.GetType().Name} 成功.");
        }
    }

    public T? GetView<T>(string? instanceId = null) where T : UserControl
    {
        var key = instanceId != null ? $"{typeof(T).FullName}_{instanceId}" : typeof(T).FullName ?? typeof(T).Name;

        if (_viewsDictionary.TryGetValue(key, out var view) && view is T typedView) return typedView;

        return null;
    }

    public Window? DetachView<T>(string? instanceId = null, string? title = null, Size? size = null,
        bool removeFromCache = false) where T : UserControl
    {
        var view = GetView<T>(instanceId);
        if (view == null)
            return null;

        if (_currentView == view)
            _container.Content = null;

        var window = new Window
        {
            Title = title ?? view.GetType().Name,
            Content = view,
            SizeToContent = SizeToContent.Manual,
            Width = size?.Width ?? 800,
            Height = size?.Height ?? 600,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        if (removeFromCache)
        {
            var key = instanceId != null ? $"{typeof(T).FullName}_{instanceId}" : typeof(T).FullName ?? typeof(T).Name;
            _viewsDictionary.Remove(key);
        }

        window.Show();
        return window;
    }

    public bool RemoveView<T>(string? instanceId = null) where T : UserControl
    {
        var key = instanceId != null ? $"{typeof(T).FullName}_{instanceId}" : typeof(T).FullName ?? typeof(T).Name;

        if (_viewsDictionary.TryGetValue(key, out var view))
        {
            if (view == _currentView)
            {
                _container.Content = null;
                _currentView = null;
            }

            _viewsDictionary.Remove(key);
            return true;
        }

        return false;
    }

    public int CloseAllViewsOfType<T>() where T : UserControl
    {
        var typeName = typeof(T).FullName!;
        var keysToRemove = _viewsDictionary
            .Where(kv => kv.Key.StartsWith($"{typeName}_") || kv.Key == typeName)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in keysToRemove)
            if (_viewsDictionary.TryGetValue(key, out var view))
            {
                if (view == _currentView)
                {
                    _container.Content = null;
                    _currentView = null;
                }

                _viewsDictionary.Remove(key);
            }

        return keysToRemove.Count;
    }

    public void ClearCache()
    {
        var keys = _viewsDictionary.Keys.ToList();
        foreach (var key in keys)
            if (_viewsDictionary[key] != _currentView)
                _viewsDictionary.Remove(key);

        if (_currentView != null)
        {
            var key = GetViewKey(_currentView);
            _viewsDictionary[key] = _currentView;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void ShowView(UserControl view)
    {
        if (_currentView != view)
            Application.Current.Dispatcher.Invoke(() =>
            {
                _container.Content = view;
                _currentView = view;
                Log.Debug($"显示视图 {view.GetType().Name}");
            });
    }

    private string GetViewKey(UserControl view)
    {
        var entry = _viewsDictionary.FirstOrDefault(kv => kv.Value == view);
        return entry.Key ?? view.GetType().FullName ?? view.GetType().Name;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _viewsDictionary.Clear();
                _currentView = null;
            }

            _disposed = true;
        }
    }
}