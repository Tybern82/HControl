using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HControl.Game;
using HControl.Game.Quick;

namespace HControl;

public partial class QuickSettingsControl : HCSettingsControl {

    public override QuickGameParameters GameParameters { get; } = new QuickGameParameters();

    public QuickSettingsControl() {
        InitializeComponent();
        this.DataContext = GameParameters;
    }
}