using Core.Localization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Window = System.Windows.Window;

namespace UI.Controls;

/// <summary>
///     自定义消息框，支持多种类型和按钮组合
/// </summary>
public partial class MessageBox : Window
{
    // 消息框按钮组合枚举
    public enum MessageBoxButtons
    {
        OK, // 仅确定
        OKCancel, // 确定/取消
        YesNo, // 是/否
        YesNoCancel, // 是/否/取消
        Custom // 自定义按钮
    }

    // 消息框类型枚举
    public enum MessageBoxType
    {
        Information, // 信息
        Warning, // 警告
        Error, // 错误
        Success, // 成功
        Question // 询问
    }

    public MessageBox()
    {
        InitializeComponent();
        // 启用窗口拖动
        MouseLeftButtonDown += (s, e) => DragMove();
    }

    // 存储消息框结果
    public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

    /// <summary>
    ///     显示消息框（无父窗口）
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    /// <param name="type">消息类型</param>
    /// <param name="buttons">按钮组合</param>
    /// <param name="details">详细信息（可选）</param>
    public static MessageBoxResult Show(string message, string title = "",
        MessageBoxType type = MessageBoxType.Information,
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        string details = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            title = GetDefaultTitle(type);
		var messageBox = new MessageBox();
        messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        messageBox.SetupMessageBox(message, title, type, buttons, details);
        messageBox.ShowDialog();
        return messageBox.Result;
    }

    private static string GetDefaultTitle(MessageBoxType type) => type switch
    {
        MessageBoxType.Information => LocalizationProvider.Default["Info"],
        MessageBoxType.Warning => LocalizationProvider.Default["Warning"],
        MessageBoxType.Error => LocalizationProvider.Default["Error"],
        MessageBoxType.Success => LocalizationProvider.Default["Success"],
        MessageBoxType.Question => LocalizationProvider.Default["Question"],
        _ => LocalizationProvider.Default["Tip"]
	};

	/// <summary>
	///     显示消息框（指定父窗口）
	/// </summary>
	/// <param name="owner">父窗口</param>
	/// <param name="message">消息内容</param>
	/// <param name="title">标题</param>
	/// <param name="type">消息类型</param>
	/// <param name="buttons">按钮组合</param>
	/// <param name="details">详细信息（可选）</param>
	public static MessageBoxResult Show(Window owner, string message, string title = "",
        MessageBoxType type = MessageBoxType.Information,
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        string details = null)
    {
		if (string.IsNullOrWhiteSpace(title))
			title = GetDefaultTitle(type);
		var messageBox = new MessageBox { Owner = owner };
        messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        messageBox.SetupMessageBox(message, title, type, buttons, details);
        messageBox.ShowDialog();
        return messageBox.Result;
    }

    /// <summary>
    ///     显示自定义按钮的消息框
    /// </summary>
    /// <param name="owner">父窗口（可选）</param>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    /// <param name="type">消息类型</param>
    /// <param name="customButtons">自定义按钮（名称和结果的元组）</param>
    /// <param name="details">详细信息（可选）</param>
    public static MessageBoxResult ShowCustom(Window owner, string message, string title,
        MessageBoxType type, (string Text, MessageBoxResult Result, bool IsPrimary)[] customButtons,
        string details = null)
    {
        var messageBox = new MessageBox { Owner = owner };
        messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        messageBox.SetupMessageBox(message, title, type, MessageBoxButtons.Custom, details, customButtons);
        messageBox.ShowDialog();
        return messageBox.Result;
    }

    /// <summary>
    ///     配置消息框
    /// </summary>
    private void SetupMessageBox(string message, string title, MessageBoxType type,
        MessageBoxButtons buttons, string details,
        (string Text, MessageBoxResult Result, bool IsPrimary)[] customButtons = null)
    {
        // 设置标题和消息
        TitleTextBlock.Text = title;
        MessageTextBlock.Text = message;

        // 设置详细信息
        if (!string.IsNullOrEmpty(details))
        {
            DetailsTextBlock.Content = details;
            DetailsExpander.Visibility = Visibility.Visible;
        }

        // 设置图标和外观
        SetupTypeAppearance(type);

        // 设置按钮
        if (buttons == MessageBoxButtons.Custom && customButtons != null)
            SetupCustomButtons(customButtons);
        else
            SetupButtons(buttons);
    }

    /// <summary>
    ///     设置消息框类型的外观（图标和标题栏颜色）
    /// </summary>
    private void SetupTypeAppearance(MessageBoxType type)
    {
        // 获取标题栏
        var titleBar = TitleBar;

        // 根据类型设置图标和标题栏背景
        switch (type)
        {
            case MessageBoxType.Information:
                IconTextBlock.Text = "ℹ️";
                titleBar.Background = new SolidColorBrush(Color.FromRgb(36, 59, 118)); // #243b76
                break;
            case MessageBoxType.Warning:
                IconTextBlock.Text = "⚠️";
                titleBar.Background = new SolidColorBrush(Color.FromRgb(255, 165, 0)); // Orange
                break;
            case MessageBoxType.Error:
                IconTextBlock.Text = "❌";
                titleBar.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Red
                break;
            case MessageBoxType.Success:
                IconTextBlock.Text = "✅";
                titleBar.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)); // Green
                break;
            case MessageBoxType.Question:
                IconTextBlock.Text = "❓";
                titleBar.Background = new SolidColorBrush(Color.FromRgb(36, 59, 118)); // #243b76
                break;
        }
    }

    /// <summary>
    ///     设置标准按钮组合
    /// </summary>
    private void SetupButtons(MessageBoxButtons buttons)
    {
        ButtonPanel.Children.Clear();

        switch (buttons)
        {
            case MessageBoxButtons.OK:
                AddButton(LocalizationProvider.Default["OK"], MessageBoxResult.OK, true);
                break;
            case MessageBoxButtons.OKCancel:
                AddButton(LocalizationProvider.Default["Cancel"], MessageBoxResult.Cancel, false);
                AddButton(LocalizationProvider.Default["Ok"], MessageBoxResult.OK, true);
                break;
            case MessageBoxButtons.YesNo:
                AddButton(LocalizationProvider.Default["No"], MessageBoxResult.No, false);
                AddButton(LocalizationProvider.Default["Yes"], MessageBoxResult.Yes, true);
                break;
            case MessageBoxButtons.YesNoCancel:
                AddButton(LocalizationProvider.Default["Cancel"], MessageBoxResult.Cancel, false);
                AddButton(LocalizationProvider.Default["No"], MessageBoxResult.No, false);
                AddButton(LocalizationProvider.Default["Yes"], MessageBoxResult.Yes, true);
                break;
        }
    }

    /// <summary>
    ///     设置自定义按钮
    /// </summary>
    private void SetupCustomButtons((string Text, MessageBoxResult Result, bool IsPrimary)[] customButtons)
    {
        ButtonPanel.Children.Clear();
        foreach (var buttonInfo in customButtons) AddButton(buttonInfo.Text, buttonInfo.Result, buttonInfo.IsPrimary);
    }

    /// <summary>
    ///     添加按钮到按钮面板
    /// </summary>
    private void AddButton(string text, MessageBoxResult result, bool isPrimary)
    {
        var button = new Button
        {
            Content = text,
            Style = isPrimary ? (Style)FindResource("ButtonPrimary") : (Style)FindResource("ButtonDefault"),
            Margin = new Thickness(10, 0, 0, 0),
            Tag = result
        };

        button.Click += (s, e) =>
        {
            Result = (MessageBoxResult)((Button)s).Tag;
            Close();
        };

        ButtonPanel.Children.Add(button);
    }

    /// <summary>
    ///     关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Cancel;
        Close();
    }

    /// <summary>
    ///     处理键盘输入（Esc 键关闭）
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        base.OnKeyDown(e);
    }

    #region 快捷调用方法

    /// <summary>
    ///     显示信息消息框
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："信息"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Info(string message, string details = null)
    {
        return Show(message, GetDefaultTitle(MessageBoxType.Information), MessageBoxType.Information, MessageBoxButtons.OK, details);
    }

    /// <summary>
    ///     显示信息消息框（指定父窗口）
    /// </summary>
    /// <param name="owner">父窗口</param>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："信息"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Info(Window owner, string message, string details = null)
    {
        return Show(owner, message, GetDefaultTitle(MessageBoxType.Information), MessageBoxType.Information, MessageBoxButtons.OK, details);
    }

    /// <summary>
    ///     显示警告消息框
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："警告"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Warn(string message, string details = null)
    {
        return Show(message, GetDefaultTitle(MessageBoxType.Warning), MessageBoxType.Warning, MessageBoxButtons.OK, details);
    }

    /// <summary>
    ///     显示警告消息框（指定父窗口）
    /// </summary>
    /// <param name="owner">父窗口</param>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："警告"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Warn(Window owner, string message, string details = null)
    {
        return Show(owner, message, GetDefaultTitle(MessageBoxType.Warning), MessageBoxType.Warning, MessageBoxButtons.OK, details);
    }

    /// <summary>
    ///     显示错误消息框
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："错误"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Error(string message,  string details = null)
    {
        return Show(message, GetDefaultTitle(MessageBoxType.Error), MessageBoxType.Error, MessageBoxButtons.OK, details);
    }

    /// <summary>
    ///     显示错误消息框（指定父窗口）
    /// </summary>
    /// <param name="owner">父窗口</param>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："错误"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Error(Window owner, string message,  string details = null)
    {
        return Show(owner, message, GetDefaultTitle(MessageBoxType.Error), MessageBoxType.Error, MessageBoxButtons.OK, details);
    }

    /// <summary>
    ///     显示成功消息框
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："成功"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Success(string message, string details = null)
    {
        return Show(message, GetDefaultTitle(MessageBoxType.Success), MessageBoxType.Success, MessageBoxButtons.OK, details);
    }

    /// <summary>
    ///     显示成功消息框（指定父窗口）
    /// </summary>
    /// <param name="owner">父窗口</param>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："成功"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Success(Window owner, string message, string details = null)
    {
        return Show(owner, message, GetDefaultTitle(MessageBoxType.Success), MessageBoxType.Success, MessageBoxButtons.OK, details);
    }

    /// <summary>
    ///     显示询问消息框（是/否按钮）
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："询问"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Question(string message, string details = null)
    {
        return Show(message, GetDefaultTitle(MessageBoxType.Question), MessageBoxType.Question, MessageBoxButtons.YesNo, details);
    }

    /// <summary>
    ///     显示询问消息框（指定父窗口，是/否按钮）
    /// </summary>
    /// <param name="owner">父窗口</param>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："询问"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Question(Window owner, string message, string details = null)
    {
        return Show(owner, message, GetDefaultTitle(MessageBoxType.Question), MessageBoxType.Question, MessageBoxButtons.YesNo, details);
    }

    /// <summary>
    ///     显示确认消息框（确定/取消按钮）
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："确认"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Confirm(string message, string details = null)
    {
        return Show(message, GetDefaultTitle(MessageBoxType.Question), MessageBoxType.Question, MessageBoxButtons.OKCancel, details);
    }

    /// <summary>
    ///     显示确认消息框（指定父窗口，确定/取消按钮）
    /// </summary>
    /// <param name="owner">父窗口</param>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题（默认："确认"）</param>
    /// <param name="details">详细信息（可选）</param>
    /// <returns>消息框结果</returns>
    public static MessageBoxResult Confirm(Window owner, string message, string details = null)
    {
        return Show(owner, message, GetDefaultTitle(MessageBoxType.Question), MessageBoxType.Question, MessageBoxButtons.OKCancel, details);
    }

    #endregion
}