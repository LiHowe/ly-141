using System.Windows;
using System.Windows.Controls;

namespace MainApp.Menu;

public interface IViewsManager : IDisposable
{
	/// <summary>
	///     切换到指定类型的视图
	/// </summary>
	/// <typeparam name="T">要切换到的视图类型</typeparam>
	/// <param name="instanceId">视图实例标识符，用于区分同类型的多个视图</param>
	/// <param name="useCache">是否使用缓存</param>
	/// <param name="viewInitializer">可选的视图初始化函数</param>
	/// <returns>切换后的视图实例</returns>
	T ChangeView<T>(string? instanceId = null, bool useCache = true, Action<T>? viewInitializer = null)
        where T : UserControl, new();

	/// <summary>
	///     初始化指定类型的视图，并缓存它
	/// </summary>
	/// <typeparam name="T">视图类型</typeparam>
	/// <param name="instanceId">视图实例标识符</param>
	/// <param name="useCache">是否缓存此视图</param>
	void InitView<T>(string? instanceId = null, bool useCache = true) where T : UserControl, new();

	/// <summary>
	///     初始化视图并缓存它，用于提前准备视图但不立即显示
	/// </summary>
	/// <param name="view">要初始化的视图</param>
	/// <param name="instanceId">视图实例标识符</param>
	/// <param name="useCache">是否缓存此视图</param>
	void InitView(UserControl view, string? instanceId = null, bool useCache = true);

	/// <summary>
	///     获取指定类型和实例ID的视图（如果存在）
	/// </summary>
	/// <typeparam name="T">视图类型</typeparam>
	/// <param name="instanceId">视图实例标识符</param>
	/// <returns>存在则返回视图实例，否则返回 null</returns>
	T? GetView<T>(string? instanceId = null) where T : UserControl;

	/// <summary>
	///     分离视图为独立窗口进行显示
	/// </summary>
	/// <typeparam name="T">视图类型</typeparam>
	/// <param name="instanceId">视图实例标识符</param>
	/// <param name="title">窗口标题</param>
	/// <param name="size">窗口大小</param>
	/// <param name="removeFromCache">是否从缓存中移除</param>
	/// <returns>窗口对象，如果视图不存在返回 null</returns>
	Window? DetachView<T>(
        string? instanceId = null,
        string? title = null,
        Size? size = null,
        bool removeFromCache = false) where T : UserControl;

	/// <summary>
	///     移除特定类型和实例ID的视图
	/// </summary>
	/// <typeparam name="T">视图类型</typeparam>
	/// <param name="instanceId">视图实例标识符</param>
	/// <returns>是否成功移除</returns>
	bool RemoveView<T>(string? instanceId = null) where T : UserControl;

	/// <summary>
	///     关闭并移除指定类型的所有视图
	/// </summary>
	/// <typeparam name="T">视图类型</typeparam>
	/// <returns>移除的数量</returns>
	int CloseAllViewsOfType<T>() where T : UserControl;

	/// <summary>
	///     清除缓存中所有视图（当前显示的除外）
	/// </summary>
	void ClearCache();
}