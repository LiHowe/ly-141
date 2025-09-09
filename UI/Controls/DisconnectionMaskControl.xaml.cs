using System.Windows;
using System.Windows.Controls;

namespace UI.Controls;

public partial class DisconnectionMaskControl : UserControl
{
    public DisconnectionMaskControl()
    {
        InitializeComponent();
    }

    public event EventHandler? OnRetryClick;

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        OnRetryClick?.Invoke(this, EventArgs.Empty);
    }
}