namespace MainApp.Initializers;

public interface IInitializer
{
	/// <summary>
	///     初始化任务名称
	/// </summary>
	static string Name { get; }

	/// <summary>
	///     初始化方法
	/// </summary>
	/// <returns></returns>
	static abstract Task Initialize();
}