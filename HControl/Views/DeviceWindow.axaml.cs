using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HControl.Devices;

namespace HControl;

public partial class DeviceWindow : Window {

    public DeviceManager Devices { get { return wdgDevice.Devices; } }

    public DeviceWindow() {
        InitializeComponent();

        SettingsStore.Instance.loadWindow(this);

        this.Closing += (sender, args) => SettingsStore.Instance.saveWindow(this);
    }
}