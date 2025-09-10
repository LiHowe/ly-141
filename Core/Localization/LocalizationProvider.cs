using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Core.Localization
{
	/// <summary>
	/// 可复用的本地化提供器（放到 Core），支持运行时切换并通知绑定（索引器 "Item[]"）。
	/// 依赖 CommunityToolkit.Mvvm 的 ObservableObject（方便使用 SetProperty/OnPropertyChanged）。
	/// </summary>
	public class LocalizationProvider : ObservableObject
	{
		private ResourceManager _rm;
		private CultureInfo _culture;
		private Type _langType;

		/// <summary>
		/// 设置为生成的 Lang 类型（resx 自动生成的强类型类）。
		/// 例如：typeof(MyApp.Core.Resources.Lang)
		/// </summary>
		public Type LangType
		{
			get => _langType;
			set
			{
				if (SetProperty(ref _langType, value))
				{
					if (_langType != null)
					{
						// 资源基名通常等于 Lang 类型的全名
						_rm = new ResourceManager(_langType.FullName, _langType.Assembly);
					}
					else
					{
						_rm = null;
					}
					// 索引器改变 -> 通知所有绑定到 [key] 的控件刷新
					OnPropertyChanged("Item[]");
				}
			}
		}

		/// <summary>
		/// 当前 UI Culture
		/// </summary>
		public CultureInfo Culture => _culture;

		/// <summary>
		/// 通过索引器获取字符串：UI 可用 {Binding Source={StaticResource Loc}, Path=[Controls_Button]}
		/// 约定 key 使用下划线分隔（例如 Controls_Button），内部会将 '_' 替换为 '.'（可按需改）
		/// </summary>
		public string this[string key]
		{
			get
			{
				if (_rm == null) return key ?? string.Empty;
				if (string.IsNullOrEmpty(key)) return string.Empty;
				var name = key.Replace('_', '.'); // 如果你在 resx 中使用 dot 分隔，也可以去掉这步
				try
				{
					var val = _culture == null ? _rm.GetString(name) : _rm.GetString(name, _culture);
					return val ?? key; // 找不到则返回 key（方便调试）
				}
				catch
				{
					return key;
				}
			}
		}

		/// <summary>
		/// 切换文化（使用文化名，例如 "fr-FR"）
		/// </summary>
		public void SetCulture(string cultureName) => SetCulture(new CultureInfo(cultureName));

		/// <summary>
		/// 切换文化（传入 CultureInfo）
		/// 会设置 DefaultThreadCurrentCulture & DefaultThreadCurrentUICulture，并通知 UI 刷新索引器。
		/// </summary>
		public void SetCulture(CultureInfo culture)
		{
			if (culture == null) throw new ArgumentNullException(nameof(culture));
			_culture = culture;

			// 设置线程默认文化，保证格式化（日期/数字）与资源文件都生效
			CultureInfo.DefaultThreadCurrentCulture = culture;
			CultureInfo.DefaultThreadCurrentUICulture = culture;

			// 通知所有绑定到索引器的控件刷新
			OnPropertyChanged("Item[]");
		}

		#region Default singleton （方便全局使用）
		private static LocalizationProvider _default;
		public static LocalizationProvider Default
		{
			get
			{
				if (_default == null) _default = new LocalizationProvider();
				return _default;
			}
			set => _default = value;
		}
		#endregion
	}
}
