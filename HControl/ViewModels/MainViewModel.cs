using System;
using System.Threading;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using HControl.Game;
using HControl.Views;
using ReactiveUI;

namespace HControl.ViewModels;

public class MainViewModel : ViewModelBase {
	
	public CommonGameParameters CommonParameters { get; } = new CommonGameParameters();

    public event EventHandler? OnCancelClicked;

    public event EventHandler? OnNextImage;
    public event EventHandler? OnPrevImage;
    public event EventHandler? OnPause;

    public void triggerOnCancelClicked() {
        OnCancelClicked?.Invoke(this, new EventArgs());
    }

    public void triggerOnNextImage() => OnNextImage?.Invoke(this, new EventArgs());
    public void triggerOnPrevImage() => OnPrevImage?.Invoke(this, new EventArgs());
    public void triggerOnPause() => OnPause?.Invoke(this, new EventArgs());

    private static readonly int PAGE_SETTINGS = 0;
    private static readonly int PAGE_SLIDESHOW = 1;

    private int _SelectedPage = 0;
    public int SelectedPage {
        get => _SelectedPage;
        set => this.RaiseAndSetIfChanged(ref _SelectedPage, value);
    }

    public void showSettings() => SelectedPage = PAGE_SETTINGS;
    public void showSlideshow() => SelectedPage = PAGE_SLIDESHOW;

    private IImage? _CurrentImage;
    public IImage? CurrentImage {
        get => _CurrentImage;
        set => this.RaiseAndSetIfChanged(ref _CurrentImage, value);
    }

    private bool _ShowImage = false;
    public bool ShowImage {
        get => _ShowImage;
        set => this.RaiseAndSetIfChanged(ref _ShowImage, value);
    }

    private string _MessageContent = "Loading...";
    public string MessageContent {
        get => _MessageContent;
        set => this.RaiseAndSetIfChanged(ref _MessageContent, value);
    }

    private string _TimerText = string.Empty;
    public string TimerText {
        get => _TimerText;
        set => this.RaiseAndSetIfChanged(ref _TimerText, value);
    }

    private string _FinishText = string.Empty;
    public string FinishText {
        get => _FinishText;
        set => this.RaiseAndSetIfChanged(ref _FinishText, value);
    }

    private double _TotalDuration = 0;
    public double TotalDuration {
        get => _TotalDuration;
        set => this.RaiseAndSetIfChanged(ref _TotalDuration, value);
    }

    public void setDuration(TimeSpan duration) {
        TotalDuration = duration.Ticks;
    }

    private double _CurrentProgress = 0;
    public double CurrentProgress {
        get => _CurrentProgress;
        set => this.RaiseAndSetIfChanged(ref _CurrentProgress, value);
    }

    public void setProgress(TimeSpan remaining) {
        CurrentProgress = TotalDuration - remaining.Ticks;
    }

    private double _RoundDuration = 0;
    public double RoundDuration {
        get => _RoundDuration;
        set => this.RaiseAndSetIfChanged(ref _RoundDuration, value);
    }

    private double _RoundProgress = 0;
    public double RoundProgress {
        get => _RoundProgress;
        set => this.RaiseAndSetIfChanged(ref _RoundProgress, value);
    }

    public void setRoundDuration(TimeSpan duration) => RoundDuration = duration.Ticks;
    public void setRoundProgress(TimeSpan remaining) => RoundProgress = RoundDuration - remaining.Ticks;

    public enum StateIndicator { PAUSE, FAP, DENY, CUM }

    private StateIndicator _CurrentState = StateIndicator.DENY;
    private IImage? _StateImage;
    public IImage? StateImage {
        get => _StateImage;
        private set => this.RaiseAndSetIfChanged(ref _StateImage, value);
    }

    public StateIndicator CurrentState {
        get => _CurrentState;
        set {
            if (_CurrentState != value) StateImage = getImage(value);   // only update if state changes
            _CurrentState = value;
        }
    }

    private bool _TickerVisible = true;
    public bool TickerVisible {
        get => _TickerVisible;
        set => this.RaiseAndSetIfChanged(ref _TickerVisible, value);
    }

    private string _StatusText = string.Empty;
    public string StatusText {
        get {
            lock (this)
                return _StatusText;
        }
        set {
            lock (this) {
                this.RaiseAndSetIfChanged(ref _StatusText, value);
                LastStatusUpdate = DateTime.Now;
                IsFading = false;
                StatusOpacity = 1.0;
            }
        }
    }

    private double _StatusOpacity = 1.0;
    public double StatusOpacity {
        get {
            lock (this) 
                return _StatusOpacity;
        }
        set {
            lock (this) {
                this.RaiseAndSetIfChanged(ref _StatusOpacity, value);
            }
        }
    }

    private DateTime LastStatusUpdate = DateTime.Now;
    private bool _IsFading = false;
    private bool IsFading {
        get {
            lock (this) return _IsFading;
        }
        set {
            lock (this) _IsFading = value;
        }
    }

    private static Avalonia.Media.Imaging.Bitmap getImage(StateIndicator currState) => currState switch {
        StateIndicator.FAP => (Random.Shared.Next(1) == 0)
                            ? (SettingsStore.Instance.isMaleUI ? LUCAS_FAP1 : LUCIA_FAP1)
                            : (SettingsStore.Instance.isMaleUI ? LUCAS_FAP2 : LUCIA_FAP2),
        StateIndicator.DENY => (SettingsStore.Instance.isMaleUI ? LUCAS_DENY : LUCIA_DENY),
        StateIndicator.CUM => (SettingsStore.Instance.isMaleUI ? LUCAS_CUM : LUCIA_CUM),
        _ => (SettingsStore.Instance.isMaleUI ? LUCAS_PAUSE : LUCIA_PAUSE),
    };

    private static readonly Bitmap LUCAS_DENY = new("./Assets/img/head_deny_Lucas.png");
    private static readonly Bitmap LUCIA_DENY = new("./Assets/img/head_deny_Lucia.png");
    private static readonly Bitmap LUCAS_CUM = new("./Assets/img/head_cum_Lucas.png");
    private static readonly Bitmap LUCIA_CUM = new("./Assets/img/head_cum_Lucia.png");
    private static readonly Bitmap LUCAS_FAP1 = new("./Assets/img/head_fap_Lucas.png");
    private static readonly Bitmap LUCIA_FAP1 = new("./Assets/img/head_fap_Lucia.png");
    private static readonly Bitmap LUCAS_FAP2 = new("./Assets/img/head_fap2_Lucas.png");
    private static readonly Bitmap LUCIA_FAP2 = new("./Assets/img/head_fap2_Lucia.png");
    private static readonly Bitmap LUCAS_PAUSE = new("./Assets/img/head_pause_Lucas.png");
    private static readonly Bitmap LUCIA_PAUSE = new("./Assets/img/head_pause_Lucia.png");

    public MainViewModel() {
        LOG.Debug("Creating Escape command...");
        this.CloseCommand = ReactiveCommand.Create(() => {
            LOG.Debug("Triggered KeyBinding: Escape");
            MainWindow.PrimaryWindowReference?.doTriggerClose();
        });

        LOG.Debug("Creating Fullscreen command...");
        this.FullscreenCommand = ReactiveCommand.Create(() => {
            LOG.Debug("Triggered KeyBinding: F11");
            Window? wndMain = MainWindow.PrimaryWindowReference;
            if (wndMain != null) {
                (wndMain.WindowState, oldState) = (oldState, wndMain.WindowState);
            }
        });

        LOG.Debug("Creating Next Image command...");
        this.NextImageCommand = ReactiveCommand.Create(() => {
            LOG.Debug("Triggered KeyBinding: ->");
            triggerOnNextImage();
        });

        LOG.Debug("Creating Prev Image command...");
        this.PrevImageCommand = ReactiveCommand.Create(() => {
            LOG.Debug("Triggered KeyBinding: <-");
            triggerOnPrevImage();
        });

        LOG.Debug("Creating Pause command...");
        this.PauseCommand = ReactiveCommand.Create(() => {
            LOG.Debug("Triggered KeyBinding: Space");
            triggerOnPause();
        });

        Thread updateThread = new(() => {
            while (true) {
                Thread.Sleep(5000);
                if ((StatusText != string.Empty) && (LastStatusUpdate + TimeSpan.FromSeconds(30) < DateTime.Now)) {
                    IsFading = true;
                    for (double o = 1.0; IsFading && o > 0.0; o -= 0.01) {
                        Thread.Sleep(50);
                        StatusOpacity = o;
                    }
                    if (IsFading) {
                        StatusText = string.Empty;
                        StatusOpacity = 1.0;
                        IsFading = false;
                    }
                }
            }
        }) {
            IsBackground = true
        };
        updateThread.Start();
    }

    private WindowState oldState = WindowState.FullScreen;

    public ICommand CloseCommand { get; } 
    public ICommand FullscreenCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand NextImageCommand { get; }
    public ICommand PrevImageCommand { get; }

    protected static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();
}
