using Module.Business.SG141.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Module.Business.SG141.Controls
{
	/// <summary>
	/// FaultPlot.xaml 的交互逻辑
	/// </summary>
	public partial class FaultPlot : UserControl
	{
		public FaultPlot()
		{
			InitializeComponent();
			FaultPlotViewModel vm = new();
			DataContext = vm;
		}
	}
}
