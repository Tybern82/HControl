using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using HControl.Game;
using HControl.Game.Endurance;
using HControl.Game.Intensity;
using HControl.Game.Quick;
using HControl.Game.Slideshow;
using HControl.Game.Timed;
using Newtonsoft.Json.Linq;

namespace HControl.Views;

public partial class MainWindow : Window {

    protected static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

    public static MainWindow? PrimaryWindowReference { get; private set; }

    private readonly List<HCGameModule> gameModules = [];


    public MainWindow() {
        PrimaryWindowReference = this;
        SettingsStore.Instance.load(SettingsStore.DEFAULT_STORE);
        InitializeComponent();
        LOG.Debug("Setting MainWindow context...");
        this.DataContext = vMain.ViewModel;

        SettingsStore.Instance.loadWindow(this);

        // Load built-in modules
        var selectedModule = doLoadModules(Assembly.GetExecutingAssembly());

        if (selectedModule != null) vMain.tcSettings.SelectedItem = selectedModule;

        // TODO: add user-defined modules - load defined assembly and run as for built-ins
        // TODO: separate the individual modules out to plugin assemblies
        // SettingsStore.Instance.Plugins:List<string>
        // loadModules(Assembly.LoadFrom(<path>));

        this.Closing += (sender, args) => {
            SettingsStore.Instance.saveWindow(this);
            vMain.ViewModel.CommonParameters.saveCommonSettings();
            foreach (var module in gameModules) module.saveParameters();
            if (vMain.tcSettings.SelectedItem is TabItem selectedModule) 
                if (selectedModule.Header != null) 
                    SettingsStore.Instance.setParameter(SettingsStore.CONFIG_SELECTEDMOD, JValue.FromObject(selectedModule.Header));
            SettingsStore.Instance.save(SettingsStore.DEFAULT_STORE);
        };
    }

    private TabItem? doLoadModules(Assembly assembly) {
        string? moduleName = null;
        var moduleSetting = SettingsStore.Instance.getParameter(SettingsStore.CONFIG_SELECTEDMOD);
        if (moduleSetting != null && moduleSetting is JValue moduleValue) {
            moduleName = (string?)moduleValue;
            LOG.Debug("Selected Module: " + moduleName);
        }
        TabItem? _result = null;

        System.Type baseType = typeof(HCGameModule);
        var types = assembly.GetTypes().Where(baseType.IsAssignableFrom).Where(t => baseType != t);
        foreach (var moduleType in types) {
            object? obj = Activator.CreateInstance(moduleType);
            if (obj != null && obj is HCGameModule module) {
                gameModules.Add(module);
                TabItem moduleTab = vMain.registerModule(module);
                if ((moduleName != null) && string.Equals(moduleName, module.ModuleName, StringComparison.InvariantCulture)) 
                    _result = moduleTab;
            }
        }
        return _result;
    }

    public void doTriggerClose() {
        vMain.ViewModel.triggerOnCancelClicked();
    }
}
