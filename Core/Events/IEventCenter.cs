namespace Core.Events;

/// <summary>
/// 定义事件中心的接口，提供事件订阅、发布和管理的功能。
/// </summary>
public interface IEventCenter : IDisposable
{
    /// <summary>
    /// 事件执行超时时间（毫秒）
    /// </summary>
    int ExecutionTimeoutMs { get; set; }

    /// <summary>
    /// 订阅指定名称的事件。
    /// </summary>
    /// <param name="eventName">要订阅的事件名称。</param>
    /// <param name="handler">当事件发布时要执行的操作。</param>
    /// <param name="priority">处理器优先级（数字越大优先级越高）。</param>
    /// <returns>订阅的唯一标识符。</returns>
    Guid Subscribe(string eventName, Action<object> handler, int priority = 0);

    /// <summary>
    /// 取消订阅指定的事件。
    /// </summary>
    /// <param name="eventName">要取消订阅的事件名称。</param>
    /// <param name="subscriberId">订阅的唯一标识符。</param>
    /// <returns>是否成功取消订阅。</returns>
    bool Unsubscribe(string eventName, Guid subscriberId);

    /// <summary>
    /// 移除指定事件的所有订阅者。
    /// </summary>
    /// <param name="eventName">要清除的事件名称。</param>
    /// <returns>是否成功移除事件。</returns>
    bool ClearEvent(string eventName);

    /// <summary>
    /// 发布指定名称的事件。
    /// </summary>
    /// <param name="eventName">要发布的事件名称。</param>
    /// <param name="eventArgs">要传递给事件处理器的参数。</param>
    /// <param name="sync">是否同步执行（默认为异步）。</param>
    void Publish(string eventName, object eventArgs, bool sync = false);

    /// <summary>
    /// 异步发布指定名称的事件，并等待所有处理器执行完成。
    /// </summary>
    /// <param name="eventName">要发布的事件名称。</param>
    /// <param name="eventArgs">要传递给事件处理器的参数。</param>
    /// <returns>表示异步操作的任务。</returns>
    Task PublishAsync(string eventName, object eventArgs);

    /// <summary>
    /// 获取指定事件的订阅者数量。
    /// </summary>
    /// <param name="eventName">事件名称。</param>
    /// <returns>订阅者数量。</returns>
    int GetSubscriberCount(string eventName);

    /// <summary>
    /// 判断事件是否有订阅者。
    /// </summary>
    /// <param name="eventName">事件名称。</param>
    /// <returns>是否有订阅者。</returns>
    bool HasSubscribers(string eventName);
}