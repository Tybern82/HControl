using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HControl.Game;
using HControl.Game.Endurance;
using HControl.Game.Intensity;

namespace HControl;

public partial class IntensitySettingsControl : HCSettingsControl {

    public override IntensityGameParameters GameParameters { get; } = new IntensityGameParameters();
    public IntensitySettingsControl() {
        InitializeComponent();
        this.DataContext = GameParameters;
        updateChangeAfter();

        cmbMode.SelectionChanged += (sender, args) => updateChangeAfter();
    }

    private void updateChangeAfter() {
        switch (cmbMode.SelectedItem) {
            case IntensityGameParameters.GameMode.Fixed:
            case IntensityGameParameters.GameMode.Increasing:
            case IntensityGameParameters.GameMode.Decreasing:
            case IntensityGameParameters.GameMode.IncreasingInverted:
            case IntensityGameParameters.GameMode.DecreasingInverted:
                numChange.IsEnabled = false;
                break;

            default:
                numChange.IsEnabled = true;
                break;
        }
    }
}