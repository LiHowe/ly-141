using CommunityToolkit.Mvvm.ComponentModel;
using Core.Utils;
using Logger;

namespace Module.Business.SG141.ViewModels
{
	/// <summary>
	/// 今日生产趋势
	/// </summary>
	public partial class TrendPlotViewModel:ObservableObject
	{
		[ObservableProperty]
		private string[] _labels = new string[24];
		/// <summary>
		/// 计划产量
		/// </summary>
		[ObservableProperty]
		private double[] _planValues = new double[24];
		/// <summary>
		/// 实际产量
		/// </summary>
		[ObservableProperty]
		private double[] _actualValues = new double[24];

		private SG141Settings? _settings;

		public TrendPlotViewModel()
		{
			LoadSettings();
			GenerateTimeLabels();
			GeneratePlanValues();
			MockDatas();
		}

		private void LoadSettings()
		{
			var config = ConfigManager.Instance.LoadConfig<SG141Settings>(SG141Module.SettingFilePath);
			if (config == null)
			{
				Log.Error("TrendPlotViewModel未找到模块配置文件");
				return;
			}
			_settings = config;
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

		private void GeneratePlanValues()
		{
			var v = _settings?.DailyPlanCount ?? 100;
			if (v <= 0) return;
			for(var i = 0;i < 24;i++)
			{
				PlanValues[i] = v / 24 * i;
			}
		}

		private void MockDatas()
		{
			Random r = new();
			for (var i = 0; i < 24; i++)
			{
				ActualValues[i] = r.Next(0, 100);
			}
		}
	}
}
