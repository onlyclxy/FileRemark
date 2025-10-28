using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WriteRemark
{
    /// <summary>
    /// 动画辅助类 - 提供各种窗口动画效果
    /// </summary>
    public static class AnimationHelper
    {
        /// <summary>
        /// 从上到下滚动渐变消失动画
        /// 用于掩盖保存操作的延迟
        /// </summary>
        /// <param name="window">目标窗口</param>
        /// <param name="duration">动画时长（秒）</param>
        /// <returns>动画完成的Task</returns>
        public static Task AnimateSlideDownFadeOut(Window window, double duration = 0.5)
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                // 创建 Storyboard
                var storyboard = new Storyboard();

                // 1. 向下移动动画（使用 TranslateTransform）
                var translateTransform = new TranslateTransform();
                window.RenderTransform = translateTransform;

                var slideAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 100, // 向下移动100像素
                    Duration = new Duration(TimeSpan.FromSeconds(duration)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };

                Storyboard.SetTarget(slideAnimation, window);
                Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("RenderTransform.Y"));
                storyboard.Children.Add(slideAnimation);

                // 2. 渐变透明动画
                var fadeAnimation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromSeconds(duration)),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                Storyboard.SetTarget(fadeAnimation, window);
                Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
                storyboard.Children.Add(fadeAnimation);

                // 动画完成事件
                storyboard.Completed += (s, e) =>
                {
                    tcs.SetResult(true);
                };

                // 启动动画
                storyboard.Begin();
            }
            catch (Exception ex)
            {
                // 如果动画失败，立即返回
                tcs.SetException(ex);
            }

            return tcs.Task;
        }

        /// <summary>
        /// 从下到上滑入渐显动画
        /// 用于窗口打开时的效果
        /// </summary>
        /// <param name="window">目标窗口</param>
        /// <param name="duration">动画时长（秒）</param>
        /// <returns>动画完成的Task</returns>
        public static Task AnimateSlideUpFadeIn(Window window, double duration = 0.3)
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                // 创建 Storyboard
                var storyboard = new Storyboard();

                // 1. 向上移动动画
                var translateTransform = new TranslateTransform(0, 50);
                window.RenderTransform = translateTransform;
                window.Opacity = 0;

                var slideAnimation = new DoubleAnimation
                {
                    From = 50,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(duration)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(slideAnimation, window);
                Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("RenderTransform.Y"));
                storyboard.Children.Add(slideAnimation);

                // 2. 渐显动画
                var fadeAnimation = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromSeconds(duration)),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(fadeAnimation, window);
                Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
                storyboard.Children.Add(fadeAnimation);

                // 动画完成事件
                storyboard.Completed += (s, e) =>
                {
                    tcs.SetResult(true);
                };

                // 启动动画
                storyboard.Begin();
            }
            catch (Exception ex)
            {
                // 如果动画失败，立即返回
                tcs.SetException(ex);
            }

            return tcs.Task;
        }

        /// <summary>
        /// 快速淡出动画
        /// </summary>
        /// <param name="element">目标元素</param>
        /// <param name="duration">动画时长（秒）</param>
        /// <returns>动画完成的Task</returns>
        public static Task FadeOut(FrameworkElement element, double duration = 0.3)
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                var animation = new DoubleAnimation
                {
                    From = element.Opacity,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromSeconds(duration)),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                animation.Completed += (s, e) =>
                {
                    tcs.SetResult(true);
                };

                element.BeginAnimation(UIElement.OpacityProperty, animation);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }
    }
}

