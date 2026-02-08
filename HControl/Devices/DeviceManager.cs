using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HControl.Devices {
    public class DeviceManager : Device {

        public static readonly DeviceManager Instance = new();

        private HashSet<Device> ActiveDevices = [];

        public void registerDevice(Device d) {
            ActiveDevices.Add(d);
            d.IntensityChanged += intensityEvent;
        }

        public void unregisterDevice(Device d) {
            ActiveDevices.Remove(d);
            d.IntensityChanged -= intensityEvent;
        }

        private void intensityEvent(object? sender, IntensityEvent e) {
            OnIntensityChanged(e.Intensity);
        }

        public DeviceManager() {
            // TODO: Register based on active Settings
            registerDevice(HandyControl.Instance);
        }

        public override bool connect(string? key) {
            bool _result = true;
            foreach (var device in ActiveDevices) _result &= device.connect(key);
            isConnected = _result;
            return _result;
        }

        public override bool disconnect() {
            bool _result = true;
            foreach (var device in ActiveDevices) _result &= device.disconnect();
            isConnected = !_result;
            return _result;
        }

        public override void start() {
            foreach (var device in ActiveDevices) device.start();
        }
        public override void stop() {
            foreach (var device in ActiveDevices) device.stop();
            OnIntensityChanged(0);
        }
        public override void setIntensity(uint intensity) {
            foreach (var device in ActiveDevices) {
                if (!device.isConnected) device.connect(null);
                device.setIntensity(intensity);
                device.start();
            }
            OnIntensityChanged(intensity);
        }
    }
}
