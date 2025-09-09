using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Controls;

public partial class S7PlcControl : UserControl
{
    public S7PlcControl()
    {
        // S7PlcViewModel vm = new();
        InitializeComponent();
        // DataContext = vm;
    }

    public S7PlcControl(S7PlcViewModel vm) : this()
    {
    }
}