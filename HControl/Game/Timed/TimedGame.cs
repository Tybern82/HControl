using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HControl.ViewModels;

namespace HControl.Game.Timed {
    public class TimedGame : ActiveGame {

        private int FinishCounter { get; set; } = 0;

        private TimedGameParameters TimedParameters { get; }

        public TimedGame(MainViewModel gameView, TimedGameParameters gameParameters) : base(gameView, gameParameters.CommonParameters) {
            TimedParameters = gameParameters;
            LOG.Info(getLogText());

            gameView.setDuration(gameParameters.Duration);
        }

        protected override string getLogText() => "Starting Timed Game: " + GameStartTime.ToString(@"HH\:mm\:ss") + " for " + formatTime(TimedParameters.Duration) + " - ETA: " + (GameStartTime + TimedParameters.Duration).ToString(@"HH\:mm\:ss");

        protected override GameRound loadFirstRound() {
            GameRound currRound = RoundCatalog.getFirstRound();

            RoundStartTime = GameStartTime;

            return currRound;
        }

        protected override void doRoundUpdate() {
            switch (CurrentStage) {
                case GameStage.Main:
                    doGameRound();
                    break;

                case GameStage.Finish:
                    doFinishRound();
                    break;

                default:
                    doEndedRound();
                    break;
            }
        }
        protected override void doPause() { }
        protected override void doResume() { }

        private void doGameRound() {
            bool isFinish = (GameStartTime + TimedParameters.Duration) <= DateTime.Now;
            if ((RoundStartTime + CurrentRound.RoundDuration) < DateTime.Now) {
                if (isFinish) {
                    CurrentStage = GameStage.Finish;
                } else {
                    // End of Round
                    LOG.Debug("Next Round Starting...");
                    CurrentRound = (CurrentRound.IsActiveRound) ? RoundCatalog.getPauseRound() : RoundCatalog.getActiveRound(CommonParameters.IsIntense, false);
                    RoundStartTime = DateTime.Now;
                    resetLastChangeTime();
                }
            }
            ActiveGameView.FinishText = formatTime((GameStartTime + TimedParameters.Duration) - DateTime.Now);
            ActiveGameView.setProgress((GameStartTime + TimedParameters.Duration) - DateTime.Now);
            doRoundUpdate(RoundStartTime, false);
        }

        private void doFinishRound() {
            if ((RoundStartTime + CurrentRound.RoundDuration) < DateTime.Now) {
                FinishCounter++;
                if ((FinishCounter > 1) && !CommonParameters.LoopFinish) {
                    CurrentStage = GameStage.Ended;
                } else {
                    // End of Round
                    LOG.Debug("Finish Round Starting...");
                    CurrentRound = RoundCatalog.getFinishRound(CommonParameters.IsIntense);
                    RoundStartTime = DateTime.Now;
                    resetLastChangeTime();
                }
            }
            ActiveGameView.FinishText = formatTime((GameStartTime + TimedParameters.Duration) - DateTime.Now);
            ActiveGameView.setProgress((GameStartTime + TimedParameters.Duration) - DateTime.Now);
            doRoundUpdate(RoundStartTime, FinishCounter > 0);
        }
    }
}
