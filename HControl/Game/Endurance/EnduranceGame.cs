using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HControl.ViewModels;
using StateMachine;

namespace HControl.Game.Endurance {
    public class EnduranceGame : ActiveGame {

        private static readonly State FirstRound = State.getState("endurance:first");
        private static readonly State BaseActive = State.getState("endurance:base:active");
        private static readonly State BasePause = State.getState("endurance:base:pause");
        private static readonly State RoundBreak = State.getState("endurance:round:break");
        private static readonly State RoundActive = State.getState("endurance:round:active");
        private static readonly State RoundPause = State.getState("endurance:round:pause");
        private static readonly State FinishRound = State.getState("endurance:finish");
        private static readonly State GameEnded = State.getState("endurance:ended");

        private StateManager GameStates { get; }

        private ushort TotalRounds { get; }
        private ushort CurrentRoundIndex { get; set; }
        private TimeSpan TotalTime { get; set; }

        private DateTime StageStartTime { get; set; }
        private TimeSpan StageDuration { get; set; }

        private int FinishCounter { get; set; } = 0;

        private EnduranceGameParameters EnduranceParameters { get; }

        public EnduranceGame(MainViewModel gameView, EnduranceGameParameters gameParameters) : base(gameView, gameParameters.CommonParameters) {
            EnduranceParameters = gameParameters;

            LOG.Debug("Building Endurance StateMachine...");

            // Add logging
            FirstRound.enterState += (prev, args) => LOG.Debug("Entering First Round...");
            BasePause.enterState += (prev, args) => LOG.Debug("Entering Pause Round (base)...");
            BaseActive.enterState += (prev, args) => LOG.Debug("Entering Active Round (base)...");
            RoundBreak.enterState += (prev, args) => LOG.Debug("Entering Round Break...");
            RoundPause.enterState += (prev, args) => LOG.Debug("Entering Pause Round (active)...");
            RoundActive.enterState += (prev, args) => LOG.Debug("Entering Active Round (active)...");
            FinishRound.enterState += (prev, args) => LOG.Debug("Entering Finish Round...");
            GameEnded.enterState += (prev, args) => LOG.Debug("Game Ended...");

            RoundBreak.enterState += (prevState, args) => {
                // Adjust Total Time to accomodate extra time in previous rounds
                LOG.Debug("Recalculating total time...");
                var newTime = (DateTime.Now - GameStartTime) + (EnduranceParameters.RoundTime * (TotalRounds - CurrentRoundIndex));
                LOG.Debug("Adjusting time from [" + TotalTime + "] to [" + newTime + "]");
                TotalTime = newTime;
                gameView.setDuration(TotalTime);

                CurrentRoundIndex++;
                StageStartTime = DateTime.Now;
                StageDuration = EnduranceParameters.RoundTime;
            };

            // Link the current states to regular game stages
            FinishRound.enterState += (prevState, args) => CurrentStage = GameStage.Finish;
            GameEnded.enterState += (prevState, args) => CurrentStage = GameStage.Ended;

            GameStates = new(FirstRound);
            GameStates.add(FirstRound, BasePause);
            GameStates.add(BasePause, BaseActive, false);
            GameStates.add(BaseActive, RoundBreak);
            GameStates.add(RoundBreak, RoundActive, false);
            GameStates.add(RoundActive, RoundPause, false);
            GameStates.add(RoundActive, FinishRound);
            GameStates.add(FinishRound, GameEnded);

            // Calculate number of rounds after base time
            int minRounds = gameParameters.MinRounds; int maxRounds = gameParameters.MaxRounds;
            if (minRounds > maxRounds) (maxRounds, minRounds) = (minRounds, maxRounds); // swap so min <= max
            if (minRounds < 0) minRounds = 0;   // make sure non-negative
            if (maxRounds < 0) maxRounds = 0;
            TotalRounds = (ushort)(minRounds + (Random.Shared.Next(maxRounds - minRounds)));
            CurrentRoundIndex = 0;

            TotalTime = gameParameters.BaseTime + (gameParameters.RoundTime * TotalRounds);
            gameView.setDuration(TotalTime);

            LOG.Info("Starting Endurance Game: " + GameStartTime.ToString(@"HH\:mm\:ss") + " for approx " + formatTime(TotalTime) + " - ETA: " + (GameStartTime + TotalTime).ToString(@"HH\:mm\:ss"));
            LOG.Info(getLogText());
        }

        protected override string getLogText() => "Running Endurance " + formatTime(EnduranceParameters.BaseTime) + " + " + TotalRounds + " rounds of " + formatTime(EnduranceParameters.RoundTime);

        protected override void doRoundUpdate() {
            if (GameStates.CurrentState == GameEnded) {
                doEndedRound();
                return;
            }

            if ((RoundStartTime + CurrentRound.RoundDuration) < DateTime.Now) {
                // Select next round based on new state
                if (GameStates.CurrentState == FirstRound) {
                    // End of First Round
                    GameStates.gotoState(BasePause);
                    CurrentRound = EnduranceCatalog.getPauseRound();
                } else if (GameStates.CurrentState == BasePause) {
                    // End of pause round; can only move to BaseActive
                    GameStates.gotoState(BaseActive);
                    CurrentRound = EnduranceCatalog.getActiveRound(CommonParameters.IsIntense, false);
                } else if (GameStates.CurrentState == BaseActive) {
                    // End of active round; goes to either BasePause, or RoundBreak to proceed into active rounds
                    GameStates.gotoState(((StageStartTime + StageDuration) < DateTime.Now) ? RoundBreak : BasePause);
                    if (GameStates.CurrentState == RoundBreak) {
                        CurrentRound = EnduranceCatalog.getEnduranceRound(EnduranceParameters.Type, CurrentRoundIndex, TotalRounds);
                    } else {
                        CurrentRound = EnduranceCatalog.getPauseRound();
                    }
                } else if (GameStates.CurrentState == RoundBreak) {
                    // End of RoundBreak; always moves to RoundActive
                    GameStates.gotoState(RoundActive);
                    CurrentRound = EnduranceCatalog.getActiveRound(CommonParameters.IsIntense, false);
                } else if (GameStates.CurrentState == RoundPause) {
                    // End of RoundPause; always moves to RoundActive
                    GameStates.gotoState(RoundActive);
                    CurrentRound = EnduranceCatalog.getActiveRound(CommonParameters.IsIntense, false);
                } else if (GameStates.CurrentState == RoundActive) {
                    // End of RoundActive; moves to either RoundPause, RoundBreak, or FinishRound
                    if ((StageStartTime + StageDuration) < DateTime.Now) {
                        // End of Stage; moves to either RoundBreak, or FinishRound
                        if (CurrentRoundIndex == TotalRounds) {
                            // End of Rounds; goto FinishRound
                            GameStates.gotoState(FinishRound);
                            FinishCounter++;
                            CurrentRound = EnduranceCatalog.getFinishRound(CommonParameters.IsIntense);
                        } else {
                            // End of current Round; goto RoundBreak
                            GameStates.gotoState(RoundBreak);
                            CurrentRound = EnduranceCatalog.getEnduranceRound(EnduranceParameters.Type, CurrentRoundIndex, TotalRounds);
                        }
                    } else {
                        // just regular pause round
                        GameStates.gotoState(RoundPause);
                        CurrentRound = EnduranceCatalog.getPauseRound();
                    }
                } else if (GameStates.CurrentState == FinishRound) {
                    FinishCounter++;
                    if ((FinishCounter > 1) && !CommonParameters.LoopFinish) {
                        GameStates.gotoState(GameEnded);
                        doEndedRound();
                        return;
                    } else {
                        LOG.Debug("Starting Finish Round...");
                        CurrentRound = RoundCatalog.getFinishRound(CommonParameters.IsIntense).setBeatRate(5.0, 5.0).setHandyRate(100, 100);
                    }
                }

                RoundStartTime = DateTime.Now;
                resetLastChangeTime();
            }
            var timeRemaining = (GameStartTime + TotalTime) - DateTime.Now;
            ActiveGameView.FinishText = formatTime(timeRemaining);
            ActiveGameView.setProgress(timeRemaining);
            doRoundUpdate(RoundStartTime, (GameStates.CurrentState == FinishRound));
        }

        protected override void doPause() { }
        protected override void doResume() {
            // Adjust StageStartTime as for RoundStartTime
            StageStartTime += (DateTime.Now - PauseStartTime);
        }

        protected override GameRound loadFirstRound() {
            GameRound currRound = EnduranceCatalog.getFirstRound();

            StageStartTime = GameStartTime;
            StageDuration = EnduranceParameters.BaseTime;
            RoundStartTime = StageStartTime;

            return currRound;
        }
    }
}
