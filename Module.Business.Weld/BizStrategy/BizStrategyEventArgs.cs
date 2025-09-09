namespace Module.Business.BizStrategy;

public class BizStrategyEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public object? Data { get; set; }

    public BizStrategyEventArgs(string name, string message, object data = null) : base()
    {
        Name = name;
        Message = message;
        Data = data;
    }
}

/// <summary>
///  业务数据接收事件参数
/// </summary>
public class BizStrategyDataReceivedEventArgs : EventArgs
{
    public Dictionary<string, object> Data { get; set; } = new();
    public BizStrategyDataReceivedEventArgs(Dictionary<string, object> data)
    {
        Data = data;
    }
}

public class SimpleStringEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;

    public SimpleStringEventArgs(string message) : base()
    {
        Message = message;
    }
}