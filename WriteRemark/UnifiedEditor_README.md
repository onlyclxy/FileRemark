# 统一编辑器接口使用指南

## 概述

`FilePropertyEditor.ShowEditor()` 是一个智能的统一接口，可以自动判断传入的路径类型并打开相应的编辑器。这是**推荐使用**的接口，极大简化了调用逻辑。

## 功能特点

✅ **自动识别路径类型** - 无需手动判断是文件还是文件夹  
✅ **智能选择编辑器** - 单个路径用单文件/文件夹编辑器，多个路径用批量编辑器  
✅ **完善的错误处理** - 空路径、不存在的路径都有友好的提示  
✅ **灵活的输入方式** - 支持单个string或List<string>  
✅ **向后兼容** - 旧版API仍然保留，标记为过时但仍可使用

## 快速开始

### 1. 最简单的使用方式（推荐）

```csharp
using WriteRemark;

// 传入任何路径，自动处理
string result = FilePropertyEditor.ShowEditor(@"C:\test\myfile.txt");
// 或
string result = FilePropertyEditor.ShowEditor(@"C:\test\myfolder");
```

### 2. 批量处理

```csharp
var paths = new List<string>
{
    @"C:\test\file1.txt",
    @"C:\test\file2.txt",
    @"C:\test\folder1"
};

string result = FilePropertyEditor.ShowEditor(paths);
```

## 完整API说明

### 主要接口（推荐使用）

#### `ShowEditor(string path)` 
**单路径智能接口** - 自动判断文件还是文件夹

```csharp
string result = FilePropertyEditor.ShowEditor(path);
```

**行为规则：**
- 如果是文件 → 打开文件属性编辑器
- 如果是文件夹 → 打开文件夹属性编辑器
- 如果路径为空 → 返回 "请提供有效的文件或文件夹路径"
- 如果路径不存在 → 返回 "路径不存在：{path}"

#### `ShowEditor(List<string> paths)`
**多路径智能接口** - 自动判断单个还是批量

```csharp
string result = FilePropertyEditor.ShowEditor(paths);
```

**行为规则：**
- 如果列表为空或null → 返回 "请提供有效的文件或文件夹路径"
- 如果只有1个文件 → 打开文件属性编辑器
- 如果只有1个文件夹 → 打开文件夹属性编辑器
- 如果有多个文件/文件夹 → 打开批量编辑器
- 如果所有路径都无效 → 返回 "没有找到有效的文件或文件夹"

### 专用接口（需要明确指定类型时使用）

#### `ShowFileEditor(string filePath)`
**文件属性编辑器** - 只用于单个文件

```csharp
string result = FilePropertyEditor.ShowFileEditor(@"C:\test\file.txt");
```

#### `ShowFolderEditor(string folderPath)`
**文件夹属性编辑器** - 只用于单个文件夹

```csharp
string result = FilePropertyEditor.ShowFolderEditor(@"C:\test\folder");
```

#### `ShowBatchEditor(List<string> paths)`
**批量编辑器** - 用于多个文件和文件夹

```csharp
var paths = new List<string> { /* ... */ };
string result = FilePropertyEditor.ShowBatchEditor(paths);
```

## 返回值说明

所有方法都返回 `string` 类型的结果信息：

| 返回值 | 含义 |
|--------|------|
| `"Success"` | 用户点击保存并成功保存 |
| `"操作已取消"` | 用户点击取消或关闭窗口 |
| `"请提供有效的文件或文件夹路径"` | 输入参数为空或无效 |
| `"路径不存在：{path}"` | 指定的路径不存在 |
| `"没有找到有效的文件或文件夹"` | 列表中所有路径都无效 |
| `"打开XXX编辑器时出错: {错误信息}"` | 打开编辑器时发生异常 |

## 使用场景示例

### 场景1: Shell扩展集成

```csharp
public void OnFileContextMenuClick(string[] selectedPaths)
{
    // 无需判断，直接调用
    var result = FilePropertyEditor.ShowEditor(new List<string>(selectedPaths));
    
    if (result == "Success")
    {
        MessageBox.Show("属性已保存");
    }
}
```

### 场景2: 命令行工具

```csharp
public static void Main(string[] args)
{
    if (args.Length == 0)
    {
        Console.WriteLine("用法: editor.exe <文件或文件夹路径>");
        return;
    }

    // 单个参数，使用字符串重载
    if (args.Length == 1)
    {
        string result = FilePropertyEditor.ShowEditor(args[0]);
        Console.WriteLine(result);
    }
    // 多个参数，使用列表重载
    else
    {
        string result = FilePropertyEditor.ShowEditor(new List<string>(args));
        Console.WriteLine(result);
    }
}
```

### 场景3: GUI应用程序

```csharp
private void btnOpenEditor_Click(object sender, EventArgs e)
{
    // 从OpenFileDialog获取路径
    if (openFileDialog.ShowDialog() == DialogResult.OK)
    {
        string result = FilePropertyEditor.ShowEditor(openFileDialog.FileName);
        
        if (result == "Success")
        {
            RefreshFileList();
        }
        else if (result != "操作已取消")
        {
            MessageBox.Show(result, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
```

### 场景4: 拖放操作

```csharp
private void Form_DragDrop(object sender, DragEventArgs e)
{
    if (e.Data.GetDataPresent(DataFormats.FileDrop))
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        string result = FilePropertyEditor.ShowEditor(new List<string>(files));
        
        statusLabel.Text = result;
    }
}
```

## 完整示例代码

详细示例请参考 `UnifiedEditorExample.cs` 文件，包含：
- ✅ 单个文件处理
- ✅ 单个文件夹处理
- ✅ 批量处理
- ✅ 错误处理
- ✅ 命令行参数处理
- ✅ 所有边界情况

## 旧版API迁移指南

如果你的代码使用旧版API，建议迁移到新API：

| 旧API | 新API | 说明 |
|-------|-------|------|
| `ShowPropertyEditor(string)` | `ShowEditor(string)` 或 `ShowFileEditor(string)` | 旧API标记为过时 |
| `ShowPropertyEditor(List<string>)` | `ShowEditor(List<string>)` | 旧API标记为过时 |
| `ShowBatchPropertyEditor(List<string>)` | `ShowBatchEditor(List<string>)` | 旧API标记为过时 |

**注意：** 旧API仍然可以使用，但编译器会显示过时警告。

## 特性对比

| 特性 | ShowEditor | 专用接口 |
|------|-----------|---------|
| 自动识别路径类型 | ✅ | ❌ |
| 自动选择编辑器 | ✅ | ❌ |
| 单个路径支持 | ✅ | ✅ |
| 批量路径支持 | ✅ | ✅ |
| 错误提示 | ✅ | ✅ |
| 代码简洁 | ✅ | ❌ |
| 类型安全 | ⚠️ 运行时判断 | ✅ |

## 最佳实践

1. **优先使用 `ShowEditor()`** - 除非你需要强制指定编辑器类型
2. **检查返回值** - 判断用户是否成功保存或取消操作
3. **处理错误** - 非 "Success" 和 "操作已取消" 的返回值都应该向用户显示
4. **批量操作** - 超过1个路径时，自动使用批量编辑器以提高效率

## UI界面说明

### 单文件编辑器
- 显示文件完整路径
- 支持编辑：标题、主题、分级、标记、类别、备注
- 可自定义字段显示/隐藏和顺序

### 单文件夹编辑器  
- 显示文件夹完整路径
- 支持编辑：别名、备注、标题、主题、作者、标记
- 可自定义字段显示/隐藏和顺序

### 批量编辑器
- 显示操作目录（自动计算共同父目录）
- 同时编辑多个文件和文件夹
- 支持批量赋值和清空操作
- 可选择性保存
- 支持导入/导出配置（TOML格式）

## 技术支持

如有问题，请查看：
- `README.md` - 项目总体说明
- `FolderEditor_README.md` - 文件夹编辑器专项说明
- `UnifiedEditorExample.cs` - 详细使用示例

