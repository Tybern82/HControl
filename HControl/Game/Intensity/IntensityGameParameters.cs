using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace HControl.Game.Intensity {
    public class IntensityGameParameters : BasicGameParameters {

        public enum IntensityLevel {
            [Description("Level 1 (10-30%)")]
            Level1,

            [Description("Level 2 (15-40%)")]
            Level2,

            [Description("Level 3 (20-50%)")]
            Level3,

            [Description("Level 4 (25-60%)")]
            Level4,

            [Description("Level 5 (30-70%)")]
            Level5,

            [Description("Level 6 (40-75%)")]
            Level6,

            [Description("Level 7 (50-80%)")]
            Level7,

            [Description("Level 8 (60-85%)")]
            Level8,

            [Description("Level 9 (70-90%)")]
            Level9
        }

        public IEnumerable<IntensityLevel> AllIntensityLevels {
            get => Enum.GetValues<IntensityLevel>();
        }

        public enum GameMode {
            // ChangeAfter ignored
            [Description("Fixed Intensity")]
            Fixed,      // runs entire duration at initial level

            [Description("Increasing Intensity")]
            Increasing, // changes incrementally from initial level to max level

            [Description("Increasing Intensity (inverted time)")]
            IncreasingInverted,

            [Description("Decreasing Intensity")]
            Decreasing, // changes incrementally from initial level to min level

            [Description("Decreasing Intensity (inverted time)")]
            DecreasingInverted,

            // Uses ChangeAfter value
            [Description("Random Intensity")]
            Random,     // changes to a random level on cycle

            [Description("Cycling Intensity (increasing)")]
            PulseUp,    // increases to max, then switches to PulseDown

            [Description("Cycling Intensity (decreasing)")]
            PulseDown   // decreases to min, then switches to PulseUp
        }

        public IEnumerable<GameMode> AllGameModes {
            get => Enum.GetValues<GameMode>();
        }

        private IntensityLevel _InitialLevel = IntensityLevel.Level1;
        public IntensityLevel InitialLevel {
            get => _InitialLevel;
            set => this.RaiseAndSetIfChanged(ref _InitialLevel, value);
        }

        private GameMode _Mode = GameMode.Increasing;
        public GameMode Mode {
            get => _Mode;
            set => this.RaiseAndSetIfChanged(ref _Mode, value);
        }

        private TimeSpan _Duration = TimeSpan.FromMinutes(30);
        public TimeSpan Duration {
            get => _Duration;
            set => this.RaiseAndSetIfChanged(ref _Duration, value);
        }

        private int _ChangeAfter = 10;
        public int ChangeAfter {
            get => _ChangeAfter;
            set => this.RaiseAndSetIfChanged(ref _ChangeAfter, value);
        }

        public IntensityGameParameters() {
            JToken? p = SettingsStore.Instance.getParameter("module_" + IntensityGameModule.NAME);
            if (p != null && p is JObject) {
                JObject qParams = (JObject)p;
                if (qParams.ContainsKey(PARAM_INITIAL)) {
                    string? level = qParams.Value<string>(PARAM_INITIAL);
                    if (level != null) InitialLevel = Enum.Parse<IntensityLevel>(level);
                }
                if (qParams.ContainsKey(PARAM_MODE)) {
                    string? mode = qParams.Value<string>(PARAM_MODE);
                    if (mode != null) Mode = Enum.Parse<GameMode>(mode);
                }
                if (qParams.ContainsKey(PARAM_DURATION)) {
                    string? duration = qParams.Value<string>(PARAM_DURATION);
                    if (duration != null) Duration = TimeSpan.Parse(duration);
                }
                if (qParams.ContainsKey(PARAM_CHANGE)) {
                    ChangeAfter = qParams.Value<int>(PARAM_CHANGE);
                }
            }
        }

        public override void saveParameters() {
            JObject qParams = new JObject();

            qParams[PARAM_INITIAL] = InitialLevel.ToString();
            qParams[PARAM_MODE] = Mode.ToString();
            qParams[PARAM_DURATION] = Duration;
            qParams[PARAM_CHANGE] = ChangeAfter;

            SettingsStore.Instance.setParameter("module_" + IntensityGameModule.NAME, qParams);
        }

        private static readonly string PARAM_INITIAL = "InitialLevel";
        private static readonly string PARAM_MODE = "GameMode";
        private static readonly string PARAM_DURATION = "Duration";
        private static readonly string PARAM_CHANGE = "ChangeAfter";
    }
}
