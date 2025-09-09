using System.Collections.ObjectModel;
using Core.Models.Records;
using Core.Repositories;
using Core.Services;
using Core.Utils;
using Module.Business.BizStrategy;
using UI.Controls;
using UI.ViewModels;

namespace Module.Business.Biz;

/// <summary>
/// 点焊逻辑
/// </summary>
/// <param name="monitorView"></param>
public class SpotBiz() : BizBase<SpotWeldRecord>()
{
    public override string Name => "点焊业务";

    public override string TargetPlcKey => "plc_1";

    private MonitorBlockViewModel _monitorBlockViewModel;
    
    private ObservableCollection<double> CurrentDataList { get; set; } = new();

    protected override async void InitComponents()
    {
        _monitorBlockViewModel = new MonitorBlockViewModel();
        Control = new MonitorBlockControl((MonitorBlockViewModel)_monitorBlockViewModel);
        
        _monitorBlockViewModel.Title = "点焊测试";
        _monitorBlockViewModel.AddDisplay("Current", new()
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
        
        _monitorBlockViewModel.AddPlot(new()
        {
            Key = "测试",
            ShowLegend = false,
            YLabel = "电流 / A",
            DataSeries = new()
            {
                new()
                {
                    Legend = "电流1",
                    DataY = CurrentDataList,
                },
            }
        });
    }

    protected override async void UseStrategy()
    {
        SpotBizStrategy simple = new("基础点焊策略");
        
        var plcConfigs = await ConfigManager.Instance.LoadPlcConfigAsync();
        var plcConfig = plcConfigs.Get(TargetPlcKey);
        if (plcConfig == null)
        {
            MessageBox.Error($"点焊逻辑初始化失败，缺少plc1配置");
            return;
        }
        var plc = plcConfig.GetPlc();
        SpotBizStrategyConfig config = new()
        {
            StartNodeKey = "r1.start",
            CodeNodeKey = "r1.vin",
            EndNodeKey = "r1.end",
            SpotIndexNode = "r1.index",
            Mode = StrategyStartType.StartEnd,
            PlcConfig = plcConfig,
            TargetPlcKey = TargetPlcKey,
            
            DataNodeKeys =
            {
                "r1.current",
                "r1.voltage",
                "r1.time",
            }
        };
        simple.Apply(config);

        string currentCode = string.Empty;
        
        simple.Started += (sender, args) =>
        {
            UIThreadHelper.InvokeAsync(() =>
            {
                Control.Start();
            });
        };

        simple.CodeReceived += (sender, args) =>
        {
            UIThreadHelper.InvokeAsync(() =>
            {
                currentCode = args.Message;
                _monitorBlockViewModel.Vin = currentCode;
            });
            RecordRepository.Record.SerialNo = args.Message;
        };

        simple.DataReceived += (sender, args) =>
        {
            var data = args.Data;
            // 保存点焊数据
            RecordRepository.Record.Current1 = double.Parse(data["r1.current"].ToString());
            RecordRepository.Record.Voltage = double.Parse(data["r1.voltage"].ToString());
            RecordRepository.Record.WeldTime = int.Parse(data["r1.time"].ToString());
            RecordRepository.SaveAndCreateRecord();
            RecordRepository.Record.SerialNo = currentCode;
            UIThreadHelper.InvokeAsync(() =>
            {
                _monitorBlockViewModel.SetDisplay("Current", data["r1.current"].ToString());
                _monitorBlockViewModel.SetDisplay("Vol", data["r1.voltage"].ToString());
                _monitorBlockViewModel.SetDisplay("Time", data["r1.time"].ToString());
                CurrentDataList.Add(double.Parse(data["r1.current"].ToString()));
            });
           
        };

        simple.SpotIndexChanged += (idx) =>
        {
            RecordRepository.Record.SpotIndex = idx;
            UIThreadHelper.InvokeAsync(() =>
            {
                _monitorBlockViewModel.SetDisplay("index", idx.ToString());
            });
        };
        
        simple.Ended += (sender, args) =>
        {
            if (RecordRepository.Record.SpotIndex != 0)
            {
                RecordRepository.SaveAndCreateRecord();
            }
            UIThreadHelper.InvokeAsync(() =>
            {
                Control?.Stop();
                Control?.Reset();
            });
            
        };

    }
}