using System.Collections.Concurrent;
using Core.Services;
using Logger;


namespace Core.Events;

/// <summary>
/// 提供一个集中的事件管理系统，用于跨窗口和跨线程的事件处理。
/// </summary>
public class EventCenter : IEventCenter
{
    private static readonly Lazy<EventCenter> instance = new Lazy<EventCenter>(() => new EventCenter());
    private bool isDisposed;

    /// <summary>
    /// 获取 EventCenter 的单例实例。
    /// </summary>
    public static EventCenter Instance => instance.Value;

    // 事件处理器字典，键为事件名称，值为包含订阅ID和处理器的字典
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Subscription>> eventHandlers
        = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, Subscription>>();

    /// <summary>
    /// 事件执行超时时间（毫秒）
    /// </summary>
    public int ExecutionTimeoutMs { get; set; } = 5000;

    private EventCenter() { }

    /// <summary>
    /// 表示事件订阅的结构
    /// </summary>
    private class Subscription
    {
        public Action<object> Handler { get; }
        public int Priority { get; }

        public Subscription(Action<object> handler, int priority)
        {
            Handler = handler;
            Priority = priority;
        }
    }

    /// <summary>
    /// 订阅指定名称的事件。
    /// </summary>
    /// <param name="eventName">要订阅的事件名称。</param>
    /// <param name="handler">当事件发布时要执行的操作。</param>
    /// <param name="priority">处理器优先级（数字越大优先级越高）。</param>
    /// <returns>订阅的唯一标识符。</returns>
    public Guid Subscribe(string eventName, Action<object> handler, int priority = 0)
    {
        if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        var subscriberId = Guid.NewGuid();
        var subscription = new Subscription(handler, priority);

        eventHandlers.AddOrUpdate(eventName,
            _ => new ConcurrentDictionary<Guid, Subscription>(),
            (_, dict) => dict);

        eventHandlers[eventName][subscriberId] = subscription;

        Log.Info($"已订阅事件: {eventName}, 订阅ID: {subscriberId}, 优先级: {priority}");

        return subscriberId;
    }

    /// <summary>
    /// 取消订阅指定的事件。
    /// </summary>
    /// <param name="eventName">要取消订阅的事件名称。</param>
    /// <param name="subscriberId">订阅的唯一标识符。</param>
    /// <returns>是否成功取消订阅。</returns>
    public bool Unsubscribe(string eventName, Guid subscriberId)
    {
        if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));

        if (eventHandlers.TryGetValue(eventName, out var subscribers))
        {
            if (subscribers.TryRemove(subscriberId, out _))
            {
                Log.Info($"已取消订阅事件: {eventName}, 订阅ID: {subscriberId}");
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// 移除指定事件的所有订阅者。
    /// </summary>
    /// <param name="eventName">要清除的事件名称。</param>
    /// <returns>是否成功移除事件。</returns>
    public bool ClearEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));

        var result = eventHandlers.TryRemove(eventName, out _);
        if (result)
        {
            Log.Info($"已清除事件: {eventName} 的所有订阅");
        }
        
        return result;
    }

    /// <summary>
    /// 发布指定名称的事件。
    /// </summary>
    /// <param name="eventName">要发布的事件名称。</param>
    /// <param name="eventArgs">要传递给事件处理器的参数。</param>
    /// <param name="sync">是否同步执行（默认为异步）。</param>
    public void Publish(string eventName, object eventArgs, bool sync = false)
    {
        if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));

        Log.Info($"正在发布事件: {eventName}");

        if (eventHandlers.TryGetValue(eventName, out var subscribers) && subscribers.Count > 0)
        {
            // 按优先级排序处理器
            var sortedHandlers = subscribers.Values
                .OrderByDescending(s => s.Priority)
                .Select(s => s.Handler)
                .ToList();

            if (sync)
            {
                foreach (var handler in sortedHandlers)
                {
                    try
                    {
                        handler(eventArgs);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"事件 {eventName} 处理器执行异常", ex);
                    }
                }
            }
            else
            {
                foreach (var handler in sortedHandlers)
                {
                    ExecuteHandlerAsync(handler, eventArgs, eventName);
                }
            }
        }
        else
        {
            Log.Warn($"没有找到事件 '{eventName}' 的订阅者");
        }
    }

    /// <summary>
    /// 异步发布指定名称的事件，并等待所有处理器执行完成。
    /// </summary>
    /// <param name="eventName">要发布的事件名称。</param>
    /// <param name="eventArgs">要传递给事件处理器的参数。</param>
    /// <returns>表示异步操作的任务。</returns>
    public async Task PublishAsync(string eventName, object eventArgs)
    {
        if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));

        Log.Info($"正在异步发布事件: {eventName}");

        if (eventHandlers.TryGetValue(eventName, out var subscribers) && subscribers.Count > 0)
        {
            // 按优先级排序处理器
            var sortedHandlers = subscribers.Values
                .OrderByDescending(s => s.Priority)
                .Select(s => s.Handler)
                .ToList();

            var tasks = sortedHandlers.Select(handler => 
                Task.Run(() => ExecuteHandlerWithTimeout(handler, eventArgs, eventName)))
                .ToArray();

            await Task.WhenAll(tasks);
        }
        else
        {
            Log.Warn($"没有找到事件 '{eventName}' 的订阅者");
        }
    }

    /// <summary>
    /// 使用超时机制执行事件处理器
    /// </summary>
    /// <param name="handler">事件处理器</param>
    /// <param name="eventArgs">事件参数</param>
    /// <param name="eventName">事件名称</param>
    /// <returns>表示异步操作的任务</returns>
    private async Task ExecuteHandlerWithTimeout(Action<object> handler, object eventArgs, string eventName)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(ExecutionTimeoutMs));
        
        try
        {
            await Task.Run(() => 
            {
                try
                {
                    handler(eventArgs);
                }
                catch (Exception ex)
                {
                    Log.Error($"事件 {eventName} 处理器执行异常",ex);
                }
            }, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Log.Warn($"事件 {eventName} 处理器执行超时 ({ExecutionTimeoutMs}ms)");
        }
        catch (Exception ex)
        {
            Log.Error($"事件 {eventName} 处理器执行异常", ex);
        }
        finally
        {
            cts.Dispose();
        }
    }

    /// <summary>
    /// 异步执行事件处理器
    /// </summary>
    private async void ExecuteHandlerAsync(Action<object> handler, object eventArgs, string eventName)
    {
        try
        {
            var task = Task.Run(() => handler(eventArgs));
            
            if (await Task.WhenAny(task, Task.Delay(ExecutionTimeoutMs)) != task)
            {
                Log.Warn($"事件 {eventName} 处理器执行超时 ({ExecutionTimeoutMs}ms)");
            }
            else if (task.IsFaulted && task.Exception != null)
            {
                Log.Error( $"事件 {eventName} 处理器执行异常",task.Exception.InnerException);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"事件 {eventName} 处理器执行异常",ex);
        }
    }

    /// <summary>
    /// 获取指定事件的订阅者数量
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>订阅者数量</returns>
    public int GetSubscriberCount(string eventName)
    {
        if (string.IsNullOrEmpty(eventName)) return 0;
        
        return eventHandlers.TryGetValue(eventName, out var subscribers) 
            ? subscribers.Count 
            : 0;
    }

    /// <summary>
    /// 判断事件是否有订阅者
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>是否有订阅者</returns>
    public bool HasSubscribers(string eventName)
    {
        return GetSubscriberCount(eventName) > 0;
    }

    /// <summary>
    /// 释放所有资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否正在执行Dispose</param>
    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed) return;

        if (disposing)
        {
            // 清空所有事件订阅
            eventHandlers.Clear();
            Log.Info("已清空所有事件订阅");
        }

        isDisposed = true;
    }
}