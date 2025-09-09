namespace Core.Events;

using System;

/// <summary>
/// 定义事件订阅者的接口，提供事件订阅、取消订阅和资源管理的功能。
/// </summary>
public interface IEventSubscriber : IDisposable
{
    /// <summary>
    /// 订阅指定名称的事件。
    /// </summary>
    /// <param name="eventName">要订阅的事件名称。</param>
    /// <param name="handler">当事件发布时要执行的操作。</param>
    /// <param name="multiple">是否允许同一事件名多次订阅，默认为 true。</param>
    /// <exception cref="ArgumentNullException">当 eventName 或 handler 为 null 时抛出。</exception>
    /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
    /// <exception cref="InvalidOperationException">当 multiple 为 false 且事件已订阅时抛出。</exception>
    void Subscribe(string eventName, Action<object> handler, bool multiple = true);

    /// <summary>
    /// 一次性订阅指定名称的事件，事件触发后自动取消订阅。
    /// </summary>
    /// <param name="eventName">要订阅的事件名称。</param>
    /// <param name="handler">当事件发布时要执行的操作。</param>
    /// <exception cref="ArgumentNullException">当 eventName 或 handler 为 null 时抛出。</exception>
    /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
    void Once(string eventName, Action<object> handler);

    /// <summary>
    /// 取消订阅指定名称的事件。
    /// </summary>
    /// <param name="eventName">要取消订阅的事件名称。</param>
    /// <exception cref="ArgumentNullException">当 eventName 为 null 时抛出。</exception>
    /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
    void Unsubscribe(string eventName);

    /// <summary>
    /// 清空所有事件订阅。
    /// </summary>
    /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
    void Clear();
}