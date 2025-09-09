using System.Windows;
using System.Windows.Threading;

namespace Core.Utils
{
    /// <summary>
    /// UI线程辅助工具类
    /// 提供线程安全的UI操作方法，避免WaitHelper导致的假死问题
    /// </summary>
    public static class UIThreadHelper
    {
        /// <summary>
        /// 在UI线程上异步执行操作
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <param name="priority">调度优先级</param>
        /// <returns>异步任务</returns>
        public static Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (Application.Current?.Dispatcher == null)
            {
                // 如果没有UI线程，直接执行
                action();
                return Task.CompletedTask;
            }

            if (Application.Current.Dispatcher.CheckAccess())
            {
                // 如果已经在UI线程上，直接执行
                action();
                return Task.CompletedTask;
            }

            // 在UI线程上异步执行
            return Application.Current.Dispatcher.InvokeAsync(action, priority).Task;
        }

        /// <summary>
        /// 在UI线程上异步执行操作并返回结果
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <param name="priority">调度优先级</param>
        /// <returns>异步任务</returns>
        public static Task<T> InvokeAsync<T>(Func<T> func, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (Application.Current?.Dispatcher == null)
            {
                // 如果没有UI线程，直接执行
                return Task.FromResult(func());
            }

            if (Application.Current.Dispatcher.CheckAccess())
            {
                // 如果已经在UI线程上，直接执行
                return Task.FromResult(func());
            }

            // 在UI线程上异步执行
            return Application.Current.Dispatcher.InvokeAsync(func, priority).Task;
        }

        /// <summary>
        /// 在UI线程上异步执行异步操作
        /// </summary>
        /// <param name="asyncAction">要执行的异步操作</param>
        /// <param name="priority">调度优先级</param>
        /// <returns>异步任务</returns>
        public static Task InvokeAsync(Func<Task> asyncAction, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (Application.Current?.Dispatcher == null)
            {
                // 如果没有UI线程，直接执行
                return asyncAction();
            }

            if (Application.Current.Dispatcher.CheckAccess())
            {
                // 如果已经在UI线程上，直接执行
                return asyncAction();
            }

            // 在UI线程上异步执行
            return Application.Current.Dispatcher.InvokeAsync(asyncAction, priority).Task.Unwrap();
        }

        /// <summary>
        /// 在UI线程上异步执行异步操作并返回结果
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="asyncFunc">要执行的异步函数</param>
        /// <param name="priority">调度优先级</param>
        /// <returns>异步任务</returns>
        public static Task<T> InvokeAsync<T>(Func<Task<T>> asyncFunc, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (Application.Current?.Dispatcher == null)
            {
                // 如果没有UI线程，直接执行
                return asyncFunc();
            }

            if (Application.Current.Dispatcher.CheckAccess())
            {
                // 如果已经在UI线程上，直接执行
                return asyncFunc();
            }

            // 在UI线程上异步执行
            return Application.Current.Dispatcher.InvokeAsync(asyncFunc, priority).Task.Unwrap();
        }

        /// <summary>
        /// 安全地在UI线程上执行操作，捕获并记录异常
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <param name="onError">错误处理回调</param>
        /// <param name="priority">调度优先级</param>
        /// <returns>异步任务</returns>
        public static async Task SafeInvokeAsync(Action action, Action<Exception>? onError = null, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            try
            {
                await InvokeAsync(action, priority);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                // 可以在这里添加日志记录
                System.Diagnostics.Debug.WriteLine($"UI线程操作异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查当前是否在UI线程上
        /// </summary>
        /// <returns>如果在UI线程上返回true，否则返回false</returns>
        public static bool IsOnUIThread()
        {
            return Application.Current?.Dispatcher?.CheckAccess() ?? false;
        }

        /// <summary>
        /// 确保操作在UI线程上执行（同步版本，谨慎使用）
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否成功执行</returns>
        [Obsolete("尽量避免使用同步版本，推荐使用InvokeAsync")]
        public static bool TryInvokeSync(Action action, TimeSpan? timeout = null)
        {
            if (Application.Current?.Dispatcher == null)
            {
                try
                {
                    action();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            if (Application.Current.Dispatcher.CheckAccess())
            {
                try
                {
                    action();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                var timeoutValue = timeout ?? TimeSpan.FromSeconds(5);
                var operation = Application.Current.Dispatcher.InvokeAsync(action);
                var status = operation.Wait(timeoutValue);
                return status == DispatcherOperationStatus.Completed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"同步UI调用失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 同步执行UI操作并返回结果（谨慎使用）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="defaultValue">超时或失败时的默认值</param>
        /// <returns>执行结果或默认值</returns>
        [Obsolete("尽量避免使用同步版本，推荐使用InvokeAsync")]
        public static T TryInvokeSync<T>(Func<T> func, TimeSpan? timeout = null, T defaultValue = default(T))
        {
            if (Application.Current?.Dispatcher == null)
            {
                try
                {
                    return func();
                }
                catch
                {
                    return defaultValue;
                }
            }

            if (Application.Current.Dispatcher.CheckAccess())
            {
                try
                {
                    return func();
                }
                catch
                {
                    return defaultValue;
                }
            }

            try
            {
                var timeoutValue = timeout ?? TimeSpan.FromSeconds(5);
                var operation = Application.Current.Dispatcher.InvokeAsync(func);
                var status = operation.Wait(timeoutValue);

                if (status == DispatcherOperationStatus.Completed)
                {
                    return operation.Result;
                }
                else
                {
                    return defaultValue;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"同步UI调用失败: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 延迟执行UI操作
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <param name="delay">延迟时间</param>
        /// <param name="priority">调度优先级</param>
        /// <returns>异步任务</returns>
        public static async Task DelayedInvokeAsync(Action action, TimeSpan delay, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            await Task.Delay(delay);
            await InvokeAsync(action, priority);
        }

        /// <summary>
        /// 批量执行UI操作
        /// </summary>
        /// <param name="actions">要执行的操作列表</param>
        /// <param name="priority">调度优先级</param>
        /// <returns>异步任务</returns>
        public static Task BatchInvokeAsync(IEnumerable<Action> actions, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            return InvokeAsync(() =>
            {
                foreach (var action in actions)
                {
                    action();
                }
            }, priority);
        }

        /// <summary>
        /// 使用低优先级执行UI操作，避免阻塞用户交互
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <returns>异步任务</returns>
        public static Task InvokeBackgroundAsync(Action action)
        {
            return InvokeAsync(action, DispatcherPriority.Background);
        }

        /// <summary>
        /// 使用空闲时间执行UI操作
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <returns>异步任务</returns>
        public static Task InvokeIdleAsync(Action action)
        {
            return InvokeAsync(action, DispatcherPriority.ApplicationIdle);
        }
    }
}
