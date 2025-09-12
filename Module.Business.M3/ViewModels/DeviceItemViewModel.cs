using CommunityToolkit.Mvvm.ComponentModel;
using Core.Localization;

namespace Module.Business.SG141.ViewModels
{
	public enum DeviceStatus
	{
		Running,
		Standby,
		Fault
	}

	public partial class DeviceItemViewModel : ObservableObject
	{
		[ObservableProperty]
		private string _deviceName = string.Empty;

		[ObservableProperty]
		private string _statusText = string.Empty;

		[ObservableProperty]
		private DeviceStatus _deviceStatus = DeviceStatus.Standby;
	

		private string GetLabel (DeviceStatus status)
		{
			return status switch
			{
				DeviceStatus.Running => LocalizationProvider.Default["DeviceStatus_Running"],
				DeviceStatus.Standby => LocalizationProvider.Default["DeviceStatus_Standby"],
				DeviceStatus.Fault => LocalizationProvider.Default["DeviceStatus_Fault"],
				_ => LocalizationProvider.Default["DeviceStatus_Unknown"],
			};
		}

		public DeviceItemViewModel()
		{
			LocalizationProvider.Default.PropertyChanged += (s, e) =>
			{
				StatusText = GetLabel(DeviceStatus);
			};
		}

		// Example method to update status
		public void UpdateStatus(DeviceStatus newStatus, DeviceStatus status)
		{
			DeviceStatus = newStatus;
			DeviceStatus = status;
		}
		partial void OnDeviceStatusChanged(DeviceStatus oldValue, DeviceStatus newValue)
		{
			StatusText = GetLabel(DeviceStatus);
		}
	}
}
