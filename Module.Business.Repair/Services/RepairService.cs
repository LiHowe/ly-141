using Core;
using Core.Interfaces;
using Core.Utils;
using Logger;
using Module.Business.Repair.Jobs;
using Quartz;
using Quartz.Impl;
using SqlSugar;

namespace Module.Business.Repair.Services;

/// <summary>
/// 返修的业务逻辑，按照功能进行独立方法开发，供上层调用
/// </summary>
public class RepairService
{

    private readonly SqlSugarClient _db;
    private RepairModuleSettings? _repairModuleSettings;
    private IScheduler? _scheduler;
    private bool _isStarted = false;
    
    /// <summary>
    /// 返修业务逻辑
    /// </summary>
    /// <param name="client"></param>
    public RepairService(SqlSugarClient client)
    {
        _db = client;
        _repairModuleSettings = ConfigManager.Instance.LoadConfig<RepairModuleSettings>(Constants.RepairModuleConfigFilePath);
        if (_repairModuleSettings == null)
        {
            _repairModuleSettings = new RepairModuleSettings();
            ConfigManager.Instance.SaveConfig(Constants.RepairModuleConfigFilePath, _repairModuleSettings);
        }
        ConfigManager.Instance.EnableFileWatcher(Constants.RepairModuleConfigFilePath);
        // 如果配置发生变更
        ConfigManager.Instance.ConfigChanged += (sender, args) =>
        {
            if (args.ConfigType == ConfigType.RepairModule)
            {
                _repairModuleSettings = ConfigManager.Instance.LoadConfig<RepairModuleSettings>(Constants.RepairModuleConfigFilePath);
                if (_repairModuleSettings.EnableTimer)
                {
                    StartTimerTask();
                }
                else
                {
                    StopTimerTask();
                }
            }
        };
        
        if (_repairModuleSettings.EnableTimer)
        {
            StartTimerTask();
        }
    }


    /// <summary>
    /// 启动每日检查, 使用Quartz
    /// </summary>
    public async Task StartTimerTask()
    {
        // 避免重复启动
        if (_isStarted && _scheduler != null) return;
        _isStarted = true;
        // 1. 创建调度器工厂
        StdSchedulerFactory factory = new StdSchedulerFactory();

        // 2. 从工厂获取调度器
        _scheduler = await factory.GetScheduler();

        // 3. 启动调度器
        await _scheduler.Start();
        
        // 4. 定义任务
        IJobDetail job = JobBuilder.Create<CheckUnloadJob>()
            .WithIdentity(nameof(CheckUnloadJob), nameof(RepairBusinessModule))
            .Build();

        // 5. 创建Cron触发器，设置每天16:30执行
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(nameof(CheckUnloadJob), nameof(RepairBusinessModule))
            .WithCronSchedule(_repairModuleSettings.CronExpression)
            .Build();

        // 6. 立即执行一次任务
        // await scheduler.TriggerJob(job.Key);

        // 7. 告诉调度器使用触发器调度任务
        await _scheduler.ScheduleJob(job, trigger);
        Log.Info("返修模块定时任务启动成功");
    }
    
    public async Task StopTimerTask()
    {
        if (_scheduler == null) return;
        await _scheduler.Shutdown(true);
        _scheduler = null;
        _isStarted = false;
        Log.Info("返修模块定时任务已停止");
    }
   
}