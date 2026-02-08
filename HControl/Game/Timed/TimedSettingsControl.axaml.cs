using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HControl.Game;
using HControl.Game.Timed;

namespace HControl;

public partial class TimedSettingsControl : HCSettingsControl {

    public override TimedGameParameters GameParameters { get; } = new TimedGameParameters();

    public TimedSettingsControl() {
        InitializeComponent();
        this.DataContext = GameParameters;
    }
}