using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HControl.Game;
using HControl.Game.Endurance;

namespace HControl;

public partial class EnduranceSettingsControl : HCSettingsControl {

    public override EnduranceGameParameters GameParameters { get; } = new EnduranceGameParameters();

    public EnduranceSettingsControl() {
        InitializeComponent();
        this.DataContext = GameParameters;

        btnGentle.Click += (sender, args) => {
            GameParameters.Type = EnduranceGameParameters.GameMode.Gentle;
            GameParameters.BaseTime = TimeSpan.FromMinutes(30);
            GameParameters.MinRounds = 0;
            GameParameters.MaxRounds = 3;
            GameParameters.CommonParameters.IsIntense = false;
        };

        btnNormal.Click += (sender, args) => {
            GameParameters.Type = EnduranceGameParameters.GameMode.Normal;
            GameParameters.BaseTime = TimeSpan.FromMinutes(30);
            GameParameters.MinRounds = 0;
            GameParameters.MaxRounds = 8;
            GameParameters.CommonParameters.IsIntense = false;
        };

        btnEvil.Click += (sender, args) => {
            GameParameters.Type = EnduranceGameParameters.GameMode.Evil;
            GameParameters.BaseTime = TimeSpan.FromMinutes(60);
            GameParameters.MinRounds = 0;
            GameParameters.MaxRounds = 15;
            GameParameters.CommonParameters.IsIntense = true;
        };
    }
}