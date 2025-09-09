
using System.Collections.Concurrent;
using Core.Services;
using Logger;

namespace Core.Events;

/// <summary>
/// 提供一种方便的方式来管理事件订阅，支持线程安全和高效的事件管理。
/// </summary>
public class EventSubscriber : IEventSubscriber
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<Guid>> subscriptions = new();
    private bool disposed;

    /// <summary>
    /// 初始化 EventSubscriber 的新实例。
    /// </summary>
    public EventSubscriber()
    {
    }

    /// <summary>
    /// 订阅指定名称的事件。
    /// </summary>
    /// <param name="eventName">要订阅的事件名称。</param>
    /// <param name="handler">当事件发布时要执行的操作。</param>
    /// <param name="multiple">是否允许同一事件名多次订阅，默认为 true。</param>
    /// <exception cref="ArgumentNullException">当 eventName 或 handler 为 null 时抛出。</exception>
    /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
    /// <exception cref="InvalidOperationException">当 multiple 为 false 且事件已订阅时抛出。</exception>
    public void Subscribe(string eventName, Action<object> handler, bool multiple = true)
    {
        ArgumentNullException.ThrowIfNull(eventName, nameof(eventName));
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        ObjectDisposedException.ThrowIf(disposed, nameof(EventSubscriber));

        var subscriberId = EventCenter.Instance.Subscribe(eventName, handler);
        
        subscriptions.AddOrUpdate(
            eventName,
            _ => 
            {
                var bag = new ConcurrentBag<Guid>();
                bag.Add(subscriberId);
                return bag;
            },
            (_, bag) =>
            {
                if (!multiple && !bag.IsEmpty)
                {
                    EventCenter.Instance.Unsubscribe(eventName, subscriberId);
                    throw new InvalidOperationException($"Event '{eventName}' is already subscribed and multiple subscriptions are not allowed.");
                }
                bag.Add(subscriberId);
                return bag;
            });

        Log.Debug($"EventSubscriber 已订阅事件: {eventName}");
    }

    /// <summary>
    /// 一次性订阅指定名称的事件，事件触发后自动取消订阅。
    /// </summary>
    /// <param name="eventName">要订阅的事件名称。</param>
    /// <param name="handler">当事件发布时要执行的操作。</param>
    /// <exception cref="ArgumentNullException">当 eventName 或 handler 为 null 时抛出。</exception>
    /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
    public void Once(string eventName, Action<object> handler)
    {
        ArgumentNullException.ThrowIfNull(eventName, nameof(eventName));
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        ObjectDisposedException.ThrowIf(disposed, nameof(EventSubscriber));

        Guid subscriberId = default;
        Action<object> wrappedHandler = null!; // 非空断言，因为在订阅前会赋值

        wrappedHandler = args =>
        {
            try
            {
                handler(args);
            }
            finally
            {
                EventCenter.Instance.Unsubscribe(eventName, subscriberId);
                if (subscriptions.TryGetValue(eventName, out var bag))
                {
                    bag.TryTake(out _); // 移除订阅ID
                    if (bag.IsEmpty)
                    {
                        subscriptions.TryRemove(eventName, out _);
                    }
                }
            }
        };

        subscriberId = EventCenter.Instance.Subscribe(eventName, wrappedHandler);
        
        subscriptions.AddOrUpdate(
            eventName,
            _ => 
            {
                var bag = new ConcurrentBag<Guid>();
                bag.Add(subscriberId);
                return bag;
            },
            (_, bag) =>
            {
                bag.Add(subscriberId);
                return bag;
            });

        Log.Debug($"EventSubscriber 已一次性订阅事件: {eventName}");
    }

    /// <summary>
    /// 取消订阅指定名称的事件。
    /// </summary>
    /// <param name="eventName">要取消订阅的事件名称。</param>
    /// <exception cref="ArgumentNullException">当 eventName 为 null 时抛出。</exception>
    /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
    public void Unsubscribe(string eventName)
    {
        ArgumentNullException.ThrowIfNull(eventName, nameof(eventName));
        ObjectDisposedException.ThrowIf(disposed, nameof(EventSubscriber));

        if (subscriptions.TryRemove(eventName, out var bag))
        {
            foreach (var subscriberId in bag)
            {
                EventCenter.Instance.Unsubscribe(eventName, subscriberId);
            }
            Log.Debug($"EventSubscriber 已取消订阅事件: {eventName}");
        }
    }

    /// <summary>
    /// 清空所有事件订阅。
    /// </summary>
    /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(disposed, nameof(EventSubscriber));

        foreach (var (eventName, bag) in subscriptions)
        {
            foreach (var subscriberId in bag)
            {
                EventCenter.Instance.Unsubscribe(eventName, subscriberId);
            }
        }
        subscriptions.Clear();
        Log.Debug("EventSubscriber 已清空所有订阅");
    }

    /// <summary>
    /// 释放所有订阅并清理资源。
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源。
    /// </summary>
    /// <param name="disposing">指示是否正在执行 Dispose。</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            Clear(); // 复用 Clear 方法，避免重复代码
        }

        disposed = true;
    }
}