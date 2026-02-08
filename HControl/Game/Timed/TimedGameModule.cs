using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.tybern.CMDProcessor;
using HControl.Devices;
using HControl.ViewModels;
using HControl.Views;

namespace HControl.Game.Timed {
    public class TimedGameModule : HCGameModule {

        protected static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();
        public static readonly string NAME = "Timed";

        public string ModuleName => NAME;

        private readonly TimedSettingsControl _SettingsPage = new();
        public HCSettingsControl getSettingsPage(CommonGameParameters commonParams) {
            _SettingsPage.GameParameters.CommonParameters = commonParams;
            return _SettingsPage;
        }

        public void saveParameters() => _SettingsPage.GameParameters.saveParameters();

        public void startGame(HCGameParameters gameParameters, MainViewModel mainView) {
            if (gameParameters is TimedGameParameters parameters) {
                startGame(parameters, mainView);
            }
        }

        private static void startGame(TimedGameParameters gameParameters, MainViewModel gameView) => (new TimedGame(gameView, gameParameters)).runGame();
    }
}
