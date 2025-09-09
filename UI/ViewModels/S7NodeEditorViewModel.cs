using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MiniExcelLibs;
using UI.Controls;
using MessageBox = UI.Controls.MessageBox;

namespace UI.ViewModels;

public partial class S7NodeEditorViewModel : ObservableObject
{
    [ObservableProperty] private int _nodeCount;

    [ObservableProperty] private ObservableCollection<S7NodeViewModel> _nodes;

    [ObservableProperty] private S7NodeViewModel? _selectedNode;

    public S7NodeEditorViewModel()
    {
        Nodes = new ObservableCollection<S7NodeViewModel>();
        UpdateNodeCount();
    }

    partial void OnNodesChanged(ObservableCollection<S7NodeViewModel> value)
    {
        UpdateNodeCount();
    }

    [RelayCommand]
    private void AddNode()
    {
        // 获取最后一个节点的DB值，如果没有节点则使用默认值801
        var lastDb = Nodes.LastOrDefault()?.Db ?? 801;

        Nodes.Add(new S7NodeViewModel
        {
            Type = "bool",
            Db = lastDb,
            Offset = "0.0",
            BitLength = 1,
            Description = "新节点",
            Title = "new.node",
            Enabled = true
        });
        UpdateNodeCount();
    }

    [RelayCommand]
    private void DeleteNode()
    {
        if (SelectedNode != null)
        {
            var result = MessageBox.Question("确定删除节点 " + SelectedNode.Title + " 吗？", "确认删除");
            if (result == MessageBoxResult.Yes)
            {
                Nodes.Remove(SelectedNode);
                UpdateNodeCount();
            }
        }
        else
        {
            MessageBox.Warn("请先选择要删除的节点");
        }
    }

    [RelayCommand]
    private void SetAllDb()
    {
        var dialog = new InputDialog
        {
            Title = "设置统一DB号"
        };

        if (dialog.ShowDialog() == true)
        {
            if (int.TryParse(dialog.Content.ToString(), out var db))
                foreach (var node in Nodes)
                    node.Db = db;
            else
                MessageBox.Error("请输入有效的DB号");
        }
    }

    public event EventHandler? OnSave;

    [RelayCommand]
    private void Save()
    {
        OnSave?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Test()
    {
        // 实现测试逻辑，例如验证节点配置
        MessageBox.Show("待实现");
    }

    /// <summary>
    ///     校验节点命令
    /// </summary>
    [RelayCommand]
    private void ValidateNodes()
    {
        var errors = new List<string>();

        // 检查空标题
        var emptyTitles = Nodes.Where(n => string.IsNullOrWhiteSpace(n.Title)).ToList();
        if (emptyTitles.Any()) errors.Add($"发现 {emptyTitles.Count} 个节点标题为空");

        // 检查重复标题
        var duplicateTitles = Nodes
            .Where(n => !string.IsNullOrWhiteSpace(n.Title))
            .GroupBy(n => n.Title)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateTitles.Any()) errors.Add($"发现重复标题: {string.Join(", ", duplicateTitles)}");

        // 检查重复偏移量（同一DB下）
        var duplicateOffsets = Nodes
            .Where(n => !string.IsNullOrWhiteSpace(n.Offset))
            .GroupBy(n => new { n.Db, n.Offset })
            .Where(g => g.Count() > 1)
            .Select(g => $"DB{g.Key.Db}.{g.Key.Offset}")
            .ToList();
        if (duplicateOffsets.Any()) errors.Add($"发现重复偏移量: {string.Join(", ", duplicateOffsets)}");

        // 显示结果
        if (errors.Any())
        {
            var errorMessage = "校验发现以下问题:\n\n" + string.Join("\n", errors);
            MessageBox.Error(errorMessage, "校验失败");
        }
        else
        {
            MessageBox.Success("校验通过，所有节点配置正确！", "校验成功");
        }
    }

    /// <summary>
    ///     Excel导入命令
    /// </summary>
    [RelayCommand]
    private async Task ImportExcel()
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx|Excel文件 (*.xls)|*.xls",
                Title = "选择要导入的Excel文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var importedNodes = await Task.Run(() => ImportNodesFromExcel(openFileDialog.FileName));

                if (importedNodes.Any())
                {
                    // 询问是否清空现有节点
                    var result = MessageBox.Question(
                        $"将导入 {importedNodes.Count} 个节点。\n是否清空现有节点？\n\n选择'是'：清空现有节点后导入\n选择'否'：追加到现有节点",
                        "导入确认");

                    if (result == MessageBoxResult.Yes) Nodes.Clear();

                    foreach (var node in importedNodes) Nodes.Add(node);

                    UpdateNodeCount();
                    MessageBox.Success($"成功导入 {importedNodes.Count} 个节点！", "导入成功");
                }
                else
                {
                    MessageBox.Warn("Excel文件中没有找到有效的节点数据", "导入警告");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Error($"导入失败: {ex.Message}", "导入错误");
        }
    }

    /// <summary>
    ///     Excel导出命令
    /// </summary>
    [RelayCommand]
    private async Task ExportExcel()
    {
        try
        {
            if (!Nodes.Any())
            {
                MessageBox.Warn("没有节点数据可导出", "导出警告");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"S7节点配置_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await Task.Run(() => ExportNodesToExcel(saveFileDialog.FileName));
                MessageBox.Success($"成功导出 {Nodes.Count} 个节点到文件:\n{saveFileDialog.FileName}", "导出成功");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Error($"导出失败: {ex.Message}", "导出错误");
        }
    }

    private void UpdateNodeCount()
    {
        NodeCount = Nodes.Count;
    }

    /// <summary>
    ///     从Excel文件导入节点数据
    /// </summary>
    private List<S7NodeViewModel> ImportNodesFromExcel(string filePath)
    {
        var nodes = new List<S7NodeViewModel>();

        try
        {
            var rows = MiniExcel.Query(filePath, true).ToList();

            foreach (var row in rows)
            {
                var rowDict = row as IDictionary<string, object>;
                if (rowDict == null) continue;

                // 跳过空行
                if (rowDict.Values.All(v => v == null || string.IsNullOrWhiteSpace(v.ToString())))
                    continue;

                var node = new S7NodeViewModel();

                // 解析各个字段
                if (rowDict.TryGetValue("标题", out var title) || rowDict.TryGetValue("Title", out title))
                    node.Title = title?.ToString() ?? "";

                if (rowDict.TryGetValue("DB", out var db))
                    node.Db = int.TryParse(db?.ToString(), out var dbValue) ? dbValue : 801;

                if (rowDict.TryGetValue("偏移量", out var offset) || rowDict.TryGetValue("Offset", out offset))
                    node.Offset = offset?.ToString() ?? "0.0";

                if (rowDict.TryGetValue("类型", out var type) || rowDict.TryGetValue("Type", out type))
                    node.Type = type?.ToString() ?? "bool";

                if (rowDict.TryGetValue("位长度", out var bitLength) || rowDict.TryGetValue("BitLength", out bitLength))
                    node.BitLength = int.TryParse(bitLength?.ToString(), out var bitLengthValue) ? bitLengthValue : 1;

                if (rowDict.TryGetValue("描述", out var description) ||
                    rowDict.TryGetValue("Description", out description))
                    node.Description = description?.ToString() ?? "";

                if (rowDict.TryGetValue("启用", out var enabled) || rowDict.TryGetValue("Enabled", out enabled))
                    node.Enabled = bool.TryParse(enabled?.ToString(), out var enabledValue) ? enabledValue : true;

                if (rowDict.TryGetValue("需要反馈", out var needFeedback) ||
                    rowDict.TryGetValue("NeedFeedback", out needFeedback))
                    node.NeedFeedback = bool.TryParse(needFeedback?.ToString(), out var needFeedbackValue)
                        ? needFeedbackValue
                        : false;

                if (rowDict.TryGetValue("反馈节点键", out var feedbackNodeKey) ||
                    rowDict.TryGetValue("FeedbackNodeKey", out feedbackNodeKey))
                    node.FeedbackNodeKey = feedbackNodeKey?.ToString() ?? "";

                // 只有标题不为空才添加节点
                if (!string.IsNullOrWhiteSpace(node.Title)) nodes.Add(node);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"解析Excel文件失败: {ex.Message}", ex);
        }

        return nodes;
    }

    /// <summary>
    ///     导出节点数据到Excel文件
    /// </summary>
    private void ExportNodesToExcel(string filePath)
    {
        try
        {
            var exportData = Nodes.Select(node => new
            {
                标题 = node.Title,
                DB = node.Db,
                偏移量 = node.Offset,
                类型 = node.Type,
                位长度 = node.BitLength,
                描述 = node.Description,
                启用 = node.Enabled,
                需要反馈 = node.NeedFeedback,
                反馈节点键 = node.FeedbackNodeKey
            }).ToList();

            MiniExcel.SaveAs(filePath, exportData);
        }
        catch (Exception ex)
        {
            throw new Exception($"保存Excel文件失败: {ex.Message}", ex);
        }
    }
}