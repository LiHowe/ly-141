using System.Windows.Controls;
using Module.Business.Repair.ViewModels;

namespace Module.Business.Repair;

public partial class RepairControl : UserControl
{
    public RepairControl()
    {
        InitializeComponent();
        DataContext = new RepairControlViewModel();
    }
}