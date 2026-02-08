using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace HControl.Game {
    public class BasicGameParameters : ReactiveObject, HCGameParameters {

        private CommonGameParameters _CommonParameters = new CommonGameParameters();
        public CommonGameParameters CommonParameters {
            get => _CommonParameters;
            set => this.RaiseAndSetIfChanged(ref _CommonParameters, value);
        }

        public virtual void saveParameters() { }
    }
}
