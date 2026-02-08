using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.tybern.CMDProcessor;
using HControl.ViewModels;

namespace HControl.Game.Slideshow {
    public class SlideshowGameModule : HCGameModule {

        protected static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        public static readonly string NAME = "Slideshow";

        public string ModuleName => NAME;


        private readonly EmptySettingsControl _SettingsPage = new();
        public HCSettingsControl getSettingsPage(CommonGameParameters commonParams) {
            _SettingsPage.GameParameters.CommonParameters = commonParams;
            return _SettingsPage;
        }

        public void saveParameters() => _SettingsPage.GameParameters.saveParameters();

        public void startGame(HCGameParameters gameParameters, MainViewModel gameView) => (new SlideshowGame(gameView, gameParameters.CommonParameters)).runGame();
    }
}
