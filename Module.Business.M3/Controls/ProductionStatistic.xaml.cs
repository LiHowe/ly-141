using Module.Business.SG141.ViewModels;
using System.Windows.Controls;

namespace Module.Business.SG141.Controls
{
	/// <summary>
	/// ProductionStatistic.xaml 的交互逻辑
	/// </summary>
	public partial class ProductionStatistic : UserControl
	{

		public ProductionStatistic()
		{
			ProductionStatisticViewModel vm = new();
			DataContext = vm;
			InitializeComponent();
		}
	}
}
