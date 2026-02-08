using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HControl.Game.Timed;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace HControl.Game.Endurance {
    public class EnduranceGameParameters : BasicGameParameters {

        public enum GameMode { Gentle, Normal, Evil }

        public IEnumerable<GameMode> AllGameModes {
            get => Enum.GetValues<GameMode>();
        }

        public EnduranceGameParameters() {
            JToken? p = SettingsStore.Instance.getParameter("module_" + EnduranceGameModule.NAME);
            if (p != null && p is JObject) {
                JObject qParams = (JObject)p;
                if (qParams.ContainsKey(PARAM_BASETIME)) {
                    string? duration = qParams.Value<string>(PARAM_BASETIME);
                    if (duration != null) BaseTime = TimeSpan.Parse(duration);
                }
                if (qParams.ContainsKey(PARAM_ROUNDTIME)) {
                    string? duration = qParams.Value<string>(PARAM_ROUNDTIME);
                    if (duration != null) RoundTime = TimeSpan.Parse(duration);
                }
                if (qParams.ContainsKey(PARAM_TYPE)) {
                    string? type = qParams.Value<string>(PARAM_TYPE);
                    if (type != null) Type = Enum.Parse<GameMode>(type);
                }
                if (qParams.ContainsKey(PARAM_MINROUNDS)) {
                    MinRounds = qParams.Value<int>(PARAM_MINROUNDS);
                }
                if (qParams.ContainsKey(PARAM_MAXROUNDS)) {
                    MaxRounds = qParams.Value<int>(PARAM_MAXROUNDS);
                }
            }
        }
        public override void saveParameters() {
            JObject qParams = new JObject();

            qParams[PARAM_TYPE] = Type.ToString();
            qParams[PARAM_BASETIME] = BaseTime;
            qParams[PARAM_ROUNDTIME] = RoundTime;
            qParams[PARAM_MINROUNDS] = MinRounds;
            qParams[PARAM_MAXROUNDS] = MaxRounds;

            SettingsStore.Instance.setParameter("module_" + EnduranceGameModule.NAME, qParams);
        }

        private GameMode _Type = GameMode.Normal;
        public GameMode Type {
            get => _Type;
            set => this.RaiseAndSetIfChanged(ref _Type, value);
        }

        private TimeSpan _BaseTime = TimeSpan.FromMinutes(30);
        public TimeSpan BaseTime {
            get => _BaseTime;
            set => this.RaiseAndSetIfChanged(ref _BaseTime, value);
        }

        private TimeSpan _RoundTime = TimeSpan.FromMinutes(10);
        public TimeSpan RoundTime {
            get => _RoundTime;
            set => this.RaiseAndSetIfChanged(ref _RoundTime, value);
        }

        private int _MinRounds = 3;
        public int MinRounds {
            get => _MinRounds;
            set => this.RaiseAndSetIfChanged(ref _MinRounds, value);
        }

        private int _MaxRounds = 5;
        public int MaxRounds {
            get => _MaxRounds;
            set => this.RaiseAndSetIfChanged(ref _MaxRounds, value);
        }

        private static readonly string PARAM_TYPE = "Type";
        private static readonly string PARAM_BASETIME = "BaseTime";
        private static readonly string PARAM_ROUNDTIME = "RoundTime";
        private static readonly string PARAM_MINROUNDS = "MinimumRounds";
        private static readonly string PARAM_MAXROUNDS = "MaximumRounds";
    }
}
