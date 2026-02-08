using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HControl.ViewModels;

namespace HControl.Game.Quick {
    public class QuickGame(MainViewModel gameView, QuickGameParameters quickGameParameters) : ActiveGame(gameView, quickGameParameters.CommonParameters) {

        private ushort TotalRounds { get; } = (ushort)(2 * quickGameParameters.RoundCount);  // count active and pause rounds

        private ushort CurrentRoundIndex { get; set; } = 0;

        private int FinishCounter { get; set; } = 0;

        protected override string getLogText() => "Quick Game: " + TotalRounds + " rounds";

        protected override GameRound loadFirstRound() {
            GameRound currRound = RoundCatalog.getFirstRound();
            LOG.Info("Round: " + currRound.RoundDuration + " - " + currRound.RoundContent);
            return currRound;
        }

        protected override void doRoundUpdate() {
            switch (CurrentStage) {
                case GameStage.Main: 
                    doMainRound();
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

        private void doMainRound() {
            bool isFinish = (CurrentRoundIndex >= TotalRounds);
            ActiveGameView.FinishText = isFinish ? "--" : "[" + (CurrentRoundIndex) + " / " + TotalRounds + "]";
            if ((RoundStartTime + CurrentRound.RoundDuration) < DateTime.Now) {
                LOG.Info("Next round starting... [" + (CurrentRoundIndex + 1) + " / " + TotalRounds + "]");
                CurrentRoundIndex++;
                CurrentRound = CurrentRound.IsActiveRound ? RoundCatalog.getPauseRound() : RoundCatalog.getActiveRound(CommonParameters.IsIntense, false);
                LOG.Info("Round: " + CurrentRound.RoundDuration + " - " + CurrentRound.RoundContent);
                RoundStartTime = DateTime.Now;
                resetLastChangeTime();
            }
            doRoundUpdate(RoundStartTime, false);
            if (isFinish) CurrentStage = GameStage.Finish;
        }

        private void doFinishRound() {
            if ((RoundStartTime + CurrentRound.RoundDuration) < DateTime.Now) {
                FinishCounter++;
                if ((FinishCounter > 1) && !CommonParameters.LoopFinish) {
                    CurrentStage = GameStage.Ended;
                } else {
                    LOG.Info("Start Finish Round...");
                    CurrentRound = RoundCatalog.getFinishRound(CommonParameters.IsIntense).setBeatRate(5.0, 5.0).setHandyRate(100, 100);
                    RoundStartTime = DateTime.Now;
                    resetLastChangeTime();
                }
            }
            doRoundUpdate(RoundStartTime, FinishCounter > 0);
        }
    }
}
