using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Interfaces;
using ValidationResult = Core.Interfaces.ValidationResult;

namespace UI.Controls;

/// <summary>
///     设置控件基类
///     提供通用的设置控件功能和UI组件创建方法
/// </summary>
public abstract class BaseSettingsControl : UserControl, ISettingsControl
{
    protected IConfigManager? _configManager;
    protected bool _isLoading;

    /// <summary>
    ///     构造函数
    /// </summary>
    protected BaseSettingsControl()
    {
        InitializeBaseComponent();
    }

    /// <summary>
    ///     设置分类名称
    /// </summary>
    public abstract string CategoryName { get; }

    /// <summary>
    ///     设置显示标题
    /// </summary>
    public abstract string DisplayTitle { get; }

    /// <summary>
    ///     是否有未保存的更改
    /// </summary>
    public virtual bool HasUnsavedChanges { get; protected set; }

    /// <summary>
    ///     配置更改事件
    /// </summary>
    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    /// <summary>
    ///     初始化设置控件
    /// </summary>
    public virtual async Task InitializeAsync(IConfigManager configManager)
    {
        _configManager = configManager;
        await OnInitializeAsync();
    }

    /// <summary>
    ///     加载设置数据
    /// </summary>
    public virtual async Task LoadSettingsAsync()
    {
        if (_configManager == null) return;

        _isLoading = true;
        try
        {
            await OnLoadSettingsAsync();
            HasUnsavedChanges = false;
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    ///     保存设置数据
    /// </summary>
    public virtual async Task SaveSettingsAsync()
    {
        if (_configManager == null) return;

        await OnSaveSettingsAsync();
        HasUnsavedChanges = false;
    }

    /// <summary>
    ///     验证设置数据
    /// </summary>
    public virtual ValidationResult ValidateSettings()
    {
        return OnValidateSettings();
    }

    /// <summary>
    ///     重置设置到默认值
    /// </summary>
    public virtual async Task ResetToDefaultAsync()
    {
        await OnResetToDefaultAsync();
        HasUnsavedChanges = true;
        OnSettingsChanged("Reset", null, null);
    }

    /// <summary>
    ///     刷新设置显示
    /// </summary>
    public virtual async Task RefreshAsync()
    {
        await LoadSettingsAsync();
    }

    /// <summary>
    ///     初始化基础组件（子类可重写）
    /// </summary>
    protected virtual void InitializeBaseComponent()
    {
        Background = Brushes.White;
        Padding = new Thickness(0);
    }

    #region 事件处理

    /// <summary>
    ///     触发设置更改事件
    /// </summary>
    protected virtual void OnSettingsChanged(string propertyName, object? oldValue, object? newValue)
    {
        if (!_isLoading)
        {
            HasUnsavedChanges = true;
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs
            {
                CategoryName = CategoryName,
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue
            });
        }
    }

    #endregion

    #region 抽象方法 - 子类必须实现

    /// <summary>
    ///     初始化设置控件的具体实现
    /// </summary>
    protected virtual Task OnInitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     加载设置数据的具体实现
    /// </summary>
    protected abstract Task OnLoadSettingsAsync();

    /// <summary>
    ///     保存设置数据的具体实现
    /// </summary>
    protected abstract Task OnSaveSettingsAsync();

    /// <summary>
    ///     验证设置数据的具体实现
    /// </summary>
    protected abstract ValidationResult OnValidateSettings();

    /// <summary>
    ///     重置到默认值的具体实现
    /// </summary>
    protected abstract Task OnResetToDefaultAsync();

    #endregion

    #region UI组件创建辅助方法

    /// <summary>
    ///     创建设置项
    /// </summary>
    protected StackPanel CreateSettingItem(string label, string value, Action<string> onChanged)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

        var labelBlock = new TextBlock
        {
            Text = label,
            Margin = new Thickness(0, 0, 0, 5),
            FontWeight = FontWeights.Medium
        };
        panel.Children.Add(labelBlock);

        var textBox = new TextBox
        {
            Text = value,
            Height = 30,
            Padding = new Thickness(8)
        };

        textBox.TextChanged += (s, e) =>
        {
            var oldValue = value;
            var newValue = textBox.Text;
            onChanged(newValue);
            OnSettingsChanged(label, oldValue, newValue);
        };

        panel.Children.Add(textBox);
        return panel;
    }

    /// <summary>
    ///     创建只读设置项
    /// </summary>
    protected StackPanel CreateReadOnlyItem(string label, string value)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

        var labelBlock = new TextBlock
        {
            Text = label,
            Margin = new Thickness(0, 0, 0, 5),
            FontWeight = FontWeights.Medium
        };
        panel.Children.Add(labelBlock);

        var textBox = new TextBox
        {
            Text = value,
            Height = 30,
            Padding = new Thickness(8),
            IsReadOnly = true,
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
        };

        panel.Children.Add(textBox);
        return panel;
    }

    /// <summary>
    ///     创建复选框设置项
    /// </summary>
    protected StackPanel CreateCheckBoxItem(string label, bool value, Action<bool> onChanged)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

        var checkBox = new CheckBox
        {
            Content = label,
            IsChecked = value,
            FontWeight = FontWeights.Medium
        };

        checkBox.Checked += (s, e) =>
        {
            onChanged(true);
            OnSettingsChanged(label, value, true);
        };

        checkBox.Unchecked += (s, e) =>
        {
            onChanged(false);
            OnSettingsChanged(label, value, false);
        };

        panel.Children.Add(checkBox);
        return panel;
    }

    /// <summary>
    ///     创建下拉框设置项
    /// </summary>
    protected StackPanel CreateComboBoxItem(string label, string[] options, string value, Action<string> onChanged)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

        var labelBlock = new TextBlock
        {
            Text = label,
            Margin = new Thickness(0, 0, 0, 5),
            FontWeight = FontWeights.Medium
        };
        panel.Children.Add(labelBlock);

        var comboBox = new ComboBox { Height = 30, Padding = new Thickness(8) };
        foreach (var option in options) comboBox.Items.Add(option);
        comboBox.SelectedItem = value;

        comboBox.SelectionChanged += (s, e) =>
        {
            if (comboBox.SelectedItem is string selectedValue)
            {
                var oldValue = value;
                onChanged(selectedValue);
                OnSettingsChanged(label, oldValue, selectedValue);
            }
        };

        panel.Children.Add(comboBox);
        return panel;
    }

    /// <summary>
    ///     创建数字输入项
    /// </summary>
    protected FrameworkElement CreateNumberItem(string label, int value, Action<int> onValueChanged)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

        var labelBlock = new TextBlock
        {
            Text = label,
            Width = 150,
            VerticalAlignment = VerticalAlignment.Center
        };

        var textBox = new TextBox
        {
            Text = value.ToString(),
            Width = 200,
            Margin = new Thickness(10, 0, 0, 0)
        };

        textBox.TextChanged += (s, e) =>
        {
            if (int.TryParse(textBox.Text, out var newValue))
            {
                var oldValue = value;
                onValueChanged(newValue);
                OnSettingsChanged(label, oldValue, newValue);
            }
        };

        panel.Children.Add(labelBlock);
        panel.Children.Add(textBox);

        return panel;
    }

    #endregion
}