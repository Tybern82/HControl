using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.tybern.CMDProcessor;
using HControl.Game.Endurance;
using HControl.ViewModels;
using static HControl.Game.Intensity.IntensityGameParameters;

namespace HControl.Game.Intensity {
    internal class IntensityGameModule : HCGameModule {

        protected static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        public static readonly string NAME = "Intensity";

        public string ModuleName => NAME;

        private readonly IntensitySettingsControl _SettingsPage = new();
        public HCSettingsControl getSettingsPage(CommonGameParameters commonParams) {
            _SettingsPage.GameParameters.CommonParameters = commonParams;
            return _SettingsPage;
        }

        public void saveParameters() => _SettingsPage.GameParameters.saveParameters();

        public void startGame(HCGameParameters gameParameters, MainViewModel mainView) {
            if (gameParameters is IntensityGameParameters parameters) {
                startGame(parameters, mainView);
            }
        }

        public static void startGame(IntensityGameParameters gameParameters, MainViewModel gameView) => (new IntensityGame(gameView, gameParameters)).runGame();
    }
}
