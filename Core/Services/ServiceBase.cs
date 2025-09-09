using Logger;

namespace Core.Services
{
    /// <summary>
    /// 服务基类
    /// 为所有服务提供通用的日志记录功能
    /// </summary>
    public abstract class ServiceBase
    {

        protected ServiceBase()
        {
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        protected void OnInfo(string message)
        {
            Log.Info(message);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        protected void OnWarning(string message)
        {
            Log.Warn(message);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息</param>
        protected void OnError(string message, Exception? exception = null)
        {
            Log.Error(message, exception);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        protected void OnDebug(string message)
        {
            Log.Debug(message);
        }

        /// <summary>
        /// 记录致命错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息</param>
        protected void OnFatal(string message, Exception? exception = null)
        {
            Log.Fatal(message, exception);
        }
    }
}
