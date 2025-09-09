using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Controls;

public partial class S7PlcEditControl : UserControl
{
    private readonly S7PlcEditViewModel _viewModel;

    public S7PlcEditControl()
    {
        InitializeComponent();
    }

    public S7PlcEditControl(S7PlcEditViewModel vm) : this()
    {
        _viewModel = vm;
        DataContext = _viewModel;
    }
}