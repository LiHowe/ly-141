using System.Windows;
using System.Windows.Controls;

namespace UI.Controls;

public partial class ShellWindow : Window
{
    public string IconText { get; set; }
    public string TitleText { get; set; }
    
    public event EventHandler<BeforeCloseEventArgs>? BeforeClose;
    
    /// <summary>
    /// 壳窗口
    /// </summary>
    /// <param name="title"></param>
    /// <param name="iconText">支持IconFont, 需要以/u开头</param>
    public ShellWindow(string title, string iconText = "⚙️")
    {
        InitializeComponent();
        // 启用窗口拖动
        this.MouseLeftButtonDown += (s, e) => this.DragMove();
        IconTextBlock.Text = iconText ?? "⚙️";
        TitleTextBlock.Text = title ?? "系统设置";
    }
    
    public void SetContent(UserControl content)
    {
        ContentControl.Content = content;
    }
    
    
    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        var args = new BeforeCloseEventArgs();
        BeforeClose?.Invoke(this, args);
        if (!args.Cancel)
        {
            Close();
        }
    }
}

public class BeforeCloseEventArgs : EventArgs
{
    /// <summary>
    /// 是否取消关闭，默认为 false（允许关闭）
    /// </summary>
    public bool Cancel { get; set; }
}