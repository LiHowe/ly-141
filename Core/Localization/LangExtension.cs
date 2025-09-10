using System.Windows.Data;
using System.Windows.Markup;

namespace Core.Localization
{
	public class LangExtension : MarkupExtension
	{
		/// <summary>
		/// 资源 Key，位置参数（允许直接写 {loc:Lang Controls_Button}）。
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// 可指定 Provider（否则使用默认的 LocalizationProvider.Default）
		/// </summary>
		public LocalizationProvider Provider { get; set; }

		public LangExtension() { }
		public LangExtension(string key) { Key = key; }

		/// <summary>
		/// 全局默认 Provider（可在 App 启动时初始化）
		/// </summary>
		public static LocalizationProvider DefaultProvider { get; set; } = LocalizationProvider.Default;

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (string.IsNullOrEmpty(Key))
				return this; // 未指定 key 时返回自身（便于设计时）

			var provider = Provider ?? DefaultProvider;

			var binding = new Binding
			{
				Source = provider,
				Path = new System.Windows.PropertyPath($"[{Key}]")
			};

			return binding.ProvideValue(serviceProvider);
		}
	}
}
