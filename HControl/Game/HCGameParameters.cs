using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace HControl.Game {
    public interface HCGameParameters {

        public CommonGameParameters CommonParameters { get; set; }

        public void saveParameters();
    }
}
