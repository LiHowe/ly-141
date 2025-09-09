using System.IO;
using System.Text.RegularExpressions;
using Core.Models;

namespace Core.Services
{
    /// <summary>
    /// 日志读取服务
    /// </summary>
    public class LogReaderService
    {
        private readonly string _logDirectory;
        private readonly Regex _logLinePattern;

        /// <summary>
        /// 构造函数
        /// </summary>
        public LogReaderService()
        {
            // 使用Constants中定义的日志目录
            _logDirectory = Constants.LogRootPath;

            // 日志行匹配模式：匹配Serilog输出格式
            // 格式：[2025-07-30 14:30:45.123] [DEBUG] 消息内容
            // 支持更灵活的等级格式，如 [DEBUG], [INFO ], [WARN ], [ERROR], [FATAL]
            _logLinePattern = new Regex(
                @"^(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{4})\|([A-Z]+)\|([^|]+)\|(.*)$",
                RegexOptions.Compiled);
        }

        /// <summary>
        /// 获取日志目录路径
        /// </summary>
        public string LogDirectory => _logDirectory;

        /// <summary>
        /// 检查日志目录是否存在
        /// </summary>
        public bool LogDirectoryExists => Directory.Exists(_logDirectory);

        /// <summary>
        /// 获取所有日志文件（支持新旧两种格式）
        /// </summary>
        /// <returns>日志文件列表</returns>
        public List<FileInfo> GetLogFiles()
        {
            if (!LogDirectoryExists)
                return new List<FileInfo>();

            var allFiles = new List<FileInfo>();
            var directory = new DirectoryInfo(_logDirectory);

            // 1. 搜索新格式：按日期命名的.log文件（格式：app-yyyyMMdd.log）
            var newFormatFiles = directory.GetFiles("app-*.log", SearchOption.TopDirectoryOnly);
            allFiles.AddRange(newFormatFiles);

            // 2. 搜索旧格式：日期子目录中的.txt文件（向后兼容）
            foreach (var subDir in directory.GetDirectories())
            {
                if (DateTime.TryParseExact(subDir.Name, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var _))
                {
                    var oldFormatFiles = subDir.GetFiles("*.txt", SearchOption.TopDirectoryOnly);
                    allFiles.AddRange(oldFormatFiles);
                }
            }

            return allFiles.OrderByDescending(f => f.LastWriteTime).ToList();
        }

        /// <summary>
        /// 获取指定日期的日志文件（支持新旧两种格式）
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>该日期的日志文件列表</returns>
        public List<FileInfo> GetLogFilesByDate(DateTime date)
        {
            if (!LogDirectoryExists)
                return new List<FileInfo>();

            var files = new List<FileInfo>();

            // 1. 检查新格式文件（app-yyyyMMdd.log）
            var newFileName = $"app-{date:yyyyMMdd}.log";
            var newFilePath = Path.Combine(_logDirectory, newFileName);
            if (File.Exists(newFilePath))
            {
                files.Add(new FileInfo(newFilePath));
            }

            // 2. 检查旧格式文件（yyyy-MM-dd目录下的.txt文件）
            var oldDateFolder = Path.Combine(_logDirectory, date.ToString("yyyy-MM-dd"));
            if (Directory.Exists(oldDateFolder))
            {
                var directory = new DirectoryInfo(oldDateFolder);
                var oldFiles = directory.GetFiles("*.txt", SearchOption.TopDirectoryOnly);
                files.AddRange(oldFiles);
            }

            return files.OrderBy(f => f.Name).ToList();
        }

        /// <summary>
        /// 获取可用的日期列表（支持新旧两种格式）
        /// </summary>
        /// <returns>有日志文件的日期列表</returns>
        public List<DateTime> GetAvailableDates()
        {
            if (!LogDirectoryExists)
                return new List<DateTime>();

            var dates = new HashSet<DateTime>();
            var directory = new DirectoryInfo(_logDirectory);

            // 1. 查找新格式日志文件（格式：app-yyyyMMdd.log）
            foreach (var file in directory.GetFiles("app-*.log"))
            {
                var fileName = Path.GetFileNameWithoutExtension(file.Name);
                var datePart = fileName.Replace("app-", "");

                if (DateTime.TryParseExact(datePart, "yyyyMMdd", null,
                    System.Globalization.DateTimeStyles.None, out var date))
                {
                    dates.Add(date);
                }
            }

            // 2. 查找旧格式日志文件（日期子目录）
            foreach (var subDir in directory.GetDirectories())
            {
                if (DateTime.TryParseExact(subDir.Name, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var date))
                {
                    dates.Add(date);
                }
            }

            return dates.OrderByDescending(d => d).ToList();
        }

        /// <summary>
        /// 读取所有日志条目
        /// </summary>
        /// <param name="maxEntries">最大条目数（0表示不限制）</param>
        /// <returns>日志条目列表</returns>
        public async Task<List<LogEntry>> ReadAllLogsAsync(int maxEntries = 0)
        {
            var allEntries = new List<LogEntry>();
            var logFiles = GetLogFiles();

            foreach (var file in logFiles)
            {
                try
                {
                    var entries = await ReadLogFileAsync(file.FullName);
                    allEntries.AddRange(entries);

                    // 如果指定了最大条目数且已达到，则停止读取
                    if (maxEntries > 0 && allEntries.Count >= maxEntries)
                        break;
                }
                catch (Exception ex)
                {
                    // 记录读取文件失败的错误，但继续处理其他文件
                    System.Diagnostics.Debug.WriteLine($"读取日志文件失败: {file.Name}, 错误: {ex.Message}");
                }
            }

            // 按时间倒序排列（最新的在最上面）
            var sortedEntries = allEntries.OrderByDescending(e => e.Timestamp).ToList();

            // 如果指定了最大条目数，则截取
            if (maxEntries > 0 && sortedEntries.Count > maxEntries)
            {
                sortedEntries = sortedEntries.Take(maxEntries).ToList();
            }

            return sortedEntries;
        }

        /// <summary>
        /// 读取指定日志文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>日志条目列表</returns>
        public async Task<List<LogEntry>> ReadLogFileAsync(string filePath)
        {
            var entries = new List<LogEntry>();

            if (!File.Exists(filePath))
                return entries;

            try
            {
                // 使用 FileStream 以支持文件共享读取
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream);

                var content = await reader.ReadToEndAsync();
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                LogEntry? currentEntry = null;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var match = _logLinePattern.Match(line);
                    if (match.Success)
                    {
                        // 如果有当前条目，先添加到列表
                        if (currentEntry != null)
                        {
                            entries.Add(currentEntry);
                        }

                        // 创建新的日志条目
                        // 格式：[时间戳] [等级] 消息
                        if (DateTime.TryParse(match.Groups[1].Value, out var timestamp))
                        {
                            currentEntry = new LogEntry
                            {
                                Timestamp = timestamp,
                                Level = ParseLogLevel(match.Groups[2].Value.Trim()),
                                Message = match.Groups[3].Value,
                                Source = Path.GetFileNameWithoutExtension(filePath),
                                ThreadId = 0 // Serilog格式中没有线程信息，设为0
                            };
                        }
                    }
                    else if (currentEntry != null)
                    {
                        // 这是异常信息或多行消息的续行
                        var trimmedLine = line.Trim();

                        // 检查是否是异常堆栈跟踪
                        if (trimmedLine.StartsWith("System.") ||
                            trimmedLine.StartsWith("   at ") ||
                            trimmedLine.StartsWith("   在 ") ||
                            !string.IsNullOrEmpty(currentEntry.Exception))
                        {
                            // 这是异常信息
                            if (string.IsNullOrEmpty(currentEntry.Exception))
                            {
                                currentEntry.Exception = trimmedLine;
                            }
                            else
                            {
                                currentEntry.Exception += Environment.NewLine + trimmedLine;
                            }
                        }
                        else
                        {
                            // 这可能是多行消息的续行
                            currentEntry.Message += Environment.NewLine + trimmedLine;
                        }
                    }
                }

                // 添加最后一个条目
                if (currentEntry != null)
                {
                    entries.Add(currentEntry);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取日志文件失败: {filePath}, 错误: {ex.Message}");
                // 可以考虑抛出异常或记录到日志系统
            }

            return entries;
        }

        /// <summary>
        /// 解析日志等级
        /// </summary>
        /// <param name="levelText">等级文本</param>
        /// <returns>日志等级</returns>
        private LogLevel ParseLogLevel(string levelText)
        {
            return levelText.ToUpper().Trim() switch
            {
                "DEBUG" or "DBG" => LogLevel.Debug,
                "INFORMATION" or "INFO" or "INF" => LogLevel.Info,
                "WARNING" or "WARN" or "WRN" => LogLevel.Warning,
                "ERROR" or "ERR" => LogLevel.Error,
                "FATAL" or "FTL" => LogLevel.Error, // 将Fatal映射到Error
                _ => LogLevel.Info
            };
        }

        /// <summary>
        /// 从线程信息中提取线程ID
        /// </summary>
        /// <param name="threadInfo">线程信息字符串，如"Thread-1"</param>
        /// <returns>线程ID</returns>
        private int ExtractThreadId(string threadInfo)
        {
            try
            {
                // 尝试从"Thread-1"格式中提取数字
                var match = Regex.Match(threadInfo, @"Thread-(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int threadId))
                {
                    return threadId;
                }

                // 如果无法解析，返回哈希码作为ID
                return threadInfo.GetHashCode();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 过滤日志条目
        /// </summary>
        /// <param name="entries">原始条目列表</param>
        /// <param name="level">日志等级过滤</param>
        /// <param name="searchText">搜索文本</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>过滤后的条目列表</returns>
        public List<LogEntry> FilterLogs(List<LogEntry> entries, LogLevel? level = null, 
            string? searchText = null, DateTime? startTime = null, DateTime? endTime = null)
        {
            var filtered = entries.AsEnumerable();

            // 按等级过滤
            if (level.HasValue)
            {
                filtered = filtered.Where(e => e.Level == level.Value);
            }

            // 按搜索文本过滤
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(e => e.FullText.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            // 按时间范围过滤
            if (startTime.HasValue)
            {
                filtered = filtered.Where(e => e.Timestamp >= startTime.Value);
            }

            if (endTime.HasValue)
            {
                filtered = filtered.Where(e => e.Timestamp <= endTime.Value);
            }

            return filtered.ToList();
        }

        /// <summary>
        /// 打开日志文件夹
        /// </summary>
        public void OpenLogDirectory()
        {
            try
            {
                if (LogDirectoryExists)
                {
                    System.Diagnostics.Process.Start("explorer.exe", _logDirectory);
                }
                else
                {
                    // 如果目录不存在，创建它
                    Directory.CreateDirectory(_logDirectory);
                    System.Diagnostics.Process.Start("explorer.exe", _logDirectory);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"打开日志目录失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除指定的日志文件
        /// </summary>
        /// <param name="filePath">文件完整路径</param>
        /// <returns>是否删除成功</returns>
        public bool DeleteLogFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除日志文件失败: {filePath}, 错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清空指定的日志文件内容
        /// </summary>
        /// <param name="filePath">文件完整路径</param>
        /// <returns>是否清空成功</returns>
        public bool ClearLogFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.WriteAllText(filePath, string.Empty);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清空日志文件失败: {filePath}, 错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除指定日期的日志文件
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否删除成功</returns>
        public bool DeleteLogsByDate(DateTime date)
        {
            try
            {
                var fileName = $"app-{date:yyyyMMdd}.log";
                var filePath = Path.Combine(_logDirectory, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除日期日志失败: {date:yyyy-MM-dd}, 错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取日志文件信息
        /// </summary>
        /// <param name="filePath">文件完整路径</param>
        /// <returns>文件信息</returns>
        public LogFileInfo? GetLogFileInfo(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    return new LogFileInfo
                    {
                        FileName = fileInfo.Name,
                        FilePath = filePath,
                        FileSize = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime,
                        CreatedTime = fileInfo.CreationTime
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取日志文件信息失败: {filePath}, 错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取今天的日志文件路径
        /// </summary>
        /// <returns>今天的日志文件路径</returns>
        public string GetTodayLogFilePath()
        {
            var fileName = $"app-{DateTime.Now:yyyyMMdd}.log";
            return Path.Combine(_logDirectory, fileName);
        }

        /// <summary>
        /// 检查今天是否有日志文件
        /// </summary>
        /// <returns>是否存在今天的日志文件</returns>
        public bool HasTodayLogFile()
        {
            return File.Exists(GetTodayLogFilePath());
        }

        /// <summary>
        /// 获取日志文件的行数（估算）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>估算的行数</returns>
        public async Task<int> GetLogFileLineCountAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return 0;

            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream);

                int lineCount = 0;
                while (await reader.ReadLineAsync() != null)
                {
                    lineCount++;
                }

                return lineCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取日志文件行数失败: {filePath}, 错误: {ex.Message}");
                return 0;
            }
        }
    }
}
