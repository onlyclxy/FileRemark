using System;

namespace WriteRemark
{
    /// <summary>
    /// 应用程序版本信息
    /// </summary>
    public static class AppVersion
    {
        /// <summary>
        /// 当前版本号
        /// 格式: 主版本.次版本.修订号
        /// </summary>
        public const string Version = "1.0.7";

        /// <summary>
        /// 版本发布日期
        /// </summary>
        public static readonly DateTime ReleaseDate = new DateTime(2025, 10, 29);

        /// <summary>
        /// 版本更新日志
        /// </summary>
        public const string ChangeLog = @"
v1.0.7 (2025-10-29)
- [修复] 修复保存操作阻塞 UI 的问题: 整个保存过程真正异步化
- [优化] 使用 Task.Run + Dispatcher.BeginInvoke 实现非阻塞保存
- [性能] 动画和保存完全并行, UI 完全流畅

v1.0.6 (2025-10-29)
- [体验] 保存时添加从上到下滚动渐变消失动画, 掩盖延迟
- [新增] AnimationHelper 动画辅助类
- [优化] 成功保存后直接关闭窗口, 无需等待消息框

v1.0.5 (2025-10-29)
- [简化] 移除显示更多功能, 固定显示20条历史记录
- [优化] 简化代码逻辑, 提升稳定性

v1.0.4 (2025-10-29)
- [修复] 修复显示更多历史记录功能: 点击后不再清空输入框
- [优化] 使用 Dispatcher 确保 UI 正确更新

v1.0.3 (2025-10-29)
- [性能] 历史记录保存改为异步, 彻底解决 Tab 切换卡顿
- [新增] 添加版本号管理系统

v1.0.2 (2025-10-29)
- [性能] DataGrid 所有文本列改为带历史记录的 ComboBox
- [性能] 实现超轻量级加载: QuickLoadFromCache + AttachMinimalEvents
- [性能] 移除 Visual Tree 遍历和实时数据库查询
- [优化] Tab 切换响应时间从 500ms+ 降至 < 50ms

v1.0.1 (2025-10-29)
- [修复] 修复历史记录功能未正常工作的问题
- [修复] 修复 SQLite.Interop.dll 缺失导致的崩溃
- [新增] 添加历史记录诊断工具
- [优化] 批量编辑窗口支持历史记录

v1.0.0 (2025-10-28)
- [新增] 初始版本发布
- [功能] 文件属性批量编辑
- [功能] 文件夹属性批量编辑
- [功能] 历史记录功能(基础版)
";

        /// <summary>
        /// 获取完整版本信息
        /// </summary>
        public static string GetFullVersionInfo()
        {
            return $"写入文件备注 v{Version} ({ReleaseDate:yyyy-MM-dd})";
        }
    }
}

