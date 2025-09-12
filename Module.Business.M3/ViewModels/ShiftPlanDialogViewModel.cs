using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.Business.SG141.ViewModels
{
	public partial class ShiftPlanDialogViewModel: ObservableObject
	{
		[ObservableProperty]
		private string _title;
		[ObservableProperty]
		private string _shiftName;
		[ObservableProperty]
		private TimeSpan _startTime;
		[ObservableProperty]
		private TimeSpan _endTime;
		[ObservableProperty]
		private bool _isNightShift;

		/// <summary>
		/// 班次名称集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<string> _shiftNames;

		public ShiftPlanDialogViewModel()
		{
			
		}
	}
}
