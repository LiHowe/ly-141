using CommunityToolkit.Mvvm.ComponentModel;

namespace Module.Business.SG141.ViewModels
{
	/// <summary>
	/// 产线状态 - 包含线速 与 线体运行状态
	/// </summary>
	public partial class LineStatusViewModel: ObservableObject
	{
		[ObservableProperty]
		private double _value = 45;

		public LineStatusViewModel()
		{
			
		}
	}
}
