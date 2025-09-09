using System.Configuration;
using System.Windows.Controls;
using Core;

using Core.Models;
using Core.Models.Settings;
using Core.Repositories;
using Core.Services;
using Core.Utils;
using Data.SqlSugar;
using Logger;
using UI.Controls;
using UI.ViewModels;

namespace Module.Business.Biz;

/// <summary>
/// 业务逻辑基类,
/// 1. 保存零件
/// 2. 保存参数
/// </summary>
public abstract class BizBase<T> where T: RecordBase, new()
{
    /// <summary>
    /// 业务名称
    /// </summary>
    public abstract string Name { get; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public virtual bool IsEnabled { get; set; } = true;
    
    /// <summary>
    ///  业务目标PLC
    /// </summary>
    public abstract string TargetPlcKey { get; }
    
    /// <summary>
    ///  产品仓库
    /// </summary>
    protected readonly ProductRepository ProductRepository;

    protected readonly RecordCommonRepository<T> RecordRepository;
    
    protected MonitorBlockViewModel MonitorBlockViewModel;
    
    protected BizControlBase? Control;

    /// <summary>
    /// 当前状态 - 是否在运行中
    /// </summary>
    protected bool IsRunning = false;
    
    protected BizBase()
    {
        // 获取本地数据库配置
        var localDbConfig = ConfigManager.Instance.LoadConfig<DatabaseSettings>(Constants.LocalDbConfigFilePath);
        if (localDbConfig == null)
        {
            Log.Error("获取本地数据库配置失败");
            throw new ConfigurationErrorsException("获取本地数据库配置失败");
        }
        var sugarConfig = localDbConfig.ToSugarConfig();
        Sugar sugar = new(sugarConfig);
        ProductRepository = new(sugar);
        RecordRepository = new(sugar);
        InitComponents();
        UseStrategy();
    }
    
    /// <summary>
    /// 子类必须实现初始化方法 <br/>
    /// ⚠️注意: 方法会在子类的构造函数前被调用。
    /// </summary>
    protected abstract void InitComponents();

    /// <summary>
    /// 应用策略
    /// </summary>
    /// <typeparam name="T"></typeparam>
    protected abstract void UseStrategy();

    public Control? GetControl => Control;
}

/// <summary>
/// 业务逻辑配置基类
/// </summary>
public class BizConfigBase
{
    
}