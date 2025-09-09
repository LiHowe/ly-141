using System.Windows;
using System.Windows.Controls;

namespace UI.Controls;

/// <summary>
///     动态表格布局
/// </summary>
public partial class DynamicTablePanel
{
    private int _columns = 4;
    private int _rows = 2;

    public DynamicTablePanel()
    {
        InitializeComponent();
        RebuildGrid();
    }

    public void AddControl(UIElement control, int column, int row, int colspan = 1, int rowspan = 1)
    {
        if (column < 0 || row < 0 || column >= _columns || row >= _rows)
            throw new ArgumentOutOfRangeException("Column or row index is out of bounds.");

        if (column + colspan > _columns || row + rowspan > _rows)
            throw new ArgumentOutOfRangeException("Colspan or rowspan exceeds grid bounds.");

        SetColumn(control, column);
        SetRow(control, row);

        if (colspan > 1)
            SetColumnSpan(control, colspan);

        if (rowspan > 1)
            SetRowSpan(control, rowspan);

        Children.Add(control);
    }


    /// <summary>
    ///     会根据传入的总元素数量来自动规划列数。行固定为2行
    /// </summary>
    /// <param name="totalItems"></param>
    public void AutoAdjustLayout(int totalItems)
    {
        // 例如根据总数量和当前行数重新计算列数
        var newColumns = (int)Math.Ceiling((double)totalItems / _rows);

        if (newColumns != _columns)
        {
            _columns = newColumns;
            RebuildGrid();

            // 重新添加已有控件到新位置
            var existingControls = Children.Cast<UIElement>().ToList();
            Children.Clear();

            for (var i = 0; i < existingControls.Count; i++)
            {
                var col = i % _columns;
                var row = i / _columns;
                AddControl(existingControls[i], col, row);
            }
        }
    }

    /// <summary>
    ///     手动设置行列数量
    /// </summary>
    /// <param name="col">列, 默认4</param>
    /// <param name="row">行, 默认2</param>
    public void AdjustLayout(int col = 4, int row = 2)
    {
        if (col == _columns && row == _rows) return;
        _columns = col;
        _rows = row;
        RebuildGrid();
    }

    private void RebuildGrid()
    {
        ColumnDefinitions.Clear();
        RowDefinitions.Clear();

        for (var i = 0; i < _columns; i++)
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < _rows; i++)
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
    }
}