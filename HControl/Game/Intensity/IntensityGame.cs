using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HControl.Game.Timed;
using HControl.ViewModels;
using static HControl.Game.Intensity.IntensityGameParameters;

namespace HControl.Game.Intensity {
    public class IntensityGame : ActiveGame {

        private IntensityLevel CurrentLevel { get; set; }
        private GameMode CurrentMode { get; set; }
        private IntensityGameParameters IntensityParameters { get; set; }

        private DateTime StageStartTime { get; set; }
        private TimeSpan StageDuration { get; set; } = TimeSpan.Zero;
        private TimeSpan RemainingDuration { get; set; } = TimeSpan.Zero;
        private int StageCounter { get; set; } = 0;

        private int FinishCounter { get; set; } = 0;

        public IntensityGame(MainViewModel gameView, IntensityGameParameters gameParameters) : base(gameView, gameParameters.CommonParameters) {
            IntensityParameters = gameParameters;

            LOG.Info(getLogText());

            gameView.setDuration(gameParameters.Duration);

            CurrentLevel = gameParameters.InitialLevel;
            CurrentMode = gameParameters.Mode;
        }

        protected override string getLogText() => "Starting Intensity Game: " + GameStartTime.ToString(@"HH\:mm\:ss") + " for " + formatTime(IntensityParameters.Duration) + " - ETA: " + (GameStartTime + IntensityParameters.Duration).ToString(@"HH\:mm\:ss");

        protected override GameRound loadFirstRound() {
            GameRound currRound = getFirstRound(CurrentLevel);

            RoundStartTime = GameStartTime;
            StageStartTime = RoundStartTime;
            StageDuration = IntensityParameters.Duration;   // default to full length
            RemainingDuration = IntensityParameters.Duration;

            switch (CurrentMode) {
                // Set timed duration for Increasing/Decreasing; Fixed has no change; other modes use ChangeAfter counter
                case GameMode.Increasing:
                case GameMode.Decreasing:
                case GameMode.IncreasingInverted:
                case GameMode.DecreasingInverted:
                    StageDuration = getIncrementalPeriod(CurrentLevel, CurrentMode, RemainingDuration);
                    var logText = "Sequenced Level: [" + EnumBindingWithDescription.EnumConverter.GetEnumDescription(CurrentLevel) + "] - " + StageDuration;
                    LOG.Info(logText);
                    ActiveGameView.StatusText = logText;
                    break;
            }
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
        protected override void doResume() {
            // Adjust StageStartTime as for RoundStartTime
            StageStartTime += (DateTime.Now - PauseStartTime);
        }

        private void doMainRound() {
            bool isFinish = (GameStartTime + IntensityParameters.Duration) <= DateTime.Now;
            if ((RoundStartTime + CurrentRound.RoundDuration) < DateTime.Now) {
                if (!CurrentRound.IsActiveRound) {
                    // at end of Pause round, adjust Level if required before starting next active round
                    StageCounter++;
                    switch (CurrentMode) {
                        case GameMode.Fixed:
                            break; // level does not change

                        case GameMode.Increasing:
                        case GameMode.Decreasing:
                        case GameMode.IncreasingInverted:
                        case GameMode.DecreasingInverted:
                            if ((DateTime.Now - StageStartTime) >= StageDuration) {
                                // Time to update to next stage
                                if ((CurrentMode == GameMode.Increasing) || (CurrentMode == GameMode.IncreasingInverted)) {
                                    CurrentLevel = incrementLevel(CurrentLevel);
                                } else {
                                    CurrentLevel = decrementLevel(CurrentLevel);
                                }
                                StageStartTime = DateTime.Now;
                                RemainingDuration = GameStartTime + IntensityParameters.Duration - DateTime.Now;
                                StageDuration = getIncrementalPeriod(CurrentLevel, CurrentMode, RemainingDuration);
                                var logText = "Sequenced Level: [" + EnumBindingWithDescription.EnumConverter.GetEnumDescription(CurrentLevel) + "] - " + formatTime(StageDuration);
                                LOG.Info(logText);
                                ActiveGameView.StatusText = logText;
                            }
                            break;

                        case GameMode.Random:
                            if (StageCounter >= IntensityParameters.ChangeAfter) {
                                CurrentLevel = getLevel(Random.Shared.Next(9) + 1); // (Random[0-8])
                                var logText = "Changing to random level: [" + EnumBindingWithDescription.EnumConverter.GetEnumDescription(CurrentLevel) + "]";
                                LOG.Info(logText);
                                ActiveGameView.StatusText = logText;
                                StageCounter = 0;
                            }
                            break;

                        case GameMode.PulseDown:
                            if (StageCounter >= IntensityParameters.ChangeAfter) {
                                CurrentLevel = decrementLevel(CurrentLevel);
                                if (CurrentLevel == IntensityLevel.Level1) CurrentMode = GameMode.PulseUp;  // hit the lowest value, reverse direction
                                StageCounter = 0;
                                var logText = "Pulse Down: [" + EnumBindingWithDescription.EnumConverter.GetEnumDescription(CurrentLevel) + "]";
                                LOG.Info(logText);
                                ActiveGameView.StatusText = logText;
                            }
                            break;

                        case GameMode.PulseUp:
                            if (StageCounter >= IntensityParameters.ChangeAfter) {
                                CurrentLevel = incrementLevel(CurrentLevel);
                                if (CurrentLevel == IntensityLevel.Level9) CurrentMode = GameMode.PulseDown;    // hit the highest value, reverse direction
                                StageCounter = 0;
                                var logText = "Pulse Up: [" + EnumBindingWithDescription.EnumConverter.GetEnumDescription(CurrentLevel) + "]";
                                LOG.Info(logText);
                                ActiveGameView.StatusText = logText;
                            }
                            break;
                    }
                }
                if (isFinish) {
                    CurrentStage = GameStage.Finish;
                } else {
                    // End of Round
                    CurrentRound = (CurrentRound.IsActiveRound && !isFinish) ? RoundCatalog.getPauseRound() : getNextRound(CurrentLevel);
                    LOG.Debug(CurrentRound.IsActiveRound ? "Next round starting..." : "Next break starting...");
                    RoundStartTime = DateTime.Now;
                    resetLastChangeTime();
                }
            }
            if (CurrentStage != GameStage.Finish) {
                ActiveGameView.FinishText = formatTime((GameStartTime + IntensityParameters.Duration) - DateTime.Now);
                ActiveGameView.setProgress((GameStartTime + IntensityParameters.Duration) - DateTime.Now);
                doRoundUpdate(RoundStartTime, false);
            }
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
            ActiveGameView.FinishText = formatTime((GameStartTime + IntensityParameters.Duration) - DateTime.Now);
            ActiveGameView.setProgress((GameStartTime + IntensityParameters.Duration) - DateTime.Now);
            doRoundUpdate(RoundStartTime, FinishCounter > 0);
        }

        private static GameRound getFirstRound(IntensityGameParameters.IntensityLevel level) {
            ushort minPercent = getMinPercent(level);
            ushort maxPercent = getMaxPercent(level);
            return GameRound.newRound(true, "Let's start, warm up and fap for a while, then stop when the EDGE bar reaches the end.", 30, 50).setBeatRate(5.0 * minPercent / 100, 5.0 * maxPercent / 100).setHandyRate(minPercent, maxPercent);
        }

        private static TimeSpan getIncrementalPeriod(IntensityLevel level, GameMode mode, TimeSpan totalDuration) {
            switch (mode) {
                case GameMode.Increasing:
                    return level switch {
                        IntensityLevel.Level1 => (totalDuration * 9 / 45),
                        IntensityLevel.Level2 => (totalDuration * 8 / 36),
                        IntensityLevel.Level3 => (totalDuration * 7 / 28),
                        IntensityLevel.Level4 => (totalDuration * 6 / 21),
                        IntensityLevel.Level5 => (totalDuration * 5 / 15),
                        IntensityLevel.Level6 => (totalDuration * 4 / 10),
                        IntensityLevel.Level7 => (totalDuration * 3 / 6),
                        IntensityLevel.Level8 => (totalDuration * 2 / 3),
                        IntensityLevel.Level9 => totalDuration,
                        _ => totalDuration,
                    };
                case GameMode.Decreasing:
                    return level switch {
                        IntensityLevel.Level1 => totalDuration,
                        IntensityLevel.Level2 => (totalDuration * 2 / 3),
                        IntensityLevel.Level3 => (totalDuration * 3 / 6),
                        IntensityLevel.Level4 => (totalDuration * 4 / 10),
                        IntensityLevel.Level5 => (totalDuration * 5 / 15),
                        IntensityLevel.Level6 => (totalDuration * 6 / 21),
                        IntensityLevel.Level7 => (totalDuration * 7 / 28),
                        IntensityLevel.Level8 => (totalDuration * 8 / 36),
                        IntensityLevel.Level9 => (totalDuration * 9 / 45),
                        _ => totalDuration,
                    };

                case GameMode.IncreasingInverted:
                    return level switch {
                        IntensityLevel.Level1 => (totalDuration * 1 / 45),
                        IntensityLevel.Level2 => (totalDuration * 2 / 44),
                        IntensityLevel.Level3 => (totalDuration * 3 / 42),
                        IntensityLevel.Level4 => (totalDuration * 4 / 39),
                        IntensityLevel.Level5 => (totalDuration * 5 / 35),
                        IntensityLevel.Level6 => (totalDuration * 6 / 30),
                        IntensityLevel.Level7 => (totalDuration * 7 / 24),
                        IntensityLevel.Level8 => (totalDuration * 8 / 17),
                        IntensityLevel.Level9 => totalDuration,
                        _ => totalDuration,
                    };

                case GameMode.DecreasingInverted:
                    return level switch {
                        IntensityLevel.Level1 => totalDuration,
                        IntensityLevel.Level2 => (totalDuration * 8 / 17),
                        IntensityLevel.Level3 => (totalDuration * 7 / 24),
                        IntensityLevel.Level4 => (totalDuration * 6 / 30),
                        IntensityLevel.Level5 => (totalDuration * 5 / 35),
                        IntensityLevel.Level6 => (totalDuration * 4 / 39),
                        IntensityLevel.Level7 => (totalDuration * 3 / 42),
                        IntensityLevel.Level8 => (totalDuration * 2 / 44),
                        IntensityLevel.Level9 => (totalDuration * 1 / 45),
                        _ => totalDuration,
                    };

            }
            return totalDuration;
        }

        /* Original
        private static TimeSpan getIncrementalPeriod(IntensityLevel level, GameMode mode, TimeSpan totalDuration) => level switch {
            IntensityLevel.Level1 => (mode == GameMode.Increasing) ? (totalDuration * 9 / 45) : totalDuration,
            IntensityLevel.Level2 => (mode == GameMode.Increasing) ? (totalDuration * 8 / 36) : (totalDuration * 2 / 3),
            IntensityLevel.Level3 => (mode == GameMode.Increasing) ? (totalDuration * 7 / 28) : (totalDuration * 3 / 6),
            IntensityLevel.Level4 => (mode == GameMode.Increasing) ? (totalDuration * 6 / 21) : (totalDuration * 4 / 10),
            IntensityLevel.Level5 => (totalDuration * 5 / 15),
            IntensityLevel.Level6 => (mode == GameMode.Increasing) ? (totalDuration * 4 / 10) : (totalDuration * 6 / 21),
            IntensityLevel.Level7 => (mode == GameMode.Increasing) ? (totalDuration * 3 / 6) : (totalDuration * 7 / 28),
            IntensityLevel.Level8 => (mode == GameMode.Increasing) ? (totalDuration * 2 / 3) : (totalDuration * 8 / 36),
            IntensityLevel.Level9 => (mode == GameMode.Increasing) ? totalDuration : (totalDuration * 9 / 45),
            _ => totalDuration,
        }; */

        private static IntensityLevel getLevel(int index) {
            if (index <= 1) return IntensityLevel.Level1;
            else if (index == 2) return IntensityLevel.Level2;
            else if (index == 3) return IntensityLevel.Level3;
            else if (index == 4) return IntensityLevel.Level4;
            else if (index == 5) return IntensityLevel.Level5;
            else if (index == 6) return IntensityLevel.Level6;
            else if (index == 7) return IntensityLevel.Level7;
            else if (index == 8) return IntensityLevel.Level8;
            else return IntensityLevel.Level9;
        }

        private static IntensityLevel decrementLevel(IntensityLevel level) => level switch {
            IntensityLevel.Level9 => IntensityLevel.Level8,
            IntensityLevel.Level8 => IntensityLevel.Level7,
            IntensityLevel.Level7 => IntensityLevel.Level6,
            IntensityLevel.Level6 => IntensityLevel.Level5,
            IntensityLevel.Level5 => IntensityLevel.Level4,
            IntensityLevel.Level4 => IntensityLevel.Level3,
            IntensityLevel.Level3 => IntensityLevel.Level2,
            _ => IntensityLevel.Level1,
        };

        private static IntensityLevel incrementLevel(IntensityLevel level) => level switch {
            IntensityLevel.Level1 => IntensityLevel.Level2,
            IntensityLevel.Level2 => IntensityLevel.Level3,
            IntensityLevel.Level3 => IntensityLevel.Level4,
            IntensityLevel.Level4 => IntensityLevel.Level5,
            IntensityLevel.Level5 => IntensityLevel.Level6,
            IntensityLevel.Level6 => IntensityLevel.Level7,
            IntensityLevel.Level7 => IntensityLevel.Level8,
            _ => IntensityLevel.Level9,
        };

        private static GameRound getNextRound(IntensityGameParameters.IntensityLevel level) {
            ushort minPercent = getMinPercent(level);
            ushort maxPercent = getMaxPercent(level);
            string message = getMessage(level);
            return GameRound.newRound(true, message, 20, 50).setBeatRate(5.0 * minPercent / 100, 5.0 * maxPercent / 100).setHandyRate(minPercent, maxPercent);
        }

        private static ushort getMinPercent(IntensityGameParameters.IntensityLevel level) => level switch {
            IntensityGameParameters.IntensityLevel.Level9 => 70,
            IntensityGameParameters.IntensityLevel.Level8 => 60,
            IntensityGameParameters.IntensityLevel.Level7 => 50,
            IntensityGameParameters.IntensityLevel.Level6 => 40,
            IntensityGameParameters.IntensityLevel.Level5 => 30,
            IntensityGameParameters.IntensityLevel.Level4 => 25,
            IntensityGameParameters.IntensityLevel.Level3 => 20,
            IntensityGameParameters.IntensityLevel.Level2 => 15,
            _ => 10,
        };

        private static ushort getMaxPercent(IntensityGameParameters.IntensityLevel level) => level switch {
            IntensityGameParameters.IntensityLevel.Level1 => 30,
            IntensityGameParameters.IntensityLevel.Level2 => 40,
            IntensityGameParameters.IntensityLevel.Level3 => 50,
            IntensityGameParameters.IntensityLevel.Level4 => 60,
            IntensityGameParameters.IntensityLevel.Level5 => 70,
            IntensityGameParameters.IntensityLevel.Level6 => 75,
            IntensityGameParameters.IntensityLevel.Level7 => 80,
            IntensityGameParameters.IntensityLevel.Level8 => 85,
            _ => 90,
        };

        private static string getMessage(IntensityGameParameters.IntensityLevel level) => level switch {
            IntensityGameParameters.IntensityLevel.Level1 or IntensityGameParameters.IntensityLevel.Level2 => "Go SLOW and steady...",
            IntensityGameParameters.IntensityLevel.Level8 or IntensityGameParameters.IntensityLevel.Level9 => "Go HARD and FAST",
            _ => "Stroke",
        };
    }
}
