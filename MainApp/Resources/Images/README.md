# 图标资源目录

此目录用于存放菜单图标文件。

## 支持的格式

- PNG（推荐，支持透明背景）
- JPG

## 图标规格建议

- 尺寸：16x16 或 24x24 像素
- 文件大小：小于 10KB
- 背景：透明背景（PNG格式）

## 使用方法

在菜单配置中，将IconType设置为"Image"，Icon字段设置为图片文件名即可：

```json
{
  "Id": "device-management",
  "Name": "设备管理", 
  "Icon": "device.png",
  "IconType": "Image"
}
```

## 示例图标

目前包含以下示例图标文件（占位符）：

- device.png - 设备图标
- alarm.png - 报警图标
- settings.png - 设置图标

实际使用时请替换为真实的图片文件。
