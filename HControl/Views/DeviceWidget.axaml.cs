using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using HControl.Devices;

namespace HControl;

public partial class DeviceWidget : UserControl
{

    public DeviceManager Devices { get; } = new DeviceManager();

    public DeviceWidget()
    {
        InitializeComponent();
        btnStop.Click += (sender, args) => {
            Devices.stop();
            lblIntensity.Text = "Off";
        };
        btn10.Click += (sender, args) => setIntensity(10);
        btn20.Click += (sender, args) => setIntensity(20);
        btn30.Click += (sender, args) => setIntensity(30);
        btn40.Click += (sender, args) => setIntensity(40);
        btn50.Click += (sender, args) => setIntensity(50);
        btn60.Click += (sender, args) => setIntensity(60);
        btn70.Click += (sender, args) => setIntensity(70);
        btn80.Click += (sender, args) => setIntensity(80);
        btn90.Click += (sender, args) => setIntensity(90);
        btn100.Click += (sender, args) => setIntensity(100);

        this.Unloaded += (sender, args) => Devices.disconnect();

        Devices.IntensityChanged += (sender, args) => { 
            Dispatcher.UIThread.Invoke(() => {
                lblIntensity.Text = (args.Intensity == 0) ? "Off" : args.Intensity + " %";
                }); 
        };
    }

    private void setIntensity(uint intensity) {
        Devices.setIntensity(intensity);
        lblIntensity.Text = intensity + " %";
    }

    public void doPause(bool isPaused) {
        btn10.IsEnabled = !isPaused;
        btn20.IsEnabled = !isPaused;
        btn30.IsEnabled = !isPaused;
        btn40.IsEnabled = !isPaused;
        btn50.IsEnabled = !isPaused;
        btn60.IsEnabled = !isPaused;
        btn70.IsEnabled = !isPaused;
        btn80.IsEnabled = !isPaused;
        btn90.IsEnabled = !isPaused;
        btn100.IsEnabled = !isPaused;
    }
}