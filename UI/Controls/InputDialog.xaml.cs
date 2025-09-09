using System.Windows;
using System.Windows.Input;

namespace UI.Controls;

/// <summary>
///     输入对话框
/// </summary>
public partial class InputDialog : Window
{
    public enum InputType
    {
        Text,
        Password,
        Number
    }

    private InputType _inputType;
    private Func<string, (bool isValid, string errorMessage)> _validator;

    public InputDialog()
    {
        InitializeComponent();
        MouseLeftButtonDown += (s, e) => DragMove();

        // 默认焦点到输入框
        Loaded += (s, e) => InputTextBox.Focus();
    }

    public string InputValue { get; private set; } = string.Empty;
    public new bool DialogResult { get; private set; }

    /// <summary>
    ///     显示文本输入对话框
    /// </summary>
    public static (bool success, string value) ShowTextInput(string title, string prompt,
        string defaultValue = "", Func<string, (bool, string)> validator = null)
    {
        return ShowInput(title, prompt, InputType.Text, defaultValue, validator);
    }

    /// <summary>
    ///     显示密码输入对话框
    /// </summary>
    public static (bool success, string value) ShowPasswordInput(string title, string prompt)
    {
        return ShowInput(title, prompt, InputType.Password);
    }

    /// <summary>
    ///     显示数字输入对话框
    /// </summary>
    public static (bool success, string value) ShowNumberInput(string title, string prompt,
        string defaultValue = "", double? min = null, double? max = null)
    {
        Func<string, (bool, string)> numberValidator = input =>
        {
            if (string.IsNullOrWhiteSpace(input))
                return (false, "请输入数字");

            if (!double.TryParse(input, out var value))
                return (false, "请输入有效的数字");

            if (min.HasValue && value < min.Value)
                return (false, $"数值不能小于 {min.Value}");

            if (max.HasValue && value > max.Value)
                return (false, $"数值不能大于 {max.Value}");

            return (true, "");
        };

        return ShowInput(title, prompt, InputType.Number, defaultValue, numberValidator);
    }

    /// <summary>
    ///     显示输入对话框（指定父窗口）
    /// </summary>
    public static (bool success, string value) ShowInput(Window owner, string title, string prompt,
        InputType type = InputType.Text, string defaultValue = "",
        Func<string, (bool, string)> validator = null)
    {
        var dialog = new InputDialog();
        dialog.Owner = owner;
        dialog.SetupDialog(title, prompt, type, defaultValue, validator);
        dialog.ShowDialog();
        return (dialog.DialogResult, dialog.InputValue);
    }

    private static (bool success, string value) ShowInput(string title, string prompt,
        InputType type = InputType.Text, string defaultValue = "",
        Func<string, (bool, string)> validator = null)
    {
        var dialog = new InputDialog();
        dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        dialog.SetupDialog(title, prompt, type, defaultValue, validator);
        dialog.ShowDialog();
        return (dialog.DialogResult, dialog.InputValue);
    }

    private void SetupDialog(string title, string prompt, InputType type,
        string defaultValue, Func<string, (bool, string)> validator)
    {
        TitleTextBlock.Text = title;
        PromptTextBlock.Text = prompt;
        _inputType = type;
        _validator = validator;

        switch (type)
        {
            case InputType.Text:
                InputTextBox.Text = defaultValue;
                break;
            case InputType.Password:
                InputTextBox.Visibility = Visibility.Collapsed;
                PasswordPanel.Visibility = Visibility.Visible;
                break;
            case InputType.Number:
                InputTextBox.Text = defaultValue;
                break;
        }
    }

    private bool ValidateInput()
    {
        var input = _inputType == InputType.Password ? PasswordBox.Password : InputTextBox.Text;

        if (_validator != null)
        {
            var (isValid, errorMessage) = _validator(input);
            if (!isValid)
            {
                ValidationTextBlock.Text = errorMessage;
                ValidationTextBlock.Visibility = Visibility.Visible;
                return false;
            }
        }

        ValidationTextBlock.Visibility = Visibility.Collapsed;
        return true;
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        if (ValidateInput())
        {
            InputValue = _inputType == InputType.Password ? PasswordBox.Password : InputTextBox.Text;
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            OKButton_Click(sender, e);
        else if (e.Key == Key.Escape) CancelButton_Click(sender, e);
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            OKButton_Click(sender, e);
        else if (e.Key == Key.Escape) CancelButton_Click(sender, e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }

        base.OnKeyDown(e);
    }
}