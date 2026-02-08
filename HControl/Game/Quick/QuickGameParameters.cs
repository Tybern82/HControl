using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace HControl.Game.Quick {
    public class QuickGameParameters : BasicGameParameters {

        private int _RoundCount = 40;
        public int RoundCount {
            get => _RoundCount;
            set => this.RaiseAndSetIfChanged(ref _RoundCount, value);
        }

        public QuickGameParameters() {
            JToken? p = SettingsStore.Instance.getParameter("module_" + QuickGameModule.NAME);
            if (p != null && p is JObject) {
                JObject qParams = (JObject)p;
                RoundCount = qParams.Value<int>(PARAM_ROUNDCOUNT);
            }
        }

        public override void saveParameters() {
            JObject qParams = new JObject();
            qParams[PARAM_ROUNDCOUNT] = RoundCount;
            SettingsStore.Instance.setParameter("module_" + QuickGameModule.NAME, qParams);
        }

        private static readonly string PARAM_ROUNDCOUNT = "RoundCount";
    }
}
