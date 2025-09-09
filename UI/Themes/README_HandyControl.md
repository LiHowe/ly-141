# WinmTech-Scada HandyControl 集成指南

## 概述

本项目已成功集成 HandyControl UI 库，提供现代化、美观的工业SCADA界面控件。HandyControl 是一个基于 WPF 的开源 UI
控件库，提供了丰富的现代化控件和主题。

## 集成内容

### 1. 核心集成文件

- **HandyControlTheme.xaml**: HandyControl 主题集成文件，包含所有自定义样式
- **Generic_HandyControl.xaml**: 更新后的主题资源字典，整合 HandyControl 和项目自定义样式
- **Core.csproj**: 已添加 HandyControl NuGet 包引用

### 2. 已替换的控件

#### 输入控件

- `TextBox` → `hc:TextBox` (使用 `WinmTextBox` 样式)
- `PasswordBox` → `hc:PasswordBox` (使用 `WinmPasswordBox` 样式)
- `ComboBox` → `hc:ComboBox` (使用 `WinmComboBox` 样式)
- `CheckBox` → `CheckBox` (使用 `WinmCheckBox` 样式)
- `DatePicker` → `hc:DatePicker` (使用 `WinmDatePicker` 样式)

#### 操作控件

- `Button` → `Button` (多种样式可选)
- `DataGrid` → `hc:DataGrid` (使用 `WinmDataGrid` 样式)

### 3. 自定义样式定义

#### 按钮样式

- `WinmPrimaryButton`: 主要按钮样式 (蓝色)
- `WinmSecondaryButton`: 次要按钮样式 (灰色)
- `WinmSuccessButton`: 成功按钮样式 (绿色)
- `WinmWarningButton`: 警告按钮样式 (橙色)
- `WinmDangerButton`: 危险按钮样式 (红色)

#### 输入控件样式

- `WinmTextBox`: 统一的文本输入框样式
- `WinmPasswordBox`: 统一的密码输入框样式
- `WinmComboBox`: 统一的下拉框样式
- `WinmCheckBox`: 统一的复选框样式
- `WinmDatePicker`: 统一的日期选择器样式

#### 数据展示样式

- `WinmDataGrid`: 统一的数据网格样式

#### 工业SCADA专用样式

- `WinmDeviceStatusPanel`: 设备状态面板样式
- `WinmAlarmPanel`: 报警面板样式
- `WinmValueDisplay`: 数值显示样式
- `WinmUnitDisplay`: 单位显示样式

## 使用方法

### 1. 在 XAML 中引用命名空间

```xml
xmlns:hc="https://handyorg.github.io/handycontrol"
```

### 2. 使用 HandyControl 控件

```xml
<!-- 按钮示例 -->
<Button Content="主要操作" Style="{StaticResource WinmPrimaryButton}"/>
<Button Content="次要操作" Style="{StaticResource WinmSecondaryButton}"/>
<Button Content="成功操作" Style="{StaticResource WinmSuccessButton}"/>
<Button Content="警告操作" Style="{StaticResource WinmWarningButton}"/>
<Button Content="危险操作" Style="{StaticResource WinmDangerButton}"/>

<!-- 输入控件示例 -->
<hc:TextBox Text="输入文本" Style="{StaticResource WinmTextBox}"/>
<hc:PasswordBox Style="{StaticResource WinmPasswordBox}"/>
<hc:ComboBox Style="{StaticResource WinmComboBox}">
    <ComboBoxItem Content="选项1"/>
    <ComboBoxItem Content="选项2"/>
</hc:ComboBox>
<CheckBox Content="复选框" Style="{StaticResource WinmCheckBox}"/>
<hc:DatePicker Style="{StaticResource WinmDatePicker}"/>

<!-- 数据网格示例 -->
<hc:DataGrid Style="{StaticResource WinmDataGrid}" AutoGenerateColumns="False">
    <hc:DataGrid.Columns>
        <DataGridTextColumn Header="列1" Binding="{Binding Property1}"/>
        <DataGridTextColumn Header="列2" Binding="{Binding Property2}"/>
    </hc:DataGrid.Columns>
</hc:DataGrid>
```

### 3. 工业SCADA专用控件使用

```xml
<!-- 设备状态面板 -->
<Border Style="{StaticResource WinmDeviceStatusPanel}">
    <StackPanel>
        <TextBlock Text="设备名称" Style="{StaticResource DeviceNameStyle}"/>
        <TextBlock Text="100.5" Style="{StaticResource WinmValueDisplay}"/>
        <TextBlock Text="°C" Style="{StaticResource WinmUnitDisplay}"/>
    </StackPanel>
</Border>

<!-- 报警面板 -->
<Border Style="{StaticResource WinmAlarmPanel}">
    <TextBlock Text="报警信息" Foreground="White"/>
</Border>
```

## 向后兼容性

为了保持向后兼容性，我们提供了样式别名：

- `PrimaryButtonStyle` → `WinmPrimaryButton`
- `SecondaryButtonStyle` → `WinmSecondaryButton`
- `StandardTextBoxStyle` → `WinmTextBox`
- `StandardDataGridStyle` → `WinmDataGrid`

## 主题颜色

项目使用统一的颜色方案，定义在 `Colors.xaml` 中：

- `PrimaryBrush`: 主色调
- `SecondaryBrush`: 次要色调
- `SuccessBrush`: 成功状态色
- `WarningBrush`: 警告状态色
- `ErrorBrush`: 错误状态色
- `SurfaceBrush`: 表面背景色
- `BorderBrush`: 边框颜色

## 已更新的模块

1. **MainApp**: 主窗口和应用程序入口
2. **Module.Login**: 登录模块
3. **Module.Device**: 设备管理模块
4. **Module.Alarm**: 报警管理模块
5. **Module.History**: 历史数据模块
6. **Module.Threshold**: 阈值配置模块

## 注意事项

1. **命名空间**: 确保在使用 HandyControl 控件的 XAML 文件中添加正确的命名空间引用
2. **样式引用**: 使用预定义的样式以保持界面一致性
3. **颜色主题**: 遵循项目颜色规范，使用预定义的颜色资源
4. **响应式设计**: HandyControl 控件支持响应式设计，适配不同屏幕尺寸

## 扩展指南

### 添加新的自定义样式

1. 在 `HandyControlTheme.xaml` 中定义新样式
2. 基于 HandyControl 的基础样式进行扩展
3. 使用项目统一的颜色和字体规范
4. 在 `Generic_HandyControl.xaml` 中添加样式别名（如需要）

### 示例：创建自定义按钮样式

```xml
<Style x:Key="WinmCustomButton" TargetType="Button" BasedOn="{StaticResource ButtonDefault}">
    <Setter Property="Background" Value="{StaticResource CustomBrush}"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="Padding" Value="16,8"/>
    <Setter Property="CornerRadius" Value="4"/>
</Style>
```

## 技术支持

如有问题或需要技术支持，请参考：

- HandyControl 官方文档: https://handyorg.github.io/handycontrol/
- 项目内部文档和代码注释
- 联系项目维护团队

---

**版本**: v1.0  
**更新日期**: 2024年  
**维护者**: WinmTech 开发团队
