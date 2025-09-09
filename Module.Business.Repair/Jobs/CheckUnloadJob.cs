using Core;
using Core.Events;
using Core.Models.Settings;
using Core.Utils;
using Data.SqlSugar;
using Logger;
using Module.Business.Repair.Repositories;
using Quartz;

namespace Module.Business.Repair.Jobs;

/// <summary>
/// 用来统计每日下料未上料的零件
/// </summary>
public class CheckUnloadJob: IJob
{
    private DatabaseSettings? _databaseSettings;
    private RepairRepository? _repository;
    private RepairModuleSettings? _repairModuleSettings;
    
    public async Task Execute(IJobExecutionContext context)
    {
        // 获取数据库配置
        _databaseSettings ??= ConfigManager.Instance.LoadConfig<DatabaseSettings>(Constants.SystemConfigFilePath);
        Log.Debug("检查下料未上料零件任务执行");
        if (_databaseSettings == null)
        {
            Log.Error("数据库配置为空，无法执行检查下料未上料零件任务");
            return;
        }
        // 获取模块配置
        _repairModuleSettings = ConfigManager.Instance.LoadConfig<RepairModuleSettings>(Constants.RepairModuleConfigFilePath);
        
        if (_repairModuleSettings == null)
        {
            Log.Error("返修模块配置为空，无法执行检查下料未上料零件任务");
            return;
        }
        
        // 为了避免创建多个PLC实例，使用事件中心进行事件发布
        EventCenter.Instance.Publish($"{_repairModuleSettings.TargetPlc}.WriteNode", new
        {
            Node = _repairModuleSettings.AlarmNode,
            Value = true
        });
        
        
        // 0. 获取数据库连接
        Sugar sugar = new(_databaseSettings.ToSugarConfig());
        // 创建服务
        _repository = new(sugar.GetDb());
        // 1. 获取今日所有下料未上料的零件
        var res = await _repository.GetTodayUnloadRecords();

        if (res.Count > 0)
        {
            
        } 
        // 2. 发送报警

        // 3. 清空资源
        _repository = null;
    }
}