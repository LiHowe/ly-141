using System.Text.RegularExpressions;
using Connection.S7;
using Core.Models.Records;
using Core.Repositories;
using Core.Services;
using Core.Utils;
using Data.SqlSugar;
using Logger;

namespace Module.Business.BizStrategy;

/// <summary>
/// 基础简单数据采集策略, 继承类表明采集方式后只需要覆写 HandleStart() 和 HandleEnd(), DataReceived() 方法即可
/// </summary>
public abstract class SimpleStrategyBase : IBizStrategy
{
    #region 事件

    /// <summary>
    /// 收到开始信号后
    /// </summary>
    public event EventHandler<BizStrategyEventArgs>? Started;

    /// <summary>
    /// 收到结束信号后
    /// </summary>
    public event EventHandler<BizStrategyEventArgs>? Ended;

    /// <summary>
    /// 采集到扫码信息后
    /// </summary>
    public event EventHandler<SimpleStringEventArgs>? CodeReceived;

    /// <summary>
    /// 采集到业务数据后
    /// </summary>
    public event EventHandler<BizStrategyDataReceivedEventArgs>? DataReceived;

    /// <summary>
    /// 发生错误时调用
    /// </summary>
    public event EventHandler<BizStrategyEventArgs>? ErrorOccurred;

    #endregion

    #region 属性

    /// <summary>
    /// 策略名称
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// 策略描述
    /// </summary>
    public virtual string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 用来记录产品
    /// </summary>
    public ProductRepository ProductRepository;

    #endregion


    protected virtual IBizStrategyConfig Config { get; set; }

    protected SimpleStrategyBase(string name)
    {
        Name = name;
        var dbConfig = ConfigManager.Instance.LoadDbConfig();
        var sugar = new Sugar(dbConfig.ToSugarConfig());
        ProductRepository = new(sugar);
    }

    /// <summary>
    /// 执行业务逻辑
    /// </summary>
    public async Task Apply(IBizStrategyConfig config)
    {
        Config = config;
        // 1. 验证配置
        if (!Config.ValidateConfig(out var error))
        {
            OnErrorOccurred("配置错误", error);
            return;
        }

        // 2. 验证节点是否存在
        if (Config.PlcConfig == null)
        {
            OnErrorOccurred("PLC配置错误", "PLC配置不存在");
            return;
        }

        // 如果PLC没有连接，先尝试连接
        if (!Config.Plc.IsConnected)
        {
            try
            {
                await Config.Plc.ConnectAsync();
            }
            catch (Exception ex)
            {
                OnErrorOccurred("PLC连接错误", ex.Message);
                return;
            }
        }

        // 3. 根据策略监听开始节点
        switch (Config.Mode)
        {
            case StrategyStartType.StartEnd:
                await HandleStartEndStrategy();
                break;
            case StrategyStartType.CodeAsStart:
                await HandleCodeAsStartStrategy();
                break;
            case StrategyStartType.StartOnly:
                await HandleStartOnlyStrategy();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #region 受保护的触发器（派生类可调用）

    protected virtual void OnStarted(string? code = null)
        => Started?.Invoke(this, new BizStrategyEventArgs("开始信号", "开始信号已收到", code));

    protected virtual void OnEnded()
        => Ended?.Invoke(this, new BizStrategyEventArgs("结束信号", "结束信号已收到"));

    protected virtual void OnCodeReceived(string code)
        => CodeReceived?.Invoke(this, new(code));

    protected virtual void OnDataReceived(string key, Dictionary<string, object> data)
        => DataReceived?.Invoke(this, new BizStrategyDataReceivedEventArgs(data));

    protected virtual void OnErrorOccurred(string key, string message)
    {
        Log.Error($"{key}: {message}");
        ErrorOccurred?.Invoke(this, new BizStrategyEventArgs(key, message));
    }

    #endregion


    #region 供派生类重写的生命周期钩子

    protected virtual Task HandleStart(string? code = null) => Task.CompletedTask;
    protected virtual Task HandleEnd() => Task.CompletedTask;

    #endregion

    #region 策略

    /// <summary>
    /// 使用开始节点作为开始结束
    /// </summary>
    private async Task HandleStartOnlyStrategy()
    {
        // 1. 获取节点
        S7PlcNode startNode = Config.PlcConfig.GetNode(Config.StartNodeKey);
        if (startNode is null)
        {
            OnErrorOccurred("节点错误", $"开始节点配置不正确: {Config.StartNodeKey}");
            return;
        }

        bool isStart = false;
        Config.Plc!.Watch(startNode, obj =>
        {
            var flag = bool.Parse(obj.ToString()!);
            if (flag == isStart) return;
            isStart = flag;

            if (isStart)
            {
                var code = ReadCodeIfNeeded();
                OnStarted(code);
                _ = HandleStart(code);
                // 保存零件记录
                SaveProductRecord(code);
            }
            else
            {
                OnEnded();
                _ = HandleEnd();
            }
        });
    }

    /// <summary>
    /// 使用Code节点作为开始节点
    /// </summary>
    private async Task HandleCodeAsStartStrategy()
    {
        var codeNode = Config.PlcConfig!.GetNode(Config.CodeNodeKey!);
        var endNode = Config.PlcConfig!.GetNode(Config.EndNodeKey!);

        if (codeNode is null || endNode is null)
        {
            OnErrorOccurred("节点错误", "Code节点或End节点不存在");
            return;
        }

        Config.Plc!.Watch(codeNode, obj =>
        {
            var code = CleanCode(obj.ToString());
            if (string.IsNullOrWhiteSpace(code)) return;

            OnCodeReceived(code);
            OnStarted(code);
            _ = HandleStart(code);
            // 保存零件记录
            SaveProductRecord(code);
        });

        Config.Plc!.Watch(endNode, obj =>
        {
            if (bool.Parse(obj.ToString()!))
            {
                OnEnded();
                _ = HandleEnd();
            }
        });
    }

    /// <summary>
    /// 具有开始与结束节点
    /// </summary>
    /// <returns></returns>
    private async Task HandleStartEndStrategy()
    {
        var startNode = Config.PlcConfig!.GetNode(Config.StartNodeKey!);
        var endNode = Config.PlcConfig!.GetNode(Config.EndNodeKey!);

        if (startNode is null || endNode is null)
        {
            OnErrorOccurred("节点错误", "Start节点或End节点不存在");
            return;
        }

        Config.Plc!.Watch(startNode, obj =>
        {
            if (!bool.Parse(obj.ToString()!)) return;

            var code = ReadCodeIfNeeded();
            OnStarted(code);
            OnCodeReceived(code);
            _ = HandleStart(code);
            // 保存零件记录
            SaveProductRecord(code);
        });

        Config.Plc!.Watch(endNode, obj =>
        {
            if (!bool.Parse(obj.ToString()!)) return;

            OnEnded();
            _ = HandleEnd();
        });
    }

    private async Task SaveProductRecord(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        var record = new ProductRecord
        {
            SerialNo = code
        };
        ProductRepository.SaveWhenNotExist(record);
    }
    
    #endregion


    #region 工具方法

    private string? ReadCodeIfNeeded()
    {
        if (string.IsNullOrWhiteSpace(Config.CodeNodeKey)) return null;

        var node = Config.PlcConfig!.GetNode(Config.CodeNodeKey);
        if (node is null)
        {
            OnErrorOccurred("节点错误", $"零件码节点配置不正确: {Config.CodeNodeKey}");
            return null;
        }

        return CleanCode(Config.Plc!.ReadNode<string>(node));
    }

    private static readonly Regex VinCleanRegex =
        new(@"[\u0000-\u001F]+", RegexOptions.Compiled);

    private static string CleanCode(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? $"NOCODE-{DateTime.Now:yyyyMMddHHmmss}"
            : VinCleanRegex.Replace(raw, string.Empty);

    #endregion
}