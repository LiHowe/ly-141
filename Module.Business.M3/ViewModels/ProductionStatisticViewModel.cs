using CommunityToolkit.Mvvm.ComponentModel;

namespace Module.Business.SG141.ViewModels
{
	public partial class ProductionStatisticViewModel: ObservableObject
	{

		[ObservableProperty]
		private double[] _okValues = new double[24];

		[ObservableProperty]

		private double[] _ngValues= new double[24];

		[ObservableProperty]

		private string[] _labels = new string[24];


		public ProductionStatisticViewModel()
		{
			GenerateTimeLabels();
			MockDatas();
		}

		private void GenerateTimeLabels()
		{
			var today = DateTime.Today;
			Labels = new string[24];
			for (var i = 0; i < 24; i++)
			{
				Labels[i] = today.AddHours(i).ToString("HH");
			}
		}
		
		private void MockDatas()
		{
			Random r = new();
			for (var i = 0; i < 24; i++)
			{
				OkValues[i] = r.Next(0, 100);
				NgValues[i] = r.Next(0, 20);
			}
		}
	}
}
