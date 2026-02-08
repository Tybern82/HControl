using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HControl.Devices {
    public abstract class Device {

        public class IntensityEvent {
            public uint Intensity { get; }

            public IntensityEvent(uint intensity) { Intensity = intensity; }
        }

        public event EventHandler<IntensityEvent>? IntensityChanged;

        protected virtual void OnIntensityChanged(uint intensity) {
            IntensityChanged?.Invoke(this, new IntensityEvent(intensity));
        }

        public bool isConnected { get; protected set; }

        protected Device() { }

        public abstract bool connect(string? key);
        public abstract bool disconnect();

        public abstract void start();
        public abstract void stop();
        public abstract void setIntensity(uint intensity);
    }
}
