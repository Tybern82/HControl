using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HControl.Game;

namespace HControl;

public partial class EmptySettingsControl : HCSettingsControl {

    public override BasicGameParameters GameParameters { get; } = new BasicGameParameters();
    public EmptySettingsControl() {
        InitializeComponent();
        this.DataContext = GameParameters;
    }
}