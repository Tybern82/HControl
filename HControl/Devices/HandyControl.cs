using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HControl.Devices {
    public class HandyControl : Device {

        protected static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        public static readonly HandyControl Instance = new HandyControl();
        private HttpClient Client = new HttpClient();

        private HandyControl() {
            Client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static readonly string BASE = "https://www.handyfeeling.com/api/handy/v2";

        public enum HandyMode {
            HAMP = 0,
            HDSP = 2,
        }

        private bool isStarted = false;
        private HandyMode currentMode = HandyMode.HAMP;

        private string _ConnectionKey = string.Empty;
        public string ConnectionKey {
            get => _ConnectionKey;
            private set => _ConnectionKey = value;
        }

        private JObject httpPut(string url, string body = "") {
            if (string.IsNullOrEmpty(ConnectionKey)) return new JObject();    // don't send if empty key

            LOG.Debug("HTTP [" + url + "]:{" + ConnectionKey + "}");
            if (!string.IsNullOrWhiteSpace(body)) LOG.Debug("Body: " + body);
            // using var client = new HttpClient();
            // Client.DefaultRequestHeaders.Add("X-Connection-Key", ConnectionKey);
            // Client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var message = new HttpRequestMessage(HttpMethod.Put, url);
            message.Headers.Add("X-Connection-Key", ConnectionKey);
            message.Content = new StringContent(body);
            var response = Client.Send(message);
            if ((response != null) && (response.StatusCode == System.Net.HttpStatusCode.OK)) {
                string respText = response.Content.ReadAsStringAsync().Result;
                LOG.Debug(respText);
                return JObject.Parse(respText);
            } else {
                if (response != null) LOG.Debug(response.Content.ReadAsStringAsync().Result);
                return new JObject();
            }
        }

        private void httpPut(string url, JObject obj) => httpPut(url, obj.ToString());

        private void setMode(HandyMode mode) {
            JObject param = new JObject();
            param.Add("mode", (int)mode);
            httpPut(BASE + "/mode", param);
            currentMode = mode;
        }

        public void setHAMP() => setMode(HandyMode.HAMP);
        public void setHDSP() => setMode(HandyMode.HDSP);

        public override bool connect(string? key) {
            this.ConnectionKey = key ?? string.Empty;
            setHAMP();
            isConnected = true;
            return true;
        }

        public override bool disconnect() {
            stop();
            if (isConnected) {
                isConnected = false;
                return true;
            }
            return false;
        }

        public override void setIntensity(uint intensity) {
            if (!isStarted) start();
            int pVelocity = (int)intensity;
            if (pVelocity <= 0) {
                stop();
                return;
            } else if (pVelocity > 100) {
                pVelocity = 100;    // max 100
            }
            if (currentMode != HandyMode.HAMP) throw new InvalidOperationException("Device must be in HAMP mode to set velocity");
            JObject param = new JObject();
            param.Add("velocity", pVelocity);
            httpPut(BASE+"/hamp/velocity", param);
            OnIntensityChanged(intensity);
        }

        public override void start() {
            if (!isStarted) {
                if (currentMode != HandyMode.HAMP) throw new InvalidOperationException("Device must be in HAMP mode to start");
                httpPut(BASE+"/hamp/start");
                isStarted = true;
            }
        }

        public override void stop() {
            if (isStarted) {
                if (currentMode != HandyMode.HAMP) throw new InvalidOperationException("Device must be in HAMP mode to stop");
                httpPut(BASE+"/hamp/stop");
                isStarted = false;
                OnIntensityChanged(0);
            }
        }
    }
}
