using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Module.Business.SG141.ViewModels
{
	public partial class FaultPlotViewModel: ObservableObject
	{
		[ObservableProperty]
		private ObservableCollection<PieData> _data = [
			new("Mary", 10),
			new("John", 20),
			new("Alice", 30),
			new("Bob", 40),
			new("Charlie", 50)
		];
	}

	public class PieData(string name, double value)
	{
		public string Name { get; set; } = name;
		public double[] Values { get; set; } = [value];
	}
}
