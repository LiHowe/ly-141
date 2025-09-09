using Connection.S7;
using UI.Controls;
using UI.ViewModels;

namespace Module.Business.BizStrategy;

/// <summary>
/// 点焊业务策略, 不涉及UI相关
/// </summary>
public class SpotBizStrategy: SimpleStrategyBase
{
    public override string Name => "基础点焊策略";
    public override string Description => "简单策略 - 扫码 -> 加工 -> 结束";

    public bool IsRunning = false;
    
    public event Action<int>? SpotIndexChanged;
    
    public SpotBizStrategyConfig Config => (SpotBizStrategyConfig)base.Config;

    public SpotBizStrategy(string name) : base(name)
    {
        
    }
    protected override async Task HandleStart(string? code = null)
    {
        IsRunning = true;
        // 开始监听焊点变化
        var plc = Config.Plc;
        var indexNode = Config.PlcConfig.GetNode(Config.SpotIndexNode);
        if (indexNode == null)
        {
            OnErrorOccurred("焊点", "焊点节点不能为空");
        }
        plc.Watch(indexNode, IndexWatcher);
    }

    protected override Task HandleEnd()
    {
        IsRunning = false;
        var plc = Config.Plc;
        var indexNode = Config.PlcConfig.GetNode(Config.SpotIndexNode);
        plc.UnWatch(indexNode);
        return base.HandleEnd();
    }

    private void IndexWatcher(object o)
    {
        if (!IsRunning) return;
        int index = int.Parse(o.ToString());
        if (index <= 0) return;
        SpotIndexChanged?.Invoke(index);
        // 读取节点 - 同步
        var dataPairs = ReadNodeData();
        // 1. 触发事件
        OnDataReceived("data", dataPairs);
    }

    private Dictionary<string, object> ReadNodeData()
    {
        var res = new Dictionary<string, object>();
        if (Config.DataNodeKeys.Count == 0) return res;
        List<S7PlcNode> nodes = new List<S7PlcNode>();
        foreach (var nodeKey in Config.DataNodeKeys)
        {
            // 忽略未定义数据节点
            var node = Config.PlcConfig.GetNode(nodeKey);
            if (node is null) continue;
            nodes.Add(node);
        }

        if (nodes is { Count: 0 }) return res;
        return Config.Plc!.ReadNodes(nodes);
    }
    
}