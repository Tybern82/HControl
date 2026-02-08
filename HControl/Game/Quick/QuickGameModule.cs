using System;
using System.Threading;
using com.tybern.CMDProcessor;
using HControl.ViewModels;

namespace HControl.Game.Quick {
    public class QuickGameModule : HCGameModule {

        protected static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();
        public static readonly string NAME = "Quick";

        public string ModuleName { get; } = NAME;


        private readonly QuickSettingsControl _SettingsPage = new();
        public HCSettingsControl getSettingsPage(CommonGameParameters commonParams) {
            _SettingsPage.GameParameters.CommonParameters = commonParams;
            return _SettingsPage;
        }

        public void saveParameters() => _SettingsPage.GameParameters.saveParameters();

        public void startGame(HCGameParameters gameParameters, MainViewModel gameView) {
            if (gameParameters is QuickGameParameters p) {
                (new QuickGame(gameView, p)).runGame();
            }
        }
    }
}
