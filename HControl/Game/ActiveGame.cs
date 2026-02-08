using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using com.tybern.CMDProcessor;
using HControl.Devices;
using HControl.ViewModels;
using NAudio.Wave;
using static HControl.ViewModels.MainViewModel;

namespace HControl.Game {
    public abstract class ActiveGame {

        protected static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        public static string formatTime(TimeSpan currTime) {
            if (currTime < TimeSpan.Zero) return "--";
            return (currTime.TotalHours >= 1) ? currTime.ToString(@"hh\:mm\:ss") : ((currTime.TotalMinutes >= 1) ? currTime.ToString(@"mm\:ss") : currTime.ToString(@"ss"));
        }

        protected enum GameStage { Main, Finish, Ended };

        protected abstract string getLogText();

        protected abstract GameRound loadFirstRound();

        protected abstract void doRoundUpdate();

        protected void doRoundUpdate(DateTime roundStartTime, bool isFinish) {
            DateTime currTime = DateTime.Now;
            if (CurrentRound.IsActiveRound || CommonParameters.NoImageBreak) {
                if (currTime >= (lastChangeTime + ActiveGameView.CommonParameters.PictureCycle)) {
                    triggerImageChange();
                    string imgFile = (isFinish ? FinishImageList.getNextImage() : MainImageList.getNextImage());
                    LOG.Debug("Selecting Image: [" + imgFile + "] - isFinish: " + isFinish);
                    var img = new Avalonia.Media.Imaging.Bitmap(imgFile);

                    ActiveGameView.ShowImage = true;
                    ActiveGameView.CurrentImage = img;
                    ActiveGameView.MessageContent = CurrentRound.RoundContent;
                    ActiveGameView.CurrentState = isFinish ? MainViewModel.StateIndicator.CUM : MainViewModel.StateIndicator.FAP;
                }
            } else {
                ActiveGameView.ShowImage = false;
                ActiveGameView.MessageContent = CurrentRound.RoundContent;
                ActiveGameView.CurrentState = MainViewModel.StateIndicator.PAUSE;
            }
            ActiveGameView.TimerText = formatTime(CurrentRound.RoundDuration - (currTime - roundStartTime));
            ActiveGameView.setRoundProgress(CurrentRound.RoundDuration - (currTime - roundStartTime));
        }

        public void cleanupGame() {
            CurrentIntensity = 0;
            var finalDurationText = "Final Duration: " + formatTime(DateTime.Now - GameStartTime);
            LOG.Info(finalDurationText);
            ActiveGameView.StatusText = finalDurationText;
            LOG.Debug("Cleaning up game...");
            closeDeviceWindow();
            LOG.Debug("Change Main View...");
            ActiveGameView.showSettings();
            LOG.Debug("Disconnect Handy...");
            CommandQueue.Enqueue(new InlineCommand(() => HandyControl.Instance.disconnect()));
            LOG.Debug("Closing processing thread...");
            CommandQueue.Terminate();
        }

        public void runGame() {
            ActiveGameView.StatusText = getLogText();
            new Thread(() => {
                startGame();

                try {
                    while (!IsTerminated) {
                        Thread.Sleep(SettingsStore.PULSE_TIME);
                        if (!isPaused) doRoundUpdate();
                    }
                } catch (Exception e) { LOG.Error(e); }

                cleanupGame();
            }).Start();
        }

        public void startGame() {
            if (CommonParameters.EnableHandy) {
                LOG.Info("Connecting to HandyControl....");
                CommandQueue.Enqueue(new InlineCommand(() => HandyControl.Instance.connect(CommonParameters.HandyKey)));
            }
            ActiveGameView.showSlideshow();
            Thread.CurrentThread.IsBackground = true;
        }

        public void closeDeviceWindow() {
            if (DWindow != null) {
                CommandQueue.Enqueue(new InlineCommand(() => DWindow.Close(), Command.RunThread.UIThread));
                LOG.Debug("Closing device window...");
            };
        }

        private void setIntensity() {
            ushort selectedRate = (CommonParameters.NoBeatBreak && CurrentRound.HandyRate == 0) ? (ushort)CommonParameters.BeatDefault : CurrentRound.HandyRate;
            LOG.Debug("Set Intensity: " + selectedRate);
            if (CommonParameters.EnableHandy) {
                CommandQueue.Enqueue(new InlineCommand(() => HandyControl.Instance.setIntensity(selectedRate)));
            }
            CurrentIntensity = selectedRate;
        }

        public DateTime GameStartTime { get; }
        public DateTime RoundStartTime { get; protected set; }
        public ImageList MainImageList { get; } = new ImageList();
        public ImageList FinishImageList { get; } = new ImageList();

        public bool IsTerminated { get; set; } = false;

        public MainViewModel ActiveGameView { get; }

        private CommandProcessor? _commandQueue;
        public CommandProcessor CommandQueue { 
            get {
                _commandQueue ??= getCommandQueue();
                return _commandQueue;
            }
        }

        private DeviceWindow? _DWindow;
        public DeviceWindow? DWindow {
            get => _DWindow;
            set => _DWindow = value;
        }

        public CommonGameParameters CommonParameters { get; }

        private GameRound? _CurrentRound;
        public GameRound CurrentRound {
            get {
                if (_CurrentRound == null) {
                    _CurrentRound = loadFirstRound();
                    ActiveGameView.setRoundDuration(_CurrentRound.RoundDuration);
                    setIntensity();
                }
                return _CurrentRound;
            }

            set {
                if (_CurrentRound != value) {
                    // only update if changed
                    _CurrentRound = value;
                    ActiveGameView.setRoundDuration(value.RoundDuration);
                    setIntensity();
                }
            }
        }

        public DateTime lastChangeTime { get; private set; } = DateTime.MinValue;

        protected void resetLastChangeTime() => lastChangeTime = DateTime.MinValue;
        private void triggerImageChange() => lastChangeTime = DateTime.Now;

        private ushort _CurrentIntensity = 0;
        public ushort CurrentIntensity {
            get => _CurrentIntensity;
            set {
                LOG.Debug("Intensity: " + value);
                _CurrentIntensity = value;
            }
        }

        private bool IsTickerRunning = true;

        protected GameStage CurrentStage { get; set; } = GameStage.Main;

        public ActiveGame(MainViewModel gameView, CommonGameParameters gameParameters) {
            this.ActiveGameView = gameView;
            this.CommonParameters = gameParameters;

            var (audioFile, audioPlayer) = getAudioFile();
            LOG.Debug("Audio Length: " + audioFile.TotalTime);
            audioPlayer.PlaybackStopped += (sender, args) => gameView.TickerVisible = false;

            Thread tickerThread = new(() => {
                gameView.TickerVisible = false;
                int counter = 0;
                while (IsTickerRunning) {
                    Thread.Sleep(50);
                    counter++;
                    if (CurrentIntensity == 0 || CurrentIntensity == 100) {
                        counter = 0;
                        continue;
                    }
                    if (counter >= getCounter(CurrentIntensity)) {
                        gameView.TickerVisible = true;
                        audioFile.Position = 0;
                        audioPlayer.Play();
                        counter = 0;
                    }
                }
            }) {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            if (gameParameters.DoTicker)
                tickerThread.Start();
            else
                gameView.TickerVisible = false;

            DWindow = openDeviceWindow(gameParameters);    // allows manual control
            gameView.OnCancelClicked += (sender, args) => IsTerminated = true;
            gameView.OnNextImage += doNextImage;
            gameView.OnPrevImage += doPrevImage;
            gameView.OnPause += doPause;

            foreach (var item in gameParameters.ImageList) {
                if (item.IsEnabled) MainImageList.addFolder(item.Folder, item.AddSubfolders);
            }
            foreach (var item in gameParameters.FinishList) {
                if (item.IsEnabled) FinishImageList.addFolder(item.Folder, item.AddSubfolders);
            }

            LOG.Info("Loaded " + MainImageList.ImageCount + " images");
            LOG.Info("Loaded " + FinishImageList.ImageCount + " images for finish");

            // preload an image (forces FS preload)
            string imgFile = MainImageList.getNextImage();
            var img = new Avalonia.Media.Imaging.Bitmap(imgFile);
            gameView.ShowImage = false;
            gameView.CurrentImage = img;

            this.GameStartTime = DateTime.Now;
            this.RoundStartTime = GameStartTime;
        }

        protected void doEndedRound() {
            if ((RoundStartTime + CurrentRound.RoundDuration) < DateTime.Now) {
                CurrentRound = GameRound.newRound(false, "Well done, let's do this again when you feel ready :3", 60, 60);
                RoundStartTime = DateTime.Now;
                resetLastChangeTime();
            }
            doRoundUpdate(RoundStartTime, false);
        }

        private static DeviceWindow? openDeviceWindow(CommonGameParameters gameParams) {
            if (!gameParams.EnableHandy) return null;
            var devWindow = new DeviceWindow();
            devWindow.Devices.registerDevice(HandyControl.Instance);
            devWindow.Show();
            return devWindow;
        }

        private static CommandProcessor getCommandQueue() {
            CommandProcessor.RunAsUIThread = (cmd) => {
                if (Dispatcher.UIThread.CheckAccess()) {
                    cmd.Process();
                } else {
                    Dispatcher.UIThread.Invoke(() => cmd.Process());
                }
            };
            return new CommandProcessor(1, true);
        }

        private static int getCounter(int intensity) => intensity switch {
            >= 95 => 3,
            >= 90 => 4,
            >= 85 => 5,
            >= 80 => 6,
            >= 75 => 7,
            >= 70 => 8,
            >= 65 => 9,
            >= 60 => 10,
            >= 55 => 12,
            >= 50 => 14,
            >= 45 => 16,
            >= 40 => 18,
            >= 35 => 20,
            >= 30 => 22,
            >= 25 => 24,
            >= 20 => 26,
            >= 15 => 27,
            >= 10 => 28,
            _ => 30,
        };

        private static (AudioFileReader, WaveOutEvent) getAudioFile() {
            var audioFile = new AudioFileReader("Audio/Tick.wav");
            var outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
            return (audioFile, outputDevice);
        }

        ~ActiveGame() {
            IsTickerRunning = false;
            ActiveGameView.OnNextImage += doNextImage;
            ActiveGameView.OnPrevImage += doPrevImage;
            ActiveGameView.OnPause += doPause;
        }

        private void doNextImage(object? sender, EventArgs e) {
            LOG.Debug("Moving to next image...");
            bool isFinish = CurrentStage != GameStage.Main;
            string nextImage = (!isFinish) ? MainImageList.getNextImage() : FinishImageList.getNextImage();

            triggerImageChange();
            LOG.Debug("Selecting Image: [" + nextImage + "] - isFinish: " + isFinish);
            var img = new Avalonia.Media.Imaging.Bitmap(nextImage);

            ActiveGameView.CurrentImage = img;
        }

        private void doPrevImage(object? sender, EventArgs e) {
            LOG.Debug("Moving to prev image...");
            bool isFinish = CurrentStage != GameStage.Main;
            string? prevImage = (!isFinish) ? MainImageList.getPrevImage() : FinishImageList.getPrevImage();
            if (prevImage != null) {
                triggerImageChange();
                LOG.Debug("Selecting Previous Image: [" + prevImage + "]");
                var img = new Avalonia.Media.Imaging.Bitmap(prevImage);

                ActiveGameView.CurrentImage = img;
            }
        }

        private ushort pauseIntensity = 0;
        private StateIndicator pauseState;
        protected DateTime PauseStartTime;
        private bool isPaused = false;
        protected void doPause(object? sender, EventArgs e) {
            if (!isPaused) {
                LOG.Debug("Pausing...");
                // Save the time when the pause started
                PauseStartTime = DateTime.Now;

                // Save the current intensity and drop to 0
                pauseIntensity = CurrentIntensity;
                CurrentIntensity = 0;

                // Update UI to show paused state
                ActiveGameView.TimerText = "||";
                pauseState = ActiveGameView.CurrentState;
                ActiveGameView.CurrentState = StateIndicator.PAUSE;

                isPaused = true;
                doPause();
            } else {
                LOG.Debug("Resuming...");
                CurrentIntensity = pauseIntensity;
                ActiveGameView.CurrentState = pauseState;
                RoundStartTime += (DateTime.Now - PauseStartTime);  // shift the round start time forward by the time paused
                isPaused = false;
                doResume();
            }
            // Disables the manual controls
            DWindow?.wdgDevice.doPause(isPaused);
        }

        protected abstract void doPause();
        protected abstract void doResume();
    }
}
