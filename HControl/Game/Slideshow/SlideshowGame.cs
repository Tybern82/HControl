using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HControl.ViewModels;

namespace HControl.Game.Slideshow {
    public class SlideshowGame : ActiveGame {

        public SlideshowGame(MainViewModel gameView, CommonGameParameters gameParameters) : base(gameView, gameParameters) {
            gameView.setDuration(TimeSpan.Zero);
        }

        protected override string getLogText() => "Starting Slideshow...";

        protected override GameRound loadFirstRound() {
            GameRound currRound = GameRound.newRound(true, "Stroke", 0, 0);
            ActiveGameView.FinishText = "";
            return currRound;
        }

        protected override void doRoundUpdate() => doRoundUpdate(GameStartTime, false);


        protected override void doPause() { }
        protected override void doResume() { }
    }
}
