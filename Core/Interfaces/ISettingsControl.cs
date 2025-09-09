namespace Core.Interfaces
{
    /// <summary>
    /// 设置控件接口
    /// 定义所有设置控件必须实现的基本功能
    /// </summary>
    public interface ISettingsControl
    {
        /// <summary>
        /// 设置分类名称
        /// </summary>
        string CategoryName { get; }

        /// <summary>
        /// 设置显示标题
        /// </summary>
        string DisplayTitle { get; }

        /// <summary>
        /// 是否有未保存的更改
        /// </summary>
        bool HasUnsavedChanges { get; }

        /// <summary>
        /// 配置更改事件
        /// </summary>
        event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        /// <summary>
        /// 初始化设置控件
        /// </summary>
        /// <param name="configManager">配置管理器</param>
        Task InitializeAsync(IConfigManager configManager);

        /// <summary>
        /// 加载设置数据
        /// </summary>
        Task LoadSettingsAsync();

        /// <summary>
        /// 保存设置数据
        /// </summary>
        Task SaveSettingsAsync();

        /// <summary>
        /// 验证设置数据
        /// </summary>
        /// <returns>验证结果</returns>
        ValidationResult ValidateSettings();

        /// <summary>
        /// 重置设置到默认值
        /// </summary>
        Task ResetToDefaultAsync();

        /// <summary>
        /// 刷新设置显示
        /// </summary>
        Task RefreshAsync();
    }

    /// <summary>
    /// 设置更改事件参数
    /// </summary>
    public class SettingsChangedEventArgs : EventArgs
    {
        public string CategoryName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }

    /// <summary>
    /// 验证结果
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public void AddError(string error)
        {
            IsValid = false;
            Errors.Add(error);
        }

        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }
    }
}
