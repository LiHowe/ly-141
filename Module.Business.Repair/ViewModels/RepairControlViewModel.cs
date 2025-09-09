using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Models;
using Data.SqlSugar;
using Logger;

namespace Module.Business.Repair.ViewModels
{
    /// <summary>
    /// 返修控制视图模型
    /// </summary>
    public partial class RepairControlViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<RepairRecordViewModel> repairRecords = new();

        [ObservableProperty]
        private bool isLoading;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RepairControlViewModel()
        {
            _ = LoadDataAsync(); // 初始化时加载数据
        }

        /// <summary>
        /// 刷新命令
        /// </summary>
        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // 模拟数据加载，实际应该从数据库获取
                var records = await GetRepairRecordsFromDatabaseAsync();

                // 使用 Dispatcher 确保在 UI 线程上更新
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    RepairRecords.Clear();
                    foreach (var record in records)
                    {
                        RepairRecords.Add(new RepairRecordViewModel(record));
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error("加载返修记录失败", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 从数据库获取返修记录
        /// </summary>
        private async Task<List<RepairRecord>> GetRepairRecordsFromDatabaseAsync()
        {
            try
            {
                // 这里应该使用实际的数据库连接
                // 暂时返回模拟数据
                await Task.Delay(800); // 模拟网络延迟

                return new List<RepairRecord>
                {
                    // 已上料状态 - 成功状态
                    new RepairRecord
                    {
                        PartCode = "WM2024010001",
                        UnloadTime = DateTime.Now.AddHours(-3),
                        LoadTime = DateTime.Now.AddMinutes(-45),
                        CreateTime = DateTime.Now.AddHours(-4)
                    },
                    // 待上料状态 - 危险状态
                    new RepairRecord
                    {
                        PartCode = "WM2024010002",
                        UnloadTime = DateTime.Now.AddHours(-2),
                        LoadTime = null, // 未上料
                        CreateTime = DateTime.Now.AddHours(-3)
                    },
                    // 已上料状态 - 成功状态
                    new RepairRecord
                    {
                        PartCode = "WM2024010003",
                        UnloadTime = DateTime.Now.AddMinutes(-90),
                        LoadTime = DateTime.Now.AddMinutes(-20),
                        CreateTime = DateTime.Now.AddMinutes(-120)
                    },
                    // 待上料状态 - 危险状态
                    new RepairRecord
                    {
                        PartCode = "WM2024010004",
                        UnloadTime = DateTime.Now.AddMinutes(-30),
                        LoadTime = null, // 未上料
                        CreateTime = DateTime.Now.AddMinutes(-45)
                    },
                    // 已上料状态 - 成功状态
                    new RepairRecord
                    {
                        PartCode = "WM2024010005",
                        UnloadTime = DateTime.Now.AddHours(-1),
                        LoadTime = DateTime.Now.AddMinutes(-10),
                        CreateTime = DateTime.Now.AddHours(-2)
                    },
                    // 待上料状态 - 危险状态（刚下料）
                    new RepairRecord
                    {
                        PartCode = "WM2024010006",
                        UnloadTime = DateTime.Now.AddMinutes(-5),
                        LoadTime = null, // 未上料
                        CreateTime = DateTime.Now.AddMinutes(-10)
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Error("从数据库获取返修记录失败", ex);
                return new List<RepairRecord>();
            }
        }


    }

    /// <summary>
    /// 返修记录视图模型
    /// </summary>
    public partial class RepairRecordViewModel : ObservableObject
    {
        private readonly RepairRecord _record;

        public RepairRecordViewModel(RepairRecord record)
        {
            _record = record;
        }

        /// <summary>
        /// 记录ID
        /// </summary>
        public int Id => _record.Id;

        /// <summary>
        /// 零件码
        /// </summary>
        public string PartCode => _record.PartCode ?? "未知";

        /// <summary>
        /// 下料时间
        /// </summary>
        public DateTime UnloadTime => _record.UnloadTime;

        /// <summary>
        /// 下料时间显示文本
        /// </summary>
        public string UnloadTimeText => UnloadTime.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// 上料时间
        /// </summary>
        public DateTime? LoadTime => _record.LoadTime;

        /// <summary>
        /// 上料时间显示文本
        /// </summary>
        public string LoadTimeText => LoadTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";

        /// <summary>
        /// 是否已上料
        /// </summary>
        public bool IsLoaded => LoadTime.HasValue;

        /// <summary>
        /// 状态文本
        /// </summary>
        public string StatusText => IsLoaded ? "已重新上料" : "待重新上料";

        /// <summary>
        /// 状态样式键
        /// </summary>
        public string StatusStyleKey => IsLoaded ? "SuccessStatusIndicator" : "DangerStatusIndicator";
    }
}
