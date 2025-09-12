using Module.Business.SG141.ViewModels;
using System.Windows.Controls;

namespace Module.Business.SG141.Controls
{
	/// <summary>
	/// LineStatus.xaml 的交互逻辑
	/// </summary>
	public partial class LineStatus : UserControl
	{
		public LineStatus()
		{
			InitializeComponent();
			LineStatusViewModel vm = new();
			DataContext = vm;
		}
	}
}
