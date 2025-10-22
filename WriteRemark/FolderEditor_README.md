# 文件夹属性编辑器

这是一个基于现有 WriteRemark 项目的文件夹属性编辑器扩展，允许用户为文件夹设置提示信息和标签。

## 功能特性

- **图形化界面**：使用与文件属性编辑器相同的现代化 WPF 界面
- **字段管理**：支持显示/隐藏字段、拖拽排序、字段配置
- **备注写入**：使用 desktop.ini 文件为文件夹设置提示信息和标签
- **备注读取**：读取现有的文件夹属性信息
- **批量操作**：支持批量设置多个文件夹的属性

## 主要文件

### 核心文件
- `FolderPropertyEditorWindow.xaml` - 文件夹属性编辑窗口界面
- `FolderPropertyEditorWindow.xaml.cs` - 窗口逻辑代码
- `FolderRemarkManager.cs` - 文件夹备注读写管理器
- `FolderFieldConfigManager.cs` - 字段配置管理器

### 演示程序
- `FolderTestProgram.cs` - 独立测试程序
- `FolderDemo.cs` - 演示和工具类
- `FolderProgram.cs` - 在现有项目中的使用示例

## 使用方法

### 1. 基本使用

```csharp
// 打开文件夹属性编辑器
string folderPath = @"C:\MyFolder";
bool success = FolderPropertyEditor.ShowPropertyEditor(folderPath);

if (success)
{
    Console.WriteLine("文件夹属性已保存");
}
```

### 2. 读取文件夹信息

```csharp
// 读取文件夹属性
var folderInfo = FolderRemarkManager.ReadFolderRemark(folderPath);
Console.WriteLine($"提示信息: {folderInfo.InfoTip}");
Console.WriteLine($"标签: {folderInfo.Prop5}");
```

### 3. 直接写入文件夹属性

```csharp
// 直接写入文件夹属性（使用用户提供的方法）
string result = FolderRemarkManager.WriteFolderRemark(folderPath, "这是提示信息", "标签内容");
Console.WriteLine(result);
```

### 4. 批量操作

```csharp
// 批量设置多个文件夹的属性
string[] folders = { @"C:\Folder1", @"C:\Folder2", @"C:\Folder3" };
int successCount = FolderDemo.BatchSetFolderProperties(folders, "统一提示", "统一标签");
Console.WriteLine($"成功处理了 {successCount} 个文件夹");
```

## 技术实现

### 写入机制
使用用户提供的完整方法，通过以下步骤写入文件夹属性：
1. 创建包含 InfoTip 和 Prop5 的 desktop.ini 文件
2. 使用 COM 接口的 MoveHere 方法移动文件到目标文件夹
3. 设置 desktop.ini 为隐藏和系统文件
4. 设置文件夹为只读以启用 InfoTip 功能

### 读取机制
1. 检查文件夹中是否存在 desktop.ini 文件
2. 解析 INI 文件格式的内容
3. 提取 [.ShellClassInfo] 节中的 InfoTip
4. 提取 [{F29F85E0-4FF9-1068-AB91-08002B27B3D9}] 节中的 Prop5

### 界面特性
- 完全复制现有文件属性编辑器的界面逻辑
- 支持字段的显示/隐藏切换
- 支持拖拽排序和上下箭头排序
- 支持字段配置的持久化保存
- 现代化的按钮样式和悬停效果

## 字段配置

默认支持以下字段：
- **提示信息** (InfoTip)：鼠标悬停时显示的提示信息
- **标签** (Prop5)：文件夹标签信息

字段配置保存在 `folder_field_config.json` 文件中，支持：
- 字段显示/隐藏
- 字段排序
- 字段名称和提示信息自定义

## 运行测试

### 方法1：独立测试程序
```csharp
// 运行 FolderTestProgram
FolderTestProgram.Main(new string[] { @"C:\TestFolder" });
```

### 方法2：演示程序
```csharp
// 运行演示程序
FolderDemo.MainDemo(new string[] { @"C:\TestFolder" });
```

### 方法3：快速编辑
```csharp
// 快速编辑文件夹
bool success = FolderDemo.QuickEditFolder(@"C:\MyFolder");
```

## 注意事项

1. **权限要求**：需要对目标文件夹有写入权限
2. **文件系统**：仅支持 NTFS 文件系统
3. **系统兼容性**：需要 Windows 系统支持 desktop.ini 功能
4. **依赖项**：需要引用 WriteRemark 项目和相关 WPF 库

## 效果查看

设置完文件夹属性后，可以通过以下方式查看效果：
1. 在资源管理器中将鼠标悬停在文件夹上
2. 查看文件夹的详细信息面板
3. 使用支持扩展属性的文件管理器

## 扩展性

该实现完全基于现有的 WriteRemark 架构，可以轻松扩展：
- 添加更多字段类型
- 自定义字段验证逻辑
- 集成到现有的文件管理工具中
- 支持更多文件夹属性操作
