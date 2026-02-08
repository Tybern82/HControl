using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;

namespace HControl {
    public class SettingsStore {

        public static readonly int PULSE_TIME = 50;

        public static readonly string DEFAULT_STORE = "HControl.config";

        public static readonly SettingsStore Instance = new SettingsStore();

        private Dictionary<string,WindowPosition> savedWindows = new Dictionary<string,WindowPosition>();

        private Dictionary<string,JToken> savedParameters = new Dictionary<string, JToken>();

        public bool isMaleUI { get; set; } = true;

        private SettingsStore() {}

        public JToken? getParameter(string param) {
            return (savedParameters.ContainsKey(param) ? savedParameters[param] : null);
        }

        public void setParameter(string param, JToken token) {
            savedParameters[param] = token;
        }

        public void loadWindow(Window wnd) {
            if (wnd.Title == null) return;
            if (savedWindows.ContainsKey(wnd.Title)) {
                WindowPosition.setPosition(wnd, savedWindows[wnd.Title]);
            }
        }

        public void saveWindow(Window wnd) {
            if (wnd.Title == null) return;
            WindowPosition pos = WindowPosition.getPosition(wnd);
            savedWindows[wnd.Title] = pos;
        }

        public void load(string fName) {
            using (FileStream configFile = File.Open(fName, FileMode.OpenOrCreate)) {
                using (StreamReader configReader = new StreamReader(configFile)) {
                    string configJSON = configReader.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(configJSON)) {
                        JObject configData = JObject.Parse(configJSON);
                        decodeJSON(configData);
                    }
                }
            }
        }

        public void save(string fName) {
            using (FileStream configFile = File.Open(fName, FileMode.Create)) {
                using (StreamWriter configWriter = new StreamWriter(configFile)) {
                    configWriter.Write(encodeJSON().ToString());
                    configWriter.Flush();
                    configWriter.Close();
                }
            }
        }

        private void decodeJSON(JObject configData) {
            if (configData.ContainsKey(CONFIGKEY_WPOS)) {
                JArray? windowPositions = configData.Value<JArray>(CONFIGKEY_WPOS);
                if (windowPositions != null) {
                    foreach (JObject wnd in windowPositions) {
                        WindowPosition wPos = new WindowPosition(wnd);
                        string? name = wnd.Value<string>(CONFIGKEY_WPOS_NAME);
                        if (name != null) {
                            savedWindows[name] = wPos;
                        }
                    }
                }
            }
            if (configData.ContainsKey(CONFIG_GENDER)) {
                isMaleUI = configData.Value<bool>(CONFIG_GENDER);
            }
            foreach (var item in configData) {
                if (!string.Equals(item.Key, CONFIGKEY_WPOS) && !string.Equals(item.Key, CONFIG_GENDER))
                    if (item.Value != null) 
                        savedParameters[item.Key] = item.Value;
            }
        }

        private JObject encodeJSON() {
            JObject _result = new JObject();

            if (savedWindows.Count > 0) {
                JArray windowPositions = new JArray();
                foreach (string s in savedWindows.Keys) {
                    WindowPosition wPos = savedWindows[s];
                    JObject wnd = wPos.toJSON();
                    wnd.Add(CONFIGKEY_WPOS_NAME, s);
                    windowPositions.Add(wnd);
                }
                _result.Add(CONFIGKEY_WPOS, windowPositions);
                _result.Add(CONFIG_GENDER, isMaleUI);
            }
            foreach (string s in savedParameters.Keys) {
                _result.Add(s, savedParameters[s]);
            }

            return _result;
        }

        private static readonly string CONFIGKEY_WPOS = "WindowPositions";
        private static readonly string CONFIGKEY_WPOS_NAME = "Name";
        private static readonly string CONFIG_GENDER = "UIGender";
        public static readonly string CONFIG_SELECTEDMOD = "SelectedModule";
    }
}