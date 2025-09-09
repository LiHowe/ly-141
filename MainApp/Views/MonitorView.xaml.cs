using System.Windows;
using System.Windows.Controls;
using UI.Controls;

namespace MainApp.Views;

/// <summary>
///     MonitorView.xaml 的交互逻辑
/// </summary>
public partial class MonitorView : UserControl
{
    public MonitorView()
    {
        InitializeComponent();
    }

    public DynamicTablePanel Container => TablePanel;

    /// <summary>
    ///     添加元素
    /// </summary>
    /// <param name="control"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="colspan"></param>
    /// <param name="rowspan"></param>
    public void AddControl(UIElement control, int column, int row, int colspan = 1, int rowspan = 1)
    {
        TablePanel.AddControl(control, column, row, colspan, rowspan);
    }
}