using Core.Interfaces;

namespace MainApp.Views.Settings;

/// <summary>
///     设置控件工厂
///     负责创建和管理所有设置控件实例
/// </summary>
public class SettingsControlFactory
{
    private readonly Dictionary<string, Func<ISettingsControl>> _controlFactories;
    private readonly Dictionary<string, ISettingsControl> _controlInstances;

    public SettingsControlFactory()
    {
        _controlFactories = new Dictionary<string, Func<ISettingsControl>>();
        _controlInstances = new Dictionary<string, ISettingsControl>();

        RegisterDefaultControls();
    }

    /// <summary>
    ///     注册默认的设置控件
    /// </summary>
    private void RegisterDefaultControls()
    {
        RegisterControl("System", () => new SystemSettingsControl());
        RegisterControl("Database", () => new DatabaseSettingsControl());
        RegisterControl("Plc", () => new PlcSettingsControl());
        RegisterControl("HistoryDetail", () => new HistoryDetailSettingsControl());
    }

    /// <summary>
    ///     注册设置控件
    /// </summary>
    /// <param name="categoryName">分类名称</param>
    /// <param name="factory">控件工厂方法</param>
    public void RegisterControl(string categoryName, Func<ISettingsControl> factory)
    {
        _controlFactories[categoryName] = factory;
    }

    /// <summary>
    ///     获取设置控件实例
    /// </summary>
    /// <param name="categoryName">分类名称</param>
    /// <returns>设置控件实例</returns>
    public ISettingsControl? GetControl(string categoryName)
    {
        // 如果已经创建过实例，直接返回
        if (_controlInstances.TryGetValue(categoryName, out var existingControl)) return existingControl;

        // 创建新实例
        if (_controlFactories.TryGetValue(categoryName, out var factory))
        {
            var control = factory();
            _controlInstances[categoryName] = control;
            return control;
        }

        return null;
    }

    /// <summary>
    ///     获取所有已注册的设置分类
    /// </summary>
    /// <returns>设置分类列表</returns>
    public IEnumerable<string> GetRegisteredCategories()
    {
        return _controlFactories.Keys.ToList();
    }

    /// <summary>
    ///     获取设置分类的显示名称
    /// </summary>
    /// <param name="categoryName">分类名称</param>
    /// <returns>显示名称</returns>
    public string GetCategoryDisplayName(string categoryName)
    {
        var control = GetControl(categoryName);
        return control?.DisplayTitle ?? categoryName;
    }

    /// <summary>
    ///     检查是否有未保存的更改
    /// </summary>
    /// <returns>是否有未保存的更改</returns>
    public bool HasUnsavedChanges()
    {
        return _controlInstances.Values.Any(control => control.HasUnsavedChanges);
    }

    /// <summary>
    ///     获取有未保存更改的控件列表
    /// </summary>
    /// <returns>控件列表</returns>
    public IEnumerable<ISettingsControl> GetControlsWithUnsavedChanges()
    {
        return _controlInstances.Values.Where(control => control.HasUnsavedChanges);
    }

    /// <summary>
    ///     验证所有设置控件
    /// </summary>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateAllControls()
    {
        var result = new ValidationResult();

        foreach (var control in _controlInstances.Values)
        {
            var controlResult = control.ValidateSettings();
            if (!controlResult.IsValid)
                foreach (var error in controlResult.Errors)
                    result.AddError($"[{control.DisplayTitle}] {error}");

            foreach (var warning in controlResult.Warnings) result.AddWarning($"[{control.DisplayTitle}] {warning}");
        }

        return result;
    }

    /// <summary>
    ///     释放所有控件实例
    /// </summary>
    public void DisposeAllControls()
    {
        foreach (var control in _controlInstances.Values)
            if (control is IDisposable disposable)
                disposable.Dispose();

        _controlInstances.Clear();
    }

    /// <summary>
    ///     移除控件实例（强制重新创建）
    /// </summary>
    /// <param name="categoryName">分类名称</param>
    public void RemoveControlInstance(string categoryName)
    {
        if (_controlInstances.TryGetValue(categoryName, out var control))
        {
            if (control is IDisposable disposable) disposable.Dispose();
            _controlInstances.Remove(categoryName);
        }
    }

    /// <summary>
    ///     检查分类是否已注册
    /// </summary>
    /// <param name="categoryName">分类名称</param>
    /// <returns>是否已注册</returns>
    public bool IsRegistered(string categoryName)
    {
        return _controlFactories.ContainsKey(categoryName);
    }
}