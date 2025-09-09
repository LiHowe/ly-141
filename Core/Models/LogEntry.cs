namespace Core.Models;

/// <summary>
/// 日志条目模型
/// </summary>
public class LogEntry
{
    /// <summary>
    /// 日志时间
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 日志等级
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// 日志消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 异常信息（可选）
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// 日志来源（线程信息）
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 线程ID
    /// </summary>
    public int ThreadId { get; set; }

    /// <summary>
    /// 日志等级显示文本
    /// </summary>
    public string LevelText => Level.ToString();

    /// <summary>
    /// 格式化的时间戳
    /// </summary>
    public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

    /// <summary>
    /// 日志等级对应的颜色
    /// </summary>
    public string LevelColor => Level switch
    {
        LogLevel.Debug => "#6C757D",
        LogLevel.Info => "#0D6EFD",
        LogLevel.Warning => "#FFC107",
        LogLevel.Error => "#DC3545",
        _ => "#6C757D"
    };

    /// <summary>
    /// 是否有异常信息
    /// </summary>
    public bool HasException => !string.IsNullOrEmpty(Exception);

    /// <summary>
    /// 完整的日志文本（用于搜索）
    /// </summary>
    public string FullText => $"{FormattedTimestamp} [{LevelText}] {Source}: {Message} {Exception}";
}

/// <summary>
/// 日志等级枚举
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// 调试信息
    /// </summary>
    Debug = 0,

    /// <summary>
    /// 一般信息
    /// </summary>
    Info = 1,

    /// <summary>
    /// 警告信息
    /// </summary>
    Warning = 2,

    /// <summary>
    /// 错误信息
    /// </summary>
    Error = 3
}

/// <summary>
/// 日志文件信息
/// </summary>
public class LogFileInfo
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件完整路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 格式化的文件大小
    /// </summary>
    public string FormattedFileSize
    {
        get
        {
            if (FileSize < 1024)
                return $"{FileSize} B";
            else if (FileSize < 1024 * 1024)
                return $"{FileSize / 1024.0:F1} KB";
            else if (FileSize < 1024 * 1024 * 1024)
                return $"{FileSize / (1024.0 * 1024.0):F1} MB";
            else
                return $"{FileSize / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }
    }

    /// <summary>
    /// 格式化的最后修改时间
    /// </summary>
    public string FormattedLastModified => LastModified.ToString("yyyy-MM-dd HH:mm:ss");
}