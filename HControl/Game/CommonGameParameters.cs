using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace HControl.Game {
    public class CommonGameParameters : ReactiveObject {

        private static readonly string PARAM_IMG_ENABLE = "enabled";
        private static readonly string PARAM_IMG_FOLDER = "folder";
        private static readonly string PARAM_IMG_ADDSUB = "addSubfolders";

        public static readonly string PARAM_INTENSE = "IsIntense";
        public static readonly string PARAM_LOOPFINISH = "LoopFinish";
        public static readonly string PARAM_HANDY = "EnableHandy";
        public static readonly string PARAM_HANDYKEY = "HandyConnectionKey";
        public static readonly string PARAM_IMAGES = "ImageList";
        public static readonly string PARAM_FINISH = "FinishList";
        public static readonly string PARAM_CYCLE = "PictureCycle";
        public static readonly string PARAM_IMGBREAK = "NoImageBreak";
        public static readonly string PARAM_BEATBREAK = "NoBeatBreak";
        public static readonly string PARAM_BEATDEFAULT = "MinBeatBreak";
        public static readonly string PARAM_DOTICKER = "Ticker";

        public CommonGameParameters() {
            JToken? iList = SettingsStore.Instance.getParameter(PARAM_IMAGES);
            if (iList != null && iList is JArray imgItems) {
                foreach (var item in imgItems) {
                    if (item is JObject i) {
                        bool enabled = i.Value<bool>(PARAM_IMG_ENABLE);
                        bool addSubs = i.Value<bool>(PARAM_IMG_ADDSUB);
                        string? folder = i.Value<string>(PARAM_IMG_FOLDER);
                        if (folder != null) {
                            ImageList.Add(new ImageFolderListItem(folder, addSubs) {
                                IsEnabled = enabled
                            });
                        }
                    }
                }
            }
            JToken? fList = SettingsStore.Instance.getParameter(PARAM_FINISH);
            if (fList != null && fList is JArray fimgItems) {
                foreach (var item in fimgItems) {
                    if (item is JObject i) {
                        bool enabled = i.Value<bool>(PARAM_IMG_ENABLE);
                        bool addSubs = i.Value<bool>(PARAM_IMG_ADDSUB);
                        string? folder = i.Value<string>(PARAM_IMG_FOLDER);
                        if (folder != null) {
                            FinishList.Add(new ImageFolderListItem(folder, addSubs) {
                                IsEnabled = enabled
                            });
                        }
                    }
                }
            }

            JToken? isIntense = SettingsStore.Instance.getParameter(PARAM_INTENSE);
            if (isIntense != null && isIntense is JValue isIntenseValue) {
                IsIntense = (bool)isIntenseValue;
            }

            JToken? isLoopFinish = SettingsStore.Instance.getParameter(PARAM_LOOPFINISH);
            if (isLoopFinish != null && isLoopFinish is JValue isLoopFinishValue) {
                LoopFinish = (bool)isLoopFinishValue;
            }

            JToken? enableHandy = SettingsStore.Instance.getParameter(PARAM_HANDY);
            if (enableHandy != null && enableHandy is JValue enableHandyValue) {
                EnableHandy = (bool)enableHandyValue;
            }

            JToken? handyKey = SettingsStore.Instance.getParameter(PARAM_HANDYKEY);
            if (handyKey != null && handyKey is JValue handyKeyValue) {
                HandyKey = (string?)handyKeyValue ?? string.Empty;
            }

            JToken? cycle = SettingsStore.Instance.getParameter(PARAM_CYCLE);
            if (cycle != null && cycle is JValue cycleValue) {
                string? time = (string?)cycleValue;
                if (time != null) PictureCycle = TimeSpan.Parse(time);
            }

            JToken? noImageBreak = SettingsStore.Instance.getParameter(PARAM_IMGBREAK);
            if (noImageBreak != null && noImageBreak is JValue noImageBreakValue) {
                NoImageBreak = (bool)noImageBreakValue;
            }

            JToken? noBeatBreak = SettingsStore.Instance.getParameter(PARAM_BEATBREAK);
            if (noBeatBreak != null && noBeatBreak is JValue noBeatBreakValue) {
                NoBeatBreak = (bool)noBeatBreakValue;
            }

            JToken? beatDefault = SettingsStore.Instance.getParameter(PARAM_BEATDEFAULT);
            if (beatDefault != null && beatDefault is JValue beatDefaultValue) {
                BeatDefault = (int)beatDefaultValue;
            }

            JToken? doTicker = SettingsStore.Instance.getParameter(PARAM_DOTICKER);
            if (doTicker != null && doTicker is JValue doTickerValue) {
                DoTicker = (bool)doTickerValue;
            }
        }

        public void saveCommonSettings() {
            JArray imgItems = [];
            foreach (var item in ImageList) {
                JObject fItem = new() {
                    [PARAM_IMG_ENABLE] = item.IsEnabled,
                    [PARAM_IMG_FOLDER] = item.Folder,
                    [PARAM_IMG_ADDSUB] = item.AddSubfolders
                };
                imgItems.Add(fItem);
            }
            JArray fImgItems = [];
            foreach (var item in FinishList) {
                JObject fItem = new() {
                    [PARAM_IMG_ENABLE] = item.IsEnabled,
                    [PARAM_IMG_FOLDER] = item.Folder,
                    [PARAM_IMG_ADDSUB] = item.AddSubfolders
                };
                fImgItems.Add(fItem);
            }
            SettingsStore.Instance.setParameter(PARAM_IMAGES, imgItems);
            SettingsStore.Instance.setParameter(PARAM_FINISH, fImgItems);
            SettingsStore.Instance.setParameter(PARAM_INTENSE, JValue.FromObject(IsIntense));
            SettingsStore.Instance.setParameter(PARAM_LOOPFINISH, JValue.FromObject(LoopFinish));
            SettingsStore.Instance.setParameter(PARAM_HANDY, JValue.FromObject(EnableHandy));
            SettingsStore.Instance.setParameter(PARAM_HANDYKEY, JValue.FromObject(HandyKey));
            SettingsStore.Instance.setParameter(PARAM_CYCLE, JValue.FromObject(PictureCycle));
            SettingsStore.Instance.setParameter(PARAM_IMGBREAK, JValue.FromObject(NoImageBreak));
            SettingsStore.Instance.setParameter(PARAM_BEATBREAK, JValue.FromObject(NoBeatBreak));
            SettingsStore.Instance.setParameter(PARAM_BEATDEFAULT, JValue.FromObject(BeatDefault));
            SettingsStore.Instance.setParameter(PARAM_DOTICKER, JValue.FromObject(DoTicker));
        }

        private bool _NoImageBreak = false;
        public bool NoImageBreak {
            get => _NoImageBreak;
            set => this.RaiseAndSetIfChanged(ref _NoImageBreak, value);
        }

        private bool _NoBeatBreak = false;
        public bool NoBeatBreak {
            get => _NoBeatBreak;
            set => this.RaiseAndSetIfChanged(ref _NoBeatBreak, value);
        }

        private int _BeatDefault = 0;
        public int BeatDefault {
            get => _BeatDefault;
            set => this.RaiseAndSetIfChanged(ref _BeatDefault, value);
        }

        private bool _LoopFinish = true;
        public bool LoopFinish {
            get => _LoopFinish;
            set => this.RaiseAndSetIfChanged(ref _LoopFinish, value);
        }

        private bool _IsIntense = false;
        public bool IsIntense {
            get => _IsIntense;
            set => this.RaiseAndSetIfChanged(ref _IsIntense, value);
        }

        private bool _EnableHandy = true;
        public bool EnableHandy {
            get => _EnableHandy;
            set => this.RaiseAndSetIfChanged(ref _EnableHandy, value);
        }

        private string _HandyKey = string.Empty;
        public string HandyKey {
            get => _HandyKey;
            set => this.RaiseAndSetIfChanged(ref _HandyKey, value);
        }

        private TimeSpan _PictureCycle = TimeSpan.FromSeconds(5);
        public TimeSpan PictureCycle {
            get => _PictureCycle;
            set => this.RaiseAndSetIfChanged(ref _PictureCycle, value);
        }

        private bool _DoTicker = false;
        public bool DoTicker {
            get => _DoTicker;
            set => this.RaiseAndSetIfChanged(ref _DoTicker, value);
        }

        public ObservableCollection<ImageFolderListItem> ImageList { get; } = [];
        public ObservableCollection<ImageFolderListItem> FinishList { get; } = [];
    }
}
