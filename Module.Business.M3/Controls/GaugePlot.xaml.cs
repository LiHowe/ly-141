using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Module.Business.SG141.Controls
{
	/// <summary>
	/// GaugePlot.xaml 的交互逻辑
	/// </summary>
	public partial class GaugePlot : UserControl
	{
		// 依赖属性：当前值
		public static readonly DependencyProperty CurrentValueProperty =
			DependencyProperty.Register(nameof(CurrentValue), typeof(double), typeof(GaugePlot),
				new PropertyMetadata(0.0, OnValueChanged));

		public double CurrentValue
		{
			get => (double)GetValue(CurrentValueProperty);
			set => SetValue(CurrentValueProperty, value);
		}

		// 依赖属性：最大值
		public static readonly DependencyProperty MaxValueProperty =
			DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(GaugePlot),
				new PropertyMetadata(200.0, OnValueChanged));

		public double MaxValue
		{
			get => (double)GetValue(MaxValueProperty);
			set => SetValue(MaxValueProperty, value);
		}

		private double _currentAngle;  // 当前指针角度
		private double _radius;  // 动态半径
		private double _centerX;  // 动态中心 X
		private double _centerY;  // 动态中心 Y

		public GaugePlot()
		{
			InitializeComponent();
			SizeChanged += OnSizeChanged;  // 监听尺寸变化
			MouseDoubleClick += (s, e) => OnSizeChanged(s, null);
		}

		// 尺寸变化时更新
		private void OnSizeChanged(object sender, SizeChangedEventArgs? e)
		{
			_centerX = ActualWidth / 2;
			_centerY = ActualHeight / 1.2;
			_radius = Math.Min(ActualWidth, ActualHeight) * 0.7;  // 90% 留边距
			UpdateVisuals();
			DrawScale();
		}

		// 属性变化回调：更新视觉
		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is GaugePlot control)
			{
				control.AnimateToValue();
			}
		}

		// 更新视觉元素
		private void UpdateVisuals()
		{
			if (MaxValue <= 0 || _radius <= 0) return;

			double percent = Math.Max(0, Math.Min(1, CurrentValue / MaxValue));
			double totalSweep = 180;  // 总扫描角度180度

			// 更新背景弧线：内侧半径、对齐厚度，顺时针绘制
			double bgRadius = _radius - (_radius * 0.15 / 2);  // 内侧对齐彩色弧线
			DrawArc(backgroundArc, 0, totalSweep, _centerX, _centerY, bgRadius, _radius * 0.08,
					FindResource("BorderBrush") as Brush, SweepDirection.Counterclockwise);

			// 绘制彩色段落（阈值：红 0-60%, 黄 60-80%, 绿 80-100%）
			double colorThickness = _radius * 0.15;

			// 修正：从左侧开始，按正确顺序绘制各个颜色段
			// 红色段：占总角度的60%（0-60%区间），从左侧180度开始
			double redSweep = totalSweep * 0.6;  // 108度
			double redStartAngle = 180;   // 从180度开始
			DrawArc(redArc, redStartAngle, redSweep, _centerX, _centerY, _radius, colorThickness,
					FindResource("DangerBrush") as Brush, SweepDirection.Clockwise);

			// 黄色段：占总角度的20%（60-80%区间）
			double yellowSweep = totalSweep * 0.2;  // 36度
			double yellowStartAngle = redStartAngle + redSweep;  // 从红色段结束位置开始：288度
			DrawArc(yellowArc, yellowStartAngle, yellowSweep, _centerX, _centerY, _radius, colorThickness,
					FindResource("WarningBrush") as Brush, SweepDirection.Clockwise);

			// 绿色段：占总角度的20%（80-100%区间）
			double greenSweep = totalSweep * 0.2;  // 36度
			double greenStartAngle = yellowStartAngle + yellowSweep;  // 从黄色段结束位置开始：324度
			DrawArc(greenArc, greenStartAngle, greenSweep, _centerX, _centerY, _radius, colorThickness,
					FindResource("SuccessBrush") as Brush, SweepDirection.Clockwise);

			// 更新指针：从左侧180度开始，顺时针旋转
			_currentAngle = 180 + (percent * totalSweep);
			UpdateNeedle(_currentAngle);

			// 更新文本位置
			Canvas.SetLeft(labelText, _centerX - 20);
			Canvas.SetTop(labelText, _centerY - _radius * 0.6);
			Canvas.SetLeft(valueText, _centerX - 10);
			Canvas.SetTop(valueText, _centerY - _radius * 0.4);
			valueText.Text = $"{CurrentValue:N0}";
		}

		// 绘制刻度
		private void DrawScale()
		{
			gaugeCanvas.Children.Clear();
			int step = (int)Math.Max(5, MaxValue / 36);  // 动态步长，确保约 36 个刻度
			for (int i = 0; i <= (int)MaxValue; i += step)
			{
				// 修正角度计算：从左侧180度开始，顺时针到右侧0度
				double angle = 180 + (i / MaxValue * 180);  
				Line lineScale = new Line();

				if (i % (step * 5) == 0)  // 大刻度
				{
					lineScale.X1 = _centerX + (_radius * 0.9) * Math.Cos(angle * Math.PI / 180);
					lineScale.Y1 = _centerY + (_radius * 0.9) * Math.Sin(angle * Math.PI / 180);
					lineScale.Stroke = FindResource("PrimaryTextBrush") as Brush;
					lineScale.StrokeThickness = 2;

					// 添加刻度值
					TextBlock txtScale = new TextBlock
					{
						Text = i.ToString(),
						FontSize = _radius * 0.12,
						Foreground = FindResource("PrimaryTextBrush") as Brush
					};
					Canvas.SetLeft(txtScale, _centerX + (_radius * 0.85) * Math.Cos(angle * Math.PI / 180) - (angle > 270 ? 20 : 0));
					Canvas.SetTop(txtScale, _centerY + (_radius * 0.85) * Math.Sin(angle * Math.PI / 180) - 0);
					gaugeCanvas.Children.Add(txtScale);
				}
				else  // 小刻度
				{
					lineScale.X1 = _centerX + (_radius * 0.95) * Math.Cos(angle * Math.PI / 180);
					lineScale.Y1 = _centerY + (_radius * 0.95) * Math.Sin(angle * Math.PI / 180);
					lineScale.Stroke = FindResource("PrimaryTextBrush") as Brush;
					lineScale.StrokeThickness = 1;
				}

				lineScale.X2 = _centerX + _radius * Math.Cos(angle * Math.PI / 180);
				lineScale.Y2 = _centerY + _radius * Math.Sin(angle * Math.PI / 180);
				gaugeCanvas.Children.Add(lineScale);
			}
		}

		// 更新指针
		private void UpdateNeedle(double angle)
		{
			Point centerPoint = new Point(_centerX, _centerY);
			Point needleEnd = CalculatePoint(angle, _radius * 0.7, _centerX, _centerY);
			LineGeometry needleGeometry = new LineGeometry(centerPoint, needleEnd);
			needle.Data = needleGeometry;

			// 修正百分比计算
			double percent = Math.Max(0, Math.Min(1, CurrentValue / MaxValue));
			Brush needleColor = percent < 0.6 ? (FindResource("DangerBrush") as Brush) :
								 percent < 0.8 ? (FindResource("WarningBrush") as Brush) :
								 (FindResource("SuccessBrush") as Brush);
			needle.Stroke = needleColor;
			center.Fill = needleColor;
			Canvas.SetLeft(center, _centerX - 5);
			Canvas.SetTop(center, _centerY - 5);

			labelText.Foreground = needleColor;
			valueText.Foreground = needleColor;
			needleRotate.CenterX = _centerX;
			needleRotate.CenterY = _centerY;
		}

		// 动画过渡
		private void AnimateToValue()
		{
			// 修正目标角度计算
			double targetAngle = 0 + (Math.Max(0, Math.Min(1, CurrentValue / MaxValue)) * 180);
			double timeAnimation = Math.Abs(_currentAngle - targetAngle) * 8 / 1000.0;

			DoubleAnimation da = new DoubleAnimation
			{
				From = _currentAngle,
				To = targetAngle,
				Duration = TimeSpan.FromSeconds(timeAnimation),
				AccelerationRatio = 0.3
			};
			da.Completed += (s, e) => _currentAngle = targetAngle;
			needleRotate.BeginAnimation(RotateTransform.AngleProperty, da);

			double percent = Math.Max(0, Math.Min(1, CurrentValue / MaxValue));
			Brush needleColor = percent < 0.6 ? (FindResource("DangerBrush") as Brush) :
								 percent < 0.8 ? (FindResource("WarningBrush") as Brush) :
								 (FindResource("SuccessBrush") as Brush);
			needle.Stroke = needleColor;

			valueText.Text = $"{CurrentValue:N0}";
		}

		// 计算点坐标
		private Point CalculatePoint(double angleDeg, double radius, double cx, double cy)
		{
			double rad = angleDeg * Math.PI / 180;
			return new Point(cx + radius * Math.Cos(rad), cy + radius * Math.Sin(rad));
		}

		// 绘制弧线
		private void DrawArc(Path path, double startAngle, double sweepAngle, double cx, double cy,
					 double radius, double thickness, Brush brush,
					 SweepDirection direction = SweepDirection.Clockwise)
		{
			double innerRadius = radius - thickness;
			double outerRadius = radius;

			// 外弧起点和终点
			Point outerStart = CalculatePoint(startAngle, outerRadius, cx, cy);
			Point outerEnd = CalculatePoint(startAngle + sweepAngle, outerRadius, cx, cy);

			// 内弧起点和终点
			Point innerStart = CalculatePoint(startAngle, innerRadius, cx, cy);
			Point innerEnd = CalculatePoint(startAngle + sweepAngle, innerRadius, cx, cy);

			bool isLargeArc = sweepAngle > 180;

			StreamGeometry geometry = new StreamGeometry();
			using (StreamGeometryContext ctx = geometry.Open())
			{
				// 从外弧起点开始
				ctx.BeginFigure(outerStart, true, true);
				// 绘制外弧到终点
				ctx.ArcTo(outerEnd, new Size(outerRadius, outerRadius), 0, isLargeArc, direction, true, false);
				// 连线到内弧终点
				ctx.LineTo(innerEnd, true, false);
				// 绘制内弧回到起点（反方向）
				ctx.ArcTo(innerStart, new Size(innerRadius, innerRadius), 0, isLargeArc,
						  direction == SweepDirection.Clockwise ? SweepDirection.Counterclockwise : SweepDirection.Clockwise,
						  true, false);
				// 闭合路径
				ctx.LineTo(outerStart, true, false);
			}
			geometry.Freeze();

			path.Data = geometry;
			path.Fill = brush;   // 使用 Fill 填充
			path.Stroke = null;  // 不使用描边
		}
	}
}