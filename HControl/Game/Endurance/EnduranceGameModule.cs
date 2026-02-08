using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.tybern.CMDProcessor;
using HControl.Game.Timed;
using HControl.ViewModels;
using StateMachine;

namespace HControl.Game.Endurance {
    public class EnduranceGameModule : HCGameModule {

        protected static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        public static readonly string NAME = "Endurance";

        public string ModuleName => NAME;

        private readonly EnduranceSettingsControl _SettingsPage = new();
        public HCSettingsControl getSettingsPage(CommonGameParameters commonParams) {
            _SettingsPage.GameParameters.CommonParameters = commonParams;
            return _SettingsPage;
        }

        public void saveParameters() => _SettingsPage.GameParameters.saveParameters();

        public void startGame(HCGameParameters gameParameters, MainViewModel mainView) {
            if (gameParameters is EnduranceGameParameters parameters) {
                startGame(parameters, mainView);
            }
        }

        private static void startGame(EnduranceGameParameters gameParameters, MainViewModel gameView) => (new EnduranceGame(gameView, gameParameters)).runGame();
    }
}
