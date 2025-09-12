using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Module.Business.SG141.ViewModels
{
	public partial class DeviceListViewModel: ObservableObject
	{
		[ObservableProperty]
		private ObservableCollection<DeviceItemViewModel> _devices = new();


		public DeviceListViewModel()
		{
			Devices.Add(new DeviceItemViewModel() { DeviceName = "设备1", DeviceStatus=DeviceStatus.Standby});
			Devices.Add(new DeviceItemViewModel() { DeviceName = "设备2", DeviceStatus=DeviceStatus.Running});
			Devices.Add(new DeviceItemViewModel() { DeviceName = "设备3", DeviceStatus=DeviceStatus.Fault});
		}
	}
}
