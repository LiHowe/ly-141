using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace UI.Controls;

/// <summary>
/// TimeSelector.xaml 的交互逻辑
/// </summary>
public partial class TimeSelector : UserControl
{
	public static readonly DependencyProperty StartTimeProperty =
			DependencyProperty.Register(nameof(StartTime), typeof(DateTime?), typeof(TimeSelector), new PropertyMetadata(null));

	public static readonly DependencyProperty EndTimeProperty =
		DependencyProperty.Register(nameof(EndTime), typeof(DateTime?), typeof(TimeSelector), new PropertyMetadata(null));

	public static readonly DependencyProperty QuickSelectEnabledProperty =
		DependencyProperty.Register(nameof(QuickSelectEnabled), typeof(bool), typeof(TimeSelector), new PropertyMetadata(true));

	public static readonly DependencyProperty SelectedQuickOptionProperty =
	DependencyProperty.Register(
		nameof(SelectedQuickOption),
		typeof(string),
		typeof(TimeSelector),
		new PropertyMetadata("今日", OnQuickOptionChanged));

	public DateTime? StartTime
	{
		get => (DateTime?)GetValue(StartTimeProperty);
		set => SetValue(StartTimeProperty, value);
	}

	public DateTime? EndTime
	{
		get => (DateTime?)GetValue(EndTimeProperty);
		set => SetValue(EndTimeProperty, value);
	}

	public bool QuickSelectEnabled
	{
		get => (bool)GetValue(QuickSelectEnabledProperty);
		set => SetValue(QuickSelectEnabledProperty, value);
	}

	public string SelectedQuickOption
	{
		get => (string)GetValue(SelectedQuickOptionProperty);
		set => SetValue(SelectedQuickOptionProperty, value);
	}

	public ObservableCollection<string> QuickOptions { get; } =
		new ObservableCollection<string> { "今日", "本周", "本月", "今年", "自定义" };

	public TimeSelector()
	{
		InitializeComponent();
		ApplyQuickSelection();
	}

	private static void OnQuickOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is TimeSelector ts)
			ts.ApplyQuickSelection();
	}

	private void ApplyQuickSelection()
	{
		if (SelectedQuickOption == "自定义")
			return;

		var now = DateTime.Now;
		switch (SelectedQuickOption)
		{
			case "今日":
				StartTime = now.Date;
				EndTime = now;
				break;

			case "本周":
				int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
				StartTime = now.Date.AddDays(-diff);
				EndTime = now;
				break;

			case "近7天":
				StartTime = now.Date.AddDays(-6); // 含今天
				EndTime = now;
				break;

			case "本月":
				StartTime = new DateTime(now.Year, now.Month, 1);
				EndTime = now;
				break;

			case "本季度":
				int quarter = (now.Month - 1) / 3 + 1;
				StartTime = new DateTime(now.Year, (quarter - 1) * 3 + 1, 1);
				EndTime = now;
				break;

			case "上半年":
				StartTime = new DateTime(now.Year, 1, 1);
				EndTime = new DateTime(now.Year, 6, 30, 23, 59, 59);
				break;

			case "下半年":
				StartTime = new DateTime(now.Year, 7, 1);
				EndTime = new DateTime(now.Year, 12, 31, 23, 59, 59);
				break;

			case "今年":
				StartTime = new DateTime(now.Year, 1, 1);
				EndTime = now;
				break;
		}
	}
	// RadioButton Checked 事件：把选中的项写回 SelectedQuickOption（由此触发 ApplyQuickSelection）
	private void QuickOptionRadio_Checked(object sender, RoutedEventArgs e)
	{
		if (sender is RadioButton rb && rb.Content is string option)
		{
			// 仅在不同的时候赋值，避免多余触发
			if (SelectedQuickOption != option)
				SelectedQuickOption = option;
		}
	}
}
