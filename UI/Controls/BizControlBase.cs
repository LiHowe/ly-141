using System.Windows.Controls;

namespace UI.Controls;

public abstract class BizControlBase: UserControl
{
    public event EventHandler? OnStarted;
    public event EventHandler? OnStopped;
    public event EventHandler? OnReset;
    public event EventHandler? OnError;
    
    /// <summary>
    /// 开始控件
    /// </summary>
    public abstract void Start();
    
    /// <summary>
    /// 停止控件
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// 重置控件
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// 控件错误
    /// </summary>
    public abstract void Error();
}