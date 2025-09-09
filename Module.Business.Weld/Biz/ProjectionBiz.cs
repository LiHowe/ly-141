using System.Collections.ObjectModel;
using Core.Models.Records;
using UI.Controls;
using UI.ViewModels;

namespace Module.Business.Biz;
/// <summary>
/// 凸焊逻辑 - PLC
/// </summary>
public class ProjectionBiz: BizBase<ProjectionWeldRecord>
{
    public override string Name  => "凸焊业务";
    public override string TargetPlcKey => "plc_1";
    
    private MonitorBlockViewModel _monitorBlockViewModel;
    
    /// <summary>
    /// 电流数据
    /// </summary>
    private ObservableCollection<double> CurrentDataList { get; set; } = new();
    /// <summary>
    ///  电压数据
    /// </summary>
    private ObservableCollection<double> VoltageDataList { get; set; } = new();
    /// <summary>
    ///  时间数据
    /// </summary>
    private ObservableCollection<double> TimeDataList { get; set; } = new();


    protected override void InitComponents()
    {
        _monitorBlockViewModel = new MonitorBlockViewModel();
        Control = new MonitorBlockControl((MonitorBlockViewModel)_monitorBlockViewModel);
        _monitorBlockViewModel.Title = "凸焊测试";

        _monitorBlockViewModel.AddDisplay("current", new()
        {
            Label = "电流",
            SubLabel = "Weld Current",
            Unit = "A"
        });
        
        _monitorBlockViewModel.AddDisplay("Vol", new()
        {
            Label = "电压",
            SubLabel = "Weld Current",
            Unit = "A"
        });

        _monitorBlockViewModel.AddDisplay("Time", new()
        {
            Label = "时间",
            SubLabel = "Weld Current",
            Unit = "A"
        });
        
        _monitorBlockViewModel.AddDisplay("index", new()
        {
            Label = "焊点序号",
            SubLabel = "Spot Index",
        });
}

    protected override void UseStrategy()
    {
        throw new NotImplementedException();
    }
}