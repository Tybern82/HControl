using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HControl.Game.Quick;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace HControl.Game.Timed {
    public class TimedGameParameters : BasicGameParameters {

        private TimeSpan _Duration = TimeSpan.FromMinutes(10);
        public TimeSpan Duration {
            get => _Duration;
            set => this.RaiseAndSetIfChanged(ref _Duration, value);
        }

        public TimedGameParameters() {
            JToken? p = SettingsStore.Instance.getParameter("module_" + TimedGameModule.NAME);
            if (p != null && p is JObject) {
                JObject qParams = (JObject)p;
                string? duration = qParams.Value<string>(PARAM_DURATION);
                if (duration != null) Duration = TimeSpan.Parse(duration);
            }
        }

        public override void saveParameters() {
            JObject qParams = new JObject();
            qParams[PARAM_DURATION] = Duration;
            SettingsStore.Instance.setParameter("module_" + TimedGameModule.NAME, qParams);
        }

        private static readonly string PARAM_DURATION = "Duration";
    }
}
