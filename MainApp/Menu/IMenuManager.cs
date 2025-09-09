using System.Windows;
using System.Windows.Controls;

namespace MainApp.Menu;

public interface IMenuManager
{
	/// <summary>
	///     添加一个菜单项并返回对应的按钮控件
	/// </summary>
	/// <typeparam name="T">目标窗体类型，必须继承 Form 且有无参构造函数</typeparam>
	/// <param name="config">菜单配置</param>
	/// <returns>创建的按钮控件</returns>
	Button AddMenu(IMenuConfig menuConfig);

	/// <summary>
	///     根据菜单名称移除菜单项
	/// </summary>
	/// <param name="text"></param>
	void RemoveMenu(string text);

	/// <summary>
	///     应用菜单配置
	/// </summary>
	void Apply(Window mainView, Panel menuPanel, ViewsManager viewManager);
}