using Core.Utils;
using Module.Business.SG141.ViewModels;
using ScottPlot;
using ScottPlot.Interactivity;
using System.Windows.Controls;

namespace Module.Business.SG141.Controls;

public partial class TrendPlot : UserControl
{
    
    ///// <summary>
    ///// 图表的X轴数据
    ///// </summary>
    //private DateTime[] _xs = new DateTime[25];
    ///// <summary>
    ///// 计划产量
    ///// </summary>
    //private double[] _planY = new double[25];
    ///// <summary>
    ///// 实际产量
    ///// </summary>
    //private double[] _actualY = new double[25];
    
    //private SG141Settings? _settings;
    
    public TrendPlot()
    {
        InitializeComponent();
        TrendPlotViewModel vm = new();
        DataContext = vm;
		//GetModuleSettings();
		//InitPlot();
	}

    /// <summary>
    /// 获取模块配置数据
    /// </summary>
    //private void GetModuleSettings()
    //{
    //    _settings = ConfigManager.Instance.LoadConfig<SG141Settings>(SG141Module.SettingFilePath) ?? new();
    //}

    /// <summary>
    /// 根据配置中的计划产量来计算24小时计划产量数据
    /// </summary>
    //private void CalculatePlanYData()
    //{
    //    _planY = new double[25];
    //    // 今日产量
    //    var count = _settings?.TodayPlan ?? 0;
    //    if (count <= 0) return;
    //    for (int i = 0; i <= 24; i++)
    //    {
    //        _planY[i] = (count / 24) * (i);
    //    }
    //    _planY[24] = count;
    //}

    ///// <summary>
    ///// 根据传入日期构建当天的0点到23点的数据
    ///// </summary>
    ///// <param name="date"> </param>
    //private void GenerateXAxisData(DateTime date)
    //{
    //    _xs = new DateTime[25];
    //    for (int i = 0; i <= 24; i++)
    //    {
    //        _xs[i] = date.AddHours(i);
    //    }
    //}
    
 //   /// <summary>
 //   /// 初始化图表
 //   /// </summary>
 //   private void InitPlot()
 //   {
 //       var p = Plot.Plot;
        
	//	p.Axes.DateTimeTicksBottom();
	//	GenerateXAxisData(DateTime.Today);
 //       CalculatePlanYData();
		
	//	InitPlotStyle();
	//	RenderPlot();
 //   }

 //   private void InitPlotStyle()
 //   {
	//	var p = Plot.Plot;
	//	//p.Grid.MajorLineColor = Color.FromHex("#0e3d54");
	//	p.FigureBackground.Color = Color.FromHex("#243B76");
	//	p.DataBackground.Color = Color.FromHex("#243B76");
	//	p.Axes.Color(Color.FromHex("#e9e9e9"));
 //       p.Legend.IsVisible = true;
 //       p.Axes.Right.FrameLineStyle.Width = 0;
 //       p.Axes.Top.FrameLineStyle.Width = 0;
 //       p.ScaleFactor = 1.2;
 //       Plot.UserInputProcessor.IsEnabled = false;

 //       p.RenderManager.RenderStarting += (s, e) =>
 //       {
 //           Tick[] ticks = p.Axes.Bottom.TickGenerator.Ticks;
 //           for (int i = 0; i < ticks.Length; i++)
 //           {
	//			DateTime dt = DateTime.FromOADate(ticks[i].Position);
 //               string label = $"{dt:HH:mm}";
	//			ticks[i] = new Tick(ticks[i].Position, label);
	//		}
 //       };
	//}

	///// <summary>
	///// 渲染图表
	///// </summary>
	//private void RenderPlot()
 //   {
 //       var p = Plot.Plot;
 //       p.Clear();
 //       p.Add.Scatter(_xs, _planY);
 //       p.Add.Scatter(_xs, _actualY);
	//	var count = _settings?.TodayPlan ?? 0;
	//	if (count > 0)
	//	{
	//		var axLine4 = p.Add.HorizontalLine(count);
 //           //axLine4.LinePattern = LinePattern.Dotted;
	//		//axLine4.Text = count.ToString();
	//		//axLine4.LabelOppositeAxis = true;
 //       }
	//	p.Font.Automatic();
	//	Plot.Refresh();
 //   }
}