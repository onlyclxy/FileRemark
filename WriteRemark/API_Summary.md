# WriteRemark API 使用总结

## 🎯 推荐使用的统一接口（最简单）

```csharp
using WriteRemark;

// ✅ 方式1: 单个路径（自动判断文件/文件夹）
string result = FilePropertyEditor.ShowEditor(@"C:\test\any-path");

// ✅ 方式2: 多个路径（自动选择单个或批量编辑器）
var paths = new List<string> { /* ... */ };
string result = FilePropertyEditor.ShowEditor(paths);
```

**特点：** 
- 🚀 一行代码搞定
- 🤖 自动判断路径类型
- 🎨 自动选择合适的UI
- ✅ 完善的错误处理

---

## 📚 完整API列表

### 智能接口（推荐）

| 方法 | 参数 | 说明 |
|------|------|------|
| `ShowEditor(string)` | 单个路径 | 自动判断文件/文件夹，打开对应编辑器 |
| `ShowEditor(List<string>)` | 路径列表 | 自动判断单个/批量，打开对应编辑器 |

### 专用接口（需要明确指定时使用）

| 方法 | 参数 | 说明 |
|------|------|------|
| `ShowFileEditor(string)` | 文件路径 | 打开单文件属性编辑器 |
| `ShowFolderEditor(string)` | 文件夹路径 | 打开单文件夹属性编辑器 |
| `ShowBatchEditor(List<string>)` | 路径列表 | 打开批量编辑器（文件+文件夹） |

### 旧版接口（已过时，不推荐）

| 方法 | 新版替代 |
|------|---------|
| `ShowPropertyEditor(string)` | `ShowEditor(string)` 或 `ShowFileEditor(string)` |
| `ShowPropertyEditor(List<string>)` | `ShowEditor(List<string>)` |
| `ShowBatchPropertyEditor(List<string>)` | `ShowBatchEditor(List<string>)` |

---

## 💡 典型使用场景

### 场景1: Shell扩展右键菜单

```csharp
public void OnContextMenuClick(string[] selectedItems)
{
    // 一行代码，无需判断
    FilePropertyEditor.ShowEditor(new List<string>(selectedItems));
}
```

### 场景2: 命令行工具

```csharp
static void Main(string[] args)
{
    if (args.Length == 0)
    {
        Console.WriteLine("请提供路径参数");
        return;
    }
    
    // 自动处理单个或多个参数
    var result = args.Length == 1 
        ? FilePropertyEditor.ShowEditor(args[0])
        : FilePropertyEditor.ShowEditor(new List<string>(args));
    
    Console.WriteLine(result);
}
```

### 场景3: GUI拖放操作

```csharp
private void Form_DragDrop(object sender, DragEventArgs e)
{
    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
    string result = FilePropertyEditor.ShowEditor(new List<string>(files));
    
    if (result == "Success")
        RefreshUI();
}
```

---

## ✨ 返回值说明

| 返回值 | 含义 | 建议操作 |
|--------|------|---------|
| `"Success"` | 保存成功 | 刷新UI或显示成功提示 |
| `"操作已取消"` | 用户取消 | 无需操作 |
| 其他字符串 | 错误信息 | 显示给用户 |

---

## 🎨 三种UI界面

### 1️⃣ 单文件编辑器
**触发条件：** 传入单个文件路径  
**显示内容：** 
- 文件完整路径
- 标题、主题、分级、标记、类别、备注

### 2️⃣ 单文件夹编辑器
**触发条件：** 传入单个文件夹路径  
**显示内容：**
- 文件夹完整路径
- 别名、备注、标题、主题、作者、标记

### 3️⃣ 批量编辑器
**触发条件：** 传入多个路径（文件+文件夹混合）  
**显示内容：**
- 操作目录（智能计算共同父目录）
- 文件和文件夹分组显示
- 批量操作工具栏
- 支持导入/导出配置

---

## 📖 详细文档

- **UnifiedEditor_README.md** - 统一接口完整使用指南
- **UnifiedEditorExample.cs** - 详细示例代码
- **QuickTest.cs** - 快速测试代码
- **README.md** - 项目总体说明
- **FolderEditor_README.md** - 文件夹编辑器专项说明

---

## 🔄 快速迁移指南

**如果你的代码使用旧API：**

```csharp
// 旧代码（会显示过时警告）
string result = FilePropertyEditor.ShowPropertyEditor(filePath);

// 新代码（推荐）
string result = FilePropertyEditor.ShowEditor(filePath);
```

**或者更明确：**

```csharp
// 如果确定是文件
string result = FilePropertyEditor.ShowFileEditor(filePath);

// 如果确定是文件夹
string result = FilePropertyEditor.ShowFolderEditor(folderPath);
```

---

## ⚡ 性能说明

- 路径验证使用 `File.Exists()` 和 `Directory.Exists()`
- 单个路径：O(1) 判断
- 批量路径：O(n) 遍历验证
- UI加载：延迟加载，仅在显示时初始化

---

## 🛡️ 错误处理

所有接口都进行了完善的错误处理：

✅ 空路径检查  
✅ 路径存在性验证  
✅ 异常捕获和友好提示  
✅ 无效路径自动过滤  
✅ 返回明确的错误信息  

---

## 🎯 设计原则

1. **简单优先** - 一行代码完成常见操作
2. **自动判断** - 减少使用者的决策负担
3. **向后兼容** - 旧API继续可用
4. **错误友好** - 清晰的错误提示
5. **类型安全** - 提供专用接口供类型明确场景使用

---

## 📞 联系支持

如有问题，请参考示例代码或查看详细文档。

