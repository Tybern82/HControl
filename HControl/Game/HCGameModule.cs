using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using com.tybern.CMDProcessor;
using HControl.Devices;
using HControl.ViewModels;
using HControl.Views;

namespace HControl.Game {
    public interface HCGameModule {

        public string ModuleName { get; }

        public void startGame(HCGameParameters gameParameters, MainViewModel mainView);

        public HCSettingsControl getSettingsPage(CommonGameParameters commonParams);
        public void saveParameters();
    }
}
