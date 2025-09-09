using System.Windows.Controls;
using Connection.S7;
using UI.Converters;
using S7NodeEditorViewModel = UI.ViewModels.S7NodeEditorViewModel;

namespace UI.Controls;

public partial class S7NodeEditorControl : UserControl
{
    private S7NodeEditorViewModel _viewModel;

    public S7NodeEditorControl()
    {
        InitializeComponent();
    }

    public S7NodeEditorControl(S7NodeEditorViewModel vm) : this()
    {
        _viewModel = vm;
        DataContext = _viewModel;
    }

    public void LoadConfig(S7PlcConfig config)
    {
        if (config != null)
        {
            _viewModel = new S7NodeEditorViewModel
            {
                Nodes = S7NodeConverter.ToViewModelCollection(config.Nodes.ToList())
            };
            DataContext = _viewModel;
        }
    }
}